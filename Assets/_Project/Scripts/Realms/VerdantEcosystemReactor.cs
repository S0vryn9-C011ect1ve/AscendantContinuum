using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.VFX;

namespace AscendantContinuum.Realms
{
    /// <summary>
    /// Reacts to sigil completion events while inside the Verdant Sanctuary.
    /// Triggers bioluminescent flora bloom: soft green/teal particle burst +
    /// plant root LineRenderer glow pulse.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VerdantEcosystemReactor : MonoBehaviour
    {
        [Header("Bloom Visual")]
        [SerializeField] private int   bloomParticleCount = 28;
        [SerializeField] private float bloomSpread        = 2.5f;
        [SerializeField] private Color bloomColorA        = new Color(0.2f, 1f, 0.6f, 0.9f);
        [SerializeField] private Color bloomColorB        = new Color(0.1f, 0.8f, 1f, 0.6f);

        [Header("Root Glow")]
        [SerializeField] private LineRenderer[] plantRoots;
        [SerializeField] private float          glowDuration = 2.8f;

        private float   _glowTimer;
        private bool    _glowing;
        private Color[] _originalRootColors;

        private void Awake()
        {
            if (plantRoots != null && plantRoots.Length > 0)
            {
                _originalRootColors = new Color[plantRoots.Length];
                for (int i = 0; i < plantRoots.Length; i++)
                    _originalRootColors[i] = plantRoots[i] != null ? plantRoots[i].startColor : Color.white;
            }
        }

        private void OnEnable()  => GameEvents.OnSigilCompleted += HandleSigilCompleted;
        private void OnDisable() => GameEvents.OnSigilCompleted -= HandleSigilCompleted;

        private void Update()
        {
            if (!_glowing) return;
            _glowTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_glowTimer / glowDuration);
            float intensity = Mathf.Sin(t * Mathf.PI); // bell curve

            if (plantRoots != null)
            {
                for (int i = 0; i < plantRoots.Length; i++)
                {
                    if (plantRoots[i] == null) continue;
                    Color glow = Color.Lerp(_originalRootColors[i], bloomColorA * 2f, intensity);
                    plantRoots[i].startColor = glow;
                    plantRoots[i].endColor   = Color.Lerp(_originalRootColors[i], bloomColorB, intensity);
                }
            }

            if (t >= 1f)
            {
                _glowing = false;
                ResetRootColors();
            }
        }

        private void HandleSigilCompleted(Data.SigilData sigil)
        {
            if (AccessibilityManager.Instance?.ReducedMotionEnabled == true) return;

            // Particle burst across the floor
            if (ParticleManager.Instance != null)
            {
                for (int i = 0; i < bloomParticleCount; i++)
                {
                    Vector3 offset = new Vector3(
                        Random.Range(-bloomSpread, bloomSpread),
                        Random.Range(0f, 1.5f),
                        Random.Range(-bloomSpread, bloomSpread));
                    Color col = Color.Lerp(bloomColorA, bloomColorB, Random.value);
                    ParticleManager.Instance.PlayEcosystemBloom(
                        transform.position + offset, col, "verdant");
                }
            }

            // Root glow
            _glowTimer = 0f;
            _glowing   = true;

            // Audio: rising arpeggio from ProceduralToneGenerator
            AudioManager.Instance?.PlayRealmEcosystemReaction("verdant", sigil.primaryColor);

            Debug.Log("[VerdantReactor] Bioluminescent bloom triggered.");
        }

        private void ResetRootColors()
        {
            if (plantRoots == null) return;
            for (int i = 0; i < plantRoots.Length; i++)
            {
                if (plantRoots[i] == null) continue;
                plantRoots[i].startColor = _originalRootColors[i];
                plantRoots[i].endColor   = _originalRootColors[i];
            }
        }
    }
}
