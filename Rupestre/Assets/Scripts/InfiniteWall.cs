using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Recycles the child tiles of the cave wall around the camera so the wall looks infinite.
    /// Tiles must be evenly spaced by tileWidth and cover more than the visible width.
    /// </summary>
    public class InfiniteWall : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField, Min(0.1f)] float tileWidth = 10f;
        [Tooltip("Level design backdrop: the camera frustum gizmo lines span its width.")]
        [SerializeField] Renderer levelBackground;

        void LateUpdate()
        {
            if (cameraTransform == null)
            {
                return;
            }

            float totalWidth = tileWidth * transform.childCount;
            float halfTotal = totalWidth * 0.5f;
            float cameraX = cameraTransform.position.x;

            foreach (Transform tile in transform)
            {
                Vector3 position = tile.position;
                while (position.x < cameraX - halfTotal)
                {
                    position.x += totalWidth;
                }
                while (position.x > cameraX + halfTotal)
                {
                    position.x -= totalWidth;
                }
                tile.position = position;
            }
        }

        /// <summary>Draws the lowest and highest heights of the wall the scrolling camera can show.</summary>
        void OnDrawGizmos()
        {
            if (cameraTransform == null || levelBackground == null || !cameraTransform.TryGetComponent(out Camera cam))
            {
                return;
            }

            Bounds bounds = levelBackground.bounds;
            float z = transform.position.z;

            // The camera only scrolls along x, so the visible band spans the extreme heights of its frustum corners on the wall.
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            for (int corner = 0; corner < 4; corner++)
            {
                float y = CameraScroller.ViewportToPlane(cam, corner & 1, corner >> 1, z).y;
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            Gizmos.color = Color.cyan;
            foreach (float y in new[] { minY, maxY })
            {
                Gizmos.DrawLine(new Vector3(bounds.min.x, y, z), new Vector3(bounds.max.x, y, z));
            }
        }
    }
}
