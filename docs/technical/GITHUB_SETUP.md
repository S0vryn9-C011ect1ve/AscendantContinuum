# Environment and CI/CD Setup

This is the canonical setup guide for local development, automation, and deployment.

## Required Toolchain

- Unity 6000.3.9f1 (required)
- Unity modules: WebGL Build Support, Android Build Support, iOS Build Support
- Node.js 18+
- Git
- Firebase CLI (`npm install -g firebase-tools`)
- Optional: GitHub CLI (`gh`)

## Local Project Setup

1. Clone and open the repository in Unity Hub.
2. Confirm Unity version is exactly 6000.3.9f1.
3. Let packages import fully before running scenes or tests.
4. Verify build scenes are configured in `ProjectSettings/EditorBuildSettings.asset`.

## Firebase Local Setup and Deploy

- Canonical Firebase hosting surface is `firebase/public/`.
- Use Firebase CLI from the `firebase/` directory when verifying hosting or project access.
- Confirm the active Firebase project matches repository deployment settings before pushing a hosting deploy.
- Treat Firebase hosting/deploy fixes as environment validation, not as a reason to add new root setup docs.

## Build and Validation

- Game build script: `Build.ps1`
- Firebase deploy script: `Deploy-Firebase.ps1`
- Main CI workflow: `.github/workflows/build-deploy.yml`

Recommended local checks:

```powershell
npm ci
npm run validate:auto-publish
npm run validate:content
npm run validate:dedup
```

## GitHub Actions Secrets (Names Only)

Store all values in GitHub Secrets. Never commit values to markdown or source files.

Core build/deploy:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`
- `FIREBASE_TOKEN`
- `ANDROID_KEYSTORE_BASE64`
- `ANDROID_KEYSTORE_PASS`
- `ANDROID_KEY_ALIAS`
- `ANDROID_KEY_ALIAS_PASS`

Social automation:

- `BLUESKY_IDENTIFIER`
- `BLUESKY_PASSWORD`
- `MASTODON_INSTANCE`
- `MASTODON_API_URL`
- `MASTODON_ACCESS_TOKEN`
- `DISCORD_WEBHOOK_URL`

Compatibility note:

- The current automation scripts and workflows expect `BLUESKY_IDENTIFIER` and `BLUESKY_PASSWORD`.
- Some workflows still reference `MASTODON_API_URL` while posting modules commonly use `MASTODON_INSTANCE`; keep both populated until workflow consolidation is complete.

## Security Requirements

- Never store credentials in tracked files.
- Rotate any previously exposed credential immediately.
- Prefer `npm ci` over `npm install` for reproducible dependency resolution.
- Keep `.npmrc` protections active (release-age delay, script controls).

## Branch and PR Governance

Recommended branch protection for `main`:

- Require pull request reviews.
- Require required status checks.
- Require up-to-date branch before merge.
- Block force push and branch deletion.

## Related Canonical Docs

- Build readiness: `docs/BUILD_READINESS_STATUS.md`
- Security and supply chain: `docs/security/SUPPLY_CHAIN_PROTECTION.md`
- Testing and verification: `docs/technical/testing-and-verification.md`
- Social automation: `docs/social/README.md`
- Unity scene and prefab workflows: `docs/operations/unity-scene-prefab-workflows.md`
- Legacy migration tracker: `docs/operations/legacy-doc-consolidation-wave-tracker.md`
