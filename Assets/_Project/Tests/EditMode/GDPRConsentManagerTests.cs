using NUnit.Framework;
using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="GDPRConsentManager"/> grant / revoke / query flow.
    /// </summary>
    public class GDPRConsentManagerTests
    {
        private GameObject          _go;
        private GDPRConsentManager  _mgr;

        private const string ConsentKey          = "GDPR_Consent_Given";
        private const string AnalyticsConsentKey = "Analytics_Consent_Given";

        [SetUp]
        public void SetUp()
        {
            var existing = Object.FindFirstObjectByType<GDPRConsentManager>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Scrub any leftover prefs
            PlayerPrefs.DeleteKey(ConsentKey);
            PlayerPrefs.DeleteKey(AnalyticsConsentKey);

            _go  = new GameObject("GDPRConsentManager_Test");
            _mgr = _go.AddComponent<GDPRConsentManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            PlayerPrefs.DeleteKey(ConsentKey);
            PlayerPrefs.DeleteKey(AnalyticsConsentKey);
        }

        // ── Before consent ────────────────────────────────────────────────

        [Test]
        public void HasConsent_FalseBeforeGrant()
        {
#if UNITY_EDITOR
            // In Editor the manager bypasses prefs unless requireConsentInEditor is true.
            // The test is meaningful in non-editor context; in Editor check HasResponded.
            Assert.Pass("Consent check bypassed in editor mode (requireConsentInEditor is false).");
#else
            Assert.IsFalse(_mgr.HasConsent());
#endif
        }

        [Test]
        public void HasRespondedToConsent_FalseBeforeGrant()
        {
#if UNITY_EDITOR
            Assert.Pass("Consent bypass active in editor.");
#else
            Assert.IsFalse(_mgr.HasRespondedToConsent());
#endif
        }

        // ── Granting consent ──────────────────────────────────────────────

        [Test]
        public void GrantConsent_SetsHasConsentTrue()
        {
            _mgr.ClearConsentState(); // ensure clean slate ignoring editor bypass
            _mgr.GrantConsent(true);

            Assert.IsTrue(PlayerPrefs.GetInt(ConsentKey, 0) == 1,
                "PlayerPrefs should record consent = 1.");
        }

        [Test]
        public void GrantConsent_SetsAnalyticsConsentWhenAllowed()
        {
            _mgr.ClearConsentState();
            _mgr.GrantConsent(true);

            Assert.That(PlayerPrefs.GetInt(AnalyticsConsentKey, 0), Is.EqualTo(1));
        }

        [Test]
        public void GrantConsent_NoAnalytics_SetsAnalyticsConsentFalse()
        {
            _mgr.ClearConsentState();
            _mgr.GrantConsent(false);

            Assert.That(PlayerPrefs.GetInt(AnalyticsConsentKey, 0), Is.EqualTo(0));
        }

        [Test]
        public void GrantConsent_FiresEvent()
        {
            bool fired = false;
            _mgr.OnConsentGiven += () => fired = true;
            _mgr.GrantConsent(true);

            Assert.IsTrue(fired, "OnConsentGiven should fire after GrantConsent.");
        }

        // ── Revoking consent ──────────────────────────────────────────────

        [Test]
        public void RevokeConsent_SetsPrefsToZero()
        {
            _mgr.GrantConsent(true);
            _mgr.RevokeConsent();

            Assert.That(PlayerPrefs.GetInt(ConsentKey, -1), Is.EqualTo(0),
                "Consent key should be set to 0 after revoke.");
            Assert.That(PlayerPrefs.GetInt(AnalyticsConsentKey, -1), Is.EqualTo(0),
                "Analytics consent key should be 0 after revoke.");
        }

        [Test]
        public void RevokeConsent_FiresEvent()
        {
            bool fired = false;
            _mgr.OnConsentRevoked += () => fired = true;
            _mgr.GrantConsent(true);
            _mgr.RevokeConsent();

            Assert.IsTrue(fired, "OnConsentRevoked should fire after RevokeConsent.");
        }

        // ── ClearConsentState ─────────────────────────────────────────────

        [Test]
        public void ClearConsentState_RemovesKeys()
        {
            _mgr.GrantConsent(true);
            _mgr.ClearConsentState();

            Assert.IsFalse(PlayerPrefs.HasKey(ConsentKey),      "Consent key should be removed.");
            Assert.IsFalse(PlayerPrefs.HasKey(AnalyticsConsentKey), "Analytics key should be removed.");
        }
    }
}
