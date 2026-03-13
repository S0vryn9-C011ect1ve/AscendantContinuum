using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Audio manager with spatial sound, dynamic mixing, and accessibility features
    /// Supports 3D audio for immersion and adaptive audio for reduced-motion modes
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer masterMixer;
        
        [Header("Volume Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float ambientVolume = 0.5f;
        [SerializeField] private float uiVolume = 0.8f;
        
        [Header("Pools")]
        [SerializeField] private int sfxPoolSize = 20;
        
        private Queue<AudioSource> sfxPool;
        private AudioSource musicSource;
        private AudioSource ambientSource;
        private AudioSource uiSource;
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private bool isShuttingDown;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAudioSources();
            LoadSettings();
        }

        private void InitializeAudioSources()
        {
            // Create music source
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.priority = 0; // Highest priority
            
            // Create ambient source
            GameObject ambientObj = new GameObject("AmbientSource");
            ambientObj.transform.SetParent(transform);
            ambientSource = ambientObj.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            ambientSource.spatialBlend = 0f; // 2D sound
            
            // Create SFX pool
            sfxPool = new Queue<AudioSource>();
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject sfxObj = new GameObject($"SFXSource_{i}");
                sfxObj.transform.SetParent(transform);
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                sfxPool.Enqueue(source);
            }
            
            Debug.Log($"[AudioManager] Initialized - {sfxPoolSize} SFX sources pooled");
        }

        public void PlayMusic(AudioClip clip, float fadeTime = 1f)
        {
            if (isShuttingDown || musicSource == null) return;

            if (clip == null)
            {
                StopMusic(Mathf.Max(0.01f, fadeTime));
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying) return;
            
            if (musicSource.isPlaying)
            {
                StartCoroutine(CrossfadeMusic(clip, fadeTime));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
            
            Debug.Log($"[AudioManager] Playing music: {clip.name}");
        }

        private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip, float fadeTime)
        {
            if (isShuttingDown || musicSource == null) yield break;
            float startVolume = musicSource.volume;
            
            // Fade out
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                if (isShuttingDown || musicSource == null) yield break;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                yield return null;
            }
            
            if (isShuttingDown || musicSource == null) yield break;
            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.Play();
            
            // Fade in
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                if (isShuttingDown || musicSource == null) yield break;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeTime);
                yield return null;
            }
            
            if (musicSource == null) yield break;
            musicSource.volume = musicVolume;
        }

        public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, bool spatial = false)
        {
            if (isShuttingDown) return;
            if (clip == null) return;
            
            AudioSource source = GetAvailableSFXSource();
            if (source == null)
            {
                Debug.LogWarning("[AudioManager] No available SFX sources in pool");
                return;
            }
            
            source.transform.position = position;
            source.clip = clip;
            source.volume = sfxVolume * volume;
            source.spatialBlend = spatial ? 1f : 0f; // 0 = 2D, 1 = 3D
            source.Play();
            
            StartCoroutine(ReturnToPoolAfterPlay(source));
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            PlaySFX(clip, position, volume, false);
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (AudioSource source in sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            
            // If all busy, return the first one (it will interrupt)
            return sfxPool.Count > 0 ? sfxPool.Peek() : null;
        }

        private System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            
            // Source is now available for reuse
            source.clip = null;
        }

        public void PlayAmbient(AudioClip clip, float volume = -1f)
        {
            if (isShuttingDown || ambientSource == null) return;
            if (clip == null) return;

            ambientSource.clip = clip;
            ambientSource.volume = volume >= 0 ? volume : ambientVolume;
            ambientSource.Play();
            
            Debug.Log($"[AudioManager] Playing ambient: {clip.name}");
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = masterVolume;
            SaveSettings();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            SaveSettings();
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            ambientSource.volume = ambientVolume;
            SaveSettings();
        }

        public void SetAmbienceVolume(float volume) => SetAmbientVolume(volume);

        public float GetAmbienceVolume() => ambientVolume;

        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            if (uiSource != null)
            {
                uiSource.volume = uiVolume;
            }
            SaveSettings();
        }

        public float GetUIVolume() => uiVolume;

        public void SetAmbienceLayers(float cosmicIntensity, float tonalIntensity, float windIntensity, float reverbIntensity)
        {
            float targetAmbient = Mathf.Clamp01((cosmicIntensity + tonalIntensity + windIntensity + reverbIntensity) * 0.25f);
            SetAmbientVolume(targetAmbient);
        }

        public void PlayUISound(AudioClip clip, float volume = 1f)
        {
            if (clip == null || uiSource == null) return;

            uiSource.PlayOneShot(clip, Mathf.Clamp01(volume) * uiVolume);
        }

        private void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("Audio_MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1f);
            ambientVolume = PlayerPrefs.GetFloat("Audio_AmbientVolume", 0.5f);
            uiVolume = PlayerPrefs.GetFloat("Audio_UIVolume", 0.8f);
            
            AudioListener.volume = masterVolume;
            if (musicSource != null) musicSource.volume = musicVolume;
            if (ambientSource != null) ambientSource.volume = ambientVolume;
            if (uiSource != null) uiSource.volume = uiVolume;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("Audio_MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("Audio_MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("Audio_SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("Audio_AmbientVolume", ambientVolume);
            PlayerPrefs.SetFloat("Audio_UIVolume", uiVolume);
            PlayerPrefs.Save();
        }

        public void StopMusic(float fadeTime = 1f)
        {
            if (isShuttingDown || musicSource == null) return;
            StartCoroutine(FadeOutMusic(fadeTime));
        }

        private System.Collections.IEnumerator FadeOutMusic(float fadeTime)
        {
            if (isShuttingDown || musicSource == null) yield break;
            float startVolume = musicSource.volume;
            
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                if (isShuttingDown || musicSource == null) yield break;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                yield return null;
            }
            
            if (musicSource == null) yield break;
            musicSource.Stop();
            musicSource.volume = musicVolume;
        }

        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;

        private void OnEnable()
        {
            if (isShuttingDown) return;
            if (uiSource == null)
            {
                GameObject uiObj = new GameObject("UISource");
                uiObj.transform.SetParent(transform);
                uiSource = uiObj.AddComponent<AudioSource>();
                uiSource.loop = false;
                uiSource.playOnAwake = false;
                uiSource.spatialBlend = 0f;
                uiSource.volume = uiVolume;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            isShuttingDown = true;
            StopAllCoroutines();
        }

        // ── Sigil audio ───────────────────────────────────────────────────

        /// <summary>
        /// Called by SigilCompletionHandler — routes to ProceduralToneGenerator.
        /// </summary>
        public void PlaySigilCompletion(AscendantContinuum.Systems.SigilAnalysisResult analysis)
        {
            var tone = GetComponent<AscendantContinuum.Audio.ProceduralToneGenerator>()
                    ?? gameObject.AddComponent<AscendantContinuum.Audio.ProceduralToneGenerator>();
            // Tone generator responds automatically via GameEvents.OnSigilCompleted;
            // no explicit call needed — this method exists as a documented hook.
            Debug.Log($"[AudioManager] Sigil completion — complexity: {analysis.Complexity:F2}");
        }

        /// <summary>
        /// Called by realm ecosystem reactors (Verdant / Echo / Lantern).
        /// Routes to ProceduralToneGenerator arpeggio on the attached GameObject.
        /// </summary>
        public void PlayRealmEcosystemReaction(string realmId, UnityEngine.Color primaryColor)
        {
            var tone = GetComponent<AscendantContinuum.Audio.ProceduralToneGenerator>()
                    ?? gameObject.AddComponent<AscendantContinuum.Audio.ProceduralToneGenerator>();

            float rootHz = realmId switch
            {
                "verdant"  => 261.6f,  // C4
                "echo"     => 329.6f,  // E4
                "lantern"  => 392f,    // G4
                "ember"    => 220f,    // A3
                "dawn"     => 440f,    // A4
                _          => 300f
            };
            tone.PlayArpeggio(rootHz, steps: 4, stepRatio: 1.2599f);
        }

        /// <summary>
        /// Warm resolving chord for Kindness Chain / blessing reveals.
        /// </summary>
        public void PlayBlessing()
        {
            var tone = GetComponent<AscendantContinuum.Audio.ProceduralToneGenerator>()
                    ?? gameObject.AddComponent<AscendantContinuum.Audio.ProceduralToneGenerator>();
            tone.PlayBlessingChord();
        }
    }
}
