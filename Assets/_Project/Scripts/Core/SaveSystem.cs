using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Local save system with AES-256 encryption for security
    /// Handles player progress, settings, and offline data
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool encryptSaveData = true;
        [SerializeField] private string saveFileName = "ascendant_save.dat";
        
        private string savePath;
        private PlayerData currentPlayerData;
        
        // Encryption key (in production, generate per-device or use keychain)
        private readonly byte[] encryptionKey = Encoding.UTF8.GetBytes("AscendantContinuum32BytesKeyHere"); // 32 bytes for AES-256
        private readonly byte[] encryptionIV = Encoding.UTF8.GetBytes("16BytesIVHere123"); // 16 bytes IV

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            Debug.Log($"[SaveSystem] Save path: {savePath}");
        }

        private void Start()
        {
            LoadGame();
        }

        public void SaveGame()
        {
            try
            {
                // Gather data from managers
                currentPlayerData = new PlayerData
                {
                    // Game progress
                    currentRealm = GameManager.Instance?.CurrentRealm ?? "emberforge",
                    sparksCollected = FindObjectOfType<Emberforge.EmberforgeSparks>()?.SparksCollected ?? 0,
                    
                    // Accessibility settings
                    colorblindMode = (int)(AccessibilityManager.Instance?.CurrentColorblindMode ?? 0),
                    reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled ?? false,
                    hapticsEnabled = AccessibilityManager.Instance?.HapticsEnabled ?? true,
                    
                    // Meta
                    lastSaveTime = DateTime.UtcNow.ToString("o"),
                    version = Application.version
                };
                
                string json = JsonUtility.ToJson(currentPlayerData, true);
                
                if (encryptSaveData)
                {
                    byte[] encryptedData = EncryptString(json);
                    File.WriteAllBytes(savePath, encryptedData);
                }
                else
                {
                    File.WriteAllText(savePath, json);
                }
                
                Debug.Log("[SaveSystem] ✅ Game saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] ❌ Save failed: {e.Message}");
            }
        }

        public void LoadGame()
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    Debug.Log("[SaveSystem] No save file found - creating new player data");
                    currentPlayerData = new PlayerData();
                    return;
                }
                
                string json;
                
                if (encryptSaveData)
                {
                    byte[] encryptedData = File.ReadAllBytes(savePath);
                    json = DecryptString(encryptedData);
                }
                else
                {
                    json = File.ReadAllText(savePath);
                }
                
                currentPlayerData = JsonUtility.FromJson<PlayerData>(json);
                
                Debug.Log($"[SaveSystem] ✅ Game loaded - Last save: {currentPlayerData.lastSaveTime}");
                
                // Apply loaded settings
                ApplyLoadedData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] ❌ Load failed: {e.Message}");
                currentPlayerData = new PlayerData();
            }
        }

        private void ApplyLoadedData()
        {
            // Restore accessibility settings
            AccessibilityManager.Instance?.SetColorblindMode((AccessibilityManager.ColorblindMode)currentPlayerData.colorblindMode);
            AccessibilityManager.Instance?.SetReducedMotion(currentPlayerData.reducedMotion);
            AccessibilityManager.Instance?.SetHaptics(currentPlayerData.hapticsEnabled);
            
            Debug.Log("[SaveSystem] Settings restored from save file");
        }

        private byte[] EncryptString(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.IV = encryptionIV;
                
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        private string DecryptString(byte[] cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.IV = encryptionIV;
                
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        public PlayerData CurrentPlayerData => currentPlayerData;
    }

    [Serializable]
    public class PlayerData
    {
        // Game progress
        public string currentRealm = "emberforge";
        public int sparksCollected = 0;
        public int totalPlayTime = 0; // seconds
        
        // Accessibility
        public int colorblindMode = 0;
        public bool reducedMotion = false;
        public bool hapticsEnabled = true;
        
        // Meta
        public string lastSaveTime;
        public string version = "1.0.0";
    }
}
