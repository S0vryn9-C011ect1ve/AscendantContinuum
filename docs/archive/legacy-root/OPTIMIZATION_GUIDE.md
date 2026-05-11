# Legacy Optimization Guide (Superseded)

This file is superseded by canonical technical documentation.

Use:

1. `docs/technical/OPTIMIZATION_SECURITY.md` for optimization and security guidance
2. `docs/technical/testing-and-verification.md` for verification expectations
3. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress

**Problem:** Uncompressed audio is huge

**Solutions:**

**A. Compress Music**
1. Select music file in Project
2. Inspector:
   - Load Type: **Streaming** (music)
   - Compression Format: **Vorbis**
   - Quality: **70** (balance size/quality)
   - Sample Rate: **44100 Hz** (music)

**For music files:**
- Streaming loads audio in chunks (saves RAM)
- Vorbis is OGG format (small file size)

**B. Compress SFX**
1. Select SFX file
2. Inspector:
   - Load Type: **Decompress On Load** (short sounds)
   - Compression Format: **ADPCM** (fast) or **Vorbis** (smaller)
   - Sample Rate: **22050 Hz** (half quality, fine for SFX)

**Impact:** 
- Before: 50 MB audio
- After: 10-15 MB audio ✅

---

### 6. REDUCE BUILD SIZE

**Target:** <100MB total

**A. Player Settings**
- Edit → Project Settings → Player
- Tab: Other Settings
- "Strip Engine Code": ✅ (removes unused Unity code)
- "Script Debugging": ❌ (disable for release builds)
- "Managed Stripping Level": Medium or High

**B. Texture Compression** (see section 4)

**C. Audio Compression** (see section 5)

**D. Remove Unused Assets**
- Assets → Right-click → "Select Dependencies"
- Delete unused sprites, sounds, scripts

**E. WebGL Build Size**
- Build Settings → Player Settings → WebGL
- "Compression Format": Brotli or Gzip
- "Code Optimization": Size (smaller but slightly slower) or Speed

---

### 7. OPTIMIZE UI

**Problem:** UI updates every frame = CPU waste

**Solutions:**

**A. Update UI Only When Changed**

**Bad:**
```csharp
void Update()
{
    sparkCountText.text = $"Sparks: {sparks}"; // Every frame!
}
```

**Good:**
```csharp
void OnSparkCollected()
{
    sparks++;
    UpdateSparkUI(); // Only when changed
}

void UpdateSparkUI()
{
    sparkCountText.text = $"Sparks: {sparks}";
}
```

