using AscendantContinuum.Core;
using NUnit.Framework;
using UnityEngine;

namespace AscendantContinuum.Tests.EditMode
{
    public class SaveSystemEditModeTests
    {
        [Test]
        public void Awake_InitializesPlayerData_WhenNoSaveHasBeenLoadedYet()
        {
            var gameObject = new GameObject("SaveSystem_Test");
            var saveSystem = gameObject.AddComponent<SaveSystem>();

            Assert.IsNotNull(saveSystem.CurrentPlayerData);

            Object.DestroyImmediate(gameObject);
        }
    }
}
