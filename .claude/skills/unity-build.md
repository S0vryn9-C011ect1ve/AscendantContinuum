# Unity Build Skill

**Type:** Tool-Powered  
**Purpose:** Automate Unity builds with validation and error handling

---

## When to Use

- User requests: "build android", "create APK", "build for iOS"
- After C# script changes that need build verification
- Before deployment to test environments
- When switching platforms

---

## Prerequisites Check

Before building, verify:
1. Unity Editor is **closed** (script builds can't run with Editor open)
2. Target platform modules are installed (Android/iOS/WebGL)
3. No compilation errors in project
4. Firebase config files present (if using backend)

---

## Build Process

### 1. Validate Environment

```powershell
# Check Unity installation
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe"
if (!(Test-Path $unityPath)) {
    Write-Error "Unity 6000.3.9f1 not found"
    exit 1
}

# Check for platform modules
$androidModule = "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Data\PlaybackEngines\AndroidPlayer"
if (!(Test-Path $androidModule) -and $Platform -eq "Android") {
    Write-Error "Android Build Support not installed. Run: .\Setup-UnityModules.ps1"
    exit 1
}
```

### 2. Run Build Script

```powershell
# Development build (faster, includes debug symbols)
.\Build.ps1 -Platform WebGL -BuildType Development

# Release build (optimized, production-ready)
.\Build.ps1 -Platform Android -BuildType Release

# All platforms
.\Build.ps1 -Platform All -BuildType Release
```

### 3. Validate Output

After build completes:
- Check exit code (0 = success)
- Verify build artifacts exist in `Builds/[Platform]/`
- Test build on target device/browser

---

## Common Errors & Fixes

### "Could not find Unity installation"
**Cause:** Unity not installed or wrong version  
**Fix:** Install Unity 6000.3.9f1 via Unity Hub

### "Android Build Support not found"
**Cause:** Android module not installed  
**Fix:** Run `.\Setup-UnityModules.ps1` and install Android Build Support

### "Compilation errors detected"
**Cause:** C# scripts have errors  
**Fix:** 
1. Use `get_errors` tool to see errors
2. Fix compilation issues
3. Retry build

### "Build succeeded but file not found"
**Cause:** Build path mismatch  
**Fix:** Check `Build.ps1` output for actual build location

### "Unity process already running"
**Cause:** Unity Editor is open  
**Fix:** Close Unity Editor, then retry

---

## Platform-Specific Notes

### Android
- Requires Android SDK (auto-installed with module)
- Keystore needed for release builds (use `Setup-AndroidKeystore.ps1`)
- Test on real device, not just emulator
- APK size target: < 150MB

### iOS
- Requires macOS for final build (Xcode)
- Windows Unity can generate Xcode project
- Test on real device via TestFlight
- IPA size target: < 200MB

### WebGL
- Fastest build time (~3-5 min)
- Test in multiple browsers (Chrome, Firefox, Safari)
- Check console for errors (JS interop issues)
- Compressed size target: < 50MB

---

## Builder-Validator Loop

1. **Build:** Run `.\Build.ps1`
2. **Validate:** Check for errors in output
3. **Test:** Load build on target platform
4. **Fix:** Address any runtime issues
5. **Iterate:** Repeat until validation passes

---

## Post-Build Checklist

- [ ] Build completed with exit code 0
- [ ] Output files exist and are non-zero size
- [ ] Build loads without crashes
- [ ] Core gameplay loop works (30-sec test)
- [ ] IAP stubs don't cause errors (if not implemented)
- [ ] Firebase connects (if online)

---

## Automation Opportunities

Create GitHub Action that:
1. Triggers on push to `main`
2. Runs all platform builds
3. Uploads artifacts
4. Deploys WebGL to Firebase Hosting
5. Notifies on Discord/Slack

Already implemented: `.github/workflows/build-deploy.yml`
