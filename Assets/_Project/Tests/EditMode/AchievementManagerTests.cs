using NUnit.Framework;
using UnityEngine;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="AchievementManager"/> unlock, progress tracking, and persistence.
    /// </summary>
    public class AchievementManagerTests
    {
        private GameObject       _go;
        private AchievementManager _mgr;

        [SetUp]
        public void SetUp()
        {
            // Destroy any singleton left from a prior test
            var existing = Object.FindFirstObjectByType<AchievementManager>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Clear persisted state so tests are isolated
            PlayerPrefs.DeleteKey("Achievements_Unlocked");

            _go  = new GameObject("AchievementManager_Test");
            _mgr = _go.AddComponent<AchievementManager>();
            // Awake() runs automatically and calls InitializeAchievements()
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            PlayerPrefs.DeleteKey("Achievements_Unlocked");
        }

        // ── Initial state ──────────────────────────────────────────────────

        [Test]
        public void GetUnlockedCount_ZeroOnInit()
        {
            Assert.That(_mgr.GetUnlockedCount(), Is.EqualTo(0));
        }

        [Test]
        public void GetTotalAchievementCount_GreaterThanZero()
        {
            Assert.That(_mgr.GetTotalAchievementCount(), Is.GreaterThan(0),
                "Achievement list should be populated after init.");
        }

        [Test]
        public void GetCompletionPercentage_ZeroOnInit()
        {
            Assert.That(_mgr.GetCompletionPercentage(), Is.EqualTo(0f));
        }

        // ── UnlockAchievement ──────────────────────────────────────────────

        [Test]
        public void UnlockAchievement_KnownId_IncreasesCount()
        {
            _mgr.UnlockAchievement("first_spark");
            Assert.That(_mgr.GetUnlockedCount(), Is.EqualTo(1));
        }

        [Test]
        public void UnlockAchievement_DuplicateCall_DoesNotDoubleCount()
        {
            _mgr.UnlockAchievement("first_spark");
            _mgr.UnlockAchievement("first_spark");
            Assert.That(_mgr.GetUnlockedCount(), Is.EqualTo(1),
                "Unlocking the same achievement twice should not duplicate it.");
        }

        [Test]
        public void UnlockAchievement_UnknownId_DoesNotCrash()
        {
            Assert.DoesNotThrow(() => _mgr.UnlockAchievement("nonexistent_achievement_xyz"));
        }

        [Test]
        public void UnlockAchievement_FiresEvent()
        {
            Achievement received = null;
            _mgr.OnAchievementUnlocked += a => received = a;
            _mgr.UnlockAchievement("first_spark");

            Assert.IsNotNull(received, "OnAchievementUnlocked should fire.");
            Assert.That(received.id, Is.EqualTo("first_spark"));
        }

        // ── TrackProgress ──────────────────────────────────────────────────

        [Test]
        public void TrackProgress_BelowTarget_DoesNotUnlock()
        {
            // lunar_devotee needs progress 8
            _mgr.TrackProgress("lunar_devotee", 4);
            Assert.That(_mgr.GetUnlockedCount(), Is.EqualTo(0),
                "Achievement should not unlock before reaching target progress.");
        }

        [Test]
        public void TrackProgress_AtTarget_UnlocksAchievement()
        {
            // lunar_devotee targetProgress = 8
            _mgr.TrackProgress("lunar_devotee", 8);
            Assert.That(_mgr.GetUnlockedCount(), Is.EqualTo(1),
                "Achievement should unlock when progress reaches target.");
        }
    }
}
