using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AscendantContinuum.Core;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// In-realm pause menu — shown when the player taps the pause button.
    /// Pauses time, offers Resume / Settings / Return to Main Menu.
    /// Automatically wires SettingsPanel if one is found in the scene.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Pause Panel")]
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private GameObject  panelRoot;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Settings")]
        [SerializeField] private SettingsPanel settingsPanel;

        private bool _isPaused;

        private void Awake()
        {
            if (panelGroup == null)
                panelGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            SetVisible(false, instant: true);
        }

        private void Start()
        {
            if (resumeButton   != null) resumeButton.onClick.AddListener(Resume);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);

            // Auto-find SettingsPanel in scene if not assigned
            if (settingsPanel == null)
                settingsPanel = FindFirstObjectByType<SettingsPanel>();
        }

        // ── Public API ────────────────────────────────────────────────────────

        public bool IsPaused => _isPaused;

        public void Pause()
        {
            if (_isPaused) return;
            _isPaused = true;
            Time.timeScale = 0f;
            panelRoot?.SetActive(true);
            StartCoroutine(FadePanel(0f, 1f, 0.2f));
        }

        public void Resume()
        {
            if (!_isPaused) return;
            _isPaused = false;
            Time.timeScale = 1f;
            StartCoroutine(FadeAndDisable());
        }

        public void TogglePause()
        {
            if (_isPaused) Resume();
            else Pause();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.Show();
                settingsPanel.OnClosed += OnSettingsClosed;
                // Hide pause backdrop while settings is open (optional UX choice)
                SetVisible(false, instant: true);
            }
        }

        private void OnSettingsClosed()
        {
            if (settingsPanel != null)
                settingsPanel.OnClosed -= OnSettingsClosed;

            // Show pause panel again once settings close
            panelRoot?.SetActive(true);
            SetVisible(true, instant: true);
        }

        private void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            _isPaused = false;

            if (AudioManager.Instance != null)
                AudioManager.Instance.StopMusic(0.5f);

            if (!SceneService.TryLoadScene(SceneNames.MainMenu))
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        private void SetVisible(bool visible, bool instant = false)
        {
            if (panelGroup == null) return;
            panelGroup.alpha = visible ? 1f : 0f;
            panelGroup.interactable = visible;
            panelGroup.blocksRaycasts = visible;
            if (!visible && instant) panelRoot?.SetActive(false);
        }

        private IEnumerator FadePanel(float from, float to, float duration)
        {
            if (panelGroup == null) yield break;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;

            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                panelGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            panelGroup.alpha = to;
            bool shown = to > 0.5f;
            panelGroup.interactable = shown;
            panelGroup.blocksRaycasts = shown;
        }

        private IEnumerator FadeAndDisable()
        {
            yield return FadePanel(1f, 0f, 0.2f);
            panelRoot?.SetActive(false);
        }
    }
}
