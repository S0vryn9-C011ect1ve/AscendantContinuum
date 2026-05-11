# Project Overview

This is the canonical high-level overview for Ascendant Continuum during repository consolidation.

## Current State

- Unity project is established under `Assets/`, `Packages/`, and `ProjectSettings/`.
- Core runtime, realm, UI, platform, and automation surfaces already exist.
- WebGL build and Firebase-hosted website paths are in place.
- Repository cleanup and standards enforcement are in progress under the legacy-doc wave tracker.

## Canonical Documentation Entry Points

- `README.md`: primary repository entry point
- `docs/_index.md`: documentation navigation hub
- `docs/BUILD_READINESS_STATUS.md`: build and technical readiness
- `docs/technical/GITHUB_SETUP.md`: environment, CI/CD, and secret handling
- `docs/security/SUPPLY_CHAIN_PROTECTION.md`: security controls
- `docs/operations/legacy-doc-consolidation-wave-tracker.md`: migration status

## Project Differentiators

- Accessibility-first gameplay where accommodations can expose distinct content.
- Five realm-based gameplay spaces with different interaction patterns.
- Ethical monetization and short-session design goals.
- Async social and content automation surfaces tied to the website and publishing workflows.

## Repository Reality

- `firebase/public/` is the current live website deploy surface.
- Root `public/social/` is legacy automation state, not the website root.
- Legacy root markdown files are being merged into canonical docs before any archive/delete wave.

## Near-Term Priorities

1. Validate the completed Wave 1 consolidation set.
2. Tighten workflow standardization and secret handling.
3. Continue structural enforcement so new files land in correct locations.
4. Prepare Wave 2 archive moves only after link/reference verification.

## Historical Consolidation Additions

- Agent workflow legacy material now rolls up under `docs/operations/agent-operations-context.md`.
- Launch, tutorial, and zero-budget production notes now roll up under `docs/operations/launch-resources.md`.
- Website and automation milestone notes now roll up under `docs/operations/website-automation-history.md`.
- Non-game and cross-project legacy material now rolls up under `docs/operations/legacy-cross-project-context.md`.
