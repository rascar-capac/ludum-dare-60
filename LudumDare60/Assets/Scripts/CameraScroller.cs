using System.Collections;
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
        Vector3 startPosition;

        public float CurrentSpeed { get; private set; }
        public float LeftEdgeX => transform.position.x - HalfWidthAt(0f);
        public float RightEdgeX => transform.position.x + HalfWidthAt(0f);
        public float BottomEdgeY => transform.position.y - HalfHeightAt(0f);
        public Camera Camera => cam;
        public ScrollSettings Settings => settings;
        public bool Scrolling
        {
            get => scrolling;
            set => scrolling = value;
        }

        void Awake()
        {
            cam = GetComponent<Camera>();
            startPosition = transform.position;
        }

        void Update()
        {
            CurrentSpeed = scrolling && settings != null ? ScrollSpeedZone.GetSpeedAt(transform.position.x, settings.DefaultSpeed) : 0f;
            transform.position += Vector3.right * (CurrentSpeed * Time.deltaTime);
        }

        /// <summary>Stops scrolling and moves the camera to the given x, at its start height and depth, with an ease in-out.</summary>
        public IEnumerator ReturnTo(float x, float duration)
        {
            scrolling = false;
            CurrentSpeed = 0f;
            Vector3 from = transform.position;
            Vector3 to = new(x, startPosition.y, startPosition.z);
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, time / duration));
                yield return null;
            }
            MoveTo(x);
        }

        /// <summary>Places the camera instantly at the given x, at its start height and depth.</summary>
        public void MoveTo(float x) => transform.position = new Vector3(x, startPosition.y, startPosition.z);

        /// <summary>Half of the visible width on the plane at the given world z.</summary>
        public float HalfWidthAt(float z) => HalfHeightAt(z) * Camera.aspect;

        /// <summary>Half of the visible height on the plane at the given world z.</summary>
        public float HalfHeightAt(float z)
        {
            if (cam == null)
            {
                cam = GetComponent<Camera>();
            }
            float distance = Mathf.Abs(z - transform.position.z);
            return distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
    }
}
