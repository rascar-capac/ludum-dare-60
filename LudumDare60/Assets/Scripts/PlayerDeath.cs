using System.Collections;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Kills the player when the left edge of the camera catches them or when they fall below the screen,
    /// then moves the camera back and respawns the player at the last reached Checkpoint with a fade in.
    /// The level starts with the same spawn sequence at the first Checkpoint, the camera being placed instantly.
    /// Every enemy is reset to its initial position at the start of the spawn sequence, killed ones included.
    /// The player also loses their weapon, every collected Pickup comes back and every ThrownSpear is removed.
    /// Without any Checkpoint, the player spawns at their position in the scene.
    /// Controls are disabled for the whole sequence.
    /// </summary>
    [DefaultExecutionOrder(-10)] // Places the player and the camera before the other Start, which read their positions.
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] CameraScroller scroller;
        [SerializeField] RespawnSettings settings;
        [SerializeField] ParticleSystem deathBurstPrefab;

        Rigidbody2D body;
        Collider2D bodyCollider;
        SpriteRenderer spriteRenderer;
        PlayerController controller;
        CameraPush cameraPush;
        PlayerWeapon weapon;
        SpearThrower thrower;
        Vector3 spawnPosition;
        Checkpoint currentCheckpoint;

        public bool IsDead { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            controller = GetComponent<PlayerController>();
            cameraPush = GetComponent<CameraPush>();
            weapon = GetComponent<PlayerWeapon>();
            thrower = GetComponent<SpearThrower>();
            spawnPosition = transform.position;
        }

        void Start()
        {
            // Checkpoints register in OnEnable, so they are all known by now.
            Checkpoint first = Checkpoint.GetFirst();
            if (first != null)
            {
                SetCheckpoint(first);
            }
            if (settings != null)
            {
                StartCoroutine(SpawnSequence(0f));
            }
        }

        void FixedUpdate()
        {
            if (IsDead || scroller == null)
            {
                return;
            }

            Checkpoint checkpoint = Checkpoint.GetLastReached(body.position.x);
            if (checkpoint != null && (currentCheckpoint == null || checkpoint.ActivationX > currentCheckpoint.ActivationX))
            {
                SetCheckpoint(checkpoint);
                checkpoint.Activate();
            }

            Bounds bounds = bodyCollider.bounds;
            if (bounds.min.x <= scroller.LeftEdgeX || bounds.max.y < scroller.BottomEdgeY)
            {
                Kill();
            }
        }

        public void Kill()
        {
            if (IsDead || settings == null)
            {
                return;
            }
            StartCoroutine(DeathSequence());
        }

        IEnumerator DeathSequence()
        {
            IsDead = true;
            Bounds bounds = bodyCollider.bounds;
            SetControlsEnabled(false);
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            spriteRenderer.enabled = false;
            if (deathBurstPrefab != null)
            {
                // After a fall the player is below the screen: raise the burst so it stays visible.
                Vector3 center = bounds.center;
                if (scroller != null)
                {
                    center.y = Mathf.Max(center.y, scroller.BottomEdgeY + bounds.extents.y);
                }
                center.z = deathBurstPrefab.transform.position.z;
                Instantiate(deathBurstPrefab, center, deathBurstPrefab.transform.rotation);
            }
            if (scroller != null)
            {
                scroller.Scrolling = false;
            }

            yield return new WaitForSeconds(settings.DeathDelay);
            yield return SpawnSequence(settings.CameraReturnDuration);
        }

        /// <summary>
        /// Moves the camera to the spawn position, then places the player there, facing right, and fades them in.
        /// With a camera duration of 0 the camera and the player are placed synchronously.
        /// </summary>
        IEnumerator SpawnSequence(float cameraDuration)
        {
            IsDead = true;
            SetControlsEnabled(false);
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            spriteRenderer.enabled = false;
            EnemyController.ResetAll();
            Pickup.ResetAll();
            ThrownSpear.ClearAll();
            if (weapon != null)
            {
                weapon.Weapon = WeaponType.None;
            }

            if (scroller != null)
            {
                scroller.Scrolling = false;
                float cameraX = spawnPosition.x + scroller.HalfWidthAt(0f) * (1f - 2f * settings.SpawnScreenX);
                if (cameraDuration > 0f)
                {
                    yield return scroller.ReturnTo(cameraX, cameraDuration);
                }
                else
                {
                    scroller.MoveTo(cameraX);
                }
            }

            transform.position = spawnPosition;
            body.position = spawnPosition;
            body.simulated = true;
            spriteRenderer.flipX = false;
            SetAlpha(0f);
            spriteRenderer.enabled = true;
            for (float time = 0f; time < settings.FadeInDuration; time += Time.deltaTime)
            {
                SetAlpha(time / settings.FadeInDuration);
                yield return null;
            }
            SetAlpha(1f);

            SetControlsEnabled(true);
            if (scroller != null)
            {
                scroller.Scrolling = true;
            }
            IsDead = false;
        }

        void SetCheckpoint(Checkpoint checkpoint)
        {
            currentCheckpoint = checkpoint;
            Vector3 position = checkpoint.SpawnPosition;
            spawnPosition = new Vector3(position.x, position.y, spawnPosition.z);
        }

        void SetControlsEnabled(bool value)
        {
            if (controller != null)
            {
                controller.enabled = value;
            }
            if (cameraPush != null)
            {
                cameraPush.enabled = value;
            }
            if (thrower != null)
            {
                thrower.enabled = value;
            }
        }

        void SetAlpha(float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
