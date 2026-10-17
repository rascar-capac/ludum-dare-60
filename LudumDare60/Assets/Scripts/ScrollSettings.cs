using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Side scrolling tuning. Per-section speeds are set by ScrollSpeedZone in the scene.
    /// </summary>
    [CreateAssetMenu(fileName = "ScrollSettings", menuName = "LD60/Scroll Settings")]
    public class ScrollSettings : ScriptableObject
    {
        [Tooltip("Scroll speed outside any ScrollSpeedZone.")]
        [SerializeField, Min(0f)] float defaultSpeed = 2f;

        [Tooltip("Distance kept between the player and the left/right edges of the screen.")]
        [SerializeField, Min(0f)] float edgeMargin = 0.5f;

        public float DefaultSpeed => defaultSpeed;
        public float EdgeMargin => edgeMargin;
    }
}
