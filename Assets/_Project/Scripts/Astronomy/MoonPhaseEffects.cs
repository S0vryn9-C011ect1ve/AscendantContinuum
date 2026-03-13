using UnityEngine;
using UnityEngine.Rendering.Universal;
using AscendantContinuum.Core;
using AscendantContinuum.UI;

namespace AscendantContinuum.Astronomy
{
    /// <summary>
    /// Applies moon phase effects to gameplay and visuals
    /// Different moon phases unlock different secrets and affect game mechanics
    /// </summary>
    public class MoonPhaseEffects : MonoBehaviour
    {
        public static MoonPhaseEffects Instance { get; private set; }

        [Header("Current Effects")]
        [SerializeField] private float sparkMultiplier = 1f;
        [SerializeField] private float plantGrowthMultiplier = 1f;
        [SerializeField] private Color currentMoonlight = Color.white;
        
        [Header("Visual")]
        [SerializeField] private SpriteRenderer moonSprite;
        [SerializeField] private Sprite[] moonPhaseSprites; // 8 sprites for 8 phases
        [SerializeField] private Light2D moonLight; // If using 2D lights
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        private void Start()
        {
            if (CosmicDataManager.Instance != null)
            {
                CosmicDataManager.Instance.OnMoonPhaseChanged += ApplyMoonPhaseEffects;
                ApplyMoonPhaseEffects(CosmicDataManager.Instance.CurrentMoonPhase);
            }
        }

        private void ApplyMoonPhaseEffects(MoonPhase phase)
        {
            Debug.Log($"[MoonPhaseEffects] Applying effects for {phase}");
            
            switch (phase)
            {
                case MoonPhase.NewMoon:
                    // Darkness reveals hidden paths
                    sparkMultiplier = 0.5f;
                    plantGrowthMultiplier = 1.5f; // Best time to plant
                    currentMoonlight = new Color(0.1f, 0.1f, 0.2f);
                    EnableNewMoonSecrets();
                    break;
                    
                case MoonPhase.WaxingCrescent:
                    sparkMultiplier = 0.75f;
                    plantGrowthMultiplier = 1.3f;
                    currentMoonlight = new Color(0.3f, 0.3f, 0.4f);
                    break;
                    
                case MoonPhase.FirstQuarter:
                    sparkMultiplier = 1f;
                    plantGrowthMultiplier = 1f;
                    currentMoonlight = new Color(0.5f, 0.5f, 0.6f);
                    EnableBalanceSecrets(); // Half-light reveals balance puzzles
                    break;
                    
                case MoonPhase.WaxingGibbous:
                    sparkMultiplier = 1.25f;
                    plantGrowthMultiplier = 0.9f;
                    currentMoonlight = new Color(0.7f, 0.7f, 0.8f);
                    break;
                    
                case MoonPhase.FullMoon:
                    // Maximum power!
                    sparkMultiplier = 2f;
                    plantGrowthMultiplier = 0.8f;
                    currentMoonlight = new Color(0.9f, 0.9f, 1f);
                    EnableFullMoonSecrets();
                    TriggerFullMoonEvent();
                    break;
                    
                case MoonPhase.WaningGibbous:
                    sparkMultiplier = 1.25f;
                    plantGrowthMultiplier = 0.9f;
                    currentMoonlight = new Color(0.7f, 0.7f, 0.8f);
                    break;
                    
                case MoonPhase.LastQuarter:
                    sparkMultiplier = 1f;
                    plantGrowthMultiplier = 1f;
                    currentMoonlight = new Color(0.5f, 0.5f, 0.6f);
                    EnableBalanceSecrets();
                    break;
                    
                case MoonPhase.WaningCrescent:
                    sparkMultiplier = 0.75f;
                    plantGrowthMultiplier = 1.2f;
                    currentMoonlight = new Color(0.3f, 0.3f, 0.4f);
                    EnableWisdomSecrets(); // Waning moon = reflection
                    break;
            }
            
            // Update visual moon sprite
            UpdateMoonVisual(phase);
            
            // Apply moonlight color
            if (moonLight != null)
            {
                moonLight.color = currentMoonlight;
                moonLight.intensity = (CosmicDataManager.Instance != null
                    ? CosmicDataManager.Instance.MoonIllumination
                    : 0.5f) * 0.5f;
            }
            
            // Notify other systems
            NotifyGameSystems();
        }

