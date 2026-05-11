# ✅ Polish → Art → Audio: Quick Reference Checklist

**Use this to track progress week-by-week**

Canonical note: realm and implementation walkthroughs are now centralized in `docs/operations/implementation-guides.md`.

---

## 📋 WEEK 1: HIRING & BRIEFING

### Recruitment
- [ ] Post job listings on Fiverr/Upwork/ArtStation
- [ ] Post sound designer job on SoundCloud/Upwork
- [ ] Review portfolio submissions (5-10 candidates per role)
- [ ] Interview top 3 candidates per role
- [ ] Negotiate rates + timeline
- [ ] Sign contracts + onboard

### Contractor Kickoff
- [ ] Send Art Guide (`ART_ASSET_GUIDE.md`) to 2D artist
- [ ] Send Audio Spec (`AUDIO_SPECIFICATION.md`) to composer
- [ ] Send design docs (realm descriptions, vision)
- [ ] Send color reference files + inspiration links
- [ ] Send WebGL build link (show current state)
- [ ] Schedule weekly check-ins
- [ ] Discuss feedback/revision process

### Dev Team
- [ ] Review + test TransitionEffects.cs (no errors)
- [ ] Review + test AchievementCelebration.cs (no errors)
- [ ] Review + test ButtonJuice.cs (no errors)
- [ ] Commit to git ("Add polish scripts")
- [ ] Prepare to integrate next week

---

## 🎨 WEEK 2: ART PRODUCTION & EARLY TESTS

### Artist Deliverables
- [ ] Emberforge background (4 layer files, 2048x1536)
- [ ] Verdant Sanctuary background (4 layer files)
- [ ] 8 base sigil sprites (512x512 each) OR
- [ ] First 3 sigil sprites + feed back on style

### Composer Deliverables
- [ ] Emberforge theme (3-min loop) OR draft
- [ ] Verdant theme (3:30-min loop) OR draft
- [ ] 3-5 sample SFX (to establish sound direction)

### Dev Team Integration
- [ ] Create folder structure in Assets:
  - `Assets/_Project/Sprites/Realms/`
  - `Assets/_Project/Sprites/Sigils/`
  - `Assets/Audio/Music/`
  - `Assets/Audio/SFX/`
- [ ] Import first batch of art (backgrounds)
- [ ] Configure RealmData.cs with new sprites
- [ ] Test realm visuals in-engine
- [ ] Wire up TransitionEffects.cs to scene transitions
- [ ] Add ButtonJuice to main menu buttons
- [ ] Internal playtest pass (feel + feedback)

### Internal Feedback
- [ ] Playtesting notes from dev team
- [ ] Send feedback to artist (improvements needed)
- [ ] Send feedback to composer (vibe check)

### Quality Checks
- [ ] Backgrounds render cleanly (no compression artifacts)
- [ ] Sigils scale without blurriness (test 1x, 2x, 3x)
- [ ] Music loops seamlessly (test 5+ loops)
- [ ] SFX sound good at game volume levels

---

## 🎵 WEEK 3: FINAL ASSET DELIVERY & INTEGRATION

### Artist Final Delivery
- [ ] All 5 realm backgrounds complete (all 4 layers each) ✓
- [ ] All 8 base sigils delivered ✓
- [ ] 5 NPC character sprite sheets (256x256, 3-4 poses each) ✓
- [ ] 20+ UI icons (buttons, menus, etc.) ✓
- [ ] Particle texture atlas (1024x1024) ✓
- [ ] All files properly named + organized

### Composer Final Delivery
- [ ] All 7 music tracks (looping, 2.5-4.5 min each) ✓
- [ ] All 20+ SFX files (ritual, UI, rewards) ✓
- [ ] All 5 ambient loops (30 sec, seamless) ✓
- [ ] All files exported to OGG Vorbis format ✓
- [ ] Metadata/cue points for loops included ✓

### Dev Team Integration
- [ ] Import all art into project
- [ ] Configure all realm backgrounds (parallax layers)
- [ ] Configure all sigil sprites in SigilViewerManager
- [ ] Import all audio files
- [ ] Wire up music to AudioManager
- [ ] Wire up SFX to ritual events
- [ ] Test ambient loops (no volume clipping)
- [ ] Integrate AchievementCelebration (sigil unlocks)
- [ ] Test all transitions (zoom, shake, effects)

