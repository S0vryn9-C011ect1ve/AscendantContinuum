using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// "Juice" effect for buttons - scale animations on hover and press
    /// Creates satisfying tactile feedback
    /// Respects accessibility settings (no animations in reduced motion)
    /// </summary>
    public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scale Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float animDuration = 0.1f;

        [Header("Color Settings")]
        [SerializeField] private bool colorTintOnHover = true;
        [SerializeField] private Color hoverColorTint = new Color(1.1f, 1.1f, 1.1f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        [Header("Haptics")]
        [SerializeField] private bool hapticOnHover = true;
        [SerializeField] private bool hapticOnClick = true;

        private Vector3 originalScale;
        private Color originalColor;
        private Image buttonImage;
        private Button button;
        private bool isPointerOver = false;

        private Coroutine currentScaleCoroutine;
        private Coroutine currentColorCoroutine;

        private void Start()
        {
            originalScale = transform.localScale;
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            if (buttonImage != null)
                originalColor = buttonImage.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isPointerOver = true;

            // Stop any ongoing animations
            if (currentScaleCoroutine != null)
                StopCoroutine(currentScaleCoroutine);

            // Play hover sound
            if (hoverSound != null)
                AudioManager.Instance?.PlaySFX(hoverSound, 0.5f);

            // Haptic feedback
            if (hapticOnHover)
                HapticFeedback.PlayPattern(HapticFeedbackPattern.LightTap);

            // Scale animation
            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            if (reducedMotion)
            {
                transform.localScale = originalScale * hoverScale;
            }
            else
            {
                currentScaleCoroutine = StartCoroutine(ScaleTo(originalScale * hoverScale, animDuration));
            }

            // Color tint
            if (colorTintOnHover && buttonImage != null)
            {
                if (currentColorCoroutine != null)
                    StopCoroutine(currentColorCoroutine);

                if (reducedMotion)
                {
                    buttonImage.color = originalColor * hoverColorTint;
                }
                else
                {
                    currentColorCoroutine = StartCoroutine(ColorTo(originalColor * hoverColorTint, animDuration));
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;

            if (currentScaleCoroutine != null)
                StopCoroutine(currentScaleCoroutine);

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            if (reducedMotion)
            {
                transform.localScale = originalScale;
            }
            else
            {
                currentScaleCoroutine = StartCoroutine(ScaleTo(originalScale, animDuration));
            }

            // Reset color
            if (colorTintOnHover && buttonImage != null)
            {
                if (currentColorCoroutine != null)
                    StopCoroutine(currentColorCoroutine);

                if (reducedMotion)
                {
                    buttonImage.color = originalColor;
                }
                else
                {
                    currentColorCoroutine = StartCoroutine(ColorTo(originalColor, animDuration));
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;

            if (currentScaleCoroutine != null)
                StopCoroutine(currentScaleCoroutine);

            // Play click sound
            if (clickSound != null)
                AudioManager.Instance?.PlaySFX(clickSound, 0.7f);

            // Haptic feedback
            if (hapticOnClick)
                HapticFeedback.PlayPattern(HapticFeedbackPattern.Click);

            // Scale down
            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            if (reducedMotion)
            {
                transform.localScale = originalScale * pressScale;
            }
            else
            {
                currentScaleCoroutine = StartCoroutine(ScaleTo(originalScale * pressScale, animDuration * 0.5f));
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!button.interactable) return;

            if (currentScaleCoroutine != null)
                StopCoroutine(currentScaleCoroutine);

            // Return to hover scale if still over button
            Vector3 targetScale = isPointerOver ? originalScale * hoverScale : originalScale;

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            if (reducedMotion)
            {
                transform.localScale = targetScale;
            }
            else
            {
                currentScaleCoroutine = StartCoroutine(ScaleTo(targetScale, animDuration));
            }
        }

        private IEnumerator ScaleTo(Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, targetScale, EaseOutQuad(elapsed / duration));
                yield return null;
            }

            transform.localScale = targetScale;
        }

        private IEnumerator ColorTo(Color targetColor, float duration)
        {
            if (buttonImage == null) yield break;

            Color startColor = buttonImage.color;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                buttonImage.color = Color.Lerp(startColor, targetColor, EaseOutQuad(elapsed / duration));
                yield return null;
            }

            buttonImage.color = targetColor;
        }

        private static float EaseOutQuad(float t)
        {
            return t * (2 - t);
        }
    }
}
