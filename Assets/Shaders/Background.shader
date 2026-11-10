Shader "Custom/Background"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _TextureSize("Texture Size", Vector) = (1920, 1080, 0, 0)
        _ValidLayerCount("Layer Count", Int) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define MAX_LAYERS 20
            #define LAYER_PACKED_SIZE (MAX_LAYERS * 3)
            uniform float4 _CircleLayers[LAYER_PACKED_SIZE];

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float2 _TextureSize;
                int _ValidLayerCount;
            CBUFFER_END

            struct CircleLayer {
                float2 position;
                float radius;
                float alpha;
                float softness;
                float noiseStrength;
                float noiseSpeed;
                float pulseAmount;
                float pulseSpeed;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ChunkNoise(float2 uv, float scale)
            {
                float2 aspectUV = uv * _TextureSize;
                float2 cell = floor(aspectUV / scale);
                return Hash21(cell);
            }

            float AnimatedChunkNoise(float2 uv, float scale, float speed)
            {
                float2 aspectUV = uv * _TextureSize;
                float2 cell = floor(aspectUV / scale);

                float t = _Time.y * speed;

                float frame = floor(t);
                float blend = frac(t);

                // Interpolate between animation frames
                blend = smoothstep(0.0, 1.0, blend);

                float noiseA = Hash21(cell + frame * 127.1);
                float noiseB = Hash21(cell + (frame + 1.0) * 127.1);

                return lerp(noiseA, noiseB, blend);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                // Smooth interpolation
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
            }

            float CircleMask(float2 uv, CircleLayer layer, float time) {
                if (layer.radius == 0) return 1;
                
                float2 noiseUV = uv * 20.0;

                noiseUV += float2(
                    time * layer.noiseSpeed,
                    time * layer.noiseSpeed * 0.7
                );
                
                float noise = AnimatedChunkNoise(uv, 3.0, layer.noiseSpeed) + ValueNoise(noiseUV);
                float pulse = layer.pulseAmount * sin(time * layer.pulseSpeed);

                float radius = layer.radius + pulse + noise * layer.noiseStrength;

                float dist = length(uv - layer.position);
                float mask = smoothstep(radius - layer.softness, radius + layer.softness, dist);

                return 1.0 - mask * layer.alpha;
            }

            void ParsePackedArrayToCircleLayer(float4 array[LAYER_PACKED_SIZE], out CircleLayer layers[MAX_LAYERS]) {
                for (int i = 0; i < MAX_LAYERS; i ++) {
                    layers[i].position.x = array[i * 3 + 0].x;
                    layers[i].position.y = array[i * 3 + 0].y;
                    layers[i].radius = array[i * 3 + 0].z;
                    layers[i].alpha = array[i * 3 + 0].w;
                    layers[i].softness = array[i * 3 + 1].x;
                    layers[i].noiseStrength = array[i * 3 + 1].y;
                    layers[i].noiseSpeed = array[i * 3 + 1].z;
                    layers[i].pulseAmount = array[i * 3 + 1].w;
                    layers[i].pulseSpeed = array[i * 3 + 2].x;
                }
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                CircleLayer layers[MAX_LAYERS];
                ParsePackedArrayToCircleLayer(_CircleLayers, layers);

                for (int i = 0; i < _ValidLayerCount; i++) {
                    color.rgb *= CircleMask(IN.uv, layers[i], _Time.y);
                }
                
                return color;
            }

            ENDHLSL
        }
    }
}
