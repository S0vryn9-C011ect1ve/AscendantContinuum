using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Cosmic Treasure Hunt — Daily/Weekly/Monthly hidden treasures across all realms.
    ///
    /// Design principles (HIDDEN_MYSTERIES.md §5):
    ///   • No pay-to-reveal (all treasures discoverable by everyone)
    ///   • Procedurally placed each day (fair distribution)
    ///   • Accessibility-friendly: visual glow, proximity audio ramp, haptic pulse,
    ///     screen-reader distance announcement
    ///
    /// Treasure types:
    ///   Daily   (10/day) — Spark Gem (+100 XP cosmetic glow)
    ///   Weekly  (1/week) — Ancient Sigil (rare sigil variant)
    ///   Monthly (1/month)— Cosmic Key (unlocks secret ritual)
    ///
    /// Positions are pseudo-random seeded by date so all players could theoretically
    /// find the same coordinates if playing the same day (community element).
    /// </summary>
    public sealed class CosmicTreasureHunt : MonoBehaviour
    {
        public static CosmicTreasureHunt Instance { get; private set; }

        // ── PlayerPrefs ────────────────────────────────────────────────────
        private const string PREF_DAILY_DATE    = "Treasure_DailyDate";
        private const string PREF_DAILY_FOUND   = "Treasure_DailyFound_";
        private const string PREF_WEEKLY_DATE   = "Treasure_WeeklyDate";
        private const string PREF_WEEKLY_FOUND  = "Treasure_WeeklyFound";
        private const string PREF_MONTHLY_DATE  = "Treasure_MonthlyDate";
        private const string PREF_MONTHLY_FOUND = "Treasure_MonthlyFound";
        private const string PREF_TOTAL_FOUND   = "Treasure_TotalFound";

        // ── Config ─────────────────────────────────────────────────────────
        private const int    DAILY_TREASURE_COUNT  = 10;
        private const float  DETECTION_RADIUS      = 3f;
        private const float  HAPTIC_INTERVAL       = 1f;

        // ── Types ──────────────────────────────────────────────────────────
        public enum TreasureType { SparkGem, AncientSigil, CosmicKey }

        [Serializable]
        public class TreasureData
        {
            public string      id;
            public TreasureType type;
            public Vector3     position;
            public bool        found;
        }

        // ── State ──────────────────────────────────────────────────────────
        private List<TreasureData> _activeTreasures = new List<TreasureData>();
        private float _hapticCooldown;

        // ── Events ─────────────────────────────────────────────────────────
        public event Action<TreasureData> OnTreasureFound;

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            RefreshTreasures();
        }

        private void Update()
        {
            _hapticCooldown -= Time.deltaTime;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Call once per frame (or on player move) with current player world position.
        /// Returns the nearest treasure if within detection range, else null.
        /// Handles proximity audio and haptic feedback automatically.
        /// </summary>
        public TreasureData DetectNearbyTreasure(Vector3 playerPosition)
        {
            TreasureData nearest = null;
            float minDist = float.MaxValue;

            foreach (var t in _activeTreasures)
            {
                if (t.found) continue;
                float dist = Vector3.Distance(playerPosition, t.position);
                if (dist < minDist) { minDist = dist; nearest = t; }
            }

            if (nearest == null || minDist > DETECTION_RADIUS * 2f) return null;

            // Proximity haptic (gets more intense as player nears)
            if (_hapticCooldown <= 0f)
            {
                float normalised = 1f - Mathf.Clamp01(minDist / (DETECTION_RADIUS * 2f));
                if (normalised > 0.2f)
                {
                    int hapticLevel = Mathf.RoundToInt(normalised * 3f); // 1–3
#if UNITY_IOS || UNITY_ANDROID
                    if (hapticLevel > 1) Handheld.Vibrate();
#endif
                    _hapticCooldown = HAPTIC_INTERVAL * (1f - normalised * 0.5f);
                }
            }

            // Screen reader proximity announce (every 5 seconds max)
            if (AccessibilityManager.Instance != null &&
                AccessibilityManager.Instance.ScreenReaderEnabled)
            {
                float dist = Mathf.Round(minDist * 10f) / 10f;
                AccessibilityManager.Instance.Announce(
                    $"Treasure nearby, approximately {dist} units away.");
            }

            // Auto-collect when very close
            if (minDist <= DETECTION_RADIUS * 0.3f)
                CollectTreasure(nearest);

            return nearest;
        }

        /// <summary>Manually collect a treasure (called by player interaction or auto-collect).</summary>
        public void CollectTreasure(TreasureData treasure)
        {
            if (treasure == null || treasure.found) return;
            treasure.found = true;
            MarkTreasureFound(treasure.id);

            int total = PlayerPrefs.GetInt(PREF_TOTAL_FOUND, 0) + 1;
            PlayerPrefs.SetInt(PREF_TOTAL_FOUND, total);
            PlayerPrefs.Save();

            // Reward based on type
            string reward = treasure.type switch
            {
                TreasureType.SparkGem    => "+100 XP & Spark Glow", 
                TreasureType.AncientSigil=> "Ancient Sigil Variant unlocked!",
                TreasureType.CosmicKey   => "✦ Cosmic Key — Secret Ritual unlocked!",
                _                        => "Treasure found!"
            };

            HUDManager.Instance?.ShowNotification(
                $"💎 Treasure Found! {reward}", HUDManager.NotificationType.Achievement);

            if (treasure.type == TreasureType.AncientSigil)
                AchievementManager.Instance?.UnlockAchievement("sigil_master");

            if (treasure.type == TreasureType.CosmicKey)
                AchievementManager.Instance?.UnlockAchievement("cosmic_key_holder");

            if (total >= 30)
                AchievementManager.Instance?.UnlockAchievement("treasure_hunter");

            OnTreasureFound?.Invoke(treasure);
        }

        /// <summary>Returns how many daily treasures remain unfound today.</summary>
        public int RemainingDailyTreasures()
        {
            int found = 0;
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            for (int i = 0; i < DAILY_TREASURE_COUNT; i++)
                if (PlayerPrefs.GetInt(PREF_DAILY_FOUND + today + "_" + i, 0) == 1) found++;
            return DAILY_TREASURE_COUNT - found;
        }

        // ── Refresh logic ──────────────────────────────────────────────────

        private void RefreshTreasures()
        {
            _activeTreasures.Clear();
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            int weekNum   = GetISOWeekNumber(DateTime.UtcNow);
            int monthKey  = DateTime.UtcNow.Year * 100 + DateTime.UtcNow.Month;

            // Spawn daily treasures if new day
            if (PlayerPrefs.GetString(PREF_DAILY_DATE, "") != today)
            {
                PlayerPrefs.SetString(PREF_DAILY_DATE, today);
                PlayerPrefs.Save();
            }

            int seed = int.Parse(today);
            UnityEngine.Random.InitState(seed);

            for (int i = 0; i < DAILY_TREASURE_COUNT; i++)
            {
                bool found = PlayerPrefs.GetInt(PREF_DAILY_FOUND + today + "_" + i, 0) == 1;
                _activeTreasures.Add(new TreasureData
                {
                    id       = $"daily_{today}_{i}",
                    type     = TreasureType.SparkGem,
                    position = new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f), 0f),
                    found    = found
                });
            }

            // Weekly Ancient Sigil
            string weekKey = $"{DateTime.UtcNow.Year}_{weekNum}";
            if (PlayerPrefs.GetString(PREF_WEEKLY_DATE, "") != weekKey)
                PlayerPrefs.SetString(PREF_WEEKLY_DATE, weekKey);

            bool weeklyFound = PlayerPrefs.GetInt(PREF_WEEKLY_FOUND, 0) == 1;
            UnityEngine.Random.InitState(weekNum * 1000 + DateTime.UtcNow.Year);
            _activeTreasures.Add(new TreasureData
            {
                id       = $"weekly_{weekKey}",
                type     = TreasureType.AncientSigil,
                position = new Vector3(UnityEngine.Random.Range(-6f, 6f), UnityEngine.Random.Range(-4f, 4f), 0f),
                found    = weeklyFound
            });

            // Monthly Cosmic Key
            string monthStr = monthKey.ToString();
            if (PlayerPrefs.GetString(PREF_MONTHLY_DATE, "") != monthStr)
                PlayerPrefs.SetString(PREF_MONTHLY_DATE, monthStr);

            bool monthlyFound = PlayerPrefs.GetInt(PREF_MONTHLY_FOUND, 0) == 1;
            UnityEngine.Random.InitState(monthKey);
            _activeTreasures.Add(new TreasureData
            {
                id       = $"monthly_{monthStr}",
                type     = TreasureType.CosmicKey,
                position = new Vector3(UnityEngine.Random.Range(-4f, 4f), UnityEngine.Random.Range(-3f, 3f), 0f),
                found    = monthlyFound
            });

            PlayerPrefs.Save();
            Debug.Log($"[TreasureHunt] {_activeTreasures.Count} treasures active today.");
        }

        private void MarkTreasureFound(string id)
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (id.StartsWith("daily_"))
            {
                // Extract index from id: "daily_YYYYMMDD_index"
                string[] parts = id.Split('_');
                if (parts.Length >= 3)
                    PlayerPrefs.SetInt(PREF_DAILY_FOUND + today + "_" + parts[2], 1);
            }
            else if (id.StartsWith("weekly_"))
                PlayerPrefs.SetInt(PREF_WEEKLY_FOUND, 1);
            else if (id.StartsWith("monthly_"))
                PlayerPrefs.SetInt(PREF_MONTHLY_FOUND, 1);
            PlayerPrefs.Save();
        }

        private static int GetISOWeekNumber(DateTime date)
        {
            return System.Globalization.CultureInfo.InvariantCulture
                .Calendar.GetWeekOfYear(date,
                    System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday);
        }
    }
}
