using UnityEngine;
using System.Collections;
using AscendantContinuum.Core;

namespace AscendantContinuum.Realms.Verdant
{
    /// <summary>
    /// Represents a magical plant in Verdant Sanctuary with growth stages and watering mechanics.
    /// Plants grow through 4 stages: Seed → Sprout → Plant → Bloom
    /// </summary>
    public class MagicalPlant : MonoBehaviour
    {
        [Header("Growth Configuration")]
        [SerializeField] private float growthTimePerStage = 30f; // 30 seconds per stage
        [SerializeField] private bool requiresWater = true;
        [SerializeField] private float swaySpeed = 1.6f;
        [SerializeField] private float swayAmount = 0.12f;
        [SerializeField] private float idleMovementMultiplier = 0.75f;

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite seedSprite;
        [SerializeField] private Sprite sproutSprite;
        [SerializeField] private Sprite plantSprite;
        [SerializeField] private Sprite bloomSprite;
        [SerializeField] private ParticleSystem bloomParticles;

        [Header("VFX")]
        [SerializeField] private GameObject waterVFXPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip waterSound;
        [SerializeField] private AudioClip growthSound;
        [SerializeField] private AudioClip bloomSound;

        // Growth State
        public enum GrowthStage { Seed, Sprout, Plant, Bloom, Fading }
        private GrowthStage currentStage = GrowthStage.Seed;
        private float stageTimer = 0f;
        private bool isWatered = false;
        private bool isGrowing = false;
        private float verticalPhase;

        // Sway Animation
        private Vector3 originalPosition;
        private float swayOffset;

        // Accessibility
        private bool reducedMotion = false;

        // Scale
        private float baseScale = 1f;

        // Events
        public System.Action<MagicalPlant> OnBloom;
        public System.Action<MagicalPlant> OnFaded;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            swayOffset = Random.Range(0f, Mathf.PI * 2f);
            verticalPhase = Random.Range(0f, Mathf.PI * 2f);
            // Note: originalPosition is saved in Initialize() after positioning
        }

        private void Start()
        {
            // Check accessibility settings
            if (AccessibilityManager.Instance != null)
            {
                reducedMotion = AccessibilityManager.Instance.IsReducedMotionEnabled();
            }

            CalculateBaseScale();
            SetStageVisuals(GrowthStage.Seed);
        }

        private void Update()
        {
            if (!reducedMotion)
            {
                ApplySwayAnimation();
            }
        }

        #region Growth Management

        /// <summary>
        /// Initializes the plant with custom growth time
        /// </summary>
        public void Initialize(float customGrowthTime)
        {
            growthTimePerStage = customGrowthTime;
            originalPosition = transform.localPosition; // Save position AFTER it's been set
        }

        /// <summary>
        /// Waters the plant, enabling growth if required
        /// </summary>
        public void Water()
        {
            if (!requiresWater || isWatered || currentStage == GrowthStage.Bloom || currentStage == GrowthStage.Fading)
            {
                Debug.Log($"[MagicalPlant] Water skipped - requiresWater: {requiresWater}, isWatered: {isWatered}, stage: {currentStage}");
                return;
            }

            isWatered = true;
            Debug.Log($"[MagicalPlant] Plant watered! Starting growth...");

            // Play water effect
            if (AudioManager.Instance != null && waterSound != null)
                AudioManager.Instance.PlaySFX(waterSound, transform.position);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Light);

            // Spawn water particle VFX
            if (waterVFXPrefab != null)
            {
                GameObject vfx = Instantiate(waterVFXPrefab, transform.position, Quaternion.identity);
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
                Destroy(vfx, 2f);
            }
            else
            {
                SpawnFallbackWaterBurst();
            }

