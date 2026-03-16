# 🎯 EMBERFORGE SCENE - EXACT WIRING GUIDE
**Use this alongside TODAYS_UNITY_WORK.md**

Based on your actual EmberforgeController.cs and EmberforgeSparks.cs scripts.

---

## WHAT YOU NEED TO CREATE

### GameObject Structure:
```
Realm_Emberforge (Scene Root)
├── Main Camera
├── Background (sprite: bg_emberforge.png)
├── EmberforgeRealm (empty GameObject)
│   ├── EmberforgeController.cs (attached)
│   └── EmberforgeSparks.cs (attached)
├── Sparks_Container (empty, for organization)
└── UI_Canvas
    ├── SparkCounter_Text (TextMeshPro)
    └── CompletionPanel (optional, or use prefab later)
```

---

## STEP-BY-STEP WIRING

### 1. Create EmberforgeRealm GameObject (5 min)

1. Hierarchy → Right-click → Create Empty
2. Rename: `EmberforgeRealm`
3. Position: 0, 0, 0
4. Add Component → Search: "EmberforgeController"
5. Add Component → Search: "EmberforgeSparks"

You should now see BOTH scripts attached to EmberforgeRealm.

---

### 2. Wire EmberforgeController (2 min)

Select `EmberforgeRealm` → In Inspector, find **EmberforgeController (Script)**:

**Field to fill:**
- **Sparks System:** 
  - Drag `EmberforgeRealm` (the same GameObject) into this field
  - OR leave empty - script auto-finds it with `GetComponentInChildren<EmberforgeSparks>()`
  
That's it! EmberforgeController only needs one reference.

---

### 3. Wire EmberforgeSparks (10 min)

Select `EmberforgeRealm` → In Inspector, find **EmberforgeSparks (Script)**:

#### Spark Settings:
- **Spark Prefab:** 
  - Project window → Navigate to `Assets/_Project/Prefabs/Realms/Emberforge/`
  - Drag `Spark.prefab` into this field
- **Pool Size:** 50 (default is fine)
- **Spark Lifetime:** 3 (seconds before spark auto-despawns)
- **Spawn Radius:** 5 (how far from center sparks appear)

#### Spawn Settings:
- **Spawn Interval:** 0.5 (seconds between spawns)
- **Max Active Sparks:** 20 (max sparks on screen at once)

#### Audio:
- **Collect Sound:** 
  - Drag your `spark_collect.wav` SFX here (once you import it)
  - Leave empty for now if you haven't imported audio yet
- **Spawn Sound:** 
  - Drag your `spark_spawn.wav` SFX here
  - Leave empty for now if needed

