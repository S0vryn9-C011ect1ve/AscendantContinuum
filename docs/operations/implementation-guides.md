# Implementation Guides

This is the canonical destination for high-level implementation walkthroughs during repository consolidation.

## Starter Workflow

1. Confirm the target scene, prefab, and script locations in `Assets/_Project/`.
2. Follow the common realm assembly pattern:
   - create or open the target realm scene
   - add the realm root GameObject
   - attach the realm controller and gameplay script(s)
   - assign required prefabs and visual/audio references
   - verify camera, UI canvas, and event system presence
   - test in Play Mode and confirm the expected loop works
3. Record follow-up wiring or asset gaps in canonical docs, not new root markdown files.

## Realm Assembly Pattern

### Emberforge baseline
- Open `Assets/_Project/Scenes/Realms/Realm_Emberforge.unity`.
- Create the main realm root and attach `EmberforgeController` plus `EmberforgeSparks`.
- Assign the spark prefab from `Assets/_Project/Prefabs/Realms/Emberforge/`.
- Configure spawn count, interval, lifetime, and goal values.
- Ensure a background sprite, camera, UI canvas, and counter are present.
- Test that sparks spawn, can be collected, and advance progress.

### Verdant baseline
- Open or create the Verdant realm scene.
- Create the garden root and attach `VerdantController` plus `VerdantGarden`.
- Verify `MagicalPlant` prefab setup and growth-stage sprites.
- Confirm watering/click interaction works and plant progression is visible.
- Ensure camera, UI, and basic background/readability are in place.

## Polish and Content Integration

Use these canonical docs for execution detail:

- `docs/QUICK_REFERENCE_CHECKLIST.md`
- `docs/ASSET_PRODUCTION_GUIDE.md`
- `docs/POLISH_ART_AUDIO_ROADMAP.md`
- `docs/POLISH_ART_AUDIO_SUMMARY.md`

## Scope Rule

These guides describe how to wire and validate existing systems. They are not approval to invent a parallel architecture or duplicate system.
