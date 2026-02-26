using UnityEngine;
using UnityEditor;
using AscendantContinuum.Realms;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Helper to create and configure realm atmosphere data assets
    /// </summary>
    public class RealmAtmosphereSetup
    {
        #if UNITY_EDITOR

        private const string ATMOSPHERE_FOLDER = "Assets/_Project/Data/Atmospheres";

        /// <summary>
        /// Create all realm atmosphere data assets
        /// </summary>
        [MenuItem("Ascendant Continuum/Setup/Create Realm Atmospheres")]
        public static void CreateAllAtmospheres()
        {
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(ATMOSPHERE_FOLDER))
            {
                string parentFolder = "Assets/_Project/Data";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                    AssetDatabase.CreateFolder("Assets/_Project", "Data");
                AssetDatabase.CreateFolder(parentFolder, "Atmospheres");
            }

            // Create each realm atmosphere
            CreateEmberforgeAtmosphere();
            CreateVerdantAtmosphere();
            CreateEchoAtmosphere();
            CreateDawnAtmosphere();
            CreateLanternAtmosphere();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RealmAtmosphere] All realm atmospheres created successfully!");
        }

        private static void CreateEmberforgeAtmosphere()
        {
            var atmosphere = ScriptableObject.CreateInstance<RealmAtmosphereData>();
            atmosphere.realmName = "Emberforge";
            atmosphere.realmId = "emberforge";

            // Warm, fiery lighting
            atmosphere.ambientLightColor = new Color(0.8f, 0.3f, 0.1f);
            atmosphere.ambientLightIntensity = 0.6f;
            atmosphere.mainLightColor = new Color(1f, 0.6f, 0.2f);
            atmosphere.mainLightIntensity = 1.2f;
            atmosphere.mainLightRotation = 45f;

            // Warm fog
            atmosphere.enableFog = true;
            atmosphere.fogColor = new Color(0.6f, 0.2f, 0.05f);
            atmosphere.fogStart = 5f;
            atmosphere.fogEnd = 50f;

            // Warm post-processing
            atmosphere.enablePostProcessing = true;
            atmosphere.exposure = 1.05f;
            atmosphere.saturation = 1.1f;
            atmosphere.bloomIntensity = 0.8f;
            atmosphere.vignettIntensity = 0.35f;

            atmosphere.particleIntensity = 1.2f;
            atmosphere.ambienceKeyColor = new Color(1f, 0.5f, 0.1f);
            atmosphere.ambienceFillColor = new Color(0.3f, 0.1f, 0.05f);
            atmosphere.ambienceIntensity = 0.7f;

            SaveAsset(atmosphere, "Emberforge");
        }

        private static void CreateVerdantAtmosphere()
        {
            var atmosphere = ScriptableObject.CreateInstance<RealmAtmosphereData>();
            atmosphere.realmName = "Verdant Sanctuary";
            atmosphere.realmId = "verdant";

            // Cool, natural green lighting
            atmosphere.ambientLightColor = new Color(0.4f, 0.6f, 0.3f);
            atmosphere.ambientLightIntensity = 0.7f;
            atmosphere.mainLightColor = new Color(0.7f, 1f, 0.5f);
            atmosphere.mainLightIntensity = 1.0f;
            atmosphere.mainLightRotation = -30f;

            // Green-tinted fog
            atmosphere.enableFog = true;
            atmosphere.fogColor = new Color(0.3f, 0.5f, 0.2f);
            atmosphere.fogStart = 8f;
            atmosphere.fogEnd = 80f;

            // Garden-like post-processing
            atmosphere.enablePostProcessing = true;
            atmosphere.exposure = 0.95f;
            atmosphere.saturation = 1.15f;
            atmosphere.bloomIntensity = 0.5f;
            atmosphere.vignettIntensity = 0.25f;

            atmosphere.particleIntensity = 0.8f;
            atmosphere.ambienceKeyColor = new Color(0.5f, 0.8f, 0.3f);
            atmosphere.ambienceFillColor = new Color(0.2f, 0.4f, 0.1f);
            atmosphere.ambienceIntensity = 0.6f;

            SaveAsset(atmosphere, "Verdant");
        }

        private static void CreateEchoAtmosphere()
        {
            var atmosphere = ScriptableObject.CreateInstance<RealmAtmosphereData>();
            atmosphere.realmName = "Echo Fields";
            atmosphere.realmId = "echo";

            // Cool, mysterious blue lighting
            atmosphere.ambientLightColor = new Color(0.3f, 0.5f, 0.8f);
            atmosphere.ambientLightIntensity = 0.8f;
            atmosphere.mainLightColor = new Color(0.4f, 0.7f, 1f);
            atmosphere.mainLightIntensity = 0.9f;
            atmosphere.mainLightRotation = -60f;

            // Cool blue fog
            atmosphere.enableFog = true;
            atmosphere.fogColor = new Color(0.2f, 0.3f, 0.6f);
            atmosphere.fogStart = 10f;
            atmosphere.fogEnd = 120f;

            // Ethereal post-processing
            atmosphere.enablePostProcessing = true;
            atmosphere.exposure = 1.0f;
            atmosphere.saturation = 0.95f;
            atmosphere.bloomIntensity = 0.7f;
            atmosphere.vignettIntensity = 0.4f;

            atmosphere.particleIntensity = 1.1f;
            atmosphere.ambienceKeyColor = new Color(0.3f, 0.6f, 0.9f);
            atmosphere.ambienceFillColor = new Color(0.1f, 0.2f, 0.5f);
            atmosphere.ambienceIntensity = 0.8f;

            SaveAsset(atmosphere, "Echo");
        }

        private static void CreateDawnAtmosphere()
        {
            var atmosphere = ScriptableObject.CreateInstance<RealmAtmosphereData>();
            atmosphere.realmName = "Dawn Citadel";
            atmosphere.realmId = "dawn";

            // Bright, golden morning lighting
            atmosphere.ambientLightColor = new Color(0.9f, 0.7f, 0.4f);
            atmosphere.ambientLightIntensity = 0.9f;
            atmosphere.mainLightColor = new Color(1f, 0.9f, 0.6f);
            atmosphere.mainLightIntensity = 1.3f;
            atmosphere.mainLightRotation = -45f;

            // Warm, clear fog
            atmosphere.enableFog = true;
            atmosphere.fogColor = new Color(0.8f, 0.7f, 0.5f);
            atmosphere.fogStart = 15f;
            atmosphere.fogEnd = 200f;

            // Bright, uplifting post-processing
            atmosphere.enablePostProcessing = true;
            atmosphere.exposure = 1.1f;
            atmosphere.saturation = 1.05f;
            atmosphere.bloomIntensity = 0.9f;
            atmosphere.vignettIntensity = 0.15f;

            atmosphere.particleIntensity = 1.3f;
            atmosphere.ambienceKeyColor = new Color(1f, 0.8f, 0.4f);
            atmosphere.ambienceFillColor = new Color(0.4f, 0.3f, 0.1f);
            atmosphere.ambienceIntensity = 0.9f;

            SaveAsset(atmosphere, "Dawn");
        }

        private static void CreateLanternAtmosphere()
        {
            var atmosphere = ScriptableObject.CreateInstance<RealmAtmosphereData>();
            atmosphere.realmName = "Lantern Ascension";
            atmosphere.realmId = "lantern";

            // Deep, mystical purple lighting
            atmosphere.ambientLightColor = new Color(0.5f, 0.3f, 0.7f);
            atmosphere.ambientLightIntensity = 0.7f;
            atmosphere.mainLightColor = new Color(0.8f, 0.5f, 1f);
            atmosphere.mainLightIntensity = 1.1f;
            atmosphere.mainLightRotation = 90f;

            // Purple-tinted fog
            atmosphere.enableFog = true;
            atmosphere.fogColor = new Color(0.4f, 0.2f, 0.6f);
            atmosphere.fogStart = 12f;
            atmosphere.fogEnd = 150f;

            // Mystical post-processing
            atmosphere.enablePostProcessing = true;
            atmosphere.exposure = 1.05f;
            atmosphere.saturation = 1.1f;
            atmosphere.bloomIntensity = 0.85f;
            atmosphere.vignettIntensity = 0.45f;

            atmosphere.particleIntensity = 1.4f;
            atmosphere.ambienceKeyColor = new Color(0.8f, 0.4f, 1f);
            atmosphere.ambienceFillColor = new Color(0.3f, 0.1f, 0.5f);
            atmosphere.ambienceIntensity = 0.95f;

            SaveAsset(atmosphere, "Lantern");
        }

        private static void SaveAsset(RealmAtmosphereData asset, string realmName)
        {
            string path = $"{ATMOSPHERE_FOLDER}/{realmName}Atmosphere.asset";
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[RealmAtmosphere] Created: {path}");
        }

        #endif
    }
}
