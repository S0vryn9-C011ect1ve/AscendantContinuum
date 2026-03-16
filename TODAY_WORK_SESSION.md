# 🚀 TODAY'S WORK SESSION - March 14, 2026

**Goal:** Audio sourced + Firebase backend configured + Scene assembly prep complete

---

## 👤 YOUR TASKS (Audio - 2-3 hours)

### Phase 1: Account Setup (15 min)
- [ ] Go to [Freesound.org](https://freesound.org) → Create account
- [ ] Go to [Pixabay Music](https://pixabay.com/music/) → Create account
- [ ] Verify email confirmations

### Phase 2: Download Emberforge Audio (45 min)
**Music (1 track needed):**
- [ ] Search Freesound.org: "ambient fire meditation calm"
- [ ] Preview 5-10 tracks
- [ ] Download best one (look for 3-5 minute loop, CC0 license)
- [ ] Rename: `emberforge_ambient.ogg`

**SFX (5 sounds needed):**
- [ ] "spark collect" (whoosh, small chime) → `spark_collect.wav`
- [ ] "spark spawn" (soft pop, magic appear) → `spark_spawn.wav`
- [ ] "ritual complete" (success chime, victory) → `ritual_complete.wav`
- [ ] "button click" (UI tap) → `ui_click.wav`
- [ ] "menu open" (soft whoosh) → `menu_open.wav`

### Phase 3: Import to Unity (30 min)
- [ ] Open Unity project
- [ ] Drag music file to: `Assets/_Project/Audio/Music/Emberforge/`
- [ ] Drag SFX files to: `Assets/_Project/Audio/SFX/`
- [ ] Select each file in Project window
- [ ] Inspector settings:
  - **Music:** Load Type = Streaming, Compression = Vorbis
  - **SFX:** Load Type = Decompress On Load, Compression = ADPCM
- [ ] Test: Create empty GameObject, add AudioSource, drag clip, Play

### Phase 4: Quick Test in Emberforge Scene (30 min)
- [ ] Open `Realm_Emberforge.unity`
- [ ] Find `EmberforgeController` GameObject (or create if missing)
- [ ] In Inspector, find EmberforgeController script
- [ ] Look for `AudioClip` fields (if they exist)
- [ ] Drag audio files into fields
- [ ] Hit Play → Verify music starts

**If audio fields don't exist yet:** That's OK! We'll wire them up during scene assembly.

---

## 🤖 AGENT TASKS (Automated)

### Agent 1: Firebase Configuration Setup
- Verify Firebase project requirements
- Check if google-services.json exists
- Create Firebase initialization script if missing
- Prepare Cloud Functions structure
- Generate deployment checklist

### Agent 2: Scene Assembly Verification
- Check all 5 realm scenes exist
- Verify all controller scripts exist and compile
- List missing components
- Create pre-flight checklist

### Agent 3: Prefab Audit
- Check existing prefabs in each realm folder
- Document what needs to be created
- Verify sprite assets are linked correctly
- Generate prefab creation priority list

---

## ✅ END OF SESSION GOALS

**You (Audio):**
- ✅ Freesound.org & Pixabay accounts created
- ✅ 1 music track + 5 SFX downloaded
- ✅ Audio imported to Unity with correct settings
- ✅ Audio files tested (can hear them play)

**Agents (Backend/Prep):**
- ✅ Firebase setup checklist ready
- ✅ All scene/script dependencies verified
- ✅ Prefab creation priority list complete
- ✅ Any missing scripts created

---

## 🎯 TOMORROW'S PREVIEW

**Day 2 Tasks:**
- Continue audio sourcing (Verdant Sanctuary = 1 track + 5 SFX)
- Begin Emberforge scene assembly (following SCENE_ASSEMBLY_GUIDE.md)
- Create Spark.prefab
- Test spark collection mechanic

---

## 📊 PROGRESS TRACKING

Session Start: [Your time here]
Session End: [Your time here]
Total Time: [Calculate]

**Blockers:** [Note any issues]
**Wins:** [Celebrate successes!]
**Tomorrow's Priority:** [Top 3 tasks]

---

**Let's do this!** 🔥
