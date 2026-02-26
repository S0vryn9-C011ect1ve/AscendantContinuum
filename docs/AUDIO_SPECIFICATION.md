# 🎵 Audio Design Specification for The Ascendant Continuum

**Created:** February 25, 2026  
**For:** Sound Designer / Composer  
**Scope:** 7 music tracks, 20+ SFX, ambient soundscapes

---

## 📌 **QUICK REFERENCE**

### **Total Audio Assets Needed:**
- **7 Music Tracks** (looping ambient themes)
- **20+ Sound Effects** (ritual interactions, UI, rewards)
- **5 Ambient Soundscapes** (realm background loops)

### **Technical Requirements:**
- **Format:** WAV (uncompressed) exported as OGG Vorbis (compressed for game)
- **Sample Rate:** 44.1 kHz or 48 kHz
- **Channels:** Stereo (music), Mono (SFX)
- **Bit Rate:** 128-192 kbps (OGG Vorbis quality)

---

## 🎼 **MUSIC TRACKS (Composition Specification)**

### **1. EMBERFORGE THEME**
**File:** `emberforge_theme.ogg`

**Duration:** 3:00 loop (seamless)  
**BPM:** 100  
**Key:** A Major / C# Minor (warm, energetic)  
**Mood:** Playful, energetic, creative, warm

**Instrumentation:**
- **Foundation:** Warm strings (cello, viola) providing steady bass
- **Rhythm:** Gentle percussion (wood blocks, light timpani) on beat
- **Melody:** Bright bells or harpsichord (playful, childlike wonder)
- **Harmony:** Warm pads supporting with golden resonance
- **Accent:** Occasional chime dings (magical moments)

**Dynamics:**
- **Intro (0:00-0:30):** Build slowly, introduce each instrument
- **Main Loop (0:30-2:30):** Steady, engaging, loops smoothly
- **Outro (2:30-3:00):** Gentle fade to silence (smooth loop boundary)

**Emotional Arc:** Creates sense of entering a magical workshop where creation happens joyfully

**Technical Reference:**
- No abrupt endings
- Peak volume: -6dB (leaves headroom)
- Loop point: Exactly 3:00 mark, silence for 0.1s before restart

---

### **2. VERDANT SANCTUARY THEME**
**File:** `verdant_theme.ogg`

**Duration:** 3:30 loop (seamless)  
**BPM:** 80  
**Key:** D Major / B Minor (calming, organic)  
**Mood:** Peaceful, meditative, grounded, organic

**Instrumentation:**
- **Foundation:** Soft piano (left hand, steady, meditative)
- **Melody:** Gentle flute or open woodwind (nature-inspired)
- **Harmony:** Subtle string pads (sustaining, calming)
- **Texture:** Ambient nature sounds layer (birds distant, wind gentle, water trickle)
- **Percussion:** Minimal (soft mallets, one note every 8 beats to ground rhythm)

**Dynamics:**
- **Intro (0:00-0:45):** Soft entry, warm piano foundation, flute enters slowly
- **Main Loop (0:45-3:00):** Steady, repeating, creates sense of timeless peace
- **Outro (3:00-3:30):** Nature sounds swell slightly, piano/flute fade

**Emotional Arc:** Listener feels welcomed into a sacred garden, safe and calm

**Technical Reference:**
- Extremely smooth dynamics (no sudden changes)
- Peak volume: -8dB (intentionally quieter, meditative)
- Nature ambient layer: -18dB (background texture)

---

### **3. ECHO FIELDS THEME**
**File:** `echo_fields_theme.ogg`

**Duration:** 4:00 loop (seamless)  
**BPM:** 75  
**Key:** F# Minor / A Major (ethereal, mysterious)  
**Mood:** Mysterious, ethereal, dreamy, introspective

**Instrumentation:**
- **Foundation:** Ambient synthesizer pads (floating, weightless)
- **Melody:** Bells or vibraphone (sparse, thoughtful)
- **Texture:** Ethereal vocal ahhs (wordless, human but distant)
- **Depth:** Reverse reverb effects (sounds come from everywhere)
- **Cinematic:** String swells (sparse moments of emotional peaks)

