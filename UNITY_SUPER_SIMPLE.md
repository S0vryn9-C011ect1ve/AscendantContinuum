# 🎮 UNITY FIRST STEPS - Super Simple Guide
**For complete Unity beginners - exact clicks, exact locations**

---

## 🎯 PREFAB #1: ConstellationLine (5 minutes)

### Where you are now:
You should see Unity with several windows. Don't panic! We only need 3 windows:
- **Left side:** Hierarchy (list of objects)
- **Middle:** Scene view (3D/2D workspace)
- **Right side:** Inspector (properties panel)
- **Bottom:** Project (your files)

---

### Step 1: Create Empty GameObject
**📍 Location: Hierarchy window (left side, lists objects)**

1. Find the word "Hierarchy" at top of left panel
2. **Right-click** anywhere in the empty space below it
3. You'll see a menu pop up
4. Click: **"Create Empty"**
5. A new item appears called "GameObject"

### Step 2: Rename It
**📍 Location: Still in Hierarchy**

1. The new GameObject should be selected (highlighted blue)
2. Press **F2** key on keyboard (or right-click → Rename)
3. Type: `ConstellationLine`
4. Press **Enter**

### Step 3: Add Line Renderer Component
**📍 Location: Inspector window (right side)**

1. Look at the right side of Unity
2. Find the word "Inspector" at the top
3. With ConstellationLine still selected, scroll down in Inspector
4. At the bottom, click the button: **"Add Component"**
5. A search box appears
6. Type: `Line Renderer`
7. Click the option that says "Line Renderer" when it appears

### Step 4: Configure Line Renderer
**📍 Location: Inspector window, Line Renderer section**

You should now see "Line Renderer" in the Inspector. Let's change some numbers:

**Positions:**
- Find "Positions" (has a little arrow you can collapse/expand)
- Next to "Size", it says a number (probably 2)
- Leave it at **2**

**Width:**
- Scroll down to find "Width"
- You'll see a graph/curve - ignore that for now
- Look for a number field labeled "Width" 
- Change it to: **0.05**
- (Click the number, delete it, type 0.05, press Enter)

