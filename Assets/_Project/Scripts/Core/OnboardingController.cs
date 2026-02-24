using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Drives the first-run onboarding / tutorial flow.
    ///
    /// Shows a series of UI panels that introduce each mechanic and
    /// accessibility option. Skipping is always available so players
    /// are never blocked. Accessibility settings toggled during
    /// onboarding are applied immediately and persisted to the save file.
    ///
    /// On completion it calls <see cref="GameManager.MarkOnboardingCompleted"/>
    /// and transitions to the MainMenu.
    /// </summary>
    public sealed class OnboardingController : MonoBehaviour
    {
        // ── Inspector refs ────────────────────────────────────────────────
        [Header("Panel References")]
        [SerializeField] private GameObject[] panels;           // ordered onboarding panels
        [SerializeField] private Button       nextButton;
        [SerializeField] private Button       skipButton;
        [SerializeField] private TMP_Text     stepLabel;        // "Step 1 / 6"

        [Header("Accessibility Panel")]
        [SerializeField] private Toggle colorblindToggle;
        [SerializeField] private Toggle reducedMotionToggle;
        [SerializeField] private Toggle hapticsToggle;

        [Header("Timing")]
        [SerializeField] private float panelFadeDuration = 0.4f;

        // ── State ──────────────────────────────────────────────────────────
        private int   _currentPanel;
        private bool  _transitioning;

        // ── Unity lifecycle ────────────────────────────────────────────────
        private void Awake()
        {
            // Accessibility check: if reduced motion is on, skip transitions
            if (AccessibilityManager.Instance != null &&
                AccessibilityManager.Instance.IsReducedMotionEnabled())
            {
                panelFadeDuration = 0f;
            }
        }

        private void Start()
        {
            // Guard: if player has already run onboarding, skip to main menu
            if (GameManager.Instance != null && !GameManager.Instance.ShouldRunOnboarding())
            {
                CompleteOnboarding();
                return;
            }

            SetupButtons();
            SetupAccessibilityToggles();
            ShowPanel(0, instant: true);
        }

        // ── Setup ──────────────────────────────────────────────────────────
        private void SetupButtons()
        {
            nextButton?.onClick.AddListener(AdvancePanel);
            skipButton?.onClick.AddListener(CompleteOnboarding);
        }

        private void SetupAccessibilityToggles()
        {
            if (AccessibilityManager.Instance == null) return;

            if (colorblindToggle != null)
            {
                colorblindToggle.isOn = AccessibilityManager.Instance.CurrentColorblindMode != ColorblindMode.None;
                colorblindToggle.onValueChanged.AddListener(on =>
                {
                    AccessibilityManager.Instance.SetColorblindMode(
                        on ? ColorblindMode.Deuteranopia
                           : ColorblindMode.None);
                });
            }

            if (reducedMotionToggle != null)
            {
                reducedMotionToggle.isOn = AccessibilityManager.Instance.IsReducedMotionEnabled();
                reducedMotionToggle.onValueChanged.AddListener(on =>
                    AccessibilityManager.Instance.SetReducedMotion(on));
            }

            if (hapticsToggle != null)
            {
                hapticsToggle.isOn = AccessibilityManager.Instance.HapticsEnabled;
                hapticsToggle.onValueChanged.AddListener(on =>
                    AccessibilityManager.Instance.SetHaptics(on));
            }
        }

        // ── Navigation ─────────────────────────────────────────────────────
        private void AdvancePanel()
        {
            if (_transitioning) return;

            int next = _currentPanel + 1;

            if (next >= panels.Length)
            {
                CompleteOnboarding();
                return;
            }

            ShowPanel(next);
        }

        private void ShowPanel(int index, bool instant = false)
        {
            if (panels == null || panels.Length == 0) return;

            StartCoroutine(TransitionToPanel(index, instant));
        }

        private IEnumerator TransitionToPanel(int index, bool instant)
        {
            _transitioning = true;

            // Hide current
            if (!instant && _currentPanel < panels.Length)
            {
                yield return StartCoroutine(FadePanel(panels[_currentPanel], fadeIn: false));
                panels[_currentPanel].SetActive(false);
            }
            else if (_currentPanel < panels.Length)
            {
                panels[_currentPanel].SetActive(false);
            }

            _currentPanel = index;

            // Show next
            panels[_currentPanel].SetActive(true);

            if (!instant)
                yield return StartCoroutine(FadePanel(panels[_currentPanel], fadeIn: true));

            UpdateStepLabel();
            UpdateNextButtonLabel();

            _transitioning = false;
        }

        private IEnumerator FadePanel(GameObject panel, bool fadeIn)
        {
            if (panelFadeDuration <= 0f) yield break;

            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            float start   = fadeIn ? 0f : 1f;
            float end     = fadeIn ? 1f : 0f;

            cg.alpha = start;

            while (elapsed < panelFadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(start, end, elapsed / panelFadeDuration);
                yield return null;
            }

            cg.alpha = end;
        }

        private void UpdateStepLabel()
        {
            if (stepLabel != null)
                stepLabel.text = $"Step {_currentPanel + 1} / {panels?.Length ?? 1}";
        }

        private void UpdateNextButtonLabel()
        {
            if (nextButton == null) return;
            var label = nextButton.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = (_currentPanel >= (panels?.Length ?? 1) - 1) ? "Begin Journey" : "Next";
        }

        // ── Completion ─────────────────────────────────────────────────────
        private void CompleteOnboarding()
        {
            // Persist accessibility choices chosen during onboarding
            SaveSystem.Instance?.SaveGame();

            // Mark onboarding done so it doesn't run again
            GameManager.Instance?.MarkOnboardingCompleted();

            Debug.Log("[OnboardingController] Onboarding complete — loading MainMenu.");
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
