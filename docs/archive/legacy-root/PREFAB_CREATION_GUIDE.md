# Legacy Prefab Creation Guide (Superseded)

This file is superseded by canonical Unity workflow documentation.

Use:

1. `docs/operations/unity-scene-prefab-workflows.md` for shared prefab workflow
2. `docs/operations/implementation-guides.md` for realm-specific setup patterns
3. `docs/ASSET_PRODUCTION_GUIDE.md` for asset destination folders and specs
4. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress
   - Polygon Collider 2D (matches triangle/prism shape)
   - OR Box Collider 2D (simpler)
   - Is Trigger: ✅

3. **Add rotation logic:**
   - Script should rotate 90° on tap
   - `transform.Rotate(0, 0, 90);`
   - Or smooth rotation with Lerp

4. **Add visual feedback:**
   - Optional: Particle effect when rotated
   - Optional: Audio clip

5. **Variables script needs:**
   - `int currentRotation` (0, 90, 180, 270)
   - `void OnTap()` → increment rotation

6. **Create prefab**

### 2. Light Beam Prefab (12 min)

This is trickier - uses Line Renderer or custom mesh.

**Option A: Line Renderer (Simpler)**

1. **Create empty GameObject:**
   - Rename "LightBeam"

2. **Add Line Renderer:**
   - Width: 0.2
   - Positions: Start (0,0,0), End (5,0,0)
   - Material: Particles/Additive (glowing effect)
   - Color: #FFD700 (gold)
   - Start Width: 0.3, End Width: 0.1 (tapered beam)

3. **Add emission (optional glow):**
   - Material → Enable Emission
   - Color: Yellow

4. **Script calculates beam path:**
   - Raycasts from light source
   - Hits prism → redirects 90°
   - Updates Line Renderer positions

5. **Create prefab**

**Option B: Sprite Beam (Easier for 2D)**

1. Create stretched sprite (1x10 pixels, white)
2. Scale X to beam length in script
3. Rotate toward target
4. Simpler than Line Renderer!

### 3. Light Target Prefab (5 min)

1. Create sprite (target/goal icon)
2. Add Circle Collider 2D (Is Trigger ✅)
3. Script detects beam hit
4. Create prefab

---

## 🏮 LANTERN ASCENSION PREFABS

### 1. Lantern Prefab (15 min)

1. **Create Sprite:**
   - Use lantern sprite
   - Rename "Lantern"

