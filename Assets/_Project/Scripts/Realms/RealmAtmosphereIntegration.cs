using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Core
{
    using AscendantContinuum.Realms;
    using AscendantContinuum.Audio;
    using AscendantContinuum.VFX;
    using AscendantContinuum.Cameras;

    /// <summary>
    /// Extension to integrate realm atmosphere into realm controller lifecycle
    /// Add this to the RealmController base class or use as a mixin
    /// </summary>
    public abstract partial class RealmController : MonoBehaviour
    {
        [SerializeField] protected RealmAtmosphereData realmAtmosphere;
        [SerializeField] protected bool autoApplyAtmosphereOnEnter = true;
        [SerializeField] protected float atmosphereTransitionDuration = 2f;

        protected RealmAtmosphereController atmosphereController;

        // Cached singleton-style lookups (populated once, null-safe)
        private static RealmVFXManager   _vfxManager;
        private static RealmCameraRig    _cameraRig;

        /// <summary>
        /// Called during realm enter - applies atmosphere, VFX, and camera.
        /// </summary>
        protected virtual void ApplyRealmAtmosphere()
        {
            if (realmAtmosphere == null)
            {
                Debug.LogWarning($"[RealmAtmosphere] No atmosphere data assigned to {gameObject.name}");
                return;
            }

            // Get or create atmosphere controller
            if (atmosphereController == null)
                atmosphereController = GetComponent<RealmAtmosphereController>();

            if (atmosphereController == null)
                atmosphereController = gameObject.AddComponent<RealmAtmosphereController>();

            // Apply atmosphere
            atmosphereController.ApplyAtmosphere(realmAtmosphere);

            // Activate matching VFX
            if (_vfxManager == null) _vfxManager = Object.FindObjectOfType<RealmVFXManager>();
            if (_vfxManager != null)
            {
                _vfxManager.ActivateRealm(realmAtmosphere.realmId);
                _vfxManager.SyncColourFromAtmosphere(realmAtmosphere);
                _vfxManager.PlayEntryBurst();
            }

            // Switch Cinemachine camera
            if (_cameraRig == null) _cameraRig = Object.FindObjectOfType<RealmCameraRig>();
            if (_cameraRig != null)
                _cameraRig.ActivateRealmCamera(realmAtmosphere.realmId);

            // Set ambience layers via AudioManager if available
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetAmbienceLayers(
                    cosmicIntensity: realmAtmosphere.ambienceIntensity * 0.8f,
                    tonalIntensity:  realmAtmosphere.ambienceIntensity * 0.9f,
                    windIntensity:   realmAtmosphere.ambienceIntensity * 0.6f,
                    reverbIntensity: realmAtmosphere.ambienceIntensity
                );
            }
        }

        /// <summary>
        /// Transition to new realm atmosphere smoothly
        /// </summary>
        public void TransitionToAtmosphere(RealmAtmosphereData targetAtmosphere)
        {
            if (atmosphereController == null)
                atmosphereController = GetComponent<RealmAtmosphereController>();

            if (atmosphereController != null)
                atmosphereController.TransitionToAtmosphere(targetAtmosphere, atmosphereTransitionDuration);
        }

        /// <summary>
        /// Get current realm atmosphere
        /// </summary>
        public RealmAtmosphereData GetAtmosphere() => realmAtmosphere;
    }
}
