using UnityEngine;

namespace AscendantContinuum.Realms.EchoFields
{
    /// <summary>
    /// Represents an individual star in the constellation tracing ritual.
    /// Stars pulse gently and connect when touched in the correct sequence.
    /// </summary>
    public class Star : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color idleColor = new Color(1f, 1f, 1f, 0.7f);
        [SerializeField] private Color connectedColor = new Color(0.4f, 0.8f, 1f, 1f);
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.6f, 1f);
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float pulseAmount = 0.2f;
        
        [Header("Connection Visual")]
        [SerializeField] private GameObject glowEffect;
        [SerializeField] private ParticleSystem connectionParticles;
        
        [Header("Audio")]
        [SerializeField] private AudioClip touchSound;
        [SerializeField] private AudioClip connectionSound;
        
        // State
        private bool isConnected = false;
        private bool isHighlighted = false;
        private int starIndex;
        
        // Animation
        private float basePulseOffset;
        private Vector3 originalScale;
        
        // Accessibility
        private bool reducedMotion = false;
        
        // Events
        public System.Action<Star> OnStarTouched;
        
        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            originalScale = transform.localScale;
            basePulseOffset = Random.Range(0f, Mathf.PI * 2f);
        }
        
        private void Start()
        {
            // Check accessibility settings
            if (AccessibilityManager.Instance != null)
            {
                reducedMotion = AccessibilityManager.Instance.IsReducedMotionEnabled();
            }
            
            SetIdleState();
            
            if (glowEffect != null)
                glowEffect.SetActive(false);
        }
        
        private void Update()
        {
            if (!reducedMotion && !isConnected)
            {
                ApplyPulseAnimation();
            }
        }
        
        #region Initialization
        
        /// <summary>
        /// Initializes the star with its index in the constellation
        /// </summary>
        public void Initialize(int index)
        {
            starIndex = index;
            gameObject.name = $"Star_{index}";
        }
        
        #endregion
        
        #region State Management
        
        public void SetConnected()
        {
            if (isConnected) return;
            
            isConnected = true;
            spriteRenderer.color = connectedColor;
            
            if (glowEffect != null)
                glowEffect.SetActive(true);
            
            if (connectionParticles != null && !reducedMotion)
                connectionParticles.Play();
            
            // Play connection sound
            if (AudioManager.Instance != null && connectionSound != null)
                AudioManager.Instance.PlaySFX(connectionSound, transform.position);
            
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Light);
            
            // Scale up slightly
            if (!reducedMotion)
                transform.localScale = originalScale * 1.2f;
        }
        
        public void SetHighlighted(bool highlighted)
        {
            isHighlighted = highlighted;
            
            if (!isConnected)
            {
                spriteRenderer.color = highlighted ? highlightColor : idleColor;
            }
        }
        
        public void SetIdleState()
        {
            isConnected = false;
            isHighlighted = false;
            spriteRenderer.color = idleColor;
            transform.localScale = originalScale;
            
            if (glowEffect != null)
                glowEffect.SetActive(false);
        }
        
        public void Reset()
        {
            SetIdleState();
        }
        
        #endregion
        
        #region Animation
        
        private void ApplyPulseAnimation()
        {
            // Gentle pulsing glow
            float pulse = Mathf.Sin((Time.time * pulseSpeed) + basePulseOffset) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(idleColor.a - pulseAmount, idleColor.a + pulseAmount, pulse);
            
            Color currentColor = isHighlighted ? highlightColor : idleColor;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            
            // Subtle scale pulse
            float scale = Mathf.Lerp(0.95f, 1.05f, pulse);
            transform.localScale = originalScale * scale;
        }
        
        #endregion
        
        #region Touch Interaction
        
        private void OnMouseDown()
        {
            HandleTouch();
        }
        
        private void HandleTouch()
        {
            // Play touch sound
            if (AudioManager.Instance != null && touchSound != null)
                AudioManager.Instance.PlaySFX(touchSound, transform.position);
            
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Selection);
            
            // Notify constellation tracer
            OnStarTouched?.Invoke(this);
        }
        
        /// <summary>
        /// Called by touch input manager for mobile
        /// </summary>
        public void OnTouched()
        {
            HandleTouch();
        }
        
        #endregion
        
        #region Public Properties
        
        public int StarIndex => starIndex;
        public bool IsConnected => isConnected;
        public Vector3 Position => transform.position;
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            OnStarTouched = null;
        }
        
        #endregion
    }
}
