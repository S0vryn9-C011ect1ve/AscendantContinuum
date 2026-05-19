# Development Guide

This guide covers local setup, daily workflow, and validation commands for contributors.

## Prerequisites

- Unity `6000.3.9f1` (Unity 6 LTS)
- Node.js `20.x` (required for automation scripts)
- npm `10+`
- Git with LFS enabled
- Firebase CLI (`npx firebase-tools@13` recommended)

## Local Setup

```powershell
git clone <repo-url>
cd "1-Ascendant Continuum Game"
git lfs install
npm ci
```

Unity setup:

1. Open project with Unity Hub using version `6000.3.9f1`.
2. Wait for package restore and script compilation.
3. Confirm no compile errors in Console.

## Day-to-Day Workflow

1. Create a branch from `main`.
2. Keep scope focused (gameplay, automation, docs, or security).
3. Run only relevant checks locally before push.
4. Open PR and ensure all required checks pass.

## Required Validation Commands

```powershell
npm run validate:structure
npm run validate:auto-publish
npm run validate:content
npm run validate:dedup
npm run validate:links
```

If your change touches Firebase Functions:

```powershell
cd firebase/functions
npm ci
npm audit --audit-level=high --omit=dev
```

## Unity-Specific Safety

- Never move Unity assets without preserving `.meta` files.
- Prefer git-aware moves in editor or git rename operations.
- Avoid casual asmdef changes.
- Validate affected scenes/prefabs after structural edits.

## CI/CD Entry Points

- CI/CD reference: `docs/CI_CD.md`
- Release process: `docs/RELEASE.md`
- Security policy: `SECURITY.md`

## Commit Convention

Use conventional commits:

- `feat(scope): ...`
- `fix(scope): ...`
- `docs(scope): ...`
- `ci(scope): ...`
- `security(scope): ...`

Commit messages are validated in CI by `.github/workflows/commitlint.yml`.
