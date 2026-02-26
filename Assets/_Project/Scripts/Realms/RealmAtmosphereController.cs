using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace AscendantContinuum.Realms
{
    /// <summary>
    /// Defines visual atmosphere for each realm (lighting, post-processing, particles)
    /// </summary>
    [CreateAssetMenu(fileName = "RealmAtmosphere", menuName = "Ascendant Continuum/Realm Atmosphere")]
    public class RealmAtmosphereData : ScriptableObject
    {
        [Header("Identity")]
        public string realmName = "Emberforge";
        public string realmId = "emberforge";

        [Header("Lighting")]
        public Light mainLight;
        public Color ambientLightColor = Color.white;
        [Range(0f, 1f)] public float ambientLightIntensity = 0.5f;
        public Color mainLightColor = Color.white;
        [Range(0.5f, 2f)] public float mainLightIntensity = 1f;
        [Range(-180f, 180f)] public float mainLightRotation = 0f;

        [Header("Fog Settings")]
        public bool enableFog = true;
        public Color fogColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [Range(0f, 100f)] public float fogStart = 10f;
        [Range(10f, 1000f)] public float fogEnd = 100f;

        [Header("Post Processing")]
        public bool enablePostProcessing = true;
        public float exposure = 1f;
        [Range(0f, 1f)] public float saturation = 1f;
        [Range(0f, 2f)] public float bloomIntensity = 0.8f;
        [Range(0f, 1f)] public float vignettIntensity = 0.3f;

        [Header("Particle System Presets")]
        public ParticleSystem[] particleSystemPresets = new ParticleSystem[0];
        [Range(0f, 10f)] public float particleIntensity = 1f;
        public bool autoPlayParticles = true;

        [Header("Color Grading")]
        public Texture3D colorGradingLUT;
        [Range(-100f, 100f)] public float colorGradingStrength = 0f;

        [Header("Ambience Settings")]
        public Color ambienceKeyColor = new Color(1f, 0.6f, 0.2f);
        public Color ambienceFillColor = new Color(0.2f, 0.3f, 0.6f);
        [Range(0f, 1f)] public float ambienceIntensity = 0.5f;
    }

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
            mainLight = FindObjectOfType<Light>();
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
            postProcessVolume = FindObjectOfType<Volume>();
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
                        main.startLifetime *= atmosphereData.particleIntensity;

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
