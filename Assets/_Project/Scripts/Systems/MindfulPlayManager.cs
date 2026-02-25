using UnityEngine;
using System;
using AscendantContinuum.Astronomy;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Anti-addiction system that encourages mindful, healthy play
    /// "Digital Sunset" - app gently closes at sunset
    /// "Meditation Rewards" - rewards for taking breaks
    /// </summary>
    public class MindfulPlayManager : MonoBehaviour
    {
        public static MindfulPlayManager Instance { get; private set; }

        [Header("Digital Sunset")]
        [SerializeField] private bool enableDigitalSunset = true;
        [SerializeField] private int sunsetWarningMinutes = 15; // Warn 15 min before sunset

        [Header("Healthy Play Limits")]
        [SerializeField] private int recommendedDailyMinutes = 60; // 1 hour recommended
        [SerializeField] private int maxContinuousMinutes = 45; // Suggest break after 45 min

        [Header("Meditation Rewards")]
        [SerializeField] private int meditationMinutes = 10; // Minimum break time
        [SerializeField] private float meditationBonusMultiplier = 1.5f;

        private DateTime sessionStartTime;
        private DateTime lastBreakTime;
        private bool _started = false;          // guards OnApplicationPause pre-Start
        private float totalPlayTimeToday = 0f; // minutes
        private bool hasShownSunsetWarning = false;
        private bool isMeditationRewardActive = false;
        private bool showFullScreenOverlay = false;
        private string fullScreenOverlayMessage = string.Empty;

        [Header("Digital Sunset UI")]
        [SerializeField] private Color overlayBackgroundColor = new Color(0f, 0f, 0f, 0.88f);
        [SerializeField] private Color overlayTextColor = new Color(0.95f, 0.92f, 0.86f, 1f);

        public Action OnSuggestBreak;
        public Action OnDigitalSunset;
        public Action OnMeditationRewardActivated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Initialise here so OnApplicationPause(false) — which Unity WebGL fires
            // before Start() on first load — doesn't compute a ~2026-year time gap.
            sessionStartTime = DateTime.Now;
            lastBreakTime    = DateTime.Now;
        }

        private void Start()
        {
            // sessionStartTime / lastBreakTime already set in Awake to avoid the
            // pre-Start OnApplicationPause(false) issue on WebGL.
            _started = true;

            LoadPlayTimeProgress();

            // Check for sunset every 5 minutes
            InvokeRepeating(nameof(CheckSunsetTime), 0f, 300f);

            // Check play time every minute
            InvokeRepeating(nameof(CheckPlayTime), 60f, 60f);
        }

        private void CheckSunsetTime()
        {
            if (!enableDigitalSunset) return;

            DateTime now = DateTime.Now;
            DateTime sunset = GetSunsetTime(now);

            TimeSpan timeUntilSunset = sunset - now;

            // Warning phase
            if (timeUntilSunset.TotalMinutes <= sunsetWarningMinutes &&
                timeUntilSunset.TotalMinutes > 0 &&
                !hasShownSunsetWarning)
            {
                ShowSunsetWarning((int)timeUntilSunset.TotalMinutes);
                hasShownSunsetWarning = true;
            }

            // Sunset - time to close
            if (timeUntilSunset.TotalMinutes <= 0 && timeUntilSunset.TotalMinutes > -5)
            {
                TriggerDigitalSunset();
            }

            // Reset warning for next day
            if (timeUntilSunset.TotalHours < -1)
            {
                hasShownSunsetWarning = false;
            }
        }

        private DateTime GetSunsetTime(DateTime date)
        {
            // Simplified sunset calculation
            // For production, use astronomical library or API

            int dayOfYear = date.DayOfYear;

            // Approximate sunset hour based on day of year
            // Summer solstice (day 172) = ~8:30 PM
            // Winter solstice (day 355) = ~4:30 PM

            double sunsetHour;
            if (dayOfYear < 172) // Before summer solstice
            {
                sunsetHour = 16.5 + (dayOfYear / 172.0) * 4.0;
            }
            else // After summer solstice
            {
                sunsetHour = 20.5 - ((dayOfYear - 172) / 183.0) * 4.0;
            }

            int hour = (int)sunsetHour;
            int minute = (int)((sunsetHour - hour) * 60);

            return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
        }

        private void CheckPlayTime()
        {
            TimeSpan sessionDuration = DateTime.Now - sessionStartTime;
            totalPlayTimeToday += 1f; // 1 minute elapsed

            // ADHD mode: use shorter session reminder interval if set
            int adhdReminderMinutes = PlayerPrefs.GetInt("ADHD_SessionReminderMinutes", 0);
            int effectiveMaxMinutes = adhdReminderMinutes > 0 ? adhdReminderMinutes : maxContinuousMinutes;

            // Continuous play check
            if (sessionDuration.TotalMinutes >= effectiveMaxMinutes)
            {
                SuggestBreak();
            }

            // Daily limit check
            if (totalPlayTimeToday >= recommendedDailyMinutes)
            {
                SuggestDailyLimitReached();
            }

            SavePlayTimeProgress();
        }

        private void ShowSunsetWarning(int minutesLeft)
        {
            string message = $"🌅 The sun sets in {minutesLeft} minutes.\n\n" +
                           "Soon, this digital world will close.\n" +
                           "Go outside and witness the real sunset.\n\n" +
                           "The cosmos is more beautiful than any screen. ✨";

            Debug.Log($"[Mindful Play] {message}");

            // Show gentle in-game notification
            // Play peaceful chime
            Core.AudioManager.Instance?.PlaySFX(
                Resources.Load<AudioClip>("Audio/SunsetChime"),
                0.5f
            );
        }

        private void TriggerDigitalSunset()
        {
            Debug.Log("[Mindful Play] 🌅 Digital Sunset - The app gently closes...");

            OnDigitalSunset?.Invoke();

            string message = "🌅 Digital Sunset 🌅\n\n" +
                           "The digital realm fades with the sun.\n" +
                           "Go outside. Look up.\n" +
                           "See the colors paint the sky.\n\n" +
                           "The game will return at sunrise.\n" +
                           "May you touch the earth tonight. 🌙";

            ShowFullScreenMessage(message);

            // Track mindful closure
            Core.FirebaseManager.Instance?.TrackEvent("digital_sunset",
                new System.Collections.Generic.Dictionary<string, object>
            {
                { "play_time_today", totalPlayTimeToday }
            });

            // Wait 5 seconds, then close app
            Invoke(nameof(CloseApp), 5f);
        }

        private void SuggestBreak()
        {
            Debug.Log($"[Mindful Play] You've been playing for {maxContinuousMinutes} minutes. Consider a break! 🌿");

            OnSuggestBreak?.Invoke();

            string message = $"🌿 Gentle Reminder 🌿\n\n" +
                           $"You've been playing for {maxContinuousMinutes} minutes.\n\n" +
                           "Take a break:\n" +
                           "• Look away from screen (20-20-20 rule)\n" +
                           "• Stretch your body\n" +
                           "• Step outside for fresh air\n\n" +
                           $"Close app for {meditationMinutes}+ minutes = bonus rewards! ✨";

            Debug.Log($"[Notification] {message}");

            // Reset session timer
            sessionStartTime = DateTime.Now;
        }

        private void SuggestDailyLimitReached()
        {
            Debug.Log($"[Mindful Play] You've played {recommendedDailyMinutes} minutes today. Well done! 🌟");

            string message = $"⭐ You've honored your practice today! ⭐\n\n" +
                           $"You've played {recommendedDailyMinutes} minutes - a healthy amount.\n\n" +
                           "The game encourages you to:\n" +
                           "• Go experience the real world\n" +
                           "• Look at the actual stars tonight\n" +
                           "• Touch grass (literally!)\n\n" +
                           "You can keep playing, but the universe is calling. 🌌";

            Debug.Log($"[Notification] {message}");
        }

        /// <summary>
        /// Called when app returns from background - check for meditation reward
        /// </summary>
        public void OnReturnFromBackground(TimeSpan timeAway)
        {
            Debug.Log($"[Mindful Play] Welcome back! You were away for {timeAway.TotalMinutes:F1} minutes.");

            // Meditation reward
            if (timeAway.TotalMinutes >= meditationMinutes)
            {
                GrantMeditationReward(timeAway);
            }

            // Check if they went outside during special events
            if (MeteorShowerEvent.Instance != null)
            {
                string activeShower = CosmicDataManager.Instance?.GetActiveMeteorShower();
                if (!string.IsNullOrEmpty(activeShower) && timeAway.TotalMinutes >= 10)
                {
                    GrantMeteorMeditationBonus();
                }
            }
        }

        private void GrantMeditationReward(TimeSpan timeAway)
        {
            isMeditationRewardActive = true;

            Debug.Log($"[Mindful Play] ✨ MEDITATION REWARD! You took a {timeAway.TotalMinutes:F0} minute break.");

            OnMeditationRewardActivated?.Invoke();

            string message = "✨ Meditation Reward Activated! ✨\n\n" +
                           $"Thank you for taking a {timeAway.TotalMinutes:F0} minute break.\n\n" +
                           "Rewards:\n" +
                           $"• {meditationBonusMultiplier}x spark multiplier for next session\n" +
                           "• Bonus plant growth\n" +
                           "• Clear mind = better sigil patterns\n\n" +
                           "The game rewards presence, not addiction. 🌿";

            Debug.Log($"[Notification] {message}");

            // Achievement
            AchievementManager.Instance?.TrackProgress("mindful_player", 1);

            // Track event
            Core.FirebaseManager.Instance?.TrackEvent("meditation_reward",
                new System.Collections.Generic.Dictionary<string, object>
            {
                { "break_minutes", timeAway.TotalMinutes }
            });

            // Bonus active for 30 minutes
            Invoke(nameof(DeactivateMeditationReward), 1800f);
        }

        private void GrantMeteorMeditationBonus()
        {
            Debug.Log("[Mindful Play] 🌠 You went outside during a meteor shower! COSMIC BONUS!");

            string message = "🌠 COSMIC MEDITATION BONUS! 🌠\n\n" +
                           "You left the app during a meteor shower.\n" +
                           "We hope you saw the real stars! ✨\n\n" +
                           "Special Reward:\n" +
                           "• Falling Star Blessing (rare)\n" +
                           "• 3x sparks for 1 hour\n" +
                           "• Cosmic Achievement unlocked\n\n" +
                           "You experienced the real thing. 🌌";

            Debug.Log($"[Notification] {message}");

            AchievementManager.Instance?.TrackProgress("touched_by_starlight", 1);
        }

        private void DeactivateMeditationReward()
        {
            isMeditationRewardActive = false;
            Debug.Log("[Mindful Play] Meditation reward has faded. Take another break to reactivate! 🌿");
        }

        private void ShowFullScreenMessage(string message)
        {
            fullScreenOverlayMessage = message;
            showFullScreenOverlay = true;
            Debug.Log($"[Full Screen] {message}");
        }

        private void OnGUI()
        {
            if (!showFullScreenOverlay) return;

            Color previousGuiColor = GUI.color;
            GUI.color = overlayBackgroundColor;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousGuiColor;

            GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontSize = Mathf.Max(18, Mathf.RoundToInt(Screen.height * 0.03f)),
                normal = { textColor = overlayTextColor }
            };

            float horizontalMargin = Screen.width * 0.12f;
            float verticalMargin = Screen.height * 0.18f;

            Rect messageRect = new Rect(
                horizontalMargin,
                verticalMargin,
                Screen.width - (horizontalMargin * 2f),
                Screen.height - (verticalMargin * 2f));

            GUI.Label(messageRect, fullScreenOverlayMessage, messageStyle);
        }

        private void CloseApp()
        {
            Debug.Log("[Mindful Play] Closing app for digital sunset... 🌅");

            // Save all progress
            Core.SaveSystem.Instance?.SaveGame();

            // Close application
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void LoadPlayTimeProgress()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string savedDate = PlayerPrefs.GetString("PlayTimeDate", "");

            if (savedDate == today)
            {
                totalPlayTimeToday = PlayerPrefs.GetFloat("PlayTimeTodayMinutes", 0f);
            }
            else
            {
                totalPlayTimeToday = 0f;
                SavePlayTimeProgress();
            }
        }

        private void SavePlayTimeProgress()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            PlayerPrefs.SetString("PlayTimeDate", today);
            PlayerPrefs.SetFloat("PlayTimeTodayMinutes", totalPlayTimeToday);
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (_started) SavePlayTimeProgress();
                lastBreakTime = DateTime.Now;
            }
            else
            {
                // Only process "return from background" once the manager has fully started.
                // On first WebGL load, Unity fires OnApplicationPause(false) before Start(),
                // which would produce a ~2026-year time gap with the default DateTime value.
                if (!_started) return;

                TimeSpan timeAway = DateTime.Now - lastBreakTime;
                // Sanity cap: ignore spurious gaps longer than 1 year
                if (timeAway.TotalDays > 365) return;

                OnReturnFromBackground(timeAway);
            }
        }

        // Public API
        public bool IsMeditationRewardActive() => isMeditationRewardActive;
        public float GetMeditationMultiplier() => isMeditationRewardActive ? meditationBonusMultiplier : 1f;
        public float GetPlayTimeToday() => totalPlayTimeToday;
    }
}
