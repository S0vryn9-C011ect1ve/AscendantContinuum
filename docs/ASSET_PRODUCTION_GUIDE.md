# ASSET PRODUCTION GUIDE — THE ASCENDANT CONTINUUM
**Complete Visual & Audio Specification for Asset Creation**
Version 1.0 | March 2026

---

## HOW TO USE THIS DOCUMENT

Every table row is one deliverable file. The **Drop Folder** column tells you exactly where to put the file in Unity — the import settings apply automatically the moment you drop it in. Work top-to-bottom. Priority 1 assets are needed to make any scene playable.

---

# PART A — VISUAL ASSETS

Total visual assets needed: **62 files**

---

## A1 — REALM BACKGROUNDS (5 files)
**Drop folder:** `Assets/_Project/Art/Backgrounds/`
**Format:** PNG with no alpha (solid backgrounds), 2048 × 2048 px (Unity will letterbox to portrait)
**Import:** Auto-applied by SpriteImportPostprocessor — 2048 max, ETC2/ASTC/DXT5 per platform

| # | File Name | Realm | Primary Color | Secondary Color | Art Direction |
|---|---|---|---|---|---|
| 1 | `bg_emberforge.png` | The Emberforge | `#FF6119` (molten orange) | `#1A0800` (deep char black) | Volcanic forge interior. Glowing cracks in dark stone floor and walls. Lava pools in mid-distance. Floating embers rising from below. Hot orange-red light radiates from below frame. Dark vaulted ceiling above. |
| 2 | `bg_verdant.png` | The Verdant Sanctuary | `#1DB84A` (vivid green) | `#0A2E18` (deep forest night) | Bioluminescent garden at dusk. Glowing mushrooms, fireflies, luminous moss. Soft blue-green light pools on dark earth. Canopy of large magical leaves above. Still water reflecting light at the bottom edge. |
| 3 | `bg_echofields.png` | The Echo Fields | `#1A1F8C` (deep space blue) | `#2E0A52` (deep violet) | Infinite star field at night. Milky way band across upper half. Floating constellation lines faintly visible. Soft purple nebula wisps. Dark rolling hills silhouette at bottom. No moon — only stars. |
| 4 | `bg_dawncitadel.png` | The Dawn Citadel | `#FFD700` (gold) | `#FFF5CC` (pale cream light) | Interior of a crystal cathedral at sunrise. Geometric prism shapes hanging from vaulted ceiling. Light beams refracting into rainbow spectrums on the floor. White-gold stone architecture. Warm sunlight floods from tall arched windows. |
| 5 | `bg_lanternascension.png` | The Lantern Ascension | `#7B3FD4` (deep purple) | `#1A0A30` (midnight indigo) | Vast night sky, no horizon visible — camera points straight up. Hundreds of glowing amber/gold paper lanterns at varying distances, drifting upward. Thin clouds and stars between them. A few large close lanterns glow warm orange in foreground. |

---

## A2 — REALM ICONS / HUD ICONS (7 files)
**Drop folder:** `Assets/_Project/Art/Sprites/UI/`
**Format:** PNG with transparency, 256 × 256 px, circular or iconic shape
**Usage in code:** `HUDManager.realmIcon` (Image), `MainMenuManager.realmButtons` backgrounds

| # | File Name | Represents | Art Direction |
|---|---|---|---|
| 1 | `icon_realm_emberforge.png` | Emberforge realm button | Stylised flame or anvil spark. Warm orange/red. Bold silhouette readable at 64px. |
| 2 | `icon_realm_verdant.png` | Verdant realm button | Stylised 3-petal flower or leaf. Vivid green. |
| 3 | `icon_realm_echofields.png` | Echo Fields realm button | Three connected stars forming a small triangle constellation. White/light blue. |
| 4 | `icon_realm_dawncitadel.png` | Dawn Citadel realm button | Crystal prism or sun rays. Gold/yellow. |
| 5 | `icon_realm_lanternascension.png` | Lantern Ascension realm button | Paper lantern silhouette with glow. Warm amber on deep purple. |
| 6 | `icon_sigil.png` | Sigil currency — `HUDManager.sigilCountIcon` | Abstract rune symbol, geometric, purple/violet. |
| 7 | `icon_sparks.png` | Spark currency — `HUDManager.sparksCountIcon` | Small bright spark/star. Orange-gold. |

