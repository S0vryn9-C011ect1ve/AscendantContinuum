# Legacy Verdant Simple Guide (Superseded)

This file is superseded by canonical implementation guidance.

Use:

1. `docs/operations/implementation-guides.md` for realm assembly patterns
2. `docs/QUICK_REFERENCE_CHECKLIST.md` for checklist tracking
3. `docs/ASSET_PRODUCTION_GUIDE.md` for asset placement and naming
4. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress
   - Click ⊙ → search `growing` → select **`plant_growing`**
   
4. **Bloom Sprite:** 
   - Click ⊙ → search `bloom` → select **`plant_bloom`**

**Bloom Particles:** Leave as `None` for now (optional polish later)

**Audio (All 3 fields):** Leave as `None` (we'll add audio later)
- Water Sound
- Grow Sound  
- Bloom Sound

### Step 6: Save & Exit Prefab

1. **Ctrl+S** to save prefab
2. Click **`<`** arrow at top-left of Hierarchy
   - This exits prefab mode and returns to scene

---

## Part 5: Wire Everything Together (10 minutes)

### Step 1: Go Back to Scene

- Make sure you're viewing **`Realm_Verdant`** scene
- Hierarchy should show: `Main Camera`, `Directional Light`, `[Verdant Garden]`
- If you see just "MagicalPlant", click the **`<`** arrow to exit prefab mode

### Step 2: Connect MagicalPlant Prefab to VerdantGarden

1. In **Hierarchy**, click **`[Verdant Garden]`**
2. In **Inspector**, find **`Verdant Garden (Script)`** component
3. Look for field: **`Plant Prefab`**
4. **From Project panel** (bottom):
   - Navigate to: `Assets/_Project/Prefabs/Realms/Verdant/`
   - **Drag** `MagicalPlant` prefab into the **`Plant Prefab`** field

### Step 3: Configure Garden Settings

Still in `Verdant Garden (Script)` component:

**Plant Settings:**
- **Max Plants:** `10` (max plants on screen at once)
- **Growth Time Per Stage:** `30` (seconds)

**Spawn Settings:**
- **Garden Size:**
  - **X:** `8`
  - **Y:** `6`

**Audio:** Leave as `None` for now

**Completion:**
- **Goal Plants:** `5` (need to grow 5 plants to complete)
- **Completion Panel:** Leave as `None` for now (we'll add UI later)

### Step 4: Connect Garden to Controller

1. Still on **`[Verdant Garden]`** in Hierarchy
2. In **Inspector**, find **`Verdant Controller (Script)`** component
3. Look for field: **`Garden`**
4. **From Hierarchy:**
   - **Drag** `[Verdant Garden]` GameObject into the **`Garden`** field
   - (Yes, dragging the GameObject onto its own field - this is normal!)

---

## Part 6: Add Background (Optional - 5 minutes)

### Step 1: Create Background Sprite

1. **Hierarchy** → **Right-click** → **2D Object → Sprite**
2. Name it: **`Background`**

### Step 2: Assign Background Image

In **Inspector**:
- **Sprite Renderer → Sprite:** 
  - Click ⊙
  - Search for a nature/forest background sprite
  - Or leave as white square for now

- **Transform → Position:**
  - X: `0`, Y: `0`, Z: `1` (behind everything)

- **Sprite Renderer → Order in Layer:** `-10` (renders behind plants)

### Step 3: Scale to Fit

- **Transform → Scale:**
  - X: `10`, Y: `10`, Z: `1`
  - (Adjust until it fills camera view in Scene window)

---

## Part 7: TEST IT! (10 minutes)

### Step 1: Save Everything

- **Ctrl+S** (save scene)

### Step 2: Hit Play

- Click the **Play button** ▶️ at top of Unity

### Step 3: What Should Happen

**Immediate:**
- ✅ 3 plants should spawn in random positions
- ✅ They should look like small sprites on screen
- ✅ Background shows (if you added one)

**Try Clicking a Plant:**
- Click directly on a plant sprite
- **What SHOULD happen:**
  - Plant gets watered
  - After 30 seconds, plant grows to next stage
  - After 2 minutes (4 stages × 30s), plant blooms

**What you WON'T see yet:**
- ❌ Visual changes (all stages use same sprite for now)
- ❌ Counter UI (not added yet)
- ❌ Audio (not added yet)
- ❌ Completion screen (not wired yet)

### Step 4: Check Console for Errors

Look at **Console tab** (bottom):
- **Only warnings:** ✅ Okay!
- **Red errors:** ❌ Something needs fixing (message me!)

---

## Part 8: Add UI Counter (10 minutes)

### Step 1: Create Canvas

**If you already have HUDCanvas from Emberforge:**
- ✅ Skip to Step 2

**If no Canvas exists:**
1. **Hierarchy** → **Right-click** → **UI → Canvas**
2. Name it: **`HUDCanvas`**
3. Canvas settings:
   - Render Mode: `Screen Space - Overlay`

### Step 2: Create Counter Text

1. **Right-click** `HUDCanvas` → **UI → Text - TextMeshPro**
   - If popup appears about "Import TMP Essentials", click **Import**
2. Name it: **`PlantCounterText`**

### Step 3: Configure Text

In **Inspector**, find `TextMeshPro - Text (UI)` component:

**Text:**
- Type: **`Plants Grown: 0 / 5`**

**Font Settings:**
- **Font Size:** `36` or `48`
- **Color:** White or green
- **Alignment:** Center or Top-Left

**Position (Rect Transform):**
- Click the **anchor preset** box (top-left of Rect Transform)
- Hold **ALT+SHIFT**, click **top-left** square
- **Pos X:** `50`
- **Pos Y:** `-50`
- **Width:** `400`
- **Height:** `100`

### Step 4: Wire Counter to VerdantController

**WE NEED TO ADD CODE FOR THIS - Skip for now, we'll do this together when you're testing!**

---

## Common Issues & Fixes

**Issue:** Plants don't spawn when I hit Play  
**Fix:** 
- Check `[Verdant Garden]` → `Verdant Garden (Script)` → `Plant Prefab` field has `MagicalPlant` assigned
- Check Console for errors

**Issue:** I can't click plants  
**Fix:** 
- Open `MagicalPlant` prefab
- Make sure `Circle Collider 2D` exists and `Is Trigger` is checked
- Make sure `Radius` is at least `0.5`

**Issue:** Plants are invisible  
**Fix:**
- Open `MagicalPlant` prefab
- Check `Sprite Renderer` → `Sprite` field has `Plant_sprite` assigned
- Check `Color` is white and not transparent

**Issue:** Plants spawn but don't grow  
**Fix:**
- Plants need to be **clicked** to be watered first
- After clicking, wait 30 seconds for growth
- Check Console for script errors

**Issue:** Plants spawn outside camera view  
**Fix:**
- `[Verdant Garden]` → `Verdant Garden (Script)` → Garden Size
- Reduce X and Y (try X: `6`, Y: `4`)

---

## Next Steps

**After this guide:**
1. ✅ Test plant spawning
2. ✅ Test clicking to water
3. ✅ Test growth timing
4. ⏭️ **Create different sprites for each growth stage** (seed, sprout, plant, bloom)
5. ⏭️ **Add UI counter** (need to add code - I'll help!)
6. ⏭️ **Add audio** (watering sound, bloom sound)
7. ⏭️ **Polish visuals** (particle effects on bloom)

---

## Quick Checklist

- [ ] Created Realm_Verdant scene
- [ ] Camera configured (Orthographic, Size 5)
- [ ] Created [Verdant Garden] GameObject
- [ ] Attached VerdantController and VerdantGarden scripts
- [ ] Configured MagicalPlant prefab with sprite and collider
- [ ] Connected Plant Prefab to VerdantGarden
- [ ] Connected Garden to VerdantController  
- [ ] Tested: 3 plants spawn on Play
- [ ] Tested: Can click plants to water them
- [ ] Optional: Added background
- [ ] Optional: Added counter UI

---

## What Makes This Different from Emberforge?

**Emberforge:** Click sparks → disappear → counter goes up  
**Verdant:** Click plants → stay on screen → grow over time → bloom → counter goes up

**Growth stages:** 4 stages (Seed/Sprout/Plant/Bloom) vs sparks (1 state)  
**Timing:** Plants take 2 minutes to fully grow vs sparks disappear in 6 seconds  
**Feel:** Patient, nurturing, slow vs active, reactive, fast

---

## Achievement Unlocked 🏆
**"Green Thumb"** - You've built your second playable realm!

---

**Got stuck? Message me the error/screenshot and I'll help debug! 🌿**
