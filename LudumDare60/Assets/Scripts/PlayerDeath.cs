using System.Collections;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Kills the player when the left edge of the camera catches them or when they fall below the screen,
    /// then moves the camera back to its start and respawns the player at their start position with a fade in.
    /// Controls are disabled for the whole sequence.
    /// </summary>
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
        Vector3 spawnPosition;

        public bool IsDead { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            controller = GetComponent<PlayerController>();
            cameraPush = GetComponent<CameraPush>();
            spawnPosition = transform.position;
        }

        void FixedUpdate()
        {
            if (IsDead || scroller == null)
            {
                return;
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
            if (scroller != null)
            {
                yield return scroller.ReturnToStart(settings.CameraReturnDuration);
            }

            transform.position = spawnPosition;
            body.position = spawnPosition;
            body.simulated = true;
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
        }

        void SetAlpha(float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
