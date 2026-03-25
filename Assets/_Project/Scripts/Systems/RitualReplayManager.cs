using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;
using AscendantContinuum.Platform;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Ritual Replay System — Auto-generates shareable 6-second replay data for every
    /// ritual completion and surfaces a "share" prompt to the player.
    ///
    /// Because Unity's ScreenCapture API only produces static images on WebGL, the
    /// "replay" is encoded as a structured data packet (keyframes) that can feed a
    /// WebGL canvas animation. On mobile it delegates to NativeShare / social intents.
    ///
    /// Each ritual type produces a distinct emoji/sigil signature for the share card,
    /// matching the VIRAL_MECHANICS spec's 9:16 social-native format.
    /// </summary>
    public sealed class RitualReplayManager : MonoBehaviour
    {
        public static RitualReplayManager Instance { get; private set; }

        // ── PlayerPrefs ──────────────────────────────────────────────────
        private const string PREF_TOTAL_REPLAYS = "Replay_TotalGenerated";

        // ── Emoji signatures per realm ────────────────────────────────────
        private static readonly Dictionary<string, string[]> REALM_EMOJIS = new Dictionary<string, string[]>
        {
            { "Emberforge",   new[] { "🔥", "⚡", "✨", "🌋", "💫" } },
            { "Verdant",      new[] { "🌸", "🌿", "🍀", "🌺", "🌱" } },
            { "EchoFields",   new[] { "⭐", "🌟", "✨", "🌌", "💫" } },
            { "DawnCitadel",  new[] { "🌅", "💛", "🔆", "🌈", "✨" } },
            { "Lantern",      new[] { "🏮", "🕯️", "💙", "🌙", "✨" } },
        };

        // ── Replay frame record ───────────────────────────────────────────
        [Serializable]
        public class ReplayFrame
        {
            public float  timestamp;
            public string particleType;
            public Vector3 position;
            public Color  color;
            public float  scale;
        }

        [Serializable]
        public class RitualReplayData
        {
            public string        ritualName;
            public string        realmId;
            public List<ReplayFrame> frames = new List<ReplayFrame>();
            public float         totalDuration;
            public string        emojiSignature;
            public long          epochMs;
        }

        // ── State ─────────────────────────────────────────────────────────
        private RitualReplayData _activeCapture;
        private bool             _isCapturing;
        private float            _captureTimer;
        private const float      MAX_CAPTURE_DURATION = 6f;

        // ── Events ────────────────────────────────────────────────────────
        public event Action<RitualReplayData> OnReplayReady;

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            GameEvents.OnSigilDrawingCommitted += HandleSigilDrawingCommitted;
            GameEvents.OnSigilStrokePoint      += HandleSigilStrokePoint;
            GameEvents.OnSigilCompleted        += HandleSigilCompleted;
        }

        private void OnDestroy()
        {
            GameEvents.OnSigilDrawingCommitted -= HandleSigilDrawingCommitted;
            GameEvents.OnSigilStrokePoint      -= HandleSigilStrokePoint;
            GameEvents.OnSigilCompleted        -= HandleSigilCompleted;
        }

        private void Update()
        {
            if (!_isCapturing) return;
            _captureTimer += Time.deltaTime;
            if (_captureTimer >= MAX_CAPTURE_DURATION)
                EndCapture();
        }

        // ── GameEvent handlers ──────────────────────────────────────────────

        private void HandleSigilDrawingCommitted(AscendantContinuum.Systems.SigilAnalysisResult analysis)
        {
            string realmId = AscendantContinuum.Core.GameManager.Instance?.CurrentRealm ?? "Unknown";
            string ritualName = $"Sigil ({analysis.strokeCount} strokes, {analysis.totalArcLength:F1} len)";
            BeginCapture(ritualName, realmId);
        }

        private void HandleSigilStrokePoint(float velocity, float curvature)
        {
            if (!_isCapturing) return;
            // Map velocity to hue: slow (blue) → fast (red)
            Color col = Color.HSVToRGB(Mathf.Clamp01(1f - velocity / 5f), 0.9f, 1f);
            CaptureFrame("stroke", UnityEngine.Random.insideUnitSphere * 0.3f, col, velocity);
        }

        private void HandleSigilCompleted(AscendantContinuum.Data.SigilData data)
        {
            EndCapture();
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Call at the START of a ritual to begin the 6-second capture window.</summary>
        public void BeginCapture(string ritualName, string realmId)
        {
            if (_isCapturing) EndCapture();

            _activeCapture = new RitualReplayData
            {
                ritualName      = ritualName,
                realmId         = realmId,
                emojiSignature  = BuildEmojiSignature(realmId),
                epochMs         = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            _captureTimer = 0f;
            _isCapturing  = true;
            Debug.Log($"[RitualReplay] Capture started: {ritualName} / {realmId}");
        }

        /// <summary>Record a single visual keyframe during an active capture.</summary>
        public void CaptureFrame(string particleType, Vector3 position, Color color, float scale = 1f)
        {
            if (!_isCapturing || _activeCapture == null) return;
            _activeCapture.frames.Add(new ReplayFrame
            {
                timestamp    = _captureTimer,
                particleType = particleType,
                position     = position,
                color        = color,
                scale        = scale
            });
        }

        /// <summary>Call when the ritual visually completes to finalise the replay.</summary>
        public void EndCapture()
        {
            if (!_isCapturing || _activeCapture == null) return;
            _activeCapture.totalDuration = _captureTimer;
            _isCapturing = false;

            int total = PlayerPrefs.GetInt(PREF_TOTAL_REPLAYS, 0) + 1;
            PlayerPrefs.SetInt(PREF_TOTAL_REPLAYS, total);

            Debug.Log($"[RitualReplay] Captured {_activeCapture.frames.Count} frames in {_activeCapture.totalDuration:F1}s");
            OnReplayReady?.Invoke(_activeCapture);

            // Show share prompt after a short delay
            StartCoroutine(ShowSharePromptDelayed(_activeCapture));
        }

        /// <summary>Build and open the platform share dialog for a completed replay.</summary>
        public void ShareReplay(RitualReplayData replay)
        {
            string shareText = BuildShareCard(replay);
            string url       = "https://ascendantcontinuum.game";

#if UNITY_WEBGL && !UNITY_EDITOR
            Application.OpenURL($"https://twitter.com/intent/tweet?text={Uri.EscapeDataString(shareText + " " + url)}");
#elif UNITY_IOS || UNITY_ANDROID
            // NativeShare integration point — if package present, use it; otherwise fallback
            ShareViaSystem(shareText, url);
#else
            // Editor / fallback: copy to clipboard
            GUIUtility.systemCopyBuffer = shareText + " " + url;
            HUDManager.Instance?.ShowNotification("Replay share text copied to clipboard!", HUDManager.NotificationType.Info);
#endif
        }

        // ── Share card builder ─────────────────────────────────────────────

        public string BuildShareCard(RitualReplayData replay)
        {
            string sig = replay.emojiSignature;

            // Cosmic Patron bonus row — extra emoji line for patrons
            string patronRow = string.Empty;
            var patron = CosmicPatronManager.Instance;
            if (patron != null)
            {
                string bonusEmoji = patron.GetPatronShareBonus();
                if (!string.IsNullOrEmpty(bonusEmoji))
                    patronRow = $"\n{bonusEmoji}";
            }

            return
                $"✨ The Ascendant Continuum ✨\n" +
                $"{replay.ritualName} in {replay.realmId}\n\n" +
                $"{sig}{patronRow}\n\n" +
                $"Join the seekers → ascendantcontinuum.game\n" +
                $"#AscendantContinuum #CozyGame";
        }

        // ── Private ───────────────────────────────────────────────────────

        private IEnumerator ShowSharePromptDelayed(RitualReplayData replay)
        {
            yield return new WaitForSeconds(1.5f);
            string preview = BuildShareCard(replay);
            HUDManager.Instance?.ShowNotification(
                $"✨ Ritual Replay ready! Tap to share.\n\n{replay.emojiSignature}",
                HUDManager.NotificationType.Info);
        }

        private string BuildEmojiSignature(string realmId)
        {
            if (!REALM_EMOJIS.ContainsKey(realmId))
                realmId = "EchoFields";

            string[] pool = REALM_EMOJIS[realmId];
            string row1 = ""; string row2 = ""; string row3 = "";
            for (int i = 0; i < 4; i++) row1 += pool[UnityEngine.Random.Range(0, pool.Length)];
            for (int i = 0; i < 4; i++) row2 += pool[UnityEngine.Random.Range(0, pool.Length)];
            for (int i = 0; i < 4; i++) row3 += pool[UnityEngine.Random.Range(0, pool.Length)];
            return $"{row1}\n{row2}\n{row3}";
        }

        private void ShareViaSystem(string text, string url)
        {
            string fullText = text + "\n" + url;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android ACTION_SEND intent via URIBuilder scheme
            // Opens the system share-chooser without requiring a NativeShare plugin.
            string encoded = Uri.EscapeDataString(fullText);
            Application.OpenURL(
                $"intent:#Intent;action=android.intent.action.SEND;" +
                $"type=text%2Fplain;" +
                $"S.android.intent.extra.TEXT={encoded};" +
                $"S.android.intent.extra.SUBJECT=Ascendant%20Continuum;end");
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS doesn't support intent URIs. Copy to clipboard and prompt the player.
            // When the NativeShare package is imported (com.unity.mobile.notifications),
            // replace this block with: new NativeShare().SetText(fullText).Share();
            GUIUtility.systemCopyBuffer = fullText;
            HUDManager.Instance?.ShowNotification(
                "\u2728 Share text copied! Paste it anywhere to share your ritual.",
                HUDManager.NotificationType.Info);
#else
            // Editor / unsupported platform: put in clipboard.
            GUIUtility.systemCopyBuffer = fullText;
            HUDManager.Instance?.ShowNotification("Replay text copied to clipboard!", HUDManager.NotificationType.Info);
#endif
        }
    }
}
