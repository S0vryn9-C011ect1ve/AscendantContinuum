# Release Process

> Step-by-step guide for creating a production release of Ascendant Continuum.
> For CI/CD pipeline details see [CI_CD.md](CI_CD.md).

---

## Release Types

| Type | Tag format | Example | Notes |
|------|-----------|---------|-------|
| Stable | `v{major}.{minor}.{patch}` | `v1.2.0` | Full GitHub release, `prerelease: false` |
| Pre-release / RC | `v{major}.{minor}.{patch}-{label}` | `v1.2.0-rc.1` | GitHub release, `prerelease: true` |

---

## Pre-Release Checklist

Before tagging, confirm:

- [ ] All required secrets are set (see [CI_CD.md — Required Secrets](CI_CD.md#required-secrets))
- [ ] `main` branch is green — no failing required status checks
- [ ] Unity tests pass (EditMode + PlayMode) — check `UNITY_LICENSE` is configured
- [ ] WebGL build has been validated locally or via the last successful CI run
- [ ] `UNITY_VERSION` in `build-deploy.yml` and `release.yml` matches `ProjectSettings/ProjectVersion.txt`
- [ ] Version bump applied in Unity (`ProjectSettings/ProjectSettings.asset` → `bundleVersion`)
- [ ] `ANDROID_KEYSTORE_BASE64` and related secrets are fresh (not expired)
- [ ] Firebase Hosting shows the correct live build at `ascendant-continuum.web.app`
- [ ] `package.json` overrides contain no known CVE versions
- [ ] No open critical/high security alerts in GitHub Security tab

---

## Tagging and Releasing

### Step 1 — Bump the version

Update `ProjectSettings/ProjectSettings.asset`:
```
bundleVersion: 1.2.0
```

Commit and push to `main`:
```bash
git add ProjectSettings/ProjectSettings.asset
git commit -m "chore: bump version to 1.2.0"
git push origin main
```

### Step 2 — Create and push the tag

```bash
git tag v1.2.0
git push origin v1.2.0
```

This triggers the [release.yml](../.github/workflows/release.yml) workflow automatically.

### Step 3 — Monitor the workflow

1. Go to **GitHub → Actions → Release**.
2. Watch the three jobs:
   - `validate-tag` — confirms the tag is valid semver
   - `build` (matrix: WebGL, Android, iOS) — builds all platforms
   - `create-release` — packages artifacts and creates the GitHub Release

Expected run time: ~45–60 minutes (dominated by Unity build on macOS for iOS).

### Step 4 — Verify the release

1. Go to **GitHub → Releases**.
2. Confirm:
   - Release notes are auto-generated from commits since the previous tag
   - `AscendantContinuum-{version}-WebGL.zip` is attached
   - `AscendantContinuum-{version}.aab` is attached (if Android build succeeded)
   - The release is marked stable (not pre-release) for stable tags

### Step 5 — Post-release

- [ ] Verify WebGL build is deployed to `ascendant-continuum.web.app` (handled by `build-deploy.yml` on the main push)
- [ ] Submit Android AAB to Google Play Console
- [ ] Submit iOS IPA to App Store Connect (requires Xcode signing — iOS build currently marked `continue-on-error`)
- [ ] Announce in Discord and social media (can trigger `publish-content.yml` manually)

---

## Hotfix Releases

For urgent patches on an already-released version:

```bash
# Create a hotfix branch from the release tag
git checkout -b hotfix/1.2.1 v1.2.0
# Apply fix
git commit -m "fix: critical crash on realm transition"
git push origin hotfix/1.2.1
# Merge to main via PR, then tag
git checkout main && git pull
git tag v1.2.1
git push origin v1.2.1
```

---

## Rolling Back a Release

GitHub Releases can be deleted or marked as pre-release from the UI. The associated tag must be deleted separately if needed:

```bash
# Delete the remote tag (requires repo write access)
git push --delete origin v1.2.0
# Optionally delete locally
git tag -d v1.2.0
```

Note: This does not revert the Firebase deployment. Roll back Firebase Hosting manually via the Firebase Console → Hosting → Release history → Roll back.

---

## Release Notes Format

GitHub auto-generates release notes from PR titles and commit messages since the last tag.
To customise the groupings, add a `.github/release.yml` file:

```yaml
changelog:
  categories:
    - title: New Features
      labels: [feature, enhancement]
    - title: Bug Fixes
      labels: [bug, fix]
    - title: Security
      labels: [security]
    - title: Dependencies
      labels: [dependencies]
```

---

## Artifact Retention

| Artifact | Retention | Location |
|----------|-----------|----------|
| CI build artifacts (per push) | 14 days | GitHub Actions → Artifacts |
| Release build artifacts | 30 days | GitHub Actions → Artifacts |
| GitHub Release binaries | Permanent | GitHub Releases page |
| Security scan reports | 90 days | GitHub Actions → Artifacts |