        private void UpdateMoonVisual(MoonPhase phase)
        {
            if (moonSprite != null && moonPhaseSprites != null && moonPhaseSprites.Length == 8)
            {
                moonSprite.sprite = moonPhaseSprites[(int)phase];
                
                // Scale based on illumination (supermoon effect)
                float illumination = CosmicDataManager.Instance != null ? CosmicDataManager.Instance.MoonIllumination : 0f;
                float scale = 1f + (illumination * 0.2f);
                moonSprite.transform.localScale = Vector3.one * scale;
            }
        }

        private void EnableNewMoonSecrets()
        {
            // Dark-only secrets become visible
            GameObject[] darkSecrets = GameObject.FindGameObjectsWithTag("NewMoonSecret");
            foreach (var secret in darkSecrets)
            {
                secret.SetActive(true);
            }
            
            Debug.Log("[MoonPhaseEffects] 🌑 New moon secrets revealed!");
        }

        private void EnableFullMoonSecrets()
        {
            // Bright-only secrets
            GameObject[] brightSecrets = GameObject.FindGameObjectsWithTag("FullMoonSecret");
            foreach (var secret in brightSecrets)
            {
                secret.SetActive(true);
            }
            
            Debug.Log("[MoonPhaseEffects] 🌕 Full moon secrets revealed!");
        }

        private void EnableBalanceSecrets()
        {
            // Quarter moon secrets
            GameObject[] balanceSecrets = GameObject.FindGameObjectsWithTag("QuarterMoonSecret");
            foreach (var secret in balanceSecrets)
            {
                secret.SetActive(true);
            }
        }

        private void EnableWisdomSecrets()
        {
            // Waning crescent wisdom secrets
            GameObject[] wisdomSecrets = GameObject.FindGameObjectsWithTag("WisdomSecret");
            foreach (var secret in wisdomSecrets)
            {
                secret.SetActive(true);
            }
        }

        private void TriggerFullMoonEvent()
        {
            // Special full moon event
            Systems.DailyChallengeManager.Instance?.IncrementChallengeProgress(
                Systems.ChallengeType.DiscoverSecret, 1
            );
            
            // Play special audio
            Core.AudioManager.Instance?.PlaySFX(
                Resources.Load<AudioClip>("Audio/FullMoonChime"), 
                0.7f
            );
            
            // Track achievement progress — first unlock this full moon phase
            Systems.AchievementManager.Instance?.TrackProgress("lunar_devotee", 1);

            // Check for special moon variants
            CheckSupermoon();
            CheckBlueMoon();
            CheckBloodMoon();
        }

        /// <summary>
        /// Supermoon: Full moon with extra-high illumination (peak cycle near 0.5).
        /// Awards 2× sparks for 24 h via PlayerPrefs and unlocks Supermoon achievement.
        /// </summary>
        private void CheckSupermoon()
        {
            if (CosmicDataManager.Instance == null) return;

            // MoonIllumination here is the fractional cycle position (0-1).
            // Near 0.5 = dead centre = closest & brightest = supermoon in our simplified model.
            float cyclePos = CosmicDataManager.Instance.MoonIllumination;
            bool isSupermoon = cyclePos >= 0.48f && cyclePos <= 0.52f;

            if (!isSupermoon) return;

            // Prevent firing multiple times per supermoon (check date)
            string lastSupermoonKey = "Supermoon_LastDate";
            string today = System.DateTime.UtcNow.ToString("yyyyMMdd");
            if (PlayerPrefs.GetString(lastSupermoonKey, "") == today) return;

            PlayerPrefs.SetString(lastSupermoonKey, today);
            // Set the Mars multiplier slot to 2× for 24 h (keys checked by EmberforgeSparks)
            PlayerPrefs.SetFloat("Solstice_LightMultiplier", 2f);
            long expiry = System.DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();
            PlayerPrefs.SetString("Solstice_LightExpiry", expiry.ToString());
            PlayerPrefs.Save();

            // Award supermoon achievement progress
            Systems.AchievementManager.Instance?.TrackProgress("supermoon_powered", 1);

            // Update HUD
            HUDManager.Instance?.ShowNotification("🌕 Supermoon! 2× spark power for 24 hours!", HUDManager.NotificationType.Achievement);

            Debug.Log("[MoonPhaseEffects] 🌕 Supermoon detected — 2× sparks for 24 h!");
        }

