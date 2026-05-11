# Ascendant Continuum — Social Media Automation System

Automated daily content posting to Bluesky and Mastodon for pre-launch audience building.

Canonical note: `public/social/` is legacy automation state, not the website root. Do not add website files there.

Content quality note: use `docs/social/CONTENT_GOVERNANCE.md` as the canonical source for factual-claim, duplication, and optimization rules.

---

## 📋 Overview

This system automatically:
- Posts 1x daily to Bluesky and Mastodon (2 PM UTC / 9 AM EST)
- Rotates through 100 pre-written content items (dev updates, tutorials, philosophy, behind-the-scenes)
- Tracks posting history and content usage
- Uses proven viral hooks for engagement
- Maintains brand voice (warm, educational, community-first)

**Content Distribution (Pre-Launch):**
- Dev Updates: 40%
- Dev Education: 30%
- Design Philosophy: 20%
- Behind-the-Scenes: 10%

---

## 🚀 Quick Start

### 1. Install Dependencies

```bash
npm install
```

### 2. Set Up Environment Variables

Copy `.env.example` to `.env` and fill in your credentials:

```bash
cp .env.example .env
```

Required variables:
```env
BLUESKY_IDENTIFIER=your-handle.bsky.social
BLUESKY_PASSWORD=your-app-password
MASTODON_INSTANCE=https://mastodon.social
MASTODON_API_URL=https://mastodon.social
MASTODON_ACCESS_TOKEN=your-access-token
```

The current repo uses a mixed legacy naming set. Keep both `MASTODON_INSTANCE` and `MASTODON_API_URL` populated until workflow cleanup is complete.

### 3. Test Posting Modules

Test Bluesky:
```bash
npm run social:test-bluesky
```

Test Mastodon:
```bash
npm run social:test-mastodon
```

### 4. Run Scheduler Manually

```bash
npm run social:post
```

Or in dry-run mode (no actual posting):
```bash
DRY_RUN=true npm run social:post
```

---

## 🔐 GitHub Secrets Setup

For automation via GitHub Actions, add these secrets to your repository:

**Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Description | How to Get |
|-------------|-------------|------------|
| `BLUESKY_IDENTIFIER` | Your Bluesky handle (e.g., `username.bsky.social`) | Your Bluesky profile |
| `BLUESKY_PASSWORD` | App-specific password | Bluesky Settings → App Passwords → Create |
| `MASTODON_INSTANCE` | Your Mastodon instance URL | e.g., `https://mastodon.social` |
| `MASTODON_API_URL` | Legacy workflow alias for Mastodon base URL | Same as `MASTODON_INSTANCE` |
| `MASTODON_ACCESS_TOKEN` | API access token | Mastodon Settings → Development → New Application |

### Getting Mastodon Access Token

1. Go to your Mastodon instance → Settings → Development
2. Click "New Application"
3. Name: "Ascendant Continuum Automation"
4. Scopes: `read` + `write:statuses` + `write:media`
5. Submit → Copy "Your access token"

---

## 📁 Project Structure

```
scripts/
├── automation/
│   ├── content-scheduler.js      # Main orchestrator
│   └── viral-hooks-gaming.js     # Hook library (50+ hooks)
├── content/
│   ├── build-content-bank.js     # Content bank builder
│   ├── dev-update-generator.js   # Git commit → dev update (TODO)
│   └── [other generators...]     # (TODO)
├── posting/
│   ├── post-to-bluesky.js        # Bluesky ATP API
│   └── post-to-mastodon.js       # Mastodon API
└── analytics/
    └── social-analytics.js       # Engagement tracking (TODO)

public/social/
├── content-bank.json             # 100 pre-written content items
└── posting-history.json          # Posting log + stats

Future target location after migration: `tools/content-automation/state/` or equivalent canonical automation-state path.

.github/workflows/
├── daily-social.yml              # Daily posting (2 PM UTC)
└── test-social.yml               # Manual test workflow
```

---

## ⚙️ GitHub Actions Workflows

### Daily Social Media Post
- **Schedule:** 2 PM UTC daily (9 AM EST, 6 AM PST)
- **Trigger:** Automatic via cron + manual dispatch
- **Action:** Picks content → posts to Bluesky + Mastodon → commits history

### Test Social Media Posting
- **Trigger:** Manual only
- **Action:** Tests credentials by posting test message
- **Use:** Verify setup before enabling automation

---

## 📝 Content Management

### Content Bank (`public/social/content-bank.json`)

100 pre-written items organized by type. Each item has:
- `id`: Unique identifier
- `type`: devUpdate, devEducation, designPhilosophy, behindScenes, loreSnippet
- `hook`: Scroll-stopping hook (from viral-hooks-gaming.js)
- `body`: Main content
- `platforms`: Which platforms to post to
- `priority`: high, medium, low
- `used`: Boolean tracking

