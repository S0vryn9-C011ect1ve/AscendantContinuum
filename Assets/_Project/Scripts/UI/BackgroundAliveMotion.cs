using UnityEngine;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Alive background motion with positive uplighting feel.
    /// Layered sine drift, warm brightness pulse, and gentle upward float
    /// give backgrounds an ascending, living energy.
    /// </summary>
    public sealed class BackgroundAliveMotion : MonoBehaviour
    {
        [Header("Drift & Float")]
        [Tooltip("Horizontal sway amplitude (world units)")]
        [SerializeField] private float driftX = 0.25f;
        [Tooltip("Vertical sway amplitude (world units)")]
        [SerializeField] private float driftY = 0.18f;
        [Tooltip("Speed of the drift oscillation")]
        [SerializeField] private float driftSpeed = 0.22f;
        [Tooltip("Continuous upward float per second, resets seamlessly")]
        [SerializeField] private float upwardFloat = 0.06f;
        [Tooltip("Vertical distance before upward reset (should match or exceed 2x upwardFloat * cycle)")]
        [SerializeField] private float upwardResetRange = 0.5f;

        [Header("Breathing Scale")]
        [Tooltip("How much the background breathes in and out (fraction of base scale)")]
        [SerializeField] private float breatheAmount = 0.04f;
        [Tooltip("Speed of the breathing cycle")]
        [SerializeField] private float breatheSpeed = 0.28f;

        [Header("Uplighting Brightness Pulse")]
        [Tooltip("Enable warm brightness pulse on the SpriteRenderer")]
        [SerializeField] private bool enableBrightnessPulse = true;
        [Tooltip("Warm uplighting tint to pulse toward (white = pure brightness)")]
        [SerializeField] private Color uplightColor = new Color(1f, 0.97f, 0.85f, 1f);
        [Tooltip("How strongly the color pulses toward the uplight tint (0–1)")]
        [SerializeField] [Range(0f, 1f)] private float pulseStrength = 0.22f;
        [Tooltip("Speed of the brightness pulse cycle")]
        [SerializeField] private float pulseSpeed = 0.35f;

        [Header("Accessibility")]
        [SerializeField] private bool disableWhenReducedMotion = true;

        private Vector3 _baseLocalPosition;
        private Vector3 _baseLocalScale;
        private Color _baseColor;
        private float _phase;
        private float _upwardOffset;
        private SpriteRenderer _sprite;
        private UnityEngine.UI.Image _image;

        private void Awake()
        {
            _baseLocalPosition = transform.localPosition;
            _baseLocalScale = transform.localScale;
            _phase = Random.Range(0f, Mathf.PI * 2f);
            _upwardOffset = 0f;

            _sprite = GetComponent<SpriteRenderer>();
            _image = GetComponent<UnityEngine.UI.Image>();

            if (_sprite != null) _baseColor = _sprite.color;
            else if (_image != null) _baseColor = _image.color;
        }

        private void LateUpdate()
        {
            bool reduced = disableWhenReducedMotion &&
                           AscendantContinuum.Core.AccessibilityManager.Instance != null &&
                           AscendantContinuum.Core.AccessibilityManager.Instance.IsReducedMotionEnabled();

            if (reduced)
            {
                transform.localPosition = _baseLocalPosition;
                transform.localScale = _baseLocalScale;
                SetColor(_baseColor);
                return;
            }

            float t = Time.unscaledTime;

            // --- Layered drift: two sine waves offset for organic feel ---
            float x = (Mathf.Sin((t * driftSpeed) + _phase) * driftX)
                    + (Mathf.Sin((t * driftSpeed * 0.47f) + _phase + 1.1f) * driftX * 0.35f);

            float y = (Mathf.Cos((t * driftSpeed * 0.73f) + (_phase * 1.3f)) * driftY)
                    + (Mathf.Cos((t * driftSpeed * 0.31f) + _phase + 2.4f) * driftY * 0.4f);

            // --- Continuous upward float (ascending energy) ---
            _upwardOffset += upwardFloat * Time.unscaledDeltaTime;
            if (_upwardOffset > upwardResetRange)
                _upwardOffset -= upwardResetRange;          // seamless loop

            transform.localPosition = _baseLocalPosition + new Vector3(x, y + _upwardOffset, 0f);

            // --- Breathing scale ---
            float breathe = 1f + Mathf.Sin((t * breatheSpeed) + (_phase * 0.6f)) * breatheAmount;
            transform.localScale = _baseLocalScale * breathe;

            // --- Warm uplighting brightness pulse ---
            if (enableBrightnessPulse)
            {
                // Pulse rides a slow sine so it feels like light gently washing over the scene
                float pulse = (Mathf.Sin((t * pulseSpeed) + (_phase * 0.4f)) * 0.5f) + 0.5f; // 0..1
                Color lerpedColor = Color.Lerp(_baseColor, uplightColor, pulse * pulseStrength);
                SetColor(lerpedColor);
            }
        }

        private void SetColor(Color c)
        {
            if (_sprite != null) _sprite.color = c;
            else if (_image != null) _image.color = c;
        }
    }
}
