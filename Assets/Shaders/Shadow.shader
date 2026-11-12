Shader "Custom/Shadow"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [Header(Shadow Settings)]
        _ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
        _ShadowAlpha("Shadow Alpha", Range(0, 1)) = 0.6
        _ShadowAngle("Angle (Degrees)", Range(0, 360)) = 315.0
        _ShadowDistance("Distance", Range(0, 0.2)) = 0.03
        _ShadowBlur("Blur Radius", Range(0, 0.05)) = 0.01
        _ShadowSpread("Spread (Thickness)", Range(0, 1)) = 0.2

        // Required Unity UI Properties (Prevents Console Warnings & Supports Masks)
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _BaseColorMask("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { 
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "RenderType" = "Plane" 
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 worldPos     : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _ShadowColor;
                float _ShadowAlpha;
                float _ShadowAngle;
                float _ShadowDistance;
                float _ShadowBlur;
                float _ShadowSpread;
                float4 _ClipRect;
            CBUFFER_END

            half4 SampleSpriteSafe(float2 uv)
            {
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return half4(0, 0, 0, 0);

                return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
            }

            Varyings vert(Attributes IN)
            {
                Varyings output;
                output.worldPos = IN.positionOS;

                // 1. Calculate required UV padding
                float uvPad = _ShadowDistance + _ShadowBlur;
                float scaleFactor = 1.0 + (uvPad * 2.0);

                // 2. Expand UVs cleanly from the center (0.5, 0.5)
                float2 uvOffsetFromCenter = IN.uv - 0.5;
                float2 expandedUV = 0.5 + (uvOffsetFromCenter * scaleFactor);
                float2 uvDelta = expandedUV - IN.uv;

                // 3. Expand UI vertices using the sprite's native pixel dimensions
                // This avoids relying on pivot-dependent vertex positions
                float4 posOS = IN.positionOS;
                posOS.xy += uvDelta * _BaseMap_TexelSize.zw;

                output.positionCS = TransformObjectToHClip(posOS.xyz);
                output.uv = TRANSFORM_TEX(expandedUV, _BaseMap);
                output.color = IN.color * _BaseColor;

                return output;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1. Sample main sprite
                half4 mainColor = SampleSpriteSafe(IN.uv) * IN.color;

                // 2. Calculate directional offset
                float rad = radians(_ShadowAngle);
                float2 direction = float2(cos(rad), sin(rad));
                float2 shadowUV = IN.uv - (direction * _ShadowDistance);

                // 3. Apply Blur
                float shadowMask = 0.0;
                if (_ShadowBlur > 0.0001)
                {
                    const int SAMPLES = 12;
                    const float TAU = 6.28318530718;

                    for (int i = 0; i < SAMPLES; i++)
                    {
                        float angle = (TAU / SAMPLES) * i;
                        float2 blurOffset = float2(cos(angle), sin(angle)) * _ShadowBlur;
                        shadowMask += SampleSpriteSafe(shadowUV + blurOffset).a;
                    }
                    shadowMask /= SAMPLES;
                }
                else
                {
                    shadowMask = SampleSpriteSafe(shadowUV).a;
                }

                // 4. Apply Spread
                float spreadThreshold = max(0.001, 1.0 - _ShadowSpread);
                shadowMask = smoothstep(0.0, spreadThreshold, shadowMask);

                // 5. Final Shadow Color
                float finalShadowAlpha = shadowMask * _ShadowAlpha * IN.color.a;
                half4 shadow = half4(_ShadowColor.rgb * finalShadowAlpha, finalShadowAlpha);

                // 6. Composite Main Sprite over Shadow
                mainColor.rgb *= mainColor.a;
                half4 finalColor = mainColor + shadow * (1.0 - mainColor.a);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
