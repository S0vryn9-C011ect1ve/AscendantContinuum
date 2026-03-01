using UnityEngine;
using UnityEngine.UI;
using AscendantContinuum.Core;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Unified settings menu controller for Audio, Accessibility, and Legal.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Audio Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider ambienceVolumeSlider;
        [SerializeField] private Slider uiVolumeSlider;

        [Header("Accessibility Toggles")]
        [SerializeField] private Toggle reducedMotionToggle;
        [SerializeField] private Toggle hapticsToggle;
        [SerializeField] private Dropdown colorblindDropdown;

        [Header("Legal & Data")]
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button revokeConsentButton;
        [SerializeField] private Button deleteAccountButton;

        [Header("UI References")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            InitializeUI();
            LoadCurrentSettings();
        }

        private void InitializeUI()
        {
            // Audio Listeners
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMasterVolume(v));
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetSFXVolume(v));
            if (ambienceVolumeSlider != null) ambienceVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetAmbienceVolume(v));
            if (uiVolumeSlider != null) uiVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetUIVolume(v));

            // Accessibility Listeners
            if (reducedMotionToggle != null) reducedMotionToggle.onValueChanged.AddListener(v => AccessibilityManager.Instance?.SetReducedMotion(v));
            if (hapticsToggle != null) hapticsToggle.onValueChanged.AddListener(v => AccessibilityManager.Instance?.SetHaptics(v));
            if (colorblindDropdown != null) colorblindDropdown.onValueChanged.AddListener(v => AccessibilityManager.Instance?.SetColorblindMode((ColorblindMode)v));

            // Legal Listeners
            if (privacyPolicyButton != null) privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
            if (revokeConsentButton != null) revokeConsentButton.onClick.AddListener(RevokeDataConsent);
            if (deleteAccountButton != null) deleteAccountButton.onClick.AddListener(DeleteAccount);

            if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);
        }

        private void LoadCurrentSettings()
        {
            // Load Audio
            if (AudioManager.Instance != null)
            {
                if (masterVolumeSlider != null) masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
                if (musicVolumeSlider != null) musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
                if (sfxVolumeSlider != null) sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                if (ambienceVolumeSlider != null) ambienceVolumeSlider.value = AudioManager.Instance.GetAmbienceVolume();
                if (uiVolumeSlider != null) uiVolumeSlider.value = AudioManager.Instance.GetUIVolume();
            }

            // Load Accessibility
            if (AccessibilityManager.Instance != null)
            {
                if (reducedMotionToggle != null) reducedMotionToggle.isOn = AccessibilityManager.Instance.ReducedMotionEnabled;
                if (hapticsToggle != null) hapticsToggle.isOn = AccessibilityManager.Instance.HapticsEnabled;
                if (colorblindDropdown != null) colorblindDropdown.value = (int)AccessibilityManager.Instance.CurrentColorblindMode;
            }
        }

        public void OpenSettings()
        {
            LoadCurrentSettings();
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            // Save settings when closing
            SaveSystem.Instance?.SaveGame();
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void OpenPrivacyPolicy()
        {
            Application.OpenURL("https://ascendant-continuum.web.app/privacy");
        }

        private void RevokeDataConsent()
        {
            GDPRConsentManager.Instance?.RevokeConsent();
            // Optionally show a confirmation dialog here
            Debug.Log("[SettingsMenu] Data consent revoked by user.");
        }

        private void DeleteAccount()
        {
            // In a real app, this would call a Firebase Function to delete the user record
            SaveSystem.Instance?.DeleteSaveData();
            GDPRConsentManager.Instance?.ClearConsentState();
            Debug.Log("[SettingsMenu] Account deletion requested.");
            
            // Return to title screen
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveAllListeners();
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveAllListeners();
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            if (ambienceVolumeSlider != null) ambienceVolumeSlider.onValueChanged.RemoveAllListeners();
            if (uiVolumeSlider != null) uiVolumeSlider.onValueChanged.RemoveAllListeners();

            if (reducedMotionToggle != null) reducedMotionToggle.onValueChanged.RemoveAllListeners();
            if (hapticsToggle != null) hapticsToggle.onValueChanged.RemoveAllListeners();
            if (colorblindDropdown != null) colorblindDropdown.onValueChanged.RemoveAllListeners();

            if (privacyPolicyButton != null) privacyPolicyButton.onClick.RemoveAllListeners();
            if (revokeConsentButton != null) revokeConsentButton.onClick.RemoveAllListeners();
            if (deleteAccountButton != null) deleteAccountButton.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        }
    }
}
