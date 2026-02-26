# 🎨 Polish → Art → Audio Roadmap

**Created:** February 25, 2026  
**Status:** Implementation Guide for Game Feel, Visuals, and Sound

---

## 📋 **PHASE 1: POLISH (Improve Game Feel & Responsiveness)**

### 1.1 **Enhanced Transition Animations**

**What's Already Built:**
- ✅ Fade in/out on realm transitions
- ✅ Respects reduced motion mode
- ✅ Particle effects during transition

**What to Add:**

```csharp
// File: Assets/_Project/Scripts/VFX/TransitionEffects.cs (NEW)
using UnityEngine;
using System.Collections;

public class TransitionEffects : MonoBehaviour
{
    // Enhanced camera zoom effect during transition
    public static IEnumerator CameraZoomTransition(Camera cam, float targetFOV, float duration)
    {
        float startFOV = cam.fieldOfView;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, 
                EaseInOutQuad(elapsed / duration));
            yield return null;
        }
        cam.fieldOfView = targetFOV;
    }
    
    // Screen shake on realm entry (dramatic impact!)
    public static IEnumerator ScreenShake(Camera cam, float duration, float intensity)
    {
        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float shake = Random.Range(-intensity, intensity);
            cam.transform.localPosition = originalPos + 
                new Vector3(shake, shake * 0.5f, 0);
            yield return null;
        }
        cam.transform.localPosition = originalPos;
    }
    
    // Easing function for smooth animations
    private static float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }
}
```

**Implementation Checklist:**
- [ ] Add camera zoom effect to `RealmTransitionManager.cs`
- [ ] Add subtle screen shake on first realm entry
- [ ] Add swirl/portal effect (use ParticleManager)
- [ ] Ensure all effects respect reduced motion mode

---

### 1.2 **Achievement/Reward Celebration Animations**

**Current Status:** `AchievementDisplayManager.cs` has hooks but animations are incomplete

**Improvements:**

```csharp
// File: Assets/_Project/Scripts/UI/AchievementCelebration.cs (NEW)
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class AchievementCelebration : MonoBehaviour
{
    public static void CelebrateSigilUnlock(Image sigilImage, TextMeshProUGUI titleText)
    {
        StartCoroutine(SigilUnlockSequence(sigilImage, titleText));
    }
    
    private static IEnumerator SigilUnlockSequence(Image sigilImage, TextMeshProUGUI titleText)
    {
        // Scale up from tiny
        sigilImage.transform.localScale = Vector3.one * 0.1f;
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            sigilImage.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 1.2f, t / 0.5f);
            yield return null;
        }
        
        // Bounce back to normal
        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            sigilImage.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, t / 0.3f);
            yield return null;
        }
        
        // Spin celebration
        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
            sigilImage.transform.Rotate(0, 0, 360 * Time.deltaTime);
            yield return null;
        }
        
        // Glow pulse 3x
        CanvasGroup cg = sigilImage.GetComponent<CanvasGroup>();
        if (cg == null) cg = sigilImage.gameObject.AddComponent<CanvasGroup>();
        
        for (int i = 0; i < 3; i++)
        {
            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                cg.alpha = Mathf.Lerp(1f, 0.6f, t / 0.3f);
                yield return null;
            }
            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                cg.alpha = Mathf.Lerp(0.6f, 1f, t / 0.3f);
                yield return null;
            }
        }
    }
}
```

**Checklist:**
- [ ] Implement sigil pop-in celebration on first unlock
- [ ] Add confetti particle burst (use existing ParticleManager)
- [ ] Play success chime sound + haptic feedback
- [ ] Add text pop animation ("✓ New Sigil Unlocked!")
- [ ] Color flash effect on achievement unlock
- [ ] Respect reduced motion - skip animations, show static effect

---

### 1.3 **UI Responsiveness & Juice**

**Improvements to Add:**

