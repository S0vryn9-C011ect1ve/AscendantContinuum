# Legacy Emberforge Polish Guide (Superseded)

This file is superseded by canonical implementation guidance.

Use:

1. `docs/operations/implementation-guides.md` for canonical implementation flow
2. `docs/QUICK_REFERENCE_CHECKLIST.md` for checklist tracking
3. `docs/POLISH_ART_AUDIO_ROADMAP.md` and `docs/POLISH_ART_AUDIO_SUMMARY.md` for polish rollout guidance
4. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress
- Click **`+`** next to **Bursts**
- **Time:** `0`, **Count:** `10`

**Shape:**
- **Shape:** `Sphere`
- **Radius:** `0.2`

### Step 3: Save as Prefab
1. **Drag** `SparkCollectVFX` from Hierarchy → Project panel → `Assets/_Project/Prefabs/VFX/` folder
2. Create `VFX` folder if it doesn't exist
3. **Optional:** Delete `SparkCollectVFX` from Hierarchy (we'll spawn it via code)

### Step 4: Wire to Script (Advanced - Skip if Unsure)
*This requires editing Spark.cs to play VFX on collect. If comfortable with code, add this to CollectSpark() method:*
```csharp
// Play VFX at spark position
GameObject vfx = Instantiate(collectVFXPrefab, transform.position, Quaternion.identity);
Destroy(vfx, 1f);
```
**Or skip this step - visual polish can wait!**

---

## Part 6: Test Full Session (10 minutes)

### Final Playtest

1. Hit **Play** ▶️
2. **Play for 5 minutes straight:**
   - Click sparks
   - Watch counter increase
   - Get to 20/20
   - See completion screen
3. **Ask yourself:**
   - ✅ Is clicking satisfying?
   - ✅ Are sparks visible and pretty?
   - ✅ Is counter readable?
   - ✅ Does completion feel rewarding?
   - ✅ Would I want to play this for 5 minutes daily?

4. **If YES to all:** ✅ Emberforge is DONE!
5. **If NO to any:** Go back to relevant Part above and adjust

---

## Quick Checklist

- [ ] Completion screen appears at 20 sparks
- [ ] Spark colors look good
- [ ] Spark movement feels natural
- [ ] Spawn rate feels balanced (not too many, not too few)
- [ ] Counter is readable and well-positioned
- [ ] Overall experience feels meditative and satisfying

---

## Common Issues & Fixes

**Issue:** Sparks collect but counter stays at 0  
**Fix:** EmberforgeRealm → EmberforgeController → Spark Counter Text field is empty. Drag SparkCounterText into it.

**Issue:** No completion screen at 20 sparks  
**Fix:** EmberforgeRealm → EmberforgeSparks → Completion Panel field is empty. Find RealmCompletionPanel in Hierarchy and drag it in.

**Issue:** Counter says "Sparks: 12/20" but I've clicked way more  
**Fix:** Sparks are collecting correctly! The first number is YOUR total, the `/20` is just the goal. It's working!

**Issue:** Sparks are invisible again  
**Fix:** Open Spark prefab → SpriteRenderer → Sprite field → make sure `icon_sparks` is assigned (not "None")

**Issue:** Can't click sparks anymore  
**Fix:** Open Spark prefab → Circle Collider 2D → Radius should be `10` or higher (not `0.4`)

---

## What's Next?

✅ **Emberforge is polished!**  
➡️ **Move to Verdant Sanctuary** (see VERDANT_SUPER_SIMPLE.md)  
➡️ **Or add audio** (see ZERO_BUDGET_AUDIO_GUIDE.md)  
➡️ **Or build for Android/iOS** to test on device

---

## Achievement Unlocked 🏆
**"Forge Master"** - You've created and polished a complete, playable game realm from scratch!
