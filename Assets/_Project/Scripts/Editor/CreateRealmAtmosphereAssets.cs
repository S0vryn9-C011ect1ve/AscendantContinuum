using UnityEngine;
using UnityEditor;
using System.IO;
using AscendantContinuum.Realms;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Generates RealmAtmosphereData ScriptableObject assets for all 5 realms.
    /// Run once via: Tools > Ascendant Continuum > Create Realm Atmosphere Assets
    /// </summary>
    public static class CreateRealmAtmosphereAssets
    {
        private const string OutputPath = "Assets/_Project/Resources/Atmospheres";

        [MenuItem("Tools/Ascendant Continuum/Create Realm Atmosphere Assets")]
        [MenuItem("Ascendant Continuum/Setup/\U0001f30c Create Realm Atmosphere Assets", priority = 210)]
        public static void CreateAll()
        {
            if (!AssetDatabase.IsValidFolder(OutputPath))
            {
                Directory.CreateDirectory(Application.dataPath +
                    "/_Project/Resources/Atmospheres");
                AssetDatabase.Refresh();
            }

            CreateAtmosphere(new RealmAtmosphereConfig
            {
                fileName         = "Emberforge_Atmosphere",
                realmName        = "Emberforge",
                realmId          = "emberforge",
                ambientColor     = new Color(0.25f, 0.08f, 0.03f),
                ambientIntensity = 0.40f,
                lightColor       = new Color(1.00f, 0.55f, 0.15f),
                lightIntensity   = 1.60f,
                lightRotation    = 48f,
                fogColor         = new Color(0.30f, 0.10f, 0.04f),
                fogStart         = 4f,  fogEnd = 70f,
                bloom            = 1.80f,
                vignette         = 0.42f,
                exposure         = 0.20f,
                saturation       = 20f,
                keyColor         = new Color(1.00f, 0.38f, 0.08f),
                fillColor        = new Color(0.28f, 0.09f, 0.04f),
                ambienceIntensity= 0.85f
            });

            CreateAtmosphere(new RealmAtmosphereConfig
            {
                fileName         = "DawnCitadel_Atmosphere",
                realmName        = "Dawn Citadel",
                realmId          = "dawncitadel",
                ambientColor     = new Color(0.60f, 0.45f, 0.70f),
                ambientIntensity = 0.55f,
                lightColor       = new Color(1.00f, 0.85f, 0.60f),
                lightIntensity   = 1.25f,
                lightRotation    = 22f,
                fogColor         = new Color(0.75f, 0.55f, 0.80f),
                fogStart         = 15f, fogEnd = 140f,
                bloom            = 1.10f,
                vignette         = 0.25f,
                exposure         = 0.10f,
                saturation       = 12f,
                keyColor         = new Color(1.00f, 0.75f, 0.40f),
                fillColor        = new Color(0.50f, 0.35f, 0.75f),
                ambienceIntensity= 0.60f
            });

            CreateAtmosphere(new RealmAtmosphereConfig
            {
                fileName         = "EchoFields_Atmosphere",
                realmName        = "Echo Fields",
                realmId          = "echofields",
                ambientColor     = new Color(0.04f, 0.06f, 0.18f),
                ambientIntensity = 0.20f,
                lightColor       = new Color(0.55f, 0.65f, 1.00f),
                lightIntensity   = 0.60f,
                lightRotation    = 75f,
                fogColor         = new Color(0.05f, 0.07f, 0.22f),
                fogStart         = 20f, fogEnd = 200f,
                bloom            = 2.20f,
                vignette         = 0.55f,
                exposure         = -0.30f,
                saturation       = 8f,
                keyColor         = new Color(0.40f, 0.55f, 1.00f),
                fillColor        = new Color(0.08f, 0.08f, 0.35f),
                ambienceIntensity= 0.35f
            });

            CreateAtmosphere(new RealmAtmosphereConfig
            {
                fileName         = "LanternAscension_Atmosphere",
                realmName        = "Lantern Ascension",
                realmId          = "lanternascension",
                ambientColor     = new Color(0.30f, 0.20f, 0.08f),
                ambientIntensity = 0.35f,
                lightColor       = new Color(1.00f, 0.80f, 0.35f),
                lightIntensity   = 0.90f,
                lightRotation    = 30f,
                fogColor         = new Color(0.25f, 0.18f, 0.08f),
                fogStart         = 10f, fogEnd = 120f,
                bloom            = 1.50f,
                vignette         = 0.35f,
                exposure         = 0.05f,
                saturation       = 15f,
                keyColor         = new Color(1.00f, 0.75f, 0.20f),
                fillColor        = new Color(0.35f, 0.22f, 0.06f),
                ambienceIntensity= 0.70f
            });

            CreateAtmosphere(new RealmAtmosphereConfig
            {
                fileName         = "Verdant_Atmosphere",
                realmName        = "Verdant",
                realmId          = "verdant",
                ambientColor     = new Color(0.05f, 0.18f, 0.08f),
                ambientIntensity = 0.50f,
                lightColor       = new Color(0.50f, 1.00f, 0.45f),
                lightIntensity   = 1.00f,
                lightRotation    = 35f,
                fogColor         = new Color(0.06f, 0.20f, 0.10f),
                fogStart         = 8f,  fogEnd = 90f,
                bloom            = 1.30f,
                vignette         = 0.28f,
                exposure         = 0.08f,
                saturation       = 25f,
                keyColor         = new Color(0.20f, 1.00f, 0.35f),
                fillColor        = new Color(0.04f, 0.30f, 0.12f),
                ambienceIntensity= 0.65f
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateRealmAtmosphereAssets] All 5 realm atmosphere assets created at: " + OutputPath);
        }

        private static void CreateAtmosphere(RealmAtmosphereConfig cfg)
        {
            string assetPath = $"{OutputPath}/{cfg.fileName}.asset";

            // Delete stale asset so broken script references are always replaced
            if (File.Exists(assetPath))
                AssetDatabase.DeleteAsset(assetPath);

            var data = ScriptableObject.CreateInstance<RealmAtmosphereData>();
            data.realmName            = cfg.realmName;
            data.realmId              = cfg.realmId;
            data.ambientLightColor    = cfg.ambientColor;
            data.ambientLightIntensity= cfg.ambientIntensity;
            data.mainLightColor       = cfg.lightColor;
            data.mainLightIntensity   = cfg.lightIntensity;
            data.mainLightRotation    = cfg.lightRotation;
            data.enableFog            = true;
            data.fogColor             = cfg.fogColor;
            data.fogStart             = cfg.fogStart;
            data.fogEnd               = cfg.fogEnd;
            data.enablePostProcessing = true;
            data.bloomIntensity       = cfg.bloom;
            data.vignettIntensity     = cfg.vignette;
            data.exposure             = cfg.exposure;
            data.saturation           = cfg.saturation;
            data.autoPlayParticles    = true;
            data.particleIntensity    = 1f;
            data.ambienceKeyColor     = cfg.keyColor;
            data.ambienceFillColor    = cfg.fillColor;
            data.ambienceIntensity    = cfg.ambienceIntensity;

            AssetDatabase.CreateAsset(data, assetPath);
            Debug.Log($"[CreateRealmAtmosphereAssets] Created: {cfg.fileName}");
        }

        private struct RealmAtmosphereConfig
        {
            public string fileName, realmName, realmId;
            public Color  ambientColor, lightColor, fogColor, keyColor, fillColor;
            public float  ambientIntensity, lightIntensity, lightRotation;
            public float  fogStart, fogEnd, bloom, vignette, exposure, saturation, ambienceIntensity;
        }
    }
}
