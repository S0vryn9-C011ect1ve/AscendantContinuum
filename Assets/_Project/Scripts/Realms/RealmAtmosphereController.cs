using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace AscendantContinuum.Realms
{
    /// <summary>
    /// Controller for realm visual atmosphere - applies settings from scriptable object
    /// </summary>
    public class RealmAtmosphereController : MonoBehaviour
    {
        [SerializeField] private RealmAtmosphereData atmosphereData;
        [SerializeField] private bool autoApplyOnStart = true;

        private Light mainLight;
        private Volume postProcessVolume;
        private Bloom bloomComponent;
        private Vignette vignetteComponent;
        private ColorAdjustments colorAdjustmentsComponent;
        private ParticleSystem[] activeParticleSystems;

        private void Start()
        {
            // Auto-load from Resources/Atmospheres/{realmId}_Atmosphere if not assigned in Inspector
            if (atmosphereData == null)
            {
                // Try to resolve via the RealmController on the same GameObject or parent
                var realmCtrl = GetComponentInParent<AscendantContinuum.Core.RealmController>();
                if (realmCtrl != null)
                {
                    atmosphereData = realmCtrl.GetAtmosphere();
                }

                // Fallback: try Resources lookup using the realmId serialized field
                if (atmosphereData == null && realmCtrl != null)
                {
                    string realmId = realmCtrl.GetAtmosphere()?.realmId ?? string.Empty;
                    if (!string.IsNullOrEmpty(realmId))
                        atmosphereData = Resources.Load<RealmAtmosphereData>($"Atmospheres/{realmId}_Atmosphere");
                }
            }

            if (autoApplyOnStart && atmosphereData != null)
            {
                ApplyAtmosphere(atmosphereData);
            }
        }

        /// <summary>
        /// Apply realm atmosphere settings
        /// </summary>
        public void ApplyAtmosphere(RealmAtmosphereData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[RealmAtmosphere] No atmosphere data provided");
                return;
            }

            atmosphereData = data;

            // Apply lighting
            ApplyLighting();

            // Apply fog
            ApplyFog();

            // Apply post-processing
            if (data.enablePostProcessing)
                ApplyPostProcessing();

            // Apply particle systems
            ApplyParticlePresets();

            Debug.Log($"[RealmAtmosphere] Applied atmosphere: {data.realmName}");
        }

        private void ApplyLighting()
        {
            // Find or create main light
            mainLight = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (mainLight == null)
            {
                GameObject lightObj = new GameObject("DirectionalLight");
                mainLight = lightObj.AddComponent<Light>();
                mainLight.type = LightType.Directional;
            }

            // Apply light settings
            mainLight.color = atmosphereData.mainLightColor;
            mainLight.intensity = atmosphereData.mainLightIntensity;
            mainLight.transform.rotation = Quaternion.Euler(atmosphereData.mainLightRotation, 0f, 0f);

            // Apply ambient light
            RenderSettings.ambientLight = atmosphereData.ambientLightColor;
            RenderSettings.ambientIntensity = atmosphereData.ambientLightIntensity;

            Debug.Log($"[RealmAtmosphere] Applied lighting: color={atmosphereData.mainLightColor}, intensity={atmosphereData.mainLightIntensity}");
        }

        private void ApplyFog()
        {
            RenderSettings.fog = atmosphereData.enableFog;
            if (atmosphereData.enableFog)
            {
                RenderSettings.fogColor = atmosphereData.fogColor;
                RenderSettings.fogStartDistance = atmosphereData.fogStart;
                RenderSettings.fogEndDistance = atmosphereData.fogEnd;
                RenderSettings.fogMode = FogMode.Linear;
            }
        }

        private void ApplyPostProcessing()
        {
            // Find or create post-processing volume
            postProcessVolume = UnityEngine.Object.FindFirstObjectByType<Volume>();
            if (postProcessVolume == null)
            {
                GameObject volumeObj = new GameObject("PostProcessVolume");
                postProcessVolume = volumeObj.AddComponent<Volume>();
                postProcessVolume.isGlobal = true;
                postProcessVolume.priority = 0f;

                // Create profile
                var profile = ScriptableObject.CreateInstance<VolumeProfile>();
                postProcessVolume.profile = profile;
            }

            var profile2 = postProcessVolume.profile;

            // Bloom
            if (!profile2.TryGet<Bloom>(out bloomComponent))
            {
                bloomComponent = profile2.Add<Bloom>(true);
            }
            bloomComponent.intensity.value = atmosphereData.bloomIntensity;

            // Vignette
            if (!profile2.TryGet<Vignette>(out vignetteComponent))
            {
                vignetteComponent = profile2.Add<Vignette>(true);
            }
            vignetteComponent.intensity.value = atmosphereData.vignettIntensity;

            // Color Adjustments (Exposure, Saturation)
            if (!profile2.TryGet<ColorAdjustments>(out colorAdjustmentsComponent))
            {
                colorAdjustmentsComponent = profile2.Add<ColorAdjustments>(true);
            }
            colorAdjustmentsComponent.postExposure.value = atmosphereData.exposure;
            colorAdjustmentsComponent.saturation.value = atmosphereData.saturation;
        }

        private void ApplyParticlePresets()
        {
            // Stop any active particles first
            if (activeParticleSystems != null)
            {
                foreach (ParticleSystem ps in activeParticleSystems)
                {
                    if (ps != null) ps.Stop();
                }
            }

            // Apply new particle presets
            if (atmosphereData.particleSystemPresets.Length > 0)
            {
                activeParticleSystems = new ParticleSystem[atmosphereData.particleSystemPresets.Length];

                for (int i = 0; i < atmosphereData.particleSystemPresets.Length; i++)
                {
                    ParticleSystem preset = atmosphereData.particleSystemPresets[i];
                    if (preset != null)
                    {
                        // Instantiate particle system
                        ParticleSystem particleInstance = Instantiate(preset, transform);
                        activeParticleSystems[i] = particleInstance;

                        // Adjust intensity
                        var main = particleInstance.main;
                        var lifetime = main.startLifetime;
                        lifetime.constant *= atmosphereData.particleIntensity;
                        lifetime.constantMin *= atmosphereData.particleIntensity;
                        lifetime.constantMax *= atmosphereData.particleIntensity;
                        main.startLifetime = lifetime;

                        if (atmosphereData.autoPlayParticles)
                            particleInstance.Play();
                    }
                }

                Debug.Log($"[RealmAtmosphere] Applied {activeParticleSystems.Length} particle system presets");
            }
        }

        /// <summary>
        /// Smoothly transition between two atmospheres
        /// </summary>
        public void TransitionToAtmosphere(RealmAtmosphereData targetData, float duration = 2f)
        {
            if (targetData == null) return;

            StartCoroutine(AtmosphereTransitionCoroutine(targetData, duration));
        }

        /// <summary>
        /// Called by SigilCompletionHandler — briefly overlays <paramref name="tint"/> on the
        /// ambient light color before fading back to realm baseline over 2 seconds.
        /// </summary>
        public void SetCompletionOverlay(UnityEngine.Color tint)
        {
            StartCoroutine(CompletionOverlayCo(tint));
        }

        private System.Collections.IEnumerator CompletionOverlayCo(UnityEngine.Color tint)
        {
            var baseAmbient = UnityEngine.RenderSettings.ambientLight;
            float elapsed   = 0f;

            // Fade in
            while (elapsed < 0.4f)
            {
                elapsed += UnityEngine.Time.deltaTime;
                UnityEngine.RenderSettings.ambientLight =
                    UnityEngine.Color.Lerp(baseAmbient, tint, elapsed / 0.4f);
                yield return null;
            }

            yield return new UnityEngine.WaitForSeconds(0.6f);

            // Fade out
            elapsed = 0f;
            while (elapsed < 1.2f)
            {
                elapsed += UnityEngine.Time.deltaTime;
                UnityEngine.RenderSettings.ambientLight =
                    UnityEngine.Color.Lerp(tint, baseAmbient, elapsed / 1.2f);
                yield return null;
            }

            UnityEngine.RenderSettings.ambientLight = baseAmbient;
        }

        private System.Collections.IEnumerator AtmosphereTransitionCoroutine(RealmAtmosphereData targetData, float duration)
        {
            RealmAtmosphereData startData = atmosphereData;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                // Interpolate settings
                InterpolateAtmosphere(startData, targetData, t);

                yield return null;
            }

            // Final apply
            ApplyAtmosphere(targetData);
        }

        private void InterpolateAtmosphere(RealmAtmosphereData from, RealmAtmosphereData to, float t)
        {
            if (mainLight != null)
            {
                mainLight.color = Color.Lerp(from.mainLightColor, to.mainLightColor, t);
                mainLight.intensity = Mathf.Lerp(from.mainLightIntensity, to.mainLightIntensity, t);
            }

            RenderSettings.ambientLight = Color.Lerp(from.ambientLightColor, to.ambientLightColor, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(from.ambientLightIntensity, to.ambientLightIntensity, t);

            if (from.enableFog && to.enableFog)
            {
                RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, t);
                RenderSettings.fogStartDistance = Mathf.Lerp(from.fogStart, to.fogStart, t);
                RenderSettings.fogEndDistance = Mathf.Lerp(from.fogEnd, to.fogEnd, t);
            }

            if (bloomComponent != null)
                bloomComponent.intensity.value = Mathf.Lerp(from.bloomIntensity, to.bloomIntensity, t);

            if (vignetteComponent != null)
                vignetteComponent.intensity.value = Mathf.Lerp(from.vignettIntensity, to.vignettIntensity, t);

            if (colorAdjustmentsComponent != null)
            {
                colorAdjustmentsComponent.postExposure.value = Mathf.Lerp(from.exposure, to.exposure, t);
                colorAdjustmentsComponent.saturation.value = Mathf.Lerp(from.saturation, to.saturation, t);
            }
        }

        public RealmAtmosphereData GetCurrentAtmosphere() => atmosphereData;
    }
}
