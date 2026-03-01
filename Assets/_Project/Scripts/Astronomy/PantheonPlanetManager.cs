using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.UI;

namespace AscendantContinuum.Astronomy
{
    /// <summary>
    /// Connects Pantheon deities to visible planets in the real sky
    /// Creates deep cosmic connection between game lore and actual universe
    /// </summary>
    public class PantheonPlanetManager : MonoBehaviour
    {
        public static PantheonPlanetManager Instance { get; private set; }

        [System.Serializable]
        public class PlanetaryDeity
        {
            public string deityName;
            public string planetName;
            public Color deityColor;
            public string realm; // Which realm they govern
            public float magnitude; // Brightness (-4 to 6, lower = brighter)
            public string blessing; // What they grant when visible
        }

        [Header("Planetary Deities")]
        [SerializeField]
        private List<PlanetaryDeity> deities = new List<PlanetaryDeity>
        {
            new PlanetaryDeity
            {
                deityName = "The Lightbringer",
                planetName = "Venus",
                deityColor = new Color(0.9f, 0.85f, 0.7f),
                realm = "Dawn Citadel",
                magnitude = -4.6f, // Brightest planet
                blessing = "Love, beauty, new beginnings at dawn"
            },
            new PlanetaryDeity
            {
                deityName = "The Flame Warrior",
                planetName = "Mars",
                deityColor = new Color(0.9f, 0.4f, 0.3f),
                realm = "Emberforge",
                magnitude = -2.9f,
                blessing = "Courage, passion, transformative fire"
            },
            new PlanetaryDeity
            {
                deityName = "The Cosmic Sage",
                planetName = "Jupiter",
                deityColor = new Color(0.8f, 0.7f, 0.5f),
                realm = "Lantern Ascension",
                magnitude = -2.9f, // Second brightest
                blessing = "Wisdom, expansion, divine perspective"
            },
            new PlanetaryDeity
            {
                deityName = "The Timekeeper",
                planetName = "Saturn",
                deityColor = new Color(0.7f, 0.7f, 0.6f),
                realm = "Echo Fields",
                magnitude = 0.5f,
                blessing = "Patience, discipline, sacred cycles"
            },
            new PlanetaryDeity
            {
                deityName = "The Quicksilver Messenger",
                planetName = "Mercury",
                deityColor = new Color(0.7f, 0.7f, 0.7f),
                realm = "Dawn Citadel",
                magnitude = -1.9f,
                blessing = "Communication, swift action, morning clarity"
            }
        };

        public Action<PlanetaryDeity> OnDeityVisible;
        public Action<PlanetaryDeity> OnDeityEncounter;

        private Dictionary<string, bool> visiblePlanets = new Dictionary<string, bool>();
        private DateTime lastCheck;
        private HUDManager hudManager;

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
            hudManager = UnityEngine.Object.FindFirstObjectByType<HUDManager>();
            lastCheck = DateTime.MinValue;
            CheckVisiblePlanets();
            InvokeRepeating(nameof(CheckVisiblePlanets), 0f, 3600f); // Check hourly
        }

        private void CheckVisiblePlanets()
        {
            DateTime now = DateTime.UtcNow;

            foreach (var deity in deities)
            {
                bool wasVisible = visiblePlanets.ContainsKey(deity.planetName) && visiblePlanets[deity.planetName];
                bool isVisible = IsPlanetVisible(deity.planetName, now);

                visiblePlanets[deity.planetName] = isVisible;

                // Deity just became visible!
                if (isVisible && !wasVisible)
                {
                    OnDeityAppears(deity);
                }
            }
        }

        private bool IsPlanetVisible(string planetName, DateTime dateTime)
        {
            // Simplified visibility calculations
            // In production, use astronomical APIs or libraries like AstronomicalAlgorithms

            int dayOfYear = dateTime.DayOfYear;
            int hour = dateTime.Hour;

            switch (planetName)
            {
                case "Venus":
                    // Morning star (Jan-Mar, Oct-Dec) or evening star (Apr-Sep)
                    if (dayOfYear < 90 || dayOfYear > 273) // Morning
                        return hour >= 5 && hour <= 7;
                    else // Evening
                        return hour >= 18 && hour <= 21;

                case "Mars":
                    // Visible most of night when in opposition (varies by year)
                    // Simplified: visible evening/night
                    return hour >= 19 || hour <= 4;

                case "Jupiter":
                    // Bright, visible much of year in evening
                    return hour >= 20 || hour <= 3;

                case "Saturn":
                    // Similar to Jupiter but dimmer
                    return hour >= 21 || hour <= 2;

                case "Mercury":
                    // Hardest to see, only at twilight
                    return (hour >= 6 && hour <= 7) || (hour >= 18 && hour <= 19);

                default:
                    return false;
            }
        }

