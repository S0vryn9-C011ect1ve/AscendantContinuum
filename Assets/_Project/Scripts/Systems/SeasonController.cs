using System;
using System.Collections.Generic;
using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Seasonal Battle Pass and live-content scheduler.
    ///
    /// Each season runs <see cref="SEASON_DURATION_DAYS"/> days (9 weeks), starting
    /// from <see cref="SEASON_EPOCH"/> (Jan 1, 2026). On rollover the XP and
    /// claimed-reward state reset automatically.
    ///
    /// Two reward tracks:
    ///   Free      — 10 cosmetic tiers, reachable by every player.
    ///   Premium   — 10 additional tiers unlocked via a one-time season pass ($9.99).
    ///
    /// DESIGN RULES (CONSTITUTION.md / AGENT_INSTRUCTIONS.md):
    ///   • No gameplay advantage on either track — cosmetics only.
    ///   • No FOMO: season rewards return in later seasons.
    ///   • Parental / personal spending caps honoured via <see cref="CosmicPatronManager"/>.
    /// </summary>
    public sealed class SeasonController : MonoBehaviour
    {
        public static SeasonController Instance { get; private set; }

        // ── Duration constants ──────────────────────────────────────────────
        public const int SEASON_DURATION_DAYS = 63;   // 9 weeks
        public const int FREE_TIERS           = 10;
        public const int PREMIUM_TIERS        = 10;
        public const int XP_PER_TIER          = 500;

        // Season origin: Jan 1, 2026 UTC
        public static readonly DateTime SEASON_EPOCH =
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── PlayerPrefs key fragments (prefixed per-season) ─────────────────
        private const string PREF_PREMIUM         = "Season_Premium";
        private const string PREF_XP              = "Season_XP";
        private const string PREF_CLAIMED_FREE    = "Season_ClaimedFree";     // int bitmask
        private const string PREF_CLAIMED_PREMIUM = "Season_ClaimedPremium";  // int bitmask
        private const string PREF_TRACKED_SEASON  = "Season_TrackedNumber";

        // ── Events ───────────────────────────────────────────────────────────
        /// <summary>Fired whenever XP is awarded. Arg: amount gained this call.</summary>
        public event Action<int>        OnXPGained;
        /// <summary>Fired when a reward tier is claimed. Args: tier (0-based), isPremium.</summary>
        public event Action<int, bool>  OnRewardClaimed;
        /// <summary>Fired at the start of each new season.</summary>
        public event Action             OnSeasonChanged;

        // ── State ─────────────────────────────────────────────────────────────
        private int  _xp;
        private bool _isPremium;
        private int  _claimedFreeMask;
        private int  _claimedPremiumMask;
        private int  _trackedSeason = -1;

        // ── Properties ────────────────────────────────────────────────────────

        /// <summary>Zero-based season number since <see cref="SEASON_EPOCH"/>.</summary>
        public static int CurrentSeasonNumber()
        {
            double days = (DateTime.UtcNow - SEASON_EPOCH).TotalDays;
            return Mathf.Max(0, Mathf.FloorToInt((float)days / SEASON_DURATION_DAYS));
        }

        /// <summary>Day within the current season [0, <see cref="SEASON_DURATION_DAYS"/>-1].</summary>
        public static int DayInSeason()
        {
            double days = (DateTime.UtcNow - SEASON_EPOCH).TotalDays;
            return (int)(days % SEASON_DURATION_DAYS);
        }

        /// <summary>Week number within the current season [1–9].</summary>
        public static int WeekInSeason() => DayInSeason() / 7 + 1;

        /// <summary>Season progress [0, 1] where 1.0 = last day.</summary>
        public static float SeasonProgress()
            => Mathf.Clamp01(DayInSeason() / (float)(SEASON_DURATION_DAYS - 1));

        /// <summary>Human-readable season title, e.g. "Season 1 — The Awakening".</summary>
        public static string SeasonDisplayName(int seasonNumber = -1)
        {
            if (seasonNumber < 0) seasonNumber = CurrentSeasonNumber();
            string[] themes =
            {
                "The Awakening",  "The Ember",      "The Bloom",
                "The Echo",       "The Citadel",    "The Ascension",
                "The Convergence","The Wandering",  "The Returning"
            };
            string theme = themes[seasonNumber % themes.Length];
            return $"Season {seasonNumber + 1} \u2014 {theme}";
        }

        /// <summary>Current accumulated XP this season.</summary>
        public int  PlayerXP        => _xp;

        /// <summary>Whether the premium pass is active for the current season.</summary>
        public bool IsPremiumActive => _isPremium;

        /// <summary>Current 0-based tier reached (each tier requires <see cref="XP_PER_TIER"/> XP).</summary>
        public int  CurrentTier     => Mathf.Min(Mathf.FloorToInt(_xp / (float)XP_PER_TIER), FREE_TIERS - 1);

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadState();
            CheckSeasonRollover();
            SubscribeGameEvents();
        }

        private void OnDestroy()
        {
            GameEvents.OnSigilCompleted     -= OnSigilCompleted;
            GameEvents.OnChainCompleted     -= OnChainCompleted;
            GameEvents.OnCelestialEventPeak -= OnCelestialPeak;
        }

        private void SubscribeGameEvents()
        {
            GameEvents.OnSigilCompleted     += OnSigilCompleted;
            GameEvents.OnChainCompleted     += OnChainCompleted;
            GameEvents.OnCelestialEventPeak += OnCelestialPeak;

            if (DailyChallengeManager.Instance != null)
                DailyChallengeManager.Instance.OnChallengeCompleted += OnDailyChallengeCompleted;
        }

        private void OnSigilCompleted(Data.SigilData _)              => AddXP(150, "sigil_drawn");
        private void OnChainCompleted(string _)                       => AddXP(200, "chain_completed");
        private void OnCelestialPeak(string _)                        => AddXP(100, "celestial_peak");
        private void OnDailyChallengeCompleted(int sparks)            => AddXP(sparks / 2, "daily_challenge");

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Award XP toward the current battle pass. <paramref name="source"/> is for debug logs.</summary>
        public void AddXP(int amount, string source = "")
        {
            if (amount <= 0) return;
            int oldTier = CurrentTier;
            _xp = Mathf.Min(_xp + amount, FREE_TIERS * XP_PER_TIER);
            SaveState();
            OnXPGained?.Invoke(amount);

            if (CurrentTier > oldTier)
                Debug.Log($"[SeasonController] Tier reached: {CurrentTier + 1}/{FREE_TIERS} ({source})");
        }

        /// <summary>
        /// Activate the premium pass for this season. Should be called once the IAP is
        /// confirmed successful by <see cref="Platform.CosmicPatronManager"/>.
        /// </summary>
        public void UnlockPremiumTrack()
        {
            _isPremium = true;
            PlayerPrefs.SetInt(SeasonKey(PREF_PREMIUM), 1);
            PlayerPrefs.Save();
            Debug.Log("[SeasonController] Premium battle pass track unlocked.");
        }

        /// <summary>
        /// Claim a free-track reward at <paramref name="tier"/> (0-based).
        /// Returns <c>true</c> if the reward was successfully claimed.
        /// </summary>
        public bool ClaimFreeReward(int tier)
        {
            if (tier < 0 || tier >= FREE_TIERS) return false;
            if (CurrentTier < tier)             return false;
            int bit = 1 << tier;
            if ((_claimedFreeMask & bit) != 0)  return false;   // already claimed

            _claimedFreeMask |= bit;
            PlayerPrefs.SetInt(SeasonKey(PREF_CLAIMED_FREE), _claimedFreeMask);
            PlayerPrefs.Save();
            OnRewardClaimed?.Invoke(tier, false);
            Debug.Log($"[SeasonController] Free tier {tier} ({FREE_REWARD_LABELS[tier]}) claimed.");
            return true;
        }

        /// <summary>
        /// Claim a premium-track reward at <paramref name="tier"/> (0-based).
        /// Returns <c>true</c> if the reward was successfully claimed.
        /// </summary>
        public bool ClaimPremiumReward(int tier)
        {
            if (!_isPremium)                       return false;
            if (tier < 0 || tier >= PREMIUM_TIERS) return false;
            if (CurrentTier < tier)                return false;
            int bit = 1 << tier;
            if ((_claimedPremiumMask & bit) != 0)  return false;

            _claimedPremiumMask |= bit;
            PlayerPrefs.SetInt(SeasonKey(PREF_CLAIMED_PREMIUM), _claimedPremiumMask);
            PlayerPrefs.Save();
            OnRewardClaimed?.Invoke(tier, true);
            Debug.Log($"[SeasonController] Premium tier {tier} ({PREMIUM_REWARD_LABELS[tier]}) claimed.");
            return true;
        }

        public bool IsFreeRewardClaimed(int tier)
            => tier >= 0 && tier < FREE_TIERS && (_claimedFreeMask    & (1 << tier)) != 0;

        public bool IsPremiumRewardClaimed(int tier)
            => tier >= 0 && tier < PREMIUM_TIERS && (_claimedPremiumMask & (1 << tier)) != 0;

        // ── Reward catalogues (cosmetic descriptions only) ────────────────────

        public static readonly string[] FREE_REWARD_LABELS =
        {
            "Ember Sigil Glow",         // 0
            "Amber Particle Trail",     // 1
            "Verdant Stamp",            // 2
            "Echo Constellation Set",   // 3
            "Journal Cover — Season",   // 4
            "Dawn Star Aura",           // 5
            "Lantern Orb Trail",        // 6
            "Sigil Frame — Woven",      // 7
            "Animated Glow Pulse",      // 8
            "Season Title Badge"        // 9
        };

        public static readonly string[] PREMIUM_REWARD_LABELS =
        {
            "Cosmic Nameplate Frame",   // 0
            "Aurora Particle Burst",    // 1
            "Holographic Sigil Skin",   // 2
            "Realm Key Cosmetic",       // 3
            "Prismatic Trail",          // 4
            "Deity Glyph Stamp",        // 5
            "Full Animated Sigil Skin", // 6
            "Sound Orb Cosmetic",       // 7
            "Exclusive Season Sigil",   // 8
            "Cosmic Patron Title"       // 9
        };

        // ── Private ───────────────────────────────────────────────────────────

        private void LoadState()
        {
            _xp                 = PlayerPrefs.GetInt(SeasonKey(PREF_XP), 0);
            _isPremium          = PlayerPrefs.GetInt(SeasonKey(PREF_PREMIUM), 0) == 1;
            _claimedFreeMask    = PlayerPrefs.GetInt(SeasonKey(PREF_CLAIMED_FREE), 0);
            _claimedPremiumMask = PlayerPrefs.GetInt(SeasonKey(PREF_CLAIMED_PREMIUM), 0);
            _trackedSeason      = PlayerPrefs.GetInt(PREF_TRACKED_SEASON, -1);
        }

        private void SaveState()
        {
            PlayerPrefs.SetInt(SeasonKey(PREF_XP), _xp);
            PlayerPrefs.Save();
        }

        private void CheckSeasonRollover()
        {
            int current = CurrentSeasonNumber();
            if (_trackedSeason >= 0 && _trackedSeason != current)
            {
                // New season — wipe player's seasonal state
                _xp                 = 0;
                _isPremium          = false;
                _claimedFreeMask    = 0;
                _claimedPremiumMask = 0;
                Debug.Log($"[SeasonController] Season changed: {SeasonDisplayName(current)} begins.");
                OnSeasonChanged?.Invoke();
            }
            _trackedSeason = current;
            PlayerPrefs.SetInt(PREF_TRACKED_SEASON, current);
            PlayerPrefs.Save();
        }

        /// <summary>Prefix a PlayerPrefs key with the current season number for automatic isolation.</summary>
        private static string SeasonKey(string key) => $"{key}_{CurrentSeasonNumber()}";
    }
}
