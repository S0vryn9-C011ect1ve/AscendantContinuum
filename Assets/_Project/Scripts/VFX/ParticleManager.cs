using UnityEngine;
using System.Collections;
using AscendantContinuum.Core;

namespace AscendantContinuum.VFX
{
    /// <summary>
    /// Particle effects manager with object pooling and reduced-motion support
    /// Creates magical visual effects for all realms
    /// </summary>
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        [Header("Particle Prefabs")]
        [SerializeField] private GameObject sparkParticlePrefab;
        [SerializeField] private GameObject collectEffectPrefab;
        [SerializeField] private GameObject realmTransitionPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 30;

        private System.Collections.Generic.Queue<ParticleSystem> particlePool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePool();
        }

        private void InitializePool()
        {
            particlePool = new System.Collections.Generic.Queue<ParticleSystem>();

            // Pre-instantiate particle systems
            for (int i = 0; i < poolSize; i++)
            {
                CreatePooledParticle();
            }

            Debug.Log($"[ParticleManager] Pool initialized with {poolSize} particle systems");
        }

        private void CreatePooledParticle()
        {
            GameObject particleObj = new GameObject("PooledParticle");
            particleObj.transform.SetParent(transform);
            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

            // Default settings
            var main = ps.main;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.Disable;

            particleObj.SetActive(false);
            particlePool.Enqueue(ps);
        }

        public void PlaySparkCollectEffect(Vector3 position)
        {
            // Check reduced motion setting
            if (Core.AccessibilityManager.Instance?.ReducedMotionEnabled == true)
            {
                // Simple flash instead of particles
                PlaySimpleFlash(position, new Color(1f, 0.8f, 0.2f));
                return;
            }

            ParticleSystem ps = GetParticleSystem();
            if (ps == null) return;

            ConfigureSparkCollectParticle(ps);
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();

            StartCoroutine(ReturnToPoolAfterPlay(ps));
        }

        public void PlayRealmTransitionEffect(Vector3 position, Color realmColor)
        {
            if (Core.AccessibilityManager.Instance?.ReducedMotionEnabled == true)
            {
                PlaySimpleFlash(position, realmColor, 1f);
                return;
            }

            ParticleSystem ps = GetParticleSystem();
            if (ps == null) return;

            ConfigureRealmTransitionParticle(ps, realmColor);
            ps.transform.position = position;
            ps.gameObject.SetActive(true);
            ps.Play();

            StartCoroutine(ReturnToPoolAfterPlay(ps));
        }

        private void ConfigureSparkCollectParticle(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startSize = 0.2f;
            main.startColor = new Color(1f, 0.6f, 0.2f);
            main.maxParticles = 20;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 15)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.yellow, 0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;
        }

        private void ConfigureRealmTransitionParticle(ParticleSystem ps, Color realmColor)
        {
            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 2f;
            main.startSize = 0.5f;
            main.startColor = realmColor;
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.rateOverTime = 30;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        private void PlaySimpleFlash(Vector3 position, Color color, float duration = 0.3f)
        {
            // Create temporary sprite for accessibility-friendly flash
            GameObject flashObj = new GameObject("AccessibleFlash");
            flashObj.transform.position = position;

            SpriteRenderer sr = flashObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = color;
            sr.sortingOrder = 100;

            StartCoroutine(FadeOutAndDestroy(sr, duration));
        }

        private Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];

            Vector2 center = new Vector2(32, 32);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * 64 + x] = dist < 32 ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        private IEnumerator FadeOutAndDestroy(SpriteRenderer sr, float duration)
        {
            Color startColor = sr.color;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                sr.color = Color.Lerp(startColor, Color.clear, t / duration);
                yield return null;
            }

            Destroy(sr.gameObject);
        }

        private ParticleSystem GetParticleSystem()
        {
            if (particlePool.Count == 0)
            {
                CreatePooledParticle();
            }

            return particlePool.Dequeue();
        }

        private IEnumerator ReturnToPoolAfterPlay(ParticleSystem ps)
        {
            yield return new WaitWhile(() => ps.isPlaying);

            ps.gameObject.SetActive(false);
            ps.Clear();
            particlePool.Enqueue(ps);
        }

        /// <summary>
        /// Called by AccessibilityManager when Reduced Motion setting changes.
        /// Scale = 1 means full effects; 0.25 = reduced motion; 0 = disabled.
        /// </summary>
        public void SetMotionScale(float scale)
        {
            // Apply to all currently active pooled and non-pooled particle systems in scene
            var allPS = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in allPS)
            {
                if (ps == null) continue;
                var main = ps.main;
                // Scale emission rate and simulation speed proportionally
                main.simulationSpeed = Mathf.Clamp(scale, 0.1f, 2f);
            }
            Debug.Log($"[ParticleManager] Motion scale set to {scale:F2} for {allPS.Length} particle systems");
        }
    }
}
