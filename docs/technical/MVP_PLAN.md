# MVP Implementation Plan - The Ascendant Continuum

**Version:** 1.0  
**Last Updated:** February 1, 2026  
**Target:** 3-Month Development Timeline

---

## 🎯 MVP Definition

### What's In MVP (Minimum Viable Product)

**Core Experience:**
- ✅ 2 Realms: Emberforge + Verdant Sanctuary
- ✅ Daily Constellation Challenge (Wordle-style viral mechanic)
- ✅ Infinite Loop Nexus (hub world)
- ✅ Basic Sigil System (collection, no crafting yet)
- ✅ Core accessibility features (colorblind, screen reader, reduced motion)
- ✅ Anonymous authentication + cloud save
- ✅ 3-day onboarding flow (simplified from 7-day)
- ✅ iOS + Android builds

**What's NOT in MVP (Post-Launch):**
- ❌ Echo Fields, Dawn Citadel, Lantern Ascension (Realms 3-5)
- ❌ Pantheon System (deities)
- ❌ Sigil crafting
- ❌ Ritual Replay video generation
- ❌ Personal Sigil creation
- ❌ Advanced social features
- ❌ PC/Console versions
- ❌ In-app purchases (monetization comes later)

---

## 📅 3-Month Timeline

### Month 1: Foundation (Weeks 1-4)

#### Week 1: Project Setup
**Goals:**
- Unity project initialization
- Firebase project creation
- Version control setup
- Team roles defined

**Tasks:**
1. Create Unity 2022.3 LTS project
2. Import Firebase SDK
3. Configure build settings (iOS/Android)
4. Set up Git repository
5. Create initial scenes (Nexus, Emberforge, Menu)
6. Implement GameManager singleton
7. Basic input system setup

**Deliverables:**
- ✅ Playable empty scenes
- ✅ Firebase authentication working
- ✅ Build pipeline functional

---

#### Week 2: Nexus Hub + Navigation
**Goals:**
- Build Infinite Loop Nexus
- Implement scene transitions
- Create UI framework

**Tasks:**
1. Model/design Nexus environment
2. Implement portal system to realms
3. Create main menu UI
4. Build accessibility settings menu
5. Implement scene loading with fade transitions
6. Add background music crossfade
7. Create tutorial popup system

**Deliverables:**
- ✅ Functional Nexus hub
- ✅ Smooth realm transitions
- ✅ Settings menu with basic options

---

#### Week 3: Emberforge Realm - Part 1
**Goals:**
- Build core Emberforge visuals
- Implement first ritual mechanics

**Tasks:**
1. Create Emberforge environment (fire, sparks, embers)
2. Implement "Ignite Spark" ritual (tap to light)
3. Add particle effects (flames, sparks)
4. Create Sparkus NPC (basic dialogue)
5. Implement progression tracking
6. Add ritual completion UI
7. Build reward system (sigils)

**Deliverables:**
- ✅ Playable Emberforge realm
- ✅ 1 complete ritual type
- ✅ Visual feedback and effects

---

#### Week 4: Emberforge Realm - Part 2
**Goals:**
- Complete Emberforge rituals
- Add procedural generation

**Tasks:**
1. Implement "Thread Light" ritual (swipe patterns)
2. Add "Animate Stars" ritual (sequencing)
3. Procedural spark placement (Voronoi)
4. Ritual variation based on daily seed
5. Serendipity system (1% deity appearance placeholder)
6. Audio implementation (SFX, music)
7. Performance optimization

**Deliverables:**
- ✅ Fully functional Emberforge
- ✅ 3 ritual types with variations
- ✅ Procedural generation working

---

### Month 2: Core Features (Weeks 5-8)

#### Week 5: Verdant Sanctuary Realm
**Goals:**
- Build second realm
- Diversify gameplay

**Tasks:**
1. Create Verdant Sanctuary environment (nature, plants)
2. Implement "Grow Garden" ritual (tap/hold)
3. Add "Match Leaf Patterns" ritual (memory game)
4. Create Petalina NPC
5. Implement meditation pools
6. Add nature audio/music
7. Test realm transitions

