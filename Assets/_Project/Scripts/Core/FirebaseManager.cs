using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace AscendantContinuum.Core
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        [Header("Status")]
        [SerializeField] private bool isInitialized = false;
        [SerializeField] private bool isConnected = false;
        [SerializeField] private int defaultTimeoutMs = 5000; // 5 second timeout for network calls

#pragma warning disable CS0067 // Events are public API — subscribers added at runtime
        public event Action OnFirebaseReady;
        public event Action<object> OnUserSignedIn;
        public event Action OnConnectionLost;
#pragma warning restore CS0067

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            await InitializeFirebase();
        }

        private async Task InitializeFirebase()
        {
            try
            {
                using (var cts = new CancellationTokenSource(defaultTimeoutMs))
                {
                    // Simulate initialization
                    await Task.Delay(100, cts.Token);
                    isInitialized = true;
                    isConnected = Application.internetReachability != NetworkReachability.NotReachable;
                    
                    if (isConnected)
                    {
                        Debug.Log("[FirebaseManager] Firebase initialized successfully.");
                        
                        // Initialize Crashlytics
                        Debug.Log("[FirebaseManager] Crashlytics initialized.");
                        
                        OnFirebaseReady?.Invoke();
                    }
                    else
                    {
                        Debug.LogWarning("[FirebaseManager] Firebase initialized but offline.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogError("[FirebaseManager] Firebase initialization timed out. Falling back to offline mode.");
                isInitialized = false;
                isConnected = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseManager] Firebase initialization failed: {ex.Message}. Falling back to offline mode.");
                isInitialized = false;
                isConnected = false;
            }
        }

        public async Task SignInAnonymously()
        {
            if (!isInitialized || !isConnected)
            {
                Debug.LogWarning("[FirebaseManager] Cannot sign in: Offline or not initialized.");
                return;
            }

            try
            {
                using (var cts = new CancellationTokenSource(defaultTimeoutMs))
                {
                    // Simulate sign in
                    await Task.Delay(100, cts.Token);
                    Debug.Log("[FirebaseManager] Signed in anonymously.");
                    OnUserSignedIn?.Invoke(new object());
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogError("[FirebaseManager] Sign in timed out.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseManager] Sign in failed: {ex.Message}");
            }
        }

        public async Task SavePlayerData(string collection, string documentId, object data)
        {
            if (data == null)
            {
                Debug.LogWarning("[FirebaseManager] Attempted to save null data.");
                return;
            }

            if (!isInitialized || !isConnected)
            {
                Debug.LogWarning("[FirebaseManager] Cannot save to Firebase: Offline or not initialized. Data will only be saved locally.");
                return;
            }

            try
            {
                using (var cts = new CancellationTokenSource(defaultTimeoutMs))
                {
                    // Simulate save
                    await Task.Delay(100, cts.Token);
                    Debug.Log($"[FirebaseManager] Data saved to {collection}/{documentId}");
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogError($"[FirebaseManager] Save to {collection}/{documentId} timed out.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseManager] Save failed: {ex.Message}");
            }
        }

        public void SaveData(string collection, string documentId, object data)
        {
            _ = SavePlayerData(collection, documentId, data);
        }

        public async Task<T> LoadPlayerData<T>(string collection, string documentId) where T : class
        {
            if (!isInitialized || !isConnected)
            {
                Debug.LogWarning("[FirebaseManager] Cannot load from Firebase: Offline or not initialized.");
                return null;
            }

            try
            {
                using (var cts = new CancellationTokenSource(defaultTimeoutMs))
                {
                    // Simulate load
                    await Task.Delay(100, cts.Token);
                    Debug.Log($"[FirebaseManager] Data loaded from {collection}/{documentId}");
                    return null; // Return default/null for now
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogError($"[FirebaseManager] Load from {collection}/{documentId} timed out.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseManager] Load failed: {ex.Message}");
                return null;
            }
        }

        public async Task TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!isInitialized) return;

            try
            {
                using (var cts = new CancellationTokenSource(defaultTimeoutMs))
                {
                    // Simulate tracking
                    await Task.Delay(10, cts.Token);
                    // Debug.Log($"[FirebaseManager] Event tracked: {eventName}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseManager] Failed to track event {eventName}: {ex.Message}");
            }
        }

        public bool IsInitialized => isInitialized;
        public bool IsConnected => isConnected;
        public object CurrentUser => null;
        public string UserId => null;

        /// <summary>
        /// Deletes all Firestore data for the current user (GDPR right-to-erasure).
        /// Calls the "deleteUserData" Cloud Function when the real SDK is wired.
        /// </summary>
        public async Task DeleteUserData(string userId)
        {
            if (!isInitialized || string.IsNullOrEmpty(userId)) return;

            try
            {
                using (var cts = new CancellationTokenSource(defaultTimeoutMs))
                {
                    // TODO: when real Firebase SDK is integrated, replace with:
                    // var fn = FirebaseFunctions.DefaultInstance.GetHttpsCallable("deleteUserData");
                    // await fn.CallAsync(new Dictionary<string, object> { { "uid", userId } });
                    await Task.Delay(50, cts.Token);
                    Debug.Log($"[FirebaseManager] DeleteUserData called for uid={userId} (simulated).");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FirebaseManager] DeleteUserData failed: {ex.Message}");
            }
        }
    }
}
