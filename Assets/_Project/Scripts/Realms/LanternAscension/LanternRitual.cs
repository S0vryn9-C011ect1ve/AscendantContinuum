using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Social;

namespace AscendantContinuum.Realms.LanternAscension
{
    /// <summary>
    /// Manages the lantern release ritual in Lantern Ascension realm.
    /// Players create and release wish lanterns into the cosmic void.
    /// Provides meditation and reflection experience.
    /// </summary>
    public class LanternRitual : MonoBehaviour
    {
        [Header("Lantern Configuration")]
        [SerializeField] private GameObject lanternPrefab;
        [SerializeField] private Transform lanternReleasePoint;
        [SerializeField] private float ascensionSpeed = 0.5f;
        [SerializeField] private float maxLanternsVisible = 100;

        [Header("Wish System")]
        [SerializeField] private bool allowTextWishes = true;
        [SerializeField] private bool allowAnonymousSharing = true;
        [SerializeField] private int maxWishLength = 140; // Like a tweet
        [SerializeField] private GameObject wishInputPanel;
        [SerializeField] private TMP_InputField wishInputField;

        [Header("Visual Settings")]
        [SerializeField]
        private Color[] lanternColors = new Color[]
        {
            new Color(1f, 0.7f, 0.3f), // Warm amber
            new Color(1f, 0.5f, 0.2f), // Soft orange
            new Color(0.9f, 0.8f, 0.6f), // Pale gold
            new Color(1f, 0.9f, 0.7f) // Cream
        };
        [SerializeField] private ParticleSystem stardustEffect;

        [Header("Audio")]
        [SerializeField] private AudioClip lanternCreateSound;
        [SerializeField] private AudioClip lanternReleaseSound;
        [SerializeField] private AudioClip ambientMusic;
        [SerializeField] private AudioClip breathingGuide;

        [Header("Meditation Mode")]
        [SerializeField] private bool meditationModeEnabled = false;
        [SerializeField] private float meditationDuration = 300f; // 5 minutes default
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float cameraDriftSpeed = 0.1f;

        // State
        private List<Lantern> activeLanterns = new List<Lantern>();
        private bool isCreatingLantern = false;
        private bool isMeditating = false;
        private string currentWish = "";
        private WishVisibility wishVisibility = WishVisibility.Private;

        // Accessibility
        private bool reducedMotion = false;
        private bool textToSpeechEnabled = false;
        private bool hapticBreathing = false;

        // Lunar / solstice boost (Winter Solstice: Solstice_LunarMultiplier > 1)
        private float lunarMultiplier = 1f;

        // Events
        public System.Action<Lantern> OnLanternReleased;
        public System.Action OnMeditationStarted;
        public System.Action OnMeditationEnded;

        [Header("Visibility UI")]
        [SerializeField] private GameObject visibilityPanel;
        [SerializeField] private UnityEngine.UI.Button visibilityPrivateBtn;
        [SerializeField] private UnityEngine.UI.Button visibilityAnonymousBtn;
        [SerializeField] private UnityEngine.UI.Button visibilityPublicBtn;

        [Header("Completion")]
        [SerializeField] private AscendantContinuum.UI.RealmCompletionPanel completionPanel;
        [SerializeField] private int goalLanterns = 1;
        private int _lanternsReleased = 0;
        private bool _completionShown = false;

        public enum WishVisibility
        {
            Private,      // Only you see it
            Anonymous,    // Others see it, but no author
            Public        // Others see it with your identifier (if allowed)
        }

        private void Start()
        {
            // Check accessibility settings
            if (AccessibilityManager.Instance != null)
            {
                reducedMotion = AccessibilityManager.Instance.IsReducedMotionEnabled();
                textToSpeechEnabled = AccessibilityManager.Instance.ScreenReaderEnabled;
            }

            // Load lunar solstice multiplier (Winter Solstice boosts lantern power)
            lunarMultiplier = PlayerPrefs.GetFloat("Solstice_LunarMultiplier", 1f);
            // Apply to shader global so VFX glow responds too
            Shader.SetGlobalFloat("_LunarPowerBoost", lunarMultiplier);

            // Start ambient music
            if (AudioManager.Instance != null && ambientMusic != null)
            {
                AudioManager.Instance.PlayMusic(ambientMusic);
            }

            // Load existing lanterns from other players
            LoadCommunityLanterns();
        }

        private void Update()
        {
            // Update lantern positions
            UpdateLanterns();

            // Update meditation camera if active
            if (isMeditating)
            {
                UpdateMeditationCamera();
            }
        }

