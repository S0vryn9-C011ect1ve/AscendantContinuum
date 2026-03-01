using System;
using UnityEngine;
using AscendantContinuum.Systems;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Manages local push notifications to drive the 7-day retention loop.
    /// Reminds players of daily challenges and sigil crafting.
    /// </summary>
    public class LocalNotificationManager : MonoBehaviour
    {
        public static LocalNotificationManager Instance { get; private set; }

        private const string DailyChallengeChannelId = "daily_challenge_channel";
        private const string SigilCraftingChannelId    = "sigil_crafting_channel";
        private const string LiveEventChannelId         = "live_event_channel";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeChannels();
        }

        private void Start()
        {
            // Schedule tonight-at-8am reminder on startup (respects consent gate)
            ScheduleDailyChallengeReminder();
        }

        // ── Consent gate ────────────────────────────────────────────────

        /// <summary>Returns false when GDPR consent has been explicitly revoked.</summary>
        private bool IsNotificationAllowed()
        {
#if UNITY_EDITOR
            return true;
#else
            // Null = consent manager not yet spawned; allow in that case (pre-consent scene)
            if (GDPRConsentManager.Instance == null) return true;
            return GDPRConsentManager.Instance.HasConsent();
#endif
        }

        private void InitializeChannels()
        {
#if UNITY_ANDROID
            var dailyChannel = new AndroidNotificationChannel()
            {
                Id = DailyChallengeChannelId,
                Name = "Daily Challenges",
                Importance = Importance.Default,
                Description = "Reminders for new daily challenges in the Ascendant Continuum.",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(dailyChannel);

            var sigilChannel = new AndroidNotificationChannel()
            {
                Id = SigilCraftingChannelId,
                Name = "Sigil Crafting",
                Importance = Importance.Default,
                Description = "Notifications when your personal sigil is ready to be crafted.",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(sigilChannel);
#endif
        }

        /// <summary>
        /// Schedules a reminder for the next daily challenge (usually at midnight UTC).
        /// </summary>
        public void ScheduleDailyChallengeReminder()
        {
            if (!IsNotificationAllowed()) return;

            // Calculate time until next midnight UTC
            DateTime now = DateTime.UtcNow;
            DateTime nextMidnight = now.Date.AddDays(1);
            TimeSpan timeUntilMidnight = nextMidnight - now;

            // Add a small buffer (e.g., 8 AM local time) so they don't get pinged at exactly midnight
            DateTime scheduledTime = DateTime.Now.Date.AddDays(1).AddHours(8);

#if UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title = "New Daily Challenge",
                Text = "The cosmos has shifted. A new daily challenge awaits in the Ascendant Continuum.",
                FireTime = scheduledTime,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large"
            };

            AndroidNotificationCenter.SendNotification(notification, DailyChallengeChannelId);
            Debug.Log($"[LocalNotificationManager] Scheduled Daily Challenge reminder for {scheduledTime}");
#elif UNITY_IOS
            var timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = scheduledTime - DateTime.Now,
                Repeats = false
            };

            var notification = new iOSNotification()
            {
                Identifier = "daily_challenge",
                Title = "New Daily Challenge",
                Body = "The cosmos has shifted. A new daily challenge awaits in the Ascendant Continuum.",
                Subtitle = "Ascendant Continuum",
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
            Debug.Log($"[LocalNotificationManager] Scheduled Daily Challenge reminder for {scheduledTime}");
#endif
        }

        /// <summary>
        /// Schedules a reminder when a sigil is ready to be crafted (e.g., after 24 hours).
        /// </summary>
        public void ScheduleSigilCraftingReminder(float hoursDelay = 24f)
        {
            if (!IsNotificationAllowed()) return;

            DateTime scheduledTime = DateTime.Now.AddHours(hoursDelay);

#if UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title = "Sigil Ready",
                Text = "Your personal sigil has finished resonating. Return to craft it.",
                FireTime = scheduledTime,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large"
            };

            AndroidNotificationCenter.SendNotification(notification, SigilCraftingChannelId);
            Debug.Log($"[LocalNotificationManager] Scheduled Sigil Crafting reminder for {scheduledTime}");
#elif UNITY_IOS
            var timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = new TimeSpan(0, (int)(hoursDelay * 60), 0),
                Repeats = false
            };

            var notification = new iOSNotification()
            {
                Identifier = "sigil_crafting",
                Title = "Sigil Ready",
                Body = "Your personal sigil has finished resonating. Return to craft it.",
                Subtitle = "Ascendant Continuum",
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
            Debug.Log($"[LocalNotificationManager] Scheduled Sigil Crafting reminder for {scheduledTime}");
#endif
        }

        /// <summary>
        /// Schedules a push notification ~3 hours before a live event peak.
        /// Called by <see cref="GameBootstrapper"/> when <see cref="LiveEventEngine.OnEventApproaching"/> fires.
        /// </summary>
        public void ScheduleLiveEventNotification(LiveEvent evt)
        {
            if (evt == null) return;
            if (!IsNotificationAllowed()) return;

            // Fire 3 hours before peak — if already past, show in 5 minutes
            DateTime fireTime = evt.peakUtc.ToLocalTime().AddHours(-3);
            if (fireTime <= DateTime.Now) fireTime = DateTime.Now.AddMinutes(5);

            string title = $"\u2726 {evt.displayName}";
            string body  = $"The {evt.displayName} reaches its peak soon. Enter a realm to receive its blessing.";

#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id          = LiveEventChannelId,
                Name        = "Live Events",
                Importance  = Importance.High,
                Description = "Alerts for approaching celestial events in Ascendant Continuum.",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var notification = new AndroidNotification
            {
                Title     = title,
                Text      = body,
                FireTime  = fireTime,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large"
            };
            AndroidNotificationCenter.SendNotification(notification, LiveEventChannelId);
            Debug.Log($"[LocalNotificationManager] Live event notification scheduled: {evt.displayName} at {fireTime}");
#elif UNITY_IOS
            var trigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = fireTime - DateTime.Now,
                Repeats      = false
            };
            var iosNotification = new iOSNotification()
            {
                Identifier                   = $"live_event_{evt.id}",
                Title                        = title,
                Body                         = body,
                Subtitle                     = "Ascendant Continuum",
                ShowInForeground             = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier           = "category_a",
                ThreadIdentifier             = "thread1",
                Trigger                      = trigger,
            };
            iOSNotificationCenter.ScheduleNotification(iosNotification);
            Debug.Log($"[LocalNotificationManager] Live event notification scheduled: {evt.displayName} at {fireTime}");
#else
            Debug.Log($"[LocalNotificationManager] Live event '{evt.displayName}' — no notification platform active.");
#endif
        }

        /// <summary>
        /// Cancels all pending notifications (e.g., if the user logs out or disables notifications).
        /// </summary>
        public void CancelAllNotifications()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
            Debug.Log("[LocalNotificationManager] Cancelled all pending notifications.");
        }
    }
}
