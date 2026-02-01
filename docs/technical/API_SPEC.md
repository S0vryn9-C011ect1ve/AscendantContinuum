# Backend API Specification - The Ascendant Continuum

**Version:** 1.0  
**Last Updated:** February 1, 2026  
**Backend:** Firebase (Cloud Firestore + Cloud Functions)

---

## 📋 Table of Contents

1. [API Overview](#api-overview)
2. [Authentication](#authentication)
3. [User Profile Endpoints](#user-profile-endpoints)
4. [Daily Challenge System](#daily-challenge-system)
5. [Progression & Rituals](#progression--rituals)
6. [Social Features](#social-features)
7. [Lantern Ascension](#lantern-ascension)
8. [Analytics & Metrics](#analytics--metrics)
9. [Remote Config](#remote-config)
10. [Error Handling](#error-handling)

---

## 🌐 API Overview

### Communication Protocols
- **Real-time:** Firestore real-time listeners
- **HTTP:** Cloud Functions (HTTPS callable)
- **REST:** Firebase REST API (backup)

### Base URLs
```
Firestore: firestore.googleapis.com
Functions: us-central1-ascendant-continuum.cloudfunctions.net
Storage: firebasestorage.googleapis.com
```

### Authentication
All requests require Firebase Authentication token in header:
```
Authorization: Bearer <firebase-id-token>
```

---

## 🔐 Authentication

### 1. Anonymous Sign-In

**Unity Client:**
```csharp
async Task SignInAnonymously()
{
    FirebaseAuth auth = FirebaseAuth.DefaultInstance;
    
    try 
    {
        var result = await auth.SignInAnonymouslyAsync();
        string userId = result.User.UserId;
        Debug.Log($"Signed in: {userId}");
        
        // Initialize player profile if new user
        await InitializePlayerProfile(userId);
    }
    catch (FirebaseException ex)
    {
        Debug.LogError($"Auth failed: {ex.Message}");
    }
}
```

**Firestore Trigger (Cloud Function):**
```javascript
exports.onUserCreated = functions.auth.user().onCreate(async (user) => {
    const userId = user.uid;
    
    // Create default profile
    await admin.firestore().collection('users').doc(userId).set({
        createdAt: admin.firestore.FieldValue.serverTimestamp(),
        lastLogin: admin.firestore.FieldValue.serverTimestamp(),
        totalRituals: 0,
        currentStreak: 0,
        chosenDeity: null,
        version: '1.0.0'
    });
    
    console.log(`User profile created: ${userId}`);
});
```

### 2. Optional Account Linking

**Upgrade Anonymous to Google/Apple:**
```csharp
async Task LinkGoogleAccount()
{
    FirebaseAuth auth = FirebaseAuth.DefaultInstance;
    FirebaseUser currentUser = auth.CurrentUser;
    
    // Google Sign-In
    var credential = GoogleAuthProvider.GetCredential(idToken, accessToken);
    
    try
    {
        await currentUser.LinkWithCredentialAsync(credential);
        Debug.Log("Account upgraded successfully");
    }
    catch (FirebaseException ex)
    {
        Debug.LogError($"Account linking failed: {ex.Message}");
    }
}
```

---

## 👤 User Profile Endpoints

### 1. Get Player Profile

**Firestore Query:**
```csharp
async Task<PlayerProfile> GetPlayerProfile()
{
    string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    
    DocumentSnapshot snapshot = await FirebaseFirestore.DefaultInstance
        .Collection("users")
        .Document(userId)
        .GetSnapshotAsync();
    
    if (snapshot.Exists)
    {
        return snapshot.ConvertTo<PlayerProfile>();
    }
    
    return null;
}
```

**Data Structure:**
```json
{
    "users/{userId}": {
        "createdAt": "2026-02-01T10:00:00Z",
        "lastLogin": "2026-02-01T15:30:00Z",
        "totalRituals": 47,
        "currentStreak": 5,
        "longestStreak": 12,
        "chosenDeity": "Ascendant Flame",
        "level": 8,
        "experience": 2450,
        "version": "1.0.0"
    }
}
```

### 2. Update Profile (Cloud Function)

**HTTPS Callable Function:**
```javascript
exports.updateProfile = functions.https.onCall(async (data, context) => {
    // Verify authentication
    if (!context.auth) {
        throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
    }
    
    const userId = context.auth.uid;
    const { chosenDeity, customData } = data;
    
    // Validate deity choice
    const validDeities = [
        'Ascendant Flame',
        'Herald of Joyful Curiosity',
        'Archivist of Bright Memories',
        'Mechanic of Helpful Wonders',
        'Scribe of Magical Knowledge',
        'Silent Nurturer'
    ];
    
    if (chosenDeity && !validDeities.includes(chosenDeity)) {
        throw new functions.https.HttpsError('invalid-argument', 'Invalid deity choice');
    }
    
    // Update profile
    const updates = {
        lastLogin: admin.firestore.FieldValue.serverTimestamp()
    };
    
    if (chosenDeity) {
        updates.chosenDeity = chosenDeity;
        updates.deityChosenAt = admin.firestore.FieldValue.serverTimestamp();
    }
    
    await admin.firestore()
        .collection('users')
        .doc(userId)
        .update(updates);
    
    return { success: true, message: 'Profile updated' };
});
```

**Unity Client:**
```csharp
async Task ChooseDeity(string deityName)
{
    var function = FirebaseFunctions.DefaultInstance.GetHttpsCallable("updateProfile");
    
    var data = new Dictionary<string, object>
    {
        { "chosenDeity", deityName }
    };
    
    try
    {
        var result = await function.CallAsync(data);
        Debug.Log("Deity chosen successfully");
    }
    catch (FirebaseFunctionsException ex)
    {
        Debug.LogError($"Failed to choose deity: {ex.Message}");
    }
}
```

---

## 📅 Daily Challenge System

### 1. Generate Daily Constellation Challenge

**Scheduled Cloud Function:**
```javascript
exports.generateDailyChallenge = functions.pubsub
    .schedule('every day 00:00')
    .timeZone('UTC')
    .onRun(async (context) => {
        const today = new Date().toISOString().split('T')[0]; // "2026-02-01"
        const seed = generateDailySeed(today); // Deterministic from date
        
        const themes = [
            'Growth', 'Discovery', 'Connection', 'Joy', 'Reflection',
            'Wonder', 'Courage', 'Peace', 'Curiosity', 'Gratitude'
        ];
        
        const theme = themes[seed % themes.length];
        const pattern = generateConstellationPattern(seed, theme);
        
        const challenge = {
            date: today,
            seed: seed,
            theme: theme,
            targetPattern: pattern,
            difficulty: calculateDifficulty(seed),
            participants: 0,
            completions: 0,
            createdAt: admin.firestore.FieldValue.serverTimestamp()
        };
        
        await admin.firestore()
            .collection('dailyChallenges')
            .doc(today)
            .set(challenge);
        
        console.log(`Daily challenge created: ${today} - ${theme}`);
        
        // Send push notifications (optional)
        await notifyPlayers(challenge);
        
        return null;
    });

function generateDailySeed(dateString) {
    // Convert date to consistent seed
    const date = new Date(dateString);
    return date.getTime() / 1000 / 60 / 60 / 24; // Days since epoch
}

function generateConstellationPattern(seed, theme) {
    const rng = seedrandom(seed); // Deterministic random
    const nodeCount = 5 + Math.floor(rng() * 5); // 5-9 nodes
    
    const pattern = [];
    for (let i = 0; i < nodeCount; i++) {
        pattern.push({
            x: rng(),
            y: rng(),
            type: theme.toLowerCase()
        });
    }
    
    return pattern;
}
```

### 2. Fetch Daily Challenge

**Unity Client:**
```csharp
async Task<DailyChallenge> FetchDailyChallenge()
{
    string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
    
    DocumentSnapshot snapshot = await FirebaseFirestore.DefaultInstance
        .Collection("dailyChallenges")
        .Document(today)
        .GetSnapshotAsync();
    
    if (snapshot.Exists)
    {
        return snapshot.ConvertTo<DailyChallenge>();
    }
    
    return null;
}
```

### 3. Submit Challenge Completion

**Cloud Function:**
```javascript
exports.submitChallengeCompletion = functions.https.onCall(async (data, context) => {
    if (!context.auth) {
        throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
    }
    
    const userId = context.auth.uid;
    const { date, timeTaken, shareImage } = data;
    
    // Verify challenge exists
    const challengeRef = admin.firestore().collection('dailyChallenges').doc(date);
    const challengeDoc = await challengeRef.get();
    
    if (!challengeDoc.exists) {
        throw new functions.https.HttpsError('not-found', 'Challenge not found');
    }
    
    // Check if already completed today
    const completionRef = challengeRef.collection('completions').doc(userId);
    const existingCompletion = await completionRef.get();
    
    if (existingCompletion.exists) {
        return { 
            success: false, 
            message: 'Already completed today',
            alreadyCompleted: true 
        };
    }
    
    // Record completion
    await completionRef.set({
        userId: userId,
        completedAt: admin.firestore.FieldValue.serverTimestamp(),
        timeTaken: timeTaken,
        shared: shareImage !== null
    });
    
    // Increment challenge counters
    await challengeRef.update({
        completions: admin.firestore.FieldValue.increment(1),
        participants: admin.firestore.FieldValue.increment(1)
    });
    
    // Update user streak
    await updateUserStreak(userId, date);
    
    // Generate share card if requested
    let shareUrl = null;
    if (shareImage) {
        shareUrl = await generateShareCard(userId, date, shareImage);
    }
    
    return { 
        success: true, 
        shareUrl: shareUrl,
        totalCompletions: challengeDoc.data().completions + 1
    };
});

async function updateUserStreak(userId, date) {
    const userRef = admin.firestore().collection('users').doc(userId);
    const userDoc = await userRef.get();
    const userData = userDoc.data();
    
    const yesterday = new Date(date);
    yesterday.setDate(yesterday.getDate() - 1);
    const yesterdayStr = yesterday.toISOString().split('T')[0];
    
    // Check if completed yesterday
    const yesterdayChallenge = await admin.firestore()
        .collection('dailyChallenges')
        .doc(yesterdayStr)
        .collection('completions')
        .doc(userId)
        .get();
    
    let newStreak = 1;
    if (yesterdayChallenge.exists) {
        newStreak = (userData.currentStreak || 0) + 1;
    }
    
    await userRef.update({
        currentStreak: newStreak,
        longestStreak: Math.max(newStreak, userData.longestStreak || 0),
        lastChallengeDate: date
    });
}
```

---

## 🎮 Progression & Rituals

### 1. Record Ritual Completion

**Cloud Function:**
```javascript
exports.recordRitualCompletion = functions.https.onCall(async (data, context) => {
    if (!context.auth) {
        throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
    }
    
    const userId = context.auth.uid;
    const { realmName, ritualType, duration, sigilsEarned, serendipityEvent } = data;
    
    // Validate realm
    const validRealms = ['Emberforge', 'VerdantSanctuary', 'EchoFields', 'DawnCitadel', 'LanternAscension'];
    if (!validRealms.includes(realmName)) {
        throw new functions.https.HttpsError('invalid-argument', 'Invalid realm');
    }
    
    // Record completion
    const completionData = {
        userId: userId,
        realmName: realmName,
        ritualType: ritualType,
        duration: duration,
        sigilsEarned: sigilsEarned,
        serendipityEvent: serendipityEvent || null,
        completedAt: admin.firestore.FieldValue.serverTimestamp()
    };
    
    await admin.firestore()
        .collection('ritualCompletions')
        .add(completionData);
    
    // Update user progress
    const userRef = admin.firestore().collection('users').doc(userId);
    
    await userRef.update({
        totalRituals: admin.firestore.FieldValue.increment(1),
        [`realmProgress.${realmName}.completed`]: admin.firestore.FieldValue.increment(1),
        lastRitualAt: admin.firestore.FieldValue.serverTimestamp()
    });
    
    // Add sigils to collection
    if (sigilsEarned && sigilsEarned.length > 0) {
        await addSigilsToCollection(userId, sigilsEarned);
    }
    
    // Calculate XP and level
    const xpGained = calculateXP(duration, serendipityEvent);
    await awardXP(userId, xpGained);
    
    return { 
        success: true, 
        xpGained: xpGained,
        message: 'Ritual recorded successfully' 
    };
});

function calculateXP(duration, serendipityEvent) {
    let baseXP = 50;
    
    // Bonus for time spent (up to 5 minutes)
    const timeBonus = Math.min(duration / 60, 5) * 10;
    
    // Serendipity bonus
    const serendipityBonus = serendipityEvent ? 100 : 0;
    
    return Math.floor(baseXP + timeBonus + serendipityBonus);
}

async function awardXP(userId, xp) {
    const userRef = admin.firestore().collection('users').doc(userId);
    const userDoc = await userRef.get();
    const currentXP = userDoc.data().experience || 0;
    const currentLevel = userDoc.data().level || 1;
    
    const newXP = currentXP + xp;
    const newLevel = calculateLevel(newXP);
    
    await userRef.update({
        experience: newXP,
        level: newLevel
    });
    
    // Level up notification
    if (newLevel > currentLevel) {
        await notifyLevelUp(userId, newLevel);
    }
}

function calculateLevel(xp) {
    // XP curve: level = floor(sqrt(xp / 100))
    return Math.floor(Math.sqrt(xp / 100)) + 1;
}
```

### 2. Get Realm Progress

**Unity Client:**
```csharp
async Task<RealmProgress> GetRealmProgress(string realmName)
{
    string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    
    DocumentSnapshot snapshot = await FirebaseFirestore.DefaultInstance
        .Collection("users")
        .Document(userId)
        .GetSnapshotAsync();
    
    if (snapshot.Exists)
    {
        var data = snapshot.ToDictionary();
        var realmProgress = data["realmProgress"] as Dictionary<string, object>;
        
        if (realmProgress != null && realmProgress.ContainsKey(realmName))
        {
            return JsonUtility.FromJson<RealmProgress>(
                JsonUtility.ToJson(realmProgress[realmName])
            );
        }
    }
    
    return new RealmProgress { completed = 0, unlocked = true };
}
```

---

## 🌟 Social Features

### 1. Generate Ritual Replay Video

**Cloud Function:**
```javascript
exports.generateRitualReplay = functions.https.onCall(async (data, context) => {
    if (!context.auth) {
        throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
    }
    
    const userId = context.auth.uid;
    const { realmName, actionSequence, duration } = data;
    
    // Store action sequence for replay generation
    const replayId = admin.firestore().collection('ritualReplays').doc().id;
    
    await admin.firestore().collection('ritualReplays').doc(replayId).set({
        userId: userId,
        realmName: realmName,
        actionSequence: actionSequence,
        duration: duration,
        views: 0,
        createdAt: admin.firestore.FieldValue.serverTimestamp(),
        expiresAt: admin.firestore.Timestamp.fromDate(
            new Date(Date.now() + 7 * 24 * 60 * 60 * 1000) // 7 days
        )
    });
    
    // Generate shareable link
    const shareUrl = `https://ascendant-continuum.com/replay/${replayId}`;
    
    // Track viral event
    await admin.firestore().collection('analytics').add({
        eventType: 'replay_generated',
        userId: userId,
        realmName: realmName,
        timestamp: admin.firestore.FieldValue.serverTimestamp()
    });
    
    return { 
        success: true, 
        replayId: replayId,
        shareUrl: shareUrl 
    };
});
```

### 2. Fetch Nearby Player Echoes

**Cloud Function:**
```javascript
exports.fetchNearbyEchoes = functions.https.onCall(async (data, context) => {
    if (!context.auth) {
        throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
    }
    
    const { realmName, limit } = data;
    const maxLimit = Math.min(limit || 10, 50); // Cap at 50
    
    // Fetch recent completions in this realm (last 24 hours)
    const yesterday = new Date(Date.now() - 24 * 60 * 60 * 1000);
    
    const echoes = await admin.firestore()
        .collection('ritualCompletions')
        .where('realmName', '==', realmName)
        .where('completedAt', '>', admin.firestore.Timestamp.fromDate(yesterday))
        .orderBy('completedAt', 'desc')
        .limit(maxLimit)
        .get();
    
    const echoData = [];
    echoes.forEach(doc => {
        const data = doc.data();
        echoData.push({
            // Anonymized - no userId
            ritualType: data.ritualType,
            duration: data.duration,
            completedAt: data.completedAt.toDate().toISOString(),
            hadSerendipity: data.serendipityEvent !== null
        });
    });
    
    return { echoes: echoData };
});
```

---

## 🕯️ Lantern Ascension

### 1. Release Lantern

**Cloud Function:**
```javascript
exports.releaseLantern = functions.https.onCall(async (data, context) => {
    if (!context.auth) {
        throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
    }
    
    const userId = context.auth.uid;
    const { wish, visibility, position } = data;
    
    // Validate visibility
    if (!['private', 'shared'].includes(visibility)) {
        throw new functions.https.HttpsError('invalid-argument', 'Invalid visibility');
    }
    
    // Moderate wish if shared
    let moderatedWish = wish;
    let isApproved = true;
    
    if (visibility === 'shared' && wish) {
        isApproved = await moderateContent(wish);
        moderatedWish = isApproved ? wish : null;
    }
    
    const lanternData = {
        userId: userId, // Stored but never displayed
        wish: moderatedWish,
        visibility: isApproved ? visibility : 'private',
        position: position,
        createdAt: admin.firestore.FieldValue.serverTimestamp(),
        moderationStatus: isApproved ? 'approved' : 'rejected'
    };
    
    const lanternRef = await admin.firestore()
        .collection('lanterns')
        .add(lanternData);
    
    // Update user stats
    await admin.firestore().collection('users').doc(userId).update({
        lanternsReleased: admin.firestore.FieldValue.increment(1)
    });
    
    return { 
        success: true, 
        lanternId: lanternRef.id,
        wasModerated: !isApproved,
        message: isApproved ? 'Lantern released' : 'Lantern made private (content policy)'
    };
});

async function moderateContent(text) {
    // Use Google Cloud Natural Language API
    const language = require('@google-cloud/language');
    const client = new language.LanguageServiceClient();
    
    const document = {
        content: text,
        type: 'PLAIN_TEXT',
    };
    
    // Sentiment analysis
    const [sentiment] = await client.analyzeSentiment({ document });
    
    // Reject extremely negative content
    if (sentiment.documentSentiment.score < -0.8) {
        return false;
    }
    
    // Additional checks (profanity filter, etc.)
    // ... implement as needed
    
    return true;
}
```

### 2. Fetch Visible Lanterns

**Cloud Function:**
```javascript
exports.fetchVisibleLanterns = functions.https.onCall(async (data, context) => {
    const { limit } = data;
    const maxLimit = Math.min(limit || 50, 100);
    
    // Fetch recent shared lanterns
    const lanterns = await admin.firestore()
        .collection('lanterns')
        .where('visibility', '==', 'shared')
        .where('moderationStatus', '==', 'approved')
        .orderBy('createdAt', 'desc')
        .limit(maxLimit)
        .get();
    
    const lanternData = [];
    lanterns.forEach(doc => {
        const data = doc.data();
        lanternData.push({
            lanternId: doc.id,
            wish: data.wish,
            position: data.position,
            createdAt: data.createdAt.toDate().toISOString(),
            // NO userId - fully anonymous
        });
    });
    
    return { lanterns: lanternData };
});
```

---

## 📊 Analytics & Metrics

### Custom Events

```javascript
// Track custom analytics events
exports.trackCustomEvent = functions.https.onCall(async (data, context) => {
    if (!context.auth) {
        throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
    }
    
    const userId = context.auth.uid;
    const { eventName, eventParams } = data;
    
    await admin.firestore().collection('analytics').add({
        userId: userId,
        eventName: eventName,
        eventParams: eventParams,
        timestamp: admin.firestore.FieldValue.serverTimestamp()
    });
    
    return { success: true };
});
```

**Unity Client:**
```csharp
public static void TrackCustomEvent(string eventName, Dictionary<string, object> parameters)
{
    var function = FirebaseFunctions.DefaultInstance.GetHttpsCallable("trackCustomEvent");
    
    var data = new Dictionary<string, object>
    {
        { "eventName", eventName },
        { "eventParams", parameters }
    };
    
    function.CallAsync(data);
}
```

---

## ⚙️ Remote Config

### Fetch Configuration

**Unity Client:**
```csharp
async Task FetchRemoteConfig()
{
    FirebaseRemoteConfig remoteConfig = FirebaseRemoteConfig.DefaultInstance;
    
    // Set defaults
    var defaults = new Dictionary<string, object>
    {
        { "daily_challenge_enabled", true },
        { "serendipity_chance", 0.01 }, // 1%
        { "viral_share_xp_bonus", 50 },
        { "season_name", "Launch Season" },
        { "featured_realm", "Emberforge" }
    };
    
    remoteConfig.SetDefaultsAsync(defaults);
    
    // Fetch and activate
    await remoteConfig.FetchAsync(TimeSpan.FromHours(12));
    await remoteConfig.ActivateAsync();
    
    // Use values
    bool challengeEnabled = remoteConfig.GetValue("daily_challenge_enabled").BooleanValue;
    double serendipityChance = remoteConfig.GetValue("serendipity_chance").DoubleValue;
}
```

---

## ❌ Error Handling

### Standard Error Codes

```javascript
// Standardized error responses
const ErrorCodes = {
    UNAUTHENTICATED: 'unauthenticated',
    INVALID_ARGUMENT: 'invalid-argument',
    NOT_FOUND: 'not-found',
    ALREADY_EXISTS: 'already-exists',
    PERMISSION_DENIED: 'permission-denied',
    RESOURCE_EXHAUSTED: 'resource-exhausted',
    INTERNAL: 'internal'
};

function throwError(code, message) {
    throw new functions.https.HttpsError(code, message);
}
```

### Unity Error Handling

```csharp
try
{
    var result = await function.CallAsync(data);
    // Success
}
catch (FirebaseFunctionsException ex)
{
    switch (ex.ErrorCode)
    {
        case FunctionsErrorCode.Unauthenticated:
            Debug.LogError("User not authenticated");
            // Prompt re-login
            break;
            
        case FunctionsErrorCode.InvalidArgument:
            Debug.LogError($"Invalid data: {ex.Message}");
            break;
            
        case FunctionsErrorCode.NotFound:
            Debug.LogError("Resource not found");
            break;
            
        default:
            Debug.LogError($"Unknown error: {ex.Message}");
            break;
    }
}
```

---

## 🔒 Security Rules

### Firestore Security Rules

```javascript
// firestore.rules
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    
    // Users can only read/write their own profile
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
    
    // Daily challenges are read-only for clients
    match /dailyChallenges/{date} {
      allow read: if request.auth != null;
      allow write: if false; // Only Cloud Functions can write
      
      match /completions/{userId} {
        allow read: if request.auth != null && request.auth.uid == userId;
        allow write: if false; // Only Cloud Functions
      }
    }
    
    // Lanterns: users can create, read shared ones
    match /lanterns/{lanternId} {
      allow create: if request.auth != null;
      allow read: if resource.data.visibility == 'shared' 
                  || (request.auth != null && request.auth.uid == resource.data.userId);
      allow update, delete: if false; // Immutable
    }
    
    // Ritual replays: creator can read, anyone can view
    match /ritualReplays/{replayId} {
      allow read: if true; // Public for sharing
      allow create: if request.auth != null;
      allow update, delete: if false;
    }
    
    // Analytics: write-only for clients
    match /analytics/{docId} {
      allow read: if false; // Only backend
      allow write: if request.auth != null;
    }
  }
}
```

---

## 📈 Rate Limiting

### Prevent Abuse

```javascript
// Check rate limits before processing
async function checkRateLimit(userId, action, maxPerHour) {
    const oneHourAgo = new Date(Date.now() - 60 * 60 * 1000);
    
    const recentActions = await admin.firestore()
        .collection('rateLimits')
        .where('userId', '==', userId)
        .where('action', '==', action)
        .where('timestamp', '>', admin.firestore.Timestamp.fromDate(oneHourAgo))
        .get();
    
    if (recentActions.size >= maxPerHour) {
        throw new functions.https.HttpsError(
            'resource-exhausted',
            `Rate limit exceeded for ${action}. Try again later.`
        );
    }
    
    // Record this action
    await admin.firestore().collection('rateLimits').add({
        userId: userId,
        action: action,
        timestamp: admin.firestore.FieldValue.serverTimestamp()
    });
}

// Usage in functions
exports.releaseLantern = functions.https.onCall(async (data, context) => {
    await checkRateLimit(context.auth.uid, 'release_lantern', 10); // Max 10/hour
    // ... rest of function
});
```

---

## 🚀 Performance Optimization

### Batch Writes

```csharp
// Batch multiple writes for efficiency
async Task BatchUpdateProgress(List<SigilData> sigils)
{
    WriteBatch batch = FirebaseFirestore.DefaultInstance.StartBatch();
    string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
    
    DocumentReference userRef = FirebaseFirestore.DefaultInstance
        .Collection("users")
        .Document(userId);
    
    foreach (var sigil in sigils)
    {
        batch.Update(userRef, new Dictionary<string, object>
        {
            { $"sigils.{sigil.id}", true }
        });
    }
    
    await batch.CommitAsync();
}
```

### Caching Strategy

```csharp
// Cache frequently accessed data
public class CacheManager
{
    private static DailyChallenge cachedChallenge;
    private static DateTime cacheTime;
    
    public static async Task<DailyChallenge> GetDailyChallengeWithCache()
    {
        // Cache for 1 hour
        if (cachedChallenge != null && 
            (DateTime.UtcNow - cacheTime).TotalHours < 1)
        {
            return cachedChallenge;
        }
        
        cachedChallenge = await FetchDailyChallenge();
        cacheTime = DateTime.UtcNow;
        
        return cachedChallenge;
    }
}
```

---

**End of Backend API Specification**

*Next: Procedural Generation Algorithms & MVP Implementation Plan*  
*Contact: ascendantcontinuum@gmail.com*
