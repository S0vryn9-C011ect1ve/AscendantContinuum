using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Living Lore System — The world responds to collective player behaviour.
    ///
    /// Two axes drive the living world:
    ///   1. Deity Alignment Balance — if one deity exceeds 40% of players it
    ///      becomes "Ascendant" and its realm reacts visually
    ///   2. Emotion Economy — the dominant community emotion colours NPCs,
    ///      ambient sfx, and event frequency
    ///
    /// This manager is the single source of truth. Other managers call:
    ///   RecordDeityAlignment(deity)  — from OnboardingController / CosmicIdentitySystem
    ///   RecordCommunityEmotion(e)    — from NPCCollectiveMemory events
    ///
    /// The lore state is published via <see cref="OnLoreStateChanged"/> so scene
    /// managers can drive ambient visual changes without polling.
    /// </summary>
    public sealed class LivingLoreManager : MonoBehaviour
    {
        public static LivingLoreManager Instance { get; private set; }

        // ── PlayerPrefs keys ─────────────────────────────────────────────
        private const string PREF = "LivingLore_";

        // ── Deity identifiers (must match CosmicIdentitySystem ALIGNMENTS) ──
        public static readonly string[] DEITIES =
        {
            "Ascendant Flame",
            "Herald of Joyful Curiosity",
            "Archivist of Bright Memories",
            "Mechanic of Helpful Wonders",
            "Scribe of Magical Knowledge",
            "Silent Nurturer"
        };

        // ── Events ────────────────────────────────────────────────────────
        public event Action<string> OnAscendantDeityChanged;   // deity name or "Balanced"
        public event Action<string> OnEmotionEconomyChanged;   // emotion name

        // ── State properties ──────────────────────────────────────────────

        /// <summary>Returns the currently ascendant deity, or "Balanced" if no deity exceeds 40%.</summary>
        public string AscendantDeity
        {
            get
            {
                string stored = PlayerPrefs.GetString($"{PREF}AscendantDeity", "");
                return string.IsNullOrEmpty(stored) ? "Balanced" : stored;
            }
        }

        /// <summary>Returns the dominant community emotion (e.g. "Joy", "Peace").</summary>
        public string DominantEmotion =>
            PlayerPrefs.GetString($"{PREF}DominantEmotion", "Curiosity");

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Subscribe to NPCCollectiveMemory mood shifts
            if (NPCCollectiveMemory.Instance != null)
                NPCCollectiveMemory.Instance.OnCommunityMoodShift += HandleMoodShift;
        }

        private void OnDestroy()
        {
            if (NPCCollectiveMemory.Instance != null)
                NPCCollectiveMemory.Instance.OnCommunityMoodShift -= HandleMoodShift;
        }

        // ── Recording ─────────────────────────────────────────────────────

        /// <summary>
        /// Called when a player selects or changes their Pantheon deity alignment.
        /// Increments the deity's vote count and checks for Ascendant threshold.
        /// </summary>
        public void RecordDeityAlignment(string deity)
        {
            string key = $"{PREF}Deity_{deity.Replace(" ", "_")}";
            int count = PlayerPrefs.GetInt(key, 0) + 1;
            PlayerPrefs.SetInt(key, count);
            PlayerPrefs.Save();

            CheckAscendantThreshold();
        }

        /// <summary>
        /// Propagates a community emotion change into the lore state and fires events.
        /// Called automatically from NPCCollectiveMemory via subscription.
        /// </summary>
        public void RecordCommunityEmotion(string emotion)
        {
            string prev = DominantEmotion;
            PlayerPrefs.SetString($"{PREF}DominantEmotion", emotion);
            PlayerPrefs.Save();

            if (emotion != prev)
            {
                OnEmotionEconomyChanged?.Invoke(emotion);
                ApplyEmotionEffects(emotion);
            }
        }

        // ── Lore state queries ────────────────────────────────────────────

        public int GetDeityVoteCount(string deity) =>
            PlayerPrefs.GetInt($"{PREF}Deity_{deity.Replace(" ", "_")}", 0);

        public bool IsAgeOfHarmony()
        {
            int total = 0;
            foreach (string d in DEITIES) total += GetDeityVoteCount(d);
            if (total == 0) return false;
            foreach (string d in DEITIES)
            {
                float pct = GetDeityVoteCount(d) / (float)total;
                if (pct > 0.22f) return false;   // any deity > 22% breaks harmony
            }
            return true;
        }

        /// <summary>
        /// Returns a lore descriptor string for the current world state, used by NPCs and HUD.
        /// </summary>
        public string GetCurrentLoreState()
        {
            if (IsAgeOfHarmony()) return "Age of Harmony";
            string ascendant = AscendantDeity;
            if (ascendant != "Balanced") return $"Age of {ascendant}";
            return $"Age of {DominantEmotion}";
        }

        // ── Private ───────────────────────────────────────────────────────

        private void CheckAscendantThreshold()
        {
            int total = 0;
            foreach (string d in DEITIES) total += GetDeityVoteCount(d);
            if (total == 0) return;

            string newAscendant = "Balanced";
            foreach (string d in DEITIES)
            {
                float pct = GetDeityVoteCount(d) / (float)total;
                if (pct >= 0.40f) { newAscendant = d; break; }
            }

            string old = PlayerPrefs.GetString($"{PREF}AscendantDeity", "Balanced");
            if (newAscendant == old) return;

            PlayerPrefs.SetString($"{PREF}AscendantDeity", newAscendant);
            PlayerPrefs.Save();

            OnAscendantDeityChanged?.Invoke(newAscendant);

            if (newAscendant == "Balanced")
            {
                // Age of Harmony event
                AchievementManager.Instance?.UnlockAchievement("age_of_harmony");
                HUDManager.Instance?.ShowNotification(
                    "✦ AGE OF HARMONY — All six deities are perfectly aligned!",
                    HUDManager.NotificationType.Achievement);
            }
            else
            {
                HUDManager.Instance?.ShowNotification(
                    $"✦ {newAscendant} becomes THE ASCENDANT — the world shifts...",
                    HUDManager.NotificationType.Achievement);
            }

            Debug.Log($"[LivingLore] Deity shift: {old} → {newAscendant}");
        }

        private void HandleMoodShift(NPCCollectiveMemory.Emotion emotion)
        {
            RecordCommunityEmotion(emotion.ToString());
        }

        private void ApplyEmotionEffects(string emotion)
        {
            // Adjust global colour tint / ambient volume via GameManager / the existing SacredTimingManager
            switch (emotion)
            {
                case "Joy":
                    PlayerPrefs.SetFloat("Lore_AmbientBrightness", 1.2f);
                    break;
                case "Peace":
                    PlayerPrefs.SetFloat("Lore_AmbientBrightness", 0.9f);
                    break;
                case "Sadness":
                    PlayerPrefs.SetFloat("Lore_AmbientBrightness", 0.7f);
                    break;
                default:
                    PlayerPrefs.SetFloat("Lore_AmbientBrightness", 1.0f);
                    break;
            }
            PlayerPrefs.Save();
        }
    }
}
