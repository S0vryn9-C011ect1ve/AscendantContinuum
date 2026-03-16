# 🔥 Firebase Setup Guide - Free Tier (Zero Budget)
**Backend, Cloud Save, Analytics - All Free**

## What Firebase Provides (Free)

- **Authentication:** Anonymous sign-in (no email required)
- **Firestore Database:** Store player data, wishes, time capsules
- **Cloud Storage:** Store images (player sigils if needed)
- **Cloud Functions:** Server-side logic (anti-cheat, daily challenges)
- **Analytics:** Track player behavior
- **Hosting:** Deploy WebGL build
- **Free Limits:**
  - 10GB bandwidth/month
  - 1GB storage
  - 50K reads/day, 20K writes/day
  - **Sufficient for 10,000+ players**

---

## 📋 PART 1: Firebase Project Setup (15 min)

### Step 1: Create Firebase Account
1. Go to https://firebase.google.com/
2. Click "Get Started"
3. Sign in with Google account (free)

### Step 2: Create Project
1. Click "Add Project"
2. Project name: "ascendant-continuum"
3. Enable Google Analytics: **Yes** ✅ (free, useful data)
4. Analytics location: Your country
5. Accept terms
6. Click "Create Project" (takes 1-2 minutes)

### Step 3: Register Unity App
1. In Firebase Console, click "Add app"
2. Select **Unity** icon
3. Platform: Select both **iOS** and **Android** (or start with one)
4. Package name: `com.YourName.AscendantContinuum`
   - IMPORTANT: Must match Unity's Package Identifier
   - Find in Unity: Edit → Project Settings → Player → Other Settings → Package Name

### Step 4: Download Config Files

**For Android:**
1. Download `google-services.json`
2. Place in Unity: `Assets/Firebase/`

**For iOS:**
1. Download `GoogleService-Info.plist`
2. Place in Unity: `Assets/Firebase/`