```csharp
// File: Assets/_Project/Scripts/UI/ButtonJuice.cs (NEW)
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonJuice : MonoBehaviour
{
    [SerializeField] private float scaleOnPress = 0.95f;
    [SerializeField] private float scaleOnHover = 1.05f;
    [SerializeField] private float animDuration = 0.1f;

    private Button button;
    private Vector3 originalScale;

    private void Start()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;

        if (button != null)
        {
            button.targetGraphic?.RegisterDirtyLayoutsForComponent();
        }
    }

    public void OnHoverEnter()
    {
        if (AccessibilityManager.Instance?.ReducedMotionEnabled == true)
            transform.localScale = originalScale * scaleOnHover;
        else
            StartCoroutine(ScaleTo(originalScale * scaleOnHover, animDuration));
    }

    public void OnHoverExit()
    {
        if (AccessibilityManager.Instance?.ReducedMotionEnabled == true)
            transform.localScale = originalScale;
        else
            StartCoroutine(ScaleTo(originalScale, animDuration));
    }

    public void OnPress()
    {
        if (AccessibilityManager.Instance?.ReducedMotionEnabled == true)
            transform.localScale = originalScale * scaleOnPress;
        else
            StartCoroutine(ScaleTo(originalScale * scaleOnPress, animDuration * 0.5f));
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
```

**Checklist:**
- [ ] Add hover/press scale animations to all buttons
- [ ] Add color tint on button hover
- [ ] Add satisfying click haptic feedback
- [ ] Text scaling animation on UI open
- [ ] Smooth menu transitions (slide in from edges)
- [ ] Swipe animation on daily challenge reveal

---

### 1.4 **Ritual Feedback Improvements**

**When Player Completes Ritual:**

```csharp
// Enhancements to ritual completion flow:
// 1. Screen flash (white/realm color)
// 2. Camera shake
// 3. Particle burst at tap location
// 4. Score/XP popup animation
// 5. Sigil unlock notification (if applicable)
// 6. Music swells/victory sting
// 7. Haptic triple-tap celebration pattern
```

**Checklist:**
- [ ] Implement ritual completion celebration sequence
- [ ] Score popup float animation ("+100 XP" rises + fades)
- [ ] Combo counter visual feedback (if applicable)
- [ ] Particle trails on successful taps
- [ ] Add to `RealmController.cs` base class

---

### 1.5 **Performance & Responsiveness Tuning**

**Checklist:**
- [ ] Frame rate monitoring (target 60 FPS on mobile)
- [ ] Touch input latency optimization (< 50ms tap-to-response)
- [ ] Particle effect LOD system (fewer particles on low-end devices)
- [ ] Audio callback optimizations
- [ ] Memory profile in reduced-motion mode (disable heavy effects)

---

## 🎨 **PHASE 2: ART ASSETS (Visual Identity)**

### 2.1 **Sprite Sheet Specifications**

#### **Realm Backgrounds (Priority 1)**

| Realm | Dimensions | Layers | Colors | Details |
|-------|-----------|--------|--------|---------|
| **Emberforge** | 2048x1536 | 4 parallax layers | Red-orange, gold | Floating forge platforms, spinning gears of light, ember particles |
| **Verdant Sanctuary** | 2048x1536 | 4 parallax layers | Green, cyan | Growing vines, blooming flowers, meditation pools, gentle mist |
| **Echo Fields** | 2048x1536 | 4 parallax layers | Pastel rainbow | Floating stars, memory orbs, constellations, ethereal glow |
| **Dawn Citadel** | 2048x1536 | 4 parallax layers | Golden, white | Floating platforms, radiant structures, light beams, crystalline |
| **Lantern Ascension** | 2048x1536 | 3 parallax layers | Soft pastels | Floating lanterns, void with distant lights, liminal space, peaceful |

**Parallax Layer Breakdown (per realm):**
- **Layer 1:** Extreme background (stars/void/sky)
- **Layer 2:** Far background (distant structures)
- **Layer 3:** Mid background (interactive elements)
- **Layer 4:** Foreground (closest interactive objects)

---

#### **Sigil Sprites (Priority 2)**

**Base Realm Sigils:**

```
Assets/Sprites/Sigils/Realm/
├── 01_DoubleFame.png         (Red-orange, 512x512)
├── 02_BloomingLoop.png       (Green-cyan, 512x512)
├── 03_SpiralRune.png         (Pastel rainbow, 512x512)
├── 04_RadiantStar.png        (Golden, 512x512)
└── 05_LanternOrb.png         (Soft white-blue, 512x512)
```

**Universal Sigils:**

```
Assets/Sprites/Sigils/Universal/
├── 06_GlowingThread.png      (White-cyan, 256x256)
├── 07_InfiniteKnot.png       (Multi-color, 256x256)
└── 08_PlayfulSpark.png       (Rainbow burst, 128x128)
```

