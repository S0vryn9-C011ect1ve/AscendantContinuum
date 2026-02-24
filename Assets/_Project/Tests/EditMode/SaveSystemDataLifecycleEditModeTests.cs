using System;
using System.IO;
using System.Reflection;
using AscendantContinuum.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AscendantContinuum.Tests.EditMode
{
    public class SaveSystemDataLifecycleEditModeTests
    {
        [Test]
        public void DeleteSaveData_RemovesSaveFile_AndResetsPlayerData()
        {
            var saveFilePath = Path.Combine(Path.GetTempPath(), $"ascendant_test_{Guid.NewGuid():N}.dat");
            File.WriteAllText(saveFilePath, "test-save-content");

            var gameObject = new GameObject("SaveSystem_Delete_Test");
            var saveSystem = gameObject.AddComponent<SaveSystem>();

            var savePathField = typeof(SaveSystem).GetField("savePath", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(savePathField);
            savePathField.SetValue(saveSystem, saveFilePath);

            saveSystem.DeleteSaveData();

            Assert.IsFalse(File.Exists(saveFilePath));
            Assert.IsNotNull(saveSystem.CurrentPlayerData);
            Assert.AreEqual("emberforge", saveSystem.CurrentPlayerData.currentRealm);

            Object.DestroyImmediate(gameObject);
        }
    }
}
