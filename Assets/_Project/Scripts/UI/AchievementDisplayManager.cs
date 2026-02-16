using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
            // Create a toast notification
            // In a real implementation, this would be a UI popup
            Debug.Log($"Achievement Unlocked: {achievement.Title}");
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
    
    #region Achievement Data Structure
    
    /// <summary>
    /// Data structure for an achievement
    /// </summary>
    [System.Serializable]
    public class Achievement
    {
        public string Id;
        public string Title;
        public string Description;
        public string Category;
        public Sprite Icon;
        public bool IsHidden;
        public bool IsLocked;
        public int CurrentProgress;
        public int RequiredProgress;
        public string RewardDescription;
        public System.DateTime UnlockDate;
    }
    
    #endregion
}
