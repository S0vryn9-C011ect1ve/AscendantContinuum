using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.EchoFields;
using AscendantContinuum.Social;

namespace AscendantContinuum.Realms.EchoFields
{
    /// <summary>
    /// Scene controller for the Echo Fields realm.
    ///
    /// Wires <see cref="ConstellationTracer"/> completion events into the core loop:
    ///   • Daily challenge progress (<see cref="ChallengeType.CollectSparks"/> for star-sigils,
    ///     <see cref="ChallengeType.DiscoverSecret"/> for hidden constellations)
    ///   • Auto-save after each constellation completed
    ///   • Achievement: "stargazer" on first completion, "astronomer" on 5 completions
    ///
    /// Attach to the root GameObject of the Realm_EchoFields scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EchoFieldsController : RealmController
    {
        [Header("Echo Fields Components")]
        [SerializeField] private ConstellationTracer tracer;

        protected override void Awake()
        {
            base.Awake();
            realmId          = "echo";
            realmDisplayName = "The Echo Fields";

            if (tracer == null)
                tracer = GetComponentInChildren<ConstellationTracer>(true);
        }

        protected override void SubscribeToRealmEvents()
        {
            if (tracer != null)
                tracer.OnConstellationCompleted += HandleConstellationCompleted;
            else
                Debug.LogWarning("[EchoFieldsController] ConstellationTracer not found in scene.");

            // Start a ritual replay capture for this realm session
            RitualReplayManager.Instance?.BeginCapture("Constellation Trace", realmId);
        }

        protected override void UnsubscribeFromRealmEvents()
        {
            if (tracer != null)
                tracer.OnConstellationCompleted -= HandleConstellationCompleted;
        }

        private void HandleConstellationCompleted(int totalCompleted)
        {
            // Completing a constellation yields star-sigils → treat as sigil discovery
            OnRealmProgressMade(ChallengeType.DiscoverSecret, amount: 1, sigils: totalCompleted);

            if (totalCompleted == 1)
                TryUnlockAchievement("stargazer");

            if (totalCompleted >= 5)
            {
                TryUnlockAchievement("astronomer");
                if (totalCompleted == 5)
                    CompleteRealm(totalCompleted);
            }

            // Echo Archive — record this ritual as a memory echo
            EchoArchiveManager.Instance?.RecordRitualEcho("EchoFields", "Constellation Trace");

            // Ritual Replay — finalise capture
            RitualReplayManager.Instance?.EndCapture();

            // Puzzle Chain — try to link this realm to an active chain
            PuzzleChainManager.Instance?.TryMatchRealmToChain("EchoFields");

            // Mystery — surface a First Seeker fragment every 5th constellation
            if (totalCompleted % 5 == 0)
                MysteryManager.Instance?.DiscoverClue(MysteryManager.MYSTERY_FIRST_SEEKER,
                    fragmentIndex: (totalCompleted / 5 - 1) % 6);
        }
    }
}
