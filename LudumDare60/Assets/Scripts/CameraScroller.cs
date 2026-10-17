using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Moves the camera to the right at the speed of the current ScrollSpeedZone,
    /// or ScrollSettings.DefaultSpeed outside any zone.
    /// The gameplay plane is z = 0.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraScroller : MonoBehaviour
    {
        [SerializeField] ScrollSettings settings;
        [SerializeField] bool scrolling = true;

        Camera cam;

        public float CurrentSpeed { get; private set; }
        public float LeftEdgeX => transform.position.x - HalfWidthAt(0f);
        public float RightEdgeX => transform.position.x + HalfWidthAt(0f);
        public Camera Camera => cam;
        public ScrollSettings Settings => settings;

        void Awake() => cam = GetComponent<Camera>();

        void Update()
        {
            CurrentSpeed = scrolling && settings != null ? ScrollSpeedZone.GetSpeedAt(transform.position.x, settings.DefaultSpeed) : 0f;
            transform.position += Vector3.right * (CurrentSpeed * Time.deltaTime);
        }

        /// <summary>Half of the visible width on the plane at the given world z.</summary>
        public float HalfWidthAt(float z)
        {
            if (cam == null)
            {
                cam = GetComponent<Camera>();
            }
            float distance = Mathf.Abs(z - transform.position.z);
            float halfHeight = distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return halfHeight * cam.aspect;
        }
    }
}
