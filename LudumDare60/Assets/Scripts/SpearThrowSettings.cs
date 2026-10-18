using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Charge and flight tuning of the thrown spear.
    /// </summary>
    [CreateAssetMenu(fileName = "SpearThrowSettings", menuName = "LD60/Spear Throw Settings")]
    public class SpearThrowSettings : ScriptableObject
    {
        [Header("Charge")]
        [Tooltip("Hold time giving the minimum launch speed. The throw is cancelled when the input is released before.")]
        [SerializeField, Min(0f)] float minChargeTime = 0.15f;

        [Tooltip("Hold time giving the maximum launch speed. Holding longer does not throw further.")]
        [SerializeField, Min(0f)] float maxChargeTime = 0.8f;

        [Header("Launch")]
        [SerializeField, Min(0f)] float minSpeed = 6f;
        [SerializeField, Min(0f)] float maxSpeed = 16f;

        [Tooltip("Angle of the launch direction above the horizontal, in degrees, toward the facing direction, when not aiming with the pointer.")]
        [SerializeField, Range(-90f, 90f)] float launchAngle = 20f;

        [Tooltip("Lowest launch angle above the horizontal, in degrees, reachable when aiming with the pointer.")]
        [SerializeField, Range(-90f, 90f)] float minAimAngle = -30f;

        [Tooltip("Highest launch angle above the horizontal, in degrees, reachable when aiming with the pointer.")]
        [SerializeField, Range(-90f, 90f)] float maxAimAngle = 75f;

        [Header("Flight")]
        [SerializeField, Min(0f)] float gravity = 20f;

        [Tooltip("Minimum speed of the spear for an enemy it touches to be killed.")]
        [SerializeField, Min(0f)] float minKillSpeed = 4f;

        public float MinChargeTime => minChargeTime;
        public float MaxChargeTime => maxChargeTime;
        public float MinSpeed => minSpeed;
        public float MaxSpeed => maxSpeed;
        public float LaunchAngle => launchAngle;
        public float MinAimAngle => minAimAngle;
        public float MaxAimAngle => maxAimAngle;
        public float Gravity => gravity;
        public float MinKillSpeed => minKillSpeed;

        /// <summary>Launch speed for an input held during the given time, or null when the throw is cancelled.</summary>
        public float? GetLaunchSpeed(float holdTime)
        {
            if (holdTime < minChargeTime)
            {
                return null;
            }
            float t = maxChargeTime > minChargeTime ? Mathf.InverseLerp(minChargeTime, maxChargeTime, holdTime) : 1f;
            return Mathf.Lerp(minSpeed, maxSpeed, t);
        }

        /// <summary>Rigidbody2D.gravityScale that produces gravity with the project's Physics2D gravity.</summary>
        public float GravityScale
        {
            get
            {
                float worldGravity = Mathf.Abs(Physics2D.gravity.y);
                return worldGravity > 0f ? gravity / worldGravity : 0f;
            }
        }

        void OnValidate()
        {
            maxChargeTime = Mathf.Max(maxChargeTime, minChargeTime);
            maxSpeed = Mathf.Max(maxSpeed, minSpeed);
            maxAimAngle = Mathf.Max(maxAimAngle, minAimAngle);
        }
    }
}
