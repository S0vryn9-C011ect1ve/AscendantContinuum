using UnityEngine;
using System;

namespace AscendantContinuum.Data
{
    /// <summary>
    /// Scriptable Object defining a realm's properties, visuals, and mechanics
    /// </summary>
    [CreateAssetMenu(fileName = "NewRealm", menuName = "Ascendant Continuum/Realm Data")]
    public class RealmData : ScriptableObject
    {
        [Header("Identity")]
        public string realmId = "emberforge";
        public string realmName = "Emberforge";
        public string tagline = "Where flames dance and creation ignites";
        
        [Header("Visual Theme")]
        public Color primaryColor = new Color(1f, 0.4f, 0.2f);
        public Color secondaryColor = new Color(1f, 0.6f, 0f);
        public Gradient atmosphereGradient;
        public Sprite realmIcon;
        public Sprite backgroundSprite;
        
        [Header("Audio")]
        public AudioClip ambientMusic;
        public AudioClip ritualSound;
        public AudioClip transitionSound;
        
        [Header("Gameplay")]
        public RitualType primaryRitual;
        public int unlockRequirement = 0; // Sparks needed to unlock
        public bool isStartingRealm = false;
        
        [Header("Description")]
        [TextArea(3, 6)]
        public string description = "Dance with living flames...";
        
        [Header("Accessibility Secrets")]
        public ColorblindSecret[] colorblindSecrets;
        public ReducedMotionContent reducedMotionAlternative;
    }

    [Serializable]
    public class ColorblindSecret
    {
        public Core.ColorblindMode mode;
        public string secretDescription;
        public Vector3 secretLocation;
        public Sprite hiddenSprite;
    }

    [Serializable]
    public class ReducedMotionContent
    {
        public bool hasAlternative = true;
        public string contentDescription;
        [TextArea(2, 4)]
        public string narrativeReveal;
    }

    public enum RitualType
    {
        SparkCollection,    // Emberforge: Collect dancing sparks
        GrowthNurturing,    // Verdant: Nurture magical plants
        StarTracing,        // Echo Fields: Connect constellations
        LightReflection,    // Dawn Citadel: Manipulate prisms
        LanternRelease      // Lantern Ascension: Release wishes
    }
}
