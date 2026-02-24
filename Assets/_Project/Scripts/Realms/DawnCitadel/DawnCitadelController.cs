using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Social;
using AscendantContinuum.EchoFields;

namespace AscendantContinuum.Realms.DawnCitadel
{
    /// <summary>
    /// Scene controller for the Dawn Citadel realm.
    ///
    /// Wires <see cref="LightRefractionPuzzle"/> completion into the full system stack:
    ///   • Daily challenge progress (CompleteRitualsFast)
    ///   • EchoArchive: records every solve as a ritual echo
    ///   • RitualReplay: captures the solve moment for sharing
    ///   • PuzzleChain: each solve attempts to progress the Elemental Chain
    ///   • MysteryManager: 3rd-solve seeds a Seventh Realm "Prism" clue
    ///   • PantheonDeityEffects: reports ritual for deity-specific bonuses
    ///     — Scribe: deep hints wired into LightRefractionPuzzle
    ///   • Achievements: light_bender (1st), master_refractor (3rd+),
    ///                   dawn_architect (10 solves)
    ///
    /// Attach to the root GameObject of the Realm_DawnCitadel scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DawnCitadelController : RealmController
    {
        [Header("Dawn Citadel Components")]
        [SerializeField] private LightRefractionPuzzle[] puzzles;

        private const string PREF_SOLVED_COUNT = "Dawn_SolvedCount";
        private int _solvedCount;

        protected override void Awake()
        {
            base.Awake();
            realmId          = "dawn";
            realmDisplayName = "The Dawn Citadel";

            if (puzzles == null || puzzles.Length == 0)
                puzzles = GetComponentsInChildren<LightRefractionPuzzle>(true);

            _solvedCount = PlayerPrefs.GetInt(PREF_SOLVED_COUNT, 0);
        }

        protected override void SubscribeToRealmEvents()
        {
            if (puzzles != null && puzzles.Length > 0)
            {
                foreach (var puzzle in puzzles)
                {
                    if (puzzle != null)
                        puzzle.OnPuzzleComplete += HandlePuzzleComplete;
                }

                // Wire Scribe deity hint depth
                ApplyDeityPuzzleSettings();
            }
            else
            {
                Debug.LogWarning("[DawnCitadelController] No LightRefractionPuzzle found in scene.");
            }
        }

        protected override void UnsubscribeFromRealmEvents()
        {
            if (puzzles == null) return;
            foreach (var puzzle in puzzles)
            {
                if (puzzle != null)
                    puzzle.OnPuzzleComplete -= HandlePuzzleComplete;
            }
        }

        // ── Puzzle Completed ──────────────────────────────────────────────
        private void HandlePuzzleComplete(LightRefractionPuzzle puzzle)
        {
            _solvedCount++;
            PlayerPrefs.SetInt(PREF_SOLVED_COUNT, _solvedCount);
            PlayerPrefs.Save();

            // Core progress
            OnRealmProgressMade(ChallengeType.CompleteRitualsFast, amount: 1);

            // ── EchoArchive ──────────────────────────────────────────────
            EchoArchiveManager.Instance?.RecordRitualEcho("dawn", "Prism Alignment");

            // ── RitualReplay ─────────────────────────────────────────────
            RitualReplayManager.Instance?.BeginCapture("Prism Alignment", "dawn");
            RitualReplayManager.Instance?.EndCapture();

            // ── Puzzle Chain ─────────────────────────────────────────────
            PuzzleChainManager.Instance?.TryMatchRealmToChain("DawnCitadel");

            // ── Pantheon deity effect ─────────────────────────────────────
            PantheonDeityEffects.Instance?.OnRitualCompleted("dawn");

            // ── Serendipity ───────────────────────────────────────────────
            SerendipityManager.Instance?.TryTrigger("dawn");

            // ── Mystery: Seventh Realm "Prism" clue on 3rd solve ─────────
            if (_solvedCount == 3)
                MysteryManager.Instance?.DiscoverClue("seventh_realm", "dawn", 3);

            // ── Achievements ──────────────────────────────────────────────
            if (_solvedCount == 1)
                TryUnlockAchievement("light_bender");

            if (_solvedCount >= 3)
                TryUnlockAchievement("master_refractor");

            if (_solvedCount >= 10)
                TryUnlockAchievement("dawn_architect");
        }

        // ── Deity hint depth application ──────────────────────────────────
        private void ApplyDeityPuzzleSettings()
        {
            if (PantheonDeityEffects.Instance == null) return;

            bool deepHints = PantheonDeityEffects.Instance.HintDepthLevel >= 2;
            if (puzzles == null) return;

            foreach (var puzzle in puzzles)
            {
                if (puzzle == null) continue;
                // Expose hints immediately for Scribe followers;
                // others still get hints after the standard delay
                puzzle.showHints  = true;
                puzzle.hintDelay  = deepHints ? 10f : 30f;
            }
        }
    }
}
