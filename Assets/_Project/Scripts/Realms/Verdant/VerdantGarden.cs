using UnityEngine;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Astronomy;
using AscendantContinuum.Systems;
using AscendantContinuum.Social;
using AscendantContinuum.Realms.Verdant;

namespace AscendantContinuum.Verdant
{
    /// <summary>
    /// Verdant Sanctuary ritual - Nurture magical plants by tapping to water them
    /// Plants grow over time and bloom with beautiful flowers
    /// </summary>
    public class VerdantGarden : MonoBehaviour
    {
        [Header("Plant Settings")]
        [SerializeField] private GameObject plantPrefab;
        [SerializeField] private int maxPlants = 10;
        [SerializeField] private float growthTimePerStage = 30f; // seconds
        [SerializeField] [Range(0.05f, 1f)] private float plantDisplayScale = 0.32f;

        [Header("Spawn Settings")]
        [SerializeField] private Vector2 gardenSize = new Vector2(8f, 6f);
        [SerializeField] private float minSpawnDistance = 3.1f;
        [SerializeField] private int spawnPositionAttempts = 24;

        [Header("Audio")]
        [SerializeField] private AudioClip waterSound;
        [SerializeField] private AudioClip bloomSound;

        [Header("Completion")]
        [SerializeField] private AscendantContinuum.UI.RealmCompletionPanel completionPanel;
        [SerializeField] private int goalPlants = 5;

        private List<AscendantContinuum.Realms.Verdant.MagicalPlant> activePlants = new List<AscendantContinuum.Realms.Verdant.MagicalPlant>();
        private int totalPlantsGrown = 0;
        private bool _completionShown = false;

        public System.Action<int> OnPlantBloomed;

        private void Start()
        {
            SpawnInitialPlants();
        }

        private void SpawnInitialPlants()
        {
            for (int i = 0; i < 3; i++) // Start with 3 seedlings
            {
                SpawnPlant();
            }
        }

        public void SpawnPlant()
        {
            if (activePlants.Count >= maxPlants) return;

            Vector3 position = GetSpreadOutSpawnPosition();

            GameObject plantObj = Instantiate(plantPrefab, transform);
            plantObj.transform.localPosition = position; // Set local position explicitly
            plantObj.transform.localScale = Vector3.one * plantDisplayScale;
            AscendantContinuum.Realms.Verdant.MagicalPlant plant = plantObj.GetComponent<AscendantContinuum.Realms.Verdant.MagicalPlant>();

            if (plant != null)
            {
                // Moon phase affects growth rate: New Moon speeds growth, Full Moon slows it
                float moonGrowthMult = MoonPhaseEffects.Instance?.GetPlantGrowthMultiplier() ?? 1f;
                float effectiveGrowthTime = moonGrowthMult > 0f
                    ? growthTimePerStage / moonGrowthMult
                    : growthTimePerStage;

                plant.Initialize(effectiveGrowthTime);
                plant.OnBloom += HandlePlantBloomed;
                activePlants.Add(plant);
            }
        }

