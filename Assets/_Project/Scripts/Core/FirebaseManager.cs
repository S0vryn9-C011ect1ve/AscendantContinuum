using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AscendantContinuum.Core
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        [Header("Status")]
        [SerializeField] private bool isInitialized = false;
        [SerializeField] private bool isConnected = false;

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
            await Task.CompletedTask;
            isInitialized = false;
            isConnected = false;
            Debug.LogWarning("[FirebaseManager] Firebase SDK not present. Running in offline/no-backend mode.");
        }

        public async Task SignInAnonymously()
        {
            await Task.CompletedTask;
        }

        public async Task SavePlayerData(string collection, string documentId, object data)
        {
            await Task.CompletedTask;
        }

        public void SaveData(string collection, string documentId, object data)
        {
            _ = SavePlayerData(collection, documentId, data);
        }

        public async Task<T> LoadPlayerData<T>(string collection, string documentId) where T : class
        {
            await Task.CompletedTask;
            return null;
        }

        public async Task TrackEvent(string eventName, System.Collections.Generic.Dictionary<string, object> parameters = null)
        {
            await Task.CompletedTask;
        }

        public bool IsInitialized => isInitialized;
        public bool IsConnected => isConnected;
        public object CurrentUser => null;
        public string UserId => null;
    }
}
