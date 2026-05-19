# Troubleshooting

Common issues and quick fixes for local development and CI.

## 1. Unity Tests Skipped in CI

Symptom:
- Build passes but Unity test steps show a warning about missing license.

Cause:
- `UNITY_LICENSE` secret is not configured.

Fix:
1. Add `UNITY_LICENSE` in GitHub Actions secrets.
2. Ensure `UNITY_EMAIL` and `UNITY_PASSWORD` are valid.
3. Re-run `build-deploy` workflow.

## 2. Firebase Deploy Authentication Failure

Symptom:
- `firebase deploy` fails with auth/token errors.

Fix:
```powershell
npx firebase-tools@13 login:ci
```
Update `FIREBASE_TOKEN` secret with the new token.

## 3. PR Fails Commitlint Check

Symptom:
- `commitlint` job fails on pull request.

Fix:
- Rewrite commit messages to conventional format:
  - `feat(ui): add realm transition animation`
  - `fix(build): correct android keystore path`

If needed:
```powershell
git rebase -i HEAD~N
```
Then reword commits and push with lease.

## 4. PR Labels Not Applied

Symptom:
- Expected labels are missing after opening PR.

Checks:
1. Confirm `.github/labeler.yml` includes paths touched by your PR.
2. Confirm workflow `.github/workflows/pr-labeler.yml` succeeded.
3. Verify PR is not from a restricted fork context.

## 5. Security Scan Flags Dependency Vulnerabilities

Symptom:
- `weekly-security-scan.yml` creates a security issue.

Fix workflow:
1. Inspect reported package/version.
2. Add or update `overrides` in affected `package.json`.
3. Regenerate lockfile with npm.
4. Re-run scans.

## 6. Repository Structure Validation Warning

Symptom:
- `validate:structure` reports unexpected root files.

Fix:
- Move operational docs/config under approved folders (`docs/`, `.github/`, `scripts/`).
- Do not add new operational markdown at repository root.

## 7. Android Build Signing Fails

Symptom:
- Unity Android build fails at signing.

Fix:
- Verify secrets:
  - `ANDROID_KEYSTORE_BASE64`
  - `ANDROID_KEYSTORE_PASS`
  - `ANDROID_KEY_ALIAS`
  - `ANDROID_KEY_ALIAS_PASS`
- Confirm keystore can be decoded and alias exists.

## 8. Local npm Warnings About `min-release-age`

Symptom:
- npm outputs: `Unknown project config "min-release-age"`.

Explanation:
- Current npm version does not support that config key yet.
- This is non-blocking and does not break builds.

Action:
- Keep npm updated and monitor upcoming npm major behavior changes.

## 9. Link Validation Failure

Symptom:
- `validate:links` fails.

Fix:
1. Check target URL status manually.
2. Update stale links in docs or automation content.
3. Re-run:
```powershell
npm run validate:links
```

## 10. Where to Escalate

- Security concerns: follow `SECURITY.md` reporting instructions.
- CI/CD details: `docs/CI_CD.md`
- Release process: `docs/RELEASE.md`
- Contributor policies: `CONTRIBUTING.md`
