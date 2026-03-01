using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AscendantContinuum.Editor
{
    public static class RealmSceneValidator
    {
        private static readonly string[] RealmScenePaths =
        {
            "Assets/_Project/Scenes/Realms/Realm_Emberforge.unity",
            "Assets/_Project/Scenes/Realms/Realm_Verdant.unity",
            "Assets/_Project/Scenes/Realms/Realm_EchoFields.unity",
            "Assets/_Project/Scenes/Realms/Realm_DawnCitadel.unity",
            "Assets/_Project/Scenes/Realms/Realm_LanternAscension.unity"
        };

        [MenuItem("Tools/Ascendant Continuum/Validate Realm Scenes")]
        public static void ValidateRealmScenesMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (ValidateRealmScenesForBuild(out string report))
            {
                Debug.Log(report);
                EditorUtility.DisplayDialog("Realm Scene Validator", "Validation passed. Realm scenes are build-ready.", "OK");
            }
            else
            {
                Debug.LogError(report);
                EditorUtility.DisplayDialog("Realm Scene Validator", "Validation failed. See Console for details.", "OK");
            }
        }

        public static bool ValidateRealmScenesForBuild(out string report)
        {
            StringBuilder sb = new StringBuilder();
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                HashSet<string> enabledBuildScenes = new HashSet<string>();
                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                {
                    if (scene.enabled)
                    {
                        enabledBuildScenes.Add(scene.path);
                    }
                }

                foreach (string scenePath in RealmScenePaths)
                {
                    ValidateScene(scenePath, enabledBuildScenes, errors, warnings);
                }
            }
            finally
            {
                if (previousSetup != null && previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }

            sb.AppendLine("[RealmSceneValidator] Validation Summary");
            sb.AppendLine($"Checked realm scenes: {RealmScenePaths.Length}");
            sb.AppendLine($"Errors: {errors.Count}");
            sb.AppendLine($"Warnings: {warnings.Count}");

            if (warnings.Count > 0)
            {
                sb.AppendLine("Warnings:");
                for (int i = 0; i < warnings.Count; i++)
                {
                    sb.AppendLine($" - {warnings[i]}");
                }
            }

            if (errors.Count > 0)
            {
                sb.AppendLine("Errors:");
                for (int i = 0; i < errors.Count; i++)
                {
                    sb.AppendLine($" - {errors[i]}");
                }

                report = sb.ToString();
                return false;
            }

            sb.AppendLine("Result: PASS");
            report = sb.ToString();
            return true;
        }

        private static void ValidateScene(
            string scenePath,
            HashSet<string> enabledBuildScenes,
            List<string> errors,
            List<string> warnings)
        {
            if (!File.Exists(scenePath))
            {
                errors.Add($"Missing realm scene file: {scenePath}");
                return;
            }

            if (!enabledBuildScenes.Contains(scenePath))
            {
                errors.Add($"Realm scene is not enabled in Build Settings: {scenePath}");
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                errors.Add($"Could not load scene for validation: {scenePath}");
                return;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            int missingScriptCount = 0;
            int cameraCount = 0;
            int audioListenerCount = 0;
            int realmControllerCount = 0;

            foreach (GameObject root in roots)
            {
                CollectMissingScriptsRecursive(root, ref missingScriptCount);

                Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
                cameraCount += cameras.Length;

                AudioListener[] listeners = root.GetComponentsInChildren<AudioListener>(true);
                audioListenerCount += listeners.Length;

                AscendantContinuum.Core.RealmController[] controllers = root.GetComponentsInChildren<AscendantContinuum.Core.RealmController>(true);
                realmControllerCount += controllers.Length;
            }

            if (missingScriptCount > 0)
            {
                errors.Add($"{scenePath}: found {missingScriptCount} missing script reference(s)");
            }

            if (realmControllerCount == 0)
            {
                errors.Add($"{scenePath}: no RealmController found");
            }

            if (cameraCount == 0)
            {
                errors.Add($"{scenePath}: no Camera found");
            }

            if (audioListenerCount == 0)
            {
                warnings.Add($"{scenePath}: no AudioListener found");
            }
            else if (audioListenerCount > 1)
            {
                warnings.Add($"{scenePath}: multiple AudioListeners found ({audioListenerCount})");
            }
        }

        private static void CollectMissingScriptsRecursive(GameObject go, ref int missingScriptCount)
        {
            missingScriptCount += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

            Transform transform = go.transform;
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                CollectMissingScriptsRecursive(transform.GetChild(i).gameObject, ref missingScriptCount);
            }
        }
    }
}
