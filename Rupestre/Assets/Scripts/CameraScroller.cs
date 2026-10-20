using System.Collections;
using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Moves the camera to the right at the speed of the current ScrollSpeedZone,
    /// or ScrollSettings.DefaultSpeed outside any zone.
    /// The gameplay plane is z = 0. The camera can be rotated: its edges are the intersections of its frustum with that plane.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraScroller : MonoBehaviour
    {
        [SerializeField] ScrollSettings settings;
        [SerializeField] bool scrolling = true;

        Camera cam;
        Vector3 startPosition;

        public float CurrentSpeed { get; private set; }
        public Camera Camera
        {
            get
            {
                if (cam == null)
                {
                    cam = GetComponent<Camera>();
                }
                return cam;
            }
        }
        public ScrollSettings Settings => settings;
        public bool Scrolling
        {
            get => scrolling;
            set => scrolling = value;
        }
        /// <summary>Debug pause, independent from Scrolling so that the respawn sequence does not resume it.</summary>
        public bool DebugPaused { get; set; }

        void Awake()
        {
            cam = GetComponent<Camera>();
            startPosition = transform.position;
        }

        void Update()
        {
            CurrentSpeed = scrolling && !DebugPaused && settings != null ? ScrollSpeedZone.GetSpeedAt(transform.position.x, settings.DefaultSpeed) : 0f;
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

        /// <summary>x of the left edge of the view on the gameplay plane at the given height.</summary>
        public float LeftEdgeXAt(float y) => XAtY(ViewportToPlane(Camera, 0f, 0f, 0f), ViewportToPlane(Camera, 0f, 1f, 0f), y);

        /// <summary>x of the right edge of the view on the gameplay plane at the given height.</summary>
        public float RightEdgeXAt(float y) => XAtY(ViewportToPlane(Camera, 1f, 0f, 0f), ViewportToPlane(Camera, 1f, 1f, 0f), y);

        /// <summary>y of the bottom edge of the view on the gameplay plane at the given x.</summary>
        public float BottomEdgeYAt(float x) => YAtX(ViewportToPlane(Camera, 0f, 0f, 0f), ViewportToPlane(Camera, 1f, 0f, 0f), x);

        /// <summary>Camera x that shows the given world x at the given horizontal viewport position, mid-height on the gameplay plane.</summary>
        public float CameraXFor(float x, float viewportX) => x + transform.position.x - ViewportToPlane(Camera, viewportX, 0.5f, 0f).x;

        /// <summary>Point of the plane at the given world z seen at the given viewport position.</summary>
        public static Vector3 ViewportToPlane(Camera camera, float viewportX, float viewportY, float z)
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0f));
            return ray.GetPoint((z - ray.origin.z) / ray.direction.z);
        }

        static float XAtY(Vector3 a, Vector3 b, float y) => Mathf.Approximately(a.y, b.y) ? a.x : Mathf.LerpUnclamped(a.x, b.x, (y - a.y) / (b.y - a.y));

        static float YAtX(Vector3 a, Vector3 b, float x) => Mathf.Approximately(a.x, b.x) ? a.y : Mathf.LerpUnclamped(a.y, b.y, (x - a.x) / (b.x - a.x));
    }
}