### Testing
- [ ] Play through all 5 realms (visuals look good?)
- [ ] Check colorblind modes (all sprites readable?)
- [ ] Test on 3+ mobile devices (performance 60 FPS?)
- [ ] Play with audio on (music smooth? SFX clear?)
- [ ] Test accessibility (reduced motion = no animations?)
- [ ] Bug log + known issues list

### Quality Assurance
- [ ] All backgrounds anti-aliased (no pixelation)
- [ ] All sigils glow correctly
- [ ] Music crossfades smooth (2-3 seconds)
- [ ] SFX volumes balanced
- [ ] No audio clicks at loop boundaries
- [ ] Touch input responsive (< 50ms lag)

---

## 🚀 WEEK 4: FINAL POLISH & LAUNCH PREP

### Final Dev Pass
- [ ] Fix all reported bugs (priority: crash fixes first)
- [ ] Performance optimization (60 FPS locked testing)
- [ ] UI polish (button animations on hover/press)
- [ ] Visual polish (celebration effects working)
- [ ] Audio mixing (all volumes correct)
- [ ] Accessibility validation (WCAG 2.1 AA+ check)

### Testing Coverage
- [ ] Internal team playtesting (2+ hours per realm)
- [ ] Mobile device testing (iOS + Android, 3+ devices)
- [ ] Accessibility user testing (5+ disabled players)
- [ ] Stress testing (play for 1 hour, monitor memory)
- [ ] Battery drain test (mobile, typical session)

### Launch Prep
- [ ] Create 6+ app store screenshots (high-res)
- [ ] Write app store description (compelling, honest)
- [ ] Create privacy policy + terms of service
- [ ] Prepare press release (media pitch)
- [ ] Set up social media posts (ready to publish)
- [ ] Notify press/influencers (1-2 weeks early)

### Build & Submit
- [ ] Create final production build (iOS + Android)
- [ ] Test build thoroughly (same as development)
- [ ] Submit to App Store + Google Play
- [ ] Note submission dates + expected review times

---

## 📊 PROGRESS TRACKER

### Art Assets Status
```
Backgrounds:  ☐ Draft  ☐ In Progress  ☐ In Engine  ☐ Final
Sigils:       ☐ Draft  ☐ In Progress  ☐ In Engine  ☐ Final
NPCs:         ☐ Draft  ☐ In Progress  ☐ In Engine  ☐ Final
UI/Icons:     ☐ Draft  ☐ In Progress  ☐ In Engine  ☐ Final
Particles:    ☐ Draft  ☐ In Progress  ☐ In Engine  ☐ Final
```

### Audio Assets Status
```
Music(7):     ☐ Draft  ☐ In Progress  ☐ Mixed    ☐ Exported
SFX (20+):    ☐ Draft  ☐ In Progress  ☐ Mixed    ☐ Exported
Ambient (5):  ☐ Draft  ☐ In Progress  ☐ Mixed    ☐ Exported
```

### Dev Integration Status
```
Polish Scripts:    ☐ Created  ☐ Tested   ☐ Integrated
Transitions:       ☐ Coded    ☐ Tested   ☐ Working
Celebrations:      ☐ Coded    ☐ Tested   ☐ Working
UI Juice:          ☐ Coded    ☐ Tested   ☐ Working
Music System:      ☐ Integrated ☐ Tested ☐ Looping
SFX System:        ☐ Integrated ☐ Tested ☐ Responsive
```

### Testing Status
```
Visual Quality:    ☐ Not Started  ☐ In Progress  ☐ Pass
Audio Quality:     ☐ Not Started  ☐ In Progress  ☐ Pass
Performance:       ☐ Not Started  ☐ In Progress  ☐ 60 FPS
Accessibility:     ☐ Not Started  ☐ In Progress  ☐ Pass
Mobile Devices:    ☐ Not Started  ☐ In Progress  ☐ Pass
```

---

