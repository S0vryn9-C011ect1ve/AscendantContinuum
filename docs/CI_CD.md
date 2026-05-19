# CI/CD Reference

> **Canonical reference for all GitHub Actions workflows, required secrets, and debugging guidance.**
> For release-specific steps see [RELEASE.md](RELEASE.md).

---

## Workflow Overview

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| [build-deploy.yml](#build-deployyml) | push/PR to main, develop | Unity tests + builds + Firebase deploy |
| [content-guard.yml](#content-guardyml) | push/PR to main, develop | Auto-publish safety validation |
| [codeql.yml](#codeqlyml) | push/PR/weekly | Static security analysis (JS + C#) |
| [commitlint.yml](#commitlintyml) | PR to main | Enforce conventional commit messages |
| [lint.yml](#lintyml) | push/PR to main, develop | YAML, JSON, Markdown, npm audit |
| [pr-labeler.yml](#pr-labeleryml) | PR events | Auto-label pull requests by changed files |
| [release.yml](#releaseyml) | version tag push | Build artifacts + create GitHub release |
| [daily-blog.yml](#daily-blogyml) | daily 10:00 UTC / push main | Generate + publish daily blog post |
| [daily-social.yml](#daily-socialyml) | daily 14:00 UTC | Post content to Bluesky, Mastodon, Discord |
| [daily-whatsnew.yml](#daily-whatsnewml) | daily 23:00 UTC / push main | Generate What's New + Firebase deploy |
| [dev-update-social.yml](#dev-update-socialyml) | Monday 20:00 UTC | Weekly dev update social post |
| [weekend-philosophy.yml](#weekend-philosophyyml) | Sat+Sun 16:00 UTC | Weekend philosophy content |
| [weekly-digest.yml](#weekly-digestyml) | Monday 09:00 UTC | Weekly digest + Firebase deploy |
| [weekly-security-scan.yml](#weekly-security-scanyml) | Monday 09:00 UTC / package changes | npm audit + supply chain threat scan |

All workflows use **pinned action SHA versions** to prevent supply-chain attacks.

---

## Required Secrets

Configure all secrets at: **GitHub repo → Settings → Secrets and variables → Actions**

| Secret | Used By | Description |
|--------|---------|-------------|
| `UNITY_LICENSE` | build-deploy, release | Unity license content (base64-encoded `.ulf` file) |
| `UNITY_EMAIL` | build-deploy, release | Unity account email |
| `UNITY_PASSWORD` | build-deploy, release | Unity account password |
| `ANDROID_KEYSTORE_BASE64` | build-deploy, release | Release keystore (base64-encoded) |
| `ANDROID_KEYSTORE_PASS` | build-deploy, release | Keystore password |
| `ANDROID_KEY_ALIAS` | build-deploy, release | Key alias name |
| `ANDROID_KEY_ALIAS_PASS` | build-deploy, release | Key alias password |
| `FIREBASE_TOKEN` | build-deploy, daily-whatsnew, weekly-digest | Firebase CLI token |
| `BLUESKY_IDENTIFIER` | 6 social workflows | Bluesky handle (e.g. `user.bsky.social`) |
| `BLUESKY_PASSWORD` | 6 social workflows | Bluesky app password |
| `MASTODON_INSTANCE` | 5 social workflows | Mastodon server URL |
| `MASTODON_ACCESS_TOKEN` | 6 social workflows | Mastodon OAuth access token |
| `MASTODON_API_URL` | 3 social workflows | Mastodon API endpoint |
| `DISCORD_WEBHOOK_URL` | 6 social workflows | Discord channel webhook URL |

---

## Workflow Details

### build-deploy.yml

**Jobs (sequential):** `content-guard` → `test` → `build` (matrix) → `deploy-firebase` → `notify`

**Unity Tests (`test` job):**
- Checks for `UNITY_LICENSE` secret before running. If absent, emits a warning and skips gracefully — no build failure.
- Once `UNITY_LICENSE` is configured, test failures are **blocking** (no `continue-on-error`).
- Uploads EditMode + PlayMode results as `Unity-Test-Results` artifact (14-day retention).

**Build matrix:**

| Platform | Runner | `continue-on-error` | Notes |
|----------|--------|---------------------|-------|
| WebGL | ubuntu-latest | No (blocking) | Deploys to Firebase Hosting |
| Android | ubuntu-latest | Yes | Requires Android Build Support module |
| iOS | macos-latest | Yes | Requires iOS Build Support module |

**Firebase deploy (`deploy-firebase` job):**
- Runs only when pushing to `main`.
- Deploys: Firestore rules, Storage rules, Cloud Functions, Hosting (WebGL).
- Uses `firebase-tools@13` (pinned major version).

**Permissions:** `contents: read` (default); individual jobs use only what they need.

---

### content-guard.yml

Runs `npm run validate:auto-publish` — validates content bank integrity and prevents accidental auto-publishing of unsafe content. Required check on all PRs to main.

---

### codeql.yml

- **JavaScript** analysis covers `scripts/` and `firebase/functions/`.
- **C#** analysis covers `Assets/_Project/Scripts/` via `AscendantContinuum.Runtime.csproj`.
  - Runs on push to main/develop only (not PRs) — the dotnet build step is slow.
- Results appear in **Security → Code scanning alerts**.

---

### commitlint.yml

- Runs on pull requests to `main`.
- Enforces conventional commit format:
  - `type(scope): subject`
  - Example: `feat(ui): add realm selection transitions`
- Uses `.github/commitlint.config.cjs`.

---

### lint.yml

| Job | Tool | What it checks |
|-----|------|---------------|
| `actionlint` | raven-actions/actionlint | Workflow YAML syntax and expressions |
| `validate-json` | node JSON.parse | `package.json`, `Packages/manifest.json`, scripts JSON |
| `markdown-lint` | markdownlint-cli2 | All `*.md` files |
| `npm-audit` | npm audit | Critical-severity vulnerabilities (fails build) |

Add a `.markdownlint.json` at the repo root to customise rules (e.g. disable MD013 line length).

---

### release.yml

See [RELEASE.md](RELEASE.md) for the full release process.

Release notes categories are configured in `.github/release.yml` and map labels
to sections (Features, Fixes, Security, CI/CD & Automation, Documentation, Maintenance).

---

### pr-labeler.yml

- Runs on `pull_request_target` to apply labels automatically based on changed files.
- Rules live in `.github/labeler.yml`.
- Helps CODEOWNERS/review routing and improves release note categorization.

---

### daily-blog.yml

- Generates and publishes a daily blog post to `firebase/public/blog/`.
- Validates links (HTTP 200 check against `ascendant-continuum.web.app`).
- Commits and pushes with `[skip ci]` to avoid recursive triggering.
- Uses `fetch-depth: 0` for full git history needed by commit-based post generation.

---

### daily-social.yml

- Reads from `public/social/content-bank.json` (pre-populated content bank).
- Runs deduplication check before posting — **hard fails** if duplicates detected.
- Commits posting history after each run.
- Creates a GitHub issue automatically if the posting fails.

---

### daily-whatsnew.yml

- Generates What's New update from recent commits.
- Only deploys to Firebase and posts to social media if changes were detected.
- Runs `firebase-tools@13` inline (no separate install step).

---

### dev-update-social.yml

- Posts a weekly developer update every Monday at 20:00 UTC.
- Uses `fetch-depth: 0` for full git history.

---

### weekend-philosophy.yml

- Posts philosophy/reflection content Saturdays and Sundays at 16:00 UTC.
- Creates a GitHub issue on failure for visibility.

---

### weekly-digest.yml

- Generates a weekly digest every Monday at 09:00 UTC.
- Deploys to Firebase and posts to social media only if new content was generated.
- Uses `firebase-tools@13` inline.

---

### weekly-security-scan.yml

- Runs every Monday at 09:00 UTC and on any `package.json` / `package-lock.json` changes.
- **Permissions:** `contents: read, issues: write` (creates security alert issues).
- Checks for:
  - `npm audit` at `moderate` level.
  - Known compromised packages (April 2026 threat database: axios 1.14.1/0.30.4, litellm, separadordeinfocc, etc.).
  - Firebase Functions dependencies separately.
- Uploads a scan report artifact (90-day retention).
- Creates a labelled GitHub issue (`security`, `critical`) if threats are found.
- Optional `socket-scan` job (continue-on-error) for Socket.dev malware detection.

---

## Debugging Failures

### Unity tests skip with "UNITY_LICENSE not configured"
Set the `UNITY_LICENSE` secret. Obtain the license from Unity Hub:
1. Activate in Unity Hub → Licenses.
2. Locate `*.ulf` in `C:\ProgramData\Unity\Unity_v6.x.ulf` (Windows) or `/Library/Application Support/Unity/` (macOS).
3. Base64-encode and add as `UNITY_LICENSE` secret.

### Firebase deploy fails with "Error: Failed to authenticate"
The `FIREBASE_TOKEN` may be expired. Re-generate:
```bash
npx firebase-tools@13 login:ci
```
Copy the token and update the `FIREBASE_TOKEN` secret.

### Social media workflow fails with "Invalid credentials"
- **Bluesky:** Go to Settings → App Passwords and generate a new app password.
- **Mastodon:** Go to Preferences → Development → Your applications and regenerate the token.
- Update `BLUESKY_PASSWORD` / `MASTODON_ACCESS_TOKEN` secrets accordingly.

### Deduplication check fails
The content bank has duplicate posts. Run locally:
```bash
npm run validate:dedup
```
Then run `npm run content:backfill` to repair fingerprints, or remove the duplicate entry from `content-bank.json`.

### actionlint reports workflow errors
Run locally:
```bash
# Install actionlint
go install github.com/rhysd/actionlint/cmd/actionlint@latest
# Or use the binary release from https://github.com/rhysd/actionlint/releases
actionlint .github/workflows/*.yml
```

---

## Branch Protection (Recommended Settings)

Apply via **GitHub repo → Settings → Branches → Add branch protection rule** for `main`:

- [x] Require a pull request before merging
- [x] Require status checks to pass before merging
  - Required checks: `Content Guard`, `Validate Workflow Syntax (actionlint)`, `Validate JSON Files`, `npm audit (critical only)`
- [x] Require branches to be up to date before merging
- [x] Do not allow bypassing the above settings
- [x] Restrict force pushes

---

## Action SHA Reference

All pinned SHAs as of May 2026:

| Action | Tag | SHA |
|--------|-----|-----|
| `actions/checkout` | v4.2.2 | `11bd71901bbe5b1630ceea73d27597364c9af683` |
| `actions/setup-node` | v4.1.0 | `39370e3970a6d050c480ffad4ff0ed4d3fdee5af` |
| `actions/cache` | v4.1.2 | `6849a6489940f00c2f30c0fb92c6274307ccb58a` |
| `actions/upload-artifact` | v4.3.4 | `0b2256b8c012f0828dc542b3febcab082c67f72b` |
| `actions/download-artifact` | v4.1.7 | `65a9edc5881444af0b9093a5e628f2fe47ea3b2e` |
| `actions/github-script` | v7.0.1 | `60a0d83039c74a4aee543508d2ffcb1c3799cdea` |
| `github/codeql-action` | v3.28.1 | `b6a472f63d85b9c78a3ac5e89422239fc15e9b3c` |
| `game-ci/unity-test-runner` | v4 | `0ff419b913a3630032cbe0de48a0099b5a9f0ed9` |
| `game-ci/unity-builder` | v4 | `1d4ee0697f193f54668e98961d79907911f4b4f2` |
| `jlumbroso/free-disk-space` | v1.3.1 | `54081f138730dfa15788a46383842cd2f914a1be` |

Dependabot is configured to propose updates for GitHub Actions versions weekly.
When updating, replace both the SHA and the version comment on the same line.