**Personal Sigils (Procedural):**
- Generated at runtime using `SigilGenerator.cs`
- 5 base shapes: Triangle, Hexagon, Circle, Square, Star
- 4 pattern overlays: Sharp, Balanced, Flowing, Calm
- Color combinations: 12+ per realm
- Output: 512x512 PNG texture (runtime-rendered)

---

#### **Particle Textures (Priority 3)**

```
Assets/Textures/Particles/
├── spark_small.png           (32x32, glow)
├── spark_large.png           (64x64, glow)
├── flame_burst.png           (128x128, radial gradient)
├── plant_leaf.png            (64x64, leaf shape)
├── light_orb.png             (128x128, radial glow)
├── star_twinkle.png          (32x32, star)
├── mist_cloud.png            (256x256, soft cloud)
├── dust_particle.png         (16x16, tiny dot)
└── lantern_glow.png          (128x128, warm glow)
```

**Technical Specs:**
- Format: PNG with alpha transparency
- Color space: sRGB
- Unity compression: ASTC (mobile), DXT5 (desktop)
- Target: < 500 KB per texture sheet

---

#### **UI Icon Pack (Priority 4)**

```
Assets/Sprites/UI/
├── Icons/
│   ├── menu_play.png         (64x64)
│   ├── menu_settings.png     (64x64)
│   ├── menu_achievements.png (64x64)
│   ├── menu_sigils.png       (64x64)
│   ├── icon_accessibility.png(64x64)
│   └── icon_daily_challenge.png (64x64)
├── Buttons/
│   ├── btn_play_default.png
│   ├── btn_play_hover.png
│   └── btn_play_pressed.png
└── Panels/
    ├── panel_corner_left.png
    ├── panel_corner_right.png
    └── panel_center.png
```

---

#### **NPC Character Sprites (Priority 5)**

```
Assets/Sprites/NPCs/
├── Sparkus_Emberforge/
│   ├── sparkus_default.png   (256x256)
│   ├── sparkus_happy.png     (256x256)
│   ├── sparkus_excited.png   (256x256)
│   └── sparkus_thinking.png  (256x256)
├── Petalina_Verdant/
│   ├── petalina_default.png  (256x256)
│   ├── petalina_blooming.png (256x256)
│   └── petalina_peaceful.png (256x256)
└── [Similar for other realm NPCs]
```

---

### 2.2 **Animation Sprites (If Needed)**

**For rituals that need frame animation:**

```
Assets/Sprites/Animations/
├── spark_collection/
│   ├── spark_1.png
│   ├── spark_2.png
│   └── spark_3.png
├── plant_growth/
│   ├── plant_small.png
│   ├── plant_medium.png
│   └── plant_large.png
└── flame_flicker/
    ├── flame_1.png
    ├── flame_2.png
    └── flame_3.png
```

---

### 2.3 **Color Palette Reference**

**Emberforge:**
- Primary: #FF6633 (red-orange)
- Secondary: #FFAA00 (gold)
- Accent: #33CCFF (teal spark)
- Dark: #330000 (shadow)

**Verdant Sanctuary:**
- Primary: #33CC66 (green)
- Secondary: #00FFAA (cyan)
- Accent: #FFFFFF (light)
- Dark: #003333 (deep shadow)

**Echo Fields:**
- Primary: #CC99FF (lavender)
- Secondary: #FF99FF (pink)
- Tertiary: #FFCC99 (peach)
- Dark: #330033 (void)

**Dawn Citadel:**
- Primary: #FFDD00 (golden)
- Secondary: #FFFFFF (white)
- Accent: #FFAA00 (warm gold)
- Dark: #333300 (shadow)

**Lantern Ascension:**
- Primary: #FFCCCC (soft pink)
- Secondary: #CCFFFF (soft cyan)
- Tertiary: #FFFFCC (soft yellow)
- Dark: #000000 (void)

---

## 🎵 **PHASE 3: AUDIO ASSETS (Sound & Music)**

### 3.1 **Music Tracks (Composition Spec)**

#### **Realm Ambient Themes**

