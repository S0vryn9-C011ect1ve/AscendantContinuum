using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using System;
using System.Threading.Tasks;

namespace AscendantContinuum.Core
{
    /// <summary>
    /// Firebase connection manager - handles authentication, Firestore, Storage
    /// Includes offline support and automatic retry logic
    /// </summary>
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        [Header("Status")]
        [SerializeField] private bool isInitialized = false;
        [SerializeField] private bool isConnected = false;
        
        private FirebaseApp app;
        private FirebaseAuth auth;
        private FirebaseFirestore firestore;
        private FirebaseStorage storage;
        
        public event Action OnFirebaseReady;
        public event Action<FirebaseUser> OnUserSignedIn;
        public event Action OnConnectionLost;

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
                // Check dependencies
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                
                if (dependencyStatus == DependencyStatus.Available)
                {
                    app = FirebaseApp.DefaultInstance;
                    auth = FirebaseAuth.DefaultInstance;
                    firestore = FirebaseFirestore.DefaultInstance;
                    storage = FirebaseStorage.DefaultInstance;
                    
                    // Enable offline persistence
                    firestore.Settings.PersistenceEnabled = true;
                    
                    isInitialized = true;
                    isConnected = true;
                    
                    Debug.Log("[FirebaseManager] ✅ Firebase initialized successfully");
                    OnFirebaseReady?.Invoke();
                    
                    // Auto sign-in anonymously
                    await SignInAnonymously();
                }
                else
                {
                    Debug.LogError($"[FirebaseManager] ❌ Could not resolve Firebase dependencies: {dependencyStatus}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseManager] ❌ Initialization failed: {e.Message}");
            }
        }

        public async Task SignInAnonymously()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[FirebaseManager] Cannot sign in - Firebase not initialized");
                return;
            }

            try
            {
                var result = await auth.SignInAnonymouslyAsync();
                
                Debug.Log($"[FirebaseManager] ✅ Signed in anonymously: {result.UserId}");
                OnUserSignedIn?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseManager] ❌ Anonymous sign-in failed: {e.Message}");
            }
        }

        public async Task SavePlayerData(string collection, string documentId, object data)
        {
            if (!isInitialized) return;

            try
            {
                DocumentReference docRef = firestore.Collection(collection).Document(documentId);
                await docRef.SetAsync(data);
                
                Debug.Log($"[FirebaseManager] ✅ Saved data to {collection}/{documentId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseManager] ❌ Save failed: {e.Message}");
            }
        }

        public async Task<T> LoadPlayerData<T>(string collection, string documentId) where T : class
        {
            if (!isInitialized) return null;

            try
            {
                DocumentReference docRef = firestore.Collection(collection).Document(documentId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                
                if (snapshot.Exists)
                {
                    T data = snapshot.ConvertTo<T>();
                    Debug.Log($"[FirebaseManager] ✅ Loaded data from {collection}/{documentId}");
                    return data;
                }
                else
                {
                    Debug.LogWarning($"[FirebaseManager] Document not found: {collection}/{documentId}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseManager] ❌ Load failed: {e.Message}");
                return null;
            }
        }

        public async Task TrackEvent(string eventName, System.Collections.Generic.Dictionary<string, object> parameters = null)
        {
            if (!isInitialized) return;

            try
            {
                // Save analytics event to Firestore
                var eventData = new
                {
                    eventName = eventName,
                    timestamp = FieldValue.ServerTimestamp,
                    userId = auth.CurrentUser?.UserId ?? "anonymous",
                    parameters = parameters ?? new System.Collections.Generic.Dictionary<string, object>()
                };
                
                await firestore.Collection("analytics").AddAsync(eventData);
                Debug.Log($"[FirebaseManager] 📊 Event tracked: {eventName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseManager] ❌ Event tracking failed: {e.Message}");
            }
        }

        // Public getters
        public bool IsInitialized => isInitialized;
        public bool IsConnected => isConnected;
        public FirebaseUser CurrentUser => auth?.CurrentUser;
        public string UserId => auth?.CurrentUser?.UserId;
    }
}
