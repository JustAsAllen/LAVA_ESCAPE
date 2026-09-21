Shader "MainCourse/IslandRock"
{
    Properties
    {
        _Base ("Rock Base", Color) = (0.42, 0.26, 0.58, 1)
        _Band ("Band Tint", Color) = (0.85, 0.34, 0.62, 1)
        _Rim ("Rim Glow", Color) = (1, 0.38, 0.72, 1)
        _RimPower ("Rim Power", Range(1, 8)) = 3.2
        _RimStrength ("Rim Strength", Range(0, 3)) = 1.6
        _BandScale ("Band Scale", Range(0.1, 3)) = 1.1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "IslandRockForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Base;
                float4 _Band;
                float4 _Rim;
                float _RimPower;
                float _RimStrength;
                float _BandScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            static float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            static float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash(i);
                float b = Hash(i + float2(1.0, 0.0));
                float c = Hash(i + float2(0.0, 1.0));
                float d = Hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                o.positionWS = posWS;
                o.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                o.positionCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 n = normalize(IN.normalWS);
                float3 v = normalize(_WorldSpaceCameraPos.xyz - IN.positionWS);
                float fres = pow(1.0 - saturate(dot(n, v)), _RimPower);

                float band = ValueNoise(float2(IN.positionWS.y * _BandScale * 0.6, IN.positionWS.x * 0.12 + IN.positionWS.z * 0.12));
                half3 col = lerp(_Base.rgb, _Band.rgb, smoothstep(0.28, 0.78, band));

                col += _Rim.rgb * fres * _RimStrength;
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}