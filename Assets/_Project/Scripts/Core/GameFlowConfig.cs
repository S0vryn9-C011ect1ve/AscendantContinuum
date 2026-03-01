using UnityEngine;

namespace AscendantContinuum.Core
{
    [CreateAssetMenu(fileName = "GameFlowConfig", menuName = "Ascendant Continuum/Core/Game Flow Config")]
    public sealed class GameFlowConfig : ScriptableObject
    {
        [Header("Scenes")]
        [SerializeField] private string bootstrapScene = SceneNames.Bootstrap;
        [SerializeField] private string mainMenuScene = SceneNames.MainMenu;
        [SerializeField] private string onboardingScene = SceneNames.Onboarding;

        [Header("Defaults")]
        [SerializeField] private string defaultRealmId = "emberforge";
        [SerializeField] private bool forceOnboardingInEditor = false;

        [Header("Transition")]
        [SerializeField] private float normalTransitionDuration = 2f;
        [SerializeField] private float reducedMotionTransitionDuration = 0.5f;

        public string BootstrapScene => string.IsNullOrWhiteSpace(bootstrapScene) ? SceneNames.Bootstrap : bootstrapScene;
        public string MainMenuScene => string.IsNullOrWhiteSpace(mainMenuScene) ? SceneNames.MainMenu : mainMenuScene;
        public string OnboardingScene => string.IsNullOrWhiteSpace(onboardingScene) ? SceneNames.Onboarding : onboardingScene;
        public string DefaultRealmId => string.IsNullOrWhiteSpace(defaultRealmId) ? "emberforge" : defaultRealmId;
        public bool ForceOnboardingInEditor => forceOnboardingInEditor;
        public float NormalTransitionDuration => Mathf.Max(0.1f, normalTransitionDuration);
        public float ReducedMotionTransitionDuration => Mathf.Max(0.05f, reducedMotionTransitionDuration);
    }

    public static class GameFlow
    {
        private const string ResourcePath = "Config/GameFlowConfig";

        private static bool _loaded;
        private static GameFlowConfig _config;

        public static GameFlowConfig Config
        {
            get
            {
                if (!_loaded)
                {
                    _config = Resources.Load<GameFlowConfig>(ResourcePath);
                    _loaded = true;
                }

                return _config;
            }
        }
    }
}