        private void OnDeityAppears(PlanetaryDeity deity)
        {
            Debug.Log($"[Pantheon] 🌟 {deity.deityName} appears in the sky! {deity.planetName} is visible.");

            OnDeityVisible?.Invoke(deity);

            // Send notification to player
            ShowDeityNotification(deity);

            // Track event
            Core.FirebaseManager.Instance?.TrackEvent("deity_appears",
                new Dictionary<string, object>
            {
                { "deity", deity.deityName },
                { "planet", deity.planetName },
                { "realm", deity.realm }
            });
        }

        private void ShowDeityNotification(PlanetaryDeity deity)
        {
            string direction = GetPlanetDirection(deity.planetName);
            string message = $"🌟 {deity.deityName} appears in the {direction} sky!\n" +
                           $"Go outside and look for {deity.planetName}.\n" +
                           $"Find it to receive: {deity.blessing}";

            ShowHudNotification(message, HUDManager.NotificationType.Info);
        }

        private void ShowHudNotification(string message, HUDManager.NotificationType notificationType)
        {
            if (hudManager == null)
            {
                hudManager = UnityEngine.Object.FindFirstObjectByType<HUDManager>();
            }

            if (hudManager != null)
            {
                hudManager.ShowNotification(message, notificationType);
                return;
            }

            Debug.Log($"[Notification] {message}");
        }

        private string GetPlanetDirection(string planetName)
        {
            int hour = DateTime.UtcNow.Hour;

            // Simplified directional guidance
            switch (planetName)
            {
                case "Venus":
                    return hour < 12 ? "eastern (morning)" : "western (evening)";
                case "Mars":
                case "Jupiter":
                case "Saturn":
                    return "southern";
                case "Mercury":
                    return hour < 12 ? "eastern (dawn)" : "western (dusk)";
                default:
                    return "night";
            }
        }

        /// <summary>
        /// Player claims they saw the planet - start verification
        /// </summary>
        public void ClaimPlanetSighting(string planetName)
        {
            var deity = deities.Find(d => d.planetName == planetName);
            if (deity == null) return;

            if (!visiblePlanets.ContainsKey(planetName) || !visiblePlanets[planetName])
            {
                Debug.Log($"[Pantheon] {planetName} is not visible right now. Try again when it appears!");
                return;
            }

            // Start verification flow
            StartPlanetVerification(deity);
        }

        private void StartPlanetVerification(PlanetaryDeity deity)
        {
            Debug.Log($"[Pantheon] Starting verification for {deity.deityName} ({deity.planetName})");

            // Verification steps:
            // 1. Check if player has been outside (GPS movement)
            // 2. Check time spent away from app (should be at least 2-5 minutes)
            // 3. Optional: Camera verification (advanced)

            // For now, simple time-based verification
            DateTime appPausedTime = GetAppPausedTime();
            TimeSpan timeAway = DateTime.UtcNow - appPausedTime;

            if (timeAway.TotalMinutes >= 2)
            {
                // Player was away long enough to actually look!
                GrantDeityBlessing(deity);
            }
            else
            {
                Debug.Log("[Pantheon] Go outside and actually look for at least 2 minutes! ✨");
                ShowEncouragementMessage();
            }
        }

        private void GrantDeityBlessing(PlanetaryDeity deity)
        {
            Debug.Log($"[Pantheon] 🎁 {deity.deityName} grants their blessing: {deity.blessing}");

            OnDeityEncounter?.Invoke(deity);

            // Grant rewards based on deity
            ApplyDeityBlessings(deity);

            // Achievement tracking
            Systems.AchievementManager.Instance?.TrackProgress("planet_seeker", 1);
            Systems.AchievementManager.Instance?.TrackProgress($"deity_{deity.deityName.ToLower().Replace(" ", "_")}", 1);

            // Show beautiful blessing UI
            ShowBlessingReceived(deity);
        }

