Shader "Custom/Totora_Uros"
{
    Properties
    {
        _ColorLight   ("Totora Clara",       Color)      = (0.85, 0.72, 0.40, 1)
        _ColorDark    ("Totora Oscura",      Color)      = (0.55, 0.42, 0.18, 1)
        _ColorTip     ("Punta de Cana",      Color)      = (0.92, 0.84, 0.55, 1)
        _Tiling       ("Tiling Canas",       Float)      = 16.0
        _ReedWidth    ("Grosor Cana",        Range(0.1, 0.9)) = 0.55
        _ReedJitter   ("Jitter/Irregularidad", Range(0, 1)) = 0.35
        _CrossBlend   ("Mezcla Entrecruzado", Range(0, 1)) = 0.28
        _NoiseScale   ("Escala Ruido Base",  Float)      = 3.0
        _NoisePower   ("Contraste Variacion", Float)     = 2.2
        _Smoothness   ("Suavidad (mate)",    Range(0,1)) = 0.08
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
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
                float4 _ColorLight;
                float4 _ColorDark;
                float4 _ColorTip;
                float  _Tiling;
                float  _ReedWidth;
                float  _ReedJitter;
                float  _CrossBlend;
                float  _NoiseScale;
                float  _NoisePower;
                float  _Smoothness;
            CBUFFER_END

            struct Attr { float4 pos:POSITION; float2 uv:TEXCOORD0; float3 n:NORMAL; };
            struct Vary {
                float4 cs  : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float3 nws : TEXCOORD1;
                float3 pws : TEXCOORD2;
            };

            // ------------------------------------------------------------------
            // Utilidades de ruido / hash
            // ------------------------------------------------------------------
            float hash11(float n)
            {
                return frac(sin(n) * 43758.5453);
            }
            float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            // Value noise suavizado (para variacion de color base)
            float vnoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1,0));
                float c = hash21(i + float2(0,1));
                float d = hash21(i + float2(1,1));
                return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
            }
            // FBM de 3 octavas (variacion macro de la totora)
            float fbm(float2 uv)
            {
                float v = 0;
                float a = 0.5;
                for (int i = 0; i < 3; i++)
                {
                    v += a * vnoise(uv);
                    uv  = uv * 2.0 + float2(1.7, 0.9);
                    a  *= 0.5;
                }
                return v;
            }

            // ------------------------------------------------------------------
            // Reed pattern: simula canas paralelas con jitter por celda
            //   uv  -> UVs ya escaladas en TILING
            //   dir -> 0 = horizontal (canas paralelas al eje X)
            //          1 = vertical   (canas paralelas al eje Z)
            // Devuelve:
            //   .x = mascara de cana (0=entre canas, 1=dentro de una cana)
            //   .y = posicion longitudinal normalizada dentro de la cana (0-1)
            // ------------------------------------------------------------------
            float2 reedPattern(float2 uv, int dir)
            {
                // Elegimos el eje "transversal" y "longitudinal"
                float transv = (dir == 0) ? uv.x : uv.y;
                float longit = (dir == 0) ? uv.y : uv.x;

                // Celda de la cana (en eje transversal)
                float cellIdx = floor(transv);
                float cellFrac = frac(transv);

                // Jitter: cada cana se desplaza un poco en longitud
                float jitter = (hash11(cellIdx) - 0.5) * _ReedJitter;
                float longitJ = frac(longit + jitter);

                // Grosor de la cana: zona central de ancho _ReedWidth
                float halfW = _ReedWidth * 0.5;
                float reedMask = smoothstep(0.5 - halfW - 0.04, 0.5 - halfW + 0.04, cellFrac)
                               * smoothstep(0.5 + halfW + 0.04, 0.5 + halfW - 0.04, cellFrac);

                // Variacion de tono a lo largo de la cana (extremos mas claros = punta)
                float tipMask = pow(sin(longitJ * 3.14159), 0.4);

                return float2(reedMask, tipMask);
            }

            Vary vert(Attr IN)
            {
                Vary o;
                o.pws = TransformObjectToWorld(IN.pos.xyz);
                o.cs  = TransformWorldToHClip(o.pws);
                o.nws = TransformObjectToWorldNormal(IN.n);
                o.uv  = IN.uv;
                return o;
            }

            half4 frag(Vary IN) : SV_Target
            {
                float2 uv = IN.uv * _Tiling;

                // ------------------------------------------------------------------
                // Capa 1: canas horizontales (eje X, la "trama" principal de totora)
                // ------------------------------------------------------------------
                float2 r1 = reedPattern(uv, 0);

                // ------------------------------------------------------------------
                // Capa 2: canas perpendiculares (eje Z, el "entrecruzado")
                //   El tiling de la segunda capa es un poco menor para que las canas
                //   perpendiculares se vean mas gruesas (como la trama de soporte)
                // ------------------------------------------------------------------
                float2 r2 = reedPattern(uv * 0.6, 1);

                // Mezcla: la capa principal domina, la segunda se ve entretejida debajo
                float reedMain  = r1.x;
                float reedCross = r2.x * _CrossBlend;

                // En los cruces, combinamos ambas capas; fuera de las canas, se ve el
                // "espacio" entre canas (un poco mas oscuro)
                float combinedMask = saturate(reedMain + reedCross);
                float tipFactor = lerp(r1.y, r2.y, _CrossBlend * 0.5);

                // ------------------------------------------------------------------
                // Variacion de color macro con FBM (manchas irregulares de humedad,
                // partes mas secas/doradas y partes con mas sombra — como en la foto)
                // ------------------------------------------------------------------
                float noise = fbm(IN.uv * _NoiseScale);
                noise = saturate(pow(noise, _NoisePower));

                // Color base: de oscuro a claro segun el ruido
                float3 baseCol = lerp(_ColorDark.rgb, _ColorLight.rgb, noise);

                // Dentro de las canas: levantar al tono claro; fuera: oscuro
                float3 reedCol = lerp(_ColorDark.rgb * 0.7, baseCol, combinedMask);

                // Las puntas/crestas de las canas con un toque mas amarillo claro
                reedCol = lerp(reedCol, _ColorTip.rgb, tipFactor * combinedMask * 0.45);

                // Variacion micro dentro de cada cana con hash de la celda
                float cellID = floor(uv.x) * 7.3 + floor(uv.y) * 3.7;
                float microVar = (hash11(cellID) - 0.5) * 0.12;
                reedCol = saturate(reedCol + microVar);

                // ------------------------------------------------------------------
                // Iluminacion Lambert simple (paja es muy mate, sin especular)
                // ------------------------------------------------------------------
                float3 N = normalize(IN.nws);
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);

                float NdotL = saturate(dot(N, L));
                float3 diffuse = reedCol * mainLight.color * (NdotL * 0.75 + 0.25);

                // Pequeño toque de especular muy bajo (paja semi-seca tiene algo de brillo)
                float3 V = GetWorldSpaceNormalizeViewDir(IN.pws);
                float3 H = normalize(L + V);
                float NdotH = saturate(dot(N, H));
                float shininess = 8.0; // muy mate
                float spec = pow(NdotH, shininess) * _Smoothness * 0.3;
                float3 finalCol = diffuse + spec * mainLight.color;

                return half4(finalCol, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
