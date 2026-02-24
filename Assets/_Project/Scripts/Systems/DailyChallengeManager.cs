using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Platform;

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
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
            GenerateChallengeForDate(DateTime.UtcNow, true, true);
        }

        public DailyChallenge GenerateChallengeForDate(DateTime utcDate, bool saveProgress, bool notify)
        {
            DateTime challengeDate = utcDate.Date;
            int seed = challengeDate.Year * 10000 + challengeDate.Month * 100 + challengeDate.Day;
            UnityEngine.Random.InitState(seed);

            ChallengeType type = SelectChallengeType();

            currentChallenge = new DailyChallenge
            {
                challengeId = $"daily_{challengeDate:yyyyMMdd}",
                date = challengeDate,
                type = type,
                targetValue = DetermineChallengeTarget(type),
                currentProgress = 0,
                isCompleted = false,
                rewardSparks = CalculateReward()
            };

            nextChallengeTime = challengeDate.AddDays(1);

            if (saveProgress)
            {
                SaveProgress(challengeDate);
            }

            if (notify)
            {
                OnNewChallengeAvailable?.Invoke(currentChallenge);
            }

            Debug.Log($"[DailyChallenge] New challenge: {type} - Target: {currentChallenge.targetValue}");
            return currentChallenge;
        }

        private ChallengeType SelectChallengeType()
        {
            bool reducedMotion = Core.AccessibilityManager.Instance?.ReducedMotionEnabled ?? false;
            bool hasColorblindMode = Core.AccessibilityManager.Instance?.CurrentColorblindMode != Core.ColorblindMode.None;
            List<ChallengeType> availableTypes = GetAvailableChallengeTypes(reducedMotion, hasColorblindMode);
            return availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];
        }
            
        public List<ChallengeType> GetAvailableChallengeTypes()
        {
            bool reducedMotion = Core.AccessibilityManager.Instance?.ReducedMotionEnabled ?? false;
            bool hasColorblindMode = Core.AccessibilityManager.Instance?.CurrentColorblindMode != Core.ColorblindMode.None;
            return GetAvailableChallengeTypes(reducedMotion, hasColorblindMode);
        }

        private List<ChallengeType> GetAvailableChallengeTypes(bool reducedMotion, bool hasColorblindMode)
        {
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
            
            if (hasColorblindMode)
            {
                availableTypes.Add(ChallengeType.FindColorblindSecret);
            }

            return availableTypes;
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
            
            SaveProgress(currentChallenge.date);
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
            SaveProgress(currentChallenge.date);
            
            Debug.Log($"[DailyChallenge] ✅ Completed! Reward: {reward} sparks. Streak: {consecutiveDaysCompleted} days");
        }

        private void LoadProgress()
        {
            consecutiveDaysCompleted = PlayerPrefs.GetInt("DailyChallenge_Streak", 0);

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
                        nextChallengeTime = savedDateTime.Date.AddDays(1);
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
        }

        private void SaveProgress(DateTime saveDate)
        {
            PlayerPrefs.SetString("DailyChallenge_Date", saveDate.Date.ToString());
            PlayerPrefs.SetInt("DailyChallenge_Streak", consecutiveDaysCompleted);
            
            if (currentChallenge != null)
            {
                string json = JsonUtility.ToJson(currentChallenge);
                PlayerPrefs.SetString("DailyChallenge_Data", json);
            }
            
            PlayerPrefs.Save();
        }

        public void ResetProgress()
        {
            currentChallenge = null;
            consecutiveDaysCompleted = 0;
            nextChallengeTime = DateTime.MinValue;

            PlayerPrefs.DeleteKey("DailyChallenge_Date");
            PlayerPrefs.DeleteKey("DailyChallenge_Streak");
            PlayerPrefs.DeleteKey("DailyChallenge_Data");
            PlayerPrefs.Save();

            Debug.Log("[DailyChallenge] Progress reset");
        }

        public DailyChallenge CurrentChallenge => currentChallenge;
        public int StreakDays => consecutiveDaysCompleted;

        // ── Cosmic Patron: bonus daily challenge ───────────────────────────

        private DailyChallenge _bonusChallenge;
        private string _bonusChallengeDate = string.Empty;

        /// <summary>
        /// Returns a second challenge for Cosmic Patrons generated from an
        /// offset seed so it never duplicates the primary challenge.
        /// Returns null if the player is not a patron or when the patron
        /// package is unavailable.
        /// </summary>
        public DailyChallenge GetBonusPatronChallenge()
        {
            if (!HasBonusPatronChallenge) return null;

            string todayKey = DateTime.UtcNow.Date.ToString("yyyyMMdd");
            if (_bonusChallengeDate == todayKey && _bonusChallenge != null)
                return _bonusChallenge;

            // Offset by 99 999 so the seed is always distinct from the primary
            DateTime today = DateTime.UtcNow.Date;
            int seed = (today.Year * 10000 + today.Month * 100 + today.Day) + 99999;
            UnityEngine.Random.InitState(seed);

            var types = GetAvailableChallengeTypes();
            ChallengeType bonusType = types[UnityEngine.Random.Range(0, types.Count)];

            _bonusChallenge = new DailyChallenge
            {
                challengeId     = $"bonus_{todayKey}",
                date            = today,
                type            = bonusType,
                targetValue     = DetermineChallengeTarget(bonusType),
                currentProgress = PlayerPrefs.GetInt($"BonusChallenge_Progress_{todayKey}", 0),
                isCompleted     = PlayerPrefs.GetInt($"BonusChallenge_Done_{todayKey}", 0) == 1,
                rewardSparks    = Mathf.RoundToInt(baseRewardSparks * 0.5f)
            };

            _bonusChallengeDate = todayKey;
            return _bonusChallenge;
        }

        /// <summary>True when the player is an active Cosmic Patron.</summary>
        public bool HasBonusPatronChallenge =>
            CosmicPatronManager.Instance != null &&
            CosmicPatronManager.Instance.HasBonusDailyChallenge;

        /// <summary>Records progress on the patron bonus challenge.</summary>
        public void IncrementBonusChallengeProgress(ChallengeType type, int amount = 1)
        {
            var bonus = GetBonusPatronChallenge();
            if (bonus == null || bonus.isCompleted || bonus.type != type) return;

            bonus.currentProgress += amount;
            string todayKey = DateTime.UtcNow.Date.ToString("yyyyMMdd");
            PlayerPrefs.SetInt($"BonusChallenge_Progress_{todayKey}", bonus.currentProgress);

            if (bonus.currentProgress >= bonus.targetValue)
            {
                bonus.isCompleted = true;
                PlayerPrefs.SetInt($"BonusChallenge_Done_{todayKey}", 1);
                OnChallengeCompleted?.Invoke(bonus.rewardSparks);
                Debug.Log("[DailyChallenge] ✅ Bonus Patron challenge completed!");
            }
            PlayerPrefs.Save();
        }

        public DailyChallenge GetTodayChallenge()
        {
            return currentChallenge;
        }

        public int GetCurrentStreak()
        {
            return consecutiveDaysCompleted;
        }

        /// <summary>
        /// Generates a Wordle-style emoji share card string for the current challenge.
        /// Example:
        ///   Ascendant Continuum — Daily Ritual
        ///   ✨✨✨✨✨
        ///   🔥 Collect 50 sparks
        ///   Streak: 7 🔥
        /// </summary>
        public string GenerateShareText()
        {
            if (currentChallenge == null)
                return "Ascendant Continuum ✨\nNo active challenge today.";

            // Pick type emoji
            string typeEmoji = GetTypeEmoji(currentChallenge.type);

            // Build a 5-block progress bar (like Wordle)
            int totalBlocks = 5;
            float pct = currentChallenge.GetProgress();
            int filledBlocks = Mathf.RoundToInt(pct * totalBlocks);
            string progressBar = "";
            for (int i = 0; i < totalBlocks; i++)
                progressBar += i < filledBlocks ? "✨" : "⬛";

            string result = currentChallenge.isCompleted ? "✅ COMPLETED" : progressBar;
            string streakLine = consecutiveDaysCompleted > 0
                ? $"Streak: {consecutiveDaysCompleted} {GetStreakEmoji(consecutiveDaysCompleted)}"
                : "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("✨ Ascendant Continuum — Daily Ritual ✨");
            sb.AppendLine($"{typeEmoji} {currentChallenge.GetDescription()}");
            sb.AppendLine(result);
            if (!string.IsNullOrEmpty(streakLine))
                sb.AppendLine(streakLine);
            sb.Append("ascendantcontinuum.com");

            return sb.ToString();
        }

        private static string GetTypeEmoji(ChallengeType type)
        {
            switch (type)
            {
                case ChallengeType.CollectSparks:       return "🔥";
                case ChallengeType.VisitRealms:         return "🌍";
                case ChallengeType.CompleteRitualsFast: return "⚡";
                case ChallengeType.MeditateInRealm:     return "🌿";
                case ChallengeType.DiscoverSecret:      return "🔮";
                case ChallengeType.FindColorblindSecret: return "🌈";
                default:                                return "⭐";
            }
        }

        private static string GetStreakEmoji(int streak)
        {
            if (streak >= 30) return "🏆";
            if (streak >= 14) return "💎";
            if (streak >= 7)  return "🔥";
            return "⭐";
        }
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

        public string Title => GetDescription();
        public int CurrentProgress => currentProgress;
        public int RequiredProgress => targetValue;
        public string RealmName => "Emberforge";
        
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
            if (targetValue <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01((float)currentProgress / targetValue);
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
