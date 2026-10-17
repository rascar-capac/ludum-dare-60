using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Applies SpriteRenderer.flipX to the PaintingDecal shader, which ignores the sprite flip:
    /// the sprite UV range and flip state are sent per renderer so the shader mirrors the UVs.
    /// Requires sprites imported with the Full Rect mesh type.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PaintingSpriteFlip : MonoBehaviour
    {
        static readonly int SpriteFlipRectId = Shader.PropertyToID("_SpriteFlipRect");

        SpriteRenderer spriteRenderer;
        MaterialPropertyBlock propertyBlock;
        Sprite appliedSprite;
        bool appliedFlip;

        void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            propertyBlock ??= new MaterialPropertyBlock();
            appliedSprite = null;
        }

        void LateUpdate()
        {
            Sprite sprite = spriteRenderer.sprite;
            bool flip = spriteRenderer.flipX;
            if (sprite == null || (sprite == appliedSprite && flip == appliedFlip))
            {
                return;
            }

            float uMin = float.MaxValue;
            float uMax = float.MinValue;
            foreach (Vector2 uv in sprite.uv)
            {
                uMin = Mathf.Min(uMin, uv.x);
                uMax = Mathf.Max(uMax, uv.x);
            }

            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(SpriteFlipRectId, new Vector4(uMin, uMax, flip ? 1f : 0f, 0f));
            spriteRenderer.SetPropertyBlock(propertyBlock);
            appliedSprite = sprite;
            appliedFlip = flip;
        }
    }
}
