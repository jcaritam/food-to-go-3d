Shader "Custom/Water_Titicaca"
{
    Properties
    {
        _ColorDeep    ("Color Profundo",   Color)      = (0.02, 0.15, 0.35, 1)
        _ColorShallow ("Color Orilla",     Color)      = (0.18, 0.55, 0.75, 1)
        _ColorFoam    ("Color Cresta Ola", Color)      = (0.65, 0.85, 0.95, 1)
        _SpeedA       ("Velocidad Ola A",  Vector)     = (0.06, 0.04, 0, 0)
        _SpeedB       ("Velocidad Ola B",  Vector)     = (-0.04, 0.07, 0, 0)
        _ScaleA       ("Escala Ruido A",   Float)      = 4.0
        _ScaleB       ("Escala Ruido B",   Float)      = 7.0
        _WaveContrast ("Contraste Olas",   Float)      = 2.8
        _FoamThresh   ("Umbral Espuma",    Range(0,1)) = 0.72
        _Smoothness   ("Smoothness",       Range(0,1)) = 0.92
        _SpecColor2   ("Color Especular",  Color)      = (1, 1, 1, 1)
        _Tiling       ("Tiling UV",        Float)      = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry-1" }
        LOD 200
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorDeep;
                float4 _ColorShallow;
                float4 _ColorFoam;
                float4 _SpeedA;
                float4 _SpeedB;
                float  _ScaleA;
                float  _ScaleB;
                float  _WaveContrast;
                float  _FoamThresh;
                float  _Smoothness;
                float4 _SpecColor2;
                float  _Tiling;
            CBUFFER_END

            struct Attr { float4 pos:POSITION; float2 uv:TEXCOORD0; float3 n:NORMAL; float4 tan:TANGENT; };
            struct Vary {
                float4 cs   : SV_POSITION;
                float2 uv   : TEXCOORD0;
                float3 nws  : TEXCOORD1;
                float3 pws  : TEXCOORD2;
                float3 tws  : TEXCOORD3;
                float3 bws  : TEXCOORD4;
            };

            // Hash + Value Noise
            float2 hash2x2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453);
            }
            float gnoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(dot(hash2x2(i + float2(0,0)), f - float2(0,0)),
                         dot(hash2x2(i + float2(1,0)), f - float2(1,0)), u.x),
                    lerp(dot(hash2x2(i + float2(0,1)), f - float2(0,1)),
                         dot(hash2x2(i + float2(1,1)), f - float2(1,1)), u.x),
                    u.y);
            }

            // FBM de 4 octavas
            float fbm(float2 uv)
            {
                float v = 0;
                float a = 0.5;
                float2x2 rot = float2x2(1.6, 1.2, -1.2, 1.6);
                for (int i = 0; i < 4; i++)
                {
                    v += a * gnoise(uv);
                    uv = mul(rot, uv);
                    a *= 0.5;
                }
                return v;
            }

            Vary vert(Attr IN)
            {
                Vary o;
                o.pws = TransformObjectToWorld(IN.pos.xyz);
                o.cs  = TransformWorldToHClip(o.pws);
                o.nws = TransformObjectToWorldNormal(IN.n);
                float3 tWS = TransformObjectToWorldDir(IN.tan.xyz);
                float3 bWS = cross(o.nws, tWS) * IN.tan.w;
                o.tws = tWS;
                o.bws = bWS;
                o.uv  = IN.uv * _Tiling;
                return o;
            }

            half4 frag(Vary IN) : SV_Target
            {
                float t = _Time.y;

                // Dos capas de FBM desplazadas en el tiempo
                float2 uvA = IN.uv + _SpeedA.xy * t;
                float2 uvB = IN.uv + _SpeedB.xy * t;

                float nA = fbm(uvA * _ScaleA);
                float nB = fbm(uvB * _ScaleB);

                // Combinar con suma ponderada y aplicar contraste
                float wave = saturate((nA * 0.6 + nB * 0.4) * 0.5 + 0.5);
                wave = saturate(pow(wave, 1.0 / _WaveContrast));

                // Derivadas del FBM para normal de agua
                float eps = 0.01;
                float nAx = fbm((uvA + float2(eps, 0)) * _ScaleA);
                float nAz = fbm((uvA + float2(0, eps)) * _ScaleA);
                float nBx = fbm((uvB + float2(eps, 0)) * _ScaleB);
                float nBz = fbm((uvB + float2(0, eps)) * _ScaleB);

                float3 waveNorm = normalize(float3(
                    -((nAx - nA) * 0.6 + (nBx - nB) * 0.4) / eps * 0.4,
                    1.0,
                    -((nAz - nA) * 0.6 + (nBz - nB) * 0.4) / eps * 0.4
                ));
                // Pasar la normal perturbada al espacio mundo
                float3x3 tbn = float3x3(normalize(IN.tws), normalize(IN.bws), normalize(IN.nws));
                float3 N = normalize(mul(waveNorm, tbn));

                // Color base interpolado segun altura de ola
                float3 baseCol = lerp(_ColorDeep.rgb, _ColorShallow.rgb, wave);
                // Espuma en crestas
                float foam = smoothstep(_FoamThresh - 0.08, _FoamThresh + 0.08, wave);
                baseCol = lerp(baseCol, _ColorFoam.rgb, foam * 0.7);

                // Iluminacion
                Light mainLight = GetMainLight();
                float3 V = GetWorldSpaceNormalizeViewDir(IN.pws);
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(L + V);

                float NdotL = saturate(dot(N, L));
                float NdotH = saturate(dot(N, H));
                float NdotV = saturate(dot(N, V));

                // Fresnel: mas reflexion en angulos rasantes
                float fresnel = pow(1.0 - NdotV, 4.0);

                // Difuso
                float3 diffuse = baseCol * mainLight.color * (NdotL * 0.7 + 0.3);

                // Especular Blinn-Phong ampliado
                float shininess = lerp(32, 512, _Smoothness);
                float spec = pow(NdotH, shininess) * _Smoothness * 2.5;
                float3 specular = spec * mainLight.color * _SpecColor2.rgb;

                // Reflexion del cielo aproximada via Fresnel
                float3 skyRefl = lerp(float3(0.4, 0.6, 0.8), float3(0.7, 0.85, 1.0), fresnel * 0.5);
                float3 finalCol = lerp(diffuse + specular, skyRefl * baseCol + specular, fresnel * 0.35);

                return half4(finalCol, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}