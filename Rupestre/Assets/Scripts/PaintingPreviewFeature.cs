using UnityEngine;

using UnityEngine.Rendering.Universal;

namespace LD60
{
    /// <summary>
    /// Render Objects feature limited to the Scene view: draws the PaintLayer pass of the paintings flat over the scene,
    /// so they stay visible for level design outside the area captured by PaintLayerCapture.
    /// </summary>
    public class PaintingPreviewFeature : RenderObjects
    {
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                base.AddRenderPasses(renderer, ref renderingData);
            }
        }
    }
}
