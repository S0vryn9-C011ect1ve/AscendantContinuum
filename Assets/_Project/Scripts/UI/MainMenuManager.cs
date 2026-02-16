using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Main menu controller with accessibility features and beautiful transitions.
    /// Entry point for the game experience.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject realmSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;
        
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button realmSelectButton;
        [SerializeField] private Button dailyChallengeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Realm Selection")]
        [SerializeField] private Button[] realmButtons;
        [SerializeField] private string[] realmSceneNames;
        
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private TextMeshProUGUI sigilCountText;
        [SerializeField] private Image playerSigilImage;
        
        [Header("Daily Challenge Preview")]
        [SerializeField] private GameObject dailyChallengePreview;
        [SerializeField] private TextMeshProUGUI challengeText;
        [SerializeField] private TextMeshProUGUI streakText;
        
        [Header("Audio")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip transitionSound;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem ambientParticles;
        [SerializeField] private Image fadePanel;
        
        private void Start()
        {
            // Initialize menu
            ShowMainPanel();
            LoadPlayerInfo();
            LoadDailyChallengePreview();
            
            // Setup button listeners
            SetupButtons();
            
            // Play menu music
            if (AudioManager.Instance != null && menuMusic != null)
            {
                AudioManager.Instance.PlayMusic(menuMusic);
            }
            
            // Start ambient particles
            if (ambientParticles != null)
            {
                bool reducedMotion = AccessibilityManager.Instance != null && 
                                   AccessibilityManager.Instance.IsReducedMotionEnabled();
                ambientParticles.gameObject.SetActive(!reducedMotion);
            }
            
            // Fade in
            StartCoroutine(FadeIn());
        }
        
        #region Button Setup
        
        private void SetupButtons()
        {
            if (playButton != null)
                playButton.onClick.AddListener(() => OnPlayClicked());
            
            if (realmSelectButton != null)
                realmSelectButton.onClick.AddListener(() => ShowRealmSelect());
            
            if (dailyChallengeButton != null)
                dailyChallengeButton.onClick.AddListener(() => OnDailyChallengeClicked());
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => ShowSettings());
            
            if (creditsButton != null)
                creditsButton.onClick.AddListener(() => ShowCredits());
            
            if (quitButton != null)
                quitButton.onClick.AddListener(() => OnQuitClicked());
            
            // Setup realm buttons
            for (int i = 0; i < realmButtons.Length; i++)
            {
                int index = i; // Capture for closure
                if (realmButtons[i] != null)
                {
                    realmButtons[i].onClick.AddListener(() => OnRealmSelected(index));
                }
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnPlayClicked()
        {
            PlayButtonSound();
            
            // Load last visited realm or Emberforge by default
            string lastRealm = PlayerPrefs.GetString("LastRealm", "Emberforge");
            LoadRealm(lastRealm);
        }
        
        private void OnDailyChallengeClicked()
        {
            PlayButtonSound();
            
            // Load today's challenge realm
            if (DailyChallengeManager.Instance != null)
            {
                var challenge = DailyChallengeManager.Instance.GetTodayChallenge();
                if (challenge != null)
                {
                    LoadRealm(challenge.RealmName);
                }
            }
        }
        
        private void OnQuitClicked()
        {
            PlayButtonSound();
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        private void OnRealmSelected(int realmIndex)
        {
            PlayButtonSound();
            
            if (realmIndex >= 0 && realmIndex < realmSceneNames.Length)
            {
                LoadRealm(realmSceneNames[realmIndex]);
            }
        }
        
        #endregion
        
        #region Panel Management
        
        private void ShowMainPanel()
        {
            HideAllPanels();
            if (mainPanel != null)
                mainPanel.SetActive(true);
        }
        
        private void ShowRealmSelect()
        {
            PlayButtonSound();
            HideAllPanels();
            if (realmSelectPanel != null)
                realmSelectPanel.SetActive(true);
        }
        
        private void ShowSettings()
        {
            PlayButtonSound();
            HideAllPanels();
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }
        
        private void ShowCredits()
        {
            PlayButtonSound();
            HideAllPanels();
            if (creditsPanel != null)
                creditsPanel.SetActive(true);
        }
        
        public void BackToMain()
        {
            PlayButtonSound();
            ShowMainPanel();
        }
        
        private void HideAllPanels()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (realmSelectPanel != null) realmSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }
        
        #endregion
        
        #region Player Info
        
        private void LoadPlayerInfo()
        {
            // Load player data
            // In a real implementation, this would load from SaveSystem
            
            if (playerLevelText != null)
            {
                int level = PlayerPrefs.GetInt("PlayerLevel", 1);
                playerLevelText.text = $"Level {level}";
            }
            
            if (sigilCountText != null)
            {
                int sigils = PlayerPrefs.GetInt("SigilCount", 0);
                sigilCountText.text = $"{sigils} Sigils";
            }
            
            // Load player's personal sigil
            if (playerSigilImage != null)
            {
                // Would load generated sigil texture
                // playerSigilImage.sprite = LoadPlayerSigil();
            }
        }
        
        private void LoadDailyChallengePreview()
        {
            if (DailyChallengeManager.Instance == null) return;
            
            var challenge = DailyChallengeManager.Instance.GetTodayChallenge();
            if (challenge == null) return;
            
            if (challengeText != null)
                challengeText.text = challenge.Title;
            
            if (streakText != null)
            {
                int streak = DailyChallengeManager.Instance.GetCurrentStreak();
                streakText.text = streak > 0 ? $"{streak} Day Streak!" : "Start your streak today!";
            }
        }
        
        #endregion
        
        #region Scene Loading
        
        private void LoadRealm(string realmName)
        {
            StartCoroutine(LoadRealmCoroutine(realmName));
        }
        
        private System.Collections.IEnumerator LoadRealmCoroutine(string realmName)
        {
            // Play transition sound
            if (AudioManager.Instance != null && transitionSound != null)
                AudioManager.Instance.PlaySFX(transitionSound);
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Save last realm
            PlayerPrefs.SetString("LastRealm", realmName);
            PlayerPrefs.Save();
            
            // Load scene
            SceneManager.LoadScene(realmName);
        }
        
        #endregion
        
        #region Visual Effects
        
        private System.Collections.IEnumerator FadeIn()
        {
            if (fadePanel == null) yield break;
            
            float duration = 1f;
            float elapsed = 0f;
            
            fadePanel.gameObject.SetActive(true);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            
            fadePanel.gameObject.SetActive(false);
        }
        
        private System.Collections.IEnumerator FadeOut()
        {
            if (fadePanel == null) yield break;
            
            float duration = 1f;
            float elapsed = 0f;
            
            fadePanel.gameObject.SetActive(true);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }
        
        #endregion
        
        #region Audio
        
        private void PlayButtonSound()
        {
            if (AudioManager.Instance != null && buttonClickSound != null)
                AudioManager.Instance.PlaySFX(buttonClickSound);
            
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Selection);
        }
        
        #endregion
    }
}
