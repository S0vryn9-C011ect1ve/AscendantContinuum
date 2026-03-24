using UnityEngine;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Spawns and manages floating ambient particles that slowly drift across the background.
    /// Creates magical, living atmosphere. Realm-specific particle types: sparkles, petals, motes, etc.
    /// </summary>
    public sealed class AmbientParticles : MonoBehaviour
    {
        [System.Serializable]
        public class ParticleSettings
        {
            [Tooltip("Prefab of particle to spawn (sprite with Rigidbody2D, no gravity)")]
            public GameObject particlePrefab;
            [Tooltip("Number of this particle type to spawn")]
            public int count = 3;
            [Tooltip("Base drift speed (units/sec)")]
            public float driftSpeed = 0.1f;
            [Tooltip("Random speed variance")]
            public float speedVariance = 0.05f;
        }

        [Header("Particle Types")]
        [SerializeField] private ParticleSettings[] particleTypes = new ParticleSettings[1];

        [Header("Spawn Volume")]
        [Tooltip("Spawn particles within camera view")]
        [SerializeField] private bool useCamera = true;
        [Tooltip("If not using camera, spawn within this bounds")]
        [SerializeField] private Bounds spawnBounds = new Bounds(Vector3.zero, new Vector3(20f, 12f, 1f));

        [Header("Motion")]
        [Tooltip("Ambient particles drift mostly horizontally")]
        [SerializeField] private float horizontalDriftAmount = 0.2f;
        [Tooltip("Subtle vertical bobbing")]
        [SerializeField] private float verticalBobAmount = 0.05f;
        [Tooltip("Speed of bobbing cycle")]
        [SerializeField] private float bobSpeed = 0.3f;

        [Header("Lifecycle")]
        [Tooltip("Particle lifespan in seconds (0 = infinite)")]
        [SerializeField] private float particleLifespan = 120f;
        [Tooltip("Alpha fade as particle ages")]
        [SerializeField] private bool fadeAtLife = true;

        [Header("Accessibility")]
        [SerializeField] private bool disableWhenReducedMotion = true;

        private class AmbientParticle
        {
            public GameObject gameObject;
            public SpriteRenderer spriteRenderer;
            public Vector3 basePosition;
            public Vector3 targetPosition;
            public float driftSpeed;
            public float spawnTime;
            public Color baseColor;
        }

        private AmbientParticle[] _particles;
        private Camera _mainCamera;
        private Bounds _cachedCameraBounds;

        private void Awake()
        {
            _mainCamera = Camera.main;
            SpawnParticles();
        }

           /// <summary>
           /// Attempt to auto-populate particleTypes from instantiated child GameObjects (created by builder).
           /// If still empty, create simple default particles so users see something immediately.
           /// </summary>
           private void Start()
           {
               if (particleTypes == null || particleTypes.Length == 0 || AllParticleTypesNull())
               {
                   CreateDefaultParticles();
               }
           }

           private bool AllParticleTypesNull()
           {
               foreach (var pt in particleTypes)
               {
                   if (pt != null && pt.particlePrefab != null)
                       return false;
               }
               return true;
           }

           private void CreateDefaultParticles()
           {
               // Create 2-3 simple default particle prefabs so users see something
               particleTypes = new ParticleSettings[2];

               // Particle 1: small colored circle (subtle)
               var p1go = new GameObject("DefaultParticle_Soft");
               p1go.transform.SetParent(transform, false);
               var p1sr = p1go.AddComponent<SpriteRenderer>();
               p1sr.sprite = CreateSimpleCircleSprite(Color.white, 0.1f);
           
               particleTypes[0] = new ParticleSettings
               {
                   particlePrefab = p1go,
                   count = 4,
                   driftSpeed = 0.05f,
                   speedVariance = 0.01f
               };

               // Particle 2: slightly larger glow (more visible)
               var p2go = new GameObject("DefaultParticle_Glow");
               p2go.transform.SetParent(transform, false);
               var p2sr = p2go.AddComponent<SpriteRenderer>();
               p2sr.sprite = CreateSimpleCircleSprite(new Color(0.8f, 0.8f, 1f, 0.6f), 0.15f);

               particleTypes[1] = new ParticleSettings
               {
                   particlePrefab = p2go,
                   count = 3,
                   driftSpeed = 0.03f,
                   speedVariance = 0.008f
               };

               Debug.Log("[AmbientParticles] Auto-created default particles for visibility.");
               SpawnParticles();
           }

           private Sprite CreateSimpleCircleSprite(Color color, float sizeUnits)
           {
               int size = 32;
               var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
               float center = (size - 1) / 2f;
               float radius = center - 2f;

               for (int y = 0; y < size; y++)
               {
                   for (int x = 0; x < size; x++)
                   {
                       float dx = x - center;
                       float dy = y - center;
                       float dist = Mathf.Sqrt(dx * dx + dy * dy);
                       float alpha = Mathf.Clamp01((radius - dist + 1.5f) / 1.5f) * color.a;
                       tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                   }
               }
               tex.Apply();

               var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 32f / sizeUnits);
               return sprite;
           }

        private void SpawnParticles()
        {
            int totalCount = 0;
            foreach (var type in particleTypes)
            {
                if (type != null && type.particlePrefab != null)
                    totalCount += type.count;
            }

            _particles = new AmbientParticle[totalCount];
            int index = 0;

            foreach (var type in particleTypes)
            {
                if (type == null || type.particlePrefab == null) continue;

                for (int i = 0; i < type.count; i++)
                {
                    Vector3 spawnPos = GetRandomSpawnPosition();
                    GameObject go = Instantiate(type.particlePrefab, spawnPos, Quaternion.identity, transform);
                    go.name = $"AmbientParticle_{index}";

                    var particle = new AmbientParticle
                    {
                        gameObject = go,
                        spriteRenderer = go.GetComponent<SpriteRenderer>(),
                        basePosition = spawnPos,
                        targetPosition = spawnPos,
                        driftSpeed = type.driftSpeed + Random.Range(-type.speedVariance, type.speedVariance),
                        spawnTime = Time.unscaledTime
                    };

                    if (particle.spriteRenderer != null)
                        particle.baseColor = particle.spriteRenderer.color;

                    _particles[index] = particle;
                    index++;
                }
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (useCamera && _mainCamera != null)
            {
                float camHeight = _mainCamera.orthographicSize * 2f;
                float camWidth = camHeight * _mainCamera.aspect;

                Vector3 camCenter = _mainCamera.transform.position;
                return new Vector3(
                    camCenter.x + Random.Range(-camWidth / 2f, camWidth / 2f),
                    camCenter.y + Random.Range(-camHeight / 2f, camHeight / 2f),
                    Random.Range(0.5f, 2f) // slight depth variation
                );
            }
            else
            {
                return spawnBounds.center + new Vector3(
                    Random.Range(-spawnBounds.extents.x, spawnBounds.extents.x),
                    Random.Range(-spawnBounds.extents.y, spawnBounds.extents.y),
                    Random.Range(-spawnBounds.extents.z, spawnBounds.extents.z)
                );
            }
        }

        private void LateUpdate()
        {
            bool reduced = disableWhenReducedMotion &&
                           AscendantContinuum.Core.AccessibilityManager.Instance != null &&
                           AscendantContinuum.Core.AccessibilityManager.Instance.IsReducedMotionEnabled();

            float t = Time.unscaledTime;

            foreach (var particle in _particles)
            {
                if (particle?.gameObject == null) continue;

                // Check lifespan
                if (particleLifespan > 0f)
                {
                    float age = t - particle.spawnTime;
                    if (age > particleLifespan)
                    {
                        Destroy(particle.gameObject);
                        continue;
                    }

                    // Fade at end of life
                    if (fadeAtLife && particle.spriteRenderer != null)
                    {
                        float fadeStart = particleLifespan * 0.8f;
                        if (age > fadeStart)
                        {
                            float fadeAlpha = 1f - ((age - fadeStart) / (particleLifespan - fadeStart));
                            Color c = particle.baseColor;
                            c.a = particle.baseColor.a * fadeAlpha;
                            particle.spriteRenderer.color = c;
                        }
                    }
                }

                if (reduced)
                {
                    particle.gameObject.transform.localPosition = particle.basePosition;
                    continue;
                }

                // Horizontal drift (slow sine wave)
                float driftX = Mathf.Sin((t * particle.driftSpeed) + particle.basePosition.x) * horizontalDriftAmount;

                // Vertical bob (slow cosine, minimal)
                float bobY = Mathf.Cos((t * bobSpeed) + particle.basePosition.y) * verticalBobAmount;

                Vector3 newPos = particle.basePosition + new Vector3(driftX, bobY, 0f);
                particle.gameObject.transform.localPosition = newPos;
            }
        }
    }
}
