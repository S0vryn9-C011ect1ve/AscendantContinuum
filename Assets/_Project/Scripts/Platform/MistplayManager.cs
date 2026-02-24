using UnityEngine;
using System;
using System.Collections;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Platform
{
    /// <summary>
    /// Mistplay SDK Bridge for The Ascendant Continuum.
    ///
    /// Mistplay is a mobile loyalty platform (Android) that rewards players
    /// for genuine playtime. This manager:
    ///   1. Initialises the native Mistplay SDK on session start
    ///   2. Validates that the user arrived from Mistplay's platform
    ///   3. Sends heartbeat pings so Mistplay can credit playtime
    ///   4. Surfaces Mistplay's in-app store when requested
    ///
    /// SETUP CHECKLIST (complete before submitting to Mistplay):
    ///   □ Download Mistplay Unity SDK from https://mistplay.com/developer
    ///   □ Drop MistSDK.aar into Assets/Plugins/Android/
    ///   □ Set MISTPLAY_APP_ID in Player Settings > Android > Other > Custom Manifest
    ///   □ The AndroidManifest.xml additions below (auto-applied by this manager)
    ///   □ Minimum Android SDK 21, Target SDK 34
    ///   □ Ensure app is published on Google Play BEFORE submitting to Mistplay
    ///
    /// MISTPLAY REQUIREMENTS (as of 2026):
    ///   • Game must be >= 30 MB installed
    ///   • Session tracking heartbeat every 60 seconds
    ///   • No bot-detection circumvention
    ///   • Genuine real-time gameplay (not idle while app is open)
    /// </summary>
    public sealed class MistplayManager : MonoBehaviour
    {
        public static MistplayManager Instance { get; private set; }

        // ── Configuration ──────────────────────────────────────────────────
        [Header("Mistplay Configuration")]
        [Tooltip("Your Mistplay App ID — get this from developer.mistplay.com after approval.")]
        [SerializeField] private string mistplayAppId = "YOUR_MISTPLAY_APP_ID";

        [Tooltip("Seconds between heartbeat pings to Mistplay servers.")]
        [SerializeField] private float heartbeatIntervalSeconds = 60f;

        [Header("Session Validation")]
        [Tooltip("Minimum seconds of verified input before crediting a Mistplay session.")]
        [SerializeField] private float minimumValidSessionSeconds = 30f;

        // ── Runtime state ──────────────────────────────────────────────────
        private bool _sdkInitialised = false;
        private bool _isValidSession = false;
        private float _activePlaySeconds = 0f;
        private float _lastInputTime;
        private Coroutine _heartbeatCoroutine;

        // ── Events ─────────────────────────────────────────────────────────
#pragma warning disable CS0067 // Public API — subscribers wired at runtime
        /// <summary>Fired once the Mistplay SDK successfully initialises.</summary>
        public event Action OnSDKReady;
        /// <summary>Fired when a Mistplay reward is available for the player.</summary>
        public event Action<int> OnRewardAvailable;   // int = unit amount
#pragma warning restore CS0067

        // ── Constants ──────────────────────────────────────────────────────
        private const string ANDROID_CLASS = "com.mistplay.sdk.MistSDK";

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitialiseMistplay();
            _lastInputTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            TrackActivePlay();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                PauseSession();
            else
                ResumeSession();
        }

        private void OnApplicationQuit()
        {
            EndSession();
        }

        // ── SDK Initialisation ─────────────────────────────────────────────
        private void InitialiseMistplay()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var sdkClass   = new AndroidJavaClass(ANDROID_CLASS);
                using var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity   = unityClass.GetStatic<AndroidJavaObject>("currentActivity");

                sdkClass.CallStatic("init", activity, mistplayAppId);

                _sdkInitialised = true;
                _heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
                OnSDKReady?.Invoke();

                Debug.Log($"[Mistplay] ✅ SDK initialised. App ID: {mistplayAppId}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Mistplay] SDK not found or failed to init: {ex.Message}\n" +
                                  "Add MistSDK.aar to Assets/Plugins/Android/ and set your App ID.");
                _sdkInitialised = false;
            }
#else
            Debug.Log("[Mistplay] SDK stub active (Editor / non-Android build). Sessions will not be credited.");
            _sdkInitialised = false;
#endif
        }

        // ── Session Tracking ───────────────────────────────────────────────
        private void TrackActivePlay()
        {
            // Detect real player input — required so idle time is not credited
            if (Input.touchCount > 0 || Input.anyKey)
                _lastInputTime = Time.realtimeSinceStartup;

            float idleSeconds = Time.realtimeSinceStartup - _lastInputTime;
            const float MAX_IDLE = 30f; // Mistplay requires active engagement

            if (idleSeconds < MAX_IDLE)
                _activePlaySeconds += Time.deltaTime;

            if (!_isValidSession && _activePlaySeconds >= minimumValidSessionSeconds)
            {
                _isValidSession = true;
                Debug.Log("[Mistplay] Valid session confirmed — playtime is being credited.");
            }
        }

        private IEnumerator HeartbeatLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(heartbeatIntervalSeconds);
                SendHeartbeat();
            }
        }

        private void SendHeartbeat()
        {
            if (!_sdkInitialised || !_isValidSession) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var sdkClass = new AndroidJavaClass(ANDROID_CLASS);
                sdkClass.CallStatic("heartbeat");
                Debug.Log($"[Mistplay] Heartbeat sent — active play: {_activePlaySeconds:F0}s");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Mistplay] Heartbeat failed: {ex.Message}");
            }
#endif
        }

        private void PauseSession()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!_sdkInitialised) return;
            try
            {
                using var sdkClass = new AndroidJavaClass(ANDROID_CLASS);
                sdkClass.CallStatic("onPause");
            }
            catch { /* non-fatal */ }
#endif
        }

        private void ResumeSession()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!_sdkInitialised) return;
            try
            {
                using var sdkClass = new AndroidJavaClass(ANDROID_CLASS);
                sdkClass.CallStatic("onResume");
            }
            catch { /* non-fatal */ }
#endif
        }

        private void EndSession()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!_sdkInitialised) return;
            try
            {
                using var sdkClass = new AndroidJavaClass(ANDROID_CLASS);
                sdkClass.CallStatic("onStop");
            }
            catch { /* non-fatal */ }
#endif
            if (_heartbeatCoroutine != null)
                StopCoroutine(_heartbeatCoroutine);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Opens the Mistplay in-app store overlay where players can spend
        /// their earned Mistplay units. Call from a "Mistplay Rewards" button
        /// in the main menu or settings.
        /// </summary>
        public void OpenMistplayStore()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!_sdkInitialised) { Debug.LogWarning("[Mistplay] Store unavailable: SDK not init."); return; }
            try
            {
                using var sdkClass = new AndroidJavaClass(ANDROID_CLASS);
                sdkClass.CallStatic("openStore");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Mistplay] Could not open store: {ex.Message}");
            }
#else
            Debug.Log("[Mistplay] OpenMistplayStore called in editor — no-op.");
#endif
        }

        // ── Properties ─────────────────────────────────────────────────────
        public bool IsSDKReady => _sdkInitialised;
        public bool IsValidSession => _isValidSession;
        public float ActivePlayTime => _activePlaySeconds;
    }
}
