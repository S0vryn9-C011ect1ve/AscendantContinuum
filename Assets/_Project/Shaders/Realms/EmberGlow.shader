// EmberGlow.shader
// URP Lit-surface shader with animated heat distortion and rim-glow emission.
// Use on forge surfaces, lava cracks, and ember meshes in the Emberforge realm.
Shader "AscendantContinuum/EmberGlow"
{
    Properties
    {
        _BaseMap        ("Base Texture",    2D)    = "white" {}
        _BaseColor      ("Base Color",      Color) = (0.9, 0.3, 0.05, 1.0)
        _EmissionMap    ("Emission Map",    2D)    = "black" {}
        _EmissionColor  ("Emission Color",  Color) = (1.0, 0.4, 0.0, 1.0)
        _EmissionPower  ("Emission Power",  Range(0.0, 10.0)) = 3.5
        _FlickerSpeed   ("Flicker Speed",   Range(0.0, 20.0)) = 6.0
        _FlickerStrength("Flicker Strength",Range(0.0, 1.0))  = 0.20
        _RimColor       ("Rim Color",       Color) = (1.0, 0.6, 0.1, 1.0)
        _RimPower       ("Rim Power",       Range(0.5, 8.0))  = 3.0
        _SmoothnessMap  ("Smoothness Map",  2D)    = "black" {}
        _Smoothness     ("Smoothness",      Range(0.0, 1.0))  = 0.05
        _Metallic       ("Metallic",        Range(0.0, 1.0))  = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "Queue"          = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_SmoothnessMap); SAMPLER(sampler_SmoothnessMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half4  _EmissionColor;
                half   _EmissionPower;
                half   _FlickerSpeed;
                half   _FlickerStrength;
                half4  _RimColor;
                half   _RimPower;
                half   _Smoothness;
                half   _Metallic;
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
                half4 base = SAMPLE_TEXTURE2D(_BaseMap,     sampler_BaseMap,     IN.uv) * _BaseColor;
                half4 emm  = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv);

                // Animated flicker
                half flicker = 1.0h - _FlickerStrength * 0.5h *
                               (sin(_Time.y * _FlickerSpeed) +
                                sin(_Time.y * _FlickerSpeed * 1.7h + 0.9h));

                // Rim (fresnel)
                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionWS));
                half   rim     = 1.0h - saturate(dot(normalize(IN.normalWS), (half3)viewDir));
                half3  rimGlow = _RimColor.rgb * pow(rim, _RimPower);

                // Emission
                half3 emission = emm.rgb * _EmissionColor.rgb * _EmissionPower * flicker;

                // Simplified diffuse
                Light mainLight = GetMainLight(
                    TransformWorldToShadowCoord(IN.positionWS));
                half  NdotL  = saturate(dot(normalize(IN.normalWS),
                                            mainLight.direction));
                half3 diffuse = base.rgb * mainLight.color * NdotL;

                half3 col = diffuse + emission + rimGlow;
                col = MixFog(col, IN.fogFactor);
                return half4(col, 1.0h);
            }
            ENDHLSL
        }

        // Depth & shadows
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