**Dynamics:**
- **Intro (0:00-1:00):** Pad introduces, very sparse
- **Development (1:00-2:30):** Bells + pads, distant vocals enter
- **Peak (2:30-3:30):** String swell, moment of revelation
- **Outro (3:30-4:00):** Back to sparse, fading into silence

**Special Effects:**
- Heavy use of reverb (suggest space/void)
- Slight pitch modulation on pad (creates movement in stillness)
- Wide stereo field (images spread across left/right)

**Emotional Arc:** Listener feels suspended in a memory, timeless and vast

**Technical Reference:**
- Peak volume: -6dB
- Use generous reverb (1.5+ second tail)
- Ensure loop point is absolutely silent for seamless restart

---

### **4. DAWN CITADEL THEME**
**File:** `dawn_citadel_theme.ogg`

**Duration:** 3:00 loop (seamless)  
**BPM:** 95  
**Key:** G Major / E Minor (triumphant, bright)  
**Mood:** Triumphant, inspiring, hopeful, bright

**Instrumentation:**
- **Foundation:** Orchestral brass section (trumpets, horns) steady and strong
- **Melody:** Soaring strings (violin, driving rhythm)
- **Harmony:** Choir or choir-like synth (wordless vocals, celebratory)
- **Rhythm:** Timpani/percussion (marching beat, steady )
- **Accent:** Harp or celesta (shimmering moments)

**Dynamics:**
- **Intro (0:00-0:30):** Strings begin alone, building anticipation
- **Entrance (0:30-1:00):** Brass enters triumphantly
- **Apex (1:00-2:30):** Full orchestration, feeling of victory, glory
- **Transition (2:30-3:00):** Slight diminuendo but staying strong

**Emotional Arc:** Listener feels they've overcome a challenge and reached enlightenment

**Technical Reference:**
- Peak volume: -3dB (dynamic, exciting)
- Ensure brass doesn't clip (important for clarity)
- Loop must feel conclusive but ready to repeat immediately

---

### **5. LANTERN ASCENSION THEME**
**File:** `lantern_ascension_theme.ogg`

**Duration:** 4:30 loop (seamless)  
**BPM:** 60  
**Key:** Amajor / F# Minor (transcendent, peaceful)  
**Mood:** Meditative, transcendent, peaceful, reflective

**Instrumentation:**
- **Foundation:** Minimalist ambient pad (single long tone)
- **Texture:** Sparse bells (every 4-8 beats, gentle)
- **Melody:** Nothing prominent (or extremely subtle)
- **Special:** Wind chimes or glass harmonicas (barely audible)
- **Depth:** Space/reverb (sense of vast emptiness with peace)

**Dynamics:**
- **Entire Track:** Extremely static, almost no change
- **Subtle variation:** Very slow modulation on pad (micro-dynamics)

**Emotional Arc:** Listener enters a meditative state of acceptance and peace

**Technical Note:**
- This track should be almost boring to maintain deep meditation state
- Peak volume: -10dB (very quiet, intimate)
- Listener should forget they're listening to music

---

### **6. MAIN MENU THEME**
**File:** `main_menu_theme.ogg`

**Duration:** 2:30 loop (seamless)  
**BPM:** 90  
**Key:** D Major (welcoming, magical)  
**Mood:** Welcoming, magical, curious, inviting

**Instrumentation:**
- **Concept:** Blend elements from all 5 realms
- **Opening:** Soft bells (Lantern theme essence)
- **Build:** Add warm strings (Verdant essence)
- **Peak:** Bring in bright melody (Emberforge essence)
- **Sustain:** Hold with all elements together

**Composition Approach:**
- Instrumentally blend themes from 3-4 realms
- Create sense of wonder and invitation
- Not too intense (player is in menu, not active gameplay)

---

### **7. ONBOARDING TUTORIAL THEME**
**File:** `onboarding_theme.ogg`

**Duration:** 1:30 loop (seamless)  
**BPM:** 85  
**Key:** C Major (friendly, approachable)  
**Mood:** Gentle, educational, warm, welcoming

