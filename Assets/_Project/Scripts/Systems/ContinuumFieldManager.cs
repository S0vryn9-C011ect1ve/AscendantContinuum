using System;
using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Offline-first collective energy simulation.
    ///
    /// Tracks sigils completed by THIS player and blends with a deterministic
    /// pseudo-collective value (seeded by day-of-year) to produce a
    /// believable "global activity" feel, even on first install.
    ///
    /// Outputs:
    ///   <see cref="GetCollectiveEnergy"/>      → [0,1] smooth float
    ///   <see cref="GetTimeZoneActivityPulse"/> → [0,1] peaks during local evening
    /// </summary>
    public sealed class ContinuumFieldManager : MonoBehaviour
    {
        public static ContinuumFieldManager Instance { get; private set; }

        // ── Constants ─────────────────────────────────────────────────────────
        public  const int   PSEUDO_MIN  = 800;    // believable daily global floor
        public  const int   PSEUDO_MAX  = 18000;  // believable daily global ceiling
        private const float NORM        = 20000f; // normalisation denominator

        // ── State ─────────────────────────────────────────────────────────────
        private int   _localDailyCount;
        private int   _localTotalCount;
        private float _collectiveEnergy;
        private string _lastSigilDate;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            LoadFromSave();
            RefreshCollectiveEnergy();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Must be called by <see cref="Systems.SigilCompletionHandler"/> each time
        /// a sigil is completed.
        /// </summary>
        public void RegisterSigilCompleted()
        {
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (_lastSigilDate != today)
            {
                _localDailyCount = 0;
                _lastSigilDate   = today;
            }

            _localDailyCount++;
            _localTotalCount++;

            PersistToSave();
            RefreshCollectiveEnergy();
        }

        /// <summary>Global collective energy level [0,1].</summary>
        public float GetCollectiveEnergy() => _collectiveEnergy;

        /// <summary>
        /// Returns [0,1] activity pulse based on local evening hours (6 pm – midnight peak).
        /// The idea: most players draw sigils in the evening, so the field feels busier then.
        /// </summary>
        public float GetTimeZoneActivityPulse() => GetTimeZoneActivityPulse(DateTime.Now.Hour);

        /// <summary>
        /// Returns the total local sigil count (persisted across sessions).
        /// </summary>
        public int GetLocalTotalCount() => _localTotalCount;

        // ── Private ────────────────────────────────────────────────────────────

        private void RefreshCollectiveEnergy()
        {
            var now  = DateTime.UtcNow;
            float raw = (_localDailyCount + DeterministicPseudoCollective(now.DayOfYear, now.Year)) / NORM;
            float prev = _collectiveEnergy;
            _collectiveEnergy = Mathf.Clamp01(raw);

            if (Mathf.Abs(_collectiveEnergy - prev) > 0.001f)
            {
                GameEvents.RaiseCollectiveEnergyChanged(_collectiveEnergy);

                // Also update save snapshot
                if (SaveSystem.Instance?.CurrentPlayerData != null)
                    SaveSystem.Instance.CurrentPlayerData.lastKnownCollectiveEnergy = _collectiveEnergy;
            }
        }

        /// <summary>
        /// Produces a believable pseudo-collective daily value seeded by day+year.
        /// Follows a seasonal sine envelope so energy is higher in spring/summer.
        /// Result is clamped to [PSEUDO_MIN, PSEUDO_MAX].
        /// </summary>
        private static int DeterministicPseudoCollective()
        {
            DateTime now = DateTime.UtcNow;
            return DeterministicPseudoCollective(now.DayOfYear, now.Year);
        }

        /// <summary>Parameterised overload for unit testing.</summary>
        public static int DeterministicPseudoCollective(int dayOfYear, int year)
        {
            int   seed  = dayOfYear * 1000 + year;
            var   rng   = new System.Random(seed);
            float base_ = rng.Next(PSEUDO_MIN, PSEUDO_MAX);

            // Seasonal multiplier: peaks around day 172 (mid-June) for N.hemisphere
            float season = 0.75f + 0.25f * Mathf.Sin((dayOfYear / 365f - 0.25f) * 2f * Mathf.PI);
            int   result = Mathf.RoundToInt(base_ * season);
            return Mathf.Clamp(result, PSEUDO_MIN, PSEUDO_MAX);
        }

        /// <summary>
        /// Purely functional energy computation — no MonoBehaviour state required.
        /// Used by unit tests and may be used by UI previews.
        /// </summary>
        public static float ComputeCollectiveEnergy(int localDailyCount, int dayOfYear, int year)
        {
            int pseudo = DeterministicPseudoCollective(dayOfYear, year);
            return Mathf.Clamp01((localDailyCount + pseudo) / NORM);
        }

        /// <summary>Parameterised overload of <see cref="GetTimeZoneActivityPulse"/> for unit tests.</summary>
        public static float GetTimeZoneActivityPulse(int hour)
        {
            float normalized = ((hour - 6f + 24f) % 24f) / 24f;
            return Mathf.Clamp01(Mathf.Sin(normalized * Mathf.PI));
        }

        private void LoadFromSave()
        {
            var data = SaveSystem.Instance?.CurrentPlayerData;
            if (data == null) return;

            _localTotalCount = data.sigilCountTotal;
            _lastSigilDate   = data.lastSigilDate;

            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            _localDailyCount = _lastSigilDate == today ? data.sigilCountToday : 0;
        }

        private void PersistToSave()
        {
            var data = SaveSystem.Instance?.CurrentPlayerData;
            if (data == null) return;

            string today           = DateTime.UtcNow.ToString("yyyy-MM-dd");
            data.sigilCountToday   = _localDailyCount;
            data.sigilCountTotal   = _localTotalCount;
            data.lastSigilDate     = today;
        }
    }
}
