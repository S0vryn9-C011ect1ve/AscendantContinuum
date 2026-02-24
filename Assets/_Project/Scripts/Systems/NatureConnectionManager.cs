using UnityEngine;
using System;
using AscendantContinuum.Core;
using AscendantContinuum.UI;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Encourages real-world nature connection through GPS detection and rewards
    /// "Touch Grass" mechanic - rewards players for going outside
    /// </summary>
    public class NatureConnectionManager : MonoBehaviour
    {
        public static NatureConnectionManager Instance { get; private set; }

        [Header("GPS Settings")]
        [SerializeField] private bool enableGPS = true;
        [SerializeField] private float gpsUpdateInterval = 30f; // Check every 30 seconds

        [Header("Nature Detection")]
        [SerializeField] private float parkDetectionRadius = 100f; // meters

        [Header("Rewards")]
        [SerializeField] private float natureBonusMultiplier = 2f; // 2x sparks in nature
        [SerializeField] private int dailyNatureGoalMinutes = 30; // 30 min outside

        private bool isInNature = false;
        private bool wasInNature = false;
        private DateTime natureSessionStart;
        private float totalNatureTimeToday = 0f; // minutes
        private Vector2 lastKnownLocation;
        private HUDManager hudManager;

        public Action<bool> OnNatureStatusChanged;
        public Action OnNatureGoalAchieved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            hudManager = FindFirstObjectByType<HUDManager>();

            if (enableGPS)
            {
                StartLocationServices();
                InvokeRepeating(nameof(CheckNatureLocation), 0f, gpsUpdateInterval);
            }

            // Load today's nature time
            LoadNatureProgress();
        }

        private void StartLocationServices()
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("[Nature] Location services not enabled. Enable in settings for nature bonuses!");
                return;
            }

            Input.location.Start(10f, 10f); // 10m accuracy, 10m distance filter
            Debug.Log("[Nature] 🌿 GPS started - detecting nature areas...");
        }

        private void CheckNatureLocation()
        {
            if (Input.location.status != LocationServiceStatus.Running)
                return;

            LocationInfo location = Input.location.lastData;
            Vector2 currentLocation = new Vector2(location.latitude, location.longitude);

            // Check if location changed
            if (Vector2.Distance(currentLocation, lastKnownLocation) > 0.0001f)
            {
                lastKnownLocation = currentLocation;
                DetermineNatureStatus(currentLocation, location.altitude);
            }
        }

        private void DetermineNatureStatus(Vector2 location, double altitude)
        {
            // Method 1: Simple heuristics
            // - Check if moving (indicates outdoor activity)
            // - Check altitude (parks often have elevation data)
            // - Time of day (outside during daylight)

            bool likelyOutside = IsLikelyOutside(altitude);

            // Method 2: API integration (for production)
            // Uses parkDetectionRadius (metres) to radius-check against OSM park polygons:
            // - Parks, forests, beaches, trails within parkDetectionRadius metres
            // For now, heuristic only; production will integrate OpenStreetMap Overpass API.
            _ = parkDetectionRadius; // reserved for future OSM integration

            UpdateNatureStatus(likelyOutside);
        }

        private bool IsLikelyOutside(double altitude)
        {
            // Simple heuristics for now:
            // 1. Is it daytime?
            int hour = DateTime.Now.Hour;
            bool isDaytime = hour >= 6 && hour <= 20;

            // 2. Has altitude changed? (indicates movement/outdoor)
            // 3. GPS accuracy is high? (better outdoors)

            // Simplified: if user has GPS on during daytime, assume nature attempt
            return isDaytime;
        }

        private void UpdateNatureStatus(bool inNature)
        {
            wasInNature = isInNature;
            isInNature = inNature;

            if (isInNature && !wasInNature)
            {
                // Just entered nature!
                OnEnterNature();
            }
            else if (!isInNature && wasInNature)
            {
                // Just left nature
                OnLeaveNature();
            }

            // Track time in nature
            if (isInNature)
            {
                float deltaMinutes = gpsUpdateInterval / 60f;
                totalNatureTimeToday += deltaMinutes;

                // Check daily goal
                if (totalNatureTimeToday >= dailyNatureGoalMinutes)
                {
                    AchieveNatureGoal();
                }
            }
        }

        private void OnEnterNature()
        {
            natureSessionStart = DateTime.Now;

            Debug.Log("[Nature] 🌿 You're outside! Nature bonus active: 2x sparks!");

            OnNatureStatusChanged?.Invoke(true);

            ShowNatureWelcome();

            // Apply nature bonuses
            ApplyNatureBonus(true);

            // Track event
            Core.FirebaseManager.Instance?.TrackEvent("entered_nature", null);
        }

        private void OnLeaveNature()
        {
            TimeSpan sessionDuration = DateTime.Now - natureSessionStart;

            Debug.Log($"[Nature] Nature session ended. Duration: {sessionDuration.TotalMinutes:F1} minutes");

            OnNatureStatusChanged?.Invoke(false);

            ShowNatureGoodbye(sessionDuration);

            // Remove nature bonuses
            ApplyNatureBonus(false);

            // Track session
            Core.FirebaseManager.Instance?.TrackEvent("left_nature",
                new System.Collections.Generic.Dictionary<string, object>
            {
                { "duration_minutes", sessionDuration.TotalMinutes }
            });
        }

        private void ApplyNatureBonus(bool enable)
        {
            if (enable)
            {
                // 2x spark multiplier — read by EmberforgeSparks.CollectSpark()
                // Note: Nature bonus stacks multiplicatively with Mars blessing
                float existing = PlayerPrefs.GetFloat("Mars_SparkMultiplier", 1f);
                PlayerPrefs.SetFloat("Nature_SparkMultiplier", 2f);
                // Composite multiplier used by EmberforgeSparks
                PlayerPrefs.SetFloat("Mars_SparkMultiplier", existing * 2f);

                // Faster plant growth — boost Venus multiplier while in nature
                float venusBase = PlayerPrefs.GetFloat("Venus_GrowthMultiplier", 1f);
                float natureGrowthBoost = Mathf.Max(venusBase, 2f);
                PlayerPrefs.SetFloat("Venus_GrowthMultiplier", natureGrowthBoost);
                // Set a rolling expiry: reset 1 h after leaving nature
                float expiry = (float)(DateTime.UtcNow.AddHours(1) -
                    new DateTime(1970, 1, 1)).TotalSeconds;
                PlayerPrefs.SetString("Venus_GrowthExpiry", expiry.ToString(
                    System.Globalization.CultureInfo.InvariantCulture));

                PlayerPrefs.Save();
                Debug.Log("[Nature] \ud83c\udf3f NATURE BONUS ACTIVE: 2x sparks, faster plants!");
            }
            else
            {
                // Restore spark multiplier (remove 2x nature layer)
                float composite = PlayerPrefs.GetFloat("Mars_SparkMultiplier", 2f);
                PlayerPrefs.SetFloat("Mars_SparkMultiplier", composite / 2f);
                PlayerPrefs.SetFloat("Nature_SparkMultiplier", 1f);

                // Plant growth stays for 1 h (set above), no reset needed here
                PlayerPrefs.Save();
                Debug.Log("[Nature] Nature bonus ended. Come back outside soon! \ud83c\udf31");
            }
        }

        private void AchieveNatureGoal()
        {
            Debug.Log($"[Nature] ✨ DAILY NATURE GOAL ACHIEVED! {dailyNatureGoalMinutes} minutes outside! ✨");

            OnNatureGoalAchieved?.Invoke();

            // Grant special reward
            GrantNatureGoalReward();

            // Achievement
            AchievementManager.Instance?.TrackProgress("nature_devotee", 1);
            AchievementManager.Instance?.TrackProgress("touch_grass_daily", 1);
        }

        private void GrantNatureGoalReward()
        {
            // Grant nature-themed sigil pattern unlock
            int natureSignilCount = PlayerPrefs.GetInt("Nature_SigilCount", 0) + 1;
            PlayerPrefs.SetInt($"Sigil_Nature_{natureSignilCount}", 1);
            PlayerPrefs.SetInt("Nature_SigilCount", natureSignilCount);

            // Bonus spark burst (50 sparks credited directly)
            int bankedSparks = PlayerPrefs.GetInt("Banked_BonusSparks", 0) + 50;
            PlayerPrefs.SetInt("Banked_BonusSparks", bankedSparks);

            PlayerPrefs.Save();

            ShowHudNotification("🎁 Nature's Gift received! +50 sparks & nature sigil unlocked!", HUDManager.NotificationType.Reward);
            AchievementManager.Instance?.TrackProgress("nature_gift_received", 1);
            Debug.Log($"[Nature] \ud83c\udf81 Nature's Gift received! Sigil #{natureSignilCount} + 50 bonus sparks");
        }

        private void ShowNatureWelcome()
        {
            string message = "🌿 Welcome to Nature! 🌿\n\n" +
                           "While you're here:\n" +
                           "• 2x spark collection\n" +
                           "• Plants grow faster\n" +
                           "• Special secrets revealed\n\n" +
                           "Enjoy the real world! ✨";

            ShowHudNotification(message, HUDManager.NotificationType.Info);
        }

        private void ShowNatureGoodbye(TimeSpan duration)
        {
            string message = $"🌱 Thanks for spending {duration.TotalMinutes:F0} minutes outside!\n\n" +
                           $"Today's total: {totalNatureTimeToday:F0}/{dailyNatureGoalMinutes} minutes\n\n" +
                           "Come back soon! The earth misses you. 💚";

            ShowHudNotification(message, HUDManager.NotificationType.Reward);
        }

        private void ShowHudNotification(string message, HUDManager.NotificationType notificationType)
        {
            if (hudManager == null)
            {
                hudManager = FindFirstObjectByType<HUDManager>();
            }

            if (hudManager != null)
            {
                hudManager.ShowNotification(message, notificationType);
                return;
            }

            Debug.Log($"[Notification] {message}");
        }

        private void LoadNatureProgress()
        {
            // Load today's nature time from PlayerPrefs
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string savedDate = PlayerPrefs.GetString("NatureDate", "");

            if (savedDate == today)
            {
                totalNatureTimeToday = PlayerPrefs.GetFloat("NatureTodayMinutes", 0f);
            }
            else
            {
                // New day, reset
                totalNatureTimeToday = 0f;
                SaveNatureProgress();
            }
        }

        private void SaveNatureProgress()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            PlayerPrefs.SetString("NatureDate", today);
            PlayerPrefs.SetFloat("NatureTodayMinutes", totalNatureTimeToday);
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveNatureProgress();
            }
        }

        // Public API
        public bool IsInNature() => isInNature;
        public float GetNatureTimeToday() => totalNatureTimeToday;
        public float GetNatureBonusMultiplier() => isInNature ? natureBonusMultiplier : 1f;
        public int GetNatureGoalMinutes() => dailyNatureGoalMinutes;
    }
}
