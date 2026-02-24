using AscendantContinuum.Core;
using NUnit.Framework;
using UnityEngine;

namespace AscendantContinuum.Tests.EditMode
{
    public class AccessibilityManagerEditModeTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteAll();
        }

        [Test]
        public void SetHaptics_DisablesHaptics_WhenFalseIsProvided()
        {
            var gameObject = new GameObject("AccessibilityManager_Test");
            var manager = gameObject.AddComponent<AccessibilityManager>();

            manager.SetHaptics(false);

            Assert.IsFalse(manager.HapticsEnabled);

            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void UiCompatibilityMethods_UpdateUnderlyingAccessibilityState()
        {
            var gameObject = new GameObject("AccessibilityManager_UI_Test");
            var manager = gameObject.AddComponent<AccessibilityManager>();

            manager.SetColorblindMode("Deuteranopia");
            manager.SetHighContrast(true);
            manager.SetTextScale(1.5f);
            manager.SetHapticsEnabled(false);
            manager.SetHapticIntensity(0.35f);

            Assert.AreEqual(ColorblindMode.Deuteranopia, manager.CurrentColorblindMode);
            Assert.IsTrue(manager.IsHighContrastEnabled());
            Assert.AreEqual(1.5f, manager.GetTextScale());
            Assert.IsFalse(manager.HapticsEnabled);

            Object.DestroyImmediate(gameObject);
        }
    }
}
