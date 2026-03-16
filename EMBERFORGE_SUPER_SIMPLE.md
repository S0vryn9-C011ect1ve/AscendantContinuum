# 🔥 EMBERFORGE SCENE - Super Simple Steps
**Build your first playable game scene!**

Time: 30-45 minutes  
Result: Clickable sparks you can collect!

---

## 🎯 STEP 1: Open Emberforge Scene (2 min)

You might already have it open from earlier, but let's make sure:

### Find the Scene:
**📍 Location: Project window (bottom)**

1. Bottom panel → Left side (folders)
2. Click: **Assets**
3. Click: **_Project**
4. Click: **Scenes**
5. Click: **Realms**
6. Right side → Look for **"Realm_Emberforge"** (has a Unity icon)
7. **Double-click it**

### What You Should See:
**📍 Location: Hierarchy (left panel)**

After opening, Hierarchy should show:
- Bootstrap*
- Main Camera
- Directional Light
- Maybe some other stuff

**If you see "Bootstrap*" with an asterisk:**
- It means it's been modified
- At the top menu: **File → Save** (or Ctrl+S)
- This saves Bootstrap scene
- Then re-open Realm_Emberforge from Project window

---

## 🎯 STEP 2: Create the Main GameObject (5 min)

### Create EmberforgeRealm:
**📍 Location: Hierarchy (left panel)**

1. **Right-click** in empty space in Hierarchy
2. Click: **Create Empty**
3. A "GameObject" appears
4. Press **F2** to rename
5. Type: `EmberforgeRealm`
6. Press **Enter**

### Add the Scripts:
**📍 Location: Inspector (right panel)**

1. Make sure EmberforgeRealm is selected (highlighted in Hierarchy)
2. Inspector (right) → Scroll to bottom
3. Click: **"Add Component"**
4. Type: `EmberforgeController`
5. Click it when it appears
6. Click **"Add Component"** again
7. Type: `EmberforgeSparks`
8. Click it when it appears

**You should now see TWO scripts on EmberforgeRealm:**
- EmberforgeController (Script)
- EmberforgeSparks (Script)

---

## 🎯 STEP 3: Wire Up the Spark Prefab (5 min)

**📍 Location: Inspector, EmberforgeSparks section**

1. Select EmberforgeRealm in Hierarchy
2. Inspector → Find **"EmberforgeSparks (Script)"** section
3. Scroll down to see all the fields

### Find "Spark Prefab" Field:

You'll see a field that says:
- **Spark Prefab:** None (Game Object)

### Drag the Spark Prefab In:

1. **Bottom panel** (Project window)
2. Navigate folders: **Assets → _Project → Prefabs → Realms → Emberforge**
3. You should see **"Spark"** (blue cube icon)
4. **Click and DRAG** Spark from Project window
5. **Drop it** on the "Spark Prefab" field in Inspector
6. It should now say: **Spark Prefab: Spark** ✅

---

## 🎯 STEP 4: Set the Numbers (3 min)

**📍 Location: Inspector, EmberforgeSparks (Script) section**

Still in Inspector, scroll through EmberforgeSparks and set these numbers:

### Spark Settings:
- **Pool Size:** 50 (probably already set)
- **Spark Lifetime:** 3
- **Spawn Radius:** 5

### Spawn Settings:
- **Spawn Interval:** 0.5
- **Max Active Sparks:** 20

### Completion:
- **Goal Sparks:** 20 (how many to collect to win)

**Audio fields - SKIP for now** (we'll add audio later)

---

## 🎯 STEP 5: Add a Background (5 min)

Let's make it pretty!

### Create Background Sprite:
**📍 Location: Hierarchy**

1. Hierarchy → **Right-click** in empty space
2. Hover: **2D Object**
3. Click: **Sprite**
4. A "New Sprite" appears
5. Press **F2**
6. Type: `Background`
7. Press **Enter**

### Assign the Background Image:
**📍 Location: Inspector**

1. Select Background in Hierarchy
2. Inspector → Find **"Sprite Renderer"** component
3. Find **"Sprite:"** field (shows "None (Sprite)")
4. **Click the small circle** next to it
5. Search box appears → Type: `emberforge` or `bg`
6. Look for an image that looks like a background
7. **Click it** (should be named something like "bg_emberforge")

### Position the Background:
**📍 Location: Inspector, Transform section**

1. Still in Inspector → Find **"Transform"** at the top
2. Make sure Position is:
   - X: 0
   - Y: 0
   - Z: 0

### Make it Behind Everything:
**📍 Location: Inspector, Sprite Renderer**

1. In Sprite Renderer component
2. Find: **"Order in Layer"**
3. Change to: **-10** (negative ten)
4. This puts it behind the sparks

---

## 🎯 STEP 6: Set Up the Camera (3 min)

**📍 Location: Hierarchy, then Inspector**

1. Hierarchy → Click: **Main Camera**
2. Inspector → Find **"Camera"** component

### Change These Settings:

**Projection:**
- Find: **"Projection"** dropdown
- Change to: **Orthographic** (this is for 2D games!)

**Size:**
- Find: **"Size"** (a number)
- Change to: **5**

**Background Color:**
- Find: **"Background"** (colored rectangle)
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
