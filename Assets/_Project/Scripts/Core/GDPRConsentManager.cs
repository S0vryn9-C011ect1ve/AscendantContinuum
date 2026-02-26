using System;
using UnityEngine;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Manages GDPR/CCPA consent state.
    /// Must be checked before initializing analytics or backend services.
    /// </summary>
    public class GDPRConsentManager : MonoBehaviour
    {
        public static GDPRConsentManager Instance { get; private set; }

        private const string ConsentKey = "GDPR_Consent_Given";
        private const string AnalyticsConsentKey = "Analytics_Consent_Given";

        public event Action OnConsentGiven;
        public event Action OnConsentRevoked;

        [SerializeField] private bool requireConsentInEditor = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Checks if the user has already responded to the consent prompt.
        /// </summary>
        public bool HasRespondedToConsent()
        {
#if UNITY_EDITOR
            if (!requireConsentInEditor) return true;
#endif
            return PlayerPrefs.HasKey(ConsentKey);
        }

        /// <summary>
        /// Checks if the user has given consent for data collection.
        /// </summary>
        public bool HasConsent()
        {
#if UNITY_EDITOR
            if (!requireConsentInEditor) return true;
#endif
            return PlayerPrefs.GetInt(ConsentKey, 0) == 1;
        }

        /// <summary>
        /// Checks if the user has given specific consent for analytics.
        /// </summary>
        public bool HasAnalyticsConsent()
        {
#if UNITY_EDITOR
            if (!requireConsentInEditor) return true;
#endif
            return PlayerPrefs.GetInt(AnalyticsConsentKey, 0) == 1;
        }

        /// <summary>
        /// Grants consent and fires the OnConsentGiven event.
        /// </summary>
        public void GrantConsent(bool allowAnalytics = true)
        {
            PlayerPrefs.SetInt(ConsentKey, 1);
            PlayerPrefs.SetInt(AnalyticsConsentKey, allowAnalytics ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log($"[GDPRConsentManager] Consent granted. Analytics: {allowAnalytics}");
            OnConsentGiven?.Invoke();
        }

        /// <summary>
        /// Revokes consent and fires the OnConsentRevoked event.
        /// </summary>
        public void RevokeConsent()
        {
            PlayerPrefs.SetInt(ConsentKey, 0);
            PlayerPrefs.SetInt(AnalyticsConsentKey, 0);
            PlayerPrefs.Save();

            Debug.Log("[GDPRConsentManager] Consent revoked.");
            OnConsentRevoked?.Invoke();
        }

        /// <summary>
        /// Clears the consent state entirely (useful for testing or account deletion).
        /// </summary>
        public void ClearConsentState()
        {
            PlayerPrefs.DeleteKey(ConsentKey);
            PlayerPrefs.DeleteKey(AnalyticsConsentKey);
            PlayerPrefs.Save();
            Debug.Log("[GDPRConsentManager] Consent state cleared.");
        }
    }
}
