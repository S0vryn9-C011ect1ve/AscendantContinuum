# Mistplay Submission & Featuring Strategy
## The Ascendant Continuum — Complete Playbook

---

## PART 1: What Mistplay Is and Why It Matters

Mistplay is a mobile loyalty platform (Android only, 30M+ users) where players
earn real gift card rewards by playing games. For a developer, Mistplay is:

- **A UA channel** — players discover your game through Mistplay's app
- **A retention booster** — Mistplay rewards incentivise return sessions
- **A monetisation moat** — players who came through Mistplay tend to stick longer
- **A featuring opportunity** — top-performing games get featured placement

Games that perform well on Mistplay consistently see **20-40% better Day 30 retention**
than organic installs because the reward loop reinforces your in-game loop.

---

## PART 2: Mistplay Requirements Checklist

### Hard Requirements (must be done before applying)
- [ ] Game is published on **Google Play Store** (not sideloaded)
- [ ] Minimum Android SDK 21, Target SDK 34+
- [ ] `MistplayManager.cs` integrated, `MISTPLAY_APP_ID` set
- [ ] `MistSDK.aar` placed in `Assets/Plugins/Android/`
- [ ] App size ≥ 30 MB installed
- [ ] Genuine real-time gameplay (not idle/clicker)
- [ ] No bots, no session faking
- [ ] Privacy Policy URL live and linked in Play Console

### Highly Recommended (for featuring)
- [ ] Day 1 retention ≥ 35%
- [ ] Day 7 retention ≥ 15%
- [ ] Average session length ≥ 8 minutes
- [ ] Google Play rating ≥ 4.3 stars before applying
- [ ] At least 500 organic reviews

### SDK Integration Steps

```bash
# 1. Get the Mistplay Unity SDK
# Apply at: https://developer.mistplay.com
# You'll receive: MistSDK.aar + integration docs

# 2. Drop the .aar into your project
cp MistSDK.aar "d:\1-Ascendant Continuum Game\Assets\Plugins\Android\MistSDK.aar"

# 3. Set App ID in MistplayManager.cs Inspector field
#    OR in Assets/Plugins/Android/AndroidManifest.xml:
```

```xml
<!-- Add inside <application> tag in AndroidManifest.xml -->
<meta-data
    android:name="com.mistplay.appId"
    android:value="YOUR_APP_ID_HERE" />
```

### How to Apply
1. Go to **https://developer.mistplay.com**
2. Sign up as a developer (free)
3. Submit game details + Play Store URL
4. Mistplay reviews within 1-3 business days
5. Once approved, you receive your App ID
6. Replace `"YOUR_MISTPLAY_APP_ID"` in `MistplayManager.cs`
7. Publish an update with the SDK integrated
8. Mistplay validates the integration (1-2 days)
9. Game goes live on Mistplay platform

---

## PART 3: Google Play Store Listing (Copy)

### App Title (50 chars max)
```
The Ascendant Continuum: Cosmic Realms
```

### Short Description (80 chars max)
```
5 mystical realms. Your unique cosmic sigil. Real astronomical events. Zero ads.
```

### Full Description (4,000 chars max)

```
WHAT IF A GAME KNEW WHEN TO LET YOU GO?

The Ascendant Continuum is a meditative exploration game built around a radical
idea: the most meaningful play happens when a game respects your time as much as
you do.

★ 5 HAND-CRAFTED COSMIC REALMS
Each realm is a different kind of meditation:
• Emberforge — collect drifting sparks, feel the rhythm of fire and breath
• Verdant Garden — nurture magical plants that bloom in real time
• Echo Fields — trace real constellations (Orion, Cassiopeia, Cygnus) in a living sky
• Dawn Citadel — rotate prisms to decode ancient light puzzles
• Lantern Ascension — release wish-lanterns into a sky shared with players worldwide

★ YOUR COSMIC FINGERPRINT — UNIQUE TO YOU
Every choice you make generates your living Cosmic Sigil: a symbol that no other
player will ever have. The time of day you play, the realms you love, whether you
play outside — all of it shapes a sigil that evolves for as long as you play.

You also receive a Cosmic Name ("Ember-dawn Wanderer of the Still Veil") and an
Aura Tier that other players see floating beside your wishes in the Lantern Sky.

★ REAL ASTRONOMICAL EVENTS — LIVE IN-GAME
The game's calendar is the real sky. When the Perseid meteor shower peaks,
Echo Fields blazes with extra constellations. During the Spring Equinox, the
Verdant Garden blooms twice as fast. Lunar eclipses turn Dawn Citadel's prisms
blood-red.

These moments happen once. Miss the 2026 Blood Moon (March 3) and it's gone.
Players who attend earn exclusive titles no one else will ever have.

★ THE DIGITAL SUNSET PROMISE
The game gently closes at local sunset. Every day. This isn't a bug. It's the
whole point. We believe a game that knows when to stop is more trustworthy — and
more enjoyable — than one designed to never let you leave.

★ THE WISH WALL — CONNECT WITHOUT COMPETING
Instead of leaderboards and PvP, we built the Wish Wall: an asynchronous layer
where your lanterns drift into other players' skies, where Time Capsules you
plant today are discovered by strangers months from now, where community Puzzle
Chains require thousands of players to complete during live events.

No names. No winners. Just the quiet sense that you are not playing alone.

★ TOUCH GRASS REWARDS
The game uses optional GPS to detect when you are in a park or natural area.
Play in nature and every spark you collect doubles. Walk 30 minutes outside?
Your sigil permanently shifts to reflect your connection to the real world.

★ ACCESSIBILITY FIRST — ALWAYS
• Full colorblind support (5 modes), with hidden secrets exclusive to each mode
• Reduced motion throughout
• Haptic guidance for every interaction
• Spatial audio descriptions
• Secret achievements unlocked BY using accessibility features (not despite them)

★ ZERO ADS. ZERO ENERGY TIMERS. ZERO LOOT BOXES.
The Ascendant Continuum has no in-app purchases and no ads. It will always be free.
We believe the game itself is enough.

Supported languages: English (more coming)
Internet connection: Optional (full offline play supported)
Storage: ~85 MB
```

