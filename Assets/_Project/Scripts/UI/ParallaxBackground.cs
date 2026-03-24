using UnityEngine;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Creates parallax depth effect by drifting sprite layers at different speeds.
    /// Each child sprite drifts horizontally and vertically at its own rate.
    /// Syncs with BackgroundAliveMotion for cohesive alive feel.
    /// </summary>
    public sealed class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax Configuration")]
        [Tooltip("Speed multiplier for Layer 1 (front). Scale 0-1 (0.5 = half speed of main motion)")]
        [SerializeField] [Range(0f, 1f)] private float layer1Speed = 0.8f;
        [Tooltip("Speed multiplier for Layer 2 (mid). Slower = more depth")]
        [SerializeField] [Range(0f, 1f)] private float layer2Speed = 0.5f;
        [Tooltip("Speed multiplier for Layer 3 (back). Slowest = deepest")]
        [SerializeField] [Range(0f, 1f)] private float layer3Speed = 0.25f;

        [Header("Individual Layer Drift")]
        [Tooltip("Horizontal drift amount per layer (world units)")]
        [SerializeField] private float driftX = 0.15f;
        [Tooltip("Vertical drift amount per layer (world units)")]
        [SerializeField] private float driftY = 0.1f;
        [Tooltip("Speed of drift oscillation")]
        [SerializeField] private float driftSpeed = 0.15f;

        [Header("Accessibility")]
        [SerializeField] private bool disableWhenReducedMotion = true;

        private SpriteRenderer[] _layers;
        private Vector3[] _basePositions;
        private float _phase;

        private void Awake()
        {
            _layers = GetComponentsInChildren<SpriteRenderer>();
            _basePositions = new Vector3[_layers.Length];

            for (int i = 0; i < _layers.Length; i++)
            {
                _basePositions[i] = _layers[i].transform.localPosition;
            }

            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void LateUpdate()
        {
            bool reduced = disableWhenReducedMotion &&
                           AscendantContinuum.Core.AccessibilityManager.Instance != null &&
                           AscendantContinuum.Core.AccessibilityManager.Instance.IsReducedMotionEnabled();

            if (reduced)
            {
                for (int i = 0; i < _layers.Length; i++)
                {
                    _layers[i].transform.localPosition = _basePositions[i];
                }
                return;
            }

            float t = Time.unscaledTime;
            float[] speeds = { layer1Speed, layer2Speed, layer3Speed };

            for (int i = 0; i < _layers.Length && i < speeds.Length; i++)
            {
                if (_layers[i] == null) continue;

                float speed = speeds[i];

                // Layered drift with phase offset per layer for organic feel
                float x = (Mathf.Sin((t * driftSpeed * speed) + _phase) * driftX)
                        + (Mathf.Sin((t * driftSpeed * speed * 0.6f) + _phase + 1.5f) * driftX * 0.4f);

                float y = (Mathf.Cos((t * driftSpeed * speed * 0.7f) + (_phase * 1.2f)) * driftY)
                        + (Mathf.Cos((t * driftSpeed * speed * 0.4f) + _phase + 2.8f) * driftY * 0.5f);

                Vector3 newPos = _basePositions[i] + new Vector3(x, y, 0f);
                _layers[i].transform.localPosition = newPos;
            }
        }
    }
}
