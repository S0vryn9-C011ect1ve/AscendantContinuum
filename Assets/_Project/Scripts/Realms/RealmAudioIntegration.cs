using UnityEngine;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Partial extension of RealmController that handles realm audio:
    /// • Plays realm music and ambient loop from AudioSourceData on Enter
    /// • Fades music out on Exit
    ///
    /// No Inspector wiring required — music is resolved from
    /// Resources/Config/AudioSourceData by realm ID.
    /// </summary>
    public abstract partial class RealmController : MonoBehaviour
    {
        [Header("Realm Audio (auto-resolved from AudioSourceData)")]
        [Tooltip("Override music. Leave null to auto-load from AudioSourceData by realm ID.")]
        [SerializeField] protected AudioClip overrideMusic;

        [Tooltip("Override ambient loop. Leave null to auto-load from AudioSourceData by realm ID.")]
        [SerializeField] protected AudioClip overrideAmbient;

        [SerializeField] protected float musicFadeIn  = 2f;
        [SerializeField] protected float musicFadeOut = 1.5f;

        // ── Enter / Exit hooks called by the base EnterRealm / ExitRealm ──

        /// <summary>Call from EnterRealm (already wired inside RealmController.EnterRealm).</summary>
        protected void StartRealmAudio()
        {
            if (AudioManager.Instance == null) return;

            var sourceData = AudioSourceData.Instance;
            if (sourceData == null)
            {
                Debug.LogWarning("[RealmAudio] AudioSourceData not found at Resources/Config/AudioSourceData.");
                return;
            }

            // ── Music ────────────────────────────────────────────────────
            AudioClip music = overrideMusic != null
                ? overrideMusic
                : sourceData.GetRealmMusic(realmId);

            if (music != null)
                AudioManager.Instance.PlayMusic(music, musicFadeIn);
            else
                Debug.LogWarning($"[RealmAudio] No music clip found for realm '{realmId}'. " +
                                 "Assign in AudioSourceData or set overrideMusic.");

            // ── Ambient ──────────────────────────────────────────────────
            AudioClip ambient = overrideAmbient != null
                ? overrideAmbient
                : sourceData.GetRealmAmbient(realmId);

            if (ambient != null)
                AudioManager.Instance.PlayAmbient(ambient);
        }

        /// <summary>Call from ExitRealm (already wired inside RealmController.ExitRealm).</summary>
        protected void StopRealmAudio()
        {
            AudioManager.Instance?.StopMusic(musicFadeOut);
        }
    }
}
