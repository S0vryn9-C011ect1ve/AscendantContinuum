# Security Policy

Canonical security implementation and supply-chain controls are documented in `docs/security/SUPPLY_CHAIN_PROTECTION.md` and `docs/security/WEEKLY_SCAN_SETUP.md`.

## 🔒 Latest Security Status

**Last Scan:** April 1, 2026  
**Status:** ✅ **SECURE** - All systems protected  
**Details:** See [Axios Security Scan Report](logs/AXIOS_SECURITY_SCAN_2026-04-01.md)

### Quick Security Check
```bash
npm run security:full-scan
```

---

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

**DO NOT create a public GitHub issue for security vulnerabilities.**

Instead, email: **security@ascendantcontinuum.com**

Include:
- Description of vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if known)

We will respond within 48 hours and aim to patch critical issues within 7 days.

## Security Updates

Security patches are released as soon as possible after verification. Users will be notified via:
- In-app notification
- Email (if account linked)
- Discord announcement

## Operational References

- Weekly automated scan setup: `docs/security/WEEKLY_SCAN_SETUP.md`
- Supply-chain controls: `docs/security/SUPPLY_CHAIN_PROTECTION.md`
- Social automation credential handling: `docs/social/README.md`
- CI/CD pipeline and secrets: `docs/CI_CD.md`

---

## Firebase Configuration Files

`Assets/google-services.json` (Android) and `Assets/GoogleService-Info.plist` (iOS) contain
Firebase project identifiers and API keys. **These files must never be committed to the repository.**

Both paths are listed in `.gitignore`. If you find them tracked by git, remove them with:

```bash
git rm --cached Assets/google-services.json Assets/google-services.json.meta
git rm --cached Assets/GoogleService-Info.plist Assets/GoogleService-Info.plist.meta
git commit -m "security: remove Firebase config files from tracking"
```

Obtain these files from Firebase Console or a secure secrets manager and place them locally
before running Unity builds. CI builds receive them through environment variables or build scripts.

---

## Secret Rotation Policy

Rotate the following secrets **at minimum every 90 days**, or immediately after any suspected exposure:

| Secret | Service | Priority |
|--------|---------|---------|
| `UNITY_LICENSE` | Unity Cloud / personal license | High |
| `UNITY_EMAIL` / `UNITY_PASSWORD` | Unity account | Critical — consider migrating to `.ulf` file-based license to eliminate password |
| `ANDROID_KEYSTORE_BASE64` / `ANDROID_KEYSTORE_PASS` / `ANDROID_KEY_ALIAS_PASS` | Android signing | Critical — rotation requires a new keystore and Play Console update |
| `FIREBASE_TOKEN` | Firebase CLI | Medium — generate a service-account key with deployment-only IAM roles instead |
| `BLUESKY_PASSWORD` | Bluesky | High |
| `MASTODON_ACCESS_TOKEN` | Mastodon | Medium (scoped token) |
| `DISCORD_WEBHOOK_URL` | Discord | Medium |

Rotate secrets via: **GitHub repo → Settings → Secrets and variables → Actions.**

## Bug Bounty

We currently do not have a formal bug bounty program, but responsible disclosure of serious vulnerabilities will be acknowledged and rewarded on a case-by-case basis.
