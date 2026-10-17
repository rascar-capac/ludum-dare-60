using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Keeps the player inside the visible area: the right edge of the camera blocks them.
    /// The left edge kills them instead (see PlayerDeath).
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

            float maxX = scroller.RightEdgeX - scroller.Settings.EdgeMargin;
            Vector2 position = body.position;
            if (position.x <= maxX)
            {
                return;
            }

            Vector2 velocity = body.linearVelocity;
            position.x = maxX;
            velocity.x = Mathf.Min(velocity.x, 0f);
            body.position = position;
            body.linearVelocity = velocity;
        }
    }
}
