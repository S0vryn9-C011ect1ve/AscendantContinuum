using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Achievement and progression tracking system
    /// Includes hidden achievements unlocked by accessibility features
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Achievement Definitions")]
        [SerializeField] private List<Achievement> allAchievements = new List<Achievement>();
        
        [Header("Progress")]
        [SerializeField] private List<string> unlockedAchievements = new List<string>();
        
        public event Action<Achievement> OnAchievementUnlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAchievements();
        }

        private void Start()
        {
            LoadProgress();
            RegisterEventListeners();
        }

        private void InitializeAchievements()
        {
            // Core progression achievements
            allAchievements.Add(new Achievement
            {
                id = "first_spark",
                name = "First Spark",
                description = "Collect your first spark in Emberforge",
                category = AchievementCategory.Progression,
                iconName = "icon_first_spark",
                isSecret = false
            });
            
            allAchievements.Add(new Achievement
            {
                id = "realm_explorer",
                name = "Realm Explorer",
                description = "Visit all 5 realms",
                category = AchievementCategory.Exploration,
                iconName = "icon_realm_explorer",
                isSecret = false
            });
            
            // Accessibility achievements (hidden until unlocked)
            allAchievements.Add(new Achievement
            {
                id = "colorblind_master",
                name = "Through Different Eyes",
                description = "Discover secrets in all 5 colorblind modes",
                category = AchievementCategory.Accessibility,
                iconName = "icon_colorblind",
                isSecret = true
            });
            
            allAchievements.Add(new Achievement
            {
                id = "calm_seeker",
                name = "The Calm Seeker",
                description = "Complete the game with reduced motion enabled",
                category = AchievementCategory.Accessibility,
                iconName = "icon_calm",
                isSecret = true
            });
            
            allAchievements.Add(new Achievement
            {
                id = "haptic_harmony",
                name = "Haptic Harmony",
                description = "Find all haptic-guided secrets",
                category = AchievementCategory.Accessibility,
                iconName = "icon_haptic",
                isSecret = true
            });
            
            // Social achievements
            allAchievements.Add(new Achievement
            {
                id = "time_capsule_creator",
                name = "Time Capsule Creator",
                description = "Create 10 time capsules for future players",
                category = AchievementCategory.Social,
                iconName = "icon_time_capsule",
                isSecret = false
            });
            
            allAchievements.Add(new Achievement
            {
                id = "puzzle_chain_helper",
                name = "Chain Link",
                description = "Help complete a cross-player puzzle chain",
                category = AchievementCategory.Social,
                iconName = "icon_chain",
                isSecret = false
            });
            
            // Mastery achievements
            allAchievements.Add(new Achievement
            {
                id = "sigil_master",
                name = "Sigil Master",
                description = "Fully evolve your personal sigil",
                category = AchievementCategory.Mastery,
                iconName = "icon_sigil",
                isSecret = false
            });
            
            allAchievements.Add(new Achievement
            {
                id = "consecutive_champion",
                name = "Consecutive Champion",
                description = "Complete daily challenges for 30 consecutive days",
                category = AchievementCategory.Mastery,
                iconName = "icon_streak",
                isSecret = false
            });
            
            // Secret achievements
            allAchievements.Add(new Achievement
            {
                id = "midnight_visitor",
                name = "Midnight Visitor",
                description = "Visit a realm exactly at midnight",
                category = AchievementCategory.Secret,
                iconName = "icon_midnight",
                isSecret = true
            });
            
            allAchievements.Add(new Achievement
            {
                id = "archaeologist",
                name = "Digital Archaeologist",
                description = "Find a player's data from over 6 months ago",
                category = AchievementCategory.Secret,
                iconName = "icon_archaeologist",
                isSecret = true
            });
            
            Debug.Log($"[AchievementManager] Initialized {allAchievements.Count} achievements");
        }

        private void RegisterEventListeners()
        {
            // Listen to game events
            if (Emberforge.EmberforgeSparks spark = FindObjectOfType<Emberforge.EmberforgeSparks>())
            {
                spark.OnSparkCollected += CheckSparkAchievements;
            }
            
            if (DailyChallengeManager.Instance != null)
            {
                DailyChallengeManager.Instance.OnChallengeCompleted += CheckDailyChallengeAchievements;
            }
            
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnRealmChanged += CheckRealmAchievements;
            }
        }

        private void CheckSparkAchievements(int totalSparks)
        {
            if (totalSparks == 1)
            {
                UnlockAchievement("first_spark");
            }
            else if (totalSparks == 100)
            {
                UnlockAchievement("spark_collector_100");
            }
            else if (totalSparks == 1000)
            {
                UnlockAchievement("spark_master_1000");
            }
        }

        private void CheckDailyChallengeAchievements(int reward)
        {
            if (DailyChallengeManager.Instance?.StreakDays >= 7)
            {
                UnlockAchievement("weekly_warrior");
            }
            
            if (DailyChallengeManager.Instance?.StreakDays >= 30)
            {
                UnlockAchievement("consecutive_champion");
            }
        }

        private void CheckRealmAchievements(string realmId)
        {
            // Track unique realms visited
            string visitedKey = $"realm_visited_{realmId}";
            if (!PlayerPrefs.HasKey(visitedKey))
            {
                PlayerPrefs.SetInt(visitedKey, 1);
                
                // Check if all 5 realms visited
                int realmsVisited = 0;
                string[] realms = { "emberforge", "verdant", "echo", "dawn", "lantern" };
                foreach (string realm in realms)
                {
                    if (PlayerPrefs.HasKey($"realm_visited_{realm}"))
                        realmsVisited++;
                }
                
                if (realmsVisited >= 5)
                {
                    UnlockAchievement("realm_explorer");
                }
            }
            
            // Check midnight visit
            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0)
            {
                UnlockAchievement("midnight_visitor");
            }
        }

        public void UnlockAchievement(string achievementId)
        {
            if (unlockedAchievements.Contains(achievementId)) return;
            
            Achievement achievement = allAchievements.FirstOrDefault(a => a.id == achievementId);
            if (achievement == null)
            {
                Debug.LogWarning($"[AchievementManager] Achievement not found: {achievementId}");
                return;
            }
            
            unlockedAchievements.Add(achievementId);
            achievement.unlockedAt = DateTime.UtcNow;
            
            SaveProgress();
            
            // Track with Firebase
            Core.FirebaseManager.Instance?.TrackEvent("achievement_unlocked", new Dictionary<string, object>
            {
                { "achievement_id", achievementId },
                { "category", achievement.category.ToString() },
                { "is_secret", achievement.isSecret }
            });
            
            OnAchievementUnlocked?.Invoke(achievement);
            
            Debug.Log($"[AchievementManager] 🏆 Achievement unlocked: {achievement.name}");
        }

        public void TrackProgress(string achievementId, int progress)
        {
            Achievement achievement = allAchievements.FirstOrDefault(a => a.id == achievementId);
            if (achievement != null)
            {
                achievement.currentProgress = progress;
                
                if (achievement.currentProgress >= achievement.targetProgress)
                {
                    UnlockAchievement(achievementId);
                }
            }
        }

        public List<Achievement> GetUnlockedAchievements()
        {
            return allAchievements.Where(a => unlockedAchievements.Contains(a.id)).ToList();
        }

        public List<Achievement> GetLockedAchievements()
        {
            return allAchievements.Where(a => !unlockedAchievements.Contains(a.id) && !a.isSecret).ToList();
        }

        public float GetCompletionPercentage()
        {
            return (float)unlockedAchievements.Count / allAchievements.Count * 100f;
        }

        private void LoadProgress()
        {
            string json = PlayerPrefs.GetString("Achievements_Unlocked", "");
            if (!string.IsNullOrEmpty(json))
            {
                AchievementSaveData saveData = JsonUtility.FromJson<AchievementSaveData>(json);
                unlockedAchievements = new List<string>(saveData.unlockedIds);
            }
        }

        private void SaveProgress()
        {
            AchievementSaveData saveData = new AchievementSaveData
            {
                unlockedIds = unlockedAchievements.ToArray()
            };
            
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("Achievements_Unlocked", json);
            PlayerPrefs.Save();
        }
    }

    [Serializable]
    public class Achievement
    {
        public string id;
        public string name;
        public string description;
        public AchievementCategory category;
        public string iconName;
        public bool isSecret;
        public int targetProgress = 1;
        public int currentProgress = 0;
        public DateTime unlockedAt;
    }

    [Serializable]
    public class AchievementSaveData
    {
        public string[] unlockedIds;
    }

    public enum AchievementCategory
    {
        Progression,
        Exploration,
        Accessibility,
        Social,
        Mastery,
        Secret
    }
}
