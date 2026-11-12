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
                float4 uvBounds     : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _ShadowColor;
                float _ShadowAlpha;
                float _ShadowAngle;
                float _ShadowDistance;
                float _ShadowBlur;
                float _ShadowSpread;
            CBUFFER_END

            // Safely sample texture while treating anything outside 0..1 as transparent
            half4 SampleSpriteSafe(float2 uv)
            {
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return half4(0, 0, 0, 0);

                return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
            }

            Varyings vert(Attributes IN)
            {
                Varyings output;

                // 1. Calculate required UV padding from distance + blur
                float uvPad = _ShadowDistance + _ShadowBlur;
                float scaleFactor = 1.0 + (uvPad * 2.0);

                // 2. Find the center of the sprite in Object Space using UVs
                // Assuming Full Rect sprite where UVs go 0 -> 1
                float2 uvOffsetFromCenter = IN.uv - 0.5;
                
                // Determine sprite world-unit size per UV unit
                // (Works even if pivot is not centered)
                float2 spriteSizeOS = float2(
                    length(ddx(IN.positionOS.xy)), 
                    length(ddy(IN.positionOS.xy))
                );
                
                // Fallback/Exact calculation using texture dimensions & default 100 PPU:
                // Better approach: Expand outward from vertex UV direction
                float2 expandDir = sign(uvOffsetFromCenter);

                // Calculate UV expansion
                float2 expandedUV = 0.5 + (uvOffsetFromCenter * scaleFactor);

                // Calculate corresponding Object Space expansion
                // We derive the local scale ratio from the sprite's current vertex vs UV
                float4 posOS = IN.positionOS;
                
                // Exact geometric expansion for Full Rect quads:
                // Shift vertex outward proportional to UV expansion
                float2 uvDelta = expandedUV - IN.uv;
                
                // Convert UV delta to Object Space using texture aspect ratio / texel size
                // Note: Unity sprites map 1 pixel = 1 / PixelsPerUnit. 
                // Since we don't have PPU directly, we infer local quad size from vertex positions:
                float2 quadScale = abs(posOS.xy / max(abs(uvOffsetFromCenter), 0.0001));
                posOS.xy += uvDelta * quadScale;

                output.positionCS = TransformObjectToHClip(posOS.xyz);
                output.uv = TRANSFORM_TEX(expandedUV, _BaseMap);
                output.color = IN.color * _BaseColor;
                output.uvBounds = float4(0.0, 0.0, 1.0, 1.0);

                return output;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 1. Sample main sprite (returns transparent in the newly padded border area)
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
