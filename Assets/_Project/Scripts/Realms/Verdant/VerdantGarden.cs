using UnityEngine;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Astronomy;
using AscendantContinuum.Systems;
using AscendantContinuum.Social;

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

        [Header("Spawn Settings")]
        [SerializeField] private Vector2 gardenSize = new Vector2(8f, 6f);

        [Header("Audio")]
        [SerializeField] private AudioClip waterSound;
        [SerializeField] private AudioClip bloomSound;

        [Header("Completion")]
        [SerializeField] private AscendantContinuum.UI.RealmCompletionPanel completionPanel;
        [SerializeField] private int goalPlants = 5;

        private List<MagicalPlant> activePlants = new List<MagicalPlant>();
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

            // Random position in garden
            Vector3 position = new Vector3(
                Random.Range(-gardenSize.x / 2f, gardenSize.x / 2f),
                Random.Range(-gardenSize.y / 2f, gardenSize.y / 2f),
                0f
            );

            GameObject plantObj = Instantiate(plantPrefab, position, Quaternion.identity, transform);
            MagicalPlant plant = plantObj.GetComponent<MagicalPlant>();

            if (plant != null)
            {
                // Moon phase affects growth rate: New Moon speeds growth, Full Moon slows it
                float moonGrowthMult = MoonPhaseEffects.Instance?.GetPlantGrowthMultiplier() ?? 1f;
                float effectiveGrowthTime = moonGrowthMult > 0f
                    ? growthTimePerStage / moonGrowthMult
                    : growthTimePerStage;

                plant.Initialize(effectiveGrowthTime);
                plant.OnBloomed += HandlePlantBloomed;
                activePlants.Add(plant);
            }
        }

        public void WaterPlant(MagicalPlant plant)
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

        private void HandlePlantBloomed(MagicalPlant plant)
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

        public System.Action<MagicalPlant> OnBloomed;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
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
                    transform.localScale = Vector3.one * 0.5f;
                    break;
                case 1:
                    spriteRenderer.sprite = sproutSprite;
                    transform.localScale = Vector3.one * 0.7f;
                    break;
                case 2:
                    spriteRenderer.sprite = plantSprite;
                    transform.localScale = Vector3.one * 1f;
                    break;
                case 3:
                    spriteRenderer.sprite = bloomSprite;
                    transform.localScale = Vector3.one * 1.2f;
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
