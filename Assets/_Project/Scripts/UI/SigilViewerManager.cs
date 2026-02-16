using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Displays the player's personal sigil - their unique magical signature.
    /// Celebrates the player's identity and provides share functionality.
    /// </summary>
    public class SigilViewerManager : MonoBehaviour
    {
        [Header("Sigil Display")]
        [SerializeField] private Image sigilImage;
        [SerializeField] private RawImage sigilRawImage; // For runtime-generated texture
        [SerializeField] private TextMeshProUGUI sigilNameText;
        [SerializeField] private TextMeshProUGUI sigilDescriptionText;
        
        [Header("Playstyle Info")]
        [SerializeField] private TextMeshProUGUI archetypeText;
        [SerializeField] private TextMeshProUGUI favoriteRealmText;
        [SerializeField] private TextMeshProUGUI playTimeText;
        [SerializeField] private TextMeshProUGUI interactionStyleText;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem glowEffect;
        [SerializeField] private Light sigilLight;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private bool autoRotate = true;
        
        [Header("Share")]
        [SerializeField] private Button shareButton;
        [SerializeField] private Button regenerateButton;
        [SerializeField] private GameObject sharePanel;
        
        [Header("Audio")]
        [SerializeField] private AudioClip sigilRevealSound;
        [SerializeField] private AudioClip shareSound;
        
        // State
        private Texture2D currentSigilTexture;
        private PlayerPlaystyleMetrics playstyleMetrics;
        private bool isRevealing = false;
        
        private void Start()
        {
            SetupButtons();
            LoadPlayerSigil();
            
            if (sharePanel != null)
                sharePanel.SetActive(false);
        }
        
        private void Update()
        {
            if (autoRotate && sigilImage != null && !isRevealing)
            {
                sigilImage.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
        }
        
        #region Setup
        
        private void SetupButtons()
        {
            if (shareButton != null)
                shareButton.onClick.AddListener(OnShareClicked);
            
            if (regenerateButton != null)
                regenerateButton.onClick.AddListener(OnRegenerateClicked);
        }
        
        #endregion
        
        #region Load Sigil
        
        private void LoadPlayerSigil()
        {
            // Load playstyle metrics
            // In a real implementation, this would load from SaveSystem
            playstyleMetrics = LoadPlaystyleMetrics();
            
            // Generate or load sigil
            if (SigilGenerator.Instance != null)
            {
                GenerateSigil();
            }
            else
            {
                // Load from saved texture
                LoadSavedSigil();
            }
            
            // Update playstyle info
            UpdatePlaystyleInfo();
            
            // Reveal animation
            StartCoroutine(RevealSigilCoroutine());
        }
        
        private void GenerateSigil()
        {
            if (SigilGenerator.Instance == null || playstyleMetrics == null) return;
            
            // Generate sigil texture based on playstyle
            currentSigilTexture = SigilGenerator.Instance.GenerateSigil(playstyleMetrics);
            
            // Display sigil
            if (sigilRawImage != null)
            {
                sigilRawImage.texture = currentSigilTexture;
            }
            
            // Save sigil
            SaveSigil(currentSigilTexture);
            
            // Update sigil info
            UpdateSigilInfo();
        }
        
        private void LoadSavedSigil()
        {
            // Load saved sigil texture
            // In a real implementation, this would load from local storage
            string sigilPath = Application.persistentDataPath + "/player_sigil.png";
            
            if (System.IO.File.Exists(sigilPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(sigilPath);
                currentSigilTexture = new Texture2D(256, 256);
                currentSigilTexture.LoadImage(bytes);
                
                if (sigilRawImage != null)
                    sigilRawImage.texture = currentSigilTexture;
            }
            
            UpdateSigilInfo();
        }
        
        private void SaveSigil(Texture2D texture)
        {
            if (texture == null) return;
            
            byte[] bytes = texture.EncodeToPNG();
            string path = Application.persistentDataPath + "/player_sigil.png";
            System.IO.File.WriteAllBytes(path, bytes);
            
            Debug.Log($"Sigil saved to: {path}");
        }
        
        private PlayerPlaystyleMetrics LoadPlaystyleMetrics()
        {
            // In a real implementation, load from SaveSystem
            // For now, create sample data
            return new PlayerPlaystyleMetrics
            {
                totalPlayTimeMinutes = 120f,
                emberforgeVisits = 10,
                verdantSanctuaryVisits = 8,
                averageActionSpeed = 1.2f
            };
        }
        
        #endregion
        
        #region Display Updates
        
        private void UpdateSigilInfo()
        {
            if (playstyleMetrics == null) return;
            
            // Sigil name (generated from archetype + favorite realm)
            if (sigilNameText != null)
            {
                string name = $"{playstyleMetrics.PlaystyleArchetype} Sigil";
                sigilNameText.text = name;
            }
            
            // Description
            if (sigilDescriptionText != null)
            {
                sigilDescriptionText.text = GenerateSigilDescription();
            }
        }
        
        private void UpdatePlaystyleInfo()
        {
            if (playstyleMetrics == null) return;
            
            if (archetypeText != null)
                archetypeText.text = $"Archetype: {playstyleMetrics.PlaystyleArchetype}";
            
            if (favoriteRealmText != null)
                favoriteRealmText.text = $"Favorite Realm: {playstyleMetrics.FavoriteRealm}";
            
            if (playTimeText != null)
                playTimeText.text = $"Play Time: {Mathf.RoundToInt(playstyleMetrics.totalPlayTimeMinutes)} minutes";
            
            if (interactionStyleText != null)
                interactionStyleText.text = $"Style: {playstyleMetrics.DominantInteraction}";
        }
        
        private string GenerateSigilDescription()
        {
            if (playstyleMetrics == null) return "";
            
            string description = $"A {playstyleMetrics.PlaystyleArchetype.ToLower()} sigil forged from ";
            
            // Add favorite realm influence
            description += $"{playstyleMetrics.FavoriteRealm.ToLower()} energy";
            
            // Add interaction style
            description += $", shaped by {playstyleMetrics.DominantInteraction.ToLower()} gestures";
            
            // Add accessibility influence
            if (playstyleMetrics.usesColorblindMode || playstyleMetrics.usesReducedMotion)
            {
                description += ", enhanced by accessibility wisdom";
            }
            
            description += $". Play time: {Mathf.RoundToInt(playstyleMetrics.totalPlayTimeMinutes)} minutes.";
            
            return description;
        }
        
        #endregion
        
        #region Animations
        
        private System.Collections.IEnumerator RevealSigilCoroutine()
        {
            isRevealing = true;
            
            // Play reveal sound
            if (AudioManager.Instance != null && sigilRevealSound != null)
                AudioManager.Instance.PlaySFX(sigilRevealSound);
            
            // Check accessibility
            bool reducedMotion = AccessibilityManager.Instance != null && 
                               AccessibilityManager.Instance.IsReducedMotionEnabled();
            
            if (!reducedMotion)
            {
                // Fade in with rotation
                float duration = 2f;
                float elapsed = 0f;
                
                CanvasGroup canvasGroup = sigilImage != null ? sigilImage.GetComponent<CanvasGroup>() : null;
                if (canvasGroup == null && sigilImage != null)
                    canvasGroup = sigilImage.gameObject.AddComponent<CanvasGroup>();
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    
                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                        
                        // Slow growing scale
                        float scale = Mathf.Lerp(0.5f, 1f, elapsed / duration);
                        sigilImage.transform.localScale = Vector3.one * scale;
                        
                        yield return null;
                    }
                    
                    canvasGroup.alpha = 1f;
                }
                
                // Activate glow effect
                if (glowEffect != null)
                    glowEffect.Play();
                
                if (sigilLight != null)
                    sigilLight.enabled = true;
            }
            else
            {
                // Simple fade in for reduced motion
                CanvasGroup canvasGroup = sigilImage != null ? sigilImage.GetComponent<CanvasGroup>() : null;
                if (canvasGroup == null && sigilImage != null)
                    canvasGroup = sigilImage.gameObject.AddComponent<CanvasGroup>();
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    float duration = 0.5f;
                    float elapsed = 0f;
                    
                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                        yield return null;
                    }
                    
                    canvasGroup.alpha = 1f;
                }
            }
            
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Success);
            
            isRevealing = false;
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnShareClicked()
        {
            if (AudioManager.Instance != null && shareSound != null)
                AudioManager.Instance.PlaySFX(shareSound);
            
            if (sharePanel != null)
            {
                sharePanel.SetActive(true);
            }
            else
            {
                // Direct share
                ShareSigil();
            }
        }
        
        private void OnRegenerateClicked()
        {
            // Regenerate sigil with current playstyle
            GenerateSigil();
            StartCoroutine(RevealSigilCoroutine());
        }
        
        #endregion
        
        #region Sharing
        
        public void ShareSigil()
        {
            if (currentSigilTexture == null) return;
            
            // Save to gallery/photos
            string path = Application.persistentDataPath + "/sigil_share.png";
            byte[] bytes = currentSigilTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            
            // Use native share (platform-specific)
            #if UNITY_ANDROID || UNITY_IOS
                // Mobile share
                ShareToSocial(path);
            #else
                Debug.Log($"Sigil saved for sharing: {path}");
            #endif
            
            // Record achievement
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.UnlockAchievement("share_sigil");
            }
            
            if (sharePanel != null)
                sharePanel.SetActive(false);
        }
        
        private void ShareToSocial(string imagePath)
        {
            // Platform-specific sharing
            // This would use native sharing plugins
            Debug.Log($"Sharing sigil from: {imagePath}");
        }
        
        public void CloseSharePanel()
        {
            if (sharePanel != null)
                sharePanel.SetActive(false);
        }
        
        #endregion
    }
}
