using System;
using NUnit.Framework;
using UnityEngine;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="DailyChallengeManager.GenerateShareText"/> format.
    /// </summary>
    public class DailyChallengeShareCardTests
    {
        private GameObject           _go;
        private DailyChallengeManager _mgr;

        // Use a fixed date so challenge generation is fully deterministic
        private static readonly DateTime FIXED_DATE =
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public void SetUp()
        {
            var existing = UnityEngine.Object.FindFirstObjectByType<DailyChallengeManager>();
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

            PlayerPrefs.DeleteKey("DailyChallenge_Data");
            PlayerPrefs.DeleteKey("DailyChallenge_Date");
            PlayerPrefs.DeleteKey("DailyChallenge_Streak");

            _go  = new GameObject("DailyChallengeManager_Test");
            _mgr = _go.AddComponent<DailyChallengeManager>();
            // Generate a deterministic challenge without saving or notifying
            _mgr.GenerateChallengeForDate(FIXED_DATE, false, false);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) UnityEngine.Object.DestroyImmediate(_go);
            PlayerPrefs.DeleteKey("DailyChallenge_Data");
            PlayerPrefs.DeleteKey("DailyChallenge_Date");
            PlayerPrefs.DeleteKey("DailyChallenge_Streak");
        }

        // ── Header ────────────────────────────────────────────────────────

        [Test]
        public void ShareText_ContainsBrandName()
        {
            string text = _mgr.GenerateShareText();
            Assert.That(text, Does.Contain("Ascendant Continuum"),
                "Share card must include the game name.");
        }

        [Test]
        public void ShareText_ContainsDailyRitualLabel()
        {
            string text = _mgr.GenerateShareText();
            Assert.That(text, Does.Contain("Daily Ritual"),
                "Share card must reference the daily ritual.");
        }

        // ── Progress bar ──────────────────────────────────────────────────

        [Test]
        public void ShareText_ContainsProgressBlocks_WhenIncomplete()
        {
            string text = _mgr.GenerateShareText();
            // The progress bar uses ⬛ for unfilled blocks — should appear if not completed
            Assert.That(text, Does.Contain("\u2b1b").Or.Contains("\u2728"),
                "Share card should contain progress bar blocks.");
        }

        [Test]
        public void ShareText_ContainsCompleted_WhenChallengeFinished()
        {
            // Drive progress to completion
            var challenge = _mgr.CurrentChallenge;
            if (challenge != null)
            {
                _mgr.IncrementChallengeProgress(challenge.type, challenge.RequiredProgress);
            }

            string text = _mgr.GenerateShareText();
            Assert.That(text, Does.Contain("COMPLETED"),
                "Share card should say COMPLETED when challenge is done.");
        }

        // ── URL ───────────────────────────────────────────────────────────

        [Test]
        public void ShareText_ContainsSiteUrl()
        {
            string text = _mgr.GenerateShareText();
            Assert.That(text, Does.Contain("ascendantcontinuum"),
                "Share card should include the game URL.");
        }

        // ── Streak line ───────────────────────────────────────────────────

        [Test]
        public void ShareText_NoStreakLine_WhenStreakZero()
        {
            string text = _mgr.GenerateShareText();
            // Streak line only appears when consecutiveDaysCompleted > 0
            // On a fresh setup, streak = 0, so "Streak:" should be absent
            Assert.That(text, Does.Not.Contain("Streak:"),
                "No streak line for 0-day streak.");
        }

        // ── Null guard ────────────────────────────────────────────────────

        [Test]
        public void ShareText_WhenNoChallengeSet_ReturnsFallback()
        {
            // Reset so no challenge is active
            var freshGo  = new GameObject("DC_NoChallenge");
            var freshMgr = freshGo.AddComponent<DailyChallengeManager>();
            // Don't call GenerateChallengeForDate — currentChallenge stays null

            string text = freshMgr.GenerateShareText();
            Assert.IsNotNull(text, "Share text must not be null even with no challenge.");
            Assert.IsNotEmpty(text);

            UnityEngine.Object.DestroyImmediate(freshGo);
        }
    }
}
