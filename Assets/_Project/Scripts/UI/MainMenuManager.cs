using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

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
        [SerializeField] private Button seasonPassButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Realm Selection")]
        [SerializeField] private Button[] realmButtons;
        [SerializeField] private Button realmBackButton;
        [SerializeField] private Text realmsLabel;
        [SerializeField] private GameObject realmOverlay;
        [SerializeField] private string[] realmSceneNames;

        [Header("Onboarding")]
        [SerializeField] private string onboardingSceneName = SceneNames.Onboarding;
        
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

        private Coroutine realmButtonsRoutine;
        
        private void Start()
        {
            TryAutoBindRealmButtons();
            EnsureDefaultRealmSceneNames();
            SetRealmButtonsVisible(false, immediate: true);

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
            
            if (seasonPassButton != null)
                seasonPassButton.onClick.AddListener(() => SeasonPassUIManager.Instance?.OpenPanel());
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => ShowSettings());
            
            if (creditsButton != null)
                creditsButton.onClick.AddListener(() => ShowCredits());
            
            if (quitButton != null)
                quitButton.onClick.AddListener(() => OnQuitClicked());

            if (realmBackButton != null)
                realmBackButton.onClick.AddListener(() => BackToMain());
            
            // Setup realm buttons
            if (realmButtons == null) return;

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
            ShowRealmSelect();
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
            SetRealmButtonsVisible(false);
        }
        
        private void ShowRealmSelect()
        {
            PlayButtonSound();
            HideAllPanels();
            if (realmSelectPanel != null)
            {
                realmSelectPanel.SetActive(true);
            }

            SetRealmButtonsVisible(true);
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

        private void TryAutoBindRealmButtons()
        {
            if (realmButtons != null && realmButtons.Length == 5)
            {
                bool alreadyBound = true;
                for (int i = 0; i < realmButtons.Length; i++)
                {
                    if (realmButtons[i] == null)
                    {
                        alreadyBound = false;
                        break;
                    }
                }
                if (alreadyBound) return;
            }

            realmButtons = new Button[5];
            realmButtons[0] = FindButtonByName("RealmBtn_Emberforge");
            realmButtons[1] = FindButtonByName("RealmBtn_Verdant");
            realmButtons[2] = FindButtonByName("RealmBtn_EchoFields");
            realmButtons[3] = FindButtonByName("RealmBtn_DawnCitadel");
            realmButtons[4] = FindButtonByName("RealmBtn_LanternAscension");

            if (realmBackButton == null)
                realmBackButton = FindButtonByName("RealmBackButton");

            if (realmOverlay == null)
            {
                var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var t in allTransforms)
                    if (t != null && t.name == "[ Realm Overlay ]")
                    {
                        realmOverlay = t.gameObject;
                        break;
                    }
            }

            if (realmsLabel == null)
            {
                var labels = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < labels.Length; i++)
                {
                    if (labels[i] != null && labels[i].name == "RealmsLabel")
                    {
                        realmsLabel = labels[i];
                        break;
                    }
                }
            }
        }

        private Button FindButtonByName(string buttonName)
        {
            Transform t = transform.root.Find(buttonName);
            if (t != null) return t.GetComponent<Button>();

            var allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allButtons.Length; i++)
            {
                if (allButtons[i] != null && allButtons[i].name == buttonName)
                    return allButtons[i];
            }

            return null;
        }

        private void SetRealmButtonsVisible(bool visible, bool immediate = false)
        {
            if (realmButtonsRoutine != null)
            {
                StopCoroutine(realmButtonsRoutine);
                realmButtonsRoutine = null;
            }

            if (immediate)
            {
                ApplyRealmButtonsStateInstant(visible);
                return;
            }

            realmButtonsRoutine = StartCoroutine(AnimateRealmButtons(visible));
        }

        private void ApplyRealmButtonsStateInstant(bool visible)
        {
            if (realmOverlay != null)
                realmOverlay.SetActive(visible);

            if (realmButtons != null)
            {
                for (int i = 0; i < realmButtons.Length; i++)
                {
                    if (realmButtons[i] != null)
                    {
                        realmButtons[i].gameObject.SetActive(visible);
                        var g = EnsureCanvasGroup(realmButtons[i].gameObject);
                        g.alpha = visible ? 1f : 0f;
                        realmButtons[i].transform.localScale = Vector3.one;
                    }
                }
            }

            if (realmBackButton != null)
            {
                realmBackButton.gameObject.SetActive(visible);
                var g = EnsureCanvasGroup(realmBackButton.gameObject);
                g.alpha = visible ? 1f : 0f;
                realmBackButton.transform.localScale = Vector3.one;
            }

            if (realmsLabel != null)
            {
                realmsLabel.gameObject.SetActive(visible);
                var g = EnsureCanvasGroup(realmsLabel.gameObject);
                g.alpha = visible ? 1f : 0f;
                realmsLabel.transform.localScale = Vector3.one;
            }
        }

        private System.Collections.IEnumerator AnimateRealmButtons(bool visible)
        {
            if (!visible)
            {
                // Quick fade-out everything at once
                var items = GetRealmItems();
                float elapsed = 0f;
                while (elapsed < 0.15f)
                {
                    elapsed += Time.deltaTime;
                    float alpha = 1f - Mathf.Clamp01(elapsed / 0.15f);
                    foreach (var go in items)
                        if (go != null) EnsureCanvasGroup(go).alpha = alpha;
                    yield return null;
                }
                ApplyRealmButtonsStateInstant(false);
                realmButtonsRoutine = null;
                yield break;
            }

            // Fade in the overlay backdrop quickly
            if (realmOverlay != null)
            {
                realmOverlay.SetActive(true);
                var overlayCg = EnsureCanvasGroup(realmOverlay);
                overlayCg.alpha = 0f;
                float e = 0f;
                while (e < 0.12f)
                {
                    e += Time.deltaTime;
                    overlayCg.alpha = Mathf.Clamp01(e / 0.12f);
                    yield return null;
                }
                overlayCg.alpha = 1f;
            }

            // Reset all items to invisible/scaled-down before stagger
            var staggerItems = GetRealmItems();
            foreach (var go in staggerItems)
            {
                if (go == null) continue;
                go.SetActive(true);
                var cg = EnsureCanvasGroup(go);
                cg.alpha = 0f;
                go.transform.localScale = new Vector3(0.88f, 0.88f, 1f);
            }

            // Stagger: fire each item 75 ms after the previous (overlapping animations)
            for (int i = 0; i < staggerItems.Length; i++)
            {
                if (staggerItems[i] != null)
                    StartCoroutine(RevealItem(staggerItems[i], 0.18f));

                if (i < staggerItems.Length - 1)
                    yield return new WaitForSeconds(0.075f);
            }

            // Wait for last item's reveal to complete
            yield return new WaitForSeconds(0.18f);
            realmButtonsRoutine = null;
        }

        private System.Collections.IEnumerator RevealItem(GameObject go, float duration)
        {
            if (go == null) yield break;
            var cg = EnsureCanvasGroup(go);
            var startScale = new Vector3(0.88f, 0.88f, 1f);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                cg.alpha = t;
                go.transform.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                yield return null;
            }
            cg.alpha = 1f;
            go.transform.localScale = Vector3.one;
        }

        private GameObject[] GetRealmItems()
        {
            var items = new System.Collections.Generic.List<GameObject>();
            if (realmsLabel != null) items.Add(realmsLabel.gameObject);
            if (realmButtons != null)
                foreach (var b in realmButtons)
                    if (b != null) items.Add(b.gameObject);
            if (realmBackButton != null) items.Add(realmBackButton.gameObject);
            return items.ToArray();
        }

        private CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }

        private void EnsureDefaultRealmSceneNames()
        {
            if (realmSceneNames != null && realmSceneNames.Length == 5) return;

            realmSceneNames = new[]
            {
                SceneNames.Emberforge,
                SceneNames.Verdant,
                SceneNames.EchoFields,
                SceneNames.DawnCitadel,
                SceneNames.LanternAscension
            };
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
            // Use direct scene-load path for reliability from menu clicks.
            // RealmTransitionManager is still used by in-game transitions.
            StartCoroutine(LoadRealmCoroutine(realmName));
        }

        public string ResolveInitialPlayScene(bool shouldRunOnboarding, string lastRealm, string onboardingSceneName, bool onboardingSceneAvailable)
        {
            if (shouldRunOnboarding && onboardingSceneAvailable)
            {
                return onboardingSceneName;
            }

            return string.IsNullOrWhiteSpace(lastRealm) ? "Emberforge" : lastRealm;
        }

        private bool TryTransitionToRealm(string realmName)
        {
            if (RealmTransitionManager.Instance == null)
            {
                return false;
            }

            string realmId = ToRealmId(realmName);
            if (string.IsNullOrWhiteSpace(realmId))
            {
                return false;
            }

            try
            {
                RealmTransitionManager.Instance.TransitionToRealm(realmId);
                GameManager.Instance?.RecordLastRealm(realmName);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MainMenu] RealmTransitionManager path failed: {ex.Message}. Falling back to direct scene load.");
                return false;
            }
        }

        private string ToRealmId(string realmName)
        {
            if (string.IsNullOrWhiteSpace(realmName))
            {
                return string.Empty;
            }

            switch (realmName.Trim())
            {
                case "Emberforge":
                case "emberforge":
                    return "emberforge";
                case "VerdantSanctuary":
                case "verdant_sanctuary":
                case "verdant sanctuary":
                    return "verdant_sanctuary";
                case "EchoFields":
                case "echo_fields":
                case "echo fields":
                    return "echo_fields";
                case "DawnCitadel":
                case "dawn_citadel":
                case "dawn citadel":
                    return "dawn_citadel";
                case "LanternAscension":
                case "lantern_ascension":
                case "lantern ascension":
                    return "lantern_ascension";
                default:
                    return realmName.ToLowerInvariant().Replace(" ", "_");
            }
        }
        
        private System.Collections.IEnumerator LoadRealmCoroutine(string realmName)
        {
            // Play transition sound
            if (AudioManager.Instance != null && transitionSound != null)
                AudioManager.Instance.PlaySFX(transitionSound);
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Save last realm
            GameManager.Instance?.RecordLastRealm(realmName);
            if (GameManager.Instance == null)
            {
                PlayerPrefs.SetString("LastRealm", realmName);
                PlayerPrefs.Save();
            }
            
            // Load scene (accepts either realm id/name or direct scene name)
            string targetScene = ResolveSceneNameForLoad(realmName);
            SceneManager.LoadScene(targetScene);
        }

        private string ResolveSceneNameForLoad(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return SceneNames.MainMenu;
            }

            // Already a valid scene name
            if (Application.CanStreamedLevelBeLoaded(raw))
            {
                return raw;
            }

            // Convert realm variants to canonical scene name
            string realmId = ToRealmId(raw);
            string mapped  = SceneNames.FromRealmId(realmId);
            if (Application.CanStreamedLevelBeLoaded(mapped))
            {
                return mapped;
            }

            // Last resort: never crash load path; go back to main menu.
            Debug.LogWarning($"[MainMenu] Could not resolve scene '{raw}', falling back to {SceneNames.MainMenu}.");
            return SceneNames.MainMenu;
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
