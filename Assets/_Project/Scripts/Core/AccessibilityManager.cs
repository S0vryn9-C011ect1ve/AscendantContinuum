using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using AscendantContinuum.VFX;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Manages ALL accessibility features - colorblind modes, screen reader support,
    /// reduced motion, haptics, neurodivergent modes, and text scaling.
    /// Accessibility IS gameplay.
    /// </summary>
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        [Header("Colorblind Modes")]
        [SerializeField] private ColorblindMode currentColorblindMode = ColorblindMode.None;

        [Header("Motion & Animation")]
        [SerializeField] private bool reducedMotionEnabled = false;
        [SerializeField] private float motionIntensity = 1f; // 0-1 scale

        [Header("Audio & Haptics")]
        [SerializeField] private bool hapticsEnabled = true;
        [SerializeField] private float hapticIntensity = 1f;
        [SerializeField] private bool screenReaderEnabled = false;

        [Header("Visual Settings")]
        [SerializeField] private float textScale = 1f; // 0.8 - 2.0
        [SerializeField] private bool highContrastMode = false;

        [Header("Neurodivergent Support")]
        [SerializeField] private NeurodivergentMode neurodivergentMode = NeurodivergentMode.None;

        public event Action<ColorblindMode> OnColorblindModeChanged;
        public event Action<bool> OnReducedMotionChanged;
        public event Action<bool> OnHapticsChanged;
        public event Action<NeurodivergentMode> OnNeurodivergentModeChanged;
        public event Action<bool> OnScreenReaderChanged;
        public event Action<float> OnTextScaleChanged;

        private Dictionary<ColorblindMode, Color> hiddenContentColors = new Dictionary<ColorblindMode, Color>();

        // Public properties
        public ColorblindMode CurrentColorblindMode => currentColorblindMode;
        public bool ReducedMotionEnabled => reducedMotionEnabled;
        public bool HapticsEnabled => hapticsEnabled;
        public float TextScale => textScale;
        public bool HighContrastMode => highContrastMode;
        public bool ScreenReaderEnabled => screenReaderEnabled;
        public NeurodivergentMode CurrentNeurodivergentMode => neurodivergentMode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Build hidden content color map
            hiddenContentColors[ColorblindMode.Protanopia] = new Color(1f, 0.4f, 0.4f);
            hiddenContentColors[ColorblindMode.Deuteranopia] = new Color(0.4f, 1f, 0.4f);
            hiddenContentColors[ColorblindMode.Tritanopia] = new Color(0.4f, 0.4f, 1f);
            hiddenContentColors[ColorblindMode.Achromatopsia] = new Color(0.7f, 0.7f, 0.7f);
            hiddenContentColors[ColorblindMode.Protanomaly] = new Color(1f, 0.6f, 0.5f);
        }

        private void Start()
        {
            LoadSettings();
        }

        // ── Persistence ────────────────────────────────────────────────────────

        public void LoadSettings()
        {
            currentColorblindMode = (ColorblindMode)PlayerPrefs.GetInt("Accessibility_ColorblindMode", 0);
            reducedMotionEnabled = PlayerPrefs.GetInt("Accessibility_ReducedMotion", 0) == 1;
            hapticsEnabled = PlayerPrefs.GetInt("Accessibility_Haptics", 1) == 1;
            textScale = PlayerPrefs.GetFloat("Accessibility_TextScale", 1f);
            highContrastMode = PlayerPrefs.GetInt("Accessibility_HighContrast", 0) == 1;
            screenReaderEnabled = PlayerPrefs.GetInt("Accessibility_ScreenReader", 0) == 1;
            neurodivergentMode = (NeurodivergentMode)PlayerPrefs.GetInt("Accessibility_NeurodivergentMode", 0);

            ApplySettings();
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetInt("Accessibility_ColorblindMode", (int)currentColorblindMode);
            PlayerPrefs.SetInt("Accessibility_ReducedMotion", reducedMotionEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Accessibility_Haptics", hapticsEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("Accessibility_TextScale", textScale);
            PlayerPrefs.SetInt("Accessibility_HighContrast", highContrastMode ? 1 : 0);
            PlayerPrefs.SetInt("Accessibility_ScreenReader", screenReaderEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Accessibility_NeurodivergentMode", (int)neurodivergentMode);
            PlayerPrefs.Save();
        }

        // ── Apply ──────────────────────────────────────────────────────────────

        private void ApplySettings()
        {
            ApplyColorblind();
            ApplyMotion();
            ApplyHighContrast();
            ApplyTextScale();
            ApplyNeurodivergentMode();

            Debug.Log($"[AccessibilityManager] Applied — Colorblind:{currentColorblindMode} Motion:{reducedMotionEnabled} ND:{neurodivergentMode}");
        }

        private void ApplyColorblind()
        {
            Shader.SetGlobalInt("_ColorblindMode", (int)currentColorblindMode);
            Shader.SetGlobalFloat("_HighContrastEnabled", highContrastMode ? 1f : 0f);
        }

        private void ApplyHighContrast()
        {
            Shader.SetGlobalFloat("_HighContrastEnabled", highContrastMode ? 1f : 0f);
        }

        private void ApplyMotion()
        {
            float intensity = reducedMotionEnabled ? 0.3f : motionIntensity;
            Shader.SetGlobalFloat("_MotionIntensity", intensity);

            var pm = FindFirstObjectByType<ParticleManager>();
            pm?.SetMotionScale(intensity);
        }

        private void ApplyTextScale()
        {
            var scalers = FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
            foreach (var scaler in scalers)
            {
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
                    scaler.scaleFactor = textScale;
                else
                    scaler.referenceResolution = new Vector2(1920f / textScale, 1080f / textScale);
            }
        }

        private void ApplyNeurodivergentMode()
        {
            // Reset defaults
            PlayerPrefs.SetFloat("ADHD_SpawnMultiplier", 1f);
            PlayerPrefs.SetInt("Autism_ReduceSurprises", 0);
            PlayerPrefs.SetInt("Dyslexia_WideSpacing", 0);
            float particleBoost = 1f;

            switch (neurodivergentMode)
            {
                case NeurodivergentMode.ADHD:
                    float adhdMult = PlayerPrefs.GetFloat("ADHD_SpawnMultiplier_Custom", 1.5f);
                    PlayerPrefs.SetFloat("ADHD_SpawnMultiplier", adhdMult);
                    PlayerPrefs.SetInt("ADHD_SessionReminderMinutes", 20);
                    particleBoost = 1.5f;
                    break;

                case NeurodivergentMode.Autism:
                    PlayerPrefs.SetInt("Autism_ReduceSurprises", 1);
                    particleBoost = 0.5f;
                    break;

                case NeurodivergentMode.Dyslexia:
                    PlayerPrefs.SetInt("Dyslexia_WideSpacing", 1);
                    ApplyDyslexiaSpacing(true);
                    break;

                default: // None
                    ApplyDyslexiaSpacing(false);
                    break;
            }

            Shader.SetGlobalFloat("_ParticleBoost", particleBoost);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Applies or removes wide character/word spacing for dyslexia support.
        /// Wide spacing = 20 units character spacing, 10 units word spacing (TMPro units).
        /// Normal spacing = 0 (TMPro default).
        /// </summary>
        private void ApplyDyslexiaSpacing(bool enable)
        {
            var allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
            float charSpacing = enable ? 20f : 0f;
            float wordSpacing = enable ? 10f : 0f;
            float lineSpacing = enable ? 5f : 0f;

            foreach (var text in allTexts)
            {
                text.characterSpacing = charSpacing;
                text.wordSpacing = wordSpacing;
                text.lineSpacing = lineSpacing;
            }

            Debug.Log($"[Accessibility] Dyslexia spacing {(enable ? "enabled" : "disabled")} on {allTexts.Length} text components.");
        }

        public void SetColorblindMode(ColorblindMode mode)
        {
            if (currentColorblindMode == mode) return;
            currentColorblindMode = mode;
            ApplyColorblind();
            SaveSettings();
            OnColorblindModeChanged?.Invoke(mode);
        }

        public void SetColorblindMode(string mode)
        {
            if (Enum.TryParse(mode, true, out ColorblindMode parsed))
                SetColorblindMode(parsed);
            else
                SetColorblindMode(ColorblindMode.None);
        }

        public void SetNeurodivergentMode(NeurodivergentMode mode)
        {
            if (neurodivergentMode == mode) return;
            neurodivergentMode = mode;
            ApplyNeurodivergentMode();
            SaveSettings();
            OnNeurodivergentModeChanged?.Invoke(mode);
        }

        public void SetNeurodivergentMode(string mode)
        {
            if (Enum.TryParse(mode, true, out NeurodivergentMode parsed))
                SetNeurodivergentMode(parsed);
            else
                SetNeurodivergentMode(NeurodivergentMode.None);
        }

        public void SetScreenReader(bool enabled)
        {
            if (screenReaderEnabled == enabled) return;
            screenReaderEnabled = enabled;
            SaveSettings();
            OnScreenReaderChanged?.Invoke(enabled);
        }

        public void SetReducedMotion(bool enabled)
        {
            reducedMotionEnabled = enabled;
            ApplyMotion();
            SaveSettings();
            OnReducedMotionChanged?.Invoke(enabled);
        }

        public void SetHaptics(bool enabled)
        {
            hapticsEnabled = enabled;
            SaveSettings();
            OnHapticsChanged?.Invoke(enabled);
        }

        public void SetHapticsEnabled(bool enabled) => SetHaptics(enabled);

        public void SetHapticIntensity(float intensity)
        {
            hapticIntensity = Mathf.Clamp01(intensity);
            SaveSettings();
        }

        public void SetTextScale(float scale)
        {
            textScale = Mathf.Clamp(scale, 0.8f, 2.0f);
            ApplyTextScale();
            SaveSettings();
            OnTextScaleChanged?.Invoke(textScale);
        }

        public void SetHighContrast(bool enabled)
        {
            highContrastMode = enabled;
            ApplyHighContrast();
            SaveSettings();
        }

        // ── Queries ────────────────────────────────────────────────────────────

        public float GetTextScale() => textScale;
        public bool IsHighContrastEnabled() => highContrastMode;
        public bool IsReducedMotionEnabled() => reducedMotionEnabled;

        /// <summary>Returns the tint color for hidden content unlocked by the current colorblind mode.</summary>
        public Color GetHiddenContentColor()
        {
            return hiddenContentColors.TryGetValue(currentColorblindMode, out Color c)
                ? c
                : Color.white;
        }

        /// <summary>Broadcasts a message to screen readers when screen reader mode is active.</summary>
        public void Announce(string message)
        {
            if (!screenReaderEnabled || string.IsNullOrEmpty(message)) return;
            Debug.Log($"[ScreenReader] {message}");
        }

        public void TriggerHaptic(HapticType type)
        {
            if (!hapticsEnabled) return;

#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }
    }

    public enum ColorblindMode
    {
        None = 0,
        Protanopia = 1,   // Red-blind
        Deuteranopia = 2,   // Green-blind
        Tritanopia = 3,   // Blue-blind
        Achromatopsia = 4,   // Monochrome
        Protanomaly = 5    // Weak red
    }

    public enum NeurodivergentMode
    {
        None = 0,
        ADHD = 1,
        Autism = 2,
        Dyslexia = 3
    }

    public enum HapticType
    {
        Selection,
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Error
    }
}