**B. Disable Raycast on Non-Interactive UI**
- Select UI element (Image, Text)
- Uncheck "Raycast Target" (unless it's a button)
- Reduces raycasting overhead

**C. Use Canvas Groups for Fading**
Instead of changing alpha on each child, use CanvasGroup:
```csharp
CanvasGroup cg = panel.GetComponent<CanvasGroup>();
cg.alpha = 0.5f; // One operation vs. many
```

---

### 8. OPTIMIZE PHYSICS (CPU)

**Problem:** Too many physics calculations

**Solutions:**

**A. Use Triggers for Detection**
Already done! Your colliders are set to "Is Trigger" ✅

**B. Reduce Physics Simulation Rate**
- Edit → Project Settings → Time
- Fixed Timestep: 0.02 (50 times per second, default)
- Increase to 0.03 or 0.04 if physics isn't critical

**C. Disable Unnecessary Rigidbodies**
- If object doesn't need physics, don't add Rigidbody2D
- If static, set Body Type: Kinematic or Static

---

### 9. QUALITY SETTINGS (Player Choice!)

Give players performance options.

**Create 3 Quality Presets:**

1. **Low (Default for old devices):**
   - Particle count: 50%
   - Texture quality: Half resolution
   - Shadow quality: Disabled
   - Anti-aliasing: Disabled

2. **Medium:**
   - Particle count: 75%
   - Texture quality: Full resolution
   - Shadow quality: Hard shadows
   - Anti-aliasing: 2x

3. **High (Default for new devices):**
   - Particle count: 100%
   - Texture quality: Full resolution
   - Shadow quality: Soft shadows
   - Anti-aliasing: 4x

**How to Implement:**

1. **Edit → Project Settings → Quality**
2. **Add 3 levels** (Low, Medium, High)
3. **Per level, adjust:**
   - Pixel Light Count
   - Shadows
   - Anti-Aliasing

4. **In Settings Menu, add dropdown:**
```csharp
public void SetQualityLevel(int level)
{
    QualitySettings.SetQualityLevel(level);
    PlayerPrefs.SetInt("QualityLevel", level);
}
```

5. **Auto-detect on first launch:**
```csharp
void Start()
{
    if (!PlayerPrefs.HasKey("QualityLevel"))
    {
        // Auto-detect
        if (SystemInfo.systemMemorySize < 4000) // <4GB RAM
            QualitySettings.SetQualityLevel(0); // Low
        else if (SystemInfo.systemMemorySize < 6000)
            QualitySettings.SetQualityLevel(1); // Medium
        else
            QualitySettings.SetQualityLevel(2); // High
    }
}
```

---

### 10. MEMORY OPTIMIZATION

**Problem:** High memory usage → crashes on low-RAM devices

**Solutions:**

**A. Reduce Textures in Memory**
- Compress textures (section 4)
- Unload unused textures: `Resources.UnloadUnusedAssets();`

**B. Avoid Memory Leaks**
Common leaks:
- Subscribing to events without unsubscribing
- Static references to destroyed objects
- Circular references

**Fix:**
```csharp
void OnEnable()
{
    EventManager.OnSparkCollected += HandleSparkCollected;
}

void OnDisable()
{
    EventManager.OnSparkCollected -= HandleSparkCollected; // Unsubscribe!
}
```

**C. Reduce Garbage Collection (GC) Spikes**

**Bad:**
```csharp
void Update()
{
    string text = "Sparks: " + sparks; // Creates garbage every frame!
}
```

**Good:**
```csharp
void OnSparkCollected()
{
    sparkText.text = $"Sparks: {sparks}"; // Only when changed
}
```

Also avoid:
- LINQ in Update() (use for loops)
- new Object() in Update() (cache references)
- String concatenation in loops (use StringBuilder)

---

## 📊 OPTIMIZATION RESULTS CHECKLIST

After optimizing, verify:

- [ ] **FPS:** Stable 60fps on target device (Galaxy S8 / iPhone 8)
- [ ] **Draw Calls:** <100 (check Profiler → Rendering)
- [ ] **Batches:** <50 (sprite atlas helps here)
- [ ] **Memory:** <200MB (check Profiler → Memory)
- [ ] **Build Size:** <100MB (check build folder)
- [ ] **Load Time:** <5 seconds to main menu
- [ ] **Battery:** <5% drain per 10-minute session
- [ ] **No stutters** during gameplay

---

## 🎯 QUICK WINS (Do These First)

If you only have 1 hour to optimize:

1. **Create Sprite Atlas** (15 min) → Reduces 50+ draw calls
2. **Compress Textures** (15 min) → Reduces build size 50%
3. **Compress Audio** (10 min) → Reduces build size 20%
4. **Cache Component References** (10 min) → Speeds up scripts
5. **Strip Engine Code** (5 min) → Reduces build size 10%
6. **Test on real device** (5 min) → Verify improvements

**Total: 60 min, Massive improvement** ✅

---

## 🔍 PROFILER INTERPRETATION

### Reading the Profiler Graph

**CPU Usage (ms per frame):**
- **0-10ms:** Excellent (100+ fps possible)
- **10-16ms:** Good (60fps achievable)
- **16-33ms:** Poor (30-60fps, target higher)
- **33ms+:** Bad (<30fps, unplayable)

**Top CPU Consumers (Usually):**
1. Rendering (draw calls, batching)
2. Scripts (Update, FixedUpdate)
3. Physics (collision detection)
4. UI (Canvas rebuilds)
5. Garbage Collection spikes

**Memory Usage:**
- **<100MB:** Excellent
- **100-200MB:** Good
- **200-300MB:** Acceptable
- **300MB+:** Will crash on low-end devices

---

## 🆘 PERFORMANCE DEBUGGING

**If FPS is still low after optimization:**

1. **Disable Half the Objects**
   - Hide half the sparks, test FPS
   - If FPS improves → too many objects
   - If no change → bottleneck is elsewhere

2. **Disable Particles**
   - Turn off all particle systems
   - If FPS improves → particles are the issue

3. **Disable Scripts**
   - Comment out Update() in major scripts
   - Test each script individually
   - Find which script is slow

4. **Simplify Visuals**
   - Use solid colors instead of textures
   - Reduce resolution
   - If FPS improves → GPU bottleneck

5. **Profile on Real Device**
   - Simulator performance ≠ real device
   - Test on oldest device you support

---

## ✅ OPTIMIZATION COMPLETE

You're ready to launch when:
- [ ] Profiler shows 60fps sustained
- [ ] No frame drops during gameplay
- [ ] Build size <100MB
- [ ] Tested on 3+ devices successfully
- [ ] Cold start time <5 seconds
- [ ] Realm transitions smooth (<1 second)

---

## 🚀 POST-LAUNCH OPTIMIZATION

After launch, monitor:
- Firebase Performance Monitoring (free)
  - Track FPS across all devices
  - Identify slow devices
  - See crash reports

If players report lag:
1. Check which devices (target optimization there)
2. Add Quality Settings for those devices
3. Push update with optimization

**Remember:** You can optimize more post-launch. Don't delay launch for perfection!

---

## 📚 FURTHER READING

- Unity Manual: Optimizing for Mobile
- Unity Learn: Performance Optimization
- r/Unity3D subreddit (search "mobile optimization")

---

**You now have everything you need to launch your game!** 🎉

**Your Complete Guide Package:**
1. ✅ SCENE_ASSEMBLY_GUIDE.md
2. ✅ PREFAB_CREATION_GUIDE.md
3. ✅ TUTORIAL_IMPLEMENTATION_GUIDE.md
4. ✅ FIREBASE_SETUP_GUIDE.md
5. ✅ TESTING_GUIDE.md
6. ✅ OPTIMIZATION_GUIDE.md
7. ✅ ZERO_BUDGET_AUDIO_GUIDE.md
8. ✅ 6_WEEK_LAUNCH_ROADMAP.md
9. ✅ ZERO_BUDGET_MARKETING_GUIDE.md

**Start with:** SCENE_ASSEMBLY_GUIDE.md → Build Emberforge first!