---

## A3 — EMBERFORGE GAMEPLAY SPRITES (4 files)
**Drop folder:** `Assets/_Project/Art/Sprites/Realms/Emberforge/`
**Format:** PNG with transparency
**Usage:** `EmberforgeSparks.sparkPrefab` → `Spark` component needs a `SpriteRenderer`

| # | File Name | Size | Spark Color | Art Direction |
|---|---|---|---|---|
| 1 | `spark_small.png` | 64 × 64 px | `#FF9933` (amber) | Tiny glowing ember dot. Soft radial glow. Like a firefly. Transparent outer edge. |
| 2 | `spark_medium.png` | 96 × 96 px | `#FF6619` (orange) | Teardrop or elongated spark shape with motion blur suggestion. Brighter centre. |
| 3 | `spark_large.png` | 128 × 128 px | `#FF3300` (fire red-orange) | Large dramatic spark. Star-like with 4–6 pointed rays. Intense centre glow. |
| 4 | `spark_glow_ring.png` | 256 × 256 px | `#FF8800` (warm gold) | Soft circular glow ring. Used as a particle / background halo behind spark clusters. Pure radial gradient, fully transparent at edges. |

---

## A4 — VERDANT SANCTUARY GAMEPLAY SPRITES (7 files)
**Drop folder:** `Assets/_Project/Art/Sprites/Realms/Verdant/`
**Format:** PNG with transparency
**Usage:** `MagicalPlant` fields: `seedSprite`, `sproutSprite`, `plantSprite`, `bloomSprite` + 3 soil patches for the garden

| # | File Name | Size | Growth Stage | Art Direction |
|---|---|---|---|---|
| 1 | `plant_seed.png` | 128 × 128 px | Stage 1 — Seed | Small round seed half-buried in dark soil. Tiny white root visible at bottom. Warm brown tones. |
| 2 | `plant_sprout.png` | 200 × 280 px | Stage 2 — Sprout | Two small oval leaves on a thin green stem. Light lime green. Slightly dewy. |
| 3 | `plant_growing.png` | 256 × 384 px | Stage 3 — Plant (mid) | Larger multi-leaf plant, 4–5 leaves, slightly luminous edges. Deep green with teal veins. |
| 4 | `plant_bloom.png` | 320 × 480 px | Stage 4 — Full Bloom | Majestic glowing flower fully open. Central bioluminescent petals in cyan/teal. Radiates soft light. This is the reward visual — make it beautiful. |
| 5 | `soil_patch_1.png` | 300 × 160 px | Garden soil slot | Oval dark earth mound with subtle moss. Used as sprite in scene for each plant slot. |
| 6 | `soil_patch_2.png` | 300 × 160 px | Garden soil slot | Same as above, slight variation (different moss pattern). |
| 7 | `soil_patch_3.png` | 300 × 160 px | Garden soil slot | Same as above, variation 3. |

---

## A5 — ECHO FIELDS GAMEPLAY SPRITES (4 files)
**Drop folder:** `Assets/_Project/Art/Sprites/Realms/EchoFields/`
**Format:** PNG with transparency
**Usage:** `Star` component → `SpriteRenderer`. `Star.idleColor` tints to white/blue; `Star.connectedColor` tints to cyan.

| # | File Name | Size | State | Art Direction |
|---|---|---|---|---|
| 1 | `star_idle.png` | 64 × 64 px | Unconnected star | Soft white/cream star point, 4–6 rays, slight glow. Semi-transparent outer glow. |
| 2 | `star_connected.png` | 80 × 80 px | Connected/active star | Same star but brighter, more defined, ice-blue tint. Centre is pure white. |
| 3 | `star_highlight.png` | 80 × 80 px | Hover/touch target | Slightly larger, warm yellow-white, slight pulse halo around it. |
| 4 | `constellation_line.png` | 128 × 8 px | Line between stars | Thin glowing line segment. White-cyan gradient. Used by `LineRenderer` or as a stretched sprite between connected stars. |

---

