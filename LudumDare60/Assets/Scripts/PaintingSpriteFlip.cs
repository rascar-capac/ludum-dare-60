using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Applies SpriteRenderer.flipX and the alpha of SpriteRenderer.color to the PaintingDecal shader, which ignores both:
    /// the sprite UV range and flip state are sent per renderer so the shader mirrors the UVs, and the alpha as _Opacity.
    /// Requires sprites imported with the Full Rect mesh type.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PaintingSpriteFlip : MonoBehaviour
    {
        static readonly int SpriteFlipRectId = Shader.PropertyToID("_SpriteFlipRect");
        static readonly int OpacityId = Shader.PropertyToID("_Opacity");

        SpriteRenderer spriteRenderer;
        MaterialPropertyBlock propertyBlock;
        Sprite appliedSprite;
        bool appliedFlip;
        float appliedAlpha;

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
            float alpha = spriteRenderer.color.a;
            if (sprite == null || (sprite == appliedSprite && flip == appliedFlip && alpha == appliedAlpha))
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
            propertyBlock.SetFloat(OpacityId, alpha);
            spriteRenderer.SetPropertyBlock(propertyBlock);
            appliedSprite = sprite;
            appliedFlip = flip;
            appliedAlpha = alpha;
        }
    }
}
