using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Run and jump tuning of the player.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "LD60/Player Movement Settings")]
    public class PlayerMovementSettings : ScriptableObject
    {
        [Header("Run")]
        [SerializeField, Min(0f)] float maxSpeed = 6f;
        [SerializeField, Min(0f)] float acceleration = 60f;
        [SerializeField, Min(0f)] float deceleration = 50f;
        [SerializeField, Min(0f)] float airControl = 0.6f;

        [Header("Jump")]
        [SerializeField, Min(0f)] float jumpHeight = 2.05f;
        [SerializeField, Min(0.01f)] float gravity = 29.43f;
        [SerializeField, Range(0f, 1f)] float jumpCutMultiplier = 0.5f;
        [SerializeField, Min(0f)] float coyoteTime = 0.1f;
        [SerializeField, Min(0f)] float jumpBufferTime = 0.12f;

        [Header("Ground check")]
        [SerializeField, Min(0f)] float groundCheckDistance = 0.08f;

        public float MaxSpeed => maxSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirControl => airControl;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public float GroundCheckDistance => groundCheckDistance;

        /// <summary>Initial vertical speed needed to reach jumpHeight under gravity.</summary>
        public float JumpSpeed => Mathf.Sqrt(2f * gravity * jumpHeight);

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
