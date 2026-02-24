using AscendantContinuum.UI;
using NUnit.Framework;
using UnityEngine;

namespace AscendantContinuum.Tests.EditMode
{
    public class MainMenuPlayFlowEditModeTests
    {
        [Test]
        public void ResolveInitialPlayScene_PrefersOnboardingWhenNeededAndAvailable()
        {
            var gameObject = new GameObject("MainMenu_Flow_Test");
            var manager = gameObject.AddComponent<MainMenuManager>();

            string scene = manager.ResolveInitialPlayScene(
                shouldRunOnboarding: true,
                lastRealm: "Emberforge",
                onboardingSceneName: "Onboarding_Scene",
                onboardingSceneAvailable: true);

            Assert.AreEqual("Onboarding_Scene", scene);

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void ResolveInitialPlayScene_FallsBackToLastRealmWhenOnboardingUnavailable()
        {
            var gameObject = new GameObject("MainMenu_Fallback_Test");
            var manager = gameObject.AddComponent<MainMenuManager>();

            string scene = manager.ResolveInitialPlayScene(
                shouldRunOnboarding: true,
                lastRealm: "VerdantSanctuary",
                onboardingSceneName: "Onboarding_Scene",
                onboardingSceneAvailable: false);

            Assert.AreEqual("VerdantSanctuary", scene);

            Object.DestroyImmediate(gameObject);
        }
    }
}
