using UnityEngine;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// ScriptableObject that holds all global AudioClip references.
    /// Loaded at runtime via Resources.Load&lt;AudioSourceData&gt;("Config/AudioSourceData").
    /// Assign clips in the Inspector after importing audio assets.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AudioSourceData",
        menuName = "Ascendant Continuum/Core/Audio Source Data")]
    public sealed class AudioSourceData : ScriptableObject
    {
        [Header("Main Menu & Onboarding")]
        public AudioClip mainMenuMusic;
        public AudioClip onboardingMusic;

        [Header("Realm Music")]
        public AudioClip emberforgeMusic;
        public AudioClip verdantMusic;
        public AudioClip echoFieldsMusic;
        public AudioClip dawnCitadelMusic;
        public AudioClip lanternAscensionMusic;

        [Header("Realm Ambient Loops")]
        public AudioClip emberforgeAmbient;
        public AudioClip verdantAmbient;
        public AudioClip echoFieldsAmbient;
        public AudioClip dawnCitadelAmbient;
        public AudioClip lanternAmbient;

        [Header("Ritual SFX")]
        public AudioClip sparkTap;
        public AudioClip sparkCollect;
        public AudioClip plantGrow;
        public AudioClip constellationConnect;
        public AudioClip lightRefract;
        public AudioClip lanternPlace;

        [Header("UI SFX")]
        public AudioClip buttonClick;
        public AudioClip menuOpen;
        public AudioClip menuClose;
        public AudioClip achievementUnlock;
        public AudioClip sigilUnlock;
        public AudioClip notificationPing;

        [Header("Celebration SFX")]
        public AudioClip ritualCompleteMinor;
        public AudioClip ritualCompleteMajor;
        public AudioClip dailyChallengeComplete;
        public AudioClip achievementSecretUnlock;
        public AudioClip levelUp;

        // ── Runtime lookup ─────────────────────────────────────────────────
        private static AudioSourceData _instance;

        /// <summary>Cached singleton loaded from Resources/Config/AudioSourceData.</summary>
        public static AudioSourceData Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<AudioSourceData>("Config/AudioSourceData");
                return _instance;
            }
        }

        /// <summary>Returns the realm music clip for a given realm ID string.</summary>
        public AudioClip GetRealmMusic(string realmId)
        {
            return realmId?.ToLowerInvariant() switch
            {
                "emberforge"       => emberforgeMusic,
                "verdant"          => verdantMusic,
                "verdant_sanctuary"=> verdantMusic,
                "echo_fields"      => echoFieldsMusic,
                "dawn_citadel"     => dawnCitadelMusic,
                "lantern_ascension"=> lanternAscensionMusic,
                _                  => null
            };
        }

        /// <summary>Returns the ambient loop clip for a given realm ID string.</summary>
        public AudioClip GetRealmAmbient(string realmId)
        {
            return realmId?.ToLowerInvariant() switch
            {
                "emberforge"       => emberforgeAmbient,
                "verdant"          => verdantAmbient,
                "verdant_sanctuary"=> verdantAmbient,
                "echo_fields"      => echoFieldsAmbient,
                "dawn_citadel"     => dawnCitadelAmbient,
                "lantern_ascension"=> lanternAmbient,
                _                  => null
            };
        }
    }
}
