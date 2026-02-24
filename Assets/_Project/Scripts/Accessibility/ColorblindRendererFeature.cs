using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AscendantContinuum.Accessibility
{
    /// <summary>
    /// URP ScriptableRendererFeature that applies colorblind-correction and
    /// high-contrast post-processing.
    ///
    /// SETUP: Add to your URP Renderer Data asset via Inspector → Add Renderer Feature.
    /// Reads _ColorblindMode (int 0-5) and _HighContrastEnabled (float) globals
    /// set by AccessibilityManager. Zero GPU cost when both are at defaults.
    /// </summary>
    [Serializable]
    public class ColorblindRendererFeature : ScriptableRendererFeature
    {
        [Tooltip("When to inject the correction pass.")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        private Material blitMaterial;
        private ColorblindRenderPass renderPass;

        public override void Create()
        {
            var shader = Shader.Find("Custom/ColorblindCorrection");
            if (shader == null)
            {
                Debug.LogWarning("[ColorblindRendererFeature] Shader 'Custom/ColorblindCorrection' not found. " +
                    "Ensure ColorblindCorrection.shader is in the project.");
                return;
            }

            blitMaterial = CoreUtils.CreateEngineMaterial(shader);
            renderPass = new ColorblindRenderPass(blitMaterial, name)
            {
                renderPassEvent = renderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var cameraType = renderingData.cameraData.cameraType;
            if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
                return;
            if (blitMaterial == null) return;

            // Skip when neither effect is active — zero GPU cost at defaults
            bool colorblindActive = Shader.GetGlobalInt("_ColorblindMode") != 0;
            bool highContrastActive = Shader.GetGlobalFloat("_HighContrastEnabled") > 0.5f;
            if (!colorblindActive && !highContrastActive) return;

            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            renderPass?.Dispose();
            if (blitMaterial != null)
                CoreUtils.Destroy(blitMaterial);
        }

        // ── Inner pass (compatibility Execute path; works with URP 14-17) ────

#pragma warning disable CS0672
        private class ColorblindRenderPass : ScriptableRenderPass, IDisposable
        {
            private readonly Material material;
            private readonly string profilerTag;
            private RTHandle tempRT;

            public ColorblindRenderPass(Material mat, string tag)
            {
                material = mat;
                profilerTag = tag;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc, name: "_ColorblindCorrectionTemp");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null || tempRT == null) return;

                var cmd = CommandBufferPool.Get(profilerTag);
                RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;
                Blitter.BlitCameraTexture(cmd, source, tempRT, material, 0);
                Blitter.BlitCameraTexture(cmd, tempRT, source);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public override void OnCameraCleanup(CommandBuffer cmd) { }

            public void Dispose()
            {
                tempRT?.Release();
                tempRT = null;
            }
        }
#pragma warning restore CS0672
    }
}
