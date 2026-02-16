using System;
using System.Collections.Generic;
using UnityEngine;

namespace AscendantContinuum.Data
{
    /// <summary>
    /// Tracks player behavior and preferences for sigil generation.
    /// Metrics influence the visual style and attributes of the player's personal sigil.
    /// </summary>
    [Serializable]
    public class PlayerPlaystyleMetrics
    {
        [Header("Play Time Tracking")]
        public float totalPlayTimeMinutes = 0f;
        public float averageSessionLengthMinutes = 0f;
        public int totalSessions = 0;
        
        [Header("Realm Visits")]
        public int emberforgeVisits = 0;
        public int verdantSanctuaryVisits = 0;
        public int echoFieldsVisits = 0;
        public int dawnCitadelVisits = 0;
        public int lanternAscensionVisits = 0;
        
        [Header("Ritual Performance")]
        public int ritualsCompleted = 0;
        public int ritualsAbandoned = 0;
        public float averageRitualCompletionTime = 0f;
        public int perfectRituals = 0; // Completed without errors
        
        [Header("Pace & Timing")]
        public float averageActionSpeed = 1f; // 1.0 = normal, <1 = slow, >1 = fast
        public bool prefersMorningPlay = false;
        public bool prefersEveningPlay = false;
        public bool prefersNightPlay = false;
        
        [Header("Accessibility Usage")]
        public bool usesColorblindMode = false;
        public bool usesReducedMotion = false;
        public bool usesTextScaling = false;
        public bool usesHaptics = true;
        public string preferredColorblindMode = "None";
        
        [Header("Interaction Style")]
        public int tapCount = 0;
        public int swipeCount = 0;
        public int holdCount = 0;
        public int pinchCount = 0;
        
        [Header("Social Behavior")]
        public int dailyChallengesCompleted = 0;
        public int sharesCreated = 0;
        public bool prefersCollaboration = false;
        public bool prefersSoloPlay = true;
        
        [Header("Exploration")]
        public int secretsDiscovered = 0;
        public int achievementsUnlocked = 0;
        public bool explorerType = false; // Explores everything thoroughly
        public bool speedrunnerType = false; // Completes quickly
        
        #region Calculated Properties
        
        /// <summary>
        /// Returns the most visited realm
        /// </summary>
        public string FavoriteRealm
        {
            get
            {
                int max = Mathf.Max(emberforgeVisits, verdantSanctuaryVisits, echoFieldsVisits, 
                                   dawnCitadelVisits, lanternAscensionVisits);
                
                if (max == emberforgeVisits) return "Emberforge";
                if (max == verdantSanctuaryVisits) return "Verdant Sanctuary";
                if (max == echoFieldsVisits) return "Echo Fields";
                if (max == dawnCitadelVisits) return "Dawn Citadel";
                if (max == lanternAscensionVisits) return "Lantern Ascension";
                
                return "None";
            }
        }
        
        /// <summary>
        /// Returns ritual completion percentage
        /// </summary>
        public float RitualCompletionRate
        {
            get
            {
                int total = ritualsCompleted + ritualsAbandoned;
                if (total == 0) return 0f;
                return (float)ritualsCompleted / total;
            }
        }
        
        /// <summary>
        /// Returns dominant interaction type
        /// </summary>
        public string DominantInteraction
        {
            get
            {
                int max = Mathf.Max(tapCount, swipeCount, holdCount, pinchCount);
                
                if (max == tapCount) return "Tap";
                if (max == swipeCount) return "Swipe";
                if (max == holdCount) return "Hold";
                if (max == pinchCount) return "Pinch";
                
                return "Mixed";
            }
        }
        
        /// <summary>
        /// Returns preferred play time
        /// </summary>
        public string PreferredPlayTime
        {
            get
            {
                if (prefersMorningPlay) return "Morning";
                if (prefersEveningPlay) return "Evening";
                if (prefersNightPlay) return "Night";
                return "Flexible";
            }
        }
        
        /// <summary>
        /// Overall playstyle: Creative, Patient, Energetic, Focused, Explorer
        /// </summary>
        public string PlaystyleArchetype
        {
            get
            {
                // Creative: Lots of time in Emberforge/Verdant
                bool creative = (emberforgeVisits + verdantSanctuaryVisits) > (echoFieldsVisits + dawnCitadelVisits);
                
                // Patient: Slow pace, high completion rate
                bool patient = averageActionSpeed < 0.8f && RitualCompletionRate > 0.8f;
                
                // Energetic: Fast pace, lots of interactions
                bool energetic = averageActionSpeed > 1.2f && (tapCount + swipeCount) > 100;
                
                // Focused: High perfect ritual count
                bool focused = perfectRituals > (ritualsCompleted * 0.5f);
                
                // Explorer: High secrets and achievements
                bool explorer = secretsDiscovered > 10 || achievementsUnlocked > 15;
                
                if (explorer) return "Explorer";
                if (patient) return "Patient";
                if (energetic) return "Energetic";
                if (focused) return "Focused";
                if (creative) return "Creative";
                
                return "Balanced";
            }
        }
        
