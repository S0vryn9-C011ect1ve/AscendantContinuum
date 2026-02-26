using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace AscendantContinuum.Audio
{
    /// <summary>
    /// Modular audio system with layered ambience, music transitions, and SFX triggers
    /// Manages volume for all audio types independently
    /// Supports cosmic ambience, tonal drones, UI chimes, wind textures, and reverb-heavy pads
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public class AudioCategory
        {
            public string name = "Master";
            [Range(0f, 1f)] public float volume = 1f;
            public List<AudioSource> sources = new List<AudioSource>();
        }

        [Header("Audio Channels")]
        [SerializeField] private int maxAudioSources = 32;
        [SerializeField] private Vector3 audioListenerOffset = Vector3.zero;

        [Header("Volume Settings")]
        [SerializeField] private float masterVolume = 0.8f;
        [SerializeField] private float musicVolume = 0.6f;
        [SerializeField] private float sfxVolume = 0.7f;
        [SerializeField] private float ambienceVolume = 0.4f;
        [SerializeField] private float uiVolume = 0.8f;

        [Header("Music Crossfade")]
        [SerializeField] private float musicCrossfadeDuration = 2.5f;
        [SerializeField] private float reducedMotionCrossfadeDuration = 0.3f;

        [Header("Ambience Layers")]
        [SerializeField] private AudioClip cosmicAmbience;      // Soft cosmic hum
        [SerializeField] private AudioClip tonalDrone;          // Subtle tonal foundation
        [SerializeField] private AudioClip windTexture;         // Breath-like wind
        [SerializeField] private AudioClip reverbPad;           // Gentle reverb-heavy pad

        // Audio source pools
        private Dictionary<string, AudioCategory> audioCategories = new Dictionary<string, AudioCategory>();
        private AudioSource musicSource;
        private AudioSource musicSourceSecondary; // For crossfading
        
        // Ambience sources
        private AudioSource cosmicSource;
        private AudioSource tonalSource;
        private AudioSource windSource;
        private AudioSource reverbSource;

        private List<AudioSource> sfxSources = new List<AudioSource>();
        private List<AudioSource> uiSources = new List<AudioSource>();

        private Coroutine musicCrossfadeCoroutine;
        private bool isInitialized = false;

        // Layer states
        private bool ambienceLayersEnabled = true;
        private Dictionary<string, bool> ambienceLayerStates = new Dictionary<string, bool>();
        private Dictionary<string, float> ambienceLayerVolumes = new Dictionary<string, float>();

        // Music state
        private AudioClip currentMusicClip;
        private AudioClip targetMusicClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSystem();
        }

        private void InitializeAudioSystem()
        {
            if (isInitialized) return;

            // Create music sources
            musicSource = CreateAudioSource("Music_Primary", false, 0f);
            musicSourceSecondary = CreateAudioSource("Music_Secondary", false, 0f);

            // Create ambience sources (for layered ambience)
            cosmicSource = CreateAudioSource("Ambience_Cosmic", true, 0f);
            tonalSource = CreateAudioSource("Ambience_Tonal", true, 0f);
            windSource = CreateAudioSource("Ambience_Wind", true, 0f);
            reverbSource = CreateAudioSource("Ambience_Reverb", true, 0f);

            if (cosmicAmbience != null) { cosmicSource.clip = cosmicAmbience; cosmicSource.Play(); }
            if (tonalDrone != null) { tonalSource.clip = tonalDrone; tonalSource.Play(); }
            if (windTexture != null) { windSource.clip = windTexture; windSource.Play(); }
            if (reverbPad != null) { reverbSource.clip = reverbPad; reverbSource.Play(); }

            // Create SFX source pool
            for (int i = 0; i < 16; i++)
            {
                AudioSource sfxSource = CreateAudioSource($"SFX_{i}", true, 0f);
                sfxSources.Add(sfxSource);
            }

            // Create UI source pool
            for (int i = 0; i < 8; i++)
            {
                AudioSource uiSource = CreateAudioSource($"UI_{i}", true, 0f);
                uiSources.Add(uiSource);
            }

            // Initialize ambience layers
            InitializeAmbienceLayers();

            isInitialized = true;
            Debug.Log("[AudioManager] Initialized with modular audio system");
        }

        private void InitializeAmbienceLayers()
        {
            // Set up layered ambience
            ambienceLayerStates["cosmic"] = true;
            ambienceLayerStates["tonal"] = true;
            ambienceLayerStates["wind"] = false;
            ambienceLayerStates["reverb"] = false;

            ambienceLayerVolumes["cosmic"] = 0.3f;
            ambienceLayerVolumes["tonal"] = 0.2f;
            ambienceLayerVolumes["wind"] = 0.1f;
            ambienceLayerVolumes["reverb"] = 0.25f;
        }

        private AudioSource CreateAudioSource(string name, bool loop, float spatialBlend)
        {
            GameObject audioObj = new GameObject(name);
            audioObj.transform.SetParent(transform);
            AudioSource source = audioObj.AddComponent<AudioSource>();

            source.loop = loop;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.dopplerLevel = 0f; // No Doppler effect for space
            source.volume = 0f; // Start silent

            return source;
        }

        #region Music Control

        /// <summary>
        /// Crossfade to new music with smooth transition
        /// </summary>
        public void PlayMusic(AudioClip clip, float fadeInDuration = -1)
        {
            if (clip == null) return;

            fadeInDuration = fadeInDuration < 0 ? musicCrossfadeDuration : fadeInDuration;

            // Check if reduced motion
            if (AccessibilityManager.Instance?.ReducedMotionEnabled == true)
                fadeInDuration = reducedMotionCrossfadeDuration;

            targetMusicClip = clip;

            // If no music playing, just play
            if (musicSource.clip == null || !musicSource.isPlaying)
            {
                musicSource.clip = clip;
                musicSource.time = 0f;
                musicSource.Play();
                currentMusicClip = clip;
                Debug.Log($"[AudioManager] Playing music: {clip.name}");
                return;
            }

            // Otherwise, crossfade
            if (musicCrossfadeCoroutine != null)
                StopCoroutine(musicCrossfadeCoroutine);

            musicCrossfadeCoroutine = StartCoroutine(
                MusicCrossfadeCoroutine(musicSource, musicSourceSecondary, clip, fadeInDuration)
            );
        }

        private IEnumerator MusicCrossfadeCoroutine(
            AudioSource fadeOutSource,
            AudioSource fadeInSource,
            AudioClip clipToFadeIn,
            float duration)
        {
            float elapsedTime = 0f;
            float startVolumeOut = fadeOutSource.volume;
            float startVolumeIn = 0f;

            // Setup fade-in source
            fadeInSource.clip = clipToFadeIn;
            fadeInSource.time = 0f;
            fadeInSource.volume = 0f;
            fadeInSource.Play();

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration);

                fadeOutSource.volume = Mathf.Lerp(startVolumeOut, 0f, progress);
                fadeInSource.volume = Mathf.Lerp(startVolumeIn, musicVolume, progress);

                yield return null;
            }

            // Ensure final volumes
            fadeOutSource.volume = 0f;
            fadeOutSource.Stop();
            fadeInSource.volume = musicVolume;

            // Swap sources
            AudioSource temp = (AudioSource)musicSource;
            musicSource = fadeInSource;
            musicSourceSecondary = fadeOutSource;

            currentMusicClip = clipToFadeIn;
            Debug.Log($"[AudioManager] Music crossfaded to: {clipToFadeIn.name}");
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            if (musicCrossfadeCoroutine != null)
                StopCoroutine(musicCrossfadeCoroutine);

            StartCoroutine(FadeMusicOut(fadeDuration));
        }

        private IEnumerator FadeMusicOut(float duration)
        {
            float elapsedTime = 0f;
            float startVolume = musicSource.volume;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = 0f;
        }

        #endregion

        #region Ambience Layers

        /// <summary>
        /// Set up realm-specific ambience (cosmic, tonal, wind, reverb combinations)
        /// </summary>
        public void SetAmbienceLayers(string cosmicIntensity, string tonalIntensity, string windIntensity, string reverbIntensity)
        {
            // Example: SetAmbienceLayers("high", "medium", "off", "low");
            ambienceLayerVolumes["cosmic"] = ParseIntensity(cosmicIntensity);
            ambienceLayerVolumes["tonal"] = ParseIntensity(tonalIntensity);
            ambienceLayerVolumes["wind"] = ParseIntensity(windIntensity);
            ambienceLayerVolumes["reverb"] = ParseIntensity(reverbIntensity);

            Debug.Log("[AudioManager] Ambience layers updated");
        }

        /// <summary>
        /// Set up realm-specific ambience with float intensities
        /// </summary>
        public void SetAmbienceLayers(float cosmicIntensity, float tonalIntensity, float windIntensity, float reverbIntensity)
        {
            ambienceLayerVolumes["cosmic"] = Mathf.Clamp01(cosmicIntensity);
            ambienceLayerVolumes["tonal"] = Mathf.Clamp01(tonalIntensity);
            ambienceLayerVolumes["wind"] = Mathf.Clamp01(windIntensity);
            ambienceLayerVolumes["reverb"] = Mathf.Clamp01(reverbIntensity);

            Debug.Log("[AudioManager] Ambience layers updated (float)");
        }

        private float ParseIntensity(string intensity)
        {
            return intensity switch
            {
                "off" => 0f,
                "low" => 0.15f,
                "medium" => 0.3f,
                "high" => 0.5f,
                _ => 0.2f
            };
        }

        public void EnableAmbienceLayers(bool enabled)
        {
            ambienceLayersEnabled = enabled;
            if (!enabled)
            {
                if (cosmicSource != null) cosmicSource.volume = 0f;
                if (tonalSource != null) tonalSource.volume = 0f;
                if (windSource != null) windSource.volume = 0f;
                if (reverbSource != null) reverbSource.volume = 0f;
            }
        }

        private void Update()
        {
            if (!isInitialized || !ambienceLayersEnabled) return;

            // Smoothly interpolate ambience volumes
            float dt = Time.deltaTime * 2f;
            if (cosmicSource != null) cosmicSource.volume = Mathf.Lerp(cosmicSource.volume, ambienceLayerVolumes["cosmic"] * ambienceVolume * masterVolume, dt);
            if (tonalSource != null) tonalSource.volume = Mathf.Lerp(tonalSource.volume, ambienceLayerVolumes["tonal"] * ambienceVolume * masterVolume, dt);
            if (windSource != null) windSource.volume = Mathf.Lerp(windSource.volume, ambienceLayerVolumes["wind"] * ambienceVolume * masterVolume, dt);
            if (reverbSource != null) reverbSource.volume = Mathf.Lerp(reverbSource.volume, ambienceLayerVolumes["reverb"] * ambienceVolume * masterVolume, dt);
        }

        #endregion

        #region SFX Control

        /// <summary>
        /// Play a sound effect (pooled from available sources)
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f, bool loop = false)
        {
            if (clip == null) return;

            AudioSource availableSource = GetAvailableSFXSource();
            if (availableSource == null) return;

            availableSource.clip = clip;
            availableSource.volume = volume * sfxVolume;
            availableSource.loop = loop;
            availableSource.time = 0f;
            availableSource.Play();

            if (!loop)
            {
                StartCoroutine(ReleaseAfterDuration(availableSource, clip.length));
            }
        }

        /// <summary>
        /// Play UI sound (clicks, pings, notifications)
        /// </summary>
        public void PlayUISound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource availableSource = GetAvailableUISource();
            if (availableSource == null) return;

            availableSource.clip = clip;
            availableSource.volume = volume * uiVolume;
            availableSource.loop = false;
            availableSource.time = 0f;
            availableSource.Play();

            StartCoroutine(ReleaseAfterDuration(availableSource, clip.length));
        }

        /// <summary>
        /// Play 3D positional audio
        /// </summary>
        public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f, float spatialRange = 50f)
        {
            if (clip == null) return;

            AudioSource availableSource = GetAvailableSFXSource();
            if (availableSource == null) return;

            availableSource.transform.position = position;
            availableSource.spatialBlend = 1f; // Full 3D
            availableSource.maxDistance = spatialRange;

            availableSource.clip = clip;
            availableSource.volume = volume * sfxVolume;
            availableSource.loop = false;
            availableSource.time = 0f;
            availableSource.Play();

            StartCoroutine(ReleaseAfterDuration(availableSource, clip.length));
        }

        private AudioSource GetAvailableSFXSource()
        {
            // Find first non-playing source
            foreach (AudioSource source in sfxSources)
            {
                if (!source.isPlaying)
                    return source;
            }

            // If all busy, create temporary one
            AudioSource temp = CreateAudioSource("SFX_Overflow", false, 0f);
            sfxSources.Add(temp);
            return temp;
        }

        private AudioSource GetAvailableUISource()
        {
            // Find first non-playing source
            foreach (AudioSource source in uiSources)
            {
                if (!source.isPlaying)
                    return source;
            }

            // If all busy, create temporary one
            AudioSource temp = CreateAudioSource("UI_Overflow", false, 0f);
            uiSources.Add(temp);
            return temp;
        }

        private IEnumerator ReleaseAfterDuration(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration);
            source.Stop();
            source.clip = null;
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeToAll();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetAmbienceVolume(float volume)
        {
            ambienceVolume = Mathf.Clamp01(volume);
        }

        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
        }

        private void ApplyVolumeToAll()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;

            foreach (AudioSource source in sfxSources)
                if (source != null && source.isPlaying)
                    source.volume *= masterVolume;

            foreach (AudioSource source in uiSources)
                if (source != null && source.isPlaying)
                    source.volume *= masterVolume;
        }

        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        public float GetAmbienceVolume() => ambienceVolume;
        public float GetUIVolume() => uiVolume;

        #endregion

        #region Utility

        public bool IsPlayingMusic() => musicSource != null && musicSource.isPlaying;
        public AudioClip GetCurrentMusicClip() => currentMusicClip;

        #endregion
    }
}
