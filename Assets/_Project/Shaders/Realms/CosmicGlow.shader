// CosmicGlow.shader
// URP Unlit shader with pulsing emission and soft additive blending.
// Use on EchoFields stars, constellation lines, nebula particles.
Shader "AscendantContinuum/CosmicGlow"
{
    Properties
    {
        _MainTex        ("Texture",         2D)    = "white" {}
        _GlowColor      ("Glow Color",      Color) = (0.4, 0.6, 1.0, 1.0)
        _GlowIntensity  ("Glow Intensity",  Range(0.0, 8.0)) = 2.0
        _PulseSpeed     ("Pulse Speed",     Range(0.0, 5.0)) = 1.2
        _PulseAmount    ("Pulse Amount",    Range(0.0, 1.0)) = 0.25
        _AlphaCutoff    ("Alpha Cutoff",    Range(0.0, 1.0)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend One One               // Additive — correctly stacks glows
        ZWrite Off
        Cull Off

        Pass
        {
            Name "CosmicGlowPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _GlowColor;
                half   _GlowIntensity;
                half   _PulseSpeed;
                half   _PulseAmount;
                half   _AlphaCutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                half4  color       : COLOR;
                float  fogFactor   : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color       = IN.color;
                OUT.fogFactor   = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(tex.a * IN.color.a - _AlphaCutoff);

                // Pulse the intensity over time
                half pulse   = 1.0h + _PulseAmount *
                               sin(_Time.y * _PulseSpeed * UNITY_TWO_PI);

                half3 glow   = _GlowColor.rgb * _GlowIntensity * pulse;
                half4 col    = half4(glow * tex.rgb * IN.color.rgb,
                                     tex.a * IN.color.a);

                col.rgb = MixFog(col.rgb, IN.fogFactor);
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
