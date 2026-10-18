using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Applies SpriteRenderer.flipX and SpriteRenderer.color to the PaintingDecal shader, which ignores both:
    /// the sprite UV range and flip state are sent per renderer so the shader mirrors the UVs, the color as _Tint
    /// and its alpha as _Opacity. Flash is sent as _Flash, which blends the painting color towards white.
    /// Requires sprites imported with the Full Rect mesh type.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PaintingSpriteSync : MonoBehaviour
    {
        static readonly int SpriteFlipRectId = Shader.PropertyToID("_SpriteFlipRect");
        static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        static readonly int TintId = Shader.PropertyToID("_Tint");
        static readonly int FlashId = Shader.PropertyToID("_Flash");

        SpriteRenderer spriteRenderer;
        MaterialPropertyBlock propertyBlock;
        Sprite appliedSprite;
        bool appliedFlip;
        Color appliedColor;
        float appliedFlash;

        /// <summary>Blend of the painting color towards white, from 0 (original color) to 1 (white).</summary>
        public float Flash { get; set; }

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
            Color color = spriteRenderer.color;
            if (sprite == null || (sprite == appliedSprite && flip == appliedFlip && color == appliedColor && Flash == appliedFlash))
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
            propertyBlock.SetColor(TintId, color);
            propertyBlock.SetFloat(OpacityId, color.a);
            propertyBlock.SetFloat(FlashId, Mathf.Clamp01(Flash));
            spriteRenderer.SetPropertyBlock(propertyBlock);
            appliedSprite = sprite;
            appliedFlip = flip;
            appliedColor = color;
            appliedFlash = Flash;
        }
    }
}
