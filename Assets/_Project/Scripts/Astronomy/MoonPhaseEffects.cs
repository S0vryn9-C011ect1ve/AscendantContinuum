using UnityEngine;

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
                moonLight.intensity = CosmicDataManager.Instance.MoonIllumination * 0.5f;
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
                float scale = 1f + (CosmicDataManager.Instance.MoonIllumination * 0.2f);
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
            
            // Track achievement
            Systems.AchievementManager.Instance?.TrackProgress("lunar_devotee", 1);
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
