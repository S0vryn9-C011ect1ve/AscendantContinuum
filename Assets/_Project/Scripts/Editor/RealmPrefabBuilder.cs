#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
// Gameplay scripts
using AscendantContinuum.Emberforge;
using AscendantContinuum.Verdant;
using AscendantContinuum.Realms.Verdant;   // AscendantContinuum.Realms.Verdant.MagicalPlant
using AscendantContinuum.EchoFields;
using AscendantContinuum.Realms.EchoFields;  // Star
using AscendantContinuum.Realms.DawnCitadel;
using AscendantContinuum.Realms.LanternAscension;
using AscendantContinuum.UI;
using AscendantContinuum.Core;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Creates all realm gameplay prefabs from scratch and wires them
    /// into each realm scene.
    ///
    /// Menu → Ascendant Continuum → Realms → 🌟 Build & Wire All Realms
    /// </summary>
    public static class RealmPrefabBuilder
    {
        // ── Paths ────────────────────────────────────────────────────────────
        private const string TEX_ROOT  = "Assets/_Project/Art/Generated";
        private const string PREF_ROOT = "Assets/_Project/Prefabs/Realms";
        private const string SCENE_ROOT = "Assets/_Project/Scenes/Realms";

        // ── Menu items ───────────────────────────────────────────────────────

        [MenuItem("Ascendant Continuum/Setup/\U0001f31f Build and Wire All Realms", priority = 200)]
        public static void BuildAndWireAll()
        {
            if (!EditorUtility.DisplayDialog(
                    "Build & Wire All Realms",
                    "This will:\n" +
                    "• Generate gameplay prefabs (Spark, Plant, Star, Lantern, Prism)\n" +
                    "• Open each realm scene and wire prefab references\n" +
                    "• Add Back-to-Menu logic on every BackButton\n" +
                    "• Configure orthographic cameras and per-realm colours\n\n" +
                    "Existing prefabs will NOT be overwritten. Continue?",
                    "Build", "Cancel"))
                return;

            try
            {
                EditorUtility.DisplayProgressBar("Realm Builder", "Creating prefabs…", 0.1f);
                EnsureDirectories();

                var sparkPrefab   = CreateSparkPrefab();
                var plantPrefab   = CreatePlantPrefab();
                var starPrefab    = CreateStarPrefab();
                var lanternPrefab = CreateLanternPrefab();
                var prismPrefab   = CreatePrismPrefab();
                var targetPrefab  = CreateLightTargetPrefab();
                var lineRenPrefab = CreateLineRendererPrefab();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayProgressBar("Realm Builder", "Wiring Emberforge…", 0.3f);
                WireEmberforge(sparkPrefab);

                EditorUtility.DisplayProgressBar("Realm Builder", "Wiring Verdant…", 0.45f);
                WireVerdant(plantPrefab);

                EditorUtility.DisplayProgressBar("Realm Builder", "Wiring Echo Fields…", 0.60f);
                WireEchoFields(starPrefab, lineRenPrefab);

                EditorUtility.DisplayProgressBar("Realm Builder", "Wiring Dawn Citadel…", 0.75f);
                WireDawnCitadel(prismPrefab, targetPrefab, lineRenPrefab);

                EditorUtility.DisplayProgressBar("Realm Builder", "Wiring Lantern Ascension…", 0.90f);
                WireLanternAscension(lanternPrefab);

                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();

                EditorUtility.DisplayDialog(
                    "✅ All Realms Wired",
                    "Prefabs created and all 5 realm scenes updated.\n\n" +
                    "Press Play in Bootstrap to test the full flow.",
                    "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[RealmPrefabBuilder] Failed: {ex}");
                EditorUtility.DisplayDialog("Build Failed", ex.Message, "OK");
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  REALM SELECT CARD STYLING
        // ════════════════════════════════════════════════════════════════════

        [MenuItem("Ascendant Continuum/Setup/\U0001f3a8 Style Realm Select Cards", priority = 220)]
        public static void StyleRealmCards()
        {
            const string menuPath = "Assets/_Project/Scenes/Core/MainMenu.unity";
            if (!File.Exists(menuPath))
            {
                Debug.LogError("[RealmPrefabBuilder] MainMenu scene not found.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.OpenScene(menuPath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            var cards = new (string btnName, string bgPath, string label, string emoji)[]
            {
                ("Btn_Emberforge",       "Assets/_Project/Art/Backgrounds/bg_emberforge.png",       "Emberforge",       "\U0001F525"),
                ("Btn_Verdant",          "Assets/_Project/Art/Backgrounds/bg_verdant.png",           "Verdant Garden",   "\U0001F33F"),
                ("Btn_EchoFields",       "Assets/_Project/Art/Backgrounds/bg_echofields.png",        "Echo Fields",      "\u2B50"),
                ("Btn_DawnCitadel",      "Assets/_Project/Art/Backgrounds/bg_dawncitadel.jpg",       "Dawn Citadel",     "\u2600\uFE0F"),
                ("Btn_LanternAscension", "Assets/_Project/Art/Backgrounds/bg_lanternascension.jpg",  "Lantern Ascension","\U0001F3EE"),
            };

            foreach (var c in cards)
                StyleOneRealmCard(scene, c.btnName, c.bgPath, c.label, c.emoji);

            EnsureMainMenuBackground(scene);
            EnsureMainMenuSettingsPanel(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[RealmPrefabBuilder] \U0001F3A8 All realm select cards styled.");
        }

        /// <summary>
        /// Sets a background SpriteRenderer behind the Main Menu canvas using the
        /// Echo Fields sky art (cosmic/starry — perfect for a title screen).
        /// Creates a Camera if none exists and positions the background at z=10, order=-100.
        /// </summary>
        private static void EnsureMainMenuBackground(
            UnityEngine.SceneManagement.Scene scene)
        {
            const string bgPath = "Assets/_Project/Art/AscendantContinuumGameGraphics/bg_HomeMainMenu.jpeg";
            EnsureTextureIsSprite(bgPath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            if (sprite == null)
            {
                Debug.LogWarning("[RealmPrefabBuilder] Main Menu background texture not found.");
                return;
            }

            // Find or create the [ Background ] object
            var bgGO = FindGoInScene(scene, "[ Background ]");
            if (bgGO == null)
            {
                bgGO = new GameObject("[ Background ]");
                bgGO.transform.position = new Vector3(0f, 0f, 10f);
            }
            bgGO.transform.position = new Vector3(0f, 0f, 10f);

            var sr = GetOrAdd<SpriteRenderer>(bgGO);
            sr.sprite       = sprite;
            sr.color        = new Color(0.6f, 0.5f, 0.9f, 1f); // slight cool tint for menu mood
            sr.sortingOrder = -100;
            sr.drawMode     = SpriteDrawMode.Simple;

            var alive = GetOrAdd<AscendantContinuum.UI.BackgroundAliveMotion>(bgGO);

            // Ensure there is a camera set up
            Camera cam = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                cam = root.GetComponentInChildren<Camera>(true);
                if (cam != null) break;
            }
            if (cam == null)
            {
                var camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                cam = camGO.AddComponent<Camera>();
                cam.transform.position = new Vector3(0f, 0f, -10f);
            }
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.02f, 0.1f);

            // Scale background to stretch-fill camera view
            float viewH = cam.orthographicSize * 2f;
            float viewW = viewH * cam.aspect;
            float ppu     = sprite.pixelsPerUnit > 0f ? sprite.pixelsPerUnit : 100f;
            float spriteH = sprite.rect.height / ppu;
            float spriteW = sprite.rect.width  / ppu;
            if (spriteH > 0f && spriteW > 0f)
            {
                float scaleX = viewW / spriteW;
                float scaleY = viewH / spriteH;
                bgGO.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            Debug.Log("[RealmPrefabBuilder] \u2705 Main Menu background set.");
        }

        /// <summary>
        /// Creates a SettingsPanel in the Main Menu canvas and wires it to MainMenuController.
        /// The panel floats above all other UI elements (last in canvas hierarchy).
        /// </summary>
        private static void EnsureMainMenuSettingsPanel(
            UnityEngine.SceneManagement.Scene scene)
        {
            // Find the root canvas (or any canvas)
            Canvas mainCanvas = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                mainCanvas = root.GetComponentInChildren<Canvas>(true);
                if (mainCanvas != null) break;
            }
            if (mainCanvas == null) return;

            var canvasGO = mainCanvas.gameObject;

            // Re-use the same EnsureSettingsPanel builder by spoofing a scene canvas lookup
            // — but here we build it directly under the main canvas.
            var panelGO = canvasGO.transform.Find("SettingsPanel")?.gameObject;
            if (panelGO == null)
            {
                panelGO = new GameObject("SettingsPanel");
                panelGO.transform.SetParent(canvasGO.transform, false);
            }

            // Put settings panel at the top of render order
            panelGO.transform.SetAsLastSibling();

            // Dim background
            var bg = GetOrAdd<Image>(panelGO);
            bg.color = new Color(0.04f, 0.02f, 0.1f, 0.96f);
            var bgRT = panelGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

            // Title
            var titleGO = GetOrCreateChildGO(panelGO, "SettingsTitle");
            var titleTMP = GetOrAdd<TextMeshProUGUI>(titleGO);
            titleTMP.text      = "⚙  Settings";
            titleTMP.fontSize  = 28f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color     = new Color(0.9f, 0.85f, 1f);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.86f);
            titleRT.anchorMax = new Vector2(0.9f, 0.96f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

            // Audio section label
            var audioLblGO = GetOrCreateChildGO(panelGO, "AudioLabel");
            var audioLbl = GetOrAdd<TextMeshProUGUI>(audioLblGO);
            audioLbl.text      = "AUDIO";
            audioLbl.fontSize  = 14f;
            audioLbl.color     = new Color(0.6f, 0.55f, 0.9f);
            audioLbl.alignment = TextAlignmentOptions.Left;
            var audioLblRT = audioLblGO.GetComponent<RectTransform>();
            audioLblRT.anchorMin = new Vector2(0.1f, 0.80f);
            audioLblRT.anchorMax = new Vector2(0.9f, 0.86f);
            audioLblRT.offsetMin = audioLblRT.offsetMax = Vector2.zero;

            Slider BuildMMSliderRow(string rowName, string label, float yMin, float yMax,
                out TMP_Text valueLabel)
            {
                var rowGO = GetOrCreateChildGO(panelGO, rowName);
                var rowRT = rowGO.GetComponent<RectTransform>();
                rowRT.anchorMin = new Vector2(0.08f, yMin);
                rowRT.anchorMax = new Vector2(0.92f, yMax);
                rowRT.offsetMin = rowRT.offsetMax = Vector2.zero;
                var lblGO = GetOrCreateChildGO(rowGO, "Label");
                var lbl2  = GetOrAdd<TextMeshProUGUI>(lblGO);
                lbl2.text = label; lbl2.fontSize = 14f; lbl2.color = Color.white;
                lbl2.alignment = TextAlignmentOptions.MidlineLeft;
                var lblRT2 = lblGO.GetComponent<RectTransform>();
                lblRT2.anchorMin = Vector2.zero; lblRT2.anchorMax = new Vector2(0.32f, 1f);
                lblRT2.offsetMin = lblRT2.offsetMax = Vector2.zero;
                var slGO  = GetOrCreateChildGO(rowGO, "Slider");
                var sl    = GetOrAdd<Slider>(slGO);
                sl.minValue = 0f; sl.maxValue = 1f; sl.value = 1f;
                var slBG = GetOrCreateChildGO(slGO, "Background");
                GetOrAdd<Image>(slBG).color = new Color(0.15f, 0.1f, 0.25f);
                var slBGRT = slBG.GetComponent<RectTransform>();
                slBGRT.anchorMin = new Vector2(0f, 0.3f); slBGRT.anchorMax = new Vector2(1f, 0.7f);
                slBGRT.offsetMin = slBGRT.offsetMax = Vector2.zero;
                var fillArea = GetOrCreateChildGO(slGO, "Fill Area");
                var fill     = GetOrCreateChildGO(fillArea, "Fill");
                var fillImg  = GetOrAdd<Image>(fill);
                fillImg.color = new Color(0.5f, 0.35f, 0.9f);
                var faRT = fillArea.GetComponent<RectTransform>();
                faRT.anchorMin = new Vector2(0f, 0.25f); faRT.anchorMax = new Vector2(1f, 0.75f);
                faRT.offsetMin = new Vector2(5f, 0f); faRT.offsetMax = new Vector2(-5f, 0f);
                var fRT = fill.GetComponent<RectTransform>();
                fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
                fRT.offsetMin = fRT.offsetMax = Vector2.zero;
                var hsa  = GetOrCreateChildGO(slGO, "Handle Slide Area");
                var hndl = GetOrCreateChildGO(hsa, "Handle");
                GetOrAdd<Image>(hndl).color = new Color(0.8f, 0.7f, 1f);
                var hsaRT = hsa.GetComponent<RectTransform>();
                hsaRT.anchorMin = Vector2.zero; hsaRT.anchorMax = Vector2.one;
                hsaRT.offsetMin = new Vector2(10f, 0f); hsaRT.offsetMax = new Vector2(-10f, 0f);
                var hRT = hndl.GetComponent<RectTransform>();
                hRT.anchorMin = new Vector2(0.5f, 0f); hRT.anchorMax = new Vector2(0.5f, 1f);
                hRT.sizeDelta = new Vector2(20f, 0f);
                sl.fillRect   = fill.GetComponent<RectTransform>();
                sl.handleRect = hndl.GetComponent<RectTransform>();
                var slRT = slGO.GetComponent<RectTransform>();
                slRT.anchorMin = new Vector2(0.34f, 0f); slRT.anchorMax = new Vector2(0.80f, 1f);
                slRT.offsetMin = slRT.offsetMax = Vector2.zero;
                var valGO  = GetOrCreateChildGO(rowGO, "ValueLabel");
                var valTMP = GetOrAdd<TextMeshProUGUI>(valGO);
                valTMP.text = "100%"; valTMP.fontSize = 13f;
                valTMP.color = new Color(0.7f, 0.65f, 1f);
                valTMP.alignment = TextAlignmentOptions.MidlineLeft;
                var valRT  = valGO.GetComponent<RectTransform>();
                valRT.anchorMin = new Vector2(0.82f, 0f); valRT.anchorMax = Vector2.one;
                valRT.offsetMin = valRT.offsetMax = Vector2.zero;
                valueLabel = valTMP;
                return sl;
            }

            TMP_Text ml, msl, sl2, al;
            var masterSl  = BuildMMSliderRow("MasterRow",  "Master",  0.72f, 0.80f, out ml);
            var musicSl   = BuildMMSliderRow("MusicRow",   "Music",   0.63f, 0.71f, out msl);
            var sfxSl     = BuildMMSliderRow("SFXRow",     "SFX",     0.54f, 0.62f, out sl2);
            var ambientSl = BuildMMSliderRow("AmbientRow", "Ambient", 0.45f, 0.53f, out al);

            var accLblGO = GetOrCreateChildGO(panelGO, "AccessibilityLabel");
            var accLbl   = GetOrAdd<TextMeshProUGUI>(accLblGO);
            accLbl.text = "ACCESSIBILITY"; accLbl.fontSize = 14f;
            accLbl.color = new Color(0.6f, 0.55f, 0.9f);
            accLbl.alignment = TextAlignmentOptions.Left;
            var accLblRT = accLblGO.GetComponent<RectTransform>();
            accLblRT.anchorMin = new Vector2(0.1f, 0.39f);
            accLblRT.anchorMax = new Vector2(0.9f, 0.45f);
            accLblRT.offsetMin = accLblRT.offsetMax = Vector2.zero;

            Toggle BuildMMToggleRow(string rowName, string label, float yMin, float yMax)
            {
                var rowGO = GetOrCreateChildGO(panelGO, rowName);
                var rowRT = rowGO.GetComponent<RectTransform>();
                rowRT.anchorMin = new Vector2(0.08f, yMin);
                rowRT.anchorMax = new Vector2(0.92f, yMax);
                rowRT.offsetMin = rowRT.offsetMax = Vector2.zero;
                var toggle = GetOrAdd<Toggle>(rowGO);
                var lblGO  = GetOrCreateChildGO(rowGO, "Label");
                var lbl2   = GetOrAdd<TextMeshProUGUI>(lblGO);
                lbl2.text = label; lbl2.fontSize = 14f; lbl2.color = Color.white;
                lbl2.alignment = TextAlignmentOptions.MidlineLeft;
                var lblRT = lblGO.GetComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = new Vector2(0.7f, 1f);
                lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
                var bgGO2 = GetOrCreateChildGO(rowGO, "Background");
                var bgImg2 = GetOrAdd<Image>(bgGO2);
                bgImg2.color = new Color(0.15f, 0.1f, 0.25f);
                var bgRT2 = bgGO2.GetComponent<RectTransform>();
                bgRT2.anchorMin = new Vector2(0.75f, 0.1f); bgRT2.anchorMax = new Vector2(0.9f, 0.9f);
                bgRT2.offsetMin = bgRT2.offsetMax = Vector2.zero;
                toggle.targetGraphic = bgImg2;
                var chkGO  = GetOrCreateChildGO(bgGO2, "Checkmark");
                var chkImg = GetOrAdd<Image>(chkGO);
                chkImg.color = new Color(0.5f, 0.35f, 0.9f);
                var chkRT = chkGO.GetComponent<RectTransform>();
                chkRT.anchorMin = new Vector2(0.1f, 0.1f); chkRT.anchorMax = new Vector2(0.9f, 0.9f);
                chkRT.offsetMin = chkRT.offsetMax = Vector2.zero;
                toggle.graphic = chkImg;
                return toggle;
            }

            var rmTog = BuildMMToggleRow("ReducedMotionRow", "Reduced Motion", 0.31f, 0.39f);
            var hapTog = BuildMMToggleRow("HapticsRow",       "Haptics",        0.22f, 0.30f);
            var hcTog  = BuildMMToggleRow("HighContrastRow",  "High Contrast",  0.13f, 0.21f);

            // Close button
            var closeGO = GetOrCreateChildGO(panelGO, "CloseButton");
            GetOrAdd<Image>(closeGO).color = new Color(0.4f, 0.25f, 0.65f);
            GetOrAdd<Button>(closeGO);
            var closeRT = closeGO.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.3f, 0.04f);
            closeRT.anchorMax = new Vector2(0.7f, 0.12f);
            closeRT.offsetMin = closeRT.offsetMax = Vector2.zero;
            var closeLblGO = GetOrCreateChildGO(closeGO, "Label");
            var closeLbl   = GetOrAdd<TextMeshProUGUI>(closeLblGO);
            closeLbl.text = "Close"; closeLbl.fontSize = 18f;
            closeLbl.alignment = TextAlignmentOptions.Center; closeLbl.color = Color.white;
            var closeLblRT = closeLblGO.GetComponent<RectTransform>();
            closeLblRT.anchorMin = Vector2.zero; closeLblRT.anchorMax = Vector2.one;
            closeLblRT.offsetMin = closeLblRT.offsetMax = Vector2.zero;

            // Wire SettingsPanel component
            panelGO.SetActive(false);
            var comp = GetOrAdd<AscendantContinuum.UI.SettingsPanel>(panelGO);
            var cg   = GetOrAdd<CanvasGroup>(panelGO);
            var so   = new SerializedObject(comp);
            so.FindProperty("panelRoot").objectReferenceValue         = panelGO;
            so.FindProperty("panelGroup").objectReferenceValue        = cg;
            so.FindProperty("masterVolumeSlider").objectReferenceValue  = masterSl;
            so.FindProperty("musicVolumeSlider").objectReferenceValue   = musicSl;
            so.FindProperty("sfxVolumeSlider").objectReferenceValue     = sfxSl;
            so.FindProperty("ambientVolumeSlider").objectReferenceValue = ambientSl;
            so.FindProperty("masterValueLabel").objectReferenceValue  = ml;
            so.FindProperty("musicValueLabel").objectReferenceValue   = msl;
            so.FindProperty("sfxValueLabel").objectReferenceValue     = sl2;
            so.FindProperty("ambientValueLabel").objectReferenceValue = al;
            so.FindProperty("reducedMotionToggle").objectReferenceValue = rmTog;
            so.FindProperty("hapticsToggle").objectReferenceValue       = hapTog;
            so.FindProperty("highContrastToggle").objectReferenceValue  = hcTog;
            so.FindProperty("closeButton").objectReferenceValue         = closeGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            // Wire into MainMenuController.settingsPanel
            var menuCtrl = FindFirstObjectInScene<AscendantContinuum.UI.MainMenuController>(scene);
            if (menuCtrl != null)
            {
                var mso = new SerializedObject(menuCtrl);
                mso.FindProperty("settingsPanel").objectReferenceValue = comp;
                mso.ApplyModifiedProperties();
            }

            Debug.Log("[RealmPrefabBuilder] ✅ Main Menu SettingsPanel built and wired.");
        }

        private static T FindFirstObjectInScene<T>(UnityEngine.SceneManagement.Scene scene)
            where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var c = root.GetComponentInChildren<T>(true);
                if (c != null) return c;
            }
            return null;
        }

        private static void StyleOneRealmCard(
            UnityEngine.SceneManagement.Scene scene,
            string btnName, string bgPath, string label, string emoji)
        {
            EnsureTextureIsSprite(bgPath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);

            var btnGO = FindGoInScene(scene, btnName);
            if (btnGO == null)
            {
                Debug.LogWarning($"[RealmPrefabBuilder] Realm card button not found: {btnName}");
                return;
            }

            // Full-button background from realm art
            var cardBg = GetOrAdd<Image>(btnGO);
            if (sprite != null) { cardBg.sprite = sprite; cardBg.preserveAspect = false; }
            cardBg.type  = Image.Type.Simple;
            cardBg.color = Color.white;

            // Semi-transparent bottom-half overlay so label is readable
            var overlayGO = GetOrCreateChildGO(btnGO, "GradientOverlay");
            var overlay   = GetOrAdd<Image>(overlayGO);
            overlay.color           = new Color(0f, 0f, 0f, 0.6f);
            overlay.raycastTarget   = false;
            var ovRT = overlayGO.GetComponent<RectTransform>();
            ovRT.anchorMin = Vector2.zero;
            ovRT.anchorMax = new Vector2(1f, 0.42f);
            ovRT.offsetMin = ovRT.offsetMax = Vector2.zero;

            // Realm name label at bottom centre
            var lblGO  = GetOrCreateChildGO(btnGO, "RealmLabel");
            var lbl    = GetOrAdd<TextMeshProUGUI>(lblGO);
            lbl.text          = $"{emoji}  {label}";
            lbl.fontSize      = 13f;
            lbl.fontStyle     = FontStyles.Bold;
            lbl.alignment     = TextAlignmentOptions.Center;
            lbl.color         = Color.white;
            lbl.raycastTarget = false;
            var lblRT = lblGO.GetComponent<RectTransform>();
            lblRT.anchorMin = new Vector2(0f, 0.04f);
            lblRT.anchorMax = new Vector2(1f, 0.38f);
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;

            // Wire button colours
            var btn    = GetOrAdd<Button>(btnGO);
            var colors = btn.colors;
            colors.normalColor      = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.8f);
            colors.pressedColor     = new Color(0.7f, 0.7f, 0.7f);
            colors.disabledColor    = new Color(0.35f, 0.35f, 0.35f, 0.7f);
            btn.colors        = colors;
            btn.targetGraphic = cardBg;

            Debug.Log($"[RealmPrefabBuilder] \u2705 Card styled: {btnName}");
        }

        // ════════════════════════════════════════════════════════════════════
        //  PREFAB FACTORIES
        // ════════════════════════════════════════════════════════════════════

        private static GameObject CreateSparkPrefab()
        {
            const string path = PREF_ROOT + "/Emberforge/Spark.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[RealmPrefabBuilder] Spark prefab exists — skipping.");
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var sprite = GetOrCreateCircleSprite("Spark", new Color(1f, 0.55f, 0.1f), 64);

            var go = new GameObject("Spark");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = new Color(1f, 0.55f, 0.1f);
            sr.sortingOrder = 2;

            // Add small 2D collider for click/touch detection
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.isTrigger = true;

            go.AddComponent<AscendantContinuum.Emberforge.Spark>();
            go.transform.localScale = Vector3.one * 0.5f;

            return SavePrefab(go, path);
        }

        private static GameObject CreatePlantPrefab()
        {
            const string path = PREF_ROOT + "/Verdant/MagicalPlant.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[RealmPrefabBuilder] MagicalPlant prefab exists — skipping.");
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var sprite = GetOrCreateCircleSprite("Plant", new Color(0.2f, 0.8f, 0.3f), 64);

            var go = new GameObject("MagicalPlant");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = new Color(0.2f, 0.8f, 0.3f);
            sr.sortingOrder = 2;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            go.AddComponent<AscendantContinuum.Realms.Verdant.MagicalPlant>();

            return SavePrefab(go, path);
        }

        private static GameObject CreateStarPrefab()
        {
            const string path = PREF_ROOT + "/EchoFields/Star.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[RealmPrefabBuilder] Star prefab exists — skipping.");
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var sprite = GetOrCreateCircleSprite("Star", new Color(0.85f, 0.9f, 1f), 48);

            var go = new GameObject("Star");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = new Color(0.85f, 0.9f, 1f);
            sr.sortingOrder = 3;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.isTrigger = true;

            go.AddComponent<AscendantContinuum.Realms.EchoFields.Star>();

            // Glow child
            var glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(go.transform, false);
            var glowSR = glowGO.AddComponent<SpriteRenderer>();
            glowSR.sprite = sprite;
            glowSR.color  = new Color(0.4f, 0.6f, 1f, 0.4f);
            glowSR.sortingOrder = 2;
            glowGO.transform.localScale = Vector3.one * 1.6f;
            glowGO.SetActive(false);

            // Wire glow into Star component via SerializedObject on the temp instance
            var starComp = go.GetComponent<AscendantContinuum.Realms.EchoFields.Star>();
            var so = new UnityEditor.SerializedObject(starComp);
            so.FindProperty("glowEffect").objectReferenceValue = glowGO;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SavePrefab(go, path);
        }

        private static GameObject CreateLanternPrefab()
        {
            const string path = PREF_ROOT + "/LanternAscension/Lantern.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[RealmPrefabBuilder] Lantern prefab exists — skipping.");
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var sprite = GetOrCreateCircleSprite("Lantern", new Color(1f, 0.85f, 0.3f), 56);

            var go = new GameObject("Lantern");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = new Color(1f, 0.85f, 0.3f);
            sr.sortingOrder = 3;

            go.AddComponent<Lantern>();
            go.transform.localScale = Vector3.one * 0.7f;

            return SavePrefab(go, path);
        }

        private static GameObject CreatePrismPrefab()
        {
            const string path = PREF_ROOT + "/DawnCitadel/Prism.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[RealmPrefabBuilder] Prism prefab exists — skipping.");
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var sprite = GetOrCreateCircleSprite("Prism", new Color(0.7f, 0.9f, 1f), 64);

            var go = new GameObject("Prism");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = new Color(0.7f, 0.9f, 1f, 0.85f);
            sr.sortingOrder = 2;

            // 3D collider for LightRefractionPuzzle's light-beam raycast (Physics.Raycast)
            go.AddComponent<BoxCollider>();
            // 2D collider for TouchInputManager's tap detection (Physics2D.Raycast)
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<Prism>();

            return SavePrefab(go, path);
        }

        private static GameObject CreateLightTargetPrefab()
        {
            const string path = PREF_ROOT + "/DawnCitadel/LightTarget.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[RealmPrefabBuilder] LightTarget prefab exists — skipping.");
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var sprite = GetOrCreateCircleSprite("LightTarget", new Color(1f, 1f, 0.4f), 48);

            var go = new GameObject("LightTarget");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color  = new Color(1f, 1f, 0.4f);
            sr.sortingOrder = 1;

            go.AddComponent<BoxCollider>();
            go.AddComponent<LightTarget>();

            return SavePrefab(go, path);
        }

        private static GameObject CreateLineRendererPrefab()
        {
            const string path = PREF_ROOT + "/Shared/StarLine.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                Debug.Log("[RealmPrefabBuilder] StarLine prefab exists — skipping.");
                return existing;
            }

            var go = new GameObject("StarLine");
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace  = true;
            lr.startWidth     = 0.05f;
            lr.endWidth       = 0.05f;
            lr.startColor     = new Color(0.5f, 0.7f, 1f, 0.85f);
            lr.endColor       = new Color(0.5f, 0.7f, 1f, 0.85f);
            lr.positionCount  = 2;
            lr.material       = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder   = 4;

            return SavePrefab(go, path);
        }

        // ════════════════════════════════════════════════════════════════════
        //  SCENE WIRING
        // ════════════════════════════════════════════════════════════════════

        private static void WireEmberforge(GameObject sparkPrefab)
        {
            var scene = OpenRealmScene("Realm_Emberforge");
            if (!scene.IsValid()) return;

            // Configure camera
            ConfigureRealmCamera(scene, new Color(0.12f, 0.05f, 0.02f));

            // Scaffold gameplay root if scene is empty
            var sparks = FindInScene<EmberforgeSparks>(scene);
            if (sparks == null)
            {
                var root = GetOrCreateSceneRoot(scene, "[ Emberforge Gameplay ]");
                sparks = GetOrAdd<EmberforgeSparks>(root);
                GetOrAdd<AscendantContinuum.Realms.Emberforge.EmberforgeController>(root);
                Debug.Log("[RealmPrefabBuilder] Scaffolded Emberforge gameplay root.");
            }
            SetPrefabField(sparks, "sparkPrefab", sparkPrefab);

            // BackButton
            WireBackButton(scene);

            // Settings + Pause
            var sp1 = EnsureSettingsPanel(scene);
            EnsurePauseMenu(scene, sp1);

            // Completion panel
            EnsureCompletionPanel(scene, sparks);

            // HUD realm name
            SetHUDRealmName(scene, "The Emberforge — Collect the Dancing Sparks");

            // Background image + colour tint
            SetRealmBackgroundImage(scene, "Assets/_Project/Art/AscendantContinuumGameGraphics/bg_emberforge.png");
            SetBackgroundColor(scene, Color.white);

            SaveAndCloseScene(scene);
        }

        private static void WireVerdant(GameObject plantPrefab)
        {
            var scene = OpenRealmScene("Realm_Verdant");
            if (!scene.IsValid()) return;

            ConfigureRealmCamera(scene, new Color(0.03f, 0.12f, 0.05f));

            // Scaffold gameplay root if scene is empty
            var garden = FindInScene<VerdantGarden>(scene);
            if (garden == null)
            {
                var root = GetOrCreateSceneRoot(scene, "[ Verdant Gameplay ]");
                garden = GetOrAdd<VerdantGarden>(root);
                GetOrAdd<AscendantContinuum.Verdant.VerdantController>(root);
                Debug.Log("[RealmPrefabBuilder] Scaffolded Verdant gameplay root.");
            }
            SetPrefabField(garden, "plantPrefab", plantPrefab);

            WireBackButton(scene);
            var sp2 = EnsureSettingsPanel(scene);
            EnsurePauseMenu(scene, sp2);
            EnsureCompletionPanel(scene, garden);
            SetHUDRealmName(scene, "The Verdant Garden — Nurture Life");
            SetRealmBackgroundImage(scene, "Assets/_Project/Art/AscendantContinuumGameGraphics/bg_VerdantSantuary.png");
            SetBackgroundColor(scene, Color.white);

            SaveAndCloseScene(scene);
        }

        private static void WireEchoFields(GameObject starPrefab, GameObject lineRenPrefab)
        {
            var scene = OpenRealmScene("Realm_EchoFields");
            if (!scene.IsValid()) return;

            ConfigureRealmCamera(scene, new Color(0.02f, 0.02f, 0.14f));

            // Scaffold gameplay root if scene is empty
            var tracer = FindInScene<ConstellationTracer>(scene);
            if (tracer == null)
            {
                var root = GetOrCreateSceneRoot(scene, "[ EchoFields Gameplay ]");
                tracer = GetOrAdd<ConstellationTracer>(root);
                GetOrAdd<AscendantContinuum.EchoFields.EchoFieldsController>(root);
                // Create secret-tagged helpers for moon-phase accessibility
                CreateTaggedChild(root, "[ NewMoon Secret ]", "NewMoonSecret");
                CreateTaggedChild(root, "[ FullMoon Secret ]", "FullMoonSecret");
                Debug.Log("[RealmPrefabBuilder] Scaffolded EchoFields gameplay root.");
            }
            SetPrefabField(tracer, "starPrefab", starPrefab);
            SetLineRendererPrefabField(tracer, "lineRendererPrefab", lineRenPrefab);

            WireBackButton(scene);
            var sp3 = EnsureSettingsPanel(scene);
            EnsurePauseMenu(scene, sp3);
            EnsureCompletionPanel(scene, tracer);
            SetHUDRealmName(scene, "The Echo Fields — Trace the Constellations");
            SetRealmBackgroundImage(scene, "Assets/_Project/Art/AscendantContinuumGameGraphics/bg_EchoFields.png");
            SetBackgroundColor(scene, Color.white);

            SaveAndCloseScene(scene);
        }

        private static void WireDawnCitadel(GameObject prismPrefab, GameObject targetPrefab, GameObject lineRenPrefab)
        {
            var scene = OpenRealmScene("Realm_DawnCitadel");
            if (!scene.IsValid()) return;

            ConfigureRealmCamera(scene, new Color(0.12f, 0.10f, 0.02f));

            // Scaffold gameplay root if scene is empty
            if (FindGoInScene(scene, "[ DawnCitadel Gameplay ]") == null)
            {
                var root = GetOrCreateSceneRoot(scene, "[ DawnCitadel Gameplay ]");
                GetOrAdd<LightRefractionPuzzle>(root);
                GetOrAdd<AscendantContinuum.Realms.DawnCitadel.DawnCitadelController>(root);
                Debug.Log("[RealmPrefabBuilder] Scaffolded DawnCitadel gameplay root.");
            }

            // Build a default puzzle layout inside the Gameplay root
            BuildDawnCitadelPuzzle(scene, prismPrefab, targetPrefab, lineRenPrefab);

            WireBackButton(scene);
            var puzzle = FindInScene<LightRefractionPuzzle>(scene);
            var sp4 = EnsureSettingsPanel(scene);
            EnsurePauseMenu(scene, sp4);
            EnsureCompletionPanel(scene, puzzle);
            SetHUDRealmName(scene, "The Dawn Citadel — Bend Light, Reveal Truth");
            SetRealmBackgroundImage(scene, "Assets/_Project/Art/AscendantContinuumGameGraphics/bg_DawnCitadel.jpeg");
            SetBackgroundColor(scene, Color.white);

            SaveAndCloseScene(scene);
        }

        private static void WireLanternAscension(GameObject lanternPrefab)
        {
            var scene = OpenRealmScene("Realm_LanternAscension");
            if (!scene.IsValid()) return;

            ConfigureRealmCamera(scene, new Color(0.06f, 0.03f, 0.12f));

            // Scaffold gameplay root if scene is empty
            var ritual = FindInScene<LanternRitual>(scene);
            if (ritual == null)
            {
                var root = GetOrCreateSceneRoot(scene, "[ LanternAscension Gameplay ]");
                ritual = GetOrAdd<LanternRitual>(root);
                GetOrAdd<AscendantContinuum.Realms.LanternAscension.LanternAscensionController>(root);
                Debug.Log("[RealmPrefabBuilder] Scaffolded LanternAscension gameplay root.");
            }
            SetPrefabField(ritual, "lanternPrefab", lanternPrefab);

            // Ensure a release point transform exists
            EnsureLanternReleasePoint(scene, ritual);

            // Ensure a wish input panel exists
            EnsureWishInputPanel(scene, ritual);

            WireBackButton(scene);
            var sp5 = EnsureSettingsPanel(scene);
            EnsurePauseMenu(scene, sp5);
            EnsureCompletionPanel(scene, ritual);
            SetHUDRealmName(scene, "The Lantern Ascension — Release Your Wish");
            SetRealmBackgroundImage(scene, "Assets/_Project/Art/AscendantContinuumGameGraphics/bg_LanternAscension.jpeg");
            SetBackgroundColor(scene, Color.white);

            SaveAndCloseScene(scene);
        }

        // ────────────────────────────────────────────────────────────────────
        //  Dawn Citadel: assemble one default puzzle
        // ────────────────────────────────────────────────────────────────────

        private static void BuildDawnCitadelPuzzle(
            UnityEngine.SceneManagement.Scene scene,
            GameObject prismPrefab,
            GameObject targetPrefab,
            GameObject lineRenPrefab)
        {
            // Find the Gameplay root
            var gameplayRoot = FindGoInScene(scene, "[ DawnCitadel Gameplay ]");
            if (gameplayRoot == null) return;

            var puzzle = gameplayRoot.GetComponent<LightRefractionPuzzle>();
            if (puzzle == null) return;

            var so = new SerializedObject(puzzle);

            // Light source
            var lightSourceGO = GetOrCreateChild(gameplayRoot, "LightSource");
            lightSourceGO.transform.localPosition = new Vector3(-5f, 0f, -1f);
            SetTransformField(so, "lightSource", lightSourceGO.transform);

            // Line renderer for the main beam
            var beamGO = GetOrCreateChild(gameplayRoot, "LightBeam");
            var lr     = GetOrAdd<LineRenderer>(beamGO);
            lr.startWidth = 0.08f;
            lr.endWidth   = 0.08f;
            lr.startColor = new Color(1f, 0.95f, 0.3f);
            lr.endColor   = new Color(1f, 0.95f, 0.3f);
            lr.material   = new Material(Shader.Find("Sprites/Default"));
            so.FindProperty("lightBeamRenderer").objectReferenceValue = lr;

            // Beam material
            so.FindProperty("lightBeamMaterial").objectReferenceValue = lr.material;

            // Create prisms (only if the array is empty)
            var prismsArrayProp = so.FindProperty("prisms");
            if (prismsArrayProp != null && prismsArrayProp.arraySize == 0)
            {
                prismsArrayProp.arraySize = 2;
                for (int i = 0; i < 2; i++)
                {
                    var prismInst = (GameObject)PrefabUtility.InstantiatePrefab(prismPrefab, gameplayRoot.transform);
                    prismInst.name = $"Prism_{i + 1}";
                    prismInst.transform.localPosition = new Vector3(i * 3f - 2f, 0f, 0f);
                    Undo.RegisterCreatedObjectUndo(prismInst, "Add Prism");
                    prismsArrayProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        prismInst.GetComponent<Prism>();
                }
            }

            // Create targets
            var targetsArrayProp = so.FindProperty("targets");
            if (targetsArrayProp != null && targetsArrayProp.arraySize == 0)
            {
                targetsArrayProp.arraySize = 1;
                var targetInst = (GameObject)PrefabUtility.InstantiatePrefab(targetPrefab, gameplayRoot.transform);
                targetInst.name = "LightTarget_1";
                targetInst.transform.localPosition = new Vector3(5f, 0f, 0f);
                Undo.RegisterCreatedObjectUndo(targetInst, "Add LightTarget");
                targetsArrayProp.GetArrayElementAtIndex(0).objectReferenceValue =
                    targetInst.GetComponent<LightTarget>();
            }

            so.ApplyModifiedProperties();
        }

        // ────────────────────────────────────────────────────────────────────
        //  Lantern release point & wish input UI
        // ────────────────────────────────────────────────────────────────────

        private static void EnsureLanternReleasePoint(
            UnityEngine.SceneManagement.Scene scene,
            LanternRitual ritual)
        {
            var gameplayRoot = ritual.gameObject;
            var releaseGO    = GetOrCreateChild(gameplayRoot, "LanternReleasePoint");
            releaseGO.transform.localPosition = new Vector3(0f, -2f, 0f);

            var so = new SerializedObject(ritual);
            so.FindProperty("lanternReleasePoint").objectReferenceValue = releaseGO.transform;
            so.ApplyModifiedProperties();
        }

        private static void EnsureWishInputPanel(
            UnityEngine.SceneManagement.Scene scene,
            LanternRitual ritual)
        {
            // Find or create HUDCanvas
            var canvas = FindGoInScene(scene, "HUDCanvas");
            if (canvas == null) return;

            // Wish input panel
            var panelGO = canvas.transform.Find("WishInputPanel")?.gameObject;
            if (panelGO == null)
            {
                panelGO = new GameObject("WishInputPanel");
                panelGO.transform.SetParent(canvas.transform, false);
                var img = panelGO.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.75f);
                var rt = panelGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.1f, 0.2f);
                rt.anchorMax = new Vector2(0.9f, 0.55f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                // Title
                var titleGO = new GameObject("Title");
                titleGO.transform.SetParent(panelGO.transform, false);
                var titleText = titleGO.AddComponent<TextMeshProUGUI>();
                titleText.text = "What is your wish?";
                titleText.fontSize = 22f;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = new Color(1f, 0.9f, 0.6f);
                var titleRT = titleGO.GetComponent<RectTransform>();
                titleRT.anchorMin = new Vector2(0f, 0.7f);
                titleRT.anchorMax = Vector2.one;
                titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

                // InputField
                var inputGO = new GameObject("WishInputField");
                inputGO.transform.SetParent(panelGO.transform, false);
                var inputField = inputGO.AddComponent<TMP_InputField>();
                var inputImg = inputGO.AddComponent<Image>();
                inputImg.color = new Color(0.1f, 0.08f, 0.18f);
                var inputRT = inputGO.GetComponent<RectTransform>();
                inputRT.anchorMin = new Vector2(0.05f, 0.35f);
                inputRT.anchorMax = new Vector2(0.95f, 0.70f);
                inputRT.offsetMin = inputRT.offsetMax = Vector2.zero;

                // TMP requires a Text Area with child text components
                var textAreaGO = new GameObject("Text Area");
                textAreaGO.transform.SetParent(inputGO.transform, false);
                var viewport = textAreaGO.AddComponent<RectMask2D>();
                var textAreaRT = textAreaGO.GetComponent<RectTransform>();
                textAreaRT.anchorMin = Vector2.zero;
                textAreaRT.anchorMax = Vector2.one;
                textAreaRT.offsetMin = new Vector2(4f, 4f);
                textAreaRT.offsetMax = new Vector2(-4f, -4f);

                var inputTextGO = new GameObject("Text");
                inputTextGO.transform.SetParent(textAreaGO.transform, false);
                var inputTextComp = inputTextGO.AddComponent<TextMeshProUGUI>();
                inputTextComp.fontSize = 16f;
                inputTextComp.color = Color.white;
                var inputTextRT = inputTextGO.GetComponent<RectTransform>();
                inputTextRT.anchorMin = Vector2.zero;
                inputTextRT.anchorMax = Vector2.one;
                inputTextRT.offsetMin = inputTextRT.offsetMax = Vector2.zero;

                var placeholderGO = new GameObject("Placeholder");
                placeholderGO.transform.SetParent(textAreaGO.transform, false);
                var placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
                placeholder.text = "Write your wish…";
                placeholder.fontSize = 16f;
                placeholder.color = new Color(0.6f, 0.55f, 0.7f);
                placeholder.fontStyle = FontStyles.Italic;
                var phRT = placeholderGO.GetComponent<RectTransform>();
                phRT.anchorMin = Vector2.zero;
                phRT.anchorMax = Vector2.one;
                phRT.offsetMin = phRT.offsetMax = Vector2.zero;

                inputField.textViewport = textAreaRT;
                inputField.textComponent = inputTextComp;
                inputField.placeholder = placeholder;

                // Confirm button
                var confirmGO = new GameObject("ConfirmButton");
                confirmGO.transform.SetParent(panelGO.transform, false);
                confirmGO.AddComponent<Image>().color = new Color(0.5f, 0.3f, 0.8f);
                var confirmBtn = confirmGO.AddComponent<Button>();
                var confirmRT  = confirmGO.GetComponent<RectTransform>();
                confirmRT.anchorMin = new Vector2(0.3f, 0.05f);
                confirmRT.anchorMax = new Vector2(0.7f, 0.28f);
                confirmRT.offsetMin = confirmRT.offsetMax = Vector2.zero;

                var confirmLabel = new GameObject("Label");
                confirmLabel.transform.SetParent(confirmGO.transform, false);
                var confirmTxt = confirmLabel.AddComponent<TextMeshProUGUI>();
                confirmTxt.text = "Release Lantern ✦";
                confirmTxt.fontSize = 16f;
                confirmTxt.alignment = TextAlignmentOptions.Center;
                confirmTxt.color = Color.white;
                var confirmLabelRT = confirmLabel.GetComponent<RectTransform>();
                confirmLabelRT.anchorMin = Vector2.zero;
                confirmLabelRT.anchorMax = Vector2.one;
                confirmLabelRT.offsetMin = confirmLabelRT.offsetMax = Vector2.zero;

                // Wire confirm button to ritual.ConfirmWish
                // (done at runtime via WishInputConfirmButton component)
                var confirmClickWirer = confirmGO.AddComponent<WishInputConfirmButton>();
                var ccSO = new SerializedObject(confirmClickWirer);
                ccSO.FindProperty("ritual").objectReferenceValue = ritual;
                ccSO.ApplyModifiedProperties();

                panelGO.SetActive(false); // Hidden until player taps the ritual button
            }

            // Ensure a "Release Lantern" button on the HUD
            EnsureReleaseLanternButton(canvas, ritual);

            var so = new SerializedObject(ritual);
            var wishPanel = canvas.transform.Find("WishInputPanel")?.gameObject;
            if (wishPanel != null)
                so.FindProperty("wishInputPanel").objectReferenceValue = wishPanel;

            var inputFieldComp = canvas.transform
                .Find("WishInputPanel/WishInputField")?.GetComponent<TMP_InputField>();
            if (inputFieldComp != null)
                so.FindProperty("wishInputField").objectReferenceValue = inputFieldComp;

            so.ApplyModifiedProperties();
        }

        private static void EnsureReleaseLanternButton(GameObject canvas, LanternRitual ritual)
        {
            if (canvas.transform.Find("ReleaseLanternButton") != null) return;

            var btnGO = new GameObject("ReleaseLanternButton");
            btnGO.transform.SetParent(canvas.transform, false);
            btnGO.AddComponent<Image>().color = new Color(0.6f, 0.4f, 0.9f);
            btnGO.AddComponent<Button>();

            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.3f, 0.08f);
            rt.anchorMax = new Vector2(0.7f, 0.20f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelTxt = labelGO.AddComponent<TextMeshProUGUI>();
            labelTxt.text = "✦  Create Lantern";
            labelTxt.fontSize = 18f;
            labelTxt.alignment = TextAlignmentOptions.Center;
            labelTxt.color = Color.white;
            var labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = labelRT.offsetMax = Vector2.zero;

            // Wire at runtime via tiny helper component
            var wirer = btnGO.AddComponent<ReleaseLanternButtonWirer>();
            var so = new SerializedObject(wirer);
            so.FindProperty("ritual").objectReferenceValue = ritual;
            so.ApplyModifiedProperties();
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS — scene manipulation
        // ════════════════════════════════════════════════════════════════════

        private static void WireBackButton(UnityEngine.SceneManagement.Scene scene)
        {
            var backGO = FindGoInScene(scene, "BackButton");
            if (backGO == null)
            {
                Debug.LogWarning($"[RealmPrefabBuilder] No BackButton found in {scene.name}");
                return;
            }

            // Ensure Button component
            var btn = backGO.GetComponent<Button>();
            if (btn == null)
            {
                btn = backGO.AddComponent<Button>();
                backGO.AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.35f);
            }

            // Ensure RealmBackButton component
            if (backGO.GetComponent<RealmBackButton>() == null)
                backGO.AddComponent<RealmBackButton>();

            // Make label readable
            var labelTmp = backGO.GetComponentInChildren<TextMeshProUGUI>();
            if (labelTmp != null)
            {
                labelTmp.text = "← Exit";
                labelTmp.color = new Color(0.9f, 0.85f, 1f);
            }
            else
            {
                var legacyText = backGO.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = "← Exit";
                    legacyText.color = new Color(0.9f, 0.85f, 1f);
                }
            }

            // Ensure EventSystem exists
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private static void SetHUDRealmName(UnityEngine.SceneManagement.Scene scene, string name)
        {
            EnsureFullHUD(scene, name);
        }

        /// <summary>
        /// Builds a complete HUD inside HUDCanvas and wires all fields in HUDManager.
        /// Safe to call multiple times — existing child GameObjects are reused.
        /// </summary>
        private static void EnsureFullHUD(UnityEngine.SceneManagement.Scene scene, string realmLabel)
        {
            var hudCanvas = FindGoInScene(scene, "HUDCanvas");
            if (hudCanvas == null) return;

            var hud = hudCanvas.GetComponent<HUDManager>();
            if (hud == null) hud = hudCanvas.AddComponent<HUDManager>();

            var so = new SerializedObject(hud);

            // ── CanvasGroup for fade ──────────────────────────────────────
            var cg = GetOrAdd<CanvasGroup>(hudCanvas);
            so.FindProperty("hudCanvasGroup").objectReferenceValue = cg;

            // ── Top bar ──────────────────────────────────────────────────
            var topBar = GetOrCreateChildGO(hudCanvas, "TopBar");
            {
                var rt = GetOrAdd<RectTransform>(topBar);
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot     = new Vector2(0.5f, 1f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(0f, 64f);
                GetOrAdd<Image>(topBar).color = new Color(0f, 0f, 0f, 0.45f);
            }

            // Realm name (top-left)
            var realmNameGO = GetOrCreateChildGO(topBar, "RealmNameText");
            {
                var tmp = GetOrAdd<TextMeshProUGUI>(realmNameGO);
                tmp.text      = realmLabel;
                tmp.fontSize  = 18f;
                tmp.color     = new Color(1f, 0.95f, 0.8f);
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                var rt = realmNameGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = new Vector2(0.65f, 1f);
                rt.offsetMin = new Vector2(14f, 0f);
                rt.offsetMax = Vector2.zero;
                so.FindProperty("realmNameText").objectReferenceValue = tmp;
            }

            // Sparks counter (top-right)
            var sparksPanel = GetOrCreateChildGO(topBar, "SparksPanel");
            {
                var rt = sparksPanel.GetComponent<RectTransform>();
                if (rt == null) rt = sparksPanel.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.65f, 0f);
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;

                var iconGO = GetOrCreateChildGO(sparksPanel, "SparksIcon");
                var iconImg = GetOrAdd<Image>(iconGO);
                iconImg.color = new Color(1f, 0.6f, 0.2f);
                var iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.05f, 0.15f);
                iconRT.anchorMax = new Vector2(0.35f, 0.85f);
                iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
                so.FindProperty("sparksCountIcon").objectReferenceValue = iconImg;

                var sparksTextGO = GetOrCreateChildGO(sparksPanel, "SparksCountText");
                var sparksTMP = GetOrAdd<TextMeshProUGUI>(sparksTextGO);
                sparksTMP.text      = "0";
                sparksTMP.fontSize  = 22f;
                sparksTMP.color     = new Color(1f, 0.85f, 0.5f);
                sparksTMP.alignment = TextAlignmentOptions.MidlineLeft;
                var sparksRT = sparksTextGO.GetComponent<RectTransform>();
                sparksRT.anchorMin = new Vector2(0.38f, 0f);
                sparksRT.anchorMax = Vector2.one;
                sparksRT.offsetMin = sparksRT.offsetMax = Vector2.zero;
                so.FindProperty("sparksCountText").objectReferenceValue = sparksTMP;
            }

            // ── Notification panel (top-center, hidden by default) ────────
            var notifPanel = GetOrCreateChildGO(hudCanvas, "NotificationPanel");
            {
                var rt = notifPanel.GetComponent<RectTransform>();
                if (rt == null) rt = notifPanel.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.1f, 0.82f);
                rt.anchorMax = new Vector2(0.9f, 0.95f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                GetOrAdd<Image>(notifPanel).color = new Color(0.1f, 0.05f, 0.2f, 0.9f);
                notifPanel.SetActive(false);
                so.FindProperty("notificationPanel").objectReferenceValue = notifPanel;

                var notifIconGO = GetOrCreateChildGO(notifPanel, "NotificationIcon");
                var notifIcon = GetOrAdd<Image>(notifIconGO);
                notifIcon.color = new Color(0.8f, 0.7f, 1f);
                var niRT = notifIconGO.GetComponent<RectTransform>();
                niRT.anchorMin = new Vector2(0.02f, 0.1f);
                niRT.anchorMax = new Vector2(0.12f, 0.9f);
                niRT.offsetMin = niRT.offsetMax = Vector2.zero;
                so.FindProperty("notificationIcon").objectReferenceValue = notifIcon;

                var notifTextGO = GetOrCreateChildGO(notifPanel, "NotificationText");
                var notifTMP = GetOrAdd<TextMeshProUGUI>(notifTextGO);
                notifTMP.text      = "";
                notifTMP.fontSize  = 16f;
                notifTMP.color     = Color.white;
                notifTMP.alignment = TextAlignmentOptions.MidlineLeft;
                var ntRT = notifTextGO.GetComponent<RectTransform>();
                ntRT.anchorMin = new Vector2(0.14f, 0f);
                ntRT.anchorMax = Vector2.one;
                ntRT.offsetMin = new Vector2(4f, 0f);
                ntRT.offsetMax = Vector2.zero;
                so.FindProperty("notificationText").objectReferenceValue = notifTMP;
            }

            // ── Daily challenge panel (bottom strip) ──────────────────────
            var challengePanel = GetOrCreateChildGO(hudCanvas, "DailyChallengePanel");
            {
                var rt = challengePanel.GetComponent<RectTransform>();
                if (rt == null) rt = challengePanel.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot     = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2(14f, 80f);
                rt.sizeDelta = new Vector2(0f, 56f);
                GetOrAdd<Image>(challengePanel).color = new Color(0f, 0f, 0f, 0.35f);
                so.FindProperty("dailyChallengePanel").objectReferenceValue = challengePanel;

                var ctGO = GetOrCreateChildGO(challengePanel, "ChallengeTitleText");
                var ctTMP = GetOrAdd<TextMeshProUGUI>(ctGO);
                ctTMP.text = "Daily Challenge";
                ctTMP.fontSize = 13f;
                ctTMP.color = new Color(0.85f, 0.75f, 1f);
                var ctRT = ctGO.GetComponent<RectTransform>();
                ctRT.anchorMin = new Vector2(0.02f, 0.5f);
                ctRT.anchorMax = Vector2.one;
                ctRT.offsetMin = ctRT.offsetMax = Vector2.zero;
                so.FindProperty("challengeTitleText").objectReferenceValue = ctTMP;

                var cpGO = GetOrCreateChildGO(challengePanel, "ChallengeProgressText");
                var cpTMP = GetOrAdd<TextMeshProUGUI>(cpGO);
                cpTMP.text = "0 / 10";
                cpTMP.fontSize = 12f;
                cpTMP.color = new Color(0.7f, 0.65f, 0.9f);
                var cpRT = cpGO.GetComponent<RectTransform>();
                cpRT.anchorMin = new Vector2(0.02f, 0f);
                cpRT.anchorMax = new Vector2(0.7f, 0.5f);
                cpRT.offsetMin = cpRT.offsetMax = Vector2.zero;
                so.FindProperty("challengeProgressText").objectReferenceValue = cpTMP;

                // Progress bar image
                var barBG = GetOrCreateChildGO(challengePanel, "ProgressBarBG");
                GetOrAdd<Image>(barBG).color = new Color(0.15f, 0.1f, 0.25f);
                var barBGRT = barBG.GetComponent<RectTransform>();
                barBGRT.anchorMin = new Vector2(0.72f, 0.15f);
                barBGRT.anchorMax = new Vector2(0.98f, 0.45f);
                barBGRT.offsetMin = barBGRT.offsetMax = Vector2.zero;

                var barFill = GetOrCreateChildGO(barBG, "ProgressBarFill");
                var barImg = GetOrAdd<Image>(barFill);
                barImg.color = new Color(0.6f, 0.4f, 1f);
                barImg.type = Image.Type.Filled;
                barImg.fillMethod = Image.FillMethod.Horizontal;
                barImg.fillAmount = 0f;
                var barRT = barFill.GetComponent<RectTransform>();
                barRT.anchorMin = Vector2.zero;
                barRT.anchorMax = Vector2.one;
                barRT.offsetMin = barRT.offsetMax = Vector2.zero;
                so.FindProperty("challengeProgressBar").objectReferenceValue = barImg;
            }

            // ── Season XP mini-bar ────────────────────────────────────────
            var seasonGO = GetOrCreateChildGO(hudCanvas, "SeasonXPBar");
            {
                var slider = GetOrAdd<Slider>(seasonGO);
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value    = 0f;
                var rt = seasonGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot     = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-14f, 80f);
                rt.sizeDelta = new Vector2(160f, 14f);
                so.FindProperty("seasonXPBar").objectReferenceValue = slider;

                var tierGO = GetOrCreateChildGO(hudCanvas, "SeasonTierLabel");
                var tierTMP = GetOrAdd<TextMeshProUGUI>(tierGO);
                tierTMP.text      = "Tier 1";
                tierTMP.fontSize  = 12f;
                tierTMP.color     = new Color(0.7f, 0.6f, 1f);
                tierTMP.alignment = TextAlignmentOptions.Right;
                var tierRT = tierGO.GetComponent<RectTransform>();
                tierRT.anchorMin = new Vector2(0.7f, 0f);
                tierRT.anchorMax = new Vector2(1f, 0f);
                tierRT.pivot     = new Vector2(1f, 0f);
                tierRT.anchoredPosition = new Vector2(-14f, 98f);
                tierRT.sizeDelta = new Vector2(0f, 22f);
                so.FindProperty("seasonTierLabel").objectReferenceValue = tierTMP;
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[RealmPrefabBuilder] HUD built for {scene.name}");
        }

        // Helper: Get-or-create a named child without RectTransform dependency
        private static GameObject GetOrCreateChildGO(GameObject parent, string name)
        {
            var rt = parent.GetComponent<RectTransform>();
            if (rt != null)
            {
                // UI context — use Transform.Find
                var existing = parent.transform.Find(name);
                if (existing != null) return existing.gameObject;
                var go = new GameObject(name);
                go.transform.SetParent(parent.transform, false);
                go.AddComponent<RectTransform>();
                return go;
            }
            else
            {
                return GetOrCreateChild(parent, name);
            }
        }

        private static void ConfigureRealmCamera(
            UnityEngine.SceneManagement.Scene scene, Color ambientColor)
        {
            Camera cam = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                cam = root.GetComponentInChildren<Camera>(true);
                if (cam != null) break;
            }

            if (cam == null) return;

            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = ambientColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            // AudioListener guard — only add if none in scene
            if (cam.GetComponent<AudioListener>() == null &&
                UnityEngine.Object.FindFirstObjectByType<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();
        }

        private static void SetBackgroundColor(
            UnityEngine.SceneManagement.Scene scene, Color col)
        {
            var bgGO = FindGoInScene(scene, "[ Background ]");
            if (bgGO == null) return;

            var sr = bgGO.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = col;
        }

        /// <summary>
        /// Builds or updates a RealmCompletionPanel overlay inside HUDCanvas and
        /// wires its serialized 'completionPanel' field on the target MonoBehaviour.
        /// </summary>
        // ════════════════════════════════════════════════════════════════════
        //  PAUSE MENU + SETTINGS PANEL
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Builds/wires a SettingsPanel overlay (full-screen, hidden by default)
        /// inside HUDCanvas and returns the SettingsPanel component.
        /// </summary>
        private static AscendantContinuum.UI.SettingsPanel EnsureSettingsPanel(
            UnityEngine.SceneManagement.Scene scene)
        {
            var hudCanvas = FindGoInScene(scene, "HUDCanvas");
            if (hudCanvas == null) return null;

            var panelGO = hudCanvas.transform.Find("SettingsPanel")?.gameObject;
            if (panelGO == null)
            {
                panelGO = new GameObject("SettingsPanel");
                panelGO.transform.SetParent(hudCanvas.transform, false);
            }

            // Full-screen dimmer
            var bg = GetOrAdd<Image>(panelGO);
            bg.color = new Color(0.04f, 0.02f, 0.1f, 0.94f);
            var bgRT = panelGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

            // ── Title ──────────────────────────────────────────────────────
            var titleGO = GetOrCreateChildGO(panelGO, "SettingsTitle");
            var titleTMP = GetOrAdd<TextMeshProUGUI>(titleGO);
            titleTMP.text      = "⚙  Settings";
            titleTMP.fontSize  = 24f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color     = new Color(0.9f, 0.85f, 1f);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.86f);
            titleRT.anchorMax = new Vector2(0.9f, 0.96f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

            // ── Audio section label ────────────────────────────────────────
            var audioLblGO = GetOrCreateChildGO(panelGO, "AudioLabel");
            var audioLbl = GetOrAdd<TextMeshProUGUI>(audioLblGO);
            audioLbl.text      = "AUDIO";
            audioLbl.fontSize  = 14f;
            audioLbl.color     = new Color(0.6f, 0.55f, 0.9f);
            audioLbl.alignment = TextAlignmentOptions.Left;
            var audioLblRT = audioLblGO.GetComponent<RectTransform>();
            audioLblRT.anchorMin = new Vector2(0.1f, 0.80f);
            audioLblRT.anchorMax = new Vector2(0.9f, 0.86f);
            audioLblRT.offsetMin = audioLblRT.offsetMax = Vector2.zero;

            // Helper to create a labelled slider row
            Slider BuildSliderRow(string rowName, string label, float anchorYMin, float anchorYMax,
                out TMP_Text valueLabel)
            {
                var rowGO = GetOrCreateChildGO(panelGO, rowName);
                var rowRT = rowGO.GetComponent<RectTransform>();
                rowRT.anchorMin = new Vector2(0.08f, anchorYMin);
                rowRT.anchorMax = new Vector2(0.92f, anchorYMax);
                rowRT.offsetMin = rowRT.offsetMax = Vector2.zero;

                var lblGO  = GetOrCreateChildGO(rowGO, "Label");
                var lbl    = GetOrAdd<TextMeshProUGUI>(lblGO);
                lbl.text      = label;
                lbl.fontSize  = 14f;
                lbl.color     = Color.white;
                lbl.alignment = TextAlignmentOptions.MidlineLeft;
                var lblRT = lblGO.GetComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero;
                lblRT.anchorMax = new Vector2(0.32f, 1f);
                lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;

                var sliderGO  = GetOrCreateChildGO(rowGO, "Slider");
                var slider    = GetOrAdd<Slider>(sliderGO);
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value    = 1f;
                // Slider background
                var sliderBG = GetOrCreateChildGO(sliderGO, "Background");
                GetOrAdd<Image>(sliderBG).color = new Color(0.15f, 0.1f, 0.25f);
                var sliderBGRT = sliderBG.GetComponent<RectTransform>();
                sliderBGRT.anchorMin = new Vector2(0f, 0.3f);
                sliderBGRT.anchorMax = new Vector2(1f, 0.7f);
                sliderBGRT.offsetMin = sliderBGRT.offsetMax = Vector2.zero;
                // Fill
                var fillAreaGO = GetOrCreateChildGO(sliderGO, "Fill Area");
                var fillGO     = GetOrCreateChildGO(fillAreaGO, "Fill");
                var fillImg    = GetOrAdd<Image>(fillGO);
                fillImg.color  = new Color(0.5f, 0.35f, 0.9f);
                var fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
                fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
                fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
                fillAreaRT.offsetMin = new Vector2(5f, 0f);
                fillAreaRT.offsetMax = new Vector2(-5f, 0f);
                var fillRT = fillGO.GetComponent<RectTransform>();
                fillRT.anchorMin = Vector2.zero;
                fillRT.anchorMax = Vector2.one;
                fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
                // Handle
                var handleSlideArea = GetOrCreateChildGO(sliderGO, "Handle Slide Area");
                var handleGO        = GetOrCreateChildGO(handleSlideArea, "Handle");
                var handleImg       = GetOrAdd<Image>(handleGO);
                handleImg.color     = new Color(0.8f, 0.7f, 1f);
                var hsaRT = handleSlideArea.GetComponent<RectTransform>();
                hsaRT.anchorMin = Vector2.zero;
                hsaRT.anchorMax = Vector2.one;
                hsaRT.offsetMin = new Vector2(10f, 0f);
                hsaRT.offsetMax = new Vector2(-10f, 0f);
                var handleRT = handleGO.GetComponent<RectTransform>();
                handleRT.anchorMin = new Vector2(0.5f, 0f);
                handleRT.anchorMax = new Vector2(0.5f, 1f);
                handleRT.sizeDelta = new Vector2(20f, 0f);
                slider.fillRect   = fillGO.GetComponent<RectTransform>();
                slider.handleRect = handleGO.GetComponent<RectTransform>();

                var sliderRT = sliderGO.GetComponent<RectTransform>();
                sliderRT.anchorMin = new Vector2(0.34f, 0f);
                sliderRT.anchorMax = new Vector2(0.80f, 1f);
                sliderRT.offsetMin = sliderRT.offsetMax = Vector2.zero;

                var valGO  = GetOrCreateChildGO(rowGO, "ValueLabel");
                var valTMP = GetOrAdd<TextMeshProUGUI>(valGO);
                valTMP.text      = "100%";
                valTMP.fontSize  = 13f;
                valTMP.color     = new Color(0.7f, 0.65f, 1f);
                valTMP.alignment = TextAlignmentOptions.MidlineLeft;
                var valRT = valGO.GetComponent<RectTransform>();
                valRT.anchorMin = new Vector2(0.82f, 0f);
                valRT.anchorMax = Vector2.one;
                valRT.offsetMin = valRT.offsetMax = Vector2.zero;
                valueLabel = valTMP;

                return slider;
            }

            TMP_Text masterLbl, musicLbl, sfxLbl, ambLbl;
            var masterSlider  = BuildSliderRow("MasterRow",  "Master",  0.72f, 0.80f, out masterLbl);
            var musicSlider   = BuildSliderRow("MusicRow",   "Music",   0.63f, 0.71f, out musicLbl);
            var sfxSlider     = BuildSliderRow("SFXRow",     "SFX",     0.54f, 0.62f, out sfxLbl);
            var ambientSlider = BuildSliderRow("AmbientRow", "Ambient", 0.45f, 0.53f, out ambLbl);

            // ── Accessibility section label ─────────────────────────────────
            var accLblGO = GetOrCreateChildGO(panelGO, "AccessibilityLabel");
            var accLbl = GetOrAdd<TextMeshProUGUI>(accLblGO);
            accLbl.text      = "ACCESSIBILITY";
            accLbl.fontSize  = 14f;
            accLbl.color     = new Color(0.6f, 0.55f, 0.9f);
            accLbl.alignment = TextAlignmentOptions.Left;
            var accLblRT = accLblGO.GetComponent<RectTransform>();
            accLblRT.anchorMin = new Vector2(0.1f, 0.39f);
            accLblRT.anchorMax = new Vector2(0.9f, 0.45f);
            accLblRT.offsetMin = accLblRT.offsetMax = Vector2.zero;

            // Helper for a toggle row
            Toggle BuildToggleRow(string rowName, string label, float anchorYMin, float anchorYMax)
            {
                var rowGO = GetOrCreateChildGO(panelGO, rowName);
                var rowRT = rowGO.GetComponent<RectTransform>();
                rowRT.anchorMin = new Vector2(0.08f, anchorYMin);
                rowRT.anchorMax = new Vector2(0.92f, anchorYMax);
                rowRT.offsetMin = rowRT.offsetMax = Vector2.zero;

                var toggle = GetOrAdd<Toggle>(rowGO);

                var lblGO  = GetOrCreateChildGO(rowGO, "Label");
                var lbl    = GetOrAdd<TextMeshProUGUI>(lblGO);
                lbl.text      = label;
                lbl.fontSize  = 14f;
                lbl.color     = Color.white;
                lbl.alignment = TextAlignmentOptions.MidlineLeft;
                var lblRT = lblGO.GetComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero;
                lblRT.anchorMax = new Vector2(0.7f, 1f);
                lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;

                var bgGO2  = GetOrCreateChildGO(rowGO, "Background");
                var bgImg2 = GetOrAdd<Image>(bgGO2);
                bgImg2.color = new Color(0.15f, 0.1f, 0.25f);
                var bgRT2 = bgGO2.GetComponent<RectTransform>();
                bgRT2.anchorMin = new Vector2(0.75f, 0.1f);
                bgRT2.anchorMax = new Vector2(0.9f, 0.9f);
                bgRT2.offsetMin = bgRT2.offsetMax = Vector2.zero;
                toggle.targetGraphic = bgImg2;

                var checkGO  = GetOrCreateChildGO(bgGO2, "Checkmark");
                var checkImg = GetOrAdd<Image>(checkGO);
                checkImg.color = new Color(0.5f, 0.35f, 0.9f);
                var checkRT = checkGO.GetComponent<RectTransform>();
                checkRT.anchorMin = new Vector2(0.1f, 0.1f);
                checkRT.anchorMax = new Vector2(0.9f, 0.9f);
                checkRT.offsetMin = checkRT.offsetMax = Vector2.zero;
                toggle.graphic = checkImg;

                return toggle;
            }

            var reducedMotionToggle = BuildToggleRow("ReducedMotionRow",  "Reduced Motion",  0.31f, 0.39f);
            var hapticsToggle       = BuildToggleRow("HapticsRow",        "Haptics",         0.22f, 0.30f);
            var highContrastToggle  = BuildToggleRow("HighContrastRow",   "High Contrast",   0.13f, 0.21f);

            // ── Close button ───────────────────────────────────────────────
            var closeGO = GetOrCreateChildGO(panelGO, "CloseButton");
            GetOrAdd<Image>(closeGO).color = new Color(0.4f, 0.25f, 0.65f);
            GetOrAdd<Button>(closeGO);
            var closeRT = closeGO.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.3f, 0.04f);
            closeRT.anchorMax = new Vector2(0.7f, 0.12f);
            closeRT.offsetMin = closeRT.offsetMax = Vector2.zero;
            var closeLblGO = GetOrCreateChildGO(closeGO, "Label");
            var closeLbl = GetOrAdd<TextMeshProUGUI>(closeLblGO);
            closeLbl.text = "Close";
            closeLbl.fontSize = 18f;
            closeLbl.alignment = TextAlignmentOptions.Center;
            closeLbl.color = Color.white;
            var closeLblRT = closeLblGO.GetComponent<RectTransform>();
            closeLblRT.anchorMin = Vector2.zero;
            closeLblRT.anchorMax = Vector2.one;
            closeLblRT.offsetMin = closeLblRT.offsetMax = Vector2.zero;

            // ── Wire SettingsPanel component ───────────────────────────────
            panelGO.SetActive(false);
            var comp = GetOrAdd<AscendantContinuum.UI.SettingsPanel>(panelGO);
            var so = new SerializedObject(comp);
            so.FindProperty("panelRoot").objectReferenceValue         = panelGO;
            so.FindProperty("panelGroup").objectReferenceValue        = GetOrAdd<CanvasGroup>(panelGO);
            so.FindProperty("masterVolumeSlider").objectReferenceValue  = masterSlider;
            so.FindProperty("musicVolumeSlider").objectReferenceValue   = musicSlider;
            so.FindProperty("sfxVolumeSlider").objectReferenceValue     = sfxSlider;
            so.FindProperty("ambientVolumeSlider").objectReferenceValue = ambientSlider;
            so.FindProperty("masterValueLabel").objectReferenceValue  = masterLbl;
            so.FindProperty("musicValueLabel").objectReferenceValue   = musicLbl;
            so.FindProperty("sfxValueLabel").objectReferenceValue     = sfxLbl;
            so.FindProperty("ambientValueLabel").objectReferenceValue = ambLbl;
            so.FindProperty("reducedMotionToggle").objectReferenceValue = reducedMotionToggle;
            so.FindProperty("hapticsToggle").objectReferenceValue       = hapticsToggle;
            so.FindProperty("highContrastToggle").objectReferenceValue  = highContrastToggle;
            so.FindProperty("closeButton").objectReferenceValue         = closeGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            Debug.Log($"[RealmPrefabBuilder] ✅ SettingsPanel built for {scene.name}");
            return comp;
        }

        /// <summary>
        /// Builds/wires a PauseMenu overlay inside HUDCanvas and adds a "⏸" pause
        /// button to the top bar. Wires PauseMenuController and links SettingsPanel.
        /// </summary>
        private static void EnsurePauseMenu(
            UnityEngine.SceneManagement.Scene scene,
            AscendantContinuum.UI.SettingsPanel settingsPanel)
        {
            var hudCanvas = FindGoInScene(scene, "HUDCanvas");
            if (hudCanvas == null) return;

            // ── Pause overlay ──────────────────────────────────────────────
            var pauseGO = hudCanvas.transform.Find("PausePanel")?.gameObject;
            if (pauseGO == null)
            {
                pauseGO = new GameObject("PausePanel");
                pauseGO.transform.SetParent(hudCanvas.transform, false);
            }

            var dimImg = GetOrAdd<Image>(pauseGO);
            dimImg.color = new Color(0f, 0f, 0f, 0.85f);
            var pauseRT = pauseGO.GetComponent<RectTransform>();
            pauseRT.anchorMin = Vector2.zero;
            pauseRT.anchorMax = Vector2.one;
            pauseRT.offsetMin = pauseRT.offsetMax = Vector2.zero;

            // Title
            var pauseTitleGO = GetOrCreateChildGO(pauseGO, "PauseTitle");
            var pauseTitleTMP = GetOrAdd<TextMeshProUGUI>(pauseTitleGO);
            pauseTitleTMP.text      = "Paused";
            pauseTitleTMP.fontSize  = 30f;
            pauseTitleTMP.fontStyle = FontStyles.Bold;
            pauseTitleTMP.alignment = TextAlignmentOptions.Center;
            pauseTitleTMP.color     = new Color(0.9f, 0.85f, 1f);
            var pauseTitleRT = pauseTitleGO.GetComponent<RectTransform>();
            pauseTitleRT.anchorMin = new Vector2(0.15f, 0.62f);
            pauseTitleRT.anchorMax = new Vector2(0.85f, 0.72f);
            pauseTitleRT.offsetMin = pauseTitleRT.offsetMax = Vector2.zero;

            // Resume button
            var resumeGO = GetOrCreateChildGO(pauseGO, "ResumeButton");
            GetOrAdd<Image>(resumeGO).color = new Color(0.3f, 0.65f, 0.4f);
            GetOrAdd<Button>(resumeGO);
            var resumeRT = resumeGO.GetComponent<RectTransform>();
            resumeRT.anchorMin = new Vector2(0.2f, 0.48f);
            resumeRT.anchorMax = new Vector2(0.8f, 0.60f);
            resumeRT.offsetMin = resumeRT.offsetMax = Vector2.zero;
            var resumeLblGO = GetOrCreateChildGO(resumeGO, "Label");
            var resumeLbl = GetOrAdd<TextMeshProUGUI>(resumeLblGO);
            resumeLbl.text = "▶  Resume";
            resumeLbl.fontSize = 20f;
            resumeLbl.alignment = TextAlignmentOptions.Center;
            resumeLbl.color = Color.white;
            var resumeLblRT = resumeLblGO.GetComponent<RectTransform>();
            resumeLblRT.anchorMin = Vector2.zero;
            resumeLblRT.anchorMax = Vector2.one;
            resumeLblRT.offsetMin = resumeLblRT.offsetMax = Vector2.zero;

            // Settings button
            var settingsBtnGO = GetOrCreateChildGO(pauseGO, "SettingsButton");
            GetOrAdd<Image>(settingsBtnGO).color = new Color(0.3f, 0.3f, 0.6f);
            GetOrAdd<Button>(settingsBtnGO);
            var settingsBtnRT = settingsBtnGO.GetComponent<RectTransform>();
            settingsBtnRT.anchorMin = new Vector2(0.2f, 0.34f);
            settingsBtnRT.anchorMax = new Vector2(0.8f, 0.46f);
            settingsBtnRT.offsetMin = settingsBtnRT.offsetMax = Vector2.zero;
            var settingsLblGO = GetOrCreateChildGO(settingsBtnGO, "Label");
            var settingsLbl = GetOrAdd<TextMeshProUGUI>(settingsLblGO);
            settingsLbl.text = "⚙  Settings";
            settingsLbl.fontSize = 20f;
            settingsLbl.alignment = TextAlignmentOptions.Center;
            settingsLbl.color = Color.white;
            var settingsLblRT = settingsLblGO.GetComponent<RectTransform>();
            settingsLblRT.anchorMin = Vector2.zero;
            settingsLblRT.anchorMax = Vector2.one;
            settingsLblRT.offsetMin = settingsLblRT.offsetMax = Vector2.zero;

            // Main Menu button
            var menuBtnGO = GetOrCreateChildGO(pauseGO, "MainMenuButton");
            GetOrAdd<Image>(menuBtnGO).color = new Color(0.5f, 0.25f, 0.25f);
            GetOrAdd<Button>(menuBtnGO);
            var menuBtnRT = menuBtnGO.GetComponent<RectTransform>();
            menuBtnRT.anchorMin = new Vector2(0.2f, 0.20f);
            menuBtnRT.anchorMax = new Vector2(0.8f, 0.32f);
            menuBtnRT.offsetMin = menuBtnRT.offsetMax = Vector2.zero;
            var menuLblGO = GetOrCreateChildGO(menuBtnGO, "Label");
            var menuLbl = GetOrAdd<TextMeshProUGUI>(menuLblGO);
            menuLbl.text = "↩  Main Menu";
            menuLbl.fontSize = 20f;
            menuLbl.alignment = TextAlignmentOptions.Center;
            menuLbl.color = Color.white;
            var menuLblRT = menuLblGO.GetComponent<RectTransform>();
            menuLblRT.anchorMin = Vector2.zero;
            menuLblRT.anchorMax = Vector2.one;
            menuLblRT.offsetMin = menuLblRT.offsetMax = Vector2.zero;

            // ── Pause button in the HUD top bar ────────────────────────────
            var topBar = hudCanvas.transform.Find("TopBar")?.gameObject;
            if (topBar != null)
            {
                var pauseBtnGO = GetOrCreateChildGO(topBar, "PauseButton");
                GetOrAdd<Image>(pauseBtnGO).color = new Color(0.2f, 0.15f, 0.35f, 0.8f);
                GetOrAdd<Button>(pauseBtnGO);
                var pauseBtnRT = pauseBtnGO.GetComponent<RectTransform>();
                pauseBtnRT.anchorMin = new Vector2(0f, 0f);
                pauseBtnRT.anchorMax = new Vector2(0f, 1f);
                pauseBtnRT.pivot     = new Vector2(0f, 0.5f);
                pauseBtnRT.anchoredPosition = new Vector2(2f, 0f);
                pauseBtnRT.sizeDelta = new Vector2(44f, 0f);
                var pauseIconGO = GetOrCreateChildGO(pauseBtnGO, "Icon");
                var pauseIconTMP = GetOrAdd<TextMeshProUGUI>(pauseIconGO);
                pauseIconTMP.text = "⏸";
                pauseIconTMP.fontSize = 22f;
                pauseIconTMP.alignment = TextAlignmentOptions.Center;
                pauseIconTMP.color = new Color(0.9f, 0.85f, 1f);
                var pauseIconRT = pauseIconGO.GetComponent<RectTransform>();
                pauseIconRT.anchorMin = Vector2.zero;
                pauseIconRT.anchorMax = Vector2.one;
                pauseIconRT.offsetMin = pauseIconRT.offsetMax = Vector2.zero;

                // Wire PauseMenuController on the pauseButton so it calls TogglePause
                var pauseMenuCtrl = GetOrAdd<AscendantContinuum.UI.PauseMenuController>(hudCanvas);
                var pso = new SerializedObject(pauseMenuCtrl);
                pso.FindProperty("panelRoot").objectReferenceValue    = pauseGO;
                pso.FindProperty("panelGroup").objectReferenceValue   = GetOrAdd<CanvasGroup>(pauseGO);
                pso.FindProperty("resumeButton").objectReferenceValue = resumeGO.GetComponent<Button>();
                pso.FindProperty("settingsButton").objectReferenceValue = settingsBtnGO.GetComponent<Button>();
                pso.FindProperty("mainMenuButton").objectReferenceValue = menuBtnGO.GetComponent<Button>();
                if (settingsPanel != null)
                    pso.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
                pso.ApplyModifiedProperties();

                // Wire the pause button click to TogglePause via a tiny helper
                var pauseBtnComp = pauseBtnGO.GetComponent<Button>();
                var wirer = GetOrAdd<PauseButtonWirer>(pauseBtnGO);
                var wso = new SerializedObject(wirer);
                wso.FindProperty("controller").objectReferenceValue = pauseMenuCtrl;
                wso.ApplyModifiedProperties();
            }

            pauseGO.SetActive(false);
            GetOrAdd<CanvasGroup>(pauseGO);
            Debug.Log($"[RealmPrefabBuilder] ✅ PauseMenu built for {scene.name}");
        }

        private static void EnsureCompletionPanel(
            UnityEngine.SceneManagement.Scene scene,
            MonoBehaviour target)
        {
            var hudCanvas = FindGoInScene(scene, "HUDCanvas");
            if (hudCanvas == null) return;

            // Create or reuse panel root
            var panelGO = hudCanvas.transform.Find("CompletionPanel")?.gameObject;
            if (panelGO == null)
            {
                panelGO = new GameObject("CompletionPanel");
                panelGO.transform.SetParent(hudCanvas.transform, false);
                panelGO.AddComponent<CanvasGroup>();
            }

            // Full-screen dimmer
            var bg = GetOrAdd<Image>(panelGO);
            bg.color = new Color(0f, 0f, 0f, 0.82f);
            var bgRT = panelGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

            // ── Title ──────────────────────────────────────────────────────
            var titleGO = GetOrCreateChildGO(panelGO, "TitleText");
            var titleTMP = GetOrAdd<TextMeshProUGUI>(titleGO);
            titleTMP.text      = "Realm Complete!";
            titleTMP.fontSize  = 32f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color     = new Color(1f, 0.92f, 0.4f);
            var titleRT = titleGO.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.65f);
            titleRT.anchorMax = new Vector2(0.9f, 0.80f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

            // ── Score text ────────────────────────────────────────────────
            var scoreGO = GetOrCreateChildGO(panelGO, "ScoreText");
            var scoreTMP = GetOrAdd<TextMeshProUGUI>(scoreGO);
            scoreTMP.text      = "";
            scoreTMP.fontSize  = 22f;
            scoreTMP.alignment = TextAlignmentOptions.Center;
            scoreTMP.color     = Color.white;
            var scoreRT = scoreGO.GetComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(0.1f, 0.55f);
            scoreRT.anchorMax = new Vector2(0.9f, 0.65f);
            scoreRT.offsetMin = scoreRT.offsetMax = Vector2.zero;

            // ── Subtitle ──────────────────────────────────────────────────
            var subGO = GetOrCreateChildGO(panelGO, "SubtitleText");
            var subTMP = GetOrAdd<TextMeshProUGUI>(subGO);
            subTMP.text      = "✦  Ascendant Continuum  ✦";
            subTMP.fontSize  = 14f;
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.color     = new Color(0.7f, 0.65f, 1f);
            var subRT = subGO.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.1f, 0.48f);
            subRT.anchorMax = new Vector2(0.9f, 0.54f);
            subRT.offsetMin = subRT.offsetMax = Vector2.zero;

            // ── Stars row ─────────────────────────────────────────────────
            var starsParent = GetOrCreateChildGO(panelGO, "StarsRow");
            var starsRT = starsParent.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(0.3f, 0.34f);
            starsRT.anchorMax = new Vector2(0.7f, 0.47f);
            starsRT.offsetMin = starsRT.offsetMax = Vector2.zero;
            var hLayout = GetOrAdd<HorizontalLayoutGroup>(starsParent);
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.spacing = 16f;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = false;

            var starImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var starGO = GetOrCreateChildGO(starsParent, $"Star_{i + 1}");
                var starImg = GetOrAdd<Image>(starGO);
                starImg.color = new Color(1f, 0.88f, 0.2f, 0f); // hidden until revealed
                var cle = GetOrAdd<LayoutElement>(starGO);
                cle.preferredWidth  = 60f;
                cle.preferredHeight = 60f;
                starImages[i] = starImg;
            }

            // ── Play Again button ─────────────────────────────────────────
            var playAgainGO = GetOrCreateChildGO(panelGO, "PlayAgainButton");
            GetOrAdd<Image>(playAgainGO).color = new Color(0.3f, 0.7f, 0.4f);
            GetOrAdd<Button>(playAgainGO);
            var paRT = playAgainGO.GetComponent<RectTransform>();
            paRT.anchorMin = new Vector2(0.12f, 0.14f);
            paRT.anchorMax = new Vector2(0.46f, 0.28f);
            paRT.offsetMin = paRT.offsetMax = Vector2.zero;
            var paLblGO = GetOrCreateChildGO(playAgainGO, "Label");
            var paLbl = GetOrAdd<TextMeshProUGUI>(paLblGO);
            paLbl.text = "Play Again";
            paLbl.fontSize = 18f;
            paLbl.alignment = TextAlignmentOptions.Center;
            paLbl.color = Color.white;
            var paLblRT = paLblGO.GetComponent<RectTransform>();
            paLblRT.anchorMin = Vector2.zero;
            paLblRT.anchorMax = Vector2.one;
            paLblRT.offsetMin = paLblRT.offsetMax = Vector2.zero;

            // ── Return to Menu button ─────────────────────────────────────
            var returnGO = GetOrCreateChildGO(panelGO, "ReturnButton");
            GetOrAdd<Image>(returnGO).color = new Color(0.5f, 0.3f, 0.7f);
            GetOrAdd<Button>(returnGO);
            var rnRT = returnGO.GetComponent<RectTransform>();
            rnRT.anchorMin = new Vector2(0.54f, 0.14f);
            rnRT.anchorMax = new Vector2(0.88f, 0.28f);
            rnRT.offsetMin = rnRT.offsetMax = Vector2.zero;
            var rnLblGO = GetOrCreateChildGO(returnGO, "Label");
            var rnLbl = GetOrAdd<TextMeshProUGUI>(rnLblGO);
            rnLbl.text = "Return to Menu";
            rnLbl.fontSize = 18f;
            rnLbl.alignment = TextAlignmentOptions.Center;
            rnLbl.color = Color.white;
            var rnLblRT = rnLblGO.GetComponent<RectTransform>();
            rnLblRT.anchorMin = Vector2.zero;
            rnLblRT.anchorMax = Vector2.one;
            rnLblRT.offsetMin = rnLblRT.offsetMax = Vector2.zero;

            // ── Wire the RealmCompletionPanel component ───────────────────
            panelGO.SetActive(false); // hidden at start
            var comp = GetOrAdd<AscendantContinuum.UI.RealmCompletionPanel>(panelGO);
            var so = new SerializedObject(comp);
            so.FindProperty("panelGroup").objectReferenceValue    = panelGO.GetComponent<CanvasGroup>();
            so.FindProperty("titleText").objectReferenceValue     = titleTMP;
            so.FindProperty("scoreText").objectReferenceValue     = scoreTMP;
            so.FindProperty("subtitleText").objectReferenceValue  = subTMP;
            so.FindProperty("playAgainButton").objectReferenceValue = playAgainGO.GetComponent<Button>();
            so.FindProperty("returnButton").objectReferenceValue   = returnGO.GetComponent<Button>();
            // Wire star images array
            var starsProp = so.FindProperty("starImages");
            starsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                starsProp.GetArrayElementAtIndex(i).objectReferenceValue = starImages[i];
            so.ApplyModifiedProperties();

            // ── Wire completionPanel field on the gameplay script ─────────
            if (target != null)
            {
                var targetSO = new SerializedObject(target);
                var cpProp = targetSO.FindProperty("completionPanel");
                if (cpProp != null)
                {
                    cpProp.objectReferenceValue = comp;
                    targetSO.ApplyModifiedProperties();
                }
            }

            Debug.Log($"[RealmPrefabBuilder] ✅ CompletionPanel wired for {scene.name}");
        }

        /// <summary>
        /// Assigns a background image sprite to the scene's [ Background ] SpriteRenderer
        /// and stretches it to fill the camera viewport exactly (ortho size 6 = 12 units tall).
        /// Uses independent X/Y scale so the full image is always visible regardless of
        /// the image's source aspect ratio.
        /// </summary>
        private static void SetRealmBackgroundImage(
            UnityEngine.SceneManagement.Scene scene, string texturePath)
        {
            EnsureTextureIsSprite(texturePath);

            // Re-load after potential reimport
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
            if (sprite == null)
            {
                Debug.LogWarning($"[RealmPrefabBuilder] Background texture not found: {texturePath}");
                return;
            }

            var bgGO = FindGoInScene(scene, "[ Background ]");
            if (bgGO == null)
            {
                bgGO = new GameObject("[ Background ]");
                bgGO.transform.position = new Vector3(0f, 0f, 10f);
                Undo.RegisterCreatedObjectUndo(bgGO, "Create Background");
            }

            // Always centre the background
            bgGO.transform.position = new Vector3(0f, 0f, 10f);

            var sr = GetOrAdd<SpriteRenderer>(bgGO);
            sr.sprite       = sprite;
            sr.color        = Color.white;
            sr.sortingOrder = -100;
            sr.drawMode     = SpriteDrawMode.Simple;

            var alive = GetOrAdd<AscendantContinuum.UI.BackgroundAliveMotion>(bgGO);
            var parallax = GetOrAdd<AscendantContinuum.UI.ParallaxBackground>(bgGO);
            
            // Ensure AmbientParticles child exists for floating particles
            var ambientChild = FindGoInScene(scene, "[ Ambient Particles ]");
            if (ambientChild == null)
            {
                ambientChild = new GameObject("[ Ambient Particles ]");
                ambientChild.transform.SetParent(bgGO.transform, false);
                Undo.RegisterCreatedObjectUndo(ambientChild, "Create AmbientParticles");
            }
            var ambientComp = GetOrAdd<AscendantContinuum.UI.AmbientParticles>(ambientChild);
            ConfigureAmbientParticlesForRealm(scene.name, ambientComp);

            // Stretch-to-fill: scale X and Y independently so the ENTIRE image fills
            // the camera viewport with no cropping, regardless of image aspect ratio.
            Camera cam = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                cam = root.GetComponentInChildren<Camera>(true);
                if (cam != null) break;
            }

            float viewH = cam != null ? cam.orthographicSize * 2f : 12f;
            float viewW = cam != null ? viewH * cam.aspect : (12f * 9f / 16f);

            float ppu     = sprite.pixelsPerUnit > 0f ? sprite.pixelsPerUnit : 100f;
            float spriteH = sprite.rect.height / ppu;
            float spriteW = sprite.rect.width  / ppu;

            if (spriteH > 0f && spriteW > 0f)
            {
                // Independent scale per axis: image fills exactly the viewport (stretch-to-fill)
                float scaleX = viewW / spriteW;
                float scaleY = viewH / spriteH;
                bgGO.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            Debug.Log($"[RealmPrefabBuilder] ✅ Background image set for {scene.name} " +
                      $"(sprite {sprite.rect.width}x{sprite.rect.height} @ {sprite.pixelsPerUnit}ppu)");
        }

        private static void EnsureTextureIsSprite(string texturePath)
        {
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) return;

            bool needsReimport = importer.textureType != TextureImporterType.Sprite;
            importer.textureType      = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            // Keep default PPU (100) — scale is handled in world space
            importer.filterMode       = FilterMode.Bilinear;

            if (needsReimport)
                importer.SaveAndReimport();
        }

        private static void ConfigureAmbientParticlesForRealm(string sceneName, AscendantContinuum.UI.AmbientParticles ambient)
        {
            if (ambient == null) return;

            string lower = sceneName.ToLowerInvariant();

            GameObject p1;
            int c1;
            float d1;
            float v1;
            GameObject p2;
            int c2;
            float d2;
            float v2;

            if (lower.Contains("ember"))
            {
                p1 = CreateAmbientParticlePrefab("EmberSparkle", new Color(1f, 0.6f, 0.2f), 32);
                c1 = 6; d1 = 0.12f; v1 = 0.03f;
                p2 = CreateAmbientParticlePrefab("EmberGlow", new Color(1f, 0.4f, 0.1f, 0.6f), 28);
                c2 = 4; d2 = 0.08f; v2 = 0.02f;
            }
            else if (lower.Contains("verdant"))
            {
                p1 = CreateAmbientParticlePrefab("BotanicalPetal", new Color(0.3f, 0.9f, 0.4f), 30);
                c1 = 7; d1 = 0.06f; v1 = 0.015f;
                p2 = CreateAmbientParticlePrefab("LifeMote", new Color(0.6f, 1f, 0.7f, 0.7f), 24);
                c2 = 5; d2 = 0.04f; v2 = 0.01f;
            }
            else if (lower.Contains("echo"))
            {
                p1 = CreateAmbientParticlePrefab("EchoStar", new Color(0.7f, 0.85f, 1f), 32);
                c1 = 8; d1 = 0.05f; v1 = 0.012f;
                p2 = CreateAmbientParticlePrefab("CosmicDust", new Color(0.5f, 0.7f, 1f, 0.5f), 20);
                c2 = 5; d2 = 0.03f; v2 = 0.008f;
            }
            else if (lower.Contains("dawn"))
            {
                p1 = CreateAmbientParticlePrefab("DawnGlow", new Color(1f, 0.95f, 0.6f), 28);
                c1 = 6; d1 = 0.07f; v1 = 0.02f;
                p2 = CreateAmbientParticlePrefab("CelestialMote", new Color(1f, 0.8f, 0.4f, 0.6f), 24);
                c2 = 4; d2 = 0.05f; v2 = 0.015f;
            }
            else if (lower.Contains("lantern"))
            {
                p1 = CreateAmbientParticlePrefab("WisdomGlow", new Color(0.9f, 0.7f, 1f), 30);
                c1 = 5; d1 = 0.06f; v1 = 0.015f;
                p2 = CreateAmbientParticlePrefab("SpiritMist", new Color(0.8f, 0.5f, 1f, 0.5f), 26);
                c2 = 4; d2 = 0.04f; v2 = 0.01f;
            }
            else
            {
                p1 = CreateAmbientParticlePrefab("CosmicStar", new Color(0.8f, 0.9f, 1f), 32);
                c1 = 5; d1 = 0.05f; v1 = 0.012f;
                p2 = CreateAmbientParticlePrefab("CosmicGlow", new Color(0.7f, 0.6f, 1f, 0.6f), 28);
                c2 = 3; d2 = 0.03f; v2 = 0.008f;
            }

            var so = new SerializedObject(ambient);
            var types = so.FindProperty("particleTypes");
            if (types == null) return;

            types.arraySize = 2;
            SetParticleSettings(types.GetArrayElementAtIndex(0), p1, c1, d1, v1);
            SetParticleSettings(types.GetArrayElementAtIndex(1), p2, c2, d2, v2);
            so.ApplyModifiedProperties();
        }

        private static void SetParticleSettings(SerializedProperty element, GameObject prefab, int count, float driftSpeed, float speedVariance)
        {
            if (element == null) return;
            var prefabProp = element.FindPropertyRelative("particlePrefab");
            var countProp = element.FindPropertyRelative("count");
            var driftProp = element.FindPropertyRelative("driftSpeed");
            var varianceProp = element.FindPropertyRelative("speedVariance");

            if (prefabProp != null) prefabProp.objectReferenceValue = prefab;
            if (countProp != null) countProp.intValue = count;
            if (driftProp != null) driftProp.floatValue = driftSpeed;
            if (varianceProp != null) varianceProp.floatValue = speedVariance;
        }

        private static GameObject CreateAmbientParticlePrefab(string name, Color color, int size)
        {
            string prefabPath = $"Assets/_Project/Prefabs/VFX/AmbientParticles/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return existing;

            var sprite = GetOrCreateCircleSprite(name, color, size);
            if (sprite == null) return null;

            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 1;

            var prefabDir = Path.GetDirectoryName(prefabPath);
            if (!string.IsNullOrEmpty(prefabDir) && !Directory.Exists(prefabDir))
                Directory.CreateDirectory(prefabDir);

            var saved = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            UnityEngine.Object.DestroyImmediate(go);
            return saved;
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS — serialized field assignment
        // ════════════════════════════════════════════════════════════════════

        private static void SetPrefabField(MonoBehaviour target, string fieldName, GameObject prefab)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[RealmPrefabBuilder] Field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
        }

        private static void SetLineRendererPrefabField(
            MonoBehaviour target, string fieldName, GameObject lineRendererPrefab)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null) return;
            // lineRendererPrefab field is typed as LineRenderer
            var lr = lineRendererPrefab.GetComponent<LineRenderer>();
            prop.objectReferenceValue = lr;
            so.ApplyModifiedProperties();
        }

        private static void SetTransformField(
            SerializedObject so, string fieldName, Transform value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedProperties();
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS — texture / sprite
        // ════════════════════════════════════════════════════════════════════

        private static Sprite GetOrCreateCircleSprite(string name, Color color, int size)
        {
            string texPath = $"{TEX_ROOT}/{name}_sprite.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
            if (existing != null) return existing;

            var tex = GenerateCircleTexture(color, size);
            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            string fullPath = Path.GetFullPath(Path.Combine("Assets", "..",
                texPath.Replace("Assets/", "")));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.ImportAsset(texPath);

            // Set texture as Sprite
            var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType     = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = size;
                importer.filterMode      = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        }

        private static Texture2D GenerateCircleTexture(Color color, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) / 2f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx   = x - center;
                    float dy   = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    // Smooth edge
                    float alpha = Mathf.Clamp01((radius - dist + 1.5f) / 1.5f) * color.a;
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                }
            }
            tex.Apply();
            return tex;
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS — scene open / close / find
        // ════════════════════════════════════════════════════════════════════

        private static UnityEngine.SceneManagement.Scene OpenRealmScene(string sceneName)
        {
            string path = $"{SCENE_ROOT}/{sceneName}.unity";
            if (!File.Exists(path))
            {
                Debug.LogError($"[RealmPrefabBuilder] Scene not found: {path}");
                return default;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return default;
            return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        private static void SaveAndCloseScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (!scene.IsValid()) return;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[RealmPrefabBuilder] ✅ {scene.name} saved.");
        }

        private static T FindInScene<T>(UnityEngine.SceneManagement.Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var c = root.GetComponentInChildren<T>(true);
                if (c != null) return c;
            }
            return null;
        }

        /// <summary>
        /// Gets or creates a root-level GameObject in the given scene.
        /// </summary>
        private static GameObject GetOrCreateSceneRoot(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name) return root;

            var go = new GameObject(name);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            return go;
        }

        /// <summary>
        /// Creates a child GameObject with a specific Unity tag.
        /// Only creates if not already present under parent.
        /// </summary>
        private static GameObject CreateTaggedChild(GameObject parent, string childName, string tag)
        {
            var existing = parent.transform.Find(childName);
            if (existing != null) return existing.gameObject;

            var go = new GameObject(childName);
            go.transform.SetParent(parent.transform, false);
            try { go.tag = tag; }
            catch { Debug.LogWarning($"[RealmPrefabBuilder] Tag '{tag}' not defined in TagManager — add it in Project Settings."); }
            return go;
        }


        private static GameObject FindGoInScene(
            UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root;
                var found = FindChildDeep(root.transform, name);
                if (found != null) return found.gameObject;
            }
            return null;
        }

        private static Transform FindChildDeep(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name) return child;
                var found = FindChildDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject GetOrCreateChild(GameObject parent, string name)
        {
            var existing = parent.transform.Find(name);
            if (existing != null) return existing.gameObject;
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS — prefab save
        // ════════════════════════════════════════════════════════════════════

        private static GameObject SavePrefab(GameObject go, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string parent = Path.GetDirectoryName(dir).Replace('\\', '/');
                string child  = Path.GetFileName(dir);
                AssetDatabase.CreateFolder(parent, child);
            }

            bool success;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path, out success);
            UnityEngine.Object.DestroyImmediate(go);

            if (success)
                Debug.Log($"[RealmPrefabBuilder] Created prefab: {path}");
            else
                Debug.LogError($"[RealmPrefabBuilder] Failed to save prefab: {path}");

            return prefab;
        }

        // ────────────────────────────────────────────────────────────────────
        //  Directory setup
        // ────────────────────────────────────────────────────────────────────

        private static void EnsureDirectories()
        {
            EnsureFolder(TEX_ROOT);
            EnsureFolder($"{PREF_ROOT}/Emberforge");
            EnsureFolder($"{PREF_ROOT}/Verdant");
            EnsureFolder($"{PREF_ROOT}/EchoFields");
            EnsureFolder($"{PREF_ROOT}/DawnCitadel");
            EnsureFolder($"{PREF_ROOT}/LanternAscension");
            EnsureFolder($"{PREF_ROOT}/Shared");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
