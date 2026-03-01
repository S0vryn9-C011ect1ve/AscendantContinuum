using UnityEngine;
using System;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Sigil Crafting System — combines collected sigils into new discoveries.
    ///
    /// The first player to craft a specific combination gets to NAME it globally.
    /// All combination names are stored locally (PlayerPrefs) and queued to
    /// Firebase for cross-player global registry.
    ///
    /// ~1,000 possible combinations exist (most unknown at launch).
    /// Rare variants (Glimmering, Radiant, Ancient, Perfect, Cosmic) are awarded
    /// on a per-craft RNG roll.
    ///
    /// Integrates with SigilGenerator for the personal-sigil generation pipeline.
    /// </summary>
    public sealed class SigilCraftingManager : MonoBehaviour
    {
        public static SigilCraftingManager Instance { get; private set; }

        // ── PlayerPrefs keys ──────────────────────────────────────────────
        private const string PREF_CRAFTED_PREFIX  = "Craft_Result_";
        private const string PREF_NAMED_PREFIX    = "Craft_Name_";
        private const string PREF_REGISTRY_COUNT  = "Craft_RegistryCount";
        private const string PREF_TOTAL_CRAFTED   = "Craft_TotalCrafted";
        private const string PREF_HALL_OF_FAME    = "Craft_HallOfFame_";

        // ── Known combinations table (sigil1 + sigil2 → result name, base) ─
        private static readonly Dictionary<string, (string resultId, string defaultName)> KNOWN_COMBOS =
            new Dictionary<string, (string, string)>
        {
            { "double_flame+glowing_thread",  ("forgeweaver",      "Forgeweaver")     },
            { "blooming_loop+spiral_rune",    ("memory_garden",    "Memory Garden")   },
            { "radiant_star+lantern_orb",     ("guiding_light",    "Guiding Light")   },
            { "double_flame+blooming_loop",   ("ember_bloom",      "Ember Bloom")     },
            { "spiral_rune+radiant_star",     ("celestial_map",    "Celestial Map")   },
            { "lantern_orb+glowing_thread",   ("drifting_lumen",   "Drifting Lumen")  },
            { "infinite_knot+playful_spark",  ("chaos_weave",      "Chaos Weave")     },
            { "double_flame+infinite_knot",   ("forge_eternal",    "Forge Eternal")   },
            { "blooming_loop+lantern_orb",    ("night_bloom",      "Night Bloom")     },
            { "spiral_rune+glowing_thread",   ("starthread",       "Starthread")      },
            { "radiant_star+playful_spark",   ("dawn_spark",       "Dawn Spark")      },
            { "blooming_loop+infinite_knot",  ("living_knot",      "Living Knot")     },
        };

        // ── Variant rarity weights ─────────────────────────────────────────
        private static readonly (string variant, float probability)[] VARIANTS =
        {
            ("Cosmic",     0.005f),
            ("Perfect",    0.01f),
            ("Ancient",    0.02f),
            ("Radiant",    0.05f),
            ("Glimmering", 0.10f),
        };

        // ── Events ────────────────────────────────────────────────────────
        public event Action<string, string, bool> OnSigilCrafted;  // resultId, name, isFirstDiscovery

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Crafting ──────────────────────────────────────────────────────

        /// <summary>
        /// Attempt to craft a sigil from two source sigil IDs.
        /// Returns the result sigil ID on success, or null on invalid combination.
        /// </summary>
        public string TryCraftSigil(string sigilA, string sigilB)
        {
            string key = BuildKey(sigilA, sigilB);
            if (!KNOWN_COMBOS.ContainsKey(key))
            {
                // Whimsical failure
                HUDManager.Instance?.ShowNotification(
                    "✧ These energies don't quite align... try another combination.",
                    HUDManager.NotificationType.Info);
                return null;
            }

            var (resultId, defaultName) = KNOWN_COMBOS[key];
            bool isNew = IsPlayerFirstDiscovery(key);
            string variantSuffix = RollVariant();
            string finalName = GetCombinationName(key, defaultName);
            string displayName = variantSuffix.Length > 0 ? $"{variantSuffix} {finalName}" : finalName;

            if (isNew)
            {
                bool isCommunityFirst = IsCommunityFirstDiscovery(key);
                RecordDiscovery(key, resultId, defaultName);

                if (isCommunityFirst)
                {
                    RegisterCommunityDiscovery(key, resultId, defaultName);
                    PromptNaming(key, resultId, defaultName);
                    AchievementManager.Instance?.UnlockAchievement("first_namer");
                    HUDManager.Instance?.ShowNotification(
                        $"✦ YOU ARE THE FIRST to discover \"{defaultName}\"! Name this ritual.",
                        HUDManager.NotificationType.Achievement);
                }
                else
                {
                    HUDManager.Instance?.ShowNotification(
                        $"✨ New sigil crafted: {displayName}",
                        HUDManager.NotificationType.Achievement);
                }

                int total = PlayerPrefs.GetInt(PREF_TOTAL_CRAFTED, 0) + 1;
                PlayerPrefs.SetInt(PREF_TOTAL_CRAFTED, total);
                if (total >= 50) AchievementManager.Instance?.UnlockAchievement("sigil_master");
            }
            else
            {
                HUDManager.Instance?.ShowNotification(
                    $"✨ Crafted: {displayName}",
                    HUDManager.NotificationType.Info);
            }

            OnSigilCrafted?.Invoke(resultId, displayName, isNew);
            GameEvents.RaiseSigilCrafted(resultId, displayName, isNew);
            return resultId;
        }

        /// <summary>Save a community-assigned name for a combination (after first-discoverer prompt).</summary>
        public void NameCombination(string comboKey, string chosenName)
        {
            PlayerPrefs.SetString(PREF_NAMED_PREFIX + comboKey, chosenName);
            PlayerPrefs.Save();

            // Sync to Firebase global registry
            FirebaseManager.Instance?.SaveData("sigilRegistry", comboKey,
                new { combo = comboKey, name = chosenName, discoverer = PlayerPrefs.GetInt("Profile_SeekerId", 0) });

            HUDManager.Instance?.ShowNotification(
                $"✦ \"{chosenName}\" is now known globally to all seekers.",
                HUDManager.NotificationType.Achievement);
        }

        /// <summary>Returns the community-named label for a combo key, or the default name.</summary>
        public string GetCombinationName(string comboKey, string fallback) =>
            PlayerPrefs.GetString(PREF_NAMED_PREFIX + comboKey, fallback);

        public int TotalCrafted => PlayerPrefs.GetInt(PREF_TOTAL_CRAFTED, 0);

        // ── Private helpers ───────────────────────────────────────────────

        private string BuildKey(string a, string b)
        {
            // Canonical ordering ensures "A+B" == "B+A"
            string[] ids = new[] { a.ToLower().Replace(" ", "_"), b.ToLower().Replace(" ", "_") };
            Array.Sort(ids);
            return ids[0] + "+" + ids[1];
        }

        private bool IsPlayerFirstDiscovery(string key) =>
            PlayerPrefs.GetInt(PREF_CRAFTED_PREFIX + key, 0) == 0;

        private bool IsCommunityFirstDiscovery(string key) =>
            PlayerPrefs.GetInt(PREF_NAMED_PREFIX + key + "_Global", 0) == 0;

        private void RecordDiscovery(string key, string resultId, string name)
        {
            PlayerPrefs.SetInt(PREF_CRAFTED_PREFIX + key, 1);
            PlayerPrefs.Save();
        }

        private void RegisterCommunityDiscovery(string key, string resultId, string defaultName)
        {
            PlayerPrefs.SetInt(PREF_NAMED_PREFIX + key + "_Global", 1);
            int count = PlayerPrefs.GetInt(PREF_REGISTRY_COUNT, 0) + 1;
            PlayerPrefs.SetInt(PREF_REGISTRY_COUNT, count);
            PlayerPrefs.SetString(PREF_HALL_OF_FAME + count, key + "|" + defaultName);
            PlayerPrefs.Save();
        }

        private void PromptNaming(string key, string resultId, string defaultName)
        {
            // In a full implementation this opens a naming UI
            // For now, auto-name with default and mark as community-named
            NameCombination(key, defaultName);
        }

        private string RollVariant()
        {
            float roll = UnityEngine.Random.value;
            float cumulative = 0f;
            foreach (var (variant, prob) in VARIANTS)
            {
                cumulative += prob;
                if (roll < cumulative) return variant;
            }
            return "";  // no variant (most common)
        }
    }
}
