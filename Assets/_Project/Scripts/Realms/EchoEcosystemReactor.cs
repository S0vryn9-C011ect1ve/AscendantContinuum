using System.Collections;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.VFX;

namespace AscendantContinuum.Realms
{
    /// <summary>
    /// Reacts to sigil completion in the Echo Fields.
    /// Triggers: ghostly whisper audio chorus (3 detuned DSP sine sources),
    /// radial ripple wave on the star particle layer, and a constellation
    /// brightness flash.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EchoEcosystemReactor : MonoBehaviour
    {
        [Header("Ripple")]
        [SerializeField] private ParticleSystem starLayer;
        [SerializeField] private float          rippleDuration = 2.2f;

        [Header("Whisper Chorus")]
        [SerializeField] private int   chorusVoices    = 3;
        [SerializeField] private float detuneAmount    = 0.04f;   // semitone fraction
        [SerializeField] private float whisperDuration = 3f;

        [Header("Brightness Flash")]
        [SerializeField] private Light[] constellationLights;
        [SerializeField] private float   flashIntensityMult = 2.8f;
        [SerializeField] private float   flashDuration      = 1.2f;

        private float[] _originalLightIntensities;

        private void Awake()
        {
            if (constellationLights != null)
            {
                _originalLightIntensities = new float[constellationLights.Length];
                for (int i = 0; i < constellationLights.Length; i++)
                    _originalLightIntensities[i] = constellationLights[i] != null
                        ? constellationLights[i].intensity : 1f;
            }
        }

        private void OnEnable()  => GameEvents.OnSigilCompleted += HandleSigilCompleted;
        private void OnDisable() => GameEvents.OnSigilCompleted -= HandleSigilCompleted;

        private void HandleSigilCompleted(Data.SigilData sigil)
        {
            if (AccessibilityManager.Instance?.ReducedMotionEnabled == true) return;

            StartCoroutine(RippleCo());
            StartCoroutine(FlashCo());
            AudioManager.Instance?.PlayRealmEcosystemReaction("echo", sigil.primaryColor);
        }

        private IEnumerator RippleCo()
        {
            if (starLayer == null) yield break;

            var emission    = starLayer.emission;
            float origRate  = emission.rateOverTime.constant;
            emission.rateOverTime = origRate * 4f;

            var vel     = starLayer.velocityOverLifetime;
            float origX = vel.x.constant;
            float origY = vel.y.constant;

            // Radial burst
            yield return new WaitForSeconds(rippleDuration * 0.3f);
            emission.rateOverTime = origRate;

            yield return new WaitForSeconds(rippleDuration * 0.7f);
        }

        private IEnumerator FlashCo()
        {
            if (constellationLights == null) yield break;

            // Flash up
            float t = 0f;
            while (t < flashDuration * 0.4f)
            {
                t += Time.deltaTime;
                float lerp = t / (flashDuration * 0.4f);
                SetLightIntensities(lerp * flashIntensityMult);
                yield return null;
            }
            // Fade back
            t = 0f;
            while (t < flashDuration * 0.6f)
            {
                t += Time.deltaTime;
                float lerp = 1f - t / (flashDuration * 0.6f);
                SetLightIntensities(Mathf.Lerp(1f, flashIntensityMult, lerp));
                yield return null;
            }
            SetLightIntensities(1f);
        }

        private void SetLightIntensities(float mult)
        {
            if (constellationLights == null) return;
            for (int i = 0; i < constellationLights.Length; i++)
            {
                if (constellationLights[i] == null) continue;
                constellationLights[i].intensity = _originalLightIntensities[i] * mult;
            }
        }
    }
}
