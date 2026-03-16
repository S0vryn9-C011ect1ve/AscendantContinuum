# 6-Week Zero-Budget Launch Roadmap
**Ascendant Continuum Game**  
**Start Date:** March 14, 2026  
**Launch Target:** April 25-30, 2026  
**Budget:** $0 (100% free resources)

---

## 🎯 WEEKLY BREAKDOWN

### WEEK 1: Audio Production (March 14-20)
**Goal:** 100% audio complete (6 music tracks + 50 SFX)  
**Time:** 10-15 hours total (~2 hours/day)

#### Monday March 14 - Setup & Assessment
- [x] Create Freesound.org account
- [x] Download Audacity (free audio editor)
- [ ] Test download 1 sound and import to Unity
- [ ] Document current project status (what art exists)
- [ ] Read ZERO_BUDGET_AUDIO_GUIDE.md

#### Tuesday March 15 - Music Day
- [ ] Search Pixabay Music for 6 tracks
  - Emberforge: meditation fire, ambient warmth
  - Verdant: nature meditation, forest ambient
  - Echo Fields: space meditation, cosmic ambient
  - Dawn Citadel: light meditation, radiant calm
  - Lantern Ascension: void meditation, infinite calm
  - Main Menu: welcome meditation, gentle intro
- [ ] Download 3 candidates per realm (18 total test tracks)
- [ ] Listen and pick best 6
- [ ] Export as OGG (128 kbps) in Audacity
- [ ] Import to `Assets/_Project/Audio/Music/`
- [ ] Test playback in Unity

**Deliverable:** 6 music tracks ready (MainMenu.ogg, Emberforge.ogg, etc.)

#### Wednesday March 16 - SFX Part 1 (Emberforge + Verdant)
- [ ] Freesound.org downloads:
  - Emberforge (10): spark crackle, collect, whoosh, complete, ambient fire, double-tap, swipe, spawn, sigil reveal, rumble
  - Verdant (10): plant growing, watering, bloom, leaf rustle, birds, tap plant, seed plant, growth transition, garden complete, wind
- [ ] Trim silence in Audacity
- [ ] Normalize to -3 dB
- [ ] Add 0.05s fade in/out
- [ ] Export as WAV (22050 Hz, mono)
- [ ] Import to `Assets/_Project/Audio/SFX/Emberforge/` and `/Verdant/`

**Deliverable:** 20 SFX files ready

#### Thursday March 17 - SFX Part 2 (Echo Fields + Dawn Citadel)
- [ ] Freesound.org downloads:
  - Echo Fields (10): star twinkle, constellation trace, pattern complete, cosmic wind, star connect, echo, ambient space, touch star, error, hum
  - Dawn Citadel (10): light beam, prism refraction, mirror rotate, light connect, puzzle complete, ambient hum, prism touch, light spawn, reflection error, dawn ambience
- [ ] Process in Audacity (same as Wednesday)
- [ ] Import to Unity

**Deliverable:** 20 more SFX files

#### Friday March 18 - SFX Part 3 (Lantern + UI)
- [ ] Freesound.org downloads:
  - Lantern Ascension (10): lantern release, wish write, lantern glow, void ambience, drift, meditation breath, wish complete, lantern appear, time capsule lock, void wind
  - UI (10): button tap, menu open, menu close, achievement unlock, daily challenge notification, sigil collect, transition whoosh, error, success fanfare, haptic feedback
- [ ] Process and import
- [ ] Test all sounds play in Unity

**Deliverable:** Final 20 SFX + all 50 sounds functional

#### Weekend March 19-20 - Audio Integration
- [ ] Link AudioClips to AudioManager.cs
- [ ] Test PlayMusic() for each realm
- [ ] Test PlaySFX() for sample sounds
- [ ] Adjust volume levels (music quieter than SFX)
- [ ] Verify spatial 3D audio works
- [ ] Create CREDITS.md for attribution

**Deliverable:** ✅ Audio 100% complete and integrated

**Week 1 Success Criteria:**
- 6 music tracks looping smoothly
- 50 SFX sounds triggering correctly
- Total audio size <50 MB
- Attribution documented

---

### WEEK 2: Scene Assembly (March 21-27)
**Goal:** All 5 realm scenes functional with GameObjects  
**Time:** 15-20 hours total