**For WebGL:**
1. Firebase Console → Project Settings → General
2. Scroll to "Your apps" → Web app
3. Click "</>" to add web app
4. Copy Firebase config object (you'll need this later)

### Step 5: Install Firebase Unity SDK (Free)
1. Download SDK: https://firebase.google.com/download/unity
2. Download: `firebase_unity_sdk.zip` (latest version)
3. Extract ZIP file
4. In Unity: Assets → Import Package → Custom Package
5. Navigate to extracted folder → Select:
   - `FirebaseAuth.unitypackage`
   - `FirebaseFirestore.unitypackage`
   - `FirebaseStorage.unitypackage`
   - `FirebaseAnalytics.unitypackage`
   - `FirebaseFunctions.unitypackage` (for Cloud Functions)
6. Click "Import" for each
7. Wait for import (takes 5-10 minutes)

---

## 📋 PART 2: Unity Integration (30 min)

### Step 1: Verify Package Import
1. Check `Assets/Firebase/` folder exists
2. Check `Assets/Plugins/` folder has Firebase DLLs

### Step 2: Update FirebaseManager.cs

Your game already has `FirebaseManager.cs` at:
`Assets/_Project/Scripts/Core/FirebaseManager.cs`

Verify it contains initialization code (check existing file, should look similar to):

```csharp
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    
    private FirebaseApp app;
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    
    private bool isInitialized = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                
                isInitialized = true;
                Debug.Log("Firebase initialized successfully!");
                
                // Sign in anonymously
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"Firebase dependency error: {task.Result}");
            }
        });
    }
    
    void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser user = auth.CurrentUser;
                Debug.Log($"Signed in anonymously: {user.UserId}");
            }
            else
            {
                Debug.LogError("Anonymous sign-in failed: " + task.Exception);
            }
        });
    }
    
    // Save player data to Firestore
    public void SavePlayerData(PlayerProfile profile)
    {
        if (!isInitialized || auth.CurrentUser == null) return;
        
        string userId = auth.CurrentUser.UserId;
        DocumentReference docRef = db.Collection("players").Document(userId);
        
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "sparksCollected", profile.sparksCollected },
            { "sigilsCollected", profile.sigilsCollected },
            { "currentRealm", profile.currentRealm },
            { "lastLogin", FieldValue.ServerTimestamp },
            // Add more fields as needed
        };
        
        docRef.SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Player data saved to Firestore!");
            }
            else
            {
                Debug.LogError("Save failed: " + task.Exception);
            }
        });
    }
    
    // Load player data from Firestore
    public void LoadPlayerData(System.Action<PlayerProfile> onComplete)
    {
        if (!isInitialized || auth.CurrentUser == null)
        {
            onComplete?.Invoke(null);
            return;
        }
        
        string userId = auth.CurrentUser.UserId;
        DocumentReference docRef = db.Collection("players").Document(userId);
        
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    PlayerProfile profile = new PlayerProfile();
                    profile.sparksCollected = snapshot.GetValue<int>("sparksCollected");
                    profile.sigilsCollected = snapshot.GetValue<int>("sigilsCollected");
                    profile.currentRealm = snapshot.GetValue<string>("currentRealm");
                    
                    Debug.Log("Player data loaded from Firestore!");
                    onComplete?.Invoke(profile);
                }
                else
                {
                    Debug.Log("No saved data found, creating new profile");
                    onComplete?.Invoke(new PlayerProfile());
                }
            }
            else
            {
                Debug.LogError("Load failed: " + task.Exception);
                onComplete?.Invoke(null);
            }
        });
    }
}
```

### Step 3: Add FirebaseManager to Scene
1. Open Bootstrap.unity scene
2. Create empty GameObject
3. Rename "FirebaseManager"
4. Add Component → FirebaseManager script
5. Save scene

### Step 4: Test Firebase Connection
1. Press Play in Unity
2. Check Console for:
   - "Firebase initialized successfully!" ✅
   - "Signed in anonymously: [user ID]" ✅
3. If errors, check:
   - google-services.json is in Assets/Firebase/
   - Package name matches between Unity and Firebase Console
   - Internet connection active

---

## 📋 PART 3: Firestore Database Setup (20 min)

### Step 1: Enable Firestore
1. Firebase Console → Build → Firestore Database
2. Click "Create database"
3. Mode: **Start in test mode** (allows all reads/writes)
4. Location: Choose nearest region (e.g., us-central)
5. Click "Enable"

### Step 2: Create Collections

**Collection: players**
- Stores player profiles
- Document ID = user ID (auto from auth)
- Fields:
  - sparkCollected (number)
  - sigilsCollected (number)
  - currentRealm (string)
  - lastLogin (timestamp)

**Collection: wishes**
- Stores time capsules and wish wall
- Fields:
  - userId (string)
  - wishText (string)
  - timestamp (timestamp)
  - unlockDate (timestamp)
  - isDiscovered (boolean)

**Collection: npcMemory**
- Global NPC collective memory
- Fields:
  - emotion (string): "joy", "peace", "curiosity"
  - count (number)
  - lastUpdated (timestamp)

### Step 3: Security Rules (IMPORTANT!)

Replace test mode rules with secure production rules:

1. Firestore → Rules tab
2. Paste this:

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    
    // Players can only read/write their own data
    match /players/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
    
    // Wishes are readable by all, writable by creator
    match /wishes/{wishId} {
      allow read: if request.auth != null;
      allow create: if request.auth != null && request.resource.data.userId == request.auth.uid;
      allow update, delete: if request.auth != null && resource.data.userId == request.auth.uid;
    }
    
    // NPC memory is readable by all, writable by Cloud Functions only
    match /npcMemory/{npcId} {
      allow read: if request.auth != null;
      allow write: if false; // Only Cloud Functions can write
    }
  }
}
```

3. Click "Publish"

---

## 📋 PART 4: Cloud Functions Setup (30 min)

Cloud Functions run server-side logic (anti-cheat, daily challenges, etc.)

### Step 1: Install Firebase CLI
1. Install Node.js: https://nodejs.org/ (LTS version, free)
2. Open terminal (PowerShell on Windows)
3. Install Firebase CLI:
   ```
   npm install -g firebase-tools
   ```
4. Verify: `firebase --version` (should show version number)

### Step 2: Login to Firebase
```
firebase login
```
- Opens browser, sign in with Google account
- Grant permissions

### Step 3: Initialize Functions
Navigate to your project folder in terminal:
```
cd "d:\1-Ascendant Continuum Game"
firebase init functions
```

**Setup wizard:**
1. "Use an existing project" → Select "ascendant-continuum"
2. Language: **JavaScript** (simpler than TypeScript)
3. ESLint: **No** (skip linting for now)
4. Install dependencies: **Yes**

This creates:
- `functions/` folder
- `functions/index.js` (your Cloud Functions)

### Step 4: Write Cloud Functions

Edit `functions/index.js`:

```javascript
const functions = require("firebase-functions");
const admin = require("firebase-admin");
admin.initializeApp();