**Deliverables:**
- ✅ Playable Verdant Sanctuary
- ✅ 2 ritual types (contrasting Emberforge)
- ✅ Smooth cross-realm experience

---

#### Week 6: Daily Constellation Challenge
**Goals:**
- Implement viral mechanic #1
- Backend integration

**Tasks:**
1. Build constellation UI (star placement)
2. Implement pattern matching logic
3. Create share card generation (image)
4. Connect to Firebase Cloud Functions
5. Fetch daily challenge at startup
6. Submit completion to backend
7. Display global completion count

**Deliverables:**
- ✅ Functional daily challenge
- ✅ Shareable results (image)
- ✅ Backend integration complete

---

#### Week 7: Accessibility Implementation
**Goals:**
- Implement core accessibility features
- WCAG 2.1 AA compliance

**Tasks:**
1. Colorblind shader implementation (3 modes)
2. Screen reader integration (iOS VoiceOver, Android TalkBack)
3. Reduced motion mode
4. Adjustable text size
5. Haptic feedback system
6. Voice navigation (basic)
7. Accessibility testing with real users

**Deliverables:**
- ✅ 3 colorblind modes functional
- ✅ Screen reader support working
- ✅ All rituals accessible

---

#### Week 8: Sigil System
**Goals:**
- Implement collection mechanics
- Create progression loop

**Tasks:**
1. Sigil database (Firestore)
2. Collection UI (sigil gallery)
3. Unlock logic (realm-specific)
4. Visual sigil designs (2D art)
5. Sigil display in rituals
6. Progress tracking
7. Achievement notifications

**Deliverables:**
- ✅ Sigil collection working
- ✅ 10+ unique sigils
- ✅ Visual feedback on unlock

---

### Month 3: Polish & Launch (Weeks 9-12)

#### Week 9: Onboarding Flow
**Goals:**
- Create smooth first-time experience
- Ensure player retention

**Tasks:**
1. Welcome screen + account creation
2. Tutorial for first ritual (Emberforge)
3. Accessibility setup prompt (Day 1)
4. Introduction to Daily Constellation (Day 2)
5. Realm unlock progression (Day 3)
6. In-game tooltips and hints
7. Skip tutorial option

**Deliverables:**
- ✅ Complete 3-day onboarding
- ✅ Tutorial skippable
- ✅ Accessibility prioritized

---

#### Week 10: Backend & Analytics
**Goals:**
- Finalize backend integration
- Set up analytics

**Tasks:**
1. Cloud save implementation
2. User profile system
3. Streak tracking
4. Firebase Analytics integration
5. Custom event tracking
6. Crash reporting (Crashlytics)
7. Remote Config setup

**Deliverables:**
- ✅ Cloud saves working
- ✅ Analytics tracking all KPIs
- ✅ Crash reporting live

---

#### Week 11: Polish & Bug Fixing
**Goals:**
- Refine experience
- Fix all critical bugs

**Tasks:**
1. Visual polish (VFX, animations)
2. Audio mixing and mastering
3. Performance optimization (60 FPS target)
4. UI/UX improvements
5. Bug fixing sprint
6. Load time optimization
7. Battery usage optimization (mobile)

**Deliverables:**
- ✅ Stable build, no critical bugs
- ✅ 60 FPS on target devices
- ✅ Polished visuals and audio

---

#### Week 12: Testing & Launch Prep
**Goals:**
- Final testing
- Prepare for app store submission

**Tasks:**
1. Internal QA testing
2. Beta testing (TestFlight, Google Play Beta)
3. Accessibility audit
4. Privacy policy + terms of service
5. App store assets (screenshots, description)
6. Submit to App Store & Google Play
7. Marketing materials (trailer, website)

**Deliverables:**
- ✅ App submitted to stores
- ✅ Marketing site live
- ✅ Ready for public launch

---

## 👥 Team Structure (Recommended)

