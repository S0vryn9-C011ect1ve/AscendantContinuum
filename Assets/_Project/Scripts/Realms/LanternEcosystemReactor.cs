using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Realms
{
    /// <summary>
    /// Reacts to sigil completion in Lantern Ascension.
    /// Triggers: active lanterns drift toward a loose cluster then re-scatter,
    /// lantern glows pulse in sync with the completion harmonic, soft gold
    /// ambient fade-in.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LanternEcosystemReactor : MonoBehaviour
    {
        [Header("Cluster Drift")]
        [SerializeField] private float clusterRadius   = 1.8f;
        [SerializeField] private float driftDuration   = 1.4f;
        [SerializeField] private float scatterDelay    = 0.9f;
        [SerializeField] private float scatterDuration = 1.8f;

        [Header("Glow Pulse")]
        [SerializeField] private Light[] lanternLights;
        [SerializeField] private float   pulsePeakMult = 2.2f;
        [SerializeField] private float   pulseDuration = 2f;

        [Header("Ambient Gold")]
        [SerializeField] private float ambientFadeDuration = 3f;
        [SerializeField] private Color goldAmbient         = new Color(1f, 0.85f, 0.3f);

        private OnEnable_OnDisable_Lanterns _subscription;
        private Color _originalAmbient;
        private float[] _originalLightIntensities;

        private void Awake()
        {
            _originalAmbient = RenderSettings.ambientLight;
            if (lanternLights != null)
            {
                _originalLightIntensities = new float[lanternLights.Length];
                for (int i = 0; i < lanternLights.Length; i++)
                    _originalLightIntensities[i] = lanternLights[i] != null ? lanternLights[i].intensity : 1f;
            }
        }

        private void OnEnable()  => GameEvents.OnSigilCompleted += HandleSigilCompleted;
        private void OnDisable() => GameEvents.OnSigilCompleted -= HandleSigilCompleted;

        private void HandleSigilCompleted(Data.SigilData sigil)
        {
            if (AccessibilityManager.Instance?.ReducedMotionEnabled == true) return;

            StartCoroutine(ClusterDriftCo());
            StartCoroutine(GlowPulseCo());
            StartCoroutine(AmbientFadeCo());
            AudioManager.Instance?.PlayRealmEcosystemReaction("lantern", sigil.primaryColor);
        }

        private IEnumerator ClusterDriftCo()
        {
            // Gather active lantern GameObjects in this scene
            var lanterns = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            var lanternList = new List<(Transform t, Vector3 origin)>();
            foreach (var lt in lanterns)
            {
                if (lt.gameObject.name.StartsWith("Lantern"))
                    lanternList.Add((lt, lt.position));
            }

            if (lanternList.Count == 0) yield break;

            Vector3 clusterCenter = transform.position + Vector3.up * 3f;

            // Drift toward cluster
            float t = 0f;
            while (t < driftDuration)
            {
                t += Time.deltaTime;
                float lerp = t / driftDuration;
                Vector3 clusterOffset = Random.insideUnitSphere * clusterRadius;
                foreach (var (lt, origin) in lanternList)
                {
                    if (lt == null) continue;
                    lt.position = Vector3.Lerp(origin, clusterCenter + clusterOffset, Mathf.SmoothStep(0, 1, lerp));
                }
                yield return null;
            }

            yield return new WaitForSeconds(scatterDelay);

            // Re-scatter back
            t = 0f;
            foreach (var (lt, origin) in lanternList)
            {
                if (lt == null) continue;
                Vector3 captured = lt.position;
                StartCoroutine(LerpBack(lt, captured, origin, scatterDuration));
            }
        }

        private IEnumerator LerpBack(Transform lt, Vector3 from, Vector3 to, float dur)
        {
            float t = 0f;
            while (t < dur && lt != null)
            {
                t += Time.deltaTime;
                lt.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t / dur));
                yield return null;
            }
            if (lt != null) lt.position = to;
        }

        private IEnumerator GlowPulseCo()
        {
            if (lanternLights == null) yield break;
            float half = pulseDuration * 0.5f;
            float t = 0f;

            while (t < half)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(1f, pulsePeakMult, t / half);
                for (int i = 0; i < lanternLights.Length; i++)
                    if (lanternLights[i] != null)
                        lanternLights[i].intensity = _originalLightIntensities[i] * v;
                yield return null;
            }
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(pulsePeakMult, 1f, t / half);
                for (int i = 0; i < lanternLights.Length; i++)
                    if (lanternLights[i] != null)
                        lanternLights[i].intensity = _originalLightIntensities[i] * v;
                yield return null;
            }
        }

        private IEnumerator AmbientFadeCo()
        {
            float t = 0f;
            float half = ambientFadeDuration * 0.5f;

            // Fade in gold
            while (t < half)
            {
                t += Time.deltaTime;
                RenderSettings.ambientLight = Color.Lerp(_originalAmbient, goldAmbient, t / half);
                yield return null;
            }
            // Fade back
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                RenderSettings.ambientLight = Color.Lerp(goldAmbient, _originalAmbient, t / half);
                yield return null;
            }
            RenderSettings.ambientLight = _originalAmbient;
        }

        // Helper: dummy class to avoid CS0029 on field
        private sealed class OnEnable_OnDisable_Lanterns { }
    }
}
