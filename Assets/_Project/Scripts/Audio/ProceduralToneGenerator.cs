using System;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Data;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Audio
{
    /// <summary>
    /// Procedural audio engine — generates all in-game tones in real-time using
    /// Unity's DSP <c>OnAudioFilterRead</c> callback.
    ///
    /// Zero audio files required.
    ///
    /// Channels:
    ///   • Drawing tone    — pitch tracks stroke velocity, timbre tracks curvature
    ///   • Completion chord — harmonic triad derived from gesture analysis
    ///   • Ambient pad     — slow 0.1 Hz breathing oscillator, modulated by collective energy
    ///   • Blessing chord  — warm resolving chord for Kindness Chain reveals
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class ProceduralToneGenerator : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Drawing Tone")]
        [SerializeField] private float baseFrequency  = 220f;  // Hz — A3
        [SerializeField] private float maxFrequency   = 880f;  // Hz — A5
        [SerializeField] private float drawingVolume  = 0.18f;

        [Header("Completion Chord")]
        [SerializeField] private float completionVolume = 0.28f;
        [SerializeField] private float chordFadeSeconds = 2.8f;

        [Header("Ambient Pad")]
        [SerializeField] private float padBaseFreq   = 55f;   // Hz — low A
        [SerializeField] private float padVolume     = 0.08f;
        [SerializeField] private float padBreathHz   = 0.08f; // breathing rate

        // ── DSP state ─────────────────────────────────────────────────────────
        private float _sampleRate;

        // Drawing tone oscillator
        private double _drawPhase;
        private float  _drawFreq;
        private float  _drawVol;
        private float  _drawCurvature;

        // Chord oscillators (3 voices for harmonic completion)
        private double _chordPhase0, _chordPhase1, _chordPhase2;
        private float  _chordFreq0,  _chordFreq1,  _chordFreq2;
        private float  _chordVol;
        private float  _chordFadeTimer;
        private bool   _chordActive;

        // Ambient pad
        private double _padPhase;
        private double _padBreathPhase;

        // Blessing chord (4-voice)
        private double _blessPhase0, _blessPhase1, _blessPhase2, _blessPhase3;
        private float  _blessFreq0, _blessFreq1, _blessFreq2, _blessFreq3;
        private float  _blessVol;
        private float  _blessFadeTimer;
        private bool   _blessActive;

        // Collective modulation
        private float _collectiveEnergy;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            _sampleRate = AudioSettings.outputSampleRate;
        }

        private void OnEnable()
        {
            GameEvents.OnSigilStrokePoint    += HandleStrokePoint;
            GameEvents.OnSigilStrokeEnded    += HandleStrokeEnded;
            GameEvents.OnSigilCompleted      += HandleSigilCompleted;
            GameEvents.OnCollectiveEnergyChanged += HandleCollectiveEnergy;
        }

        private void OnDisable()
        {
            GameEvents.OnSigilStrokePoint    -= HandleStrokePoint;
            GameEvents.OnSigilStrokeEnded    -= HandleStrokeEnded;
            GameEvents.OnSigilCompleted      -= HandleSigilCompleted;
            GameEvents.OnCollectiveEnergyChanged -= HandleCollectiveEnergy;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void HandleStrokePoint(float velocity, float curvature)
        {
            // Map velocity to pitch logarithmically
            float velNorm   = Mathf.Clamp01(velocity / 15f);
            _drawFreq       = Mathf.Lerp(baseFrequency, maxFrequency,
                                Mathf.Pow(velNorm, 0.7f));  // slight log curve
            _drawCurvature  = curvature;
            _drawVol        = drawingVolume;
        }

        private void HandleStrokeEnded()
        {
            // Fade drawing tone to silence quickly
            _drawVol = 0f;
        }

        private void HandleSigilCompleted(SigilData sigil)
        {
            // Build harmonic triad from sigil's primary color (hue → root note)
            float hue, sat, val;
            Color.RGBToHSV(sigil.primaryColor, out hue, out sat, out val);

            // Map hue [0,1] to root frequency in a musical scale
            float root = Mathf.Lerp(200f, 600f, hue);
            _chordFreq0 = root;
            _chordFreq1 = root * 1.2599f; // perfect fourth
            _chordFreq2 = root * 1.4983f; // perfect fifth

            _chordVol       = completionVolume;
            _chordFadeTimer = 0f;
            _chordActive    = true;
        }

        private void HandleCollectiveEnergy(float energy)
        {
            _collectiveEnergy = energy;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Triggers a warm resolving 4-voice blessing chord.</summary>
        public void PlayBlessingChord()
        {
            _blessFreq0 = 261.6f;  // C4
            _blessFreq1 = 329.6f;  // E4
            _blessFreq2 = 392f;    // G4
            _blessFreq3 = 523.3f;  // C5

            _blessVol       = 0.22f;
            _blessFadeTimer = 0f;
            _blessActive    = true;
        }

        /// <summary>
        /// Called from <see cref="AudioManager"/> for realm ecosystem reactions.
        /// Triggers a brief arpeggio effect by briefly setting a rising DrawFreq.
        /// </summary>
        public void PlayArpeggio(float rootHz, int steps, float stepRatio)
        {
            StartCoroutine(ArpeggioCo(rootHz, steps, stepRatio));
        }

        private System.Collections.IEnumerator ArpeggioCo(float root, int steps, float ratio)
        {
            for (int i = 0; i < steps; i++)
            {
                _drawFreq = root * Mathf.Pow(ratio, i);
                _drawVol  = drawingVolume * 0.8f;
                yield return new WaitForSeconds(0.1f);
            }
            _drawVol = 0f;
        }

        // ── DSP ───────────────────────────────────────────────────────────────

        private void OnAudioFilterRead(float[] data, int channels)
        {
            double sampleDelta = 1.0 / _sampleRate;
            float  invSamples  = 1f / data.Length;

            for (int i = 0; i < data.Length; i += channels)
            {
                float sample = 0f;

                // ── Drawing tone (sine + slight harmonic) ─────────────────────
                if (_drawVol > 0.001f)
                {
                    // Curvature adds a second harmonic for "rougher" lines
                    float s = (float)(Math.Sin(_drawPhase * 2 * Math.PI) * (1f - _drawCurvature * 0.3f)
                            + Math.Sin(_drawPhase * 4 * Math.PI) * _drawCurvature * 0.3f);
                    sample += s * _drawVol;
                    _drawPhase += _drawFreq * sampleDelta;
                    if (_drawPhase >= 1.0) _drawPhase -= 1.0;
                }

                // ── Completion chord (3 sine voices fading out) ───────────────
                if (_chordActive)
                {
                    float fade = Mathf.Clamp01(1f - _chordFadeTimer / chordFadeSeconds);
                    float cv   = _chordVol * fade;
                    sample += (float)(Math.Sin(_chordPhase0 * 2 * Math.PI)
                                    + Math.Sin(_chordPhase1 * 2 * Math.PI) * 0.7f
                                    + Math.Sin(_chordPhase2 * 2 * Math.PI) * 0.5f) * cv / 3f;

                    _chordPhase0 += _chordFreq0 * sampleDelta;
                    _chordPhase1 += _chordFreq1 * sampleDelta;
                    _chordPhase2 += _chordFreq2 * sampleDelta;
                    if (_chordPhase0 >= 1.0) _chordPhase0 -= 1.0;
                    if (_chordPhase1 >= 1.0) _chordPhase1 -= 1.0;
                    if (_chordPhase2 >= 1.0) _chordPhase2 -= 1.0;

                    _chordFadeTimer += (1f / _sampleRate);
                    if (fade <= 0f) _chordActive = false;
                }

                // ── Ambient pad (breathing oscillator, collective modulated) ──
                {
                    float breathAmp = 0.5f + 0.5f * (float)Math.Sin(_padBreathPhase * 2 * Math.PI);
                    float collAmp   = 1f + _collectiveEnergy * 0.4f; // collective boost
                    float padSample = (float)Math.Sin(_padPhase * 2 * Math.PI);
                    // Slight detuned second voice for warmth
                    padSample += (float)Math.Sin(_padPhase * 2 * Math.PI * 1.005f) * 0.4f;
                    sample    += padSample * padVolume * breathAmp * collAmp * 0.5f;

                    _padPhase       += padBaseFreq * sampleDelta;
                    _padBreathPhase += padBreathHz * sampleDelta;
                    if (_padPhase       >= 1.0) _padPhase       -= 1.0;
                    if (_padBreathPhase >= 1.0) _padBreathPhase -= 1.0;
                }

                // ── Blessing chord (4 voices fading out) ─────────────────────
                if (_blessActive)
                {
                    float fade = Mathf.Clamp01(1f - _blessFadeTimer / 4f);
                    float bv   = _blessVol * fade;
                    sample += (float)(Math.Sin(_blessPhase0 * 2 * Math.PI)
                                    + Math.Sin(_blessPhase1 * 2 * Math.PI) * 0.85f
                                    + Math.Sin(_blessPhase2 * 2 * Math.PI) * 0.7f
                                    + Math.Sin(_blessPhase3 * 2 * Math.PI) * 0.55f) * bv / 4f;

                    _blessPhase0 += _blessFreq0 * sampleDelta;
                    _blessPhase1 += _blessFreq1 * sampleDelta;
                    _blessPhase2 += _blessFreq2 * sampleDelta;
                    _blessPhase3 += _blessFreq3 * sampleDelta;
                    if (_blessPhase0 >= 1.0) _blessPhase0 -= 1.0;
                    if (_blessPhase1 >= 1.0) _blessPhase1 -= 1.0;
                    if (_blessPhase2 >= 1.0) _blessPhase2 -= 1.0;
                    if (_blessPhase3 >= 1.0) _blessPhase3 -= 1.0;

                    _blessFadeTimer += (1f / _sampleRate);
                    if (fade <= 0f) _blessActive = false;
                }

                // ── Write to all channels ─────────────────────────────────────
                float clamped = Mathf.Clamp(sample, -1f, 1f);
                for (int c = 0; c < channels; c++)
                    data[i + c] += clamped;
            }
        }
    }
}
