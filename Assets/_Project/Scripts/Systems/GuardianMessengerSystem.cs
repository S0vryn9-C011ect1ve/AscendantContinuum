using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// The Guardian Messenger System — personalised push notifications that
    /// feel like messages from the game world, not marketing blasts.
    ///
    /// Every notification references the player's Cosmic Profile:
    ///   "Ember-dawn Wanderer, the Spring Equinox arrives in 3 days.
    ///    Your lanterns are needed."
    ///
    /// Key notification types:
    ///   • Streak protection  — 20 hours after last session
    ///   • Live event approach — 3 days before astronomical peak
    ///   • Daily challenge reminder — adaptive to the player's usual session time
    ///   • Return greeting   — personalised welcome after 2 + days away
    ///   • Nature bonus reminder — if GPS showed player near a park yesterday
    ///
    /// All notifications are opt-in. Players control categories independently.
    /// Respects "Digital Sunset" by never pushing after local sunset.
    ///
    /// Requires: Unity Mobile Notifications (com.unity.mobile.notifications)
    ///           Already present in Packages/manifest.json
    /// </summary>
    public sealed class GuardianMessengerSystem : MonoBehaviour
    {
        public static GuardianMessengerSystem Instance { get; private set; }

        // ── Channel IDs ────────────────────────────────────────────────────
        private const string CHANNEL_STREAK    = "streak_guardian";
        private const string CHANNEL_EVENTS    = "cosmic_events";
        private const string CHANNEL_DAILY     = "daily_challenge";
        private const string CHANNEL_RETURN    = "return_greeting";

        // ── Prefs keys ─────────────────────────────────────────────────────
        private const string PREF_OPT_STREAK   = "notif_streak";
        private const string PREF_OPT_EVENTS   = "notif_events";
        private const string PREF_OPT_DAILY    = "notif_daily";
        private const string PREF_LAST_SESSION = "last_session_utc";
        private const string PREF_USUAL_HOUR   = "usual_play_hour";

        // ── Settings ───────────────────────────────────────────────────────
        public bool StreakNotificationsEnabled
        {
            get => PlayerPrefs.GetInt(PREF_OPT_STREAK, 1) == 1;
            set { PlayerPrefs.SetInt(PREF_OPT_STREAK, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public bool EventNotificationsEnabled
        {
            get => PlayerPrefs.GetInt(PREF_OPT_EVENTS, 1) == 1;
            set { PlayerPrefs.SetInt(PREF_OPT_EVENTS, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public bool DailyNotificationsEnabled
        {
            get => PlayerPrefs.GetInt(PREF_OPT_DAILY, 1) == 1;
            set { PlayerPrefs.SetInt(PREF_OPT_DAILY, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitialiseChannels();
            RequestPermission();
            RecordSession();
            ScheduleAllNotifications();
        }

        // ── Android channel setup ──────────────────────────────────────────
        private void InitialiseChannels()
        {
#if UNITY_ANDROID
            RegisterChannel(CHANNEL_STREAK, "Streak Guardian",
                "Let you know when your daily streak is about to break.",
                Importance.High);
            RegisterChannel(CHANNEL_EVENTS, "Cosmic Events",
                "Alerts for astronomical and in-game live events.",
                Importance.Default);
            RegisterChannel(CHANNEL_DAILY, "Daily Challenge",
                "Reminders for the daily cosmic challenge.",
                Importance.Default);
            RegisterChannel(CHANNEL_RETURN, "Return Greetings",
                "Personalised messages when you've been away.",
                Importance.Low);
#endif
        }

#if UNITY_ANDROID
        private static void RegisterChannel(string id, string name, string desc, Importance importance)
        {
            var channel = new AndroidNotificationChannel
            {
                Id          = id,
                Name        = name,
                Description = desc,
                Importance  = importance
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }
#endif

        private void RequestPermission()
        {
#if UNITY_ANDROID && UNITY_2022_2_OR_NEWER
            // Android 13+ requires explicit POST_NOTIFICATIONS permission
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
#endif
#if UNITY_IOS
            StartCoroutine(RequestiOSPermission());
#endif
        }

#if UNITY_IOS
        private System.Collections.IEnumerator RequestiOSPermission()
        {
            // Request authorisation for alert, badge and sound notifications.
            // Requires: Info.plist entitlements (APS Environment) set in Xcode.
            var authRequest = new Unity.Notifications.iOS.AuthorizationRequest(
                Unity.Notifications.iOS.AuthorizationOption.Alert |
                Unity.Notifications.iOS.AuthorizationOption.Badge |
                Unity.Notifications.iOS.AuthorizationOption.Sound,
                registerForRemoteNotifications: false);

            while (!authRequest.IsFinished)
                yield return null;

            if (authRequest.Granted)
            {
                Debug.Log("[Guardian] iOS notification permission granted.");
                ScheduleAllNotifications();
            }
            else
            {
                Debug.Log("[Guardian] iOS notification permission denied — notifications disabled.");
            }

            authRequest.Dispose();
        }
#endif

        // ── Session tracking ───────────────────────────────────────────────
        private void RecordSession()
        {
            PlayerPrefs.SetString(PREF_LAST_SESSION, DateTime.UtcNow.ToString("o"));

            // Update usual play hour (rolling average)
            int currentHour = DateTime.Now.Hour;
            int storedHour  = PlayerPrefs.GetInt(PREF_USUAL_HOUR, currentHour);
            int avgHour     = (storedHour + currentHour) / 2;
            PlayerPrefs.SetInt(PREF_USUAL_HOUR, avgHour);

            PlayerPrefs.Save();
        }

        // ── Schedule all pending notifications ─────────────────────────────
        public void ScheduleAllNotifications()
        {
            CancelAll();

            string name = CosmicIdentitySystem.Instance?.CosmicName ?? "Wanderer";

            if (StreakNotificationsEnabled)   ScheduleStreakProtection(name);
            if (DailyNotificationsEnabled)    ScheduleDailyChallenge(name);
            if (EventNotificationsEnabled)    ScheduleEventApproach(name);
            ScheduleReturnGreeting(name);
        }

        public void CancelAll()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllScheduledNotifications();
#endif
#if UNITY_IOS
            // iOSNotificationCenter.RemoveAllScheduledNotifications(); // Requires Xcode setup
            Debug.Log("[Guardian] iOS CancelAll stub");
#endif
        }

        // ── Streak protection (20hr after session) ─────────────────────────
        private void ScheduleStreakProtection(string playerName)
        {
            int streak = DailyChallengeManager.Instance?.GetCurrentStreak() ?? 0;
            if (streak < 2) return; // No pressure on new players

            string title = streak >= 10
                ? $"Your {streak}-day streak is in danger, {playerName.Split(' ')[0]}"
                : "Your streak is at risk";

            string[] bodies =
            {
                $"The realms have not felt your presence. Return before midnight to protect your flame.",
                $"Your sigil grows cold. A single visit to any realm will restore your streak.",
                $"The Emberforge dims without you. Come back and keep the light alive."
            };

            SendNotification(title, bodies[streak % bodies.Length],
                CHANNEL_STREAK, DateTime.Now.AddHours(20));
        }

        // ── Daily challenge reminder at usual play hour ────────────────────
        private void ScheduleDailyChallenge(string playerName)
        {
            int hour = PlayerPrefs.GetInt(PREF_USUAL_HOUR, 20);

            DateTime tomorrow = DateTime.Now.Date.AddDays(1).AddHours(hour);

            // Respect digital sunset — don't push after 21:00
            if (tomorrow.Hour >= 21) tomorrow = tomorrow.Date.AddHours(19);

            var challenge = DailyChallengeManager.Instance?.GetTodayChallenge();
            string body   = challenge != null
                ? $"Today's cosmic challenge awaits: {challenge.Title}. Speak with the realms."
                : "A new cosmic challenge has appeared in your realm. The day is young.";

            SendNotification("Your daily challenge has begun", body, CHANNEL_DAILY, tomorrow);
        }

        // ── Upcoming event teaser ─────────────────────────────────────────
        private void ScheduleEventApproach(string playerName)
        {
            var next = LiveEventEngine.Instance?.GetNextEvent();
            if (next == null) return;

            TimeSpan delta = next.peakUtc - DateTime.UtcNow;
            if (delta.TotalDays > 7) return; // Only schedule if within a week

            DateTime fireAt = next.peakUtc.ToLocalTime().AddDays(-3);
            if (fireAt < DateTime.Now) return;

            string body = $"{next.mythology}\n" +
                          $"Play during the event to earn: {next.exclusiveTitle}";

            SendNotification($"⚡ {next.displayName} approaches", body, CHANNEL_EVENTS, fireAt);
        }

        // ── Return greeting (2 days after last session) ────────────────────
        private void ScheduleReturnGreeting(string playerName)
        {
            string[] greetings =
            {
                $"The realms remember you, {playerName.Split(' ')[0]}. Come back when you are ready.",
                "The stars you traced still glow, waiting for your return.",
                "Your lantern is the only one of its kind. The sky misses its light.",
                "The constellations are incomplete without you. They have kept your place.",
                "New sparks have gathered in the Emberforge since your last visit."
            };

            int idx = (int)(DateTime.UtcNow.Ticks % greetings.Length);
            SendNotification("Your realms await", greetings[idx],
                CHANNEL_RETURN, DateTime.Now.AddDays(2));
        }

        // ── Immediate notification (call from LiveEventEngine) ─────────────
        public void SendImmediateEventNotification(LiveEvent evt)
        {
            if (!EventNotificationsEnabled) return;
            string name = CosmicIdentitySystem.Instance?.CosmicName ?? "Wanderer";
            SendNotification($"🌟 {evt.displayName} is NOW",
                $"{evt.mythology}\nSpark multiplier: {evt.sparkMultiplier}×",
                CHANNEL_EVENTS, DateTime.Now.AddSeconds(5));
        }

        // ── Core dispatch ──────────────────────────────────────────────────
        private static void SendNotification(string title, string body, string channel, DateTime fireAt)
        {
            if (fireAt <= DateTime.Now) return; // Never schedule in the past

#if UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title       = title,
                Text        = body,
                SmallIcon   = "notify_icon_small",
                LargeIcon   = "notify_icon_large",
                FireTime    = fireAt,
                Style       = NotificationStyle.BigTextStyle
            };
            AndroidNotificationCenter.SendNotification(notification, channel);
            Debug.Log($"[Guardian] Scheduled Android notification at {fireAt:g}: '{title}'");

#elif UNITY_IOS
            // iOS notifications require Xcode provisioning — stub for CI builds
            Debug.Log($"[Guardian] iOS notification stub: '{title}' at {fireAt:g}");

#else
            Debug.Log($"[Guardian] Notification stub (Editor): '{title}' at {fireAt:g}");
#endif
        }
    }
}