## A6 — DAWN CITADEL GAMEPLAY SPRITES (5 files)
**Drop folder:** `Assets/_Project/Art/Sprites/Realms/DawnCitadel/`
**Format:** PNG with transparency
**Usage:** `LightRefractionPuzzle` — prism objects, light beam material source, and target sprites

| # | File Name | Size | Element | Art Direction |
|---|---|---|---|---|
| 1 | `prism_default.png` | 128 × 128 px | Rotatable prism (player interacts) | Equilateral triangle crystal prism, glassy with internal refraction rainbow. Clear outline, see-through interior. |
| 2 | `prism_active.png` | 128 × 128 px | Prism when light hits it | Same prism but with a warm glow and visible refracted spectrum inside. More vivid. |
| 3 | `light_beam.png` | 64 × 256 px | Light beam segment | Vertical soft white/yellow beam. Feathered soft edges. Used as `lightBeamMaterial` texture / stretched sprite. |
| 4 | `target_inactive.png` | 96 × 96 px | Puzzle target (light must reach here) | Circular target ring, dark grey/stone, faint etched rune pattern. |
| 5 | `target_active.png` | 96 × 96 px | Target when lit | Same ring but lit gold, radiating energy. Rainbow shimmer around the ring. |

---

## A7 — LANTERN ASCENSION GAMEPLAY SPRITES (5 files)
**Drop folder:** `Assets/_Project/Art/Sprites/Realms/LanternAscension/`
**Format:** PNG with transparency
**Usage:** `LanternRitual.lanternPrefab` sprite; `LanternRitual.lanternColors` tints the sprite at runtime in 4 color variants

| # | File Name | Size | State | Art Direction |
|---|---|---|---|---|
| 1 | `lantern_unlit.png` | 128 × 200 px | Not yet activated | Traditional paper lantern with bamboo frame. Pale cream/white paper. Tassel hanging below. The sprite is white/neutral so runtime tinting works. |
| 2 | `lantern_lit.png` | 128 × 200 px | On the ground, lit | Same lantern but with warm amber glow from within. Paper appears translucent with internal fire. |
| 3 | `lantern_releasing.png` | 128 × 200 px | Mid-release animation | Lantern tilted slightly, lifting, a few paper creases suggesting movement. |
| 4 | `lantern_ascending.png` | 96 × 160 px | Far-distance (smaller, in sky) | Simplified lantern silhouette with glow. Used for the hundreds of distant lanterns in the sky. Less detail. |
| 5 | `wish_glow.png` | 96 × 96 px | Wish particle/glow | Soft round glow in warm amber. Radial gradient. Used as particle texture for the `stardustEffect` particle system. |

---

## A8 — SHARED / UI SPRITES (9 files)
**Drop folder:** `Assets/_Project/Art/Sprites/Shared/`
**Format:** PNG with transparency
**Usage:** Various UI panels, buttons, background panels, loading screen

| # | File Name | Size | Usage | Art Direction |
|---|---|---|---|---|
| 1 | `panel_bg_dark.png` | 512 × 512 px, 9-sliced | Background for all UI panels (HUD, menus) | Deep dark purple-black, subtle magic circle/rune border. 9-slice border = 32px each side. |
| 2 | `panel_bg_light.png` | 512 × 512 px, 9-sliced | Lighter panels (info overlays) | Same panel, slightly lighter purple. |
| 3 | `button_default.png` | 512 × 128 px, 9-sliced | All regular game buttons | Pill-shaped button. Deep purple fill, light purple border glow. 9-slice = 64px sides. |
| 4 | `button_hover.png` | 512 × 128 px, 9-sliced | Button hover/pressed state | Same button, brighter border, slightly lighter fill. |
| 5 | `button_accent.png` | 512 × 128 px, 9-sliced | Primary CTA buttons (Play, Enter Realm) | Vibrant purple-to-violet gradient fill. Bright white border. |
| 6 | `splash_logo.png` | 1024 × 512 px | App launch screen logo | "The Ascendant Continuum" wordmark in magical serif/fantasy font. White text with soft purple glow. On transparent background. |
| 7 | `sigil_placeholder.png` | 256 × 256 px | `playerSigilImage` in MainMenu — shown before player draws their first sigil | Faint outline of a blank rune circle. Dashed line. Gray/translucent. A subtle "draw your sigil" prompt feel. |
| 8 | `progress_bar_fill.png` | 256 × 32 px | `challengeProgressBar` fill image in HUD | Horizontal gradient strip. Left = dark purple, right = bright violet/cyan. Slight glow. |
| 9 | `fade_black.png` | 2 × 2 px | `MainMenuManager.fadePanel` / transitions | Solid black. Used as a full-screen fade overlay. 2×2 px is enough. |

