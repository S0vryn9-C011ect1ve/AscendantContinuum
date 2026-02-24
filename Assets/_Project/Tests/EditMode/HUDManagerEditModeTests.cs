using AscendantContinuum.Core;
using AscendantContinuum.UI;
using NUnit.Framework;
using UnityEngine;

namespace AscendantContinuum.Tests.EditMode
{
    public class HUDManagerEditModeTests
    {
        [Test]
        public void SyncFromPlayerData_UpdatesHudStateFromSavedProgress()
        {
            var gameObject = new GameObject("HUDManager_Test");
            var hudManager = gameObject.AddComponent<HUDManager>();

            var data = new PlayerData
            {
                currentRealm = "verdant_sanctuary",
                sparksCollected = 55,
                sigilsCollected = 4
            };

            hudManager.SyncFromPlayerData(data);

            Assert.AreEqual(55, hudManager.CurrentSparks);
            Assert.AreEqual(4, hudManager.CurrentSigils);
            Assert.AreEqual("verdant_sanctuary", hudManager.CurrentRealmName);

            Object.DestroyImmediate(gameObject);
        }
    }
}