const db = admin.firestore();

// Daily Challenge Generator
exports.generateDailyChallenge = functions.pubsub
  .schedule("0 0 * * *") // Every day at midnight UTC
  .onRun(async (context) => {
    const today = new Date().toISOString().split("T")[0]; // YYYY-MM-DD
    const seed = parseInt(today.replace(/-/g, ""));

    // Generate challenge based on date seed
    const challengeTypes = ["CollectSparks", "VisitRealms", "CompleteRituals"];
    const chosenType = challengeTypes[seed % challengeTypes.length];

    await db.collection("dailyChallenges").doc(today).set({
      type: chosenType,
      seed: seed,
      date: today,
      created: admin.firestore.FieldValue.serverTimestamp(),
    });

    console.log(`Generated daily challenge for ${today}: ${chosenType}`);
    return null;
  });

// Validate Sigil Crafting (Anti-Cheat)
exports.validateSigilCraft = functions.https.onCall(async (data, context) => {
  // Verify user is authenticated
  if (!context.auth) {
    throw new functions.https.HttpsError("unauthenticated", "User must be signed in");
  }

  const {sigil1, sigil2} = data;

  // Server-side validation logic
  const validCombinations = {
    "DoubleFlame_GlowingThread": "Forgeweaver",
    "BloomingLoop_InfiniteKnot": "GardenWeaver",
    // Add all valid combinations
  };

  const key = `${sigil1}_${sigil2}`;
  const result = validCombinations[key];

  if (result) {
    // Award sigil to player
    await db.collection("players").doc(context.auth.uid).update({
      [`sigils.${result}`]: admin.firestore.FieldValue.increment(1),
    });

    return {success: true, newSigil: result};
  } else {
    return {success: false, error: "Invalid combination"};
  }
});

// Aggregate NPC Collective Memory
exports.aggregateNPCMemory = functions.firestore
  .document("players/{userId}/emotions/{emotionId}")
  .onCreate(async (snap, context) => {
    const emotion = snap.data().emotion; // "joy", "peace", etc.

    // Increment global emotion counter
    const memoryRef = db.collection("npcMemory").doc("global");

    await memoryRef.set(
      {
        [emotion]: admin.firestore.FieldValue.increment(1),
        lastUpdated: admin.firestore.FieldValue.serverTimestamp(),
      },
      {merge: true}
    );

    console.log(`Incremented ${emotion} emotion in collective memory`);
    return null;
  });
```

### Step 5: Deploy Cloud Functions
```
firebase deploy --only functions
```

Wait 2-5 minutes for deployment.

Check Firebase Console → Functions to see deployed functions.

---

## 📋 PART 5: Analytics Setup (10 min)

### Step 1: Enable Analytics
Already enabled if you selected "Yes" during project creation!

### Step 2: Track Events from Unity

In your scripts, track key events:

```csharp
using Firebase.Analytics;

// Track realm completion
FirebaseAnalytics.LogEvent("realm_complete", new Parameter[]
{
    new Parameter("realm_name", "Emberforge"),
    new Parameter("time_spent", 120)
});

