using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Manages the First Time User Experience (FTUE).
    /// Highlights UI elements, pauses gameplay, and guides the player contextually.
    /// </summary>
    public class ContextualTutorialManager : MonoBehaviour
    {
        public static ContextualTutorialManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject tutorialOverlayPanel;
        [SerializeField] private Text tutorialText;
        [SerializeField] private Button continueButton;

        [Header("Highlight Settings")]
        [SerializeField] private RectTransform highlightMask; // A mask that cuts a hole in the dark overlay
        [SerializeField] private float highlightPadding = 20f;

        private bool isTutorialActive = false;
        private int currentStep = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(NextStep);
            }
        }

        /// <summary>
        /// Starts the contextual tutorial sequence.
        /// </summary>
        public void StartTutorial()
        {
            if (GameManager.Instance != null && !GameManager.Instance.ShouldRunOnboarding())
            {
                Debug.Log("[ContextualTutorial] Tutorial already completed. Skipping.");
                return;
            }

            isTutorialActive = true;
            currentStep = 0;
            tutorialOverlayPanel.SetActive(true);
            
            // Pause game logic (if applicable)
            Time.timeScale = 0f;

            ShowStep(currentStep);
        }

        private void ShowStep(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0:
                    tutorialText.text = "Welcome to the Ascendant Continuum.\n\nYour journey begins in the Emberforge.";
                    // Hide highlight mask for intro
                    highlightMask.gameObject.SetActive(false);
                    break;
                case 1:
                    tutorialText.text = "Tap the glowing sparks to collect them.\n\nThey are the lifeblood of this realm.";
                    // Example: Highlight a specific spark (requires finding it in the scene)
                    HighlightObjectWithTag("TutorialSpark");
                    break;
                case 2:
                    tutorialText.text = "Once you collect enough sparks, your personal Sigil will begin to form.";
                    highlightMask.gameObject.SetActive(false);
                    break;
                default:
                    EndTutorial();
                    break;
            }
        }

        private void HighlightObjectWithTag(string tag)
        {
            GameObject target = GameObject.FindGameObjectWithTag(tag);
            if (target != null)
            {
                highlightMask.gameObject.SetActive(true);
                
                // Convert world position to screen space
                Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
                highlightMask.position = screenPos;

                // Adjust size based on object bounds (simplified)
                highlightMask.sizeDelta = new Vector2(100f + highlightPadding, 100f + highlightPadding);
            }
            else
            {
                highlightMask.gameObject.SetActive(false);
            }
        }

        public void NextStep()
        {
            if (!isTutorialActive) return;

            currentStep++;
            ShowStep(currentStep);
        }

        public void EndTutorial()
        {
            isTutorialActive = false;
            tutorialOverlayPanel.SetActive(false);
            
            // Resume game logic
            Time.timeScale = 1f;

            // Mark onboarding as complete
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MarkOnboardingCompleted();
            }

            Debug.Log("[ContextualTutorial] Tutorial completed.");
        }

        public bool IsTutorialActive => isTutorialActive;
    }
}
