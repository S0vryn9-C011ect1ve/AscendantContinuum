using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Social;
using AscendantContinuum.EchoFields;

namespace AscendantContinuum.Realms.LanternAscension
{
    /// <summary>
    /// Scene controller for the Lantern Ascension realm.
    ///
    /// Wires <see cref="LanternRitual"/> events into the full system stack:
    ///   • Daily challenge progress (MeditateInRealm / CompleteRitualsFast)
    ///   • EchoArchive: records each release as a ritual echo
    ///   • RitualReplay: captures the release moment for sharing
    ///   • PuzzleChain: each release attempts to progress the Elemental Chain
    ///   • NPCCollectiveMemory: records "peace" emotion for Petalina
    ///   • TimeCapsule: checks for newly discoverable capsules after each release
    ///   • MysteryManager: 100k+ global lanterns seed a Seventh Realm clue
    ///   • PantheonDeityEffects: reports ritual for deity-specific bonuses
    ///   • Lantern Keeper: 0.1% chance per release; guaranteed at 50 lanterns
    ///   • Achievements: lantern_lighter, sky_illuminator, lantern_keeper_met
    ///
    /// Attach to the root GameObject of the Realm_LanternAscension scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LanternAscensionController : RealmController
    {
        [Header("Lantern Ascension Components")]
        [SerializeField] private LanternRitual ritual;

        private const string PREF_RELEASED_COUNT = "Lantern_ReleasedCount";
        private const int    LANTERN_KEEPER_GUARANTEE = 50;

        private int _lanternsReleased;

        protected override void Awake()
        {
            base.Awake();
            realmId          = "lantern";
            realmDisplayName = "The Lantern Ascension";

            if (ritual == null)
                ritual = GetComponentInChildren<LanternRitual>(true);

            _lanternsReleased = PlayerPrefs.GetInt(PREF_RELEASED_COUNT, 0);
        }

        protected override void SubscribeToRealmEvents()
        {
            if (ritual != null)
            {
                ritual.OnLanternReleased    += HandleLanternReleased;
                ritual.OnMeditationStarted  += HandleMeditationStarted;
                ritual.OnMeditationEnded    += HandleMeditationEnded;
            }
            else
            {
                Debug.LogWarning("[LanternAscensionController] LanternRitual not found in scene.");
            }
        }

        protected override void UnsubscribeFromRealmEvents()
        {
            if (ritual != null)
            {
                ritual.OnLanternReleased   -= HandleLanternReleased;
                ritual.OnMeditationStarted -= HandleMeditationStarted;
                ritual.OnMeditationEnded   -= HandleMeditationEnded;
            }
        }

        // ── Lantern Released ──────────────────────────────────────────────
        private void HandleLanternReleased(Lantern lantern)
        {
            _lanternsReleased++;
            PlayerPrefs.SetInt(PREF_RELEASED_COUNT, _lanternsReleased);
            PlayerPrefs.Save();

            // Core progress
            OnRealmProgressMade(ChallengeType.CompleteRitualsFast, amount: 1);

            // ── EchoArchive ──────────────────────────────────────────────
            EchoArchiveManager.Instance?.RecordRitualEcho("lantern", "Lantern Release");

            // ── RitualReplay capture ─────────────────────────────────────
            RitualReplayManager.Instance?.EndCapture();

            // ── Puzzle Chain ─────────────────────────────────────────────
            PuzzleChainManager.Instance?.TryMatchRealmToChain("lantern");

            // ── NPC Collective Memory: Petalina feels peace ──────────────
            NPCCollectiveMemory.Instance?.RecordPlayerEmotion(
                NPCCollectiveMemory.NPC_PETALINA, "peace");

            // ── Time Capsule: check for discoverable capsules ─────────────
            TimeCapsuleManager.Instance?.CheckForDiscoverableCapsules();

            // ── Pantheon deity effect ─────────────────────────────────────
            PantheonDeityEffects.Instance?.OnRitualCompleted("lantern");

            // ── Serendipity roll ─────────────────────────────────────────
            SerendipityManager.Instance?.TryTrigger("lantern");

            // ── Mystery: Seventh Realm — "Lantern" fragment ───────────────
            // Every 100th release seeds a global clue
            if (_lanternsReleased % 100 == 0)
                MysteryManager.Instance?.DiscoverClue("seventh_realm", "lantern", 1);

            // ── Achievements ──────────────────────────────────────────────
            if (_lanternsReleased == 1)
                TryUnlockAchievement("lantern_lighter");

            if (_lanternsReleased >= 10)
                TryUnlockAchievement("sky_illuminator");

            // ── Lantern Keeper: 0.1% chance; guaranteed at 50 ─────────────
            bool meetKeeper = _lanternsReleased == LANTERN_KEEPER_GUARANTEE
                || (UnityEngine.Random.value < 0.001f && _lanternsReleased < LANTERN_KEEPER_GUARANTEE);

            if (meetKeeper)
                TriggerLanternKeeperAppearance();
        }

        // ── Meditation ────────────────────────────────────────────────────
        private void HandleMeditationStarted()
        {
            OnRealmProgressMade(ChallengeType.MeditateInRealm, amount: 1);

            // Begin replay capture for the meditation ritual
            RitualReplayManager.Instance?.BeginCapture("Void Meditation", "lantern");

            // Nurturer: calm aura during meditation
            if (PantheonDeityEffects.Instance != null &&
                PantheonDeityEffects.Instance.CalmAuraActive)
            {
                AudioListener.volume = Mathf.Min(AudioListener.volume, 0.5f);
            }
        }

        private void HandleMeditationEnded()
        {
            SaveCurrentProgress();
            EchoArchiveManager.Instance?.RecordRitualEcho("lantern", "Void Meditation");
            MysteryManager.Instance?.DiscoverClue("seventh_realm", "lantern", 2);
        }

        // ── Lantern Keeper ────────────────────────────────────────────────
        private void TriggerLanternKeeperAppearance()
        {
            TryUnlockAchievement("lantern_keeper_met");

            UI.HUDManager.Instance?.ShowNotification(
                "✨ The Lantern Keeper appears — a rare guardian of ascending light",
                UI.HUDManager.NotificationType.Achievement);

            Core.FirebaseManager.Instance?.TrackEvent("lantern_keeper_appeared",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { "lanterns_released", _lanternsReleased }
                });

            Debug.Log($"[LanternAscension] Lantern Keeper appeared at {_lanternsReleased} lanterns.");
        }
    }
}
