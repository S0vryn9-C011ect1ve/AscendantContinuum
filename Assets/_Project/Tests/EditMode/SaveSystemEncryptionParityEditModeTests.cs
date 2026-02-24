using System;
using System.IO;
using System.Reflection;
using AscendantContinuum.Core;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AscendantContinuum.Tests.EditMode
{
    public class SaveSystemEncryptionParityEditModeTests
    {
        [Test]
        public void EncryptedAndPlaintext_SaveLoadRoundTrip_ProduceEquivalentData()
        {
            var plainData = SaveAndLoadSnapshot(false, "echo_fields", 77, 5);
            var encryptedData = SaveAndLoadSnapshot(true, "echo_fields", 77, 5);

            Assert.AreEqual(plainData.currentRealm, encryptedData.currentRealm);
            Assert.AreEqual(plainData.sparksCollected, encryptedData.sparksCollected);
            Assert.AreEqual(plainData.sigilsCollected, encryptedData.sigilsCollected);
        }

        [Test]
        public void EncryptedSave_DoesNotContainPlaintextRealmNameInFile()
        {
            string tempSavePath = Path.Combine(Path.GetTempPath(), $"ascendant_encrypted_{Guid.NewGuid():N}.dat");
            var gameObject = new GameObject("SaveSystem_Encrypted_Test");
            var saveSystem = gameObject.AddComponent<SaveSystem>();

            ConfigureSaveSystem(saveSystem, tempSavePath, true);
            saveSystem.UpdateProgressSnapshot("lantern_ascension", 10, 1);

            string fileText = File.ReadAllText(tempSavePath);
            Assert.IsFalse(fileText.Contains("lantern_ascension"));

            if (File.Exists(tempSavePath))
                File.Delete(tempSavePath);

            Object.DestroyImmediate(gameObject);
        }

        private static PlayerData SaveAndLoadSnapshot(bool encrypt, string realm, int sparks, int sigils)
        {
            string tempSavePath = Path.Combine(Path.GetTempPath(), $"ascendant_{(encrypt ? "enc" : "plain")}_{Guid.NewGuid():N}.dat");
            var gameObject = new GameObject($"SaveSystem_{(encrypt ? "Enc" : "Plain")}_Test");
            var saveSystem = gameObject.AddComponent<SaveSystem>();

            ConfigureSaveSystem(saveSystem, tempSavePath, encrypt);
            saveSystem.UpdateProgressSnapshot(realm, sparks, sigils);
            saveSystem.LoadGame();

            PlayerData loaded = saveSystem.CurrentPlayerData;

            if (File.Exists(tempSavePath))
                File.Delete(tempSavePath);

            Object.DestroyImmediate(gameObject);
            return loaded;
        }

        private static void ConfigureSaveSystem(SaveSystem saveSystem, string savePath, bool encrypt)
        {
            FieldInfo savePathField = typeof(SaveSystem).GetField("savePath", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo encryptField = typeof(SaveSystem).GetField("encryptSaveData", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(savePathField);
            Assert.IsNotNull(encryptField);

            savePathField.SetValue(saveSystem, savePath);
            encryptField.SetValue(saveSystem, encrypt);
        }
    }
}