**Color:**
- Find "Color Gradient" (it's a colored bar)
- **Click the colored bar**
- A window pops up called "Gradient Editor"
- On the left side, find "Color" dropdown
- Click the white rectangle next to it
- In the color picker:
  - In the Hex field at bottom, type: **00FFFF**
  - Press Enter
- Close the color picker (X in corner)
- Click anywhere outside the Gradient Editor to close it

**Material:**
- Find the word "Materials" 
- Below it, there's "Element 0" with a circle next to it
- **Click the small circle** (not the words)
- A window pops up showing materials
- In the search box at top, type: **Default**
- Click anything that says "Default-Line" or "Sprites-Default"
- If nothing appears, just click the first option you see

### Step 5: Save as Prefab
**📍 Location: Project window (bottom panel) + Hierarchy**

1. Look at the **bottom panel** (Project window)
2. Find the folder panel on the left side showing folders
3. Click: **Assets**
4. Double-click: **_Project**
5. Double-click: **Prefabs**
6. Double-click: **Realms**
7. Double-click: **Shared**

Now you're in the Shared folder. You should see the right side is mostly empty.

8. Go back to **Hierarchy** (left panel)
9. **Click and DRAG** ConstellationLine
10. **Drag it down** to the Project window (bottom, right side where it's empty)
11. **Let go of mouse** 
12. You should see a blue cube icon appear called "ConstellationLine"

### Step 6: Delete from Hierarchy
**📍 Location: Hierarchy**

1. Go back to Hierarchy (left panel)
2. **Right-click** on ConstellationLine
3. Click: **Delete**
4. It's gone - that's OK! The prefab is saved in Project window

### ✅ DONE! You created your first prefab! 🎉

**To verify it worked:**
- Bottom panel (Project) → You should still be in Realms/Shared folder
- You should see a blue cube icon: "ConstellationLine"
- Click it once → Right side shows a preview

---

## 🎯 PREFAB #2: UI Button (10 minutes)

### Step 1: Create Canvas
**📍 Location: Hierarchy window**

1. **Right-click** in Hierarchy (left panel, empty space)
2. Hover over "UI" (a submenu appears)
3. Click: **Canvas**

Two things appear:
- "Canvas" (with a little arrow you can expand)
- "EventSystem"

**Both are needed! Don't delete them!**

If prompted "Import TMP Essentials":
- A window pops up
- Click: **"Import TMP Essentials"**
- Wait for it to finish

### Step 2: Create Button
**📍 Location: Hierarchy**

1. **Right-click** on the word "Canvas" in Hierarchy
2. Hover over "UI"
3. Click: **"Button - TextMeshPro"**

If ANOTHER prompt appears "Import TMP Essentials":
- Click Import again
- Wait for it to finish

You should now see in Hierarchy:
```
Canvas
  └─ Button
EventSystem
```

### Step 3: Rename Button
**📍 Location: Hierarchy**

1. Click on "Button" (the one under Canvas)
2. Press **F2**
3. Just type: `Button` (or leave it as is)
4. Press Enter

### Step 4: Configure Button Size
**📍 Location: Inspector, with Button selected**

1. Make sure Button is selected (click it in Hierarchy)
2. Look at Inspector (right side)
3. Find "Rect Transform" section at the top
4. Find "Width:" - change to **300**
5. Find "Height:" - change to **80**

### Step 5: Configure Button Text
**📍 Location: Hierarchy, then Inspector**

1. In Hierarchy, click the **little arrow** next to Button to expand it
2. You'll see "Text (TMP)" underneath
3. **Click "Text (TMP)"**
4. Look at Inspector (right side)
5. Find "TextMeshPro - Text (UI)" section
6. Find the big text box at top that says "New Text"
7. **Delete all the text**
8. Type: `Button`
9. Press Enter

**Change text size:**
10. Find "Font Size" (a number)
11. Change to: **36**

**Center the text:**
12. Find "Alignment" (shows 9 little squares in a grid)
13. Click the **middle square** (center alignment)

### Step 6: Save Button as Prefab
**📍 Location: Project window + Hierarchy**

1. Bottom panel (Project window)
2. Navigate to folders on left:
   - Click **Assets**
   - Click **_Project**
   - Click **Prefabs**
   - Click **UI**

3. Go to Hierarchy (left panel)
4. Find "Button" (under Canvas)
5. **Click and DRAG** Button
6. Drag down to Project window (bottom right, the UI folder)
7. **Let go** - a blue cube "Button" appears

### Step 7: Don't Delete Yet!
**Leave Canvas and Button in Hierarchy - we need it for the next prefab!**

### ✅ DONE! Button prefab created! 🎉

---

## 🎯 PREFAB #3: LightBeam (12 minutes)

### Step 1: Create Empty GameObject
**📍 Location: Hierarchy**

1. **Right-click** in empty space in Hierarchy
2. Click: **"Create Empty"**
3. Press **F2** to rename
4. Type: `LightBeam`
5. Press Enter

### Step 2: Add Line Renderer
**📍 Location: Inspector**

1. With LightBeam selected
2. Inspector (right side)
3. Scroll to bottom
4. Click: **"Add Component"**
5. Type: `Line Renderer`
6. Click it when it appears

### Step 3: Configure Width
**📍 Location: Inspector, Line Renderer section**

Find "Width" - this is tricky! Here's how:

1. Find "Width" text
2. You'll see a curve/graph - **click on the curve/graph itself**
3. A "Width over Lifetime" window might pop up
4. If not, look for two number fields:
   - First one (left side/start): Type **0.2**
   - Second one (right side/end): Type **0.1**

(If you only see one Width number, just set it to **0.15** and that's fine!)

### Step 4: Set Color to Gold
**📍 Location: Inspector, Line Renderer**

1. Find "Color Gradient" (colored bar)
2. **Click the colored bar**
3. In the Gradient Editor that appears:
   - Find the left color marker (little square at bottom left)
   - Click it
   - In Color picker, find "Hex" field
   - Type: **FFD700**
   - Press Enter
4. Close the gradient editor

### Step 5: Set Material
**📍 Location: Inspector, Line Renderer**

1. Find "Materials" in Line Renderer section
2. Find "Element 0" with a circle next to it
3. **Click the circle**
4. Search: `Sprites` or `Default`
5. Click: **"Sprites-Default"** (or any Default material)

### Step 6: Save as Prefab
**📍 Location: Project + Hierarchy**

1. Project window (bottom)
2. Navigate to: Assets → _Project → Prefabs → Realms → **DawnCitadel**
3. Hierarchy: **Click and drag** LightBeam
4. Drop it in Project window (DawnCitadel folder)
5. Blue cube appears!

### Step 7: Delete from Hierarchy
**📍 Location: Hierarchy**

1. **Right-click** LightBeam
2. Click: **Delete**

### ✅ DONE! LightBeam created! 🎉

---

## 🎯 PREFAB #4: WishText (10 minutes)

We're using the Canvas you created earlier!

### Step 1: Create Text Object
**📍 Location: Hierarchy**

1. Find "Canvas" in Hierarchy (should still be there)
2. **Right-click** on Canvas
3. Hover: **UI**
4. Click: **"Text - TextMeshPro"**

### Step 2: Rename It
**📍 Location: Hierarchy**

1. New text object appears under Canvas
2. It's called "Text (TMP)" or similar
3. Press **F2**
4. Type: `WishText`
5. Press Enter

### Step 3: Configure Size
**📍 Location: Inspector**

1. With WishText selected
2. Inspector → Find "Rect Transform"
3. **Width:** 300
4. **Height:** 100

### Step 4: Configure Text Content
**📍 Location: Inspector, TextMeshPro section**

1. Find "TextMeshPro - Text (UI)" section
2. Big text box at top
3. Delete existing text
4. Type: `Make a wish...`

**Other settings:**
5. **Font Size:** 24
6. **Alignment:** Click middle square (center)
7. **Color:** 
   - Click the white rectangle next to "Vertex Color"
   - In color picker, make sure it's white (or type FFFFFF in Hex)
   - Close color picker

### Step 5: Save as Prefab
**📍 Location: Project + Hierarchy**

1. Project window → Navigate to: Assets → _Project → Prefabs → Realms → **LanternAscension**
2. Hierarchy → **Drag** WishText to Project window
3. Blue cube appears!

### Step 6: Delete from Hierarchy
**📍 Location: Hierarchy**

1. **Right-click** WishText
2. **Delete**

### ✅ DONE! WishText created! 🎉

---

## 🎯 PREFAB #5: Bloom (8 minutes)

This one is easiest - we're copying an existing prefab!

### Step 1: Find MagicalPlant Prefab
**📍 Location: Project window (bottom)**

1. Project window → Left side folders
2. Click: **Assets**
3. Click: **_Project**
4. Click: **Prefabs**
5. Click: **Realms**
6. Click: **Verdant**

In the right side, you should see blue cubes. One is called "MagicalPlant"

### Step 2: Duplicate It
**📍 Location: Project window**

1. **Right-click** on "MagicalPlant"
2. Click: **Duplicate**
3. A copy appears called "MagicalPlant 1" or similar

### Step 3: Rename the Copy
**📍 Location: Project window**

1. Click the duplicate once
2. Press **F2**
3. Type: `Bloom`
4. Press Enter

### Step 4: Change the Sprite
**📍 Location: Project window, then Inspector**

1. **Double-click** the Bloom prefab (opens Prefab Edit Mode)
2. Look at the top of the screen - you should see "< Prefabs" button
3. Look at Inspector (right side)
4. Find "Sprite Renderer" component
5. Find "Sprite" field (has a tiny picture next to it)
6. Click the **small circle** next to the sprite picture
7. A "Select Sprite" window opens
8. In search box, type: `bloom` or `plant`
9. Look for a sprite image that looks like a bloomed plant
10. **Click it**
11. Hit the **"< Prefabs"** button at top to exit Prefab Mode

### ✅ DONE! All 5 prefabs created! 🎉🎉🎉

---

## 🏆 CHECKPOINT - You Did It!

You should now have created:
1. ✅ ConstellationLine (in Realms/Shared/)
2. ✅ Button (in UI/)
3. ✅ LightBeam (in Realms/DawnCitadel/)
4. ✅ WishText (in Realms/LanternAscension/)
5. ✅ Bloom (in Realms/Verdant/)

**To verify:**
- Go to Project window
- Navigate to each folder
- You should see blue cube icons with those names

---

## 🔥 NEXT: Open Emberforge Scene

### Step 1: Find the Scene File
**📍 Location: Project window**

1. Project window (bottom)
2. Left folders: Assets → _Project → **Scenes** → **Realms**
3. Right side: Look for files with a Unity icon
4. Find: **"Realm_Emberforge"** (might have .unity at the end)

### Step 2: Open the Scene
**📍 Location: Project window**

1. **Double-click** Realm_Emberforge
2. The middle panel (Scene view) might change
3. Hierarchy (left) shows what's in the scene

**What you should see in Hierarchy:**
- Main Camera
- Directional Light
- Maybe some other objects

### ✅ Scene is open!

---

## 🎮 BUILDING THE SCENE (Simplified!)

I'll create an even simpler step-by-step for scene building. Take a break first!

**When you're ready for the next part, let me know and I'll give you the ultra-simple scene building steps.**

---

**Great work so far!** 🎉 You've been clicking and dragging like a pro. Take a 5-minute break, then we'll tackle the scene assembly!