---

## A9 — APP ICON (2 files)
**Drop folder:** `Assets/_Project/Art/Sprites/UI/` (iOS) and `Assets/_Project/Art/Sprites/UI/` (Android)
**These are NOT imported as Unity sprites — they go into PlayerSettings**

| # | File Name | Size | Platform | Art Direction |
|---|---|---|---|---|
| 1 | `app_icon_ios.png` | 1024 × 1024 px | iOS — no rounded corners (OS applies) | The AC sigil mark (abstract geometric rune) centred on a radial gradient background: `#231F20` (near black) at edges → `#2d1a56` (deep purple) in centre. Subtle starfield dots. The sigil is white with a violet inner glow. |
| 2 | `app_icon_android_fg.png` | 1024 × 1024 px | Android adaptive icon foreground layer | Sigil mark only on transparent background. Centred with 33% safe zone padding. |
| 3 | `app_icon_android_bg.png` | 1024 × 1024 px | Android adaptive icon background layer | The radial gradient background from the iOS icon, but no sigil. Just the gradient + starfield. |

---

## A10 — PARTICLE TEXTURES (5 files)
**Drop folder:** `Assets/_Project/Art/Particles/`
**Format:** PNG with transparency, small (32–128 px), used as `Texture Sheet` in Unity Particle Systems
**Import:** Auto-applied by SpriteImportPostprocessor — 128px max, ETC2/ASTC compressed

| # | File Name | Size | Usage | Art Direction |
|---|---|---|---|---|
| 1 | `particle_glow_soft.png` | 64 × 64 px | Generic soft glow (used in all realms) | Pure white radial gradient circle. Fully transparent at edges. Fully opaque white at centre. |
| 2 | `particle_spark.png` | 32 × 64 px | Emberforge sparks burst | Elongated white teardrop / comet streak. Bright at head, transparent at tail. |
| 3 | `particle_star.png` | 32 × 32 px | Echo Fields star particles | Sharp 4-point white star. Clean crisp edges. |
| 4 | `particle_petal.png` | 48 × 32 px | Verdant bloom particles (MagicalPlant.bloomParticles) | Soft oval petal. White with slight transparency. Slight curve. |
| 5 | `particle_lantern_glow.png` | 64 × 64 px | Lantern Ascension stardust (LanternRitual.stardustEffect) | Soft amber/warm glow dot. RGB `#FFC87A` colour baked in slightly. |

---

## A11 — STORE SCREENSHOTS (8 files)
**These are NOT in Unity — produced externally from device recordings or mockup tools**
**Drop folder:** `docs/store-screenshots/`

| # | File Name | Size | Content |
|---|---|---|---|
| 1 | `screenshot_01_hero.png` | 1290 × 2796 px | Main menu with realm select visible. Beautiful full-bleed background. App name prominent. |
| 2 | `screenshot_02_emberforge.png` | 1290 × 2796 px | Emberforge scene — sparks floating, player mid-collection. HUD visible showing spark count. |
| 3 | `screenshot_03_verdant.png` | 1290 × 2796 px | Verdant plant at Stage 4 (full bloom) with bloom particles. |
| 4 | `screenshot_04_echofields.png` | 1290 × 2796 px | Echo Fields constellation partially traced, glowing lines between stars. |
| 5 | `screenshot_05_lantern.png` | 1290 × 2796 px | Lantern Ascension — dozens of lanterns floating upward against deep purple sky. |
| 6 | `screenshot_06_sigil.png` | 1290 × 2796 px | Sigil journal screen showing a drawn sigil and its stats. |
| 7 | `screenshot_07_accessibility.png` | 1290 × 2796 px | Side-by-side colorblind mode comparison (normal vs deuteranopia mode). |
| 8 | `feature_graphic.png` | 1024 × 500 px | Google Play feature graphic. Wide format. All 5 realm icons in a row against the Lantern background. App name below. |

