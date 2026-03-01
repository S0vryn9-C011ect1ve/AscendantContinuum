using System.Collections;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AscendantContinuum.Tests.PlayMode
{
    /// <summary>
    /// PlayMode smoke tests covering the core session loop:
    ///   SessionManager → GameEvents → SeasonController XP award
    ///   GameEvents.OnSigilCompleted → SeasonController.AddXP(+150)
    ///   GameEvents.OnChainCompleted → SeasonController.AddXP(+200)
    ///   PantheonDeityEffects Day4 gate
    ///   SeasonController tier progression
    /// </summary>
    public class RealmLoopSmokeTests
    {
        private GameObject _seasonGO;
        private GameObject _pantheonGO;
        private GameObject _sessionGO;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_seasonGO);
            Object.Destroy(_pantheonGO);
            Object.Destroy(_sessionGO);
            _seasonGO   = null;
            _pantheonGO = null;
            _sessionGO  = null;
        }

        // ── SeasonController XP award via GameEvents ──────────────────────

        [UnityTest]
        public IEnumerator SigilCompleted_GameEvent_AwardsSeasonXP()
        {
            _seasonGO = new GameObject("SeasonController_Test");
            var sc = _seasonGO.AddComponent<SeasonController>();

            yield return null; // let Start() run — subscribes to GameEvents

            int xpBefore = sc.PlayerXP;

            // Fire sigil completed event
            GameEvents.RaiseSigilCompleted(new SigilData { sigilId = "test_sigil" });
            yield return null;

            Assert.Greater(sc.PlayerXP, xpBefore, "Sigil completion should award season XP");
        }

        [UnityTest]
        public IEnumerator ChainCompleted_GameEvent_AwardsSeasonXP()
        {
            _seasonGO = new GameObject("SeasonController_Test");
            var sc = _seasonGO.AddComponent<SeasonController>();

            yield return null;

            int xpBefore = sc.PlayerXP;

            GameEvents.RaiseChainCompleted("chain_alpha");
            yield return null;

            Assert.Greater(sc.PlayerXP, xpBefore, "Chain completion should award season XP");
        }

        [UnityTest]
        public IEnumerator CelestialEventPeak_AwardsSeasonXP()
        {
            _seasonGO = new GameObject("SeasonController_Test");
            var sc = _seasonGO.AddComponent<SeasonController>();

            yield return null;

            int xpBefore = sc.PlayerXP;

            GameEvents.RaiseCelestialEventPeak("lunar_eclipse");
            yield return null;

            Assert.Greater(sc.PlayerXP, xpBefore, "Celestial event peak should award season XP");
        }

        // ── Tier progression ──────────────────────────────────────────────

        [UnityTest]
        public IEnumerator AddXP_ExactThreshold_AdvancesTier()
        {
            _seasonGO = new GameObject("SeasonController_Test");
            var sc = _seasonGO.AddComponent<SeasonController>();

            yield return null;

            Assert.AreEqual(0, sc.CurrentTier);

            sc.AddXP(SeasonController.XP_PER_TIER, "test");
            yield return null;

            Assert.AreEqual(1, sc.CurrentTier, "One full tier of XP should advance tier to 1");
        }

        [UnityTest]
        public IEnumerator ClaimFreeReward_AfterTierReached_Succeeds()
        {
            _seasonGO = new GameObject("SeasonController_Test");
            var sc = _seasonGO.AddComponent<SeasonController>();

            yield return null;

            // Grant enough XP for tier 0
            sc.AddXP(SeasonController.XP_PER_TIER, "test");
            yield return null;

            bool claimed = sc.ClaimFreeReward(0);
            Assert.IsTrue(claimed, "Should be able to claim tier 0 reward after reaching it");
            Assert.IsTrue(sc.IsFreeRewardClaimed(0));
        }

        [UnityTest]
        public IEnumerator ClaimFreeReward_BeforeTierReached_Fails()
        {
            _seasonGO = new GameObject("SeasonController_Test");
            var sc = _seasonGO.AddComponent<SeasonController>();

            yield return null;

            // No XP earned — tier 0 not reached
            bool claimed = sc.ClaimFreeReward(5);
            Assert.IsFalse(claimed, "Cannot claim tier 5 without reaching it");
        }

        // ── PantheonDeityEffects Day4 gate ────────────────────────────────

        [UnityTest]
        public IEnumerator ShouldShowPantheonQuiz_FalseBeforeDay4()
        {
            _pantheonGO = new GameObject("PantheonDeityEffects_Test");
            var pde = _pantheonGO.AddComponent<PantheonDeityEffects>();

            yield return null;

            // SessionCount_Total is 0 (PlayerPrefs wiped in SetUp)
            Assert.IsFalse(pde.ShouldShowPantheonQuiz(),
                "Quiz should not show before session 4");
        }

        [UnityTest]
        public IEnumerator ShouldShowPantheonQuiz_TrueOnDay4_WithNoDeity()
        {
            PlayerPrefs.SetInt("SessionCount_Total", 4);

            _pantheonGO = new GameObject("PantheonDeityEffects_Test");
            var pde = _pantheonGO.AddComponent<PantheonDeityEffects>();

            yield return null;

            Assert.IsTrue(pde.ShouldShowPantheonQuiz(),
                "Quiz should appear on session 4 when no deity is chosen");
        }

        [UnityTest]
        public IEnumerator ShouldShowPantheonQuiz_FalseIfDeityAlreadyChosen()
        {
            PlayerPrefs.SetInt("SessionCount_Total", 4);
            PlayerPrefs.SetInt("Pantheon_ActiveDeity", PantheonDeityEffects.DEITY_FLAME);
            PlayerPrefs.SetInt("Pantheon_QuizCompleted", 1);

            _pantheonGO = new GameObject("PantheonDeityEffects_Test");
            var pde = _pantheonGO.AddComponent<PantheonDeityEffects>();

            yield return null;

            Assert.IsFalse(pde.ShouldShowPantheonQuiz(),
                "Quiz should not show again once a deity is already chosen");
        }

        // ── SessionManager increments counter ─────────────────────────────

        [UnityTest]
        public IEnumerator SessionManager_Awake_IncrementsSessionCount()
        {
            PlayerPrefs.SetInt("SessionCount_Total", 2);

            _sessionGO = new GameObject("SessionManager_Test");
            _sessionGO.AddComponent<SessionManager>();

            yield return null; // let Awake + Start run

            Assert.AreEqual(3, PlayerPrefs.GetInt("SessionCount_Total"),
                "SessionManager.Awake should increment SessionCount_Total");
        }

        // ── Full loop: sigil draw → XP → tier check ───────────────────────

        [UnityTest]
        public IEnumerator FullLoop_DrawFiveSigils_AdvancesTier()
        {
            _seasonGO = new GameObject("SeasonController_Test");
            var sc = _seasonGO.AddComponent<SeasonController>();

            yield return null;

            // 5 sigils × 150 XP = 750 XP → tier 1 (need XP_PER_TIER = 500)
            for (int i = 0; i < 5; i++)
            {
                GameEvents.RaiseSigilCompleted(new SigilData { sigilId = $"sigil_{i}" });
                yield return null;
            }

            Assert.GreaterOrEqual(sc.CurrentTier, 1,
                "Five sigils should award at least 750 XP, crossing tier 1 threshold");
        }
    }
}
