using UnityEngine;
using System.Collections.Generic;
using AscendantContinuum.Astronomy;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Social;
using AscendantContinuum.Realms.EchoFields;

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
            // Use real astronomical data if available
            if (Astronomy.CosmicDataManager.Instance != null)
            {
                GenerateRealConstellation();
            }
            else
            {
                GenerateConstellation();
            }
        }

        private void GenerateRealConstellation()
        {
            ClearConstellation();

            var visibleConstellations = Astronomy.CosmicDataManager.Instance.VisibleConstellations;

            if (visibleConstellations.Count > 0)
            {
                // Pick a random visible constellation
                var constellation = visibleConstellations[Random.Range(0, visibleConstellations.Count)];
                Debug.Log($"[ConstellationTracer] Using real constellation: {constellation.name} ({constellation.season})");

                // Get star pattern for this constellation
                ConstellationPattern pattern = GetRealConstellationPattern(constellation.name);
                GenerateFromPattern(pattern, constellation.name);
            }
            else
            {
                // Fallback to procedural
                GenerateConstellation();
            }
        }

        private ConstellationPattern GetRealConstellationPattern(string constellationName)
        {
            // Real constellation star patterns (simplified major stars)
            return constellationName switch
            {
                "Orion" => new ConstellationPattern
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 2),      // Betelgeuse (shoulder)
                        new Vector2(-0.5f, 0),  // Belt star 1
                        new Vector2(0, 0),      // Belt star 2 (Alnitak)
                        new Vector2(0.5f, 0),   // Belt star 3
                        new Vector2(0, -2),     // Rigel (foot)
                    }
                },
                "Ursa Major" => new ConstellationPattern // Big Dipper
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 0),
                        new Vector2(1, 0),
                        new Vector2(2, 0),
                        new Vector2(2.5f, 0.5f),
                        new Vector2(2, 1.5f),
                        new Vector2(1, 2),
                        new Vector2(0, 1.5f)
                    }
                },
                "Cassiopeia" => new ConstellationPattern // W-shape
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 0),
                        new Vector2(0.8f, -0.8f),
                        new Vector2(1.6f, 0),
                        new Vector2(2.4f, -0.8f),
                        new Vector2(3.2f, 0)
                    }
                },
                "Cygnus" => new ConstellationPattern // Northern Cross
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 2),      // Deneb (tail)
                        new Vector2(0, 1),
                        new Vector2(0, 0),      // Center
                        new Vector2(-1, 0),     // Wing
                        new Vector2(1, 0),      // Wing
                        new Vector2(0, -1),     // Body
                        new Vector2(0, -2)      // Albireo (head)
                    }
                },
                "Leo" => new ConstellationPattern
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 0),      // Regulus
                        new Vector2(1, 0.5f),
                        new Vector2(2, 1),
                        new Vector2(2.5f, 0.5f),
                        new Vector2(2, 0)
                    }
                },
                _ => GetRandomPattern() // Fallback
            };
        }

        private void GenerateFromPattern(ConstellationPattern pattern, string name)
        {
            // Scale the real pattern positions by starSpacing so the designer
            // can adjust constellation spread from the Inspector.
            float scale = Mathf.Max(0.5f, starSpacing);
            foreach (Vector2 offset in pattern.positions)
            {
                Vector3 position = transform.position + new Vector3(offset.x * scale, offset.y * scale, 0f);
                GameObject starObj = Instantiate(starPrefab, position, Quaternion.identity, transform);

                Star star = starObj.GetComponent<Star>();
                if (star != null)
                {
                    star.OnStarTouched += HandleStarTouched;
                    star.SetConstellationName(name); // Show real name
                    activeStars.Add(star);
                }
            }

            Debug.Log($"[ConstellationTracer] Generated {name} with {activeStars.Count} stars (spacing: {scale})");
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
            // Procedurally generate a constellation using starsPerConstellation and starSpacing
            // so the designer can tune density and spread from the Inspector.
            int count = Mathf.Max(3, starsPerConstellation);
            float scale = Mathf.Max(0.5f, starSpacing);

            List<ConstellationPattern> patterns = new List<ConstellationPattern>
            {
                // Arc pattern — uses full count and spacing
                new ConstellationPattern
                {
                    positions = GenerateArcPositions(count, scale)
                },
                // Zigzag pattern
                new ConstellationPattern
                {
                    positions = GenerateZigzagPositions(count, scale)
                },
                // Triangle or polygon (for small counts, pre-defined shape)
                new ConstellationPattern
                {
                    positions = new Vector2[]
                    {
                        new Vector2(0, 0),
                        new Vector2(scale * 2, 0),
                        new Vector2(scale, scale * 2),
                        new Vector2(0, 0),
                    }
                }
            };

            return patterns[Random.Range(0, patterns.Count)];
        }

        private Vector2[] GenerateArcPositions(int count, float spacing)
        {
            Vector2[] pos = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float t = count > 1 ? (float)i / (count - 1) : 0f;
                float angle = Mathf.Lerp(-60f, 60f, t) * Mathf.Deg2Rad;
                pos[i] = new Vector2(Mathf.Sin(angle) * spacing * (count * 0.5f),
                                     Mathf.Cos(angle) * spacing * (count * 0.3f));
            }
            return pos;
        }

        private Vector2[] GenerateZigzagPositions(int count, float spacing)
        {
            Vector2[] pos = new Vector2[count];
            for (int i = 0; i < count; i++)
                pos[i] = new Vector2(i * spacing, (i % 2 == 0 ? 0f : spacing));
            return pos;
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

            // Serendipity roll on every constellation completion
            SerendipityManager.Instance?.TryTrigger("EchoFields");

            // Track stargazer achievement (4 seasons of constellations)
            AchievementManager.Instance?.TrackProgress("stargazer", 1);

            // Apply Real Stargazing bonus (2× if player verified real sky recently)
            float starBonus = RealStargazingManager.Instance?.GetStarTraceBonus() ?? 1f;
            if (starBonus > 1f)
                Debug.Log($"[ConstellationTracer] Real Stargazing bonus active: {starBonus}×");

            // Offer kindness blessing on constellation completion
            Social.KindnessChainManager.Instance?.ShowSendBlessingPrompt("EchoFields");

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
        private string constellationName = "";

        public System.Action<Star> OnStarTouched;

        public void SetConstellationName(string name)
        {
            constellationName = name;
        }

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
