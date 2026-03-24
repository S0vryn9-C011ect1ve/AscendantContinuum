using UnityEngine;
using UnityEngine.UI;
using AscendantContinuum.Core;
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
        [SerializeField] private Button seasonPassButton;
        [SerializeField] private Text titleText;

        [Header("Realm Select Buttons")]
        [Tooltip("Assign the 5 realm buttons in order: Emberforge, Verdant, EchoFields, DawnCitadel, LanternAscension")]
        [SerializeField] private Button[] realmSelectButtons = new Button[5];

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

        [Header("Settings")]
        [SerializeField] private SettingsPanel settingsPanel;

        [Header("Animation")]
        [SerializeField] private float titleAnimationDuration = 1f;
        [SerializeField] private AnimationCurve titleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private FloatingSignilUI[] floatingSignils;
        private CanvasGroup titleCanvasGroup;
        private CanvasGroup buttonsCanvasGroup;
        private Vector3[] parallaxStartPositions;
        private bool allowMotionEffects = true;

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
            allowMotionEffects = !(AccessibilityManager.Instance?.ReducedMotionEnabled ?? false);

            InitializeUI();
            if (allowMotionEffects)
            {
                SpawnFloatingSignils();
            }
            PlayAmbientMusic();
            StartCoroutine(AnimateMenuEntrance());
        }

        private void Update()
        {
            if (!allowMotionEffects)
            {
                return;
            }

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

            if (seasonPassButton != null)
                seasonPassButton.onClick.AddListener(() => SeasonPassUIManager.Instance?.OpenPanel());

            TryAutoBindRealmButtons();
            SetRealmButtonsVisible(false);
            InitializeRealmButtons();

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
                if (allowMotionEffects)
                {
                    particleDrift.Play();
                    var emission = particleDrift.emission;
                    emission.rateOverTime = emission.rateOverTime.constant * particleIntensity;
                }
                else
                {
                    particleDrift.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
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
            SetRealmButtonsVisible(true);
        }

        /// <summary>Wire each realm-select button to its scene and lock it based on progression.</summary>
        private void InitializeRealmButtons()
        {
            if (realmSelectButtons == null || realmSelectButtons.Length == 0) return;

            string[] realmScenes = {
                SceneNames.Emberforge,
                SceneNames.Verdant,
                SceneNames.EchoFields,
                SceneNames.DawnCitadel,
                SceneNames.LanternAscension
            };
            string[] realmIds = { "emberforge", "verdant", "echo", "dawn", "lantern" };

            for (int i = 0; i < realmSelectButtons.Length && i < realmScenes.Length; i++)
            {
                var btn = realmSelectButtons[i];
                if (btn == null) continue;

                bool unlocked = ProgressionManager.Instance?.IsRealmUnlocked(realmIds[i]) ?? (i == 0);
                btn.interactable = unlocked;

                string sceneName = realmScenes[i]; // capture for closure
                btn.onClick.AddListener(() => StartCoroutine(TransitionToSpecificRealm(sceneName)));
            }
        }

        private IEnumerator TransitionToSpecificRealm(string sceneName)
        {
            AudioManager.Instance?.StopMusic(1f);

            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime;
                    cg.alpha = Mathf.Clamp01(1f - t);
                    yield return null;
                }
            }

            if (!SceneService.TryLoadScene(sceneName))
                SceneService.TryLoadScene(SceneNames.Emberforge);
        }

        private void OnContinueClicked()
        {
            // Resume from last realm
            StartCoroutine(TransitionToRealm());
        }

        private void OnSettingsClicked()
        {
            if (settingsPanel != null)
                settingsPanel.Show();
            else
                Debug.LogWarning("[MainMenu] SettingsPanel not assigned — run the builder tool.");
        }

        private IEnumerator TransitionToRealm()
        {
            // Fade out music
            if (AudioManager.Instance != null)
                AudioManager.Instance.StopMusic(1f);

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

            // Always route to a real scene as a safe fallback path.
            string lastRealm = GameManager.Instance != null
                ? GameManager.Instance.GetLastRealmOrDefault("emberforge")
                : "emberforge";

            string targetScene = SceneNames.FromRealmId(lastRealm);
            if (!SceneService.TryLoadScene(targetScene))
            {
                SceneService.TryLoadScene(SceneNames.Emberforge);
            }
        }

        private void TryAutoBindRealmButtons()
        {
            if (realmSelectButtons != null && realmSelectButtons.Length == 5)
            {
                bool allAssigned = true;
                for (int i = 0; i < realmSelectButtons.Length; i++)
                {
                    if (realmSelectButtons[i] == null)
                    {
                        allAssigned = false;
                        break;
                    }
                }
                if (allAssigned) return;
            }

            realmSelectButtons = new Button[5];
            realmSelectButtons[0] = FindButtonByName("RealmBtn_Emberforge");
            realmSelectButtons[1] = FindButtonByName("RealmBtn_Verdant");
            realmSelectButtons[2] = FindButtonByName("RealmBtn_EchoFields");
            realmSelectButtons[3] = FindButtonByName("RealmBtn_DawnCitadel");
            realmSelectButtons[4] = FindButtonByName("RealmBtn_LanternAscension");
        }

        private static Button FindButtonByName(string buttonName)
        {
            var allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allButtons.Length; i++)
            {
                if (allButtons[i] != null && allButtons[i].name == buttonName)
                    return allButtons[i];
            }

            return null;
        }

        private void SetRealmButtonsVisible(bool visible)
        {
            if (realmSelectButtons == null) return;

            for (int i = 0; i < realmSelectButtons.Length; i++)
            {
                if (realmSelectButtons[i] != null)
                    realmSelectButtons[i].gameObject.SetActive(visible);
            }
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
        private RectTransform rectTransform;

        public void Initialize(float moveSpeed, float bob, float phase)
        {
            speed = moveSpeed;
            bobAmount = bob;
            phaseOffset = phase * 2f * Mathf.PI; // Convert phase to radians
            rectTransform = GetComponent<RectTransform>();
            startPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector3.zero;
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
            if (rectTransform == null) return;

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
