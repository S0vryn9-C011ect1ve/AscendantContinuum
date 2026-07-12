# CLAUDE.md — Ascendant Continuum

Unity 6000.3.9f1 game repo. Accessibility-first mindful cosmic adventure (5 realms, ethical monetization, 60fps target). Part of the S0vryn9 C011ect1ve ecosystem (community-before-profit, disabled-worker-led).

## Canonical docs (read these, not root .md stubs)
- `README.md` — project hub
- `docs/BUILD_READINESS_STATUS.md` — build readiness
- `docs/CI_CD.md` — CI/CD reference
- `docs/RELEASE.md` — release process
- `docs/DEVELOPMENT.md` — dev guide
- `docs/security/SUPPLY_CHAIN_PROTECTION.md` — security
- `docs/operations/legacy-doc-consolidation-wave-tracker.md` — doc migration status

> Many root-level `*.md` files (START_HERE_TODAY.md, STATUS.md, etc.) are **archived stubs** pointing to `docs/archive/`. Do not treat them as current.

## Layout
- `Assets/` — Unity project content
- `ProjectSettings/` — Unity config (Unity version pinned in `ProjectSettings/ProjectVersion.txt`)
- `firebase/` — Firebase hosting + Cloud Functions (`firebase/functions/package.json`)
- `docs/legal/governance/` — ecosystem governance docs (dead man's switch, board charter, user councils)
- `.github/workflows/` — CI: `build-deploy.yml` (Unity Android/iOS/WebGL + Firebase deploy), `claude-code-validation.yml`, `codeql.yml`, `lint.yml`, `content-guard.yml`

## Build & deploy
- **Local web deploy (manual):** `cd firebase && firebase deploy --only hosting` (Firebase CLI global, v15.23.0)
- **CI builds:** GitHub Actions `build-deploy.yml` runs Unity builds for Android/iOS/WebGL. NOTE: these currently fail with exit 139 (SIGSEGV) — tracked in kanban, not yet fixed.
- **UnityTests + ContentGuard** CI jobs pass.

## Constraints (do not violate)
- Proprietary license — no open-source release of game code.
- Accessibility-first is non-negotiable (colorblind modes, reduced motion, screen reader).
- Ethical monetization: cosmetics only, no pay-to-win, no FOMO.
- Firebase functions deps: `npm audit` must stay clean (lockfile audited, protobufjs >=8.6.0).
- CodeQL alerts on trusted-local build/audit scripts are known false-positives — do NOT patch without explicit review.

## Working with this repo
- Branch: `main` (protected-ish; push triggers full CI).
- Commits: conventional commits (commitlint CI enforced).
- Don't commit `Library/`, `Temp/`, or local Unity cache.
- Secrets live in GitHub Actions / Firebase env — never hardcode.
