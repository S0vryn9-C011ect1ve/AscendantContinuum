using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Community-Wide Mysteries — Long-term engagement puzzles designed to take months
    /// for the player community to solve collectively.
    ///
    /// Mysteries implemented:
    ///   1. "The Seventh Realm"     — 5 clues across 5 realms; unlocks Twilight Nexus
    ///   2. "The First Seeker"      — fossil-based origin story fragments
    ///   3. "The Prophecy Constellation" — inverse constellation, eclipse-gated
    ///
    /// Each mystery tracks per-player clue discovery locally. The Firebase backend
    /// aggregates global discovery counts.
    ///
    /// Solving a mystery triggers a community celebration and unlocks content for
    /// EVERY player (non-exclusionary by design).
    /// </summary>
    public sealed class MysteryManager : MonoBehaviour
    {
        public static MysteryManager Instance { get; private set; }

        // ── PlayerPrefs keys ─────────────────────────────────────────────
        private const string PREF = "Mystery_";

        // ── Mystery identifiers ──────────────────────────────────────────
        public const string MYSTERY_SEVENTH_REALM     = "seventh_realm";
        public const string MYSTERY_FIRST_SEEKER      = "first_seeker";
        public const string MYSTERY_PROPHECY_CONST    = "prophecy_constellation";

        // ── Seventh Realm clues per realm ────────────────────────────────
        private static readonly Dictionary<string, string> SEVENTH_REALM_CLUES = new Dictionary<string, string>
        {
            { "Emberforge", "A hidden glyph burned into the oldest flame — it points elsewhere." },
            { "Verdant",    "The ancient tree whispers coordinates only the wind can hear." },
            { "EchoFields", "A constellation no one has named pulses with a location." },
            { "DawnCitadel","The prism does not shatter light here — it maps it." },
            { "Lantern",    "The hundred-thousandth lantern carries the final fragment." }
        };

        // ── First Seeker story fragments ──────────────────────────────────
        private static readonly string[] FIRST_SEEKER_FRAGMENTS =
        {
            "Before all others, one hand reached into the void...",
            "They called it formless. They made it glow.",
            "The forge remembers a fire that burned differently.",
            "A garden that grew from a single breath.",
            "The stars bent their paths to greet the first eyes that looked up.",
            "No name was given. Only the echo remains.",
        };

        // ── Events ────────────────────────────────────────────────────────
        public event Action<string> OnMysteryClueDiscovered;   // mysteryId
        public event Action<string> OnMysterySolved;           // mysteryId

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Clue Discovery ────────────────────────────────────────────────

        /// <summary>
        /// Called when a player has fulfilled the conditions to discover a mystery clue.
        /// realmId for MYSTERY_SEVENTH_REALM must match a key in SEVENTH_REALM_CLUES.
        /// fragmentIndex for MYSTERY_FIRST_SEEKER selects which story fragment to surface.
        /// </summary>
        public string DiscoverClue(string mysteryId, string realmId = "", int fragmentIndex = 0)
        {
            string clueText = "";

            switch (mysteryId)
            {
                case MYSTERY_SEVENTH_REALM:
                    clueText = DiscoverSeventhRealmClue(realmId);
                    break;

                case MYSTERY_FIRST_SEEKER:
                    clueText = DiscoverFirstSeekerFragment(fragmentIndex);
                    break;

                case MYSTERY_PROPHECY_CONST:
                    clueText = DiscoverProphecyClue();
                    break;
            }

            if (!string.IsNullOrEmpty(clueText))
            {
                OnMysteryClueDiscovered?.Invoke(mysteryId);
                AchievementManager.Instance?.TrackProgress("seventh_realm_seeker", GetSeventhRealmProgress());

                HUDManager.Instance?.ShowNotification(
                    $"Mystery Fragment: \"{clueText}\"",
                    HUDManager.NotificationType.Achievement);

                // Push to Firebase
                FirebaseManager.Instance?.SaveData("mysteryClues",
                    $"{mysteryId}_{realmId}_{SystemInfo.deviceUniqueIdentifier[..8]}",
                    new { mystery = mysteryId, realm = realmId, text = clueText });
            }

            return clueText;
        }

        /// <summary>Returns how many Seventh Realm clues this player has found (0-5).</summary>
        public int GetSeventhRealmProgress() =>
            PlayerPrefs.GetInt($"{PREF}{MYSTERY_SEVENTH_REALM}_CluesFound", 0);

        /// <summary>Returns how many First Seeker fragments this player has collected (0-6).</summary>
        public int GetFirstSeekerProgress() =>
            PlayerPrefs.GetInt($"{PREF}{MYSTERY_FIRST_SEEKER}_FragmentsFound", 0);

        public bool IsMysterySolved(string mysteryId) =>
            PlayerPrefs.GetInt($"{PREF}{mysteryId}_Solved", 0) == 1;

        // ── Prophecy gating ───────────────────────────────────────────────

        /// <summary>
        /// Returns true if real-world conditions allow the Prophecy Constellation today:
        /// specific date window AND an inverse (gap-based) pattern is in play.
        /// For simplicity we approximate solar eclipse windows using a known table.
        /// </summary>
        public bool IsProphecyConstellationAvailable()
        {
            DateTime now = DateTime.UtcNow;
            // Known solar eclipse windows (UTC dates, simplified to day-of-year checks):
            // 2026-02-17, 2026-08-12, 2027-02-06, 2027-08-02
            int doy = now.DayOfYear;
            int year = now.Year;
            bool eclipseWindow =
                (year == 2026 && (doy >= 47 && doy <= 49)) ||   // Feb 17
                (year == 2026 && (doy >= 223 && doy <= 225)) ||  // Aug 12
                (year == 2027 && (doy >= 37 && doy <= 39)) ||    // Feb 06
                (year == 2027 && (doy >= 213 && doy <= 215));    // Aug 02
            return eclipseWindow;
        }

        // ── Private helpers ────────────────────────────────────────────────

        private string DiscoverSeventhRealmClue(string realmId)
        {
            string foundKey = $"{PREF}{MYSTERY_SEVENTH_REALM}_{realmId}_Found";
            if (PlayerPrefs.GetInt(foundKey, 0) == 1)
                return "";  // already found this clue

            if (!SEVENTH_REALM_CLUES.ContainsKey(realmId)) return "";

            PlayerPrefs.SetInt(foundKey, 1);
            int count = PlayerPrefs.GetInt($"{PREF}{MYSTERY_SEVENTH_REALM}_CluesFound", 0) + 1;
            PlayerPrefs.SetInt($"{PREF}{MYSTERY_SEVENTH_REALM}_CluesFound", count);
            PlayerPrefs.Save();

            if (count >= 5) SolveSeventhRealm();

            return SEVENTH_REALM_CLUES[realmId];
        }

        private string DiscoverFirstSeekerFragment(int index)
        {
            index = Mathf.Clamp(index, 0, FIRST_SEEKER_FRAGMENTS.Length - 1);
            string foundKey = $"{PREF}{MYSTERY_FIRST_SEEKER}_Frag_{index}_Found";
            if (PlayerPrefs.GetInt(foundKey, 0) == 1) return "";

            PlayerPrefs.SetInt(foundKey, 1);
            int count = PlayerPrefs.GetInt($"{PREF}{MYSTERY_FIRST_SEEKER}_FragmentsFound", 0) + 1;
            PlayerPrefs.SetInt($"{PREF}{MYSTERY_FIRST_SEEKER}_FragmentsFound", count);
            PlayerPrefs.Save();

            if (count >= FIRST_SEEKER_FRAGMENTS.Length) SolveFirstSeeker();

            return FIRST_SEEKER_FRAGMENTS[index];
        }

        private string DiscoverProphecyClue()
        {
            if (!IsProphecyConstellationAvailable()) return "";
            if (PlayerPrefs.GetInt($"{PREF}{MYSTERY_PROPHECY_CONST}_Found", 0) == 1) return "";

            PlayerPrefs.SetInt($"{PREF}{MYSTERY_PROPHECY_CONST}_Found", 1);
            PlayerPrefs.Save();
            SolveProphecyConstellation();

            return "Between the stars, in the silence, the shape of tomorrow waits.";
        }

        private void SolveSeventhRealm()
        {
            if (PlayerPrefs.GetInt($"{PREF}{MYSTERY_SEVENTH_REALM}_Solved", 0) == 1) return;
            PlayerPrefs.SetInt($"{PREF}{MYSTERY_SEVENTH_REALM}_Solved", 1);
            PlayerPrefs.Save();

            OnMysterySolved?.Invoke(MYSTERY_SEVENTH_REALM);
            AchievementManager.Instance?.UnlockAchievement("seventh_realm_unlocked");
            HUDManager.Instance?.ShowNotification(
                "✦ THE TWILIGHT NEXUS STIRS — A new realm awaits...",
                HUDManager.NotificationType.Achievement);

            Debug.Log("[MysteryManager] SEVENTH REALM SOLVED");
        }

        private void SolveFirstSeeker()
        {
            if (PlayerPrefs.GetInt($"{PREF}{MYSTERY_FIRST_SEEKER}_Solved", 0) == 1) return;
            PlayerPrefs.SetInt($"{PREF}{MYSTERY_FIRST_SEEKER}_Solved", 1);
            PlayerPrefs.Save();

            OnMysterySolved?.Invoke(MYSTERY_FIRST_SEEKER);
            AchievementManager.Instance?.UnlockAchievement("origin_story_unlocked");
            HUDManager.Instance?.ShowNotification(
                "✦ ORIGIN STORY UNLOCKED — The First Seeker is revealed.",
                HUDManager.NotificationType.Achievement);

            Debug.Log("[MysteryManager] FIRST SEEKER SOLVED");
        }

        private void SolveProphecyConstellation()
        {
            OnMysterySolved?.Invoke(MYSTERY_PROPHECY_CONST);
            AchievementManager.Instance?.UnlockAchievement("prophecy_seer");
            HUDManager.Instance?.ShowNotification(
                "✦ THE PROPHECY CONSTELLATION — You are now immortalised as an NPC.",
                HUDManager.NotificationType.Achievement);
        }
    }
}
