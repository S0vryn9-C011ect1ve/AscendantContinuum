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

## Bug Bounty

We currently do not have a formal bug bounty program, but responsible disclosure of serious vulnerabilities will be acknowledged and rewarded on a case-by-case basis.