        private void ApplyDeityBlessings(PlanetaryDeity deity)
        {
            switch (deity.planetName)
            {
                case "Venus":
                    // Love/beauty — 2x plant growth multiplier for 24 h
                    ApplyGrowthMultiplier(2f, TimeSpan.FromHours(24));
                    Debug.Log("[PantheonPlanet] Venus Blessing: Plants bloom 2x faster for 24 hours");
                    break;

                case "Mars":
                    // Courage — 2x spark multiplier for the current session
                    ApplySparkMultiplier(2f);
                    Debug.Log("[PantheonPlanet] Mars Blessing: 2x sparks for this session");
                    break;

                case "Jupiter":
                    // Wisdom — unlock a special constellation story fragment
                    UnlockConstellationStory(deity);
                    Debug.Log("[PantheonPlanet] Jupiter Blessing: Ancient wisdom revealed");
                    break;

                case "Saturn":
                    // Time — add a bonus daily-challenge streak day
                    ApplySaturnTimeReward();
                    Debug.Log("[PantheonPlanet] Saturn Blessing: Bonus challenge streak awarded");
                    break;

                case "Mercury":
                    // Communication — unlock a rare sigil pattern
                    GrantRareSigilPattern();
                    Debug.Log("[PantheonPlanet] Mercury Blessing: Rare sigil pattern unlocked");
                    break;
            }
        }

        // ── Blessing implementations ──────────────────────────────────────────

        private void ApplyGrowthMultiplier(float multiplier, TimeSpan duration)
        {
            float expiry = (float)(DateTime.UtcNow + duration - new DateTime(1970, 1, 1)).TotalSeconds;
            PlayerPrefs.SetFloat("Venus_GrowthMultiplier", multiplier);
            PlayerPrefs.SetFloat("Venus_GrowthExpiry", expiry);
            PlayerPrefs.Save();
        }

        private void ApplySparkMultiplier(float multiplier)
        {
            // Session-scoped: cleared by EmberforgeSparks on scene unload
            PlayerPrefs.SetFloat("Mars_SparkMultiplier", multiplier);
            PlayerPrefs.Save();
        }

        private void UnlockConstellationStory(PlanetaryDeity deity)
        {
            string storyKey = $"Jupiter_Story_{deity.deityName.Replace(" ", "_")}_Unlocked";
            if (PlayerPrefs.GetInt(storyKey, 0) == 0)
            {
                PlayerPrefs.SetInt(storyKey, 1);
                PlayerPrefs.Save();
                Systems.AchievementManager.Instance?.TrackProgress("constellation_story_unlocked", 1);
            }
        }

        private void ApplySaturnTimeReward()
        {
            int currentStreak = PlayerPrefs.GetInt("DailyChallenge_Streak", 0);
            PlayerPrefs.SetInt("DailyChallenge_Streak", currentStreak + 1);
            PlayerPrefs.Save();
            Systems.AchievementManager.Instance?.TrackProgress("saturn_streak_bonus", 1);
        }

        private void GrantRareSigilPattern()
        {
            int patternsUnlocked = PlayerPrefs.GetInt("Mercury_RareSigilCount", 0);
            string patternKey = $"Sigil_Mercury_Rare_{patternsUnlocked + 1}";
            PlayerPrefs.SetInt(patternKey, 1);
            PlayerPrefs.SetInt("Mercury_RareSigilCount", patternsUnlocked + 1);
            PlayerPrefs.Save();
            Systems.AchievementManager.Instance?.TrackProgress("rare_sigil_unlocked", 1);
        }

        // ─────────────────────────────────────────────────────────────────────

        private DateTime GetAppPausedTime()
        {
            // Track actual app pause time via GameManager.OnApplicationPause
            float pausedAtUnix = PlayerPrefs.GetFloat("App_LastPausedUnixTime", 0f);
            if (pausedAtUnix > 0f)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(pausedAtUnix);
            }
            // Fallback: assume paused 5 minutes ago
            return DateTime.UtcNow.AddMinutes(-5);
        }

        private void ShowEncouragementMessage()
        {
            Debug.Log("✨ The cosmos is waiting. Step outside and truly look up. ✨");
        }

        private void ShowBlessingReceived(PlanetaryDeity deity)
        {
            string message = $"⭐ {deity.deityName} sees you ⭐\n\n" +
                           $"You gazed upon {deity.planetName} in the heavens.\n" +
                           $"You are blessed with: {deity.blessing}\n\n" +
                           $"Realm power increased in: {deity.realm}";

            Debug.Log($"[Blessing] {message}");
        }

        // Public API
        public bool IsPlanetVisibleNow(string planetName)
        {
            return visiblePlanets.ContainsKey(planetName) && visiblePlanets[planetName];
        }

        public List<PlanetaryDeity> GetVisibleDeitiesNow()
        {
            List<PlanetaryDeity> visible = new List<PlanetaryDeity>();
            foreach (var deity in deities)
            {
                if (IsPlanetVisibleNow(deity.planetName))
                {
                    visible.Add(deity);
                }
            }
            return visible;
        }

        public PlanetaryDeity GetDeityForRealm(string realmName)
        {
            return deities.Find(d => d.realm == realmName);
        }
    }
}