---

---

# PART B — AUDIO ASSETS

Total audio assets needed: **34 files** (7 music + 5 ambient + 22 SFX/celebration)

All music →  `Assets/_Project/Audio/Music/`
All ambient → `Assets/_Project/Audio/Ambient/`
All SFX →     `Assets/_Project/Audio/SFX/`
Import settings apply automatically on drop.

---

## B1 — MUSIC TRACKS (7 files)
**Final format:** OGG Vorbis, 192 kbps, 44.1 kHz, Stereo
**Unity Load Type:** Streaming (auto-set by AudioImportPostprocessor)
**Naming convention must match exactly — AudioSourceData.cs looks these up by realm ID**

| # | File Name | Duration | BPM | Key | Peak | Suno/Udio Prompt Keywords |
|---|---|---|---|---|---|---|
| 1 | `emberforge_theme.ogg` | 3:00 seamless loop | 100 | A Major | -6 dB | orchestral, warm forge, tribal wood blocks light timpani, bright harpsichord melody, cello foundation, magical chimes accent, playful creative energy, looping seamless |
| 2 | `verdant_theme.ogg` | 3:30 seamless loop | 80 | D Major | -8 dB | meditative garden, soft acoustic piano, gentle bamboo flute, warm string pads, distant birds wind water, minimal soft mallets, peaceful timeless, loop seamless |
| 3 | `echo_fields_theme.ogg` | 4:00 seamless loop | 75 | F# Minor | -6 dB | ethereal space ambient, floating synth pads, sparse vibraphone bells, wordless distant vocals ahh, heavy reverb, wide stereo, mysterious introspective, seamless loop |
| 4 | `dawn_citadel_theme.ogg` | 3:00 seamless loop | 95 | G Major | -3 dB | triumphant orchestral, soaring strings violin, brass trumpets horns, wordless choir, timpani marching, harp celesta shimmer, inspirational glory, seamless loop |
| 5 | `lantern_ascension_theme.ogg` | 4:30 seamless loop | 60 | A Major | -10 dB | minimalist ambient meditation, single long pad tone, sparse bells every 4-8 beats, wind chimes barely audible, vast reverb space, near silence, transcendent, loop |
| 6 | `main_menu_theme.ogg` | 2:30 seamless loop | 90 | D Major | -6 dB | magical welcoming, blend of five realms, soft bells then warm strings then bright melody, wonder invitation, not intense, seamless loop |
| 7 | `onboarding_theme.ogg` | 1:30 seamless loop | 85 | C Major | -8 dB | gentle glockenspiel piano, warm embracing strings, optional gentle vocal hum, friendly approachable, tutorial tutorial first-time, seamless loop |

---

## B2 — AMBIENT LOOPS (5 files)
**Final format:** OGG Vorbis, 128 kbps, 44.1 kHz, Stereo, exactly 30 seconds
**Unity Load Type:** Streaming (auto-set)
**Volume:** -24 dB to -26 dB — these layer UNDER music. They are texture, not content.
**Critical:** Zero-crossing loop boundary. No click at the 30-second mark. Silence-test the loop.

| # | File Name | Content | Peak |
|---|---|---|---|
| 1 | `emberforge_ambience.ogg` | Crackling fire (steady), occasional pop + spark sound, low-frequency forge machinery hum, faint metallic resonance | -22 dB |
| 2 | `verdant_ambience.ogg` | Bird calls (2–3 species, distant), soft wind through leaves, gentle water trickle, occasional insect chirp | -24 dB |
| 3 | `echo_fields_ambience.ogg` | Subtle ethereal pad tone (barely there), very distant bells, slow air movement, long reverb tail of silence | -26 dB |
| 4 | `dawn_citadel_ambience.ogg` | Distant wordless choir hum (very soft), light wind, occasional crystal bell resonance | -22 dB |
| 5 | `lantern_ambience.ogg` | Soft distant wind, very sparse bells (3–4 strikes in 30 sec), paper/fabric rustle, mostly silence | -26 dB |

---

