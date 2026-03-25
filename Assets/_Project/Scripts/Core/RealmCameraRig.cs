using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CINEMACHINE_3_0_OR_NEWER
using Unity.Cinemachine;
#endif
using AscendantContinuum.Realms;

namespace AscendantContinuum.Cameras
{
    /// <summary>
    /// Manages Cinemachine virtual cameras for each realm.
    /// Place one Cinemachine Virtual Camera per realm in the scene, assign them here.
    /// Call ActivateRealmCamera(realmId) whenever the player enters a realm.
    /// </summary>
    public class RealmCameraRig : MonoBehaviour
    {
        [System.Serializable]
        public class RealmCameraEntry
        {
            [Tooltip("Must match RealmAtmosphereData.realmId exactly")]
            public string realmId;

#if CINEMACHINE_3_0_OR_NEWER
            [Tooltip("The Cinemachine Camera for this realm")]
            public CinemachineCamera virtualCamera;

            [Tooltip("Priority when this realm is active (inactive = 0)")]
            public int activePriority = 15;
#else
            [Tooltip("Cinemachine not installed — camera assignment disabled")]
            public GameObject virtualCamera;

            public int activePriority = 15;
#endif
        }

        [Header("Realm Cameras")]
        [SerializeField] private List<RealmCameraEntry> realmCameras = new List<RealmCameraEntry>();

        [Header("Transition")]
        [SerializeField, Tooltip("Cinemachine Brain blend time in seconds")]
        private float blendDuration = 1.5f;

        [Header("Cinematic Entry Shake")]
        [SerializeField] private bool playImpulseOnEntry = true;
        [SerializeField, Range(0.1f, 3f)] private float impulseStrength = 0.4f;

#if CINEMACHINE_3_0_OR_NEWER
    private CinemachineBrain _brain;
#else
        private object _brain;
#endif
        private RealmCameraEntry _activeEntry;

        private void Awake()
        {
#if CINEMACHINE_3_0_OR_NEWER
            _brain = UnityEngine.Object.FindFirstObjectByType<CinemachineBrain>();

            if (_brain != null)
                _brain.DefaultBlend = new CinemachineBlendDefinition(
                    CinemachineBlendDefinition.Styles.EaseInOut, blendDuration);

            // Ensure all cameras start with priority 0
            foreach (var entry in realmCameras)
                SetCameraPriority(entry.virtualCamera, 0);
#else
            Debug.Log("[RealmCameraRig] Cinemachine not installed. Camera transitions disabled.");
#endif
        }

        // ------------------------------------------------------------------ //
        //  Public API
        // ------------------------------------------------------------------ //

        /// <summary>Cross-fade to the virtual camera registered for this realmId.</summary>
        public void ActivateRealmCamera(string realmId)
        {
#if CINEMACHINE_3_0_OR_NEWER
            RealmCameraEntry target = realmCameras.Find(c => c.realmId == realmId);
            if (target == null)
            {
                Debug.LogWarning($"[RealmCameraRig] No camera entry for realmId: {realmId}");
                return;
            }

            // Deactivate previous
            if (_activeEntry != null && _activeEntry != target)
                SetCameraPriority(_activeEntry.virtualCamera, 0);

            // Activate target — Cinemachine Brain handles the blend automatically
            SetCameraPriority(target.virtualCamera, target.activePriority);

            if (playImpulseOnEntry)
                TriggerImpulse(target.virtualCamera);

            _activeEntry = target;
#else
            Debug.LogWarning("[RealmCameraRig] Cinemachine not installed. Camera switch skipped.");
#endif
        }

        /// <summary>Returns true if a blend is currently in progress.</summary>
        public bool IsBlending
        {
            get
            {
#if CINEMACHINE_3_0_OR_NEWER
                return _brain != null && _brain.IsBlending;
#else
                return false;
#endif
            }
        }

        /// <summary>Returns the current active realmId, or empty string if none.</summary>
        public string ActiveRealmId => _activeEntry?.realmId ?? string.Empty;

        // ------------------------------------------------------------------ //
        //  Helpers
        // ------------------------------------------------------------------ //

#if CINEMACHINE_3_0_OR_NEWER
        private static void SetCameraPriority(CinemachineCamera cam, int priority)
        {
            if (cam != null)
                cam.Priority = priority;
        }

        private void TriggerImpulse(CinemachineCamera cam)
        {
            if (cam == null) return;
            var impulse = cam.GetComponent<CinemachineImpulseSource>();
            if (impulse == null)
                impulse = cam.gameObject.AddComponent<CinemachineImpulseSource>();
            impulse.GenerateImpulse(impulseStrength);
        }
#else
        /// <summary>
        /// Without Cinemachine, "priority" maps to enable/disable so only one camera is active.
        /// </summary>
        private static void SetCameraPriority(GameObject cam, int priority)
        {
            if (cam == null) return;
            var camera = cam.GetComponent<Camera>();
            if (camera != null) camera.enabled = (priority > 0);
            else cam.SetActive(priority > 0);
        }

        /// <summary>
        /// Without Cinemachine, shakes Camera.main via a coroutine using perlin noise.
        /// </summary>
        private void TriggerImpulse(GameObject cam)
        {
            if (!playImpulseOnEntry) return;
            Camera target = (cam != null ? cam.GetComponent<Camera>() : null)
                            ?? Camera.main;
            if (target != null)
                StartCoroutine(ShakeCameraCoroutine(target.transform, impulseStrength, 0.35f));
        }

        private static IEnumerator ShakeCameraCoroutine(Transform camTransform, float magnitude, float duration)
        {
            Vector3 originalPos = camTransform.localPosition;
            float elapsed = 0f;
            float seed = UnityEngine.Random.value * 100f;

            while (elapsed < duration)
            {
                float fade   = 1f - (elapsed / duration);   // linear falloff
                float offsetX = (Mathf.PerlinNoise(seed + elapsed * 20f, 0f) * 2f - 1f) * magnitude * fade;
                float offsetY = (Mathf.PerlinNoise(0f, seed + elapsed * 20f) * 2f - 1f) * magnitude * fade;
                camTransform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            camTransform.localPosition = originalPos;
        }
#endif
    }
}
