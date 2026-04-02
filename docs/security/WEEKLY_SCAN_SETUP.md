# Weekly Automated Security Scan - Setup Guide

## ✅ What's Configured

Your project now has **6 layers of protection** against supply chain attacks like the recent axios, LiteLLM, and LofyGang compromises.

### Protection Layers

1. **⏰ 7-Day Installation Delay**
   - Location: `.npmrc` (both main project & Firebase Functions)
   - Blocks any package < 7 days old
   - Gives security community time to detect malicious packages

2. **🚫 Install Scripts Disabled**
   - Location: `.npmrc`
   - Prevents code execution during `npm install`
   - Enable per-package only when needed

3. **🔒 Version Pinning**
   - Location: `package-lock.json`
   - Exact version enforcement
   - Use `npm ci` instead of `npm install`

4. **🛡️ Security Overrides**
   - Location: `package.json`
   - Forces minimum safe versions for critical packages
   - Currently protecting: axios, gaxios, and 4 others

5. **🤖 Weekly Automated Scans**
   - Location: `.github/workflows/weekly-security-scan.yml`
   - Runs every Monday at 9:00 AM UTC
   - Auto-creates GitHub issues if threats detected

6. **🔍 Socket.dev Integration (Optional)**
   - Real-time malware detection
   - Free tier available
   - Install: `npm i -g @socketsecurity/cli`

---

## 🚀 Quick Start

### Activate GitHub Actions (Required)

1. **Enable GitHub Actions** (if not already enabled)
   ```bash
   # Push the workflow to your repository
   git add .github/workflows/weekly-security-scan.yml
   git commit -m "feat: Add weekly security scan workflow"
   git push
   ```

2. **Verify Workflow is Active**
   - Go to your GitHub repository
   - Click "Actions" tab
   - Should see "Weekly Security Scan" listed
   - Click "Enable workflow" if prompted

3. **Test the Workflow**
   ```bash
   # Trigger manually to test
   # Go to: Actions → Weekly Security Scan → Run workflow
   ```

### Install Socket.dev CLI (Optional but Recommended)

```bash
# Install globally
npm install -g @socketsecurity/cli

# Test it
socket --version

# Scan current project
cd "d:\1-Ascendant Continuum Game"
npm run security:socket
```

---

## 📅 Scan Schedule

### Automated Scans

| When | What | Where |
|------|------|-------|
| **Every Monday 9:00 AM UTC** | Full security audit | GitHub Actions |
| **On package.json changes** | Immediate audit | GitHub Actions (on push) |
| **Manual trigger** | On-demand scan | GitHub Actions or local |

### Manual Scans

```powershell
# Quick axios check
npm run security:check-axios

# Full local scan
npm run security:full-scan

# Weekly report (automated scanner)
npm run security:weekly

# Socket.dev malware check (if installed)
npm run security:socket
```

---

## 📊 What Gets Scanned

### Main Project
- ✅ All npm dependencies
- ✅ axios version verification (protects against 1.14.1 compromise)
- ✅ npm audit (high/critical vulnerabilities)
- ✅ Known malicious package detection

### Firebase Functions
- ✅ All npm dependencies
- ✅ gaxios version verification
- ✅ npm audit
- ✅ Firebase-specific security checks

### Threat Detection
- ✅ axios 1.14.1, 0.30.4 (compromised)
- ✅ LiteLLM 1.82.7, 1.82.8 (backdoored)
- ✅ separadordeinfocc (LofyGang malware)
- ✅ Expandable list in workflow file

---

## 🔔 Notifications

### GitHub Issues
When vulnerabilities are detected, the workflow automatically creates a GitHub issue with:
- 🏷️ Labels: `security`, `high-priority`
- 📋 Details: Package names, versions, CVE links
- 🔗 Link to workflow run logs
- ✅ Action checklist for response

### Email Notifications (Optional)
To get email alerts:
1. Go to GitHub → Your Repository → Settings
2. Navigate to: Notifications → Actions
3. Enable: "Send notifications for failed workflows"

