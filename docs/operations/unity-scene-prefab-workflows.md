# Unity Scene and Prefab Workflows

This is the canonical operational guide for scene assembly and prefab creation during repository consolidation.

## Common Workflow

1. Open or create the target scene under `Assets/_Project/Scenes/`.
2. Confirm the required background, camera, canvas, and event system are present.
3. Create the realm or system root GameObject and attach existing controller scripts.
4. Create or update prefabs under `Assets/_Project/Prefabs/`.
5. Assign prefab, container, UI, audio, and visual references in the inspector.
6. Test the interaction loop in Play Mode.

## Scene Assembly Rules

- Prefer one obvious root object per realm or system.
- Keep hierarchy organization predictable with named containers.
- Treat scene setup as wiring of existing scripts and assets, not as a place to invent new architecture.
- Save scenes only after required references are confirmed.

## Prefab Rules

- Create prefabs from fully configured scene objects.
- Put prefabs in the closest matching realm or shared folder.
- Include only the components needed for the prefab's behavior.
- Re-test a dragged-in instance before relying on runtime spawning.

## Realm-Oriented Application

Use `docs/operations/implementation-guides.md` for realm-specific assembly patterns such as Emberforge and Verdant. Use this document for the shared workflow shape that applies to all scene and prefab setup.

## Related Canonical Docs

- `docs/operations/implementation-guides.md`
- `docs/ASSET_PRODUCTION_GUIDE.md`
- `docs/QUICK_REFERENCE_CHECKLIST.md`
