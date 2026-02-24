using UnityEngine;
using System;
using System.Collections.Generic;

namespace AscendantContinuum.Astronomy
{
    /// <summary>
    /// Real-time astronomical data system - Moon phases, star positions, constellations
    /// Connects the game to the actual cosmos for immersive, time-based gameplay
    /// </summary>
    public class CosmicDataManager : MonoBehaviour
    {
        public static CosmicDataManager Instance { get; private set; }

        [Header("Current Astronomical Data")]
        [SerializeField] private MoonPhase currentMoonPhase;
        [SerializeField] private float moonIllumination; // 0-1
        [SerializeField] private DateTime lastUpdate;
        
        [Header("Location (for accurate sky)")]
        [SerializeField] private float playerLatitude = 0f;
        [SerializeField] private float playerLongitude = 0f;
        
        [Header("Visible Celestial Objects")]
        [SerializeField] private List<CelestialBody> visibleStars = new List<CelestialBody>();
        [SerializeField] private List<Constellation> visibleConstellations = new List<Constellation>();
        
        public event Action<MoonPhase> OnMoonPhaseChanged;
        public event Action<string> OnAstronomicalEvent; // Eclipses, meteor showers, etc.

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Try to get player's location (with permission)
            TryGetPlayerLocation();
            
            UpdateCosmicData();
            
            // Update astronomical data every hour
            InvokeRepeating(nameof(UpdateCosmicData), 3600f, 3600f);
        }

        private void TryGetPlayerLocation()
        {
            #if UNITY_ANDROID || UNITY_IOS
            if (Input.location.isEnabledByUser)
            {
                Input.location.Start();
                StartCoroutine(WaitForLocation());
            }
            else
            {
                // Default to equator if location not available
                playerLatitude = 0f;
                playerLongitude = 0f;
                Debug.Log("[CosmicData] Location not available - using default position");
            }
            #else
            // For testing, use a default location
            playerLatitude = 40.7128f; // New York
            playerLongitude = -74.0060f;
            #endif
        }

        private System.Collections.IEnumerator WaitForLocation()
        {
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogWarning("[CosmicData] Location service failed");
                yield break;
            }

            playerLatitude = Input.location.lastData.latitude;
            playerLongitude = Input.location.lastData.longitude;
            
            Debug.Log($"[CosmicData] Location: {playerLatitude}, {playerLongitude}");
            
