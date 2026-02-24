using UnityEngine;
using System;
using System.Collections;
using AscendantContinuum.Core;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// The Serendipity System — ultra-rare screenshot-worthy moments that make
    /// players stop and share. Triggered probabilistically on ritual completions.
    ///
    /// Tiers (per the viral mechanics design doc):
    ///   Legendary  1%  – Full deity appearance, unique interaction
    ///   Epic        4%  – Sigil Aurora (entire realm transforms)
    ///   Rare       10%  – Divine Blessing (beam of light, boosted particles)
    ///   Uncommon   25%  – Enhanced Effects (extra sparkles, special sounds)
    ///   Common     ~60% – No serendipity event
    ///
    /// Results are fed into:
    ///   • AchievementManager  (first_serendipity, rare_encounter, deity_touched)
    ///   • CosmicIdentitySystem (aura tier progress)
    ///   • HUDManager notifications
    ///   • Firebase analytics (so we can track how often these fire)
    /// </summary>
    public sealed class SerendipityManager : MonoBehaviour
    {
        public static SerendipityManager Instance { get; private set; }

        // ── Events ─────────────────────────────────────────────────────────
        public event Action<SerendipityTier, string> OnSerendipityTriggered;

        // ── Inspector ──────────────────────────────────────────────────────
        [Header("Tier Probabilities")]
        [SerializeField, Range(0f, 1f)] private float legendaryChance  = 0.01f;  // 1%
        [SerializeField, Range(0f, 1f)] private float epicChance       = 0.04f;  // 4%
        [SerializeField, Range(0f, 1f)] private float rareChance       = 0.10f;  // 10%
        [SerializeField, Range(0f, 1f)] private float uncommonChance   = 0.25f;  // 25%

        [Header("Cooldowns (seconds)")]
        [SerializeField] private float legendaryCooldown  = 86400f; // 24 h
        [SerializeField] private float epicCooldown       = 3600f;  // 1 h
        [SerializeField] private float rareCooldown       = 600f;   // 10 min
        [SerializeField] private float uncommonCooldown   = 60f;    // 1 min

        [Header("Visual Prefabs (assign in Inspector)")]
        [SerializeField] private GameObject deityAppearancePrefab;
        [SerializeField] private GameObject sigilAuroraPrefab;
        [SerializeField] private GameObject divineBlessingPrefab;
        [SerializeField] private GameObject enhancedEffectsPrefab;

        // ── State ──────────────────────────────────────────────────────────
        private float _lastLegendaryTime  = float.MinValue;
        private float _lastEpicTime       = float.MinValue;
        private float _lastRareTime       = float.MinValue;
        private float _lastUncommonTime   = float.MinValue;

        private static readonly string[] DEITY_NAMES =
        {
            "Ascendant Flame",
            "Herald of Joyful Curiosity",
            "Archivist of Bright Memories",
            "Mechanic of Helpful Wonders",
            "Weaver of the Living Root",
            "Keeper of Infinite Light"
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
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            // Hook into EmberforgeSparks collection events
            var sparks = FindFirstObjectByType<Emberforge.EmberforgeSparks>();
            if (sparks != null)
                sparks.OnSparkCollected += _ => TryTrigger("Emberforge");

            // Hook into DailyChallenge completion
            if (DailyChallengeManager.Instance != null)
                DailyChallengeManager.Instance.OnChallengeCompleted += _ => TryTrigger("DailyChallenge");
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Adds a flat bonus to the legendary-tier chance.
        /// Called by <see cref="PantheonDeityEffects"/> when the Herald of Joyful
        /// Curiosity is active (+5%).  Clamped to [0, 0.25] to prevent runaway odds.
        /// </summary>
        public void AddLegendaryChanceBonus(float bonus)
        {
            legendaryChance = Mathf.Clamp(legendaryChance + bonus, 0f, 0.25f);
            Debug.Log($"[Serendipity] Legendary chance adjusted to {legendaryChance:P0} (Herald bonus).");
        }

        /// <summary>
        /// Call this whenever any ritual-like action completes. Rolls the dice
        /// and fires the appropriate serendipity tier.
        /// </summary>
        public void TryTrigger(string contextRealm = "Unknown")
        {
            float roll = UnityEngine.Random.value;

            if (roll < legendaryChance && CanFire(SerendipityTier.Legendary))
            {
                StartCoroutine(FireLegendary(contextRealm));
            }
            else if (roll < legendaryChance + epicChance && CanFire(SerendipityTier.Epic))
            {
                StartCoroutine(FireEpic(contextRealm));
            }
            else if (roll < legendaryChance + epicChance + rareChance && CanFire(SerendipityTier.Rare))
            {
                StartCoroutine(FireRare(contextRealm));
            }
            else if (roll < legendaryChance + epicChance + rareChance + uncommonChance && CanFire(SerendipityTier.Uncommon))
            {
                FireUncommon(contextRealm);
            }
        }

        // ── Tier implementations ───────────────────────────────────────────

        private IEnumerator FireLegendary(string realm)
        {
            _lastLegendaryTime = Time.realtimeSinceStartup;
            string deityName = DEITY_NAMES[UnityEngine.Random.Range(0, DEITY_NAMES.Length)];

            Debug.Log($"[Serendipity] ✨ LEGENDARY — {deityName} appears in {realm}!");

            // Slow time briefly for impact
            Time.timeScale = 0.25f;
            yield return new WaitForSecondsRealtime(0.5f);
            Time.timeScale = 1f;

            // Spawn visual prefab at camera centre
            if (deityAppearancePrefab != null)
            {
                var go = Instantiate(deityAppearancePrefab, GetScreenCentre(), Quaternion.identity);
                Destroy(go, 10f);
            }

            // HUD notifications
            UI.HUDManager.Instance?.ShowNotification(
                $"✨ {deityName} appears before you! A rare blessing descends.",
                UI.HUDManager.NotificationType.Achievement);

            // Announce for screen reader
            AccessibilityManager.Instance?.Announce(
                $"Legendary serendipity: {deityName} appears and grants you a blessing.");

            // Haptic
            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Success);

            // Award divine spark bonus (2× for 5 min)
            PlayerPrefs.SetFloat("Serendipity_SparkBonus", 2f);
            PlayerPrefs.SetInt("Serendipity_BonusExpiry",
                (int)DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds());
            PlayerPrefs.Save();

            // Achievements
            AchievementManager.Instance?.UnlockAchievement("deity_touched");
            AchievementManager.Instance?.TrackProgress("first_serendipity", 1);

            // Cosmic identity
            CosmicIdentitySystem.Instance?.RecordAchievement("deity_touched");

            // Mystery — deity appearance seeds Seventh Realm clue discovery
            MysteryManager.Instance?.DiscoverClue(MysteryManager.MYSTERY_SEVENTH_REALM, realm);

            // Analytics
            Core.FirebaseManager.Instance?.TrackEvent("serendipity_legendary", new System.Collections.Generic.Dictionary<string, object>
            {
                { "deity", deityName }, { "realm", realm }
            });

            OnSerendipityTriggered?.Invoke(SerendipityTier.Legendary, deityName);
        }

        private IEnumerator FireEpic(string realm)
        {
            _lastEpicTime = Time.realtimeSinceStartup;

            Debug.Log($"[Serendipity] 🌌 EPIC — Sigil Aurora transforms {realm}!");

            if (sigilAuroraPrefab != null)
            {
                var go = Instantiate(sigilAuroraPrefab, GetScreenCentre(), Quaternion.identity);
                Destroy(go, 8f);
            }

            // Flash sky tint to aurora colours
            Color auroraColor = new Color(
                UnityEngine.Random.Range(0.4f, 1f),
                UnityEngine.Random.Range(0.2f, 0.8f),
                UnityEngine.Random.Range(0.6f, 1f));
            Color originalAmbient = RenderSettings.ambientLight;

            // Only tint if not in autism mode (avoid surprises)
            bool autismMode = PlayerPrefs.GetInt("Autism_ReduceSurprises", 0) == 1;
            if (!autismMode)
            {
                RenderSettings.ambientLight = auroraColor;
                yield return new WaitForSecondsRealtime(3f);
                RenderSettings.ambientLight = originalAmbient;
            }
            else
            {
                yield return null; // no delay in autism mode
            }

            UI.HUDManager.Instance?.ShowNotification(
                "🌌 Sigil Aurora! The realm transforms with rare light.",
                UI.HUDManager.NotificationType.Achievement);

            AccessibilityManager.Instance?.Announce(
                "Epic serendipity: the Sigil Aurora fills the realm with beautiful colours.");

            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Medium);

            AchievementManager.Instance?.TrackProgress("first_serendipity", 1);

            Core.FirebaseManager.Instance?.TrackEvent("serendipity_epic",
                new System.Collections.Generic.Dictionary<string, object> { { "realm", realm } });

            OnSerendipityTriggered?.Invoke(SerendipityTier.Epic, "Sigil Aurora");
        }

        private IEnumerator FireRare(string realm)
        {
            _lastRareTime = Time.realtimeSinceStartup;

            Debug.Log($"[Serendipity] 💫 RARE — Divine Blessing in {realm}!");

            if (divineBlessingPrefab != null)
            {
                var go = Instantiate(divineBlessingPrefab, GetScreenCentre(), Quaternion.identity);
                Destroy(go, 5f);
            }

            // Small temporary spark bonus (1.5× for 2 min)
            PlayerPrefs.SetFloat("Serendipity_SparkBonus", 1.5f);
            PlayerPrefs.SetInt("Serendipity_BonusExpiry",
                (int)DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds());
            PlayerPrefs.Save();

            UI.HUDManager.Instance?.ShowNotification(
                "💫 A divine spark blesses you! Enhanced power for 2 minutes.",
                UI.HUDManager.NotificationType.Reward);

            AccessibilityManager.Instance?.TriggerHaptic(HapticType.Light);

            AchievementManager.Instance?.TrackProgress("first_serendipity", 1);

            Core.FirebaseManager.Instance?.TrackEvent("serendipity_rare",
                new System.Collections.Generic.Dictionary<string, object> { { "realm", realm } });

            yield return null;
            OnSerendipityTriggered?.Invoke(SerendipityTier.Rare, "Divine Blessing");
        }

        private void FireUncommon(string realm)
        {
            _lastUncommonTime = Time.realtimeSinceStartup;

            Debug.Log($"[Serendipity] ✨ UNCOMMON — Enhanced effects in {realm}.");

            if (enhancedEffectsPrefab != null)
            {
                var go = Instantiate(enhancedEffectsPrefab, GetScreenCentre(), Quaternion.identity);
                Destroy(go, 3f);
            }

            VFX.ParticleManager.Instance?.SetMotionScale(2f);
            // Reset particle scale after 3 seconds
            StartCoroutine(ResetParticleScale(3f));

            OnSerendipityTriggered?.Invoke(SerendipityTier.Uncommon, "Enhanced Effects");
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private bool CanFire(SerendipityTier tier)
        {
            float elapsed = Time.realtimeSinceStartup;
            switch (tier)
            {
                case SerendipityTier.Legendary: return elapsed - _lastLegendaryTime >= legendaryCooldown;
                case SerendipityTier.Epic:      return elapsed - _lastEpicTime      >= epicCooldown;
                case SerendipityTier.Rare:      return elapsed - _lastRareTime      >= rareCooldown;
                case SerendipityTier.Uncommon:  return elapsed - _lastUncommonTime  >= uncommonCooldown;
                default: return false;
            }
        }

        private static Vector3 GetScreenCentre()
        {
            if (Camera.main == null) return Vector3.zero;
            return Camera.main.transform.position + Camera.main.transform.forward * 2f;
        }

        private IEnumerator ResetParticleScale(float delay)
        {
            yield return new WaitForSeconds(delay);
            VFX.ParticleManager.Instance?.SetMotionScale(1f);
        }

        /// <summary>Returns the active serendipity spark bonus (1× if none).</summary>
        public float GetActiveSparkBonus()
        {
            int expiry = PlayerPrefs.GetInt("Serendipity_BonusExpiry", 0);
            if (expiry == 0) return 1f;
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry)
            {
                PlayerPrefs.DeleteKey("Serendipity_BonusExpiry");
                PlayerPrefs.DeleteKey("Serendipity_SparkBonus");
                PlayerPrefs.Save();
                return 1f;
            }
            return PlayerPrefs.GetFloat("Serendipity_SparkBonus", 1f);
        }
    }

    public enum SerendipityTier
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
