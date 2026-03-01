using NUnit.Framework;
using UnityEngine;
using AscendantContinuum.Progression;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="ProgressionManager"/> realm unlock / mastery / prestige.
    /// </summary>
    public class ProgressionManagerTests
    {
        private GameObject     _go;
        private ProgressionManager _mgr;

        [SetUp]
        public void SetUp()
        {
            // Destroy any existing singleton left from a previous test
            var existing = Object.FindFirstObjectByType<ProgressionManager>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            _go  = new GameObject("ProgressionManager_Test");
            _mgr = _go.AddComponent<ProgressionManager>();
            _mgr.LoadProgression(); // Start() not called in EditMode
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        // ── Initial state ──────────────────────────────────────────────────

        [Test]
        public void EmberforgeUnlocked_OnInit()
        {
            Assert.IsTrue(_mgr.IsRealmUnlocked("emberforge"),
                "Emberforge should be unlocked at game start.");
        }

        [Test]
        public void OtherRealms_LockedOnInit()
        {
            Assert.IsFalse(_mgr.IsRealmUnlocked("verdant"),  "Verdant should be locked");
            Assert.IsFalse(_mgr.IsRealmUnlocked("echo"),     "Echo should be locked");
            Assert.IsFalse(_mgr.IsRealmUnlocked("dawn"),     "Dawn should be locked");
            Assert.IsFalse(_mgr.IsRealmUnlocked("lantern"),  "Lantern should be locked");
        }

        [Test]
        public void PrestigePoints_ZeroOnInit()
        {
            Assert.That(_mgr.GetPrestigePoints(), Is.EqualTo(0));
        }

        // ── Realm completion & unlock ──────────────────────────────────────

        [Test]
        public void VerdantUnlocks_After5EmberforgeCompletions()
        {
            for (int i = 0; i < 5; i++)
                _mgr.RecordRealmCompletion("emberforge", 100, 60f);

            Assert.IsTrue(_mgr.IsRealmUnlocked("verdant"),
                "Verdant should unlock after 5 Emberforge completions.");
        }

        [Test]
        public void FourEmberforgeCompletions_DoNotUnlockVerdant()
        {
            for (int i = 0; i < 4; i++)
                _mgr.RecordRealmCompletion("emberforge", 100, 60f);

            Assert.IsFalse(_mgr.IsRealmUnlocked("verdant"),
                "4 completions should not unlock the next realm.");
        }

        [Test]
        public void RealmMastery_UnlockedAfter5Completions()
        {
            for (int i = 0; i < 5; i++)
                _mgr.RecordRealmCompletion("emberforge", 100, 60f);

            Assert.IsTrue(_mgr.IsRealmMastered("emberforge"),
                "Realm should be mastered after 5 completions.");
        }

        // ── Sigil mastery ──────────────────────────────────────────────────

        [Test]
        public void SigilMastery_AfterTenCollections()
        {
            for (int i = 0; i < 10; i++)
                _mgr.RecordSigilObtained("flame_sigil");

            Assert.That(_mgr.GetSigilMasteryCount(), Is.EqualTo(1),
                "Sigil should be mastered after 10 collections.");
        }

        // ── Prestige ───────────────────────────────────────────────────────

        [Test]
        public void Prestige_GrantsPointsAndResetsCompletions()
        {
            // First master the realm
            for (int i = 0; i < 5; i++)
                _mgr.RecordRealmCompletion("emberforge", 100, 60f);

            int completionsBefore = _mgr.GetRealmCompletions("emberforge");
            _mgr.ExecutePrestige("emberforge");

            Assert.That(_mgr.GetPrestigePoints(), Is.GreaterThan(0),
                "Prestige should award points.");
            Assert.That(_mgr.GetRealmCompletions("emberforge"), Is.EqualTo(0),
                "Prestige should reset completion count.");
            Assert.That(completionsBefore, Is.GreaterThan(0)); // guard
        }
    }
}
