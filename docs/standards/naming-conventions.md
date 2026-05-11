# Naming Conventions

## Scripts and Code

- C# scripts: `PascalCase.cs`
- Classes and structs: `PascalCase`
- Methods and properties: `PascalCase`
- Private fields: `_camelCase`
- Constants: `UPPER_SNAKE_CASE` only when true constants

## Unity Assets

- Scenes: `SceneName.unity` with stable prefixes when useful (`Realm_Emberforge.unity`)
- Prefabs: `PascalCase.prefab` (example: `PlayerCharacter.prefab`)
- ScriptableObjects: `PascalCase.asset`
- Materials: `PascalCase.mat`
- Shaders: `PascalCase.shader`

## Art and Audio

- Textures/sprites: `domain_subject_variant_nn.ext`
- Example: `environment_forest_01.png`
- UI sounds: `ui_action_variant_nn.ext`
- Example: `ui_button_click_01.wav`
- Music: `music_theme_variant_nn.ext`
- Example: `music_emberforge_01.ogg`

## Levels and Content

- Level slugs for docs/web: `chapter-01-introduction`
- Realm folders: `PascalCase` or existing stable realm naming; do not mix styles in same domain.

## Documentation

- Canonical docs: descriptive kebab or existing stable uppercase style per folder norms.
- New standards/process docs should prefer kebab-case.
