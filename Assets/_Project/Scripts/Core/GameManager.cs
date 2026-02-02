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
            
            // Initialize accessibility settings first
            AccessibilityManager.Instance?.LoadSettings();
            
            Debug.Log("[GameManager] Game initialized - Accessibility-first mode enabled");
            
            ChangeState(GameState.MainMenu);
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

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Save game state when app goes to background
                SaveSystem.Instance?.SaveGame();
                Debug.Log("[GameManager] Game paused - auto-saved");
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
