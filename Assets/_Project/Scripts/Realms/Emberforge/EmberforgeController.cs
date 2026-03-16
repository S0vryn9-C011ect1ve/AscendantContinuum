using UnityEngine;
using TMPro;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Emberforge;
using AscendantContinuum.EchoFields;
using AscendantContinuum.Social;

namespace AscendantContinuum.Realms.Emberforge
{
    /// <summary>
    /// Scene controller for the Emberforge realm.
    ///
    /// Wires the <see cref="EmberforgeSparks"/> mechanic into the core loop:
    ///   • Daily challenge progress (<see cref="ChallengeType.CollectSparks"/>)
    ///   • Auto-save via <see cref="RealmController"/>
    ///   • Achievement checks (first spark, 100 sparks, 1 000 sparks)
    ///
    /// Attach this MonoBehaviour to the root GameObject of the Emberforge scene
    /// alongside an <see cref="EmberforgeSparks"/> component.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EmberforgeController : RealmController
    {
        // ── Inspector refs ────────────────────────────────────────────────
        [Header("Emberforge Components")]
        [SerializeField] private EmberforgeSparks sparksSystem;

        [Header("UI")]
        [SerializeField] private TMP_Text sparkCounterText;

        // ── Unity lifecycle ────────────────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            realmId          = "emberforge";
            realmDisplayName = "The Emberforge";

            if (sparksSystem == null)
                sparksSystem = GetComponentInChildren<EmberforgeSparks>(true);
        }

        // ── Event wiring ───────────────────────────────────────────────────
        protected override void SubscribeToRealmEvents()
        {
            if (sparksSystem != null)
                sparksSystem.OnSparkCollected += HandleSparkCollected;
            else
                Debug.LogWarning("[EmberforgeController] EmberforgeSparks not found in scene.");

            // Start a ritual replay capture for this realm session
            RitualReplayManager.Instance?.BeginCapture("Spark Forge", realmId);
        }

        protected override void UnsubscribeFromRealmEvents()
        {
            if (sparksSystem != null)
                sparksSystem.OnSparkCollected -= HandleSparkCollected;
        }

        // ── Handlers ──────────────────────────────────────────────────────
        private void HandleSparkCollected(int totalSparks)
        {
            // Update UI counter
            if (sparkCounterText != null)
                sparkCounterText.text = $"Sparks: {totalSparks}/20";

            // Each collected spark = 1 unit of CollectSparks challenge progress
            OnRealmProgressMade(ChallengeType.CollectSparks, amount: 1, sparks: totalSparks);

            // Achievement gates
            switch (totalSparks)
            {
                case 1:
                    TryUnlockAchievement("first_spark");
                    // Puzzle Chain — first spark initiates an elemental chain if none active
                    if (!PuzzleChainManager.Instance.HasActiveChain)
                        PuzzleChainManager.Instance?.StartElementalChain("Emberforge");
                    break;
                case 100:
                    TryUnlockAchievement("spark_collector_100");
                    // Echo Archive record at milestone
                    EchoArchiveManager.Instance?.RecordRitualEcho("Emberforge", "Spark Forge");
                    // Ritual Replay
                    RitualReplayManager.Instance?.EndCapture();
                    break;
                case 1000:
                    TryUnlockAchievement("spark_master_1000");
                    EchoArchiveManager.Instance?.RecordRitualEcho("Emberforge", "Thousand Sparks");
                    // Seventh Realm clue — 1000 sparks unlocks the Emberforge glyph
                    MysteryManager.Instance?.DiscoverClue(MysteryManager.MYSTERY_SEVENTH_REALM, "Emberforge");
                    CompleteRealm(totalSparks);
                    break;
            }
        }
    }
}
