using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using AscendantContinuum.Core;

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

            // ── Cosmic / Astronomy achievements ─────────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "lunar_devotee",
                name = "Lunar Devotee",
                description = "Play the game through all 8 moon phases",
                category = AchievementCategory.Exploration,
                iconName = "icon_moon",
                isSecret = false,
                targetProgress = 8
            });

            allAchievements.Add(new Achievement
            {
                id = "meteor_hunter",
                name = "Meteor Hunter",
                description = "Collect sparks during a meteor shower event",
                category = AchievementCategory.Exploration,
                iconName = "icon_meteor",
                isSecret = false,
                targetProgress = 1
            });

            allAchievements.Add(new Achievement
            {
                id = "stargazer",
                name = "Stargazer",
                description = "Complete constellation traces in all 4 seasons",
                category = AchievementCategory.Mastery,
                iconName = "icon_stars",
                isSecret = false,
                targetProgress = 4
            });

            allAchievements.Add(new Achievement
            {
                id = "solstice_seeker",
                name = "Solstice Seeker",
                description = "Play on both the summer and winter solstice",
                category = AchievementCategory.Secret,
                iconName = "icon_solstice",
                isSecret = true,
                targetProgress = 2
            });

            allAchievements.Add(new Achievement
            {
                id = "equinox_keeper",
                name = "Equinox Keeper",
                description = "Play on both the spring and autumn equinox",
                category = AchievementCategory.Secret,
                iconName = "icon_equinox",
                isSecret = true,
                targetProgress = 2
            });

            allAchievements.Add(new Achievement
            {
                id = "blood_moon_witness",
                name = "Blood Moon Witness",
                description = "Discover the rare secrets that appear under a blood moon",
                category = AchievementCategory.Secret,
                iconName = "icon_blood_moon",
                isSecret = true
            });

            allAchievements.Add(new Achievement
            {
                id = "once_in_a_blue_moon",
                name = "Once in a Blue Moon",
                description = "Log in on a blue moon (second full moon in a month)",
                category = AchievementCategory.Secret,
                iconName = "icon_blue_moon",
                isSecret = true
            });

            allAchievements.Add(new Achievement
            {
                id = "supermoon_powered",
                name = "Supermoon Powered",
                description = "Collect 100 sparks during a supermoon",
                category = AchievementCategory.Mastery,
                iconName = "icon_supermoon",
                isSecret = false,
                targetProgress = 100
            });

            // ── Serendipity achievements ─────────────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "first_serendipity",
                name = "A Touch of Magic",
                description = "Experience your first serendipity moment",
                category = AchievementCategory.Secret,
                iconName = "icon_serendipity",
                isSecret = true
            });

            allAchievements.Add(new Achievement
            {
                id = "deity_touched",
                name = "Chosen by the Divine",
                description = "A deity appeared before you — legendary serendipity!",
                category = AchievementCategory.Secret,
                iconName = "icon_deity",
                isSecret = true
            });

            allAchievements.Add(new Achievement
            {
                id = "rare_encounter",
                name = "Rare Encounter",
                description = "Witness a Rare or higher serendipity event 5 times",
                category = AchievementCategory.Mastery,
                iconName = "icon_rare",
                isSecret = false,
                targetProgress = 5
            });

            allAchievements.Add(new Achievement
            {
                id = "screen_reader_storyteller",
                name = "The Listening Ear",
                description = "Hear an NPC whisper a secret only the screen reader can reveal",
                category = AchievementCategory.Accessibility,
                iconName = "icon_screen_reader",
                isSecret = true
            });

            // ── Kindness Chain achievements ──────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "first_kindness",
                name = "First Blessing",
                description = "Send your first anonymous blessing to a stranger",
                category = AchievementCategory.Social,
                iconName = "icon_kindness",
                isSecret = false
            });

            allAchievements.Add(new Achievement
            {
                id = "kindness_giver",
                name = "The Compassionate Seeker",
                description = "Send 50 anonymous blessings",
                category = AchievementCategory.Social,
                iconName = "icon_kindness_giver",
                isSecret = false,
                targetProgress = 50
            });

            allAchievements.Add(new Achievement
            {
                id = "first_blessing_received",
                name = "You Are Not Forgotten",
                description = "Receive your first blessing from a stranger",
                category = AchievementCategory.Social,
                iconName = "icon_blessing_received",
                isSecret = true
            });

            // ── Real Stargazing achievement ─────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "real_stargazer",
                name = "Look Up",
                description = "Hold your phone toward the real night sky for 30 seconds",
                category = AchievementCategory.Exploration,
                iconName = "icon_telescope",
                isSecret = false
            });

            // ── Echo Archive achievements ──────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "founder_echo_discovered",
                name = "Touching the Origin",
                description = "Discover a Founder echo — left by a launch-week seeker",
                category = AchievementCategory.Secret,
                iconName = "icon_founder",
                isSecret = true
            });

            allAchievements.Add(new Achievement
            {
                id = "time_traveler",
                name = "Time Traveler",
                description = "Discover a memory echo that is over one year old",
                category = AchievementCategory.Secret,
                iconName = "icon_time_travel",
                isSecret = true
            });

            // ── NPC Collective Memory achievements ─────────────────
            allAchievements.Add(new Achievement
            {
                id = "npc_memory_witness",
                name = "The Collective Voice",
                description = "Witness an NPC evolve based on the community's collective emotions",
                category = AchievementCategory.Secret,
                iconName = "icon_npc_evolve",
                isSecret = true
            });

            allAchievements.Add(new Achievement
            {
                id = "hall_of_fame_pattern",
                name = "Pattern Enshrined",
                description = "Your constellation pattern is traced enough times to enter the Hall of Fame",
                category = AchievementCategory.Mastery,
                iconName = "icon_hall_of_fame",
                isSecret = false
            });

            // ── Community Mystery achievements ─────────────────────
            allAchievements.Add(new Achievement
            {
                id = "seventh_realm_seeker",
                name = "Seeker of the Seventh",
                description = "Discover all five clues hidden across the realms",
                category = AchievementCategory.Exploration,
                iconName = "icon_seventh_realm",
                isSecret = true,
                targetProgress = 5
            });

            allAchievements.Add(new Achievement
            {
                id = "seventh_realm_unlocked",
                name = "Twilight Nexus",
                description = "Solve the greatest mystery — the Seventh Realm stirs",
                category = AchievementCategory.Secret,
                iconName = "icon_twilight_nexus",
                isSecret = true
            });

            allAchievements.Add(new Achievement
            {
                id = "origin_story_unlocked",
                name = "The First Seeker Revealed",
                description = "Collect all six First Seeker story fragments",
                category = AchievementCategory.Secret,
                iconName = "icon_origin",
                isSecret = true
            });

            allAchievements.Add(new Achievement
            {
                id = "prophecy_seer",
                name = "Prophet of Stars",
                description = "Discover the Prophecy Constellation during a solar eclipse window",
                category = AchievementCategory.Secret,
                iconName = "icon_prophecy",
                isSecret = true
            });

            // ── Sigil Crafting achievements ────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "first_namer",
                name = "The First Namer",
                description = "Be the first to discover and name a new sigil combination",
                category = AchievementCategory.Exploration,
                iconName = "icon_first_namer",
                isSecret = false
            });

            // ── Cosmic Patron achievement ──────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "cosmic_patron_supporter",
                name = "Cosmic Patron",
                description = "Support the game as a Cosmic Patron",
                category = AchievementCategory.Social,
                iconName = "icon_patron",
                isSecret = false
            });

            // ── Living Lore achievements ───────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "age_of_harmony",
                name = "Age of Harmony",
                description = "Witness the Age of Harmony — all six deities perfectly balanced",
                category = AchievementCategory.Secret,
                iconName = "icon_harmony",
                isSecret = true
            });

            // ── Treasure Hunt achievements ─────────────────────────
            allAchievements.Add(new Achievement
            {
                id = "treasure_hunter",
                name = "Cosmic Treasure Hunter",
                description = "Find 30 hidden treasures across all realms",
                category = AchievementCategory.Exploration,
                iconName = "icon_treasure",
                isSecret = false,
                targetProgress = 30
            });

            allAchievements.Add(new Achievement
            {
                id = "cosmic_key_holder",
                name = "Key Keeper",
                description = "Discover the monthly Cosmic Key treasure",
                category = AchievementCategory.Secret,
                iconName = "icon_cosmic_key",
                isSecret = true
            });

            // ── Cross-Player Puzzle Chain achievements ─────────────
            allAchievements.Add(new Achievement
            {
                id = "elemental_master",
                name = "Elemental Master",
                description = "Complete a full five-realm Elemental Chain",
                category = AchievementCategory.Mastery,
                iconName = "icon_elemental",
                isSecret = false
            });

            allAchievements.Add(new Achievement
            {
                id = "chain_master",
                name = "Chain Master",
                description = "Complete 10 cross-player puzzle chains",
                category = AchievementCategory.Mastery,
                iconName = "icon_chain_master",
                isSecret = false,
                targetProgress = 10
            });

            Debug.Log($"[AchievementManager] Initialized {allAchievements.Count} achievements");
        }

        private void RegisterEventListeners()
        {
            // Listen to game events
            var spark = Object.FindObjectOfType<Emberforge.EmberforgeSparks>();
            if (spark != null)
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

            // Notify Cosmic Identity System so aura tier can evolve
            CosmicIdentitySystem.Instance?.RecordAchievement(achievementId);

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

        public List<Achievement> GetAllAchievements()
        {
            return new List<Achievement>(allAchievements);
        }

        public int GetTotalAchievementCount() => allAchievements.Count;

        public int GetUnlockedCount() => unlockedAchievements.Count;

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

        // UI-facing convenience properties
        public string Title => name;
        public string Description => description;
        public string Category => category.ToString();
        public bool IsLocked => unlockedAt == default;
        public bool IsHidden => isSecret;
        public int CurrentProgress => currentProgress;
        public int RequiredProgress => targetProgress;
        public System.DateTime UnlockDate => unlockedAt;
        public Sprite Icon { get; set; } // Assigned at runtime from iconName
        public string RewardDescription { get; set; }
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
