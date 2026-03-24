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

            // ── Solid fill — prevents camera background bleeding through art gaps ──
            var solidFill = new GameObject("SolidFill");
            solidFill.transform.SetParent(canvasGO.transform, false);
            var fillImg = solidFill.AddComponent<Image>();
            fillImg.color = new Color(0.04f, 0.02f, 0.12f);
            Stretch(solidFill);
            solidFill.GetComponent<RectTransform>().SetAsFirstSibling();

            // Assets
            var logoSprite = LoadSprite(LOGO_PATH);
            var bgTex      = LoadSprite("Assets/_Project/Art/AscendantContinuumGameGraphics/bg_HomeMainMenu.jpeg")
                          ?? LoadSprite("Assets/_Project/Art/Backgrounds/bg_echofields.png")
                          ?? LoadSprite("Assets/_Project/Art/Backgrounds/bg_emberforge.png");

            // ── Background — CSS-cover behaviour (EnvelopeParent) ────────
            // Scale the image up until BOTH axes cover the canvas; center-crop any overflow.
            var bgGO = MakeImage(canvasGO, "Background", bgTex, new Color(0.05f, 0.04f, 0.14f));
            bgGO.GetComponent<Image>().preserveAspect = false;
            var bgRt = bgGO.GetComponent<RectTransform>();
            bgRt.anchorMin = bgRt.anchorMax = bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.anchoredPosition = Vector2.zero;
            bgRt.sizeDelta = Vector2.zero;
            var bgArf = bgGO.AddComponent<AspectRatioFitter>();
            bgArf.aspectMode  = AspectRatioFitter.AspectMode.EnvelopeParent;
            bgArf.aspectRatio = bgTex != null ? bgTex.rect.width / bgTex.rect.height : (16f / 9f);
            var alive = bgGO.GetComponent<AscendantContinuum.UI.BackgroundAliveMotion>();
            if (alive == null) alive = bgGO.AddComponent<AscendantContinuum.UI.BackgroundAliveMotion>();
            
            var parallax = bgGO.GetComponent<AscendantContinuum.UI.ParallaxBackground>();
            if (parallax == null) parallax = bgGO.AddComponent<AscendantContinuum.UI.ParallaxBackground>();
            
            var ambientChild = bgGO.transform.Find("[ Ambient Particles ]");
            if (ambientChild == null)
            {
                var ambObj = new GameObject("[ Ambient Particles ]");
                ambObj.transform.SetParent(bgGO.transform, false);
                ambientChild = ambObj.transform;
            }
            var ambient = ambientChild.GetComponent<AscendantContinuum.UI.AmbientParticles>();
            if (ambient == null) ambient = ambientChild.gameObject.AddComponent<AscendantContinuum.UI.AmbientParticles>();

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
                new Vector2(0f, -215f), new Color32(60, 20, 20, 200));
            quitBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 44f);

            // ── Realm Overlay (full-screen, shown when realm select is active) ──
            var realmOverlayGO = new GameObject("[ Realm Overlay ]");
            realmOverlayGO.transform.SetParent(canvasGO.transform, false);
            var overlayImg = realmOverlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0.02f, 0.01f, 0.09f, 0.82f);
            var overlayRt = realmOverlayGO.GetComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = overlayRt.offsetMax = Vector2.zero;
            overlayRt.SetAsLastSibling(); // draw on top

            var realmsLabel = MakeText(realmOverlayGO, "RealmsLabel",
                "✦  CHOOSE YOUR REALM  ✦",
                new Vector2(0f, 195f), new Vector2(520f, 44f),
                fontSize: 20, color: new Color(0.85f, 0.75f, 1f));

            var realmEntries = new (string objName, string label, Color32 color)[]
            {
                ("RealmBtn_Emberforge",       "⚒  EMBERFORGE",         new Color32(160, 65, 20, 240)),
                ("RealmBtn_Verdant",          "🌿  VERDANT SANCTUARY",  new Color32(25, 110, 50, 240)),
                ("RealmBtn_EchoFields",       "✧  ECHO FIELDS",        new Color32(20, 70, 160, 240)),
                ("RealmBtn_DawnCitadel",      "☀  DAWN CITADEL",       new Color32(150, 115, 15, 240)),
                ("RealmBtn_LanternAscension", "🏮  LANTERN ASCENSION",  new Color32(95, 30, 145, 240)),
            };

            var realmBtns = new GameObject[5];
            for (int i = 0; i < realmEntries.Length; i++)
            {
                var rb = MakeButton(realmOverlayGO, realmEntries[i].objName, realmEntries[i].label,
                    new Vector2(0f, 120f - i * 68f), realmEntries[i].color);
                rb.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 62f);
                var lbl = rb.transform.Find("Text")?.GetComponent<Text>();
                if (lbl != null) lbl.fontSize = 18;
                realmBtns[i] = rb;
            }

            var realmBackBtn = MakeButton(realmOverlayGO, "RealmBackButton", "← BACK TO MENU",
                new Vector2(0f, -220f), new Color32(50, 50, 80, 230));
            realmBackBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(240f, 50f);

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

                so2.FindProperty("realmBackButton").objectReferenceValue   = realmBackBtn.GetComponent<Button>();
                so2.FindProperty("realmsLabel").objectReferenceValue       = realmsLabel.GetComponent<Text>();
                so2.FindProperty("realmOverlay").objectReferenceValue      = realmOverlayGO;

                // Wire 5 realm buttons
                var rbArr = so2.FindProperty("realmButtons");
                rbArr.arraySize = 5;
                for (int i = 0; i < 5; i++)
                    rbArr.GetArrayElementAtIndex(i).objectReferenceValue = realmBtns[i].GetComponent<Button>();

                // Wire realm scene names
                var snArr = so2.FindProperty("realmSceneNames");
                snArr.arraySize = 5;
                snArr.GetArrayElementAtIndex(0).stringValue = AscendantContinuum.Core.SceneNames.Emberforge;
                snArr.GetArrayElementAtIndex(1).stringValue = AscendantContinuum.Core.SceneNames.Verdant;
                snArr.GetArrayElementAtIndex(2).stringValue = AscendantContinuum.Core.SceneNames.EchoFields;
                snArr.GetArrayElementAtIndex(3).stringValue = AscendantContinuum.Core.SceneNames.DawnCitadel;
                snArr.GetArrayElementAtIndex(4).stringValue = AscendantContinuum.Core.SceneNames.LanternAscension;

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
            GameObject go = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == canvasName) { go = root; break; }
                var canvas = root.GetComponentInChildren<Canvas>(true);
                if (canvas != null && canvas.gameObject.name == canvasName) { go = canvas.gameObject; break; }
            }

            if (go == null)
            {
                go = new GameObject(canvasName);
                SceneManager.MoveGameObjectToScene(go, scene);
                go.AddComponent<Canvas>();
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
            }

            // Always enforce these settings (covers newly-created AND pre-existing canvases)
            var c = go.GetComponent<Canvas>();
            c.renderMode  = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 0;
            var cs = go.GetComponent<CanvasScaler>();
            cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.screenMatchMode     = CanvasScaler.ScreenMatchMode.Expand;   // never crop content
            cs.matchWidthOrHeight  = 0.5f;
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
                img.color  = Color.white;
                img.preserveAspect = false; // caller controls aspect; default to fill
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
