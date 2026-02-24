using UnityEngine;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Accessibility
{
    /// <summary>
    /// Accessibility Secrets Manager — the feature that makes this game unique.
    ///
    /// Different accessibility modes reveal COMPLETELY DIFFERENT hidden content:
    ///   Protanopia      → "Crimson Veil" runes in Emberforge flames
    ///   Deuteranopia    → "Emerald Mysteries" lore fragments in Verdant
    ///   Tritanopia      → "Azure Pathways" in Lantern Ascension
    ///   Achromatopsia   → Edge-based sigil puzzles, monochrome secrets
    ///   Screen Reader   → NPC whisper audio secrets, audio-only rituals
    ///   Reduced Motion  → "Timeless Realm" frozen-frame secrets
    ///   Neurodivergent  → Mode-specific bonus content
    ///
    /// Objects are opted in by carrying the corresponding tag (set in Inspector):
    ///   Tag                          → Condition to reveal
    ///   "ProtanopiaSecret"           → Colorblind mode == Protanopia
    ///   "DeuteranopiaSecret"         → Colorblind mode == Deuteranopia
    ///   "TritanopiaSecret"           → Colorblind mode == Tritanopia
    ///   "AchromatopsiaSecret"        → Colorblind mode == Achromatopsia
    ///   "ScreenReaderSecret"         → ScreenReaderEnabled == true
    ///   "ReducedMotionSecret"        → ReducedMotionEnabled == true
    ///   "ADHDSecret"                 → NeurodivergentMode == ADHD
    ///   "AutismSecret"               → NeurodivergentMode == Autism
    ///   "DyslexiaSecret"             → NeurodivergentMode == Dyslexia
    ///
    /// The manager listens to AccessibilityManager events and rescans on
    /// every accessibility preference change.
    /// </summary>
    public sealed class AccessibilitySecretsManager : MonoBehaviour
    {
        public static AccessibilitySecretsManager Instance { get; private set; }

        // ── Tag catalogue ──────────────────────────────────────────────────
        // Maps each tag to the condition function that should reveal it.
        // Built at Start() to avoid GC allocations in Update.
        private readonly Dictionary<string, System.Func<bool>> _tagConditions
            = new Dictionary<string, System.Func<bool>>();

        // Keep track of the last known state to avoid redundant rescans
        private ColorblindMode    _lastColorblindMode    = (ColorblindMode)(-1);
        private bool              _lastScreenReader      = false;
        private bool              _lastReducedMotion     = false;
        private NeurodivergentMode _lastNeurodivergentMode = (NeurodivergentMode)(-1);

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            BuildTagConditions();
            // Initial scan after a frame so all scene objects are loaded
            Invoke(nameof(RescanAll), 0.1f);
        }

        private void Update()
        {
            // Poll for changes (AccessibilityManager doesn't expose change events yet)
            if (AccessibilityManager.Instance == null) return;

            bool changed = false;
            var am = AccessibilityManager.Instance;

            if (am.CurrentColorblindMode != _lastColorblindMode)
            {
                _lastColorblindMode = am.CurrentColorblindMode;
                changed = true;
            }
            if (am.ScreenReaderEnabled != _lastScreenReader)
            {
                _lastScreenReader = am.ScreenReaderEnabled;
                changed = true;
            }
            if (am.IsReducedMotionEnabled() != _lastReducedMotion)
            {
                _lastReducedMotion = am.IsReducedMotionEnabled();
                changed = true;
            }
            if (am.CurrentNeurodivergentMode != _lastNeurodivergentMode)
            {
                _lastNeurodivergentMode = am.CurrentNeurodivergentMode;
                changed = true;
            }

            if (changed)
                RescanAll();
        }

        // ── Core logic ─────────────────────────────────────────────────────

        private void BuildTagConditions()
        {
            _tagConditions["ProtanopiaSecret"]    = () => AccessibilityManager.Instance?.CurrentColorblindMode == ColorblindMode.Protanopia;
            _tagConditions["DeuteranopiaSecret"]  = () => AccessibilityManager.Instance?.CurrentColorblindMode == ColorblindMode.Deuteranopia;
            _tagConditions["TritanopiaSecret"]    = () => AccessibilityManager.Instance?.CurrentColorblindMode == ColorblindMode.Tritanopia;
            _tagConditions["AchromatopsiaSecret"] = () => AccessibilityManager.Instance?.CurrentColorblindMode == ColorblindMode.Achromatopsia;
            _tagConditions["ScreenReaderSecret"]  = () => AccessibilityManager.Instance?.ScreenReaderEnabled == true;
            _tagConditions["ReducedMotionSecret"] = () => AccessibilityManager.Instance?.IsReducedMotionEnabled() == true;
            _tagConditions["ADHDSecret"]          = () => AccessibilityManager.Instance?.CurrentNeurodivergentMode == NeurodivergentMode.ADHD;
            _tagConditions["AutismSecret"]        = () => AccessibilityManager.Instance?.CurrentNeurodivergentMode == NeurodivergentMode.Autism;
            _tagConditions["DyslexiaSecret"]      = () => AccessibilityManager.Instance?.CurrentNeurodivergentMode == NeurodivergentMode.Dyslexia;
        }

        /// <summary>
        /// Iterates every known accessibility secret tag and enables/disables
        /// all tagged objects in the scene accordingly.
        /// </summary>
        public void RescanAll()
        {
            foreach (var pair in _tagConditions)
            {
                string tag = pair.Key;
                bool   shouldReveal = pair.Value.Invoke();

                // FindGameObjectsWithTag throws if the tag doesn't exist in Tag Manager,
                // so guard with a try/catch to keep the game stable.
                try
                {
                    GameObject[] secrets = GameObject.FindGameObjectsWithTag(tag);
                    foreach (var go in secrets)
                    {
                        if (go.activeSelf != shouldReveal)
                        {
                            go.SetActive(shouldReveal);

                            if (shouldReveal)
                            {
                                OnSecretRevealed(tag, go);
                            }
                        }
                    }
                }
                catch (UnityException)
                {
                    // Tag not registered in this project yet — safe to skip
                }
            }
        }

        // ── Secret reveal callbacks ────────────────────────────────────────

        private void OnSecretRevealed(string tag, GameObject secret)
        {
            string modeName    = TagToModeName(tag);
            string secretLabel = secret.name;

            Debug.Log($"[AccessibilitySecrets] ✨ Revealed '{secretLabel}' via {modeName}");

            // Screen reader announcement
            string announcement = BuildAnnouncement(tag, secretLabel);
            AccessibilityManager.Instance?.Announce(announcement);

            // HUD notification
            UI.HUDManager.Instance?.ShowNotification(
                $"🔍 New secret found in {modeName} mode: {secretLabel}",
                UI.HUDManager.NotificationType.Achievement);

            // Haptic
            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Success);

            // Achievement — "colorblind_master" tracks unique colorblind secrets
            if (tag.Contains("Secret") && !tag.StartsWith("ScreenReader") && !tag.StartsWith("ReducedMotion"))
            {
                AchievementManager.Instance?.TrackProgress("colorblind_master", 1);
            }

            // Firebase
            Core.FirebaseManager.Instance?.TrackEvent("accessibility_secret_found",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { "tag",   tag },
                    { "secret", secretLabel },
                    { "mode",  modeName }
                });
        }

        // ── Screen-reader NPC whispers ─────────────────────────────────────

        /// <summary>
        /// Called by NPCs (Sparkus, Petalina, etc.) to deliver a secret whisper
        /// when the screen reader is active. The whisper text only plays via the
        /// accessibility announce channel — invisible to sighted players.
        /// </summary>
        public void TryWhisperNPCSecret(string npcName, string whisperText)
        {
            if (AccessibilityManager.Instance?.ScreenReaderEnabled != true) return;

            Debug.Log($"[AccessibilitySecrets] 🔊 NPC Whisper ({npcName}): {whisperText}");
            AccessibilityManager.Instance.Announce($"{npcName} whispers: {whisperText}");

            // Unlock the dedicated screen-reader achievement
            AchievementManager.Instance?.UnlockAchievement("screen_reader_storyteller");

            Core.FirebaseManager.Instance?.TrackEvent("npc_whisper_heard",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    { "npc",    npcName },
                    { "whisper", whisperText.Substring(0, System.Math.Min(50, whisperText.Length)) }
                });
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static string TagToModeName(string tag)
        {
            switch (tag)
            {
                case "ProtanopiaSecret":    return "Protanopia colorblind";
                case "DeuteranopiaSecret":  return "Deuteranopia colorblind";
                case "TritanopiaSecret":    return "Tritanopia colorblind";
                case "AchromatopsiaSecret": return "Achromatopsia colorblind";
                case "ScreenReaderSecret":  return "Screen Reader";
                case "ReducedMotionSecret": return "Reduced Motion";
                case "ADHDSecret":          return "ADHD";
                case "AutismSecret":        return "Autism";
                case "DyslexiaSecret":      return "Dyslexia";
                default:                    return tag;
            }
        }

        private static string BuildAnnouncement(string tag, string secretLabel)
        {
            switch (tag)
            {
                case "ProtanopiaSecret":
                    return $"A hidden rune appears in the Crimson Veil. {secretLabel} revealed through Protanopia vision.";
                case "DeuteranopiaSecret":
                    return $"Emerald Mystery uncovered: {secretLabel}. Visible only through Deuteranopia sight.";
                case "TritanopiaSecret":
                    return $"Azure Pathway found: {secretLabel}. Revealed by Tritanopia perception.";
                case "AchromatopsiaSecret":
                    return $"Ancient monochrome sigil discovered: {secretLabel}. The edge of all things is truth.";
                case "ScreenReaderSecret":
                    return $"Your screen reader reveals what eyes cannot see: {secretLabel}.";
                case "ReducedMotionSecret":
                    return $"In stillness, the Timeless Realm shows itself: {secretLabel}.";
                case "ADHDSecret":
                    return $"A rapid spark of discovery: {secretLabel} found through ADHD mode.";
                case "AutismSecret":
                    return $"In the predictable pattern, something unique: {secretLabel}.";
                case "DyslexiaSecret":
                    return $"Between the letters, a hidden truth: {secretLabel}.";
                default:
                    return $"Secret revealed: {secretLabel}.";
            }
        }
    }
}
