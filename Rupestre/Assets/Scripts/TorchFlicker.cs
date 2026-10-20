using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Makes a light flicker like a torch flame: slow sway plus fast crackle on intensity and range,
    /// and a small wobble of its position. Each torch gets its own noise offset so they never sync.
    /// The torch follows the avatar with damping, keeping its offset from the avatar in the scene.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class TorchFlicker : MonoBehaviour
    {
        [SerializeField] TorchFlickerSettings settings;
        [SerializeField] Transform avatar;

        Light torchLight;
        float baseIntensity;
        float baseRange;
        float seed;
        Vector3 avatarOffset;
        Vector3 followPosition;
        Vector3 followVelocity;

        void Awake()
        {
            torchLight = GetComponent<Light>();
            baseIntensity = torchLight.intensity;
            baseRange = torchLight.range;
            // Read before PlayerDeath moves the avatar to its spawn point in Start.
            avatarOffset = avatar != null ? transform.position - avatar.position : Vector3.zero;
            followPosition = transform.position;
            seed = Random.Range(0f, 1000f);
        }

        void OnDisable()
        {
            if (torchLight == null)
            {
                return;
            }

            torchLight.intensity = baseIntensity;
            torchLight.range = baseRange;
        }

        void Start() => followPosition = TargetPosition();

        void LateUpdate()
        {
            if (settings == null)
            {
                return;
            }

            float time = Time.time;
            float sway = Noise(time * settings.SwayFrequency, 0f) * settings.SwayAmplitude;
            float crackle = Noise(time * settings.CrackleFrequency, 10f) * settings.CrackleAmplitude;
            float flicker = sway + crackle;

            torchLight.intensity = baseIntensity * Mathf.Max(0f, 1f + flicker);
            torchLight.range = baseRange * Mathf.Max(0f, 1f + flicker * settings.RangeAmplitude);

            float jitterTime = time * settings.PositionJitterFrequency;
            Vector3 jitter = new Vector3(Noise(jitterTime, 20f), Noise(jitterTime, 30f), 0f) * settings.PositionJitter;

            followPosition = Vector3.SmoothDamp(followPosition, TargetPosition(), ref followVelocity, settings.AvatarFollowDamping);

            transform.position = followPosition + jitter;
        }

        Vector3 TargetPosition() => avatar != null ? avatar.position + avatarOffset : followPosition;

        // Perlin noise remapped to [-1, 1].
        float Noise(float x, float channel) => Mathf.PerlinNoise(seed + x, seed + channel) * 2f - 1f;
    }
}
