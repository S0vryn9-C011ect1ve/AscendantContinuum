using System;
using System.Collections.Generic;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Data;

namespace AscendantContinuum.Progression
{
    /// <summary>
    /// Tracks player progression across realms, rituals, and sigils
    /// Persisted via SaveSystem (AES-256 encrypted)
    /// </summary>
    [System.Serializable]
    public class ProgressionData
    {
        [System.Serializable]
        public class RealmProgress
        {
            public string realmId = "";
            public bool isUnlocked = false;
            public int timesCompleted = 0;
            public int highScore = 0;
            public float bestTime = 0f;
            public DateTime firstCompletionTime = DateTime.MinValue;
            public DateTime lastCompletionTime = DateTime.MinValue;
            public bool isMastered = false; // Unlocked prestige
        }

        [System.Serializable]
        public class RitualMemory
        {
            public string ritualId = "";
            public int timesCompleted = 0;
            public int personalBest = 0;
            public float totalTimeSpent = 0f;
            public bool isDiscovered = false;
        }

        [System.Serializable]
        public class SigilMastery
        {
            public string sigilId = "";
            public int collectionCount = 0;
            public bool isMastered = false;
            public DateTime masterDate = DateTime.MinValue;
            public int prestigeLevel = 0; // How many times prestige loop completed
        }

        // Realm progression (5 realms total)
        public List<RealmProgress> realmProgress = new List<RealmProgress>();

        // Ritual memory (each realm has multiple rituals)
        public List<RitualMemory> ritualMemories = new List<RitualMemory>();

        // Sigil mastery tracking
        public List<SigilMastery> sigilMastery = new List<SigilMastery>();

        // Global progression metrics
        public int totalRealmCompletions = 0;
        public int totalRitualCompletions = 0;
        public float totalPlayTime = 0f;
        public DateTime firstPlayTime = DateTime.MinValue;
        public DateTime lastPlayTime = DateTime.Now;

        // Prestige system
        public int prestigePoints = 0;
        public int totalPrestigeCycles = 0;
        public DateTime lastPrestigeTime = DateTime.MinValue;

        // Unlock gates
        public int unlockedRealmCount = 1; // Start with realm 1 only

        public ProgressionData()
        {
            // Initialize default realms
            string[] realmIds = { "emberforge", "verdant", "echo", "dawn", "lantern" };
            foreach (string id in realmIds)
            {
                realmProgress.Add(new RealmProgress { realmId = id, isUnlocked = (id == "emberforge") });
            }

            firstPlayTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Manager for progression system - handles realm unlocks, ritual tracking, prestige
    /// </summary>
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        private ProgressionData progressionData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadProgression();
            GameEvents.OnSigilCompleted   += HandleSigilCompleted;
            GameEvents.OnRealmCompleted   += HandleRealmCompleted;
            GameEvents.OnSigilCrafted     += HandleSigilCrafted;
        }

        private void OnDestroy()
        {
            GameEvents.OnSigilCompleted   -= HandleSigilCompleted;
            GameEvents.OnRealmCompleted   -= HandleRealmCompleted;
            GameEvents.OnSigilCrafted     -= HandleSigilCrafted;
        }

        private void HandleSigilCompleted(SigilData data)
        {
            if (data != null) RecordSigilObtained(data.sigilId);
        }

        private void HandleRealmCompleted(string realmId, int score)
        {
            RecordRealmCompletion(realmId, score, Time.realtimeSinceStartup);
        }

        private void HandleSigilCrafted(string resultId, string combinationName, bool isFirstDiscovery)
        {
            if (!string.IsNullOrEmpty(resultId)) RecordSigilObtained(resultId);
        }

        /// <summary>
        /// Load progression from SaveSystem
        /// </summary>
        public void LoadProgression()
        {
            // TODO: Integrate with SaveSystem when available
            // For now, create new or load from PlayerPrefs
            if (progressionData == null)
            {
                progressionData = new ProgressionData();
                Debug.Log("[ProgressionManager] Initialized new progression data");
            }
        }

        /// <summary>
        /// Save progression to SaveSystem
        /// </summary>
        public void SaveProgression()
        {
            // TODO: Integrate with SaveSystem when available
            progressionData.lastPlayTime = DateTime.Now;
            Debug.Log("[ProgressionManager] Saved progression data");
        }

        /// <summary>
        /// Record ritual completion
        /// </summary>
        public void RecordRitualCompletion(string ritualId, int score, float timeSpent)
        {
            var memory = progressionData.ritualMemories.Find(r => r.ritualId == ritualId);
            if (memory == null)
            {
                memory = new ProgressionData.RitualMemory { ritualId = ritualId, isDiscovered = true };
                progressionData.ritualMemories.Add(memory);
            }

            memory.timesCompleted++;
            memory.totalTimeSpent += timeSpent;
            if (score > memory.personalBest)
                memory.personalBest = score;

            progressionData.totalRitualCompletions++;
            SaveProgression();

            Debug.Log($"[ProgressionManager] Ritual completed: {ritualId}, Total: {memory.timesCompleted}, Score: {score}");
        }

