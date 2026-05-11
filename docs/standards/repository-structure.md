# Repository Structure Standard

This document defines the canonical structure and ownership model for the Ascendant Continuum repository.

## Core Top-Level Layout

- `Assets/`: Unity game content and code
- `Packages/`: Unity package manifests
- `ProjectSettings/`: Unity project configuration
- `UserSettings/`: local Unity user settings (not for policy docs)
- `firebase/`: Firebase hosting/rules/functions deployment surface
- `scripts/`: automation, validation, and migration scripts
- `docs/`: canonical documentation and standards
- `.github/workflows/`: CI/CD and scheduled automation

## Unity-Specific Rules

- Do not move Unity root folders out of repository root.
- Preserve `.meta` files on all Unity moves.
- Use git-aware moves for Unity assets to preserve history and references.
- Do not reorganize asmdefs without explicit dependency review.

## Documentation Rules

- Root-level markdown files are discouraged except `README.md`, `LICENSE`, and policy-required files.
- New docs must be placed under `docs/` by domain.
- Legacy docs must follow Wave execution (`merge` -> `archive` -> `delete`).

## Website and Automation Rules

- `firebase/public/` is the active website deploy surface.
- Root `public/` is not the website root and should not receive new website files.
- New automation state must live under a clearly named non-website path.

## CI Structure Enforcement

CI should fail when files are added to deprecated paths after migration gates are active.
Local pre-commit hooks also run structure validation so new files are blocked before commit if they land in the wrong place.
