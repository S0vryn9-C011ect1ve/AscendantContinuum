using System.Collections.Generic;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Data;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Manages all player cosmetic unlocks: particle trails, aura colors,
    /// journal themes, and sigil glow variants.
    ///
    /// Source of unlocks:
    ///   • <c>CosmicPatronManager</c>  — IAP rewards
    ///   • <c>AchievementManager</c>   — milestone rewards
    ///   • Consecutive-day milestones  — 7 / 30 / 100 days
    ///   • Sigil count milestones      — 30 / 100 / 365 sigils
    ///
    /// Active selections are persisted through <see cref="SaveSystem"/>.
    /// Fires <see cref="GameEvents.OnCosmeticUnlocked"/> on new unlocks.
    /// </summary>
    public sealed class CosmeticSystem : MonoBehaviour
    {
        public static CosmeticSystem Instance { get; private set; }

        // ── Catalogue definitions ─────────────────────────────────────────────

        public enum CosmeticType { ParticleTrail, AuraColor, JournalTheme, SigilGlow }

        public struct CosmeticItem
        {
            public string        Id;
            public string        DisplayName;
            public CosmeticType  Type;
            public Color         PrimaryColor;
            public string        UnlockCondition; // human-readable
        }

        // Free unlocks (always available)
        private static readonly CosmeticItem[] DefaultItems = new CosmeticItem[]
        {
            new CosmeticItem { Id = "trail_stardust",  DisplayName = "Stardust Trail",   Type = CosmeticType.ParticleTrail, PrimaryColor = new Color(0.85f, 0.9f, 1f),   UnlockCondition = "Default"       },
            new CosmeticItem { Id = "aura_white",      DisplayName = "Pearl Aura",        Type = CosmeticType.AuraColor,     PrimaryColor = Color.white,                 UnlockCondition = "Default"       },
            new CosmeticItem { Id = "journal_night",   DisplayName = "Midnight Journal",  Type = CosmeticType.JournalTheme,  PrimaryColor = new Color(0.05f, 0.05f, 0.15f), UnlockCondition = "Default"    },
            new CosmeticItem { Id = "glow_soft",       DisplayName = "Soft Glow",         Type = CosmeticType.SigilGlow,     PrimaryColor = new Color(0.7f, 0.85f, 1f),  UnlockCondition = "Default"       },
        };

        // Milestone unlocks (earned in-game)
        private static readonly CosmeticItem[] MilestoneItems = new CosmeticItem[]
        {
            new CosmeticItem { Id = "trail_aurora",      DisplayName = "Aurora Trail",    Type = CosmeticType.ParticleTrail, PrimaryColor = new Color(0.2f, 1f, 0.6f),   UnlockCondition = "7 consecutive days"  },
            new CosmeticItem { Id = "aura_gold",         DisplayName = "Sunrise Aura",    Type = CosmeticType.AuraColor,     PrimaryColor = new Color(1f, 0.85f, 0.3f),  UnlockCondition = "30 consecutive days" },
            new CosmeticItem { Id = "journal_cosmos",    DisplayName = "Cosmos Journal",  Type = CosmeticType.JournalTheme,  PrimaryColor = new Color(0.05f, 0f, 0.2f),  UnlockCondition = "100 sigils drawn"    },
            new CosmeticItem { Id = "glow_celestial",    DisplayName = "Celestial Glow",  Type = CosmeticType.SigilGlow,     PrimaryColor = new Color(0.5f, 0.3f, 1f),   UnlockCondition = "100 consecutive days"},
            new CosmeticItem { Id = "trail_ember",       DisplayName = "Emberforge Trail",Type = CosmeticType.ParticleTrail, PrimaryColor = new Color(1f, 0.4f, 0.1f),   UnlockCondition = "30 sigils drawn"     },
            new CosmeticItem { Id = "trail_verdant",     DisplayName = "Verdant Bloom",   Type = CosmeticType.ParticleTrail, PrimaryColor = new Color(0.2f, 0.9f, 0.4f), UnlockCondition = "Complete Verdant realm 10 times" },
            new CosmeticItem { Id = "aura_violet",       DisplayName = "Echo Aura",       Type = CosmeticType.AuraColor,     PrimaryColor = new Color(0.7f, 0.2f, 1f),   UnlockCondition = "365 sigils drawn"    },
            new CosmeticItem { Id = "journal_dawn",      DisplayName = "Dawn Journal",    Type = CosmeticType.JournalTheme,  PrimaryColor = new Color(0.25f, 0.12f, 0.05f), UnlockCondition = "7 sessions at dawn" },
        };

        // Premium items (IAP unlocked via CosmicPatronManager)
        private static readonly CosmeticItem[] PremiumItems = new CosmeticItem[]
        {
            new CosmeticItem { Id = "trail_starweaver",  DisplayName = "Starweaver Path", Type = CosmeticType.ParticleTrail, PrimaryColor = new Color(0.9f, 0.7f, 1f),   UnlockCondition = "Cosmic Patron"       },
            new CosmeticItem { Id = "glow_prismatic",    DisplayName = "Prismatic Sigil", Type = CosmeticType.SigilGlow,     PrimaryColor = Color.white,                 UnlockCondition = "Cosmic Patron"       },
            new CosmeticItem { Id = "journal_crystal",   DisplayName = "Crystal Journal", Type = CosmeticType.JournalTheme,  PrimaryColor = new Color(0.9f, 0.97f, 1f),  UnlockCondition = "Cosmic Patron"       },
        };

        // ── Runtime state ──────────────────────────────────────────────────────

        private HashSet<string>      _unlockedIds;
        private string               _activeTrailId;
        private string               _activeAuraId;
        private string               _activeJournalId;
        private string               _activeSigilGlowId;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            LoadFromSave();
            GrantDefaults();
            CheckMilestoneUnlocks();
        }

        // ── Public API — active cosmetic queries ──────────────────────────────

        public Color GetActiveAuraColor()
        {
            var item = FindById(_activeAuraId);
            return item.HasValue ? item.Value.PrimaryColor : Color.white;
        }

        public Color GetActiveTrailColor()
        {
            var item = FindById(_activeTrailId);
            return item.HasValue ? item.Value.PrimaryColor : new Color(0.85f, 0.9f, 1f);
        }

        public string GetActiveJournalThemeId()  => _activeJournalId  ?? "journal_night";
        public string GetActiveSigilGlowId()     => _activeSigilGlowId ?? "glow_soft";
        public string GetActiveTrailId()         => _activeTrailId    ?? "trail_stardust";

        public bool IsUnlocked(string id) => _unlockedIds != null && _unlockedIds.Contains(id);

        /// <summary>Equip a cosmetic by id. No-ops if not owned.</summary>
        public bool Equip(string id)
        {
            if (_unlockedIds == null || !_unlockedIds.Contains(id)) return false;

            var item = FindById(id);
            if (!item.HasValue) return false;

            switch (item.Value.Type)
            {
                case CosmeticType.ParticleTrail: _activeTrailId      = id; break;
                case CosmeticType.AuraColor:     _activeAuraId       = id; break;
                case CosmeticType.JournalTheme:  _activeJournalId    = id; break;
                case CosmeticType.SigilGlow:     _activeSigilGlowId  = id; break;
            }

            PersistSelections();
            return true;
        }

        /// <summary>Grant a cosmetic (from IAP, achievement, or milestone). Fires event on new unlock.</summary>
        public void Unlock(string id)
        {
            _unlockedIds ??= new HashSet<string>();
            if (_unlockedIds.Contains(id)) return;

            _unlockedIds.Add(id);
            PersistAll();
            GameEvents.RaiseCosmeticUnlocked(id);
            Debug.Log($"[CosmeticSystem] Unlocked: {id}");
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void GrantDefaults()
        {
            foreach (var item in DefaultItems) Unlock(item.Id);
        }

        private void CheckMilestoneUnlocks()
        {
            if (SaveSystem.Instance == null) return;
            var data = SaveSystem.Instance.CurrentPlayerData;

            int consec = data.consecutiveDays;
            int sigils = data.sigilCountTotal;

            if (consec >= 7)   Unlock("trail_aurora");
            if (consec >= 30)  Unlock("aura_gold");
            if (consec >= 100) Unlock("glow_celestial");
            if (sigils >= 30)  Unlock("trail_ember");
            if (sigils >= 100) Unlock("journal_cosmos");
            if (sigils >= 365) Unlock("aura_violet");
        }

        private void LoadFromSave()
        {
            _unlockedIds = new HashSet<string>();
            if (SaveSystem.Instance == null) return;

            var data = SaveSystem.Instance.CurrentPlayerData;

            if (data.unlockedCosmeticIds != null)
                foreach (var id in data.unlockedCosmeticIds)
                    _unlockedIds.Add(id);

            _activeTrailId     = string.IsNullOrEmpty(data.activeCosmeticTrailId) ? "trail_stardust" : data.activeCosmeticTrailId;
            _activeJournalId   = string.IsNullOrEmpty(data.activeJournalThemeId)  ? "journal_night"  : data.activeJournalThemeId;
            _activeSigilGlowId = string.IsNullOrEmpty(data.activeSigilGlowId)     ? "glow_soft"      : data.activeSigilGlowId;
            _activeAuraId      = "aura_white"; // default; not yet persisted separately
        }

        private void PersistAll()
        {
            if (SaveSystem.Instance == null) return;
            var data = SaveSystem.Instance.CurrentPlayerData;

            var ids = new string[_unlockedIds.Count];
            _unlockedIds.CopyTo(ids);
            data.unlockedCosmeticIds = ids;

            PersistSelections();
        }

        private void PersistSelections()
        {
            if (SaveSystem.Instance == null) return;
            var data = SaveSystem.Instance.CurrentPlayerData;
            data.activeCosmeticTrailId = _activeTrailId;
            data.activeJournalThemeId  = _activeJournalId;
            data.activeSigilGlowId     = _activeSigilGlowId;
            SaveSystem.Instance.SaveGame();
        }

        private static CosmeticItem? FindById(string id)
        {
            if (id == null) return null;
            foreach (var item in DefaultItems)  if (item.Id == id) return item;
            foreach (var item in MilestoneItems) if (item.Id == id) return item;
            foreach (var item in PremiumItems)   if (item.Id == id) return item;
            return null;
        }
    }
}
