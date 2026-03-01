using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if HAVE_VFX_GRAPH
using UnityEngine.VFX;
#endif
using AscendantContinuum.Realms;

namespace AscendantContinuum.VFX
{
    /// <summary>
    /// Manages VFX Graph effects per realm.
    /// Attach to a persistent GameObject (e.g. VFXManager in Bootstrap scene).
    /// Wire VFX Graph assets in the Inspector under each RealmVFXEntry.
    /// NOTE: Requires com.unity.visualeffectgraph package installed.
    /// </summary>
    public class RealmVFXManager : MonoBehaviour
    {
        [System.Serializable]
        public class RealmVFXEntry
        {
            [Tooltip("Must match RealmAtmosphereData.realmId exactly")]
            public string realmId;

#if HAVE_VFX_GRAPH
            [Tooltip("Primary ambient particle field (e.g. floating embers, stars, spores)")]
            public UnityEngine.VFX.VisualEffect ambientVFX;

            [Tooltip("Accent effect that plays on realm entry (portal flash, bloom burst)")]
            public UnityEngine.VFX.VisualEffect entryBurstVFX;

            [Tooltip("Ground/surface detail effect (sparks on floor, bioluminescent pulses)")]
            public UnityEngine.VFX.VisualEffect surfaceVFX;
#else
            [Tooltip("VFX Graph not installed — effects disabled")]
            public GameObject ambientVFX;

            public GameObject entryBurstVFX;

            public GameObject surfaceVFX;
#endif
            [HideInInspector] public bool isActive;
        }

        [Header("Realm VFX Entries")]
        [SerializeField] private List<RealmVFXEntry> realmEffects = new List<RealmVFXEntry>();

        [Header("Transition Settings")]
        [SerializeField] private float fadeOutDuration = 0.8f;
        [SerializeField] private float fadeInDuration  = 1.2f;

        // Exposed VFX property keys
        private static readonly int k_Intensity  = Shader.PropertyToID("Intensity");
        private static readonly int k_Color      = Shader.PropertyToID("Color");
        private static readonly int k_SpawnRate  = Shader.PropertyToID("SpawnRate");

        private RealmVFXEntry _activeEntry;
        private RealmAtmosphereController _atmosphereController;

        private void Awake()
        {
            _atmosphereController = UnityEngine.Object.FindFirstObjectByType<RealmAtmosphereController>();
        }

        // ------------------------------------------------------------------ //
        //  Public API
        // ------------------------------------------------------------------ //

        /// <summary>Activate VFX for the given realm, fading out the previous one.</summary>
        public void ActivateRealm(string realmId)
        {
#if HAVE_VFX_GRAPH
            RealmVFXEntry target = realmEffects.Find(e => e.realmId == realmId);
            if (target == null)
            {
                Debug.LogWarning($"[RealmVFXManager] No VFX entry found for realmId: {realmId}");
                return;
            }

            if (_activeEntry != null && _activeEntry != target)
                StartCoroutine(FadeOutEntry(_activeEntry));

            StartCoroutine(FadeInEntry(target));
            _activeEntry = target;
#else
            Debug.Log("[RealmVFXManager] VFX Graph not installed. VFX transitions disabled.");
#endif
        }

        /// <summary>Play the one-shot entry burst for the currently active realm.</summary>
        public void PlayEntryBurst()
        {
#if HAVE_VFX_GRAPH
            if (_activeEntry?.entryBurstVFX != null)
                ((UnityEngine.VFX.VisualEffect)_activeEntry.entryBurstVFX).SendEvent("OnPlay");
#endif
        }

        // ------------------------------------------------------------------ //
        //  Internal helpers
        // ------------------------------------------------------------------ //
#if HAVE_VFX_GRAPH
        private IEnumerator FadeOutEntry(RealmVFXEntry entry)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
                SetVFXIntensity(entry, t);
                yield return null;
            }
            StopEntry(entry);
        }

        private IEnumerator FadeInEntry(RealmVFXEntry entry)
        {
            StartEntry(entry);
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                SetVFXIntensity(entry, t);
                yield return null;
            }
            SetVFXIntensity(entry, 1f);
        }

        private static void StartEntry(RealmVFXEntry entry)
        {
            var amb = entry.ambientVFX as UnityEngine.VFX.VisualEffect;
            var surf = entry.surfaceVFX as UnityEngine.VFX.VisualEffect;
            amb?.Play();
            surf?.Play();
            entry.isActive = true;
        }

        private static void StopEntry(RealmVFXEntry entry)
        {
            var amb = entry.ambientVFX as UnityEngine.VFX.VisualEffect;
            var burst = entry.entryBurstVFX as UnityEngine.VFX.VisualEffect;
            var surf = entry.surfaceVFX as UnityEngine.VFX.VisualEffect;
            amb?.Stop();
            burst?.Stop();
            surf?.Stop();
            entry.isActive = false;
        }

        private static void SetVFXIntensity(RealmVFXEntry entry, float value)
        {
            var amb = entry.ambientVFX as UnityEngine.VFX.VisualEffect;
            var surf = entry.surfaceVFX as UnityEngine.VFX.VisualEffect;

            if (amb != null && amb.HasFloat(k_Intensity))
                amb.SetFloat(k_Intensity, value);

            if (surf != null && surf.HasFloat(k_Intensity))
                surf.SetFloat(k_Intensity, value);
        }
#else
        private IEnumerator FadeOutEntry(RealmVFXEntry entry) { yield break; }
        private IEnumerator FadeInEntry(RealmVFXEntry entry) { yield break; }
        private static void StartEntry(RealmVFXEntry entry) { }
        private static void StopEntry(RealmVFXEntry entry) { }
        private static void SetVFXIntensity(RealmVFXEntry entry, float value) { }
#endif

        // ------------------------------------------------------------------ //
        //  Atmosphere property sync
        // ------------------------------------------------------------------ //

        /// <summary>Push the realm's key colour into active VFX so they always match the atmosphere.</summary>
        public void SyncColourFromAtmosphere(RealmAtmosphereData data)
        {
#if HAVE_VFX_GRAPH
            if (_activeEntry == null) return;

            var amb = _activeEntry.ambientVFX as UnityEngine.VFX.VisualEffect;
            var surf = _activeEntry.surfaceVFX as UnityEngine.VFX.VisualEffect;

            if (amb != null && amb.HasVector4(k_Color))
                amb.SetVector4(k_Color, data.ambienceKeyColor);

            if (surf != null && surf.HasVector4(k_Color))
                surf.SetVector4(k_Color, data.ambienceKeyColor);
#endif
        }
    }
}
