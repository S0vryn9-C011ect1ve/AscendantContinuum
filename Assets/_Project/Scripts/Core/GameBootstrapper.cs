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

            // Systems
            EnsureManager<DailyChallengeManager>(root);
            EnsureManager<AchievementManager>(root);
            EnsureManager<CosmicIdentitySystem>(root);
            EnsureManager<LiveEventEngine>(root);
            EnsureManager<GuardianMessengerSystem>(root);

            // Platform
            EnsureManager<MistplayManager>(root);
            EnsureManager<GooglePlayGamesManager>(root);

            // Social
            EnsureManager<CrossPlayerWishWall>(root);

            // Optional / addon systems — gracefully skip if types are absent
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.MindfulPlayManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.NatureConnectionManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Systems.SacredTimingManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Astronomy.CosmicDataManager");
            TryEnsureManagerByName(root, "AscendantContinuum.Astronomy.MoonPhaseEffects");
        }

        /// <summary>
        /// Adds a MonoBehaviour of type <typeparamref name="T"/> to
        /// <paramref name="root"/> only when no existing instance is found in
        /// the scene.
        /// </summary>
        private static void EnsureManager<T>(GameObject root) where T : MonoBehaviour
        {
            if (Object.FindObjectOfType<T>() == null)
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

            if (Object.FindObjectOfType(type) == null)
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

            // Wire Live Event → push notifications
            if (LiveEventEngine.Instance != null)
                LiveEventEngine.Instance.OnEventPeak += evt =>
                    GuardianMessengerSystem.Instance?.SendImmediateEventNotification(evt);

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

            if (GameManager.Instance.ShouldRunOnboarding())
            {
                Debug.Log("[GameBootstrapper] First-run — routing to Onboarding.");
                SceneManager.LoadScene(SceneNames.Onboarding);
            }
            else
            {
                Debug.Log("[GameBootstrapper] Returning player — routing to MainMenu.");
                SceneManager.LoadScene(SceneNames.MainMenu);
            }
        }
    }
}
