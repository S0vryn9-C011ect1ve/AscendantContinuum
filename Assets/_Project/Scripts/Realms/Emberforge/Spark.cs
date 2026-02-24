using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Emberforge
{
    /// <summary>
    /// Individual spark behavior - movement, animation, touch detection
    /// Optimized for mobile touch input with accessibility features
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Spark : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float floatAmplitude = 0.5f;
        
        [Header("Visual")]
        [SerializeField] private Color sparkColor = new Color(1f, 0.6f, 0.2f);
        [SerializeField] private float pulseSpeed = 2f;
        
        private Vector3 startPosition;
        private float timeOffset;
        private SpriteRenderer spriteRenderer;
        private EmberforgeSparks manager;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            timeOffset = Random.Range(0f, 10f); // Randomize animation
        }

        private void OnEnable()
        {
            startPosition = transform.position;
            
            if (manager == null)
            {
                manager = GetComponentInParent<EmberforgeSparks>();
            }
        }

        private void Update()
        {
            AnimateSpark();
        }

        private void AnimateSpark()
        {
            // Skip animation if reduced motion is enabled
            if (AccessibilityManager.Instance?.ReducedMotionEnabled == true)
            {
                // Static glow only
                spriteRenderer.color = sparkColor;
                return;
            }
            
            // Floating motion
            float wave = Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatAmplitude;
            transform.position = startPosition + Vector3.up * wave;
            
            // Pulsing glow
            float pulse = (Mathf.Sin(Time.time * pulseSpeed + timeOffset) + 1f) / 2f; // 0-1 range
            spriteRenderer.color = Color.Lerp(sparkColor * 0.5f, sparkColor, pulse);
        }

        private void OnMouseDown()
        {
            CollectSpark();
        }

        private void OnTouchCollected()
        {
            CollectSpark();
        }

        private void CollectSpark()
        {
            manager?.CollectSpark(gameObject);
        }

        // For mobile touch input
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Touch"))
            {
                CollectSpark();
            }
        }
    }
}
