# Technical Architecture - The Ascendant Continuum

**Version:** 1.0  
**Last Updated:** February 1, 2026  
**Status:** Foundation Documentation

---

## 📋 Table of Contents

1. [Technology Stack](#technology-stack)
2. [Unity Project Structure](#unity-project-structure)
3. [Backend Architecture](#backend-architecture)
4. [Procedural Generation Systems](#procedural-generation-systems)
5. [Accessibility Implementation](#accessibility-implementation)
6. [Performance Optimization](#performance-optimization)
7. [Cross-Platform Strategy](#cross-platform-strategy)
8. [Security & Privacy](#security--privacy)
9. [Analytics & Metrics](#analytics--metrics)
10. [Development Workflow](#development-workflow)

---

## 🛠️ Technology Stack

### Core Engine
**Unity 2022.3 LTS (Long Term Support)**
- **Why Unity:**
  - Robust cross-platform support (iOS, Android, PC, Console)
  - Mature accessibility plugin ecosystem
  - Strong 2D/3D hybrid capabilities (realms mix both)
  - Excellent procedural generation support
  - Large community and documentation

**C# Version:** .NET Standard 2.1

### Backend Services
**Firebase (Google)**
- **Authentication:** Firebase Auth (anonymous + optional Google/Apple Sign-In)
- **Database:** Cloud Firestore (NoSQL, real-time sync)
- **Functions:** Cloud Functions for Firebase (serverless logic)
- **Storage:** Firebase Storage (player-generated content, replays)
- **Analytics:** Firebase Analytics + Google Analytics 4
- **Remote Config:** Feature flags, A/B testing
- **Crashlytics:** Error tracking and crash reporting

### Additional Services
- **Unity Analytics:** In-game behavior tracking
- **Unity Cloud Build:** Automated builds (optional)
- **PlayFab (Optional):** Leaderboards, multiplayer (if Firebase insufficient)

---

## 🏗️ Unity Project Structure

### Folder Organization

```
Assets/
├── _Project/                       # All game-specific assets
│   ├── Scenes/
│   │   ├── Core/
│   │   │   ├── Nexus.unity        # Infinite Loop Nexus hub
│   │   │   ├── MainMenu.unity
│   │   │   └── Loading.unity
│   │   ├── Realms/
│   │   │   ├── Emberforge.unity
│   │   │   ├── VerdantSanctuary.unity
│   │   │   ├── EchoFields.unity
│   │   │   ├── DawnCitadel.unity
│   │   │   └── LanternAscension.unity
│   │   └── Debug/
│   │       └── TestingGround.unity
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs           # Singleton, persistent
│   │   │   ├── SaveSystem.cs            # Local + cloud save
│   │   │   ├── SceneTransition.cs       # Realm transitions
│   │   │   └── InputManager.cs          # Unified input handling
│   │   │
│   │   ├── Realms/
│   │   │   ├── Emberforge/
│   │   │   │   ├── EmberforgeCoreRitual.cs
│   │   │   │   ├── SparkSpawner.cs
│   │   │   │   └── FlameAnimation.cs
│   │   │   ├── VerdantSanctuary/
│   │   │   ├── EchoFields/
│   │   │   ├── DawnCitadel/
│   │   │   └── LanternAscension/
│   │   │
│   │   ├── Procedural/
│   │   │   ├── ProceduralGenerator.cs   # Base class
│   │   │   ├── SigilGenerator.cs        # Personal Sigil creation
│   │   │   ├── RitualVariation.cs       # Daily rituals
│   │   │   └── NoiseUtility.cs          # Perlin/Simplex helpers
│   │   │
│   │   ├── UI/
│   │   │   ├── AccessibilityMenu.cs     # Settings UI
│   │   │   ├── SigilDisplay.cs
│   │   │   ├── DailyConstellation.cs
│   │   │   └── TooltipManager.cs
│   │   │
│   │   ├── Accessibility/
│   │   │   ├── AccessibilityManager.cs  # Central controller
│   │   │   ├── ColorblindMode.cs
│   │   │   ├── ScreenReaderBridge.cs
│   │   │   ├── HapticController.cs
│   │   │   └── TextToSpeech.cs
│   │   │
│   │   ├── Audio/
│   │   │   ├── AudioManager.cs
│   │   │   ├── MusicCrossfader.cs
│   │   │   └── DynamicSoundscape.cs
│   │   │
│   │   ├── Social/
│   │   │   ├── RitualReplay.cs          # Video generation
│   │   │   ├── ShareManager.cs
│   │   │   └── AsyncPresence.cs         # Ghost players
│   │   │
│   │   ├── Backend/
│   │   │   ├── FirebaseManager.cs       # Firebase initialization
│   │   │   ├── CloudSaveManager.cs
│   │   │   ├── DailyChallenge.cs        # Fetch daily constellation
│   │   │   └── AnalyticsTracker.cs
│   │   │
│   │   └── Data/
│   │       ├── PlayerData.cs            # Serializable player state
│   │       ├── SigilData.cs
│   │       ├── RealmProgress.cs
│   │       └── AccessibilitySettings.cs
│   │
│   ├── Prefabs/
│   │   ├── Realms/
│   │   ├── UI/
│   │   ├── Effects/
│   │   └── NPCs/
│   │
│   ├── Materials/
│   │   ├── Emberforge/
│   │   ├── VerdantSanctuary/
│   │   └── Shared/
│   │
│   ├── Shaders/
│   │   ├── ColorblindShaders/          # Accessibility shaders
│   │   ├── ProceduralEffects/
│   │   └── Transitions/
│   │
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Ambient/
│   │
│   ├── Textures/
│   ├── Models/
│   ├── Animations/
│   │
│   └── Resources/
│       ├── Sigils/                     # Procedurally combined
│       ├── Localization/               # Multi-language support
│       └── RemoteConfig/               # Default values
│
├── Plugins/
│   ├── Firebase/                       # Firebase SDK
│   ├── Accessibility/                  # Unity Accessibility Plugin
│   └── Analytics/
│
└── Settings/
    ├── Input/                          # New Input System
    ├── Quality/                        # Platform-specific settings
    └── Audio/
```

---

## 🔥 Backend Architecture

### Firebase Integration

#### 1. **Authentication Flow**

```csharp
// FirebaseManager.cs
public class FirebaseManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        SignInAnonymously();
    }
    
    async void SignInAnonymously()
    {
        try
        {
            var result = await auth.SignInAnonymouslyAsync();
            currentUser = result.User;
            Debug.Log($"Signed in as: {currentUser.UserId}");
            LoadPlayerData();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Auth failed: {ex.Message}");
        }
    }
}
```

#### 2. **Cloud Firestore Schema**

```
firestore/
├── users/
│   └── {userId}/
│       ├── profile/
│       │   ├── createdAt: timestamp
│       │   ├── lastLogin: timestamp
│       │   ├── chosenDeity: string (null until Day 4)
│       │   ├── totalRituals: number
│       │   └── currentStreak: number
│       │
│       ├── progress/
│       │   ├── realms: map
│       │   │   ├── emberforge: {completed: 15, unlocked: true}
│       │   │   ├── verdantSanctuary: {completed: 8, unlocked: true}
│       │   │   └── ...
│       │   ├── sigils: array [sigilId1, sigilId2, ...]
│       │   └── personalSigil: {seed: number, created: timestamp}
│       │
│       ├── accessibility/
│       │   ├── colorblindMode: string ("none", "protanopia", etc.)
│       │   ├── screenReader: boolean
│       │   ├── reducedMotion: boolean
│       │   ├── fontSize: number
│       │   └── hapticIntensity: number (0-100)
│       │
│       └── social/
│           ├── constellationShares: number
│           ├── ritualReplaysGenerated: number
│           └── lanternsReleased: number
│
├── dailyChallenges/
│   └── {date}/                         # Format: "2026-02-01"
│       ├── seed: number                # Procedural seed
│       ├── theme: string               # "Growth", "Discovery", etc.
│       ├── targetPattern: array        # Constellation pattern
│       └── participants: number        # Completion count
│
├── lanterns/                           # Lantern Ascension realm
│   └── {lanternId}/
│       ├── userId: string (anonymized)
│       ├── wish: string (optional, moderated)
│       ├── createdAt: timestamp
│       ├── position: {x, y, z}
│       └── visibility: string ("private", "shared")
│
└── ritualReplays/                      # Shared replay links
    └── {replayId}/
        ├── userId: string
        ├── realmName: string
        ├── videoUrl: string (Firebase Storage)
        ├── duration: number
        ├── views: number
        └── createdAt: timestamp
```

#### 3. **Cloud Functions**

```javascript
// functions/index.js
const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp();

// Generate Daily Constellation Challenge
exports.generateDailyChallenge = functions.pubsub
    .schedule('every day 00:00')
    .timeZone('America/Los_Angeles')
    .onRun(async (context) => {
        const date = new Date().toISOString().split('T')[0];
        const seed = Math.floor(Math.random() * 1000000);
        const themes = ['Growth', 'Discovery', 'Connection', 'Joy', 'Reflection'];
        const theme = themes[seed % themes.length];
        
        await admin.firestore().collection('dailyChallenges').doc(date).set({
            seed: seed,
            theme: theme,
            targetPattern: generatePattern(seed),
            participants: 0,
            createdAt: admin.firestore.FieldValue.serverTimestamp()
        });
        
        console.log(`Daily challenge created: ${date}`);
    });

// Moderate Lantern Wishes
exports.moderateLantern = functions.firestore
    .document('lanterns/{lanternId}')
    .onCreate(async (snap, context) => {
        const lantern = snap.data();
        
        if (lantern.wish && lantern.visibility === 'shared') {
            // Check for inappropriate content (use ML or moderation API)
            const isSafe = await checkContent(lantern.wish);
            
            if (!isSafe) {
                await snap.ref.update({
                    visibility: 'private',
                    moderationFlag: 'inappropriate_content'
                });
            }
        }
    });

// Track Viral Metrics
exports.trackViralEvent = functions.https.onCall(async (data, context) => {
    const { eventType, userId } = data;
    
    await admin.firestore().collection('analytics').add({
        eventType: eventType, // 'constellation_share', 'replay_share', etc.
        userId: userId,
        timestamp: admin.firestore.FieldValue.serverTimestamp()
    });
    
    return { success: true };
});
```

---

## 🎲 Procedural Generation Systems

### 1. **Personal Sigil Generator**

```csharp
// SigilGenerator.cs
using UnityEngine;

public class SigilGenerator : MonoBehaviour
{
    public Texture2D GeneratePersonalSigil(int seed)
    {
        Random.InitState(seed);
        
        // 256x256 texture
        Texture2D sigil = new Texture2D(256, 256);
        
        // Generate 6 geometric elements
        for (int i = 0; i < 6; i++)
        {
            DrawElement(sigil, Random.Range(0, 5)); // 5 element types
        }
        
        // Add symmetry (50% chance)
        if (Random.value > 0.5f)
        {
            MirrorHorizontal(sigil);
        }
        
        // Add glow effect
        ApplyGlow(sigil);
        
        sigil.Apply();
        return sigil;
    }
    
    void DrawElement(Texture2D tex, int elementType)
    {
        switch (elementType)
        {
            case 0: DrawCircle(tex); break;
            case 1: DrawTriangle(tex); break;
            case 2: DrawSpiral(tex); break;
            case 3: DrawStar(tex); break;
            case 4: DrawRune(tex); break;
        }
    }
    
    // Additional helper methods...
}
```

### 2. **Daily Ritual Variation**

```csharp
// RitualVariation.cs
public class RitualVariation
{
    public static RitualConfig GenerateDailyRitual(string realmName, int seed)
    {
        System.Random rng = new System.Random(seed);
        
        RitualConfig config = new RitualConfig();
        
        switch (realmName)
        {
            case "Emberforge":
                config.sparkCount = 3 + rng.Next(0, 5);
                config.targetPattern = GeneratePattern(rng);
                config.difficulty = rng.Next(1, 4); // 1-3
                break;
                
            case "VerdantSanctuary":
                config.plantTypes = SelectPlants(rng, 2, 4);
                config.growthSpeed = 0.5f + (float)rng.NextDouble();
                break;
                
            case "EchoFields":
                config.orbCount = 5 + rng.Next(0, 8);
                config.constellationTheme = SelectTheme(rng);
                break;
                
            case "DawnCitadel":
                config.prismCount = 1 + rng.Next(0, 5);
                config.targetCount = 2 + rng.Next(0, 3);
                config.allowColorMixing = rng.Next(0, 2) == 1;
                break;
                
            case "LanternAscension":
                // No procedural variation (meditative, consistent)
                config.ambientIntensity = 0.7f + (float)rng.NextDouble() * 0.3f;
                break;
        }
        
        return config;
    }
}
```

### 3. **Noise-Based Realm Generation**

```csharp
// NoiseUtility.cs
using UnityEngine;

public static class NoiseUtility
{
    // Perlin noise for organic shapes (Verdant Sanctuary)
    public static float[,] GeneratePerlinNoise(int width, int height, float scale, int seed)
    {
        float[,] noiseMap = new float[width, height];
        System.Random rng = new System.Random(seed);
        
        float offsetX = rng.Next(-100000, 100000);
        float offsetY = rng.Next(-100000, 100000);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sampleX = (x / scale) + offsetX;
                float sampleY = (y / scale) + offsetY;
                
                noiseMap[x, y] = Mathf.PerlinNoise(sampleX, sampleY);
            }
        }
        
        return noiseMap;
    }
    
    // Voronoi diagram for Emberforge spark placement
    public static Vector2[] GenerateVoronoiPoints(int count, Bounds area, int seed)
    {
        System.Random rng = new System.Random(seed);
        Vector2[] points = new Vector2[count];
        
        for (int i = 0; i < count; i++)
        {
            points[i] = new Vector2(
                area.min.x + (float)rng.NextDouble() * area.size.x,
                area.min.y + (float)rng.NextDouble() * area.size.y
            );
        }
        
        return points;
    }
}
```

---

## ♿ Accessibility Implementation

### 1. **Accessibility Manager**

```csharp
// AccessibilityManager.cs
using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
    public static AccessibilityManager Instance;
    
    [Header("Current Settings")]
    public ColorblindMode colorblindMode = ColorblindMode.None;
    public bool screenReaderEnabled = false;
    public bool reducedMotion = false;
    public float textScale = 1.0f;
    public float hapticIntensity = 1.0f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetColorblindMode(ColorblindMode mode)
    {
        colorblindMode = mode;
        ApplyColorblindShader();
        SaveSettings();
    }
    
    void ApplyColorblindShader()
    {
        Material colorblindMaterial = null;
        
        switch (colorblindMode)
        {
            case ColorblindMode.Protanopia:
                colorblindMaterial = Resources.Load<Material>("Shaders/Protanopia");
                break;
            case ColorblindMode.Deuteranopia:
                colorblindMaterial = Resources.Load<Material>("Shaders/Deuteranopia");
                break;
            case ColorblindMode.Tritanopia:
                colorblindMaterial = Resources.Load<Material>("Shaders/Tritanopia");
                break;
        }
        
        if (colorblindMaterial != null)
        {
            Camera.main.GetComponent<ColorblindFilter>().SetMaterial(colorblindMaterial);
        }
    }
    
    public void Announce(string message)
    {
        if (screenReaderEnabled)
        {
            ScreenReaderBridge.Speak(message);
        }
    }
    
    public void TriggerHaptic(HapticPattern pattern)
    {
        if (hapticIntensity > 0)
        {
            HapticController.Play(pattern, hapticIntensity);
        }
    }
    
    void LoadSettings()
    {
        // Load from PlayerPrefs or cloud save
        colorblindMode = (ColorblindMode)PlayerPrefs.GetInt("ColorblindMode", 0);
        screenReaderEnabled = PlayerPrefs.GetInt("ScreenReader", 0) == 1;
        reducedMotion = PlayerPrefs.GetInt("ReducedMotion", 0) == 1;
        textScale = PlayerPrefs.GetFloat("TextScale", 1.0f);
        hapticIntensity = PlayerPrefs.GetFloat("HapticIntensity", 1.0f);
        
        ApplyAllSettings();
    }
    
    void SaveSettings()
    {
        PlayerPrefs.SetInt("ColorblindMode", (int)colorblindMode);
        PlayerPrefs.SetInt("ScreenReader", screenReaderEnabled ? 1 : 0);
        PlayerPrefs.SetInt("ReducedMotion", reducedMotion ? 1 : 0);
        PlayerPrefs.SetFloat("TextScale", textScale);
        PlayerPrefs.SetFloat("HapticIntensity", hapticIntensity);
        PlayerPrefs.Save();
        
        // Also save to cloud
        CloudSaveManager.Instance.SaveAccessibilitySettings(this);
    }
}

public enum ColorblindMode
{
    None,
    Protanopia,
    Deuteranopia,
    Tritanopia,
    Achromatopsia
}
```

### 2. **Screen Reader Integration**

```csharp
// ScreenReaderBridge.cs
#if UNITY_IOS
using UnityEngine.iOS;
#elif UNITY_ANDROID
using UnityEngine.Android;
#endif

public static class ScreenReaderBridge
{
    public static void Speak(string message, bool interrupt = false)
    {
#if UNITY_IOS
        // iOS VoiceOver
        iOSVoiceOver.Speak(message, interrupt);
#elif UNITY_ANDROID
        // Android TalkBack
        AndroidTalkBack.Speak(message, interrupt);
#elif UNITY_STANDALONE || UNITY_EDITOR
        // Desktop screen readers (NVDA, JAWS via Unity Accessibility Plugin)
        Debug.Log($"[SCREEN READER]: {message}");
        UnityAccessibility.Announce(message);
#endif
    }
    
    public static bool IsScreenReaderActive()
    {
#if UNITY_IOS
        return iOSVoiceOver.isRunning;
#elif UNITY_ANDROID
        return AndroidTalkBack.isRunning;
#else
        return false;
#endif
    }
}
```

---

## ⚡ Performance Optimization

### 1. **Object Pooling**

```csharp
// ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            
            poolDictionary.Add(pool.tag, objectPool);
        }
    }
    
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }
        
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        
        poolDictionary[tag].Enqueue(objectToSpawn);
        
        return objectToSpawn;
    }
}
```

### 2. **Level of Detail (LOD) for Particle Effects**

```csharp
// ParticleLOD.cs
using UnityEngine;

public class ParticleLOD : MonoBehaviour
{
    public ParticleSystem[] particleSystems;
    public float highQualityDistance = 10f;
    public float mediumQualityDistance = 30f;
    
    private Transform playerTransform;
    
    void Start()
    {
        playerTransform = Camera.main.transform;
    }
    
    void Update()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            
            if (distance < highQualityDistance)
            {
                main.maxParticles = 1000;
            }
            else if (distance < mediumQualityDistance)
            {
                main.maxParticles = 500;
            }
            else
            {
                main.maxParticles = 100;
            }
        }
    }
}
```

### 3. **Async Loading**

```csharp
// SceneTransition.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;
    
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.0f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadRealm(string realmName)
    {
        StartCoroutine(LoadRealmAsync(realmName));
    }
    
    IEnumerator LoadRealmAsync(string realmName)
    {
        // Fade out
        yield return StartCoroutine(Fade(1f));
        
        // Begin loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(realmName);
        asyncLoad.allowSceneActivation = false;
        
        // Wait until loaded (90%)
        while (asyncLoad.progress < 0.9f)
        {
            // Update loading bar if desired
            yield return null;
        }
        
        // Activate scene
        asyncLoad.allowSceneActivation = true;
        
        // Fade in
        yield return StartCoroutine(Fade(0f));
    }
    
    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvas.alpha;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        
        fadeCanvas.alpha = targetAlpha;
    }
}
```

---

## 📱 Cross-Platform Strategy

### Platform-Specific Builds

| Platform | Resolution | FPS Target | Build Size | Special Considerations |
|----------|-----------|-----------|-----------|------------------------|
| **iOS** | 1170x2532 (iPhone 14) | 60 FPS | <200MB | Metal API, VoiceOver support |
| **Android** | 1080x2400 (avg) | 60 FPS | <200MB | Vulkan/OpenGL ES 3.0, TalkBack |
| **PC (Windows)** | 1920x1080+ | 60 FPS | <500MB | Keyboard/mouse, NVDA/JAWS |
| **PC (macOS)** | Retina displays | 60 FPS | <500MB | Metal API, VoiceOver |
| **Console** | 1920x1080 (PS5/Xbox) | 60 FPS | <1GB | Controller-only, 4K support |

### Input System Configuration

```csharp
// InputManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public PlayerInput playerInput;
    
    void OnEnable()
    {
        // Automatically switch control schemes based on device
        playerInput.onControlsChanged += OnControlsChanged;
    }
    
    void OnControlsChanged(PlayerInput input)
    {
        string controlScheme = input.currentControlScheme;
        
        switch (controlScheme)
        {
            case "Touch":
                ConfigureTouchControls();
                break;
            case "Keyboard&Mouse":
                ConfigureKeyboardMouse();
                break;
            case "Gamepad":
                ConfigureGamepad();
                break;
        }
        
        AccessibilityManager.Instance.Announce($"Controls changed to {controlScheme}");
    }
    
    void ConfigureTouchControls()
    {
        // Show touch-specific UI
        // Adjust interaction radiuses
    }
    
    // Other configurations...
}
```

---

## 🔒 Security & Privacy

### 1. **Data Privacy**

```csharp
// Privacy principles
- All user IDs anonymized in public displays
- Lantern wishes opt-in for sharing
- No collection of personal information
- GDPR/CCPA compliant
- Parental consent for users under 13 (COPPA)
```

### 2. **Content Moderation**

```javascript
// Cloud Function for moderation
const { LanguageServiceClient } = require('@google-cloud/language');

async function checkContent(text) {
    const client = new LanguageServiceClient();
    
    const document = {
        content: text,
        type: 'PLAIN_TEXT',
    };
    
    // Sentiment analysis
    const [sentiment] = await client.analyzeSentiment({ document });
    
    // Block extremely negative content
    if (sentiment.documentSentiment.score < -0.8) {
        return false;
    }
    
    // Check for profanity (use perspective API or similar)
    // ... additional checks
    
    return true;
}
```

---

## 📊 Analytics & Metrics

### Key Performance Indicators (KPIs)

```csharp
// AnalyticsTracker.cs
public static class AnalyticsTracker
{
    public static void TrackRitualComplete(string realmName, float duration)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "ritual_complete",
            new Firebase.Analytics.Parameter("realm", realmName),
            new Firebase.Analytics.Parameter("duration", duration)
        );
    }
    
    public static void TrackViralShare(string shareType)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "viral_share",
            new Firebase.Analytics.Parameter("type", shareType) // constellation, replay, sigil
        );
    }
    
    public static void TrackAccessibilityFeature(string feature)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "accessibility_used",
            new Firebase.Analytics.Parameter("feature", feature)
        );
    }
}
```

### Tracked Events
- Ritual completions (realm, duration, difficulty)
- Daily Constellation participation
- Viral shares (constellation, replay, sigil)
- Accessibility feature usage
- Deity selection (Day 4)
- IAP purchases
- Session length
- Retention (D1, D7, D30)
- Churn indicators

---

## 🚀 Development Workflow

### Version Control (Git)

```
branches/
├── main                  # Production-ready
├── develop               # Integration branch
├── feature/*             # New features
├── bugfix/*              # Bug fixes
└── hotfix/*              # Emergency fixes
```

### CI/CD Pipeline

```yaml
# .github/workflows/unity-build.yml
name: Unity Build

on:
  push:
    branches: [ develop, main ]
  pull_request:
    branches: [ develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: game-ci/unity-builder@v2
        with:
          targetPlatform: StandaloneWindows64
      - uses: actions/upload-artifact@v2
        with:
          name: Build
          path: build
```

### Testing Strategy

```csharp
// Example unit test (Unity Test Framework)
using NUnit.Framework;
using UnityEngine;

public class SigilGeneratorTests
{
    [Test]
    public void SigilGenerator_SameSeed_ProducesSameResult()
    {
        SigilGenerator generator = new SigilGenerator();
        
        Texture2D sigil1 = generator.GeneratePersonalSigil(12345);
        Texture2D sigil2 = generator.GeneratePersonalSigil(12345);
        
        Assert.AreEqual(sigil1.GetPixel(128, 128), sigil2.GetPixel(128, 128));
    }
}
```

---

## 📦 Build Optimization

### Asset Bundles
- Realm-specific assets loaded on-demand
- Reduce initial download size
- Enable seasonal content updates

### Compression
- Texture compression (ASTC for mobile, BC7 for PC)
- Audio compression (Vorbis for music, ADPCM for SFX)
- Mesh optimization (polygon reduction, vertex compression)

### Code Stripping
- IL2CPP for mobile (better performance)
- Remove unused code (Managed Stripping Level: Medium)

---

## 🔮 Future Technical Considerations

### Scalability
- Firestore can handle millions of users
- Cloud Functions auto-scale
- Consider CDN for static assets (Firebase Hosting)

### Localization
- Unity Localization Package
- Support 10+ languages at launch
- Right-to-left (RTL) text support (Arabic, Hebrew)

### Advanced Features (Post-Launch)
- Multiplayer rituals (real-time collaboration)
- AR mode (place realms in real world)
- Voice control (fully hands-free accessibility)

---

**End of Technical Architecture**

*Next: Backend API Specification*  
*Contact: ascendantcontinuum@gmail.com*
