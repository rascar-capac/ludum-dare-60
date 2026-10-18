using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Front arm of the avatar, drawn apart from the body in the avatar_aiming_max state and pointing in the launch direction
    /// of the SpearThrower. Shown only while the avatar Animator is in that state; its sprite frames are keyframed in the clip.
    /// The sprite is authored pointing right, with its pivot on the shoulder.
    /// As the PaintingDecal shader mirrors the UVs around the sprite center rather than its pivot, the arm is shifted when
    /// flipped so that it stays on the shoulder. Copies the opacity and flash of the avatar.
    /// Runs before PaintingSpriteSync so the arm's shader values are up to date in the same frame.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(SpriteRenderer))]
    public class AimingArm : MonoBehaviour
    {
        static readonly int AimingMaxStateHash = Animator.StringToHash("avatar_aiming_max");

        [SerializeField] SpearThrower thrower;
        [Tooltip("Position of the shoulder relative to the avatar, authored facing right.")]
        [SerializeField] Vector2 shoulderOffset = new(0f, 0.7f);

        SpriteRenderer spriteRenderer;
        PaintingSpriteSync sync;
        SpriteRenderer holder;
        Animator holderAnimator;
        PaintingSpriteSync holderSync;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            sync = GetComponent<PaintingSpriteSync>();
            if (thrower != null)
            {
                holder = thrower.GetComponent<SpriteRenderer>();
                holderAnimator = thrower.GetComponent<Animator>();
                holderSync = thrower.GetComponent<PaintingSpriteSync>();
            }
        }

        void LateUpdate()
        {
            bool aiming = thrower != null && holderAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == AimingMaxStateHash;
            spriteRenderer.enabled = aiming && holder.enabled;
            Sprite sprite = spriteRenderer.sprite;
            if (!spriteRenderer.enabled || sprite == null)
            {
                return;
            }

            bool flip = holder.flipX;
            Quaternion rotation = Quaternion.Euler(0f, 0f, flip ? -thrower.AimAngle : thrower.AimAngle);
            Vector3 position = new(flip ? -shoulderOffset.x : shoulderOffset.x, shoulderOffset.y, transform.localPosition.z);
            if (flip)
            {
                // Mirroring around the pivot instead of the sprite center.
                float shift = (2f * sprite.pivot.x - sprite.rect.width) / sprite.pixelsPerUnit;
                position += rotation * new Vector3(shift, 0f, 0f);
            }
            transform.SetLocalPositionAndRotation(position, rotation);
            spriteRenderer.flipX = flip;

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
