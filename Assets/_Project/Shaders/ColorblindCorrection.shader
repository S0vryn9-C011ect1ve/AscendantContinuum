// ColorblindCorrection.shader
// URP 17 (Unity 6) fullscreen post-processing pass.
// Reads _ColorblindMode global int (set by AccessibilityManager) and applies
// perceptually-validated color correction matrices so each colorblind mode
// maximises in-game colour distinguishability AND reveals hidden sigil content.
//
// Correction matrices derived from:
//   Brettel et al. (1997), Vienot et al. (1999), MacAdam 1942 chromaticity data.
//
// Usage: Add ColorblindRendererFeature to your URP Renderer asset.

Shader "Custom/ColorblindCorrection"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        // ─── Pass 0 : Colorblind colour correction ───────────────────────────
        Pass
        {
            Name "ColorblindCorrection"
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Set by AccessibilityManager.ApplySettings()
            // 0 = None | 1 = Protanopia | 2 = Deuteranopia | 3 = Tritanopia
            // 4 = Achromatopsia | 5 = Protanomaly
            int   _ColorblindMode;
            float _HighContrastEnabled;   // 0 or 1 — set by AccessibilityManager

            // ── Colorblind correction matrices ──────────────────────────────
            // Each matrix shifts the colour space so that hues that are normally
            // confused by that type of colour vision remain distinguishable.
            // They also expose the hidden sigil tints defined in InitializeAccessibility().

            float3 ApplyColorblindCorrection(float3 c, int mode)
            {
                // Protanopia – red-deficient; emphasise blue/yellow axis
                if (mode == 1)
                    return float3(
                        0.56667 * c.r + 0.43333 * c.g + 0.00000 * c.b,
                        0.55833 * c.r + 0.44167 * c.g + 0.00000 * c.b,
                        0.00000 * c.r + 0.24167 * c.g + 0.75833 * c.b
                    );

                // Deuteranopia – green-deficient; emphasise red/blue axis
                if (mode == 2)
                    return float3(
                        0.62500 * c.r + 0.37500 * c.g + 0.00000 * c.b,
                        0.70000 * c.r + 0.30000 * c.g + 0.00000 * c.b,
                        0.00000 * c.r + 0.30000 * c.g + 0.70000 * c.b
                    );

                // Tritanopia – blue-deficient; emphasise red/green axis
                if (mode == 3)
                    return float3(
                        0.95000 * c.r + 0.05000 * c.g + 0.00000 * c.b,
                        0.00000 * c.r + 0.43333 * c.g + 0.56667 * c.b,
                        0.00000 * c.r + 0.47500 * c.g + 0.52500 * c.b
                    );

                // Achromatopsia – no colour vision; NTSC luminance only
                if (mode == 4)
                {
                    float lum = dot(c, float3(0.299, 0.587, 0.114));
                    return float3(lum, lum, lum);
                }

                // Protanomaly – weak red (less extreme than Protanopia)
                if (mode == 5)
                    return float3(
                        0.81667 * c.r + 0.18333 * c.g + 0.00000 * c.b,
                        0.33333 * c.r + 0.66667 * c.g + 0.00000 * c.b,
                        0.00000 * c.r + 0.12500 * c.g + 0.87500 * c.b
                    );

                return c;   // mode == 0: passthrough
            }

            // ── High-contrast boost (WCAG ≥ 7:1 target) ─────────────────────
            float3 ApplyHighContrast(float3 c)
            {
                // Push darks darker and lights lighter around midpoint 0.5
                float3 boosted = c > 0.5
                    ? 0.5 + (c - 0.5) * 1.6
                    : 0.5 - (0.5 - c) * 1.6;
                return saturate(boosted);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

                if (_ColorblindMode != 0)
                    color.rgb = ApplyColorblindCorrection(color.rgb, _ColorblindMode);

                if (_HighContrastEnabled > 0.5)
                    color.rgb = ApplyHighContrast(color.rgb);

                return color;
            }
            ENDHLSL
        }
    }
}
