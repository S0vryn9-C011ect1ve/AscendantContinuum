using System.Collections.Generic;
using UnityEngine;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Generic Object Pool Manager to prevent GC spikes during gameplay.
    /// Pools VFX, Sparks, Floating Sigils, and other frequently instantiated objects.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }

        [SerializeField] private List<Pool> pools;
        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }

            Debug.Log($"[ObjectPoolManager] Initialized {pools.Count} pools.");
        }

        /// <summary>
        /// Spawns an object from the pool.
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool with tag {tag} doesn't exist.");
                return null;
            }

            if (poolDictionary[tag].Count == 0)
            {
                // Expand pool if empty
                Debug.LogWarning($"[ObjectPoolManager] Pool {tag} is empty. Expanding pool.");
                Pool pool = pools.Find(p => p.tag == tag);
                if (pool != null)
                {
                    GameObject newObj = Instantiate(pool.prefab);
                    newObj.SetActive(false);
                    newObj.transform.SetParent(transform);
                    poolDictionary[tag].Enqueue(newObj);
                }
                else
                {
                    return null;
                }
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            // Add back to queue for reuse
            poolDictionary[tag].Enqueue(objectToSpawn);

            return objectToSpawn;
        }

        /// <summary>
        /// Returns an object to the pool (deactivates it).
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool with tag {tag} doesn't exist.");
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);
        }
    }
}