            // Start growing
            if (!isGrowing)
                StartCoroutine(GrowthCoroutine());
        }

        /// <summary>
        /// Starts the growth process
        /// </summary>
        public void StartGrowth()
        {
            if (isGrowing) return;

            if (requiresWater && !isWatered)
            {
                // Wait for watering
                return;
            }

            StartCoroutine(GrowthCoroutine());
        }

        private IEnumerator GrowthCoroutine()
        {
            isGrowing = true;
            stageTimer = 0f;

            while (currentStage != GrowthStage.Bloom)
            {
                // Apply Venus blessing growth multiplier (set by PantheonPlanetManager)
                float venusMultiplier = PlayerPrefs.GetFloat("Venus_GrowthMultiplier", 1f);
                string expiryStr = PlayerPrefs.GetString("Venus_GrowthExpiry", "");
                if (!string.IsNullOrEmpty(expiryStr) &&
                    float.TryParse(expiryStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out float expiryUnix))
                {
                    double nowUnix = (System.DateTime.UtcNow -
                        new System.DateTime(1970, 1, 1)).TotalSeconds;
                    if (nowUnix > expiryUnix)
                        venusMultiplier = 1f;   // blessing has expired
                }

                float stageDuration = venusMultiplier > 1f
                    ? growthTimePerStage / venusMultiplier   // faster growth
                    : growthTimePerStage;

                // Track elapsed time for this stage (used by UI progress bars)
                stageTimer = 0f;
                float elapsed = 0f;
                while (elapsed < stageDuration)
                {
                    elapsed += Time.deltaTime;
                    stageTimer = elapsed / stageDuration; // 0–1 progress
                    yield return null;
                }

                // Advance to next stage
                AdvanceStage();
            }

            // After blooming, wait then fade
            yield return new WaitForSeconds(growthTimePerStage * 2f);
            StartFade();
        }

        private void AdvanceStage()
        {
            switch (currentStage)
            {
                case GrowthStage.Seed:
                    currentStage = GrowthStage.Sprout;
                    break;
                case GrowthStage.Sprout:
                    currentStage = GrowthStage.Plant;
                    break;
                case GrowthStage.Plant:
                    currentStage = GrowthStage.Bloom;
                    OnBloomReached();
                    break;
            }

            SetStageVisuals(currentStage);

            // Play growth sound
            if (AudioManager.Instance != null && growthSound != null)
                AudioManager.Instance.PlaySFX(growthSound, transform.position);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Medium);
        }

        private void OnBloomReached()
        {
            // Trigger bloom event
            OnBloom?.Invoke(this);

            // Play bloom sound
            if (AudioManager.Instance != null && bloomSound != null)
                AudioManager.Instance.PlaySFX(bloomSound, transform.position);

            // Spawn bloom particles
            if (bloomParticles != null)
            {
                if (reducedMotion)
                {
                    // Simple flash instead of particles
                    StartCoroutine(BloomFlashEffect());
                }
                else
                {
                    bloomParticles.Play();
                }
            }

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Success);
        }

        private void StartFade()
        {
            currentStage = GrowthStage.Fading;
            StartCoroutine(FadeOutCoroutine());
        }

        private IEnumerator FadeOutCoroutine()
        {
            float fadeDuration = 2f;
            float elapsed = 0f;
            Color startColor = spriteRenderer.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            OnFaded?.Invoke(this);
            gameObject.SetActive(false);
        }

        #endregion

        #region Visuals

        /// <summary>
        /// Computes baseScale so the Bloom stage plant occupies ~16% of the camera height.
        /// Called once in Start() before SetStageVisuals.
        /// </summary>
        private void CalculateBaseScale()
        {
            Sprite refSprite = seedSprite ?? sproutSprite ?? plantSprite ?? bloomSprite;
            if (refSprite == null) { baseScale = 0.1f; return; }
            float naturalH = refSprite.bounds.size.y;
            if (naturalH < 0.001f) { baseScale = 0.1f; return; }
            Camera cam = Camera.main;
            float camH = (cam != null) ? cam.orthographicSize * 2f : 12f;
            // Target: Bloom stage = ~16% of camera view height
            baseScale = (camH * 0.16f) / naturalH;
        }

        private void SetStageVisuals(GrowthStage stage)
        {
            switch (stage)
            {
                case GrowthStage.Seed:
                    spriteRenderer.sprite = seedSprite;
                    transform.localScale = Vector3.one * (baseScale * 0.65f);
                    break;
                case GrowthStage.Sprout:
                    spriteRenderer.sprite = sproutSprite;
                    transform.localScale = Vector3.one * (baseScale * 0.75f);
                    break;
                case GrowthStage.Plant:
                    spriteRenderer.sprite = plantSprite;
                    transform.localScale = Vector3.one * (baseScale * 0.88f);
                    break;
                case GrowthStage.Bloom:
                    spriteRenderer.sprite = bloomSprite;
                    transform.localScale = Vector3.one * (baseScale * 1.0f);
                    break;
            }
        }

        private void ApplySwayAnimation()
        {
            float growthFactor = isGrowing ? 1f : idleMovementMultiplier;
            float stageFactor = currentStage == GrowthStage.Seed ? 0.75f : 1f;
            float amp = swayAmount * growthFactor * stageFactor;

            float swayX = Mathf.Sin((Time.time * swaySpeed) + swayOffset) * amp;
            float swayY = Mathf.Cos((Time.time * (swaySpeed * 0.75f)) + verticalPhase) * (amp * 0.45f);
            transform.localPosition = originalPosition + new Vector3(swayX, swayY, 0f);
        }

        private void SpawnFallbackWaterBurst()
        {
            GameObject vfx = new GameObject("WaterBurstFallback");
            vfx.transform.position = transform.position;

            var ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.35f;
            main.startLifetime = 0.35f;
            main.startSpeed = 1.8f;
            main.startSize = 0.08f;
            main.startColor = new Color(0.55f, 0.95f, 1f, 1f);
            main.maxParticles = 20;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.08f;

            ps.Play();
            Destroy(vfx, 1f);
        }

        private IEnumerator BloomFlashEffect()
        {
            // Simple flash for reduced motion
            Color original = spriteRenderer.color;

            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = original;
                yield return new WaitForSeconds(0.1f);
            }
        }

        #endregion

        #region Public Properties

        public GrowthStage CurrentStage => currentStage;
        public bool IsWatered => isWatered;
        public bool IsGrowing => isGrowing;
        public bool HasBloomed => currentStage == GrowthStage.Bloom;

        #endregion

        #region Touch Interaction

        private void OnMouseDown()
        {
            Debug.Log($"[MagicalPlant] Plant clicked! Stage: {currentStage}, Watered: {isWatered}");

            if (currentStage == GrowthStage.Bloom)
            {
                // Clicking a bloomed plant creates positive feedback
                if (AudioManager.Instance != null && bloomSound != null)
                    AudioManager.Instance.PlaySFX(bloomSound, transform.position);

                if (AccessibilityManager.Instance != null)
                    AccessibilityManager.Instance.TriggerHaptic(HapticType.Light);
            }
            else
            {
                // Water the plant if it needs it
                Water();
            }
        }

        #endregion
    }
}
