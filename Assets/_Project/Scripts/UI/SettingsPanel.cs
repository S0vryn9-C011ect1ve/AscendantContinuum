using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AscendantContinuum.Core;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Settings panel — audio volumes, accessibility, and display options.
    /// Wires itself to AudioManager and AccessibilityManager automatically.
    /// Can be shown from MainMenu or from the in-realm Pause menu.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        // ── Serialized layout refs (populated by builder or manually) ──────────
        [Header("Panel Root")]
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private GameObject panelRoot;

        [Header("Audio Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider ambientVolumeSlider;

        [Header("Audio Labels")]
        [SerializeField] private TMP_Text masterValueLabel;
        [SerializeField] private TMP_Text musicValueLabel;
        [SerializeField] private TMP_Text sfxValueLabel;
        [SerializeField] private TMP_Text ambientValueLabel;

        [Header("Accessibility Toggles")]
        [SerializeField] private Toggle reducedMotionToggle;
        [SerializeField] private Toggle hapticsToggle;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Toggle screenReaderToggle;

        [Header("Colorblind Dropdown")]
        [SerializeField] private TMP_Dropdown colorblindDropdown;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        // Callback invoked when the panel closes (useful for pause menus)
        public event Action OnClosed;

        private bool _isShown;

        private void Awake()
        {
            if (panelGroup == null)
                panelGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

            // Start hidden
            SetVisible(false, instant: true);
        }

        private void Start()
        {
            WireListeners();
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void Show()
        {
            if (_isShown) return;
            _isShown = true;
            panelRoot?.SetActive(true);
            LoadCurrentValues();
            StartCoroutine(FadePanel(0f, 1f, 0.2f));
        }

        public void Hide()
        {
            if (!_isShown) return;
            _isShown = false;
            StartCoroutine(FadeAndDisable());
        }

        public void Toggle()
        {
            if (_isShown) Hide();
            else Show();
        }

        // ── Initialization ────────────────────────────────────────────────────

        private void WireListeners()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(v => {
                    AudioManager.Instance?.SetMasterVolume(v);
                    UpdateLabel(masterValueLabel, v);
                });

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(v => {
                    AudioManager.Instance?.SetMusicVolume(v);
                    UpdateLabel(musicValueLabel, v);
                });

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(v => {
                    AudioManager.Instance?.SetSFXVolume(v);
                    UpdateLabel(sfxValueLabel, v);
                });

            if (ambientVolumeSlider != null)
                ambientVolumeSlider.onValueChanged.AddListener(v => {
                    AudioManager.Instance?.SetAmbientVolume(v);
                    UpdateLabel(ambientValueLabel, v);
                });

            if (reducedMotionToggle != null)
                reducedMotionToggle.onValueChanged.AddListener(v =>
                    AccessibilityManager.Instance?.SetReducedMotion(v));

            if (hapticsToggle != null)
                hapticsToggle.onValueChanged.AddListener(v =>
                    AccessibilityManager.Instance?.SetHaptics(v));

            if (highContrastToggle != null)
                highContrastToggle.onValueChanged.AddListener(v =>
                    AccessibilityManager.Instance?.SetHighContrast(v));

            if (screenReaderToggle != null)
                screenReaderToggle.onValueChanged.AddListener(v =>
                    AccessibilityManager.Instance?.SetScreenReader(v));

            if (colorblindDropdown != null)
            {
                colorblindDropdown.ClearOptions();
                colorblindDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "None", "Protanopia (Red-Blind)", "Deuteranopia (Green-Blind)",
                    "Tritanopia (Blue-Blind)", "Achromatopsia (Mono)", "Protanomaly (Weak Red)"
                });
                colorblindDropdown.onValueChanged.AddListener(v =>
                    AccessibilityManager.Instance?.SetColorblindMode((ColorblindMode)v));
            }

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        /// <summary>Populate UI controls with current saved values.</summary>
        private void LoadCurrentValues()
        {
            var audio = AudioManager.Instance;
            if (audio != null)
            {
                SetSlider(masterVolumeSlider, audio.GetMasterVolume());
                SetSlider(musicVolumeSlider, audio.GetMusicVolume());
                SetSlider(sfxVolumeSlider, audio.GetSFXVolume());
                SetSlider(ambientVolumeSlider, audio.GetAmbienceVolume());

                UpdateLabel(masterValueLabel, audio.GetMasterVolume());
                UpdateLabel(musicValueLabel, audio.GetMusicVolume());
                UpdateLabel(sfxValueLabel, audio.GetSFXVolume());
                UpdateLabel(ambientValueLabel, audio.GetAmbienceVolume());
            }

            var access = AccessibilityManager.Instance;
            if (access != null)
            {
                SetToggle(reducedMotionToggle, access.ReducedMotionEnabled);
                SetToggle(hapticsToggle, access.HapticsEnabled);
                SetToggle(highContrastToggle, access.HighContrastMode);
                SetToggle(screenReaderToggle, access.ScreenReaderEnabled);
                SetDropdown(colorblindDropdown, (int)access.CurrentColorblindMode);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void SetSlider(Slider s, float value)
        {
            if (s == null) return;
            s.SetValueWithoutNotify(Mathf.Clamp01(value));
        }

        private static void SetToggle(Toggle t, bool value)
        {
            if (t == null) return;
            t.SetIsOnWithoutNotify(value);
        }

        private static void SetDropdown(TMP_Dropdown d, int value)
        {
            if (d == null) return;
            d.SetValueWithoutNotify(Mathf.Clamp(value, 0, d.options.Count - 1));
        }

        private static void UpdateLabel(TMP_Text label, float value)
        {
            if (label == null) return;
            label.text = Mathf.RoundToInt(value * 100f) + "%";
        }

        private void SetVisible(bool visible, bool instant = false)
        {
            if (panelGroup == null) return;
            panelGroup.alpha = visible ? 1f : 0f;
            panelGroup.interactable = visible;
            panelGroup.blocksRaycasts = visible;
            panelRoot?.SetActive(visible);
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
            panelGroup.interactable = (to > 0.5f);
            panelGroup.blocksRaycasts = (to > 0.5f);
        }

        private IEnumerator FadeAndDisable()
        {
            yield return FadePanel(1f, 0f, 0.2f);
            panelRoot?.SetActive(false);
            OnClosed?.Invoke();
        }
    }
}
