# Optimization & Security Guide - The Ascendant Continuum

**Version:** 1.0  
**Last Updated:** February 1, 2026  
**Status:** Technical Foundation  
**Repository:** https://github.com/ascendantcontinuum/AscendantContinuum.git

---

## 📋 Table of Contents

1. [File Size Optimization](#file-size-optimization)
2. [Performance Optimization](#performance-optimization)
3. [Security & Protection](#security--protection)
4. [Accessibility-First Performance](#accessibility-first-performance)
5. [Build Configuration](#build-configuration)
6. [Target Metrics](#target-metrics)

---

## 📦 File Size Optimization

### Target Goals
- **iOS:** 150-200 MB initial download
- **Android:** 120-180 MB initial download
- **WebGL:** 50-80 MB initial load
- **Runtime Memory:** < 300 MB on mid-range devices (2GB RAM)

### Critical Strategy: Asset Optimization

#### 1. **Texture Compression**
```csharp
// Unity Texture Import Settings
- Format: ASTC (iOS/Android), DXT5 (PC)
- Max Size: 2048x2048 for hero assets, 512x512 for UI
- Compression: High Quality (not Normal Quality - looks bad)
- Mipmaps: Enabled for 3D, disabled for UI
- Read/Write: DISABLED (saves RAM)
```

**Specific Settings:**
```
Realm Backgrounds (4K source):
  → Compress to 2048x2048 ASTC 6x6 (iOS)
  → Size: 1.3 MB → 340 KB (74% reduction)

Sigil Icons (512x512 source):
  → Compress to 512x512 ASTC 4x4
  → Size: 1 MB → 170 KB (83% reduction)

Particle Textures:
  → Atlas multiple into 1024x1024 sheets
  → ASTC 8x8 (aggressive, particles are small)
  → Size: 500 KB → 85 KB per sheet
```

#### 2. **Audio Compression**
```
Background Music (Ambient loops):
  - Format: Vorbis (OGG)
  - Quality: 0.5-0.7 (indistinguishable from original)
  - Streaming: YES (don't load into memory)
  - File size: 5 MB → 1.2 MB per track

Sound Effects (Short clips):
  - Format: ADPCM (iOS) / Vorbis (Android)
  - Load Type: Decompress on Load
  - Max 200 KB per effect
  
Ritual Success Chimes:
  - Ultra-short (< 1 second)
  - ADPCM, mono, 22 kHz
  - 50-80 KB each
```

#### 3. **Mesh Optimization**
```
Procedural Generation Reduces File Size:
  - Realms are procedurally generated, not baked
  - Store seed parameters (bytes) instead of full meshes (MB)
  
Example:
  Emberforge Spark Cluster:
    - NOT stored: 500 pre-made spark meshes (50 MB)
    - STORED: Perlin noise seed + generation rules (2 KB)
    - Generated at runtime using compute shaders

Particle Effects:
  - Use Unity's Particle System (GPU-instanced)
  - Single quad mesh shared by 10,000 particles
  - File size: 1 KB mesh, 200 KB texture atlas
```

#### 4. **Code Optimization**
```csharp
// Use IL2CPP (C++ compiled), not Mono
// Build Settings → Scripting Backend: IL2CPP
// Result: 30-40% smaller binary, faster execution

// Strip unnecessary Unity modules
// Project Settings → Player → Other Settings
// Disable:
  - Cloth Physics (not used)
  - Terrain Engine (not used)
  - Unity Ads (we use custom)
  - Multiplayer HLAPI (using Firebase)
  
// Result: ~15 MB saved
```

#### 5. **Asset Bundling Strategy**
```
Download on Demand (Post-Launch):
  - Core Game (Required): 100 MB
    → Main Menu, Nexus, Emberforge only
    
  - Realm 2-5 Bundles (Optional): 15 MB each
    → Downloaded when player unlocks realm
    → Cached locally, never re-download
    
  - Seasonal Content: 5-10 MB per season
    → New sigils, cosmetics, challenges
    
Total Initial Download: 100 MB
Total Full Game: 180 MB (if all realms unlocked)
```

**Unity Implementation:**
```csharp
// AssetBundle structure
Assets/AssetBundles/
├── core.bundle              // 100 MB
├── realm_verdant.bundle     // 15 MB
├── realm_echo.bundle        // 15 MB
├── realm_dawn.bundle        // 15 MB
├── realm_lantern.bundle     // 15 MB
└── seasonal_spring2026.bundle // 8 MB

// Download when needed
async Task DownloadRealm(string realmName)
{
    string url = $"https://storage.googleapis.com/ascendant-bundles/{realmName}.bundle";
    var request = UnityWebRequestAssetBundle.GetAssetBundle(url);
    
    await request.SendWebRequest();
    if (request.result == UnityWebRequest.Result.Success)
    {
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        // Cache for offline play
        Caching.AddCache(bundle);
    }
}
```

---

## ⚡ Performance Optimization

### Target Performance (Accessibility-Critical)
- **Frame Rate:** 60 FPS minimum (accessibility requirement)
- **Input Latency:** < 50ms tap-to-response
- **Load Times:** < 3 seconds between realms
- **Battery Usage:** 15-20% per hour (mobile)

### 1. **GPU Optimization**

#### Batching Strategy
```csharp
// Static Batching for Realm Environment
// Mark all non-moving realm elements as static
// Unity automatically batches into single draw call

GameObject realm = FindRealm("Emberforge");
foreach (Transform child in realm.GetComponentsInChildren<Transform>())
{
    if (child.name.Contains("Background") || child.name.Contains("Decoration"))
    {
        child.gameObject.isStatic = true; // Batched!
    }
}

// Result: 500 draw calls → 5 draw calls
```

#### Particle Optimization
```csharp
// ParticleSystem settings for 10,000 particles at 60 FPS
ParticleSystem sparks = GetComponent<ParticleSystem>();

var main = sparks.main;
main.maxParticles = 10000;
main.simulationSpace = ParticleSystemSimulationSpace.World; // GPU instancing

var renderer = sparks.GetComponent<ParticleSystemRenderer>();
renderer.renderMode = ParticleSystemRenderMode.Billboard;
renderer.enableGPUInstancing = true; // CRITICAL for performance

// Result: 60 FPS even on iPhone 8 (2017 device)
```

#### Shader Optimization
```glsl
// Custom lightweight shader for glowing effects
Shader "Ascendant/GlowEfficient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,0,1)
        _GlowIntensity ("Intensity", Range(0,2)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        // Single pass, no multi-pass glow (expensive!)
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // GPU instancing
            
            // Minimal fragment shader
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _GlowColor.rgb * _GlowIntensity;
                return col;
            }
            ENDCG
        }
    }
}
```

### 2. **CPU Optimization**

#### Object Pooling (Critical!)
```csharp
// DO NOT Instantiate() and Destroy() particles
// Use object pooling to eliminate GC spikes

public class SigilPool : MonoBehaviour
{
    public GameObject sigilPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private const int INITIAL_SIZE = 100;
    
    void Start()
    {
        // Pre-warm pool
        for (int i = 0; i < INITIAL_SIZE; i++)
        {
            GameObject obj = Instantiate(sigilPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    
    public GameObject GetSigil()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Pool exhausted, create new (rare)
            return Instantiate(sigilPrefab);
        }
    }
    
    public void ReturnSigil(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}

// Result: No GC spikes, consistent frame rate
```

#### LOD (Level of Detail) for Particles
```csharp
// Reduce particle count on low-end devices
public class AdaptiveParticles : MonoBehaviour
{
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        
        // Detect device performance
        int deviceTier = GetDeviceTier(); // 1=Low, 2=Mid, 3=High
        
        switch (deviceTier)
        {
            case 1: // Low-end (iPhone 8, Galaxy S8)
                main.maxParticles = 2000;
                break;
            case 2: // Mid-range (iPhone 11, Galaxy S20)
                main.maxParticles = 5000;
                break;
            case 3: // High-end (iPhone 15, Galaxy S23)
                main.maxParticles = 10000;
                break;
        }
    }
    
    int GetDeviceTier()
    {
        // Check SystemInfo.graphicsMemorySize, processor count
        int memoryMB = SystemInfo.graphicsMemorySize;
        
        if (memoryMB < 1500) return 1; // Low
        if (memoryMB < 3000) return 2; // Mid
        return 3; // High
    }
}
```

### 3. **Memory Optimization**

#### Texture Streaming
```csharp
// Enable Texture Streaming to load only visible textures
// Project Settings → Quality → Texture Streaming
// Memory Budget: 512 MB (mobile), 1 GB (PC)

// Result: Game uses 200 MB RAM instead of 800 MB
```

#### Garbage Collection Management
```csharp
// Avoid allocations in Update() loop
public class EfficientRitual : MonoBehaviour
{
    // BAD - creates garbage every frame
    void Update_BAD()
    {
        Vector3 position = transform.position; // Allocates!
        string message = "Ritual progress: " + progress; // Allocates!
    }
    
    // GOOD - cache references, use StringBuilder
    private Vector3 cachedPosition;
    private StringBuilder messageBuilder = new StringBuilder(50);
    
    void Update_GOOD()
    {
        cachedPosition = transform.position; // No allocation
        messageBuilder.Clear();
        messageBuilder.Append("Ritual progress: ");
        messageBuilder.Append(progress);
        // No garbage created!
    }
}
```

---

## 🔒 Security & Protection

### Threat Model
1. **Cheating/Hacking** - Modified client, time manipulation
2. **Data Theft** - User profiles, progression data
3. **DDoS Attacks** - Overwhelming backend services
4. **Injection Attacks** - Malicious input data
5. **Account Takeover** - Stolen anonymous accounts

### 1. **Client-Side Security**

#### Code Obfuscation
```csharp
// Use Unity IL2CPP + Code Stripping
// Build Settings → Scripting Backend: IL2CPP
// Project Settings → Player → Optimization → Managed Stripping Level: High

// Add anti-tampering checks
public class SecurityManager : MonoBehaviour
{
    private const string BUILD_SIGNATURE = "ASCENDANT_BUILD_2026_v1";
    
    void Start()
    {
        // Verify build integrity
        if (!VerifyBuildSignature())
        {
            Debug.LogError("Build tampering detected!");
            Application.Quit();
        }
        
        // Detect rooted/jailbroken devices
        if (IsDeviceCompromised())
        {
            ShowWarning("Playing on modified device - some features disabled");
            DisableLeaderboards();
        }
    }
    
    bool IsDeviceCompromised()
    {
        #if UNITY_IOS
        return System.IO.File.Exists("/Applications/Cydia.app");
        #elif UNITY_ANDROID
        return System.IO.File.Exists("/system/app/Superuser.apk");
        #else
        return false;
        #endif
    }
}
```

#### Secure PlayerPrefs
```csharp
// DO NOT store sensitive data in plain PlayerPrefs
// Use encrypted storage for offline progression

using System.Security.Cryptography;
using System.Text;

public class SecureStorage
{
    private static byte[] encryptionKey = Encoding.UTF8.GetBytes("UNIQUE_32_CHAR_KEY_HERE_12345"); // Change this!
    
    public static void SaveSecure(string key, string value)
    {
        string encrypted = Encrypt(value, encryptionKey);
        PlayerPrefs.SetString(key, encrypted);
    }
    
    public static string LoadSecure(string key)
    {
        string encrypted = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(encrypted)) return "";
        
        return Decrypt(encrypted, encryptionKey);
    }
    
    private static string Encrypt(string plainText, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV();
            
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encrypted = encryptor.TransformFinalBlock(
                Encoding.UTF8.GetBytes(plainText), 0, plainText.Length
            );
            
            // Prepend IV to encrypted data
            byte[] result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            
            return Convert.ToBase64String(result);
        }
    }
    
    private static string Decrypt(string cipherText, byte[] key)
    {
        byte[] buffer = Convert.FromBase64String(cipherText);
        
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            
            // Extract IV from beginning
            byte[] iv = new byte[aes.IV.Length];
            byte[] encrypted = new byte[buffer.Length - iv.Length];
            
            Buffer.BlockCopy(buffer, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(buffer, iv.Length, encrypted, 0, encrypted.Length);
            
            aes.IV = iv;
            
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
```

### 2. **Backend Security (Firebase)**

#### Firestore Security Rules
```javascript
// firestore.rules - CRITICAL for data protection
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    
    // Helper functions
    function isAuthenticated() {
      return request.auth != null;
    }
    
    function isOwner(userId) {
      return isAuthenticated() && request.auth.uid == userId;
    }
    
    function rateLimit(limit) {
      // Prevent spam - max `limit` writes per minute
      return request.time > resource.data.lastWrite + duration.value(60/limit, 's');
    }
    
    // User profiles - only owner can write
    match /users/{userId} {
      allow read: if isAuthenticated();
      allow write: if isOwner(userId) && rateLimit(10); // Max 10 updates/min
      
      // Prevent cheating - validate progression
      allow update: if isOwner(userId) 
        && request.resource.data.totalRituals >= resource.data.totalRituals // Can't decrease
        && request.resource.data.totalRituals <= resource.data.totalRituals + 50; // Max +50 per update (anti-cheat)
    }
    
    // Daily challenges - read-only for clients
    match /dailyChallenges/{challengeId} {
      allow read: if isAuthenticated();
      allow write: if false; // Only Cloud Functions can write
    }
    
    // Challenge submissions - owner can write, rate-limited
    match /challengeSubmissions/{submissionId} {
      allow read: if isAuthenticated();
      allow create: if isOwner(request.resource.data.userId) 
        && rateLimit(5); // Max 5 submissions per minute
      allow update, delete: if false; // Immutable after creation
    }
    
    // Lantern Ascension messages - read-only
    match /lanternMessages/{messageId} {
      allow read: if isAuthenticated();
      allow write: if false; // Only approved by Cloud Functions
    }
    
    // Global stats - read-only
    match /globalStats/{statsId} {
      allow read: if true; // Public
      allow write: if false; // Only Cloud Functions
    }
  }
}
```

#### Cloud Functions Security
```javascript
// functions/index.js - Server-side validation
const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp();

// Anti-cheat: Validate ritual completion time
exports.completeRitual = functions.https.onCall(async (data, context) => {
    // Ensure authenticated
    if (!context.auth) {
        throw new functions.https.HttpsError('unauthenticated', 'Must be logged in');
    }
    
    const userId = context.auth.uid;
    const { ritualId, duration, score } = data;
    
    // Validation: Ritual can't complete in < 5 seconds (impossible)
    if (duration < 5) {
        console.warn(`Cheat attempt by ${userId}: duration=${duration}s`);
        throw new functions.https.HttpsError('invalid-argument', 'Ritual too fast');
    }
    
    // Validation: Score can't exceed maximum possible
    const MAX_SCORE = 1000;
    if (score > MAX_SCORE) {
        console.warn(`Cheat attempt by ${userId}: score=${score}`);
        throw new functions.https.HttpsError('invalid-argument', 'Invalid score');
    }
    
    // Rate limiting: Max 100 rituals per day
    const userRef = admin.firestore().collection('users').doc(userId);
    const userDoc = await userRef.get();
    const userData = userDoc.data();
    
    const today = new Date().toDateString();
    if (userData.lastRitualDate === today && userData.ritualsToday >= 100) {
        throw new functions.https.HttpsError('resource-exhausted', 'Daily limit reached');
    }
    
    // Update user progression (server-authoritative)
    await userRef.update({
        totalRituals: admin.firestore.FieldValue.increment(1),
        ritualsToday: userData.lastRitualDate === today 
            ? admin.firestore.FieldValue.increment(1) 
            : 1,
        lastRitualDate: today
    });
    
    return { success: true, reward: calculateReward(score) };
});

// DDoS Protection: Rate limiting by IP
exports.getRitualData = functions.runWith({
    memory: '256MB',
    timeoutSeconds: 10
}).https.onCall(async (data, context) => {
    // Get caller IP
    const ip = context.rawRequest.ip;
    
    // Check rate limit in Firestore
    const rateLimitRef = admin.firestore().collection('rateLimits').doc(ip);
    const rateLimitDoc = await rateLimitRef.get();
    
    const now = Date.now();
    const WINDOW_MS = 60 * 1000; // 1 minute
    const MAX_REQUESTS = 60; // 60 requests per minute
    
    if (rateLimitDoc.exists) {
        const data = rateLimitDoc.data();
        if (now - data.windowStart < WINDOW_MS) {
            if (data.requestCount >= MAX_REQUESTS) {
                throw new functions.https.HttpsError(
                    'resource-exhausted',
                    'Rate limit exceeded'
                );
            }
            await rateLimitRef.update({
                requestCount: admin.firestore.FieldValue.increment(1)
            });
        } else {
            // New window
            await rateLimitRef.set({ windowStart: now, requestCount: 1 });
        }
    } else {
        await rateLimitRef.set({ windowStart: now, requestCount: 1 });
    }
    
    // Return data...
});
```

### 3. **Network Security**

#### HTTPS Only
```csharp
// Unity WebRequest - enforce HTTPS
public class SecureAPI : MonoBehaviour
{
    private const string API_BASE = "https://us-central1-ascendant-continuum.cloudfunctions.net";
    
    async Task<string> CallAPI(string endpoint, string jsonData)
    {
        string url = $"{API_BASE}/{endpoint}";
        
        // Ensure HTTPS
        if (!url.StartsWith("https://"))
        {
            Debug.LogError("HTTP not allowed! Use HTTPS only.");
            return null;
        }
        
        UnityWebRequest request = UnityWebRequest.Post(url, jsonData);
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Add Firebase auth token
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        string idToken = await user.TokenAsync(false);
        request.SetRequestHeader("Authorization", $"Bearer {idToken}");
        
        await request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            return request.downloadHandler.text;
        }
        else
        {
            Debug.LogError($"API Error: {request.error}");
            return null;
        }
    }
}
```

#### Certificate Pinning (Advanced)
```csharp
// Prevent man-in-the-middle attacks
using System.Security.Cryptography.X509Certificates;

public class CertificatePinning : CertificateHandler
{
    // Google's Firebase certificate thumbprint (example - verify current cert!)
    private static readonly string[] TRUSTED_THUMBPRINTS = new string[]
    {
        "A1B2C3D4E5F6...", // Firebase cert 1
        "F6E5D4C3B2A1..."  // Firebase cert 2 (backup)
    };
    
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        X509Certificate2 cert = new X509Certificate2(certificateData);
        string thumbprint = cert.Thumbprint;
        
        foreach (string trusted in TRUSTED_THUMBPRINTS)
        {
            if (thumbprint == trusted)
                return true;
        }
        
        Debug.LogError($"Certificate pinning failed! Thumbprint: {thumbprint}");
        return false;
    }
}

// Usage
UnityWebRequest request = UnityWebRequest.Get("https://firestore.googleapis.com/...");
request.certificateHandler = new CertificatePinning();
```

### 4. **Input Validation & Sanitization**

```csharp
// NEVER trust client input - validate everything
public class InputValidator
{
    // Validate lantern message text
    public static string SanitizeLanternMessage(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";
        
        // Max length
        if (input.Length > 280)
            input = input.Substring(0, 280);
        
        // Remove profanity (use external library or word list)
        input = RemoveProfanity(input);
        
        // Remove SQL injection attempts
        input = input.Replace("'", "").Replace("\"", "").Replace(";", "");
        
        // Remove XSS attempts
        input = input.Replace("<", "").Replace(">", "").Replace("&", "");
        
        return input.Trim();
    }
    
    // Validate sigil name
    public static bool IsValidSigilName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.Length < 3 || name.Length > 30) return false;
        
        // Only alphanumeric + spaces
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9 ]+$");
    }
    
    // Validate numerical progression (anti-cheat)
    public static bool IsValidProgression(int oldValue, int newValue, int maxIncrease)
    {
        // Can't decrease
        if (newValue < oldValue) return false;
        
        // Can't increase too much at once
        if (newValue > oldValue + maxIncrease) return false;
        
        return true;
    }
}
```

---

## ♿ Accessibility-First Performance

### Critical Principle
**Performance IS accessibility.** Low frame rates, stuttering, and lag disproportionately affect disabled players.

### 1. **60 FPS Minimum (Non-Negotiable)**

```csharp
// Monitor and enforce frame rate
public class PerformanceMonitor : MonoBehaviour
{
    private float[] frameTimes = new float[60];
    private int frameIndex = 0;
    
    void Update()
    {
        frameTimes[frameIndex] = Time.deltaTime;
        frameIndex = (frameIndex + 1) % frameTimes.Length;
        
        // Check every 60 frames
        if (frameIndex == 0)
        {
            float avgFrameTime = Average(frameTimes);
            float fps = 1f / avgFrameTime;
            
            if (fps < 55) // Below acceptable threshold
            {
                Debug.LogWarning($"Low FPS: {fps:F1}");
                ReduceQuality();
            }
        }
    }
    
    void ReduceQuality()
    {
        // Dynamically reduce particle count
        ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
        foreach (var ps in particles)
        {
            var main = ps.main;
            main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.75f);
        }
        
        Debug.Log("Quality reduced to maintain 60 FPS");
    }
}
```

### 2. **Input Latency Optimization**

```csharp
// Immediate response for accessibility
public class AccessibleInput : MonoBehaviour
{
    void Update()
    {
        // Use Input in Update(), not FixedUpdate() (lower latency)
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 touchPos = Input.mousePosition;
            ProcessTap(touchPos);
        }
    }
    
    void ProcessTap(Vector2 screenPos)
    {
        // Immediate visual feedback (< 16ms)
        ShowTapRipple(screenPos);
        
        // Haptic feedback (if enabled)
        Handheld.Vibrate();
        
        // Audio feedback
        AudioSource.PlayClipAtPoint(tapSound, Camera.main.transform.position);
        
        // Then process game logic
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(screenPos), Vector2.zero);
        if (hit.collider != null)
        {
            hit.collider.GetComponent<Interactable>()?.OnTap();
        }
    }
}
```

### 3. **Reduced Motion Mode**

```csharp
// Accessibility: Respect user's reduced motion preference
public class MotionController : MonoBehaviour
{
    public static bool reducedMotion = false;
    
    void Start()
    {
        // Check system preference (iOS/Android)
        reducedMotion = CheckSystemReducedMotion();
        
        // Allow override in settings
        reducedMotion = PlayerPrefs.GetInt("ReducedMotion", reducedMotion ? 1 : 0) == 1;
        
        ApplyMotionSettings();
    }
    
    void ApplyMotionSettings()
    {
        if (reducedMotion)
        {
            // Disable parallax scrolling
            ParallaxBackground.speed = 0;
            
            // Reduce particle movement
            ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
            foreach (var ps in particles)
            {
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = false; // Particles stay in place
            }
            
            // Disable camera shake
            CameraShake.enabled = false;
            
            // Instant transitions instead of animated
            SceneTransition.duration = 0f;
        }
    }
}
```

---

## 🔧 Build Configuration

### iOS Build Settings
```
// Build Settings
Target SDK: iOS 13.0+ (supports 98% of devices)
Architecture: ARM64 only (no 32-bit)
Scripting Backend: IL2CPP
Managed Stripping Level: High
Script Call Optimization: Fast but no Exceptions

// Player Settings
Accelerometer Frequency: Disabled (saves battery)
Location Usage: Never (we don't use GPS)
Camera Usage: Never
Microphone Usage: Never (no voice features in MVP)

// Graphics
Color Space: Linear (better visuals)
Lightmap Encoding: Normal Quality
HDR Mode: Off (mobile doesn't benefit)
```

### Android Build Settings
```
// Build Settings
Minimum API Level: Android 7.0 (API 24)
Target API Level: Android 13 (API 33)
Scripting Backend: IL2CPP
Target Architecture: ARM64 (Google Play requirement)
Managed Stripping Level: High

// Player Settings
Install Location: Auto (allow SD card)
Internet Access: Require (Firebase needs it)
Write Permission: External (for screenshot sharing)

// Graphics
Graphics API: OpenGL ES 3.0, Vulkan (fallback)
Multithreaded Rendering: Enabled
```

### WebGL Build Settings
```
// Build Settings
Compression Format: Brotli (50% smaller than gzip)
Code Optimization: Speed (not size)
Enable Exceptions: None (smaller build)

// Memory
Maximum Memory: 512 MB
```

---

## 📊 Target Metrics

### File Size Targets
```
iOS:
  - Initial Download: 180 MB
  - Installed Size: 220 MB
  - iCloud Backup: < 50 MB (save data only)

Android:
  - Download APK: 150 MB
  - Installed Size: 200 MB
  - Google Play Instant: 15 MB (demo version)

WebGL:
  - Initial Load: 60 MB
  - Runtime Memory: 300 MB
```

### Performance Targets
```
Frame Rate:
  - Low-end (2017): 60 FPS minimum
  - Mid-range (2020): 60 FPS locked
  - High-end (2023+): 120 FPS on compatible displays

Load Times:
  - App Launch: < 3 seconds
  - Realm Transition: < 1 second
  - Daily Challenge Load: < 500ms

Battery:
  - Active Play: 15-20% per hour
  - Idle (menu): < 5% per hour
```

### Security Metrics
```
Uptime: 99.9% (Firebase SLA)
Data Breach: 0 tolerance
Cheat Detection: 95%+ accuracy
DDoS Mitigation: Automatic (Firebase + rate limiting)
```

---

## 🚀 Implementation Checklist

### Phase 1: Optimization Foundation
- [ ] Set up Unity with IL2CPP backend
- [ ] Configure texture compression (ASTC/DXT5)
- [ ] Implement audio compression (Vorbis)
- [ ] Create object pooling system
- [ ] Enable texture streaming
- [ ] Set up static batching

### Phase 2: Security Foundation
- [ ] Configure Firebase security rules
- [ ] Implement secure storage
- [ ] Add code obfuscation
- [ ] Create input validation
- [ ] Set up rate limiting
- [ ] Implement anti-cheat detection

### Phase 3: Build Optimization
- [ ] Configure build settings (iOS/Android/WebGL)
- [ ] Set up AssetBundle system
- [ ] Implement on-demand downloads
- [ ] Test on low-end devices
- [ ] Profile with Unity Profiler
- [ ] Optimize based on metrics

### Phase 4: Accessibility Performance
- [ ] Implement 60 FPS monitoring
- [ ] Optimize input latency
- [ ] Create reduced motion mode
- [ ] Test with accessibility features enabled
- [ ] Validate performance on assistive devices

---

## 📞 Resources

**Unity Optimization Guide:**  
https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html

**Firebase Security Best Practices:**  
https://firebase.google.com/docs/rules/basics

**WCAG Performance Guidelines:**  
https://www.w3.org/WAI/WCAG21/Understanding/pause-stop-hide.html

**Unity Profiler Tutorial:**  
https://learn.unity.com/tutorial/profiling-your-game-performance

---

**Remember:** Lightweight + powerful is achieved through smart architecture, not cutting features. Procedural generation, efficient shaders, and aggressive compression let us deliver a AAA experience in a 150 MB package.

**Security mantra:** Trust nothing from the client. Validate everything on the server. Rate limit aggressively.

**Accessibility principle:** Performance is accessibility. 60 FPS is mandatory, not optional.