---

## PART 4: Keywords & ASO Strategy

### Primary Keywords (high intent)
- meditation game android
- relaxing mobile game no ads
- constellation game
- mindfulness game
- cosmic exploration game

### Secondary Keywords (long-tail)
- astronomy game mobile
- daily challenge mobile game
- accessibility mobile game
- aesthetic mobile game 2026
- game with real world events

### Tags for Play Console
```
meditation, relaxing, space, astronomy, exploration, accessibility, mindfulness,
daily challenge, constellation, puzzle, no ads
```

---

## PART 5: Press & Featuring Angles

These are the story hooks that tech and gaming press respond to:

### Angle 1: "The Game That Closes At Sunset"
**Target:** The Verge, Kotaku, Vice, Mashable
**Pitch:** A mobile game intentionally closes every day at local sunset. The developer
argues that a game with limits is more trustworthy. The feature is called "Digital Sunset."

### Angle 2: "Your Game Character Is Literally You"
**Target:** Wired, Fast Company, game design press
**Pitch:** No character creation. Instead, the game tracks when you play, where you play,
how you play — and generates a Cosmic Fingerprint that evolves for years. No two are alike.

### Angle 3: "The Game Tied To Real Astronomical Events"
**Target:** Space.com, astronomy blogs, TIME
**Pitch:** The game's calendar IS the sky. When there's a real lunar eclipse, the game
changes. Players who don't show up on March 3, 2026 will never see blood-red prisms again.

### Angle 4: "Playing Outside Makes You Better"
**Target:** Outdoors publications, wellness press, parenting blogs
**Pitch:** GPS detects when you're in a park. In-game rewards double. The game literally
incentivises going outside. In an era of screen addiction, this is the opposite design.

### Angle 5: "Accessibility AS Gameplay"
**Target:** Accessibility / disability gaming press, AbleGamers, Polygon
**Pitch:** Most games add accessibility as an afterthought. This game has secret achievements
ONLY unlockable through accessibility features. Colorblind mode reveals hidden levels.
Haptics guide you to hidden items non-haptic players can't find.

---

## PART 6: Pre-Launch Checklist for Mistplay & Featuring

### 3 Months Before Launch
- [ ] Set up Firebase Analytics with full funnel (install → tutorial_complete → realm_first → D7)
- [ ] Configure Firebase Crashlytics
- [ ] Set up Google Play Console: store listing, screenshots, trailer
- [ ] Beta test with 50+ players via Google Play Internal Testing
- [ ] Achieve Day 1 ≥ 35%, Day 7 ≥ 15% in beta

### 1 Month Before Launch
- [ ] Press kit ready (see `docs/PRESS_KIT.md`)
- [ ] Trailer: 30-second + 2-minute versions
- [ ] Screenshots: phone (1080×1920), tablet (1200×1920), feature graphic (1024×500)
- [ ] Apply to Mistplay developer portal
- [ ] Apply for Google Play featuring via Indie Games Accelerator
- [ ] Submit to App Annie / data.ai for tracking

### Launch Week
- [ ] Activate Mistplay integration (SDK in build)
- [ ] Post to r/androidgaming, r/indiegaming, r/Unity3D
- [ ] Contact 10 mobile gaming YouTubers with keys
- [ ] Run soft launch in CA + AU before global (standard practice)

### Post-Launch (for Mistplay featuring)
- [ ] Maintain ≥ 4.3 stars — respond to every 1-2 star review
- [ ] Release update within 30 days of launch (signals active dev)
- [ ] Hit 1,000 MAU before requesting Mistplay featuring upgrade
- [ ] Provide Mistplay with D1/D7/D30 retention data from Firebase

---

## PART 7: Monetisation Path (Zero-Friction, Player-First)

The Ascendant Continuum ships free with no ads. Future optional monetisation:

| Option | Philosophy |
|--------|-----------|
| Cosmetic Sigil skins | Player expression, never affects gameplay |
| "Patron of the Realms" tip jar | One-time, no content locked |
| Cosmic Archive (extended time capsules) | Pay to store more, never to play |

**What we will NEVER add:**
- Energy timers
- Loot boxes
- Pay-to-win anything
- Ads of any kind
- Subscription for core content

This stance IS the marketing. It generates trust and word-of-mouth that paid UA cannot buy.

---

## PART 8: Mistplay Engagement Optimisation

Mistplay rewards players by the minute — so session length and return frequency
matter more than monetisation. Our design is already aligned:

| Mistplay Metric | Our System That Drives It |
|----------------|--------------------------|
| Session start  | Daily challenge + streak + event countdown |
| Session length | Meditation mode (5-30 min), constellation tracing |
| Return rate    | Guardian Messenger push notifications |
| Long-term retention | Cosmic Identity evolution (never stops) |
| Social sharing | Wish Wall, Cosmic Name shareable card |

### Optimal Session Loop (Mistplay sweet spot: 8-20 minutes)
1. Open app → Guardian greets you with your Cosmic Name
2. Check live event status + daily challenge
3. Visit primary realm (5-10 min)
4. Release a lantern / capsule (2 min)
5. Check Wish Wall for new messages (1 min)
6. App closes at sunset if near evening

Average projected session: **12-18 minutes** — ideal for Mistplay crediting.
