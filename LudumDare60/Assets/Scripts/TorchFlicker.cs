using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Makes a light flicker like a torch flame: slow sway plus fast crackle on intensity and range,
    /// and a small wobble of its local position. Each torch gets its own noise offset so they never sync.
    /// The torch also follows, with damping, the offset of the avatar from the screen center (the parent camera axis).
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class TorchFlicker : MonoBehaviour
    {
        [SerializeField] TorchFlickerSettings settings;
        [SerializeField] Transform avatar;

        Light torchLight;
        float baseIntensity;
        float baseRange;
        Vector3 baseLocalPosition;
        float seed;
        Vector3 avatarOffset;
        Vector3 avatarOffsetVelocity;

        void Awake()
        {
            torchLight = GetComponent<Light>();
            baseIntensity = torchLight.intensity;
            baseRange = torchLight.range;
            baseLocalPosition = transform.localPosition;
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
            transform.localPosition = baseLocalPosition;
        }

        void Start() => avatarOffset = AvatarOffset();

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

            avatarOffset = Vector3.SmoothDamp(avatarOffset, AvatarOffset(), ref avatarOffsetVelocity, settings.AvatarFollowDamping);

            transform.localPosition = baseLocalPosition + jitter + avatarOffset;
        }

        // Avatar position relative to the screen center, in the parent camera space.
        Vector3 AvatarOffset()
        {
            if (avatar == null || transform.parent == null)
            {
                return Vector3.zero;
            }

            Vector3 offset = transform.parent.InverseTransformPoint(avatar.position);
            offset.z = 0f;
            return offset;
        }

        // Perlin noise remapped to [-1, 1].
        float Noise(float x, float channel) => Mathf.PerlinNoise(seed + x, seed + channel) * 2f - 1f;
    }
}
