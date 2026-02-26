using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Core
{
    using AscendantContinuum.Realms;
    using AscendantContinuum.Audio;

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

        /// <summary>
        /// Called during realm enter - applies atmosphere
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

            // Set ambience layers via AudioManager if available
            if (AudioManager.Instance != null)
            {
                // Extract ambience blend from atmosphere
                AudioManager.Instance.SetAmbienceLayers(
                    cosmicIntensity: realmAtmosphere.ambienceIntensity * 0.8f,
                    tonalIntensity: realmAtmosphere.ambienceIntensity * 0.9f,
                    windIntensity: realmAtmosphere.ambienceIntensity * 0.6f,
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
