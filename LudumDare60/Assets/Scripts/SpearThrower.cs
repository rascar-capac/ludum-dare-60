using UnityEngine;
using UnityEngine.InputSystem;

namespace LD60
{
    /// <summary>
    /// Throws the held spear toward the facing direction (SpriteRenderer.flipX) when the Player/Attack action is released.
    /// When the charge is not started from a gamepad or a joystick, the pointer orients the player while charging
    /// and sets the launch angle, clamped between SpearThrowSettings.MinAimAngle and MaxAimAngle.
    /// The longer the input is held, the faster the spear is launched; a release before SpearThrowSettings.MinChargeTime cancels the throw.
    /// Each throw removes one spear from the player's stock.
    /// Running is locked from the press until the end of the throwing animation. The next spear of the stock is only
    /// shown, and can only be charged, once the throwing animation is over; if the input is held at that moment, a new charge starts.
    /// Animator: IsAiming is set from MinChargeTime until the release, AimProgress goes from 0 at MinChargeTime to 1 at MaxChargeTime
    /// (motion time of the aiming state) and Throw is triggered at the release.
    /// </summary>
    [RequireComponent(typeof(PlayerWeapon), typeof(SpriteRenderer), typeof(Animator))]
    public class SpearThrower : MonoBehaviour
    {
        static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
        static readonly int AimProgressHash = Animator.StringToHash("AimProgress");
        static readonly int ThrowHash = Animator.StringToHash("Throw");
        static readonly int ThrowingStateHash = Animator.StringToHash("avatar_throwing");

        [SerializeField] InputActionReference attackAction;

        [Tooltip("Pointer position in screen space, such as UI/Point.")]
        [SerializeField] InputActionReference aimPointAction;

        [SerializeField] SpearThrowSettings settings;
        [SerializeField] ThrownSpear spearPrefab;
        [SerializeField] AudioClip throwSound;
        [SerializeField] CameraScroller scroller;

        [Tooltip("Start of the trajectory relative to the player, authored facing right.")]
        [SerializeField] Vector2 throwOrigin = new(0.3f, 0.2f);

        PlayerWeapon weapon;
        SpriteRenderer spriteRenderer;
        Animator animator;
        PlayerController controller;
        float chargeStartTime;
        bool charging;
        bool aimWithPointer;
        bool throwPending;
        bool throwing;

        void Awake()
        {
            weapon = GetComponent<PlayerWeapon>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            controller = GetComponent<PlayerController>();
        }

        void OnEnable()
        {
            attackAction?.action.Enable();
            // Not disabled in OnDisable: the action may be shared with the UI input module.
            aimPointAction?.action.Enable();
        }

        void OnDisable()
        {
            attackAction?.action.Disable();
            charging = false;
            throwPending = false;
            throwing = false;
            UpdateAnimator(0f);
            UpdateThrowLocks();
        }

        void Update()
        {
            UpdateThrowingState();
            if (attackAction == null || settings == null)
            {
                return;
            }

            if (weapon.Weapon != WeaponType.Spear || IsThrowing)
            {
                charging = false;
            }
            else if (!charging && attackAction.action.IsPressed())
            {
                charging = true;
                chargeStartTime = Time.time;
                InputDevice device = attackAction.action.activeControl?.device;
                aimWithPointer = aimPointAction != null && !(device is Gamepad || device is Joystick);
            }
            else if (charging && attackAction.action.WasReleasedThisFrame())
            {
                charging = false;
                float? speed = settings.GetLaunchSpeed(Time.time - chargeStartTime);
                if (speed.HasValue)
                {
                    Throw(speed.Value);
                }
            }

            if (charging && aimWithPointer && TryGetPointerWorldPosition(out Vector2 target))
            {
                spriteRenderer.flipX = target.x < transform.position.x;
            }
            UpdateAnimator(charging ? Time.time - chargeStartTime : 0f);
            UpdateThrowLocks();
        }

        /// <summary>Launch angle above the horizontal, in degrees, toward the facing direction, if the spear was thrown now.</summary>
        public float AimAngle => settings != null ? GetLaunchAngle(GetOrigin(spriteRenderer.flipX)) : 0f;

