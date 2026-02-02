using UnityEngine;
using System;
using System.Collections.Generic;

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
        
        private Queue<GameObject> sparkPool;
        private List<GameObject> activeSparks;
        private float spawnTimer;
        private int sparksCollected = 0;
        
        public event Action<int> OnSparkCollected;

        private void Start()
        {
            InitializePool();
            activeSparks = new List<GameObject>();
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
            // Auto-spawn sparks
            spawnTimer += Time.deltaTime;
            
            if (spawnTimer >= spawnInterval && activeSparks.Count < maxActiveSparks)
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
            
            // Play spawn sound (respects accessibility settings)
            if (spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(spawnSound, Camera.main.transform.position, 0.3f);
            }
            
            // Auto-return to pool after lifetime
            StartCoroutine(ReturnToPoolAfterDelay(spark, sparkLifetime));
        }

        public void CollectSpark(GameObject spark)
        {
            if (!activeSparks.Contains(spark)) return;
            
            sparksCollected++;
            
            // Haptic feedback
            AccessibilityManager.Instance?.TriggerHaptic(Core.HapticType.Light);
            
            // Play collect sound
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position, 0.5f);
            }
            
            // Return to pool
            ReturnToPool(spark);
            
            OnSparkCollected?.Invoke(sparksCollected);
            
            Debug.Log($"[EmberforgeSparks] Spark collected! Total: {sparksCollected}");
        }

        private void ReturnToPool(GameObject spark)
        {
            activeSparks.Remove(spark);
            spark.SetActive(false);
            sparkPool.Enqueue(spark);
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
