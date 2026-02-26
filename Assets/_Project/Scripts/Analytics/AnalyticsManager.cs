using System;
using System.Collections.Generic;
using UnityEngine;

namespace AscendantContinuum.Analytics
{
    /// <summary>
    /// Analytics system tracking user behavior, realm preferences, ritual completion
    /// Integrates with Firebase Analytics and Unity Analytics
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [SerializeField] private bool enableFirebaseAnalytics = true;
        [SerializeField] private bool enableUnityAnalytics = true;

        private DateTime sessionStartTime;
        private Dictionary<string, int> realmPlayCounts = new Dictionary<string, int>();
        private int ritualCompletionCount = 0;
        private float sessionPlayTime = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            sessionStartTime = DateTime.Now;

            Debug.Log("[AnalyticsManager] Initialized");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                LogSessionEnd();
            }
        }

        /// <summary>
        /// Log session start (called on app launch)
        /// </summary>
        public void LogSessionStart()
        {
            sessionStartTime = DateTime.Now;
            sessionPlayTime = 0f;

            var parameters = new Dictionary<string, object>
            {
                { "session_start_time", DateTime.Now.ToString("O") },
                { "app_version", Application.version },
                { "platform", Application.platform.ToString() }
            };

            LogEvent("session_start", parameters);
            Debug.Log("[AnalyticsManager] Session started");
        }

        /// <summary>
        /// Log session end (called on app quit)
        /// </summary>
        public void LogSessionEnd()
        {
            sessionPlayTime = (float)(DateTime.Now - sessionStartTime).TotalSeconds;

            var parameters = new Dictionary<string, object>
            {
                { "session_duration_seconds", sessionPlayTime },
                { "realm_plays_count", realmPlayCounts.Count },
                { "ritual_completions", ritualCompletionCount }
            };

            LogEvent("session_end", parameters);
            Debug.Log($"[AnalyticsManager] Session ended. Duration: {sessionPlayTime:F1}s");
        }

        /// <summary>
        /// Log realm entry
        /// </summary>
        public void LogRealmEntered(string realmId, string realmName)
        {
            if (!realmPlayCounts.ContainsKey(realmId))
                realmPlayCounts[realmId] = 0;
            realmPlayCounts[realmId]++;

            var parameters = new Dictionary<string, object>
            {
                { "realm_id", realmId },
                { "realm_name", realmName },
                { "entrance_time", DateTime.Now.ToString("O") }
            };

            LogEvent("realm_entered", parameters);
            Debug.Log($"[AnalyticsManager] Realm entered: {realmName} (Play count: {realmPlayCounts[realmId]})");
        }

        /// <summary>
        /// Log realm exit
        /// </summary>
        public void LogRealmExited(string realmId, float timeSpent, bool completed)
        {
            var parameters = new Dictionary<string, object>
            {
                { "realm_id", realmId },
                { "time_spent_seconds", timeSpent },
                { "completed", completed }
            };

            LogEvent("realm_exited", parameters);
        }

        /// <summary>
        /// Log ritual completion
        /// </summary>
        public void LogRitualCompleted(string ritualId, string realmId, int score, float timeSpent)
        {
            ritualCompletionCount++;

            var parameters = new Dictionary<string, object>
            {
                { "ritual_id", ritualId },
                { "realm_id", realmId },
                { "score", score },
                { "time_spent_seconds", timeSpent },
                { "total_ritual_completions", ritualCompletionCount }
            };

            LogEvent("ritual_completed", parameters);
            Debug.Log($"[AnalyticsManager] Ritual completed: {ritualId} - Score: {score}");
        }

        /// <summary>
        /// Log achievement unlock
        /// </summary>
        public void LogAchievementUnlocked(string achievementId, string achievementName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "achievement_id", achievementId },
                { "achievement_name", achievementName },
                { "unlock_time", DateTime.Now.ToString("O") }
            };

            LogEvent("achievement_unlocked", parameters);
            Debug.Log($"[AnalyticsManager] Achievement unlocked: {achievementName}");
        }

        /// <summary>
        /// Log sigil creation
        /// </summary>
        public void LogSigilCreated(string sigilId, string playstyleCategory, int complexity)
        {
            var parameters = new Dictionary<string, object>
            {
                { "sigil_id", sigilId },
                { "playstyle_category", playstyleCategory },
                { "complexity_level", complexity }
            };

            LogEvent("sigil_created", parameters);
        }

        /// <summary>
        /// Log accessibility feature usage
        /// </summary>
        public void LogAccessibilityFeatureUsed(string feature, string setting)
        {
            var parameters = new Dictionary<string, object>
            {
                { "feature", feature },
                { "setting", setting }
            };

            LogEvent("accessibility_feature_used", parameters);
            Debug.Log($"[AnalyticsManager] Accessibility: {feature} = {setting}");
        }

        /// <summary>
        /// Log prestige cycle completion
        /// </summary>
        public void LogPrestigeCycleCompleted(string realmId, int prestigePoints)
        {
            var parameters = new Dictionary<string, object>
            {
                { "realm_id", realmId },
                { "prestige_points", prestigePoints }
            };

            LogEvent("prestige_cycle_completed", parameters);
            Debug.Log($"[AnalyticsManager] Prestige cycle: {realmId} +{prestigePoints} points");
        }

        /// <summary>
        /// Log custom event
        /// </summary>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            // Firebase Analytics
            if (enableFirebaseAnalytics)
            {
                try
                {
                    if (AscendantContinuum.Core.FirebaseManager.Instance != null)
                    {
                        _ = AscendantContinuum.Core.FirebaseManager.Instance.TrackEvent(eventName, ConvertParameters(parameters));
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AnalyticsManager] Firebase error: {ex.Message}");
                }
            }

            // Unity Analytics
            if (enableUnityAnalytics)
            {
                try
                {
                    // Unity.Analytics.Analytics.RecordEvent(eventName, parameters);
                    // TODO: Uncomment when Unity Analytics is configured
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AnalyticsManager] Unity Analytics error: {ex.Message}");
                }
            }

            Debug.Log($"[AnalyticsManager] Event: {eventName} - {ParametersToString(parameters)}");
        }

        /// <summary>
        /// Get realm play statistics
        /// </summary>
        public Dictionary<string, int> GetRealmPlayCounts() => realmPlayCounts;

        /// <summary>
        /// Get most played realm
        /// </summary>
        public string GetMostPlayedRealm()
        {
            string mostPlayed = "";
            int maxCount = 0;

            foreach (var kvp in realmPlayCounts)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    mostPlayed = kvp.Key;
                }
            }

            return mostPlayed;
        }

        /// <summary>
        /// Get session duration
        /// </summary>
        public float GetSessionDuration() => (float)(DateTime.Now - sessionStartTime).TotalSeconds;

        /// <summary>
        /// Helper: Convert parameters to Firebase format
        /// </summary>
        private Dictionary<string, object> ConvertParameters(Dictionary<string, object> parameters)
        {
            // Firebase has specific parameter type requirements
            var converted = new Dictionary<string, object>();
            foreach (var kvp in parameters)
            {
                // Convert to appropriate types for Firebase
                if (kvp.Value is float f)
                    converted[kvp.Key] = (double)f;
                else
                    converted[kvp.Key] = kvp.Value;
            }
            return converted;
        }

        /// <summary>
        /// Helper: Convert parameters to string for logging
        /// </summary>
        private string ParametersToString(Dictionary<string, object> parameters)
        {
            var parts = new List<string>();
            foreach (var kvp in parameters)
            {
                parts.Add($"{kvp.Key}={kvp.Value}");
            }
            return string.Join(", ", parts);
        }
    }
}