**Instrumentation:**
- **Opening:** Soft piano or glockenspiel
- **Support:** Warm strings (safe, embracing)
- **Optional:** Gentle vocal humming (welcoming, non-threatening)

**Purpose:**
- First music players hear
- Should feel immediately safe and inviting
- Not too complex (cognitive load for tutorial)

**Technical Reference:**
- Peak volume: -8dB (gentle)
- Loop should feel natural and calm

---

## 🔊 **SOUND EFFECTS (SFX SPECIFICATION)**

### **Category 1: Ritual Interaction Sounds**

These sounds trigger during core gameplay (ritual completion):

#### **spark_tap.ogg** (Emberforge tap interaction)
- **Duration:** 0.3-0.5 seconds
- **Type:** Musical chime
- **Pitch:** High (880 Hz / A5)
- **Envelope:** Quick attack, short sustain, quick decay
- **Character:** Bright, satisfying, magical
- **Example:** Xylophone strike, bell chime
- **Intensity:** Medium (satisfying but not jarring)
- **Technical:** Peak -12dB, mono

*Use:* When player taps a spark in Emberforge

---

#### **spark_collect.ogg** (Successful spark collection)
- **Duration:** 0.6-0.8 seconds
- **Type:** Sparkle/cascade
- **Character:** Shimmery, playful, celebratory
- **Composition:** 3-4 quick chime notes ascending in pitch
- **Example:** Sparkle sound effect (think Zelda item collect)
- **Intensity:** Medium
- **Technical:** Peak -10dB, stereo spread

*Use:* When player successfully completes Emberforge spark ritual

---

#### **plant_grow.ogg** (Verdant plant growth)
- **Duration:** 0.5-0.7 seconds
- **Type:** Organic whoosh with subtle click
- **Character:** Natural, alive, energetic
- **Composition:** Gentle whoosh with organic "pop" at end
- **Example:** Wind sound with subtle plant snap
- **Intensity:** Soft
- **Technical:** Peak -14dB, mono

*Use:* When virtual plant grows in Verdant Sanctuary

---

#### **constellation_connect.ogg** (Echo Fields star connection)
- **Duration:** 0.6-0.9 seconds
- **Type:** Ethereal shimmer
- **Character:** Magical, connecting, ethereal
- **Composition:** Shimmer with subtle ascending tone
- **Example:** Crystal chime with reverb tail
- **Intensity:** Soft
- **Technical:** Peak -12dB, stereo with reverb

*Use:* When player connects two stars in constellation

---

#### **light_refract.ogg** (Dawn Citadel light puzzle solution)
- **Duration:** 0.5-0.7 seconds
- **Type:** Crystal resonance
- **Character:** Pure, clear, triumphant
- **Composition:** Clear bell tone with harmonic overtones
- **Example:** High-frequency crystal bowl sound
- **Intensity:** Bright but not harsh
- **Technical:** Peak -10dB, stereo

*Use:* When light beam correctly refracts in puzzle

---

#### **lantern_place.ogg** (Lantern Ascension lantern placement)
- **Duration:** 0.4-0.6 seconds
- **Type:** Soft placement sound
- **Character:** Peaceful, gentle, meditative
- **Composition:** Very soft "settling" sound, almost a sigh
- **Example:** Soft cloth settling, very gentle
- **Intensity:** Very soft
- **Technical:** Peak -16dB, mono

*Use:* When player places lantern in space

---

### **Category 2: UI Interaction Sounds**

These play when players interact with menus/buttons:

#### **button_click.ogg**
- **Duration:** 0.2-0.3 seconds
- **Type:** Satisfying click
- **Character:** Responsive, confirmatory
- **Example:** Keyboard key click, game accept sound
- **Technical:** Peak -10dB, mono

---

#### **menu_open.ogg**
- **Duration:** 0.3-0.5 seconds
- **Type:** Smooth transition whoosh
- **Character:** Smooth, no jarring
- **Example:** Soft swoosh (no harsh frequencies)
- **Technical:** Peak -12dB, stereo

---

#### **menu_close.ogg**
- **Duration:** 0.3-0.5 seconds
- **Type:** Reverse whoosh (opposite of open)
- **Character:** Complementary to open sound
- **Example:** Reverse of menu_open effect
- **Technical:** Peak -12dB, stereo

