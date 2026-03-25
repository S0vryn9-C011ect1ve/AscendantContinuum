using UnityEngine;
using System;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Pantheon Deity Effects — applies the gameplay bonuses for the player's
    /// chosen deity. All deities are equally rewarding; buffs affect style, not
    /// power-level (CONSTITUTION.md principle).
    ///
    /// Six deities (PANTHEON_SYSTEM.md):
    ///   0 – Ascendant Flame         (+15% sigil drop, every-5 ritual variation)
    ///   1 – Herald of Joyful Curiosity (+5% serendipity, hidden sigil glow)
    ///   2 – Archivist of Bright Memories (Memory Buff every 10th ritual, replay mode)
    ///   3 – Mechanic of Helpful Wonders  (crafting probability UI, bonus variant)
    ///   4 – Scribe of Magical Knowledge  (deep hints, rare Sixth Realm clue)
    ///   5 – Silent Nurturer              (calm aura, no-penalty fail, wishes glow)
    ///
    /// Active deity is stored in PlayerPrefs so it persists across scenes.
    /// All other systems call the public API to check active bonuses.
    /// </summary>
    public sealed class PantheonDeityEffects : MonoBehaviour
    {
        public static PantheonDeityEffects Instance { get; private set; }

        // ── PlayerPrefs keys ──────────────────────────────────────────────
        private const string PREF_ACTIVE_DEITY   = "Pantheon_ActiveDeity";      // int 0-5, -1 = none
        private const string PREF_RITUAL_COUNT   = "Pantheon_RitualCount";      // for variation check
        private const string PREF_MEMORY_BUFF    = "Pantheon_MemoryBuffActive"; // Archivist buff
        private const string PREF_SIXTH_REALM    = "Pantheon_SixthRealmClue";   // Scribe clue found
        private const string PREF_QUIZ_DONE      = "Pantheon_QuizCompleted";

        // ── Deity index constants ──────────────────────────────────────────
        public const int DEITY_FLAME     = 0;
        public const int DEITY_HERALD    = 1;
        public const int DEITY_ARCHIVIST = 2;
        public const int DEITY_MECHANIC  = 3;
        public const int DEITY_SCRIBE    = 4;
        public const int DEITY_NURTURER  = 5;

        // ── Events ────────────────────────────────────────────────────────
        public event Action<int> OnDeitySelected;          // index of chosen deity
        public event Action      OnMemoryBuffActivated;    // Archivist 10th-ritual buff
        public event Action      OnSixthRealmClueFound;    // Scribe rare appearance

        // ── State ─────────────────────────────────────────────────────────
        private int _activeDeity = -1;      // -1 = no deity chosen yet
        private int _ritualCount;
        private bool _memoryBuffActive;
        private float _memoryBuffExpiry;    // Time.realtimeSinceStartup timestamp

        // ── Deity names (for UI / screen reader) ──────────────────────────
        public static readonly string[] DEITY_NAMES =
        {
            "Ascendant Flame",
            "Herald of Joyful Curiosity",
            "Archivist of Bright Memories",
            "Mechanic of Helpful Wonders",
            "Scribe of Magical Knowledge",
            "Silent Nurturer"
        };

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _activeDeity   = PlayerPrefs.GetInt(PREF_ACTIVE_DEITY, -1);
            _ritualCount   = PlayerPrefs.GetInt(PREF_RITUAL_COUNT, 0);
            _memoryBuffActive = PlayerPrefs.GetInt(PREF_MEMORY_BUFF, 0) == 1;

            ApplyPassiveEffects();
        }

        // ── Deity selection ───────────────────────────────────────────────

        /// <summary>
        /// Called once on Day 4 when the player confirms their deity.
        /// Selection is permanent for the primary deity (per design doc).
        /// </summary>
        public void SelectDeity(int deityIndex)
        {
            if (deityIndex < 0 || deityIndex > 5)
            {
                Debug.LogWarning($"[PantheonDeityEffects] Invalid deity index: {deityIndex}");
                return;
            }

            _activeDeity = deityIndex;
            PlayerPrefs.SetInt(PREF_ACTIVE_DEITY, deityIndex);
            PlayerPrefs.SetInt(PREF_QUIZ_DONE, 1);
            PlayerPrefs.Save();

            ApplyPassiveEffects();
            OnDeitySelected?.Invoke(deityIndex);

            string name = DEITY_NAMES[deityIndex];

            // Record in the living lore alignment tally
            LivingLoreManager.Instance?.RecordDeityAlignment(name);

            HUDManager.Instance?.ShowNotification(
                $"✦ {name} walks beside you now.",
                HUDManager.NotificationType.Achievement);

            Debug.Log($"[PantheonDeityEffects] Deity selected: {name} (index {deityIndex})");
        }

        public int  ActiveDeity => _activeDeity;
        public bool HasDeity    => _activeDeity >= 0;
        public bool QuizDone    => PlayerPrefs.GetInt(PREF_QUIZ_DONE, 0) == 1;

        public string ActiveDeityName =>
            HasDeity ? DEITY_NAMES[_activeDeity] : "None";

        // ── Called by realm controllers on every ritual completion ─────────

        /// <summary>
        /// Notify the deity system that a ritual was completed.
        /// Call this from every realm controller's HandleXxxComplete method.
        /// Handles Ascendant Flame (5-ritual variation) and
        /// Archivist (10th-ritual Memory Buff).
        /// </summary>
        public void OnRitualCompleted(string realmId)
        {
            _ritualCount++;
            PlayerPrefs.SetInt(PREF_RITUAL_COUNT, _ritualCount);
            PlayerPrefs.Save();

            // ── Ascendant Flame: variation at every 5th ritual (base is 10) ──
            if (_activeDeity == DEITY_FLAME && _ritualCount % 5 == 0)
            {
                HUDManager.Instance?.ShowNotification(
                    "🔥 The Ascendant Flame unlocks a new ritual variation!",
                    HUDManager.NotificationType.Info);
                AchievementManager.Instance?.TrackProgress("flame_variations", 1);
            }

            // ── Archivist: Memory Buff every 10th ritual ──────────────────
            if (_activeDeity == DEITY_ARCHIVIST && _ritualCount % 10 == 0)
            {
                ActivateMemoryBuff();
            }

            // ── Scribe: 1% chance to surface a Sixth Realm clue ──────────
            if (_activeDeity == DEITY_SCRIBE)
            {
                if (UnityEngine.Random.value < 0.01f && PlayerPrefs.GetInt(PREF_SIXTH_REALM, 0) == 0)
                {
                    DiscoverSixthRealmClue();
                }
            }
        }

        // ── Ascendant Flame ───────────────────────────────────────────────

        /// <summary>
        /// Sigil drop rate multiplier (1.15 for Flame, 1.0 otherwise).
        /// Checked by sigil drop logic in EchoArchiveManager / realm controllers.
        /// </summary>
        public float SigilDropMultiplier =>
            _activeDeity == DEITY_FLAME ? 1.15f : 1.0f;

        /// <summary>
        /// How often ritual variations unlock. Flame = every 5; others = every 10.
        /// </summary>
        public int RitualVariationInterval =>
            _activeDeity == DEITY_FLAME ? 5 : 10;

        // ── Herald of Joyful Curiosity ────────────────────────────────────

        /// <summary>
        /// Extra serendipity probability bonus from the Herald.
        /// Add this to <see cref="SerendipityManager"/> legendary/epic chances.
        /// </summary>
        public float SerendipityBonus =>
            _activeDeity == DEITY_HERALD ? 0.05f : 0.0f;

        /// <summary>
        /// When true, hidden sigils in the current realm should glow faintly
        /// so the Herald's curious followers can spot them.
        /// </summary>
        public bool HiddenSigilsGlow =>
            _activeDeity == DEITY_HERALD;

        // ── Archivist of Bright Memories ─────────────────────────────────

        /// <summary>
        /// True while the Archivist's Memory Buff is active (persists 30 min real-time).
        /// All EchoArchive recordings made during this window are upgraded one tier.
        /// </summary>
        public bool IsMemoryBuffActive
        {
            get
            {
                if (!_memoryBuffActive) return false;
                if (Time.realtimeSinceStartup > _memoryBuffExpiry)
                {
                    _memoryBuffActive = false;
                    PlayerPrefs.SetInt(PREF_MEMORY_BUFF, 0);
                    PlayerPrefs.Save();
                    return false;
                }
                return true;
            }
        }

        private void ActivateMemoryBuff()
        {
            _memoryBuffActive = true;
            _memoryBuffExpiry = Time.realtimeSinceStartup + 1800f; // 30 min
            PlayerPrefs.SetInt(PREF_MEMORY_BUFF, 1);
            PlayerPrefs.Save();

            OnMemoryBuffActivated?.Invoke();
            HUDManager.Instance?.ShowNotification(
                "📚 Memory Buff active — Echo recordings upgraded for 30 minutes!",
                HUDManager.NotificationType.Reward);
        }

        // ── Mechanic of Helpful Wonders ───────────────────────────────────

        /// <summary>
        /// When true, SigilCraftingManager should display numeric probability for
        /// known combo results in the crafting UI.
        /// </summary>
        public bool ShowCraftingProbability =>
            _activeDeity == DEITY_MECHANIC;

        /// <summary>
        /// Crafting bonus: Mechanic followers have a 5% higher chance of a
        /// bonus variant when crafting a known combo.
        /// </summary>
        public float CraftingBonusVariantChance =>
            _activeDeity == DEITY_MECHANIC ? 0.05f : 0.0f;

        // ── Scribe of Magical Knowledge ───────────────────────────────────

        /// <summary>
        /// Hint depth multiplier. 1 = standard hints; 2 = detailed contextual hints
        /// (e.g. in LightRefractionPuzzle).
        /// </summary>
        public int HintDepthLevel =>
            _activeDeity == DEITY_SCRIBE ? 2 : 1;

        /// <summary>True once the Scribe's Sixth Realm clue has been discovered.</summary>
        public bool SixthRealmClueFound =>
            PlayerPrefs.GetInt(PREF_SIXTH_REALM, 0) == 1;

        private void DiscoverSixthRealmClue()
        {
            PlayerPrefs.SetInt(PREF_SIXTH_REALM, 1);
            PlayerPrefs.Save();

            OnSixthRealmClueFound?.Invoke();
            MysteryManager.Instance?.DiscoverClue("seventh_realm", "scribe", 0);

            HUDManager.Instance?.ShowNotification(
                "📜 The Scribe whispers of a Sixth Realm hidden in the stars…",
                HUDManager.NotificationType.Achievement);

            AchievementManager.Instance?.UnlockAchievement("scribe_revelation");
            Debug.Log("[PantheonDeityEffects] Scribe: Sixth Realm clue discovered.");
        }

        // ── Silent Nurturer ───────────────────────────────────────────────

        /// <summary>
        /// When true, rituals should not apply failure penalties (no spark cost on fail,
        /// no combo break). Nurturer followers always receive positive reinforcement.
        /// </summary>
        public bool NoPenaltyOnFail =>
            _activeDeity == DEITY_NURTURER;

        /// <summary>
        /// When true, lantern wishes and ritual completion VFX should use softer,
        /// moonlit visual variants (set by the Nurturer).
        /// </summary>
        public bool WishesGlowSoftly =>
            _activeDeity == DEITY_NURTURER;

        /// <summary>
        /// When true, the ambient "calm aura" reduces all realm timescale perturbations
        /// — no sudden particle bursts, no loud sounds on first entry.
        /// </summary>
        public bool CalmAuraActive =>
            _activeDeity == DEITY_NURTURER;

        // ── Passive effect application ────────────────────────────────────

        private void ApplyPassiveEffects()
        {
            if (!HasDeity) return;

            // Herald: wire serendipity bonus into SerendipityManager
            if (_activeDeity == DEITY_HERALD && SerendipityManager.Instance != null)
            {
                SerendipityManager.Instance.AddLegendaryChanceBonus(SerendipityBonus);
            }

            Debug.Log($"[PantheonDeityEffects] Passive effects applied for '{ActiveDeityName}'.");
        }

        // ── Pantheon personality quiz helper ─────────────────────────────

        /// <summary>
        /// Returns the recommended deity index based on 5-question quiz answers
        /// (answers are 0-indexed to match deity order: Flame=0, Herald=1, …).
        /// The actual selection is up to the player — this is just a suggestion.
        /// </summary>
        public static int CalculateQuizResult(int[] answers)
        {
            if (answers == null || answers.Length == 0) return DEITY_FLAME;

            int[] score = new int[6];
            foreach (int a in answers)
            {
                int clamped = Mathf.Clamp(a, 0, 5);
                score[clamped]++;
            }

            int best = 0;
            for (int i = 1; i < score.Length; i++)
                if (score[i] > score[best]) best = i;
            return best;
        }
        // ── Day 4 Pantheon unlock gate ────────────────────────────────────────────────

        /// <summary>
        /// Returns true when the Arcane Personality Quiz should be offered.
        /// Condition: the player has started 4+ sessions and has not yet chosen a deity.
        /// </summary>
        public bool ShouldShowPantheonQuiz()
        {
            if (HasDeity) return false;
            if (QuizDone) return false;
            // Prefer SaveSystem (encrypted, backed-up) over legacy PlayerPrefs
            int sessions = SaveSystem.Instance?.CurrentPlayerData?.totalSessionsCompleted
                           ?? PlayerPrefs.GetInt("SessionCount_Total", 0);
            return sessions >= 4;
        }

        /// <summary>True once the player has completed 4 or more gameplay sessions.</summary>
        public bool IsDay4UnlockAvailable()
        {
            int sessions = SaveSystem.Instance?.CurrentPlayerData?.totalSessionsCompleted
                           ?? PlayerPrefs.GetInt("SessionCount_Total", 0);
            return sessions >= 4;
        }
        // ── Screen reader summary ─────────────────────────────────────────

        public string GetAccessibilitySummary()
        {
            if (!HasDeity)
                return "No deity chosen yet. Complete the Arcane Personality Quiz on Day 4.";
            return $"Your deity is {ActiveDeityName}. " + _activeDeity switch
            {
                DEITY_FLAME     => "Sigil drops are 15% more frequent; ritual variations unlock every 5 completions.",
                DEITY_HERALD    => "Hidden sigils glow faintly; serendipity events are 5% more likely.",
                DEITY_ARCHIVIST => "Every 10th ritual activates a 30-minute Memory Buff; Echo Archive recordings upgrade a tier.",
                DEITY_MECHANIC  => "Sigil crafting shows probability; bonus rarity variants appear 5% more often.",
                DEITY_SCRIBE    => "Puzzle hints are more detailed; a rare Sixth Realm clue may appear.",
                DEITY_NURTURER  => "Ritual failures carry no penalty; ambient effects are calm and gentle.",
                _              => string.Empty
            };
        }
    }
}
