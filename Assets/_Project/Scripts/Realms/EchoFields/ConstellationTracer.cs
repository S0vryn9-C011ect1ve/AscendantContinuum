using UnityEngine;
using System.Collections.Generic;

namespace AscendantContinuum.EchoFields
{
    /// <summary>
    /// Echo Fields ritual - Trace constellations by connecting stars
    /// Creates beautiful patterns in the void that echo past players
    /// </summary>
    public class ConstellationTracer : MonoBehaviour
    {
        [Header("Star Settings")]
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private int starsPerConstellation = 5;
        [SerializeField] private float starSpacing = 2f;
        
        [Header("Line Settings")]
        [SerializeField] private LineRenderer lineRendererPrefab;
        [SerializeField] private Color lineColor = new Color(0.5f, 0.7f, 1f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip starConnectSound;
        [SerializeField] private AudioClip constellationCompleteSound;
        
        private List<Star> activeStars = new List<Star>();
        private List<Star> connectedStars = new List<Star>();
        private LineRenderer currentLine;
        private int constellationsCompleted = 0;
        
        public System.Action<int> OnConstellationCompleted;

        private void Start()
        {
            GenerateConstellation();
        }

        private void Update()
        {
            // Update line to follow touch/mouse
            if (connectedStars.Count > 0 && currentLine != null)
            {
                Vector3 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                cursorPos.z = 0f;
                
                currentLine.positionCount = connectedStars.Count + 1;
                for (int i = 0; i < connectedStars.Count; i++)
                {
                    currentLine.SetPosition(i, connectedStars[i].transform.position);
                }
                currentLine.SetPosition(connectedStars.Count, cursorPos);
            }
        }

        private void GenerateConstellation()
        {
            ClearConstellation();
            
            // Generate star positions in a pattern
            ConstellationPattern pattern = GetRandomPattern();
            
            foreach (Vector2 offset in pattern.positions)
            {
                Vector3 position = transform.position + new Vector3(offset.x, offset.y, 0f);
                GameObject starObj = Instantiate(starPrefab, position, Quaternion.identity, transform);
                
                Star star = starObj.GetComponent<Star>();
                if (star != null)
                {
                    star.OnStarTouched += HandleStarTouched;
                    activeStars.Add(star);
                }
            }
            
            Debug.Log($"[ConstellationTracer] Generated {activeStars.Count} stars");
        }

        private ConstellationPattern GetRandomPattern()
        {
            // Pre-defined constellation patterns
            List<ConstellationPattern> patterns = new List<ConstellationPattern>
            {
                // Dipper pattern
                new ConstellationPattern
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 0),
                        new Vector2(1, 0.5f),
                        new Vector2(2, 0.3f),
                        new Vector2(2.5f, -0.5f),
                        new Vector2(3, -1.5f)
                    }
                },
                // Triangle
                new ConstellationPattern
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 0),
                        new Vector2(2, 0),
                        new Vector2(1, 2),
                        new Vector2(0, 0), // Close the shape
                    }
                },
                // Zigzag
                new ConstellationPattern
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 0),
                        new Vector2(1, 1),
                        new Vector2(2, 0),
                        new Vector2(3, 1),
                        new Vector2(4, 0)
                    }
                }
            };
            
            return patterns[Random.Range(0, patterns.Count)];
        }

        private void HandleStarTouched(Star star)
        {
            // Check if this is the correct next star
            if (connectedStars.Count == 0)
            {
                // First star can be any star
                ConnectStar(star);
            }
            else if (activeStars.IndexOf(star) == connectedStars.Count)
            {
                // Correct sequential star
                ConnectStar(star);
            }
            else
            {
                // Wrong star - reset
                ResetTracing();
            }
        }

        private void ConnectStar(Star star)
        {
            star.Connect();
            connectedStars.Add(star);
            
            // Create or update line
            if (currentLine == null)
            {
                GameObject lineObj = new GameObject("ConstellationLine");
                lineObj.transform.SetParent(transform);
                currentLine = lineObj.AddComponent<LineRenderer>();
                currentLine.startWidth = 0.05f;
                currentLine.endWidth = 0.05f;
                currentLine.material = new Material(Shader.Find("Sprites/Default"));
                currentLine.startColor = lineColor;
                currentLine.endColor = lineColor;
            }
            
            // Play connect sound
            if (starConnectSound != null)
            {
                Core.AudioManager.Instance?.PlaySFX(starConnectSound, star.transform.position, 0.7f);
            }
            
            // Haptic feedback
            Core.AccessibilityManager.Instance?.TriggerHaptic(Core.HapticType.Light);
            
            // Check if constellation complete
            if (connectedStars.Count >= activeStars.Count)
            {
                CompleteConstellation();
            }
        }

        private void CompleteConstellation()
        {
            constellationsCompleted++;
            
            // Play completion sound
            if (constellationCompleteSound != null)
            {
                Core.AudioManager.Instance?.PlaySFX(constellationCompleteSound, 1f);
            }
            
            // Haptic feedback
            Core.AccessibilityManager.Instance?.TriggerHaptic(Core.HapticType.Success);
            
            // Visual effect
            foreach (Star star in activeStars)
            {
                VFX.ParticleManager.Instance?.PlaySparkCollectEffect(star.transform.position);
            }
            
            OnConstellationCompleted?.Invoke(constellationsCompleted);
            
            // Generate new constellation after delay
            Invoke(nameof(GenerateConstellation), 3f);
        }

        private void ResetTracing()
        {
            foreach (Star star in connectedStars)
            {
                star.Disconnect();
            }
            
            connectedStars.Clear();
            
            if (currentLine != null)
            {
                Destroy(currentLine.gameObject);
                currentLine = null;
            }
        }

        private void ClearConstellation()
        {
            foreach (Star star in activeStars)
            {
                if (star != null)
                    Destroy(star.gameObject);
            }
            
            activeStars.Clear();
            connectedStars.Clear();
            
            if (currentLine != null)
            {
                Destroy(currentLine.gameObject);
                currentLine = null;
            }
        }
    }

    public class Star : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private bool isConnected = false;
        private Color baseColor = new Color(0.8f, 0.9f, 1f);
        private Color connectedColor = new Color(0.3f, 0.7f, 1f);
        
        public System.Action<Star> OnStarTouched;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = baseColor;
        }

        private void Update()
        {
            // Gentle pulse (respects reduced motion)
            if (Core.AccessibilityManager.Instance?.ReducedMotionEnabled == false)
            {
                float pulse = (Mathf.Sin(Time.time * 2f) + 1f) / 2f;
                spriteRenderer.color = Color.Lerp(baseColor, Color.white, pulse * 0.3f);
            }
        }

        public void Connect()
        {
            isConnected = true;
            spriteRenderer.color = connectedColor;
        }

        public void Disconnect()
        {
            isConnected = false;
            spriteRenderer.color = baseColor;
        }

        private void OnMouseDown()
        {
            if (!isConnected)
            {
                OnStarTouched?.Invoke(this);
            }
        }
    }

    public class ConstellationPattern
    {
        public Vector2[] positions;
    }
}
