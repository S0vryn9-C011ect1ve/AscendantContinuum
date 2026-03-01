using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AscendantContinuum.Core;

namespace AscendantContinuum.VFX
{
    /// <summary>
    /// Enhanced visual effects for realm transitions
    /// Includes camera zoom, screen shake, portal effects
    /// All respect accessibility settings (reduced motion)
    /// </summary>
    public class TransitionEffects : MonoBehaviour
    {
        private static TransitionEffects _instance;
        public static TransitionEffects Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("TransitionEffects");
                    _instance = obj.AddComponent<TransitionEffects>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        /// <summary>
        /// Smooth camera zoom during realm transition
        /// </summary>
        public IEnumerator CameraZoomTransition(Camera cam, float targetFOV, float duration)
        {
            if (cam == null) yield break;

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            if (reducedMotion)
            {
                // Skip animation, snap immediately
                cam.fieldOfView = targetFOV;
                yield break;
            }

            float startFOV = cam.fieldOfView;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, 
                    EaseInOutQuad(elapsed / duration));
                yield return null;
            }
            cam.fieldOfView = targetFOV;
        }

        /// <summary>
        /// Screen shake effect - creates dramatic impact on realm entry
        /// Intensity affects magnitude; ignored in reduced motion mode
        /// </summary>
        public IEnumerator ScreenShake(Camera cam, float duration, float intensity)
        {
            if (cam == null) yield break;

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            if (reducedMotion)
            {
                // Skip animation entirely
                yield break;
            }

            Vector3 originalPos = cam.transform.localPosition;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // Decay intensity over time (easing out)
                float currentIntensity = intensity * (1 - elapsed / duration);
                
                float shakeX = Random.Range(-currentIntensity, currentIntensity);
                float shakeY = Random.Range(-currentIntensity * 0.5f, currentIntensity * 0.5f);
                
                cam.transform.localPosition = originalPos + new Vector3(shakeX, shakeY, 0);
                yield return null;
            }
            cam.transform.localPosition = originalPos;
        }

        /// <summary>
        /// Spin camera effect - rotation around Z axis
        /// Creates portal-like visual
        /// </summary>
        public IEnumerator CameraSpinTransition(Camera cam, float duration, float totalRotation = 360f)
        {
            if (cam == null) yield break;

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            if (reducedMotion)
            {
                yield break;
            }

            Vector3 originalEuler = cam.transform.localEulerAngles;
            float targetZ = originalEuler.z + totalRotation;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newZ = Mathf.Lerp(originalEuler.z, targetZ, elapsed / duration);
                cam.transform.localEulerAngles = new Vector3(originalEuler.x, originalEuler.y, newZ);
                yield return null;
            }

            cam.transform.localEulerAngles = originalEuler;
        }

        /// <summary>
        /// Fade screen to/from a color
        /// Used at transition boundaries
        /// </summary>
        public IEnumerator ScreenFlash(Color fromColor, Color toColor, float duration, CanvasGroup canvasGroup)
        {
            if (canvasGroup == null) yield break;

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            if (reducedMotion)
            {
                // Instant snap
                canvasGroup.alpha = toColor.a;
                yield break;
            }

            Image screenImage = canvasGroup.GetComponent<Image>();
            if (screenImage == null) yield break;

            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                screenImage.color = Color.Lerp(fromColor, toColor, elapsed / duration);
                canvasGroup.alpha = screenImage.color.a;
                yield return null;
            }

            screenImage.color = toColor;
            canvasGroup.alpha = toColor.a;
        }

        /// <summary>
        /// Easing functions for smooth animations
        /// </summary>
        private static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        }

        private static float EaseOutQuad(float t)
        {
            return t * (2 - t);
        }

        private static float EaseInQuad(float t)
        {
            return t * t;
        }
    }
}
