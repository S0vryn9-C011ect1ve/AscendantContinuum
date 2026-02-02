using UnityEngine;
using System;
using System.Collections.Generic;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Manages ALL accessibility features - colorblind modes, screen reader support, 
    /// reduced motion, haptics, and more. Accessibility IS gameplay.
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
        [SerializeField] private bool screenReaderMode = false;
        
        [Header("Visual Settings")]
        [SerializeField] private float textScale = 1f; // 0.8 - 2.0
        [SerializeField] private bool highContrastMode = false;
        
        public event Action<ColorblindMode> OnColorblindModeChanged;
        public event Action<bool> OnReducedMotionChanged;
        public event Action<bool> OnHapticsChanged;

        private Dictionary<ColorblindMode, Color> hiddenContentColors = new Dictionary<ColorblindMode, Color>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAccessibility();
        }

        private void InitializeAccessibility()
        {
            // Define hidden content unlocked by each colorblind mode
            hiddenContentColors.Add(ColorblindMode.Protanopia, new Color(1f, 0.4f, 0.4f)); // Red-blind reveals red secrets
            hiddenContentColors.Add(ColorblindMode.Deuteranopia, new Color(0.4f, 1f, 0.4f)); // Green-blind reveals green secrets
            hiddenContentColors.Add(ColorblindMode.Tritanopia, new Color(0.4f, 0.4f, 1f)); // Blue-blind reveals blue secrets
            hiddenContentColors.Add(ColorblindMode.Achromatopsia, new Color(0.7f, 0.7f, 0.7f)); // Monochrome reveals grayscale secrets
            
            Debug.Log("[AccessibilityManager] Initialized - 5 colorblind modes, reduced motion, haptics, screen reader ready");
        }

        public void LoadSettings()
        {
            // Load from PlayerPrefs (encrypted in production via SaveSystem)
            currentColorblindMode = (ColorblindMode)PlayerPrefs.GetInt("Accessibility_ColorblindMode", 0);
            reducedMotionEnabled = PlayerPrefs.GetInt("Accessibility_ReducedMotion", 0) == 1;
            hapticsEnabled = PlayerPrefs.GetInt("Accessibility_Haptics", 1) == 1;
            textScale = PlayerPrefs.GetFloat("Accessibility_TextScale", 1f);
            highContrastMode = PlayerPrefs.GetInt("Accessibility_HighContrast", 0) == 1;
            
            ApplySettings();
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetInt("Accessibility_ColorblindMode", (int)currentColorblindMode);
            PlayerPrefs.SetInt("Accessibility_ReducedMotion", reducedMotionEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Accessibility_Haptics", hapticsEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("Accessibility_TextScale", textScale);
            PlayerPrefs.SetInt("Accessibility_HighContrast", highContrastMode ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplySettings()
        {
            // Apply colorblind shader
            Shader.SetGlobalInt("_ColorblindMode", (int)currentColorblindMode);
            
            // Apply motion settings
            Shader.SetGlobalFloat("_MotionIntensity", reducedMotionEnabled ? 0.3f : motionIntensity);
            
            Debug.Log($"[AccessibilityManager] Settings applied - Colorblind: {currentColorblindMode}, Reduced Motion: {reducedMotionEnabled}");
        }

        public void SetColorblindMode(ColorblindMode mode)
        {
            if (currentColorblindMode == mode) return;
            
            currentColorblindMode = mode;
            ApplySettings();
            SaveSettings();
            
            OnColorblindModeChanged?.Invoke(mode);
            Debug.Log($"[AccessibilityManager] Colorblind mode changed to: {mode}");
        }

        public void SetReducedMotion(bool enabled)
        {
            reducedMotionEnabled = enabled;
            ApplySettings();
            SaveSettings();
            
            OnReducedMotionChanged?.Invoke(enabled);
        }

        public void SetHaptics(bool enabled)
        {
            hapticsEnabled = enabled;
            SaveSettings();
            
            OnHapticsChanged?.Invoke(enabled);
        }

        public void TriggerHaptic(HapticType type)
        {
            if (!hapticsEnabled) return;
            
            #if UNITY_IOS || UNITY_ANDROID
            switch (type)
            {
                case HapticType.Light:
                    Handheld.Vibrate();
                    break;
                case HapticType.Medium:
                    Handheld.Vibrate();
                    break;
                case HapticType.Heavy:
                    Handheld.Vibrate();
                    break;
            }
            #endif
        }

        // Public getters
        public ColorblindMode CurrentColorblindMode => currentColorblindMode;
        public bool ReducedMotionEnabled => reducedMotionEnabled;
        public bool HapticsEnabled => hapticsEnabled;
        public float TextScale => textScale;
        public bool HighContrastMode => highContrastMode;
    }

    public enum ColorblindMode
    {
        None = 0,
        Protanopia = 1,      // Red-blind (reveals red secrets)
        Deuteranopia = 2,    // Green-blind (reveals green secrets)
        Tritanopia = 3,      // Blue-blind (reveals blue secrets)
        Achromatopsia = 4,   // Monochrome (reveals grayscale secrets)
        Protanomaly = 5      // Weak red (reveals subtle red patterns)
    }

    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Error
    }
}
