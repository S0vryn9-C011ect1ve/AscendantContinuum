// BioluminescentPulse.shader
// URP transparent surface that pulses soft glow in UV-mapped waves.
// Use on MagicalPlant meshes and VerdantGarden foliage in the Verdant realm.
Shader "AscendantContinuum/BioluminescentPulse"
{
    Properties
    {
        _BaseMap        ("Base Texture",    2D)    = "white" {}
        _BaseColor      ("Base Color",      Color) = (0.05, 0.6, 0.25, 0.85)
        _GlowColor      ("Glow Color",      Color) = (0.1, 1.0, 0.4, 1.0)
        _GlowIntensity  ("Glow Intensity",  Range(0.0, 6.0)) = 2.0
        _PulseSpeed     ("Pulse Speed",     Range(0.0, 5.0)) = 0.8
        _PulseScale     ("Pulse UV Scale",  Range(0.5, 8.0)) = 2.0
        _PulseOffset    ("Pulse UV Offset", Range(0.0, 1.0)) = 0.0
        _FresnelPower   ("Fresnel Power",   Range(0.5, 6.0)) = 2.5
        _AlphaMult      ("Alpha Multiplier",Range(0.0, 1.0)) = 0.88
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent+10"
            "RenderPipeline"  = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "BioLumPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half4  _GlowColor;
                half   _GlowIntensity;
                half   _PulseSpeed;
                half   _PulseScale;
                half   _PulseOffset;
                half   _FresnelPower;
                half   _AlphaMult;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float  fogFactor   : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   vni = GetVertexNormalInputs(IN.normalOS);
                OUT.positionHCS = vpi.positionCS;
                OUT.positionWS  = vpi.positionWS;
                OUT.normalWS    = vni.normalWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.fogFactor   = ComputeFogFactor(vpi.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // Ripple pulse moving along V axis
                half pulse = sin(IN.uv.y * _PulseScale * UNITY_TWO_PI
                              - _Time.y * _PulseSpeed * UNITY_TWO_PI
                              + _PulseOffset * UNITY_TWO_PI) * 0.5h + 0.5h;

                // Fresnel rim glow
                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionWS));
                half   fresnel = 1.0h - saturate(dot(normalize(IN.normalWS), (half3)viewDir));
                fresnel        = pow(fresnel, _FresnelPower);

                half3  glow = _GlowColor.rgb * _GlowIntensity * (pulse + fresnel * 0.5h);
                half3  col  = _BaseColor.rgb * tex.rgb + glow;
                half   a    = tex.a * _BaseColor.a * _AlphaMult;

                col = MixFog(col, IN.fogFactor);
                return half4(col, a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
