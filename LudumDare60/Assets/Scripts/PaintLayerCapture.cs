using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Orthographic camera rendering the paintings (PaintLayer pass of the Painting shader) into its target texture,
    /// which the PaintedWall shader projects onto the wall along the Z axis. Follows the view camera and covers its
    /// frustum down to maxWallDepth behind the wall plane, snapped to the texels so the paintings don't shimmer when scrolling.
    /// Publishes the texture and its world rect as the _PaintLayer and _PaintLayerRect shader globals.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class PaintLayerCapture : MonoBehaviour
    {
        static readonly int PaintLayerId = Shader.PropertyToID("_PaintLayer");
        static readonly int PaintLayerRectId = Shader.PropertyToID("_PaintLayerRect");

        [SerializeField] Camera viewCamera;
        [Tooltip("Plane of the paintings and of the wall surface.")]
        [SerializeField] Transform wall;
        [Tooltip("How far wall geometry can go behind the wall plane while still receiving the paintings.")]
        [SerializeField, Min(0f)] float maxWallDepth = 1f;

        Camera captureCamera;

        void OnEnable()
        {
            captureCamera = GetComponent<Camera>();
        }

        void OnDisable()
        {
            Shader.SetGlobalVector(PaintLayerRectId, Vector4.zero);
        }

        void LateUpdate()
        {
            RenderTexture target = captureCamera.targetTexture;
            if (viewCamera == null || wall == null || target == null)
            {
                Shader.SetGlobalVector(PaintLayerRectId, Vector4.zero);
                return;
            }

            Vector3 viewPosition = viewCamera.transform.position;
            float distance = wall.position.z + maxWallDepth - viewPosition.z;
            float halfHeight = distance * Mathf.Tan(viewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float textureAspect = (float)target.width / target.height;
            float orthographicSize = Mathf.Max(halfHeight, halfHeight * viewCamera.aspect / textureAspect);
            float texelSize = 2f * orthographicSize / target.height;

            Vector2 center = new(
                Mathf.Round(viewPosition.x / texelSize) * texelSize,
                Mathf.Round(viewPosition.y / texelSize) * texelSize);
            transform.SetPositionAndRotation(new Vector3(center.x, center.y, viewPosition.z), Quaternion.identity);
            captureCamera.orthographic = true;
            captureCamera.orthographicSize = orthographicSize;

            Vector2 size = new(2f * orthographicSize * textureAspect, 2f * orthographicSize);
            Vector2 min = center - size * 0.5f;
            Shader.SetGlobalTexture(PaintLayerId, target);
            Shader.SetGlobalVector(PaintLayerRectId, new Vector4(min.x, min.y, 1f / size.x, 1f / size.y));
        }
    }
}
