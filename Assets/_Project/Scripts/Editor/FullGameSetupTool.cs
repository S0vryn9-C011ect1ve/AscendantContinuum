#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using AscendantContinuum.Core;
using AscendantContinuum.Data;
using AscendantContinuum.UI;
using AscendantContinuum.Realms;
using AscendantContinuum.VFX;
using AscendantContinuum.Cameras;
// Realm controllers
using AscendantContinuum.Realms.Emberforge;
using AscendantContinuum.Emberforge;
using AscendantContinuum.Realms.Verdant;
using AscendantContinuum.Verdant;
using AscendantContinuum.Realms.EchoFields;
using AscendantContinuum.EchoFields;
using AscendantContinuum.Realms.DawnCitadel;
using AscendantContinuum.Realms.LanternAscension;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// One-click full game setup: creates all missing ScriptableObject assets,
    /// wires realm scenes with the required hierarchy, and validates Build Settings.
    ///
    /// Menu → Ascendant Continuum / Setup / 🚀 Full Game Setup (Run All)
    /// </summary>
    public static class FullGameSetupTool
    {
        // ── Paths ───────────────────────────────────────────────────────────
        private const string CONFIG_PATH      = "Assets/_Project/Resources/Config";
        private const string REALM_DATA_PATH  = "Assets/_Project/Resources/RealmData";
        private const string SCENE_CORE       = "Assets/_Project/Scenes/Core";
        private const string SCENE_REALMS     = "Assets/_Project/Scenes/Realms";

        // ── Main Entry ─────────────────────────────────────────────────────
        [MenuItem("Ascendant Continuum/Setup/🚀 Full Game Setup (Run All)", priority = 0)]
        public static void RunFullSetup()
        {
            if (!EditorUtility.DisplayDialog(
                    "Full Game Setup",
                    "This will:\n" +
                    "• Create GameFlowConfig asset\n" +
                    "• Create 5 RealmData assets\n" +
                    "• Create AudioSourceData asset\n" +
                    "• Populate all 8 Unity scenes with required components\n" +
                    "• Verify Build Settings scene list\n\n" +
                    "Existing assets will NOT be overwritten. Continue?",
                    "Run Setup", "Cancel"))
                return;

            float step = 0f;

            try
            {
                EditorUtility.DisplayProgressBar("Full Game Setup", "Creating GameFlowConfig…", step += 0.1f);
                CreateGameFlowConfig();

                EditorUtility.DisplayProgressBar("Full Game Setup", "Creating RealmData assets…", step += 0.1f);
                CreateAllRealmDataAssets();

                EditorUtility.DisplayProgressBar("Full Game Setup", "Creating AudioSourceData…", step += 0.1f);
                CreateAudioSourceData();

                EditorUtility.DisplayProgressBar("Full Game Setup", "Setting up Bootstrap scene…", step += 0.1f);
                SetupBootstrapScene();

                EditorUtility.DisplayProgressBar("Full Game Setup", "Setting up MainMenu scene…", step += 0.1f);
                SetupMainMenuScene();

                EditorUtility.DisplayProgressBar("Full Game Setup", "Setting up Onboarding scene…", step += 0.1f);
                SetupOnboardingScene();

                EditorUtility.DisplayProgressBar("Full Game Setup", "Setting up Realm scenes (5)…", step += 0.1f);
                SetupAllRealmScenes();

                EditorUtility.DisplayProgressBar("Full Game Setup", "Verifying Build Settings…", step += 0.1f);
                EnsureBuildSettingsScenes();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(
                    "Full Game Setup — Complete ✅",
                    "All assets created and scenes wired.\n\n" +
                    "Next steps:\n" +
                    "1. Drop art assets into Assets/_Project/Art/\n" +
                    "2. Drop audio into Assets/_Project/Audio/\n" +
                    "3. Assign sprites/clips to RealmData assets in inspector\n" +
                    "4. Run 'Ascendant Continuum/Setup/Validate Realm Scenes'",
                    "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[FullGameSetupTool] Setup failed: {ex}");
                EditorUtility.DisplayDialog("Setup Failed", ex.Message, "OK");
            }
        }

        // ── GameFlowConfig ──────────────────────────────────────────────────
        [MenuItem("Ascendant Continuum/Setup/Create GameFlowConfig Asset", priority = 10)]
        public static void CreateGameFlowConfig()
        {
            EnsureDir(CONFIG_PATH);
            string path = CONFIG_PATH + "/GameFlowConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<GameFlowConfig>(path) != null)
            {
                Debug.Log("[Setup] GameFlowConfig already exists — skipping.");
                return;
            }

            var cfg = ScriptableObject.CreateInstance<GameFlowConfig>();
            AssetDatabase.CreateAsset(cfg, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] Created GameFlowConfig at {path}");
        }

        // ── RealmData ───────────────────────────────────────────────────────
        [MenuItem("Ascendant Continuum/Setup/Create All RealmData Assets", priority = 11)]
        public static void CreateAllRealmDataAssets()
        {
            EnsureDir(REALM_DATA_PATH);
            CreateRealmData("Emberforge", "emberforge", "The Emberforge",
                "Dance with living flames and forge your inner spark.",
                new Color(1f, 0.38f, 0.10f), new Color(1f, 0.65f, 0.05f),
                RitualType.SparkCollection, isStart: true);

            CreateRealmData("Verdant", "verdant", "The Verdant Sanctuary",
                "Nurture life, witness growth, and find serenity in nature's rhythm.",
                new Color(0.12f, 0.72f, 0.30f), new Color(0.05f, 0.45f, 0.60f),
                RitualType.GrowthNurturing);

            CreateRealmData("EchoFields", "echo_fields", "The Echo Fields",
                "Trace constellations from ancient memory and awaken the stars.",
                new Color(0.18f, 0.22f, 0.75f), new Color(0.55f, 0.25f, 0.85f),
                RitualType.StarTracing);

            CreateRealmData("DawnCitadel", "dawn_citadel", "The Dawn Citadel",
                "Bend light through crystal prisms and reveal hidden truths.",
                new Color(1f, 0.85f, 0.20f), new Color(0.90f, 0.50f, 0.05f),
                RitualType.LightReflection);

            CreateRealmData("LanternAscension", "lantern_ascension", "The Lantern Ascension",
                "Release glowing wishes into the infinite and find peace.",
                new Color(0.55f, 0.30f, 0.90f), new Color(0.85f, 0.65f, 1.00f),
                RitualType.LanternRelease);

            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] All RealmData assets created/verified.");
        }

        private static void CreateRealmData(string fileName, string realmId, string realmName,
            string description, Color primary, Color secondary, RitualType ritual, bool isStart = false)
        {
            string path = $"{REALM_DATA_PATH}/{fileName}_RealmData.asset";
            if (AssetDatabase.LoadAssetAtPath<RealmData>(path) != null)
            {
                Debug.Log($"[Setup] {fileName}_RealmData already exists — skipping.");
                return;
            }

            var rd = ScriptableObject.CreateInstance<RealmData>();
            rd.realmId = realmId;
            rd.realmName = realmName;
            rd.description = description;
            rd.primaryColor = primary;
            rd.secondaryColor = secondary;
            rd.primaryRitual = ritual;
            rd.isStartingRealm = isStart;

            // Build a simple two-key gradient matching the realm palette
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(secondary, 0f), new GradientColorKey(primary, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            rd.atmosphereGradient = gradient;

            AssetDatabase.CreateAsset(rd, path);
            Debug.Log($"[Setup] Created {path}");
        }

        // ── AudioSourceData ─────────────────────────────────────────────────
        [MenuItem("Ascendant Continuum/Setup/Create AudioSourceData Asset", priority = 12)]
        public static void CreateAudioSourceData()
        {
            EnsureDir(CONFIG_PATH);
            string path = CONFIG_PATH + "/AudioSourceData.asset";
            if (AssetDatabase.LoadAssetAtPath<AudioSourceData>(path) != null)
            {
                Debug.Log("[Setup] AudioSourceData already exists — skipping.");
                return;
            }

            var data = ScriptableObject.CreateInstance<AudioSourceData>();
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] Created AudioSourceData at {path}");
        }

        // ── Scene Setup ─────────────────────────────────────────────────────
        [MenuItem("Ascendant Continuum/Setup/Setup All Scenes", priority = 20)]
        public static void SetupAllScenes()
        {
            SetupBootstrapScene();
            SetupMainMenuScene();
            SetupOnboardingScene();
            SetupAllRealmScenes();
            EnsureBuildSettingsScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[Setup] All scenes set up.");
        }

        private static void SetupBootstrapScene()
        {
            string path = SCENE_CORE + "/Bootstrap.unity";
            if (!File.Exists(path)) { Debug.LogWarning($"[Setup] Bootstrap scene not found at {path}"); return; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            EnsureSceneComponent<GameBootstrapper>("[ Bootstrap ]");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Setup] Bootstrap scene configured.");
        }

        private static void SetupMainMenuScene()
        {
            string path = SCENE_CORE + "/MainMenu.unity";
            if (!File.Exists(path)) { Debug.LogWarning($"[Setup] MainMenu scene not found at {path}"); return; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Root manager
            EnsureSceneComponent<MainMenuManager>("[ MainMenu ]");

            // Background camera
            EnsureCamera("Main Camera");

            // UI Canvas
            var canvas = EnsureCanvas("MainMenuCanvas");

            // Fade panel – used by MainMenuManager for scene transitions
            EnsureImageChild(canvas, "FadePanel",
                new Color(0f, 0f, 0f, 0f),
                stretch: true);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Setup] MainMenu scene configured.");
        }

        private static void SetupOnboardingScene()
        {
            string path = SCENE_CORE + "/Onboarding.unity";
            if (!File.Exists(path)) { Debug.LogWarning($"[Setup] Onboarding scene not found at {path}"); return; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            EnsureSceneComponent<OnboardingController>("[ Onboarding ]");
            EnsureCamera("Main Camera");
            EnsureCanvas("OnboardingCanvas");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Setup] Onboarding scene configured.");
        }

        [MenuItem("Ascendant Continuum/Setup/Setup All Realm Scenes", priority = 21)]
        public static void SetupAllRealmScenes()
        {
            SetupRealmScene<EmberforgeController>("Realm_Emberforge");
            SetupRealmScene<VerdantController>("Realm_Verdant");
            SetupRealmScene<EchoFieldsController>("Realm_EchoFields");
            SetupRealmScene<DawnCitadelController>("Realm_DawnCitadel");
            SetupRealmScene<LanternAscensionController>("Realm_LanternAscension");
        }

        private static void SetupRealmScene<TController>(string sceneName)
            where TController : MonoBehaviour
        {
            string path = $"{SCENE_REALMS}/{sceneName}.unity";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Setup] Scene not found: {path}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            string displayName = sceneName.Replace("Realm_", "");

            // ── Camera Rig ─────────────────────────────────────────────
            var camGO = EnsureCamera("Main Camera");
            EnsureComponent<RealmCameraRig>(camGO);

            // ── Atmosphere ─────────────────────────────────────────────
            var atmosGO = EnsureNamedGO("[ Atmosphere ]");
            EnsureComponent<RealmAtmosphereController>(atmosGO);

            // ── Background ─────────────────────────────────────────────
            var bgGO = EnsureNamedGO("[ Background ]");
            var bgSR = EnsureComponent<SpriteRenderer>(bgGO);
            bgSR.sortingOrder = -100;
            bgGO.transform.position = new Vector3(0f, 0f, 10f);

            // ── VFX ────────────────────────────────────────────────────
            var vfxGO = EnsureNamedGO("[ VFX ]");
            EnsureComponent<RealmVFXManager>(vfxGO);

            // ── Gameplay Root ──────────────────────────────────────────
            var gameplayGO = EnsureNamedGO($"[ {displayName} Gameplay ]");
            EnsureComponent<TController>(gameplayGO);

            // Realm-specific child components
            AddRealmSpecificComponents(sceneName, gameplayGO);

            // ── HUD Canvas ─────────────────────────────────────────────
            var canvas = EnsureCanvas("HUDCanvas");
            EnsureComponent<HUDManager>(canvas);

            // Back button
            EnsureButtonChild(canvas, "BackButton", "← Back");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Setup] {sceneName} scene configured.");
        }

        private static void AddRealmSpecificComponents(string sceneName, GameObject parent)
        {
            switch (sceneName)
            {
                case "Realm_Emberforge":
                    EnsureComponent<EmberforgeSparks>(parent);
                    break;
                case "Realm_Verdant":
                    EnsureComponent<VerdantGarden>(parent);
                    break;
                case "Realm_EchoFields":
                    EnsureComponent<ConstellationTracer>(parent);
                    break;
                case "Realm_DawnCitadel":
                    EnsureComponent<LightRefractionPuzzle>(parent);
                    break;
                case "Realm_LanternAscension":
                    EnsureComponent<LanternRitual>(parent);
                    break;
            }
        }

        // ── Build Settings ──────────────────────────────────────────────────
        [MenuItem("Ascendant Continuum/Setup/Verify Build Settings Scenes", priority = 30)]
        public static void EnsureBuildSettingsScenes()
        {
            var orderedScenes = new[]
            {
                SCENE_CORE   + "/Bootstrap.unity",
                SCENE_CORE   + "/MainMenu.unity",
                SCENE_CORE   + "/Onboarding.unity",
                SCENE_REALMS + "/Realm_Emberforge.unity",
                SCENE_REALMS + "/Realm_Verdant.unity",
                SCENE_REALMS + "/Realm_EchoFields.unity",
                SCENE_REALMS + "/Realm_DawnCitadel.unity",
                SCENE_REALMS + "/Realm_LanternAscension.unity",
            };

            var existingScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var existingPaths  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in existingScenes) existingPaths.Add(s.path);

            bool changed = false;
            foreach (var p in orderedScenes)
            {
                if (!existingPaths.Contains(p) && File.Exists(p))
                {
                    existingScenes.Add(new EditorBuildSettingsScene(p, true));
                    existingPaths.Add(p);
                    changed = true;
                    Debug.Log($"[Setup] Added to Build Settings: {p}");
                }
            }

            if (changed)
                EditorBuildSettings.scenes = existingScenes.ToArray();
            else
                Debug.Log("[Setup] Build Settings already contains all scenes.");
        }

        // ── Helpers ─────────────────────────────────────────────────────────
        private static void EnsureDir(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static T EnsureSceneComponent<T>(string goName) where T : MonoBehaviour
        {
            var existing = UnityEngine.Object.FindObjectOfType<T>();
            if (existing != null) return existing;
            var go = new GameObject(goName);
            return go.AddComponent<T>();
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        private static GameObject EnsureNamedGO(string name)
        {
            var found = GameObject.Find(name);
            if (found != null) return found;
            return new GameObject(name);
        }

        private static GameObject EnsureCamera(string name)
        {
            var cam = UnityEngine.Object.FindObjectOfType<Camera>();
            if (cam != null) return cam.gameObject;
            var go = new GameObject(name) { tag = "MainCamera" };
            go.AddComponent<Camera>();
            return go;
        }

        private static GameObject EnsureCanvas(string name)
        {
            var found = GameObject.Find(name);
            if (found != null) return found;

            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static GameObject EnsureImageChild(GameObject parent, string name,
            Color color, bool stretch = false)
        {
            var existing = parent.transform.Find(name);
            if (existing != null) return existing.gameObject;

            var go    = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var img   = go.AddComponent<Image>();
            img.color = color;
            var rt    = go.GetComponent<RectTransform>();
            if (stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            return go;
        }

        private static GameObject EnsureButtonChild(GameObject parent, string name, string label)
        {
            var existing = parent.transform.Find(name);
            if (existing != null) return existing.gameObject;

            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>();
            go.AddComponent<Button>();

            var labelGO  = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var tmp      = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text     = label;
            tmp.fontSize = 18f;
            tmp.alignment = TextAlignmentOptions.Center;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 48f);
            rt.anchoredPosition = new Vector2(0f, -40f);

            return go;
        }
    }
}
#endif
