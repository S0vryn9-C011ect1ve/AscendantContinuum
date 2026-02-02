# Firebase Configuration Files

This folder contains Firebase configuration files for different platforms.

## Files:

- **google-services.json** - Android configuration (auto-detected by Unity)
- **firebase-web-config.json** - WebGL configuration (used for web builds)

## Note:

These files contain your Firebase project credentials. They are already in .gitignore and will NOT be committed to GitHub (for security).

If you need to regenerate these files:
1. Go to: https://console.firebase.google.com/
2. Select: ascendant-continuum project
3. Settings ⚙️ → Project Settings → Your apps
4. Download the config files again

## Unity Usage:

Unity's Firebase SDK will automatically detect:
- `google-services.json` for Android builds
- For WebGL, you'll need to manually initialize using the web config

No manual Gradle or npm setup needed - Unity handles everything!
