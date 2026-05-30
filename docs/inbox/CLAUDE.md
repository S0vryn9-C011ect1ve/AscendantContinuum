# Ascendant Continuum - AI Development Guide

**Project Type:** Unity 6 Mobile Game (C# + Firebase)  
**Status:** Pre-Beta Development  
**Updated:** May 29, 2026

---

## 🎯 Core Mission

Build a sustainable, ethical mobile game that validates product-market fit with 50 beta testers before scaling. **Traction over features. Data over assumptions.**

---

## 🧠 Coding Workflow Principles

### 1. Plan Mode First
- Write specs before code
- Break complex tasks into 3-5 step plans
- Validate approach with user before implementing
- Use todo lists for multi-step work

### 2. Verify Relentlessly
- Run builds after every C# change
- Check Firebase costs weekly (target: < $5/month)
- Test on Android device, not just Editor
- Validate player feedback against assumptions

### 3. Keep It Simple
- Prefer 100 lines over 1000
- Delete unused systems ruthlessly
- Ask: "Does this help get 50 beta testers?"
- If answer is no, don't build it

### 4. Surgical Edits Only
- Change only what's necessary
- Don't refactor unrelated code
- Minimize side effects and churn
- Use multi_replace for batch edits

### 5. Goal-Driven Execution
- Every task has clear success criteria
- Define failure conditions upfront
- Use Builder-Validator Pattern: build → test → fix → repeat
- Don't stop until goal is met or pivot is justified

### 6. Parallelize with Subagents
- Use subagents for research, exploration, complex analysis
- Keep main agent focused on implementation
- Offload "find all X" tasks to explore_subagent

---

## 🏗️ Tech Stack Context

### Unity/C# Layer
- **Engine:** Unity 6000.3.9f1 with IL2CPP
- **Rendering:** URP 17.3.0 (mobile-optimized)
- **Scripts:** 137 runtime + 24 test files
- **Key Managers:** GameBootstrapper spawns 30+ singleton managers
- **Namespaces:** `AscendantContinuum.Core|Systems|UI|Platform|Realms`

### Firebase Backend
- **Firestore:** Player data, challenges, submissions, waitlist
- **Storage:** Replays (10MB max), sigils (2MB max)
- **Rules:** Rate limits to prevent cost spikes (< $5/month target)
- **No Analytics:** Zero tracking by design (ethical positioning)

### Build System
- **Platforms:** WebGL (demo), Android (primary), iOS (future)
- **CI/CD:** GitHub Actions auto-deploys WebGL on main branch push
- **Current blocker:** Android/iOS build modules not installed locally

### Monetization
- **IAP:** Stubbed out (needs Unity IAP package installation)
- **Prices:** $0.99-$2.99 cosmetics only (ethical, no pay-to-win)
- **Cap:** $10/month spending warning

---

## 📂 Repository Structure

```
D:\1-Ascendant Continuum Game\
├── Assets/_Project/          # Unity runtime (137 C# scripts)
│   ├── Scripts/              # Game logic (Core, Systems, UI, etc.)
│   ├── Scenes/               # 9 scenes (Bootstrap, 5 realms, etc.)
│   └── Tests/                # 24 test files (EditMode + PlayMode)
├── firebase/                 # Backend config + rules
│   ├── firestore.rules       # Database security + rate limits
│   ├── storage.rules         # File upload limits
│   └── public/               # Website + WebGL hosting
├── docs/                     # Canonical documentation
├── scripts/                  # Node.js automation (social, validation)
├── .github/workflows/        # CI/CD pipelines
├── Build.ps1                 # Unity build automation
├── Maintenance.ps1           # Weekly cleanup tasks
└── PROJECT_STRATEGY_REALITY.md  # 90-day roadmap
```

---

## 🎮 Game Architecture

### Initialization Flow
1. **GameBootstrapper** (BeforeSceneLoad) → spawns all managers
2. **GameManager.Awake()** → sets up singleton + target framerate
3. **FirebaseManager.Start()** → async Firebase init
4. **Bootstrap scene** → routes to Onboarding or MainMenu
5. **Realm scenes** → load via RealmTransitionManager

### Key Systems
- **SaveSystem:** Encrypted PlayerPrefs + optional cloud sync
- **AccessibilityManager:** 8+ modes (colorblind, reduced motion, etc.)
- **DailyChallengeManager:** Streak tracking + constellation puzzles
- **CosmicPatronManager:** IAP handling (currently stubbed)
- **SessionManager:** 5-minute session soft cap with gentle nudges

### Scene Flow
```
Bootstrap → [First-time?] → Onboarding → MainMenu → Realm_* → MainMenu
```

---

## 🚨 Critical Constraints

### Must-Follow Rules
1. **No new features until 50 beta testers validated** (data-driven only)
2. **Firebase rate limits are sacred** (cost protection)
3. **All IAP must follow ethical principles** (no dark patterns)
4. **Test on real Android device** (Editor simulation != reality)
5. **Player feedback > internal opinions** (ego has no vote)

### Current Blockers
- [ ] Unity Android Build Support not installed (run `.\Setup-UnityModules.ps1`)
- [ ] Unity IAP package not installed (follow `UNITY_IAP_SETUP.md`)
- [ ] Zero active players (need 25 installs this week)
- [ ] No analytics tracking (can't measure retention/drop-off)

### Success Metrics (90 Days)
- **Week 4:** 50 Android installs OR 100 waitlist signups
- **Week 8:** 20% Day 7 retention, 30% complete Realm 1
- **Week 12:** 5% monetization conversion, $25 revenue from 500 installs

---

## 🛠️ Common Tasks

### Build Android APK
```powershell
.\Build.ps1 -Platform Android -BuildType Release
```

### Run Maintenance
```powershell
.\Maintenance.ps1  # Weekly cleanup
```

### Deploy Firebase
```powershell
.\Deploy-Firebase.ps1 -Target All
```

### Test WebGL Locally
```powershell
# After building WebGL:
cd Builds\WebGL
python -m http.server 8000
# Open: http://localhost:8000
```

### Check Errors
```csharp
// In Unity Editor: Window → General → Console
// Or use get_errors tool to check programmatically
```

---

## 🧪 Testing Strategy

### Before Every Commit
1. Build script compiles cleanly
2. No errors in Unity Console
3. WebGL build succeeds (if code changes)
4. Firestore rules validate (if rule changes)

### Before Android Release
1. Test full 30-min gameplay session on device
2. Verify IAP flow (Editor simulation OK for pre-beta)
3. Check Firebase costs (should be ~$0 with < 50 users)
4. Survey 5 testers: "Would you play this daily?"

---

## 💡 Decision Framework

### Should I build this feature?
1. Does it help get 50 beta testers? **YES** → Build it
2. Will players notice if it's missing? **NO** → Don't build it
3. Can we validate with fake door test? **YES** → Test first
4. Does it require > 3 days of work? **YES** → Break down or cut

### Should I refactor this code?
1. Is it causing actual bugs? **YES** → Refactor
2. Is it preventing new features? **YES** → Refactor
3. Does it "just feel wrong"? **NO** → Leave it
4. Will it improve performance? **MAYBE** → Measure first

---

## 🎯 Current Phase: Pre-Beta Foundation

### Focus Areas (Next 30 Days)
1. **Player Acquisition (40% time):** Post to subreddits, email journalists, activate waitlist
2. **Core Polish (30% time):** Fix onboarding, smooth first-session, remove friction
3. **Analytics (15% time):** Track retention, drop-off, session length
4. **IAP Implementation (10% time):** Install Unity IAP, test purchase flow
5. **New Features (5% time):** ONLY if player feedback demands it

### What NOT to Work On
- ❌ Additional realms (have 5 already, players haven't validated 1)
- ❌ Visual polish (acceptable > perfect)
- ❌ Social features (no players yet)
- ❌ Advanced systems (deities, mutations, etc.)
- ❌ Documentation (code comments are sufficient)

---

## 📊 Metrics That Matter

### Acquisition
- Waitlist signups per week (target: 25+)
- Android installs per week (target: 25+)
- Organic shares/posts (target: 5+ unprompted)

### Engagement
- Day 1 retention (target: 40%+)
- Day 7 retention (target: 20%+)
- Average session length (target: 3-5 min)
- Realm 1 completion rate (target: 50%+)

### Monetization
- Purchase conversion (target: 5%+)
- Average transaction value (target: $1.50)
- Monthly spending per user (target: $0.10 LTV)

---

## 🚀 Builder-Validator Pattern

Apply to all major work:

1. **Build:** Implement feature/fix in smallest viable form
2. **Validate:** Test in Editor, build APK, test on device
3. **Fix:** Address issues discovered in validation
4. **Iterate:** Repeat until validation passes
5. **Measure:** Track metrics, compare to baseline

**Never skip validation.** "Works in Editor" means nothing.

---

## 🧰 Automation & Skills

See `.claude/skills/` folder for modular skills:
- `unity-build.md` - Build automation and troubleshooting
- `firebase-deploy.md` - Backend deployment and rules
- `player-feedback.md` - Survey analysis and prioritization
- `cost-optimization.md` - Firebase cost monitoring

---

## 🎓 Learning from Mistakes

### Past Mistakes to Avoid
1. **Built 5 realms before validating 1** → Build minimal, validate, iterate
2. **Spent 6 months with 0 players** → Player acquisition = priority #1
3. **Assumed "build it and they'll come"** → Marketing > features
4. **Fake metrics in code comments** → Only track real player data
5. **Social automation with no audience** → Build audience first

### Principles Learned
- Code quality < player feedback
- Perfect < shipped
- Features < retention
- Beliefs < data
- Solo dev hours < marketing reach

---

## 🔗 Key Files Reference

| File | Purpose |
|------|---------|
| `PROJECT_STRATEGY_REALITY.md` | 90-day roadmap and assumptions |
| `UNITY_IAP_SETUP.md` | IAP implementation guide |
| `FIXES_APPLIED.md` | Red-team audit responses |
| `firebase/firestore.rules` | Database security + rate limits |
| `Build.ps1` | Unity build automation |
| `Maintenance.ps1` | Weekly cleanup tasks |

---

## 🌟 Success Definition

**This project succeeds when:**
- 50 real humans play the game and give feedback
- 20% return after 7 days (proves retention)
- 5% pay $0.99+ (proves monetization)
- Player quotes: "I play this daily" (proves habit formation)

**This project fails when:**
- We optimize code nobody uses
- We build features nobody wants
- We assume instead of measure
- We prioritize pride over pragmatism

---

**Remember:** You're building a business, not a portfolio piece. Ship fast, learn faster, iterate relentlessly.
