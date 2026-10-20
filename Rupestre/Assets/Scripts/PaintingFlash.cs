using UnityEngine;

namespace LD60
{
    /// <summary>
    /// Sends Flash to the Painting shader as _Flash, which blends the painting color towards white.
    /// Uses a material instance created on the first flash rather than a MaterialPropertyBlock: with a property block,
    /// the SpriteRenderer no longer provides its flip and color to the shader and the painting disappears.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PaintingFlash : MonoBehaviour
    {
        static readonly int FlashId = Shader.PropertyToID("_Flash");

        SpriteRenderer spriteRenderer;
        Material material;
        float appliedFlash;

        /// <summary>Blend of the painting color towards white, from 0 (original color) to 1 (white).</summary>
        public float Flash { get; set; }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnDestroy()
        {
            Destroy(material);
        }

        void LateUpdate()
        {
            if (Flash == appliedFlash)
            {
                return;
            }

            if (material == null)
            {
                material = new Material(spriteRenderer.sharedMaterial);
                spriteRenderer.sharedMaterial = material;
            }

            material.SetFloat(FlashId, Mathf.Clamp01(Flash));
            appliedFlash = Flash;
        }
    }
}
