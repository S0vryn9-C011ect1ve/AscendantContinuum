using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.Social
{
    /// <summary>
    /// Time Capsule System — Async social feature allowing players to bury messages,
    /// constellation patterns and emotions for future seekers to discover.
    ///
    /// Three capsule types:
    ///   • Personal   — opens in 30-365 days for a random future player
    ///   • Year        — opens exactly 1 year from today
    ///   • Birthday    — opens on creator's next birthday
    ///
    /// Content is moderated client-side (length/basic-filter) before Firebase upload.
    /// Discovery is simulated locally when no cross-player backend is available.
    /// </summary>
    public sealed class TimeCapsuleManager : MonoBehaviour
    {
        public static TimeCapsuleManager Instance { get; private set; }

        // ── PlayerPrefs keys ──────────────────────────────────────────────
        private const string PREF_BURIED_COUNT     = "TC_BuriedCount";
        private const string PREF_BURIED_PREFIX    = "TC_Buried_";
        private const string PREF_FOUND_COUNT      = "TC_FoundCount";
        private const string PREF_NEXT_SPAWN_EPOCH = "TC_NextSpawnEpoch";

        // ── Types ─────────────────────────────────────────────────────────
        public enum CapsuleType { Personal, Year, Birthday }

        [Serializable]
        public class TimeCapsule
        {
            public string id;
            public string creatorTag;
            public string message;
            public string emotion;
            public string realmId;
            public CapsuleType type;
            public long   buriedEpochDay;
            public long   openEpochDay;
            public bool   discovered;
        }

        // ── Epoch helper ──────────────────────────────────────────────────
        private static readonly DateTime EPOCH = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private long TodayEpochDay => (long)(DateTime.UtcNow - EPOCH).TotalDays;

        // ── Events ────────────────────────────────────────────────────────
        public event Action<TimeCapsule> OnCapsuleDiscovered;

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            CheckForDiscoverableCapsules();
        }

        // ── Burial ────────────────────────────────────────────────────────

        /// <summary>
        /// Bury a time capsule. openInDays = 0 means auto (30-365 random personal).
        /// Returns false if content is rejected by the basic safety filter.
        /// </summary>
        public bool BuryCapsule(
            string message,
            string emotion,
            string realmId,
            CapsuleType type      = CapsuleType.Personal,
            int    openInDays     = 0,
            int    birthdayMonth  = 0,
            int    birthdayDay    = 0)
        {
            if (!PassesBasicFilter(message))
            {
                HUDManager.Instance?.ShowNotification(
                    "Your message could not be buried — please adjust the content.",
                    HUDManager.NotificationType.Info);
                return false;
            }

            long today = TodayEpochDay;
            long openDay;

            switch (type)
            {
                case CapsuleType.Year:
                    openDay = today + 365;
                    break;
                case CapsuleType.Birthday:
                    openDay = GetNextBirthdayEpochDay(birthdayMonth, birthdayDay, today);
                    break;
                default:
                    openDay = today + (openInDays > 0 ? openInDays : UnityEngine.Random.Range(30, 366));
                    break;
            }

            int seekerId = PlayerPrefs.GetInt("Profile_SeekerId", UnityEngine.Random.Range(10000, 99999));

            TimeCapsule capsule = new TimeCapsule
            {
                id             = Guid.NewGuid().ToString("N")[..12],
                creatorTag     = $"Seeker #{seekerId}",
                message        = message,
                emotion        = emotion,
                realmId        = realmId,
                type           = type,
                buriedEpochDay = today,
                openEpochDay   = openDay,
                discovered     = false
            };

            SaveCapsule(capsule);

            int count = PlayerPrefs.GetInt(PREF_BURIED_COUNT, 0) + 1;
            PlayerPrefs.SetInt(PREF_BURIED_COUNT, count);
            PlayerPrefs.Save();

            if (count >= 10) AchievementManager.Instance?.UnlockAchievement("time_capsule_creator");

            HUDManager.Instance?.ShowNotification(
                $"✦ Time capsule buried. A future seeker will find it in ~{openDay - today} days.",
                HUDManager.NotificationType.Info);

            // Firebase upload
            FirebaseManager.Instance?.SaveData("timeCapsules", capsule.id, new
            {
                creator  = capsule.creatorTag,
                message  = capsule.message,
                emotion  = capsule.emotion,
                realm    = realmId,
                openDay  = openDay,
                buried   = today
            });

            return true;
        }

        // ── Discovery ─────────────────────────────────────────────────────

        /// <summary>
        /// Scans buried capsules and surfaces any that are past their open date.
        /// In offline mode this simulates "discovering" other players' capsules by
        /// treating old capsules as if they were left by strangers.
        /// </summary>
        public void CheckForDiscoverableCapsules()
        {
            long today = TodayEpochDay;
            int count  = PlayerPrefs.GetInt(PREF_BURIED_COUNT, 0);

            for (int i = 0; i < count; i++)
            {
                string json = PlayerPrefs.GetString(PREF_BURIED_PREFIX + i, "");
                if (string.IsNullOrEmpty(json)) continue;

                TimeCapsule capsule = JsonUtility.FromJson<TimeCapsule>(json);
                if (capsule == null || capsule.discovered) continue;
                if (today < capsule.openEpochDay) continue;

                // Mark discovered
                capsule.discovered = true;
                PlayerPrefs.SetString(PREF_BURIED_PREFIX + i, JsonUtility.ToJson(capsule));
                PlayerPrefs.Save();

                StartCoroutine(PlayDiscoverySequence(capsule));
            }
        }

        // ── Coroutine ─────────────────────────────────────────────────────

        private IEnumerator PlayDiscoverySequence(TimeCapsule capsule)
        {
            yield return new WaitForSeconds(0.5f);

            long age = TodayEpochDay - capsule.buriedEpochDay;
            string ageText = age < 30 ? $"{age} days" : age < 365 ? $"{age / 30} months" : $"{age / 365} years";

            HUDManager.Instance?.ShowNotification(
                $"⏰ Time Capsule from {capsule.creatorTag} ({ageText} ago):\n\"{capsule.message}\"",
                HUDManager.NotificationType.Achievement);

            int found = PlayerPrefs.GetInt(PREF_FOUND_COUNT, 0) + 1;
            PlayerPrefs.SetInt(PREF_FOUND_COUNT, found);

            AchievementManager.Instance?.TrackProgress("time_capsule_creator",
                PlayerPrefs.GetInt(PREF_FOUND_COUNT, 0));
            if (capsule.type == CapsuleType.Year)
                AchievementManager.Instance?.UnlockAchievement("time_traveler");

            OnCapsuleDiscovered?.Invoke(capsule);
        }

        // ── Private helpers ───────────────────────────────────────────────

        private void SaveCapsule(TimeCapsule capsule)
        {
            int index = PlayerPrefs.GetInt(PREF_BURIED_COUNT, 0);
            PlayerPrefs.SetString(PREF_BURIED_PREFIX + index, JsonUtility.ToJson(capsule));
        }

        private bool PassesBasicFilter(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (text.Length > 280) return false;  // Twitter-length cap
            // Minimal profanity/safety check — real implementation uses Cloud Function
            string lower = text.ToLower();
            string[] blocked = { "spam", "http://", "https://", "discord.gg" };
            foreach (string b in blocked)
                if (lower.Contains(b)) return false;
            return true;
        }

        private long GetNextBirthdayEpochDay(int month, int day, long todayEpochDay)
        {
            if (month < 1 || month > 12 || day < 1 || day > 31) return todayEpochDay + 365;
            DateTime today  = EPOCH.AddDays(todayEpochDay);
            DateTime bday   = new DateTime(today.Year, month, day);
            if (bday <= today) bday = bday.AddYears(1);
            return (long)(bday - EPOCH).TotalDays;
        }
    }
}
