Shader "Custom/PlayerToon"
{
    Properties
    {
        _BaseColor      ("Color Base",       Color) = (0.88, 0.64, 0.40, 1)
        _ShadowTint     ("Tinte Sombra",      Color) = (0.55, 0.40, 0.28, 1)
        _RampSteps      ("Bandas de Luz",     Range(1, 4)) = 2
        _RampSmoothing  ("Suavidad Banda",    Range(0.001, 0.5)) = 0.05
        _RimColor       ("Color Rim",         Color) = (1, 0.85, 0.6, 1)
        _RimPower       ("Potencia Rim",      Range(0.5, 8)) = 3
        _OutlineColor   ("Color Contorno",    Color) = (0.05, 0.03, 0.02, 1)
        _OutlineWidth   ("Grosor Contorno",   Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "Outline"
            Cull Front
            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attr { float4 pos:POSITION; float3 n:NORMAL; };
            struct Vary { float4 cs:SV_POSITION; };

            Vary vertOutline(Attr IN)
            {
                Vary OUT;
                float3 posOS = IN.pos.xyz + IN.n * _OutlineWidth;
                OUT.cs = TransformObjectToHClip(posOS);
                return OUT;
            }

            half4 fragOutline(Vary IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _ShadowTint;
                float  _RampSteps;
                float  _RampSmoothing;
                float4 _RimColor;
                float  _RimPower;
            CBUFFER_END

            struct Attr { float4 pos:POSITION; float3 n:NORMAL; };
            struct Vary
            {
                float4 cs  : SV_POSITION;
                float3 nws : TEXCOORD0;
                float3 pws : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
            };

            Vary vert(Attr IN)
            {
                Vary OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.pos.xyz);
                OUT.cs = posInputs.positionCS;
                OUT.pws = posInputs.positionWS;
                OUT.nws = TransformObjectToWorldNormal(IN.n);
                OUT.shadowCoord = GetShadowCoord(posInputs);
                return OUT;
            }

            half4 frag(Vary IN) : SV_Target
            {
                float3 N = normalize(IN.nws);
                float3 V = normalize(GetWorldSpaceViewDir(IN.pws));

                Light mainLight = GetMainLight(IN.shadowCoord);
                float NdotL = dot(N, mainLight.direction);
                float lit = NdotL * 0.5 + 0.5;
                lit *= mainLight.shadowAttenuation;

                float steppedLit = floor(lit * _RampSteps) / max(_RampSteps - 1, 1);
                steppedLit = smoothstep(0, _RampSmoothing, lit - steppedLit) + steppedLit;
                steppedLit = saturate(steppedLit);

                float3 albedo = lerp(_ShadowTint.rgb, _BaseColor.rgb, steppedLit);

                float rim = pow(1 - saturate(dot(N, V)), _RimPower);
                float3 color = albedo + rim * _RimColor.rgb * mainLight.color;

                return half4(color, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
