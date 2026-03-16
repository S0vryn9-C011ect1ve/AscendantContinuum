# 🎮 Scene Assembly Step-by-Step Guide
**Start from Scratch - Make Your Scenes Interactive**

## Current Status Understanding
- ✅ You have: Realm backgrounds (photos visible)
- ✅ You have: All code scripts written
- ❌ Missing: Interactive GameObjects (sparks, plants, stars, etc.)
- ❌ Missing: Prefabs
- ❌ Missing: Script connections to GameObjects

**Goal:** Make each realm playable with interactive objects

---

## 🔧 Unity Basics You'll Need

### Creating GameObjects
1. Right-click in Hierarchy → Create Empty (for logic controllers)
2. Right-click in Hierarchy → 2D Object → Sprite (for visual objects)
3. Drag sprite from Project window to Hierarchy (creates sprite GameObject)

### Attaching Scripts
1. Select GameObject in Hierarchy
2. In Inspector, click "Add Component"
3. Type script name (e.g., "EmberforgeController")
4. Click the script from dropdown

### Creating Prefabs
1. Create GameObject in scene with all components
2. Drag from Hierarchy to Project window (Assets/_Project/Prefabs/)
3. GameObject turns blue = prefab created
4. Now you can duplicate/reuse it

---

## 📋 REALM 1: EMBERFORGE (Start Here - Easiest)

### Step 1: Open Scene (2 min)
```
File → Open Scene → Assets/_Project/Scenes/Realms/Realm_Emberforge.unity
```

### Step 2: Verify Background (1 min)
- Look in Hierarchy - do you see a GameObject with the background sprite?
- If yes ✅, skip to Step 3
- If no ❌:
  1. Drag `bg_emberforge.png` from Assets/_Project/Art/Backgrounds/ to Hierarchy
  2. Rename it "Background"
  3. In Inspector, set Position to (0, 0, 0)
  4. Set Sorting Layer to "Background" (or Order in Layer = -10)

### Step 3: Create Realm Controller (5 min)
This is the "brain" that runs the realm.

1. **Create empty GameObject:**
   - Right-click Hierarchy → Create Empty
   - Rename to "EmberforgeController"
   - Position: (0, 0, 0)

2. **Attach script:**
   - Select EmberforgeController in Hierarchy
   - Inspector → Add Component → "EmberforgeController" (type to search)
   - Script should appear in Inspector

3. **Verify script fields:**
   - You should see fields like:
     - Spark Prefab (empty for now)
     - Sparks Container (empty for now)
     - Max Sparks, etc.

### Step 4: Create Spark Container (2 min)
This holds all the spark GameObjects to keep Hierarchy organized.

1. Right-click Hierarchy → Create Empty
2. Rename to "SparksContainer"
3. Drag it UNDER EmberforgeController (makes it a child)

### Step 5: Create ONE Spark GameObject (10 min)
We'll make this into a prefab later.

1. **Create sprite:**
   - Drag `spark_small.jpg` from Assets/_Project/Art/Sprites/Realms/Emberforge/ to Hierarchy
   - Rename to "Spark"
   - Position: (0, 0, 0)

2. **Add Circle Collider 2D:**
   - Select Spark
   - Inspector → Add Component → "Circle Collider 2D"
   - Check "Is Trigger" ✅
   - Adjust Radius if needed (default is fine)

3. **Add Spark Script:**
   - Inspector → Add Component → Search "Spark"
   - If you have a Spark.cs script, it should attach
   - If not, we'll create a simple one

4. **Optional - Add Animation:**
   - Inspector → Add Component → "Animator"
   - (Skip for now, you can add later)

