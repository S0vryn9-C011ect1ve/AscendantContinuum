# Contributing

This repository contains a Unity game project, Firebase website/deployment surfaces, and automation tooling. Follow these rules to keep the repository stable and organized.

## Core Workflow

1. Create a branch for each change.
2. Keep changes scoped by concern: gameplay, website, automation, docs, or governance.
3. Run relevant validation before opening a pull request.
4. Do not place new operational docs at the repository root.
5. Commit hooks run repository structure validation automatically, so misplaced files should be caught before they land.

## Required Local Checks

Run the checks that apply to your change:

```powershell
npm ci
npm run validate:structure
npm run validate:auto-publish
npm run validate:content
npm run validate:dedup
```

## Unity Safety Rules

- Do not move files under `Assets/` without preserving `.meta` files.
- Prefer git-aware moves for Unity assets.
- Do not reorganize asmdefs casually.
- Validate affected scenes/prefabs after asset moves.

## Documentation Rules

- `README.md` is the top-level entry point.
- New docs belong under `docs/` by domain.
- Legacy docs follow strict wave order: merge, then archive, then delete.
- Do not store secrets or tokens in markdown files.
- The pre-commit hook enforces repository structure automatically.

## Automation and Website Rules

- `firebase/public/` is the active deploy surface for the website.
- Root `public/` is legacy automation state and should not receive new website files.
- Add new automation scripts under `scripts/` by domain.

## Pull Requests

Every pull request should include:

- Scope summary
- Validation run list
- Risks or migration effects
- Docs updates if paths, workflows, or contributor processes changed
