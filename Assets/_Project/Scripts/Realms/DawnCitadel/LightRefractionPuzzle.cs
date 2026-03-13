using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Realms.DawnCitadel
{
    /// <summary>
    /// Light refraction puzzle system for Dawn Citadel.
    /// Players rotate prisms to direct light beams to targets.
    /// Supports multiple solutions and accessibility features.
    /// </summary>
    public class LightRefractionPuzzle : MonoBehaviour
    {
        [Header("Puzzle Configuration")]
        [SerializeField] private int puzzleComplexity = 1; // 1=Simple, 2=Medium, 3=Complex
        [SerializeField] private List<Prism> prisms = new List<Prism>();
        [SerializeField] private List<LightTarget> targets = new List<LightTarget>();
        [SerializeField] private Transform lightSource;

        [Header("Visual Components")]
        [SerializeField] private LineRenderer lightBeamRenderer;
        [SerializeField] private Material lightBeamMaterial;
        [SerializeField] private Color beamColor = Color.yellow;
        [SerializeField] private ParticleSystem successParticles;

        [Header("Audio")]
        [SerializeField] private AudioClip prismRotateSound;
        [SerializeField] private AudioClip targetHitSound;
        [SerializeField] private AudioClip puzzleCompleteSound;

        [Header("Accessibility")]
        [SerializeField] private bool autoAimEnabled = false;
        [SerializeField] private bool _showHints = false;
        [SerializeField] private float _hintDelay = 30f; // Show hint after 30s

        // Public accessors for controller-driven configuration (e.g. Scribe deity deep hints)
        public bool  showHints  { get => _showHints;  set => _showHints  = value; }
        public float hintDelay  { get => _hintDelay;  set => _hintDelay  = value; }

        // State
        private bool isPuzzleComplete = false;
        private float puzzleStartTime;
        private int totalRotations = 0;
        private List<LineRenderer> activeBeams = new List<LineRenderer>();

        // Accessibility
        private bool reducedMotion = false;
        private string colorblindMode = "None";

        // Events
        public System.Action<LightRefractionPuzzle> OnPuzzleComplete;

        [Header("Completion")]
        [SerializeField] private AscendantContinuum.UI.RealmCompletionPanel completionPanel;
        [SerializeField] private int goalPuzzles = 1;

        private void Start()
        {
            // Check accessibility settings
            if (AccessibilityManager.Instance != null)
            {
                reducedMotion = AccessibilityManager.Instance.IsReducedMotionEnabled();
                colorblindMode = AccessibilityManager.Instance.CurrentColorblindMode.ToString();

                // Enable auto-aim for accessibility
                if (colorblindMode != "None")
                    autoAimEnabled = true;
            }

            puzzleStartTime = Time.time;
            InitializePuzzle();

            // Start hint timer
            if (showHints)
                StartCoroutine(HintCoroutine());
        }

        private void Update()
        {
            UpdateLightBeams();
            CheckPuzzleCompletion();
        }

        #region Initialization

        private void InitializePuzzle()
        {
            // Initialize all prisms
            foreach (var prism in prisms)
            {
                prism.Initialize();
                prism.OnRotated += HandlePrismRotated;
            }

            // Initialize all targets
            foreach (var target in targets)
            {
                target.Initialize();
            }

            // Apply colorblind patterns
            if (colorblindMode != "None")
            {
                ApplyColorblindPatterns();
            }
        }

        private void ApplyColorblindPatterns()
        {
            // Different beam patterns for colorblind modes
            // This ensures puzzles are solvable without color differentiation
            lightBeamMaterial.SetFloat("_PatternType", GetPatternForMode(colorblindMode));
        }

        private int GetPatternForMode(string mode)
        {
            switch (mode)
            {
                case "Protanopia": return 1; // Solid line
                case "Deuteranopia": return 2; // Dashed line
                case "Tritanopia": return 3; // Wavy line
                case "Achromatopsia": return 4; // Dotted line
                default: return 0; // No pattern
            }
        }

        #endregion

        #region Light Beam System

        private void UpdateLightBeams()
        {
            // Clear previous beams
            ClearBeams();

            if (lightSource == null) return;

            // Trace light from source through prisms
            TraceLightPath(lightSource.position, lightSource.forward, beamColor, 0);
        }

        private void TraceLightPath(Vector3 startPos, Vector3 direction, Color color, int bounceCount)
        {
            if (bounceCount > 10) return; // Prevent infinite recursion

            RaycastHit hit;
            if (Physics.Raycast(startPos, direction, out hit, 100f))
            {
                // Draw beam to hit point
                DrawBeam(startPos, hit.point, color);

                // Check what was hit
                Prism prism = hit.collider.GetComponent<Prism>();
                LightTarget target = hit.collider.GetComponent<LightTarget>();

                if (prism != null)
                {
                    // Light hit a prism - refract it
                    Vector3 refractedDirection = prism.GetRefractedDirection(direction);
                    Color refractedColor = prism.GetRefractedColor(color);

                    // Continue tracing from prism
                    TraceLightPath(hit.point + refractedDirection * 0.1f, refractedDirection, refractedColor, bounceCount + 1);
                }
                else if (target != null)
                {
                    // Light hit a target
                    target.OnLightHit(color);

                    // Play sound
                    if (AudioManager.Instance != null && targetHitSound != null)
                        AudioManager.Instance.PlaySFX(targetHitSound, hit.point);
                }
            }
            else
            {
                // Draw beam to max distance
                DrawBeam(startPos, startPos + direction * 100f, color);
            }
        }

        private void DrawBeam(Vector3 start, Vector3 end, Color color)
        {
            LineRenderer beam = GetBeamRenderer();
            beam.positionCount = 2;
            beam.SetPosition(0, start);
            beam.SetPosition(1, end);
            beam.startColor = color;
            beam.endColor = color;
            beam.enabled = true;

            activeBeams.Add(beam);
        }

        private LineRenderer GetBeamRenderer()
        {
            // Object pooling for beam renderers
            GameObject beamObj = new GameObject("LightBeam");
            beamObj.transform.SetParent(transform);
            LineRenderer renderer = beamObj.AddComponent<LineRenderer>();
            renderer.material = lightBeamMaterial;
            renderer.startWidth = 0.1f;
            renderer.endWidth = 0.1f;
            return renderer;
        }

        private void ClearBeams()
        {
            foreach (var beam in activeBeams)
            {
                if (beam != null)
                    Destroy(beam.gameObject);
            }
            activeBeams.Clear();

            // Reset all targets
            foreach (var target in targets)
            {
                target.ResetHitState();
            }
        }

        #endregion

        #region Puzzle Logic

        private void CheckPuzzleCompletion()
        {
            if (isPuzzleComplete) return;

            // Check if all targets are lit
            bool allTargetsLit = true;
            foreach (var target in targets)
            {
                if (!target.IsLit)
                {
                    allTargetsLit = false;
                    break;
                }
            }

            if (allTargetsLit)
            {
                CompletePuzzle();
            }
        }

        private void CompletePuzzle()
        {
            isPuzzleComplete = true;
            float completionTime = Time.time - puzzleStartTime;

            // Play success effects
            if (successParticles != null && !reducedMotion)
                successParticles.Play();

            if (AudioManager.Instance != null && puzzleCompleteSound != null)
                AudioManager.Instance.PlaySFX(puzzleCompleteSound, transform.position);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Success);

            // Record achievement
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.UnlockAchievement($"dawn_puzzle_{puzzleComplexity}");

                // Special achievement for solving without hints
                if (!showHints && totalRotations < 10)
                    AchievementManager.Instance.UnlockAchievement("dawn_puzzle_master");
            }

            // Trigger event
            OnPuzzleComplete?.Invoke(this);

            // Realm completion panel
            if (completionPanel != null)
            {
                int stars = completionTime < 30f ? 3 : completionTime < 90f ? 2 : 1;
                completionPanel.ShowCompletion("Dawn Citadel Complete! ☀️",
                    $"Solved in {completionTime:F0}s", stars);
                Core.GameEvents.RaiseRealmCompleted("dawn", Mathf.RoundToInt(1000f / Mathf.Max(completionTime, 1f)));
            }

            Debug.Log($"Puzzle completed in {completionTime:F1}s with {totalRotations} rotations");
        }

        public void ResetPuzzle()
        {
            isPuzzleComplete = false;
            puzzleStartTime = Time.time;
            totalRotations = 0;

            // Reset all prisms
            foreach (var prism in prisms)
            {
                prism.ResetRotation();
            }

            // Reset all targets
            foreach (var target in targets)
            {
                target.Reset();
            }

            ClearBeams();
        }

        #endregion

        #region Event Handlers

        private void HandlePrismRotated(Prism prism)
        {
            totalRotations++;

            if (AudioManager.Instance != null && prismRotateSound != null)
                AudioManager.Instance.PlaySFX(prismRotateSound, prism.transform.position);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Light);

            // Auto-aim: when accessibility auto-aim is enabled, snap the prism to
            // the nearest 45-degree increment so the beam has a better chance of
            // reaching a target — helps players with reduced motor control.
            if (autoAimEnabled && prism != null)
            {
                float current = prism.transform.eulerAngles.z;
                float snapped = Mathf.Round(current / 45f) * 45f;
                prism.transform.eulerAngles = new Vector3(0f, 0f, snapped);
            }
        }

        #endregion

        #region Hint System

        private IEnumerator HintCoroutine()
        {
            yield return new WaitForSeconds(hintDelay);

            if (!isPuzzleComplete)
            {
                ShowHint();
            }
        }

        private void ShowHint()
        {
            // Highlight one prism that needs to be rotated
            foreach (var prism in prisms)
            {
                if (!prism.IsCorrectlyPositioned())
                {
                    prism.ShowHint();
                    break;
                }
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Unsubscribe from events
            foreach (var prism in prisms)
            {
                if (prism != null)
                    prism.OnRotated -= HandlePrismRotated;
            }

            OnPuzzleComplete = null;
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Represents a prism that refracts light
    /// </summary>
    [System.Serializable]
    public class Prism : MonoBehaviour
    {
        [SerializeField] private float targetRotation = 45f; // Correct rotation angle
        [SerializeField] private float rotationSpeed = 90f; // Degrees per second
        [SerializeField] private float rotationTolerance = 5f; // Degrees of tolerance

        private float currentRotation = 0f;
        private bool showingHint = false;

        public System.Action<Prism> OnRotated;

        public void Initialize()
        {
            currentRotation = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        }

        // Tap/click to rotate clockwise by 45°
        private void OnMouseDown()
        {
            SnapRotate(45f);
        }

        /// <summary>Called by TouchInputManager on mobile tap.</summary>
        public void OnTouchTapped()
        {
            SnapRotate(45f);
        }

        private void SnapRotate(float degrees)
        {
            currentRotation = (currentRotation + degrees) % 360f;
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            OnRotated?.Invoke(this);
        }

        public void RotateClockwise()
        {
            currentRotation += rotationSpeed * Time.deltaTime;
            currentRotation = currentRotation % 360f;
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            OnRotated?.Invoke(this);
        }

        public void RotateCounterClockwise()
        {
            currentRotation -= rotationSpeed * Time.deltaTime;
            currentRotation = (currentRotation + 360f) % 360f;
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            OnRotated?.Invoke(this);
        }

        public void SetRotation(float angle)
        {
            currentRotation = angle % 360f;
            transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        }

        public bool IsCorrectlyPositioned()
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(currentRotation, targetRotation));
            return diff <= rotationTolerance;
        }

        public Vector3 GetRefractedDirection(Vector3 incomingDirection)
        {
            // Simplified refraction - rotate by prism angle
            float angle = currentRotation * Mathf.Deg2Rad;
            return Quaternion.Euler(0, 0, currentRotation) * incomingDirection;
        }

        public Color GetRefractedColor(Color incomingColor)
        {
            // Prisms can modify color (for color mixing puzzles)
            return incomingColor;
        }

        public void ShowHint()
        {
            showingHint = true;
            StartCoroutine(HintPulseCoroutine());
        }

        private IEnumerator HintPulseCoroutine()
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite == null) yield break;

            Color original = sprite.color;
            for (int i = 0; i < 3; i++)
            {
                sprite.color = Color.yellow;
                yield return new WaitForSeconds(0.3f);
                sprite.color = original;
                yield return new WaitForSeconds(0.3f);
            }
            showingHint = false;
        }

        public void ResetRotation()
        {
            Initialize();
        }
    }

    /// <summary>
    /// Represents a target that lights up when hit by a beam
    /// </summary>
    [System.Serializable]
    public class LightTarget : MonoBehaviour
    {
        [SerializeField] private Color requiredColor = Color.white; // Color needed to activate
        [SerializeField] private float colorTolerance = 0.2f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem glowEffect;

        private bool isLit = false;

        public bool IsLit => isLit;

        public void Initialize()
        {
            ResetHitState();
        }

        public void OnLightHit(Color beamColor)
        {
            // Check if color matches
            bool colorMatches = ColorMatches(beamColor, requiredColor);

            if (colorMatches && !isLit)
            {
                isLit = true;
                spriteRenderer.color = beamColor;

                if (glowEffect != null)
                    glowEffect.Play();
            }
        }

        public void ResetHitState()
        {
            isLit = false;
            spriteRenderer.color = Color.gray;
        }

        public void Reset()
        {
            ResetHitState();
        }

        private bool ColorMatches(Color a, Color b)
        {
            return Vector3.Distance(
                new Vector3(a.r, a.g, a.b),
                new Vector3(b.r, b.g, b.b)
            ) <= colorTolerance;
        }
    }

    #endregion
}