#### Monday March 21 - Emberforge Scene
- [ ] Open `Realm_Emberforge.unity`
- [ ] Add realm background sprite (bg_emberforge.png)
- [ ] Create Spark prefab instances (20 total)
- [ ] Position sparks randomly across screen
- [ ] Link EmberforgeController.cs to GameObject
- [ ] Add AudioSource for realm music
- [ ] Test: Press Play, tap sparks, collect them

**Deliverable:** Emberforge playable start to finish

#### Tuesday March 22 - Verdant Scene
- [ ] Open `Realm_Verdant.unity`
- [ ] Add background (bg_verdant.png)
- [ ] Create Plant prefab (4 growth stages)
- [ ] Position 10 plant spawn points
- [ ] Link VerdantController.cs
- [ ] Test: Press Play, tap to nurture plants

**Deliverable:** Verdant Sanctuary playable

#### Wednesday March 23 - Echo Fields Scene
- [ ] Open `Realm_EchoFields.unity`
- [ ] Add background (bg_echofields.png)
- [ ] Create Star prefab
- [ ] Generate constellation patterns (5-9 stars)
- [ ] Link ConstellationTracer.cs
- [ ] Link EchoFieldsController.cs
- [ ] Test: Press Play, trace constellations

**Deliverable:** Echo Fields playable

#### Thursday March 24 - Dawn Citadel Scene
- [ ] Open `Realm_DawnCitadel.unity`
- [ ] Add background (bg_dawncitadel.png)
- [ ] Create Prism prefab
- [ ] Create Light Beam prefab
- [ ] Position prisms for puzzle
- [ ] Link DawnCitadelController.cs
- [ ] Test: Press Play, rotate prisms

**Deliverable:** Dawn Citadel playable

#### Friday March 25 - Lantern Ascension Scene
- [ ] Open `Realm_LanternAscension.unity`
- [ ] Add background (bg_lanternascension.jpg)
- [ ] Create Lantern prefab
- [ ] Create Wish Wall UI
- [ ] Link LanternAscensionController.cs
- [ ] Test: Press Play, write wish, release lantern

**Deliverable:** Lantern Ascension playable

#### Weekend March 26-27 - UI Assembly
- [ ] Open `MainMenu.unity`
- [ ] Build main menu UI (play button, settings, achievements)
- [ ] Link MainMenuManager.cs
- [ ] Create settings panel (audio sliders, accessibility toggles)
- [ ] Link SettingsMenuManager.cs
- [ ] Test scene transitions (menu → realm → menu)

**Deliverable:** ✅ All scenes playable with basic UI

**Week 2 Success Criteria:**
- All 5 realms functional
- Scene transitions work
- Main menu navigable
- No critical errors

---

### WEEK 3: Prefabs & Interactions (March 28 - April 3)
**Goal:** Polish interactions, create missing prefabs  
**Time:** 15-20 hours

#### Monday March 28 - Prefab Creation
- [ ] Create missing prefabs:
  - Plant.prefab (4 growth stage sprites)
  - Star.prefab (for constellations)
  - Prism.prefab (rotatable)
  - LightBeam.prefab (line renderer)
  - Lantern.prefab (animated)
  - WishCard.prefab (UI element)
- [ ] Save to `Assets/_Project/Prefabs/Realms/[RealmName]/`

**Deliverable:** All realm prefabs created

#### Tuesday March 29 - Touch Input Polish
- [ ] Link TouchInputManager.cs to scenes
- [ ] Test tap detection on all interactive objects
- [ ] Adjust touch target sizes (accessibility)
- [ ] Add haptic feedback on taps
- [ ] Test swipe gestures (Emberforge)
- [ ] Test pinch (zoom if needed)

**Deliverable:** Touch input responsive

#### Wednesday March 30 - Particle Effects
- [ ] Link ParticleManager.cs
- [ ] Add particle effects:
  - Spark collection burst
  - Plant growth sparkle
  - Star connection trail
  - Light beam glow
  - Lantern ascension trail
- [ ] Test reduced motion alternatives

**Deliverable:** Visual feedback polished

#### Thursday March 31 - Progression Integration
- [ ] Link ProgressionManager.cs
- [ ] Test realm unlock gates (5 completions → next realm)
- [ ] Test sigil collection
- [ ] Test daily challenge appears
- [ ] Verify save/load works

**Deliverable:** Progression functional

#### Friday April 1 - HUD & Notifications
- [ ] Link HUDManager.cs
- [ ] Display spark count (top right)
- [ ] Display sigil notification on collect
- [ ] Display daily challenge banner
- [ ] Test achievement popups

**Deliverable:** HUD complete