        #endregion
        
        #region Update Methods
        
        /// <summary>
        /// Updates play time metrics
        /// </summary>
        public void RecordSession(float durationMinutes)
        {
            totalPlayTimeMinutes += durationMinutes;
            totalSessions++;
            averageSessionLengthMinutes = totalPlayTimeMinutes / totalSessions;
            
            // Determine play time preference
            int hour = DateTime.Now.Hour;
            if (hour >= 5 && hour < 12) prefersMorningPlay = true;
            else if (hour >= 17 && hour < 22) prefersEveningPlay = true;
            else if (hour >= 22 || hour < 5) prefersNightPlay = true;
        }
        
        /// <summary>
        /// Records realm visit
        /// </summary>
        public void RecordRealmVisit(string realmName)
        {
            switch (realmName)
            {
                case "Emberforge":
                    emberforgeVisits++;
                    break;
                case "Verdant Sanctuary":
                    verdantSanctuaryVisits++;
                    break;
                case "Echo Fields":
                    echoFieldsVisits++;
                    break;
                case "Dawn Citadel":
                    dawnCitadelVisits++;
                    break;
                case "Lantern Ascension":
                    lanternAscensionVisits++;
                    break;
            }
        }
        
        /// <summary>
        /// Records ritual completion
        /// </summary>
        public void RecordRitualCompletion(float completionTime, bool perfect)
        {
            ritualsCompleted++;
            
            // Update average completion time
            averageRitualCompletionTime = (averageRitualCompletionTime * (ritualsCompleted - 1) + completionTime) / ritualsCompleted;
            
            if (perfect)
                perfectRituals++;
            
            // Calculate action speed relative to expected time (30 seconds average)
            averageActionSpeed = 30f / averageRitualCompletionTime;
        }
        
        /// <summary>
        /// Records ritual abandonment
        /// </summary>
        public void RecordRitualAbandoned()
        {
            ritualsAbandoned++;
        }
        
        /// <summary>
        /// Records interaction
        /// </summary>
        public void RecordInteraction(string interactionType)
        {
            switch (interactionType.ToLower())
            {
                case "tap":
                    tapCount++;
                    break;
                case "swipe":
                    swipeCount++;
                    break;
                case "hold":
                    holdCount++;
                    break;
                case "pinch":
                    pinchCount++;
                    break;
            }
        }
        
        /// <summary>
        /// Records accessibility setting usage
        /// </summary>
        public void UpdateAccessibilitySettings(bool colorblind, bool reducedMotion, bool textScale, string colorblindType)
        {
            usesColorblindMode = colorblind;
            usesReducedMotion = reducedMotion;
            usesTextScaling = textScale;
            preferredColorblindMode = colorblindType;
        }
        
        /// <summary>
        /// Records social activity
        /// </summary>
        public void RecordSocialActivity(bool shared, bool dailyChallenge)
        {
            if (shared)
                sharesCreated++;
            
            if (dailyChallenge)
                dailyChallengesCompleted++;
            
            // Update social preference
            prefersCollaboration = dailyChallengesCompleted > 5 || sharesCreated > 3;
            prefersSoloPlay = !prefersCollaboration;
        }
        
        /// <summary>
        /// Records discovery
        /// </summary>
        public void RecordDiscovery(bool isSecret, bool isAchievement)
        {
            if (isSecret)
                secretsDiscovered++;
            
            if (isAchievement)
                achievementsUnlocked++;
            
            // Update explorer type
            explorerType = secretsDiscovered > 5 || achievementsUnlocked > 10;
        }
        
        #endregion
        
        #region Serialization Helper
        
        /// <summary>
        /// Creates a summary dictionary for sigil generation
        /// </summary>
        public Dictionary<string, object> ToSigilData()
        {
            return new Dictionary<string, object>
            {
                { "archetype", PlaystyleArchetype },
                { "favoriteRealm", FavoriteRealm },
                { "dominantInteraction", DominantInteraction },
                { "preferredPlayTime", PreferredPlayTime },
                { "completionRate", RitualCompletionRate },
                { "actionSpeed", averageActionSpeed },
                { "isExplorer", explorerType },
                { "usesAccessibility", usesColorblindMode || usesReducedMotion },
                { "totalPlayTime", totalPlayTimeMinutes }
            };
        }
        
        #endregion
    }
}