## B3 — RITUAL SFX / INTERACTION SOUNDS (6 files)
**Final format:** OGG Vorbis, 128 kbps, 44.1 kHz, Mono
**Unity Load Type:** Decompress On Load (auto-set)
**Usage:** Triggered by realm gameplay events

| # | File Name | Duration | Peak | Character | Reference Sound |
|---|---|---|---|---|---|
| 1 | `spark_tap.ogg` | 0.3–0.5 s | -12 dB | Bright magical chime, immediate sharp attack, short decay. High pitch ~880 Hz (A5). | Xylophone single strike. Think Zelda item tap. |
| 2 | `spark_collect.ogg` | 0.6–0.8 s | -10 dB | 3–4 quick ascending chime notes (sparkle cascade). Shimmery, celebratory. Slight stereo spread. | Zelda item collect — magical sparkle rise. |
| 3 | `plant_grow.ogg` | 0.5–0.7 s | -14 dB | Organic plant whoosh with subtle woody "pop" at end. Natural, alive, soft. No electronic elements. | Bamboo creak + soft wind whoosh. |
| 4 | `constellation_connect.ogg` | 0.6–0.9 s | -12 dB | Crystal chime with reverb tail. Ethereal shimmer. Slight ascending tone. Wide stereo with reverb. | Crystal bowl tap with 1 s reverb tail. |
| 5 | `light_refract.ogg` | 0.5–0.7 s | -10 dB | Pure high-frequency crystal resonance. Clear harmonic overtones. Not harsh. Like a crystal wine glass struck lightly. | Crystal bowl / glass harmonica, bright. |
| 6 | `lantern_place.ogg` | 0.4–0.6 s | -16 dB | Very soft gentle settling sound. Almost a sigh. Subtle cloth/paper. No impact. Barely audible — peace. | Soft cloth landing, wind dying down. |

---

## B4 — UI SOUNDS (6 files)
**Final format:** OGG Vorbis, 128 kbps, 44.1 kHz, Mono
**Unity Load Type:** Decompress On Load

| # | File Name | Duration | Peak | Character |
|---|---|---|---|---|
| 1 | `button_click.ogg` | 0.2–0.3 s | -10 dB | Satisfying crisp click. Responsive. Like a soft keyboard key. No buzz or distortion. |
| 2 | `menu_open.ogg` | 0.3–0.5 s | -12 dB | Smooth upward whoosh. No harsh frequencies. Soft swipe/panel slide feel. |
| 3 | `menu_close.ogg` | 0.3–0.5 s | -12 dB | Exact reverse/mirror of `menu_open.ogg`. Downward whoosh. |
| 4 | `achievement_unlock.ogg` | 0.6–0.8 s | -8 dB | Mini-fanfare. 3–4 ascending notes with short swell. Exciting but brief. |
| 5 | `sigil_unlock.ogg` | 0.8–1.0 s | -6 dB | Magical shimmer fanfare. Longer than achievement_unlock. Sparkle burst into chime. Special feeling. |
| 6 | `notification_ping.ogg` | 0.15–0.25 s | -12 dB | Simple clean alert bell/ding. Noticeable but not intrusive. |

---

## B5 — CELEBRATION / REWARD SOUNDS (5 files)
**Final format:** OGG Vorbis, 128 kbps, 44.1 kHz, Stereo (these use wider soundstage)
**Unity Load Type:** Decompress On Load

| # | File Name | Duration | Peak | Character |
|---|---|---|---|---|
| 1 | `ritual_complete_minor.ogg` | 0.8 s | -10 dB | Simple success ding. Short ascending 2-note chime sequence. Understated satisfaction. |
| 2 | `ritual_complete_major.ogg` | 1.2 s | -6 dB | Triumphant 4–5 ascending notes, held top note. Video game victory sting. |
| 3 | `daily_challenge_complete.ogg` | 1.5–2.0 s | -4 dB | Epic victory sting with full swell. Most exciting sound in the game. Orchestral fanfare. |
| 4 | `achievement_secret_unlock.ogg` | 2.0–2.5 s | -6 dB | Build from single mysterious note → dramatic swell → resolution chord. Secret treasure reveal feel. |
| 5 | `level_up.ogg` | 0.6 s | -8 dB | 3-note ascending chime cascade. RPG-style level-up. Quick and bright. |

