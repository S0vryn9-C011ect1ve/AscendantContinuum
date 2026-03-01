using System.Collections;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

namespace AscendantContinuum.VFX
{
    /// <summary>
    /// Procedurally plays real-world celestial event VFX when
    /// <see cref="LiveEventEngine"/> fires <c>OnEventPeak</c>.
    ///
    /// Event type → visual:
    ///   lunar / eclipse     → AuroraEffect  (8 ribbon LineRenderers, green/violet)
    ///   meteor shower       → MeteorBurst   (40-particle streak burst)
    ///   equinox / solstice  → CosmicButterfly (sacred geometry wings, vertex displacement)
    ///   generic             → StarPulse     (brief white screen-edge glow)
    ///
    /// All effects: pooled particles, max 10-second lifetime, reduced-motion skip path.
    /// </summary>
    public sealed class CelestialEventVisuals : MonoBehaviour
    {
        public static CelestialEventVisuals Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Aurora")]
        [SerializeField] private int   auroraRibbons   = 8;
        [SerializeField] private float auroraWidth     = 0.12f;
        [SerializeField] private float auroraDuration  = 8f;

        [Header("Meteor Burst")]
        [SerializeField] private int   meteorCount     = 40;
        [SerializeField] private float meteorSpeed     = 18f;
        [SerializeField] private float meteorDuration  = 3f;

        [Header("Butterfly / Geometry")]
        [SerializeField] private float butterflyDuration = 6f;

        [Header("Generic Star Pulse")]
        [SerializeField] private float pulseDuration   = 2f;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnCelestialEventPeak += PlayEvent;
        }

