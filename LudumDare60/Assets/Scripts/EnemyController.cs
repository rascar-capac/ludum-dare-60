using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Stays still until it sees the avatar in its view cone (facing given by SpriteRenderer.flipX), then runs toward it while it is in sight.
    /// Once the avatar is out of sight, runs to the last position where it was seen and stops there.
    /// Walls and ledges stop the enemy: it then waits at the closest reachable position.
    /// Kills the avatar on contact.
    /// Killing it hides it instantly with a particle burst. Its GameObject is only deactivated, so that ResetAll can bring it back.
    /// ResetAll fades it out, moves it back to its initial position and state, then fades it in (inert during the whole sequence).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        static readonly List<EnemyController> enemies = new();
        static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

        enum State { Idle, Chasing, Searching }

        [SerializeField] EnemySettings settings;
        [Tooltip("Found in the scene when left empty.")]
        [SerializeField] PlayerDeath target;
        [SerializeField] ParticleSystem deathBurstPrefab;

        Rigidbody2D body;
        Collider2D bodyCollider;
        Collider2D targetCollider;
        Rigidbody2D targetBody;
        SpriteRenderer spriteRenderer;
        Animator animator;
        State state;
        Vector2 lastSeenPosition;
        Vector3 initialPosition;
        bool initialFlipX;
        bool respawning;

        void Awake()
        {
            // Registered for the whole lifetime, not only while active, so that killed enemies can be reset.
            enemies.Add(this);
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            if (target == null)
            {
                target = FindAnyObjectByType<PlayerDeath>();
            }
            if (target != null)
            {
                targetCollider = target.GetComponent<Collider2D>();
                targetBody = target.GetComponent<Rigidbody2D>();
            }
            initialPosition = transform.position;
            initialFlipX = spriteRenderer != null && spriteRenderer.flipX;
        }

        void OnDestroy() => enemies.Remove(this);

        /// <summary>Makes the enemy disappear instantly with a particle burst, until the next ResetAll.</summary>
        public void Kill()
        {
            if (!gameObject.activeSelf || respawning)
            {
                return;
            }
            if (deathBurstPrefab != null)
            {
                Vector3 center = bodyCollider.bounds.center;
                center.z = deathBurstPrefab.transform.position.z;
                Instantiate(deathBurstPrefab, center, deathBurstPrefab.transform.rotation);
            }
            gameObject.SetActive(false);
        }

        /// <summary>Kills every active enemy.</summary>
        public static void KillAll()
        {
            foreach (EnemyController enemy in enemies)
            {
                enemy.Kill();
            }
        }

        /// <summary>
        /// Brings every enemy, killed ones included, back to its initial position and state with a fade out then a fade in.
        /// Enemies already in their initial state are reset without fading.
        /// </summary>
        public static void ResetAll()
        {
            foreach (EnemyController enemy in enemies)
            {
                enemy.Respawn();
            }
        }

        void Respawn()
        {
            bool visible = gameObject.activeSelf;
            gameObject.SetActive(true);
            StopAllCoroutines();
            if (visible && IsInInitialState())
            {
                ResetState();
                SetAlpha(1f);
                body.simulated = true;
                respawning = false;
                return;
            }
            StartCoroutine(RespawnSequence(visible));
        }

        IEnumerator RespawnSequence(bool fadeOut)
        {
            respawning = true;
            body.simulated = false;
            SetRunning(false);
            if (fadeOut && settings != null)
            {
                yield return Fade(GetAlpha(), 0f, settings.FadeOutDuration);
            }

            ResetState();
            SetAlpha(0f);
            if (settings != null)
            {
                yield return Fade(0f, 1f, settings.FadeInDuration);
            }
            SetAlpha(1f);
            body.simulated = true;
            respawning = false;
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                SetAlpha(Mathf.Lerp(from, to, time / duration));
                yield return null;
            }
            SetAlpha(to);
        }

        bool IsInInitialState()
        {
            return ((Vector2)(transform.position - initialPosition)).sqrMagnitude < 0.0001f
                && (spriteRenderer == null || spriteRenderer.flipX == initialFlipX);
        }

        void ResetState()
        {
            transform.position = initialPosition;
            body.position = initialPosition;
            body.linearVelocity = Vector2.zero;
            state = State.Idle;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = initialFlipX;
            }
            SetRunning(false);
        }

        void SetRunning(bool value)
        {
            if (animator != null)
            {
                animator.SetBool(IsRunningHash, value);
            }
        }

        float GetAlpha() => spriteRenderer != null ? spriteRenderer.color.a : 1f;

        void SetAlpha(float alpha)
        {
            if (spriteRenderer == null)
            {
                return;
            }
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        void FixedUpdate()
        {
            if (settings == null || respawning)
            {
                return;
            }

            body.gravityScale = settings.GravityScale;

            if (CanSeeTarget())
            {
                state = State.Chasing;
                lastSeenPosition = targetCollider.bounds.center;
            }
            else if (state == State.Chasing)
            {
                state = State.Searching;
            }

            float direction = 0f;
            if (state != State.Idle)
            {
                float deltaX = lastSeenPosition.x - body.position.x;
                if (Mathf.Abs(deltaX) > settings.ArriveDistance)
                {
                    direction = Mathf.Sign(deltaX);
                }
                if (direction != 0f && IsBlocked(direction))
                {
                    direction = 0f;
                }
                if (direction == 0f && state == State.Searching)
                {
                    state = State.Idle;
                }
            }

            Vector2 velocity = body.linearVelocity;
            float rate = direction != 0f ? settings.Acceleration : settings.Deceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, direction * settings.MaxSpeed, rate * Time.fixedDeltaTime);
            body.linearVelocity = velocity;

            UpdateVisuals(direction);
        }

        void OnCollisionEnter2D(Collision2D collision) => TryKill(collision);

        void OnCollisionStay2D(Collision2D collision) => TryKill(collision);

        void TryKill(Collision2D collision)
        {
            if (target != null && collision.rigidbody == targetBody)
            {
                target.Kill();
            }
        }

        bool CanSeeTarget()
        {
            if (target == null || targetCollider == null || target.IsDead)
            {
                return false;
            }

            Vector2 eye = bodyCollider.bounds.center;
            Vector2 targetCenter = targetCollider.bounds.center;
            Vector2 toTarget = targetCenter - eye;
            if (toTarget.sqrMagnitude > settings.ViewDistance * settings.ViewDistance)
            {
                return false;
            }
            if (Vector2.Angle(GetForward(), toTarget) > settings.ViewAngle * 0.5f)
            {
                return false;
            }
            return !Physics2D.Linecast(eye, targetCenter, settings.SightBlockingMask);
        }

        /// <summary>Facing direction, given by SpriteRenderer.flipX (the sprite faces right when not flipped).</summary>
        Vector2 GetForward()
        {
            SpriteRenderer facingRenderer = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();
            return facingRenderer != null && facingRenderer.flipX ? Vector2.left : Vector2.right;
        }

        /// <summary>True when a wall or a ledge prevents moving in the given direction.</summary>
        bool IsBlocked(float direction)
        {
            Bounds bounds = bodyCollider.bounds;

            // Raised above the feet so the ground the enemy stands on is not seen as a wall.
            const float FeetClearance = 0.1f;
            Vector2 size = new(0.05f, bounds.size.y - FeetClearance);
            Vector2 origin = new(bounds.center.x + direction * (bounds.extents.x - size.x * 0.5f), bounds.center.y + FeetClearance * 0.5f);
            if (Physics2D.BoxCast(origin, size, 0f, Vector2.right * direction, settings.WallCheckDistance, settings.GroundMask))
            {
                return true;
            }

            Vector2 ledgeOrigin = new(bounds.center.x + direction * (bounds.extents.x + settings.WallCheckDistance), bounds.min.y + FeetClearance);
            return !Physics2D.Raycast(ledgeOrigin, Vector2.down, settings.LedgeCheckDepth + FeetClearance, settings.GroundMask);
        }

        void UpdateVisuals(float direction)
        {
            SetRunning(Mathf.Abs(body.linearVelocity.x) > 0.1f);
            if (spriteRenderer != null && direction != 0f)
            {
                spriteRenderer.flipX = direction < 0f;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (settings == null)
            {
                return;
            }

            Collider2D gizmoCollider = bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
            Vector3 eye = gizmoCollider != null ? gizmoCollider.bounds.center : transform.position;
            Gizmos.color = state == State.Chasing ? Color.red : Color.yellow;
            DrawViewCone(eye);
            if (Application.isPlaying && state != State.Idle)
            {
                Gizmos.DrawLine(eye, lastSeenPosition);
                Gizmos.DrawWireSphere(lastSeenPosition, 0.2f);
            }
        }

        void DrawViewCone(Vector3 eye)
        {
            const int Segments = 24;
            float halfAngle = settings.ViewAngle * 0.5f;
            Vector3 forward = GetForward();
            Vector3 previous = eye;
            for (int i = 0; i <= Segments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)Segments);
                Vector3 point = eye + Quaternion.Euler(0f, 0f, angle) * forward * settings.ViewDistance;
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
            Gizmos.DrawLine(previous, eye);
        }
    }
}
