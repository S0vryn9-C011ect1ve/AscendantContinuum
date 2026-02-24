using AscendantContinuum.Core;
using NUnit.Framework;
using UnityEngine;

namespace AscendantContinuum.Tests.EditMode
{
    public class GameManagerOnboardingEditModeTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void ShouldRunOnboarding_IsTrueUntilMarkedComplete()
        {
            var gameObject = new GameObject("GameManager_Onboarding_Test");
            var manager = gameObject.AddComponent<GameManager>();

            Assert.IsTrue(manager.ShouldRunOnboarding());

            manager.MarkOnboardingCompleted();

            Assert.IsFalse(manager.ShouldRunOnboarding());

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void GetLastRealmOrDefault_ReturnsSavedRealmWhenPresent()
        {
            PlayerPrefs.SetString("LastRealm", "EchoFields");
            PlayerPrefs.Save();

            var gameObject = new GameObject("GameManager_LastRealm_Test");
            var manager = gameObject.AddComponent<GameManager>();

            string realm = manager.GetLastRealmOrDefault("Emberforge");

            Assert.AreEqual("EchoFields", realm);

            Object.DestroyImmediate(gameObject);
        }
    }
}