---

#### **achievement_unlock.ogg**
- **Duration:** 0.6-0.8 seconds
- **Type:** Celebratory fanfare (mini version)
- **Character:** Exciting but not overwhelming
- **Composition:** 3-4 ascending notes with slight swell
- **Example:** Victory sting (video game style)
- **Technical:** Peak -8dB, stereo

---

#### **sigil_unlock.ogg**
- **Duration:** 0.8-1.0 seconds
- **Type:** Magical shimmer fanfare
- **Character:** Magical, special, exciting
- **Composition:** Longer celebration than achievement_unlock
- **Example:** Magical sparkle burst + chime
- **Technical:** Peak -6dB, stereo

---

#### **notification_ping.ogg**
- **Duration:** 0.15-0.25 seconds
- **Type:** Alert ding
- **Character:** Noticeable but not intrusive
- **Example:** Message notification bell
- **Technical:** Peak -12dB, mono

---

### **Category 3: Ambient/Environmental Sounds**

These loop in backgrounds for immersion:

#### **emberforge_ambience.ogg**
- **Duration:** 30 seconds (loop)
- **Content:**
  - Crackling fire (steady background)
  - Occasional pop/spark (adds life)
  - Distant machinery hum (low frequency)
  - Metal resonances (occasional metallic note)
- **Volume:** -24dB to -20dB (very quiet, background layer)
- **Purpose:** Layer under music for immersion

---

#### **verdant_ambience.ogg**
- **Duration:** 30 seconds (loop)
- **Content:**
  - Bird songs (distant, occasional)
  - Gentle wind rustling leaves
  - Water trickle (meditation pool)
  - Insect chirps (peaceful, natural)
- **Volume:** -26dB to -22dB (very subtle)

---

#### **echo_fields_ambience.ogg**
- **Duration:** 30 seconds (loop)
- **Content:**
  - Ethereal pad tone (very subtle)
  - Distant bells (haunting)
  - Air movement/whoosh
  - reverb tail of nothing (space itself)
- **Volume:** -28dB to -24dB (almost inaudible)

---

#### **dawn_citadel_ambience.ogg**
- **Duration:** 30 seconds (loop)
- **Content:**
  - Distant choir hum (wordless)
  - Light wind
  - Occasional bell resonance
  - Crystal resonance (harmonic)
- **Volume:** -24dB to -20dB

---

#### **lantern_ambience.ogg**
- **Duration:** 30 seconds (loop)
- **Content:**
  - Soft wind (gentle, distant)
  - Distant bells (very sparse)
  - Silence (most of it is silence!)
  - Paper rustle (fabric of lanterns)
- **Volume:** -28dB to -26dB (almost silent)

---

### **Category 4: Celebration/Success Sounds**

These play on major achievements:

#### **ritual_complete_minor.ogg**
- **Duration:** 0.8 seconds
- **Type:** Simple success ding
- **Character:** Satisfying but understated
- **Example:** Short chime sequence
- **Technical:** Peak -10dB

---

#### **ritual_complete_major.ogg**
- **Duration:** 1.2 seconds
- **Type:** Triumphant fanfare
- **Character:** More exciting, celebratory
- **Composition:** 4-5 ascending notes, held on top note
- **Example:** Victory theme (video game style)
- **Technical:** Peak -6dB, stereo

---

#### **daily_challenge_complete.ogg**
- **Duration:** 1.5-2.0 seconds
- **Type:** Epic victory sting
- **Character:** Exciting, proud, transformative
- **Composition:** Full fanfare with swell
- **Example:** Achievement unlocked orchestral swell
- **Technical:** Peak -4dB, full stereo surround

---

#### **achievement_secret_unlock.ogg**
- **Duration:** 2.0-2.5 seconds
- **Type:** Mysterious then triumphant reveal
- **Character:** Surprise, then pride
- **Composition:** Build from single note → swell to chord
- **Example:** Secret treasure found (epic reveal)
- **Technical:** Peak -6dB, stereo with reverb

---