#### Completion:
- **Completion Panel:** 
  - Leave empty for now (we'll create this later)
  - This is the popup that shows "Ritual Complete!"
- **Goal Sparks:** 20 (how many sparks to collect to complete ritual)

---

### 4. Create UI Canvas with Spark Counter (15 min)

**A. Create Canvas:**
1. Hierarchy → Right-click → UI → Canvas
2. Rename: `UI_Canvas`
3. In Inspector → Canvas:
   - Render Mode: Screen Space - Overlay
   - Pixel Perfect: ✅
4. In Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080

**B. Create Spark Counter Text:**
1. Right-click UI_Canvas → UI → Text - TextMeshPro
   - If prompted "Import TMP Essentials", click Import
2. Rename: `SparkCounter_Text`
3. Anchor to top-left:
   - Rect Transform → Click Anchor Presets icon
   - Hold Alt+Shift → Click top-left preset
4. In Rect Transform:
   - Pos X: 150
   - Pos Y: -80
   - Width: 400
   - Height: 100
5. In TextMeshPro - Text (UI):
   - Text: "Sparks: 0 / 20"
   - Font Size: 54
   - Color: Gold (#FFD700) or White
   - Alignment: Center middle
   - Font Style: Bold (optional)

---

### 5. Link Spark Counter to Script (OPTIONAL)

**Note:** Your current EmberforgeSparks.cs doesn't have a UI text field!

You have two options:

**OPTION A: Add UI field to script (requires code edit)**

Open `Assets/_Project/Scripts/Realms/Emberforge/EmberforgeSparks.cs` and add:

```csharp
[Header("UI")]
[SerializeField] private TMPro.TextMeshProUGUI sparkCounterText;
```

Then in the `CollectSpark()` method, add:
```csharp
if (sparkCounterText != null)
    sparkCounterText.text = $"Sparks: {sparksCollected} / {goalSparks}";
```

**OPTION B: Create separate UI manager**

Keep EmberforgeSparks focused on gameplay, create a separate script for UI updates. This is better architecture but more work.

**OPTION C: Manual update for testing**

Just manually change the text as you test. Not ideal but works for now.

**RECOMMENDED:** Go with Option A for today - it's a 2-minute code edit.

---

### 6. Test Configuration Checklist

Before hitting Play, verify:

**EmberforgeRealm GameObject has:**
- [x] EmberforgeController.cs attached
- [x] EmberforgeSparks.cs attached
- [x] EmberforgeSparks has Spark Prefab assigned

**Hierarchy has:**
- [x] Main Camera
- [x] Background sprite (optional but nice to have)
- [x] UI_Canvas with EventSystem
- [x] SparkCounter_Text

**Spark.prefab (in Project) has:**
- [x] SpriteRenderer component
- [x] CircleCollider2D component
- [x] "Is Trigger" checked on collider
- [x] Some script to handle OnMouseDown or OnTriggerEnter2D

---

### 7. Expected Behavior When You Hit Play ▶️

1. **Scene loads** → No errors in Console ✅
2. **Sparks start spawning** → Every 0.5 seconds, up to 20 max ✅
3. **Sparks float around** → Within spawn radius ✅
4. **Click a spark** → It disappears ✅
5. **Spark counter updates** → "Sparks: 1 / 20" ✅
6. **Collect 20 sparks** → Completion panel shows (if wired) ✅
7. **Music plays** → If AudioSource added with music clip ✅

---

## 🐛 TROUBLESHOOTING

### "Sparks don't spawn"
**Check:**
- Is Spark Prefab assigned in EmberforgeSparks inspector?
- Does Spark.prefab exist at `Assets/_Project/Prefabs/Realms/Emberforge/Spark.prefab`?
- Any red errors in Console?

**Fix:**
- Verify prefab path
- Check Console for missing reference errors
- Make sure sparkPrefab field is not "None (GameObject)"

---

### "Sparks spawn but don't disappear when clicked"
**Check:**
- Does Spark.prefab have a script that handles clicks?
- Does it have a Collider2D component?
- Is "Is Trigger" checked?
- Does EventSystem exist in Hierarchy?

**Fix:**
You may need to add click handling to Spark prefab. Create a simple script:

```csharp
// Spark.cs - attach to Spark.prefab
using UnityEngine;

public class Spark : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Tell EmberforgeSparks we were collected
        SendMessageUpwards("CollectSpark", gameObject, SendMessageOptions.DontRequireReceiver);
        gameObject.SetActive(false); // Return to pool
    }
}
```

---

### "Counter doesn't update"
**Issue:** EmberforgeSparks.cs doesn't have UI reference

**Fix:** Add UI field to script (see Option A above)

---

### "Console shows 'EmberforgeSparks not found'"
**Check:**
- Is EmberforgeSparks.cs attached to EmberforgeRealm?
- Is it attached to the same GameObject or a child?

**Fix:**
- Verify both scripts are on EmberforgeRealm
- Or drag EmberforgeSparks reference into EmberforgeController's field manually

---

### "Sparks spawn outside camera view"
**Fix:**
- Reduce Spawn Radius (try 3 instead of 5)
- Or increase Camera Size
- Or constrain spawn area in EmberforgeSparks script

---

## ✅ SUCCESS CHECKLIST

Emberforge scene is DONE when:

- [ ] Scene opens without errors
- [ ] Background visible (if added)
- [ ] Sparks spawn automatically
- [ ] Sparks are clickable
- [ ] Sparks disappear on click
- [ ] Counter shows progress (if UI wired)
- [ ] Collecting goal amount (20) triggers completion
- [ ] Music plays (if audio imported)
- [ ] No red errors in Console
- [ ] Game loop feels satisfying to play!

---

## 🎨 POLISH IDEAS (OPTIONAL - Do Later!)

After basic functionality works:

**Visual Polish:**
- Add particle effect on spark collect (sparkles)
- Add glow shader to sparks
- Animate sparks (float, rotate, pulse)
- Add trail renderer for motion blur

**Audio Polish:**
- Different pitch for each spark collected (pitch += 0.1f)
- Ambient fire crackling sound
- Whoosh sound when spark spawns
- Success fanfare on ritual complete

**Gameplay Polish:**
- Combo system (collect sparks quickly for bonus)
- Spark colors based on rarity (common white, rare gold)
- Daily challenge: "Collect 100 sparks today"
- Achievement popup on screen

**But don't do these today!** Get it working first, polish later.

---

## 📊 TIME ESTIMATE

- GameObject setup: 10 min
- Script wiring: 15 min
- UI creation: 15 min
- First test: 5 min
- Bug fixing: 10-20 min
- **Total: 55-65 minutes**

You're building a real game! Keep going! 🔥

—Chad
