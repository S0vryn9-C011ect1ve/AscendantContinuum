using UnityEngine;
using System;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Central game manager - handles game state, session management, and core systems
    /// Accessibility-first: Manages accessibility settings and ensures consistent state
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private const string OnboardingCompletedKey = "OnboardingCompleted";
        private const string LastRealmKey = "LastRealm";

        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Initializing;
        [SerializeField] private string currentRealmId = "emberforge";

        [Header("Session Settings")]
        [SerializeField] private float targetFrameRate = 60f;
        [SerializeField] private bool enableHaptics = true;

        public event Action<GameState> OnGameStateChanged;
        public event Action<string> OnRealmChanged;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeGame();
        }

        private void InitializeGame()
        {
            // Set target frame rate for mobile optimization
            Application.targetFrameRate = (int)targetFrameRate;

            // Ensure screen never sleeps during gameplay
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Pass haptic preference through to AccessibilityManager
            if (AccessibilityManager.Instance != null)
            {
                AccessibilityManager.Instance.SetHaptics(enableHaptics);
                AccessibilityManager.Instance.LoadSettings();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            GameState previousState = currentState;
            currentState = newState;

            Debug.Log($"[GameManager] State changed: {previousState} → {newState}");
            OnGameStateChanged?.Invoke(newState);
        }

        public void LoadRealm(string realmId)
        {
            if (currentRealmId == realmId) return;

            currentRealmId = realmId;
            Debug.Log($"[GameManager] Loading realm: {realmId}");
            OnRealmChanged?.Invoke(realmId);
        }

        public GameState CurrentState => currentState;
        public string CurrentRealm => currentRealmId;

        public bool ShouldRunOnboarding()
        {
            return PlayerPrefs.GetInt(OnboardingCompletedKey, 0) == 0;
        }

        public void MarkOnboardingCompleted()
        {
            PlayerPrefs.SetInt(OnboardingCompletedKey, 1);
            PlayerPrefs.Save();
        }

        public string GetLastRealmOrDefault(string fallbackRealm)
        {
            return PlayerPrefs.GetString(LastRealmKey, fallbackRealm);
        }

        public void RecordLastRealm(string realmName)
        {
            if (string.IsNullOrWhiteSpace(realmName))
            {
                return;
            }

            PlayerPrefs.SetString(LastRealmKey, realmName);
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Record pause time so PantheonPlanetManager can track how long the player was outside
                double unixNow = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                PlayerPrefs.SetFloat("App_LastPausedUnixTime", (float)unixNow);

                // Save game state when app goes to background
                SaveSystem.Instance?.SaveGame();
                Debug.Log("[GameManager] Game paused - auto-saved");
            }
            else
            {
                // Clear pause time when app resumes (so the tracker knows it was a real pause)
                double unixNow = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                PlayerPrefs.SetFloat("App_LastResumedUnixTime", (float)unixNow);
                PlayerPrefs.Save();
                Debug.Log("[GameManager] Game resumed");
            }
        }

        private void OnApplicationQuit()
        {
            SaveSystem.Instance?.SaveGame();
            Debug.Log("[GameManager] Game quit - final save complete");
        }
    }

    public enum GameState
    {
        Initializing,
        MainMenu,
        Playing,
        Paused,
        Ritual,
        Transition
    }
}
