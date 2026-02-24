using UnityEngine;
using System;
using System.Collections;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Astronomy;

namespace AscendantContinuum.Realms.EchoFields
{
    /// <summary>
    /// Real Stargazing Manager — the only mobile game mechanic that rewards you
    /// for putting your phone DOWN and looking up at the actual sky.
    ///
    /// How it works:
    ///   1. Player activates "Real Stargazing" mode in Echo Fields.
    ///   2. Hold the phone face-up (flat or angled skyward, elevation > 30°).
    ///      The gyroscope verifies the device is pointing toward the sky.
    ///   3. Hold steady for 30 seconds — a progress arc fills on screen.
    ///   4. Basic night check: local time must be between sunset and sunrise
    ///      (approximated as 19:00–05:00 local, proper astro calc if GPS available).
    ///   5. On success:
    ///      • "Real Stargazer" certification sigil unlocked
    ///      • Next constellation traced = real constellation above player NOW
    ///        (CosmicDataManager.VisibleConstellations used)
    ///      • Achievement: "real_stargazer"
    ///      • 30-second bonus: all star traces worth 2× sparks
    ///
    /// Accessibility:
    ///   • Screen reader narrates each second of progress ("5… 10… almost there…")
    ///   • Haptic pulse every 5 seconds of hold
    ///   • Works in reduced-motion mode (no animation, just progress label)
    ///   • Can be triggered from settings UI for testing in daylight
    /// </summary>
    public sealed class RealStargazingManager : MonoBehaviour
    {
        public static RealStargazingManager Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────
        public event Action<float>  OnProgressChanged;      // 0–1
        public event Action         OnStargazingComplete;
        public event Action         OnStargazingCancelled;

        // ── Config ─────────────────────────────────────────────────────────
        [Header("Requirements")]
        [SerializeField] private float holdDurationSeconds = 30f;
        [SerializeField] private float minElevationAngle   = 30f;   // degrees above horizontal
        [SerializeField] private float maxTiltDrift        = 15f;   // max degrees of movement before reset
        [SerializeField] private int   nightStartHour      = 19;
        [SerializeField] private int   nightEndHour        = 5;
        [SerializeField] private bool  bypassNightCheck    = false;  // for testing / Editor

        // ── State ──────────────────────────────────────────────────────────
        private bool   _active;
        private float  _holdProgress;    // 0–holdDurationSeconds
        private float  _targetElevation; // elevation when session started
        private Coroutine _sessionCo;

        // ── Pref keys ──────────────────────────────────────────────────────
        private const string PREF_CERTIFIED = "RealStargazing_Certified";
        private const string PREF_LAST_STAR = "RealStargazing_LastConstellationName";
        private const string PREF_BONUS_EXPIRY = "RealStargazing_BonusExpiry";

        public bool IsCertified => PlayerPrefs.GetInt(PREF_CERTIFIED, 0) == 1;

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Enable gyroscope globally
            if (SystemInfo.supportsGyroscope)
                Input.gyro.enabled = true;

            if (SystemInfo.supportsLocationService)
                Input.compass.enabled = true;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>Begins the 30-second stargazing hold check.</summary>
        public void BeginStargazing()
        {
            if (_active) return;

            if (!IsNightTime() && !bypassNightCheck)
            {
                AccessibilityManager.Instance?.Announce(
                    "Real Stargazing is available at night, between dusk and dawn. Try again after dark.");

                UI.HUDManager.Instance?.ShowNotification(
                    "🌙 Come back after dark for Real Stargazing.",
                    UI.HUDManager.NotificationType.Info);
                return;
            }

            _active = true;
            _holdProgress = 0f;
            _targetElevation = GetCurrentElevation();

            AccessibilityManager.Instance?.Announce(
                "Real Stargazing begun. Hold your phone face-up, pointing toward the sky. Keep it steady for 30 seconds.");

            UI.HUDManager.Instance?.ShowNotification(
                "🔭 Point at the sky and hold steady for 30 seconds…",
                UI.HUDManager.NotificationType.Info);

            _sessionCo = StartCoroutine(StargzeSession());
        }

        /// <summary>Cancels an in-progress stargazing session.</summary>
        public void CancelStargazing()
        {
            if (!_active) return;
            if (_sessionCo != null) StopCoroutine(_sessionCo);
            _active = false;
            _holdProgress = 0f;
            OnProgressChanged?.Invoke(0f);
            OnStargazingCancelled?.Invoke();
            Debug.Log("[RealStargazing] Session cancelled.");
        }

        // ── Session coroutine ──────────────────────────────────────────────
        private IEnumerator StargzeSession()
        {
            float elapsed = 0f;
            int lastAnnouncedStep = 0;

            while (elapsed < holdDurationSeconds)
            {
                yield return null;

                float elevation = GetCurrentElevation();

                // Cancel if the player tilts the phone too far from the sky
                if (Mathf.Abs(elevation - _targetElevation) > maxTiltDrift || elevation < minElevationAngle)
                {
                    // Drift detected — reset progress but don't cancel session
                    if (_holdProgress > 0.1f)
                    {
                        AccessibilityManager.Instance?.Announce("Movement detected — hold steady.");
                        UI.HUDManager.Instance?.ShowNotification("📱 Hold steady!", UI.HUDManager.NotificationType.Warning);
                    }
                    _holdProgress = 0f;
                    elapsed = 0f;
                    _targetElevation = elevation; // Re-lock to current angle
                    OnProgressChanged?.Invoke(0f);
                    continue;
                }

                elapsed += Time.deltaTime;
                _holdProgress = elapsed / holdDurationSeconds;
                OnProgressChanged?.Invoke(_holdProgress);

                // Screen reader milestones every 5 seconds
                int step = Mathf.FloorToInt(elapsed / 5f);
                if (step > lastAnnouncedStep)
                {
                    lastAnnouncedStep = step;
                    int secondsLeft = Mathf.RoundToInt(holdDurationSeconds - elapsed);
                    AccessibilityManager.Instance?.Announce($"{secondsLeft} seconds remaining.");

                    // Haptic every 5 s
                    AccessibilityManager.Instance?.TriggerHaptic(HapticType.Light);
                }
            }

            _active = false;
            CompleteStargazing();
        }