2. **Add Rigidbody2D:**
   - Body Type: Dynamic
   - Gravity Scale: -0.3 (negative = floats up)
   - Linear Drag: 0.5 (slow drift)
   - Angular Drag: 0
   - Freeze Rotation Z: ✅ (don't spin)

3. **Add gentle sway script:**
```csharp
// Simple sway
void Update() {
    float sway = Mathf.Sin(Time.time * 0.5f) * 0.2f;
    transform.position += new Vector3(sway * Time.deltaTime, 0, 0);
}
```

4. **Add fade-out script:**
```csharp
// Fade over time
float lifetime = 0f;
void Update() {
    lifetime += Time.deltaTime;
    if (lifetime > 5f) {
        // Fade alpha
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a -= Time.deltaTime * 0.5f;
        sr.color = c;
        
        if (c.a <= 0) Destroy(gameObject);
    }
}
```

5. **Add glow particle:**
   - Child GameObject → Particle System
   - Small particles drifting upward
   - Warm orange/yellow color

6. **Create prefab**

### 2. Wish Text Prefab (Optional - 10 min)

If you want wishes to appear on lanterns:

1. Create 3D Text or TextMeshPro
2. Parent under Lantern
3. Position on lantern "paper"
4. Populate from player input
5. Include in Lantern prefab

---

## 🎨 UI PREFABS

### 1. Button Prefab (10 min)

Reusable styled button.

1. **Create Button:**
   - Canvas → Right-click → UI → Button - TextMeshPro

2. **Style it:**
   - Image: Use your `button_primary.jpg`
   - Text: "Button Text"
   - Font size: 36
   - Color: White

3. **Create prefab:**
   - Drag to `Assets/_Project/Prefabs/UI/`

4. **Create variants:**
   - Duplicate prefab
   - Create: ButtonPrimary, ButtonSecondary, ButtonAccent
   - Use different sprites from your UI folder

### 2. Panel Prefab (8 min)

Reusable background panel.

1. Create Image → Use `panel_bg_dark.jpeg`
2. Add shadow (optional)
3. Create prefab

### 3. Achievement Popup Prefab (15 min)

1. **Create Panel Image:**
   - Rounded rectangle
   - Semi-transparent background

2. **Add elements:**
   - Icon (achievement icon)
   - Title text
   - Description text

3. **Add animation:**
   - Slide in from top
   - Hold 3 seconds
   - Slide out

4. **Create prefab**

---

## 🔊 AUDIO PREFABS (Optional but Recommended)

### Audio Source Pool Prefab (8 min)

1. **Create empty GameObject:**
   - Rename "AudioSource_Pooled"

2. **Add Audio Source:**
   - Play On Awake: No
   - Loop: No
   - Volume: 1

3. **Script manages:**
   - Play clip
   - Return to pool when done

4. **Create prefab**

5. **AudioManager spawns 20 of these at start**

---

## 🛠️ PREFAB WORKFLOW TIPS

### Making Variants
After creating base prefab:
1. Drag prefab to Hierarchy
2. Modify (change color, size, etc.)
3. Right-click → Prefab → Create Variant
4. Now you have: Spark, SparkLarge, SparkRed, etc.

### Editing Prefabs
**Option 1: Edit in Scene**
1. Drag prefab to Hierarchy
2. Modify
3. Inspector → Overrides → Apply All (pushes changes to prefab)

**Option 2: Prefab Mode**
1. Double-click prefab in Project window
2. Opens isolated edit mode
3. Edit freely
4. Auto-saves

### Nesting Prefabs
- Spark prefab can contain:
  - Sprite (child)
  - Glow ring (child)
  - Particle effect (child)
- Entire structure becomes one prefab

### Prefab Instances
- Blue name = linked to prefab
- Override values → name becomes bold
- Revert: Right-click → Prefab → Revert

---

## 📋 COMPLETE PREFAB CHECKLIST

### Emberforge
- [ ] Spark.prefab
- [ ] (Optional) SparkGlow.prefab
- [ ] (Optional) SparkLarge.prefab

### Verdant Sanctuary
- [ ] Plant.prefab (with 4 growth stages)
- [ ] (Optional) PlantSeed.prefab (separate prefabs per stage)

### Echo Fields
- [ ] Star.prefab
- [ ] (Optional) ConstellationLine.prefab

### Dawn Citadel
- [ ] Prism.prefab
- [ ] LightBeam.prefab
- [ ] LightTarget.prefab

### Lantern Ascension
- [ ] Lantern.prefab
- [ ] (Optional) WishCard.prefab

### UI (Shared)
- [ ] ButtonPrimary.prefab
- [ ] ButtonSecondary.prefab
- [ ] Panel.prefab
- [ ] AchievementPopup.prefab
- [ ] (Optional) LoadingSpinner.prefab

### Effects (Shared)
- [ ] CollectParticle.prefab
- [ ] SuccessParticle.prefab
- [ ] (Optional) TransitionEffect.prefab

---

## 🎯 TESTING PREFABS

After creating each prefab:

1. **Drag to scene** - appears instantly? ✅
2. **Check all components** - scripts, colliders attached? ✅
3. **Press Play** - behaves as expected? ✅
4. **Duplicate in scene** - creates identical copies? ✅
5. **Edit one** - changes show as overrides? ✅
6. **Revert/Apply** - prefab updates correctly? ✅

---

## 🚀 QUICK START: 30-Minute Essential Prefabs

If you want to get playable FAST, create only these:

1. **Spark.prefab** (5 min) - sprite + collider
2. **Plant.prefab** (8 min) - sprite + collider + 4 growth sprites
3. **Star.prefab** (5 min) - sprite + collider
4. **Prism.prefab** (5 min) - sprite + collider
5. **Lantern.prefab** (7 min) - sprite + rigidbody

Polish later! Get basic interaction working first.

---

## 🆘 COMMON PREFAB ISSUES

**Prefab won't spawn:**
- Check it's assigned in controller script
- Check spawning code is called (Debug.Log to verify)

**Prefab spawns but breaks:**
- Missing component in prefab
- Add component before saving prefab

**Changes don't save:**
- Must Apply Overrides or edit in Prefab Mode
- Check prefab isn't Read-Only in file system

**Prefab looks different in scene:**
- Check for overrides (bold values)
- Revert or Apply to sync

---

## ✅ SUCCESS CRITERIA

You have enough prefabs when:
- [ ] Each realm has interactive objects
- [ ] Objects spawn correctly from scripts
- [ ] Touch/click detection works
- [ ] Basic visuals are pleasing
- [ ] No critical component errors

**Polish can come later - function first!** 🎮

Once prefabs are created, go back to SCENE_ASSEMBLY_GUIDE.md and link them to your controllers!
