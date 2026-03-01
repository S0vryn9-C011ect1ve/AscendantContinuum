using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using AscendantContinuum.Systems;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Season Pass UI — displays the current seasonal battle pass state and
    /// lets the player claim cosmetic rewards on free and premium tracks.
    ///
    /// Subscribes to <see cref="SeasonController.OnXPGained"/>,
    /// <see cref="SeasonController.OnRewardClaimed"/> and
    /// <see cref="SeasonController.OnSeasonChanged"/> to stay in sync without
    /// requiring polling.
    ///
    /// Wire in Inspector:
    ///   • Call <see cref="OpenPanel"/> from MainMenu / HUD battle-pass button
    ///   • Assign tier row prefab to <see cref="tierRowPrefab"/> (see below)
    ///   • Assign XP bar, season title label, and the container transforms
    /// </summary>
    public sealed class SeasonPassUIManager : MonoBehaviour
    {
        public static SeasonPassUIManager Instance { get; private set; }

        // ── Panel root ────────────────────────────────────────────────────
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button closeButton;

        // ── Season header ─────────────────────────────────────────────────
        [Header("Season Header")]
        [SerializeField] private TextMeshProUGUI seasonTitleLabel;     // "Season 1 — Dawn of Echoes"
        [SerializeField] private TextMeshProUGUI weekLabel;            // "Week 3 / 9"
        [SerializeField] private TextMeshProUGUI tierLabel;            // "Tier 4 / 10"
        [SerializeField] private Slider xpProgressBar;                 // 0-1 within current tier
        [SerializeField] private TextMeshProUGUI xpLabel;              // "1 750 / 2 000 XP"

        // ── Premium track ─────────────────────────────────────────────────
        [Header("Premium Track")]
        [SerializeField] private GameObject premiumLockedBanner;
        [SerializeField] private Button unlockPremiumButton;
        [SerializeField] private TextMeshProUGUI unlockPremiumLabel;   // price / "Active"

        // ── Tier rows (spawned at runtime) ────────────────────────────────
        [Header("Tier Rows")]
        [SerializeField] private Transform freeTrackContainer;
        [SerializeField] private Transform premiumTrackContainer;
        [SerializeField] private GameObject tierRowPrefab;   // SeasonTierRow prefab (see below)

        // ── Feedback ──────────────────────────────────────────────────────
        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackLabel;
        [SerializeField] private float feedbackDuration = 3f;
        private Coroutine _feedbackCo;

        // ── State ─────────────────────────────────────────────────────────
        private bool _open;

        // ── Lifecycle ─────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Start()
        {
            closeButton?.onClick.AddListener(ClosePanel);
            unlockPremiumButton?.onClick.AddListener(OnUnlockPremiumPressed);

            var sc = SeasonController.Instance;
            if (sc != null)
            {
                sc.OnXPGained      += OnXPGained;
                sc.OnRewardClaimed += OnRewardClaimed;
                sc.OnSeasonChanged += OnSeasonChanged;
            }
        }

        private void OnDestroy()
        {
            var sc = SeasonController.Instance;
            if (sc != null)
            {
                sc.OnXPGained      -= OnXPGained;
                sc.OnRewardClaimed -= OnRewardClaimed;
                sc.OnSeasonChanged -= OnSeasonChanged;
            }
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>Opens and refreshes the Season Pass panel.</summary>
        public void OpenPanel()
        {
            if (panelRoot == null) return;
            panelRoot.SetActive(true);
            _open = true;
            RefreshAll();
            StartCoroutine(FadeIn());
        }

        /// <summary>Closes the Season Pass panel.</summary>
        public void ClosePanel()
        {
            if (panelRoot == null) return;
            _open = false;
            StartCoroutine(FadeOutAndClose());
        }

        // ── Full refresh ──────────────────────────────────────────────────

        private void RefreshAll()
        {
            RefreshHeader();
            RefreshPremiumBanner();
            RebuildTierRows();
        }

        private void RefreshHeader()
        {
            int tier        = SeasonController.Instance != null ? SeasonController.Instance.CurrentTier   : 0;
            int xp          = SeasonController.Instance != null ? SeasonController.Instance.PlayerXP      : 0;
            int week        = SeasonController.WeekInSeason();
            int xpInTier    = xp % SeasonController.XP_PER_TIER;
            float progress  = (float)xpInTier / SeasonController.XP_PER_TIER;

            if (seasonTitleLabel != null)
                seasonTitleLabel.text = SeasonController.SeasonDisplayName();

            if (weekLabel != null)
                weekLabel.text = $"Week {week} / 9";

            if (tierLabel != null)
                tierLabel.text = $"Tier {tier + 1} / {SeasonController.FREE_TIERS}";

            if (xpProgressBar != null)
                xpProgressBar.value = progress;

            if (xpLabel != null)
                xpLabel.text = $"{xpInTier:N0} / {SeasonController.XP_PER_TIER:N0} XP";
        }

        private void RefreshPremiumBanner()
        {
            bool hasPremium = SeasonController.Instance != null && SeasonController.Instance.IsPremiumActive;

            if (premiumLockedBanner   != null) premiumLockedBanner.SetActive(!hasPremium);
            if (unlockPremiumButton   != null) unlockPremiumButton.interactable = !hasPremium;
            if (unlockPremiumLabel    != null) unlockPremiumLabel.text = hasPremium ? "✦ Active" : "Unlock — $4.99";
        }

        // ── Tier row building ─────────────────────────────────────────────

        private void RebuildTierRows()
        {
            BuildRows(freeTrackContainer,    SeasonController.FREE_REWARD_LABELS,    isPremium: false);
            BuildRows(premiumTrackContainer, SeasonController.PREMIUM_REWARD_LABELS, isPremium: true);
        }

        private void BuildRows(Transform container, string[] labels, bool isPremium)
        {
            if (container == null || tierRowPrefab == null) return;

            // Clear existing rows
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);

            var sc          = SeasonController.Instance;
            int currentTier = sc != null ? sc.CurrentTier : 0;
            bool hasPremium = sc != null && sc.IsPremiumActive;

            for (int i = 0; i < labels.Length; i++)
            {
                var row = Instantiate(tierRowPrefab, container);
                var rowUI = row.GetComponent<SeasonTierRow>();
                if (rowUI == null) continue;

                bool claimed = isPremium
                    ? (sc != null && sc.IsPremiumRewardClaimed(i))
                    : (sc != null && sc.IsFreeRewardClaimed(i));

                bool unlocked = currentTier >= i && (!isPremium || hasPremium);

                int captured = i;
                rowUI.Populate(
                    tierNumber:   i + 1,
                    label:        labels[i],
                    unlocked:     unlocked,
                    claimed:      claimed,
                    onClaim:      () => OnClaimPressed(captured, isPremium));
            }
        }

        // ── Button handlers ───────────────────────────────────────────────

        private void OnClaimPressed(int tier, bool isPremium)
        {
            var sc = SeasonController.Instance;
            if (sc == null) return;

            bool success = isPremium ? sc.ClaimPremiumReward(tier) : sc.ClaimFreeReward(tier);
            if (!success)
                ShowFeedback("Already claimed or tier not reached yet.");
        }

        private void OnUnlockPremiumPressed()
        {
            // Delegate to CosmicPatronManager / IAP flow in a future sprint
            // For now, directly unlock (demo / editor mode)
#if UNITY_EDITOR
            SeasonController.Instance?.UnlockPremiumTrack();
            ShowFeedback("Premium track unlocked! (Editor mode)");
#else
            ShowFeedback("Premium track available via Cosmic Patron subscription.");
#endif
        }

        // ── Event handlers ────────────────────────────────────────────────

        private void OnXPGained(int amount)
        {
            if (!_open) return;
            RefreshHeader();
            ShowFeedback($"+{amount} season XP");
        }

        private void OnRewardClaimed(int tier, bool isPremium)
        {
            if (!_open) return;
            string track = isPremium ? "Premium" : "Free";
            string[] labels = isPremium ? SeasonController.PREMIUM_REWARD_LABELS : SeasonController.FREE_REWARD_LABELS;
            ShowFeedback($"✦ {track} tier {tier + 1} claimed — {labels[tier]}!");
            RebuildTierRows();
        }

        private void OnSeasonChanged()
        {
            if (!_open) return;
            RefreshAll();
            ShowFeedback("A new season has begun!");
        }

        // ── Feedback ──────────────────────────────────────────────────────

        private void ShowFeedback(string message)
        {
            if (feedbackLabel == null) return;
            feedbackLabel.text = message;
            if (_feedbackCo != null) StopCoroutine(_feedbackCo);
            _feedbackCo = StartCoroutine(ClearFeedback());
        }

        private IEnumerator ClearFeedback()
        {
            yield return new WaitForSeconds(feedbackDuration);
            if (feedbackLabel != null) feedbackLabel.text = string.Empty;
        }

        // ── Fade ──────────────────────────────────────────────────────────

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;
            canvasGroup.alpha = 0f;
            float t = 0f, d = 0.3f;
            while (t < d) { t += Time.deltaTime; canvasGroup.alpha = t / d; yield return null; }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOutAndClose()
        {
            if (canvasGroup != null)
            {
                float t = 0.3f;
                while (t > 0f) { t -= Time.deltaTime; canvasGroup.alpha = t / 0.3f; yield return null; }
                canvasGroup.alpha = 0f;
            }
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // ── Accessibility ─────────────────────────────────────────────────

        /// <summary>Summary string for screen readers.</summary>
        public string GetAccessibilitySummary()
        {
            var sc = SeasonController.Instance;
            if (sc == null) return "Season pass unavailable.";
            return $"Season Pass: {SeasonController.SeasonDisplayName()}. " +
                   $"Tier {sc.CurrentTier + 1} of {SeasonController.FREE_TIERS}. " +
                   $"Player XP: {sc.PlayerXP}. Premium track: {(sc.IsPremiumActive ? "active" : "not unlocked")}.";
        }
    }
}