#### **level_up.ogg** (Progression milestone)
- **Duration:** 0.6 seconds
- **Type:** Ascending chime cascade
- **Character:** Growth, advancement, positive progress
- **Example:** RPG level-up sound
- **Technical:** Peak -8dB, mono

---

## 📊 **AUDIO MIXING REFERENCE**

### **Master Volume Levels (dB)**

| Category | Level | Notes |
|----------|-------|-------|
| Music (Main) | -6dB | Primary audio content |
| Music (Peak) | -3dB | Dynamic peaks only |
| SFX (UI) | -12dB | Don't overpower music |
| SFX (Ritual) | -10dB | Noticeable, satisfying |
| SFX (Celebration) | -4 to -6dB | Momentarily louder (special) |
| Ambient | -24dB | Almost inaudible texture |
| Notification | -12dB | Attention without jarring |

### **EQ Guidance**

**Music Tracks:**
- Cut harsh high frequencies (reduce 3-5kHz harshness)
- Boost warmth (250-500Hz slightly)
- Keep sub-bass minimal (cleaner mobile playback)

**Ritual SFX:**
- Bright highs (preserve clarity)
- Minimal low end (sits above music)
- Sharp attack (immediate response)

**Ambient Loops:**
- Very smooth EQ (no harsh frequencies)
- Good sub-bass (60-80Hz, subtle)
- Rolled off highs (smoother)

---

## 🎚️ **DYNAMIC MUSIC IMPLEMENTATION**

### **Concept: Layered Music System**

Each realm theme can have multiple "stems" (layers):

```
Emberforge Theme Layers:
├── Base Layer (piano + foundational strings) - always plays
├── Energy Layer (percussion + bright elements) - fades in during rituals
├── Victory Layer (full orchestra) - plays on completion
└── Ambient Layer (nature/fire sounds) - background texture
```

### **Implementation:**

1. **Base Layer:** Always at -6dB
2. **Energy Layer Volume:**
   - 0% (silent) in calm states
   - Crossfade to 50% during active gameplay
   - 100% (full volume) during intense rituals
   
3. **Victory Layer Volume:**
   - 0% normally
   - Fades in (2-3 second crossfade) on major completion
   - Holds for celebration
   - Fades back to base layer after 3 seconds

### **Crossfade Timing:**
- Reduces motion mode: Snap immediately (no crossfade)
- Normal mode: 2-3 second smooth crossfade
- Never jarring, always musical

---

## 📋 **AUDIO ASSET DELIVERY CHECKLIST**

### **Batch 1: Core Music (Priority):**
- [ ] Emberforge theme (3:00 loop)
- [ ] Verdant theme (3:30 loop)
- [ ] Echo Fields theme (4:00 loop)
- [ ] Dawn Citadel theme (3:00 loop)
- [ ] Main Menu theme (2:30 loop)

### **Batch 2: SFX Core:**
- [ ] 6 Ritual interaction sounds
- [ ] 6 UI interaction sounds
- [ ] 5 Ambient loops (30 sec each)

### **Batch 3: Celebration Sounds:**
- [ ] 5 Success/celebration sounds
- [ ] Notification ping
- [ ] Achievement unlock fanfare

### **Batch 4: Optional Enhancements:**
- [ ] Lantern Ascension theme (4:30 loop)
- [ ] Onboarding theme (1:30 loop)
- [ ] Additional SFX variants

---

## 🔧 **TECHNICAL EXPORT SETTINGS**

### **For Music Loops:**

**Source File (Final Master):**
```
Format: WAV
Sample Rate: 44.1 kHz
Bit Depth: 16-bit or 24-bit
Channels: Stereo
Solo: Peak -6dB to -3dB (leave headroom)
Loop Region: Perfectly seamless (test before export)
```

**Export to Game (.ogg):**
```
Format: OGG Vorbis
Bitrate: 192 kbps (quality priority for music)
Sample Rate: 44.1 kHz
Channels: Stereo
Metadata: Include cue points for loop region
File Size Target: 1-2 MB per track
```

### **For SFX:**

**Source File:**
```
Format: WAV
Sample Rate: 44.1 kHz
Bit Depth: 16-bit
Channels: Mono (unless spatial/stereo needed)
Peak: -12dB typical (varies by SFX)
```

