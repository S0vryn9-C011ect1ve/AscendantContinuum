using UnityEngine;
using AscendantContinuum.Systems;
using AscendantContinuum.Core;

namespace AscendantContinuum.VFX
{
    /// <summary>
    /// Generates a 2,000-star procedural starfield at runtime using a single
    /// <see cref="ParticleSystem"/>. Zero texture atlas required.
    ///
    /// Features:
    ///   • Realm-tinted via <see cref="Realms.RealmAtmosphereController"/>
    ///   • Star density scales with <see cref="ContinuumFieldManager.GetCollectiveEnergy()"/>
    ///   • Gentle individual twinkle via <c>sizeOverLifetime</c> sine curve
    ///   • Night-time sessions get +400 additional stars (SkyTimeSystem.Night)
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class ProceduralStarfieldSystem : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Star Count")]
        [SerializeField] private int   baseStarCount     = 2000;
        [SerializeField] private int   collectiveBonus   = 400;

        [Header("Star Size")]
        [SerializeField] private float starSizeMin  = 0.012f;
        [SerializeField] private float starSizeMax  = 0.075f;

        [Header("Tint")]
        [SerializeField] private Color nightTint   = new Color(0.85f, 0.9f, 1f, 1f);
        [SerializeField] private Color realmTint   = Color.white;

        // ── Runtime ────────────────────────────────────────────────────────────
        private ParticleSystem           _ps;
        private ParticleSystem.Particle[] _particles;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
            ConfigureSystem();
        }

        private void Start()
        {
            Rebuild();
            GameEvents.OnCollectiveEnergyChanged += _ => Rebuild();
        }

        private void OnDestroy()
        {
            GameEvents.OnCollectiveEnergyChanged -= _ => Rebuild();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Set the realm tint and rebuild the star colors.</summary>
        public void SetRealmTint(Color tint)
        {
            realmTint = tint;
            Rebuild();
        }

        // ── Private ────────────────────────────────────────────────────────────

        private void Rebuild()
        {
            float energy  = ContinuumFieldManager.Instance?.GetCollectiveEnergy() ?? 0f;
            bool  isNight = SkyTimeSystem.Instance?.CurrentTimePeriod == SkyTimeSystem.TimePeriod.Night;

            int count = baseStarCount + (isNight ? collectiveBonus : 0)
                      + Mathf.RoundToInt(energy * collectiveBonus);

            count = Mathf.Clamp(count, baseStarCount, baseStarCount + collectiveBonus * 2);

            _particles = new ParticleSystem.Particle[count];
            Color baseColor = Color.Lerp(nightTint, realmTint, 0.35f);

            for (int i = 0; i < count; i++)
            {
                // Scatter across a 40×25 world-unit area
                _particles[i].position = new Vector3(
                    Random.Range(-20f, 20f),
                    Random.Range(-12f, 12f),
                    Random.Range(5f, 30f));   // z-depth pushes them behind everything

                float alpha = Random.Range(0.15f, 0.85f);
                _particles[i].startColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                float size = Random.Range(starSizeMin, starSizeMax);
                _particles[i].startSize      = size;
                _particles[i].remainingLifetime = 999f;
                _particles[i].velocity        = Vector3.zero;
            }

            _ps.SetParticles(_particles, count);
        }

        private void ConfigureSystem()
        {
            var main = _ps.main;
            main.loop             = false;
            main.playOnAwake      = false;
            main.maxParticles     = baseStarCount + collectiveBonus * 2 + 200;
            main.simulationSpace  = ParticleSystemSimulationSpace.World;
            main.startLifetime    = 999f;
            main.startSpeed       = 0f;
            main.gravityModifier  = 0f;

            var emission  = _ps.emission;
            emission.enabled = false; // manual SetParticles only

            // Twinkle via size-over-lifetime sine animation
            var sizeOverLife = _ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            var twinkle = new AnimationCurve();
            for (int k = 0; k <= 10; k++)
            {
                float t   = k / 10f;
                float val = 0.7f + 0.3f * Mathf.Sin(t * Mathf.PI * 2f * Random.Range(1f, 4f));
                twinkle.AddKey(t, val);
            }
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, twinkle);
        }
    }
}
