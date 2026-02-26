using UnityEngine;
using UnityEngine.UI;
using AscendantContinuum.Audio;
using AscendantContinuum.Progression;
using System.Collections;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Enhanced main menu with animated background, floating sigils, ambient music, particles
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image[] parallaxLayers = new Image[3];
        [SerializeField] private float[] parallaxSpeeds = { 0.02f, 0.05f, 0.1f };

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text titleText;

        [Header("Floating Sigils")]
        [SerializeField] private GameObject sigilPrefab;
        [SerializeField] private int sigilCount = 5;
        [SerializeField] private float sigilSpeed = 0.5f;
        [SerializeField] private float sigilBobAmount = 0.3f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem particleDrift;
        [SerializeField] private float particleIntensity = 1f;

        [Header("Audio")]
        [SerializeField] private AudioClip ambientMusic;
        [SerializeField] private float musicFadeInDuration = 2f;

        [Header("Animation")]
        [SerializeField] private float titleAnimationDuration = 1f;
        [SerializeField] private AnimationCurve titleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private FloatingSignilUI[] floatingSignils;
        private CanvasGroup titleCanvasGroup;
        private CanvasGroup buttonsCanvasGroup;
        private Vector3[] parallaxStartPositions;

        private void Awake()
        {
            // Setup canvas groups for fade animations
            if (titleText != null && titleText.GetComponent<CanvasGroup>() == null)
                titleCanvasGroup = titleText.gameObject.AddComponent<CanvasGroup>();
            else
                titleCanvasGroup = titleText?.GetComponent<CanvasGroup>();

            if (playButton != null && playButton.GetComponent<CanvasGroup>() == null)
                buttonsCanvasGroup = playButton.transform.parent.GetComponent<CanvasGroup>();

            // Store parallax starting positions
            parallaxStartPositions = new Vector3[parallaxLayers.Length];
            for (int i = 0; i < parallaxLayers.Length; i++)
            {
                if (parallaxLayers[i] != null)
                    parallaxStartPositions[i] = parallaxLayers[i].rectTransform.anchoredPosition;
            }
        }

        private void Start()
        {
            InitializeUI();
            SpawnFloatingSignils();
            PlayAmbientMusic();
            StartCoroutine(AnimateMenuEntrance());
        }

        private void Update()
        {
            UpdateParallaxBackground();
            UpdateFloatingSignils();
        }

        /// <summary>
        /// Initialize UI elements and button listeners
        /// </summary>
        private void InitializeUI()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (continueButton != null)
            {
                ProgressionManager progMgr = ProgressionManager.Instance;
                bool hasSave = progMgr != null && progMgr.GetRealmProgress().Count > 0;
                continueButton.gameObject.SetActive(hasSave);
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            // Set title with glow effect
            if (titleText != null)
            {
                titleText.text = "Ascendant Continuum";
                titleCanvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Spawn floating procedurally-generated sigils
        /// </summary>
        private void SpawnFloatingSignils()
        {
            if (sigilPrefab == null)
            {
                Debug.LogWarning("[MainMenu] Sigil prefab not assigned, skipping sigils");
                return;
            }

            floatingSignils = new FloatingSignilUI[sigilCount];
            Canvas mainCanvas = GetComponentInParent<Canvas>();

            for (int i = 0; i < sigilCount; i++)
            {
                GameObject sigilObj = Instantiate(sigilPrefab, mainCanvas.transform);
                RectTransform rectTransform = sigilObj.GetComponent<RectTransform>();

                // Position randomly on screen
                float randomX = Random.Range(-500f, 500f);
                float randomY = Random.Range(-300f, 300f);
                rectTransform.anchoredPosition = new Vector2(randomX, randomY);

                // Get floating sigil component
                FloatingSignilUI floatingSignil = sigilObj.GetComponent<FloatingSignilUI>();
                if (floatingSignil == null)
                    floatingSignil = sigilObj.AddComponent<FloatingSignilUI>();

                floatingSignil.Initialize(sigilSpeed, sigilBobAmount, (float)i / sigilCount);
                floatingSignils[i] = floatingSignil;
            }

            Debug.Log($"[MainMenu] Spawned {sigilCount} floating sigils");
        }

        /// <summary>
        /// Update parallax background layers
        /// </summary>
        private void UpdateParallaxBackground()
        {
            if (parallaxLayers.Length == 0) return;

            // Simple horizontal drift
            for (int i = 0; i < parallaxLayers.Length; i++)
            {
                if (parallaxLayers[i] != null)
                {
                    float xOffset = (Time.time * parallaxSpeeds[i]) % 1000f;
                    Vector3 newPos = parallaxStartPositions[i];
                    newPos.x += xOffset;
                    parallaxLayers[i].rectTransform.anchoredPosition = newPos;
                }
            }
        }

        /// <summary>
        /// Update floating sigil animations
        /// </summary>
        private void UpdateFloatingSignils()
        {
            if (floatingSignils == null) return;

            foreach (FloatingSignilUI sigil in floatingSignils)
            {
                if (sigil != null)
                    sigil.UpdateAnimation(Time.deltaTime);
            }
        }

        /// <summary>
        /// Play ambient background music
        /// </summary>
        private void PlayAmbientMusic()
        {
            if (AudioManager.Instance != null && ambientMusic != null)
            {
                AudioManager.Instance.PlayMusic(ambientMusic, musicFadeInDuration);
            }

            // Start particle drift
            if (particleDrift != null)
            {
                particleDrift.Play();
                var emission = particleDrift.emission;
                emission.rateOverTime = emission.rateOverTime.constant * particleIntensity;
            }
        }

        /// <summary>
        /// Animate menu entrance
        /// </summary>
        private IEnumerator AnimateMenuEntrance()
        {
            // Fade in title
            if (titleCanvasGroup != null)
            {
                float elapsedTime = 0f;
                while (elapsedTime < titleAnimationDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = titleAnimationCurve.Evaluate(elapsedTime / titleAnimationDuration);
                    titleCanvasGroup.alpha = t;
                    yield return null;
                }
                titleCanvasGroup.alpha = 1f;
            }

            // Fade in buttons slightly delayed
            yield return new WaitForSeconds(0.3f);
            if (buttonsCanvasGroup != null)
            {
                float elapsedTime = 0f;
                while (elapsedTime < titleAnimationDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = titleAnimationCurve.Evaluate(elapsedTime / titleAnimationDuration);
                    buttonsCanvasGroup.alpha = t;
                    yield return null;
                }
                buttonsCanvasGroup.alpha = 1f;
            }
        }

        private void OnPlayClicked()
        {
            if (playButton.GetComponent<ButtonJuice>() != null)
            {
                // ButtonJuice will animate the click
            }

            StartCoroutine(TransitionToRealm());
        }

        private void OnContinueClicked()
        {
            // Resume from last realm
            StartCoroutine(TransitionToRealm());
        }

        private void OnSettingsClicked()
        {
            // Open settings panel (audio volume sliders, accessibility, etc.)
            Debug.Log("[MainMenu] Settings button clicked");
        }

        private IEnumerator TransitionToRealm()
        {
            // Fade out music
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(null, 1f);

            // Fade out UI
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float elapsedTime = 0f;
                float fadeDuration = 1f;
                while (elapsedTime < fadeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    canvasGroup.alpha = 1f - (elapsedTime / fadeDuration);
                    yield return null;
                }
            }

            // Load realm (would call GameManager or RealmTransitionManager)
            Debug.Log("[MainMenu] Transitioning to realm...");
        }

        private void OnDestroy()
        {
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }

    /// <summary>
    /// Component for floating sigils with smooth animation
    /// </summary>
    public class FloatingSignilUI : MonoBehaviour
    {
        private float speed;
        private float bobAmount;
        private float phaseOffset;
        private Vector3 startPosition;
        private Image sigilImage;

        public void Initialize(float moveSpeed, float bob, float phase)
        {
            speed = moveSpeed;
            bobAmount = bob;
            phaseOffset = phase * 2f * Mathf.PI; // Convert phase to radians
            startPosition = GetComponent<RectTransform>().anchoredPosition;
            sigilImage = GetComponent<Image>();

            // Procedurally generate sigil visual (placeholder for now)
            if (sigilImage != null)
            {
                sigilImage.color = new Color(
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f),
                    0.8f
                );
            }
        }

        public void UpdateAnimation(float deltaTime)
        {
            if (GetComponent<RectTransform>() == null) return;

            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector3 currentPos = rectTransform.anchoredPosition;

            // Circular motion with bobbing
            float time = Time.time * speed + phaseOffset;
            float x = startPosition.x + Mathf.Cos(time) * bobAmount;
            float y = startPosition.y + Mathf.Sin(time) * bobAmount;

            rectTransform.anchoredPosition = new Vector2(x, y);

            // Gentle rotation
            rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(time * 0.5f) * 5f);
        }
    }
}
