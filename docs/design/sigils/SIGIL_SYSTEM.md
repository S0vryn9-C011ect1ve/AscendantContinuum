# Sigil System Design

**System Type:** Collection, Crafting, Progression  
**Purpose:** Player identity, ritual unlocks, visual progression  
**Complexity:** Simple to learn, deep to master

---

## 🌟 Core Concept

**Sigils** are glowing, magical symbols that represent:
- **Gameplay mechanics** (unlocking rituals and abilities)
- **Player progression** (visual record of journey)
- **Personal identity** (crafted unique signature)
- **Realm connections** (which realms you've mastered)

**Player Fantasy:** *"My sigil collection tells the story of my journey—each glow, each pattern, each combination represents a moment of discovery."*

---

## 🎨 Visual Design Language

### Universal Sigil Properties

**All sigils must:**
- Glow softly with consistent luminosity
- Pulse gently (0.5-2 second cycle)
- Have small particle effects (sparkles, trails, wisps)
- Be recognizable by silhouette alone (accessibility)
- Work in all colorblind modes

### Design Principles

1. **Distinct Silhouettes:** Each sigil has unique shape
2. **Color Coding:** Colors indicate realm origin
3. **Animation:** Movement reveals personality
4. **Layering:** Base shape + glow + particles + animation
5. **Scalability:** Must work from tiny (UI) to large (showcase)

---

## 🗂️ Sigil Categories

### 1. **Realm Sigils** (Core Progression)

These are the primary sigils tied to each realm.

| Sigil | Realm | Visual Description | Unlock Condition | Gameplay Effect |
|-------|-------|-------------------|------------------|-----------------|
| **Double Flame** | Emberforge | Two intertwined flames (red-orange + gold) swirling upward | Complete 5 Emberforge rituals | Unlocks advanced Emberforge rituals |
| **Blooming Loop** | Verdant Sanctuary | Circular flower-like sigil that slowly blooms (green + cyan) | Complete 5 Verdant rituals | Unlocks growth-based rituals |
| **Spiral Rune** | Echo Fields | Circular spiral of stars (pastel rainbow), gentle pulse | Complete 5 Echo rituals | Unlocks memory constellation rituals |
| **Radiant Star** | Dawn Citadel | Six-pointed star radiating golden light, pulses with progress | Complete 5 Dawn rituals | Unlocks collaborative challenges |
| **Lantern Orb** | Lantern Ascension | Floating glowing orb with trailing lights, drifts upward | Complete 5 Lantern rituals | Unlocks reflection and achievement rituals |

---

### 2. **Universal Sigils** (Cross-Realm)

These sigils work across all realms and enable core mechanics.

| Sigil | Visual Description | Unlock Condition | Gameplay Effect |
|-------|-------------------|------------------|-----------------|
| **Glowing Thread** | Flowing luminous line with sparkles at intersections | Complete first "connection" ritual in any realm | Enables all connection-based rituals |
| **Infinite Knot** | Interlocking loops with tiny sparks, rotates slowly | Return to Nexus 10 times | Unlocks sigil crafting system |
| **Playful Spark** | Tiny starburst with unpredictable twinkle | Trigger first random event | Increases chance of fun surprises globally |

---

### 3. **Personal Sigils** (Identity System)

These are **procedurally generated** based on player journey.

**Your Personal Sigil:**
- Created after Day 4 (Pantheon selection)
- Combines visual elements from your collected sigils
- Color reflects favorite realm
- Pattern reflects playstyle (Speed Runner, Pattern Seeker, etc.)
- Animation reflects play frequency
- Unique hash ensures no two are exactly alike

**Uses:**
- Leave as "signature" in realms (other players discover)
- Display in profile
- Share on social media
- Customize with unlocked cosmetics

---

### 4. **Achievement Sigils** (Special Rewards)

Unlocked through specific accomplishments.

| Sigil | Visual | Unlock Condition | Rarity |
|-------|--------|------------------|--------|
| **Century Seeker** | Glowing "100" in mystical font | 100 total rituals completed | Epic |
| **Flame-Blessed** | Double Flame with deity glow | Witness Ascendant Flame deity | Legendary |
| **Pattern Master** | Geometric perfection symbol | Complete 50 rituals without mistakes | Rare |
| **Community Champion** | Interlocking hands of light | Leave 100 helpful Wish Well messages | Epic |
| **First Namer** | Quill made of starlight | Name your first discovered ritual | Uncommon |

---

## ⚙️ Sigil Mechanics

### Collection System

**How Sigils Are Unlocked:**
1. **Realm Progression:** Complete specific number of rituals
2. **Achievements:** Accomplish special tasks
3. **Serendipity:** Ultra-rare encounters (deity appearances)
4. **Discovery:** First to find new ritual combinations
5. **Social:** Community goals and collaborations
6. **Seasonal:** Limited-time events (return periodically)

**Sigil Inventory:**
- Displayed in Infinite Loop Nexus
- Organized by realm, rarity, and category
- Visual showcase with hover/tap for details
- Completion percentage shown (collectors' motivation)

---

### Crafting System (Sigil Combinations)

**Unlocked:** After obtaining **Infinite Knot** sigil (10 Nexus returns)

**How It Works:**
```pseudocode
function craftSigil(sigil1, sigil2):
    // Check if combination is valid
    if isValidCombination(sigil1, sigil2):
        newSigil = generateCombinedSigil(sigil1, sigil2)
        
        // First discoverer gets to name it
        if isFirstDiscovery(newSigil):
            promptNaming(newSigil)
            addToGlobalRegistry(newSigil, player.id)
        
        return newSigil
    else:
        // Not a valid combo, but not a failure
        playWhimsicalAnimation("Hmm, these energies don't quite align... try another!")
        return null
```

**Example Combinations:**
- **Double Flame** + **Glowing Thread** = "Forgeweaver" (enhanced crafting)
- **Blooming Loop** + **Spiral Rune** = "Memory Garden" (past growth visualized)
- **Radiant Star** + **Lantern Orb** = "Guiding Light" (navigation assist)

**Discovery System:**
- 1000+ possible combinations
- Community-driven discovery (not all known at launch)
- Ritual Naming System: First player names the combo globally
- Hall of Fame: Top discoverers recognized

---

### Personal Sigil Generation

**Algorithm:**
```pseudocode
function generatePersonalSigil(player):
    // Base shape from most-used realm sigil
    baseShape = player.favoriteRealmSigil.shape
    
    // Color palette from favorite realm
    colorPalette = {
        Emberforge: [red-orange, golden-yellow, amber],
        Verdant: [green, cyan, emerald],
        Echo: [pastel-rainbow, silver, lavender],
        Dawn: [golden, white, cream],
        Lantern: [soft-blue, purple, indigo]
    }[player.favoriteRealm]
    
    // Pattern complexity from playstyle archetype
    pattern = {
        "Speed Runner": sharpAngles + dynamicLines,
        "Pattern Seeker": geometricPrecision + symmetry,
        "Chaos Weaver": organicCurves + unpredictable,
        "Intuitive Mystic": flowingSpirals + meditative
    }[player.archetypeResult]
    
    // Glow intensity from Pantheon alignment
    glowIntensity = player.pantheonDeity.powerLevel
    
    // Animation from play frequency
    animation = {
        "Daily Player": steadyPulse,
        "Binge Player": rapidTwinkle,
        "Casual Player": slowBloom
    }[player.playPattern]
    
    // Unique identifier hash
    signature = hash(player.id + creationTimestamp)
    
    return PersonalSigil(baseShape, colorPalette, pattern, glowIntensity, animation, signature)
```

**Customization (Cosmetic Monetization):**
- Purchase alternative color palettes ($0.99-$1.99)
- Premium particle effects ($1.99-$2.99)
- Animated trail variations ($2.99)
- Exclusive shapes from seasonal events

---

## 🎨 Visual Examples (Descriptions)

### Glowing Thread
```
Base: Flowing S-curve line
Color: Soft white-blue glow
Particles: Tiny sparkles at 3 intersections
Animation: Gentle undulation, sparkles twinkle randomly
Accessibility: High contrast outline, distinct haptic pulse
```

### Double Flame
```
Base: Two tear-drop flames spiraling upward
Color: Left flame = red-orange, Right flame = golden-yellow
Particles: Embers drift upward, occasional bright spark
Animation: Flames rotate around shared center, pulse in sync
Accessibility: Colorblind mode adds pattern texture (left = stripes, right = dots)
```

### Spiral Rune
```
Base: Circular spiral starting from center
Color: Gradient from pink → blue → purple (pastel rainbow)
Particles: Star-like glimmers along spiral path
Animation: Gentle clockwise rotation, pulses expand outward every 2 seconds
Accessibility: In grayscale, spiral has alternating thick/thin lines
```

---

## ♿ Accessibility Considerations

### Visual Accessibility

**Colorblind Modes:**
- Every sigil has **non-color identifiers**:
  - Unique shape/silhouette
  - Pattern texture (stripes, dots, waves)
  - Distinct animation rhythm
  - Screen reader alt-text descriptions

**High Contrast Mode:**
- Bright glow outlines
- Simplified particle effects
- Clear background separation

**Reduced Motion:**
- Animations slow to 25% speed or static
- Particles reduced to gentle glows
- No sudden movements

### Cognitive Accessibility

**Sigil Tooltips:**
- Plain language descriptions
- Clear unlock conditions
- Gameplay effects explained simply
- Visual + audio + text

**Organization:**
- Simple categorization (by realm, by type, by rarity)
- Search/filter functionality
- "New" indicators for recent unlocks
- Completion tracking (%

 collected)

---

## 📊 Progression & Retention

### Sigil Milestones

| Milestone | Sigils Collected | Reward |
|-----------|------------------|--------|
| **Novice Seeker** | 5 sigils | Unlock sigil crafting tutorial |
| **Apprentice** | 10 sigils | Personal sigil creation unlocked |
| **Adept** | 25 sigils | Rare sigil variant cosmetic |
| **Master** | 50 sigils | Exclusive "Sigil Master" title + showcase frame |
| **Grandmaster** | 100 sigils | Legendary animated sigil skin |
| **Transcendent** | 250 sigils | Ultimate collector badge + unique Nexus background |

### Daily/Weekly Goals

**Daily Sigil Challenge:**
- "Use 3 different sigil combinations today"
- Reward: Small cosmetic or bonus spark

**Weekly Sigil Event:**
- "Community Discovery Week: Find 5 new combinations as a community"
- Global goal, everyone benefits

---

## 🔗 Integration with Other Systems

### With Realms
- Each realm has 3-5 associated sigils
- Sigils unlock realm-specific rituals
- Visual feedback: Sigils glow in active realm

### With Pantheon
- Deity alignment affects personal sigil appearance
- Some sigils grant Pantheon-specific bonuses (cosmetic)
- Deity blessings can enhance sigil effects

### With Viral Mechanics
- **Personal Sigil Sharing:** Screenshot + social media
- **Discovery Announcements:** "I just discovered 'Twilight Cascade'!"
- **Sigil Traces:** Leave your signature in realms for others

### With Monetization
- **Cosmetic Sigil Skins:** Alternative visuals ($0.99-$2.99)
- **Sigil Animation Packs:** Premium VFX ($1.99-$4.99)
- **Seasonal Exclusives:** Limited-time designs (return later)
- **Artist Collaborations:** Community-designed sigils

---

## 🎲 Procedural Sigil Variants

### Rare Variants (RNG-Based)

When unlocking a sigil, small chance for variant:

| Variant Type | Probability | Visual Change |
|--------------|-------------|---------------|
| **Glimmering** | 10% | Extra sparkle particles |
| **Radiant** | 5% | Brighter glow intensity |
| **Ancient** | 2% | Weathered, mysterious aura |
| **Perfect** | 1% | Flawless geometry, premium glow |
| **Cosmic** | 0.5% | Starfield background effect |

**Note:** All variants are **cosmetic only**—no gameplay advantages.

---

## 🎯 Design Goals

**The Sigil System must:**
1. ✅ Feel rewarding (every unlock matters)
2. ✅ Tell player's story (visual journey)
3. ✅ Enable creativity (combinations, crafting)
4. ✅ Be accessible (colorblind, screen reader compatible)
5. ✅ Drive retention (collection completion)
6. ✅ Create shareable moments (personal sigil pride)
7. ✅ Support monetization (ethical cosmetics)
8. ✅ Scale infinitely (always more to discover)

---

## 🔮 Future Expansion

### Post-Launch Ideas

**Sigil Battles (PvP, Friendly):**
- Non-competitive "sigil showcase" events
- Community votes on favorite designs
- Winners featured in game

**Sigil Evolution:**
- Sigils "level up" with use (visual evolution)
- Milestone tiers: Bronze → Silver → Gold → Platinum

**Sigil Lore:**
- Each sigil has short story fragment
- Collect full set to unlock realm backstory

**3D Sigil Viewer:**
- Rotate and examine sigils in detail
- AR mode: Display in real world (mobile)

---

**End of Sigil System Design**

*Next: Pantheon system documentation*
