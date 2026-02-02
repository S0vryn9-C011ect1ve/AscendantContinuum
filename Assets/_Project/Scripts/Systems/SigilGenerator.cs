using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Generates unique player sigils based on playstyle and choices
    /// Each sigil is a personal magical signature - no two are exactly alike
    /// </summary>
    public class SigilGenerator : MonoBehaviour
    {
        public static SigilGenerator Instance { get; private set; }

        [Header("Sigil Generation Rules")]
        [SerializeField] private SigilShape[] baseShapes;
        [SerializeField] private SigilPattern[] patterns;
        [SerializeField] private Color[] colorPalette;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Generate sigil from player behavior metrics
        /// </summary>
        public Data.SigilData GenerateSigil(PlayerPlaystyleMetrics metrics)
        {
            Data.SigilData sigil = new Data.SigilData
            {
                sigilId = Guid.NewGuid().ToString()
            };
            
            // Base shape determined by primary realm affinity
            sigil.baseShape = DetermineBaseShape(metrics);
            
            // Pattern determined by play speed and rhythm
            sigil.pattern = DeterminePattern(metrics);
            
            // Colors determined by time of day played and accessibility mode
            (sigil.primaryColor, sigil.secondaryColor) = DetermineColors(metrics);
            
            // Store generation data
            sigil.generatedFromPlaystyle = JsonUtility.ToJson(metrics);
            
            Debug.Log($"[SigilGenerator] Generated unique sigil: {sigil.sigilId}");
            Debug.Log($"  Shape: {sigil.baseShape}, Pattern: {sigil.pattern}");
            
            return sigil;
        }

        private int DetermineBaseShape(PlayerPlaystyleMetrics metrics)
        {
            // Each realm has associated shapes
            Dictionary<string, int> realmShapes = new Dictionary<string, int>
            {
                { "emberforge", 0 },      // Triangle (fire)
                { "verdant", 1 },          // Hexagon (growth)
                { "echo", 2 },             // Circle (infinity)
                { "dawn", 3 },             // Square (stability)
                { "lantern", 4 }           // Star (transcendence)
            };
            
            // Find most visited realm
            string favoriteRealm = metrics.realmVisitCounts
                .OrderByDescending(kvp => kvp.Value)
                .FirstOrDefault().Key ?? "emberforge";
            
            return realmShapes.ContainsKey(favoriteRealm) ? realmShapes[favoriteRealm] : 0;
        }

        private int DeterminePattern(PlayerPlaystyleMetrics metrics)
        {
            // Fast players get sharp patterns, slow players get flowing patterns
            float avgSessionDuration = metrics.totalPlayTime / Mathf.Max(metrics.sessionsPlayed, 1);
            
            if (avgSessionDuration < 120f) // < 2 minutes
                return 0; // Sharp, energetic
            else if (avgSessionDuration < 300f) // < 5 minutes
                return 1; // Balanced
            else
                return 2; // Flowing, contemplative
            
            // Add more pattern variety based on other metrics
            if (metrics.reducedMotionUsage > 0.5f)
                return 3; // Calm, accessible pattern
            
            return 0;
        }

        private (Color primary, Color secondary) DetermineColors(PlayerPlaystyleMetrics metrics)
        {
            Color primary, secondary;
            
            // Colorblind mode influences colors (accessibility IS gameplay)
            if (metrics.primaryColorblindMode > 0)
            {
                // Use colors that work well with their accessibility mode
                switch (metrics.primaryColorblindMode)
                {
                    case 1: // Protanopia (red-blind)
                        primary = new Color(0.2f, 0.6f, 1f); // Blue
                        secondary = new Color(1f, 0.8f, 0.2f); // Yellow
                        break;
                    case 2: // Deuteranopia (green-blind)
                        primary = new Color(1f, 0.4f, 0.6f); // Pink
                        secondary = new Color(0.4f, 0.4f, 1f); // Purple
                        break;
                    case 3: // Tritanopia (blue-blind)
                        primary = new Color(1f, 0.3f, 0.3f); // Red
                        secondary = new Color(0.3f, 1f, 0.5f); // Green
                        break;
                    default:
                        primary = new Color(0.5f, 0.5f, 0.5f); // Neutral
                        secondary = Color.white;
                        break;
                }
            }
            else
            {
                // Time-based colors (when they play most)
                int hourMostPlayed = metrics.peakPlayHour;
                
                if (hourMostPlayed >= 5 && hourMostPlayed < 12) // Morning
                {
                    primary = new Color(1f, 0.9f, 0.4f); // Sunrise gold
                    secondary = new Color(1f, 0.6f, 0.3f); // Dawn orange
                }
                else if (hourMostPlayed >= 12 && hourMostPlayed < 17) // Afternoon
                {
                    primary = new Color(0.3f, 0.7f, 1f); // Sky blue
                    secondary = new Color(0.2f, 1f, 0.5f); // Vibrant green
                }
                else if (hourMostPlayed >= 17 && hourMostPlayed < 21) // Evening
                {
                    primary = new Color(1f, 0.4f, 0.6f); // Sunset pink
                    secondary = new Color(0.6f, 0.3f, 1f); // Twilight purple
                }
                else // Night
                {
                    primary = new Color(0.2f, 0.2f, 0.5f); // Deep blue
                    secondary = new Color(0.8f, 0.8f, 1f); // Moonlight silver
                }
            }
            
            return (primary, secondary);
        }

        /// <summary>
        /// Generate a Texture2D representation of the sigil for display
        /// </summary>
        public Texture2D RenderSigil(Data.SigilData sigilData, int resolution = 256)
        {
            Texture2D texture = new Texture2D(resolution, resolution);
            Color[] pixels = new Color[resolution * resolution];
            
            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float radius = resolution * 0.4f;
            
            // Draw base shape
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    
                    // Base shape geometry
                    bool inShape = IsPointInShape(pos, center, radius, sigilData.baseShape);
                    
                    // Pattern overlay
                    bool inPattern = IsPointInPattern(pos, center, radius, sigilData.pattern);
                    
                    if (inShape)
                    {
                        pixels[y * resolution + x] = inPattern ? sigilData.secondaryColor : sigilData.primaryColor;
                    }
                    else
                    {
                        pixels[y * resolution + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }

        private bool IsPointInShape(Vector2 point, Vector2 center, float radius, int shapeType)
        {
            Vector2 offset = point - center;
            float dist = offset.magnitude;
            
            switch (shapeType)
            {
                case 0: // Triangle
                    float angle = Mathf.Atan2(offset.y, offset.x) + Mathf.PI / 2f;
                    float triangleRadius = radius / Mathf.Cos((angle % (2f * Mathf.PI / 3f)) - Mathf.PI / 3f);
                    return dist < triangleRadius && dist > radius * 0.7f;
                    
                case 1: // Hexagon
                    float hexAngle = Mathf.Atan2(offset.y, offset.x);
                    float hexRadius = radius / Mathf.Cos((hexAngle % (Mathf.PI / 3f)) - Mathf.PI / 6f);
                    return dist < hexRadius && dist > radius * 0.6f;
                    
                case 2: // Circle
                    return dist < radius && dist > radius * 0.8f;
                    
                case 3: // Square
                    return Mathf.Abs(offset.x) < radius && Mathf.Abs(offset.y) < radius &&
                           (Mathf.Abs(offset.x) > radius * 0.7f || Mathf.Abs(offset.y) > radius * 0.7f);
                    
                case 4: // Star (5-pointed)
                    float starAngle = Mathf.Atan2(offset.y, offset.x) + Mathf.PI / 2f;
                    float starSegment = (starAngle % (2f * Mathf.PI / 5f)) - Mathf.PI / 5f;
                    float starRadius = radius * (0.6f + 0.4f * Mathf.Cos(5f * starSegment));
                    return dist < starRadius && dist > starRadius * 0.6f;
                    
                default:
                    return dist < radius;
            }
        }

        private bool IsPointInPattern(Vector2 point, Vector2 center, float radius, int patternType)
        {
            Vector2 offset = point - center;
            
            switch (patternType)
            {
                case 0: // Sharp lines
                    return Mathf.Abs(offset.x % 10f) < 2f || Mathf.Abs(offset.y % 10f) < 2f;
                    
                case 1: // Dots
                    return (Mathf.RoundToInt(offset.x / 15f) + Mathf.RoundToInt(offset.y / 15f)) % 2 == 0;
                    
                case 2: // Waves
                    return Mathf.Sin(offset.x * 0.1f) * 10f > offset.y % 20f - 10f;
                    
                case 3: // Calm (minimal pattern)
                    return offset.magnitude % 30f < 3f;
                    
                default:
                    return false;
            }
        }
    }

    [Serializable]
    public class SigilShape
    {
        public string shapeName;
        public int sides;
    }

    [Serializable]
    public class SigilPattern
    {
        public string patternName;
        public Texture2D patternTexture;
    }

    /// <summary>
    /// Metrics tracked to generate personalized sigils
    /// </summary>
    [Serializable]
    public class PlayerPlaystyleMetrics
    {
        public int totalPlayTime = 0;
        public int sessionsPlayed = 0;
        public Dictionary<string, int> realmVisitCounts = new Dictionary<string, int>();
        public int primaryColorblindMode = 0;
        public float reducedMotionUsage = 0f; // 0-1
        public int peakPlayHour = 12; // 0-23
        public int sparksCollectedPerMinute = 0;
        public bool prefersSlowPace = false;
    }
}