        #region Lantern Creation

        /// <summary>
        /// Starts the lantern creation flow
        /// </summary>
        public void BeginLanternCreation()
        {
            if (isCreatingLantern) return;

            isCreatingLantern = true;

            // Play creation sound
            if (AudioManager.Instance != null && lanternCreateSound != null)
                AudioManager.Instance.PlaySFX(lanternCreateSound);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Light);

            // Show wish input if text wishes enabled
            if (allowTextWishes && wishInputPanel != null)
            {
                ShowWishInput();
            }
            else
            {
                // Create lantern immediately without text
                CreateAndReleaseLantern("", WishVisibility.Private);
            }
        }

        private void ShowWishInput()
        {
            wishInputPanel.SetActive(true);

            if (wishInputField != null)
            {
                wishInputField.characterLimit = maxWishLength;
                wishInputField.text = "";
                wishInputField.Select();
                wishInputField.ActivateInputField();
            }
        }

        /// <summary>
        /// Called when player confirms their wish
        /// </summary>
        public void ConfirmWish()
        {
            currentWish = wishInputField != null ? wishInputField.text : "";

            // Ask about visibility if sharing is allowed
            if (allowAnonymousSharing && !string.IsNullOrEmpty(currentWish))
            {
                ShowVisibilityOptions();
            }
            else
            {
                CreateAndReleaseLantern(currentWish, WishVisibility.Private);
            }
        }

        private void ShowVisibilityOptions()
        {
            if (visibilityPanel != null)
            {
                // Wire buttons on first show
                if (visibilityPrivateBtn != null)
                    visibilityPrivateBtn.onClick.AddListener(() => ConfirmVisibility(WishVisibility.Private));
                if (visibilityAnonymousBtn != null)
                    visibilityAnonymousBtn.onClick.AddListener(() => ConfirmVisibility(WishVisibility.Anonymous));
                if (visibilityPublicBtn != null)
                    visibilityPublicBtn.onClick.AddListener(() => ConfirmVisibility(WishVisibility.Public));

                visibilityPanel.SetActive(true);
            }
            else
            {
                // No panel assigned — fall back to Anonymous
                ConfirmVisibility(WishVisibility.Anonymous);
            }
        }

        private void ConfirmVisibility(WishVisibility visibility)
        {
            // Remove listeners to prevent double-firing
            if (visibilityPrivateBtn   != null) visibilityPrivateBtn.onClick.RemoveAllListeners();
            if (visibilityAnonymousBtn != null) visibilityAnonymousBtn.onClick.RemoveAllListeners();
            if (visibilityPublicBtn    != null) visibilityPublicBtn.onClick.RemoveAllListeners();

            if (visibilityPanel != null) visibilityPanel.SetActive(false);

            wishVisibility = visibility;
            CreateAndReleaseLantern(currentWish, wishVisibility);
        }

        /// <summary>
        /// Creates and releases a lantern with optional wish.
        /// When textToSpeechEnabled, announces the wish text via Debug.Log
        /// (production: route to platform TTS API via a NativePlugin bridge).
        /// </summary>
        public void CreateAndReleaseLantern(string wish, WishVisibility visibility)
        {
            if (lanternPrefab == null) return;

            // Screen-reader / TTS announcement
            if (textToSpeechEnabled && !string.IsNullOrEmpty(wish))
            {
                string announcement = $"Lantern released with wish: {wish}";
#if UNITY_ANDROID && !UNITY_EDITOR
                // Android: use AndroidJavaClass to invoke TTS via NativeTextToSpeech plugin if present
                try
                {
                    using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        using var tts = new AndroidJavaObject("android.speech.tts.TextToSpeech",
                            activity, null);
                        tts.Call<int>("speak", announcement,
                            new AndroidJavaClass("android.speech.tts.TextToSpeech")
                                .GetStatic<int>("QUEUE_FLUSH"), null, "lantern_wish");
                    }));
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[TTS] Android TTS failed: {ex.Message}");
                }
#elif UNITY_IOS && !UNITY_EDITOR
                // iOS: bridge via NativeTextToSpeech plugin or AVSpeechSynthesizer bridge
                // Requires a native iOS plugin (NativeAudioPlugin / NativeTTS bridge)
                // Plugin contract: NativeTextToSpeech.Speak(string text)
                // Until plugin is imported, log to console so QA can verify the flow.
                Debug.Log($"[TTS-iOS] Would speak: {announcement}");
