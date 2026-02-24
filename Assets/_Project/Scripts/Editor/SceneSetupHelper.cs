using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using AscendantContinuum.Core;
using AscendantContinuum.UI;
using AscendantContinuum.Realms.Emberforge;
using AscendantContinuum.Realms.Verdant;
using AscendantContinuum.Realms.EchoFields;
using AscendantContinuum.Realms.DawnCitadel;
using AscendantContinuum.Realms.LanternAscension;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Editor helper that creates every required Unity scene asset and populates
    /// the Build Settings scene list in one click.
    ///
    /// Menu: <c>Ascendant Continuum / Setup / Create All Scenes</c>
    /// </summary>
    public static class SceneSetupHelper
    {
        private const string SCENE_ROOT = "Assets/_Project/Scenes";

        // ── Menu entry ─────────────────────────────────────────────────────
        [MenuItem("Ascendant Continuum/Setup/Create All Scenes %#&s", priority = 0)]
        public static void CreateAllScenes()
        {
            if (!PrepareEditorSceneState())
                return;

            EditorUtility.DisplayProgressBar("Scene Setup", "Creating scenes…", 0f);

            try
            {
                EnsureDirectory(SCENE_ROOT + "/Core");
                EnsureDirectory(SCENE_ROOT + "/Realms");

                CreateBootstrapScene();
                CreateMainMenuScene();
                CreateOnboardingScene();
                CreateEmberforgeScene();
                CreateVerdantScene();
                CreateEchoFieldsScene();
                CreateDawnCitadelScene();
                CreateLanternAscensionScene();

                RegisterScenesInBuildSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[SceneSetupHelper] ✅ All scenes created and registered in Build Settings.");
                EditorUtility.DisplayDialog("Scene Setup Complete",
                    "All 8 scenes have been created and added to Build Settings.", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool PrepareEditorSceneState()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            var activeScene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.path))
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Unsaved Scene Open",
                    "Create All Scenes needs a saved active scene before additive scene generation. Continue and save a temporary setup scene?",
                    "Continue",
                    "Cancel");

                if (!proceed)
                    return false;

                EnsureDirectory(SCENE_ROOT + "/Core");
                string tempPath = SCENE_ROOT + "/Core/__SceneSetupTemp.unity";
                if (!EditorSceneManager.SaveScene(activeScene, tempPath))
                {
                    EditorUtility.DisplayDialog(
                        "Scene Setup",
                        "Could not save temporary setup scene. Please save the active scene and try again.",
                        "OK");
                    return false;
                }
            }

            return true;
        }

        // ── Scene creators ─────────────────────────────────────────────────

        private static void CreateBootstrapScene()
        {
            var scene = NewScene(SceneNames.Bootstrap, SCENE_ROOT + "/Core");
            if (!scene.IsValid()) return;

            // Bootstrap scene just needs a GameObject that holds the Bootstrapper
            var go = new GameObject("[Bootstrap]");
            go.AddComponent<GameBootstrapper>();

            SaveAndClose(scene, SCENE_ROOT + "/Core/" + SceneNames.Bootstrap + ".unity");
        }

        private static void CreateMainMenuScene()
        {
            var scene = NewScene(SceneNames.MainMenu, SCENE_ROOT + "/Core");
            if (!scene.IsValid()) return;

            var go = new GameObject("[MainMenu]");
            go.AddComponent<MainMenuManager>();

            SaveAndClose(scene, SCENE_ROOT + "/Core/" + SceneNames.MainMenu + ".unity");
        }

        private static void CreateOnboardingScene()
        {
            var scene = NewScene(SceneNames.Onboarding, SCENE_ROOT + "/Core");
            if (!scene.IsValid()) return;

            var go = new GameObject("[Onboarding]");
            go.AddComponent<OnboardingController>();

            SaveAndClose(scene, SCENE_ROOT + "/Core/" + SceneNames.Onboarding + ".unity");
        }

        private static void CreateEmberforgeScene()
        {
            var scene = NewScene(SceneNames.Emberforge, SCENE_ROOT + "/Realms");
            if (!scene.IsValid()) return;

            SetupRealmSceneObjects<EmberforgeController>(scene, "Emberforge");
            SaveAndClose(scene, SCENE_ROOT + "/Realms/" + SceneNames.Emberforge + ".unity");
        }

        private static void CreateVerdantScene()
        {
            var scene = NewScene(SceneNames.Verdant, SCENE_ROOT + "/Realms");
            if (!scene.IsValid()) return;

            SetupRealmSceneObjects<VerdantController>(scene, "Verdant");
            SaveAndClose(scene, SCENE_ROOT + "/Realms/" + SceneNames.Verdant + ".unity");
        }

        private static void CreateEchoFieldsScene()
        {
            var scene = NewScene(SceneNames.EchoFields, SCENE_ROOT + "/Realms");
            if (!scene.IsValid()) return;

            SetupRealmSceneObjects<EchoFieldsController>(scene, "EchoFields");
            SaveAndClose(scene, SCENE_ROOT + "/Realms/" + SceneNames.EchoFields + ".unity");
        }

        private static void CreateDawnCitadelScene()
        {
            var scene = NewScene(SceneNames.DawnCitadel, SCENE_ROOT + "/Realms");
            if (!scene.IsValid()) return;

            SetupRealmSceneObjects<DawnCitadelController>(scene, "DawnCitadel");
            SaveAndClose(scene, SCENE_ROOT + "/Realms/" + SceneNames.DawnCitadel + ".unity");
        }

        private static void CreateLanternAscensionScene()
        {
            var scene = NewScene(SceneNames.LanternAscension, SCENE_ROOT + "/Realms");
            if (!scene.IsValid()) return;

            SetupRealmSceneObjects<LanternAscensionController>(scene, "LanternAscension");
            SaveAndClose(scene, SCENE_ROOT + "/Realms/" + SceneNames.LanternAscension + ".unity");
        }

        // ── Shared helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Creates or opens a scene. Returns an invalid scene handle if the
        /// .unity file already exists (to preserve manual work).
        /// </summary>
        private static Scene NewScene(string name, string dir)
        {
            string path = dir + "/" + name + ".unity";

            if (File.Exists(path))
            {
                Debug.Log($"[SceneSetupHelper] Skipping '{name}' — scene already exists at {path}");
                return default;       // invalid = skip
            }

            return EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
        }

        private static void SetupRealmSceneObjects<TController>(Scene scene, string realmName)
            where TController : MonoBehaviour
        {
            SceneManager.SetActiveScene(scene);

            var root = new GameObject($"[{realmName} Realm]");
            root.AddComponent<TController>();

            // Camera is added by DefaultGameObjects, but rename it
            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null) cam.gameObject.name = $"{realmName}_Camera";
        }

        private static void SaveAndClose(Scene scene, string path)
        {
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);
            Debug.Log($"[SceneSetupHelper] Created scene: {path}");
        }

        /// <summary>Creates the directory at <paramref name="path"/> if it does not already exist.</summary>
        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Builds the EditorBuildSettings.scenes list from <see cref="SceneNames.AllScenes"/>.
        /// Preserves any extra scenes the developer may have added manually.
        /// </summary>
        [MenuItem("Ascendant Continuum/Setup/Register Scenes In Build Settings", priority = 1)]
        public static void RegisterScenesInBuildSettings()
        {
            var existing = new System.Collections.Generic.HashSet<string>();
            foreach (var s in EditorBuildSettings.scenes)
                existing.Add(s.path);

            var newEntries = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            foreach (string sceneName in SceneNames.AllScenes)
            {
                // Search in both Core and Realms directories
                string[] guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
                if (guids.Length == 0)
                {
                    Debug.LogWarning($"[SceneSetupHelper] Scene file not found for '{sceneName}'. Create it first.");
                    continue;
                }

                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                if (!existing.Contains(path))
                {
                    newEntries.Add(new EditorBuildSettingsScene(path, true));
                    existing.Add(path);
                    Debug.Log($"[SceneSetupHelper] Registered: {path}");
                }
            }

            EditorBuildSettings.scenes = newEntries.ToArray();
            Debug.Log("[SceneSetupHelper] Build Settings scene list updated.");
        }
    }
}
