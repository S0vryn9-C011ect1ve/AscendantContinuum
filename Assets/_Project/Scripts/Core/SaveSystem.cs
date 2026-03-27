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
    public partial class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool encryptSaveData = true;
        [SerializeField] private string saveFileName = "ascendant_save.dat";

        private string savePath;
        private PlayerData currentPlayerData;
        private byte[] encryptionKey;
        private byte[] encryptionIV;
        private int persistedPlayTimeAtSessionStart;
        private float sessionStartRealtime;

        // ── Registered component cache (avoids FindFirstObjectByType in hot paths) ──
        private static int _registeredSparksCollected = 0;

        /// <summary>Call from EmberforgeSparks.Awake() to register the current spark count.</summary>
        public static void RegisterSparksCollected(int count) => _registeredSparksCollected = count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentPlayerData = new PlayerData();
            InitializeCryptoMaterial();

            persistedPlayTimeAtSessionStart = 0;
            sessionStartRealtime = Time.realtimeSinceStartup;

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
                    sparksCollected = _registeredSparksCollected,
                    sigilsCollected = PlayerPrefs.GetInt("SigilCount", 0),

                    // Accumulated play time (persisted baseline + current session delta)
                    totalPlayTime = CalculateTotalPlayTimeSeconds(),

                    // Accessibility settings
                    colorblindMode = (int)(AccessibilityManager.Instance?.CurrentColorblindMode ?? 0),
                    reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled ?? false,
                    hapticsEnabled = AccessibilityManager.Instance?.HapticsEnabled ?? true,

                    // Meta
                    lastSaveTime = DateTime.UtcNow.ToString("o"),
                    version = Application.version
                };

                // Persist total play time in PlayerPrefs for next session
                PlayerPrefs.SetInt("TotalPlayTimeSeconds", currentPlayerData.totalPlayTime);
                PlayerPrefs.Save();

                WriteCurrentPlayerDataToDisk();

                // Sync to Firebase if available
                if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsConnected)
                {
                    string userId = FirebaseManager.Instance.UserId ?? "anonymous_user";
                    _ = FirebaseManager.Instance.SavePlayerData("users", userId, currentPlayerData);
                }

                GameEvents.RaiseSaveCompleted(true, "saved");
                Debug.Log("[SaveSystem] ✅ Game saved successfully");
            }
            catch (Exception e)
            {
                GameEvents.RaiseSaveCompleted(false, e.Message);
                Debug.LogError($"[SaveSystem] ❌ Save failed: {e.Message}");
            }
        }

        public void UpdateProgressSnapshot(string realmId, int sparks, int sigils)
        {
            currentPlayerData.currentRealm = string.IsNullOrEmpty(realmId) ? "emberforge" : realmId;
            currentPlayerData.sparksCollected = Mathf.Max(0, sparks);
            currentPlayerData.sigilsCollected = Mathf.Max(0, sigils);
            currentPlayerData.totalPlayTime = CalculateTotalPlayTimeSeconds();
            currentPlayerData.lastSaveTime = DateTime.UtcNow.ToString("o");
            currentPlayerData.version = Application.version;

            WriteCurrentPlayerDataToDisk();
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

                string trimmed = string.IsNullOrWhiteSpace(json) ? string.Empty : json.TrimStart();
                if (!trimmed.StartsWith("{"))
                {
                    Debug.LogWarning("[SaveSystem] Save file content is not a JSON object. Resetting player data.");
                    currentPlayerData = new PlayerData();
                }
                else
                {
                    currentPlayerData = JsonUtility.FromJson<PlayerData>(json);
                    if (currentPlayerData == null)
                    {
                        currentPlayerData = new PlayerData();
                    }
                }

                persistedPlayTimeAtSessionStart = Mathf.Max(0, currentPlayerData.totalPlayTime);
                sessionStartRealtime = Time.realtimeSinceStartup;

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

        public void DeleteSaveData()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                currentPlayerData = new PlayerData();
                persistedPlayTimeAtSessionStart = 0;
                sessionStartRealtime = Time.realtimeSinceStartup;
                Debug.Log("[SaveSystem] Save data deleted and player data reset");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] ❌ Delete save failed: {e.Message}");
            }
        }

        /// <summary>
        /// Applies a JSON-encoded cloud save to the local game state.
        /// Keeps whichever save is more recent (local vs cloud); if the cloud copy is
        /// newer, writes it to disk and reloads accessibility/settings.
        /// Returns true when the cloud data was accepted and applied.
        /// Called by GooglePlayGamesManager after a successful cloud-read.
        /// </summary>
        public bool LoadFromCloudJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;

            try
            {
                string trimmed = json.TrimStart();
                if (!trimmed.StartsWith("{"))
                {
                    Debug.LogWarning("[SaveSystem] Cloud JSON is not a valid object — ignoring.");
                    return false;
                }

                var cloudData = JsonUtility.FromJson<PlayerData>(json);
                if (cloudData == null) return false;

                // Compare timestamps: keep the more recent save
                bool cloudNewer = false;
                DateTime cloudTime = DateTime.MinValue;
                DateTime localTime = DateTime.MinValue;
                bool cloudHasTime = !string.IsNullOrEmpty(cloudData.lastSaveTime) &&
                    DateTime.TryParse(cloudData.lastSaveTime, null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out cloudTime);
                bool localHasTime = !string.IsNullOrEmpty(currentPlayerData.lastSaveTime) &&
                    DateTime.TryParse(currentPlayerData.lastSaveTime, null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out localTime);

                if (cloudHasTime && localHasTime)
                    cloudNewer = cloudTime > localTime;
                else
                    cloudNewer = true; // no timestamps — trust the cloud

                if (!cloudNewer)
                {
                    Debug.Log("[SaveSystem] Local save is newer than cloud — keeping local data.");
                    return false;
                }

                currentPlayerData = cloudData;
                persistedPlayTimeAtSessionStart = Mathf.Max(0, currentPlayerData.totalPlayTime);
                sessionStartRealtime = Time.realtimeSinceStartup;

                WriteCurrentPlayerDataToDisk();
                ApplyLoadedData();

                Debug.Log($"[SaveSystem] ✅ Cloud save applied — last save: {currentPlayerData.lastSaveTime}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] ❌ Cloud load failed: {e.Message}");
                return false;
            }
        }

        private void ApplyLoadedData()
        {
            // Restore accessibility settings
            AccessibilityManager.Instance?.SetColorblindMode((AscendantContinuum.Core.ColorblindMode)currentPlayerData.colorblindMode);
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

        private void InitializeCryptoMaterial()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] keySource = Encoding.UTF8.GetBytes("AscendantContinuum.EncryptionKey.v1");
                encryptionKey = sha256.ComputeHash(keySource);

                byte[] ivSource = Encoding.UTF8.GetBytes("AscendantContinuum.EncryptionIV.v1");
                byte[] ivHash = sha256.ComputeHash(ivSource);
                encryptionIV = new byte[16];
                Array.Copy(ivHash, encryptionIV, encryptionIV.Length);
            }
        }

        private void WriteCurrentPlayerDataToDisk()
        {
            string json = JsonUtility.ToJson(currentPlayerData, true);

            persistedPlayTimeAtSessionStart = Mathf.Max(0, currentPlayerData.totalPlayTime);
            sessionStartRealtime = Time.realtimeSinceStartup;

            if (encryptSaveData)
            {
                byte[] encryptedData = EncryptString(json);
                File.WriteAllBytes(savePath, encryptedData);
            }
            else
            {
                File.WriteAllText(savePath, json);
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

        private int CalculateTotalPlayTimeSeconds()
        {
            float sessionSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - sessionStartRealtime);
            return persistedPlayTimeAtSessionStart + Mathf.RoundToInt(sessionSeconds);
        }

        public PlayerData CurrentPlayerData => currentPlayerData;
    }

    [Serializable]
    public class PlayerData
    {
        // Game progress
        public string currentRealm = "emberforge";
        public int sparksCollected = 0;
        public int sigilsCollected = 0;
        public int totalPlayTime = 0; // seconds

        // Accessibility
        public int colorblindMode = 0;
        public bool reducedMotion = false;
        public bool hapticsEnabled = true;

        // Meta
        public string lastSaveTime;
        public string version = "1.0.0";

        // ── Sigil journal ─────────────────────────────────────────────────
        public string[] sigilIds = new string[0];       // GUIDs of all owned sigils
        public int sigilCountToday = 0;
        public int sigilCountTotal = 0;
        public string lastSigilDate = "";               // yyyy-MM-dd UTC

        // ── Continuum collective ──────────────────────────────────────────
        public float lastKnownCollectiveEnergy = 0f;

        // ── NPC memory (migrated from PlayerPrefs) ────────────────────────
        public int npcSparkusCreativity = 0;
        public int npcPetalinaJoy = 0;
        public int npcLuminaPatterns = 0;

        // ── Cosmetics ─────────────────────────────────────────────────────
        public string[] unlockedCosmeticIds = new string[0];
        public string activeCosmeticTrailId = "default";
        public string activeJournalThemeId = "dark_cosmos";
        public string activeSigilGlowId = "soft";

        // ── Session tracking ──────────────────────────────────────────────
        public int totalSessionsCompleted = 0;
        public int consecutiveDays = 0;
        public string lastSessionDate = "";

        // ── Privacy ───────────────────────────────────────────────────────
        public bool analyticsEnabled  = true;
        public bool allowTimeShareData = true;  // Collective energy / social features

        // ── Export tracking ───────────────────────────────────────────────
        public int sigilExportCount = 0;        // lifetime sigil PNG/GIF shares

        // ── Daily challenge streak (migrated from PlayerPrefs) ─────────────
        public int  challengeStreak   = 0;      // consecutive daily completions
        public string lastChallengeDate = "";   // yyyy-MM-dd UTC
        public int  challengesCompletedTotal = 0;

        // ── Achievements (migrated from PlayerPrefs) ─────────────────────
        // Serialised as a JSON string to avoid Unity array-deserialisation issues
        public string achievementsUnlockedJson = "[]"; // JSON string[] of unlocked IDs

        // ── Cosmic identity (migrated from PlayerPrefs) ─────────────────
        public string cosmicProfileJson = "";   // JSON serialised CosmicProfile
        public string cosmicDeityChosen = "";   // e.g. "flame", "herald", "tides"...
        public bool   pantheonQuizDone  = false;

        // ── Per-realm progress (migrated from PlayerPrefs) ───────────────
        public int lanternReleasedCount = 0;    // LanternAscension total releases
        public int dawnSolvedCount      = 0;    // DawnCitadel puzzles solved
        public int echoArchiveCount     = 0;    // EchoFields echoes recorded
        public int timeCapsuleBuriedCount   = 0;
        public int timeCapsuleOpenedCount   = 0;
        public int meteorPendingSparks  = 0;    // MeteorShowerEvent banked sparks
        public int mercuryRareSigilCount = 0;   // PantheonPlanetManager
        public int natureSigilCount     = 0;    // NatureConnectionManager
    }
}