| Track | Duration | BPM | Mood | Instrumentation |
|-------|----------|-----|------|-----------------|
| **Emberforge Theme** | 3:00 loop | 100 | Energetic, warm, playful | Strings, bells, percussion, warm pads |
| **Verdant Sanctuary Theme** | 3:30 loop | 80 | Calm, organic, grounded | Piano, flute, strings, nature sounds |
| **Echo Fields Theme** | 4:00 loop | 75 | Ethereal, mysterious, introspective | Synth pads, bells, reverb, atmospheric |
| **Dawn Citadel Theme** | 3:00 loop | 95 | Triumphant, bright, hopeful | Strings, choir, brass, inspiring |
| **Lantern Ascension Theme** | 4:30 loop | 60 | Meditative, peaceful, reflective | Ambient pads, soft strings, minimalist |
| **Main Menu Theme** | 2:30 loop | 90 | Welcoming, magical, curious | All realms' instruments combined |
| **Onboarding Theme** | 1:30 loop | 85 | Gentle, educational, warm | Soft strings, gentle bells, welcoming |

**Technical Specs:**
- Format: WAV (uncompressed) → OGG Vorbis (compressed)
- Channels: Stereo
- Sample rate: 44.1 kHz (or 48 kHz)
- Loop points: Seamless (no clicks)
- File size target: 1-2 MB per track (after compression)

---

### 3.2 **Sound Effects (SFX Specification)**

#### **Ritual Interaction Sounds**

```
Assets/Audio/SFX/Rituals/
├── spark_tap.ogg              (100ms, uplifting chime)
├── spark_collect.ogg          (200ms, sparkly success)
├── plant_grow.ogg             (150ms, organic bloom)
├── constellation_connect.ogg  (250ms, magical whoosh)
├── light_refract.ogg          (180ms, ethereal shimmer)
└── lantern_place.ogg          (200ms, gentle placement)
```

**Characteristics:**
- **spark_tap:** Bright, high-pitched bell (0.5 second)
- **spark_collect:** Layered sparkles, ascending pitch (0.8 second)
- **plant_grow:** Organic whoosh, nature-inspired (0.6 second)
- **constellation_connect:** Ethereal shimmer, magical (0.8 second)
- **light_refract:** Crystal-like, resonant (0.5 second)
- **lantern_place:** Soft, peaceful placement (0.4 second)

#### **UI Interaction Sounds**

```
Assets/Audio/SFX/UI/
├── button_click.ogg           (80ms, satisfying click)
├── menu_open.ogg              (200ms, transition whoosh)
├── menu_close.ogg             (200ms, reverse whoosh)
├── achievement_unlock.ogg     (400ms, celebratory fanfare)
├── sigil_unlock.ogg           (500ms, magical shimmer burst)
└── notification_ping.ogg      (150ms, gentle alert)
```

#### **Ambient/Environmental Sounds**

```
Assets/Audio/SFX/Ambient/
├── emberforge_ambience.ogg    (30 sec loop, crackling fire)
├── verdant_ambience.ogg       (30 sec loop, birds, wind, water)
├── echo_ambience.ogg          (30 sec loop, ethereal reverb)
├── dawn_ambience.ogg          (30 sec loop, distant bells)
└── lantern_ambience.ogg       (30 sec loop, soft wind, distant bells)
```

#### **Celebration/Success Sounds**

```
Assets/Audio/SFX/Rewards/
├── ritual_complete_minor.ogg  (0.8s, simple ding)
├── ritual_complete_major.ogg  (1.2s, triumphant fanfare)
├── daily_challenge_complete.ogg (1.5s, epic victory sting)
├── achievement_secret_unlock.ogg (2.0s, mysterious reveal)
└── level_up.ogg               (0.6s, ascending chime)
```

**Technical Specs (All SFX):**
- Format: WAV (source) → OGG Vorbis (game)
- Channels: Mono (unless spatial needed)
- Sample rate: 44.1 kHz
- Bit depth: 16-bit
- File size: < 200 KB per effect
- No clicks/pops on loop boundaries

---

### 3.3 **Dynamic Music Implementation**

**Music System Architecture:**

```csharp
// Pseudo-code for dynamic music layering
public class DynamicMusicManager : MonoBehaviour
{
    // Each realm theme has multiple layers (stems)
    // - Base layer: Always plays
    // - Combat layer: Plays during intense rituals
    // - Victory layer: Plays on completion
    
    // AudioSource layers
    private AudioSource baseLayer;
    private AudioSource intensityLayer;
    private AudioSource victoryLayer;
    
    // Crossfade between layers based on game state
    public void SetRitualIntensity(float intensity)
    {
        // intensity: 0 = calm, 0.5 = medium, 1 = intense
        baseLayer.volume = 1f - intensity;
        intensityLayer.volume = intensity;
    }
}
```

