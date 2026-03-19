using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using AscendantContinuum.Core;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Celebration overlay shown when a realm goal is reached.
    /// Drop on the HUDCanvas — hidden until ShowCompletion() is called.
    /// </summary>
    public class RealmCompletionPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Image[] starImages;          // 3 stars
        [SerializeField] private ParticleSystem celebrationVFX;

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.6f;
        [SerializeField] private float starRevealDelay = 0.25f;

        private bool _shown = false;

        private void Awake()
        {
            gameObject.SetActive(false);

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgain);

            if (returnButton != null)
                returnButton.onClick.AddListener(OnReturnToMenu);
        }

        /// <summary>
        /// Call this when the player hits the realm goal.
        /// </summary>
        /// <param name="title">e.g. "Realm Complete!"</param>
        /// <param name="score">e.g. "20 Sparks Collected"</param>
        /// <param name="stars">1–3</param>
        public void ShowCompletion(string title, string score, int stars = 3)
        {
            if (_shown) return;
            _shown = true;

            Debug.Log("[RealmCompletionPanel] ShowCompletion called - activating panel");

            // Make sure panel is active BEFORE setting up UI
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            Debug.Log("[RealmCompletionPanel] Panel activated, setting UI text");

            if (titleText != null)    titleText.text    = title;
            if (scoreText != null)    scoreText.text    = score;
            if (subtitleText != null) subtitleText.text = "✦  Ascendant Continuum  ✦";

            // Hide all stars initially
            if (starImages != null && starImages.Length > 0)
            {
                foreach (var s in starImages)
                    if (s != null) s.color = new Color(1f, 1f, 1f, 0f);
            }

            Debug.Log("[RealmCompletionPanel] Starting animation coroutine");

            // Start animation on next frame to ensure GameObject is fully active
            StartCoroutine(AnimateInDelayed(stars));
        }

        private IEnumerator AnimateInDelayed(int stars)
        {
            Debug.Log("[RealmCompletionPanel] AnimateInDelayed - waiting one frame");
            // Wait one frame to ensure GameObject is fully activated
            yield return null;
            Debug.Log("[RealmCompletionPanel] Starting AnimateIn");
            yield return AnimateIn(stars);
            Debug.Log("[RealmCompletionPanel] Animation complete");
        }

        private IEnumerator AnimateIn(int stars)
        {
            // Fade in panel
            if (panelGroup != null)
            {
                panelGroup.alpha = 0f;
                panelGroup.interactable = false; // Disable clicks during animation
                float t = 0f;
                while (t < fadeInDuration)
                {
                    t += Time.deltaTime;
                    panelGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                    yield return null;
                }
                panelGroup.alpha = 1f;
                panelGroup.interactable = true; // Re-enable clicks
            }

            // Play VFX
            if (celebrationVFX != null)
                celebrationVFX.Play();

            // Haptic
            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Success);

            // Reveal stars one by one
            if (starImages != null && starImages.Length > 0)
            {
                int clampedStars = Mathf.Clamp(stars, 0, starImages.Length);
                for (int i = 0; i < clampedStars; i++)
                {
                    yield return new WaitForSeconds(starRevealDelay);
                    if (starImages[i] != null)
                    {
                        // Pop-in tween
                        float elapsed = 0f;
                        float dur = 0.2f;
                        while (elapsed < dur)
                        {
                            elapsed += Time.deltaTime;
                            float p = elapsed / dur;
                            float scale = Mathf.LerpUnclamped(0f, 1f,
                                p < 0.7f ? p / 0.7f * 1.2f : 1.2f - (p - 0.7f) / 0.3f * 0.2f);
                            starImages[i].transform.localScale = Vector3.one * scale;
                            starImages[i].color = new Color(1f, 0.88f, 0.2f, 1f);
                            yield return null;
                        }
                        starImages[i].transform.localScale = Vector3.one;
                    }
                }
            }
        }

        private void OnPlayAgain()
        {
            _shown = false;
            gameObject.SetActive(false);
        }

        private void OnReturnToMenu()
        {
            AudioManager.Instance?.StopMusic(0.5f);
            SceneService.TryLoadScene(SceneNames.MainMenu);
        }
    }
}