        /// <summary>
        /// Blue Moon: second full moon in a calendar month. Unlocks once-in-a-blue-moon achievement.
        /// </summary>
        private void CheckBlueMoon()
        {
            System.DateTime now = System.DateTime.UtcNow;
            string lastFullMoonMonthKey = "FullMoon_LastMonth";
            string lastFullMoonDayKey  = "FullMoon_LastDay";

            int lastMonth = PlayerPrefs.GetInt(lastFullMoonMonthKey, -1);
            int lastDay   = PlayerPrefs.GetInt(lastFullMoonDayKey, -1);

            if (lastMonth == now.Month && lastDay != now.Day && now.Day > lastDay)
            {
                // Second full moon this month  — blue moon!
                Systems.AchievementManager.Instance?.UnlockAchievement("once_in_a_blue_moon");
                HUDManager.Instance?.ShowNotification("🔵 Once in a Blue Moon! Rare achievement unlocked.", HUDManager.NotificationType.Achievement);
                Debug.Log("[MoonPhaseEffects] 🔵 Blue Moon detected!");
            }

            // Record this full moon
            PlayerPrefs.SetInt(lastFullMoonMonthKey, now.Month);
            PlayerPrefs.SetInt(lastFullMoonDayKey, now.Day);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Blood Moon: simulated as a rare full moon event (roughly once per quarter in-game).
        /// Reveals red-themed secrets and unlocks blood_moon_witness achievement.
        /// </summary>
        private void CheckBloodMoon()
        {
            string lastBloodMoonKey = "BloodMoon_LastDate";
            System.DateTime now = System.DateTime.UtcNow;

            if (PlayerPrefs.HasKey(lastBloodMoonKey))
            {
                System.DateTime last = System.DateTime.Parse(PlayerPrefs.GetString(lastBloodMoonKey));
                // Only allow one blood moon per 90 days
                if ((now - last).TotalDays < 90) return;
            }

            // 20% chance each full moon is a blood moon (after the 90-day cooldown)
            if (UnityEngine.Random.value > 0.20f) return;

            PlayerPrefs.SetString(lastBloodMoonKey, now.ToString("o"));
            PlayerPrefs.Save();

            // Apply red moonlight
            sparkMultiplier *= 1.5f;
            if (moonLight != null) moonLight.color = new Color(0.8f, 0.1f, 0.0f);

            // Reveal blood moon secrets
            GameObject[] bloodSecrets = GameObject.FindGameObjectsWithTag("BloodMoonSecret");
            foreach (var secret in bloodSecrets)
                secret.SetActive(true);

            Systems.AchievementManager.Instance?.UnlockAchievement("blood_moon_witness");
            HUDManager.Instance?.ShowNotification("🩸 Blood Moon rises! Rare secrets have appeared.", HUDManager.NotificationType.Info);
            Debug.Log("[MoonPhaseEffects] 🩸 Blood Moon event triggered!");
        }

        private void NotifyGameSystems()
        {
            // Send moon phase data to other systems
            Core.FirebaseManager.Instance?.TrackEvent("moon_phase_change", 
                new System.Collections.Generic.Dictionary<string, object>
            {
                { "phase", CosmicDataManager.Instance.CurrentMoonPhase.ToString() },
                { "illumination", CosmicDataManager.Instance.MoonIllumination },
                { "spark_multiplier", sparkMultiplier }
            });
        }

        // Public API for other systems
        public float GetSparkMultiplier() => sparkMultiplier;
        public float GetPlantGrowthMultiplier() => plantGrowthMultiplier;
        public Color GetMoonlightColor() => currentMoonlight;
        
        public bool IsFullMoon() => 
            CosmicDataManager.Instance?.CurrentMoonPhase == MoonPhase.FullMoon;
        
        public bool IsNewMoon() => 
            CosmicDataManager.Instance?.CurrentMoonPhase == MoonPhase.NewMoon;
    }
}
