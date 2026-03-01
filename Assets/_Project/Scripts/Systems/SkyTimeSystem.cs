using UnityEngine;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Provides time-of-day and seasonal sky blend values.
    ///
    /// Queried once per scene load by <see cref="Realms.RealmAtmosphereController"/>
    /// and <see cref="RealWorldNudgeSystem"/>.
    ///
    /// No external astronomy APIs — all values derived from <c>System.DateTime.Now</c>.
    /// </summary>
    public sealed class SkyTimeSystem : MonoBehaviour
    {
        public static SkyTimeSystem Instance { get; private set; }

        // ── Types ─────────────────────────────────────────────────────────────
        public enum TimePeriod { Dawn, Morning, Afternoon, Dusk, Night }
        public enum SeasonType { Spring, Summer, Autumn, Winter }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Hemisphere")]
        [SerializeField] private bool northernHemisphere = true;

        // ── Cached values (refreshed on Start) ───────────────────────────────
        private TimePeriod _timePeriod;
        private SeasonType _season;
        private SkyBlend   _currentBlend;

        public TimePeriod   CurrentTimePeriod => _timePeriod;
        public SeasonType   CurrentSeason     => _season;
        public SkyBlend     CurrentBlend      => _currentBlend;

        // ── Sky blend preset tables ───────────────────────────────────────────
        // [TimePeriod][SeasonType]
        private static readonly Color[,] PrimaryColors = {
            // Dawn:       Spring                   Summer                    Autumn                   Winter
            { new Color(1f, 0.7f, 0.5f),  new Color(1f, 0.75f, 0.45f), new Color(0.9f, 0.6f, 0.4f), new Color(0.8f, 0.65f, 0.7f) },
            // Morning
            { new Color(0.7f, 0.85f, 1f), new Color(0.6f, 0.85f, 1f),  new Color(0.75f, 0.8f, 0.9f),new Color(0.65f, 0.75f, 0.95f)},
            // Afternoon
            { new Color(0.55f, 0.8f, 1f), new Color(0.5f, 0.82f, 1f),  new Color(0.6f, 0.75f, 0.9f),new Color(0.6f,0.7f,0.88f) },
            // Dusk
            { new Color(1f, 0.55f, 0.35f),new Color(1f, 0.5f, 0.3f),   new Color(0.9f, 0.45f, 0.3f),new Color(0.7f, 0.4f, 0.55f)},
            // Night
            { new Color(0.08f, 0.06f, 0.2f),new Color(0.07f,0.05f,0.18f),new Color(0.06f,0.05f,0.15f),new Color(0.05f,0.04f,0.12f)}
        };

        private static readonly float[] StarBrightness = { 0.1f, 0f, 0f, 0.25f, 1f };
        private static readonly float[] AmbientIntensity = { 0.6f, 0.9f, 1f, 0.55f, 0.2f };

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            Refresh();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Recalculate based on current DateTime.Now. Call on scene load.</summary>
        public void Refresh()
        {
            _timePeriod = GetTimePeriod(System.DateTime.Now.Hour);
            _season     = GetSeason(System.DateTime.Now.DayOfYear, northernHemisphere);
            _currentBlend = new SkyBlend
            {
                primaryColor     = PrimaryColors[(int)_timePeriod, (int)_season],
                starBrightness   = StarBrightness[(int)_timePeriod],
                ambientIntensity = AmbientIntensity[(int)_timePeriod],
                timePeriod       = _timePeriod,
                season           = _season
            };
            Debug.Log($"[SkyTime] {_timePeriod} / {_season} → primary={_currentBlend.primaryColor}");
        }

        /// <summary>Daily variation tint: slightly different each day of week.</summary>
        public Color GetDailyVariationTint()
        {
            int dow  = (int)System.DateTime.Now.DayOfWeek;
            float h  = dow / 7f;
            return Color.HSVToRGB(h, 0.15f, 1f);
        }

        // ── Static helpers ────────────────────────────────────────────────────

        public static TimePeriod GetTimePeriod(int hour)
        {
            if (hour >= 5  && hour < 8)  return TimePeriod.Dawn;
            if (hour >= 8  && hour < 12) return TimePeriod.Morning;
            if (hour >= 12 && hour < 18) return TimePeriod.Afternoon;
            if (hour >= 18 && hour < 21) return TimePeriod.Dusk;
            return TimePeriod.Night;
        }

        public static SeasonType GetSeason(int dayOfYear, bool northernHemisphere)
        {
            // Approximate seasons by day of year (northern hemisphere)
            SeasonType s;
            if      (dayOfYear >=  80 && dayOfYear < 172) s = SeasonType.Spring;
            else if (dayOfYear >= 172 && dayOfYear < 264) s = SeasonType.Summer;
            else if (dayOfYear >= 264 && dayOfYear < 355) s = SeasonType.Autumn;
            else                                           s = SeasonType.Winter;

            // Flip for southern hemisphere
            if (!northernHemisphere)
                s = (SeasonType)(((int)s + 2) % 4);

            return s;
        }
    }

    /// <summary>
    /// Output struct from <see cref="SkyTimeSystem"/> pushed to <see cref="Realms.RealmAtmosphereController"/>.
    /// </summary>
    [System.Serializable]
    public struct SkyBlend
    {
        public Color   primaryColor;
        public float   starBrightness;   // [0,1]
        public float   ambientIntensity; // [0,1]
        public SkyTimeSystem.TimePeriod timePeriod;
        public SkyTimeSystem.SeasonType season;
    }
}
