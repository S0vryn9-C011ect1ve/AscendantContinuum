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
        
        [Header("Pools")]
        [SerializeField] private int sfxPoolSize = 20;
        
        private Queue<AudioSource> sfxPool;
        private AudioSource musicSource;
        private AudioSource ambientSource;
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

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
            float startVolume = musicSource.volume;
            
            // Fade out
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.Play();
            
            // Fade in
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeTime);
                yield return null;
            }
            
            musicSource.volume = musicVolume;
        }

        public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, bool spatial = false)
        {
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
            PlaySFX(clip, Camera.main.transform.position, volume, false);
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
            musicSource.volume = musicVolume;
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

        private void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("Audio_MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1f);
            ambientVolume = PlayerPrefs.GetFloat("Audio_AmbientVolume", 0.5f);
            
            AudioListener.volume = masterVolume;
            musicSource.volume = musicVolume;
            ambientSource.volume = ambientVolume;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("Audio_MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("Audio_MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("Audio_SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("Audio_AmbientVolume", ambientVolume);
            PlayerPrefs.Save();
        }

        public void StopMusic(float fadeTime = 1f)
        {
            StartCoroutine(FadeOutMusic(fadeTime));
        }

        private System.Collections.IEnumerator FadeOutMusic(float fadeTime)
        {
            float startVolume = musicSource.volume;
            
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.volume = musicVolume;
        }
    }
}
