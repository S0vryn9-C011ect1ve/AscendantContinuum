# Legacy Emberforge Wiring Guide (Superseded)

This file is superseded by canonical implementation guidance.

Use:

1. `docs/operations/implementation-guides.md` for realm assembly patterns
2. `docs/QUICK_REFERENCE_CHECKLIST.md` for rollout checklist tracking
3. `docs/ASSET_PRODUCTION_GUIDE.md` for asset placement and naming
4. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress
        gameObject.SetActive(false); // Return to pool
    }
}
```

---

### "Counter doesn't update"
**Issue:** EmberforgeSparks.cs doesn't have UI reference

**Fix:** Add UI field to script (see Option A above)

---

### "Console shows 'EmberforgeSparks not found'"
**Check:**
- Is EmberforgeSparks.cs attached to EmberforgeRealm?
- Is it attached to the same GameObject or a child?

**Fix:**
- Verify both scripts are on EmberforgeRealm
- Or drag EmberforgeSparks reference into EmberforgeController's field manually

---

### "Sparks spawn outside camera view"
**Fix:**
- Reduce Spawn Radius (try 3 instead of 5)
- Or increase Camera Size
- Or constrain spawn area in EmberforgeSparks script

---

## ✅ SUCCESS CHECKLIST

Emberforge scene is DONE when:

- [ ] Scene opens without errors
- [ ] Background visible (if added)
- [ ] Sparks spawn automatically
- [ ] Sparks are clickable
- [ ] Sparks disappear on click
- [ ] Counter shows progress (if UI wired)
- [ ] Collecting goal amount (20) triggers completion
- [ ] Music plays (if audio imported)
- [ ] No red errors in Console
- [ ] Game loop feels satisfying to play!

---

## 🎨 POLISH IDEAS (OPTIONAL - Do Later!)

After basic functionality works:

**Visual Polish:**
- Add particle effect on spark collect (sparkles)
- Add glow shader to sparks
- Animate sparks (float, rotate, pulse)
- Add trail renderer for motion blur

**Audio Polish:**
- Different pitch for each spark collected (pitch += 0.1f)
- Ambient fire crackling sound
- Whoosh sound when spark spawns
- Success fanfare on ritual complete

**Gameplay Polish:**
- Combo system (collect sparks quickly for bonus)
- Spark colors based on rarity (common white, rare gold)
- Daily challenge: "Collect 100 sparks today"
- Achievement popup on screen

**But don't do these today!** Get it working first, polish later.

---

## 📊 TIME ESTIMATE

- GameObject setup: 10 min
- Script wiring: 15 min
- UI creation: 15 min
- First test: 5 min
- Bug fixing: 10-20 min
- **Total: 55-65 minutes**

You're building a real game! Keep going! 🔥

—Chad
