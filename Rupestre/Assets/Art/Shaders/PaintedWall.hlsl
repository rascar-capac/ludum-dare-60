#ifndef LD60_PAINTED_WALL_INCLUDED
#define LD60_PAINTED_WALL_INCLUDED

// Paint layer rendered by PaintLayerCapture: premultiplied painting colors, projected onto the wall along the Z axis.
// _PaintLayerRect = (min X, min Y, 1 / width, 1 / height) of the captured area in world space, zero when nothing is captured.
TEXTURE2D(_PaintLayer);
SAMPLER(sampler_PaintLayer);
float4 _PaintLayerRect;

// Wall surface with the same maps and channels as the URP Lit shader (one tiling/offset for all maps, metallic in R,
// smoothness in A), the paintings replacing the albedo only so they keep the relief and lighting of the wall.
void PaintedWall_float(UnityTexture2D BaseMap, float4 BaseColor, UnityTexture2D NormalMap, float NormalScale,
    UnityTexture2D MetallicGlossMap, float Smoothness, float2 UV, float3 PositionWS,
    out float3 Albedo, out float3 NormalTS, out float Metallic, out float SmoothnessOut)
{
    float2 uv = BaseMap.GetTransformedUV(UV);
    Albedo = SAMPLE_TEXTURE2D(BaseMap.tex, BaseMap.samplerstate, uv).rgb * BaseColor.rgb;
    NormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(NormalMap.tex, NormalMap.samplerstate, uv), NormalScale);
    float4 metallicGloss = SAMPLE_TEXTURE2D(MetallicGlossMap.tex, MetallicGlossMap.samplerstate, uv);
    Metallic = metallicGloss.r;
    SmoothnessOut = metallicGloss.a * Smoothness;

    float2 paintUV = (PositionWS.xy - _PaintLayerRect.xy) * _PaintLayerRect.zw;
    float inside = all(paintUV >= 0.0) && all(paintUV <= 1.0) && _PaintLayerRect.z > 0.0 ? 1.0 : 0.0;
    float4 paint = SAMPLE_TEXTURE2D(_PaintLayer, sampler_PaintLayer, paintUV) * inside;
    Albedo = Albedo * (1.0 - paint.a) + paint.rgb;
}

#endif
