using UnityEngine;

namespace AscendantContinuum.Realms
{
    /// <summary>
    /// Defines visual atmosphere for each realm (lighting, post-processing, particles).
    /// Must live in its own file so Unity can locate the MonoScript asset by name.
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
}
