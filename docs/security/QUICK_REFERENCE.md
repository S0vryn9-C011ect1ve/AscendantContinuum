# Security Quick Reference Guide

## 🔒 Axios Compromise Protection - April 2026

### Status: ✅ PROTECTED

Your Ascendant Continuum project is fully protected from the axios npm compromise reported by Socket.dev.

---

## Quick Security Commands

### Main Project
```bash
# Check all security
npm run security:full-scan

# Quick audit
npm run security:audit

# Check axios version
npm run security:check-axios

# Auto-fix vulnerabilities
npm run security:audit-fix

# Socket.dev scan (manual)
npm run security:socket
```

### Socket CLI Commands (Optional)
```bash
# Check package before installing
socket npm info <package-name>

# Scan current project
socket scan .

# Check installed dependencies
socket npm list
```

### Firebase Functions
```bash
cd firebase/functions

# Check security
npm run security:audit

# Check axios/gaxios
npm run security:check-deps
```

---

## Protection Measures in Place

### 1. Version Pinning
Both `package.json` files now include security overrides:
```json
"overrides": {
  "axios": ">=1.13.5",
  "gaxios": ">=6.7.1",
  "fast-xml-parser": ">=5.5.7",
  "flatted": ">=3.4.2"
}
```

### 2. No Direct Axios Usage
- Axios is only used as transitive dependency through `megalodon`
- No direct imports in codebase
- Hardcoded API endpoints only

### 3. Isolated Execution
- GitHub Actions runs in fresh containers
- No persistent state between runs
- No user input accepted

### 4. Automated Monitoring
- Security scanner: `scripts/security/security-scan.js`
- Weekly automated scans
- Detailed reports in `logs/`

---

## Current Versions

**Main Project:**
- axios: 1.13.6 (overridden, safe)
- No vulnerabilities

**Firebase Functions:**
- gaxios: 6.7.1 & 7.1.3 (safe)
- 4 low-risk dev dependencies (accepted)

---

## What to Do When...

### 📦 New npm Security Alert
1. Run: `npm run security:full-scan`
2. Check: `logs/security-scan-report.md`
3. Review: Package versions vs minimum safe
4. Update: If needed, update package.json overrides

### 🚨 Supply Chain Compromise Reported
1. Immediately check affected package: `npm list <package>`
2. Review: `package-lock.json` for version
3. Update: Override in `package.json` if needed
4. Document: Add to security logs

### 🔄 Regular Maintenance
**Weekly:**
- [ ] Run `npm run security:full-scan`
- [ ] Review scan report

**Monthly:**
- [ ] Update dependencies: `npm update`
- [ ] Review npm audit: `npm audit`
- [ ] Check for outdated: `npm outdated`

**Before Deploy:**
- [ ] Run security scan
- [ ] Verify no new vulnerabilities
- [ ] Check axios version

---

## Emergency Contact

**For Security Issues:**
- Email: security@ascendantcontinuum.com
- DO NOT create public GitHub issues for vulnerabilities

**Response Time:**
- Critical: < 24 hours
- High: < 48 hours
- Medium: < 7 days

---

## Additional Resources

- [Full Security Scan Report](../logs/AXIOS_SECURITY_SCAN_2026-04-01.md)
- [NPM Security Docs](../docs/social/SECURITY.md)
- [Security Policy](../SECURITY.md)

---

**Last Updated:** April 1, 2026  
**Next Review:** May 1, 2026
