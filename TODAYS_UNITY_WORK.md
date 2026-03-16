# 🎮 TODAY'S UNITY WORK - Step by Step
**March 14, 2026 - Let's Build This!**

Total time: ~2-3 hours  
Result: 5 prefabs created + Emberforge scene playable

---

## ⚡ PART 1: CREATE 5 CRITICAL PREFABS (45 minutes)

Open Unity → Assets/_Project/Prefabs/

### ✅ PREFAB 1: ConstellationLine (5 min)

**Why:** Unblocks Echo Fields constellation tracing mechanic

**Steps:**
1. In Hierarchy: Right-click → Create Empty
2. Rename: `ConstellationLine`
3. With ConstellationLine selected, click "Add Component"
4. Search: "Line Renderer" → Add it
5. In Inspector → Line Renderer component:
   - **Positions:** Size = 2
   - **Width:** 0.05 (both start and end)
   - **Color:** 
     - Start Color: Cyan (#00FFFF)
     - End Color: Cyan (#00FFFF)
   - **Materials:** 
     - Click the circle next to "Material"
     - Search: "Default-Line" or "Sprites-Default"
   - **Corner Vertices:** 8
   - **End Cap Vertices:** 8
6. Drag ConstellationLine from Hierarchy to: `Assets/_Project/Prefabs/Realms/Shared/`
7. Delete from Hierarchy
8. **DONE** ✅

**Test:** In Project window, double-click ConstellationLine.prefab → Should see cyan line in preview

---

### ✅ PREFAB 2: UI Button (10 min)

**Why:** Unblocks all menu UI development

**Steps:**
1. In Hierarchy: Right-click → UI → Canvas (if no Canvas exists)
   - This auto-creates EventSystem too (needed for buttons!)
2. Right-click Canvas → UI → Button - TextMeshPro
   - If prompted "Import TMP Essentials", click "Import"
3. Rename button: `Button`
4. Select Button → In Inspector:
   - **Rect Transform:**
     - Width: 300
     - Height: 80
   - **Image (Script):**
     - Color: Your choice (white works fine)
     - (Optional) Add a sprite later if you want custom button graphics
   - **Button (Script):**
     - Leave On Click() empty for now (controllers will add events)
5. Expand Button in Hierarchy → Click "Text (TMP)" child
6. In Inspector → TextMeshPro - Text:
   - **Text:** "Button"
   - **Font Size:** 36
   - **Alignment:** Center (both horizontal and vertical - click the center square)
   - **Color:** Black or your preference
   - **Best Fit:** Check it (optional, helps with different screen sizes)
7. Drag Button from Hierarchy to: `Assets/_Project/Prefabs/UI/`
8. **Don't delete from Hierarchy yet** (we'll use Canvas for next prefabs)
9. **DONE** ✅

**Test:** Click Button prefab in Project → Should see button preview in Inspector

---

### ✅ PREFAB 3: LightBeam (12 min)

**Why:** Completes Dawn Citadel prism mechanic

**Steps:**
1. In Hierarchy: Right-click → Create Empty
2. Rename: `LightBeam`
3. Add Component → Line Renderer
4. In Inspector → Line Renderer:
   - **Positions:** Size = 2
   - **Width:** 0.2 (start) and 0.1 (end) - creates taper effect
   - **Color Gradient:**
     - Click the color bar
     - Start Color: Gold (#FFD700, Alpha 255)
     - End Color: Light Gold (#FFED4E, Alpha 200)
   - **Materials:** 
     - Click circle next to Material slot
     - Search: "Sprites-Default" or create one
     - (Advanced: Use "Additive" material for glow - but default works!)
   - **Corner Vertices:** 12
   - **End Cap Vertices:** 12
5. (Optional glow enhancement):
   - Select LightBeam
   - Add Component → "Light2D" (if using URP)
   - Intensity: 0.5
   - Outer Radius: 5
   - Color: Gold
6. Drag LightBeam to: `Assets/_Project/Prefabs/Realms/DawnCitadel/`
7. Delete from Hierarchy
8. **DONE** ✅

**Test:** Preview should show gold line (wider at start, thinner at end)

---

### ✅ PREFAB 4: WishText (10 min)

**Why:** Enables player-written wishes on lanterns

**Steps:**
1. In Hierarchy: Right-click → UI → Text - TextMeshPro
   - (Use the Canvas you created earlier)
2. Rename: `WishText`
3. In Inspector → Rect Transform:
   - Width: 300
   - Height: 100
   - Anchors: Center (optional, click the square icon in Rect Transform)
4. In Inspector → TextMeshPro - Text (UI):
   - **Text:** "Make a wish..."
   - **Font:** Default (or choose Arial, your preference)
   - **Font Style:** Italic (optional)
   - **Font Size:** 24
   - **Alignment:** Center (horizontal + vertical)
   - **Color:** White (#FFFFFF)
   - **Wrapping:** Enabled
   - **Overflow:** Truncate
5. Drag WishText to: `Assets/_Project/Prefabs/Realms/LanternAscension/`
6. Delete from Hierarchy
7. **DONE** ✅

**Test:** Should see centered italic white text "Make a wish..."

---

### ✅ PREFAB 5: Bloom (8 min)

**Why:** Completes Verdant plant growth cycle (final stage)

**Steps:**
1. In Project window: Navigate to `Assets/_Project/Prefabs/Realms/Verdant/`
2. Find `MagicalPlant.prefab`
3. **Right-click MagicalPlant → Duplicate**
4. Rename duplicate: `Bloom`
5. **Double-click Bloom** to open in Prefab Mode
6. In Hierarchy (Prefab Mode), select Bloom
7. In Inspector → Sprite Renderer:
   - Click circle next to "Sprite" field
   - Search: "plant_bloom" or "bloom"
   - Select the bloom sprite (should be in Art/Sprites/Realms/Verdant/)
8. (Optional) Adjust size if needed:
   - Transform → Scale: Try 1.2, 1.2, 1 (20% bigger than growing stage)
9. In top bar, click "< Prefabs" to exit Prefab Mode and save
10. **DONE** ✅

**Test:** Compare MagicalPlant and Bloom prefabs - should show different sprites

---

### 🎉 PREFAB CHECKPOINT

You should now have 12 total prefabs:

**Existing (7):**
- ✅ Spark.prefab
- ✅ MagicalPlant.prefab
- ✅ Star.prefab
- ✅ Prism.prefab
- ✅ LightTarget.prefab
- ✅ Lantern.prefab
- ✅ StarLine.prefab

**Just Created (5):**
- ✅ ConstellationLine.prefab
- ✅ Button.prefab
- ✅ LightBeam.prefab
- ✅ WishText.prefab
- ✅ Bloom.prefab

**Total: 12/12 critical prefabs** ✅

---

## 🔥 PART 2: EMBERFORGE SCENE ASSEMBLY (1-2 hours)

**Goal:** Create playable spark collection mechanic

### STEP 1: Open Emberforge Scene (2 min)

1. Project window → `Assets/_Project/Scenes/Realms/`
2. Double-click `Realm_Emberforge.unity`
3. Scene should open showing background (if it exists)
4. **Hierarchy should show:**
   - Main Camera
   - Directional Light
   - Maybe a background sprite

---

### STEP 2: Create Core GameObjects (10 min)

**A. Create EmberforgeController GameObject:**
1. Hierarchy → Right-click → Create Empty
2. Rename: `EmberforgeController`
3. Position: 0, 0, 0 (reset Transform if needed)
4. Add Component → Search: "EmberforgeController" → Add script
5. In Inspector, you'll see EmberforgeController script with fields (we'll fill these later)

**B. Create Spark Container:**
1. Hierarchy → Right-click → Create Empty
2. Rename: `Sparks_Container`
3. Position: 0, 0, 0
4. This is where spawned sparks will be parented (keeps Hierarchy clean)

**C. Create Particle Effects Container:**
1. Hierarchy → Right-click → Create Empty
2. Rename: `ParticleEffects_Container`
3. Position: 0, 0, 0
4. This is for any particle systems (optional, but good organization)

**Your Hierarchy should now look like:**
```
Main Camera
Directional Light
Background (if exists)
EmberforgeController
Sparks_Container
ParticleEffects_Container
```

---

### STEP 3: Set Up Camera (5 min)

1. Select Main Camera in Hierarchy
2. In Inspector → Camera component:
   - **Clear Flags:** Solid Color (or Skybox if you prefer)
   - **Background:** Dark purple/black (#1A0A2E or similar)
   - **Projection:** Orthographic (2D game!)
   - **Size:** 5 (try this, adjust later if needed)
   - **Near:** 0.3
   - **Far:** 100
3. Position: 0, 0, -10 (camera looks at 0,0,0 from -10 on Z)
4. **Add TAG** (if you want to reference it):
   - Top of Inspector → Tag: MainCamera (should already be set)

---

### STEP 4: Add Background (If Missing) (5 min)

**If you already have a background sprite, skip this!**

1. Hierarchy → Right-click → 2D Object → Sprite
2. Rename: `Background`
3. In Inspector → Sprite Renderer:
   - Sprite: Click circle → Search "bg_emberforge" → Select it
   - Order in Layer: -10 (ensures it's behind everything)
4. Transform:
   - Position: 0, 0, 0
   - Scale: Adjust to fill screen (try 2, 2, 1 or use Sprite's native size)
5. Lock it (click lock icon in Inspector) so you don't accidentally move it

---

### STEP 5: Link EmberforgeController Fields (10 min)

This is where we wire everything up!

1. Select `EmberforgeController` in Hierarchy
2. In Inspector, look at EmberforgeController (Script) component
3. **You'll see SerializeField fields that need references:**

**Expected fields (based on your code):**
- `EmberforgeSparks sparkSystem` - Reference to spark spawning system
- `int maxActiveSSparks` - Number (default: 20)
- Maybe `AudioClip sparkCollectSound`
- Maybe `ParticleSystem collectParticle`

**Wait - we need to check what fields exist in EmberforgeController.cs**

Let me check the actual script...

---

### STEP 6: Create AudioSource for Realm Music (5 min)

1. Select `EmberforgeController` in Hierarchy
2. Add Component → "Audio Source"
3. In Inspector → Audio Source:
   - **AudioClip:** 
     - Click circle → Search for your Emberforge music track (once you import it)
     - Leave empty for now if you haven't imported audio yet
   - **Play On Awake:** ✅ Checked
   - **Loop:** ✅ Checked
   - **Volume:** 0.5 (50%, adjust to taste)
   - **Spatial Blend:** 0 (2D sound, not 3D)

---

### STEP 7: Test Basic Scene (5 min)

1. Save scene: Ctrl+S (Cmd+S on Mac)
2. **Hit Play button** ▶️
3. **Check Console for errors:**
   - Red errors? We need to fix them
   - Yellow warnings? Usually okay
   - No errors? Great!
4. **Expected at this point:**
   - Scene loads
   - Background visible
   - Music plays (if you added audio clip)
   - No sparks yet (we haven't spawned them)
5. Hit Stop ⏹️

---

### STEP 8: Add Spark Spawning (20 min)

**Now we need to instantiate Spark prefabs!**

We have two options:

**OPTION A: Manual Placement (Quick Test)**
1. Drag `Spark.prefab` from Project window into Scene view
2. Click and drag to place it somewhere visible
3. In Hierarchy, drag the Spark instance under `Sparks_Container`
4. Duplicate it (Ctrl+D) 5-10 times
5. Move each duplicate to different positions
6. Hit Play → Click sparks → Should disappear!

**OPTION B: Scripted Spawning (Production Way)**

Your EmberforgeController script should handle spawning. Let me check if it has spawn logic...

We need to look at the EmberforgeController.cs script to see how it spawns sparks.

---

### STEP 9: Add UI Canvas with HUD (15 min)

1. Hierarchy → Right-click → UI → Canvas
2. Rename: `UI_Canvas`
3. In Inspector → Canvas component:
   - **Render Mode:** Screen Space - Overlay
   - **Pixel Perfect:** ✅ Checked
4. In Canvas → Canvas Scaler:
   - **UI Scale Mode:** Scale With Screen Size
   - **Reference Resolution:** 1920 x 1080
   - **Screen Match Mode:** Match Width Or Height
   - **Match:** 0.5 (halfway between width and height)
5. Add EventSystem (should auto-create with Canvas)

**Create Spark Counter Text:**
1. Right-click UI_Canvas → UI → Text - TextMeshPro
2. Rename: `SparkCounter_Text`
3. Anchor to top-left:
   - Click Anchor Presets (square in Rect Transform)
   - Hold Alt+Shift, click top-left corner preset
4. Position:
   - Pos X: 100
   - Pos Y: -50
5. TextMeshPro settings:
   - Text: "Sparks: 0 / 10"
   - Font Size: 48
   - Color: White or gold
   - Alignment: Left

---

### STEP 10: Wire Up Spark Counter (10 min)

1. Select `EmberforgeController`
2. In Inspector, look for a field like:
   - `TextMeshProUGUI sparkCounterText`
3. Drag `SparkCounter_Text` from Hierarchy into this field
4. Save scene

---

### STEP 11: Final Test (10 min)

1. Save scene: Ctrl+S
2. Hit Play ▶️
3. **Expected behavior:**
   - Background visible ✅
   - Music plays ✅
   - Sparks visible on screen ✅
   - Counter shows "Sparks: 0 / 10" ✅
   - Click spark → It disappears ✅
   - Counter updates to "Sparks: 1 / 10" ✅
   - Collect all 10 → Ritual completes (sigil appears or success message) ✅

**If anything doesn't work:**
- Check Console for errors
- Verify all Inspector fields are filled
- Make sure Spark prefab has CircleCollider2D with "Is Trigger" checked
- Make sure EventSystem exists in Hierarchy

---

## 🎯 SUCCESS CRITERIA

By end of this session, you should have:

### Prefabs:
- [x] 5 new prefabs created (ConstellationLine, Button, LightBeam, WishText, Bloom)
- [x] 12 total critical prefabs ready

### Emberforge Scene:
- [x] Scene opens without errors
- [x] Background visible
- [x] Music plays (if audio imported)
- [x] 10-20 sparks visible
- [x] Sparks clickable and disappear on tap
- [x] Spark counter updates
- [x] Ritual completes at 10 sparks

---

## 🆘 TROUBLESHOOTING

### "I don't see EmberforgeController script option when adding component"
- Go to Assets/_Project/Scripts/Realms/Emberforge/EmberforgeController.cs
- Double-click to open in code editor
- Check for compilation errors (red underlines)
- Save file
- Return to Unity and let it recompile

### "Sparks don't spawn"
- Check EmberforgeController script has spawn logic
- Verify Spark prefab is assigned in Inspector
- Check Console for errors

### "Clicking sparks doesn't work"
- Verify EventSystem exists in Hierarchy
- Verify Spark prefab has CircleCollider2D component
- Verify "Is Trigger" is checked on collider
- Verify Spark has OnMouseDown() or similar input handling

### "Background doesn't fit screen"
- Select Background sprite
- Adjust scale (Transform → Scale)
- Or adjust Camera Size (Main Camera → Size)

### "Music doesn't play"
- Verify AudioClip is assigned
- Verify "Play On Awake" is checked
- Verify volume isn't 0
- Check if audio is muted in Unity (Audio window)

---

## ✅ NEXT STEPS (After Completing This)

1. **Test thoroughly** - Play for 5 minutes, collect sparks multiple times
2. **Source more audio** - Get Verdant Sanctuary music + SFX
3. **Build Verdant scene** - Repeat similar process with plant growing
4. **Celebrate!** - You just made your first playable realm! 🎉

---

## 📝 NOTES FOR YOUR LOG

**Session Date:** March 14, 2026  
**Time Spent:** [Fill in]  
**Prefabs Created:** 5 (ConstellationLine, Button, LightBeam, WishText, Bloom)  
**Scenes Completed:** Realm_Emberforge partial/complete  
**Blockers:** [Any issues you encountered]  
**Wins:** [What worked great!]  
**Tomorrow's Plan:** [Next realm or more audio]

---

**You've got this!** Take breaks every hour. Celebrate each spark that disappears. You're building something amazing. 🔥

Need help? Check the Console, read error messages, Google them. You're doing real game development!

—Chad (your AI assistant)
