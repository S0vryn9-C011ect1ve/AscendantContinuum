using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using AscendantContinuum.Core;
using AscendantContinuum.VFX;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Celebration effects when achievements or rewards are unlocked
    /// Scales up, bounces, spins, and pulses with visual joy!
    /// All animations respect reduced motion mode
    /// </summary>
    public class AchievementCelebration : MonoBehaviour
    {
        /// <summary>
        /// Celebrate a sigil unlock with scale-up, bounce, spin, and pulse
        /// </summary>
        public static IEnumerator PlaySigilUnlockCelebration(Image sigilImage, TextMeshProUGUI titleText, AudioClip unlockSound = null)
        {
            if (sigilImage == null) yield break;

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;

            if (!reducedMotion)
            {
                // PHASE 1: Scale up from tiny
                sigilImage.transform.localScale = Vector3.one * 0.1f;
                float elapsed = 0;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    sigilImage.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 1.2f, EaseOutQuad(elapsed / 0.5f));
                    yield return null;
                }

                // PHASE 2: Bounce back to normal
                elapsed = 0;
                while (elapsed < 0.3f)
                {
                    elapsed += Time.deltaTime;
                    sigilImage.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, EaseInQuad(elapsed / 0.3f));
                    yield return null;
                }

                // PHASE 3: Spin celebration (1 full rotation)
                elapsed = 0;
                while (elapsed < 1f)
                {
                    elapsed += Time.deltaTime;
                    sigilImage.transform.Rotate(0, 0, 360 * Time.deltaTime);
                    yield return null;
                }

                // PHASE 4: Glow pulse 3x
                CanvasGroup cg = sigilImage.GetComponent<CanvasGroup>();
                if (cg == null) cg = sigilImage.gameObject.AddComponent<CanvasGroup>();

                for (int i = 0; i < 3; i++)
                {
                    // Fade out
                    elapsed = 0;
                    while (elapsed < 0.3f)
                    {
                        elapsed += Time.deltaTime;
                        cg.alpha = Mathf.Lerp(1f, 0.6f, elapsed / 0.3f);
                        yield return null;
                    }

                    // Fade in
                    elapsed = 0;
                    while (elapsed < 0.3f)
                    {
                        elapsed += Time.deltaTime;
                        cg.alpha = Mathf.Lerp(0.6f, 1f, elapsed / 0.3f);
                        yield return null;
                    }
                }

                cg.alpha = 1f;
            }
            else
            {
                // Reduced motion: Just scale to normal and done
                sigilImage.transform.localScale = Vector3.one;
                CanvasGroup cg = sigilImage.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }

            // Play sound and haptic
            if (unlockSound != null)
                AudioManager.Instance?.PlaySFX(unlockSound, 0.8f);

            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Success);

            // Pop text animation
            if (titleText != null)
                yield return PlayTextPopAnimation(titleText, "✓ New Sigil!");
        }

        /// <summary>
        /// Celebrate any achievement unlock with popup fanfare
        /// </summary>
        public static IEnumerator PlayAchievementUnlockCelebration(
            Image achievementIcon,
            TextMeshProUGUI titleText,
            AudioClip fanfareSound = null,
            ParticleSystem unlockParticles = null)
        {
            if (achievementIcon == null) yield break;

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;

            if (!reducedMotion)
            {
                // Scale pop
                achievementIcon.transform.localScale = Vector3.zero;
                float elapsed = 0;
                while (elapsed < 0.4f)
                {
                    elapsed += Time.deltaTime;
                    achievementIcon.transform.localScale = Vector3.one * EaseOutQuad(elapsed / 0.4f);
                    yield return null;
                }

                // Play particles
                if (unlockParticles != null)
                {
                    unlockParticles.Play();
                }

                // Hold and glow
                CanvasGroup cg = achievementIcon.GetComponent<CanvasGroup>();
                if (cg == null) cg = achievementIcon.gameObject.AddComponent<CanvasGroup>();

                for (int i = 0; i < 2; i++)
                {
                    elapsed = 0;
                    while (elapsed < 0.25f)
                    {
                        elapsed += Time.deltaTime;
                        cg.alpha = Mathf.Lerp(1f, 0.7f, elapsed / 0.25f);
                        yield return null;
                    }

                    elapsed = 0;
                    while (elapsed < 0.25f)
                    {
                        elapsed += Time.deltaTime;
                        cg.alpha = Mathf.Lerp(0.7f, 1f, elapsed / 0.25f);
                        yield return null;
                    }
                }

                cg.alpha = 1f;
            }
            else
            {
                achievementIcon.transform.localScale = Vector3.one;
            }

            // Sound and haptics
            if (fanfareSound != null)
                AudioManager.Instance?.PlaySFX(fanfareSound, 1f);

            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Success);

            // Text pop
            if (titleText != null)
                yield return PlayTextPopAnimation(titleText, "🎉 Achievement Unlocked!");
        }

        /// <summary>
        /// Simple text popup that animates upward and fades
        /// </summary>
        public static IEnumerator PlayTextPopAnimation(TextMeshProUGUI textElement, string popText)
        {
            if (textElement == null) yield break;

            string originalText = textElement.text;
            textElement.text = popText;

            CanvasGroup cg = textElement.GetComponent<CanvasGroup>();
            if (cg == null) cg = textElement.gameObject.AddComponent<CanvasGroup>();

            RectTransform rt = textElement.GetComponent<RectTransform>();
            Vector3 startPos = rt.localPosition;

            bool reducedMotion = AccessibilityManager.Instance?.ReducedMotionEnabled == true;
            float duration = reducedMotion ? 0.5f : 1f;

            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // Move up
                rt.localPosition = startPos + Vector3.up * Mathf.Lerp(0, 100, EaseOutQuad(elapsed / duration));
                
                // Fade out
                cg.alpha = Mathf.Lerp(1f, 0f, EaseOutQuad(elapsed / duration));
                
                yield return null;
            }

            rt.localPosition = startPos + Vector3.up * 100;
            cg.alpha = 0f;
            textElement.text = originalText;
            cg.alpha = 1f;
            rt.localPosition = startPos;
        }

        /// <summary>
        /// Burst effect - particles explode from center
        /// </summary>
        public static IEnumerator PlayBurstEffect(Vector3 worldPosition, Color burstColor, float scale = 1f)
        {
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayRealmTransitionEffect(worldPosition, burstColor);
            }

            yield return null;
        }

        /// <summary>
        /// Easing functions
        /// </summary>
        private static float EaseOutQuad(float t)
        {
            return t * (2 - t);
        }

        private static float EaseInQuad(float t)
        {
            return t * t;
        }

        private static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        }
    }
}