            Input.location.Stop();
            UpdateCosmicData();
        }

        public void UpdateCosmicData()
        {
            lastUpdate = DateTime.UtcNow;
            
            // Calculate moon phase
            CalculateMoonPhase();
            
            // Calculate visible stars based on time and location
            CalculateVisibleStars();
            
            // Check for special astronomical events
            CheckAstronomicalEvents();
            
            Debug.Log($"[CosmicData] Updated - Moon: {currentMoonPhase} ({moonIllumination:P0}), Stars: {visibleStars.Count}");
        }

        private void CalculateMoonPhase()
        {
            // Moon phase calculation using astronomical algorithms
            DateTime knownNewMoon = new DateTime(2000, 1, 6, 18, 14, 0); // Known new moon
            double daysSinceKnownNewMoon = (DateTime.UtcNow - knownNewMoon).TotalDays;
            double lunarCycle = 29.53058867; // Average lunar month in days
            
            double currentCycle = (daysSinceKnownNewMoon % lunarCycle) / lunarCycle;
            moonIllumination = (float)currentCycle;
            
            // Determine phase
            MoonPhase previousPhase = currentMoonPhase;
            
            if (currentCycle < 0.0625f || currentCycle >= 0.9375f)
                currentMoonPhase = MoonPhase.NewMoon;
            else if (currentCycle < 0.1875f)
                currentMoonPhase = MoonPhase.WaxingCrescent;
            else if (currentCycle < 0.3125f)
                currentMoonPhase = MoonPhase.FirstQuarter;
            else if (currentCycle < 0.4375f)
                currentMoonPhase = MoonPhase.WaxingGibbous;
            else if (currentCycle < 0.5625f)
                currentMoonPhase = MoonPhase.FullMoon;
            else if (currentCycle < 0.6875f)
                currentMoonPhase = MoonPhase.WaningGibbous;
            else if (currentCycle < 0.8125f)
                currentMoonPhase = MoonPhase.LastQuarter;
            else
                currentMoonPhase = MoonPhase.WaningCrescent;
            
            if (previousPhase != currentMoonPhase)
            {
                OnMoonPhaseChanged?.Invoke(currentMoonPhase);
                Debug.Log($"[CosmicData] 🌙 Moon phase changed to {currentMoonPhase}");
            }
        }

        private void CalculateVisibleStars()
        {
            visibleStars.Clear();
            visibleConstellations.Clear();
            
            DateTime now = DateTime.UtcNow;
            double julianDate = GetJulianDate(now);
            
            // Calculate Local Sidereal Time for star positions
            double lst = CalculateLocalSiderealTime(julianDate, playerLongitude);
            
            // Add major visible stars (simplified - in production use full star catalog)
            AddMajorStars(lst, playerLatitude);
            
            // Determine visible constellations
            DetermineVisibleConstellations(now);
        }

        private void AddMajorStars(double lst, float latitude)
        {
            // Brightest stars in the night sky with approximate positions
            // Right Ascension (RA) in hours, Declination (Dec) in degrees
            
            var brightStars = new List<(string name, double ra, double dec, float magnitude)>
            {
                ("Sirius", 6.75, -16.72, -1.46f),        // Brightest star
                ("Canopus", 6.40, -52.70, -0.72f),
                ("Arcturus", 14.26, 19.18, -0.05f),
                ("Vega", 18.62, 38.78, 0.03f),
                ("Capella", 5.28, 45.99, 0.08f),
                ("Rigel", 5.24, -8.20, 0.12f),
                ("Procyon", 7.66, 5.22, 0.38f),
                ("Betelgeuse", 5.92, 7.41, 0.50f),       // Red giant
                ("Altair", 19.85, 8.87, 0.77f),
                ("Aldebaran", 4.60, 16.51, 0.85f),
                ("Antares", 16.49, -26.43, 0.96f),       // Red supergiant
                ("Spica", 13.42, -11.16, 1.04f),
                ("Pollux", 7.76, 28.03, 1.14f),
                ("Deneb", 20.69, 45.28, 1.25f),
                ("Regulus", 10.14, 11.97, 1.35f)
            };
            
            foreach (var star in brightStars)
            {
                // Calculate altitude and azimuth
                double hourAngle = lst - star.ra;
                double altitude = CalculateAltitude(hourAngle, star.dec, latitude);
                
                // Only add if above horizon
                if (altitude > 0)
                {
                    visibleStars.Add(new CelestialBody
                    {
                        name = star.name,
                        rightAscension = star.ra,
                        declination = star.dec,
                        magnitude = star.magnitude,
                        altitude = altitude,
                        isVisible = true
                    });
                }
            }
        }

        private void DetermineVisibleConstellations(DateTime now)
        {
            // Constellations visible in different seasons
            int month = now.Month;
            
            // Northern hemisphere seasonal constellations
            if (month >= 12 || month <= 2) // Winter
            {
                visibleConstellations.Add(new Constellation { name = "Orion", season = "Winter" });
                visibleConstellations.Add(new Constellation { name = "Taurus", season = "Winter" });
                visibleConstellations.Add(new Constellation { name = "Gemini", season = "Winter" });
                visibleConstellations.Add(new Constellation { name = "Canis Major", season = "Winter" });
            }
            else if (month >= 3 && month <= 5) // Spring
            {
                visibleConstellations.Add(new Constellation { name = "Leo", season = "Spring" });
                visibleConstellations.Add(new Constellation { name = "Virgo", season = "Spring" });
                visibleConstellations.Add(new Constellation { name = "Boötes", season = "Spring" });
            }
            else if (month >= 6 && month <= 8) // Summer
            {
                visibleConstellations.Add(new Constellation { name = "Cygnus", season = "Summer" });
                visibleConstellations.Add(new Constellation { name = "Lyra", season = "Summer" });
                visibleConstellations.Add(new Constellation { name = "Aquila", season = "Summer" });
                visibleConstellations.Add(new Constellation { name = "Scorpius", season = "Summer" });
            }
            else // Fall
            {
                visibleConstellations.Add(new Constellation { name = "Pegasus", season = "Fall" });
                visibleConstellations.Add(new Constellation { name = "Andromeda", season = "Fall" });
                visibleConstellations.Add(new Constellation { name = "Perseus", season = "Fall" });
            }
            
            // Always visible (circumpolar for mid-northern latitudes)
            visibleConstellations.Add(new Constellation { name = "Ursa Major", season = "Year-round" });
            visibleConstellations.Add(new Constellation { name = "Cassiopeia", season = "Year-round" });
        }

        private void CheckAstronomicalEvents()
        {
            DateTime now = DateTime.UtcNow;
            
            // Check for meteor showers
            CheckMeteorShowers();
            
            // Check for planetary alignments (simplified)
            // In production, use ephemeris data
            
            // Check for eclipses (requires complex calculations or API)
        }

        private void CheckMeteorShowers()
        {
            DateTime now = DateTime.UtcNow;
            
            // Major meteor showers with peak dates
            var meteorShowers = new List<(string name, int month, int day)>
            {
                ("Quadrantids", 1, 3),
                ("Lyrids", 4, 22),
                ("Perseids", 8, 12),      // Most famous
                ("Orionids", 10, 21),
                ("Leonids", 11, 17),
                ("Geminids", 12, 14)
            };
            
            foreach (var shower in meteorShowers)
            {
                if (now.Month == shower.month && Math.Abs(now.Day - shower.day) <= 2)
                {
                    OnAstronomicalEvent?.Invoke($"meteor_shower_{shower.name.ToLower()}");
                    Debug.Log($"[CosmicData] ☄️ {shower.name} meteor shower is active!");
                }
            }
        }

        // Helper astronomical calculations
        private double GetJulianDate(DateTime date)
        {
            return date.ToOADate() + 2415018.5;
        }

        private double CalculateLocalSiderealTime(double julianDate, float longitude)
        {
            double d = julianDate - 2451545.0;
            double gmst = 280.46061837 + 360.98564736629 * d;
            gmst = gmst % 360;
            if (gmst < 0) gmst += 360;
            
            double lst = gmst + longitude;
            lst = lst % 360;
            if (lst < 0) lst += 360;
            
            return lst / 15.0; // Convert to hours
        }

        private double CalculateAltitude(double hourAngle, double declination, float latitude)
        {
            double ha = hourAngle * 15.0 * Mathf.Deg2Rad;
            double dec = declination * Mathf.Deg2Rad;
            double lat = latitude * Mathf.Deg2Rad;
            
            double sinAlt = Math.Sin(lat) * Math.Sin(dec) + Math.Cos(lat) * Math.Cos(dec) * Math.Cos(ha);
            return Math.Asin(sinAlt) * Mathf.Rad2Deg;
        }

        // Public getters
        public MoonPhase CurrentMoonPhase => currentMoonPhase;
        public float MoonIllumination => moonIllumination;
        public List<CelestialBody> VisibleStars => visibleStars;
        public List<Constellation> VisibleConstellations => visibleConstellations;
        
        /// <summary>Returns the name of the currently active meteor shower, or null if none is active.</summary>
        public string GetActiveMeteorShower()
        {
            DateTime now = DateTime.UtcNow;
            var showers = new System.Collections.Generic.List<(string name, int month, int day)>
            {
                ("Perseids", 8, 12),
                ("Leonids", 11, 17),
                ("Geminids", 12, 14),
                ("Lyrids", 4, 22),
                ("Orionids", 10, 21)
            };
            foreach (var s in showers)
                if (now.Month == s.month && Mathf.Abs(now.Day - s.day) <= 2)
                    return s.name;
            return null;
        }
        
        public string GetMoonPhaseDescription()
        {
            return currentMoonPhase switch
            {
                MoonPhase.NewMoon => "The moon hides in shadow, a time of new beginnings",
                MoonPhase.WaxingCrescent => "A sliver of light grows, potential awakening",
                MoonPhase.FirstQuarter => "Half-light reveals hidden paths",
                MoonPhase.WaxingGibbous => "The moon swells with energy and power",
                MoonPhase.FullMoon => "Complete illumination, all secrets revealed",
                MoonPhase.WaningGibbous => "The moon shares its wisdom as light fades",
                MoonPhase.LastQuarter => "Balance between light and shadow",
                MoonPhase.WaningCrescent => "The final whisper before renewal",
                _ => "The cosmic cycle continues"
            };
        }
    }

    [Serializable]
    public class CelestialBody
    {
        public string name;
        public double rightAscension; // Hours
        public double declination; // Degrees
        public float magnitude; // Brightness
        public double altitude; // Degrees above horizon
        public bool isVisible;
    }

    [Serializable]
    public class Constellation
    {
        public string name;
        public string season;
        public List<Vector2> starPositions = new List<Vector2>();
    }

    public enum MoonPhase
    {
        NewMoon,
        WaxingCrescent,
        FirstQuarter,
        WaxingGibbous,
        FullMoon,
        WaningGibbous,
        LastQuarter,
        WaningCrescent
    }
}
