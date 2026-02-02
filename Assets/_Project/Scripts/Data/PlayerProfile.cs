using UnityEngine;
using System;

namespace AscendantContinuum.Data
{
    /// <summary>
    /// Player's persistent data structure - syncs with Firebase
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        // Identity
        public string playerId;
        public string playerName = "Seeker";
        public string createdAt;
        public string lastLogin;
        
        // Progress
        public int totalSparksCollected = 0;
        public int totalPlayTime = 0; // seconds
        public string currentRealm = "emberforge";
        public string[] unlockedRealms = new string[] { "emberforge" };
        public int consecutiveDays = 0;
        
        // Sigil (player's unique magical symbol)
        public SigilData personalSigil;
        
        // Accessibility preferences
        public AccessibilitySettings accessibility = new AccessibilitySettings();
        
        // Social
        public bool allowTimeShareData = true;
        public bool allowPuzzleChains = true;
        
        // Achievements
        public string[] unlockedSecrets = new string[0];
        public int[] realmMasteryLevels = new int[5]; // One per realm
    }

    [Serializable]
    public class SigilData
    {
        public string sigilId;
        public int baseShape = 0; // 0-9 different base shapes
        public int pattern = 0; // 0-9 different patterns
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.cyan;
        public string generatedFromPlaystyle; // JSON of playstyle metrics
    }

    [Serializable]
    public class AccessibilitySettings
    {
        public int colorblindMode = 0; // Maps to ColorblindMode enum
        public bool reducedMotion = false;
        public bool hapticsEnabled = true;
        public float textScale = 1f;
        public bool highContrast = false;
        public bool screenReaderMode = false;
        public float touchTargetSize = 1f;
        public bool subtitlesEnabled = true;
    }

    /// <summary>
    /// Session-specific data (not persisted long-term)
    /// </summary>
    [Serializable]
    public class SessionData
    {
        public DateTime sessionStartTime;
        public int sparksThisSession = 0;
        public string[] realmsVisitedThisSession = new string[0];
        public int ritualCompletions = 0;
    }
}
