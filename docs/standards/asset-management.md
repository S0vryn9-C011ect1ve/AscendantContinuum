# Asset Management Standard

## Goals

- Keep Unity references stable.
- Prevent duplicate or orphaned assets.
- Keep asset intake consistent and reviewable.

## Intake Rules

- New gameplay scripts go to `Assets/_Project/Scripts/` domain folders.
- New textures and sprites go to `Assets/_Project/Art/` subfolders by realm/domain.
- New music and SFX go to `Assets/_Project/Audio/` subfolders by type.
- New prefabs go to `Assets/_Project/Prefabs/` by realm/system/UI domain.

## Safety Rules

- Never move Unity assets without preserving `.meta` files.
- Run scene/prefab reference checks after move batches.
- Avoid duplicate assets with slightly different names in the same domain.

## Validation Rules

- CI should validate that new files are not added to deprecated paths.
- CI should validate naming conventions for key asset domains.
- Periodic duplicate scans should be run for textures, audio, and generated docs.

## Lifecycle

- `active`: used in current build path
- `archived`: retained for history but excluded from active flows
- `generated`: reproducible outputs that should not be hand-edited
- `obsolete`: remove after reference and policy verification
