using System.Collections;
using UnityEngine;
using TMPro;
using AscendantContinuum.Core;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Displays a single, quietly poetic real-world nudge after sigil completion.
    ///
    /// Rules:
    ///   • Never repeated within 24 hours
    ///   • Never a popup or button — just a fading text line
    ///   • Weighted by time-of-day, season, and active celestial event
    ///   • Optionally includes the cosmic quote system's current quote
    /// </summary>
    public sealed class RealWorldNudgeSystem : MonoBehaviour
    {
        public static RealWorldNudgeSystem Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("UI")]
        [SerializeField] private TMP_Text nudgeLabel;
        [SerializeField] private CanvasGroup nudgeGroup;

        [Header("Timing")]
        [SerializeField] private float fadeInSeconds  = 3.5f;
        [SerializeField] private float holdSeconds    = 5f;
        [SerializeField] private float fadeOutSeconds = 4f;

        // ── Nudge string tables ───────────────────────────────────────────────

        private static readonly string[] NightNudges = {
            "The stars are patient.",
            "Look up.",
            "The sky is waiting.",
            "Something vast is above you, right now.",
            "Step outside for a moment.",
            "The night remembers everything."
        };

        private static readonly string[] DawnNudges = {
            "The sky is changing.",
            "A new light is arriving somewhere.",
            "The horizon has something to say.",
            "Dawn happens whether you watch or not.",
            "What is the first thing you notice outside?"
        };

        private static readonly string[] DayNudges = {
            "Notice what is above you.",
            "The sky is its own kind of art.",
            "Look at something that isn't a screen.",
            "Step outside. Just for a breath.",
            "The world outside is still there."
        };

        private static readonly string[] DuskNudges = {
            "The light is doing something extraordinary right now.",
            "Dusk asks nothing of you.",
            "The day is finishing. How did it feel?",
            "The sky shifts colors when you aren't watching.",
            "The horizon is softening."
        };

        private static readonly string[] CelestialNudges = {
            "Something rare is happening in the sky tonight.",
            "A celestial event is unfolding above you.",
            "Step outside. The sky has something to show you.",
            "The cosmos is moving. You are part of it."
        };

        private static readonly string[] WinterNudges = {
            "The quiet of winter holds its own light.",
            "Cold air carries sound differently. Listen.",
            "Stars are sharper in winter. Look up."
        };

        private static readonly string[] SummerNudges = {
            "Long evenings are rare. Use one.",
            "The warmth is generous tonight.",
            "Look at something green."
        };

        // ── State ─────────────────────────────────────────────────────────────
        private const string PREF_LAST_NUDGE = "LastNudgeDate";
        private bool _isShowing;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (nudgeGroup != null) nudgeGroup.alpha = 0f;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void ShowNudge()
        {
            string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (PlayerPrefs.GetString(PREF_LAST_NUDGE, "") == today) return; // once per day

            string text = SelectNudge();
            if (string.IsNullOrEmpty(text)) return;

            PlayerPrefs.SetString(PREF_LAST_NUDGE, today);
            StartCoroutine(ShowNudgeCo(text));
        }

        // ── Selection logic ────────────────────────────────────────────────────

        private string SelectNudge()
        {
            // Celestial event overrides everything
            if (LiveEventEngine.Instance?.IsEventActive == true)
                return CelestialNudges[Random.Range(0, CelestialNudges.Length)];

            // Cosmic quote system overrides (30% chance)
            if (Random.value < 0.3f && CosmicQuoteSystem.Instance != null)
                return CosmicQuoteSystem.Instance.GetNudgeQuote();

            // Time-of-day
            int hour = System.DateTime.Now.Hour;
            if      (hour >= 20 || hour < 5)  return NightNudges[Random.Range(0, NightNudges.Length)];
            else if (hour >= 5  && hour < 9)   return DawnNudges[Random.Range(0, DawnNudges.Length)];
            else if (hour >= 18 && hour < 20)  return DuskNudges[Random.Range(0, DuskNudges.Length)];

            // Seasonal
            var season = SkyTimeSystem.Instance?.CurrentSeason ?? SkyTimeSystem.SeasonType.Summer;
            string[] pool = season == SkyTimeSystem.SeasonType.Winter ? WinterNudges :
                            season == SkyTimeSystem.SeasonType.Summer ? SummerNudges :
                            DayNudges;
            return pool[Random.Range(0, pool.Length)];
        }

        // ── Display coroutine ─────────────────────────────────────────────────

        private IEnumerator ShowNudgeCo(string text)
        {
            if (_isShowing) yield break;
            _isShowing = true;

            if (nudgeLabel  != null) nudgeLabel.text = text;
            if (nudgeGroup  == null) { _isShowing = false; yield break; }

            // Fade in
            float t = 0f;
            while (t < fadeInSeconds)
            {
                t += Time.unscaledDeltaTime;
                nudgeGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInSeconds);
                yield return null;
            }
            nudgeGroup.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(holdSeconds);

            // Fade out
            t = 0f;
            while (t < fadeOutSeconds)
            {
                t += Time.unscaledDeltaTime;
                nudgeGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutSeconds);
                yield return null;
            }
            nudgeGroup.alpha = 0f;
            _isShowing = false;
        }
    }
}
