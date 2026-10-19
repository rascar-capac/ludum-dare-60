// Cave painting sprite. Not drawn by the regular URP passes: its PaintLayer pass is rendered by the paint layer camera
// into the paint layer texture, which the wall shader blends into its albedo (see PaintLayerCapture and PaintedWall.hlsl).
// Outputs premultiplied colors so the texture keeps the coverage of the paintings in its alpha channel.
// _Flash (0-1, set per renderer by PaintingFlash) blends the painting color towards white.
Shader "LD60/Painting"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _Flash ("Flash", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Pass
        {
            Name "PaintLayer"
            Tags { "LightMode" = "PaintLayer" }

            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half _Flash;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                // SpriteRenderer flipX/flipY and color are per renderer constants, not baked into the vertices.
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.color = input.color * unity_SpriteColor;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                color.rgb = lerp(color.rgb, 1.0, _Flash);
                return half4(color.rgb * color.a, color.a);
            }
            ENDHLSL
        }
    }
}
