using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Astronomy;
using AscendantContinuum.Systems;
using AscendantContinuum.Social;
using AscendantContinuum.UI;

namespace AscendantContinuum.Emberforge
{
    /// <summary>
    /// Manages the Emberforge realm's core mechanic: collecting dancing sparks
    /// Implements object pooling for performance and haptic feedback
    /// </summary>
    public class EmberforgeSparks : MonoBehaviour
    {
        [Header("Spark Settings")]
        [SerializeField] private GameObject sparkPrefab;
        [SerializeField] private int poolSize = 50;
        [SerializeField] private float sparkLifetime = 3f;
        [SerializeField] private float spawnRadius = 5f;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 0.5f;
        [SerializeField] private int maxActiveSparks = 20;

        [Header("Audio")]
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private AudioClip spawnSound;

        [Header("Completion")]
        [SerializeField] private AscendantContinuum.UI.RealmCompletionPanel completionPanel;
        [SerializeField] private int goalSparks = 20;

        private Queue<GameObject> sparkPool;
        private List<GameObject> activeSparks;
        private float spawnTimer;
        private int sparksCollected = 0;
        private bool _completionShown = false;

        public event Action<int> OnSparkCollected;

        private void Start()
        {
            InitializePool();
            activeSparks = new List<GameObject>();
            ConsumePendingSparks();
        }

        /// <summary>
        /// Consume any sparks stored from offline events (e.g. meteor shower bonus)
        /// that fired while Emberforge was not loaded.
        /// </summary>
        private void ConsumePendingSparks()
        {
            int pending = PlayerPrefs.GetInt("MeteorShower_PendingSparks", 0);
            if (pending <= 0) return;

            for (int i = 0; i < pending; i++)
            {
                InjectSpark();
            }

            PlayerPrefs.DeleteKey("MeteorShower_PendingSparks");
            PlayerPrefs.Save();

            Debug.Log($"[EmberforgeSparks] Consumed {pending} pending meteor shower sparks on load.");
        }

        private void InitializePool()
        {
            sparkPool = new Queue<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                GameObject spark = Instantiate(sparkPrefab, transform);
                spark.SetActive(false);
                sparkPool.Enqueue(spark);
            }

            Debug.Log($"[EmberforgeSparks] Pool initialized with {poolSize} sparks");
        }

        private void Update()
        {
            // Auto-spawn sparks — ADHD mode increases spawn rate for higher engagement
            float adhdMultiplier = PlayerPrefs.GetFloat("ADHD_SpawnMultiplier", 1f);
            float effectiveInterval = spawnInterval / Mathf.Max(0.1f, adhdMultiplier);

            spawnTimer += Time.deltaTime;

            if (spawnTimer >= effectiveInterval && activeSparks.Count < maxActiveSparks)
            {
                SpawnSpark();
                spawnTimer = 0f;
            }
        }

        private void SpawnSpark()
        {
            if (sparkPool.Count == 0) return;

            GameObject spark = sparkPool.Dequeue();

            // Random position in circular area
            Vector2 randomPos = UnityEngine.Random.insideUnitCircle * spawnRadius;
            spark.transform.position = new Vector3(randomPos.x, randomPos.y, 0f);

            spark.SetActive(true);
            activeSparks.Add(spark);

            // Play spawn sound via AudioManager (respects volume/accessibility settings)
            if (spawnSound != null)
                Core.AudioManager.Instance?.PlaySFX(spawnSound, spark.transform.position, 0.3f);

            // Auto-return to pool after lifetime
            StartCoroutine(ReturnToPoolAfterDelay(spark, sparkLifetime));
        }

        public void CollectSpark(GameObject spark)
        {
            if (!activeSparks.Contains(spark)) return;

            // Apply Mars blessing multiplier (default 1x, set to 2x by PantheonPlanetManager)
            float multiplier = PlayerPrefs.GetFloat("Mars_SparkMultiplier", 1f);

            // Apply Summer Solstice solar power boost (Emberforge = fire = solar)
            float solarBoost = PlayerPrefs.GetFloat("Solstice_LightMultiplier", 1f);
            multiplier *= solarBoost;

            // Apply Moon Phase spark multiplier (Full Moon = 2x, New Moon = 0.5x)
            float moonMultiplier = MoonPhaseEffects.Instance?.GetSparkMultiplier() ?? 1f;
            multiplier *= moonMultiplier;

            // Apply active Serendipity bonus (Legendary/Rare events grant temporary boosts)
            float serendipityBonus = Systems.SerendipityManager.Instance?.GetActiveSparkBonus() ?? 1f;
            multiplier *= serendipityBonus;

            int sparksToAdd = Mathf.Max(1, Mathf.RoundToInt(multiplier));
            sparksCollected += sparksToAdd;

            // Update HUD live
            HUDManager.Instance?.UpdateSparksCount(sparksCollected);

            // Haptic feedback
            AccessibilityManager.Instance?.TriggerHaptic(Core.HapticType.Light);

            // Play collect sound via AudioManager
            if (collectSound != null)
                Core.AudioManager.Instance?.PlaySFX(collectSound, spark.transform.position, 0.5f);

            // Return to pool
            ReturnToPool(spark);

            OnSparkCollected?.Invoke(sparksCollected);

            // Realm completion check
            if (!_completionShown && sparksCollected >= goalSparks && completionPanel != null)
            {
                _completionShown = true;
                int stars = sparksCollected >= goalSparks * 2 ? 3 : sparksCollected >= Mathf.RoundToInt(goalSparks * 1.4f) ? 2 : 1;
                completionPanel.ShowCompletion("Emberforge Complete! ✨",
                    $"{sparksCollected} Sparks Collected", stars);
                Core.GameEvents.RaiseRealmCompleted("emberforge", sparksCollected);
            }

            // After every 10 sparks, offer the optional kindness blessing prompt
            if (sparksCollected % 10 == 0)
                Social.KindnessChainManager.Instance?.ShowSendBlessingPrompt("Emberforge");

            if (multiplier > 1f)
                Debug.Log($"[EmberforgeSparks] Spark collected x{sparksToAdd} (Mars blessing active)! Total: {sparksCollected}");
            else
                Debug.Log($"[EmberforgeSparks] Spark collected! Total: {sparksCollected}");
        }

        private void ReturnToPool(GameObject spark)
        {
            activeSparks.Remove(spark);
            spark.SetActive(false);
            sparkPool.Enqueue(spark);
        }

        /// <summary>
        /// Directly inject a spark reward (e.g. from meteor shower bonus) without
        /// requiring a pooled spark object.
        /// </summary>
        public void InjectSpark()
        {
            sparksCollected++;
            OnSparkCollected?.Invoke(sparksCollected);
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject spark, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (spark.activeSelf) // Only return if still active (not collected)
            {
                ReturnToPool(spark);
            }
        }

        public int SparksCollected => sparksCollected;
        public int ActiveSparksCount => activeSparks.Count;
    }
}
