# Post-Safe-Mode Runtime Smoke Test

Date: 2026-02-27

## Goal

Validate core gameplay loop and stability after Safe Mode recovery and architecture/performance fixes.

## Pass Criteria

- No script exceptions in Console during full smoke pass.
- Scene transitions complete without soft-lock.
- Save/load preserves realm and progression values.
- Audio (music, SFX, ambient, UI) responds to settings and transitions.
- Accessibility toggles immediately affect motion/haptics/announcements.

## Smoke Flow (in order)

1. Boot and Main Menu
   - Launch from Bootstrap path.
   - Confirm menu opens without missing references.
   - Toggle reduced motion, then re-enter menu and confirm reduced animation workload.

2. Core Navigation
   - Open and close: Realm Select, Settings, Credits.
   - Trigger button haptics and verify no null-reference errors.

3. Realm Transitions
   - Enter each realm once:
     - Emberforge
     - Verdant Sanctuary
     - Echo Fields
     - Dawn Citadel
     - Lantern Ascension
   - Confirm transition fade, state changes, and target scene load.

4. Save/Load Integrity
   - In a realm, make visible progress (sparks/sigil/challenge step).
   - Save, return to menu, reload realm.
   - Verify progress persists and playtime increments sanely.

5. Daily Challenge
   - Confirm challenge card loads.
   - Perform action that increments current challenge.
   - Verify progress updates and completion path awards reward.

6. Treasure and Accessibility
   - Approach treasure until proximity feedback starts.
   - Confirm haptics are throttled (not constant spam).
   - If screen reader is enabled, verify announcements are rate-limited.

7. Audio Settings
   - Adjust master/music/SFX/ambient/UI sliders.
   - Confirm values apply immediately and persist after scene change.

## Regression Watchlist

- Transition manager cannot resolve realm scene.
- Missing camera in transition FX path.
- Audio manager duplicate-instance behavior.
- PlayerPrefs key drift for audio settings.
- Performance manager restoring render scale incorrectly.

## Log Review

After smoke run, export Console and check for:

- Exception
- MissingReferenceException
- NullReferenceException
- Scene not found warnings
- Audio source pool exhaustion warnings
