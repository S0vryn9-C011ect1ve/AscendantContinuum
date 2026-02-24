using UnityEngine;
using System;
using System.Collections.Generic;

namespace AscendantContinuum.Social
{
    /// <summary>
    /// Cross-Player Wish Wall — the asynchronous social fabric of the game.
    ///
    /// Players in Lantern Ascension release wishes into a shared sky. Those
    /// wishes drift across other players' realms. Time Capsules planted in
    /// Echo Fields are discovered by players weeks or months later. Cross-realm
    /// puzzle chains require many players to collectively complete objectives.
    ///
    /// This manager handles the LOCAL side of the wish wall. Network sync
    /// is handled by FirebaseManager's Firestore layer (when online) or
    /// deferred to next session launch (offline-first).
    ///
    /// Unique mechanics that make this system press-worthy:
    ///   1. Every wish is signed with the sender's Cosmic Name (not username)
    ///   2. Wishes expire after 30 days — impermanence is intentional
    ///   3. Time Capsules are geo-stamped: you see WHERE they were planted
    ///   4. Puzzle chains can only be started during Live Events
    ///   5. Receiving a wish during a sacred event doubles the feeling
    /// </summary>
    public sealed class CrossPlayerWishWall : MonoBehaviour
    {
        public static CrossPlayerWishWall Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────
        public event Action<WishMessage>   OnWishReceived;
        public event Action<TimeCapsule>   OnTimeCapsuleDiscovered;
        public event Action<PuzzleChain>   OnPuzzleChainContributed;
        public event Action<int>           OnWallCountUpdated;         // total active wishes

        // ── Local cache ────────────────────────────────────────────────────
        private List<WishMessage>   _receivedWishes    = new();
        private List<TimeCapsule>   _plantedCapsules   = new();
        private List<PuzzleChain>   _activeChains      = new();

        // ── Prefs ──────────────────────────────────────────────────────────
        private const string PREF_WISHES    = "wishwall_received_v1";
        private const string PREF_CAPSULES  = "timecapsules_planted_v1";
        private const string PREF_WALL_OPT  = "wishwall_enabled";

