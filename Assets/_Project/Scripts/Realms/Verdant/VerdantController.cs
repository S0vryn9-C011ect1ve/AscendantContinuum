using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Verdant;
using AscendantContinuum.EchoFields;
using AscendantContinuum.Social;

namespace AscendantContinuum.Realms.Verdant
{
    /// <summary>
    /// Scene controller for the Verdant Garden realm.
    ///
    /// Wires <see cref="VerdantGarden"/> bloom events into the core loop:
    ///   • Daily challenge progress (<see cref="ChallengeType.MeditateInRealm"/>)
    ///   • Auto-save on each plant bloom
    ///   • Achievement: "green_thumb" after first bloom
    ///
    /// Attach to the root GameObject of the Realm_Verdant scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VerdantController : RealmController
    {
        [Header("Verdant Components")]
        [SerializeField] private VerdantGarden garden;

        protected override void Awake()
        {
            base.Awake();
            realmId          = "verdant";
            realmDisplayName = "The Verdant Garden";

            if (garden == null)
                garden = GetComponentInChildren<VerdantGarden>(true);
        }

        protected override void SubscribeToRealmEvents()
        {
            if (garden != null)
                garden.OnPlantBloomed += HandlePlantBloomed;
            else
                Debug.LogWarning("[VerdantController] VerdantGarden not found in scene.");

            // Start a ritual replay capture for this realm session
            RitualReplayManager.Instance?.BeginCapture("Garden Bloom", realmId);
        }

        protected override void UnsubscribeFromRealmEvents()
        {
            if (garden != null)
                garden.OnPlantBloomed -= HandlePlantBloomed;
        }

        private void HandlePlantBloomed(int totalBlooms)
        {
            // Each bloom counts as one meditation/connection unit
            OnRealmProgressMade(ChallengeType.MeditateInRealm, amount: 1);

            if (totalBlooms == 1)
                TryUnlockAchievement("green_thumb");

            if (totalBlooms >= 10)
                TryUnlockAchievement("verdant_master");

            // Echo Archive
            EchoArchiveManager.Instance?.RecordRitualEcho("Verdant", "Garden Bloom");

            // Ritual Replay — finalise
            RitualReplayManager.Instance?.EndCapture();

            // NPC Collective Memory — each bloom is a joyful moment with Petalina
            NPCCollectiveMemory.Instance?.RecordPlayerEmotion(
                NPCCollectiveMemory.NPC_PETALINA, "joy");

            // Puzzle Chain
            PuzzleChainManager.Instance?.TryMatchRealmToChain("Verdant");

            // Mystery — 10th bloom surfaces ancient tree coordinates
            if (totalBlooms == 10)
                MysteryManager.Instance?.DiscoverClue(MysteryManager.MYSTERY_SEVENTH_REALM, "Verdant");
        }
    }
}
