Shader "MainCourse/Lava"
{
    Properties
    {
        _Crust    ("Crust", Color) = (0.045, 0.012, 0.008, 1)
        _Deep     ("Deep Magma", Color) = (0.55, 0.05, 0.01, 1)
        _Hot      ("Hot Lava", Color) = (0.97, 0.32, 0.04, 1)
        _White    ("White Hot", Color) = (1.00, 0.87, 0.55, 1)
        _Flow     ("Flow Speed", Float) = 0.30
        _Ripple   ("Boil", Float) = 0.55
        _Emissive ("Emissive Mult", Float) = 3.2
        _Churn    ("Churn", Float) = 0.9
        _Cell     ("Crack Scale", Float) = 1.4
        _Edge     ("Crack Width", Float) = 0.55
        _FlowDir  ("Flow Direction", Vector) = (0.35, 0, 0.94, 0)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "LavaForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Crust;
                float4 _Deep;
                float4 _Hot;
                float4 _White;
                float _Flow;
                float _Ripple;
                float _Emissive;
                float _Churn;
                float _Cell;
                float _Edge;
                float4 _FlowDir;
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

            static float Fbm(float2 p)
            {
                float v = 0.0;
                float amp = 0.5;
                for (int i = 0; i < 5; i++)
                {
                    v += ValueNoise(p) * amp;
                    p = p * 2.02 + 19.7;
                    amp *= 0.5;
                }
                return v;
            }

            // returns F1 (distance to nearest cell) and F2-F1 (hairline crack measure)
            static float2 Voronoi(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float f1 = 8.0;
                float f2 = 8.0;
                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    float2 cell = float2(x, y);
                    float2 seed = Hash(ip + cell) + Hash(ip + cell + float2(13.7, 5.9)) * 0.5;
                    float2 cp = cell + seed - fp;
                    float d = dot(cp, cp);
                    if (d < f1) { f2 = f1; f1 = d; }
                    else if (d < f2) { f2 = d; }
                }
                return float2(sqrt(f1), sqrt(f2) - sqrt(f1));
            }

            Varyings vert(Attributes IN)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float2 p = posWS.xz;
                float t = _Time.y;
                float bubble = sin(p.x * 0.7 + t * 1.6) * sin(p.y * 0.6 - t * 1.3) * _Ripple * 0.18;
                float swell = (Fbm(p * 0.08 + t * 0.05) - 0.5) * _Churn * 0.12;
                posWS.y += bubble + swell;
                o.positionWS = posWS;
                o.positionCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 p = IN.positionWS.xz;
                float t = _Time.y * _Flow * 0.5;
                float2 flow = normalize(_FlowDir.xz + 1e-6);

                float2 flowF  = p * 0.05 + flow * t;
                float2 warp   = p * 0.10 - flow * t * 0.8;
                float2 warp2  = p * 0.16 + flow * t * 0.5;

                float w = Fbm(warp);
                float w2 = Fbm(warp2 + w * 1.8 * _Churn);
                float churn = Fbm(flowF + w * 2.2 * _Churn) * 0.5 + Fbm(warp2 * 2.0) * 0.5;

                float2 vp = (p + w * 2.2 * _Churn - flow * t * 0.35) * _Cell;
                float2 vor = Voronoi(vp);
                float edge = saturate(_Edge * 0.6 - vor.y * 0.85);

                float streak = smoothstep(0.32, 0.72, Fbm(flowF * 3.0 + w2 * 2.0));
                float molten = saturate(churn * 0.6 + w2 * 0.55 + streak * 0.22);

                half3 crust = lerp(_Crust.rgb, _Deep.rgb, smoothstep(0.25, 0.5, molten) * 0.5);
                half3 glow = lerp(_Deep.rgb, _Hot.rgb, smoothstep(0.38, 0.78, molten));
                glow = lerp(glow, _White.rgb, smoothstep(0.72, 1.0, molten + edge * 0.35));

                half3 col = lerp(crust, glow, saturate(molten + edge));

                col += _White.rgb * edge * edge * 0.6;

                return half4(col * _Emissive, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}