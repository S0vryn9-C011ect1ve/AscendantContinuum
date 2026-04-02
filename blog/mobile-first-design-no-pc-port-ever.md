# Mobile-First Design: No PC Port (Ever)

**Date:** April 2, 2026  
**Theme:** Design Philosophy  
**Tags:** mobile-first, design-constraints, accessibility, no-pc-port, touch-interface

---

## This Game Will Never Come to PC

Not as a port.

Not as a remaster.

**Mobile-first. Mobile-only. Forever.**

---

## Why We're Mobile-Only

### **Constraint Breeds Creativity**

**Traditional game dev:** "Design for PC, then downgrade for mobile."

**Result:**
- Clunky touch controls (virtual joysticks overlaying screen)
- UI too small
- Features cut to "fit" mobile

**Our approach:** "Design for mobile. Ignore PC entirely."

**Result:**
- Touch is PRIMARY interaction (not adapted)
- UI designed for 5-inch screens (not shrunk from 27-inch monitors)
- Features ONLY possible on mobile (GPS, accelerometer, camera, always-with-you)

**No compromises.**

---

## What Mobile-First Actually Means

### **1. Touch is Intentional, Not Adapted**

**Bad mobile design (ported from PC):**

```
[↑] [←] [→] [↓]  (virtual D-pad)
[A] [B]           (virtual buttons)
```

**Feels like:** Playing with mittens on.

**Our design:**

**Sigil Drawing:**
- Swipe gestures (natural to touch)
- Pressure sensitivity (3D Touch / Force Touch)
- Multi-touch (two-finger rituals)

**No virtual buttons. Just your fingers on glass.**

### **2. One-Handed Play

**Most mobile games:** Require landscape mode + both hands.

**Ascendant Continuum:** Designed for one-handed portrait mode.

**Why?**

You're outside. At night. Holding a flashlight in one hand.

**Or:**

You're on a bus. Holding a handrail.

**Or:**

You've got coffee in one hand.

**One hand free = you can still play.**

**Code constraint:**
```csharp
// All UI elements must be reachable within "thumb zone"
// on 6.7" display (iPhone 14 Pro Max)
bool IsReachable(UIElement element) {
    float thumbRadius = 75mm; // Average thumb reach
    return element.position.y < Screen.height * 0.75f;
}
```

**If you can't reach it with your thumb, it gets moved.**

### **3. Screen Size Aware (But Backwards)**

**Traditional responsive design:** Design for desktop, scale down for mobile.

**Our process:**

