using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Monitors device performance (FPS, thermals) and dynamically scales quality settings.
    /// Ensures the game maintains the mandatory 60 FPS accessibility target.
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        [Header("Performance Targets")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float minAcceptableFPS = 45f;
        [SerializeField] private float checkInterval = 5f; // Check every 5 seconds

        [Header("Scaling Actions")]
        [SerializeField] private bool disablePostProcessingOnLowFPS = true;
        [SerializeField] private bool reduceResolutionOnLowFPS = true;

        private float deltaTime = 0.0f;
        private float timer = 0.0f;
        private int frameCount = 0;
        private bool isDegraded = false;
        private float originalRenderScale = 1f;
        private bool hasCapturedRenderScale = false;

        private Volume globalVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Application.targetFrameRate = (int)targetFPS;
            CaptureOriginalRenderScale();
            RefreshGlobalVolume();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (isDegraded)
            {
                RestoreQuality();
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshGlobalVolume();
        }

        private void RefreshGlobalVolume()
        {
            globalVolume = UnityEngine.Object.FindFirstObjectByType<Volume>();

            if (globalVolume != null)
            {
                globalVolume.enabled = !isDegraded;
            }
        }

        private void CaptureOriginalRenderScale()
        {
            if (hasCapturedRenderScale)
            {
                return;
            }

            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                originalRenderScale = urpAsset.renderScale;
                hasCapturedRenderScale = true;
            }
        }

        private void Update()
        {
            // Calculate FPS
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            frameCount++;
            timer += Time.unscaledDeltaTime;

            if (timer >= checkInterval)
            {
                float currentFPS = frameCount / timer;
                EvaluatePerformance(currentFPS);

                // Reset counters
                frameCount = 0;
                timer = 0f;
            }
        }

        private void EvaluatePerformance(float currentFPS)
        {
            if (currentFPS < minAcceptableFPS && !isDegraded)
            {
                Debug.LogWarning($"[PerformanceManager] FPS dropped to {currentFPS:F1}. Scaling down quality.");
                ScaleDownQuality();
            }
            else if (currentFPS >= targetFPS - 2f && isDegraded)
            {
                // Only scale back up if we are consistently hitting target FPS
                Debug.Log($"[PerformanceManager] FPS recovered to {currentFPS:F1}. Restoring quality.");
                RestoreQuality();
            }
        }

        private void ScaleDownQuality()
        {
            isDegraded = true;

            // 1. Disable Post Processing
            if (disablePostProcessingOnLowFPS && globalVolume != null)
            {
                globalVolume.enabled = false;
                Debug.Log("[PerformanceManager] Disabled Post-Processing.");
            }

            // 2. Reduce Resolution Scale (URP)
            if (reduceResolutionOnLowFPS)
            {
                UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                if (urpAsset != null)
                {
                    CaptureOriginalRenderScale();
                    urpAsset.renderScale = 0.75f; // Drop to 75% resolution
                    Debug.Log("[PerformanceManager] Reduced Render Scale to 0.75.");
                }
            }

            // 3. Disable heavy particles (optional hook for RealmAtmosphereController)
            // AscendantContinuum.Realms.RealmAtmosphereController.Instance?.DisableHeavyParticles();
        }

        private void RestoreQuality()
        {
            isDegraded = false;

            // 1. Restore Post Processing
            if (disablePostProcessingOnLowFPS && globalVolume != null)
            {
                globalVolume.enabled = true;
                Debug.Log("[PerformanceManager] Restored Post-Processing.");
            }

            // 2. Restore Resolution Scale
            if (reduceResolutionOnLowFPS)
            {
                UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                if (urpAsset != null)
                {
                    urpAsset.renderScale = hasCapturedRenderScale ? originalRenderScale : 1.0f;
                    Debug.Log($"[PerformanceManager] Restored Render Scale to {urpAsset.renderScale:F2}.");
                }
            }
        }

        public bool IsPerformanceDegraded => isDegraded;
    }
}
