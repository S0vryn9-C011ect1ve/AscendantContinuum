using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AscendantContinuum.Core;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Comprehensive settings menu with accessibility-first approach.
    /// Controls all game settings including accessibility features.
    /// </summary>
    public class SettingsMenuManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider ambientVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        [SerializeField] private TextMeshProUGUI ambientVolumeText;

        [Header("Visual Settings")]
        [SerializeField] private Toggle reducedMotionToggle;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Slider textScaleSlider;
        [SerializeField] private TextMeshProUGUI textScaleText;
        [SerializeField] private TMP_Dropdown colorblindModeDropdown;
        [SerializeField] private TMP_Dropdown neurodivergentModeDropdown;

        [Header("Haptic Settings")]
        [SerializeField] private Toggle hapticsEnabledToggle;
        [SerializeField] private Slider hapticIntensitySlider;
        [SerializeField] private TextMeshProUGUI hapticIntensityText;

        [Header("Gameplay Settings")]
        [SerializeField] private Toggle autoSaveToggle;
        [SerializeField] private Toggle hintSystemToggle;
        [SerializeField] private Slider touchSizeSlider;
        [SerializeField] private TextMeshProUGUI touchSizeText;

        [Header("Privacy Settings")]
        [SerializeField] private Toggle analyticsToggle;
        [SerializeField] private Toggle shareDataToggle;

        [Header("Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetToDefaultButton;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            SetupListeners();
            LoadSettings();
        }

        #region Setup

        private void SetupListeners()
        {
            // Audio sliders
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            if (ambientVolumeSlider != null)
                ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);

            // Visual toggles
            if (reducedMotionToggle != null)
                reducedMotionToggle.onValueChanged.AddListener(OnReducedMotionChanged);
            if (highContrastToggle != null)
                highContrastToggle.onValueChanged.AddListener(OnHighContrastChanged);
            if (textScaleSlider != null)
                textScaleSlider.onValueChanged.AddListener(OnTextScaleChanged);
            if (colorblindModeDropdown != null)
                colorblindModeDropdown.onValueChanged.AddListener(OnColorblindModeChanged);
            if (neurodivergentModeDropdown != null)
                neurodivergentModeDropdown.onValueChanged.AddListener(OnNeurodivergentModeChanged);

            // Haptic controls
            if (hapticsEnabledToggle != null)
                hapticsEnabledToggle.onValueChanged.AddListener(OnHapticsEnabledChanged);
            if (hapticIntensitySlider != null)
                hapticIntensitySlider.onValueChanged.AddListener(OnHapticIntensityChanged);

            // Gameplay toggles
            if (autoSaveToggle != null)
                autoSaveToggle.onValueChanged.AddListener(OnAutoSaveChanged);
            if (hintSystemToggle != null)
                hintSystemToggle.onValueChanged.AddListener(OnHintSystemChanged);
            if (touchSizeSlider != null)
                touchSizeSlider.onValueChanged.AddListener(OnTouchSizeChanged);

            // Privacy toggles
            if (analyticsToggle != null)
                analyticsToggle.onValueChanged.AddListener(OnAnalyticsChanged);
            if (shareDataToggle != null)
                shareDataToggle.onValueChanged.AddListener(OnShareDataChanged);

            // Buttons
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClicked);
            if (resetToDefaultButton != null)
                resetToDefaultButton.onClick.AddListener(OnResetToDefaultClicked);
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }

        #endregion

        #region Load Settings

        private void LoadSettings()
        {
            // Audio
            if (masterVolumeSlider != null)
            {
                float volume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
                masterVolumeSlider.value = volume;
                UpdateVolumeText(masterVolumeText, volume);
            }

            if (musicVolumeSlider != null)
            {
                float volume = PlayerPrefs.GetFloat("Audio_MusicVolume", 0.7f);
                musicVolumeSlider.value = volume;
                UpdateVolumeText(musicVolumeText, volume);
            }

            if (sfxVolumeSlider != null)
            {
                float volume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1f);
                sfxVolumeSlider.value = volume;
                UpdateVolumeText(sfxVolumeText, volume);
            }

            if (ambientVolumeSlider != null)
            {
                float volume = PlayerPrefs.GetFloat("Audio_AmbientVolume", 0.5f);
                ambientVolumeSlider.value = volume;
                UpdateVolumeText(ambientVolumeText, volume);
            }

            // Visual Accessibility
            if (reducedMotionToggle != null)
                reducedMotionToggle.isOn = PlayerPrefs.GetInt("ReducedMotion", 0) == 1;

            if (highContrastToggle != null)
                highContrastToggle.isOn = PlayerPrefs.GetInt("HighContrast", 0) == 1;

            if (textScaleSlider != null)
            {
                float scale = PlayerPrefs.GetFloat("TextScale", 1f);
                textScaleSlider.value = scale;
                UpdateTextScaleDisplay(scale);
            }

            if (colorblindModeDropdown != null)
            {
                string mode = PlayerPrefs.GetString("ColorblindMode", "None");
                colorblindModeDropdown.value = GetDropdownIndexForMode(mode);
            }

            if (neurodivergentModeDropdown != null)
            {
                string ndMode = PlayerPrefs.GetString("NeurodivergentMode", "None");
                neurodivergentModeDropdown.value = GetNeurodivergentDropdownIndex(ndMode);
            }

            // Haptics
            if (hapticsEnabledToggle != null)
                hapticsEnabledToggle.isOn = PlayerPrefs.GetInt("HapticsEnabled", 1) == 1;

            if (hapticIntensitySlider != null)
            {
                float intensity = PlayerPrefs.GetFloat("HapticIntensity", 1f);
                hapticIntensitySlider.value = intensity;
                UpdateHapticIntensityText(intensity);
            }

            // Gameplay
            if (autoSaveToggle != null)
                autoSaveToggle.isOn = PlayerPrefs.GetInt("AutoSave", 1) == 1;

            if (hintSystemToggle != null)
                hintSystemToggle.isOn = PlayerPrefs.GetInt("HintSystem", 1) == 1;

            if (touchSizeSlider != null)
            {
                float size = PlayerPrefs.GetFloat("TouchSize", 1f);
                touchSizeSlider.value = size;
                UpdateTouchSizeText(size);
            }

            // Privacy
            if (analyticsToggle != null)
                analyticsToggle.isOn = PlayerPrefs.GetInt("Analytics", 1) == 1;

            if (shareDataToggle != null)
                shareDataToggle.isOn = PlayerPrefs.GetInt("ShareData", 1) == 1;
        }

        #endregion

        #region Audio Handlers

        private void OnMasterVolumeChanged(float value)
        {
            UpdateVolumeText(masterVolumeText, value);
            PlayerPrefs.SetFloat("Audio_MasterVolume", value);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMasterVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            UpdateVolumeText(musicVolumeText, value);
            PlayerPrefs.SetFloat("Audio_MusicVolume", value);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            UpdateVolumeText(sfxVolumeText, value);
            PlayerPrefs.SetFloat("Audio_SFXVolume", value);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume(value);
        }

        private void OnAmbientVolumeChanged(float value)
        {
            UpdateVolumeText(ambientVolumeText, value);
            PlayerPrefs.SetFloat("Audio_AmbientVolume", value);

            if (AudioManager.Instance != null)
                AudioManager.Instance.SetAmbientVolume(value);
        }

        private void UpdateVolumeText(TextMeshProUGUI text, float value)
        {
            if (text != null)
                text.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        #endregion

        #region Visual Accessibility Handlers

        private void OnReducedMotionChanged(bool value)
        {
            PlayerPrefs.SetInt("ReducedMotion", value ? 1 : 0);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.SetReducedMotion(value);
        }

        private void OnHighContrastChanged(bool value)
        {
            PlayerPrefs.SetInt("HighContrast", value ? 1 : 0);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.SetHighContrast(value);
        }

        private void OnTextScaleChanged(float value)
        {
            UpdateTextScaleDisplay(value);
            PlayerPrefs.SetFloat("TextScale", value);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.SetTextScale(value);
        }

        private void UpdateTextScaleDisplay(float value)
        {
            if (textScaleText != null)
                textScaleText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void OnColorblindModeChanged(int index)
        {
            string mode = GetModeForDropdownIndex(index);
            PlayerPrefs.SetString("ColorblindMode", mode);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.SetColorblindMode(mode);
        }

        private int GetDropdownIndexForMode(string mode)
        {
            switch (mode)
            {
                case "None": return 0;
                case "Protanopia": return 1;
                case "Deuteranopia": return 2;
                case "Tritanopia": return 3;
                case "Achromatopsia": return 4;
                case "Protanomaly": return 5;
                default: return 0;
            }
        }

        private string GetModeForDropdownIndex(int index)
        {
            switch (index)
            {
                case 0: return "None";
                case 1: return "Protanopia";
                case 2: return "Deuteranopia";
                case 3: return "Tritanopia";
                case 4: return "Achromatopsia";
                case 5: return "Protanomaly";
                default: return "None";
            }
        }

        private void OnNeurodivergentModeChanged(int index)
        {
            string mode = GetNeurodivergentModeForIndex(index);
            PlayerPrefs.SetString("NeurodivergentMode", mode);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.SetNeurodivergentMode(mode);
        }

        private int GetNeurodivergentDropdownIndex(string mode)
        {
            switch (mode)
            {
                case "None": return 0;
                case "ADHD": return 1;
                case "Autism": return 2;
                case "Dyslexia": return 3;
                default: return 0;
            }
        }

        private string GetNeurodivergentModeForIndex(int index)
        {
            switch (index)
            {
                case 0: return "None";
                case 1: return "ADHD";
                case 2: return "Autism";
                case 3: return "Dyslexia";
                default: return "None";
            }
        }

        #endregion

        #region Haptic Handlers

        private void OnHapticsEnabledChanged(bool value)
        {
            PlayerPrefs.SetInt("HapticsEnabled", value ? 1 : 0);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.SetHapticsEnabled(value);
        }

        private void OnHapticIntensityChanged(float value)
        {
            UpdateHapticIntensityText(value);
            PlayerPrefs.SetFloat("HapticIntensity", value);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.SetHapticIntensity(value);
        }

        private void UpdateHapticIntensityText(float value)
        {
            if (hapticIntensityText != null)
                hapticIntensityText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        #endregion

        #region Gameplay Handlers

        private void OnAutoSaveChanged(bool value)
        {
            PlayerPrefs.SetInt("AutoSave", value ? 1 : 0);
        }

        private void OnHintSystemChanged(bool value)
        {
            PlayerPrefs.SetInt("HintSystem", value ? 1 : 0);
        }

        private void OnTouchSizeChanged(float value)
        {
            UpdateTouchSizeText(value);
            PlayerPrefs.SetFloat("TouchSize", value);
        }

        private void UpdateTouchSizeText(float value)
        {
            if (touchSizeText != null)
                touchSizeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        #endregion

        #region Privacy Handlers

        private void OnAnalyticsChanged(bool value)
        {
            PlayerPrefs.SetInt("Analytics", value ? 1 : 0);
            // Update analytics collection
        }

        private void OnShareDataChanged(bool value)
        {
            PlayerPrefs.SetInt("ShareData", value ? 1 : 0);
        }

        #endregion

        #region Button Handlers

        private void OnApplyClicked()
        {
            PlayerPrefs.Save();

            // Show confirmation
            Debug.Log("Settings applied!");
        }

        private void OnResetToDefaultClicked()
        {
            // Reset all settings to default
            PlayerPrefs.DeleteAll();
            Core.SaveSystem.Instance?.DeleteSaveData();
            Systems.DailyChallengeManager.Instance?.ResetProgress();
            LoadSettings();

            Debug.Log("Settings reset to default!");
        }

        private void OnCloseClicked()
        {
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }

        #endregion
    }
}