### Minimum Viable Team (3-5 people)

**1. Lead Developer / Unity Engineer**
- Core gameplay programming
- Firebase integration
- Performance optimization
- 40-60 hours/week

**2. UI/UX Designer + 2D Artist**
- Interface design
- Sigil artwork
- Accessibility design
- Marketing assets
- 30-40 hours/week

**3. 3D Artist / Environment Designer**
- Realm environments
- Particle effects
- Animation
- 30-40 hours/week

**4. Sound Designer / Composer** (Contract OK)
- Music composition (5 realms + menu)
- SFX creation
- Audio implementation
- 20-30 hours (can be contracted)

**5. QA Tester / Accessibility Specialist** (Part-time)
- Testing on multiple devices
- Accessibility validation
- Bug reporting
- 20 hours/week

---

## 💰 Budget Estimate (MVP)

### Development Costs (3 months)

| Role | Rate | Hours/Week | Duration | Total |
|------|------|-----------|----------|-------|
| Lead Developer | $75/hr | 50 | 12 weeks | $45,000 |
| UI/UX Designer | $60/hr | 35 | 12 weeks | $25,200 |
| 3D Artist | $60/hr | 35 | 12 weeks | $25,200 |
| Sound Designer | $50/hr | 25 | 12 weeks | $15,000 |
| QA Tester | $35/hr | 20 | 8 weeks | $5,600 |
| **Subtotal** | | | | **$116,000** |

### Infrastructure & Tools

| Item | Cost | Notes |
|------|------|-------|
| Unity Pro (2 seats) | $400/month × 3 | $1,200 |
| Firebase (Blaze Plan) | $50/month × 3 | $150 |
| Apple Developer | $99/year | $99 |
| Google Play Developer | $25 one-time | $25 |
| Asset Store / Plugins | $500 | One-time |
| Testing Devices | $2,000 | iOS + Android devices |
| **Subtotal** | | **$3,974** |

### **TOTAL MVP BUDGET: ~$120,000**

*(Note: Can be reduced significantly with solo/small team, equity-only contributors, or open-source tools)*

---

## 🛠️ Technical Milestones

### Milestone 1: Vertical Slice (Week 4)
**Definition:** Emberforge fully playable end-to-end
- Player can complete 1 ritual
- Earn 1 sigil
- Return to Nexus
- Save progress to cloud

**Success Criteria:**
- ✅ 1 realm 100% complete
- ✅ Core loop functional
- ✅ No critical bugs

---

### Milestone 2: Feature Complete (Week 8)
**Definition:** All MVP features implemented
- 2 realms playable
- Daily Constellation working
- Accessibility features functional
- Sigil collection working

**Success Criteria:**
- ✅ All features from MVP scope done
- ✅ Playable but may have bugs
- ✅ Ready for polish phase

---

### Milestone 3: Alpha Build (Week 10)
**Definition:** Internal testing ready
- All features complete and tested
- Backend fully integrated
- Analytics tracking

**Success Criteria:**
- ✅ No game-breaking bugs
- ✅ Ready for internal QA
- ✅ Performance targets met (50+ FPS)

---

### Milestone 4: Beta Build (Week 11)
**Definition:** External testing ready
- All critical bugs fixed
- Polished experience
- App store assets ready

**Success Criteria:**
- ✅ Stable build
- ✅ Beta testers can play without crashes
- ✅ Accessibility validated

---

### Milestone 5: Release Candidate (Week 12)
**Definition:** Ready for app store submission
- Final testing complete
- Legal/privacy compliance
- Marketing materials ready

**Success Criteria:**
- ✅ App submitted to stores
- ✅ Zero critical bugs
- ✅ WCAG 2.1 AA compliant

---

## 📊 Success Metrics (First Month Post-Launch)

### Retention Goals
- **D1 Retention:** 40%+
- **D7 Retention:** 20%+
- **D30 Retention:** 10%+

### Engagement Goals
- **Daily Active Users (DAU):** 1,000+
- **Daily Constellation Participation:** 30%+
- **Average Session Length:** 5+ minutes
- **Rituals per Session:** 2+

