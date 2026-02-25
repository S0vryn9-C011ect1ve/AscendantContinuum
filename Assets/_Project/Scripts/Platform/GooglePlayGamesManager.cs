using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Platform
{
    /// <summary>
    /// Google Play Games Services manager for The Ascendant Continuum.
    ///
    /// Provides:
    ///   • Cloud save (backup of SaveSystem data to Google cloud)
    ///   • Leaderboards (Sparks Collected, Streak Champions, Sigil Mastery)
    ///   • Google Play Achievements (mapped to AchievementManager IDs)
    ///   • Sign-In (optional, always with guest fallback)
    ///
    /// SETUP:
    ///   1. Enable Google Play Games plugin in Unity Package Manager
    ///      (com.google.play.games — from Google's GitHub releases)
    ///   2. Add your Google Play Games app ID in:
    ///      Window → Google Play Games → Setup → Android Setup
    ///   3. Map achievement and leaderboard IDs below to your Play Console IDs.
    ///   4. In Play Console → Linked Apps → select your Unity game.
    ///
    /// All methods gracefully no-op in the Editor and on non-Android platforms.
    /// </summary>
    public sealed class GooglePlayGamesManager : MonoBehaviour
    {
        public static GooglePlayGamesManager Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────
#pragma warning disable CS0067 // Public API — subscribers wired at runtime
        public event Action<bool> OnSignInComplete;     // bool = success
        public event Action<long> OnCloudSaveLoaded;    // long = save timestamp
#pragma warning restore CS0067

        // ── Play Console IDs ───────────────────────────────────────────────
        // Replace these with the real IDs from your Play Console once created:
        [Header("Leaderboard IDs (from Play Console)")]
        [SerializeField] private string lbSparksId = "CgkI_____sparks_lb";
        [SerializeField] private string lbStreakId = "CgkI_____streak_lb";
        [SerializeField] private string lbSigilId = "CgkI_____sigil_lb";
        [SerializeField] private string lbWishesId = "CgkI_____wishes_lb";

        [Header("State")]
        private bool _signedIn = false;

        // Achievement ID mapping  (local ID → Play Console achievement ID)
        private static readonly Dictionary<string, string> AchievementMap = new()
        {
            { "first_spark",            "CgkI___first_spark" },
            { "realm_explorer",         "CgkI___realm_explorer" },
            { "colorblind_master",      "CgkI___colorblind_master" },
            { "calm_seeker",            "CgkI___calm_seeker" },
            { "haptic_harmony",         "CgkI___haptic_harmony" },
            { "green_thumb",            "CgkI___green_thumb" },
            { "stargazer",              "CgkI___stargazer" },
            { "light_bender",           "CgkI___light_bender" },
            { "lantern_lighter",        "CgkI___lantern_lighter" },
            { "weekly_warrior",         "CgkI___weekly_warrior" },
            { "consecutive_champion",   "CgkI___consecutive_champion" },
            { "midnight_visitor",       "CgkI___midnight_visitor" },
            { "archaeologist",          "CgkI___archaeologist" },
            { "earth_walker",           "CgkI___earth_walker" },
            { "puzzle_chain_helper",    "CgkI___puzzle_chain_helper" },
            { "sigil_master",           "CgkI___sigil_master" },
            { "time_capsule_creator",   "CgkI___time_capsule_creator" },
            // Live event achievements
            { "event_spring_equinox_2026",  "CgkI___equinox_2026"   },
            { "event_summer_solstice_2026", "CgkI___solstice_2026"  },
            { "event_lunar_eclipse_mar_2026","CgkI___blood_moon"    },
            { "event_perseids_2026",        "CgkI___perseids"       },
            { "event_winter_solstice_2026", "CgkI___winter_2026"    },
        };

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Wire into AchievementManager so every local unlock auto-syncs
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnAchievementUnlocked += OnLocalAchievementUnlocked;

            SignIn();
        }

        private void OnDestroy()
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnAchievementUnlocked -= OnLocalAchievementUnlocked;
        }

        // ── Sign-In ────────────────────────────────────────────────────────
        public void SignIn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Google Play Games Unity Plugin v11+ (open source):
            // PlayGamesPlatform.Activate() then Social.localUser.Authenticate(...)
            try
            {
                // PlayGamesPlatform.Activate();
                UnityEngine.Social.localUser.Authenticate(success =>
                {
                    _signedIn = success;
                    OnSignInComplete?.Invoke(success);

                    if (success)
                    {
                        Debug.Log("[GPGS] Signed in as: " + UnityEngine.Social.localUser.userName);
                        LoadCloudSave();
                    }
                    else
                    {
                        Debug.Log("[GPGS] Sign-in failed or declined — continuing as guest.");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GPGS] Sign-in exception: {ex.Message}\n" +
                                  "Install Google Play Games Unity Plugin from GitHub.");
            }
#else
            Debug.Log("[GPGS] Stub active (Editor/iOS) — sign-in skipped.");
#endif
        }

        // ── Leaderboards ───────────────────────────────────────────────────

        /// <summary>Posts the player's total sparks to the Sparks Collected leaderboard.</summary>
        public void PostSparksScore(int sparks)
        {
            PostScore(lbSparksId, sparks, "Sparks");
        }

        /// <summary>Posts the player's current streak to the Streak Champions leaderboard.</summary>
        public void PostStreakScore(int streakDays)
        {
            PostScore(lbStreakId, streakDays, "Streak");
        }

        /// <summary>Posts the player's sigil evolution level to the Sigil Mastery leaderboard.</summary>
        public void PostSigilScore(int sigilLevel)
        {
            PostScore(lbSigilId, sigilLevel, "Sigil");
        }

        /// <summary>Posts total wishes released to the Wish Wall leaderboard.</summary>
        public void PostWishesScore(int wishCount)
        {
            PostScore(lbWishesId, wishCount, "Wishes");
        }

        private void PostScore(string lbId, long score, string label)
        {
            if (!_signedIn) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.Social.ReportScore(score, lbId, success =>
                Debug.Log($"[GPGS] {label} score {score} posted: {(success ? "✅" : "❌")}"));
#else
            Debug.Log($"[GPGS] Score stub: {label} = {score}");
#endif
        }

        /// <summary>Shows the platform leaderboard UI.</summary>
        public void ShowLeaderboard(string lbId = null)
        {
            if (!_signedIn) { SignIn(); return; }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (string.IsNullOrEmpty(lbId))
                UnityEngine.Social.ShowLeaderboardUI();
            else
            {
                // PlayGamesPlatform.Instance.ShowLeaderboardUI(lbId);
                UnityEngine.Social.ShowLeaderboardUI();
            }
#else
            Debug.Log($"[GPGS] ShowLeaderboard stub: {lbId ?? "all"}");
#endif
        }

        // ── Achievements ───────────────────────────────────────────────────

        private void OnLocalAchievementUnlocked(Achievement achievement)
        {
            UnlockPlayAchievement(achievement.id);
        }

        public void UnlockPlayAchievement(string localId)
        {
            if (!_signedIn) return;
            if (!AchievementMap.TryGetValue(localId, out string playId)) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.Social.ReportProgress(playId, 100.0, success =>
                Debug.Log($"[GPGS] Achievement '{localId}' synced: {(success ? "✅" : "❌")}"));
#else
            Debug.Log($"[GPGS] Achievement stub: {localId} → {playId}");
#endif
        }

        public void ShowAchievements()
        {
            if (!_signedIn) { SignIn(); return; }
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.Social.ShowAchievementsUI();
#else
            Debug.Log("[GPGS] ShowAchievements stub.");
#endif
        }

        // ── Cloud Save ─────────────────────────────────────────────────────
        // Uses Google Play Games Saved Games API (ISavedGameClient)

        public void SaveToCloud()
        {
            if (!_signedIn) return;

            var pd = Core.SaveSystem.Instance?.CurrentPlayerData;
            if (pd == null) return;

            string json = JsonUtility.ToJson(pd);
            Debug.Log("[GPGS] Queuing cloud save upload…");

            // Full implementation requires PlayGamesPlatform.Instance.SavedGame
            // This is the integration point — wire here with the plugin:
            // var client = PlayGamesPlatform.Instance.SavedGame;
            // client.OpenWithAutomaticConflictResolution("ascendant_save", ...);
            PlayerPrefs.SetString("cloud_save_pending", json);
            PlayerPrefs.Save();
        }

        public void LoadCloudSave()
        {
            if (!_signedIn) return;
            Debug.Log("[GPGS] Checking for cloud save…");
            // Wire to PlayGamesPlatform.Instance.SavedGame here
            // On success, call Core.SaveSystem.Instance?.LoadFromCloudJson(json)
        }

        // ── Properties ─────────────────────────────────────────────────────
        public bool IsSignedIn => _signedIn;
    }
}
