using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using AscendantContinuum.Platform;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Cosmic Shop — presents the Cosmic Patron subscription and cosmetic one-time
    /// purchases.  All design principles from CONSTITUTION.md apply:
    ///   • No gameplay advantage sold
    ///   • All prices displayed honestly (no artificial scarcity timers)
    ///   • Patron badge is cosmetic only
    ///   • "Restore Purchases" always visible
    ///
    /// Layout driven by Unity UI (TMP + Image/Button components wired in the
    /// Inspector).  Call <see cref="OpenShop"/> from any button in the scene.
    /// </summary>
    public sealed class ShopUIManager : MonoBehaviour
    {
        public static ShopUIManager Instance { get; private set; }

        // ── Root panel ────────────────────────────────────────────────────
        [Header("Panels")]
        [SerializeField] private GameObject shopRootPanel;
        [SerializeField] private CanvasGroup shopCanvasGroup;

        // ── Patron subscription card ──────────────────────────────────────
        [Header("Patron Card")]
        [SerializeField] private GameObject patronCard;
        [SerializeField] private TextMeshProUGUI patronStatusLabel;   // "✦ Active Patron" / "Become a Patron"
        [SerializeField] private Button patronBuyButton;
        [SerializeField] private TextMeshProUGUI patronBuyButtonLabel;
        [SerializeField] private GameObject patronPerksPanel;          // lists the boons

        // ── One-time cosmetic cards ───────────────────────────────────────
        [Header("Cosmetic Cards")]
        [SerializeField] private Button sigilFlameBuyButton;
        [SerializeField] private TextMeshProUGUI sigilFlameStatusLabel;

        [SerializeField] private Button sigilVerdantBuyButton;
        [SerializeField] private TextMeshProUGUI sigilVerdantStatusLabel;

        [SerializeField] private Button constellationPackBuyButton;
        [SerializeField] private TextMeshProUGUI constellationPackStatusLabel;

        [SerializeField] private Button seekersArchiveBuyButton;
        [SerializeField] private TextMeshProUGUI seekersArchiveStatusLabel;

        [SerializeField] private Button cosmicNameBuyButton;
        [SerializeField] private TextMeshProUGUI cosmicNameStatusLabel;

        // ── Footer ────────────────────────────────────────────────────────
        [Header("Footer")]
        [SerializeField] private Button restorePurchasesButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI patronBadgePreviewLabel;

        // ── Feedback ─────────────────────────────────────────────────────
        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackLabel;       // transient status messages
        [SerializeField] private float feedbackDisplayDuration = 3f;
        private Coroutine _feedbackCoroutine;

        // ── Price strings (matches store listings) ───────────────────────
        private const string PRICE_PATRON      = "$2.99/mo";
        private const string PRICE_SIGIL_SKIN  = "$1.99";
        private const string PRICE_CONSTEL     = "$4.99";
        private const string PRICE_ARCHIVE     = "$4.99";
        private const string PRICE_NAME_BUNDLE = "$0.99";
        private const string LABEL_OWNED       = "✦ Owned";

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            WireButtons();

            // Subscribe to purchase events so UI refreshes after purchase
            if (CosmicPatronManager.Instance != null)
            {
                CosmicPatronManager.Instance.OnPurchaseSucceeded += OnPurchaseSucceeded;
                CosmicPatronManager.Instance.OnPurchaseFailed    += OnPurchaseFailed;
            }

            // Start hidden
            if (shopRootPanel != null)
                shopRootPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (CosmicPatronManager.Instance != null)
            {
                CosmicPatronManager.Instance.OnPurchaseSucceeded -= OnPurchaseSucceeded;
                CosmicPatronManager.Instance.OnPurchaseFailed    -= OnPurchaseFailed;
            }
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>Opens the shop and refreshes all product states.</summary>
        public void OpenShop()
        {
            if (shopRootPanel == null) return;
            shopRootPanel.SetActive(true);
            RefreshAllCards();

            if (shopCanvasGroup != null)
            {
                shopCanvasGroup.interactable   = true;
                shopCanvasGroup.blocksRaycasts = true;
                shopCanvasGroup.alpha          = 1f;
            }
        }

        /// <summary>Closes the shop panel.</summary>
        public void CloseShop()
        {
            if (shopRootPanel != null)
                shopRootPanel.SetActive(false);
        }

        // ── Button wiring ─────────────────────────────────────────────────
        private void WireButtons()
        {
            patronBuyButton?       .onClick.AddListener(OnPatronBuyPressed);
            sigilFlameBuyButton?   .onClick.AddListener(OnSigilFlamePressed);
            sigilVerdantBuyButton? .onClick.AddListener(OnSigilVerdantPressed);
            constellationPackBuyButton?.onClick.AddListener(OnConstellationPackPressed);
            seekersArchiveBuyButton?.  onClick.AddListener(OnSeekersArchivePressed);
            cosmicNameBuyButton?   .onClick.AddListener(OnCosmicNamePressed);
            restorePurchasesButton?.onClick.AddListener(OnRestorePressed);
            closeButton?           .onClick.AddListener(CloseShop);
        }

        // ── Card refresh ──────────────────────────────────────────────────
        private void RefreshAllCards()
        {
            var mgr = CosmicPatronManager.Instance;
            if (mgr == null) return;

            // Patron card
            bool isPatron = mgr.IsCosmicPatron;
            if (patronStatusLabel  != null)
                patronStatusLabel.text  = isPatron ? "✦ Active — Cosmic Patron" : "✦ Cosmic Patron";
            if (patronBuyButtonLabel != null)
                patronBuyButtonLabel.text = isPatron ? "Active" : PRICE_PATRON;
            if (patronBuyButton != null)
                patronBuyButton.interactable = !isPatron;
            if (patronPerksPanel != null)
                patronPerksPanel.SetActive(true); // always show perks for transparency
            if (patronBadgePreviewLabel != null)
                patronBadgePreviewLabel.text = isPatron ? mgr.GetPatronBadge() : "[Cosmic Patron]";

            // Cosmetic cards
            RefreshCosmeticCard(sigilFlameBuyButton,         sigilFlameStatusLabel,        mgr.HasSigilFlame,        PRICE_SIGIL_SKIN);
            RefreshCosmeticCard(sigilVerdantBuyButton,       sigilVerdantStatusLabel,      mgr.HasSigilVerdant,      PRICE_SIGIL_SKIN);
            RefreshCosmeticCard(constellationPackBuyButton,  constellationPackStatusLabel, mgr.HasConstellationPack, PRICE_CONSTEL);
            RefreshCosmeticCard(seekersArchiveBuyButton,     seekersArchiveStatusLabel,    mgr.HasSeekersArchive,    PRICE_ARCHIVE);
            RefreshCosmeticCard(cosmicNameBuyButton,         cosmicNameStatusLabel,        mgr.HasCosmicNameBundle,  PRICE_NAME_BUNDLE);
        }

        private static void RefreshCosmeticCard(Button btn, TextMeshProUGUI label, bool owned, string price)
        {
            if (btn   != null) btn.interactable = !owned;
            if (label != null) label.text       = owned ? LABEL_OWNED : price;
        }

        // ── Button handlers ───────────────────────────────────────────────
        private void OnPatronBuyPressed()
        {
            ShowFeedback("Starting purchase…");
            CosmicPatronManager.Instance?.PurchaseCosmicPatron();
        }

        private void OnSigilFlamePressed()
        {
            ShowFeedback("Starting purchase…");
            CosmicPatronManager.Instance?.PurchaseSigilFlame();
        }

        private void OnSigilVerdantPressed()
        {
            ShowFeedback("Starting purchase…");
            CosmicPatronManager.Instance?.PurchaseSigilVerdant();
        }

        private void OnConstellationPackPressed()
        {
            ShowFeedback("Starting purchase…");
            CosmicPatronManager.Instance?.PurchaseConstellationPack();
        }

        private void OnSeekersArchivePressed()
        {
            ShowFeedback("Starting purchase…");
            CosmicPatronManager.Instance?.PurchaseSeekersArchive();
        }

        private void OnCosmicNamePressed()
        {
            ShowFeedback("Starting purchase…");
            CosmicPatronManager.Instance?.PurchaseCosmicNameBundle();
        }

        private void OnRestorePressed()
        {
            ShowFeedback("Restoring purchases…");
            CosmicPatronManager.Instance?.RestorePurchases();
        }

        // ── Purchase callbacks ────────────────────────────────────────────
        private void OnPurchaseSucceeded(string productId)
        {
            RefreshAllCards();
            ShowFeedback("✦ Purchase complete! Thank you for supporting the realms.");
        }

        private void OnPurchaseFailed(string productId)
        {
            ShowFeedback("Purchase could not be completed. Please try again.");
        }

        // ── Feedback helper ───────────────────────────────────────────────
        private void ShowFeedback(string message)
        {
            if (feedbackLabel == null) return;
            feedbackLabel.text = message;
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(ClearFeedbackAfterDelay());
        }

        private IEnumerator ClearFeedbackAfterDelay()
        {
            yield return new WaitForSeconds(feedbackDisplayDuration);
            if (feedbackLabel != null) feedbackLabel.text = string.Empty;
        }

        // ── Accessibility ─────────────────────────────────────────────────

        /// <summary>
        /// Summarises all active patron perks as a screen-reader-friendly string.
        /// </summary>
        public string GetShopAccessibilitySummary()
        {
            var mgr = CosmicPatronManager.Instance;
            if (mgr == null) return "Shop unavailable.";
            return mgr.IsCosmicPatron
                ? "You are an active Cosmic Patron. Bonus daily challenge slot and patron badge are active."
                : $"Cosmic Patron subscription is {PRICE_PATRON} per month. Grants a bonus daily challenge slot, a patron badge in Kindness Chain, and an extra emoji row on ritual share cards.";
        }
    }
}
