#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Builds the visual UI layout for MainMenu and Onboarding scenes.
    /// Run from: Ascendant Continuum → Setup → 🎨 Build Scene UI
    /// </summary>
    public static class BuildSceneUI
    {
        private const string MAIN_MENU_PATH  = "Assets/_Project/Scenes/Core/MainMenu.unity";
        private const string ONBOARDING_PATH = "Assets/_Project/Scenes/Core/Onboarding.unity";
        private const string LOGO_PATH       = "Assets/_Project/Art/Sprites/UI/splash_logo.png";

        // ── Menu items ────────────────────────────────────────────────────

        [MenuItem("Ascendant Continuum/Setup/\U0001f3a8 Build MainMenu UI")]
        public static void BuildMainMenuUI()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MAIN_MENU_PATH, OpenSceneMode.Single);
            BuildMainMenu(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[BuildSceneUI] ✅ MainMenu UI built and saved. Press Play to test.");
        }

        [MenuItem("Ascendant Continuum/Setup/\U0001f3a8 Build Onboarding UI")]
        public static void BuildOnboardingUI()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ONBOARDING_PATH, OpenSceneMode.Single);
            BuildOnboarding(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[BuildSceneUI] ✅ Onboarding UI built and saved.");
        }

        [MenuItem("Ascendant Continuum/Setup/\U0001f3a8 Build All Scene UI")]
        public static void BuildAllUI()
        {
            BuildOnboardingUI();
            BuildMainMenuUI();
            EditorUtility.DisplayDialog("Build Complete",
                "✅ Onboarding + MainMenu UI built!\n\nNext: Open Bootstrap scene and press Play.", "OK");
        }

        // ── MainMenu builder ──────────────────────────────────────────────

        private static void BuildMainMenu(Scene scene)
        {
            var canvasGO = GetOrCreateCanvas(scene, "MainMenuCanvas");
            EnsureEventSystem(scene);

            // Remove old UI children to avoid duplicates
            ClearChildren(canvasGO, keep: new[] { "FadePanel" });

            // Assets
            var logoSprite = LoadSprite(LOGO_PATH);
            var bgTex      = LoadSprite("Assets/_Project/Art/Backgrounds/bg_verdant.png")
                          ?? LoadSprite("Assets/_Project/Art/Backgrounds/bg_emberforge.png");

            // ── Background ────────────────────────────────────────────────
            var bgGO = MakeImage(canvasGO, "Background", bgTex, new Color(0.05f, 0.04f, 0.14f));
            Stretch(bgGO);

            // ── Logo ──────────────────────────────────────────────────────
            var logoGO = MakeImage(canvasGO, "LogoImage", logoSprite, new Color(0.2f, 0.1f, 0.4f));
            Place(logoGO, new Vector2(0f, 380f), new Vector2(560f, 170f));

            // ── Subtitle ──────────────────────────────────────────────────
            var subtitle = MakeText(canvasGO, "SubtitleText",
                "YOUR COSMIC JOURNEY AWAITS",
                new Vector2(0f, 260f), new Vector2(700f, 40f),
                fontSize: 16, color: new Color(0.75f, 0.65f, 1f));

            // ── Buttons ───────────────────────────────────────────────────
            var playBtn     = MakeButton(canvasGO, "PlayButton",     "✦  ENTER THE CONTINUUM",
                new Vector2(0f, 120f), new Color32(90, 40, 180, 255));
            var continueBtn = MakeButton(canvasGO, "ContinueButton", "↩  CONTINUE JOURNEY",
                new Vector2(0f, 35f), new Color32(50, 30, 110, 255));
            var settingsBtn = MakeButton(canvasGO, "SettingsButton", "⚙  SETTINGS",
                new Vector2(0f, -50f), new Color32(35, 35, 60, 255));
            var seasonBtn   = MakeButton(canvasGO, "SeasonPassButton","✧  COSMIC SHOP",
                new Vector2(0f, -135f), new Color32(30, 25, 55, 255));

            // Add quit button (web/desktop only caveat — safe to leave wired)
            var quitBtn = MakeButton(canvasGO, "QuitButton", "EXIT",
                new Vector2(0f, -245f), new Color32(60, 20, 20, 200));
            quitBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 52f);

            // ── Version label ─────────────────────────────────────────────
            var verLabel = MakeText(canvasGO, "VersionText", "v0.1 ALPHA",
                new Vector2(790f, -515f), new Vector2(200f, 40f),
                fontSize: 14, color: new Color(1f, 1f, 1f, 0.3f));
            verLabel.GetComponent<Text>().alignment = TextAnchor.LowerRight;

            // ── Wire MainMenuController ───────────────────────────────────
            var ctrl = FindInScene<AscendantContinuum.UI.MainMenuController>(scene);
            if (ctrl != null)
            {
                var so = new SerializedObject(ctrl);
                so.FindProperty("backgroundImage").objectReferenceValue = bgGO.GetComponent<Image>();
                so.FindProperty("playButton").objectReferenceValue      = playBtn.GetComponent<Button>();
                so.FindProperty("settingsButton").objectReferenceValue  = settingsBtn.GetComponent<Button>();
                so.FindProperty("continueButton").objectReferenceValue  = continueBtn.GetComponent<Button>();
                so.FindProperty("seasonPassButton").objectReferenceValue = seasonBtn.GetComponent<Button>();
                so.FindProperty("titleText").objectReferenceValue        = subtitle.GetComponent<Text>();
                so.ApplyModifiedProperties();
            }

            // ── Wire MainMenuManager (safe – skips TMP fields we don't set) ──
            var mgr = FindInScene<AscendantContinuum.UI.MainMenuManager>(scene);
            if (mgr != null)
            {
                var so2 = new SerializedObject(mgr);
                so2.FindProperty("playButton").objectReferenceValue        = playBtn.GetComponent<Button>();
                so2.FindProperty("settingsButton").objectReferenceValue    = settingsBtn.GetComponent<Button>();
                so2.FindProperty("seasonPassButton").objectReferenceValue  = seasonBtn.GetComponent<Button>();
                so2.FindProperty("creditsButton").objectReferenceValue     = quitBtn.GetComponent<Button>();
                so2.FindProperty("quitButton").objectReferenceValue        = quitBtn.GetComponent<Button>();
                so2.FindProperty("fadePanel").objectReferenceValue         = FindChildImage(canvasGO, "FadePanel");
                so2.ApplyModifiedProperties();
            }
        }

        // ── Onboarding builder ────────────────────────────────────────────

        private static void BuildOnboarding(Scene scene)
        {
            var canvasGO = GetOrCreateCanvas(scene, "OnboardingCanvas");
            EnsureEventSystem(scene);
            ClearChildren(canvasGO, keep: new string[0]);

            var logoSprite = LoadSprite(LOGO_PATH);

            // Background
            var bgGO = MakeImage(canvasGO, "Background", null, new Color(0.04f, 0.03f, 0.12f));
            Stretch(bgGO);

            // Logo at top
            var headerLogo = MakeImage(canvasGO, "HeaderLogo", logoSprite, new Color(0.2f, 0.1f, 0.4f));
            Place(headerLogo, new Vector2(0f, 360f), new Vector2(520f, 160f));

            // Step label
            var stepLbl = MakeText(canvasGO, "StepLabel", "Step 1 / 3",
                new Vector2(0f, 250f), new Vector2(400f, 40f),
                fontSize: 16, color: new Color(0.7f, 0.6f, 1f));

            // Panels
            var panels = new List<GameObject>
            {
                MakeOnboardingPanel(canvasGO, "WelcomePanel",
                    "Welcome, Cosmic Traveller",
                    "You are about to begin your journey through the Ascendant Continuum —\nfive sacred realms awaiting your ritual touch.",
                    active: true),

                MakeOnboardingPanel(canvasGO, "RealmsPanel",
                    "Five Sacred Realms",
                    "Emberforge  •  Verdant Sanctuary  •  Echo Fields\nDawn Citadel  •  Lantern Ascension\n\nEach holds a unique ritual for you to discover.",
                    active: false),

                MakeOnboardingPanel(canvasGO, "AccessibilityPanel",
                    "Your Journey, Your Way",
                    "Colorblind mode, reduced motion, and haptics\ncan all be adjusted anytime in Settings.",
                    active: false),
            };

            // Accessibility toggles (panel 3)
            var accPanel   = panels[2];
            var cbToggle   = MakeToggle(accPanel, "ColorblindToggle",   "Colorblind Mode",   new Vector2(0f, -85f));
            var rmToggle   = MakeToggle(accPanel, "ReducedMotionToggle","Reduced Motion",    new Vector2(0f, -145f));
            var hapToggle  = MakeToggle(accPanel, "HapticsToggle",      "Haptic Feedback",   new Vector2(0f, -205f));

            // NAV BUTTONS
            var nextBtn  = MakeButton(canvasGO, "NextButton",  "NEXT  →", new Vector2(160f, -330f),  new Color32(90, 40, 180, 255));
            var skipBtn  = MakeButton(canvasGO, "SkipButton",  "SKIP ALL", new Vector2(-160f, -330f), new Color32(45, 45, 45, 220));
            nextBtn.GetComponent<RectTransform>().sizeDelta  = new Vector2(280f, 72f);
            skipBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(220f, 72f);

            // ── Wire OnboardingController ──────────────────────────────────
            var ctrl = FindInScene<AscendantContinuum.Core.OnboardingController>(scene);
            if (ctrl != null)
            {
                var so = new SerializedObject(ctrl);

                var panelsProp = so.FindProperty("panels");
                panelsProp.arraySize = panels.Count;
                for (int i = 0; i < panels.Count; i++)
                    panelsProp.GetArrayElementAtIndex(i).objectReferenceValue = panels[i];

                so.FindProperty("nextButton").objectReferenceValue          = nextBtn.GetComponent<Button>();
                so.FindProperty("skipButton").objectReferenceValue          = skipBtn.GetComponent<Button>();
                so.FindProperty("colorblindToggle").objectReferenceValue    = cbToggle.GetComponent<Toggle>();
                so.FindProperty("reducedMotionToggle").objectReferenceValue = rmToggle.GetComponent<Toggle>();
                so.FindProperty("hapticsToggle").objectReferenceValue       = hapToggle.GetComponent<Toggle>();
                so.ApplyModifiedProperties();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static GameObject MakeOnboardingPanel(GameObject parent, string name, string title, string body, bool active)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent.transform, false);
            var img = panel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.55f);
            Place(panel, new Vector2(0f, 20f), new Vector2(920f, 420f));

            MakeText(panel, "TitleText", title,
                new Vector2(0f, 140f), new Vector2(860f, 70f),
                fontSize: 28, color: Color.white);
            MakeText(panel, "BodyText", body,
                new Vector2(0f, 40f), new Vector2(840f, 170f),
                fontSize: 18, color: new Color(0.85f, 0.82f, 1f));

            panel.SetActive(active);
            return panel;
        }

        private static GameObject GetOrCreateCanvas(Scene scene, string canvasName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == canvasName) return root;
                var canvas = root.GetComponentInChildren<Canvas>(true);
                if (canvas != null && canvas.gameObject.name == canvasName)
                    return canvas.gameObject;
            }

            var go = new GameObject(canvasName);
            SceneManager.MoveGameObjectToScene(go, scene);
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 0;
            var cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.matchWidthOrHeight  = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static void ClearChildren(GameObject parent, string[] keep)
        {
            var keepSet = new System.Collections.Generic.HashSet<string>(keep);
            var toDestroy = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                if (!keepSet.Contains(child.name))
                    toDestroy.Add(child.gameObject);
            }
            foreach (var go in toDestroy)
                Object.DestroyImmediate(go);
        }

        private static GameObject MakeImage(GameObject parent, string name, Sprite sprite, Color fallback)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var img = go.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
            }
            else img.color = fallback;
            return go;
        }

        private static GameObject MakeText(GameObject parent, string name, string content,
            Vector2 pos, Vector2 size, int fontSize, Color color)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var txt = go.AddComponent<Text>();
            txt.text      = content;
            txt.fontSize  = fontSize;
            txt.color     = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;
            return go;
        }

        private static GameObject MakeButton(GameObject parent, string name, string label,
            Vector2 pos, Color32 bgColor)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var rt  = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta        = new Vector2(340f, 72f);

            // Label child
            var lblGO = new GameObject("Text");
            lblGO.transform.SetParent(go.transform, false);
            var txt = lblGO.AddComponent<Text>();
            txt.text      = label;
            txt.fontSize  = 20;
            txt.color     = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var lrt = lblGO.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;

            return go;
        }

        private static GameObject MakeToggle(GameObject parent, string name, string label, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta        = new Vector2(400f, 50f);

            // Background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(go.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.3f);
            var bgrt = bgGO.GetComponent<RectTransform>();
            bgrt.anchoredPosition = new Vector2(-160f, 0f);
            bgrt.sizeDelta        = new Vector2(40f, 40f);

            // Checkmark
            var checkGO = new GameObject("Checkmark");
            checkGO.transform.SetParent(bgGO.transform, false);
            var checkImg = checkGO.AddComponent<Image>();
            checkImg.color = new Color(0.6f, 0.3f, 1f);
            var crt = checkGO.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.1f, 0.1f);
            crt.anchorMax = new Vector2(0.9f, 0.9f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;

            // Label text
            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(go.transform, false);
            var txt = lblGO.AddComponent<Text>();
            txt.text      = label;
            txt.fontSize  = 18;
            txt.color     = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var lrt = lblGO.GetComponent<RectTransform>();
            lrt.anchoredPosition = new Vector2(40f, 0f);
            lrt.sizeDelta        = new Vector2(300f, 40f);

            // Toggle component
            var toggle             = go.AddComponent<Toggle>();
            toggle.targetGraphic   = bgImg;
            toggle.graphic         = checkImg;
            toggle.isOn            = false;

            return go;
        }

        private static void Place(GameObject go, Vector2 pos, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;
        }

        private static void Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static Sprite LoadSprite(string path)
        {
            // Ensure texture import type is Sprite
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Image FindChildImage(GameObject parent, string name)
        {
            var t = parent.transform.Find(name);
            return t != null ? t.GetComponent<Image>() : null;
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var c = root.GetComponentInChildren<T>(true);
                if (c != null) return c;
            }
            return null;
        }

        private static void EnsureEventSystem(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponentInChildren<EventSystem>(true) != null)
                    return;
            }

            var eventGO = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventGO, scene);
            eventGO.AddComponent<EventSystem>();

            // Prefer new input-system module if installed; fallback to legacy module.
            var inputSystemModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                eventGO.AddComponent(inputSystemModuleType);
            }
            else
            {
                eventGO.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
#endif