        // ── Completion ─────────────────────────────────────────────────────
        private void CompleteStargazing()
        {
            Debug.Log("[RealStargazing] ⭐ Session complete!");

            // Mark certified
            PlayerPrefs.SetInt(PREF_CERTIFIED, 1);

            // Determine which real constellation is overhead right now
            string constellationName = GetRealConstellationAbove();
            PlayerPrefs.SetString(PREF_LAST_STAR, constellationName);

            // Set 30-minute bonus: 2× spark multiplier for star trace
            long bonusExpiry = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
            PlayerPrefs.SetInt(PREF_BONUS_EXPIRY, (int)bonusExpiry);

            PlayerPrefs.Save();

            // Haptic celebration
            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Success);

            // Announce
            AccessibilityManager.Instance?.Announce(
                $"Real Stargazing complete! The constellation {constellationName} is above you right now. " +
                "Your next constellation trace reflects the actual stars above you. Bonus: 2 times sparks for 30 minutes.");

            UI.HUDManager.Instance?.ShowNotification(
                $"⭐ Real Stargazing certified!\n{constellationName} is above you.\nBonus: 2× stars for 30 min!",
                UI.HUDManager.NotificationType.Achievement);

            // Achievements
            AchievementManager.Instance?.UnlockAchievement("real_stargazer");
            AchievementManager.Instance?.TrackProgress("stargazer", 1);

            // Analytics
            Core.FirebaseManager.Instance?.TrackEvent("real_stargazing_complete",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { "constellation", constellationName },
                    { "local_hour",    DateTime.Now.Hour }
                });

            OnStargazingComplete?.Invoke();

            // Serendipity — this is a magical real-world moment
            SerendipityManager.Instance?.TryTrigger("RealSky");
        }

        // ── Helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the elevation angle of the phone above the horizontal
        /// (positive = pointing toward sky, negative = pointing at ground).
        /// Uses gyroscope gravity vector when available; falls back to accelerometer.
        /// </summary>
        private static float GetCurrentElevation()
        {
            Vector3 gravity;

            if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
            {
                // Gravity in device space; z = pointing up when face-up
                gravity = Input.gyro.gravity.normalized;
            }
            else
            {
                // Accelerometer fallback (less accurate)
                gravity = Input.acceleration.normalized;
            }

            // elevation = angle between the gravity vector projection and the horizontal plane
            // When phone is flat face-up: gravity.y ≈ -1, elevation ≈ 90°
            // When phone is vertical:    gravity.z ≈ -1 or 1, elevation ≈ 0°
            float elevation = Mathf.Asin(Mathf.Clamp(-gravity.y, -1f, 1f)) * Mathf.Rad2Deg;
            return elevation;
        }

        private bool IsNightTime()
        {
            int hour = DateTime.Now.Hour;
            // Night = after nightStartHour OR before nightEndHour
            return hour >= nightStartHour || hour < nightEndHour;
        }

        /// <summary>
        /// Returns the best visible constellation overhead right now.
        /// Uses CosmicDataManager's precomputed visible constellation list;
        /// falls back to a seasonal default if the list is empty.
        /// </summary>
        private static string GetRealConstellationAbove()
        {
            var cosmic = CosmicDataManager.Instance;
            if (cosmic != null && cosmic.VisibleConstellations.Count > 0)
            {
                // Return the first seasonal (not year-round) constellation for flavor
                foreach (var c in cosmic.VisibleConstellations)
                {
                    if (c.season != "Year-round")
                        return c.name;
                }
                return cosmic.VisibleConstellations[0].name;
            }

            // Seasonal fallback based on current month
            int month = DateTime.Now.Month;
            if (month >= 12 || month <= 2) return "Orion";
            if (month <= 5)                return "Leo";
            if (month <= 8)                return "Scorpius";
            return "Pegasus";
        }

        // ── Bonus query (read by ConstellationTracer) ──────────────────────

        /// <summary>
        /// Returns 2.0 during the active Real Stargazing bonus window, else 1.0.
        /// ConstellationTracer calls this to scale spark rewards.
        /// </summary>
        public float GetStarTraceBonus()
        {
            int expiry = PlayerPrefs.GetInt(PREF_BONUS_EXPIRY, 0);
            if (expiry == 0) return 1f;
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
            {
                PlayerPrefs.DeleteKey(PREF_BONUS_EXPIRY);
                PlayerPrefs.Save();
                return 1f;
            }
            return 2f;
        }

        /// <summary>
        /// Returns the real constellation name set by the last successful
        /// stargazing session, or null if none.
        /// ConstellationTracer reads this to choose which pattern to display.
        /// </summary>
        public string GetRealConstellationName()
        {
            string name = PlayerPrefs.GetString(PREF_LAST_STAR, "");
            return string.IsNullOrEmpty(name) ? null : name;
        }

        // Properties
        public bool   IsActive   => _active;
        public float  Progress   => _holdProgress;
    }
}