### Viral Goals
- **Share Rate:** 10%+ share Daily Constellation results
- **Viral Coefficient:** 1.1+ (each user brings 1.1 new users)
- **App Store Rating:** 4.5+ stars

### Accessibility Goals
- **Accessibility Feature Usage:** 25%+ enable at least one
- **Screen Reader Users:** 5%+ of player base
- **Colorblind Mode Usage:** 10%+

---

## 🚀 Launch Strategy

### Soft Launch (Week 12)
**Target:** Limited geographic release
- Release in 2-3 smaller markets (e.g., Canada, Australia)
- Monitor metrics for 1-2 weeks
- Fix critical issues
- Gather feedback

### Global Launch (Week 14)
**Target:** Worldwide release
- Full App Store + Google Play release
- Press outreach (gaming media, accessibility advocates)
- Social media campaign
- Reddit AMA (r/gaming, r/accessibility)
- ProductHunt launch

---

## 🔄 Post-MVP Roadmap

### Version 1.1 (Month 4)
- Add Echo Fields realm
- Implement Ritual Replay sharing
- Bug fixes and performance improvements

### Version 1.2 (Month 5)
- Add Dawn Citadel realm
- Implement Pantheon System (6 deities)
- Add sigil crafting

### Version 1.3 (Month 6)
- Add Lantern Ascension realm
- Implement Personal Sigil creation
- Launch monetization (ethical cosmetics)

### Version 2.0 (Month 9)
- PC/Console versions
- Advanced social features
- Seasonal events

---

## ⚠️ Risk Mitigation

### Technical Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| Firebase costs exceed budget | High | Medium | Implement caching, optimize queries, monitor usage |
| Performance issues on older devices | High | Medium | Target devices 2 years old+, aggressive optimization |
| Procedural generation bugs | Medium | High | Extensive testing, deterministic seeds |
| Screen reader integration fails | High | Low | Test early, budget extra time |

### Business Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| Low user retention | High | Medium | Focus on onboarding, A/B test features |
| Poor viral coefficient | Medium | Medium | Iterate on share mechanics, incentivize sharing |
| Negative reviews | High | Low | Beta test thoroughly, respond to feedback |
| Monetization fails | Medium | Medium | Build engaged audience first, ethical IAP only |

### Schedule Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| Scope creep | High | High | Strict MVP definition, say "no" to features |
| Team member unavailable | Medium | Medium | Cross-train, document everything |
| Bug fixing takes longer than expected | Medium | High | Buffer time in Week 11, cut non-essential features |

---

## ✅ Definition of Done (DoD)

### For Each Feature:
- ✅ Code written and peer-reviewed
- ✅ Unit tests passing (where applicable)
- ✅ Tested on iOS and Android
- ✅ Accessibility validated
- ✅ No critical bugs
- ✅ Performance acceptable (60 FPS)
- ✅ Documented in code comments

### For Each Realm:
- ✅ All rituals functional
- ✅ NPCs implemented
- ✅ Audio/music integrated
- ✅ Accessibility features working
- ✅ Procedural generation tested
- ✅ Transitions smooth
- ✅ Playtested internally

### For MVP Launch:
- ✅ All MVP features complete
- ✅ Zero critical bugs
- ✅ Privacy policy live
- ✅ App stores approved
- ✅ Analytics tracking
- ✅ Marketing site live
- ✅ WCAG 2.1 AA compliant

---

## 📞 Communication Plan

### Daily Standups (15 min)
- What did you do yesterday?
- What are you doing today?
- Any blockers?

### Weekly Reviews (1 hour)
- Demo completed work
- Review metrics (if launched)
- Adjust priorities

### Sprint Planning (2 hours, bi-weekly)
- Plan next 2 weeks
- Assign tasks
- Clarify requirements

---

**End of MVP Implementation Plan**

*Ready to build The Ascendant Continuum!*  
*Contact: ascendantcontinuum@gmail.com*