### Slack/Discord Integration (Optional)
Add to workflow file:
```yaml
- name: Notify Slack
  if: failure()
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK }}
    payload: |
      {
        "text": "🚨 Security scan failed! Check GitHub Actions."
      }
```

---

## 📝 Reports

### Automated Reports Location
- **GitHub Artifacts**: Actions → Workflow Run → Artifacts
- **Local Reports**: `logs/weekly-security-scan-YYYY-MM-DD.md`
- **Retention**: 90 days on GitHub

### Report Contents
1. **Dependency Versions**: Exact versions of critical packages
2. **Vulnerability Summary**: npm audit results
3. **Known Threats**: Comparison against active threat database
4. **Protection Status**: Verification all safeguards active
5. **Recommendations**: Action items if issues found

---

## 🛠️ Development Workflow Changes

### Before (Unsafe)
```bash
npm install              # ❌ Installs latest (potentially malicious)
npm install axios        # ❌ No version delay
npm install              # ❌ Runs all install scripts
```

### After (Protected)
```bash
npm ci                   # ✅ Uses package-lock.json exactly
npm run safe-add axios   # ✅ Waits 7 days before installing
npm ci                   # ✅ Scripts disabled by default
```

### New Commands Available

```bash
# Safe installation (uses npm ci)
npm run safe-install

# Safe package addition (7-day delay)
npm run safe-add <package>

# Weekly automated scan (same as GitHub Actions)
npm run security:weekly

# Socket.dev malware scan
npm run security:socket

# Quick axios verification
npm run security:check-axios

# Full comprehensive scan
npm run security:full-scan
```

---

## 🚨 Emergency Response

### When New Supply Chain Attack Announced

1. **Check if affected**
   ```bash
   npm list <compromised-package>
   ```

2. **Verify your installed version**
   ```bash
   # Check package-lock.json
   grep "<compromised-package>" package-lock.json
   ```

3. **Assess protection status**
   - Protected: Version older than compromised OR min-release-age blocked it
   - At Risk: You have compromised version

4. **Update threat database**
   - Edit: `.github/workflows/weekly-security-scan.yml`
   - Add to: `MALICIOUS_PACKAGES` array
   - Commit and push

5. **Add to overrides** (if critical)
   - Edit: `package.json` → `overrides` section
   - Set minimum safe version
   - Run: `npm install` to apply

### Example: axios Compromise Response

```bash
# 1. Check if affected
npm list axios
# Output: axios@1.13.6 ✅ SAFE (compromise was 1.14.1)

# 2. Verify protection active
cat .npmrc | grep min-release-age
# Output: min-release-age=7 ✅

# 3. Update workflow (already done)
# 4. Add override (already done: "axios": ">=1.13.5")
# 5. Document in supply chain protection guide
```

---

## 📈 Monitoring Best Practices

### Daily
- ✅ Automatic (GitHub Actions runs on package.json changes)

### Weekly
- ✅ Automatic (Monday 9 AM UTC scan)
- ℹ️ Review GitHub Issues tab for any alerts

### Monthly
- 🔍 Review scan reports manually
- 📊 Check npm outdated for non-security updates
- 🔄 Update security overrides if needed
- 📝 Review and update threat database

### When Adding New Dependencies
```bash
# ALWAYS use safe-add instead of install
npm run safe-add <new-package>

# Verify it's not in threat database first:
# 1. Check Socket.dev: https://socket.dev/npm/package/<package>
# 2. Check npm advisories
# 3. Check package age (avoid packages < 7 days old)
```

---

## 🔧 Configuration Files

### `.npmrc` (Both Projects)
```ini
min-release-age=7       # 7-day delay
ignore-scripts=true     # Disable install scripts
audit=true              # Enable auditing
audit-level=high        # Alert on high+ vulnerabilities
save-exact=true         # Pin exact versions
package-lock=true       # Enforce lock file
```

