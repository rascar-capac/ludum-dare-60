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
    }
}
