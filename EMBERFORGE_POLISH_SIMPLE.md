# Emberforge Polish - Simple Step-by-Step

## What We're Doing
Making Emberforge feel complete and polished - test the completion screen, adjust visuals, and fix any rough edges.

---

## Part 1: Test the Completion (10 minutes)

### Step 1: Open Emberforge Scene
1. In Unity, **Project panel** (bottom)
2. Navigate: `Assets/_Project/Scenes/Realms/`
3. **Double-click** `Realm_Emberforge.unity`

### Step 2: Test Completion
1. Hit **Play** ▶️
2. **Click sparks** until counter shows `20/20`
3. **What SHOULD happen:**
   - Completion panel appears with "Emberforge Complete! ✨"
   - Shows your total sparks collected
   - Shows star rating (1-3 stars based on performance)

### Step 3: If Nothing Happens
**Check RealmCompletionPanel is connected:**

1. Stop Play mode (click ▶️ again to stop)
2. In **Hierarchy**, click `[Emberforge Realm]`
3. In **Inspector**, find `Emberforge Sparks (Script)` component
4. Look for field: **`Completion Panel`**
5. **If it says "None":**
   - Find `RealmCompletionPanel` in Hierarchy (probably under `HUDCanvas`)
   - **Drag** `RealmCompletionPanel` into the `Completion Panel` field
6. **Save scene:** Ctrl+S
7. Try Step 2 again

---

## Part 2: Adjust Spark Visuals (15 minutes)

### Fine-Tune Spark Colors

**Step 1: Open Spark Prefab**
1. **Project panel** → `Assets/_Project/Prefabs/Realms/Emberforge/`
2. **Double-click** `Spark` prefab

**Step 2: Adjust Color**
1. Select `Spark` in Hierarchy (prefab mode)
2. In **Inspector**, find `Spark (Script)` component
3. Find **`Spark Color`** field (currently orange)
4. **Click the color box** → Color picker opens
5. **Try these options:**
   - Brighter orange: `#FFA500`
   - Golden: `#FFD700`  
   - Ember red: `#FF6B35`
   - Keep current if you like it!
6. **Save:** Ctrl+S
7. Click **`<`** arrow to exit prefab mode

**Step 3: Test Colors**
1. Hit **Play** ▶️
2. Watch sparks spawn
3. **Like it?** ✅ Done!
4. **Want different?** Stop Play, repeat Step 2 with different color

---

### Adjust Spark Movement (Optional)

**If sparks feel too floaty or too static:**

**Step 1: Open Spark Prefab** (same as above)

**Step 2: Adjust Movement Settings**
1. In **Inspector**, `Spark (Script)` component
2. Find **`Float Speed`** (currently `1`)
   - **Higher = faster bobbing** (try `1.5` or `2`)
   - **Lower = slower, calmer** (try `0.5`)
3. Find **`Float Amplitude`** (currently `0.5`)
   - **Higher = bigger up/down movement** (try `0.8` or `1`)
   - **Lower = subtle movement** (try `0.3`)
4. **Save:** Ctrl+S, exit prefab mode
5. **Test:** Hit Play and watch

---

## Part 3: Adjust Spawn Rate (5 minutes)

**If sparks feel too slow or too hectic:**

### Step 1: Select EmberforgeRealm
1. In **Hierarchy**, click `[Emberforge Realm]`

### Step 2: Adjust Settings
In **Inspector**, find `Emberforge Sparks (Script)`:

**Too Many Sparks?**
- Change `Max Active Sparks` from `20` to `15` or `10`

**Too Few Sparks?**
- Change `Max Active Sparks` from `20` to `25` or `30`

**Spawning Too Fast?**
- Change `Spawn Interval` from `1` to `1.5` or `2`

**Spawning Too Slow?**
- Change `Spawn Interval` from `1` to `0.7` or `0.5`

**Sparks Disappear Too Fast?**
- Change `Spark Lifetime` from `6` to `8` or `10`

**Sparks Stay Too Long?**
- Change `Spark Lifetime` from `6` to `4` or `5`

### Step 3: Test & Iterate
1. **Save:** Ctrl+S
2. Hit **Play** ▶️
3. Play for 30 seconds - does it feel better?
4. **Yes?** ✅ Done!
5. **No?** Stop Play, tweak values more, repeat

---

## Part 4: UI Counter Position (5 minutes)

**If the counter is hard to see or in the wrong spot:**

### Step 1: Find Counter
1. In **Hierarchy**, expand `HUDCanvas`
2. Click `SparkCounterText`

### Step 2: Move It
In **Inspector**, find `Rect Transform`:

