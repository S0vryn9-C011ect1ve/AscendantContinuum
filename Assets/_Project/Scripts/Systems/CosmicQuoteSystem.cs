using System.Collections.Generic;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Curated deck of 40 cosmic/mindful quotes.
    /// Provides weighted selection for nudge overlays and session closes.
    ///
    /// Selection logic:
    ///   • 20% chance: quotes tagged for the current season
    ///   • 15% chance: quotes tagged for the current time-of-day
    ///   • Remaining: uniform random from full deck
    ///
    /// Tracks "recently shown" to avoid repeats within 10 draws.
    /// </summary>
    public sealed class CosmicQuoteSystem : MonoBehaviour
    {
        public static CosmicQuoteSystem Instance { get; private set; }

        // ── Quote data ────────────────────────────────────────────────────────

        private struct Quote
        {
            public string Text;
            public string Author;
            public string Tag;    // "night","dawn","celestial","seasonal","universal"
        }

        private static readonly Quote[] Deck = new Quote[]
        {
            // ──────────────── Universal ───────────────────────────────────────
            new Quote { Text = "You are the sky. Everything else is just the weather.", Author = "Pema Chödrön",         Tag = "universal"  },
            new Quote { Text = "The cosmos is within us. We are made of star-stuff.",   Author = "Carl Sagan",           Tag = "universal"  },
            new Quote { Text = "Be still like a mountain, flow like a great river.",    Author = "Lao Tzu",              Tag = "universal"  },
            new Quote { Text = "Every moment contains an eternity.",                   Author = "ancient wisdom",       Tag = "universal"  },
            new Quote { Text = "The light you seek is the light you are.",             Author = "Ascendant Continuum",  Tag = "universal"  },
            new Quote { Text = "What you draw in darkness, the stars remember.",        Author = "Ascendant Continuum",  Tag = "universal"  },
            new Quote { Text = "A single breath connects you to ten thousand years.",   Author = "Ascendant Continuum",  Tag = "universal"  },
            new Quote { Text = "Silence is the language of the cosmos.",               Author = "Ascendant Continuum",  Tag = "universal"  },
            new Quote { Text = "Each sketch you make is a conversation with the void.", Author = "Ascendant Continuum",  Tag = "universal"  },
            new Quote { Text = "The whole universe fits inside a single gesture.",      Author = "Ascendant Continuum",  Tag = "universal"  },
            new Quote { Text = "You are not separate from what you observe.",          Author = "Ascendant Continuum",  Tag = "universal"  },
            new Quote { Text = "Rivers know this: there is no hurry. We shall get there some day.", Author = "A. A. Milne", Tag = "universal" },
            new Quote { Text = "The quieter you become, the more you can hear.",       Author = "Ram Dass",             Tag = "universal"  },
            new Quote { Text = "In the middle of difficulty lies opportunity.",        Author = "Albert Einstein",      Tag = "universal"  },
            new Quote { Text = "Everything you can imagine is real.",                  Author = "Pablo Picasso",        Tag = "universal"  },

            // ──────────────── Night ──────────────────────────────────────────
            new Quote { Text = "The night sky is a mirror and you are its star.",      Author = "Ascendant Continuum",  Tag = "night"      },
            new Quote { Text = "Stars do not compete — they simply shine.",            Author = "Ascendant Continuum",  Tag = "night"      },
            new Quote { Text = "In the depth of winter I finally learned there was in me an invincible summer.", Author = "Albert Camus", Tag = "night" },
            new Quote { Text = "The moon does not fight. It shines.",                 Author = "Deng Ming-Dao",        Tag = "night"      },
            new Quote { Text = "Night is the other half of every day.",               Author = "Ascendant Continuum",  Tag = "night"      },

            // ──────────────── Dawn ───────────────────────────────────────────
            new Quote { Text = "Awakening is not changing who you are, but discarding who you are not.", Author = "Deepak Chopra", Tag = "dawn" },
            new Quote { Text = "With the new day comes new strength and new thoughts.", Author = "Eleanor Roosevelt",  Tag = "dawn"       },
            new Quote { Text = "The morning stars sang together.",                     Author = "Job 38:7",            Tag = "dawn"       },
            new Quote { Text = "Each dawn is a new cosmos awaiting your mark.",        Author = "Ascendant Continuum",  Tag = "dawn"       },
            new Quote { Text = "The first light does not know what it will illuminate.", Author = "Ascendant Continuum", Tag = "dawn"      },

            // ──────────────── Celestial events ───────────────────────────────
            new Quote { Text = "When the stars align, your intention becomes architecture.", Author = "Ascendant Continuum", Tag = "celestial" },
            new Quote { Text = "Eclipses teach us that darkness is temporary.",        Author = "Ascendant Continuum",  Tag = "celestial"  },
            new Quote { Text = "Meteor showers are the sky remembering what it forgot.", Author = "Ascendant Continuum", Tag = "celestial" },
            new Quote { Text = "The solstice is the universe pausing to breathe.",    Author = "Ascendant Continuum",  Tag = "celestial"  },
            new Quote { Text = "Some things must eclipse before they can illuminate.", Author = "Ascendant Continuum",  Tag = "celestial"  },

            // ──────────────── Seasonal ────────────────────────────────────────
            new Quote { Text = "In autumn, even the falling leaves know how to let go gracefully.", Author = "Ascendant Continuum", Tag = "seasonal" },
            new Quote { Text = "Winter is not the death of life — it is its quiet preparation.", Author = "Ascendant Continuum", Tag = "seasonal" },
            new Quote { Text = "Spring does not ask permission to bloom.",              Author = "Ascendant Continuum",  Tag = "seasonal"   },
            new Quote { Text = "Summer contains the memory of every sun that ever rose.", Author = "Ascendant Continuum", Tag = "seasonal"  },
            new Quote { Text = "The seasons are the universe practicing patience.",    Author = "Ascendant Continuum",  Tag = "seasonal"   },

            // ──────────────── Collective ─────────────────────────────────────
            new Quote { Text = "Right now, someone across the world drew the same sigil as you.", Author = "Ascendant Continuum", Tag = "universal" },
            new Quote { Text = "The field between people is more real than the people themselves.", Author = "Ascendant Continuum", Tag = "universal" },
            new Quote { Text = "Consciousness is not in the brain — it is between us.",  Author = "Ascendant Continuum",  Tag = "universal"  },
            new Quote { Text = "We are the cosmos made conscious.",                    Author = "Brian Cox",            Tag = "universal"  },
            new Quote { Text = "Your attention is the most precious gift you can give.", Author = "Ascendant Continuum", Tag = "universal"  },
        };

        // ── Internal state ────────────────────────────────────────────────────

        private readonly Queue<int> _recentlyShown = new Queue<int>(10);

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a quote appropriate for a <see cref="RealWorldNudgeSystem"/> nudge.
        /// Respects recency deduplication.
        /// </summary>
        public string GetNudgeQuote()
        {
            var q = Select(GetCurrentTimeTag());
            return FormatQuote(q);
        }

        /// <summary>Returns a quote for session-close Reflecting phase.</summary>
        public string GetQuoteForSession()
        {
            var q = Select("universal");
            return FormatQuote(q);
        }

        /// <summary>Returns a quote tuned for a celestial event overlay.</summary>
        public string GetCelestialQuote()
        {
            var q = Select("celestial");
            return FormatQuote(q);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private Quote Select(string preferredTag)
        {
            // Build candidate list, excluding recent indices
            var preferred  = new List<int>();
            var fallback   = new List<int>();

            for (int i = 0; i < Deck.Length; i++)
            {
                if (_recentlyShown.Contains(i)) continue;
                if (Deck[i].Tag == preferredTag) preferred.Add(i);
                else fallback.Add(i);
            }

            // Seasonal override: if seasonal tag available use it 20% of the time
            if (GetSeasonTag() != null && Random.value < 0.2f)
            {
                var seasonal = FindByTag(GetSeasonTag());
                if (seasonal >= 0) return ReturnAndRecord(seasonal);
            }

            List<int> pool = preferred.Count > 0 ? preferred : fallback;
            if (pool.Count == 0) { _recentlyShown.Clear(); pool = fallback; }

            int idx = pool[Random.Range(0, pool.Count)];
            return ReturnAndRecord(idx);
        }

        private Quote ReturnAndRecord(int idx)
        {
            _recentlyShown.Enqueue(idx);
            if (_recentlyShown.Count > 10) _recentlyShown.Dequeue();
            return Deck[idx];
        }

        private int FindByTag(string tag)
        {
            var candidates = new List<int>();
            for (int i = 0; i < Deck.Length; i++)
                if (Deck[i].Tag == tag && !_recentlyShown.Contains(i))
                    candidates.Add(i);
            return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : -1;
        }

        private static string FormatQuote(Quote q)
            => $"\"{q.Text}\"\n— {q.Author}";

        private static string GetCurrentTimeTag()
        {
            if (SkyTimeSystem.Instance == null) return "universal";
            return SkyTimeSystem.Instance.CurrentTimePeriod switch
            {
                SkyTimeSystem.TimePeriod.Night   => "night",
                SkyTimeSystem.TimePeriod.Dawn    => "dawn",
                _                                => "universal"
            };
        }

        private static string GetSeasonTag()
        {
            if (SkyTimeSystem.Instance == null) return null;
            return SkyTimeSystem.Instance.CurrentSeason switch
            {
                SkyTimeSystem.SeasonType.Winter => "seasonal",
                SkyTimeSystem.SeasonType.Summer => "seasonal",
                SkyTimeSystem.SeasonType.Spring => "seasonal",
                SkyTimeSystem.SeasonType.Autumn => "seasonal",
                _                               => null
            };
        }
    }
}
