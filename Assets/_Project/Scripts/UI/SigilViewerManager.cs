using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Data;

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

            // Generate sigil data then render to Texture2D
            var sigilData = SigilGenerator.Instance.GenerateSigil(playstyleMetrics);
            currentSigilTexture = SigilGenerator.Instance.RenderSigil(sigilData);

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
            // Load from SaveSystem if available, else fall back to PlayerPrefs / defaults
            var playerData = Core.SaveSystem.Instance?.CurrentPlayerData;

            return new PlayerPlaystyleMetrics
            {
                totalPlayTimeMinutes = playerData != null ? playerData.totalPlayTime / 60f : PlayerPrefs.GetFloat("TotalPlayTimeSeconds", 7200f) / 60f,
                emberforgeVisits = PlayerPrefs.GetInt("Visits_Emberforge", 0),
                verdantSanctuaryVisits = PlayerPrefs.GetInt("Visits_Verdant", 0),
                averageActionSpeed = PlayerPrefs.GetFloat("AverageActionSpeed", 1.0f)
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
#if UNITY_ANDROID
            try
            {
                using var intentClass = new AndroidJavaClass("android.content.Intent");
                string actionSend    = intentClass.GetStatic<string>("ACTION_SEND");
                using var intent     = new AndroidJavaObject("android.content.Intent", actionSend);
                intent.Call<AndroidJavaObject>("setType", "image/png");

                // Try FileProvider (Android 7+, API 24+), fall back to file:// on older devices
                AndroidJavaObject uri;
                try
                {
                    using var player  = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var ctx     = player.GetStatic<AndroidJavaObject>("currentActivity");
                    using var file    = new AndroidJavaObject("java.io.File", imagePath);
                    uri = new AndroidJavaClass("androidx.core.content.FileProvider")
                        .CallStatic<AndroidJavaObject>(
                            "getUriForFile", ctx,
                            Application.identifier + ".fileprovider", file);
                    intent.Call<AndroidJavaObject>("addFlags", 0x00000001); // FLAG_GRANT_READ_URI_PERMISSION
                }
                catch
                {
                    using var uriClass = new AndroidJavaClass("android.net.Uri");
                    uri = uriClass.CallStatic<AndroidJavaObject>(
                        "fromFile", new AndroidJavaObject("java.io.File", imagePath));
                }

                intent.Call<AndroidJavaObject>("putExtra",
                    intentClass.GetStatic<string>("EXTRA_STREAM"), uri);
                intent.Call<AndroidJavaObject>("putExtra",
                    intentClass.GetStatic<string>("EXTRA_TEXT"),
                    "My cosmic sigil from The Ascendant Continuum \u2726");

                using var player2   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity  = player2.GetStatic<AndroidJavaObject>("currentActivity");
                var chooser = intentClass.CallStatic<AndroidJavaObject>(
                    "createChooser", intent, "Share Sigil");
                activity.Call("startActivity", chooser);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SigilViewer] Share sheet failed: {ex.Message}");
                HUDManager.Instance?.ShowNotification(
                    "Sigil saved to device.", HUDManager.NotificationType.Info);
            }
#elif UNITY_IOS
            // Copy path to clipboard; a NativeShare plugin upgrade will replace this
            GUIUtility.systemCopyBuffer = imagePath;
            HUDManager.Instance?.ShowNotification(
                "Sigil saved \u2014 share from your Files app.", HUDManager.NotificationType.Info);
#else
            Debug.Log($"[SigilViewer] Share (Editor): {imagePath}");
#endif
        }

        public void CloseSharePanel()
        {
            if (sharePanel != null)
                sharePanel.SetActive(false);
        }

        #endregion
    }
}
