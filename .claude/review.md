# Pre-Commit Review Checklist

Before finalizing any code changes, verify:

## ✅ Code Quality

- [ ] **Compiles cleanly** - No errors in Unity Console
- [ ] **Follows conventions** - C# naming (PascalCase for public, camelCase for private)
- [ ] **No TODO/FIXME** - Address or document as GitHub issue
- [ ] **Rate limits respected** - Firebase calls follow rules
- [ ] **No hardcoded secrets** - API keys in environment variables only

## ✅ Testing

- [ ] **Editor testing** - Core functionality works in Unity Editor
- [ ] **Build succeeds** - Target platform builds without errors
- [ ] **Device testing** - If Android/iOS change, test on real device
- [ ] **Firebase rules** - Validate with emulator if rule changes

## ✅ Performance

- [ ] **No memory leaks** - Dispose of resources properly
- [ ] **60 FPS target** - Profile if adding heavy visuals/logic
- [ ] **Mobile-optimized** - Battery drain acceptable? (<5% per 5-min session)
- [ ] **Network efficient** - Minimize Firebase reads/writes

## ✅ User Impact

- [ ] **Onboarding unchanged?** - If yes, good. If no, test thoroughly.
- [ ] **Save compatibility** - Doesn't break existing player data
- [ ] **Accessibility preserved** - Colorblind modes still work
- [ ] **Ethical monetization** - No dark patterns introduced

## ✅ Documentation

- [ ] **CLAUDE.md updated** - If architecture changes
- [ ] **README.md accurate** - If setup steps change
- [ ] **Inline comments** - For non-obvious logic only

## ✅ Cost Impact

- [ ] **Firebase costs** - Change won't spike monthly bill
- [ ] **Storage usage** - Assets compressed appropriately
- [ ] **Build size** - APK/IPA under size targets (150MB/200MB)

## 🚨 Red Flags (Stop and Reconsider)

- Adding new features before 50 beta testers validated
- Refactoring working code "just because"
- Increasing complexity without player demand
- Breaking changes without migration path
- Spending > 3 days on one feature

## 📊 Definition of Done

A change is complete when:

1. **Built** - Compiles and runs
2. **Tested** - Works in Editor + target device
3. **Measured** - No performance regression
4. **Documented** - Future you understands why
5. **Shipped** - Merged to main and deployed

---

**Remember:** Good enough > perfect. Ship it, learn from players, iterate.
