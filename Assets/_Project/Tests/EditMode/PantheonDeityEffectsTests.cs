using NUnit.Framework;
using UnityEngine;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="PantheonDeityEffects"/> quiz calculation and Day 4 gate.
    /// </summary>
    public class PantheonDeityEffectsTests
    {
        private const string SESSION_KEY = "SessionCount_Total";
        private const string PREF_QUIZ   = "Pantheon_QuizCompleted";
        private const string PREF_DEITY  = "Pantheon_ActiveDeity";

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(SESSION_KEY);
            PlayerPrefs.DeleteKey(PREF_QUIZ);
            PlayerPrefs.SetInt(PREF_DEITY, -1);  // no deity
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(SESSION_KEY);
            PlayerPrefs.DeleteKey(PREF_QUIZ);
            PlayerPrefs.SetInt(PREF_DEITY, -1);
        }

        // ── CalculateQuizResult ────────────────────────────────────────────

        [Test]
        public void QuizResult_AllZero_ReturnsFlame()
        {
            int result = PantheonDeityEffects.CalculateQuizResult(new[] { 0, 0, 0, 0, 0 });
            Assert.That(result, Is.EqualTo(PantheonDeityEffects.DEITY_FLAME));
        }

        [Test]
        public void QuizResult_AllScribe_ReturnsScribe()
        {
            int result = PantheonDeityEffects.CalculateQuizResult(new[] { 4, 4, 4, 4, 4 });
            Assert.That(result, Is.EqualTo(PantheonDeityEffects.DEITY_SCRIBE));
        }

        [Test]
        public void QuizResult_MajorityHerald_ReturnsHerald()
        {
            // 3 Herald votes vs 1 each of others
            int result = PantheonDeityEffects.CalculateQuizResult(
                new[] { 1, 1, 1, 0, 2 });
            Assert.That(result, Is.EqualTo(PantheonDeityEffects.DEITY_HERALD));
        }

        [Test]
        public void QuizResult_NullInput_ReturnsFlame()
        {
            int result = PantheonDeityEffects.CalculateQuizResult(null);
            Assert.That(result, Is.EqualTo(PantheonDeityEffects.DEITY_FLAME));
        }

        [Test]
        public void QuizResult_EmptyInput_ReturnsFlame()
        {
            int result = PantheonDeityEffects.CalculateQuizResult(new int[0]);
            Assert.That(result, Is.EqualTo(PantheonDeityEffects.DEITY_FLAME));
        }

        // ── Day 4 unlock gate ──────────────────────────────────────────────

        [Test]
        public void IsDay4UnlockAvailable_FalseBeforeFourSessions()
        {
            var go  = new GameObject();
            var mgr = go.AddComponent<PantheonDeityEffects>();

            PlayerPrefs.SetInt(SESSION_KEY, 3);
            Assert.IsFalse(mgr.IsDay4UnlockAvailable());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void IsDay4UnlockAvailable_TrueAtFourSessions()
        {
            var go  = new GameObject();
            var mgr = go.AddComponent<PantheonDeityEffects>();

            PlayerPrefs.SetInt(SESSION_KEY, 4);
            Assert.IsTrue(mgr.IsDay4UnlockAvailable());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ShouldShowPantheonQuiz_FalseIfQuizAlreadyDone()
        {
            var go  = new GameObject();
            var mgr = go.AddComponent<PantheonDeityEffects>();

            PlayerPrefs.SetInt(SESSION_KEY, 10);
            PlayerPrefs.SetInt(PREF_QUIZ, 1);   // quiz completed flag
            Assert.IsFalse(mgr.ShouldShowPantheonQuiz());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ShouldShowPantheonQuiz_TrueOnDay4WithNoDeityAndNoQuiz()
        {
            var go  = new GameObject();
            var mgr = go.AddComponent<PantheonDeityEffects>();

            PlayerPrefs.SetInt(SESSION_KEY, 4);
            PlayerPrefs.SetInt(PREF_QUIZ, 0);
            PlayerPrefs.SetInt(PREF_DEITY, -1); // no deity

            Assert.IsTrue(mgr.ShouldShowPantheonQuiz());

            Object.DestroyImmediate(go);
        }
    }
}
