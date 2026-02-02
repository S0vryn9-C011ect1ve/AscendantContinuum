using UnityEngine;
using System.Collections;

namespace AscendantContinuum.Astronomy
{
    /// <summary>
    /// Handles meteor shower visual effects and gameplay events
    /// Triggered by real astronomical data from CosmicDataManager
    /// </summary>
    public class MeteorShowerEvent : MonoBehaviour
    {
        public static MeteorShowerEvent Instance { get; private set; }

        [Header("Meteor Settings")]
        [SerializeField] private GameObject meteorPrefab;
        [SerializeField] private Transform meteorContainer;
        [SerializeField] private int meteorsPerMinute = 60; // Matches real rates (Perseids = 60-100/hr)
        
        [Header("Spawn Area")]
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-10f, 6f);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(10f, 8f);
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem showerAmbience;
        [SerializeField] private AudioClip meteorWhistleSound;
        
        private bool isShowerActive = false;
        private Coroutine showerCoroutine;
        private int meteorsCollected = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            if (meteorContainer == null)
            {
                GameObject container = new GameObject("MeteorContainer");
                meteorContainer = container.transform;
                meteorContainer.SetParent(transform);
            }
        }

        private void Start()
        {
            if (CosmicDataManager.Instance != null)
            {
                CosmicDataManager.Instance.OnAstronomicalEvent += OnAstronomicalEvent;
                
                // Check if we're in a meteor shower right now
                string activeShower = CosmicDataManager.Instance.GetActiveMeteorShower();
                if (!string.IsNullOrEmpty(activeShower))
                {
                    StartMeteorShower(activeShower);
                }
            }
        }

        private void OnAstronomicalEvent(string eventType, string eventData)
        {
            if (eventType == "MeteorShower")
            {
                StartMeteorShower(eventData);
            }
            else if (eventType == "MeteorShowerEnd")
            {
                StopMeteorShower();
            }
        }

        public void StartMeteorShower(string showerName)
        {
            if (isShowerActive) return;
            
            Debug.Log($"[MeteorShower] 💫 {showerName} meteor shower starting!");
            
            isShowerActive = true;
            meteorsCollected = 0;
            
            // Adjust rate based on shower
            meteorsPerMinute = GetShowerRate(showerName);
            
            // Visual effects
            if (showerAmbience != null)
            {
                showerAmbience.Play();
            }
            
            // Start spawning meteors
            if (showerCoroutine != null)
            {
                StopCoroutine(showerCoroutine);
            }
            showerCoroutine = StartCoroutine(SpawnMeteors());
            
            // Notify players
            ShowMeteorShowerNotification(showerName);
            
            // Track event
            Core.FirebaseManager.Instance?.TrackEvent("meteor_shower_start", 
                new System.Collections.Generic.Dictionary<string, object>
            {
                { "shower_name", showerName },
                { "rate", meteorsPerMinute }
            });
        }

        public void StopMeteorShower()
        {
            if (!isShowerActive) return;
            
            Debug.Log($"[MeteorShower] Meteor shower ended. Collected: {meteorsCollected}");
            
            isShowerActive = false;
            
            if (showerCoroutine != null)
            {
                StopCoroutine(showerCoroutine);
                showerCoroutine = null;
            }
            
            if (showerAmbience != null)
            {
                showerAmbience.Stop();
            }
            
            // Award bonus for participation
            if (meteorsCollected > 0)
            {
                int bonusSparks = meteorsCollected * 10;
                // Award sparks to player
                ShowShowerCompleteNotification(meteorsCollected, bonusSparks);
                
                // Achievement tracking
                Systems.AchievementManager.Instance?.TrackProgress("sky_watcher", meteorsCollected);
            }
        }

        private IEnumerator SpawnMeteors()
        {
            float spawnInterval = 60f / meteorsPerMinute; // Convert per-minute to interval
            
            while (isShowerActive)
            {
                yield return new WaitForSeconds(spawnInterval);
                
                if (Core.AccessibilityManager.Instance?.IsReducedMotionEnabled() == true)
                {
                    // Reduced motion: just show sparkle instead
                    SpawnReducedMotionMeteor();
                }
                else
                {
                    SpawnMeteor();
                }
            }
        }

        private void SpawnMeteor()
        {
            if (meteorPrefab == null) return;
            
            // Random spawn position at top of screen
            Vector2 spawnPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            
            GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity, meteorContainer);
            
            // Add meteor script if not on prefab
            Meteor meteorScript = meteor.GetComponent<Meteor>();
            if (meteorScript == null)
            {
                meteorScript = meteor.AddComponent<Meteor>();
            }
            
            meteorScript.OnMeteorCollected += OnMeteorCollected;
            
            // Play sound
            if (meteorWhistleSound != null)
            {
                Core.AudioManager.Instance?.PlaySFX(meteorWhistleSound, 0.3f);
            }
        }

        private void SpawnReducedMotionMeteor()
        {
            // Simple sparkle effect for accessibility
            Vector2 pos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            
            // Use particle manager for simple flash
            Core.ParticleManager.Instance?.PlayParticle("SimpleSpark", pos);
        }

        private void OnMeteorCollected()
        {
            meteorsCollected++;
            
            // Haptic feedback
            Core.AccessibilityManager.Instance?.TriggerHaptic(Core.HapticType.Success);
            
            // Small reward
            Debug.Log($"[MeteorShower] Meteor collected! Total: {meteorsCollected}");
        }

        private int GetShowerRate(string showerName)
        {
            // Match real-world meteor shower rates
            switch (showerName.ToLower())
            {
                case "perseids": return 80;
                case "geminids": return 120; // Best shower!
                case "leonids": return 15;
                case "quadrantids": return 110;
                case "orionids": return 20;
                case "lyrids": return 18;
                case "eta aquarids": return 50;
                default: return 60;
            }
        }

        private void ShowMeteorShowerNotification(string showerName)
        {
            string message = $"🌠 {showerName} meteor shower is active! Collect falling stars for bonus sparks.";
            Debug.Log($"[Notification] {message}");
            
            // TODO: Show in-game notification UI
            // For now, just log and track
        }

        private void ShowShowerCompleteNotification(int collected, int bonusSparks)
        {
            string message = $"Meteor shower complete! Collected {collected} meteors. +{bonusSparks} sparks!";
            Debug.Log($"[Notification] {message}");
        }
    }

    /// <summary>
    /// Individual meteor falling effect
    /// </summary>
    public class Meteor : MonoBehaviour
    {
        public System.Action OnMeteorCollected;
        
        [SerializeField] private float fallSpeed = 8f;
        [SerializeField] private float horizontalDrift = 2f;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private SpriteRenderer sprite;
        
        private Vector2 velocity;
        private bool isCollected = false;

        private void Start()
        {
            // Falling diagonally like real meteors
            velocity = new Vector2(
                Random.Range(-horizontalDrift, horizontalDrift),
                -fallSpeed
            );
            
            // Setup trail
            if (trail == null) trail = GetComponent<TrailRenderer>();
            if (trail != null)
            {
                trail.startColor = new Color(1f, 0.9f, 0.5f, 1f);
                trail.endColor = new Color(1f, 0.5f, 0.2f, 0f);
            }
            
            // Sprite
            if (sprite == null) sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = new Color(1f, 0.9f, 0.5f);
            }
            
            // Auto-destroy after falling off screen
            Destroy(gameObject, 5f);
        }

        private void Update()
        {
            if (!isCollected)
            {
                transform.Translate(velocity * Time.deltaTime);
                
                // Slight rotation for effect
                transform.Rotate(0f, 0f, 100f * Time.deltaTime);
            }
        }

        private void OnMouseDown()
        {
            CollectMeteor();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Also collectable by touch/tap
            if (other.CompareTag("Player") || other.CompareTag("TouchArea"))
            {
                CollectMeteor();
            }
        }

        private void CollectMeteor()
        {
            if (isCollected) return;
            
            isCollected = true;
            
            // Notify event
            OnMeteorCollected?.Invoke();
            
            // Visual feedback
            if (sprite != null)
            {
                sprite.color = Color.white;
                StartCoroutine(FadeAndDestroy());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private System.Collections.IEnumerator FadeAndDestroy()
        {
            float fadeTime = 0.3f;
            float elapsed = 0f;
            
            Vector3 startScale = transform.localScale;
            Color startColor = sprite.color;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeTime;
                
                transform.localScale = Vector3.Lerp(startScale, startScale * 2f, t);
                sprite.color = Color.Lerp(startColor, new Color(1f, 1f, 1f, 0f), t);
                
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}
