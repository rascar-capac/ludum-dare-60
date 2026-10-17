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

        /// <summary>Draws the top and bottom intersections of the camera frustum with the wall.</summary>
        void OnDrawGizmos()
        {
            if (cameraTransform == null || levelBackground == null || !cameraTransform.TryGetComponent(out Camera cam))
            {
                return;
            }

            Vector3 cameraPosition = cameraTransform.position;
            float distance = Mathf.Abs(transform.position.z - cameraPosition.z);
            float halfHeight = distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            Bounds bounds = levelBackground.bounds;
            float z = transform.position.z;

            Gizmos.color = Color.cyan;
            foreach (float y in new[] { cameraPosition.y + halfHeight, cameraPosition.y - halfHeight })
            {
                Gizmos.DrawLine(new Vector3(bounds.min.x, y, z), new Vector3(bounds.max.x, y, z));
            }
        }
    }
}
