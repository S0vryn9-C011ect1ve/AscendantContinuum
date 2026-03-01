using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Manages transitions between realms with beautiful visual effects
    /// Respects accessibility settings for smooth vs reduced-motion transitions
    /// </summary>
    public class RealmTransitionManager : MonoBehaviour
    {
        public static RealmTransitionManager Instance { get; private set; }

        [Header("Transition Settings")]
        [SerializeField] private float normalTransitionDuration = 2f;
        [SerializeField] private float reducedMotionDuration = 0.5f;
        [SerializeField] private CanvasGroup transitionCanvasGroup;

        [Header("Realms")]
        [SerializeField] private Data.RealmData[] allRealms;

        private bool isTransitioning = false;
        private Data.RealmData currentRealmData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize transition canvas
            if (transitionCanvasGroup == null)
            {
                CreateTransitionCanvas();
            }
        }

        private void CreateTransitionCanvas()
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");
            canvasObj.transform.SetParent(transform);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // Always on top

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            transitionCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;

            // Add background image
            GameObject bgObj = new GameObject("TransitionBackground");
            bgObj.transform.SetParent(canvasObj.transform);

            UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = Color.black;

            RectTransform rt = bgImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        public void TransitionToRealm(string realmId)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[RealmTransition] Transition already in progress");
                return;
            }

            Data.RealmData targetRealm = GetRealmData(realmId);
            if (targetRealm == null)
            {
                Debug.LogError($"[RealmTransition] Realm not found: {realmId}");
                return;
            }

            StartCoroutine(TransitionSequence(targetRealm));
        }

        private IEnumerator TransitionSequence(Data.RealmData targetRealm)
        {
            isTransitioning = true;
            GameManager.Instance?.ChangeState(GameState.Transition);
            GameEvents.RaiseRealmTransitionStarted(targetRealm.realmId);

            GameFlowConfig flow = GameFlow.Config;
            float configuredNormalDuration = flow != null ? flow.NormalTransitionDuration : normalTransitionDuration;
            float configuredReducedDuration = flow != null ? flow.ReducedMotionTransitionDuration : reducedMotionDuration;

            float duration = AccessibilityManager.Instance?.ReducedMotionEnabled == true
                ? configuredReducedDuration
                : configuredNormalDuration;

            Debug.Log($"[RealmTransition] Starting transition to {targetRealm.realmName}");

            // Pre-transition delay for intentionality
            yield return new WaitForSeconds(0.15f);

            // Phase 1: Fade out
            yield return FadeOut(duration / 2f);

            // Phase 2: Play transition effects
            if (targetRealm.transitionSound != null)
            {
                AudioManager.Instance?.PlaySFX(targetRealm.transitionSound, 0.7f);
            }

            Vector3 fxPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            VFX.ParticleManager.Instance?.PlayRealmTransitionEffect(fxPosition, targetRealm.primaryColor);

            // Save current progress
            SaveSystem.Instance?.SaveGame();

            // Track realm visit count (used by SigilGenerator playstyle metrics)
            string visitKey = $"Visits_{System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(targetRealm.realmId.ToLower())}";
            PlayerPrefs.SetInt(visitKey, PlayerPrefs.GetInt(visitKey, 0) + 1);
            PlayerPrefs.Save();

            // Phase 3: Load new realm scene
            string sceneName = SceneService.ResolveRealmScene(targetRealm.realmId);
            if (SceneService.CanLoadScene(sceneName))
            {
                yield return SceneService.TryLoadSceneAsync(sceneName);
            }
            else
            {
                Debug.LogWarning($"[RealmTransition] Scene not found for realm '{targetRealm.realmId}' (resolved to '{sceneName}'). Falling back to {SceneNames.MainMenu}.");
                yield return SceneService.TryLoadSceneAsync(SceneNames.MainMenu);
            }

            // Update game state
            currentRealmData = targetRealm;
            GameManager.Instance?.LoadRealm(targetRealm.realmId);

            // Phase 4: Start new realm music
            if (targetRealm.ambientMusic != null)
            {
                AudioManager.Instance?.PlayMusic(targetRealm.ambientMusic, duration / 2f);
            }

            // Phase 5: Fade in
            yield return FadeIn(duration / 2f);

            // Post-transition delay for intentionality
            yield return new WaitForSeconds(0.25f);

            GameManager.Instance?.ChangeState(GameState.Playing);
            isTransitioning = false;
            GameEvents.RaiseRealmTransitionCompleted(targetRealm.realmId);

            Debug.Log($"[RealmTransition] Transition complete - Now in {targetRealm.realmName}");
        }

        private IEnumerator FadeOut(float duration)
        {
            transitionCanvasGroup.blocksRaycasts = true;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float normalizedTime = t / duration;
                // Ease in-out cubic
                float easedTime = normalizedTime < 0.5f ? 4f * normalizedTime * normalizedTime * normalizedTime : 1f - Mathf.Pow(-2f * normalizedTime + 2f, 3f) / 2f;
                transitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, easedTime);
                yield return null;
            }

            transitionCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeIn(float duration)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float normalizedTime = t / duration;
                // Ease in-out cubic
                float easedTime = normalizedTime < 0.5f ? 4f * normalizedTime * normalizedTime * normalizedTime : 1f - Mathf.Pow(-2f * normalizedTime + 2f, 3f) / 2f;
                transitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, easedTime);
                yield return null;
            }

            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
        }

        private Data.RealmData GetRealmData(string realmId)
        {
            foreach (Data.RealmData realm in allRealms)
            {
                if (realm.realmId == realmId)
                {
                    return realm;
                }
            }
            return null;
        }

        public Data.RealmData GetCurrentRealmData() => currentRealmData;
        public bool IsTransitioning => isTransitioning;
    }
}
