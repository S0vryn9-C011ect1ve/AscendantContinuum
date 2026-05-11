# Legacy Scene Assembly Guide (Superseded)

This file is superseded by canonical Unity workflow documentation.

Use:

1. `docs/operations/unity-scene-prefab-workflows.md` for shared scene assembly workflow
2. `docs/operations/implementation-guides.md` for realm-specific wiring patterns
3. `docs/ASSET_PRODUCTION_GUIDE.md` for destination folders and asset specs
4. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress
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
