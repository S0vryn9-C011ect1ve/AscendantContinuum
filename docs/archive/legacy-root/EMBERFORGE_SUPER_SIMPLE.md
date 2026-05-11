# Legacy Emberforge Simple Guide (Superseded)

This file is superseded by canonical implementation guidance.

Use:

1. `docs/operations/implementation-guides.md` for starter realm workflows
2. `docs/QUICK_REFERENCE_CHECKLIST.md` for checklist tracking
3. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress
- Click it
- Pick a dark color (dark purple or black)
- Close color picker

**Position:**
- Inspector → **Transform** section
- Position should be:
  - X: 0
  - Y: 0  
  - Z: **-10** (negative 10 is important!)

---

## 🎯 STEP 7: Test It! (5 min)

Time to see if it works!

### Save the Scene First:
**📍 Location: Top menu**

1. Top menu: **File → Save** (or press Ctrl+S)
2. Make sure scene is saved

### Hit Play!
**📍 Location: Top center of Unity**

1. Look at top center of Unity window
2. Find the **▶️ Play button**
3. **Click it!**

### What Should Happen:

1. Background appears ✅
2. After a moment, **sparks start appearing!** ✅
3. They should float around the screen ✅
4. **Click on a spark** → It should disappear! ✅
5. More sparks keep spawning (up to 20) ✅

### If It Works:
🎉 **CONGRATULATIONS!** You made a playable game mechanic!

### If Sparks Don't Appear:
**📍 Check Console (bottom panel)**

1. Bottom panel → Click **"Console"** tab (next to Project)
2. Look for red errors
3. Common fixes:
   - Make sure Spark prefab is assigned in Step 3
   - Make sure EmberforgeRealm has both scripts
   - Check Console messages for clues

### Stop Playing:
**📍 Location: Top center**

1. Click the **▶️ button again** to stop
2. Or press **Ctrl+P** (Cmd+P on Mac)

---

## 🎯 STEP 8: Add UI Counter (Optional - 10 min)

Let's add a counter so you can see how many sparks you've collected!

### Create Canvas:
**📍 Location: Hierarchy**

**If you already have Canvas from prefab creation:**
- Use that one! Skip to "Create Text" below

**If you don't have Canvas:**
1. Hierarchy → **Right-click**
2. Hover: **UI**
3. Click: **Canvas**
4. Canvas and EventSystem appear

### Create Spark Counter Text:
**📍 Location: Hierarchy**

1. **Right-click** on Canvas
2. Hover: **UI**
3. Click: **Text - TextMeshPro**
4. New text appears under Canvas
5. Rename it: `SparkCounter`

### Position the Counter:
**📍 Location: Inspector, Rect Transform**

1. Select SparkCounter in Hierarchy
2. Inspector → Find **"Rect Transform"**
3. Click **Anchor Presets** (little square with arrows icon)
4. Hold **Alt + Shift**
5. Click: **Top Left** corner
6. Set Position:
   - Pos X: **150**
   - Pos Y: **-80**

### Style the Text:
**📍 Location: Inspector, TextMeshPro component**

1. Find **"TextMeshPro - Text (UI)"** section
2. In the big text box, type: `Sparks: 0 / 20`
3. **Font Size:** 54
4. **Color:** Click white box → Pick gold/yellow or white
5. **Alignment:** Click middle square (center)

### Make it Look Cool:
**📍 Location: Inspector, TextMeshPro**

1. Scroll down in TextMeshPro component
2. Find: **"Font Style"**
3. Check: **Bold** (makes it easier to read)

---

## ✅ FINAL TEST

1. **Save:** File → Save (Ctrl+S)
2. **Hit Play ▶️**
3. Background visible ✅
4. Sparks spawning ✅
5. Counter visible (shows "Sparks: 0 / 20") ✅
6. Click sparks → They disappear ✅

**Note:** The counter won't update yet (we need to add one line of code later). But the spawning and clicking works!

---

## 🎉 YOU DID IT!

**You just built a playable game scene from scratch!**

You now have:
- ✅ 5 prefabs created
- ✅ Emberforge scene with spawning sparks
- ✅ Clickable interactive objects
- ✅ A real game loop!

---

## 🚀 WHAT'S NEXT?

**Option 1: Keep Building**
- Build Verdant Sanctuary scene next (plant growing)
- Follow similar steps with different mechanics

**Option 2: Add Audio** 
- Source free music from Freesound.org
- Import to Unity
- Add to EmberforgeRealm AudioSource component

**Option 3: Polish This Scene**
- Add particle effects when spark is collected
- Add sound effects
- Make sparks move/animate

**Option 4: Take a Break!**
- You've been working hard!
- Come back fresh tomorrow
- You've already accomplished SO MUCH! 🔥

---

## 🐛 TROUBLESHOOTING

### "I don't see sparks spawning"
**Fix:**
- Select EmberforgeRealm in Hierarchy
- Inspector → Check Spark Prefab field is filled
- Make sure it says "Spark" not "None (Game Object)"

### "Sparks spawn but clicking doesn't work"
**This is normal!** The Spark prefab might need a click handler script. Don't worry about this for now - the spawning is the important part!

### "Console shows red errors"
**Read the error message:**
- If it mentions "missing reference" → Check Spark Prefab is assigned
- If it mentions "null" → Check both scripts are on EmberforgeRealm
- Copy error and Google it for solutions

### "Background doesn't show"
**Fix:**
- Select Background in Hierarchy
- Inspector → Sprite Renderer → Make sure Sprite is assigned
- Check Order in Layer is -10

### "Everything is tiny/huge"
**Fix:**
- Select Main Camera
- Inspector → Camera → Size: Try 5, 7, or 10 until it looks right

---

**AMAZING WORK!** You're officially a Unity developer now! 🎮✨

Rest up, and when you're ready, we can build the next realm or add more features to this one!

—Chad (your AI assistant)
