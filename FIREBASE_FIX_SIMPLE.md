# Firebase Deploy - Simple Fix Steps

## What We're Doing
Getting your Firebase hosting to deploy properly so the web version of your game can go live.

---

## Step 1: Check if Firebase CLI is Installed

**In PowerShell:**
```powershell
firebase --version
```

**If you see a version number (like `13.0.0`):** ✅ Continue to Step 2  
**If you see "command not found" or error:** Install Firebase CLI:
```powershell
npm install -g firebase-tools
```

---

## Step 2: Check Login Status

**Run this:**
```powershell
cd "d:\1-Ascendant Continuum Game\firebase"
firebase login:list
```

**If you see your email:** ✅ Skip to Step 4  
**If you see "No authorized accounts":** Continue to Step 3

---

## Step 3: Login to Firebase

**Run this:**
```powershell
firebase login
```

- Your browser will open
- Sign in with your Google account (the one you used to create the Firebase project)
- Grant permissions
- Return to terminal - should say "Success! Logged in as [your-email]"

---

## Step 4: Verify Project Connection

**Run this:**
```powershell
cd "d:\1-Ascendant Continuum Game\firebase"
firebase projects:list
```

**You should see:** A list of your Firebase projects  
**Look for:** `ascendant-continuum` in the list

**If you DON'T see `ascendant-continuum`:**
- ❌ The project doesn't exist yet
- Go to https://console.firebase.google.com
- Click "Add project"
- Name it: `ascendant-continuum`
- Follow the setup wizard
- Come back and try Step 4 again

---

## Step 5: Deploy to Firebase

**Run this:**
```powershell
cd "d:\1-Ascendant Continuum Game\firebase"
firebase deploy --only hosting
```

**What should happen:**
- Terminal shows: "Deploying to 'ascendant-continuum'..."
- Progress bar appears
- After 30-60 seconds: "Deploy complete!"
- You'll get a URL like: `https://ascendant-continuum.web.app`

---

## Common Errors & Fixes

### Error: "Permission denied"
**Fix:** You're not logged into the correct Google account
```powershell
firebase logout
firebase login
```
Make sure to use the account that OWNS the Firebase project

### Error: "Project not found"
**Fix:** The project name doesn't match
1. Open `.firebaserc` in the `firebase` folder
2. Check the project name (should be `ascendant-continuum`)
3. Go to Firebase Console and verify the project ID matches exactly

### Error: "Quota exceeded"
**Fix:** Firebase free tier has daily limits
- Wait 24 hours
- Or upgrade to Blaze plan (pay-as-you-go, but still mostly free)

### Error: "No public directory"
**Fix:** Should not happen (we have `firebase/public/` folder)
But if it does:
```powershell
cd "d:\1-Ascendant Continuum Game\firebase"
mkdir public -Force
```

---

## Test Your Deployment

**After successful deploy:**
1. Copy the hosting URL from terminal (e.g., `https://ascendant-continuum.web.app`)
2. Open it in your browser
3. You should see your game's landing page

---

## Quick Reference Commands

```powershell
# Check login
firebase login:list

# Login
firebase login

# List projects
firebase projects:list

# Deploy
cd "d:\1-Ascendant Continuum Game\firebase"
firebase deploy --only hosting

# View deployment history
firebase hosting:sites:list
```

---

## When to Skip Firebase (For Now)

Firebase is **optional** for initial development. You can:
- ✅ Build all 5 realms in Unity
- ✅ Test locally on your computer
- ✅ Even build Android/iOS apps
- ⏭️ Add Firebase later in v1.1

**Only need Firebase for:**
- Web version hosting
- Cross-device save syncing
- Leaderboards
- Cloud Functions

If you want to focus on gameplay first, **skip Firebase and come back to it later!**

---

## Next Steps After Deploy Success

1. ✅ Update README.md with your live URL
2. ✅ Share the web link on social media
3. ✅ Add domain (optional): `firebase hosting:sites:create`
4. ⏭️ Deploy Cloud Functions later: `firebase deploy --only functions`
