using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Keeps the player inside the visible area: the left edge of the camera pushes them forward.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CameraPush : MonoBehaviour
    {
        [SerializeField] CameraScroller scroller;

        Rigidbody2D body;

        void Awake() => body = GetComponent<Rigidbody2D>();

        void FixedUpdate()
        {
            if (scroller == null || scroller.Settings == null)
            {
                return;
            }

            float margin = scroller.Settings.EdgeMargin;
            float minX = scroller.LeftEdgeX + margin;
            float maxX = scroller.RightEdgeX - margin;
            Vector2 position = body.position;
            Vector2 velocity = body.linearVelocity;

            if (position.x < minX)
            {
                position.x = minX;
                velocity.x = Mathf.Max(velocity.x, scroller.CurrentSpeed);
            }
            else if (position.x > maxX)
            {
                position.x = maxX;
                velocity.x = Mathf.Min(velocity.x, 0f);
            }
            else
            {
                return;
            }

            body.position = position;
            body.linearVelocity = velocity;
        }
    }
}
