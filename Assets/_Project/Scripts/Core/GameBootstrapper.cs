using UnityEngine;
using UnityEngine.SceneManagement;
using AscendantContinuum.Systems;
using AscendantContinuum.Platform;
using AscendantContinuum.Social;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// <para>Entry-point for The Ascendant Continuum runtime.</para>
    ///
    /// <para>Uses <see cref="RuntimeInitializeOnLoadMethodAttribute"/> with
    /// <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> so it fires
    /// before any scene's Awake(). It creates every persistent singleton
    /// manager once and keeps them alive for the duration of the session.</para>
    ///
    /// <para>In the Editor play-mode the Bootstrap scene is not guaranteed to
    /// be loaded first; this class compensates by creating managers
    /// programmatically whenever they are absent.</para>
    /// </summary>
    public sealed class GameBootstrapper : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        private static GameBootstrapper _instance;

        // ── Manager roots ──────────────────────────────────────────────────
        private const string ROOT_NAME = "[AscendantContinuum Managers]";

        // ─────────────────────────────────────────────────────────────────
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null) return;

            var root = new GameObject(ROOT_NAME);
            DontDestroyOnLoad(root);
            _instance = root.AddComponent<GameBootstrapper>();

            _instance.SpawnManagers(root);

            Debug.Log("[GameBootstrapper] All managers initialised.");
        }

        // ── Manager spawning ───────────────────────────────────────────────

        private void SpawnManagers(GameObject root)
        {
            // Core managers
            EnsureManager<GameManager>(root);
            EnsureManager<SaveSystem>(root);
            EnsureManager<AccessibilityManager>(root);
            EnsureManager<AudioManager>(root);
            EnsureManager<RealmTransitionManager>(root);
            EnsureManager<FirebaseManager>(root);
            EnsureManager<GDPRConsentManager>(root);
            EnsureManager<LocalNotificationManager>(root);

            // Systems
            EnsureManager<DailyChallengeManager>(root);
            EnsureManager<AchievementManager>(root);
            EnsureManager<CosmicIdentitySystem>(root);
            EnsureManager<LiveEventEngine>(root);
            EnsureManager<GuardianMessengerSystem>(root);

            // Platform
            EnsureManager<MistplayManager>(root);
            EnsureManager<GooglePlayGamesManager>(root);
            EnsureManager<CosmicPatronManager>(root);

            // Social
            EnsureManager<CrossPlayerWishWall>(root);
            EnsureManager<KindnessChainManager>(root);
            EnsureManager<TimeCapsuleManager>(root);

            // ── New systems (Phases 1-8) ───────────────────────────────────
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SessionManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.ContinuumFieldManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SigilMutationSystem");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.CosmicQuoteSystem");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.CosmeticSystem");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.RealWorldNudgeSystem");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SkyTimeSystem");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SigilCompletionHandler");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SigilArtifactExporter");

            // Optional / addon systems — gracefully skip if types are absent
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.MindfulPlayManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.NatureConnectionManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SacredTimingManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Astronomy.CosmicDataManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Astronomy.MoonPhaseEffects");
            TryEnsureManagerByName(root, "AscendantContinuum.Analytics.AnalyticsManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SeasonController");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.PantheonDeityEffects");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SerendipityManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.RitualReplayManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.LivingLoreManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SigilCraftingManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SigilGenerator");
            TryEnsureManagerByName(root, "AscendantContinuum.Progression.ProgressionManager");
            TryEnsureManagerByName(root, "AscendantContinuum.UI.SeasonPassUIManager");
            TryEnsureManagerByName(root, "AscendantContinuum.UI.PantheonQuizPanel");
        }

        /// <summary>
        /// Adds a MonoBehaviour of type <typeparamref name="T"/> to
        /// <paramref name="root"/> only when no existing instance is found in
        /// the scene.
        /// </summary>
        private static void EnsureManager<T>(GameObject root) where T : MonoBehaviour
        {
            if (UnityEngine.Object.FindFirstObjectByType<T>() == null)
            {
                root.AddComponent<T>();
                Debug.Log($"[GameBootstrapper] Created {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Reflection-based fallback for optional managers whose assembly type
        /// may not be present in every build configuration.
        /// </summary>
        private static void TryEnsureManagerByName(GameObject root, string fullTypeName)
        {
            var type = System.Type.GetType(fullTypeName);
            if (type == null) return;

            if (UnityEngine.Object.FindFirstObjectByType(type) == null)
            {
                root.AddComponent(type);
                Debug.Log($"[GameBootstrapper] Created {type.Name} (optional)");
            }
        }

        // ── Scene routing on first load ────────────────────────────────────

        private void Start()
        {
            string activeScene = SceneManager.GetActiveScene().name;

            // Record session with Cosmic Identity so play-time metrics evolve
            CosmicIdentitySystem.Instance?.RecordSessionStart();

            // Increment total session counter (shared PlayerPrefs key used by PantheonDeityEffects.ShouldShowPantheonQuiz)
            PlayerPrefs.SetInt("SessionCount_Total", PlayerPrefs.GetInt("SessionCount_Total", 0) + 1);
            PlayerPrefs.Save();

            // Day 4 gate: prompt Arcane Personality Quiz only after leaving bootstrap
            // so UI setup issues in quiz panel cannot block initial scene routing.
            if (activeScene != SceneNames.Bootstrap)
            {
                var pantheon = UnityEngine.Object.FindFirstObjectByType<PantheonDeityEffects>();
                if (pantheon != null && pantheon.ShouldShowPantheonQuiz())
                    GameEvents.RaisePantheonQuizReady();
            }

            // Wire Live Event → push notifications + local push notifications
            if (LiveEventEngine.Instance != null)
            {
                LiveEventEngine.Instance.OnEventPeak += evt =>
                    GuardianMessengerSystem.Instance?.SendImmediateEventNotification(evt);
                LiveEventEngine.Instance.OnEventApproaching += evt =>
                    LocalNotificationManager.Instance?.ScheduleLiveEventNotification(evt);
            }

            // Wire DailyChallenge completion → reschedule tomorrow's reminder
            if (DailyChallengeManager.Instance != null)
                DailyChallengeManager.Instance.OnChallengeCompleted += _ =>
                    LocalNotificationManager.Instance?.ScheduleDailyChallengeReminder();

            // Wire GDPR consent granted → schedule daily notification for first time
            if (GDPRConsentManager.Instance != null)
                GDPRConsentManager.Instance.OnConsentGiven += () =>
                    LocalNotificationManager.Instance?.ScheduleDailyChallengeReminder();

            // When the game starts on the Bootstrap scene, route to the
            // correct destination determined by save-data state.
            if (activeScene == SceneNames.Bootstrap)
            {
                RouteFromBootstrap();
                return;
            }

            // If play-mode was started from a realm scene in the Editor we
            // still need the HUD, so nothing extra to do — managers already
            // exist above.
        }

        private static void RouteFromBootstrap()
        {
            if (GameManager.Instance == null) return;

            GameFlowConfig flow = GameFlow.Config;
            string onboardingScene = flow != null ? flow.OnboardingScene : SceneNames.Onboarding;
            string mainMenuScene = flow != null ? flow.MainMenuScene : SceneNames.MainMenu;

            if (GameManager.Instance.ShouldRunOnboarding())
            {
                Debug.Log("[GameBootstrapper] First-run — routing to Onboarding.");
                if (!SceneService.TryLoadScene(onboardingScene))
                {
                    SceneService.TryLoadScene(SceneNames.Onboarding);
                }
            }
            else
            {
                Debug.Log("[GameBootstrapper] Returning player — routing to MainMenu.");
                if (!SceneService.TryLoadScene(mainMenuScene))
                {
                    SceneService.TryLoadScene(SceneNames.MainMenu);
                }
            }
        }
    }
}