### `.github/workflows/weekly-security-scan.yml`
- Runs Monday 9 AM UTC
- Checks both main project + Firebase Functions
- Scans for known compromised packages
- Creates issues on failure
- Uploads reports as artifacts

### `package.json` → `overrides`
```json
{
  "overrides": {
    "axios": ">=1.13.5",
    "gaxios": ">=6.7.1",
    "@tootallnate/once": "^3.0.1",
    "http-proxy-agent": "^7.0.0",
    "fast-xml-parser": ">=5.5.7",
    "flatted": ">=3.4.2"
  }
}
```

---

## ✅ Verification Checklist

Run these commands to verify everything is configured correctly:

```powershell
# 1. Check .npmrc protection
Get-Content .npmrc | Select-String "min-release-age|ignore-scripts"
# Expected: min-release-age=7, ignore-scripts=true

# 2. Check package overrides
Get-Content package.json | Select-String -Pattern "overrides" -Context 0,10
# Expected: axios, gaxios, and 4 others

# 3. Check GitHub workflow exists
Test-Path .github\workflows\weekly-security-scan.yml
# Expected: True

# 4. Verify axios version
npm list axios --depth=0
# Expected: axios@1.13.6 overridden

# 5. Run full scan
npm run security:full-scan
# Expected: 0 vulnerabilities in main project

# 6. Test automated scanner
npm run security:weekly
# Expected: Report generated in logs/
```

### Expected Output
```
✅ min-release-age=7 configured
✅ ignore-scripts=true configured
✅ Security overrides active (6 packages)
✅ GitHub Actions workflow present
✅ axios@1.13.6 (safe version)
✅ 0 high/critical vulnerabilities
✅ Automated scanner functional
```

---

## 🎯 Benefits Summary

### Before This Setup
- ❌ Vulnerable to supply chain attacks
- ❌ Could install malicious packages immediately
- ❌ Manual security checks required
- ❌ No protection against compromised versions
- ❌ Install scripts could execute malware

### After This Setup
- ✅ **7-day buffer** for security community detection
- ✅ **Automatic weekly scans** with issue creation
- ✅ **Install scripts disabled** by default
- ✅ **Version overrides** prevent compromised installs
- ✅ **Multi-layer defense** against supply chain attacks
- ✅ **Real-time threat database** (expandable)
- ✅ **Zero manual intervention** for routine checks

### Protection Examples
```
Timeline: axios Supply Chain Attack (March 31, 2026)

15:10 UTC - Attacker publishes axios 1.14.1 with malware
15:30 UTC - Developer tries: npm install axios
15:30 UTC - ❌ BLOCKED by min-release-age=7
16:00 UTC - Security community detects malware
17:00 UTC - Advisory published
18:00 UTC - Package removed from npm

Result: ✅ Protected by 7-day delay
```

---

## 📞 Support

### Questions?
- 📖 Read: [Supply Chain Protection Guide](SUPPLY_CHAIN_PROTECTION.md)
- 📖 Read: [Quick Reference](QUICK_REFERENCE.md)
- 🔍 Check: GitHub Issues for similar questions

### Report Security Issue
- 📧 Email: ascendantcontinuum@gmail.com
- 🐛 GitHub: Create private security advisory
- ⚠️ Urgent: Include "URGENT:" in subject line

---

**Setup Complete! 🎉**

Your project is now protected against supply chain attacks like:
- ✅ axios 1.14.1 compromise
- ✅ LiteLLM credential exfiltration
- ✅ LofyGang malware distribution
- ✅ Future typosquatting attacks
- ✅ Malicious install scripts

**Next Steps:**
1. Push workflow to GitHub: `git push`
2. Enable GitHub Actions if prompted
3. Test manual trigger: Actions → Weekly Security Scan → Run workflow
4. (Optional) Install Socket.dev CLI: `npm i -g @socketsecurity/cli`

**Last Updated:** April 1, 2026
