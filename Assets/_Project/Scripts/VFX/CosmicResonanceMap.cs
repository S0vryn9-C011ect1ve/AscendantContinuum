using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

namespace AscendantContinuum.VFX
{
    /// <summary>
    /// Renders the Continuum Field background layer:
    /// 40–60 faint translucent sigil-fragment particles drifting slowly
    /// across the scene background. Fragment count and opacity scale with
    /// <see cref="ContinuumFieldManager.GetCollectiveEnergy()"/>.
    ///
    /// Uses a single <see cref="ParticleSystem"/> for budget efficiency (1 draw call).
    /// Fragment sprites are generated at runtime as simple 16×16 cross/circle glyphs.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class CosmicResonanceMap : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Fragment Pool")]
        [SerializeField] private int   baseFragmentCount = 40;
        [SerializeField] private int   maxFragmentCount  = 60;
        [SerializeField] private float driftSpeed        = 0.12f;

        [Header("Ripple")]
        [SerializeField] private float rippleInterval    = 90f;   // seconds between auto-pulses
        [SerializeField] private float rippleDuration    = 2.8f;

        // ── Runtime state ──────────────────────────────────────────────────────
        private ParticleSystem _ps;
        private ParticleSystemRenderer _psr;
        private float _energyLevel;
        private float _rippleTimer;
        private bool  _rippleActive;
        private float _ripplePhase;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            _ps  = GetComponent<ParticleSystem>();
            _psr = GetComponent<ParticleSystemRenderer>();

            ConfigureParticleSystem();
            BuildFragmentTexture();
        }

        private void OnEnable()
        {
            GameEvents.OnCollectiveEnergyChanged += OnEnergyChanged;
            GameEvents.OnCollectivePresencePulse  += TriggerRipple;
            GameEvents.OnSigilCompleted           += _ => TriggerRipple();
        }

        private void OnDisable()
        {
            GameEvents.OnCollectiveEnergyChanged -= OnEnergyChanged;
            GameEvents.OnCollectivePresencePulse  -= TriggerRipple;
            GameEvents.OnSigilCompleted           -= _ => TriggerRipple();
        }

        private void Start()
        {
            // Initial energy from manager
            if (ContinuumFieldManager.Instance != null)
                OnEnergyChanged(ContinuumFieldManager.Instance.GetCollectiveEnergy());
        }

        private void Update()
        {
            // Auto ripple on interval
            _rippleTimer += Time.deltaTime;
            if (_rippleTimer >= rippleInterval)
            {
                _rippleTimer = 0f;
                TriggerRipple();
            }

            // Animate ripple: scale emission rate
            if (_rippleActive)
            {
                _ripplePhase += Time.deltaTime / rippleDuration;
                if (_ripplePhase >= 1f)
                {
                    _rippleActive = false;
                    _ripplePhase  = 0f;
                    ApplyEnergy(_energyLevel);
                }
                else
                {
                    float peak   = Mathf.Sin(_ripplePhase * Mathf.PI);
                    var   em     = _ps.emission;
                    em.rateOverTime = Mathf.Lerp(
                        GetTargetEmissionRate(_energyLevel),
                        GetTargetEmissionRate(_energyLevel) * 2.5f,
                        peak);
                }
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void TriggerRipple()
        {
            _rippleActive = true;
            _ripplePhase  = 0f;
        }

        // ── Private ────────────────────────────────────────────────────────────

        private void OnEnergyChanged(float energy)
        {
            _energyLevel = energy;
            ApplyEnergy(energy);
        }

        private void ApplyEnergy(float energy)
        {
            var main  = _ps.main;
            var em    = _ps.emission;

            int count = Mathf.RoundToInt(Mathf.Lerp(baseFragmentCount, maxFragmentCount, energy));
            main.maxParticles       = count;
            em.rateOverTime         = GetTargetEmissionRate(energy);

            // Fragment opacity scales with energy
            float alpha = Mathf.Lerp(0.04f, 0.18f, energy);
            var   col   = main.startColor;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 1f, 1f, alpha * 0.5f),
                new Color(0.7f, 0.5f, 1f, alpha));
        }

        private static float GetTargetEmissionRate(float energy)
            => Mathf.Lerp(1.5f, 5f, energy);

        private void ConfigureParticleSystem()
        {
            var main    = _ps.main;
            main.loop             = true;
            main.startLifetime    = new ParticleSystem.MinMaxCurve(12f, 24f);
            main.startSpeed       = new ParticleSystem.MinMaxCurve(driftSpeed * 0.5f, driftSpeed);
            main.startSize        = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            main.startRotation    = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.gravityModifier  = 0f;
            main.simulationSpace  = ParticleSystemSimulationSpace.World;

            var shape   = _ps.shape;
            shape.enabled      = true;
            shape.shapeType    = ParticleSystemShapeType.Rectangle;
            shape.scale        = new Vector3(20f, 12f, 1f);

            var vel     = _ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-driftSpeed, driftSpeed * 0.3f);
            vel.y = new ParticleSystem.MinMaxCurve(driftSpeed * 0.1f, driftSpeed * 0.5f);

            // Fade in/out over lifetime
            var colorOverLife = _ps.colorOverLifetime;
            colorOverLife.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f,   0f),
                    new GradientAlphaKey(1f,   0.15f),
                    new GradientAlphaKey(1f,   0.85f),
                    new GradientAlphaKey(0f,   1f)
                });
            colorOverLife.color = new ParticleSystem.MinMaxGradient(gradient);

            _ps.Play();
        }

        private void BuildFragmentTexture()
        {
            // Generate a tiny 16×16 cross/circle glyph as the fragment sprite
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear  = Color.clear;
            Color bright = Color.white;

            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                int dx = x - 8, dy = y - 8;
                // Circle + cross glyph
                bool circle = dx * dx + dy * dy <= 20;
                bool cross  = (Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 5) ||
                              (Mathf.Abs(dy) <= 1 && Mathf.Abs(dx) <= 5);
                tex.SetPixel(x, y, (circle || cross) ? bright : clear);
            }
            tex.Apply();

            // Assign as sprite
            if (_psr != null)
            {
                var mat = new Material(Shader.Find("Particles/Standard Unlit"));
                if (mat != null)
                {
                    mat.mainTexture = tex;
                    _psr.material   = mat;
                    _psr.renderMode = ParticleSystemRenderMode.Billboard;
                }
            }
        }
    }
}
