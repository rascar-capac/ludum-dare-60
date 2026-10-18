using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Keeps a painting held by the avatar (child SpriteRenderer, e.g. a spear) in its hand without sharing its tint:
    /// mirrors the hand offset, hand angle and flipX of the parent, and copies its visibility, opacity and flash.
    /// The hand offset and angle are authored facing right and can be keyframed in the avatar animation clips.
    /// Runs before PaintingSpriteSync so the item's shader values are up to date in the same frame.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(SpriteRenderer))]
    public class HeldItem : MonoBehaviour
    {
        [SerializeField] SpriteRenderer holder;
        [SerializeField] Vector2 handOffset;
        [Tooltip("Rotation around Z in degrees, counterclockwise.")]
        [SerializeField] float handAngle;

        SpriteRenderer spriteRenderer;
        PaintingSpriteSync sync;
        PaintingSpriteSync holderSync;

        void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            sync = GetComponent<PaintingSpriteSync>();
            holderSync = holder != null ? holder.GetComponent<PaintingSpriteSync>() : null;
        }

        void LateUpdate()
        {
            if (holder == null)
            {
                return;
            }

            bool flip = holder.flipX;
            transform.localPosition = new Vector3(flip ? -handOffset.x : handOffset.x, handOffset.y, transform.localPosition.z);
            transform.localRotation = Quaternion.Euler(0f, 0f, flip ? -handAngle : handAngle);
            spriteRenderer.flipX = flip;
            spriteRenderer.enabled = holder.enabled;

            Color color = spriteRenderer.color;
            color.a = holder.color.a;
            spriteRenderer.color = color;

            if (sync != null && holderSync != null)
            {
                sync.Flash = holderSync.Flash;
            }
        }
    }
}
