using UnityEngine;
using AscendantContinuum.Systems;
using AscendantContinuum.Platform;
using AscendantContinuum.UI;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Abstract base class for every realm scene controller.
    ///
    /// Responsibilities:
    ///   • Record the active realm with <see cref="GameManager"/>
    ///   • Trigger auto-save via <see cref="SaveSystem"/>
    ///   • Forward mechanic events to <see cref="DailyChallengeManager"/>
    ///   • Gate achievements through <see cref="AchievementManager"/>
    ///   • Expose a uniform lifecycle (EnterRealm / ExitRealm) for realm scenes
    ///
    /// Derived classes must implement <see cref="SubscribeToRealmEvents"/> and
    /// <see cref="UnsubscribeFromRealmEvents"/> to connect their specific
    /// gameplay mechanics to <see cref="OnRealmProgressMade"/>.
    /// </summary>
    public abstract class RealmController : MonoBehaviour
    {
        // ── Identity ──────────────────────────────────────────────────────
        /// <summary>Lowercase realm identifier used by GameManager / SaveSystem / Achievement checks (e.g. "emberforge").</summary>
        [SerializeField] protected string realmId = "unknown";

        /// <summary>Human-readable display name shown in the HUD / notifications.</summary>
        [SerializeField] protected string realmDisplayName = "Unknown Realm";

        // ── Auto-save ──────────────────────────────────────────────────────
        /// <summary>How many progress events fire between auto-saves.</summary>
        [SerializeField] private int autoSaveEveryNEvents = 5;

        private int _progressEventCount;
        private HUDManager _hud;                // cached at Start — may be null outside game scenes
        private float _realmEnterTime;     // Time.realtimeSinceStartup when we entered

        // ── Lifecycle ──────────────────────────────────────────────────────
        protected virtual void Awake() { }

        protected virtual void Start()
        {
            _hud = FindFirstObjectByType<HUDManager>();
            EnterRealm();
        }

        protected virtual void OnDestroy()
        {
            ExitRealm();
        }

        // ── Realm enter / exit ──────────────────────────────────────────────

        /// <summary>
        /// Called once when the realm scene starts. Informs the core systems
        /// that the player has entered this realm and subscribes to mechanic events.
        /// </summary>
        protected void EnterRealm()
        {
            _realmEnterTime = UnityEngine.Time.realtimeSinceStartup;

            // Tell GameManager which realm we are in
            GameManager.Instance?.RecordLastRealm(realmId);
            GameManager.Instance?.ChangeState(GameState.Playing);

            // Visit achievement check
            AchievementManager.Instance?.UnlockAchievement($"visited_{realmId}");

            // Notify daily-challenge system (VisitRealms challenge)
            DailyChallengeManager.Instance?.IncrementChallengeProgress(ChallengeType.VisitRealms, 1);

            // Let derived class hook into mechanic events
            SubscribeToRealmEvents();

            Debug.Log($"[RealmController] Entered {realmDisplayName} ({realmId})");
        }

        /// <summary>
        /// Called when this MonoBehaviour is destroyed (scene unload).
        /// Unsubscribes from all mechanic events to prevent memory leaks.
        /// </summary>
        protected void ExitRealm()
        {
            UnsubscribeFromRealmEvents();

            // Record realm time for cosmic identity evolution
            float minutesSpent = (UnityEngine.Time.realtimeSinceStartup - _realmEnterTime) / 60f;
            CosmicIdentitySystem.Instance?.RecordRealmTime(realmId, minutesSpent);

            // Post to Play Games leaderboards if signed in
            Platform.GooglePlayGamesManager.Instance?.PostSparksScore(
                SaveSystem.Instance?.CurrentPlayerData?.sparksCollected ?? 0);

            // Final save on exit
            SaveCurrentProgress();

            Debug.Log($"[RealmController] Exited {realmDisplayName} ({realmId}) after {minutesSpent:F1} min");
        }

        // ── Abstract hooks ──────────────────────────────────────────────────

        /// <summary>Subscribe to realm-specific mechanic events here.</summary>
        protected abstract void SubscribeToRealmEvents();

        /// <summary>Unsubscribe from realm-specific mechanic events here.</summary>
        protected abstract void UnsubscribeFromRealmEvents();

        // ── Progress reporting ──────────────────────────────────────────────

        /// <summary>
        /// Call this from derived classes whenever the player makes a unit of
        /// meaningful progress (e.g. collects a spark, completes a constellation).
        /// </summary>
        /// <param name="challengeType">The challenge category this event advances.</param>
        /// <param name="amount">How much to increment the challenge counter.</param>
        /// <param name="sparks">Sparks earned this event (passed to the HUD).</param>
        /// <param name="sigils">Sigils earned this event (passed to the HUD).</param>
        protected void OnRealmProgressMade(ChallengeType challengeType,
                                           int amount = 1,
                                           int sparks = 0,
                                           int sigils = 0)
        {
            // Daily challenge
            DailyChallengeManager.Instance?.IncrementChallengeProgress(challengeType, amount);

            // Save system snapshot
            SaveSystem.Instance?.UpdateProgressSnapshot(realmId, sparks, sigils);

            // HUD
            if (_hud != null && (sparks > 0 || sigils > 0))
            {
                var pd = SaveSystem.Instance?.CurrentPlayerData;
                if (pd != null)
                    _hud.SyncFromPlayerData(pd);
            }

            // Throttled auto-save
            _progressEventCount++;
            if (_progressEventCount % autoSaveEveryNEvents == 0)
                SaveCurrentProgress();
        }

        /// <summary>Triggers an immediate full save.</summary>
        protected void SaveCurrentProgress()
        {
            SaveSystem.Instance?.SaveGame();
        }

        /// <summary>Unlocks an achievement by ID (delegates to AchievementManager).</summary>
        protected void TryUnlockAchievement(string achievementId)
        {
            AchievementManager.Instance?.UnlockAchievement(achievementId);
        }
    }
}
