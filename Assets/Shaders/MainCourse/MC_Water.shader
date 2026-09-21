Shader "MainCourse/Water"
{
    Properties
    {
        _Deep ("Deep Water", Color) = (0.07, 0.24, 0.62, 1)
        _Shallow ("Shallow Water", Color) = (0.20, 0.68, 0.95, 1)
        _Foam ("Foam", Color) = (0.90, 0.98, 1.00, 1)
        _SunTint ("Sun Tint", Color) = (1.00, 0.85, 0.55, 1)
        _WaveAmp ("Wave Amplitude", Float) = 0.55
        _WaveFreq ("Wave Frequency", Float) = 0.045
        _WaveSpeed ("Wave Speed", Float) = 1.4
        _Glint ("Glitter", Range(0, 8)) = 2.6
        _Fresnel ("Fresnel Strength", Range(0, 2)) = 0.8
        _Opacity ("Opacity", Range(0, 1)) = 0.92
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "WaterForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Deep;
                float4 _Shallow;
                float4 _Foam;
                float4 _SunTint;
                float _WaveAmp;
                float _WaveFreq;
                float _WaveSpeed;
                float _Glint;
                float _Fresnel;
                float _Opacity;
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

            Varyings vert(Attributes IN)
            {
                Varyings o;
                float3 posOS = IN.positionOS.xyz;
                float3 posWS = TransformObjectToWorld(posOS);

                float t = _Time.y * _WaveSpeed;
                float wave = sin(posWS.x * _WaveFreq + t)
                           + sin(posWS.z * _WaveFreq * 0.83 + t * 1.37);
                posWS.y += wave * _WaveAmp * 0.5;
                o.positionWS = posWS;
                o.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                o.positionCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.positionWS);
                float t = _Time.y * _WaveSpeed;

                float2 p = IN.positionWS.xz;
                float wave = sin(p.x * _WaveFreq + t)
                           + sin(p.y * _WaveFreq * 0.83 + t * 1.37);
                float swellX = sin(p.x * _WaveFreq * 4.1 + t * 2.3);
                float swellY = sin(p.y * _WaveFreq * 3.3 - t * 1.9);
                float foam = smoothstep(0.62, 1.0, abs(swellX * swellY));
                float glint = pow(max(0.0, swellX * swellY), _Glint);

                float mixF = saturate(wave * 0.5 + 0.5);
                half3 col = lerp(_Deep.rgb, _Shallow.rgb, mixF);
                col += _SunTint.rgb * glint * 0.55;
                col += _Foam.rgb * foam * 0.4;

                float fres = pow(1.0 - saturate(dot(viewDir, IN.normalWS)), 2.0);
                col = lerp(col, _Foam.rgb, fres * _Fresnel);

                half alpha = lerp(_Opacity, _Opacity * 0.72, fres);
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}