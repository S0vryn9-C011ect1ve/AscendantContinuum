using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Live Event Engine — turns real astronomical events into in-game
    /// experiences that can never be replicated on any other day.
    ///
    /// Each event has three phases: Approach (days before), Peak (the day),
    /// and Echo (days after). Every phase modifies gameplay:
    ///   • Resource multipliers (sparks, sigil growth)
    ///   • Sky tint and ambient colour shifts
    ///   • Exclusive realm bonuses tied to the event's mythology
    ///   • Rare "Event Sigil Fragment" rewards that combine into unique sigils
    ///   • Cosmic Guardian messages sent to returning players
    ///
    /// The 2026 event calendar is seeded here. Events auto-repeat annually.
    /// </summary>
    public sealed class LiveEventEngine : MonoBehaviour
    {
        public static LiveEventEngine Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────
        public event Action<LiveEvent> OnEventApproaching;   // 3 days before peak
        public event Action<LiveEvent> OnEventPeak;           // event day
        public event Action<LiveEvent> OnEventEcho;           // 2 days after
        public event Action<LiveEvent> OnEventEnded;

        // ── State ──────────────────────────────────────────────────────────
        private LiveEvent _currentEvent;
        private bool      _eventActive;
        private List<LiveEvent> _calendar;

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            BuildCalendar();
            StartCoroutine(EventCheckLoop());
        }

        // ── Calendar ───────────────────────────────────────────────────────
        private void BuildCalendar()
        {
            _calendar = new List<LiveEvent>
            {
                // ── 2026 EQUINOXES & SOLSTICES ────────────────────────────
                new LiveEvent
                {
                    id             = "spring_equinox_2026",
                    displayName    = "The Spring Equinox",
                    mythology      = "Day and night breathe in perfect balance. The Verdant realm awakens.",
                    peakUtc        = new DateTime(2026, 3, 20, 9, 46, 0, DateTimeKind.Utc),
                    durationDays   = 3,
                    bonusRealm     = "verdant",
                    sparkMultiplier= 1.5f,
                    sigilMultiplier= 2.0f,
                    skyTintStart   = new Color(0.5f, 0.9f, 0.5f),
                    skyTintPeak    = new Color(0.3f, 1.0f, 0.4f),
                    exclusiveTitle = "Equinox Witness 2026",
                    isAnnual       = true
                },
                new LiveEvent
                {
                    id             = "summer_solstice_2026",
                    displayName    = "The Summer Solstice",
                    mythology      = "The longest day. The Emberforge burns at its greatest intensity.",
                    peakUtc        = new DateTime(2026, 6, 21, 2, 24, 0, DateTimeKind.Utc),
                    durationDays   = 3,
                    bonusRealm     = "emberforge",
                    sparkMultiplier= 2.0f,
                    sigilMultiplier= 1.5f,
                    skyTintStart   = new Color(1f, 0.8f, 0.3f),
                    skyTintPeak    = new Color(1f, 0.5f, 0.1f),
                    exclusiveTitle = "Solstice Forger 2026",
                    isAnnual       = true
                },
                new LiveEvent
                {
                    id             = "autumn_equinox_2026",
                    displayName    = "The Autumn Equinox",
                    mythology      = "The harvest. Stars in Echo Fields blaze in full constellation.",
                    peakUtc        = new DateTime(2026, 9, 22, 18, 5, 0, DateTimeKind.Utc),
                    durationDays   = 3,
                    bonusRealm     = "echo",
                    sparkMultiplier= 1.5f,
                    sigilMultiplier= 2.0f,
                    skyTintStart   = new Color(0.9f, 0.6f, 0.3f),
                    skyTintPeak    = new Color(1f, 0.4f, 0.2f),
                    exclusiveTitle = "Harvest Stargazer 2026",
                    isAnnual       = true
                },
                new LiveEvent
                {
                    id             = "winter_solstice_2026",
                    displayName    = "The Winter Solstice",
                    mythology      = "The longest night. A thousand lanterns must ascend to call back the sun.",
                    peakUtc        = new DateTime(2026, 12, 21, 20, 50, 0, DateTimeKind.Utc),
                    durationDays   = 5,
                    bonusRealm     = "lantern",
                    sparkMultiplier= 1.5f,
                    sigilMultiplier= 3.0f,     // biggest event of the year
                    skyTintStart   = new Color(0.1f, 0.1f, 0.4f),
                    skyTintPeak    = new Color(0f,   0f,   0.6f),
                    exclusiveTitle = "Light-Bringer 2026",
                    isAnnual       = true
                },

                // ── 2026 LUNAR EVENTS ─────────────────────────────────────
                new LiveEvent
                {
                    id             = "lunar_eclipse_mar_2026",
                    displayName    = "Total Lunar Eclipse",
                    mythology      = "The Moon becomes crimson. All prisms in Dawn Citadel refract blood-red light.",
                    peakUtc        = new DateTime(2026, 3, 3, 11, 33, 0, DateTimeKind.Utc),
                    durationDays   = 1,
                    bonusRealm     = "dawn",
                    sparkMultiplier= 3.0f,
                    sigilMultiplier= 3.0f,
                    skyTintStart   = new Color(0.6f, 0.1f, 0.1f),
                    skyTintPeak    = new Color(0.9f, 0.0f, 0.0f),
                    exclusiveTitle = "Blood Moon Witness",
                    isAnnual       = false      // One-time astronomical rarity
                },
                new LiveEvent
                {
                    id             = "lunar_eclipse_aug_2026",
                    displayName    = "Partial Lunar Eclipse",
                    mythology      = "The shadow reveals hidden constellations in Echo Fields.",
                    peakUtc        = new DateTime(2026, 8, 28, 4, 14, 0, DateTimeKind.Utc),
                    durationDays   = 1,
                    bonusRealm     = "echo",
                    sparkMultiplier= 2.0f,
                    sigilMultiplier= 2.5f,
                    skyTintStart   = new Color(0.4f, 0.2f, 0.5f),
                    skyTintPeak    = new Color(0.5f, 0.1f, 0.8f),
                    exclusiveTitle = "Shadow Tracer",
                    isAnnual       = false
                },

                // ── METEOR SHOWERS ────────────────────────────────────────
                new LiveEvent
                {
                    id             = "perseids_2026",
                    displayName    = "Perseid Meteor Shower",
                    mythology      = "Perseus' tears rain down. Each star drawn in Echo Fields multiplies.",
                    peakUtc        = new DateTime(2026, 8, 12, 0, 0, 0, DateTimeKind.Utc),
                    durationDays   = 3,
                    bonusRealm     = "echo",
                    sparkMultiplier= 1.8f,
                    sigilMultiplier= 2.0f,
                    skyTintStart   = new Color(0.2f, 0.3f, 0.7f),
                    skyTintPeak    = new Color(0.4f, 0.4f, 0.9f),
                    exclusiveTitle = "Meteor Chaser",
                    isAnnual       = true
                },
                new LiveEvent
                {
                    id             = "leonids_2026",
                    displayName    = "Leonid Meteor Shower",
                    mythology      = "Leo's mane blazes. The Emberforge sparks scatter across every realm.",
                    peakUtc        = new DateTime(2026, 11, 17, 0, 0, 0, DateTimeKind.Utc),
                    durationDays   = 2,
                    bonusRealm     = "emberforge",
                    sparkMultiplier= 2.5f,
                    sigilMultiplier= 1.5f,
                    skyTintStart   = new Color(1f, 0.6f, 0.1f),
                    skyTintPeak    = new Color(1f, 0.3f, 0.0f),
                    exclusiveTitle = "Lion's Spark",
                    isAnnual       = true
                },

                // ── NEW YEAR WORLD EVENT ──────────────────────────────────
                new LiveEvent
                {
                    id             = "new_year_2027",
                    displayName    = "New Year's Convergence",
                    mythology      = "All five realms glow simultaneously. A new cosmic cycle begins.",
                    peakUtc        = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    durationDays   = 3,
                    bonusRealm     = "all",      // All realms boosted
                    sparkMultiplier= 3.0f,
                    sigilMultiplier= 4.0f,       // Biggest reward
                    skyTintStart   = new Color(0.8f, 0.8f, 0.2f),
                    skyTintPeak    = new Color(1f,   1f,   0.4f),
                    exclusiveTitle = "Cycle Witness 2027",
                    isAnnual       = true
                }
            };

            Debug.Log($"[LiveEventEngine] Calendar loaded: {_calendar.Count} events.");
        }

        // ── Event check loop ───────────────────────────────────────────────
        private IEnumerator EventCheckLoop()
        {
            while (true)
            {
                EvaluateEvents();
                yield return new WaitForSecondsRealtime(300f); // check every 5 minutes
            }
        }

        private void EvaluateEvents()
        {
            DateTime now = DateTime.UtcNow;

            foreach (var evt in _calendar)
            {
                TimeSpan delta = evt.peakUtc - now;
                EventPhase phase = GetPhase(delta, evt);

                if (phase == EventPhase.None) continue;

                bool isNew = _currentEvent?.id != evt.id || !_eventActive;

                switch (phase)
                {
                    case EventPhase.Approaching when isNew:
                        _currentEvent = evt;
                        _eventActive  = true;
                        OnEventApproaching?.Invoke(evt);
                        Debug.Log($"[LiveEventEngine] Approaching: {evt.displayName} in {delta.TotalDays:F1} days");
                        break;

                    case EventPhase.Peak when isNew:
                        _currentEvent = evt;
                        _eventActive  = true;
                        OnEventPeak?.Invoke(evt);
                        // Grant exclusive title
                        CosmicIdentitySystem.Instance?.RecordSacredEventAttendance(evt.id);
                        AchievementManager.Instance?.UnlockAchievement($"event_{evt.id}");
                        // Track solstice / equinox specific achievements
                        TrackAstronomicalAchievement(evt);
                        Debug.Log($"[LiveEventEngine] 🌟 PEAK: {evt.displayName}!");
                        break;

                    case EventPhase.Echo when isNew:
                        OnEventEcho?.Invoke(evt);
                        Debug.Log($"[LiveEventEngine] Echo: {evt.displayName}");
                        break;
                }

                if (phase == EventPhase.None && _currentEvent?.id == evt.id)
                {
                    _eventActive  = false;
                    _currentEvent = null;
                    OnEventEnded?.Invoke(evt);
                }
            }
        }

        /// <summary>
        /// Pure phase calculation parameterised for unit testing.
        /// <paramref name="delta"/> = event.peakUtc - DateTime.UtcNow.
        /// </summary>
        public static EventPhase GetPhase(TimeSpan delta, int durationDays)
        {
            double days = delta.TotalDays;
            if (days > 3 || days < -durationDays) return EventPhase.None;
            if (days > 0 && days <= 3)             return EventPhase.Approaching;
            if (days >= -1 && days <= 0)           return EventPhase.Peak;
            if (days >= -durationDays && days < -1) return EventPhase.Echo;
            return EventPhase.None;
        }

        private static EventPhase GetPhase(TimeSpan delta, LiveEvent evt)
            => GetPhase(delta, evt.durationDays);

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Returns any currently active event, or null.</summary>
        public LiveEvent CurrentEvent => _eventActive ? _currentEvent : null;

        /// <summary>True while any live event is in Approaching, Peak, or Echo phase.</summary>
        public bool IsEventActive => _eventActive;

        /// <summary>Returns the active spark multiplier (1.0 if no event).</summary>
        public float ActiveSparkMultiplier
        {
            get
            {
                if (!_eventActive || _currentEvent == null) return 1f;
                TimeSpan delta = _currentEvent.peakUtc - DateTime.UtcNow;
                return GetPhase(delta, _currentEvent) == EventPhase.Peak
                    ? _currentEvent.sparkMultiplier : 1f + (_currentEvent.sparkMultiplier - 1f) * 0.3f;
            }
        }

        /// <summary>Returns the next upcoming event from now.</summary>
        public LiveEvent GetNextEvent()
        {
            DateTime now = DateTime.UtcNow;
            LiveEvent next = null;
            double minDays = double.MaxValue;

            foreach (var evt in _calendar)
            {
                double days = (evt.peakUtc - now).TotalDays;
                if (days > 0 && days < minDays) { minDays = days; next = evt; }
            }

            return next;
        }

        /// <summary>
        /// Grants progress toward solstice_seeker and equinox_keeper achievements
        /// when the player attends the corresponding live peak event.
        /// </summary>
        private static void TrackAstronomicalAchievementId(string id)
        {
            if (id.Contains("solstice"))
                AchievementManager.Instance?.TrackProgress("solstice_seeker", 1);
            else if (id.Contains("equinox"))
                AchievementManager.Instance?.TrackProgress("equinox_keeper", 1);
            else if (id.Contains("meteor") || id.Contains("perseids") || id.Contains("leonids") || id.Contains("geminids") || id.Contains("lyrids") || id.Contains("orionids"))
                AchievementManager.Instance?.TrackProgress("meteor_hunter", 1);
        }

        private static void TrackAstronomicalAchievement(LiveEvent evt)
            => TrackAstronomicalAchievementId(evt.id);

        public List<LiveEvent> Calendar => _calendar;
    }

    // ── Data types ─────────────────────────────────────────────────────────

    public enum EventPhase { None, Approaching, Peak, Echo }

    [Serializable]
    public sealed class LiveEvent
    {
        public string   id;
        public string   displayName;
        public string   mythology;
        public DateTime peakUtc;
        public int      durationDays;
        public string   bonusRealm;
        public float    sparkMultiplier;
        public float    sigilMultiplier;
        public Color    skyTintStart;
        public Color    skyTintPeak;
        public string   exclusiveTitle;
        public bool     isAnnual;
    }
}
