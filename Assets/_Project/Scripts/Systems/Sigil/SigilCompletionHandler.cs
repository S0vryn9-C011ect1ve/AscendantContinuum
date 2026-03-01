using System.Collections;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Data;
using AscendantContinuum.VFX;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Orchestrates the full ritual sequence when a sigil drawing is committed.
    ///
    /// Sequence (each step can be abbreviated or skipped by reduced-motion):
    ///   1. Receive <see cref="SigilAnalysisResult"/> from <see cref="GameEvents"/>
    ///   2. Generate <see cref="SigilData"/> via <see cref="SigilGenerator"/>
    ///   3. Harmonic bloom particle burst
    ///   4. Soft time-scale breath (disabled in reduced-motion)
    ///   5. Sky tint pulse via <see cref="Realms.RealmAtmosphereController"/>
    ///   6. Haptic completion pulse
    ///   7. Fire <see cref="GameEvents.OnSigilCompleted"/>
    ///   8. Advance session to <see cref="SessionPhase.Reflecting"/>
    ///   9. Trigger <see cref="ContinuumFieldManager"/> increment
    ///  10. Hand off to <see cref="RealWorldNudgeSystem"/>
    /// </summary>
    public sealed class SigilCompletionHandler : MonoBehaviour
    {
        public static SigilCompletionHandler Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Time Scale Breath")]
        [SerializeField] private float timeSlowTarget   = 0.55f;
        [SerializeField] private float timeSlowDuration = 0.45f;
        [SerializeField] private float timeSlowHold     = 0.6f;

        [Header("Bloom Burst")]
        [SerializeField] private int   bloomParticleCount  = 48;
        [SerializeField] private float bloomRadius         = 1.2f;

        [Header("Sky Pulse")]
        [SerializeField] private float skyPulseDuration    = 1.8f;
        [SerializeField] private Color skyPulseColorAdd    = new Color(0.15f, 0.08f, 0.3f, 0f);

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()  => GameEvents.OnSigilDrawingCommitted += HandleCommit;
        private void OnDisable() => GameEvents.OnSigilDrawingCommitted -= HandleCommit;

        // ── Entry point ────────────────────────────────────────────────────────

        private void HandleCommit(SigilAnalysisResult analysis)
        {
            StartCoroutine(RunRitualSequence(analysis));
        }

        private IEnumerator RunRitualSequence(SigilAnalysisResult analysis)
        {
            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled ?? false;

            // ── 1. Advance session phase ──────────────────────────────────────
            SessionManager.Instance?.SetPhase(SessionPhase.SkyReacting);

            // ── 2. Generate SigilData ─────────────────────────────────────────
            SigilData sigil = null;
            if (SigilGenerator.Instance != null)
            {
                // Build temporary metrics from analysis for the generator
                var metrics = new PlayerPlaystyleMetrics();
                sigil = SigilGenerator.Instance.GenerateSigil(analysis, metrics);
            }
            else
            {
                sigil = BuildFallbackSigil(analysis);
            }

            // ── 3. Sky tint pulse ──────────────────────────────────────────────
            StartCoroutine(PulseSkyCo(reducedMotion));

            // ── 4. Harmonic bloom burst ────────────────────────────────────────
            if (ParticleManager.Instance != null)
            {
                Color burstColor = sigil != null ? sigil.primaryColor : Color.white;
                burstColor.a = 1f;
                for (int i = 0; i < bloomParticleCount; i++)
                {
                    Vector3 offset = Random.insideUnitCircle * bloomRadius;
                    ParticleManager.Instance.PlayCompletionBloom(
                        transform.position + offset, burstColor, analysis.Complexity);
                }
            }

            // ── 5. Haptic pulse ───────────────────────────────────────────────
            AccessibilityManager.Instance?.TriggerHaptic(HapticType.MediumImpact);

            // ── 6. Audio completion tone ──────────────────────────────────────
            AudioManager.Instance?.PlaySigilCompletion(analysis);

            // ── 7. Time-scale breath ──────────────────────────────────────────
            if (!reducedMotion)
                yield return StartCoroutine(TimeBreathCo());

            // ── 8. Advance to Blooming ────────────────────────────────────────
            SessionManager.Instance?.SetPhase(SessionPhase.Blooming);
            yield return new WaitForSecondsRealtime(0.3f);

            // ── 9. Fire completion event ──────────────────────────────────────
            if (sigil != null)
            {
                GameEvents.RaiseSigilCompleted(sigil);

                // Persist sigil count
                int count = PlayerPrefs.GetInt("SigilCount", 0) + 1;
                PlayerPrefs.SetInt("SigilCount", count);
                SaveSystem.Instance?.BumpSigilCount();
            }

            // ── 10. Collective field increment ────────────────────────────────
            ContinuumFieldManager.Instance?.RegisterSigilCompleted();

            // ── 11. Advance to Reflecting ─────────────────────────────────────
            SessionManager.Instance?.SetPhase(SessionPhase.Reflecting);
            yield return new WaitForSecondsRealtime(1.5f);

            // ── 12. Real-world nudge ──────────────────────────────────────────
            SessionManager.Instance?.SetPhase(SessionPhase.Nudging);
            RealWorldNudgeSystem.Instance?.ShowNudge();

            // ── 13. Export artifact ───────────────────────────────────────────
            if (sigil != null)
                SigilArtifactExporter.Instance?.CaptureAndSave(sigil);

            Debug.Log("[SigilCompletion] Ritual complete.");
        }

        // ── Coroutines ────────────────────────────────────────────────────────

        private IEnumerator TimeBreathCo()
        {
            // Slow
            float elapsed = 0f;
            while (elapsed < timeSlowDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Lerp(1f, timeSlowTarget, elapsed / timeSlowDuration);
                yield return null;
            }
            Time.timeScale = timeSlowTarget;

            // Hold
            yield return new WaitForSecondsRealtime(timeSlowHold);

            // Resume
            elapsed = 0f;
            while (elapsed < timeSlowDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Lerp(timeSlowTarget, 1f, elapsed / timeSlowDuration);
                yield return null;
            }
            Time.timeScale = 1f;
        }

        private IEnumerator PulseSkyCo(bool reducedMotion)
        {
            if (reducedMotion) yield break;

            var atmo = FindFirstObjectByType<Realms.RealmAtmosphereController>();
            if (atmo == null) yield break;

            float half = skyPulseDuration * 0.5f;
            float t = 0f;

            // Pulse in
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float lerp = t / half;
                atmo.SetCompletionOverlay(Color.Lerp(Color.clear, skyPulseColorAdd, lerp));
                yield return null;
            }

            // Pulse out
            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float lerp = t / half;
                atmo.SetCompletionOverlay(Color.Lerp(skyPulseColorAdd, Color.clear, lerp));
                yield return null;
            }

            atmo.SetCompletionOverlay(Color.clear);
        }

        // ── Fallback SigilData ────────────────────────────────────────────────

        private static SigilData BuildFallbackSigil(SigilAnalysisResult a)
        {
            Color primary = Color.Lerp(new Color(0.5f, 0.3f, 1f), new Color(1f, 0.7f, 0.3f), a.Complexity);
            return new SigilData
            {
                sigilId      = System.Guid.NewGuid().ToString(),
                baseShape    = Mathf.Clamp(a.closedLoopCount, 0, 4),
                pattern      = Mathf.Clamp(a.strokeCount - 1, 0, 4),
                primaryColor = primary,
                secondaryColor = Color.Lerp(primary, Color.white, 0.4f),
                generatedFromPlaystyle = JsonUtility.ToJson(a)
            };
        }
    }
}
