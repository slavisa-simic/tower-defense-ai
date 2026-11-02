Shader "BorderDefenders/PixelShadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite alpha silhouette", 2D) = "white" {}
        _Color ("Neutral shadow tone / opacity", Color) = (0.12,0.15,0.16,0.40)
        _OffsetPixels ("World-aligned offset at 51.2 PPU", Vector) = (8,-6,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            struct Attributes { COMMON_2D_INPUTS half4 color : COLOR; };
            struct Varyings { COMMON_2D_OUTPUTS half4 color : COLOR; };
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _OffsetPixels;
            CBUFFER_END
            Varyings ShadowVertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                Varyings output = (Varyings)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                float3 world = TransformObjectToWorld(input.positionOS);
                // Apply after rotation/scale: unit facing never rotates the light direction.
                world.xy += _OffsetPixels.xy / 51.2;
                output.positionCS = TransformWorldToHClip(world);
                output.uv = input.uv;
                output.color = input.color * unity_SpriteColor * _Color;
                return output;
            }
            half4 ShadowFragment(Varyings input) : SV_Target
            {
                // Reuse source alpha only. Painted colors must never become a second ghost sprite.
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                return half4(input.color.rgb, alpha * input.color.a);
            }
            ENDHLSL
        }
    }
}