        private Vector3 GetSpreadOutSpawnPosition()
        {
            Vector3 fallback = Vector3.zero;

            for (int attempt = 0; attempt < spawnPositionAttempts; attempt++)
            {
                Vector3 candidate = new Vector3(
                    Random.Range(-gardenSize.x, gardenSize.x),
                    Random.Range(-gardenSize.y, gardenSize.y),
                    0f
                );

                fallback = candidate;
                bool tooClose = false;

                for (int i = 0; i < activePlants.Count; i++)
                {
                    var existing = activePlants[i];
                    if (existing == null || !existing.gameObject.activeInHierarchy) continue;

                    if (Vector3.Distance(existing.transform.localPosition, candidate) < minSpawnDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                    return candidate;
            }

            return fallback;
        }

        public void WaterPlant(AscendantContinuum.Realms.Verdant.MagicalPlant plant)
        {
            if (!activePlants.Contains(plant)) return;

            plant.Water();

            // Play water sound
            if (waterSound != null)
            {
                Core.AudioManager.Instance?.PlaySFX(waterSound, plant.transform.position, 1f, true);
            }

            // Haptic feedback
            Core.AccessibilityManager.Instance?.TriggerHaptic(Core.HapticType.Light);
        }

        private void HandlePlantBloomed(AscendantContinuum.Realms.Verdant.MagicalPlant plant)
        {
            totalPlantsGrown++;

            // Venus_GrowthMultiplier also amplifies bloom rewards (faster growth = bigger harvest)
            float venusMultiplier = PlayerPrefs.GetFloat("Venus_GrowthMultiplier", 1f);
            string expiryStr = PlayerPrefs.GetString("Venus_GrowthExpiry", "");
            if (!string.IsNullOrEmpty(expiryStr) &&
                double.TryParse(expiryStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double expiryUnix))
            {
                double nowUnix = (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
                if (nowUnix > expiryUnix)
                    venusMultiplier = 1f; // blessing expired
            }
            int bloomReward = Mathf.Max(1, Mathf.RoundToInt(venusMultiplier));
            if (bloomReward > 1)
                Debug.Log($"[VerdantGarden] Venus bloom bonus! x{bloomReward} reward for plant #{totalPlantsGrown}");

            // Play bloom sound
            if (bloomSound != null)
            {
                Core.AudioManager.Instance?.PlaySFX(bloomSound, plant.transform.position, 1f, true);
            }

            // Haptic feedback
            Core.AccessibilityManager.Instance?.TriggerHaptic(Core.HapticType.Success);

            // Visual effect
            VFX.ParticleManager.Instance?.PlayRealmTransitionEffect(plant.transform.position, new Color(0.3f, 1f, 0.5f));

            OnPlantBloomed?.Invoke(totalPlantsGrown);

            // Realm completion check
            if (!_completionShown && totalPlantsGrown >= goalPlants && completionPanel != null)
            {
                _completionShown = true;
                int stars = totalPlantsGrown >= goalPlants * 2 ? 3 : totalPlantsGrown >= Mathf.RoundToInt(goalPlants * 1.4f) ? 2 : 1;
                completionPanel.ShowCompletion("Verdant Garden Complete! 🌿",
                    $"{totalPlantsGrown} Plants Bloomed", stars);
                Core.GameEvents.RaiseRealmCompleted("verdant", totalPlantsGrown);
            }

            // Serendipity roll — each bloom is a ritual completion
            SerendipityManager.Instance?.TryTrigger("Verdant");

            // Offer optional kindness blessing on bloom
            Social.KindnessChainManager.Instance?.ShowSendBlessingPrompt("Verdant");

            // Spawn new plant after bloom
            Invoke(nameof(SpawnPlant), 5f);
        }

        public int TotalPlantsGrown => totalPlantsGrown;
    }

    /// <summary>
    /// Individual magical plant behavior
    /// </summary>
    public class MagicalPlant : MonoBehaviour
    {
        [Header("Growth Stages")]
        [SerializeField] private Sprite seedSprite;
        [SerializeField] private Sprite sproutSprite;
        [SerializeField] private Sprite plantSprite;
        [SerializeField] private Sprite bloomSprite;

        private SpriteRenderer spriteRenderer;
        private int currentStage = 0;
        private float growthProgress = 0f;
        private float growthTimePerStage = 30f;
        private bool isWatered = false;
        private float lastWaterTime = 0f;
        private float baseScale = 1f;

        public System.Action<MagicalPlant> OnBloomed;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            // Camera-relative base scale so Bloom = ~16% of camera height
            Sprite refSprite = seedSprite ?? sproutSprite ?? plantSprite ?? bloomSprite;
            if (refSprite != null)
            {
                float naturalH = refSprite.bounds.size.y;
                Camera cam = Camera.main;
                float camH = (cam != null) ? cam.orthographicSize * 2f : 12f;
                if (naturalH > 0.001f)
                    baseScale = (camH * 0.16f) / naturalH;
            }
        }

        public void Initialize(float growthTime)
        {
            growthTimePerStage = growthTime;
            currentStage = 0;
            UpdateVisuals();
        }

        private void Update()
        {
            // Plants only grow if watered recently (within last 60 seconds)
            if (Time.time - lastWaterTime < 60f)
            {
                growthProgress += Time.deltaTime;

                if (growthProgress >= growthTimePerStage)
                {
                    Grow();
                }
            }

            // Gentle sway animation (respects reduced motion)
            if (Core.AccessibilityManager.Instance?.ReducedMotionEnabled == false)
            {
                float sway = Mathf.Sin(Time.time * 0.5f) * 0.1f;
                transform.rotation = Quaternion.Euler(0f, 0f, sway * 10f);
            }
        }

        public void Water()
        {
            lastWaterTime = Time.time;
            isWatered = true;

            // Visual feedback
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, 0.3f);
        }

        private void Grow()
        {
            currentStage++;
            growthProgress = 0f;

            UpdateVisuals();

            if (currentStage >= 4) // Fully bloomed
            {
                OnBloomed?.Invoke(this);

                // After blooming, stay for 10 seconds then fade out
                Invoke(nameof(FadeOut), 10f);
            }
        }

        private void UpdateVisuals()
        {
            switch (currentStage)
            {
                case 0:
                    spriteRenderer.sprite = seedSprite;
                    transform.localScale = Vector3.one * (baseScale * 0.65f);
                    break;
                case 1:
                    spriteRenderer.sprite = sproutSprite;
                    transform.localScale = Vector3.one * (baseScale * 0.75f);
                    break;
                case 2:
                    spriteRenderer.sprite = plantSprite;
                    transform.localScale = Vector3.one * (baseScale * 0.88f);
                    break;
                case 3:
                    spriteRenderer.sprite = bloomSprite;
                    transform.localScale = Vector3.one * (baseScale * 1.0f);
                    break;
            }
        }

        private void FadeOut()
        {
            StartCoroutine(FadeOutCoroutine());
        }

        private System.Collections.IEnumerator FadeOutCoroutine()
        {
            Color startColor = spriteRenderer.color;

            for (float t = 0; t < 2f; t += Time.deltaTime)
            {
                spriteRenderer.color = Color.Lerp(startColor, Color.clear, t / 2f);
                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnMouseDown()
        {
            Water();
        }
    }
}
