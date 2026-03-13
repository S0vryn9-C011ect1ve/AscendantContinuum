using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Core;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// The Cosmic Identity System — the heartbeat of what makes every
    /// player's journey in The Ascendant Continuum truly unrepeatable.
    ///
    /// Every behavioural signal (realm preference, time of day, accessibility
    /// choices, nature-play, sacred-event attendance, streak length) feeds a
    /// living <see cref="CosmicProfile"/> that evolves continuously.
    ///
    /// A player's Cosmic Profile produces:
    ///   • A procedurally generated <b>Cosmic Name</b>  (e.g. "Ember-dawn Wanderer of the Still Veil")
    ///   • A living <b>Sigil</b> that shifts shape and colour over time
    ///   • A <b>Cosmic Alignment</b> (their dominant archetype)
    ///   • An <b>Aura Tier</b> that others see in the Wish Wall
    ///
    /// No two profiles are ever identical because they encode the intersection
    /// of WHO you are, WHEN you play, HOW you engage, and WHAT you care about.
    /// </summary>
    public sealed class CosmicIdentitySystem : MonoBehaviour
    {
        public static CosmicIdentitySystem Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────
        public event Action<CosmicProfile> OnProfileEvolved;
        public event Action<string>        OnTitleUnlocked;     // new cosmic title

        // ── State ──────────────────────────────────────────────────────────
        private CosmicProfile _profile;

        // ── Archetype definitions ──────────────────────────────────────────
        private static readonly string[] ALIGNMENTS =
        {
            "Ember Smith",         // Emberforge dominant
            "Root Tender",         // Verdant dominant
            "Star Reader",         // Echo Fields dominant
            "Prism Walker",        // Dawn Citadel dominant
            "Lantern Bearer",      // Lantern Ascension dominant
            "Realm Wanderer",      // Balanced across realms
            "Midnight Sage",       // Plays at night
            "Dawn Seeker",         // Plays at dawn
            "Nature Mystic",       // Uses GPS nature bonus
            "Accessibility Pioneer",// Plays with accessibility features
        };

        private static readonly string[] PREFIXES =
        {
            "Ember-dawn", "Velvet", "Pale", "Ashen", "Luminous",
            "Silver", "Cobalt", "Hollow", "Sacred", "Ancient",
            "Quiet", "Tempest", "Golden", "Crimson", "Azure"
        };

        private static readonly string[] MIDNAMES =
        {
            "Wanderer", "Seeker", "Keeper", "Weaver",  "Watcher",
            "Drifter",  "Shaper", "Walker", "Bringer", "Tender",
            "Listener", "Singer", "Dreamer","Forger",  "Herald"
        };

        private static readonly string[] REALMS_OF =
        {
            "the Still Veil",     "Endless Embers",     "Verdant Depths",
            "the Star Map",       "the First Light",    "Wandering Lamps",
            "the Quiet Between",  "Shifting Prisms",    "the Living Root",
            "Ancient Fire",       "the Moon Crest",     "Frozen Song",
            "Dawn Glass",         "the Deep Hum",       "Unformed Dark"
        };

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadOrCreateProfile();
        }

        // ── Profile bootstrap ──────────────────────────────────────────────
        private void LoadOrCreateProfile()
        {
            string json = PlayerPrefs.GetString("CosmicProfile_v2", string.Empty);
            string trimmed = string.IsNullOrWhiteSpace(json) ? string.Empty : json.TrimStart();
            if (!string.IsNullOrEmpty(trimmed) && trimmed.StartsWith("{"))
            {
                try
                {
                    _profile = JsonUtility.FromJson<CosmicProfile>(json);
                    if (_profile == null)
                    {
                        CreateFreshProfile();
                        return;
                    }
                    Debug.Log($"[CosmicIdentity] Loaded: {_profile.cosmicName}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CosmicIdentity] Failed to parse profile. Creating fresh profile. {e.Message}");
                    CreateFreshProfile();
                }
            }
            else
            {
                CreateFreshProfile();
            }
        }

        private void CreateFreshProfile()
        {
            _profile = new CosmicProfile
            {
                profileId      = Guid.NewGuid().ToString(),
                createdUtc     = DateTime.UtcNow.ToString("o"),
                auraColor      = new SerializableColor(0.8f, 0.6f, 1f, 1f),
                auaraTier      = AuraTier.Spark,
                alignment      = "Realm Wanderer",
                sigil          = new SigilSnapshot(),
                realmDurations = new SerializableDictionary()
            };
            _profile.cosmicName = GenerateName(_profile);
            SaveProfile();
            Debug.Log($"[CosmicIdentity] Created new profile: {_profile.cosmicName}");
        }

        // ── Behaviour signals ──────────────────────────────────────────────

        /// <summary>
        /// Call this every time the player spends time in a realm.
        /// The profile will re-evaluate its alignment and potentially evolve.
        /// </summary>
        public void RecordRealmTime(string realmId, float minutes)
        {
            if (_profile == null) return;

            _profile.realmDurations.AddOrIncrement(realmId, minutes);
            _profile.totalPlayMinutes += minutes;

            EvolveProfile();
        }

        /// <summary>Call when the player joins a live Sacred/Astronomical event.</summary>
        public void RecordSacredEventAttendance(string eventName)
        {
            if (_profile == null) return;
            if (!_profile.sacredEventsAttended.Contains(eventName))
            {
                _profile.sacredEventsAttended.Add(eventName);
                EvolveProfile();
            }
        }

        /// <summary>Call when the player earns a new achievement.</summary>
        public void RecordAchievement(string achievementId)
        {
            if (_profile == null) return;
            if (!_profile.achievementsEarned.Contains(achievementId))
            {
                _profile.achievementsEarned.Add(achievementId);
                CheckAuraTierUp();
                EvolveProfile();
            }
        }

        /// <summary>Call each day the player maintains a streak.</summary>
        public void RecordStreakDay(int currentStreak)
        {
            if (_profile == null) return;
            _profile.longestStreak = Mathf.Max(_profile.longestStreak, currentStreak);

            // Streak milestones unlock titles
            if (currentStreak == 7)  UnlockTitle("Week Keeper");
            if (currentStreak == 30) UnlockTitle("Month Sage");
            if (currentStreak == 100) UnlockTitle("Century Ascendant");

            EvolveProfile();
        }

        /// <summary>Call when player uses GPS nature bonus.</summary>
        public void RecordNatureSession(float minutes)
        {
            if (_profile == null) return;
            _profile.totalNatureMinutes += minutes;
            if (_profile.totalNatureMinutes >= 60f && !_profile.titlesUnlocked.Contains("Earth Walker"))
                UnlockTitle("Earth Walker");
            EvolveProfile();
        }

        /// <summary>Call when player changes accessibility settings.</summary>
        public void RecordAccessibilityEngagement(string settingChanged)
        {
            if (_profile == null) return;
            if (!_profile.accessibilityFeaturesUsed.Contains(settingChanged))
            {
                _profile.accessibilityFeaturesUsed.Add(settingChanged);
                if (_profile.accessibilityFeaturesUsed.Count >= 3)
                    UnlockTitle("Accessibility Pioneer");
                EvolveProfile();
            }
        }

        // ── Evolution engine ───────────────────────────────────────────────
        private void EvolveProfile()
        {
            if (_profile == null) return;

            // Re-determine alignment
            string newAlignment = DetermineAlignment();
            bool alignmentChanged = newAlignment != _profile.alignment;
            _profile.alignment = newAlignment;

            // Re-generate cosmic name on major milestones (every 50 play minutes)
            if (_profile.totalPlayMinutes % 50 < 1f || alignmentChanged)
                _profile.cosmicName = GenerateName(_profile);

            // Evolve sigil snapshot
            _profile.sigil = EvolveSigil(_profile);

            // Evolve aura colour
            _profile.auraColor = DetermineAuraColor(_profile);

            _profile.lastEvolvedUtc = DateTime.UtcNow.ToString("o");

            // Living lore — record alignment shift for deity balance tracking
            if (alignmentChanged)
                LivingLoreManager.Instance?.RecordDeityAlignment(_profile.alignment);

            SaveProfile();
            OnProfileEvolved?.Invoke(_profile);
        }

        private string DetermineAlignment()
        {
            if (_profile.realmDurations == null) return "Realm Wanderer";

            string dominantRealm   = _profile.realmDurations.DominantKey();
            float  totalRealmTime  = _profile.realmDurations.TotalValue();

            // Nature Mystic if significant outdoor time
            if (_profile.totalNatureMinutes > 60f) return "Nature Mystic";

            // Accessibility Pioneer
            if (_profile.accessibilityFeaturesUsed.Count >= 3) return "Accessibility Pioneer";

            // Night / Dawn personas
            // (We seed with UTC hour, but use local for flavour)
            int nowHour = DateTime.Now.Hour;
            if (_profile.totalPlayMinutes > 120f)
            {
                if (nowHour >= 0 && nowHour < 5)   return "Midnight Sage";
                if (nowHour >= 5 && nowHour < 8)   return "Dawn Seeker";
            }

            // Realm-based alignment
            return dominantRealm switch
            {
                "emberforge" => "Ember Smith",
                "verdant"    => "Root Tender",
                "echo"       => "Star Reader",
                "dawn"       => "Prism Walker",
                "lantern"    => "Lantern Bearer",
                _            => "Realm Wanderer"
            };
        }

        private string GenerateName(CosmicProfile p)
        {
            // Seed from profileId for deterministic-but-unique names
            int seed = p.profileId.GetHashCode();

            // Adjust seed slightly based on dominant realm to influence name flavour
            string dominant = p.realmDurations?.DominantKey() ?? "ember";
            seed ^= dominant.GetHashCode();

            var rng = new System.Random(seed);
            string prefix  = PREFIXES [rng.Next(PREFIXES.Length)];
            string midname = MIDNAMES [rng.Next(MIDNAMES.Length)];
            string realm   = REALMS_OF[rng.Next(REALMS_OF.Length)];

            return $"{prefix} {midname} of {realm}";
        }

        private SigilSnapshot EvolveSigil(CosmicProfile p)
        {
            // Shape index driven by dominant realm (0-4) plus overflow shapes (5,6)
            string dom   = p.realmDurations?.DominantKey() ?? "emberforge";
            int shapeIdx = dom switch
            {
                "emberforge" => 0,   // Triangle — fire
                "verdant"    => 1,   // Hexagon  — growth
                "echo"       => 2,   // Circle   — infinity
                "dawn"       => 3,   // Square   — stability
                "lantern"    => 4,   // Star     — transcendence
                _            => 0
            };

            // Add shapes as player progresses
            if (p.totalPlayMinutes > 500f) shapeIdx = (shapeIdx + 5) % 7; // evolved shape

            // Pattern driven by average session length
            int patternIdx = 1;
            if (p.totalPlayMinutes > 0 && p.sessionsPlayed > 0)
            {
                float avgMin = p.totalPlayMinutes / p.sessionsPlayed;
                patternIdx = avgMin < 5 ? 0 : (avgMin < 15 ? 1 : 2);
            }

            return new SigilSnapshot { shapeIndex = shapeIdx, patternIndex = patternIdx };
        }

        private SerializableColor DetermineAuraColor(CosmicProfile p)
        {
            return p.alignment switch
            {
                "Ember Smith"          => new SerializableColor(1f,   0.4f, 0.2f, 1f),
                "Root Tender"          => new SerializableColor(0.2f, 0.9f, 0.3f, 1f),
                "Star Reader"          => new SerializableColor(0.5f, 0.5f, 1f,   1f),
                "Prism Walker"         => new SerializableColor(1f,   0.9f, 0.2f, 1f),
                "Lantern Bearer"       => new SerializableColor(1f,   0.7f, 0.3f, 1f),
                "Midnight Sage"        => new SerializableColor(0.1f, 0.1f, 0.5f, 1f),
                "Dawn Seeker"          => new SerializableColor(1f,   0.85f,0.5f, 1f),
                "Nature Mystic"        => new SerializableColor(0.3f, 0.8f, 0.4f, 1f),
                "Accessibility Pioneer"=> new SerializableColor(0.8f, 0.4f, 1f,   1f),
                _                      => new SerializableColor(0.7f, 0.7f, 0.9f, 1f),
            };
        }

        private void CheckAuraTierUp()
        {
            if (_profile == null) return;
            int count = _profile.achievementsEarned.Count;

            AuraTier prev = _profile.auaraTier;
            _profile.auaraTier = count switch
            {
                >= 20 => AuraTier.Celestial,
                >= 10 => AuraTier.Radiant,
                >= 5  => AuraTier.Luminous,
                >= 2  => AuraTier.Kindled,
                _     => AuraTier.Spark
            };

            if (_profile.auaraTier != prev)
                Debug.Log($"[CosmicIdentity] Aura tiered up to {_profile.auaraTier}!");
        }

        private void UnlockTitle(string title)
        {
            if (_profile == null || _profile.titlesUnlocked.Contains(title)) return;
            _profile.titlesUnlocked.Add(title);
            OnTitleUnlocked?.Invoke(title);
            Debug.Log($"[CosmicIdentity] Title unlocked: {title}");
        }

        // ── Record session start ───────────────────────────────────────────
        public void RecordSessionStart()
        {
            if (_profile != null)
                _profile.sessionsPlayed++;
        }

        // ── Persistence ────────────────────────────────────────────────────
        private void SaveProfile()
        {
            PlayerPrefs.SetString("CosmicProfile_v2", JsonUtility.ToJson(_profile));
            PlayerPrefs.Save();
        }

        // ── Public accessors ───────────────────────────────────────────────
        public CosmicProfile  CurrentProfile => _profile;
        public string         CosmicName     => _profile?.cosmicName    ?? "Unknown Wanderer";
        public string         Alignment      => _profile?.alignment     ?? "Realm Wanderer";
        public AuraTier       AuraTierLevel  => _profile?.auaraTier     ?? AuraTier.Spark;
    }

    // ── Data types ─────────────────────────────────────────────────────────

    public enum AuraTier
    {
        Spark     = 0,
        Kindled   = 1,
        Luminous  = 2,
        Radiant   = 3,
        Celestial = 4
    }

    [Serializable]
    public sealed class CosmicProfile
    {
        public string profileId;
        public string cosmicName;
        public string alignment;
        public string createdUtc;
        public string lastEvolvedUtc;
        public AuraTier auaraTier;
        public SerializableColor auraColor;
        public SigilSnapshot sigil;

        // Metrics
        public float  totalPlayMinutes;
        public int    sessionsPlayed;
        public int    longestStreak;
        public float  totalNatureMinutes;

        // Collections
        public List<string>           sacredEventsAttended    = new();
        public List<string>           achievementsEarned      = new();
        public List<string>           titlesUnlocked          = new();
        public List<string>           accessibilityFeaturesUsed = new();
        public SerializableDictionary realmDurations          = new();
    }

    [Serializable]
    public sealed class SigilSnapshot
    {
        public int shapeIndex;
        public int patternIndex;
    }

    [Serializable]
    public sealed class SerializableColor
    {
        public float r, g, b, a;
        public SerializableColor(float r, float g, float b, float a)
        { this.r = r; this.g = g; this.b = b; this.a = a; }
        public Color ToColor() => new Color(r, g, b, a);
    }

    /// <summary>
    /// Lightweight serialisable dictionary (string → float) for realm durations.
    /// Unity's JsonUtility cannot serialise Dictionary<,>, so we use parallel lists.
    /// </summary>
    [Serializable]
    public sealed class SerializableDictionary
    {
        [SerializeField] private List<string> keys   = new();
        [SerializeField] private List<float>  values = new();

        public void AddOrIncrement(string key, float amount)
        {
            int idx = keys.IndexOf(key);
            if (idx >= 0)
                values[idx] += amount;
            else
            {
                keys.Add(key);
                values.Add(amount);
            }
        }

        public float Get(string key)
        {
            int idx = keys.IndexOf(key);
            return idx >= 0 ? values[idx] : 0f;
        }

        public string DominantKey()
        {
            if (keys.Count == 0) return string.Empty;
            float max = -1f;
            string best = keys[0];
            for (int i = 0; i < keys.Count; i++)
            {
                if (values[i] > max) { max = values[i]; best = keys[i]; }
            }
            return best;
        }

        public float TotalValue()
        {
            float total = 0;
            foreach (var v in values) total += v;
            return total;
        }
    }
}
