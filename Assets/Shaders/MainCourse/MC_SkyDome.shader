Shader "MainCourse/SkyDome"
{
    Properties
    {
        _Zenith     ("Zenith", Color) = (0.006, 0.009, 0.030, 1)
        _Mid        ("Mid Sky", Color) = (0.020, 0.032, 0.085, 1)
        _Horizon    ("Horizon", Color) = (0.085, 0.055, 0.150, 1)
        _Sun        ("Moon Glow", Color) = (0.550, 0.720, 1.000, 1)
        _SunDir     ("Moon Direction", Vector) = (0.350, 0.620, 0.350, 0)
        _SunSpread  ("Moon Glow Spread", Range(4, 40)) = 11
        _MoonSize   ("Moon Size", Range(0.90, 0.999)) = 0.993
        _Glow       ("Horizon Glow", Range(0, 1)) = 0.6
        _Stars      ("Star Strength", Range(0, 3)) = 1.3
        _StarTint   ("Star Tint", Color) = (0.85, 0.92, 1.0, 1)
        _StarDensity("Star Density", Range(10, 160)) = 70
        _Twinkle    ("Twinkle", Range(0, 1)) = 1
        _MWDir      ("Milky Way Center", Vector) = (0.25, 0.20, 0.95, 0)
        _MWStrength ("Milky Way Strength", Range(0, 2)) = 0.9
        _Nebula     ("Nebula Tint", Range(0, 1)) = 0.45
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "SkyDomeForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Zenith;
                float4 _Mid;
                float4 _Horizon;
                float4 _Sun;
                float4 _SunDir;
                float _SunSpread;
                float _MoonSize;
                float _Glow;
                float _Stars;
                float4 _StarTint;
                float _StarDensity;
                float _Twinkle;
                float4 _MWDir;
                float _MWStrength;
                float _Nebula;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
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

            static float Hash3(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            static float VNoise3(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                float3 u = f * f * (3.0 - 2.0 * f);
                float a = Hash3(i);
                float b = Hash3(i + float3(1, 0, 0));
                float c = Hash3(i + float3(0, 1, 0));
                float d = Hash3(i + float3(1, 1, 0));
                float e = Hash3(i + float3(0, 0, 1));
                float f1 = Hash3(i + float3(1, 0, 1));
                float g = Hash3(i + float3(0, 1, 1));
                float h = Hash3(i + float3(1, 1, 1));
                return lerp(
                    lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y),
                    lerp(lerp(e, f1, u.x), lerp(g, h, u.x), u.y),
                    u.z);
            }

            static float Fbm3(float3 p)
            {
                float v = 0.0;
                float amp = 0.5;
                for (int i = 0; i < 5; i++)
                {
                    v += VNoise3(p) * amp;
                    p = p * 2.03 + 13.7;
                    amp *= 0.5;
                }
                return v;
            }

            Varyings vert(Attributes IN)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.positionWS - _WorldSpaceCameraPos.xyz);
                float h = saturate(dir.y * 0.5 + 0.5);

                half3 col = lerp(_Horizon.rgb, _Mid.rgb, smoothstep(0.0, 0.45, h));
                col = lerp(col, _Zenith.rgb, smoothstep(0.45, 0.98, h));

                float moon = saturate(dot(dir, normalize(_SunDir.xyz)));

                // Milky Way band along a fixed great circle
                float3 mwDir = normalize(_MWDir.xyz + 1e-6);
                float band = 1.0 - abs(dot(dir, mwDir));
                float t = _Time.y * 0.012;
                float dustA = Fbm3(dir * 3.4 + float3(0.0, t, t * 0.6));
                float dustB = Fbm3(dir * 8.5 - float3(t * 0.5, t, 0.0));
                float dustC = Fbm3(dir * 1.9 + float3(t, 0.0, t * 0.4));
                float dust = saturate(dustA * 0.55 + dustB * 0.30 + dustC * 0.35);
                float coreGlow = pow(band, 2.2) * saturate(dust * 1.4);
                float outKw = pow(band, 7.0) * dustB * 0.7;

                half3 mwCol = half3(0.78, 0.82, 0.95);
                half3 dusty = half3(0.30, 0.18, 0.16) ; // warm brown dust
                col += mwCol * coreGlow * _MWStrength * 0.85;
                col += dusty * outKw * _MWStrength * 0.35;
                col += mwCol * pow(band, 4.0) * _MWStrength * 0.14;

                // Nebula tints
                half3 neb = lerp(half3(0.35, 0.18, 0.55), half3(0.12, 0.40, 0.45), Fbm3(dir * 1.5 + 31.7));
                col += neb * pow(band, 1.8) * dustC * _Nebula * 0.05;

                // Moon: visible disc + halo
                float disc = smoothstep(_MoonSize, _MoonSize - 0.0045, moon);
                float halo = pow(moon, _SunSpread) * smoothstep(0.002, 0.06, moon * 0.35 + 0.4);
                float2 mo = dir.xz / (abs(dir.y) + 0.25);
                float crater = Fbm3(dir * 90.0 + 7.7) * 0.6 + Fbm3(dir * 180.0) * 0.4;
                half3 moonCol = half3(0.96, 0.97, 1.0) * (0.72 + 0.28 * crater);
                col += _Sun.rgb * halo * 0.8;
                col += moonCol * disc;

                col += _Horizon.rgb * pow(1.0 - h, 6.0) * _Glow * 0.9;

                // Stars: layered grids with soft halo + bright core, twinkle
                float2 sxy = dir.xz / (abs(dir.y) + 0.18);
                float above = step(0.02, dir.y);
                float star = 0.0;
                float2 grids[3];
                grids[0] = sxy * _StarDensity;
                grids[1] = sxy * _StarDensity * 3.1;
                grids[2] = sxy * _StarDensity * 7.7;
                float wgt[3];
                wgt[0] = 1.0;
                wgt[1] = 0.7;
                wgt[2] = 0.45;
                for (int g = 0; g < 3; g++)
                {
                    float2 c = floor(grids[g]);
                    float2 f = frac(grids[g]);
                    float2 off = float2(Hash(c), Hash(c + float2(0.7, 1.3)));
                    float2 sc = f - (off - 0.5);
                    float d2 = dot(sc, sc);
                    float r = 0.35 + Hash(c + float2(11.7, 3.1)) * 0.75;
                    r *= (g == 2) ? 0.5 : 1.0;
                    float tw = 0.72 + 0.28 * sin(_Time.y * (1.2 + Hash(c + 4.3) * 2.4) + off.x * 6.2831);
                    float halo2 = exp(-d2 * 3.0) * 0.22;
                    float core = smoothstep(r * r, 0.0, d2) * 0.55;
                    star += (halo2 + core) * wgt[g] * lerp(1.0, tw, _Twinkle);
                }
                col += _StarTint.rgb * _Stars * star * above * 0.42;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}