using System;
using NUnit.Framework;
using UnityEngine;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="SeasonController"/> static date-math and instance reward logic.
    /// </summary>
    public class SeasonControllerTests
    {
        private GameObject      _go;
        private SeasonController _mgr;

        [SetUp]
        public void SetUp()
        {
            var existing = UnityEngine.Object.FindFirstObjectByType<SeasonController>();
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

            // Clean all season-keyed PlayerPrefs used by these tests
            for (int i = 0; i < 5; i++)
            {
                PlayerPrefs.DeleteKey($"Season_XP_{i}");
                PlayerPrefs.DeleteKey($"Season_Premium_{i}");
                PlayerPrefs.DeleteKey($"Season_ClaimedFree_{i}");
                PlayerPrefs.DeleteKey($"Season_ClaimedPremium_{i}");
            }

            _go  = new GameObject("SeasonController_Test");
            _mgr = _go.AddComponent<SeasonController>();
            // Awake sets Instance but Start is not called; jump-start manually for instance tests
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) UnityEngine.Object.DestroyImmediate(_go);
        }

        // ── Static date math ──────────────────────────────────────────────

        [Test]
        public void DayInSeason_BelongsTo_ValidRange()
        {
            int day = SeasonController.DayInSeason();
            Assert.That(day, Is.InRange(0, SeasonController.SEASON_DURATION_DAYS - 1));
        }

        [Test]
        public void WeekInSeason_BelongsTo_ValidRange()
        {
            int week = SeasonController.WeekInSeason();
            int expectedMax = SeasonController.SEASON_DURATION_DAYS / 7 + 1;
            Assert.That(week, Is.InRange(1, expectedMax));
        }

        [Test]
        public void SeasonProgress_IsInUnitRange()
        {
            float progress = SeasonController.SeasonProgress();
            Assert.That(progress, Is.InRange(0f, 1f));
        }

        [Test]
        public void SeasonDisplayName_ContainsSeasonNumber()
        {
            int n = SeasonController.CurrentSeasonNumber();
            string name = SeasonController.SeasonDisplayName(n);
            Assert.That(name, Does.Contain($"Season {n + 1}"),
                "Display name should contain 'Season N'.");
        }

        [Test]
        public void SeasonDisplayName_ContainsDash()
        {
            string name = SeasonController.SeasonDisplayName(0);
            Assert.That(name, Does.Contain("\u2014"),
                "Display name should contain an em-dash separator.");
        }

        // ── XP and tier logic ─────────────────────────────────────────────

        [Test]
        public void AddXP_IncreasesPlayerXP()
        {
            _mgr.AddXP(100);
            Assert.That(_mgr.PlayerXP, Is.EqualTo(100));
        }

        [Test]
        public void CurrentTier_ZeroWithNoXP()
        {
            Assert.That(_mgr.CurrentTier, Is.EqualTo(0));
        }

        [Test]
        public void CurrentTier_AdvancesAtXPThreshold()
        {
            _mgr.AddXP(SeasonController.XP_PER_TIER);
            Assert.That(_mgr.CurrentTier, Is.EqualTo(1));
        }

        // ── Reward claiming ───────────────────────────────────────────────

        [Test]
        public void ClaimFreeReward_Tier0_SucceedsWithNoXPRequired()
        {
            // Tier 0 requires CurrentTier >= 0 (always true)
            bool claimed = _mgr.ClaimFreeReward(0);
            Assert.IsTrue(claimed);
            Assert.IsTrue(_mgr.IsFreeRewardClaimed(0));
        }

        [Test]
        public void ClaimFreeReward_AlreadyClaimed_ReturnsFalse()
        {
            _mgr.ClaimFreeReward(0);
            bool again = _mgr.ClaimFreeReward(0);
            Assert.IsFalse(again, "Cannot claim the same tier twice.");
        }

        [Test]
        public void ClaimPremiumReward_WithoutPremiumPass_ReturnsFalse()
        {
            bool claimed = _mgr.ClaimPremiumReward(0);
            Assert.IsFalse(claimed, "Cannot claim premium reward without a premium pass.");
        }

        [Test]
        public void ClaimPremiumReward_AfterUnlockingPass_Succeeds()
        {
            _mgr.UnlockPremiumTrack();
            bool claimed = _mgr.ClaimPremiumReward(0);
            Assert.IsTrue(claimed);
            Assert.IsTrue(_mgr.IsPremiumRewardClaimed(0));
        }
    }
}
