using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Displays achievement progress and unlocked achievements.
    /// Beautiful, celebratory presentation with accessibility support.
    /// </summary>
    public class AchievementDisplayManager : MonoBehaviour
    {
        [Header("Achievement List")]
        [SerializeField] private ScrollRect achievementScrollView;
        [SerializeField] private GameObject achievementItemPrefab;
        [SerializeField] private Transform achievementListContainer;
        
        [Header("Category Filtering")]
        [SerializeField] private TMP_Dropdown categoryDropdown;
        [SerializeField] private Toggle showLockedToggle;
        
        [Header("Header Display")]
        [SerializeField] private TextMeshProUGUI totalProgressText;
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI unlockedCountText;
        
        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image achievementIcon;
        [SerializeField] private TextMeshProUGUI achievementTitleText;
        [SerializeField] private TextMeshProUGUI achievementDescriptionText;
        [SerializeField] private TextMeshProUGUI achievementRewardText;
        [SerializeField] private TextMeshProUGUI unlockDateText;
        
        [Header("Audio")]
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip selectSound;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem unlockParticles;
        
        [Header("Toast Notification")]
        [SerializeField] private GameObject toastPrefab;   // Assign a panel with TextMeshProUGUI child named "ToastText"
        [SerializeField] private Transform toastContainer; // Canvas overlay root - falls back to self
        
        // State
        private List<AchievementItemUI> achievementItems = new List<AchievementItemUI>();
        private string currentCategory = "All";
        private bool showLocked = true;
        
        private void Start()
        {
            SetupListeners();
            LoadAchievements();
            UpdateProgressDisplay();
            
            if (detailPanel != null)
                detailPanel.SetActive(false);
        }
        
        #region Setup
        
        private void SetupListeners()
        {
            if (categoryDropdown != null)
                categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            
            if (showLockedToggle != null)
                showLockedToggle.onValueChanged.AddListener(OnShowLockedChanged);
        }
        
        #endregion
        
        #region Load Achievements
        
        private void LoadAchievements()
        {
            if (AchievementManager.Instance == null) return;
            
            // Clear existing items
            ClearAchievementList();
            
            // Get all achievements
            var achievements = AchievementManager.Instance.GetAllAchievements();
            
            foreach (var achievement in achievements)
            {
                // Filter by category
                if (currentCategory != "All" && achievement.Category != currentCategory)
                    continue;
                
                // Filter by locked status
                if (!showLocked && achievement.IsLocked)
                    continue;
                
                // Create achievement item
                CreateAchievementItem(achievement);
            }
        }
        
        private void CreateAchievementItem(Achievement achievement)
        {
            if (achievementItemPrefab == null || achievementListContainer == null)
                return;
            
            GameObject itemObj = Instantiate(achievementItemPrefab, achievementListContainer);
            AchievementItemUI itemUI = itemObj.GetComponent<AchievementItemUI>();
            
            if (itemUI == null)
                itemUI = itemObj.AddComponent<AchievementItemUI>();
            
            itemUI.Initialize(achievement);
            itemUI.OnClicked += () => ShowAchievementDetail(achievement);
            
            achievementItems.Add(itemUI);
        }
        
        private void ClearAchievementList()
        {
            foreach (var item in achievementItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            achievementItems.Clear();
        }
        
        #endregion
        
        #region Progress Display
        
        private void UpdateProgressDisplay()
        {
            if (AchievementManager.Instance == null) return;
            
            int totalAchievements = AchievementManager.Instance.GetTotalAchievementCount();
            int unlockedCount = AchievementManager.Instance.GetUnlockedCount();
            float progress = totalAchievements > 0 ? (float)unlockedCount / totalAchievements : 0f;
            
            if (totalProgressText != null)
                totalProgressText.text = $"{Mathf.RoundToInt(progress * 100)}% Complete";
            
            if (progressBar != null)
                progressBar.fillAmount = progress;
            
            if (unlockedCountText != null)
                unlockedCountText.text = $"{unlockedCount}/{totalAchievements} Unlocked";
        }
        
        #endregion
        
        #region Detail Panel
        
        private void ShowAchievementDetail(Achievement achievement)
        {
            if (detailPanel == null) return;
            
            // Play sound
            if (AudioManager.Instance != null && selectSound != null)
                AudioManager.Instance.PlaySFX(selectSound);
            
            // Populate detail panel
            if (achievementIcon != null)
                achievementIcon.sprite = achievement.Icon;
            
            if (achievementTitleText != null)
                achievementTitleText.text = achievement.Title;
            
            if (achievementDescriptionText != null)
            {
                if (achievement.IsHidden && achievement.IsLocked)
                    achievementDescriptionText.text = "???";
                else
                    achievementDescriptionText.text = achievement.Description;
            }
            
            if (achievementRewardText != null)
                achievementRewardText.text = $"Reward: {achievement.RewardDescription}";
            
            if (unlockDateText != null)
            {
                if (!achievement.IsLocked)
                    unlockDateText.text = $"Unlocked: {achievement.UnlockDate.ToShortDateString()}";
                else
                    unlockDateText.text = "Locked";
            }
            
            detailPanel.SetActive(true);
        }
        
        public void CloseDetailPanel()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);
        }
        
        #endregion
        
        #region Filtering
        
        private void OnCategoryChanged(int index)
        {
            // Map dropdown index to category name
            string[] categories = { "All", "Progression", "Exploration", "Accessibility", "Social", "Mastery", "Secret" };
            if (index >= 0 && index < categories.Length)
            {
                currentCategory = categories[index];
                LoadAchievements();
            }
        }
        
        private void OnShowLockedChanged(bool value)
        {
            showLocked = value;
            LoadAchievements();
        }
        
        #endregion
        
        #region Achievement Unlock Animation
        
        /// <summary>
        /// Called when a new achievement is unlocked (by AchievementManager)
        /// </summary>
        public void OnAchievementUnlocked(Achievement achievement)
        {
            // Play unlock animation
            if (unlockParticles != null)
            {
                bool reducedMotion = AccessibilityManager.Instance != null && 
                                   AccessibilityManager.Instance.IsReducedMotionEnabled();
                
                if (!reducedMotion)
                    unlockParticles.Play();
            }
            
            // Play sound
            if (AudioManager.Instance != null && unlockSound != null)
                AudioManager.Instance.PlaySFX(unlockSound);
            
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Success);
            
            // Refresh display
            LoadAchievements();
            UpdateProgressDisplay();
            
            // Show toast notification
            ShowUnlockToast(achievement);
        }
        
        private void ShowUnlockToast(Achievement achievement)
        {
            StartCoroutine(ShowToastCoroutine(achievement.Title));
        }

        private System.Collections.IEnumerator ShowToastCoroutine(string titleText)
        {
            Transform root = toastContainer != null ? toastContainer : transform;

            // Create or reuse toast panel
            GameObject toast = null;
            if (toastPrefab != null)
            {
                toast = Instantiate(toastPrefab, root);
            }
            else
            {
                // Build a minimal toast at runtime when no prefab is assigned
                toast = new GameObject("AchievementToast");
                toast.transform.SetParent(root, false);

                var canvas = toast.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = 100;

                var bg = new GameObject("Background");
                bg.transform.SetParent(toast.transform, false);
                var img = bg.AddComponent<Image>();
                img.color = new Color(0.1f, 0.05f, 0.2f, 0.92f);
                var rt = bg.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(320, 72);

                var label = new GameObject("ToastText");
                label.transform.SetParent(bg.transform, false);
                var tmp = label.AddComponent<TextMeshProUGUI>();
                tmp.text = $"✦ Achievement Unlocked: {titleText}";
                tmp.fontSize = 14;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                var lrt = label.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(8, 4);
                lrt.offsetMax = new Vector2(-8, -4);
            }

            // Set text if prefab has a "ToastText" child
            var textComp = toast.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null && toastPrefab != null)
                textComp.text = $"✦ Achievement Unlocked: {titleText}";

            // Fade in
            var canvasGroup = toast.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = toast.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            bool reducedMotion = AccessibilityManager.Instance != null &&
                                 AccessibilityManager.Instance.IsReducedMotionEnabled();

            float fadeDuration = reducedMotion ? 0f : 0.3f;
            float holdDuration = 2.5f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                canvasGroup.alpha = elapsed / fadeDuration;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSecondsRealtime(holdDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                canvasGroup.alpha = 1f - (elapsed / fadeDuration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Destroy(toast);
        }
        
        #endregion
    }
    
    #region Achievement Item UI Component
    
    /// <summary>
    /// UI component for individual achievement items in the list
    /// </summary>
    public class AchievementItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image progressBar;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Button button;
        
        private Achievement achievement;
        
        public System.Action OnClicked;
        
        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(HandleClick);
        }
        
        public void Initialize(Achievement ach)
        {
            achievement = ach;
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (achievement == null) return;
            
            // Icon
            if (iconImage != null)
            {
                if (achievement.IsLocked)
                    iconImage.color = Color.gray;
                else
                {
                    iconImage.sprite = achievement.Icon;
                    iconImage.color = Color.white;
                }
            }
            
            // Title
            if (titleText != null)
            {
                if (achievement.IsHidden && achievement.IsLocked)
                    titleText.text = "Hidden Achievement";
                else
                    titleText.text = achievement.Title;
            }
            
            // Progress
            if (progressText != null)
            {
                progressText.text = $"{achievement.CurrentProgress}/{achievement.RequiredProgress}";
            }
            
            if (progressBar != null)
            {
                float progress = achievement.RequiredProgress > 0 ? 
                    (float)achievement.CurrentProgress / achievement.RequiredProgress : 0f;
                progressBar.fillAmount = progress;
            }
            
            // Locked overlay
            if (lockedOverlay != null)
                lockedOverlay.SetActive(achievement.IsLocked);
            
            // Background color
            if (backgroundImage != null)
            {
                if (achievement.IsLocked)
                    backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                else
                    backgroundImage.color = new Color(0.2f, 0.8f, 0.5f, 0.3f);
            }
        }
        
        private void HandleClick()
        {
            OnClicked?.Invoke();
        }
    }
    
    #endregion
}
