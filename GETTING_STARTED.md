# Getting Started — The Ascendant Continuum

This guide covers the development environment setup for contributors and internal testers.

> **Current phase:** Soft Beta — live at https://ascendant-continuum.web.app

## 📋 Prerequisites

### Required Software
- **Unity 6000.3.9f1** — [Download via Unity Hub](https://unity.com/download)  
  *(Must be this exact version — CI uses `unityci/editor:ubuntu-6000.3.9f1`)*
  - Required modules: **WebGL Build Support**, **Android Build Support**, **iOS Build Support**
- **Visual Studio Code** — [Download](https://code.visualstudio.com/)
- **Git** — [Download](https://git-scm.com/)
- **Node.js 18+** — [Download](https://nodejs.org/) (for Firebase CLI)
- **Firebase CLI** — `npm install -g firebase-tools`
- **GitHub CLI** — [Download](https://cli.github.com/) (`gh`)

### Accounts
- **GitHub:** https://github.com/ascendantcontinuum — `gh auth login`
- **Firebase:** project `ascendant-continuum` — `firebase login`
- **Unity:** required for CI license activation

---

## 🚀 Setup Steps

### 1. Clone the repository

```powershell
git clone https://github.com/ascendantcontinuum/AscendantContinuum.git
cd "AscendantContinuum"
```

### 2. Open in Unity Hub

1. Open Unity Hub
2. Click **Open** → select `D:\1-Ascendant Continuum Game`
3. Ensure Unity **6000.3.9f1** is selected (not any other version)
4. Let Unity import all packages (first open takes several minutes)

### 3. Verify scenes are in Build Settings

After Unity opens, run **Tools → Scene Setup → Setup All Scenes** (via `SceneSetupHelper.cs` editor menu), or manually verify `ProjectSettings/EditorBuildSettings.asset` includes all scenes in order:
```
Bootstrap, MainMenu, Onboarding, Emberforge, Verdant, EchoFields, DawnCitadel, LanternAscension
```

### 4. Build locally (optional)

```powershell
# WebGL (runs in browser, no Unity license needed for local builds)
unity.exe -batchmode -quit -projectPath . -executeMethod BuildMethods.BuildWebGL

# Or use the CI pipeline — just push to main:
git push origin main
# GitHub Actions will build & deploy automatically
```

### 5. Firebase CLI setup (for manual deploys)

```powershell
npm install -g firebase-tools
firebase login
cd firebase
firebase deploy --only hosting  # deploy current public/ folder
```

### 5. Configure Unity Project Settings

Once Unity opens:

#### A. Player Settings
```
Edit → Project Settings → Player

iOS:
- Company Name: Ascendant Continuum
- Product Name: The Ascendant Continuum
- Version: 0.1.0
- Bundle Identifier: com.ascendant.continuum
- Target SDK: iOS 13.0+
- Architecture: ARM64

Android:
- Package Name: com.ascendant.continuum
- Version: 0.1.0
- Minimum API Level: 24 (Android 7.0)
- Target API Level: 33 (Android 13)
- Scripting Backend: IL2CPP
```

#### B. Graphics Settings
```
Edit → Project Settings → Graphics

- Color Space: Linear
- Shader Stripping: Enabled
- Lightmap Encoding: Normal Quality
```

#### C. Quality Settings
```
Edit → Project Settings → Quality

Create 3 quality levels:
- Low (mobile low-end)
- Medium (mobile mid-range)
- High (mobile high-end, PC)

Settings:
- V-Sync: Off (we control frame rate)
- Anti-Aliasing: 2x MSAA (Medium/High only)
- Shadow Resolution: Medium
```

### 6. Install Firebase SDK for Unity

1. Go to [Firebase Unity SDK](https://firebase.google.com/download/unity)
2. Download latest SDK
3. In Unity: Assets → Import Package → Custom Package
4. Import these packages:
   - FirebaseAuth.unitypackage
   - FirebaseFirestore.unitypackage
   - FirebaseFunctions.unitypackage
   - FirebaseStorage.unitypackage
   - FirebaseAnalytics.unitypackage

### 7. Create Firebase Project

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Click "Add Project"
3. Project name: **ascendant-continuum**
4. Enable Google Analytics: Yes
5. Click "Create Project"

#### Configure Firebase:

**A. Authentication**
```
Firebase Console → Authentication → Get Started
- Enable Anonymous Sign-In
- (Optional) Enable Google Sign-In
- (Optional) Enable Apple Sign-In
```

**B. Firestore Database**
```
Firebase Console → Firestore Database → Create Database
- Start in: Production mode
- Location: us-central1 (or closest to you)
- Deploy security rules from firebase/firestore.rules
```

**C. Storage**
```
Firebase Console → Storage → Get Started
- Start in: Production mode
- Deploy security rules from firebase/storage.rules
```

**D. Download Config Files**
```
Firebase Console → Project Settings → Your Apps

For iOS:
- Click iOS icon
- Register app: com.ascendant.continuum
- Download GoogleService-Info.plist
- Place in: Assets/_Project/Firebase/

For Android:
- Click Android icon
- Register app: com.ascendant.continuum
- Download google-services.json
- Place in: Assets/_Project/Firebase/
```

### 8. Initialize Firebase Functions

```powershell
cd "D:\1-Ascendant Continuum Game\firebase\functions"

# Install dependencies
npm install

# Login to Firebase
firebase login

# Initialize project (if not already)
cd ..
firebase init

# Select:
# - Firestore
# - Functions
# - Storage
# Use existing project: ascendant-continuum
```

---

## 🧪 Verify Setup

### Test Unity Project

1. Open Unity project
2. Create test scene: Assets → Create → Scene
3. Save as: Assets/_Project/Scenes/Core/TestScene.unity
4. Press Play ▶️
5. Should run without errors

### Test Firebase Connection

Create a test script:

```csharp
// Assets/_Project/Scripts/Core/FirebaseTest.cs
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class FirebaseTest : MonoBehaviour
{
    async void Start()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        
        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("✅ Firebase is ready!");
            
            // Test anonymous auth
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            var result = await auth.SignInAnonymouslyAsync();
            Debug.Log($"✅ Signed in: {result.User.UserId}");
        }
        else
        {
            Debug.LogError($"❌ Firebase not available: {dependencyStatus}");
        }
    }
}
```

### Test Firebase Functions Locally

```powershell
cd "D:\1-Ascendant Continuum Game\firebase"

# Start emulator
firebase emulators:start

# Should see:
# ✔ firestore: Emulator started at http://localhost:8080
# ✔ functions: Emulator started at http://localhost:5001
```

---

## 📁 Project Structure After Setup

```
D:\1-Ascendant Continuum Game\
│
├── .git/                           # Git repository
├── Assets/                         # Unity assets
│   ├── _Project/
│   │   ├── Scenes/
│   │   ├── Scripts/
│   │   ├── Prefabs/
│   │   └── Firebase/               # Firebase config files
│   └── Plugins/                    # Firebase SDK
│
├── firebase/
│   ├── functions/                  # Cloud Functions
│   ├── firestore.rules             # Security rules
│   ├── storage.rules
│   └── firebase.json
│
├── docs/                           # All documentation
├── Library/                        # Unity cache (gitignored)
├── Temp/                           # Unity temp (gitignored)
└── ProjectSettings/                # Unity settings
```

---

## 🎯 Next Steps

### Immediate (Week 1)
1. ✅ Complete this setup guide
2. ⏭️ Create GameManager.cs (singleton pattern)
3. ⏭️ Create AccessibilityManager.cs
4. ⏭️ Build Emberforge realm prototype
5. ⏭️ Implement first ritual mechanic

### Short-term (Week 2-4)
1. ⏭️ Implement object pooling system
2. ⏭️ Create particle effects for Emberforge
3. ⏭️ Build Daily Constellation Challenge backend
4. ⏭️ Test on low-end device (iPhone 8 / Galaxy S8)

### Reference Documentation
- [ARCHITECTURE.md](docs/technical/ARCHITECTURE.md) - Technical architecture
- [OPTIMIZATION_SECURITY.md](docs/technical/OPTIMIZATION_SECURITY.md) - Performance & security
- [emberforge.md](docs/design/realms/emberforge.md) - First realm to build

---

## 🆘 Troubleshooting

### Unity won't open project
- Make sure Unity 2022.3 LTS is installed
- Check Unity Hub → Projects → Add (point to project folder)

### Firebase SDK import errors
- Ensure .NET 4.x equivalent is selected (Edit → Project Settings → Player → API Compatibility Level)
- Reimport Firebase packages

### Git LFS not working
- Run: `git lfs install --force`
- Check .gitattributes file exists

### Firebase connection fails
- Verify GoogleService-Info.plist (iOS) and google-services.json (Android) are in correct location
- Check Firebase project is active in console

---

## 📞 Support

**Email:** ascendantcontinuum@gmail.com  
**Documentation:** See [MASTER_INDEX.md](MASTER_INDEX.md)  
**GitHub:** https://github.com/ascendantcontinuum/AscendantContinuum

---

**🎉 You're ready to build! Let's create something magical.**