**Export to Game (.ogg):**
```
Format: OGG Vorbis
Bitrate: 128 kbps (SFX don't need high quality)
Sample Rate: 44.1 kHz
Channels: Mono (unless spatial)
File Size: 50-200 KB per effect
```

### **For Ambient Loops:**

```
Format: OGG Vorbis
Bitrate: 128 kbps
Loop Region: Set in metadata
Duration: Exactly 30 seconds
Seamless: Absolutely no clicks at loop boundary
```

---

## 📂 **FILE ORGANIZATION**

Provide all assets organized as follows:

```
AscendantContinuum_Audio/
├── Music/
│   ├── emberforge_theme.ogg
│   ├── verdant_theme.ogg
│   ├── echo_fields_theme.ogg
│   ├── dawn_citadel_theme.ogg
│   ├── lantern_ascension_theme.ogg
│   ├── main_menu_theme.ogg
│   └── onboarding_theme.ogg
├── SFX/
│   ├── Rituals/
│   │   ├── spark_tap.ogg
│   │   ├── spark_collect.ogg
│   │   ├── plant_grow.ogg
│   │   ├── constellation_connect.ogg
│   │   ├── light_refract.ogg
│   │   └── lantern_place.ogg
│   ├── UI/
│   │   ├── button_click.ogg
│   │   ├── menu_open.ogg
│   │   ├── menu_close.ogg
│   │   ├── achievement_unlock.ogg
│   │   ├── sigil_unlock.ogg
│   │   └── notification_ping.ogg
│   ├── Ambient/
│   │   ├── emberforge_ambience.ogg
│   │   ├── verdant_ambience.ogg
│   │   ├── echo_fields_ambience.ogg
│   │   ├── dawn_citadel_ambience.ogg
│   │   └── lantern_ambience.ogg
│   └── Rewards/
│       ├── ritual_complete_minor.ogg
│       ├── ritual_complete_major.ogg
│       ├── daily_challenge_complete.ogg
│       ├── achievement_secret_unlock.ogg
│       └── level_up.ogg
└── README.txt
    (Notes on BPM, key, loop points, any special mixing notes)
```

---

## 🎯 **QUALITY CHECKLIST**

- [ ] All loops seamless (no clicks at boundaries)
- [ ] No sudden EQ changes that jar listener
- [ ] Peak volumes appropriate for category
- [ ] All files properly tagged with metadata
- [ ] Bitrates optimized (music 192kbps, SFX 128kbps)
- [ ] Tested on mobile speaker + headphones
- [ ] Ambient layers don't distract from gameplay
- [ ] Music layers blend smoothly on crossfade
- [ ] No phase issues in stereo mixes
- [ ] Hearing test: Can deaf players still enjoy game (haptics + visuals)?

---

## 🎓 **ARTISTIC DIRECTION NOTES**

### **Overall Audio Philosophy**
- **Tone:** Warm, inviting, peaceful, magical
- **Intent:** Make players feel welcomed and safe
- **Avoid:** Harsh frequencies, sudden changes, dissonance
- **Goal:** Audio feels like a friend guiding player through journey

### **Music Composition Approach**
- Use modal progressions (creates sense of floating/timelessness)
- Avoid traditional V-I cadences (too "final")
- Create ongoing sense of journey, not destination
- Lean into ambient/minimalism (players spend 2-5 min per realm)

### **SFX Philosophy**
- Every action should have immediate audio feedback (but not overwhelming)
- Use natural/musical sounds over electronic
- Pitch matters: ascending = positive, descending = negative
- Keep attacks sharp (responsiveness) but tails soft (musicality)

---

## 📞 **CONTACT & TIMELINE**

**Expected Delivery:** 2-3 weeks from start date  
**Milestones:**
- Week 1: Draft all music tracks (reference timing)
- Week 2: Finalize music + create SFX library
- Week 3: Master all audio, export to OGG format, deliver organized files

**Questions?** Contact: ascendantcontinuum@gmail.com

---

**Thank you for bringing The Ascendant Continuum to life with beautiful sound! 🎵**

