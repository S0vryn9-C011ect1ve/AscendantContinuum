using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TouchPhase = UnityEngine.TouchPhase;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Mobile-first touch input system with gesture recognition
    /// Supports accessibility features like larger touch targets and haptic feedback
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        public static TouchInputManager Instance { get; private set; }

        [Header("Touch Settings")]
        [SerializeField] private float tapThreshold = 0.2f; // Max duration for tap
        [SerializeField] private float swipeThreshold = 100f; // Min distance for swipe
        [SerializeField] private float doubleTapWindow = 0.3f;
        
        [Header("Accessibility")]
        [SerializeField] private float touchTargetMultiplier = 1f; // 1.0 - 2.0 for easier tapping
        
        private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
        private float lastTapTime = 0f;

        // Action speed tracking — rolling average of tap inter-arrival seconds
        private const int ActionSpeedSamples = 20;
        private readonly float[] _actionSpeedBuffer = new float[ActionSpeedSamples];
        private int _actionSpeedIndex = 0;
        private int _actionSpeedCount = 0;
        private float _lastActionTime = -1f;
        
        public System.Action<Vector2> OnTap;
        public System.Action<Vector2> OnDoubleTap;
        public System.Action<Vector2, Vector2> OnSwipe; // start, direction
        public System.Action<float> OnPinch; // scale factor

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

        private void Update()
        {
            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            #if UNITY_EDITOR || UNITY_STANDALONE
            // Mouse input for testing in editor
            HandleMouseInput();
            #else
            // Touch input for mobile
            HandleMobileTouchInput();
            #endif
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                ProcessTap(mousePos);
            }
        }

        private void HandleMobileTouchInput()
        {
            foreach (Touch touch in Input.touches)
            {
                int touchId = touch.fingerId;
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        activeTouches[touchId] = new TouchData
                        {
                            startPosition = touch.position,
                            startTime = Time.time
                        };
                        break;
                        
                    case TouchPhase.Moved:
                        if (activeTouches.ContainsKey(touchId))
                        {
                            activeTouches[touchId].currentPosition = touch.position;
                        }
                        break;
                        
                    case TouchPhase.Ended:
                        if (activeTouches.ContainsKey(touchId))
                        {
                            ProcessTouchEnd(activeTouches[touchId]);
                            activeTouches.Remove(touchId);
                        }
                        break;
                        
                    case TouchPhase.Canceled:
                        activeTouches.Remove(touchId);
                        break;
                }
            }
            
            // Pinch detection
            if (Input.touchCount == 2)
            {
                DetectPinch(Input.GetTouch(0), Input.GetTouch(1));
            }
        }

        private void ProcessTouchEnd(TouchData touchData)
        {
            float duration = Time.time - touchData.startTime;
            float distance = Vector2.Distance(touchData.startPosition, touchData.currentPosition);
            
            // Tap detection
            if (duration < tapThreshold && distance < 50f)
            {
                ProcessTap(touchData.startPosition);
            }
            // Swipe detection
            else if (distance > swipeThreshold)
            {
                Vector2 direction = (touchData.currentPosition - touchData.startPosition).normalized;
                OnSwipe?.Invoke(touchData.startPosition, direction);
                
                AccessibilityManager.Instance?.TriggerHaptic(HapticType.Light);
                Debug.Log($"[TouchInput] Swipe detected: {direction}");
            }
        }

        private void ProcessTap(Vector2 position)
        {
            // Record action speed (inter-tap interval in seconds)
            if (_lastActionTime > 0f)
            {
                float interval = Time.time - _lastActionTime;
                _actionSpeedBuffer[_actionSpeedIndex] = interval;
                _actionSpeedIndex = (_actionSpeedIndex + 1) % ActionSpeedSamples;
                _actionSpeedCount = Mathf.Min(_actionSpeedCount + 1, ActionSpeedSamples);

                // Update rolling average in PlayerPrefs (taps/second)
                float sum = 0f;
                for (int i = 0; i < _actionSpeedCount; i++)
                    sum += _actionSpeedBuffer[i];
                float avgInterval = sum / _actionSpeedCount;
                float tapsPerSecond = avgInterval > 0f ? 1f / avgInterval : 0f;
                PlayerPrefs.SetFloat("AverageActionSpeed", tapsPerSecond);
                PlayerPrefs.Save();
            }
            _lastActionTime = Time.time;

            // Double tap detection
            if (Time.time - lastTapTime < doubleTapWindow)
            {
                OnDoubleTap?.Invoke(position);
                AccessibilityManager.Instance?.TriggerHaptic(HapticType.Medium);
                Debug.Log("[TouchInput] Double tap detected");
                lastTapTime = 0f;
            }
            else
            {
                OnTap?.Invoke(position);
                AccessibilityManager.Instance?.TriggerHaptic(HapticType.Light);
                lastTapTime = Time.time;
            }
            
            // Raycast to detect tapped objects
            RaycastTappedObject(position);
        }

        private void RaycastTappedObject(Vector2 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);
            
            if (hit.collider != null)
            {
                // Check for spark collection
                var spark = hit.collider.GetComponent<Emberforge.Spark>();
                if (spark != null)
                {
                    spark.SendMessage("OnTouchCollected", SendMessageOptions.DontRequireReceiver);
                }
                
                Debug.Log($"[TouchInput] Tapped: {hit.collider.gameObject.name}");
            }
        }

        private void DetectPinch(Touch touch0, Touch touch1)
        {
            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            
            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;
            
            float scaleFactor = currentMagnitude / prevMagnitude;
            
            OnPinch?.Invoke(scaleFactor);
        }

        public void SetTouchTargetMultiplier(float multiplier)
        {
            touchTargetMultiplier = Mathf.Clamp(multiplier, 1f, 2f);
            Debug.Log($"[TouchInput] Touch target multiplier set to {touchTargetMultiplier}x");
        }
    }

    public class TouchData
    {
        public Vector2 startPosition;
        public Vector2 currentPosition;
        public float startTime;
    }
}