        bool IsThrowing => throwPending || throwing;

        /// <summary>The throw lasts until the animator has entered then left the throwing state.</summary>
        void UpdateThrowingState()
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            bool inThrowingState = state.shortNameHash == ThrowingStateHash;
            if (throwPending && inThrowingState)
            {
                throwPending = false;
                throwing = true;
            }
            else if (throwing && !inThrowingState)
            {
                throwing = false;
            }
        }

        void UpdateAnimator(float holdTime)
        {
            bool aiming = settings != null && holdTime >= settings.MinChargeTime;
            animator.SetBool(IsAimingHash, aiming);
            float progress = aiming && settings.MaxChargeTime > settings.MinChargeTime
                ? Mathf.InverseLerp(settings.MinChargeTime, settings.MaxChargeTime, holdTime)
                : aiming ? 1f : 0f;
            animator.SetFloat(AimProgressHash, progress);
        }

        void UpdateThrowLocks()
        {
            weapon.HeldItemHidden = IsThrowing;
            if (controller != null)
            {
                controller.RunLocked = charging || IsThrowing;
            }
        }

        void Throw(float speed)
        {
            bool flip = spriteRenderer.flipX;
            weapon.SpearCount--;
            animator.SetTrigger(ThrowHash);
            throwPending = true;
            weapon.HeldItemHidden = true;
            SfxPlayer.Play(throwSound);
            if (spearPrefab == null)
            {
                return;
            }

            Vector3 origin = GetOrigin(flip);
            ThrownSpear spear = Instantiate(spearPrefab, origin, Quaternion.identity);
            spear.Launch(GetDirection(flip, GetLaunchAngle(origin)) * speed, settings, scroller);
        }

        Vector3 GetOrigin(bool flip)
        {
            return transform.position + new Vector3(flip ? -throwOrigin.x : throwOrigin.x, throwOrigin.y, 0f);
        }

        /// <summary>Launch angle above the horizontal, in degrees, toward the pointer when aiming with it.</summary>
        float GetLaunchAngle(Vector3 origin)
        {
            if (!aimWithPointer || !TryGetPointerWorldPosition(out Vector2 target))
            {
                return settings.LaunchAngle;
            }
            Vector2 offset = target - (Vector2)origin;
            float angle = Mathf.Atan2(offset.y, Mathf.Abs(offset.x)) * Mathf.Rad2Deg;
            return Mathf.Clamp(angle, settings.MinAimAngle, settings.MaxAimAngle);
        }

        static Vector2 GetDirection(bool flip, float angle)
        {
            angle *= Mathf.Deg2Rad;
            return new Vector2((flip ? -1f : 1f) * Mathf.Cos(angle), Mathf.Sin(angle));
        }

        bool TryGetPointerWorldPosition(out Vector2 position)
        {
            Camera camera = Camera.main;
            if (aimPointAction == null || camera == null)
            {
                position = default;
                return false;
            }
            Vector2 screen = aimPointAction.action.ReadValue<Vector2>();
            position = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, transform.position.z - camera.transform.position.z));
            return true;
        }

        /// <summary>Draws the throw origin and the trajectories at minimum and maximum speed, facing right.</summary>
        void OnDrawGizmosSelected()
        {
            Vector3 origin = GetOrigin(false);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, 0.05f);
            if (settings == null)
            {
                return;
            }
            Vector2 direction = GetDirection(false, settings.LaunchAngle);
            DrawTrajectory(origin, direction * settings.MinSpeed);
            DrawTrajectory(origin, direction * settings.MaxSpeed);
        }

        void DrawTrajectory(Vector3 origin, Vector2 velocity)
        {
            const int Segments = 30;
            const float Duration = 1.5f;
            Vector3 previous = origin;
            for (int i = 1; i <= Segments; i++)
            {
                float time = Duration * i / Segments;
                Vector3 point = origin + (Vector3)(velocity * time + 0.5f * settings.Gravity * time * time * Vector2.down);
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
        }
    }
}
