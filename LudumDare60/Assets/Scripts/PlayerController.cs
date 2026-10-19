using UnityEngine;
using UnityEngine.InputSystem;

namespace LD60
{
    /// <summary>
    /// Run and jump with keyboard or gamepad, using the Player/Move and Player/Jump actions.
    /// While RunLocked is set, the horizontal input is ignored and the facing direction kept: the player slows down to a stop.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

        [Header("Input")]
        [SerializeField] InputActionReference moveAction;
        [SerializeField] InputActionReference jumpAction;

        [Header("Tuning")]
        [SerializeField] PlayerMovementSettings settings;
        [SerializeField] LayerMask groundMask;

        Rigidbody2D body;
        Collider2D bodyCollider;
        SpriteRenderer spriteRenderer;
        Animator animator;
        float lastGroundedTime = float.NegativeInfinity;
        float lastJumpPressedTime = float.NegativeInfinity;
        bool jumpReleased;

        public bool IsGrounded { get; private set; }

        public bool RunLocked { get; set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        void OnEnable()
        {
            moveAction?.action.Enable();
            jumpAction?.action.Enable();
        }

        void OnDisable()
        {
            moveAction?.action.Disable();
            jumpAction?.action.Disable();
            if (animator != null)
            {
                animator.SetBool(IsRunningHash, false);
            }
        }

        void Update()
        {
            UpdateVisuals();

            if (jumpAction == null)
            {
                return;
            }
            if (jumpAction.action.WasPressedThisFrame())
            {
                lastJumpPressedTime = Time.time;
            }
            if (jumpAction.action.WasReleasedThisFrame())
            {
                jumpReleased = true;
            }
        }

        void FixedUpdate()
        {
            if (settings == null)
            {
                return;
            }

            IsGrounded = CheckGrounded();
            if (IsGrounded)
            {
                lastGroundedTime = Time.time;
            }

            Vector2 velocity = body.linearVelocity;
            body.gravityScale = settings.GravityScale * GravityMultiplier(velocity.y);
            float input = ReadRunInput();

            float targetSpeed = input * settings.MaxSpeed;
            float rate = Mathf.Abs(input) > 0.01f ? settings.Acceleration : settings.Deceleration;
            if (!IsGrounded)
            {
                rate *= settings.AirControl;
            }
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, rate * Time.fixedDeltaTime);

            bool canJump = Time.time - lastGroundedTime <= settings.CoyoteTime;
            bool wantsJump = Time.time - lastJumpPressedTime <= settings.JumpBufferTime;
            if (canJump && wantsJump)
            {
                velocity.y = settings.JumpSpeed;
                lastGroundedTime = float.NegativeInfinity;
                lastJumpPressedTime = float.NegativeInfinity;
                jumpReleased = false;
            }

            if (jumpReleased)
            {
                if (velocity.y > 0f)
                {
                    velocity.y *= settings.JumpCutMultiplier;
                }
                jumpReleased = false;
            }

            body.linearVelocity = velocity;
        }

        void UpdateVisuals()
        {
            float input = ReadRunInput();
            bool isRunning = Mathf.Abs(input) > 0.01f;

            if (animator != null)
            {
                animator.SetBool(IsRunningHash, isRunning);
            }
            if (spriteRenderer != null && isRunning)
            {
                spriteRenderer.flipX = input < 0f;
            }
        }

        float GravityMultiplier(float verticalSpeed)
        {
            if (IsGrounded)
            {
                return 1f;
            }
            if (Mathf.Abs(verticalSpeed) < settings.ApexSpeedThreshold)
            {
                return settings.ApexGravityMultiplier;
            }
            return verticalSpeed < 0f ? settings.FallGravityMultiplier : 1f;
        }

        float ReadRunInput() => moveAction != null && !RunLocked ? moveAction.action.ReadValue<Vector2>().x : 0f;

        bool CheckGrounded()
        {
            if (body.linearVelocity.y > 0.01f)
            {
                return false;
            }
            Bounds bounds = bodyCollider.bounds;
            Vector2 size = new(bounds.size.x * 0.9f, 0.05f);
            Vector2 origin = new(bounds.center.x, bounds.min.y + size.y * 0.5f);
            return Physics2D.BoxCast(origin, size, 0f, Vector2.down, settings.GroundCheckDistance, groundMask);
        }
    }
}
