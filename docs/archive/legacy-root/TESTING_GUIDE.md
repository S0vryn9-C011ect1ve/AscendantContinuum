# Legacy Testing Guide (Superseded)

This file is superseded by canonical testing documentation.

Use:

1. `docs/technical/testing-and-verification.md` for testing layers and minimum verification rules
2. `docs/social/TESTING_GUIDE.md` for social automation validation
3. `docs/BUILD_READINESS_STATUS.md` for current verified build state
4. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress
- [ ] Windowed mode resizing works

**Mobile Web:**
- [ ] Portrait orientation
- [ ] Landscape orientation
- [ ] Touch controls work
- [ ] No broken layouts

### Performance Benchmarks
Use Unity Profiler (Window → Analysis → Profiler):

**Target Metrics (60fps = 16.67ms per frame):**
- [ ] CPU time: <12ms
- [ ] GPU time: <12ms
- [ ] Memory: <200MB
- [ ] Draw calls: <100
- [ ] Batches: <50

**If performance is poor, see OPTIMIZATION_GUIDE.md**

---

## 👥 PHASE 4: USER TESTING (Beta)

### Recruit Beta Testers (10-20 people)
- Friends/family
- Reddit (r/playmygame, r/WebGames)
- Discord servers (game dev, accessibility communities)
- Twitter followers

### Beta Testing Form (Google Forms, Free)

**Questions to ask:**
1. Did the tutorial make sense?
2. Rate difficulty (1-5): Too easy → Too hard
3. Which realm was your favorite?
4. Did you understand the progression system?
5. Were any mechanics confusing?
6. Did you encounter any bugs?
7. Rate accessibility features (if you used them)
8. Would you recommend this game to a friend?
9. Any suggestions for improvement?

### Track Beta Feedback
Create spreadsheet:
- Tester name
- Device/browser used
- Bugs found (link to issue)
- Feature requests
- Overall sentiment (positive/negative)

### Common Beta Test Findings
- **"I didn't know what to do"** → Tutorial needs clarity
- **"The game is too slow"** → Adjust spawn rates
- **"I got stuck"** → Fix soft-lock bug
- **"Sparks are too small to tap"** → Increase collider size
- **"Music is too loud"** → Adjust default volume

---

## 🐛 BUG TRACKING

### Create Simple Bug List (Spreadsheet or Markdown)