        private void OnDisable()
        {
            GameEvents.OnCelestialEventPeak -= PlayEvent;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void PlayEvent(string eventId)
        {
            if (AccessibilityManager.Instance?.ReducedMotionEnabled == true) return;

            string lower = eventId.ToLower();
            if      (lower.Contains("lunar") || lower.Contains("eclipse"))
                StartCoroutine(AuroraCo());
            else if (lower.Contains("meteor") || lower.Contains("shower"))
                StartCoroutine(MeteorBurstCo());
            else if (lower.Contains("equinox") || lower.Contains("solstice"))
                StartCoroutine(CosmicButterflyCo());
            else
                StartCoroutine(StarPulseCo());

            Debug.Log($"[CelestialVFX] Playing event visual for: {eventId}");
        }

        // ── Aurora effect ─────────────────────────────────────────────────────

        private IEnumerator AuroraCo()
        {
            var ribbons = new LineRenderer[auroraRibbons];
            var startYOffsets = new float[auroraRibbons];
            var phases        = new float[auroraRibbons];
            var colors        = new Color[] {
                new Color(0.1f, 1f, 0.5f, 0.7f),
                new Color(0.4f, 0.2f, 1f, 0.6f),
                new Color(0.2f, 0.9f, 0.4f, 0.5f),
                new Color(0.6f, 0.1f, 1f, 0.5f)
            };

            for (int i = 0; i < auroraRibbons; i++)
            {
                var go          = new GameObject($"AuroraRibbon_{i}");
                go.transform.SetParent(transform);
                var lr          = go.AddComponent<LineRenderer>();
                lr.startWidth   = auroraWidth;
                lr.endWidth     = auroraWidth * 0.3f;
                lr.positionCount = 20;
                lr.useWorldSpace = true;
                lr.startColor   = colors[i % colors.Length];
                lr.endColor     = Color.clear;
                ribbons[i]      = lr;
                startYOffsets[i] = (i - auroraRibbons * 0.5f) * 0.8f + Random.Range(-0.3f, 0.3f);
                phases[i]        = Random.Range(0f, Mathf.PI * 2f);
            }

            float elapsed = 0f;
            while (elapsed < auroraDuration)
            {
                elapsed += Time.deltaTime;
                float fade = Mathf.Sin(elapsed / auroraDuration * Mathf.PI);

                for (int i = 0; i < auroraRibbons; i++)
                {
                    if (ribbons[i] == null) continue;
                    for (int p = 0; p < 20; p++)
                    {
                        float t   = p / 19f;
                        float x   = Mathf.Lerp(-12f, 12f, t);
                        float y   = 4f + startYOffsets[i]
                                  + Mathf.Sin(t * 3f + elapsed * 0.8f + phases[i]) * 1.2f;
                        ribbons[i].SetPosition(p, new Vector3(x, y, -1f));
                    }
                    Color c = ribbons[i].startColor;
                    c.a = c.a * fade;
                    ribbons[i].startColor = c;
                }
                yield return null;
            }

            foreach (var r in ribbons)
                if (r != null) Destroy(r.gameObject);
        }

        // ── Meteor burst ──────────────────────────────────────────────────────

        private IEnumerator MeteorBurstCo()
        {
            var trails = new (GameObject go, Vector3 dir, float life)[meteorCount];

            for (int i = 0; i < meteorCount; i++)
            {
                var go        = new GameObject($"Meteor_{i}");
                go.transform.SetParent(transform);
                var lr        = go.AddComponent<LineRenderer>();
                lr.startWidth = 0.04f;
                lr.endWidth   = 0f;
                lr.positionCount = 2;
                lr.startColor = new Color(1f, 0.95f, 0.8f, 0.9f);
                lr.endColor   = Color.clear;
                lr.useWorldSpace = true;

                // Spawn in top-half of screen, moving downward-diagonally
                Vector3 start = new Vector3(Random.Range(-10f, 10f), Random.Range(4f, 9f), 0f);
                Vector3 dir   = new Vector3(Random.Range(-0.3f, 0.3f), -1f, 0f).normalized;
                go.transform.position = start;
                lr.SetPosition(0, start);
                lr.SetPosition(1, start);

                trails[i] = (go, dir, 0f);
            }

            float elapsed = 0f;
            while (elapsed < meteorDuration)
            {
                elapsed += Time.deltaTime;
                for (int i = 0; i < meteorCount; i++)
                {
                    if (trails[i].go == null) continue;
                    trails[i].life           += Time.deltaTime;
                    Vector3 head              = trails[i].go.transform.position + trails[i].dir * meteorSpeed * Time.deltaTime;
                    trails[i].go.transform.position = head;
                    var lr = trails[i].go.GetComponent<LineRenderer>();
                    if (lr != null)
                    {
                        lr.SetPosition(0, head);
                        lr.SetPosition(1, head - trails[i].dir * 0.5f);
                        float alpha = 1f - trails[i].life / meteorDuration;
                        lr.startColor = new Color(1f, 0.95f, 0.8f, alpha);
                    }
                    trails[i] = (trails[i].go, trails[i].dir, trails[i].life);
                }
                yield return null;
            }

            foreach (var t in trails)
                if (t.go != null) Destroy(t.go);
        }

        // ── Cosmic butterfly ──────────────────────────────────────────────────

        private IEnumerator CosmicButterflyCo()
        {
            // Build sacred-geometry butterfly as two mirrored wing LineRenderers
            var leftWing  = BuildWing("LeftWing",  1f);
            var rightWing = BuildWing("RightWing", -1f);

            float elapsed = 0f;
            while (elapsed < butterflyDuration)
            {
                elapsed += Time.deltaTime;
                float fade  = Mathf.Sin(elapsed / butterflyDuration * Mathf.PI);
                float flap  = Mathf.Abs(Mathf.Sin(elapsed * 1.2f)) * 0.25f;

                // Animate wing flap via y-scale
                if (leftWing  != null) leftWing.transform.localScale  = new Vector3(1f, 1f + flap, 1f);
                if (rightWing != null) rightWing.transform.localScale = new Vector3(1f, 1f + flap, 1f);

                // Fade alpha
                ApplyLRAlpha(leftWing,  fade * 0.7f);
                ApplyLRAlpha(rightWing, fade * 0.7f);

                yield return null;
            }

            if (leftWing  != null) Destroy(leftWing.gameObject);
            if (rightWing != null) Destroy(rightWing.gameObject);
        }

        private LineRenderer BuildWing(string name, float xSign)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            var lr        = go.AddComponent<LineRenderer>();
            lr.startWidth = 0.03f;
            lr.endWidth   = 0.03f;
            lr.startColor = new Color(1f, 0.85f, 0.4f, 0.8f);
            lr.endColor   = new Color(0.6f, 0.2f, 1f,  0.3f);
            lr.useWorldSpace = true;
            lr.loop       = true;

            // Vesica piscis-inspired wing outline (8 points)
            int pts = 12;
            lr.positionCount = pts;
            for (int i = 0; i < pts; i++)
            {
                float t   = i / (float)pts * Mathf.PI;
                float x   = xSign * (0.8f + Mathf.Sin(t) * 1.5f);
                float y   = Mathf.Sin(t * 2f) * 1.8f;
                lr.SetPosition(i, new Vector3(x, y + 1f, 0f));
            }
            return lr;
        }

        private static void ApplyLRAlpha(LineRenderer lr, float alpha)
        {
            if (lr == null) return;
            Color sc = lr.startColor; sc.a = alpha; lr.startColor = sc;
            Color ec = lr.endColor;   ec.a = alpha; lr.endColor   = ec;
        }

        // ── Generic star pulse ────────────────────────────────────────────────

        private IEnumerator StarPulseCo()
        {
            // Simple quick burst of bright particles via ParticleManager
            if (ParticleManager.Instance != null)
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector3 pos = new Vector3(Random.Range(-8f, 8f), Random.Range(-5f, 5f), 0f);
                    ParticleManager.Instance.PlayRealmTransitionEffect(pos,
                        new Color(1f, 0.9f, 0.6f));
                }
            }
            yield return new WaitForSeconds(pulseDuration);
        }
    }
}