---

---

# PART C — PRODUCTION WORKFLOW

## C1 — Visual Asset Pipeline

1. **Generate** in Midjourney/DALL-E using the art direction in each row above
2. **Export** as PNG (transparent background where noted)
3. **Resize** to exact pixel dimensions listed using Photoshop/GIMP/Figma
4. **Name** the file exactly as listed — Unity wires by filename
5. **Drop** into the specified folder — import settings apply automatically
6. **Open Unity** and confirm no pink/missing material errors in Scene view

**Colour palette reference (from RealmAtmosphereData assets):**

| Realm | Ambience Key | Ambience Fill | Bloom Intensity |
|---|---|---|---|
| Emberforge | `#FF6114` | `#470F06` | 1.8 |
| Verdant Sanctuary | `#1A9635` | `#0A2E15` | 1.2 |
| Echo Fields | `#3544CC` | `#150C3A` | 1.5 |
| Dawn Citadel | `#FFB833` | `#7A4800` | 2.4 |
| Lantern Ascension | `#9966FF` | `#1F0847` | 1.0 |

## C2 — Audio Asset Pipeline

**Using Suno/Udio:**
1. Use the prompt keywords in column "Suno/Udio Prompt Keywords" from table B1
2. Generate 3–5 variations, pick the best
3. Download as WAV (highest quality available)
4. In Audacity or Adobe Audition: trim, normalise to target peak dB, test loop boundary
5. Export as OGG Vorbis at the kbps listed
6. Name file exactly as listed and drop into `Assets/_Project/Audio/Music/`

**Using freesound.org for UI SFX (fast option):**
- Search: "button click game", "menu swoosh", "achievement fanfare", "notification ding"
- Filter: CC0 license only
- Download WAV → convert to OGG Vorbis 128 kbps mono → drop into `Assets/_Project/Audio/SFX/`

**Using ElevenLabs Sound Effects for ritual SFX:**
- Prompt: "magical crystal chime bell echo" for `constellation_connect.ogg`
- Prompt: "paper lantern gently placed down soft"  for `lantern_place.ogg`
- Prompt: "magical sparkle cascade collect" for `spark_collect.ogg`
- Download WAV → trim → OGG mono 128 kbps

## C3 — Post-Drop Checklist (Unity)

After dropping assets, in Unity Editor run:
```
Menu → Ascendant Continuum → Setup → 🚀 Full Game Setup (Run All)
```
Then open each `RealmData` asset at `Assets/_Project/Resources/RealmData/` and assign:
- `realmIcon` → icon_realm_<name>.png
- `backgroundSprite` → bg_<name>.png
- `ambientMusic` → via `AudioSourceData` asset at `Assets/_Project/Resources/Config/AudioSourceData.asset`

Open `AudioSourceData.asset` and assign every audio clip to its matching slot.

---

# PART D — QUICK REFERENCE COUNT

| Category | Count | Priority |
|---|---|---|
| Realm backgrounds | 5 | 🔴 Critical — needed to see any realm |
| Realm icons (HUD) | 5 | 🔴 Critical — needed for main menu |
| Currency icons (Sigil + Sparks) | 2 | 🔴 Critical — HUD |
| Emberforge sprites | 4 | 🔴 Critical |
| Verdant sprites | 7 | 🔴 Critical |
| Echo Fields sprites | 4 | 🔴 Critical |
| Dawn Citadel sprites | 5 | 🔴 Critical |
| Lantern Ascension sprites | 5 | 🔴 Critical |
| Shared UI sprites | 9 | 🟡 High (needed for menus) |
| App icons | 3 | 🟡 High (needed for store) |
| Particle textures | 5 | 🟢 Medium (game works without, just less visual) |
| Store screenshots | 8 | 🟢 Medium (only for submission) |
| **TOTAL VISUAL** | **62** | |
| Music tracks | 7 | 🔴 Critical |
| Ambient loops | 5 | 🟡 High |
| Ritual SFX | 6 | 🔴 Critical |
| UI SFX | 6 | 🟡 High |
| Celebration SFX | 5 | 🟡 High |
| **TOTAL AUDIO** | **34** | |
| **GRAND TOTAL** | **96 files** | |