**Columns:**
- Bug ID (1, 2, 3...)
- Description (what's broken)
- Steps to Reproduce (how to trigger bug)
- Severity (Critical/High/Medium/Low)
- Status (Open/In Progress/Fixed/Won't Fix)
- Found By (who reported it)

**Example:**
```
ID  | Description                    | Severity | Status
----|--------------------------------|----------|--------
1   | Sparks don't spawn in WebGL    | Critical | Fixed
2   | Music loops with gap           | Medium   | Open
3   | Tutorial repeats every time    | High     | Fixed
4   | UI overlaps on iPhone 8        | High     | Open
5   | Settings don't save            | Critical | Fixed
```

### Bug Severity Levels

**Critical:** Blocks gameplay, crashes, data loss
- Fix immediately before launch

**High:** Major feature broken, affects many players
- Fix before launch

**Medium:** Minor feature broken, workarounds exist
- Fix if time allows, or in update

**Low:** Visual glitch, typo, cosmetic issue
- Nice to fix, not essential

### Debugging Tips

**Unity Console Errors:**
- Red = critical error (fix immediately)
- Yellow = warning (investigate if causing issues)
- Blue = info log (ignore unless debugging)

**Common Unity Errors:**
- `NullReferenceException` → Something is null that shouldn't be
  - Fix: Check all script fields in Inspector are assigned
- `MissingComponentException` → Script expects component that's missing
  - Fix: Add the component or check script requirements
- `IndexOutOfRangeException` → Accessing array element that doesn't exist
  - Fix: Check array length before accessing
- `ArgumentException` → Invalid parameter passed to function
  - Fix: Verify function is called with correct values

**Debugging Script Issues:**
1. Add `Debug.Log("Got here!")` to see if code runs
2. Add `Debug.Log(variableName)` to see variable values
3. Use breakpoints in Visual Studio (run in Debug mode)
4. Check Unity Console for stack trace (shows exact line number)

---

## ✅ PRE-LAUNCH TESTING CHECKLIST

**ONE WEEK BEFORE LAUNCH:**

### Functionality
- [ ] All 5 realms playable start to finish
- [ ] Progression system works (unlocks, saves)
- [ ] Tutorial guides new players successfully
- [ ] Settings save and apply correctly
- [ ] Firebase saves/loads data
- [ ] No critical bugs
- [ ] No high-severity bugs

### Performance
- [ ] 60fps on mid-range devices (Galaxy S8 equivalent)
- [ ] Build size <100MB
- [ ] Memory usage <200MB
- [ ] No crashes during 30-minute play session
- [ ] No memory leaks (memory stays stable over time)

### Compatibility
- [ ] Works on 3+ different devices/browsers
- [ ] Works on both portrait and landscape (if supported)
- [ ] Works offline (core gameplay, not online features)
- [ ] Works after backgrounding app
- [ ] Works after interruption (phone call, notification)

### Accessibility
- [ ] All 5 colorblind modes functional
- [ ] Reduced motion mode functional
- [ ] High contrast mode functional
- [ ] Text scaling doesn't break UI
- [ ] Touch targets large enough (44x44px minimum)
- [ ] Works without audio (visual feedback sufficient)

### Content
- [ ] All text has no typos
- [ ] All images load correctly
- [ ] All sounds play correctly
- [ ] All UI is readable
- [ ] Credits include audio attribution

### Legal/Policy
- [ ] Privacy policy written and linked
- [ ] Terms of service written (if needed)
- [ ] GDPR compliant (EU players can delete data)
- [ ] COPPA compliant (no data from <13 year olds)
- [ ] Age rating appropriate (likely E for Everyone or E10+)

---

## 🎯 TESTING EFFICIENCY TIPS

**Don't Test Everything Every Time:**
- Test feature → Mark as ✅
- Only re-test if you change that feature
- Focus on integration points (where features interact)

**Use Automated Testing (Advanced):**
- Unity Test Framework (Unit tests for scripts)
- But manual testing is fine for first launch!

**Test on Real Device Often:**
- Simulator is good, but real device shows actual performance
- Test at least once per week during development

**Keep Test Builds:**
- Save working builds (Build_v1.0, Build_v1.1)
- If new build breaks, can compare to old build

---

## 🆘 WHEN TO DELAY LAUNCH

**Delay if:**
- Critical bugs that block gameplay
- Data loss bugs
- Crashes on majority of devices
- Gameplay is confusing (beta testers don't understand)
- Performance is unacceptable (<30fps on target devices)

**Don't delay for:**
- Minor visual glitches
- Low-severity bugs
- "Nice to have" features
- Perfectionism

**"Done is better than perfect"** - You can update post-launch!

---

## ✅ SUCCESS CRITERIA

Testing is complete when:
- [ ] 5 different people tested and gave feedback
- [ ] No critical bugs remain
- [ ] No high-severity bugs remain
- [ ] Performance meets targets
- [ ] Accessibility features work
- [ ] You've played through the entire game 3+ times without issues

---

## 🐛 POST-LAUNCH TESTING

After launch, monitor:
- Crash reports (Unity Analytics, Firebase)
- Player feedback (Reddit, reviews, emails)
- Analytics (where players drop off)
- Performance metrics (FPS, load times)

**Plan for Update 1.1:**
- Fix bugs reported in first week
- Address top player complaints
- Improve areas where players get stuck

---

## 🚀 NEXT STEP: OPTIMIZATION

If testing reveals performance issues, see **OPTIMIZATION_GUIDE.md**!

Otherwise, you're ready to launch! 🎉