#else
                Debug.Log($"[TTS] {announcement}");
#endif
            }

            // Instantiate lantern
            Vector3 spawnPos = lanternReleasePoint != null ? lanternReleasePoint.position : transform.position;
            GameObject lanternObj = Instantiate(lanternPrefab, spawnPos, Quaternion.identity);
            Lantern lantern = lanternObj.GetComponent<Lantern>();

            if (lantern == null)
                lantern = lanternObj.AddComponent<Lantern>();

            // Initialize lantern
            Color lanternColor = lanternColors[Random.Range(0, lanternColors.Length)];
            // Winter solstice (Solstice_LunarMultiplier > 1) boosts lantern brightness and speed
            float boostedSpeed = ascensionSpeed * lunarMultiplier;
            lantern.Initialize(wish, visibility, lanternColor, reducedMotion, boostedSpeed, lunarMultiplier);

            // Add to active list
            activeLanterns.Add(lantern);

            // Cleanup if too many
            if (activeLanterns.Count > maxLanternsVisible)
            {
                Lantern oldest = activeLanterns[0];
                activeLanterns.RemoveAt(0);
                if (oldest != null)
                    Destroy(oldest.gameObject);
            }

            // Play release sound
            if (AudioManager.Instance != null && lanternReleaseSound != null)
                AudioManager.Instance.PlaySFX(lanternReleaseSound);

            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.TriggerHaptic(HapticType.Success);

            // Spawn stardust effect
            if (stardustEffect != null && !reducedMotion)
            {
                stardustEffect.transform.position = spawnPos;
                stardustEffect.Play();
            }

            // Close wish input
            if (wishInputPanel != null)
                wishInputPanel.SetActive(false);

            isCreatingLantern = false;

            // Save to Firebase (if sharing)
            if (visibility != WishVisibility.Private)
            {
                SaveLanternToCloud(wish, visibility);

                // Also queue in the Cross-Player Wish Wall so it drifts through other players' realms
                var wishWallVisibility = visibility == WishVisibility.Anonymous
                    ? Social.WishVisibility.Anonymous
                    : Social.WishVisibility.Public;
                Social.CrossPlayerWishWall.Instance?.ReleaseWish(wish, wishWallVisibility);
            }

            // Trigger event
            OnLanternReleased?.Invoke(lantern);

            _lanternsReleased++;

            // Realm completion check
            if (!_completionShown && _lanternsReleased >= goalLanterns && completionPanel != null)
            {
                _completionShown = true;
                completionPanel.ShowCompletion("Lantern Ascension Complete! 🏮",
                    "Your wish ascends to the cosmos…", 3);
                Core.GameEvents.RaiseRealmCompleted("lantern", _lanternsReleased);
            }

            // Record achievement
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.UnlockAchievement("first_wish");
            }
        }

        #endregion

        #region Lantern Management

        private void UpdateLanterns()
        {
            for (int i = activeLanterns.Count - 1; i >= 0; i--)
            {
                if (activeLanterns[i] == null)
                {
                    activeLanterns.RemoveAt(i);
                    continue;
                }

                // Move lantern upward (each lantern owns its ascensionSpeed)
                activeLanterns[i].Ascend(0f);

                // Remove if too far away
                if (activeLanterns[i].transform.position.y > 1000f)
                {
                    Destroy(activeLanterns[i].gameObject);
                    activeLanterns.RemoveAt(i);
                }
            }
        }

        private void LoadCommunityLanterns()
        {
            // Query Firebase for recent public/anonymous wishes from other players.
            // Falls back to locally-stored community snapshot when offline.
            StartCoroutine(LoadCommunityLanternsCoroutine());
        }

        private IEnumerator LoadCommunityLanternsCoroutine()
        {
            // Try to load from Firebase
            bool loadedFromCloud = false;
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsInitialized)
            {
                var task = FirebaseManager.Instance.LoadPlayerData<List<CommunityWishData>>(
                    "community_lanterns", "recent_wishes");

                float timeout = 5f;
                float elapsed = 0f;
                while (!task.IsCompleted && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (task.IsCompletedSuccessfully && task.Result != null)
                {
                    loadedFromCloud = true;
                    foreach (var wish in task.Result)
                    {
                        yield return new WaitForSeconds(Random.Range(1f, 3f));
                        SpawnCommunityLantern(wish.text ?? "A wish from a fellow traveller");
                    }
                }
            }

            if (!loadedFromCloud)
            {
                // Offline fallback — spawn placeholder community lanterns
                yield return StartCoroutine(SpawnCommunityLanternsCoroutine());
            }
        }

        [System.Serializable]
        private class CommunityWishData
        {
            public string text;
            public string visibility;
        }

        private void SpawnCommunityLantern(string wishText)
        {
            if (lanternPrefab == null) return;
            Vector3 randomPos = new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-5f, 20f),
                Random.Range(5f, 20f));
            GameObject lanternObj = Instantiate(lanternPrefab, randomPos, Quaternion.identity);
            Lantern lantern = lanternObj.GetComponent<Lantern>() ?? lanternObj.AddComponent<Lantern>();
            Color lanternColor = lanternColors[Random.Range(0, lanternColors.Length)];
            lantern.Initialize(wishText, WishVisibility.Anonymous, lanternColor, reducedMotion, ascensionSpeed * lunarMultiplier, lunarMultiplier);
            activeLanterns.Add(lantern);
        }

        private IEnumerator SpawnCommunityLanternsCoroutine()
        {
            if (lanternPrefab == null)
            {
                Debug.LogWarning("[LanternRitual] lanternPrefab is not assigned. Skipping community lantern spawns.");
                yield break;
            }

            // Gradually spawn community lanterns over time
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForSeconds(Random.Range(2f, 5f));

                // Create a lantern at random position
                Vector3 randomPos = new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(-5f, 20f),
                    Random.Range(5f, 20f)
                );

                GameObject lanternObj = Instantiate(lanternPrefab, randomPos, Quaternion.identity);
                Lantern lantern = lanternObj.GetComponent<Lantern>();

                if (lantern == null)
                    lantern = lanternObj.AddComponent<Lantern>();

                Color lanternColor = lanternColors[Random.Range(0, lanternColors.Length)];
                lantern.Initialize("Anonymous wish from another player", LanternRitual.WishVisibility.Anonymous, lanternColor, reducedMotion, ascensionSpeed * lunarMultiplier, lunarMultiplier);

                activeLanterns.Add(lantern);
            }
        }

        private void SaveLanternToCloud(string wish, WishVisibility visibility)
        {
            if (FirebaseManager.Instance == null) return;

            // Create lantern data
            var lanternData = new Dictionary<string, object>
            {
                { "wish", wish },
                { "visibility", visibility.ToString() },
                { "timestamp", System.DateTime.UtcNow.ToString("o") },
                { "realm", "LanternAscension" }
            };

            // Save to Firestore (anonymous)
            FirebaseManager.Instance.SaveData("communityLanterns", System.Guid.NewGuid().ToString(), lanternData);
        }

        #endregion

        #region Meditation Mode

        /// <summary>
        /// Enters meditation/observation mode
        /// </summary>
        public void StartMeditation(float duration = 300f)
        {
            if (isMeditating) return;

            isMeditating = true;
            // Use the serialized meditationDuration unless caller provides override
            meditationDuration = duration > 0 ? duration : meditationDuration;
            // meditationModeEnabled lets designers pre-configure this realm as
            // always starting in meditation mode (camera drift from first frame).
            if (meditationModeEnabled)
                Debug.Log("[LanternRitual] Meditation mode pre-enabled by designer.");

            // Disable player controls
            // Enable gentle camera drift

            if (AudioManager.Instance != null && breathingGuide != null && hapticBreathing)
            {
                StartCoroutine(HapticBreathingGuide());
            }

            OnMeditationStarted?.Invoke();

            // Start meditation timer
            StartCoroutine(MeditationTimerCoroutine());
        }

        /// <summary>
        /// Exits meditation mode
        /// </summary>
        public void EndMeditation()
        {
            if (!isMeditating) return;

            isMeditating = false;

            // Re-enable player controls

            OnMeditationEnded?.Invoke();

            // Record achievement
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.UnlockAchievement("meditation_complete");
            }
        }

        private void UpdateMeditationCamera()
        {
            if (cameraTransform == null) return;

            // Gentle drift through lantern field
            float driftX = Mathf.Sin(Time.time * cameraDriftSpeed * 0.5f) * 2f;
            float driftY = Mathf.Cos(Time.time * cameraDriftSpeed * 0.3f) * 1f;
            float driftZ = Time.time * cameraDriftSpeed * 0.1f;

            cameraTransform.position = new Vector3(driftX, driftY, driftZ);

            // Slowly rotate to follow nearest lantern
            if (activeLanterns.Count > 0)
            {
                Lantern nearest = FindNearestLantern();
                if (nearest != null)
                {
                    Vector3 lookDirection = (nearest.transform.position - cameraTransform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, Time.deltaTime * 0.5f);
                }
            }
        }

        private Lantern FindNearestLantern()
        {
            if (activeLanterns.Count == 0) return null;

            Lantern nearest = activeLanterns[0];
            float minDistance = Vector3.Distance(cameraTransform.position, nearest.transform.position);

            foreach (var lantern in activeLanterns)
            {
                float distance = Vector3.Distance(cameraTransform.position, lantern.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = lantern;
                }
            }

            return nearest;
        }

        private IEnumerator MeditationTimerCoroutine()
        {
            yield return new WaitForSeconds(meditationDuration);
            EndMeditation();
        }

        private IEnumerator HapticBreathingGuide()
        {
            while (isMeditating)
            {
                // Breathe in (4 seconds)
                for (int i = 0; i < 4; i++)
                {
                    if (AccessibilityManager.Instance != null)
                        AccessibilityManager.Instance.TriggerHaptic(HapticType.Light);
                    yield return new WaitForSeconds(1f);
                }

                // Hold (4 seconds)
                yield return new WaitForSeconds(4f);

                // Breathe out (4 seconds)
                for (int i = 0; i < 4; i++)
                {
                    if (AccessibilityManager.Instance != null)
                        AccessibilityManager.Instance.TriggerHaptic(HapticType.Light);
                    yield return new WaitForSeconds(1f);
                }

                // Pause (2 seconds)
                yield return new WaitForSeconds(2f);
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            OnLanternReleased = null;
            OnMeditationStarted = null;
            OnMeditationEnded = null;
        }

        #endregion
    }

    #region Lantern Class

    /// <summary>
    /// Represents an individual wish lantern
    /// </summary>
    public class Lantern : MonoBehaviour
    {
        [SerializeField] private Light lanternLight;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TextMeshPro wishText;
        [SerializeField] private ParticleSystem glowParticles;

        private string wish;
        private LanternRitual.WishVisibility visibility;
        private Color color;
        private bool reducedMotion;
        private float ascensionSpeed = 0.5f;
        private float lunarBoost = 1f;

        private float gentleSwayOffset;
        private float pulseOffset;

        private void Awake()
        {
            gentleSwayOffset = Random.Range(0f, Mathf.PI * 2f);
            pulseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        public void Initialize(string wishText, LanternRitual.WishVisibility vis, Color col, bool reducedMotionMode, float speed = 0.5f, float lunarMultiplier = 1f)
        {
            wish = wishText;
            visibility = vis;
            color = col;
            reducedMotion = reducedMotionMode;
            ascensionSpeed = speed;
            lunarBoost = lunarMultiplier;

            // Set color
            if (spriteRenderer != null)
                spriteRenderer.color = color;

            if (lanternLight != null)
            {
                lanternLight.color = color;
                // Lunar boost brightens the light (winter solstice = peak lunar power)
                lanternLight.intensity = 1f * lunarBoost;
            }

            // Set wish text (only if public/anonymous)
            if (wishText != null && visibility != LanternRitual.WishVisibility.Private)
            {
                this.wishText.text = wish;
                this.wishText.gameObject.SetActive(false); // Hidden until player looks closely
            }

            // Enable particles
            if (glowParticles != null && !reducedMotion)
            {
                var main = glowParticles.main;
                main.startColor = color;
                glowParticles.Play();
            }
        }

        public void Ascend(float distance)
        {
            // ascensionSpeed already incorporates lunarBoost from Initialize
            transform.position += Vector3.up * ascensionSpeed * Time.deltaTime;

            // Gentle sway
            if (!reducedMotion)
            {
                float sway = Mathf.Sin((Time.time + gentleSwayOffset) * 0.5f) * 0.2f;
                transform.position += Vector3.right * sway * Time.deltaTime;
            }

            // Gentle pulse
            float pulse = Mathf.Sin((Time.time + pulseOffset) * 1f) * 0.5f + 0.5f;
            if (lanternLight != null)
                lanternLight.intensity = Mathf.Lerp(0.8f, 1.2f, pulse);
        }

        private void OnMouseOver()
        {
            // Show wish text when player hovers
            if (wishText != null && visibility != LanternRitual.WishVisibility.Private)
            {
                wishText.gameObject.SetActive(true);
            }
        }

        private void OnMouseExit()
        {
            // Hide wish text
            if (wishText != null)
            {
                wishText.gameObject.SetActive(false);
            }
        }
    }

    #endregion
}