## 🎯 MILESTONES

| Milestone | Week | Owner | Status |
|-----------|------|-------|--------|
| Contractors hired | 1 | Team | ☐ |
| Art briefs sent | 1 | Team | ☐ |
| Audio briefs sent | 1 | Team | ☐ |
| Backgrounds draft | 2 | Artist | ☐ |
| Sigils draft | 2 | Artist | ☐ |
| Music drafts | 2 | Composer | ☐ |
| Internal playtest | 2 | Dev | ☐ |
| All art delivered | 3 | Artist | ☐ |
| All audio delivered | 3 | Composer | ☐ |
| Full integration | 3 | Dev | ☐ |
| QA testing | 3-4 | Dev | ☐ |
| App store ready | 4 | Dev | ☐ |
| Submissions sent | 4 | Team | ☐ |
| **PUBLIC LAUNCH** | 5+ | **Team** | **☐** |

---

## 💬 WEEKLY CHECK-IN TEMPLATE

**For each contractor, ask:**

### Artist Check-in
1. Progress on current task? (% complete)
2. Any blockers or questions?
3. Timeline on track? (yes/no)
4. Any design questions for feedback?
5. When will next batch be ready? (date)

### Composer Check-in
1. Music tracks progress? (% complete)
2. SFX samples ready for review? (yes/no)
3. Any technical questions? (exports, formats, etc.)
4. Timeline on track? (yes/no)
5. When will finished audio be ready? (date)

### Developer Check-in
1. Integration progress? (% complete)
2. Any bugs found in new assets?
3. Performance impact? (FPS still 60?)
4. Accessibility checks done? (yes/no)
5. Ready for next batch? (yes/no)

---

## 📞 CONTACT REMINDERS

**Artist Deliverables Questions → Send to Artist**  
"I'm looking at the [Emberforge] background and wondering about [specific detail]. Can you clarify?"

**Audio Technical Questions → Send to Composer**  
"I'm getting clicks at loop boundaries. Can you check the OGG export settings?"

**Schedule Delays → Address Immediately**  
"I noticed we're 2-3 days behind. What can we do to catch up?"

---

## 🎓 KEY REMINDERS

### For Artist
✅ "Follow ART_ASSET_GUIDE.md for all specs"  
✅ "Test sprites in colorblind modes"  
✅ "Anti-alias all edges (no pixelation)"  
✅ "Export as PNG, 9 compression"  

### For Composer
✅ "Follow AUDIO_SPECIFICATION.md for all specs"  
✅ "Ensure loop points are seamless (no clicks)"  
✅ "Export as OGG Vorbis, 192kbps music / 128kbps SFX"  
✅ "Include metadata tags for loop regions"  

### For Dev Team
✅ "Wire up TransitionEffects immediately"  
✅ "Test 60 FPS after each import"  
✅ "Check colorblind modes with each visual"  
✅ "Don't skip mobile device testing"  

---

## 🚨 CRITICAL PATHS (Don't Fall Behind These)

| Item | Deadline | Consequence |
|------|----------|-------------|
| Hire contractors | End of Week 1 | Launch delay |
| Artist delivers by mid-Week 3 | Mid-Week 3 | Integration delay |
| Composer delivers by end-Week 3 | End of Week 3 | Integration delay |
| Integration complete by end-Week 3 | End of Week 3 | QA delay |
| QA testing complete | End of Week 4 | Launch delay |
| AppStore submissions sent | End of Week 4 | Launch day delay |

---

## ✨ AT THE FINISH LINE

**When everything is done, you should have:**

✅ Game with beautiful, polished visuals (5 realms)  
✅ Smooth transitions with juice & celebration effects  
✅ Every interaction has satisfying audio feedback  
✅ Accessible to colorblind players (unique content per mode)  
✅ Runs 60 FPS on 2-year-old devices  
✅ Immersive soundscapes that feel like being IN each realm  
✅ Ready for app store submission  
✅ Portfolio-quality game ready for PR push  

**You'll have a REAL GAME ready to ship! 🚀**

---

**Print this. Check off items weekly. Share progress with team.**

*Good luck! You've got this! 💪✨*