// Track sigil collection
FirebaseAnalytics.LogEvent("sigil_collected", new Parameter[]
{
    new Parameter("sigil_type", "DoubleFl"),
    new Parameter("count", 1)
});

// Track tutorial completion
FirebaseAnalytics.LogEvent("tutorial_complete");
```

### Step 3: View Analytics
1. Firebase Console → Analytics → Dashboard
2. Wait 24 hours for data to appear
3. See:
   - Daily Active Users (DAU)
   - Retention
   - Event counts

---

## 📋 PART 6: WebGL Hosting (15 min)

Deploy your game to free Firebase hosting.

### Step 1: Build WebGL
1. Unity → File → Build Settings
2. Platform: WebGL
3. Click "Build"
4. Output folder: `Builds/WebGL/`
5. Wait 10-30 minutes for build

### Step 2: Initialize Hosting
```
firebase init hosting
```

**Setup:**
1. Public directory: `Builds/WebGL` (or your build folder)
2. Single-page app: **No**
3. GitHub deployment: **No**

### Step 3: Deploy
```
firebase deploy --only hosting
```

Wait 2-5 minutes.

Your game is now live at:
`https://ascendant-continuum.web.app`

---

## ✅ FREE TIER LIMITS

**Firestore:**
- 50K reads/day
- 20K writes/day
- 1GB storage

**Bandwidth:**
- 10GB downloads/month (WebGL + Firebase)

**Cloud Functions:**
- 125K invocations/month
- 40K GB-seconds compute time

**Sufficient for:**
- 10,000 Monthly Active Users (MAU)
- 500-1,000 Daily Active Users (DAU)

**When to upgrade:**
- Over limits → Blaze Plan (pay-as-you-go, only pay for overage)
- Cloud Functions needed → Blaze required (still free within limits)

---

##🧪 TESTING CHECKLIST

- [ ] Firebase initializes successfully in Unity (check Console log)
- [ ] Anonymous sign-in works (user ID appears in log)
- [ ] Save player data to Firestore
- [ ] Load player data from Firestore
- [ ] Firestore security rules prevent unauthorized access
- [ ] Cloud Functions deploy without errors
- [ ] Daily challenge function runs (check Firestore for document)
- [ ] Analytics events tracked (appear in Firebase Console after 24h)
- [ ] WebGL build deploys and loads at .web.app URL
- [ ] No errors in browser console when playing WebGL

---

## 🆘 COMMON ISSUES

**"Dependency error" on initialization:**
- Unity → Assets → External Dependency Manager → Android Resolver → Force Resolve
- Restart Unity

**"Unauthorized" errors in Firestore:**
- Check security rules allow authenticated users
- Verify user is signed in (check auth.CurrentUser != null)

**Cloud Functions not deploying:**
- Check Node.js is installed: `node --version`
- Check you're in correct folder: `cd functions`
- Run `npm install` to install dependencies

**WebGL build won't load:**
- Check browser console for errors
- Firebase hosting may have caching (clear browser cache)
- Verify `index.html` is in public folder

**Analytics not showing data:**
- Wait 24 hours (data processes overnight)
- Check events are being logged (add Debug.Log to confirm)

---

## ✅ SUCCESS CRITERIA

Firebase is fully integrated when:
- [ ] Authentication works (anonymous sign-in)
- [ ] Player data saves to Firestore
- [ ] Player data loads from Firestore
- [ ] Cloud Functions deployed and callable
- [ ] Analytics tracks key events
- [ ] WebGL build hosted and accessible
- [ ] Security rules protect player data
- [ ] No errors in Unity Console or Firebase Console

---

## 🚀 NEXT STEPS

After Firebase complete:
1. **Save/Load from Firestore on game start/quit**
2. **Track key events** (realm complete, sigil collect, daily login)
3. **Test offline persistence** (Firebase caches locally automatically)
4. **Implement social features** (Wish Wall, Time Capsules using Firestore)
5. **Move to Testing & Optimization**

Your game now has:
- ✅ Cloud backend
- ✅ Player data persistence
- ✅ Analytics tracking
- ✅ Free web hosting

Ready for testing! 🧪
