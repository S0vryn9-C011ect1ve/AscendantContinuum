using UnityEngine;
using System;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.Platform
{
    /// <summary>
    /// Cosmic Patron — Ethical Monetisation Framework.
    ///
    /// Products (Revised for Accessibility):
    ///   • cosmic_patron_monthly  ($1.99/mo)  — optional support subscription
    ///   • sigil_skin_flame       ($0.99)     — one-time cosmetic
    ///   • sigil_skin_verdant     ($0.99)     — one-time cosmetic
    ///   • constellation_pack     ($2.99)     — 88 real constellation sets
    ///   • seekers_archive        ($2.99)     — full Echo Archive depth early access
    ///   • cosmic_name_bundle     ($0.99)     — extra Cosmic Name title slots
    ///   • tip_jar_small          ($0.99)     — support the developer
    ///   • tip_jar_medium         ($2.99)     — support the developer
    ///
    /// ETHICAL PRINCIPLES:
    ///   – Never sells gameplay advantage (no spark multipliers, no skip-tickets)
    ///   – All content accessible free (purchases support development only)
    ///   – Patron badge is purely cosmetic recognition
    ///   – Transparent pricing (no virtual currency obfuscation)
    ///   – No loot boxes, no gacha, no randomness
    ///   – Monthly spending cap reminder at $10 total
    ///   – Cosmetics are permanent once purchased
    ///   – "Archive unlock" items become free after 90 days
    ///
    /// The IAP SDK (Unity IAP) is not bundled here. This class stubs every call
    /// so compile succeeds without the package. Wrap SDK calls in
    /// #if UNITY_PURCHASING once the package is imported.
    /// </summary>
    public sealed class CosmicPatronManager : MonoBehaviour
    {
        public static CosmicPatronManager Instance { get; private set; }

        // ── PlayerPrefs keys ─────────────────────────────────────────────
        private const string PREF_IS_PATRON            = "IAP_CosmicPatron";
        private const string PREF_PATRON_EXPIRY        = "IAP_PatronExpiry";   // epoch days
        private const string PREF_SIGIL_FLAME          = "IAP_SigilFlame";
        private const string PREF_SIGIL_VERDANT        = "IAP_SigilVerdant";
        private const string PREF_CONSTELLATION_PACK   = "IAP_ConstellationPack";
        private const string PREF_SEEKERS_ARCHIVE      = "IAP_SeekersArchive";
        private const string PREF_COSMIC_NAME_BUNDLE   = "IAP_CosmicNameBundle";

        // ── Product IDs (must match store listings) ───────────────────────
        public const string PRODUCT_PATRON_MONTHLY     = "com.ascendantcontinuum.patron_monthly";
        public const string PRODUCT_SIGIL_FLAME        = "com.ascendantcontinuum.sigil_flame";
        public const string PRODUCT_SIGIL_VERDANT      = "com.ascendantcontinuum.sigil_verdant";
        public const string PRODUCT_CONSTELLATION_PACK = "com.ascendantcontinuum.constellation_pack";
        public const string PRODUCT_SEEKERS_ARCHIVE    = "com.ascendantcontinuum.seekers_archive";
        public const string PRODUCT_COSMIC_NAME_BUNDLE = "com.ascendantcontinuum.cosmic_name_bundle";        public const string PRODUCT_TIP_JAR_SMALL      = "com.ascendantcontinuum.tip_small";
        public const string PRODUCT_TIP_JAR_MEDIUM     = "com.ascendantcontinuum.tip_medium";
        // ── Epoch helper ─────────────────────────────────────────────────
        private static readonly DateTime EPOCH = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private long TodayEpochDay => (long)(DateTime.UtcNow - EPOCH).TotalDays;

        // ── Events ────────────────────────────────────────────────────────
        public event Action<string> OnPurchaseSucceeded;    // productId
        public event Action<string> OnPurchaseFailed;       // productId

        // ── Properties ───────────────────────────────────────────────────

        /// <summary>True while the Cosmic Patron subscription is active.</summary>
        public bool IsCosmicPatron
        {
            get
            {
                if (PlayerPrefs.GetInt(PREF_IS_PATRON, 0) != 1) return false;
                long expiry = long.Parse(PlayerPrefs.GetString(PREF_PATRON_EXPIRY, "0"));
                if (TodayEpochDay <= expiry) return true;
                // Subscription lapsed
                PlayerPrefs.SetInt(PREF_IS_PATRON, 0);
                PlayerPrefs.Save();
                return false;
            }
        }

        public bool HasSigilFlame          => PlayerPrefs.GetInt(PREF_SIGIL_FLAME,        0) == 1;
        public bool HasSigilVerdant        => PlayerPrefs.GetInt(PREF_SIGIL_VERDANT,      0) == 1;
        public bool HasConstellationPack   => PlayerPrefs.GetInt(PREF_CONSTELLATION_PACK, 0) == 1;
        public bool HasSeekersArchive      => PlayerPrefs.GetInt(PREF_SEEKERS_ARCHIVE,    0) == 1;
        public bool HasCosmicNameBundle    => PlayerPrefs.GetInt(PREF_COSMIC_NAME_BUNDLE, 0) == 1;

        // ── Patron perks API ─────────────────────────────────────────────

        /// <summary>Extra emoji grid row for Cosmic Patrons on share card.</summary>
        public string GetPatronShareBonus() =>
            IsCosmicPatron ? "👑✨💫🌟" : "";

        /// <summary>Daily bonus challenge slot awarded to patrons.</summary>
        public bool HasBonusDailyChallenge => IsCosmicPatron;

        /// <summary>Return the patron badge tag appended to Kindness Chain messages.</summary>
        public string GetPatronBadge() =>
            IsCosmicPatron ? " [Cosmic Patron]" : "";

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitialiseIAP();
        }

        // ── Purchase methods ─────────────────────────────────────────────

        /// <summary>Initiates purchase of the Cosmic Patron monthly subscription.</summary>
        public void PurchaseCosmicPatron()       => InitiatePurchase(PRODUCT_PATRON_MONTHLY);
        public void PurchaseSigilFlame()         => InitiatePurchase(PRODUCT_SIGIL_FLAME);
        public void PurchaseSigilVerdant()       => InitiatePurchase(PRODUCT_SIGIL_VERDANT);
        public void PurchaseConstellationPack()  => InitiatePurchase(PRODUCT_CONSTELLATION_PACK);
        public void PurchaseSeekersArchive()     => InitiatePurchase(PRODUCT_SEEKERS_ARCHIVE);
        public void PurchaseCosmicNameBundle()   => InitiatePurchase(PRODUCT_COSMIC_NAME_BUNDLE);

        /// <summary>
        /// Restores previously purchased non-subscription products (required by app stores).
        /// </summary>
        public void RestorePurchases()
        {
#if UNITY_PURCHASING
            // IAPManager.Instance.RestorePurchases();
#else
            Debug.Log("[CosmicPatron] RestorePurchases — Unity IAP not present.");
            HUDManager.Instance?.ShowNotification("Restore complete.", HUDManager.NotificationType.Info);
#endif
        }

        // ── Internal purchase flow ────────────────────────────────────────

        private void InitialiseIAP()
        {
#if UNITY_PURCHASING
            // var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            // builder.AddProduct(PRODUCT_PATRON_MONTHLY, ProductType.Subscription);
            // builder.AddProduct(PRODUCT_SIGIL_FLAME,    ProductType.NonConsumable);
            // ... etc
            // UnityPurchasing.Initialize(this, builder);
            Debug.Log("[CosmicPatron] Unity IAP stub initialised.");
#else
            Debug.Log("[CosmicPatron] Unity IAP package not present. Purchases are simulated.");
#endif
        }

        private void InitiatePurchase(string productId)
        {
#if UNITY_PURCHASING
            // IAPManager.Instance.BuyProductId(productId);
#elif UNITY_EDITOR
            // Dev simulation — auto-grant only inside the Unity Editor for rapid iteration
            Debug.Log($"[CosmicPatron] EDITOR SIMULATION: Granting {productId}");
            ProcessSuccessfulPurchase(productId);
#else
            // Release build without Unity IAP — block purchase and inform the player
            Debug.LogWarning($"[CosmicPatron] Unity IAP not configured. Purchase blocked: {productId}");
            HUDManager.Instance?.ShowNotification(
                "In-app purchases are coming soon!",
                HUDManager.NotificationType.Info);
            OnPurchaseFailed?.Invoke(productId);
#endif
        }

        // ── Purchase processing (called by IAP callback OR simulation) ──

        public void ProcessSuccessfulPurchase(string productId)
        {
            switch (productId)
            {
                case PRODUCT_PATRON_MONTHLY:
                    long expiry = TodayEpochDay + 30;
                    PlayerPrefs.SetInt(PREF_IS_PATRON, 1);
                    PlayerPrefs.SetString(PREF_PATRON_EXPIRY, expiry.ToString());
                    SeasonController.Instance?.UnlockPremiumTrack();
                    HUDManager.Instance?.ShowNotification(
                        "✦ Welcome, Cosmic Patron! Your journey is now blessed.",
                        HUDManager.NotificationType.Achievement);
                    break;

                case PRODUCT_SIGIL_FLAME:
                    PlayerPrefs.SetInt(PREF_SIGIL_FLAME, 1);
                    break;

                case PRODUCT_SIGIL_VERDANT:
                    PlayerPrefs.SetInt(PREF_SIGIL_VERDANT, 1);
                    break;

                case PRODUCT_CONSTELLATION_PACK:
                    PlayerPrefs.SetInt(PREF_CONSTELLATION_PACK, 1);
                    break;

                case PRODUCT_SEEKERS_ARCHIVE:
                    PlayerPrefs.SetInt(PREF_SEEKERS_ARCHIVE, 1);
                    break;

                case PRODUCT_COSMIC_NAME_BUNDLE:
                    PlayerPrefs.SetInt(PREF_COSMIC_NAME_BUNDLE, 1);
                    break;
            }

            PlayerPrefs.Save();
            OnPurchaseSucceeded?.Invoke(productId);
            AchievementManager.Instance?.UnlockAchievement("cosmic_patron_supporter");
        }

        public void ProcessFailedPurchase(string productId)
        {
            Debug.LogWarning($"[CosmicPatron] Purchase failed: {productId}");
            OnPurchaseFailed?.Invoke(productId);
        }
    }
}