**Move to Top-Right:**
- **Anchor preset:** Click the box, hold ALT+SHIFT, click top-right square
- **Pos X:** `-50`
- **Pos Y:** `-50`

**Move to Top-Center:**
- **Anchor preset:** ALT+SHIFT + click top-center
- **Pos X:** `0`
- **Pos Y:** `-50`

**Make Bigger:**
- **Font Size:** Change `36` to `48` or `60`

**Change Color:**
- Scroll down to `Color` in TextMeshPro component
- Click color box → pick white, yellow, or orange

### Step 3: Test
1. **Save:** Ctrl+S
2. Hit **Play** ▶️ 
3. Look at counter - is it readable? Is position good?

---

## Part 5: Add Simple Particle Effect (Optional - 15 minutes)

**Make sparks explode in particles when collected:**

### Step 1: Create Particle System
1. In **Hierarchy**, right-click `[Emberforge Realm]` → **Effects → Particle System**
2. Name it: `SparkCollectVFX`

### Step 2: Configure Particles
In **Inspector**, adjust these settings:

**Main Module:**
- **Duration:** `0.5`
- **Looping:** ❌ UNCHECK
- **Play On Awake:** ❌ UNCHECK
- **Start Lifetime:** `0.5`
- **Start Speed:** `3`
- **Start Size:** `0.1`
- **Start Color:** Orange (same as sparks)
- **Max Particles:** `20`

**Emission:**
- **Rate over Time:** `0`
- Click **`+`** next to **Bursts**
- **Time:** `0`, **Count:** `10`

**Shape:**
- **Shape:** `Sphere`
- **Radius:** `0.2`

### Step 3: Save as Prefab
1. **Drag** `SparkCollectVFX` from Hierarchy → Project panel → `Assets/_Project/Prefabs/VFX/` folder
2. Create `VFX` folder if it doesn't exist
3. **Optional:** Delete `SparkCollectVFX` from Hierarchy (we'll spawn it via code)

### Step 4: Wire to Script (Advanced - Skip if Unsure)
*This requires editing Spark.cs to play VFX on collect. If comfortable with code, add this to CollectSpark() method:*
```csharp
// Play VFX at spark position
GameObject vfx = Instantiate(collectVFXPrefab, transform.position, Quaternion.identity);
Destroy(vfx, 1f);
```
**Or skip this step - visual polish can wait!**

---

## Part 6: Test Full Session (10 minutes)

### Final Playtest

1. Hit **Play** ▶️
2. **Play for 5 minutes straight:**
   - Click sparks
   - Watch counter increase
   - Get to 20/20
   - See completion screen
3. **Ask yourself:**
   - ✅ Is clicking satisfying?
   - ✅ Are sparks visible and pretty?
   - ✅ Is counter readable?
   - ✅ Does completion feel rewarding?
   - ✅ Would I want to play this for 5 minutes daily?

4. **If YES to all:** ✅ Emberforge is DONE!
5. **If NO to any:** Go back to relevant Part above and adjust

---

## Quick Checklist

- [ ] Completion screen appears at 20 sparks
- [ ] Spark colors look good
- [ ] Spark movement feels natural
- [ ] Spawn rate feels balanced (not too many, not too few)
- [ ] Counter is readable and well-positioned
- [ ] Overall experience feels meditative and satisfying

---

## Common Issues & Fixes

**Issue:** Sparks collect but counter stays at 0  
**Fix:** EmberforgeRealm → EmberforgeController → Spark Counter Text field is empty. Drag SparkCounterText into it.

**Issue:** No completion screen at 20 sparks  
**Fix:** EmberforgeRealm → EmberforgeSparks → Completion Panel field is empty. Find RealmCompletionPanel in Hierarchy and drag it in.

**Issue:** Counter says "Sparks: 12/20" but I've clicked way more  
**Fix:** Sparks are collecting correctly! The first number is YOUR total, the `/20` is just the goal. It's working!

**Issue:** Sparks are invisible again  
**Fix:** Open Spark prefab → SpriteRenderer → Sprite field → make sure `icon_sparks` is assigned (not "None")

**Issue:** Can't click sparks anymore  
**Fix:** Open Spark prefab → Circle Collider 2D → Radius should be `10` or higher (not `0.4`)

---

## What's Next?

✅ **Emberforge is polished!**  
➡️ **Move to Verdant Sanctuary** (see VERDANT_SUPER_SIMPLE.md)  
➡️ **Or add audio** (see ZERO_BUDGET_AUDIO_GUIDE.md)  
➡️ **Or build for Android/iOS** to test on device

---

## Achievement Unlocked 🏆
**"Forge Master"** - You've created and polished a complete, playable game realm from scratch!
