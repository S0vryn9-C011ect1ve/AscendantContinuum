# Socket.dev Setup Guide (No npm Install Required)

## ⚠️ Why NOT npm install?

**DON'T DO THIS:**
```bash
npm install -g @socketsecurity/cli  # ❌ IRONIC: Installing security tool via npm!
```

**The Problem:** We're protecting against npm supply chain attacks, so installing Socket.dev via npm defeats the purpose. If npm is compromised, the security tool could be compromised too.

---

## ✅ Recommended: GitHub App Integration (Zero npm Install)

### Step 1: Install Socket.dev GitHub App

1. **Go to:** https://socket.dev/
2. **Click:** "Get Started" or "Add to GitHub"
3. **Select:** Ascendant Continuum repository
4. **Authorize:** Give Socket.dev read access to:
   - `package.json`
   - `package-lock.json`
   - Pull requests
   - Commit status checks

### Step 2: Configure Repository Protection

Socket.dev will automatically:
- ✅ Scan all pull requests for malicious packages
- ✅ Block PRs with high-risk dependencies
- ✅ Comment on PRs with security findings
- ✅ Monitor dependency updates in real-time
- ✅ Alert on supply chain attacks (no 7-day delay needed!)

### Step 3: Set Alert Level

In Socket.dev dashboard:
1. Navigate to repository settings
2. Set alert threshold: **"Medium" or "High"**
3. Enable: **"Block on critical findings"**
4. Configure notifications: **Email + GitHub issues**

---

## 📊 What Socket.dev Monitors

### Real-Time Threats
- Malicious package detection (faster than 7-day delay)
- Typosquatting attempts
- Install script analysis
- Network requests during install
- Filesystem access patterns
- Obfuscated code detection

### Supply Chain Risks
- Maintainer changes
- New dependencies added
- License violations
- Unmaintained packages
- High churn (frequent updates = suspicious)

---

## 🔔 Alert Configuration

### GitHub Issue Alerts (Built-in)

Socket.dev automatically creates GitHub issues for:
- 🔴 **Critical:** Malicious packages, typosquatting
- 🟠 **High:** Risky install scripts, suspicious network calls
- 🟡 **Medium:** Unmaintained dependencies, license issues

### Email Alerts

Configure in Socket.dev dashboard:
1. **Settings** > **Notifications**
2. **Add email:** your-email@example.com
3. **Set frequency:** Real-time (for critical) + Daily digest

### Slack/Discord Integration (Optional)

Socket.dev supports webhooks:
1. **Settings** > **Integrations** > **Webhooks**
2. **Add webhook URL** from Slack/Discord
3. **Select events:** Critical alerts only

---

## 🎯 How It Works with Our Existing Protections

| Protection Layer | Purpose | Coverage |
|------------------|---------|----------|
| **Socket.dev** | Real-time malware detection | Immediate (0-day) |
| **7-Day Delay** | Community detection buffer | 7-day window |
| **GitHub Actions** | Weekly automated audits | Every Monday |
| **Version Overrides** | Force safe minimum versions | Continuous |

**Result:** Triple-layered defense
- Socket.dev catches threats **instantly**
- 7-day delay provides **buffer** if Socket.dev misses something
- Weekly scans provide **ongoing monitoring**

---

## 📋 Setup Checklist

- [ ] Install Socket.dev GitHub App (https://socket.dev/)
- [ ] Authorize for Ascendant Continuum repository
- [ ] Set alert level to "Medium" or "High"
- [ ] Enable "Block on critical findings"
- [ ] Configure email notifications
- [ ] Test with a dummy PR adding a package
- [ ] Verify alerts arrive in GitHub & email
- [ ] (Optional) Configure Slack/Discord webhooks
- [ ] Document Socket.dev credentials in password manager

---

## 🧪 Testing Socket.dev

### Test 1: Create Test PR

```bash
# Create test branch
git checkout -b test-socket-dev

# Try adding a suspicious package (Socket.dev will block it)
npm install --save-dev typosquatting-package-test

# Create PR - Socket.dev should comment with warnings
git add package.json package-lock.json
git commit -m "test: Socket.dev integration"
git push origin test-socket-dev
```

**Expected:** Socket.dev comments on PR with security findings

### Test 2: Check Dashboard

1. Visit: https://socket.dev/dashboard
2. Navigate to: Ascendant Continuum repo
3. Review: Dependency health score
4. Check: Recent alerts (should be 0 if clean)

---

## 💰 Pricing

- **Free Tier:** Open source projects (if Ascendant Continuum is public)
- **Pro Tier:** $49/month for private repos (optional)
- **Features:** Real-time scanning, PR blocking, unlimited repos

**Our Recommendation:** Start with free tier, upgrade only if going private.

---

## 🔒 Security Considerations

### What Access Does Socket.dev Get?

- ✅ **Read-only** access to package files
- ✅ **Commit status** updates (to block PRs)
- ✅ **Issue creation** (for alerts)
- ❌ **NO write** access to code
- ❌ **NO access** to secrets/credentials

### Is Socket.dev Safe?

- ✅ Trusted by **Fortune 500 companies**
- ✅ Open source security team
- ✅ NO npm installation required (GitHub App only)
- ✅ Documented security practices
- ✅ SOC 2 compliant

---

## 🚨 Alternative: Manual Web-Based Scanning

If you don't want GitHub integration:

1. **Visit:** https://socket.dev/npm/package/[package-name]
2. **Enter** package name (e.g., axios)
3. **Review** security report
4. **Check** before installing anything new

**Example:**
```
https://socket.dev/npm/package/axios/1.13.6
```

---

## 📞 Support

- **Socket.dev Docs:** https://docs.socket.dev/
- **GitHub Issues:** https://github.com/SocketDev/socket/issues
- **Email:** support@socket.dev
- **Discord:** https://discord.gg/socketdev

---

## ✅ Bottom Line

**Socket.dev GitHub App = Zero npm Install Risk**

- No CLI installation required
- No npm package to trust
- Pure GitHub integration
- Real-time protection
- Complements our existing 7-day delay

**Next Step:** Install the GitHub App → https://socket.dev/

---

**Last Updated:** April 6, 2026  
**For:** Ascendant Continuum (Solo Dev Focus)
