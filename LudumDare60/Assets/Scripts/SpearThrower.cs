using UnityEngine;
using UnityEngine.InputSystem;

namespace LD60
{
    /// <summary>
    /// Throws the held spear toward the facing direction (SpriteRenderer.flipX) when the Player/Attack action is released.
    /// The longer the input is held, the faster the spear is launched; a release before SpearThrowSettings.MinChargeTime cancels the throw.
    /// The player loses the weapon once it is thrown.
    /// Running is locked from the press until the end of the throwing animation.
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
        [SerializeField] SpearThrowSettings settings;
        [SerializeField] ThrownSpear spearPrefab;
        [SerializeField] CameraScroller scroller;

        [Tooltip("Start of the trajectory relative to the player, authored facing right.")]
        [SerializeField] Vector2 throwOrigin = new(0.3f, 0.2f);

        PlayerWeapon weapon;
        SpriteRenderer spriteRenderer;
        Animator animator;
        PlayerController controller;
        float chargeStartTime;
        bool charging;
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
        }

        void OnDisable()
        {
            attackAction?.action.Disable();
            charging = false;
            throwPending = false;
            throwing = false;
            UpdateAnimator(0f);
            UpdateRunLock();
        }

        void Update()
        {
            UpdateThrowingState();
            if (attackAction == null || settings == null)
            {
                return;
            }

            if (weapon.Weapon != WeaponType.Spear)
            {
                charging = false;
            }
            else if (attackAction.action.WasPressedThisFrame())
            {
                charging = true;
                chargeStartTime = Time.time;
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

            UpdateAnimator(charging ? Time.time - chargeStartTime : 0f);
            UpdateRunLock();
        }

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

        void UpdateRunLock()
        {
            if (controller != null)
            {
                controller.RunLocked = charging || throwPending || throwing;
            }
        }

        void Throw(float speed)
        {
            bool flip = spriteRenderer.flipX;
            weapon.Weapon = WeaponType.None;
            animator.SetTrigger(ThrowHash);
            throwPending = true;
            if (spearPrefab == null)
            {
                return;
            }

            ThrownSpear spear = Instantiate(spearPrefab, GetOrigin(flip), Quaternion.identity);
            spear.Launch(GetLaunchVelocity(flip, speed), settings, scroller);
        }

        Vector3 GetOrigin(bool flip)
        {
            return transform.position + new Vector3(flip ? -throwOrigin.x : throwOrigin.x, throwOrigin.y, 0f);
        }

        Vector2 GetLaunchVelocity(bool flip, float speed)
        {
            float angle = settings.LaunchAngle * Mathf.Deg2Rad;
            return new Vector2((flip ? -1f : 1f) * Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
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
            DrawTrajectory(origin, GetLaunchVelocity(false, settings.MinSpeed));
            DrawTrajectory(origin, GetLaunchVelocity(false, settings.MaxSpeed));
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