### Step 6: Create Spark Prefab (3 min)
1. Drag "Spark" GameObject from Hierarchy to `Assets/_Project/Prefabs/Realms/Emberforge/`
2. Spark in Hierarchy turns blue = success!
3. Now delete Spark from Hierarchy (we'll use the prefab)

### Step 7: Link Prefab to Controller (5 min)
1. Select "EmberforgeController" in Hierarchy
2. Find "Spark Prefab" field in Inspector
3. Drag your Spark prefab from Project window to that field
4. Find "Sparks Container" field
5. Drag "SparksContainer" GameObject from Hierarchy to that field

### Step 8: Create Audio Source (3 min)
1. Select "EmberforgeController"
2. Inspector → Add Component → "Audio Source"
3. Uncheck "Play On Awake" ❌
4. Check "Loop" ✅
5. Leave "AudioClip" empty for now (you'll add music later)

### Step 9: Test! (5 min)
1. Press Play ▶️
2. Check Console for errors (bottom of Unity)
3. If you see red errors:
   - Read error message
   - Likely missing script references
   - Fix and try again

**Expected at this point:**
- Scene loads without critical errors
- EmberforgeController script is running
- May not be interactive yet (that's okay!)

### Step 10: Add Camera Setup (5 min)
1. Find "Main Camera" in Hierarchy
2. Inspector → Camera component:
   - Projection: Orthographic
   - Size: 5 (adjust based on your art)
   - Background: Black or realm color
   - Position: (0, 0, -10)

### Step 11: Add Canvas for UI (7 min)
1. Right-click Hierarchy → UI → Canvas
2. Canvas automatically creates EventSystem (needed for touch input)
3. Select Canvas, in Inspector:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler → UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080

4. **Create Spark Counter UI:**
   - Right-click Canvas → UI → Text - TextMeshPro
   - If prompted to import TMP Essentials, click "Import"
   - Rename to "SparkCountText"
   - Position in top-right corner
   - Text: "Sparks: 0/10"
   - Font Size: 48
   - Color: White

### Step 12: Link UI to HUD Manager (5 min)
1. Create empty GameObject in Hierarchy
2. Rename to "HUDManager"
3. Add Component → "HUDManager" script
4. Drag "SparkCountText" to the appropriate field in HUDManager

### Step 13: Spawn Sparks at Runtime (Test Script Connection)
At this point, your EmberforgeController script should:
- Spawn sparks when scene starts
- Position them randomly

**If sparks don't appear:**
1. Check EmberforgeController script is enabled (checkbox in Inspector)
2. Verify Spark Prefab is assigned
3. Verify Sparks Container is assigned
4. Check Console for errors

### Step 14: Add Touch Input Manager (5 min)
1. Create empty GameObject
2. Rename to "TouchInputManager"
3. Add Component → "TouchInputManager" script
4. This enables tap detection

### Step 15: Test Touch Interaction (3 min)
1. Press Play ▶️
2. Click on sparks (simulates touch)
3. They should disappear and increment counter
4. If not working:
   - Check Spark has Collider2D
   - Check Collider "Is Trigger" is enabled
   - Check TouchInputManager is in scene

---

## ✅ EMBERFORGE CHECKLIST

Before moving to next realm, verify:
- [ ] Background sprite visible
- [ ] EmberforgeController in scene with script attached
- [ ] Spark prefab created and assigned
- [ ] Sparks spawn when scene starts (10-20 visible)
- [ ] Canvas with spark counter UI
- [ ] HUDManager shows spark count
- [ ] TouchInputManager in scene
- [ ] Clicking sparks makes them disappear and increment counter
- [ ] No critical errors in Console
- [ ] AudioSource ready for music (even if silent for now)

**Once all checked ✅, save scene and move to Verdant Sanctuary!**

---

## 📋 REALM 2: VERDANT SANCTUARY

### Overview
This realm has plants that grow through 4 stages when tapped.

### Step 1: Open Scene
```
File → Open Scene → Assets/_Project/Scenes/Realms/Realm_Verdant.unity
```

### Step 2: Setup (Same as Emberforge)
- Verify background (bg_verdant.png)
- Create VerdantController GameObject + script
- Create PlantsContainer GameObject
- Create Canvas + UI (Plant Count text)
- Create HUDManager
- Create TouchInputManager
- Add Main Camera (if not present)
- Add AudioSource to VerdantController

### Step 3: Create Plant GameObject (15 min)
This is more complex - 4 growth stages.

**Option A: Single Sprite (Simpler)**
1. Drag plant sprite to Hierarchy
2. Rename "Plant"
3. Add Circle Collider 2D (Is Trigger ✅)
4. Add script (if you have VerdantPlant.cs or similar)
5. Store 4 different sprites for growth stages

**Option B: Animator (Better but complex)**
1. Create Plant GameObject
2. Add Animator component
3. Create Animation Controller with 4 states:
   - Seed
   - Sprout
   - Plant
   - Bloom
4. Trigger transitions on tap

**For now, use Option A - you can upgrade later**

### Step 4: Plant Prefab
1. Drag Plant GameObject to Assets/_Project/Prefabs/Realms/Verdant/
2. Delete from Hierarchy
3. Assign to VerdantController's "Plant Prefab" field

### Step 5: Spawn Plants
VerdantController should spawn 10 plants at fixed positions
- 2 rows of 5, or grid pattern
- All start as "seed" stage

### Step 6: Growth Logic
When player taps plant:
1. Increment growth stage (seed → sprout → plant → bloom)
2. Change sprite
3. If stage 4 (bloom), mark as complete
4. Increment plant count UI

### Step 7: Test
- [ ] 10 plants spawn as seeds
- [ ] Tapping cycles through growth stages
- [ ] Fully grown plants count toward completion
- [ ] UI updates

---

## 📋 REALM 3: ECHO FIELDS (Constellations)

### Overview
Trace constellations by connecting stars in order.

### Setup (Similar to previous)
- Background (bg_echofields.png)
- EchoFieldsController + script
- StarsContainer
- Canvas + UI
- TouchInputManager

### Create Star GameObject (8 min)
1. Drag star sprite to Hierarchy
2. Rename "Star"
3. Add Circle Collider 2D (Is Trigger ✅)
4. Add Component → "Line Renderer" (for drawing connections)
5. Make prefab

### Constellation Pattern
Stars spawn in pattern (ConstellationTracer.cs handles logic):
- 5-9 stars in specific shape
- Player traces by tapping in order
- Line Renderer draws connection
- Complete pattern = sigil reward

### Test
- [ ] Stars spawn in constellation pattern
- [ ] Tapping first star starts tracing
- [ ] Line appears between tapped stars
- [ ] Completing pattern triggers success

---

## 📋 REALM 4: DAWN CITADEL (Light Puzzles)

### Overview
Rotate prisms to redirect light beams to targets.

### Create Prism GameObject (10 min)
1. Drag prism sprite to Hierarchy
2. Rename "Prism"
3. Add Box Collider 2D (Is Trigger ✅)
4. Script handles rotation on tap
5. Make prefab

### Create Light Beam GameObject (12 min)
1. Create empty GameObject "LightBeam"
2. Add Line Renderer component
3. Set width: 0.1
4. Set material: Default (white)
5. Color: Golden yellow
6. Make prefab

### Puzzle Logic
- Light source emits beam
- Prism redirects 90°
- Hit all targets = complete

### Test
- [ ] Prisms spawn
- [ ] Tapping rotates prism
- [ ] Light beams visible
- [ ] Hitting targets triggers success

---

## 📋 REALM 5: LANTERN ASCENSION

### Overview
Write wishes and release lanterns into void.

### Create Lantern GameObject (10 min)
1. Drag lantern sprite to Hierarchy
2. Rename "Lantern"
3. Add Rigidbody2D
   - Gravity Scale: -0.5 (floats upward)
4. Add script for upward drift
5. Make prefab

### Create Wish Input UI (15 min)
1. In Canvas, create Panel
2. Add InputField - TextMeshPro
3. Add Button "Release Lantern"
4. Position center screen

### Logic
- Player types wish (optional)
- Taps "Release"
- Lantern spawns and floats up
- Fades out after 5 seconds
- Wish saved to Firebase (later)

### Test
- [ ] Input field appears
- [ ] Typing works
- [ ] Release button spawns lantern
- [ ] Lantern floats upward
- [ ] Lantern fades and destroys

---

## 🎯 GENERAL SCENE ASSEMBLY TIPS

### Hierarchy Organization
```
Realm_Emberforge
├── Background
├── Main Camera
├── Managers
│   ├── EmberforgeController
│   ├── TouchInputManager
│   ├── AudioManager (optional)
│   └── HUDManager
├── GameObjects
│   └── SparksContainer
│       ├── Spark (instance)
│       ├── Spark (instance)
│       └── ...
├── Canvas
│   ├── SparkCountText
│   ├── BackButton
│   └── SettingsButton
└── EventSystem
```

### Common Issues & Fixes

**Sparks/Objects don't spawn:**
- Controller script not attached
- Prefab not assigned in Inspector
- Spawn method not called in Start()

**Touch not working:**
- Missing EventSystem in scene
- TouchInputManager not in scene
- Collider not set to "Is Trigger"
- Collider on wrong layer

**UI not visible:**
- Canvas Render Mode wrong
- UI behind camera (Z position)
- Text color same as background

**Scripts show "Missing":**
- Script file renamed or moved
- Reattach script to GameObject

---

## 🔄 WORKFLOW FOR EACH REALM

1. **Open scene** (2 min)
2. **Verify/add background** (2 min)
3. **Create controller GameObject + script** (5 min)
4. **Create container for interactive objects** (2 min)
5. **Create ONE interactive GameObject** (10-15 min)
   - Sprite
   - Collider
   - Script
   - Test in scene
6. **Create prefab** (3 min)
7. **Assign prefab to controller** (2 min)
8. **Create UI canvas + text** (7 min)
9. **Create HUDManager** (5 min)
10. **Create TouchInputManager** (3 min)
11. **Add AudioSource to controller** (3 min)
12. **Press Play and test** (10 min)
13. **Fix errors, iterate** (variable)
14. **Save scene** ✅

**Time per realm:** 1-3 hours (faster after first one)

---

## 📅 RECOMMENDED ORDER

1. **Emberforge** (easiest - learn workflow)
2. **Verdant** (medium - multi-stage objects)
3. **Lantern** (medium - UI input)
4. **Echo Fields** (harder - line rendering)
5. **Dawn Citadel** (hardest - physics + rotation)

---

## 🆘 GET UNSTUCK

### Error: "NullReferenceException"
- Means a script is trying to use something that doesn't exist
- Check all script fields in Inspector are assigned
- Common: Forgot to assign prefab, container, or UI element

### Error: "MissingComponentException"
- Script expects component that's not on GameObject
- Add the component or check script requirements

### Visual: Objects not visible
- Check camera position and size
- Check sorting layers/order in layer
- Check object is in camera bounds

### Touch: Not responding
- Verify EventSystem in scene
- Check Collider2D exists and "Is Trigger" enabled
- Verify TouchInputManager running

**Still stuck? Check Unity Console for specific error messages - they point to the exact problem!**

---

## ✅ SUCCESS CRITERIA

After each realm, you should be able to:
- Press Play ▶️
- See background and interactive objects
- Tap/click objects and see response
- See UI update
- No red errors in Console (yellow warnings OK)
- Save and reload scene without issues

**Once all 5 realms done, move to Tutorial implementation!** 🚀