**To add new content:**
1. Edit `content-bank.json`
2. Add new item with next ID number
3. Set `"used": false`
4. Commit changes

### Hook Library (`scripts/automation/viral-hooks-gaming.js`)

50+ proven hooks organized by content type:
- Dev Updates: "Most games add X. We made it a feature."
- Dev Education: "Steal this Unity system for your game."
- Design Philosophy: "Building a game that respects your time."
- Behind-the-Scenes: "Friday dev diary: here's what broke."

**Hook rotation:** Automatically rotates every 30 days (no repeats within month).

---

## 📊 Posting History

Tracked in `public/social/posting-history.json`:
- Individual post records (content ID, timestamp, platforms, URLs)
- Aggregate stats (total posts, by platform, by type)
- Used for rotation logic and analytics

**View stats:**
```bash
cat public/social/posting-history.json | jq '.stats'
```

---

## 🧪 Testing

### Local Scheduler Alternative

If GitHub Actions is unavailable or undesirable for scheduled posting, use the local PowerShell automation:

```powershell
.\setup-local-automation.ps1
.\scripts\automation\run-daily-social.ps1 -DryRun
```

This keeps scheduling on the local machine while preserving the same content/state flow.

### Test Individual Modules

```bash
# Test Bluesky posting
node scripts/posting/post-to-bluesky.js

# Test Mastodon posting
node scripts/posting/post-to-mastodon.js

# Test scheduler (dry run)
DRY_RUN=true node scripts/automation/content-scheduler.js
```

### Test via GitHub Actions

1. Go to Actions tab in GitHub
2. Select "Test Social Media Posting"
3. Click "Run workflow"
4. Choose platform (both / bluesky / mastodon)
5. Monitor logs

---

## 🔄 Content Rotation Logic

1. **Unused content prioritized:** Posts marked `"used": false` selected first
2. **Time-based preferences:**
   - Weekdays (Mon-Fri): Dev updates + dev education
   - Weekends (Sat-Sun): Design philosophy + lore
3. **Priority sorting:** high > medium > low
4. **Diversity enforcement:** Avoids posting same type consecutively
5. **Pool reset:** When all content used, resets `used` flags

---

## 🎯 Platform-Specific Behavior

### Bluesky
- **Character limit:** 300
- **Threading:** Auto-threads if content > 300 chars
- **Style:** Conversational, technical, community-focused
- **Media:** Supports image uploads

### Mastodon
- **Character limit:** 500
- **Threading:** Auto-threads if content > 500 chars
- **Content warnings:** Auto-detects mental health keywords
- **Style:** Thoughtful, philosophical, anti-corporate

---

## 📈 Analytics (TODO)

Future analytics features:
- Engagement metrics (likes, reposts, replies)
- Hook performance tracking
- Best posting times analysis
- Monthly performance reports

---

## 🐛 Troubleshooting

### "Authentication failed"
- Verify credentials in `.env` or GitHub Secrets
- Bluesky: Regenerate app password
- Mastodon: Check access token scopes

### "No content available"
- Check `content-bank.json` has `"used": false` items
- Reset pool: Set all items to `"used": false`

### "Rate limit exceeded"
- Bluesky: Max ~300 posts/hour (unlikely to hit)
- Mastodon: Varies by instance (default: 300/5min)
- Increase `postDelay` in content-scheduler.js

### Workflow not running
- Check cron schedule in `.github/workflows/daily-social.yml`
- Verify GitHub Actions enabled in repo settings
- Check workflow logs for errors

---

## 🔮 Roadmap

**Phase 1: Foundation** ✅
- [x] Core posting modules (Bluesky, Mastodon)
- [x] Content bank (100 items)
- [x] Hook library (50+ hooks)
- [x] Content scheduler
- [x] GitHub Actions automation

**Phase 2: Intelligence** (Next)
- [ ] Dev update generator (git commits → posts)
- [ ] Analytics tracking
- [ ] Hook performance optimization
- [ ] A/B testing framework

**Phase 3: Post-Launch Pivot**
- [ ] Player content resharing
- [ ] Gameplay tip generator
- [ ] Community prompt generator
- [ ] Facebook/X/Reddit integration (manual posting guides)

---

## 📞 Support

Questions? Check:
- [CONSTITUTION.md](../../CONSTITUTION.md) — Design philosophy
- [SOUL.md](../../SOUL.md) — Brand voice guidelines
- [Project Summary](../../PROJECT_SUMMARY.md) — Game overview

---

## 📜 License

Part of Ascendant Continuum Game project. See [LICENSE](../../LICENSE).

---

**Last Updated:** 2026-03-13  
**System Version:** 1.0.0 (Pre-Launch)