// ── SaveSystem extension methods (sigil helpers) ──────────────────────────────
// Placed in same file to avoid partial-class complications.
namespace AscendantContinuum.Core
{
    public partial class SaveSystem
    {
        /// <summary>
        /// Increments sigilCountToday + sigilCountTotal and appends <paramref name="sigilId"/> to sigilIds.
        /// Called by SigilCompletionHandler after every successful sigil.
        /// </summary>
        public void BumpSigilCount(string sigilId)
        {
            var data = CurrentPlayerData;
            // Reset daily count if day changed
            string todayUtc = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (data.lastSigilDate != todayUtc)
            {
                data.sigilCountToday = 0;
                data.lastSigilDate   = todayUtc;
            }
            data.sigilCountToday++;
            data.sigilCountTotal++;

            // Append to journal
            var ids = data.sigilIds ?? new string[0];
            var newIds = new string[ids.Length + 1];
            System.Array.Copy(ids, newIds, ids.Length);
            newIds[ids.Length] = sigilId;
            data.sigilIds = newIds;

            SaveGame();
        }

        /// <summary>
        /// Records that a sigil PNG/GIF was exported and persists the lifetime count.
        /// </summary>
        public void RecordSigilExport(string sigilId)
        {
            CurrentPlayerData.sigilExportCount++;
            Debug.Log($"[SaveSystem] Sigil exported: {sigilId} (total exports: {CurrentPlayerData.sigilExportCount})");
            SaveGame();
        }
    }
}
