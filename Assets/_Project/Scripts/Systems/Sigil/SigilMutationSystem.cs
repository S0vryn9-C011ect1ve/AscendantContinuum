using System;
using System.Collections.Generic;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Data;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Watches the player's owned sigils and evolves their visual properties over
    /// time and in response to celestial/collective events.
    ///
    /// Mutation stages (stored in PlayerPrefs per sigilId):
    ///   0  — freshly drawn (base)
    ///   1  — Day 7+:  slow breathing pulse (sine 0.3 Hz)
    ///   2  — Day 30+: secondary color accent bleeds in
    ///   3  — Celestial event overlap: shimmer overlay added
    ///
    /// On session start call <see cref="TickAllSigils"/> to advance any due mutations.
    /// </summary>
    public sealed class SigilMutationSystem : MonoBehaviour
    {
        public static SigilMutationSystem Instance { get; private set; }

        // ── Events ────────────────────────────────────────────────────────────
        /// <summary>Fired when a sigil's mutation stage advances. Arg: sigilId.</summary>
        public event Action<string, int> OnSigilMutated;

        // ── Constants ─────────────────────────────────────────────────────────
        private const string PREF_PREFIX    = "SigilMutation_";
        private const int    STAGE_PULSE    = 1;  // 7 days
        private const int    STAGE_TINT     = 2;  // 30 days
        private const int    STAGE_SHIMMER  = 3;  // celestial event

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            TickAllSigils();

            // Wire celestial events
            GameEvents.OnCelestialEventPeak += HandleCelestialEvent;
        }

        private void OnDestroy() => GameEvents.OnCelestialEventPeak -= HandleCelestialEvent;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluates all owned sigils and advances any mutation stages that are due.
        /// Call once per session start.
        /// </summary>
        public void TickAllSigils()
        {
            string[] ids = GetOwnedSigilIds();
            foreach (string id in ids)
                EvaluateSigil(id, DateTime.UtcNow);
        }

        /// <summary>
        /// Returns the current mutation stage for a sigil (0–3).
        /// </summary>
        public int GetMutationStage(string sigilId)
        {
            return PlayerPrefs.GetInt($"{PREF_PREFIX}{sigilId}_stage", 0);
        }

        /// <summary>
        /// Returns the material property override for the sigil LineRenderer glow.
        /// </summary>
        public SigilMutationProperties GetMutationProperties(string sigilId)
        {
            int stage = GetMutationStage(sigilId);
            return new SigilMutationProperties
            {
                pulseSpeed    = stage >= STAGE_PULSE   ? 0.3f : 0f,
                secondaryBlend= stage >= STAGE_TINT    ? 0.35f : 0f,
                shimmerActive = stage >= STAGE_SHIMMER
            };
        }

        /// <summary>
        /// Registers a newly drawn sigil with today's date as creation date.
        /// </summary>
        public void RegisterNewSigil(string sigilId)
        {
            string key = $"{PREF_PREFIX}{sigilId}_created";
            if (!PlayerPrefs.HasKey(key))
                PlayerPrefs.SetString(key, DateTime.UtcNow.ToString("o"));
            PlayerPrefs.SetInt($"{PREF_PREFIX}{sigilId}_stage", 0);
        }

        // ── Static testability helpers ────────────────────────────────────────

        /// <summary>
        /// Pure function: computes the mutation stage for a sigil given age and whether
        /// a celestial event is currently active. Stage 3 overrides all age-based stages.
        /// This is the same logic used by <see cref="EvaluateSigil"/> and
        /// <see cref="HandleCelestialEvent"/>, exposed as a static for unit testing.
        /// </summary>
        public static int CalculateMutationStage(int daysOld, bool celestialEventActive)
        {
            if (celestialEventActive)  return STAGE_SHIMMER;
            if (daysOld >= 30)         return STAGE_TINT;
            if (daysOld >= 7)          return STAGE_PULSE;
            return 0;
        }

        /// <summary>
        /// Returns a fully-built <see cref="SigilMutationProperties"/> for the given stage.
        /// Mirrors the logic in <see cref="GetMutationProperties"/> without requiring an
        /// instance or PlayerPrefs, making it suitable for pure unit tests.
        /// </summary>
        public static SigilMutationProperties BuildProperties(int stage)
        {
            return new SigilMutationProperties
            {
                pulseSpeed     = stage >= STAGE_PULSE   ? 0.3f  : 0f,
                secondaryBlend = stage >= STAGE_TINT    ? 0.35f : 0f,
                shimmerActive  = stage >= STAGE_SHIMMER
            };
        }

        // ── Private ────────────────────────────────────────────────────────────

        private void EvaluateSigil(string sigilId, DateTime now)
        {
            string dateKey = $"{PREF_PREFIX}{sigilId}_created";
            if (!PlayerPrefs.HasKey(dateKey)) return;

            if (!DateTime.TryParse(PlayerPrefs.GetString(dateKey), out DateTime created)) return;

            int daysSince = (int)(now - created).TotalDays;
            int current   = GetMutationStage(sigilId);
            int target    = current;

            if      (daysSince >= 30 && current < STAGE_TINT)    target = STAGE_TINT;
            else if (daysSince >= 7  && current < STAGE_PULSE)   target = STAGE_PULSE;

            if (target != current)
            {
                PlayerPrefs.SetInt($"{PREF_PREFIX}{sigilId}_stage", target);
                Debug.Log($"[SigilMutation] {sigilId} advanced to stage {target}.");
                OnSigilMutated?.Invoke(sigilId, target);
            }
        }

        private void HandleCelestialEvent(string eventId)
        {
            string[] ids = GetOwnedSigilIds();
            foreach (string id in ids)
            {
                int current = GetMutationStage(id);
                if (current < STAGE_SHIMMER)
                {
                    PlayerPrefs.SetInt($"{PREF_PREFIX}{id}_stage", STAGE_SHIMMER);
                    OnSigilMutated?.Invoke(id, STAGE_SHIMMER);
                }
            }
            Debug.Log($"[SigilMutation] Celestial event '{eventId}' applied shimmer to all sigils.");
        }

        private static string[] GetOwnedSigilIds()
        {
            // Try SaveSystem first, fall back to PlayerPrefs count
            if (SaveSystem.Instance?.CurrentPlayerData?.sigilIds is string[] ids && ids.Length > 0)
                return ids;

            // Scan PlayerPrefs for known sigil keys
            var found = new List<string>();
            string raw = PlayerPrefs.GetString("OwnedSigilIds", "");
            if (!string.IsNullOrEmpty(raw))
                found.AddRange(raw.Split(','));
            return found.ToArray();
        }
    }

    /// <summary>
    /// Material property overrides driven by <see cref="SigilMutationSystem"/>.
    /// Applied by the SigilJournalManager and GestureDrawingController rendering.
    /// </summary>
    [Serializable]
    public struct SigilMutationProperties
    {
        public float pulseSpeed;     // 0 = static, 0.3 Hz = slow breath
        public float secondaryBlend; // 0 = base color only, 0.35 = accented
        public bool  shimmerActive;  // true = shimmer particle overlay on

        // PascalCase accessors for C# conventions and unit-test readability
        public float PulseSpeed          { get => pulseSpeed;     set => pulseSpeed     = value; }
        public float SecondaryColorBlend { get => secondaryBlend; set => secondaryBlend = value; }
        public bool  ShimmerActive       { get => shimmerActive;  set => shimmerActive  = value; }
    }
}
