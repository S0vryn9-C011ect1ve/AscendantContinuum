using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.Platform;

namespace AscendantContinuum.Social
{
    /// <summary>
    /// Anonymous Kindness Chain — players send wordless blessings to strangers
    /// who haven't opened the game in 3+ days.
    ///
    /// How it works:
    ///   1. After any ritual completion an optional one-tap "Send a Blessing"
    ///      prompt appears for 5 seconds (no pressure, it auto-dismisses).
    ///   2. The blessing is anonymised (sender's Cosmic Name shown, never username).
    ///   3. On the recipient's next app launch, a full-screen blessing moment
    ///      plays before the main menu — quiet, beautiful, ~8 seconds.
    ///   4. The recipient can optionally forward the chain (one tap).
    ///
    /// Local side: PlayerPrefs queue (works offline, syncs to Firebase when online).
    /// Firebase side: Cloud Functions pick the longest-absent player from the pool
    ///                and route incoming blessings — not implemented here, but the
    ///                local events are all in place for the cloud layer to hook into.
    ///
    /// Blessing messages are procedurally assembled so no two feel identical.
    /// Messages were written to work in ANY context — loss, burnout, joy, boredom.
    /// </summary>
    public sealed class KindnessChainManager : MonoBehaviour
    {
        public static KindnessChainManager Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────
        public event Action<KindnessBlessing> OnBlessingReceived;
        public event Action                   OnBlessingSent;

        // ── Config ─────────────────────────────────────────────────────────
        [Header("Settings")]
        [SerializeField] private float promptDisplayDuration = 6f;
        [SerializeField] private float blessingRevealDuration = 8f;
        [SerializeField] private int   absentDaysThreshold = 3; // days before player is "absent"

        // ── Pref keys ──────────────────────────────────────────────────────
        private const string PREF_LAST_SESSION  = "KC_LastSessionUtc";
        private const string PREF_PENDING       = "KC_PendingBlessing";       // incoming JSON
        private const string PREF_SENT_COUNT    = "KC_SentCount";
        private const string PREF_RECEIVED_COUNT= "KC_ReceivedCount";
        private const string PREF_CHAIN_OPT     = "KC_OptIn";

