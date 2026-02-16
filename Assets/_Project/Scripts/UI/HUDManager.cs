using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Manages the main game HUD including resource counters, daily challenge progress, and notifications.
    /// Accessibility-first design with scalable UI and high contrast options.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Resource Display")]
        [SerializeField] private TextMeshProUGUI sigilCountText;
        [SerializeField] private TextMeshProUGUI sparksCountText;
        [SerializeField] private Image sigilCountIcon;
        [SerializeField] private Image sparksCountIcon;
        
        [Header("Daily Challenge")]
        [SerializeField] private GameObject dailyChallengePanel;
        [SerializeField] private TextMeshProUGUI challengeTitleText;
        [SerializeField] private TextMeshProUGUI challengeProgressText;
        [SerializeField] private Image challengeProgressBar;
        [SerializeField] private TextMeshProUGUI streakText;
        
        [Header("Notifications")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private Image notificationIcon;
        [SerializeField] private float notificationDuration = 3f;
        
        [Header("Realm Info")]
        [SerializeField] private TextMeshProUGUI realmNameText;
        [SerializeField] private Image realmIcon;
        
        [Header("Accessibility")]
        [SerializeField] private float textScaleMultiplier = 1f;
        [SerializeField] private bool highContrastMode = false;
        [SerializeField] private CanvasGroup hudCanvasGroup;
        
        // State
        private int currentSigils = 0;
        private int currentSparks = 0;
        private bool isNotificationShowing = false;
        
        private void Start()
        {
            // Apply accessibility settings
            ApplyAccessibilitySettings();
            
            // Initialize UI
            UpdateResourceDisplay();
            LoadDailyChallengeInfo();
            
            // Hide notification panel
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }
        
        #region Resource Display
        
        public void UpdateSigilCount(int count)
        {
            currentSigils = count;
            UpdateResourceDisplay();
        }
        
        public void UpdateSparksCount(int count)
        {
            currentSparks = count;
            UpdateResourceDisplay();
        }
        
        private void UpdateResourceDisplay()
        {
            if (sigilCountText != null)
                sigilCountText.text = currentSigils.ToString();
            
            if (sparksCountText != null)
                sparksCountText.text = currentSparks.ToString();
        }
        
        public void AddSigils(int amount)
        {
            currentSigils += amount;
            UpdateResourceDisplay();
            
            // Show notification
            ShowNotification($"+{amount} Sigil{(amount != 1 ? "s" : "")}", NotificationType.Reward);
        }
        
        public void AddSparks(int amount)
        {
            currentSparks += amount;
            UpdateResourceDisplay();
            
            // Haptic feedback
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Light);
        }
        
        #endregion
        
        #region Daily Challenge
        
        private void LoadDailyChallengeInfo()
        {
            if (DailyChallengeManager.Instance == null) return;
            
            var challenge = DailyChallengeManager.Instance.GetTodayChallenge();
            if (challenge == null) return;
            
            // Update challenge info
            if (challengeTitleText != null)
                challengeTitleText.text = challenge.Title;
            
            UpdateChallengeProgress(challenge.CurrentProgress, challenge.RequiredProgress);
            
            if (streakText != null)
            {
                int streak = DailyChallengeManager.Instance.GetCurrentStreak();
                streakText.text = streak > 0 ? $"{streak} day streak!" : "";
            }
        }
        
        public void UpdateChallengeProgress(int current, int required)
        {
            if (challengeProgressText != null)
                challengeProgressText.text = $"{current}/{required}";
            
            if (challengeProgressBar != null)
            {
                float progress = required > 0 ? (float)current / required : 0f;
                challengeProgressBar.fillAmount = progress;
            }
            
            // Show completion notification
            if (current >= required && current > 0)
            {
                ShowNotification("Daily Challenge Complete!", NotificationType.Achievement);
            }
        }
        
        public void ToggleDailyChallengePanel(bool show)
        {
            if (dailyChallengePanel != null)
                dailyChallengePanel.SetActive(show);
        }
        
        #endregion
        
        #region Notifications
        
        public enum NotificationType
        {
            Info,
            Reward,
            Achievement,
            Warning
        }
        
        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            if (isNotificationShowing)
            {
                // Queue notification
                StopAllCoroutines();
            }
            
            StartCoroutine(ShowNotificationCoroutine(message, type));
        }
        
        private System.Collections.IEnumerator ShowNotificationCoroutine(string message, NotificationType type)
        {
            isNotificationShowing = true;
            
            // Set notification content
            if (notificationText != null)
                notificationText.text = message;
            
            // Set icon color based on type
            if (notificationIcon != null)
            {
                notificationIcon.color = GetColorForNotificationType(type);
            }
            
            // Show panel
            if (notificationPanel != null)
                notificationPanel.SetActive(true);
            
            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioClip sound = type == NotificationType.Achievement ? null : null; // Load appropriate sound
                // AudioManager.Instance.PlaySFX(sound);
            }
            
            // Haptic feedback
            if (AccessibilityManager.Instance != null)
            {
                HapticType hapticType = type == NotificationType.Achievement ? HapticType.Success : HapticType.Light;
                AccessibilityManager.Instance.TriggerHaptic(hapticType);
            }
            
            // Wait
            yield return new WaitForSeconds(notificationDuration);
            
            // Hide panel
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
            
            isNotificationShowing = false;
        }
        
        private Color GetColorForNotificationType(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Info: return Color.blue;
                case NotificationType.Reward: return new Color(1f, 0.8f, 0f); // Gold
                case NotificationType.Achievement: return new Color(0.5f, 1f, 0.5f); // Green
                case NotificationType.Warning: return new Color(1f, 0.5f, 0f); // Orange
                default: return Color.white;
            }
        }
        
        #endregion
        
        #region Realm Display
        
        public void UpdateRealmDisplay(string realmName, Sprite realmSprite)
        {
            if (realmNameText != null)
                realmNameText.text = realmName;
            
            if (realmIcon != null && realmSprite != null)
                realmIcon.sprite = realmSprite;
        }
        
        #endregion
        
        #region Accessibility
        
        private void ApplyAccessibilitySettings()
        {
            if (AccessibilityManager.Instance == null) return;
            
            // Text scaling
            float scale = AccessibilityManager.Instance.GetTextScale();
            ApplyTextScale(scale);
            
            // High contrast mode
            bool highContrast = AccessibilityManager.Instance.IsHighContrastEnabled();
            ApplyHighContrastMode(highContrast);
        }
        
        private void ApplyTextScale(float scale)
        {
            textScaleMultiplier = scale;
            
            // Apply to all text elements
            TextMeshProUGUI[] allText = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in allText)
            {
                text.fontSize *= textScaleMultiplier;
            }
        }
        
        private void ApplyHighContrastMode(bool enabled)
        {
            highContrastMode = enabled;
            
            if (enabled)
            {
                // Increase contrast of all UI elements
                if (hudCanvasGroup != null)
                    hudCanvasGroup.alpha = 1f;
                
                // Make backgrounds more opaque
                Image[] allImages = GetComponentsInChildren<Image>();
                foreach (var img in allImages)
                {
                    if (img.name.Contains("Background") || img.name.Contains("Panel"))
                    {
                        Color current = img.color;
                        img.color = new Color(current.r, current.g, current.b, 0.9f);
                    }
                }
            }
        }
        
        public void SetHUDVisibility(bool visible)
        {
            if (hudCanvasGroup != null)
            {
                hudCanvasGroup.alpha = visible ? 1f : 0f;
                hudCanvasGroup.interactable = visible;
                hudCanvasGroup.blocksRaycasts = visible;
            }
        }
        
        #endregion
    }
}
