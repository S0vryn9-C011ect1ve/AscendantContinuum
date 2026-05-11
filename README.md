# ✨ The Ascendant Continuum

**A mindful cosmic adventure — traverse celestial realms, grow cosmic flora, and align with the stars.**

## Canonical Documentation (Wave 1)

This repository is under active documentation consolidation.

- Project and build readiness: `docs/BUILD_READINESS_STATUS.md`
- Environment and CI/CD setup: `docs/technical/GITHUB_SETUP.md`
- Security and supply chain: `docs/security/SUPPLY_CHAIN_PROTECTION.md`
- Documentation standards: `docs/standards/repository-structure.md`, `docs/standards/naming-conventions.md`, `docs/standards/asset-management.md`
- Legacy docs migration tracker: `docs/operations/legacy-doc-consolidation-wave-tracker.md`

> 🚀 **Release Candidate 1 (Updated March 3, 2026).** WebGL is live at [ascendant-continuum.web.app](https://ascendant-continuum.web.app); Android verification is pending local Unity Android module installation.

![Version](https://img.shields.io/badge/version-1.0.0--rc-goldenrod)
![Accessibility](https://img.shields.io/badge/accessibility-first-green)
![Performance](https://img.shields.io/badge/performance-60fps-blue)
![License](https://img.shields.io/badge/license-Proprietary-red)

---

## 🌟 What Is This?

The Ascendant Continuum is an endlessly replayable universe where players explore vivid magical realms, perform playful rituals, collect glowing sigils, and discover secrets in a world that evolves with their choices.

**Key Features:**
- ✨ **5 Magical Realms** with unique rules, mysteries, and dynamic atmospheres.
- 🔍 **REVOLUTIONARY Hidden Mysteries** (NEVER DONE BEFORE):
  - Accessibility modes unlock DIFFERENT secrets (e.g., Protanopia reveals hidden runes in flames).
  - Past players become discoverable fossils.
  - NPCs remember what ALL players tell them.
  - Community mysteries take months to solve.
- 🎯 **Daily Constellation Challenge** connecting a global community.
- ♿ **Accessibility-First Design** as core innovation (Colorblind modes, Reduced Motion, Screen Reader support).
- 🎨 **Personal Sigil Creation** for unique player identity.
- 🎭 **Ethical Monetization** (cosmetics only, no pay-to-win, no FOMO).
- 🌙 **Celestial Sync** integrating real moon phases and eclipses.
- 📱 **1-5 Minute Sessions** respecting player time with a built-in "Digital Sunset" to encourage healthy play habits.

---

## 🎮 Core Philosophy

- **Positive Empowerment:** No punishments, only joyful discovery
- **Accessibility as Innovation:** Different modes unlock different secrets
- **Viral by Design:** Built-in shareable moments
- **Anti-Grind:** Meaningful short sessions, no manipulation
- **Living Community:** Async social without forced interaction

---

## 🚀 Commercial Launch Readiness

We have just completed our final polish pass to ensure a flawless player experience:
- **Silky Smooth Performance:** Dynamic resolution scaling guarantees a locked 60 FPS on all devices.
- **Zero Interruptions:** Robust offline fallbacks and 5-second network timeouts mean the game never hangs.
- **Seamless Onboarding:** An interactive, contextual tutorial guides you through your first steps in the Emberforge.
- **Privacy First:** Full GDPR & CCPA compliance with transparent data controls.
- **Stay Connected:** Gentle, local push notifications remind you when your daily challenges and personal sigils are ready.

---

## 🏗️ Project Status

**Current Phase:** Release Candidate 1 — Build Stabilization & Launch Prep

**Infrastructure — Complete:**
- ✅ Unity 6000.3.9f1, IL2CPP, URP 17.3.0
- ✅ All 5 realm scenes (Emberforge, Verdant, Echo Fields, Dawn Citadel, Lantern Ascension)
- ✅ WebGL deployed live → https://ascendant-continuum.web.app/play/
- ⚠️ Android build — blocked locally until Unity Android Build Support is installed
- ✅ Firebase Hosting, Firestore, Storage — live with robust offline support
- ✅ GitHub Actions CI/CD — auto-deploys on every push to `main`
- ✅ EditMode test compilation blockers resolved (Object ambiguity + dominant direction angle conversion)
- ✅ Full accessibility suite (colorblind, reduced motion, dyslexia, ADHD, haptics)
- ✅ 42 achievements, daily challenges, live cosmic events
- ✅ Cross-player Wish Wall, Guardian push notifications
- ✅ Contextual FTUE (First-Time User Experience)
- ✅ Object Pooling & Dynamic Performance Scaling

**Coming Up:**
- 📣 Public Launch Announcement
- 📱 App Store & Google Play submissions
- 🌐 Website launch page with email capture

---

## 📁 Project Structure

```
D:\1-Ascendant Continuum Game\
├── Assets/              # Unity runtime/editor project data
├── Packages/            # Unity package manifest and lock data
├── ProjectSettings/     # Unity project configuration
├── firebase/            # Hosting, rules, and Cloud Functions
├── scripts/             # Automation and validation scripts
├── docs/                # Canonical project documentation
└── .github/workflows/   # CI/CD and scheduled automation
```

See `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration status and legacy mapping.

---

## 🌐 Live URLs

| | URL |
|---|---|
| **Website** | https://ascendant-continuum.web.app |
| **Play (WebGL)** | https://ascendant-continuum.web.app/play/ |
| **Firebase Console** | https://console.firebase.google.com/project/ascendant-continuum |

## 🚀 Dev Quick Start

```powershell
git clone https://github.com/ascendantcontinuum/AscendantContinuum.git
cd AscendantContinuum
# Open in Unity Hub — Unity 6000.3.9f1 required
# WebGL + Android + iOS build modules required
```

CI/CD is fully automated. Push to `main` → builds all platforms → deploys to Firebase.

---

## 🎯 Development Roadmap

### ✅ Phase 1–3: Foundation → Implementation → Beta (Complete)
- All 5 realms implemented
- Full CI/CD pipeline live
- WebGL deployed to Firebase Hosting
- Android build passing
- All core game systems implemented (69 scripts)

### 🎯 Phase 4: Soft Beta → Public Launch (Current)
- Internal testing & bug fixing
- iOS build stabilisation
- App Store / Google Play submission
- Website public launch with waitlist
- Marketing push

---

## 💰 Business Model

**Goal:** Generate passive income → reinvest in Ascendant Continuum + 3mpwr App

**Revenue Streams:**
1. Cosmetic items (sigil skins, avatar customization)
2. Seasonal battle passes (cosmetic rewards only)
3. Future realm expansions
4. Artist collaborations

**Ethical Principles:**
- ❌ No pay-to-win
- ❌ No loot boxes
- ❌ No FOMO exploitation
- ✅ Transparent pricing
- ✅ Spending limits available
- ✅ Free path to all gameplay content

---

## ♿ Accessibility Commitment

We build accessibility-first, not as an afterthought:

- **Visual:** Colorblind modes, high contrast, scalable text, screen readers
- **Motor:** Multiple input methods (touch, voice, switch, keyboard, controller)
- **Cognitive:** No timers, clear language, pause anywhere, adjustable complexity
- **Auditory:** Subtitles, haptics, visual alternatives for sound cues

Different accessibility modes unlock different secrets—it's part of the gameplay innovation.

---

## 🤝 Contributing

This is currently a proprietary project. Contributions are limited to the development team.

---

## 📄 License

Proprietary - All Rights Reserved

---

## 📧 Contact

**Project:** https://ascendant-continuum.web.app
**GitHub:** https://github.com/ascendantcontinuum/AscendantContinuum

---

## 🔮 Vision

*A universe that celebrates curiosity, creativity, and joy—offering endless surprises and empowerment in every session. Through accessibility-first design and ethical practices, we prove games can succeed while uplifting everyone who plays them.*

---

**The Ascendant Continuum** | *Explore • Create • Discover*