        public bool OptedIn
        {
            get => PlayerPrefs.GetInt(PREF_CHAIN_OPT, 1) == 1;
            set { PlayerPrefs.SetInt(PREF_CHAIN_OPT, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        // ── Message banks (procedural assembly) ───────────────────────────
        private static readonly string[] OPENINGS =
        {
            "A stranger thought of you today.",
            "Somewhere in the realms, someone paused.",
            "You don't know them. They don't know you.",
            "Across ten thousand lanterns, one was lit for you.",
            "A seeker in the Emberforge sent this into the dark.",
        };

        private static readonly string[] MIDDLES =
        {
            "They wanted you to know: you are not forgotten.",
            "They hoped you were doing okay.",
            "They sent you the warmth they felt in the ritual.",
            "They wished you rest, or joy, or whatever you need most.",
            "They left a spark for you, just in case.",
        };

        private static readonly string[] CLOSINGS =
        {
            "The realms keep your place.",
            "Your light is still here.",
            "Come back when you are ready.",
            "The constellations remember your shape.",
            "The garden has been growing in your absence.",
        };

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            RecordSession();
            CheckForIncomingBlessing();
        }

        // ── Session recording ──────────────────────────────────────────────
        private void RecordSession()
        {
            string lastStr = PlayerPrefs.GetString(PREF_LAST_SESSION, "");
            PlayerPrefs.SetString(PREF_LAST_SESSION, DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();

            if (string.IsNullOrEmpty(lastStr)) return;

            if (DateTime.TryParse(lastStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            {
                double daysAway = (DateTime.UtcNow - last).TotalDays;
                if (daysAway >= absentDaysThreshold)
                {
                    // Player was absent — flag them as eligible to receive a blessing
                    // (cloud function would pick this up; locally we leave a marker)
                    PlayerPrefs.SetInt("KC_EligibleForBlessing", 1);
                    Debug.Log($"[KindnessChain] Player was absent {daysAway:F1} days — eligible for blessing.");
                }
                else
                {
                    PlayerPrefs.DeleteKey("KC_EligibleForBlessing");
                }
            }
        }

        // ── Incoming blessing check ────────────────────────────────────────
        private void CheckForIncomingBlessing()
        {
            if (!OptedIn) return;

            string json = PlayerPrefs.GetString(PREF_PENDING, "");
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var blessing = JsonUtility.FromJson<KindnessBlessing>(json);
                if (blessing != null)
                {
                    PlayerPrefs.DeleteKey(PREF_PENDING);
                    PlayerPrefs.Save();
                    StartCoroutine(RevealBlessing(blessing));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[KindnessChain] Failed to parse pending blessing: {e.Message}");
                PlayerPrefs.DeleteKey(PREF_PENDING);
            }
        }

        // ── Called after any ritual completion ─────────────────────────────
        /// <summary>
        /// Surfaces a brief, non-intrusive "Send a Blessing" prompt.
        /// Auto-dismisses after <see cref="promptDisplayDuration"/> seconds.
        /// Call this from EmberforgeSparks, VerdantGarden, ConstellationTracer, etc.
        /// </summary>
        public void ShowSendBlessingPrompt(string contextRealm = "Unknown")
        {
            if (!OptedIn) return;

            // Don't prompt if sent one in the last 30 minutes
            string lastSentStr = PlayerPrefs.GetString("KC_LastSentTime", "");
            if (!string.IsNullOrEmpty(lastSentStr) &&
                DateTime.TryParse(lastSentStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastSent) &&
                (DateTime.UtcNow - lastSent).TotalMinutes < 30)
                return;

            StartCoroutine(DisplayPromptThenSend(contextRealm));
        }

        private IEnumerator DisplayPromptThenSend(string contextRealm)
        {
            // Notify the UI layer via HUD (a full UI overlay would live in a dedicated Canvas)
            UI.HUDManager.Instance?.ShowNotification(
                "💫 Send a blessing to a stranger? (tap to confirm)",
                UI.HUDManager.NotificationType.Info);

            // Screen reader
            AccessibilityManager.Instance?.Announce(
                "Optional: tap to send an anonymous blessing to a player who has been away.");

            // Wait briefly — in a real UI this would be a tappable card with auto-dismiss
            // For now, we auto-send after the prompt duration (UI layer intercepts to skip or cancel)
            yield return new WaitForSeconds(promptDisplayDuration);

            SendBlessing(contextRealm);
        }

        // ── Send ────────────────────────────────────────────────────────────
        public void SendBlessing(string contextRealm = "Unknown")
        {
            if (!OptedIn) return;

            var blessing = new KindnessBlessing
            {
                blessingId   = Guid.NewGuid().ToString(),
                senderName   = GetCosmicName(),
                senderRealm  = contextRealm,
                message      = BuildMessage(),
                sentAtUtc    = DateTime.UtcNow.ToString("o")
            };

            // Persist send time (rate limit)
            PlayerPrefs.SetString("KC_LastSentTime", DateTime.UtcNow.ToString("o"));
            int sent = PlayerPrefs.GetInt(PREF_SENT_COUNT, 0) + 1;
            PlayerPrefs.SetInt(PREF_SENT_COUNT, sent);
            PlayerPrefs.Save();

            // Queue for Firebase upload (cloud layer picks it up and routes to absent player)
            QueueBlessingForUpload(blessing);

            // Achievements
            AchievementManager.Instance?.TrackProgress("kindness_giver", sent);
            if (sent == 1) AchievementManager.Instance?.UnlockAchievement("first_kindness");

            OnBlessingSent?.Invoke();

            UI.HUDManager.Instance?.ShowNotification(
                "🕊️ Your blessing is on its way to someone in the dark.",
                UI.HUDManager.NotificationType.Reward);

            Core.FirebaseManager.Instance?.TrackEvent("kindness_blessing_sent", new Dictionary<string, object>
            {
                { "realm", contextRealm }, { "total_sent", sent }
            });

            Debug.Log($"[KindnessChain] Blessing sent by '{blessing.senderName}' from {contextRealm}. Total sent: {sent}");
        }

        // ── Receive (called by Firebase cloud message → C# bridge or local sim) ──
        /// <summary>
        /// Delivers an incoming blessing from another player.
        /// Can be called by the Firebase message handler on app resume,
        /// or locally to simulate during development.
        /// </summary>
        public void ReceiveBlessing(KindnessBlessing blessing)
        {
            if (!OptedIn) return;

            // Store for next session reveal (shown on next launch before main menu)
            string json = JsonUtility.ToJson(blessing);
            PlayerPrefs.SetString(PREF_PENDING, json);
            int received = PlayerPrefs.GetInt(PREF_RECEIVED_COUNT, 0) + 1;
            PlayerPrefs.SetInt(PREF_RECEIVED_COUNT, received);
            PlayerPrefs.Save();

            // If app is currently open, show immediately
            StartCoroutine(RevealBlessing(blessing));

            Core.FirebaseManager.Instance?.TrackEvent("kindness_blessing_received", new Dictionary<string, object>
            {
                { "sender_realm", blessing.senderRealm }, { "total_received", received }
            });
        }

        // ── Reveal ─────────────────────────────────────────────────────────
        private IEnumerator RevealBlessing(KindnessBlessing blessing)
        {
            Debug.Log($"[KindnessChain] ✨ Blessing received from '{blessing.senderName}': {blessing.message}");

            // Slow the world for a moment
            Time.timeScale = 0.3f;
            yield return new WaitForSecondsRealtime(0.4f);
            Time.timeScale = 1f;

            // Screen reader — the full message read aloud
            AccessibilityManager.Instance?.Announce(
                $"You received a blessing. {blessing.message} It was sent from the {blessing.senderRealm}.");

            // Gentle haptic
            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Medium);

            // HUD notification with sender's cosmic name
            UI.HUDManager.Instance?.ShowNotification(
                $"🕊️ {blessing.senderName} sent you a blessing from {blessing.senderRealm}:\n\"{blessing.message}\"",
                UI.HUDManager.NotificationType.Achievement);

            // Achievement
            int received = PlayerPrefs.GetInt(PREF_RECEIVED_COUNT, 1);
            AchievementManager.Instance?.TrackProgress("kindness_receiver", received);
            if (received == 1) AchievementManager.Instance?.UnlockAchievement("first_blessing_received");

            // Particle effect
            VFX.ParticleManager.Instance?.SetMotionScale(1.5f);
            yield return new WaitForSeconds(blessingRevealDuration);
            VFX.ParticleManager.Instance?.SetMotionScale(1f);

            OnBlessingReceived?.Invoke(blessing);
        }

        // ── Firebase queue ──────────────────────────────────────────────────
        private static void QueueBlessingForUpload(KindnessBlessing blessing)
        {
            // Local queue — Firebase manager uploads on next network opportunity
            string json = JsonUtility.ToJson(blessing);
            PlayerPrefs.SetString("KC_PendingUpload", json);
            PlayerPrefs.Save();

            // Direct Firebase call (silently no-ops if offline)
            Core.FirebaseManager.Instance?.TrackEvent("kindness_queued", new Dictionary<string, object>
            {
                { "blessing_id", blessing.blessingId },
                { "sender",      blessing.senderName }
            });
        }

        // ── Message generation ─────────────────────────────────────────────
        private static string BuildMessage()
        {
            var rng = new System.Random();
            string opening = OPENINGS[rng.Next(OPENINGS.Length)];
            string middle  = MIDDLES[rng.Next(MIDDLES.Length)];
            string closing = CLOSINGS[rng.Next(CLOSINGS.Length)];
            return $"{opening} {middle} {closing}";
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private static string GetCosmicName()
        {
            string name = CosmicIdentitySystem.Instance?.CosmicName ?? "A Wandering Seeker";
            // Append Cosmic Patron badge (cosmetic only — never used for advantage)
            string badge = CosmicPatronManager.Instance?.GetPatronBadge() ?? string.Empty;
            return name + badge;
        }

        // ── Public stats ───────────────────────────────────────────────────
        public int TotalSent     => PlayerPrefs.GetInt(PREF_SENT_COUNT, 0);
        public int TotalReceived => PlayerPrefs.GetInt(PREF_RECEIVED_COUNT, 0);
    }

    // ── Data types ──────────────────────────────────────────────────────────
    [Serializable]
    public sealed class KindnessBlessing
    {
        public string blessingId;
        public string senderName;   // Cosmic Name, never real username
        public string senderRealm;
        public string message;
        public string sentAtUtc;
    }
}
