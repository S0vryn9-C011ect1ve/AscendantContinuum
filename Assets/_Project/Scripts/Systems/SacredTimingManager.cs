using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.UI;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Manages sacred astronomical events: equinoxes, solstices, eclipses
    /// Creates reverence for natural cosmic cycles
    /// </summary>
    public class SacredTimingManager : MonoBehaviour
    {
        public static SacredTimingManager Instance { get; private set; }

        [System.Serializable]
        public class SacredEvent
        {
            public string eventName;
            public DateTime eventDate;
            public string significance;
            public Color eventColor;
            public float powerMultiplier;
            public string specialRealm; // Which realm peaks during this event
        }

        [Header("2026 Sacred Events")]
        private List<SacredEvent> sacredEvents;

        public Action<SacredEvent> OnSacredEventBegins;
        public Action<SacredEvent> OnSacredEventPeak;
        public Action<SacredEvent> OnSacredEventEnds;

        private SacredEvent currentEvent;
        private bool isEventActive = false;
        private HUDManager hudManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeSacredEvents2026();
        }

        private void Start()
        {
            hudManager = Object.FindObjectOfType<HUDManager>();
            CheckForSacredEvents();
            InvokeRepeating(nameof(CheckForSacredEvents), 0f, 3600f); // Check hourly
        }

        private void InitializeSacredEvents2026()
        {
            sacredEvents = new List<SacredEvent>
            {
                // EQUINOXES - Balance, Harmony
                new SacredEvent
                {
                    eventName = "Spring Equinox",
                    eventDate = new DateTime(2026, 3, 20, 9, 46, 0, DateTimeKind.Utc),
                    significance = "Day equals night. Perfect balance. Time of renewal and growth.",
                    eventColor = new Color(0.5f, 0.9f, 0.5f),
                    powerMultiplier = 1.5f,
                    specialRealm = "Verdant Sanctuary"
                },
                new SacredEvent
                {
                    eventName = "Autumn Equinox",
                    eventDate = new DateTime(2026, 9, 22, 18, 5, 0, DateTimeKind.Utc),
                    significance = "Day equals night. Harvest time. Gratitude and reflection.",
                    eventColor = new Color(0.9f, 0.6f, 0.3f),
                    powerMultiplier = 1.5f,
                    specialRealm = "Verdant Sanctuary"
                },
                
                // SOLSTICES - Extremes, Transformation
                new SacredEvent
                {
                    eventName = "Summer Solstice",
                    eventDate = new DateTime(2026, 6, 21, 2, 24, 0, DateTimeKind.Utc),
                    significance = "Longest day. Peak solar power. Maximum light and energy.",
                    eventColor = new Color(1f, 0.9f, 0.3f),
                    powerMultiplier = 2f,
                    specialRealm = "Dawn Citadel"
                },
                new SacredEvent
                {
                    eventName = "Winter Solstice",
                    eventDate = new DateTime(2026, 12, 21, 14, 3, 0, DateTimeKind.Utc),
                    significance = "Longest night. Deep reflection. The light returns from darkness.",
                    eventColor = new Color(0.3f, 0.3f, 0.6f),
                    powerMultiplier = 2f,
                    specialRealm = "Lantern Ascension"
                },
                
                // CROSS-QUARTER DAYS - Celtic festivals
                new SacredEvent
                {
                    eventName = "Imbolc",
                    eventDate = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc),
                    significance = "First stirrings of spring. Purification and new beginnings.",
                    eventColor = new Color(0.9f, 0.9f, 1f),
                    powerMultiplier = 1.3f,
                    specialRealm = "Dawn Citadel"
                },
                new SacredEvent
                {
                    eventName = "Beltane",
                    eventDate = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc),
                    significance = "Peak spring. Fertility, life force, and transformation.",
                    eventColor = new Color(0.9f, 0.5f, 0.9f),
                    powerMultiplier = 1.5f,
                    specialRealm = "Emberforge"
                },
                new SacredEvent
                {
                    eventName = "Lammas",
                    eventDate = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                    significance = "First harvest. Gratitude for abundance.",
                    eventColor = new Color(0.9f, 0.7f, 0.3f),
                    powerMultiplier = 1.3f,
                    specialRealm = "Verdant Sanctuary"
                },
                new SacredEvent
                {
                    eventName = "Samhain",
                    eventDate = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc),
                    significance = "Thinning veil between worlds. Ancestral connection.",
                    eventColor = new Color(0.6f, 0.3f, 0.9f),
                    powerMultiplier = 1.5f,
                    specialRealm = "Echo Fields"
                },
                
                // LUNAR ECLIPSES 2026
                new SacredEvent
                {
                    eventName = "Total Lunar Eclipse",
                    eventDate = new DateTime(2026, 3, 3, 11, 33, 0, DateTimeKind.Utc),
                    significance = "Blood Moon. Transformation and shadow work. Rare cosmic alignment.",
                    eventColor = new Color(0.7f, 0.2f, 0.2f),
                    powerMultiplier = 3f,
                    specialRealm = "All Realms"
                },
                
                // SOLAR ECLIPSES 2026
                new SacredEvent
                {
                    eventName = "Total Solar Eclipse",
                    eventDate = new DateTime(2026, 8, 12, 17, 47, 0, DateTimeKind.Utc),
                    significance = "The sun dies and is reborn. Ultimate transformation. Path of totality crosses Earth.",
                    eventColor = new Color(0.1f, 0.1f, 0.1f),
                    powerMultiplier = 5f, // MAXIMUM POWER
                    specialRealm = "All Realms"
                }
            };
        }

        private void CheckForSacredEvents()
        {
            DateTime now = DateTime.UtcNow;

            foreach (var sacredEvent in sacredEvents)
            {
                // Check if we're within event window (3 days before to 3 days after)
                TimeSpan timeDifference = sacredEvent.eventDate - now;
                double daysUntil = timeDifference.TotalDays;

                if (daysUntil >= -3 && daysUntil <= 3)
                {
                    if (!isEventActive || currentEvent != sacredEvent)
                    {
                        ActivateSacredEvent(sacredEvent, daysUntil);
                    }

                    // Peak moment (within 1 hour of exact time)
                    if (Math.Abs(timeDifference.TotalHours) < 1)
                    {
                        OnEventPeak(sacredEvent);
                    }

                    return; // Only one event at a time
                }
            }

            // No events active
            if (isEventActive)
            {
                DeactivateSacredEvent();
            }
        }

        private void ActivateSacredEvent(SacredEvent sacredEvent, double daysUntil)
        {
            currentEvent = sacredEvent;
            isEventActive = true;

            Debug.Log($"[Sacred Timing] 🌟 {sacredEvent.eventName} approaches! ({Math.Abs(daysUntil):F1} days)");

            OnSacredEventBegins?.Invoke(sacredEvent);

            // Show notification
            ShowEventNotification(sacredEvent, daysUntil);

            // Apply event effects
            ApplyEventEffects(sacredEvent);

            // Track event
            Core.FirebaseManager.Instance?.TrackEvent("sacred_event_active",
                new Dictionary<string, object>
            {
                { "event", sacredEvent.eventName },
                { "days_until", daysUntil },
                { "realm", sacredEvent.specialRealm }
            });
        }

        private void OnEventPeak(SacredEvent sacredEvent)
        {
            Debug.Log($"[Sacred Timing] ⭐ {sacredEvent.eventName} PEAK MOMENT! ⭐");

            OnSacredEventPeak?.Invoke(sacredEvent);

            // Special peak effects
            ApplyPeakEffects(sacredEvent);

            // Achievement
            AchievementManager.Instance?.TrackProgress("witness_sacred_moment", 1);
            AchievementManager.Instance?.TrackProgress($"witnessed_{sacredEvent.eventName.ToLower().Replace(" ", "_")}", 1);
        }

        private void DeactivateSacredEvent()
        {
            if (currentEvent == null) return;

            Debug.Log($"[Sacred Timing] {currentEvent.eventName} has passed.");

            OnSacredEventEnds?.Invoke(currentEvent);

            isEventActive = false;
            currentEvent = null;
        }

        private void ShowEventNotification(SacredEvent sacredEvent, double daysUntil)
        {
            string timing = daysUntil > 0
                ? $"in {Math.Abs(daysUntil):F1} days"
                : daysUntil < -1
                    ? $"{Math.Abs(daysUntil):F1} days ago"
                    : "NOW";

            // Autism mode: use calm, predictive language instead of surprise framing
            bool autismMode = PlayerPrefs.GetInt("Autism_ReduceSurprises", 0) == 1;
            string message;
            if (autismMode)
            {
                // Predictive warning — give full advance notice and avoid exclamation-heavy text
                message = $"Upcoming change: {sacredEvent.eventName}\n\n" +
                          $"This event will begin {timing}.\n" +
                          $"{sacredEvent.significance}\n\n" +
                          $"Power multiplier: {sacredEvent.powerMultiplier}x\n" +
                          $"Affected area: {sacredEvent.specialRealm}\n\n" +
                          "You will receive another notice when it starts.";
            }
            else
            {
                message = $"🌟 Sacred Event: {sacredEvent.eventName} 🌟\n\n" +
                          $"Time: {timing}\n" +
                          $"{sacredEvent.significance}\n\n" +
                          $"Power multiplier: {sacredEvent.powerMultiplier}x\n" +
                          $"Special realm: {sacredEvent.specialRealm}";
            }

            ShowHudNotification(message, HUDManager.NotificationType.Info);
        }

        private void ShowHudNotification(string message, HUDManager.NotificationType notificationType)
        {
            if (hudManager == null)
            {
                hudManager = Object.FindObjectOfType<HUDManager>();
            }

            if (hudManager != null)
            {
                hudManager.ShowNotification(message, notificationType);
                return;
            }

            Debug.Log($"[Notification] {message}");
        }

        private void ApplyEventEffects(SacredEvent sacredEvent)
        {
            // Apply power multiplier globally or to specific realm
            Debug.Log($"[Sacred Timing] Applying {sacredEvent.powerMultiplier}x power multiplier");

            // Autism mode: skip abrupt ambient color change — transition is jarring without warning
            bool autismMode = PlayerPrefs.GetInt("Autism_ReduceSurprises", 0) == 1;
            if (!autismMode)
            {
                // Change ambient colors (only for non-autism mode; transition happens instantly)
                RenderSettings.ambientLight = sacredEvent.eventColor;
            }

            // Modify gameplay
            if (sacredEvent.eventName.Contains("Eclipse"))
            {
                // Eclipses reveal ALL hidden secrets
                RevealAllSecrets();
            }

            if (sacredEvent.eventName.Contains("Equinox"))
            {
                // Equinoxes create balance challenges
                EnableBalancePuzzles();
            }

            if (sacredEvent.eventName.Contains("Solstice"))
            {
                // Solstices maximize light/dark powers
                MaximizeSolsticePower(sacredEvent);
            }
        }

        private void ApplyPeakEffects(SacredEvent sacredEvent)
        {
            // Grant special rewards for being present at exact moment
            Debug.Log($"[Sacred Timing] ✨ You witnessed {sacredEvent.eventName} at its peak! ✨");

            // Autism mode: no unexpected haptic burst at peak — announce it instead
            bool autismMode = PlayerPrefs.GetInt("Autism_ReduceSurprises", 0) == 1;
            if (autismMode)
            {
                ShowHudNotification(
                    $"Peak moment reached: {sacredEvent.eventName}\nBonus rewards applied.",
                    HUDManager.NotificationType.Info);
            }
            else
            {
                Core.AccessibilityManager.Instance?.TriggerHaptic(Core.HapticType.Success);
            }
        }

        private void RevealAllSecrets()
        {
            // During eclipses, all hidden secrets become visible
            GameObject[] allSecrets = GameObject.FindGameObjectsWithTag("Secret");
            foreach (var secret in allSecrets)
            {
                secret.SetActive(true);
            }
            Debug.Log("[Sacred Timing] 🌑 All secrets revealed during eclipse! 🌑");
        }

        private void EnableBalancePuzzles()
        {
            // Equinox: activate special balance-challenge rooms / gameplay flag
            PlayerPrefs.SetInt("Sacred_BalancePuzzlesActive", 1);
            PlayerPrefs.SetString("Sacred_BalancePuzzleExpiry",
                DateTime.UtcNow.AddHours(48).ToString("o")); // active for 48 h
            PlayerPrefs.Save();

            // Boost all damage/healing to be equal (balance mechanic)
            Shader.SetGlobalFloat("_BalanceMode", 1f);

            Systems.AchievementManager.Instance?.TrackProgress("equinox_balance", 1);
            Debug.Log("[Sacred Timing] ⚖️ Balance puzzles activated for equinox (48 h window)");
        }

        private void MaximizeSolsticePower(SacredEvent sacredEvent)
        {
            if (sacredEvent.eventName.Contains("Summer"))
            {
                // Light-based abilities: solar spark bonuses and brighter VFX
                PlayerPrefs.SetFloat("Solstice_LightMultiplier", 2f);
                PlayerPrefs.SetString("Solstice_LightExpiry",
                    DateTime.UtcNow.AddHours(24).ToString("o"));
                Shader.SetGlobalFloat("_SolarPowerBoost", 1f);
                Shader.SetGlobalFloat("_LunarPowerBoost", 0f);
                Systems.AchievementManager.Instance?.TrackProgress("summer_solstice", 1);
                Debug.Log("[Sacred Timing] ☀️ Summer solstice: solar power doubled for 24 h!");
            }
            else if (sacredEvent.eventName.Contains("Winter"))
            {
                // Dark-based abilities: sigil reveal and night-sky power
                PlayerPrefs.SetFloat("Solstice_LunarMultiplier", 2f);
                PlayerPrefs.SetString("Solstice_LunarExpiry",
                    DateTime.UtcNow.AddHours(24).ToString("o"));
                Shader.SetGlobalFloat("_LunarPowerBoost", 1f);
                Shader.SetGlobalFloat("_SolarPowerBoost", 0f);
                Systems.AchievementManager.Instance?.TrackProgress("winter_solstice", 1);
                Debug.Log("[Sacred Timing] 🌙 Winter solstice: lunar power doubled for 24 h!");
            }
        }

        // Public API
        public SacredEvent GetCurrentEvent() => currentEvent;
        public bool IsEventActive() => isEventActive;
        public float GetCurrentPowerMultiplier() => currentEvent?.powerMultiplier ?? 1f;

        public SacredEvent GetNextEvent()
        {
            DateTime now = DateTime.UtcNow;
            SacredEvent nextEvent = null;
            TimeSpan shortestWait = TimeSpan.MaxValue;

            foreach (var evt in sacredEvents)
            {
                TimeSpan wait = evt.eventDate - now;
                if (wait.TotalSeconds > 0 && wait < shortestWait)
                {
                    shortestWait = wait;
                    nextEvent = evt;
                }
            }

            return nextEvent;
        }
    }
}