1. Design for **iPhone SE (4.7" screen)** - smallest modern iPhone
2. Scale UP for larger phones
3. Ignore tablets/iPads (they get phone UI, just bigger)

**Why?**

If it works on a 4.7" screen, it works EVERYWHERE.

**No one gets a worse experience.**

---

## Features Only Possible on Mobile

### **1. GPS-Based Gameplay**

**PC version would require:**
- Manually entering coordinates (tedious)
- Or being stuck at home location (boring)

**Mobile:**
- Always knows where you are
- Rituals adapt to location
- Fossils appear based on proximity

**Can't replicate on PC without feeling forced.**

### **2. Real-Time Weather**

**PC:**
- Looks up weather via ZIP code? (clunky)
- Syncs with desktop weather app? (unreliable)

**Mobile:**
- GPS → weather API → instant accuracy
- Accelerometer detects if you're actually outside
- Camera detects ambient light (sunrise/sunset validation)

**Mobile sensors = seamless integration.**

### **3. Always-With-You Design**

**PC games:** You sit down to play.

**Mobile:** The game is in your pocket.

**Notification:**
```
Meteor shower tonight at 11:47 PM.
Perfect ritual conditions in 2 hours.
```

**You're already outside with friends?** Pull out phone. 2-minute ritual. Done.

**No PC player is lugging a laptop to a park at midnight.**

### **4. Camera Integration**

**Planned feature (Year 2): AR Sigil Overlays**

- Point camera at sky
- See constellation alignments
- Sigils appear overlaid on real stars

**PC with webcam?** Doesn't work. Webcams don't aim at sky.

**Mobile camera?** Perfect. You're ALREADY looking up.

---

## What We Lose (And We're Okay With It)

### **No Keyboard Shortcuts**

PC gamers love hotkeys.

**We don't have them.**

**Trade-off:** Touch gestures are MORE expressive than keyboard combos.

Drawing sigils > pressing "Q-W-E-R" combo.

### **No High-End Graphics**

PC: RTX 4090 can render photorealistic water.

**Mobile:** Limited to what iPhone 12 can handle.

**Our stance:**

Art style > raw polygons.

**We optimize for aesthetic beauty, not graphical fidelity.**

Stylized shaders, particle effects, lighting that works on mid-range phones.

**Beauty through constraint.**

### **No Modding**

PC games: Mod community extends lifespan.

**Mobile:** Walled gardens (App Store / Play Store) = no mods.

**We accept this.**

**Instead:**

- Built-in customization (cosmetics, sigil personalization)
- Community input → official updates
- User-generated content via in-game tools (ritual sharing)

**Controlled ecosystem, but still creative.**

---

## Performance Optimization for Mobile

### **Battery Constraint**

**Problem:** 3D games drain battery.

**Traditional solution:** "Just plug in while playing."

**Our solution:**

**Rituals are SHORT (2-5 minutes).**

You don't need 90 minutes of battery.

**15 minutes of gameplay = 5% battery drain** (target).

**How?**

- Aggressive LOD (level of detail) scaling
- Particle system pooling
- Screen dims during idle moments
- Background processes paused

**Code:**
```csharp
void OnApplicationPause(bool paused) {
    if (paused) {
        // Stop particle systems
        // Reduce physics simulation
        // Pause audio
    }
}
```

**Respect the battery.**

### **Thermal Management**

**Problem:** Phone gets HOT during intense gameplay.

**Solution:**

- Frame rate cap (60 FPS max, not 120)
- Dynamic resolution scaling
- Thermal monitoring (if device > 45°C, reduce FX)

**Code:**
```csharp
void Update() {
    float temp = SystemInfo.deviceTemperature;
    
    if (temp > 45f) {
        QualitySettings.DecreaseLevel(); // Lower graphics
    }
}
```

**Better to play longer at lower settings than crash from overheat.**

### **Download Size**

**Target:** Under 250 MB initial download.

**Why?**

- Cellular download-friendly (no Wi-Fi needed)
- Doesn't eat storage space
- Faster onboarding

**How?**

- Procedural generation (textures, not pre-baked)
- Asset streaming (download realms as you unlock them)
- Compressed audio

**No 50 GB install like AAA games.**

---

## Accessibility Advantages of Mobile

### **Screen Reader Integration (iOS VoiceOver / Android TalkBack)**

**Mobile OS:** Built-in accessibility features.

**PC:** Requires third-party tools (JAWS, NVDA).

**Our implementation:**

Every UI element has VoiceOver labels:

```csharp
accessibilityLabel = "Lunar Bloom sigil. Unlocked. Tap to equip.";
```

**Blind players can navigate entirely by audio.**

### **Haptic Feedback**

**Mobile:** Advanced haptics (Taptic Engine on iPhone).

**PC:** Keyboard has none. Mouse has basic vibration (if gaming mouse).

**Our use:**

- Sigil activation = satisfying "thunk" haptic
- Ritual success = celebratory haptic pulse
- Error = sharp warning haptic

**Tactile feedback enhances immersion.**

### **Colorblind Modes**

**Built into iOS/Android:** System-wide color filters.

**Our support:**

- Design UI to work with all OS colorblind modes
- Test with Deuteranopia, Protanopia, Tritanopia filters
- Icon shapes differ (not just colors)

**Accessible by default, not as afterthought.**

---

## The "But I Want to Play on PC" Response

**We hear you.**

**Counter-offer:**

Use an **Android emulator** (Bluestacks, LDPlayer).

**Limitations:**
- No GPS (stuck at emulator's fake location)
- No accelerometer (can't detect real movement)
- No real-world weather sync
- Loses 80% of game's magic

**Will it work?** Technically, yes.

**Will it feel right?** No.

**And we're okay with that.**

This game is MEANT to be played outside, phone in hand, under real sky.

**Emulating it defeats the purpose.**

---

## What About Tablets?

**iPads / Android tablets:**

**Will it run?** Yes.

**Will it be optimized?** No.

**Experience:**

- Same UI as phone (just scaled up)
- No tablet-specific features
- Works, but not ideal

**Why?**

Tablets aren't "always with you" devices.

You don't carry an iPad to a park at 2 AM.

**Phones are ubiquitous. Tablets aren't.**

---

## Revenue Implications

**PC gamers:** "I'd pay $60 for a PC version!"

**Our math:**

**Option A: PC Port**
- Development cost: $200,000 (porting, QA, Steam integration)
- Potential sales: 10,000 copies @ $30 = $300,000
- Net: $100,000 profit

**Option B: Stay Mobile-Only, Deepen Mobile Features**
- Development cost: $100,000 (AR features, more sigils)
- Improves retention: +15% (keeps players longer)
- In-app cosmetics revenue: +$500,000/year

**Mobile-only is MORE profitable.**

And aligns with our vision.

**Easy choice.**

---

## The Philosophy

**Most indie devs:** "If we succeed on mobile, we'll port to PC!"

**Us:** "If we succeed on mobile, we'll make mobile BETTER."

PC is not an "upgrade."

**It's a different platform with different strengths.**

We chose mobile.

**And we're doubling down.**

---

## Community Reactions

**PC Gamer (Reddit):** "No PC version? Skip."

**Mobile Gamer (Discord):** "FINALLY a game designed for phones, not ported!"

**Our response:**

We're okay losing PC-only gamers.

**We're making the best MOBILE game we can.**

Not a mediocre multi-platform game.

**Focused excellence over broad mediocrity.**

---

## What We Learned From Other Mobile-First Games

### **Monument Valley**

**Lesson:** Touch gestures can be MORE expressive than mouse clicks.

**Applied:** Sigil drawing uses swipes, pressure, multi-touch.

### **Pokémon GO**

**Lesson:** GPS + real-world integration = magic.

**Applied:** Fossils, real astronomy, sunrise/sunset mechanics.

### **Alto's Odyssey**

**Lesson:** Simple one-handed controls = accessible to everyone.

**Applied:** All rituals playable one-handed.

### **Sky: Children of the Light**

**Lesson:** Mobile can be beautiful AND performant.

**Applied:** Stylized art direction, optimized for mid-range phones.

---

## The 5-Year Vision

**2026:** Launch on iOS + Android  
**2027:** Deepen mobile features (AR, more sensors)  
**2028:** Cross-platform social (but still mobile-only gameplay)  
**2029:** Apple Watch companion app (notifications, quick rituals)  
**2030:** VR integration? (Mobile VR like Quest, not PC VR)

**Notice:** No PC port in roadmap.

**We're committed.**

---

## Final Thought

**Every game doesn't need to be on every platform.**

Some games are BETTER when they embrace constraints.

**This is a mobile game.**

Not a PC game you can play on mobile.

**A game designed for the device in your pocket.**

**And that's exactly where it belongs.**

---

**Related Posts:**
- [Real Stargazing: Anti-Screen-Time Mechanic](real-stargazing-anti-screen-time-mechanic.md)
- [Building Accessibility-First: Why Color is Just Texture](building-accessibility-first-gameplay-color-as-discovery.md)
- [One-Handed Combat: Accessibility as Core Design](one-handed-combat-accessibility-as-core-design.md)
