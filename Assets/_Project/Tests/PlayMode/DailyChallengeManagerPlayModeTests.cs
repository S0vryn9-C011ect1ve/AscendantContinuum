using System;
using System.Collections;
using System.Collections.Generic;
using AscendantContinuum.Systems;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.TestTools;

namespace AscendantContinuum.Tests.PlayMode
{
    public class DailyChallengeManagerPlayModeTests
    {
        private GameObject accessibilityGameObject;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [TearDown]
        public void TearDown()
        {
            if (accessibilityGameObject != null)
            {
                Object.Destroy(accessibilityGameObject);
                accessibilityGameObject = null;
            }
        }

        [UnityTest]
        public IEnumerator Start_ResetsStreak_WhenPreviousCompletionWasMoreThanOneDayAgo()
        {
            PlayerPrefs.SetString("DailyChallenge_Date", DateTime.UtcNow.Date.AddDays(-2).ToString());
            PlayerPrefs.SetInt("DailyChallenge_Streak", 5);
            PlayerPrefs.Save();

            var gameObject = new GameObject("DailyChallengeManager_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            Assert.AreEqual(0, manager.StreakDays);

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator GenerateChallengeForDate_IsDeterministic_ForSameDateAndState()
        {
            var gameObject = new GameObject("DailyChallengeManager_Determinism_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            DateTime fixedDate = new DateTime(2026, 2, 21, 13, 0, 0, DateTimeKind.Utc);
            DailyChallenge first = manager.GenerateChallengeForDate(fixedDate, false, false);
            DailyChallenge second = manager.GenerateChallengeForDate(fixedDate, false, false);

            Assert.AreEqual(first.challengeId, second.challengeId);
            Assert.AreEqual(first.type, second.type);
            Assert.AreEqual(first.targetValue, second.targetValue);

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator GetAvailableChallengeTypes_UsesAccessibilityState()
        {
            accessibilityGameObject = new GameObject("AccessibilityManager_Test");
            var accessibilityManager = accessibilityGameObject.AddComponent<AscendantContinuum.Core.AccessibilityManager>();

            var gameObject = new GameObject("DailyChallengeManager_Accessibility_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            accessibilityManager.SetReducedMotion(true);
            accessibilityManager.SetColorblindMode(AscendantContinuum.Core.ColorblindMode.None);

            List<ChallengeType> reducedMotionTypes = manager.GetAvailableChallengeTypes();
            Assert.Contains(ChallengeType.MeditateInRealm, reducedMotionTypes);
            Assert.IsFalse(reducedMotionTypes.Contains(ChallengeType.CompleteRitualsFast));

            accessibilityManager.SetReducedMotion(false);
            accessibilityManager.SetColorblindMode(AscendantContinuum.Core.ColorblindMode.Deuteranopia);

            List<ChallengeType> colorblindTypes = manager.GetAvailableChallengeTypes();
            Assert.Contains(ChallengeType.CompleteRitualsFast, colorblindTypes);
            Assert.Contains(ChallengeType.FindColorblindSecret, colorblindTypes);

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator ResetProgress_ClearsChallengeAndStreak()
        {
            var gameObject = new GameObject("DailyChallengeManager_Reset_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            manager.GenerateChallengeForDate(new DateTime(2026, 2, 21, 13, 0, 0, DateTimeKind.Utc), true, false);
            manager.IncrementChallengeProgress(manager.CurrentChallenge.type, manager.CurrentChallenge.targetValue);

            Assert.Greater(manager.StreakDays, 0);
            Assert.IsNotNull(manager.CurrentChallenge);

            manager.ResetProgress();

            Assert.AreEqual(0, manager.StreakDays);
            Assert.IsNull(manager.CurrentChallenge);

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator GenerateChallengeForDate_IsDeterministic_AcrossDateAccessibilityMatrix()
        {
            accessibilityGameObject = new GameObject("AccessibilityManager_Matrix_Test");
            var accessibilityManager = accessibilityGameObject.AddComponent<AscendantContinuum.Core.AccessibilityManager>();

            var gameObject = new GameObject("DailyChallengeManager_Matrix_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            var matrix = new (DateTime date, bool reducedMotion, AscendantContinuum.Core.ColorblindMode mode)[]
            {
                (new DateTime(2026, 2, 21, 12, 0, 0, DateTimeKind.Utc), false, AscendantContinuum.Core.ColorblindMode.None),
                (new DateTime(2026, 2, 22, 12, 0, 0, DateTimeKind.Utc), true, AscendantContinuum.Core.ColorblindMode.None),
                (new DateTime(2026, 2, 23, 12, 0, 0, DateTimeKind.Utc), false, AscendantContinuum.Core.ColorblindMode.Protanopia),
                (new DateTime(2026, 2, 24, 12, 0, 0, DateTimeKind.Utc), true, AscendantContinuum.Core.ColorblindMode.Tritanopia)
            };

            foreach (var entry in matrix)
            {
                accessibilityManager.SetReducedMotion(entry.reducedMotion);
                accessibilityManager.SetColorblindMode(entry.mode);

                DailyChallenge first = manager.GenerateChallengeForDate(entry.date, false, false);
                DailyChallenge second = manager.GenerateChallengeForDate(entry.date, false, false);
                List<ChallengeType> pool = manager.GetAvailableChallengeTypes();

                Assert.AreEqual(first.challengeId, second.challengeId);
                Assert.AreEqual(first.type, second.type);
                Assert.AreEqual(first.targetValue, second.targetValue);
                Assert.AreEqual($"daily_{entry.date:yyyyMMdd}", first.challengeId);
                Assert.Contains(first.type, pool);
            }

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator GenerateChallengeForDate_UsesCurrentStreakForReward()
        {
            PlayerPrefs.SetString("DailyChallenge_Date", DateTime.UtcNow.Date.ToString());
            PlayerPrefs.SetInt("DailyChallenge_Streak", 3);
            PlayerPrefs.Save();

            var gameObject = new GameObject("DailyChallengeManager_Reward_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            DailyChallenge challenge = manager.GenerateChallengeForDate(new DateTime(2026, 2, 25, 12, 0, 0, DateTimeKind.Utc), false, false);

            Assert.AreEqual(80, challenge.rewardSparks);

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator GenerateChallengeForDate_SaveProgress_StoresChallengeDateAsSourceOfTruth()
        {
            var gameObject = new GameObject("DailyChallengeManager_SaveDate_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            DateTime fixedDate = new DateTime(2026, 2, 20, 12, 0, 0, DateTimeKind.Utc);
            manager.GenerateChallengeForDate(fixedDate, true, false);

            string savedDate = PlayerPrefs.GetString("DailyChallenge_Date", string.Empty);
            Assert.AreEqual(fixedDate.Date.ToString(), savedDate);

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator DailyChallenge_ExposesUiFacingProperties_AndManagerAccessors()
        {
            var gameObject = new GameObject("DailyChallengeManager_UI_Api_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            DailyChallenge challenge = manager.GenerateChallengeForDate(new DateTime(2026, 2, 26, 12, 0, 0, DateTimeKind.Utc), false, false);

            Assert.IsNotNull(manager.GetTodayChallenge());
            Assert.AreEqual(manager.StreakDays, manager.GetCurrentStreak());
            Assert.AreEqual(challenge.GetDescription(), challenge.Title);
            Assert.AreEqual(challenge.currentProgress, challenge.CurrentProgress);
            Assert.AreEqual(challenge.targetValue, challenge.RequiredProgress);
            Assert.AreEqual("Emberforge", challenge.RealmName);

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator SaveLoadRoundTrip_RestoresChallengeProgressAndStreak_AfterManagerRecreation()
        {
            DateTime today = DateTime.UtcNow.Date.AddHours(12);

            var firstManagerGo = new GameObject("DailyChallengeManager_RoundTrip_First");
            var firstManager = firstManagerGo.AddComponent<DailyChallengeManager>();

            yield return null;

            DailyChallenge original = firstManager.GenerateChallengeForDate(today, true, false);
            firstManager.IncrementChallengeProgress(original.type, 2);

            int originalProgress = firstManager.CurrentChallenge.currentProgress;
            int originalTarget = firstManager.CurrentChallenge.targetValue;
            ChallengeType originalType = firstManager.CurrentChallenge.type;
            string originalChallengeId = firstManager.CurrentChallenge.challengeId;

            firstManager.IncrementChallengeProgress(original.type, originalTarget);
            int originalStreak = firstManager.StreakDays;

            Object.Destroy(firstManagerGo);
            yield return null;

            var secondManagerGo = new GameObject("DailyChallengeManager_RoundTrip_Second");
            var secondManager = secondManagerGo.AddComponent<DailyChallengeManager>();

            yield return null;

            Assert.IsNotNull(secondManager.CurrentChallenge);
            Assert.AreEqual(originalChallengeId, secondManager.CurrentChallenge.challengeId);
            Assert.AreEqual(originalType, secondManager.CurrentChallenge.type);
            Assert.AreEqual(originalTarget, secondManager.CurrentChallenge.targetValue);
            Assert.GreaterOrEqual(secondManager.CurrentChallenge.currentProgress, originalProgress);
            Assert.AreEqual(originalStreak, secondManager.StreakDays);

            Object.Destroy(secondManagerGo);
        }

        [UnityTest]
        public IEnumerator GenerateChallengeForDate_NotifyFlagControlsOnNewChallengeEvent()
        {
            var gameObject = new GameObject("DailyChallengeManager_EventNotify_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            int eventCount = 0;
            DailyChallenge lastPayload = null;
            manager.OnNewChallengeAvailable += challenge =>
            {
                eventCount++;
                lastPayload = challenge;
            };

            DateTime firstDate = new DateTime(2026, 2, 27, 12, 0, 0, DateTimeKind.Utc);
            manager.GenerateChallengeForDate(firstDate, false, false);
            Assert.AreEqual(0, eventCount);

            DateTime secondDate = new DateTime(2026, 2, 28, 12, 0, 0, DateTimeKind.Utc);
            DailyChallenge notified = manager.GenerateChallengeForDate(secondDate, false, true);

            Assert.AreEqual(1, eventCount);
            Assert.IsNotNull(lastPayload);
            Assert.AreEqual(notified.challengeId, lastPayload.challengeId);

            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator CompletingChallenge_EmitsRewardThroughOnChallengeCompleted()
        {
            var gameObject = new GameObject("DailyChallengeManager_EventComplete_Test");
            var manager = gameObject.AddComponent<DailyChallengeManager>();

            yield return null;

            int eventCount = 0;
            int lastReward = -1;
            manager.OnChallengeCompleted += reward =>
            {
                eventCount++;
                lastReward = reward;
            };

            DailyChallenge challenge = manager.GenerateChallengeForDate(new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc), false, false);
            manager.IncrementChallengeProgress(challenge.type, challenge.targetValue);

            Assert.AreEqual(1, eventCount);
            Assert.AreEqual(challenge.rewardSparks, lastReward);

            Object.Destroy(gameObject);
        }
    }
}