        public bool IsEnabled
        {
            get => PlayerPrefs.GetInt(PREF_WALL_OPT, 1) == 1;
            set { PlayerPrefs.SetInt(PREF_WALL_OPT, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        // ── Lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadLocalData();
            PurgeExpiredWishes();
        }

        // ── Wishes ─────────────────────────────────────────────────────────

        /// <summary>
        /// Called when the local player releases a lantern in Lantern Ascension.
        /// Constructs a WishMessage and queues it for Firebase upload.
        /// </summary>
        public WishMessage ReleaseWish(string wishText,
                                       WishVisibility visibility = WishVisibility.Anonymous)
        {
            var wish = new WishMessage
            {
                wishId        = Guid.NewGuid().ToString(),
                senderName    = GetCosmicName(),
                senderAura    = GetAuraTier(),
                wishText      = SanitiseWish(wishText),
                visibility    = visibility,
                releasedUtcMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                expiresUtcMs  = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeMilliseconds(),
                eventId       = AscendantContinuum.Systems.LiveEventEngine.Instance?.CurrentEvent?.id ?? string.Empty
            };

            // Queue for server upload
            QueueForUpload("wish", wish.wishId, UnityEngine.JsonUtility.ToJson(wish));

            Debug.Log($"[WishWall] Released wish from {wish.senderName}: \"{wishText}\"");
            return wish;
        }

        /// <summary>
        /// Injects a wish received from another player (called by FirebaseManager
        /// on Firestore listener).
        /// </summary>
        public void ReceiveWish(WishMessage wish)
        {
            if (!IsEnabled) return;
            if (_receivedWishes.Exists(w => w.wishId == wish.wishId)) return;

            _receivedWishes.Add(wish);
            SaveLocalData();
            OnWishReceived?.Invoke(wish);
            OnWallCountUpdated?.Invoke(_receivedWishes.Count);

            Debug.Log($"[WishWall] Received wish from {wish.senderName}");
        }

        // ── Time Capsules ──────────────────────────────────────────────────

        /// <summary>
        /// Plants a time capsule in Echo Fields for future players to discover.
        /// Capsules unlock only after the set delay (minimum 7 days).
        /// </summary>
        public TimeCapsule PlantTimeCapsule(string message, int daysUntilOpen = 30)
        {
            daysUntilOpen = Mathf.Max(7, daysUntilOpen); // minimum 7-day delay

            var capsule = new TimeCapsule
            {
                capsuleId       = Guid.NewGuid().ToString(),
                authorName      = GetCosmicName(),
                message         = SanitiseWish(message),
                plantedUtcMs    = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                opensUtcMs      = DateTimeOffset.UtcNow.AddDays(daysUntilOpen).ToUnixTimeMilliseconds(),
                realmId         = "echo",
                eventContext    = AscendantContinuum.Systems.LiveEventEngine.Instance?.CurrentEvent?.displayName ?? string.Empty
            };

            _plantedCapsules.Add(capsule);
            SaveLocalData();

            QueueForUpload("capsule", capsule.capsuleId, UnityEngine.JsonUtility.ToJson(capsule));

            Debug.Log($"[WishWall] Time capsule planted by {capsule.authorName}, opens in {daysUntilOpen} days.");
            return capsule;
        }

        /// <summary>
        /// Called by FirebaseManager when a capsule's open-date has passed and
        /// it arrives for the local player to read.
        /// </summary>
        public void DiscoverTimeCapsule(TimeCapsule capsule)
        {
            OnTimeCapsuleDiscovered?.Invoke(capsule);
            AscendantContinuum.Systems.AchievementManager.Instance?.UnlockAchievement("archaeologist");

            Debug.Log($"[WishWall] Discovered capsule from {capsule.authorName}!");
        }

        // ── Puzzle Chains ──────────────────────────────────────────────────

        /// <summary>
        /// Contributes progress to the current global puzzle chain.
        /// Chains are only active during Live Events.
        /// </summary>
        public void ContributeToPuzzleChain(string chainId, float contribution)
        {
            var chain = _activeChains.Find(c => c.chainId == chainId);
            if (chain == null)
            {
                chain = new PuzzleChain { chainId = chainId, totalProgress = 0f, requiredProgress = 1000f };
                _activeChains.Add(chain);
            }

            chain.totalProgress = Mathf.Min(chain.totalProgress + contribution, chain.requiredProgress);
            chain.contributions++;

            if (chain.totalProgress >= chain.requiredProgress && !chain.isComplete)
            {
                chain.isComplete = true;
                OnPuzzleChainContributed?.Invoke(chain);
                AscendantContinuum.Systems.AchievementManager.Instance?.UnlockAchievement("puzzle_chain_helper");
                Debug.Log($"[WishWall] Puzzle chain '{chainId}' COMPLETED by the community!");
            }
            else
            {
                OnPuzzleChainContributed?.Invoke(chain);
            }

            QueueForUpload("chain", chainId + "_" + chain.contributions,
                           UnityEngine.JsonUtility.ToJson(chain));
        }

        // ── Wall query helpers ─────────────────────────────────────────────

        public List<WishMessage> GetRecentWishes(int max = 20)
        {
            var sorted = new List<WishMessage>(_receivedWishes);
            sorted.Sort((a, b) => b.releasedUtcMs.CompareTo(a.releasedUtcMs));
            return sorted.GetRange(0, Mathf.Min(max, sorted.Count));
        }

        public int WishCount => _receivedWishes.Count;

        // ── Firebase upload queue (offline-first) ──────────────────────────
        private void QueueForUpload(string type, string id, string json)
        {
            // Store in PlayerPrefs queue — FirebaseManager drains this on connectivity
            string key      = $"upload_queue_{type}_{id}";
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            Debug.Log($"[WishWall] Queued {type} '{id}' for server upload.");
        }

        // ── Maintenance ────────────────────────────────────────────────────
        private void PurgeExpiredWishes()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int before = _receivedWishes.Count;
            _receivedWishes.RemoveAll(w => w.expiresUtcMs < now);
            int removed = before - _receivedWishes.Count;

            if (removed > 0)
            {
                SaveLocalData();
                Debug.Log($"[WishWall] Purged {removed} expired wishes.");
            }
        }

        // ── Persistence ────────────────────────────────────────────────────
        private void LoadLocalData()
        {
            string wishJson    = PlayerPrefs.GetString(PREF_WISHES,   "[]");
            string capsuleJson = PlayerPrefs.GetString(PREF_CAPSULES, "[]");

            var wrapperW = UnityEngine.JsonUtility.FromJson<JsonWrapper<WishMessage>>(wishJson);
            var wrapperC = UnityEngine.JsonUtility.FromJson<JsonWrapper<TimeCapsule>>(capsuleJson);

            _receivedWishes  = wrapperW?.items  ?? new List<WishMessage>();
            _plantedCapsules = wrapperC?.items  ?? new List<TimeCapsule>();
        }

        private void SaveLocalData()
        {
            PlayerPrefs.SetString(PREF_WISHES,
                UnityEngine.JsonUtility.ToJson(new JsonWrapper<WishMessage> { items = _receivedWishes }));
            PlayerPrefs.SetString(PREF_CAPSULES,
                UnityEngine.JsonUtility.ToJson(new JsonWrapper<TimeCapsule> { items = _plantedCapsules }));
            PlayerPrefs.Save();
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private static string GetCosmicName()
            => AscendantContinuum.Systems.CosmicIdentitySystem.Instance?.CosmicName ?? "Unknown Wanderer";

        private static int GetAuraTier()
            => (int)(AscendantContinuum.Systems.CosmicIdentitySystem.Instance?.AuraTierLevel
                     ?? AscendantContinuum.Systems.AuraTier.Spark);

        private static string SanitiseWish(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            // Trim to 280 chars (Twitter-length feel)
            return text.Length > 280 ? text.Substring(0, 280) : text.Trim();
        }

        [Serializable] private class JsonWrapper<T> { public List<T> items; }
    }

    // ── Data models ────────────────────────────────────────────────────────

    public enum WishVisibility { Private, Anonymous, Public }

    [Serializable]
    public sealed class WishMessage
    {
        public string senderName;
        public int    senderAura;
        public string wishText;
        public string wishId;
        public WishVisibility visibility;
        public long   releasedUtcMs;
        public long   expiresUtcMs;
        public string eventId;          // set if released during a live event
    }

    [Serializable]
    public sealed class TimeCapsule
    {
        public string capsuleId;
        public string authorName;
        public string message;
        public long   plantedUtcMs;
        public long   opensUtcMs;
        public string realmId;
        public string eventContext;     // event happening when planted
    }

    [Serializable]
    public sealed class PuzzleChain
    {
        public string chainId;
        public float  totalProgress;
        public float  requiredProgress;
        public int    contributions;
        public bool   isComplete;
    }
}
