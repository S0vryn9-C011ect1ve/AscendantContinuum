using UnityEngine;
using System;
using System.Collections.Generic;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Daily challenge system - new challenges every day at midnight UTC
    /// Challenges are tailored to accessibility settings and playstyle
    /// </summary>
    public class DailyChallengeManager : MonoBehaviour
    {
        public static DailyChallengeManager Instance { get; private set; }

        [Header("Challenge Settings")]
        [SerializeField] private DailyChallenge currentChallenge;
        [SerializeField] private DateTime nextChallengeTime;
        [SerializeField] private int consecutiveDaysCompleted = 0;
        
        [Header("Rewards")]
        [SerializeField] private int baseRewardSparks = 50;
        [SerializeField] private int streakBonusPerDay = 10;
        
        public event Action<DailyChallenge> OnNewChallengeAvailable;
        public event Action<int> OnChallengeCompleted;

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
            LoadProgress();
            CheckForNewChallenge();
        }

        private void Update()
        {
            // Check every frame if it's time for a new challenge
            if (DateTime.UtcNow >= nextChallengeTime)
            {
                GenerateNewChallenge();
            }
            
            // Track challenge progress
            if (currentChallenge != null && !currentChallenge.isCompleted)
            {
                UpdateChallengeProgress();
            }
        }

        private void CheckForNewChallenge()
        {
            if (currentChallenge == null || DateTime.UtcNow >= nextChallengeTime)
            {
                GenerateNewChallenge();
            }
        }

        private void GenerateNewChallenge()
        {
            // Use date as seed for deterministic daily challenges
            DateTime today = DateTime.UtcNow.Date;
            int seed = today.Year * 10000 + today.Month * 100 + today.Day;
            UnityEngine.Random.InitState(seed);
            
            // Choose challenge type based on player's accessibility settings
            ChallengeType type = SelectChallengeType();
            
            currentChallenge = new DailyChallenge
            {
                challengeId = $"daily_{today:yyyyMMdd}",
                date = today,
                type = type,
                targetValue = DetermineChallengeTarget(type),
                currentProgress = 0,
                isCompleted = false,
                rewardSparks = CalculateReward()
            };
            
            // Set next challenge time to midnight UTC tomorrow
            nextChallengeTime = today.AddDays(1);
            
            SaveProgress();
            OnNewChallengeAvailable?.Invoke(currentChallenge);
            
            Debug.Log($"[DailyChallenge] New challenge: {type} - Target: {currentChallenge.targetValue}");
        }

        private ChallengeType SelectChallengeType()
        {
            bool reducedMotion = Core.AccessibilityManager.Instance?.ReducedMotionEnabled ?? false;
            
            // Weight challenges based on accessibility settings
            List<ChallengeType> availableTypes = new List<ChallengeType>
            {
                ChallengeType.CollectSparks,
                ChallengeType.VisitRealms,
                ChallengeType.DiscoverSecret
            };
            
            if (!reducedMotion)
            {
                availableTypes.Add(ChallengeType.CompleteRitualsFast);
            }
            else
            {
                availableTypes.Add(ChallengeType.MeditateInRealm);
            }
            
            // Use specific colorblind mode challenges
            if (Core.AccessibilityManager.Instance?.CurrentColorblindMode != Core.ColorblindMode.None)
            {
                availableTypes.Add(ChallengeType.FindColorblindSecret);
            }
            
            return availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];
        }

        private int DetermineChallengeTarget(ChallengeType type)
        {
            switch (type)
            {
                case ChallengeType.CollectSparks:
                    return UnityEngine.Random.Range(30, 100);
                    
                case ChallengeType.VisitRealms:
                    return 3; // Visit 3 different realms
                    
                case ChallengeType.CompleteRitualsFast:
                    return 5; // Complete 5 rituals
                    
                case ChallengeType.MeditateInRealm:
                    return 300; // 5 minutes of calm gameplay
                    
                case ChallengeType.DiscoverSecret:
                    return 1; // Find 1 hidden secret
                    
                case ChallengeType.FindColorblindSecret:
                    return 1; // Find colorblind-specific secret
                    
                default:
                    return 50;
            }
        }

        private int CalculateReward()
        {
            return baseRewardSparks + (consecutiveDaysCompleted * streakBonusPerDay);
        }

        private void UpdateChallengeProgress()
        {
            if (currentChallenge == null || currentChallenge.isCompleted) return;
            
            // Update based on challenge type
            switch (currentChallenge.type)
            {
                case ChallengeType.CollectSparks:
                    // Updated externally by EmberforgeSparks
                    break;
                    
                case ChallengeType.MeditateInRealm:
                    if (Core.GameManager.Instance?.CurrentState == Core.GameState.Playing)
                    {
                        currentChallenge.currentProgress += (int)(Time.deltaTime);
                    }
                    break;
            }
            
            // Check completion
            if (currentChallenge.currentProgress >= currentChallenge.targetValue)
            {
                CompleteChallenge();
            }
        }

        public void IncrementChallengeProgress(ChallengeType type, int amount = 1)
        {
            if (currentChallenge == null || currentChallenge.isCompleted) return;
            if (currentChallenge.type != type) return;
            
            currentChallenge.currentProgress += amount;
            
            Debug.Log($"[DailyChallenge] Progress: {currentChallenge.currentProgress}/{currentChallenge.targetValue}");
            
            if (currentChallenge.currentProgress >= currentChallenge.targetValue)
            {
                CompleteChallenge();
            }
            
            SaveProgress();
        }

        private void CompleteChallenge()
        {
            if (currentChallenge.isCompleted) return;
            
            currentChallenge.isCompleted = true;
            consecutiveDaysCompleted++;
            
            // Award rewards
            int reward = currentChallenge.rewardSparks;
            
            // Track completion with Firebase
            Core.FirebaseManager.Instance?.TrackEvent("daily_challenge_completed", new Dictionary<string, object>
            {
                { "challenge_type", currentChallenge.type.ToString() },
                { "streak_days", consecutiveDaysCompleted },
                { "reward", reward }
            });
            
            OnChallengeCompleted?.Invoke(reward);
            SaveProgress();
            
            Debug.Log($"[DailyChallenge] ✅ Completed! Reward: {reward} sparks. Streak: {consecutiveDaysCompleted} days");
        }

        private void LoadProgress()
        {
            string savedDate = PlayerPrefs.GetString("DailyChallenge_Date", "");
            
            if (!string.IsNullOrEmpty(savedDate))
            {
                DateTime savedDateTime = DateTime.Parse(savedDate);
                
                // Check if it's still the same day
                if (savedDateTime.Date == DateTime.UtcNow.Date)
                {
                    // Load saved challenge
                    string json = PlayerPrefs.GetString("DailyChallenge_Data", "");
                    if (!string.IsNullOrEmpty(json))
                    {
                        currentChallenge = JsonUtility.FromJson<DailyChallenge>(json);
                    }
                }
                else
                {
                    // Check if streak was broken
                    if ((DateTime.UtcNow.Date - savedDateTime.Date).Days > 1)
                    {
                        consecutiveDaysCompleted = 0;
                        Debug.Log("[DailyChallenge] Streak broken - resetting");
                    }
                }
            }
            
            consecutiveDaysCompleted = PlayerPrefs.GetInt("DailyChallenge_Streak", 0);
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetString("DailyChallenge_Date", DateTime.UtcNow.Date.ToString());
            PlayerPrefs.SetInt("DailyChallenge_Streak", consecutiveDaysCompleted);
            
            if (currentChallenge != null)
            {
                string json = JsonUtility.ToJson(currentChallenge);
                PlayerPrefs.SetString("DailyChallenge_Data", json);
            }
            
            PlayerPrefs.Save();
        }

        public DailyChallenge CurrentChallenge => currentChallenge;
        public int StreakDays => consecutiveDaysCompleted;
    }

    [Serializable]
    public class DailyChallenge
    {
        public string challengeId;
        public DateTime date;
        public ChallengeType type;
        public int targetValue;
        public int currentProgress;
        public bool isCompleted;
        public int rewardSparks;
        
        public string GetDescription()
        {
            switch (type)
            {
                case ChallengeType.CollectSparks:
                    return $"Collect {targetValue} sparks in Emberforge";
                case ChallengeType.VisitRealms:
                    return $"Visit {targetValue} different realms";
                case ChallengeType.CompleteRitualsFast:
                    return $"Complete {targetValue} rituals";
                case ChallengeType.MeditateInRealm:
                    return $"Spend {targetValue / 60} minutes exploring calmly";
                case ChallengeType.DiscoverSecret:
                    return "Discover a hidden secret";
                case ChallengeType.FindColorblindSecret:
                    return "Find a colorblind-mode secret";
                default:
                    return "Complete today's challenge";
            }
        }
        
        public float GetProgress()
        {
            return (float)currentProgress / targetValue;
        }
    }

    public enum ChallengeType
    {
        CollectSparks,
        VisitRealms,
        CompleteRitualsFast,
        MeditateInRealm,
        DiscoverSecret,
        FindColorblindSecret
    }
}
