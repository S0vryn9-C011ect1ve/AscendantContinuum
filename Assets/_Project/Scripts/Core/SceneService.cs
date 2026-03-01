using UnityEngine;
using UnityEngine.SceneManagement;

namespace AscendantContinuum.Core
{
    public static class SceneService
    {
        public static string ResolveRealmScene(string realmId)
        {
            return SceneNames.FromRealmId(realmId);
        }

        public static bool CanLoadScene(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);
        }

        public static bool TryLoadScene(string sceneName)
        {
            if (!CanLoadScene(sceneName))
            {
                Debug.LogError($"[SceneService] Scene cannot be loaded: {sceneName}");
                return false;
            }

            SceneManager.LoadScene(sceneName);
            return true;
        }

        public static AsyncOperation TryLoadSceneAsync(string sceneName)
        {
            if (!CanLoadScene(sceneName))
            {
                Debug.LogError($"[SceneService] Scene cannot be loaded async: {sceneName}");
                return null;
            }

            return SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