        /// <summary>
        /// Record realm completion and check for unlock conditions
        /// </summary>
        public void RecordRealmCompletion(string realmId, int score, float completionTime)
        {
            var realm = progressionData.realmProgress.Find(r => r.realmId == realmId);
            if (realm == null) return;

            realm.timesCompleted++;
            realm.lastCompletionTime = DateTime.Now;

            if (realm.firstCompletionTime == DateTime.MinValue)
                realm.firstCompletionTime = DateTime.Now;

            if (score > realm.highScore)
                realm.highScore = score;

            if (realm.bestTime == 0 || completionTime < realm.bestTime)
                realm.bestTime = completionTime;

            progressionData.totalRealmCompletions++;

            // Check for mastery (5+ completions)
            if (realm.timesCompleted >= 5 && !realm.isMastered)
            {
                UnlockMastery(realmId);
            }

            // Check for next realm unlock (5 completions of current realm)
            CheckRealmUnlocks();

            SaveProgression();
            Debug.Log($"[ProgressionManager] Realm completed: {realmId}, Total: {realm.timesCompleted}, Score: {score}");
        }

        /// <summary>
        /// Unlock mastery for a realm (enables prestige loop)
        /// </summary>
        private void UnlockMastery(string realmId)
        {
            var realm = progressionData.realmProgress.Find(r => r.realmId == realmId);
            if (realm != null)
            {
                realm.isMastered = true;
                Debug.Log($"[ProgressionManager] Realm mastered: {realmId}");
            }
        }

        /// <summary>
        /// Check and unlock next realms based on completion milestones
        /// </summary>
        private void CheckRealmUnlocks()
        {
            // Unlock progression: need 5 completions of previous realm
            string[] realmIds = { "emberforge", "verdant", "echo", "dawn", "lantern" };
            int unlockedCount = 0;

            for (int i = 0; i < realmIds.Length; i++)
            {
                var realm = progressionData.realmProgress.Find(r => r.realmId == realmIds[i]);
                if (realm == null) continue;

                if (realm.isUnlocked)
                {
                    unlockedCount = i + 1;
                }
                else if (i > 0)
                {
                    var prevRealm = progressionData.realmProgress.Find(r => r.realmId == realmIds[i - 1]);
                    if (prevRealm != null && prevRealm.timesCompleted >= 5 && !realm.isUnlocked)
                    {
                        realm.isUnlocked = true;
                        unlockedCount = i + 1;
                        Debug.Log($"[ProgressionManager] Realm unlocked: {realm.realmId}");
                    }
                }
            }

            progressionData.unlockedRealmCount = unlockedCount;
        }

        /// <summary>
        /// Track sigil collection and mastery
        /// </summary>
        public void RecordSigilObtained(string sigilId)
        {
            var mastery = progressionData.sigilMastery.Find(s => s.sigilId == sigilId);
            if (mastery == null)
            {
                mastery = new ProgressionData.SigilMastery { sigilId = sigilId };
                progressionData.sigilMastery.Add(mastery);
            }

            mastery.collectionCount++;

            // Mastery at 10 collections
            if (mastery.collectionCount >= 10 && !mastery.isMastered)
            {
                mastery.isMastered = true;
                mastery.masterDate = DateTime.Now;
                Debug.Log($"[ProgressionManager] Sigil mastered: {sigilId}");
            }

            SaveProgression();
        }

        /// <summary>
        /// Execute prestige loop - reset realm but grant bonuses
        /// </summary>
        public void ExecutePrestige(string realmId)
        {
            var realm = progressionData.realmProgress.Find(r => r.realmId == realmId);
            if (realm == null || !realm.isMastered)
            {
                Debug.LogWarning("[ProgressionManager] Cannot prestige unmmastered realm");
                return;
            }

            // Grant prestige points (1 point per master level)
            progressionData.prestigePoints += 10;
            progressionData.totalPrestigeCycles++;
            progressionData.lastPrestigeTime = DateTime.Now;

            // Reset realm completions but keep mastery
            realm.timesCompleted = 0;
            realm.highScore = 0;
            realm.bestTime = 0f;

            // Increment prestige for all sigils from this realm
            foreach (var sigil in progressionData.sigilMastery)
            {
                sigil.prestigeLevel++;
            }

            SaveProgression();
            Debug.Log($"[ProgressionManager] Prestige executed for {realmId}, Total cycles: {progressionData.totalPrestigeCycles}");
        }

        // ===== Getters =====

        public ProgressionData GetProgressionData() => progressionData;

        public List<ProgressionData.RealmProgress> GetRealmProgress() 
            => progressionData != null ? progressionData.realmProgress : new List<ProgressionData.RealmProgress>();

        public bool IsRealmUnlocked(string realmId)
        {
            var realm = progressionData?.realmProgress.Find(r => r.realmId == realmId);
            return realm != null && realm.isUnlocked;
        }

        public int GetRealmCompletions(string realmId)
        {
            var realm = progressionData?.realmProgress.Find(r => r.realmId == realmId);
            return realm != null ? realm.timesCompleted : 0;
        }

        public bool IsRealmMastered(string realmId)
        {
            var realm = progressionData?.realmProgress.Find(r => r.realmId == realmId);
            return realm != null && realm.isMastered;
        }

        public int GetPrestigePoints() => progressionData?.prestigePoints ?? 0;

        public int GetTotalPlayTime() => Mathf.FloorToInt(progressionData?.totalPlayTime ?? 0f);

        public int GetSigilMasteryCount()
        {
            int count = 0;
            foreach (var sigil in progressionData?.sigilMastery ?? new List<ProgressionData.SigilMastery>())
            {
                if (sigil.isMastered) count++;
            }
            return count;
        }
    }
}
