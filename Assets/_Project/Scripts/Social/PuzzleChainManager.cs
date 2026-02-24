using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.Social
{
    /// <summary>
    /// Cross-Player Puzzle Chains — Asynchronous collaborative ritual sequences.
    ///
    /// A chain requires up to N different players to each complete a different
    /// ritual step. Players never communicate directly; the system automatically
    /// links them and notifies each player when their link is ready.
    ///
    /// Chain scales:
    ///   2-player  → Small reward
    ///   5-player  → Medium reward
    ///   10-player → Large reward (Elemental Master sigil)
    ///   50-player → Epic (very rare, community event)
    ///
    /// Without a live Firebase backend the chain still works: each player can
    /// "simulate" their chain locally and credit themselves for all links —
    /// the real cross-player magic activates once Firebase is wired.
    /// </summary>
    public sealed class PuzzleChainManager : MonoBehaviour
    {
        public static PuzzleChainManager Instance { get; private set; }

        // ── PlayerPrefs keys ─────────────────────────────────────────────
        private const string PREF_ACTIVE_CHAIN     = "Chain_ActiveId";
        private const string PREF_CHAIN_LINKS      = "Chain_LinksCompleted_";
        private const string PREF_CHAIN_TARGET     = "Chain_Target_";
        private const string PREF_CHAINS_COMPLETED = "Chain_TotalCompleted";
        private const string PREF_CHAINS_HELPED    = "Chain_TotalHelped";

        // ── Elemental chain definition ────────────────────────────────────
        private static readonly (string realm, string ritual, string element)[] ELEMENTAL_CHAIN =
        {
            ("Emberforge",  "Spark Forge",          "Fire"),
            ("Verdant",     "Bloom Ritual",         "Nature"),
            ("EchoFields",  "Constellation Trace",  "Star"),
            ("DawnCitadel", "Prism Alignment",      "Light"),
            ("Lantern",     "Ascension Ritual",     "Void"),
        };

        // ── Events ────────────────────────────────────────────────────────
        public event Action<string, int, int> OnChainProgress;  // chainId, linksComplete, totalLinks
        public event Action<string>           OnChainCompleted; // chainId

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            CheckForPendingChains();
        }

        // ── Chain management ──────────────────────────────────────────────

        /// <summary>
        /// Start a new Elemental Chain from a given realm. Returns the new chainId.
        /// </summary>
        public string StartElementalChain(string initiatingRealm)
        {
            string chainId = $"elemental_{DateTime.UtcNow:yyyyMMddHHmm}";
            PlayerPrefs.SetString(PREF_ACTIVE_CHAIN, chainId);
            PlayerPrefs.SetInt(PREF_CHAIN_TARGET + chainId, ELEMENTAL_CHAIN.Length);
            PlayerPrefs.Save();

            // Complete the first link immediately (initiating player)
            CompleteChainLink(chainId, 0);

            HUDManager.Instance?.ShowNotification(
                $"🔗 Elemental Chain started! Complete all 5 realm rituals to forge the chain.",
                HUDManager.NotificationType.Info);

            // In a live backend, notify next player via GuardianMessengerSystem / Firebase
            FirebaseManager.Instance?.SaveData("puzzleChains", chainId, new
            {
                type    = "elemental",
                realm   = initiatingRealm,
                started = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            return chainId;
        }

        /// <summary>
        /// Call when the player completes a chain ritual step.
        /// linkIndex must match the ELEMENTAL_CHAIN order (realm index).
        /// </summary>
        public void CompleteChainLink(string chainId, int linkIndex)
        {
            string key   = $"{PREF_CHAIN_LINKS}{chainId}_{linkIndex}";
            if (PlayerPrefs.GetInt(key, 0) == 1) return;    // already done

            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();

            int completed = CountCompletedLinks(chainId);
            int target    = PlayerPrefs.GetInt(PREF_CHAIN_TARGET + chainId, ELEMENTAL_CHAIN.Length);

            OnChainProgress?.Invoke(chainId, completed, target);

            HUDManager.Instance?.ShowNotification(
                $"🔗 Chain link {completed}/{target} — {ELEMENTAL_CHAIN[linkIndex].element} realm complete!",
                HUDManager.NotificationType.Info);

            int helped = PlayerPrefs.GetInt(PREF_CHAINS_HELPED, 0) + 1;
            PlayerPrefs.SetInt(PREF_CHAINS_HELPED, helped);
            AchievementManager.Instance?.UnlockAchievement("puzzle_chain_helper");

            if (completed >= target)
                FinaliseChain(chainId);
        }

        /// <summary>
        /// Try to automatically match the current realm to an active chain link and complete it.
        /// Call this from realm controllers on ritual completion.
        /// </summary>
        public void TryMatchRealmToChain(string realmId)
        {
            string chainId = PlayerPrefs.GetString(PREF_ACTIVE_CHAIN, "");
            if (string.IsNullOrEmpty(chainId)) return;

            for (int i = 0; i < ELEMENTAL_CHAIN.Length; i++)
            {
                if (ELEMENTAL_CHAIN[i].realm.Equals(realmId, StringComparison.OrdinalIgnoreCase))
                {
                    string done = $"{PREF_CHAIN_LINKS}{chainId}_{i}";
                    if (PlayerPrefs.GetInt(done, 0) == 0)
                    {
                        CompleteChainLink(chainId, i);
                        return;
                    }
                }
            }
        }

        /// <summary>Returns the number of completed links in a given chain.</summary>
        public int CountCompletedLinks(string chainId)
        {
            int count = 0;
            for (int i = 0; i < ELEMENTAL_CHAIN.Length; i++)
                if (PlayerPrefs.GetInt($"{PREF_CHAIN_LINKS}{chainId}_{i}", 0) == 1) count++;
            return count;
        }

        /// <summary>Returns true if an elemental chain is currently active for this player.</summary>
        public bool HasActiveChain => !string.IsNullOrEmpty(PlayerPrefs.GetString(PREF_ACTIVE_CHAIN, ""));

        public int TotalChainsCompleted => PlayerPrefs.GetInt(PREF_CHAINS_COMPLETED, 0);

        // ── Private ────────────────────────────────────────────────────────

        private void CheckForPendingChains()
        {
            // Detect any chains that need resumption
            string chainId = PlayerPrefs.GetString(PREF_ACTIVE_CHAIN, "");
            if (string.IsNullOrEmpty(chainId)) return;

            int completed = CountCompletedLinks(chainId);
            int target    = PlayerPrefs.GetInt(PREF_CHAIN_TARGET + chainId, ELEMENTAL_CHAIN.Length);

            if (completed < target)
            {
                string next = FindNextIncompleteRealm(chainId);
                if (!string.IsNullOrEmpty(next))
                    HUDManager.Instance?.ShowNotification(
                        $"🔗 Chain in progress ({completed}/{target}). Next: {next}",
                        HUDManager.NotificationType.Info);
            }
        }

        private string FindNextIncompleteRealm(string chainId)
        {
            for (int i = 0; i < ELEMENTAL_CHAIN.Length; i++)
            {
                if (PlayerPrefs.GetInt($"{PREF_CHAIN_LINKS}{chainId}_{i}", 0) == 0)
                    return ELEMENTAL_CHAIN[i].realm;
            }
            return "";
        }

        private void FinaliseChain(string chainId)
        {
            PlayerPrefs.SetString(PREF_ACTIVE_CHAIN, "");
            int total = PlayerPrefs.GetInt(PREF_CHAINS_COMPLETED, 0) + 1;
            PlayerPrefs.SetInt(PREF_CHAINS_COMPLETED, total);
            PlayerPrefs.Save();

            OnChainCompleted?.Invoke(chainId);
            AchievementManager.Instance?.UnlockAchievement("elemental_master");
            if (total >= 10) AchievementManager.Instance?.UnlockAchievement("chain_master");

            HUDManager.Instance?.ShowNotification(
                "✦ ELEMENTAL CHAIN COMPLETE — Elemental Master sigil unlocked!",
                HUDManager.NotificationType.Achievement);

            Debug.Log($"[PuzzleChain] Chain {chainId} completed (total: {total}).");
        }
    }
}
