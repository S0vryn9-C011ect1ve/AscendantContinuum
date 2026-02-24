using System;
using System.IO;
using System.Reflection;
using AscendantContinuum.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AscendantContinuum.Tests.EditMode
{
    public class SaveSystemProgressRoundTripEditModeTests
    {
        [Test]
        public void UpdateProgressSnapshot_PersistsRealmSparksAndSigilsAcrossLoad()
        {
            string tempSavePath = Path.Combine(Path.GetTempPath(), $"ascendant_progress_{Guid.NewGuid():N}.dat");

            var gameObject = new GameObject("SaveSystem_Progress_Test");
            var saveSystem = gameObject.AddComponent<SaveSystem>();

            var savePathField = typeof(SaveSystem).GetField("savePath", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(savePathField);
            savePathField.SetValue(saveSystem, tempSavePath);

            saveSystem.UpdateProgressSnapshot("dawn_citadel", 120, 8);
            saveSystem.LoadGame();

            Assert.AreEqual("dawn_citadel", saveSystem.CurrentPlayerData.currentRealm);
            Assert.AreEqual(120, saveSystem.CurrentPlayerData.sparksCollected);
            Assert.AreEqual(8, saveSystem.CurrentPlayerData.sigilsCollected);

            if (File.Exists(tempSavePath))
                File.Delete(tempSavePath);

            Object.DestroyImmediate(gameObject);
        }
    }
}
