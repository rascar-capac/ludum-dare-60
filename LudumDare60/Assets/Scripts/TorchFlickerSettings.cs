using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Flicker tuning shared by all torch lights. Base intensity, range and position stay on each light in the scene.
    /// </summary>
    [CreateAssetMenu(fileName = "TorchFlickerSettings", menuName = "LD60/Torch Flicker Settings")]
    public class TorchFlickerSettings : ScriptableObject
    {
        [Tooltip("Speed of the slow sway of the flame.")]
        [SerializeField, Min(0f)] float swayFrequency = 1.2f;

        [Tooltip("Intensity variation of the slow sway, as a fraction of the base intensity.")]
        [SerializeField, Range(0f, 1f)] float swayAmplitude = 0.15f;

        [Tooltip("Speed of the fast crackle of the flame.")]
        [SerializeField, Min(0f)] float crackleFrequency = 9f;

        [Tooltip("Intensity variation of the fast crackle, as a fraction of the base intensity.")]
        [SerializeField, Range(0f, 1f)] float crackleAmplitude = 0.1f;

        [Tooltip("Range variation, as a fraction of the base range. Follows the intensity flicker.")]
        [SerializeField, Range(0f, 1f)] float rangeAmplitude = 0.05f;

        [Tooltip("Maximum offset of the light from its base position, so the lit area wobbles on the wall.")]
        [SerializeField, Min(0f)] float positionJitter = 0.15f;

        [Tooltip("Speed of the position wobble.")]
        [SerializeField, Min(0f)] float positionJitterFrequency = 1.2f;

        [Tooltip("Smoothing time of the torch following the avatar offset from the screen center. 0 follows instantly.")]
        [SerializeField, Min(0f)] float avatarFollowDamping = 0.5f;

        public float SwayFrequency => swayFrequency;
        public float SwayAmplitude => swayAmplitude;
        public float CrackleFrequency => crackleFrequency;
        public float CrackleAmplitude => crackleAmplitude;
        public float RangeAmplitude => rangeAmplitude;
        public float PositionJitter => positionJitter;
        public float PositionJitterFrequency => positionJitterFrequency;
        public float AvatarFollowDamping => avatarFollowDamping;
    }
}
