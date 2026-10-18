using System.Collections.Generic;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Spear thrown by the player, simulated by physics. Its solid collider only collides with the scenery
    /// (set through the collider's Exclude Layers); the hitbox trigger kills the enemies it touches at a speed
    /// of at least SpearThrowSettings.MinKillSpeed, without affecting the spear's trajectory.
    /// Points along its velocity until it first touches the scenery, then rotates freely.
    /// The visual is a child SpriteRenderer pointing right when not flipped.
    /// Destroyed once it leaves the camera through the left, right or bottom edge, or by ClearAll.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ThrownSpear : MonoBehaviour
    {
        static readonly List<ThrownSpear> spears = new();

        Rigidbody2D body;
        SpriteRenderer spriteRenderer;
        SpearThrowSettings settings;
        CameraScroller scroller;
        bool flying = true;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spears.Add(this);
        }

        void OnDestroy() => spears.Remove(this);

        /// <summary>Destroys every thrown spear.</summary>
        public static void ClearAll()
        {
            foreach (ThrownSpear spear in spears)
            {
                Destroy(spear.gameObject);
            }
        }

        public void Launch(Vector2 velocity, SpearThrowSettings throwSettings, CameraScroller cameraScroller)
        {
            settings = throwSettings;
            scroller = cameraScroller;
            body.gravityScale = settings.GravityScale;
            body.linearVelocity = velocity;
            spriteRenderer.flipX = velocity.x < 0f;
            AlignWithVelocity();
        }

        void FixedUpdate()
        {
            if (flying)
            {
                AlignWithVelocity();
            }
            if (IsOutOfCamera())
            {
                Destroy(gameObject);
            }
        }

        void AlignWithVelocity()
        {
            Vector2 velocity = body.linearVelocity;
            if (velocity.sqrMagnitude < 0.0001f)
            {
                return;
            }
            float angle = Mathf.Atan2(velocity.y, Mathf.Abs(velocity.x)) * Mathf.Rad2Deg;
            body.rotation = spriteRenderer.flipX ? -angle : angle;
        }

        bool IsOutOfCamera()
        {
            if (scroller == null)
            {
                return false;
            }
            Bounds bounds = spriteRenderer.bounds;
            return bounds.max.x < scroller.LeftEdgeX || bounds.min.x > scroller.RightEdgeX || bounds.max.y < scroller.BottomEdgeY;
        }

        void OnCollisionEnter2D(Collision2D collision) => flying = false;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (settings == null || body.linearVelocity.magnitude < settings.MinKillSpeed)
            {
                return;
            }
            EnemyController enemy = other.attachedRigidbody != null
                ? other.attachedRigidbody.GetComponent<EnemyController>()
                : null;
            if (enemy != null)
            {
                enemy.Kill();
            }
        }
    }
}