**Music Transitions:**
- Crossfade duration: 2-3 seconds
- Respects reduced motion mode (snap instead of fade)
- Each realm has unique crossfade signature

---

### 3.4 **Audio Implementation Checklist**

**Setup:**
- [ ] Create `Assets/Audio/Music/` folder structure
- [ ] Create `Assets/Audio/SFX/` folder structure
- [ ] Import all audio files as appropriate types:
  - Music: AudioClip, streaming (to save RAM)
  - SFX: AudioClip, load in memory (< 5 sec duration)
  - Ambient: AudioClip, streaming

**Integration:**
- [ ] Wire up music tracks to realm data (`RealmData.cs`)
- [ ] Connect SFX to particle bursts and ritual completions
- [ ] Implement crossfade system in `AudioManager.cs`
- [ ] Test volume levels (target: -10dB master)
- [ ] Implement accessibility: audio subtitles UI

**Testing:**
- [ ] Test on mobile (Bluetooth speaker + headphones)
- [ ] Test ambient audio blend (not too loud)
- [ ] Test music transitions (smooth, not jarring)
- [ ] Test SFX layering (multiple sounds simultaneously)
- [ ] Ensure hearing-impaired accessible (haptics correlate to audio)

---

## 🎯 **IMPLEMENTATION TIMELINE**

### **Week 1: Polish (2-3 days of dev time)**
- Implement transition effects (zoom, shake, swirl)
- Add achievement celebration sequences
- Enhance UI button responsiveness
- Performance tuning passes

### **Week 2: Art Asset Creation (7-10 days)**
- Commission or create realm backgrounds (parallax)
- Create sigil sprite sheet
- Design and render particle textures
- Create NPC character sprites
- Design and render UI icon pack

### **Week 3: Audio Production (5-7 days)**
- Compose 7 music tracks
- Record/design 20+ SFX
- Master all audio assets
- Implement dynamic music system
- Test audio mixing

### **Final: Integration & Testing (2-3 days)**
- Import all assets into Unity
- Wire up animations to code
- Playtesting and feedback loop
- Performance profiling
- Mobile device testing

---

## 📊 **Quality Checklist**

### Polish Quality Gates
- [ ] All animations 60 FPS minimum (tested on 2-year-old device)
- [ ] Touch input lag < 50ms
- [ ] Celebration animations feel rewarding (user feedback)
- [ ] Reduced motion mode working (no smooth animations shown)
- [ ] Haptic feedback correlates to visual/audio events

### Art Quality Gates
- [ ] All sprites anti-aliased (no pixelation)
- [ ] Consistent art style across all 5 realms
- [ ] Sprites scale cleanly (tested at 1x, 2x, 3x scale)
- [ ] Colorblind modes tested (all readable)
- [ ] Backgrounds don't distract from gameplay

### Audio Quality Gates
- [ ] Music loops seamlessly (no clicks)
- [ ] SFX levels balanced (no single sound too loud)
- [ ] Music crossfades smooth (2-3 second duration)
- [ ] Audio tested on multiple speakers (mobile + headphones)
- [ ] Hearing-impaired feedback: haptics match audio cues

---

## 💾 **Asset Organization**

Final project structure:
```
Assets/
├── _Project/
│   ├── Sprites/
│   │   ├── Realms/
│   │   ├── Sigils/
│   │   ├── Particles/
│   │   ├── UI/
│   │   ├── NPCs/
│   │   └── Animations/
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Ambient/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Prefabs/
│   ├── Materials/
│   └── Shaders/
```

---

## 🔗 **Next Steps**

1. **Start Polish Phase** (Today - 2 days)
   - Implement transition effects & celebration animations
   - Run performance profiling
   - Gather team feedback

2. **Commission Art Assets** (Days 3-10)
   - Hire freelance 2D artist(s)
   - Provide detailed specs from this doc
   - Iterative feedback cycles

3. **Compose Music & SFX** (Days 8-15)
   - Hire composer/sound designer
   - Create music stems + SFX library
   - Implement dynamic music system

4. **Final Integration & Test** (Days 15-18)
   - Import all assets
   - Playtesting pass
   - Mobile device testing
   - Submit to app stores!

---

**This roadmap turns your game from "playable" to "polished & beautiful" 🎉**

*For questions on specific implementations, refer to the technical architecture docs.*
