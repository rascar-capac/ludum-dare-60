using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Vision and movement tuning of the enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemySettings", menuName = "LD60/Enemy Settings")]
    public class EnemySettings : ScriptableObject
    {
        [Header("Vision")]
        [Tooltip("Maximum distance at which the enemy sees the avatar.")]
        [SerializeField, Min(0f)] float viewDistance = 8f;

        [Tooltip("Total angle of the view cone, centered on the facing direction.")]
        [SerializeField, Range(0f, 360f)] float viewAngle = 90f;

        [Tooltip("Layers that block the line of sight.")]
        [SerializeField] LayerMask sightBlockingMask;

        [Header("Run")]
        [SerializeField, Min(0f)] float maxSpeed = 4f;
        [SerializeField, Min(0f)] float acceleration = 40f;
        [SerializeField, Min(0f)] float deceleration = 50f;
        [SerializeField, Min(0.01f)] float gravity = 29.43f;

        [Tooltip("Horizontal distance under which a target position counts as reached.")]
        [SerializeField, Min(0f)] float arriveDistance = 0.1f;

        [Header("Reachability")]
        [Tooltip("Layers the enemy walks on and that block its way.")]
        [SerializeField] LayerMask groundMask;

        [Tooltip("Distance ahead checked for walls.")]
        [SerializeField, Min(0f)] float wallCheckDistance = 0.1f;

        [Tooltip("Depth below the front edge checked for ground. The enemy stops at ledges deeper than that.")]
        [SerializeField, Min(0f)] float ledgeCheckDepth = 0.5f;

        [Header("Respawn")]
        [Tooltip("Duration of the alpha fade out before moving back to the initial position when the avatar respawns.")]
        [SerializeField, Min(0f)] float fadeOutDuration = 0.4f;

        [Tooltip("Duration of the alpha fade in at the initial position when the avatar respawns.")]
        [SerializeField, Min(0f)] float fadeInDuration = 0.6f;

        public float ViewDistance => viewDistance;
        public float ViewAngle => viewAngle;
        public LayerMask SightBlockingMask => sightBlockingMask;
        public float MaxSpeed => maxSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float ArriveDistance => arriveDistance;
        public LayerMask GroundMask => groundMask;
        public float WallCheckDistance => wallCheckDistance;
        public float LedgeCheckDepth => ledgeCheckDepth;
        public float FadeOutDuration => fadeOutDuration;
        public float FadeInDuration => fadeInDuration;

        /// <summary>Rigidbody2D.gravityScale that produces gravity with the project's Physics2D gravity.</summary>
        public float GravityScale
        {
            get
            {
                float worldGravity = Mathf.Abs(Physics2D.gravity.y);
                return worldGravity > 0f ? gravity / worldGravity : 0f;
            }
        }
    }
}