#### Weekend April 2-3 - Bug Bash
- [ ] Playtest all 5 realms
- [ ] Fix any crashes
- [ ] Fix any soft-locks (can't progress)
- [ ] Test on Unity Device Simulator (multiple resolutions)
- [ ] Document known issues

**Deliverable:** ✅ Game fully playable end-to-end

**Week 3 Success Criteria:**
- All prefabs functional
- Touch input polished
- Progression saves correctly
- Playable on multiple screen sizes

---

### WEEK 4: Tutorial & Firebase (April 4-10)
**Goal:** Onboarding complete, backend deployed  
**Time:** 12-15 hours

#### Monday April 4 - Tutorial Script
- [ ] Open `Onboarding.unity`
- [ ] Create 3-step tutorial:
  - Step 1: "Welcome to Emberforge. Tap a spark."
  - Step 2: "Collect 10 sparks to create a sigil."
  - Step 3: "Complete the ritual to unlock your journey."
- [ ] Add tutorial UI overlays (arrows, text boxes)
- [ ] Add skip button (for returning players)
- [ ] Test tutorial flow

**Deliverable:** Tutorial playable

#### Tuesday April 5 - Firebase Setup
- [ ] Sign up for Firebase (free Spark plan)
- [ ] Create new project "ascendant-continuum"
- [ ] Add web app to Firebase project
- [ ] Download `google-services.json` (Android)
- [ ] Download `GoogleService-Info.plist` (iOS)
- [ ] Place in Unity project (Assets/Firebase/)

**Deliverable:** Firebase project created

#### Wednesday April 6 - Firebase Integration
- [ ] Link FirebaseManager.cs
- [ ] Enable Firebase Authentication (Anonymous)
- [ ] Enable Firestore Database
- [ ] Test anonymous sign-in in Unity
- [ ] Test save to Firestore (player profile)
- [ ] Verify offline persistence works

**Deliverable:** Firebase connected

#### Thursday April 7 - Cloud Functions Deploy
- [ ] Install Node.js (if not installed)
- [ ] Navigate to `firebase/functions/`
- [ ] Run: `npm install`
- [ ] Run: `firebase deploy --only functions`
- [ ] Test Cloud Functions:
  - validateSigilCraft
  - aggregateNPCMemory
  - dailyChallengeGenerator

**Deliverable:** Backend deployed

#### Friday April 8 - Security Rules
- [ ] Deploy Firestore security rules
- [ ] Deploy Storage security rules
- [ ] Test: Player can only read/write their own data
- [ ] Test: Anti-cheat validation works

**Deliverable:** Security rules active

#### Weekend April 9-10 - Social Features Test
- [ ] Test Wish Wall (create wish, see in Firestore)
- [ ] Test Time Capsules (send capsule, verify timestamp)
- [ ] Test NPC Collective Memory (send emotion, verify aggregation)
- [ ] Test daily challenge seed (same for all players)

**Deliverable:** ✅ Backend 100% functional

**Week 4 Success Criteria:**
- Tutorial guides new players
- Firebase fully integrated
- Cloud Functions deployed
- Social features work

---

### WEEK 5: Testing & Optimization (April 11-17)
**Goal:** 60fps on low-end devices, accessibility verified  
**Time:** 15-20 hours

#### Monday April 11 - Performance Baseline
- [ ] Unity Profiler: Record baseline (CPU, GPU, memory)
- [ ] Target: 60fps on Unity Device Simulator (Galaxy S8 profile)
- [ ] Identify bottlenecks (likely particles or draw calls)

**Deliverable:** Performance report

#### Tuesday April 12 - Optimization Pass 1
- [ ] Reduce particle count if needed (from 30 → 15)
- [ ] Implement LOD for particles (high/medium/low quality)
- [ ] Batch sprite draw calls (use sprite atlas)
- [ ] Compress textures (reduce from 4K → 2K if needed)
- [ ] Test: Re-run profiler, verify 60fps

**Deliverable:** Performance improved

#### Wednesday April 13 - Build Size Optimization
- [ ] Check build size (WebGL)
- [ ] Target: <100 MB
- [ ] Compress audio (OGG/Vorbis, 96 kbps for music)
- [ ] Compress textures (ASTC or ETC2)
- [ ] Strip unused scripts
- [ ] Test build size again

**Deliverable:** Build under 100 MB

#### Thursday April 14 - Accessibility Verification
- [ ] Test all 5 colorblind modes
- [ ] Verify secrets appear in each mode
- [ ] Test reduced motion (particles become simple shapes)
- [ ] Test high contrast mode
- [ ] Test text scaling (0.8x → 2.0x)
- [ ] Test haptic feedback (if device supports)
- [ ] Test audio-only mode (screen reader simulation)

**Deliverable:** Accessibility audit complete

#### Friday April 15 - Device Testing
- [ ] Test on your phone (real device)
- [ ] Test on Unity Device Simulator (5 profiles)
- [ ] Test on BrowserStack free tier (100 min)
- [ ] Test different screen sizes (phone, tablet)
- [ ] Document device compatibility

**Deliverable:** Compatibility report

#### Weekend April 16-17 - Bug Fixing
- [ ] Fix all critical bugs (crashes, soft-locks)
- [ ] Fix high-priority bugs (visual glitches, audio issues)
- [ ] Add quality presets (low/medium/high)
- [ ] Test save/load extensively
- [ ] Final playthrough (all 5 realms)

**Deliverable:** ✅ Game stable and optimized

**Week 5 Success Criteria:**
- 60fps on 2019 mid-range devices
- Build size <100 MB
- Accessibility fully functional
- No critical bugs

---

### WEEK 6: Polish & Launch Prep (April 18-24)
**Goal:** Marketing assets, store listings, launch  
**Time:** 10-15 hours

#### Monday April 18 - WebGL Build
- [ ] Build WebGL (File → Build Settings → WebGL → Build)
- [ ] Test locally (Unity's built-in server)
- [ ] Deploy to Firebase Hosting: `firebase deploy --only hosting`
- [ ] Test at ascendant-continuum.web.app
- [ ] Verify works on Chrome, Firefox, Safari

**Deliverable:** WebGL live

#### Tuesday April 19 - Screenshots & Video
- [ ] Capture 5 screenshots (one per realm)
- [ ] Use Unity Recorder (Window → General → Recorder)
- [ ] Resolution: 1920x1080 minimum
- [ ] Capture 30-second gameplay video
- [ ] Use OBS Studio (free screen recorder)
- [ ] Edit video in DaVinci Resolve (free)

**Deliverable:** Marketing assets ready

#### Wednesday April 20 - Itch.io Setup
- [ ] Create itch.io account (free)
- [ ] Create new game page
- [ ] Upload WebGL build
- [ ] Write game description (use PROJECT_SUMMARY.md)
- [ ] Add screenshots
- [ ] Embed gameplay video
- [ ] Set pricing: Pay-what-you-want ($0 minimum, $4.99 suggested)
- [ ] Add tags: meditation, accessibility, indie, experimental, web

**Deliverable:** Itch.io page live

#### Thursday April 21 - Landing Page
- [ ] Create landing page on Firebase Hosting
- [ ] Content:
  - Game logo
  - 1-paragraph description
  - "Play Now" button → itch.io
  - Screenshots gallery
  - Features list (10 industry firsts!)
  - Accessibility highlights
  - Credits
- [ ] Use free Canva template or plain HTML
- [ ] Deploy to ascendant-continuum.web.app

**Deliverable:** Landing page live

#### Friday April 22 - Social Launch Prep
- [ ] Write Reddit launch post (r/WebGames, r/IndieGaming, r/incremental_games)
- [ ] Create TikTok dev diary (30-60 sec: "I built a meditation game...")
- [ ] Write Twitter/X launch thread (accessibility angle)
- [ ] Email pitch to TouchArcade, Pocket Gamer (template below)
- [ ] Post in accessibility communities (r/disabledgamers)

**Deliverable:** Marketing content ready

#### Weekend April 23-24 - Soft Launch
- [ ] Saturday: Post to Reddit (r/WebGames, r/playmygame)
- [ ] Sunday: Post to TikTok, Twitter/X
- [ ] Monitor feedback
- [ ] Fix any critical bugs immediately
- [ ] Thank early players

**Deliverable:** Soft launch complete, 100-500 players

**Week 6 Success Criteria:**
- WebGL build live and stable
- Itch.io page published
- 100+ downloads first weekend
- Positive early feedback

---

## 🚀 LAUNCH WEEK (April 25-30)

### Monday April 25 - Press Outreach
- [ ] Email press contacts (template in guide)
- [ ] Post to r/AndroidGaming, r/iosgaming (if mobile ready)
- [ ] Submit to IndieDB, GameJolt
- [ ] Contact AbleGamers, SpecialEffect (accessibility angle)

### Tuesday April 26 - Community Launch
- [ ] Reddit launch post (r/incremental_games)
- [ ] Mastodon post (accessibility community)
- [ ] Facebook groups (indie games, meditation)
- [ ] Discord servers (game dev, accessibility)

### Wednesday April 27 - Monitoring Day
- [ ] Check itch.io downloads/revenue
- [ ] Monitor Reddit comments
- [ ] Fix any urgent bugs
- [ ] Thank players publicly

### Thursday April 28 - Content Day
- [ ] Post dev diary on TikTok (behind-the-scenes)
- [ ] Write blog post on dev journey
- [ ] Share on LinkedIn (professional angle)

### Friday April 29 - Expansion Planning
- [ ] Analyze analytics (DAU, retention, revenue)
- [ ] Prioritize bug fixes
- [ ] Plan Season 2 features
- [ ] Email early players for testimonials

### Weekend April 30 - Celebrate! 🎉
- [ ] Reflect on launch
- [ ] Thank community
- [ ] Plan next steps (Google Play? Apple App Store?)
- [ ] **You shipped a game!**

---

## 📊 SUCCESS METRICS

### Week 1 Goals:
- ✅ Music: 6 tracks sourced and imported
- ✅ SFX: 50 sounds sourced and imported
- ✅ Total audio <50 MB
- ✅ All sounds play correctly in Unity

### Week 2 Goals:
- ✅ All 5 realms have backgrounds + GameObjects
- ✅ Each realm playable start-to-finish
- ✅ Scene transitions work
- ✅ No critical errors in Console

### Week 3 Goals:
- ✅ All prefabs created and functional
- ✅ Touch input responsive
- ✅ Progression saves/loads correctly
- ✅ Particle effects added

### Week 4 Goals:
- ✅ Tutorial complete (3 steps)
- ✅ Firebase deployed and connected
- ✅ Cloud Functions operational
- ✅ Social features functional

### Week 5 Goals:
- ✅ 60fps on mid-range 2019 devices
- ✅ Build size <100 MB
- ✅ Accessibility verified (10 categories)
- ✅ No critical bugs

### Week 6 Goals:
- ✅ WebGL build deployed
- ✅ Itch.io page published
- ✅ 100+ downloads first weekend
- ✅ Positive feedback

### Launch Week Goals:
- ✅ 500+ total downloads
- ✅ $50-200 revenue (pay-what-you-want)
- ✅ 5+ player testimonials
- ✅ Featured on 1-2 indie game sites

---

## 🎯 DAILY TIME COMMITMENT

**Weeks 1-5:** 2-3 hours/day (weekdays) + 4-6 hours weekends = **15-20 hours/week**  
**Week 6:** 1-2 hours/day (lighter, mostly marketing) = **10-15 hours**

**Total:** ~90-110 hours over 6 weeks

---

## 💰 ZERO-BUDGET RESOURCES

**All tools used:** 100% free
- Unity Personal (free)
- Audacity (free)
- Freesound.org (free)
- Pixabay Music (free)
- Firebase Spark plan (free)
- Itch.io (free, 10% revenue share)
- OBS Studio (free)
- DaVinci Resolve (free)
- Canva (free tier)

**Optional paid later:**
- Google Play: $25 one-time (reinvest from revenue)
- Apple App Store: $99/year (skip initially)

---

## 🆘 HELP & SUPPORT

**Stuck? Resources:**
- Unity Learn (free tutorials)
- r/Unity3D (free community help)
- r/gamedev (advice)
- Unity Forums (official support)
- YouTube: Brackeys, CodeMonkey, Jason Weimann

**Weekly Check-Ins:**
- End of each week, assess progress
- Adjust timeline if needed (but stay focused!)
- Celebrate small wins

---

## 🏁 LAUNCH CHECKLIST

Before going live, verify:
- [ ] All 5 realms playable without errors
- [ ] Tutorial guides new players correctly
- [ ] Save/load works (test by closing game and reopening)
- [ ] Audio plays on all platforms
- [ ] Accessibility features functional (test at least 3 modes)
- [ ] Firebase connected (test offline → online sync)
- [ ] Build size <100 MB
- [ ] 60fps on test device
- [ ] Credits include audio attribution
- [ ] Privacy policy added (Firebase requirement)
- [ ] GDPR compliance (EU players)

---

**You can do this.** The hardest work is done - you just need to assemble the pieces. Stay focused, work 2-3 hours daily, and you'll launch in 6 weeks. 🚀

**Questions? Blockers?** Tell me immediately and I'll help you solve them. Don't struggle alone - this is a team effort!
