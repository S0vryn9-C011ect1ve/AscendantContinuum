# Social Media Automation - Testing Guide

## 🚀 Quick Start

### 1. Setup Credentials

Copy `.env.example` to `.env` and fill in your credentials:

```bash
cp .env.example .env
```

Required credentials:
- **Bluesky**: Get app password from https://bsky.app/settings/app-passwords
- **Mastodon**: Create access token at https://your-instance.social/settings/applications
- **Discord**: Create webhook in your announcements channel settings

### 2. Install Dependencies

```bash
npm install
```

### 3. Test Generators (No Posting)

Test each content generator individually:

```bash
# Dev update generator (checks recent commits)
npm run social:dev-update

# Design philosophy generator
npm run content:philosophy

# Behind-the-scenes generator
npm run content:behind-scenes

# Lore snippet generator
npm run content:lore

# Dev education generator
npm run content:education
```

### 4. Test Posting Modules (Dry Run)

Test without actually posting:

```bash
# Test full scheduler in dry-run mode
DRY_RUN=true npm run social:post

# Or use the test-all script
npm run social:test-all
```

### 5. Test Real Posting (One Platform)

Test with real posting to verify credentials:

```bash
# Test Bluesky posting
node scripts/posting/post-to-bluesky.js

# Test Mastodon posting
node scripts/posting/post-to-mastodon.js

# Test Discord webhook
node scripts/posting/post-to-discord.js
```

### 6. Run Full Scheduler

Once everything works, run the full scheduler:

```bash
npm run social:post
```

---

## 📋 NPM Scripts Reference

| Command | Description |
|---------|-------------|
| `npm run social:post` | Run full content scheduler (posts to all platforms) |
| `npm run social:dev-update` | Generate dev update from recent commits |
| `npm run social:test-all` | Test scheduler in dry-run mode (no actual posting) |
| `npm run social:test-bluesky` | Test Bluesky posting module |
| `npm run social:test-mastodon` | Test Mastodon posting module |
| `npm run social:test-discord` | Test Discord webhook posting |
| `npm run content:build-bank` | Rebuild content bank from scratch |
| `npm run content:philosophy` | Generate philosophy post |
| `npm run content:behind-scenes` | Generate behind-the-scenes post |
| `npm run content:lore` | Generate lore snippet |
| `npm run content:education` | Generate dev education post |

---

## 🔧 Troubleshooting

### "BLUESKY_IDENTIFIER not set"

Make sure you:
1. Created `.env` file (copy from `.env.example`)
2. Filled in `BLUESKY_IDENTIFIER` and `BLUESKY_PASSWORD`
3. Used app password (not account password) from Bluesky settings

### "401 Unauthorized" from Mastodon

Check that:
1. `MASTODON_INSTANCE` includes full URL with https://
2. `MASTODON_ACCESS_TOKEN` is valid (recreate if expired)
3. Access token has `write:statuses` permission

### "Discord API error: 404"

Verify:
1. Webhook URL is complete and correct
2. Webhook hasn't been deleted in Discord
3. Bot has permission to post in that channel

### "No content available"

If content bank is empty:
```bash
npm run content:build-bank
```

This regenerates the 100-item content bank from scratch.

### "Git commits not found"

The dev update generator needs git history. Make sure:
1. You're in a git repository
2. There are commits in the past 24 hours
3. Git is installed and accessible

---

## 🎯 GitHub Actions Setup

### Required Secrets

Go to **Settings → Secrets and variables → Actions** and add:

| Secret Name | Description | Where to get it |
|-------------|-------------|-----------------|
| `BLUESKY_IDENTIFIER` | Your handle.bsky.social | Bluesky account |
| `BLUESKY_PASSWORD` | App password | https://bsky.app/settings/app-passwords |
| `MASTODON_INSTANCE` | https://your-instance.social | Your Mastodon instance URL |
| `MASTODON_ACCESS_TOKEN` | Access token | Instance settings → Applications |
| `DISCORD_WEBHOOK_URL` | Full webhook URL | Discord channel settings → Integrations |

### Test Workflows Manually

Workflows can be triggered manually before enabling automatic scheduling:

1. Go to **Actions** tab in GitHub
2. Select workflow (e.g., "Daily Social Media Posting")
3. Click "Run workflow" button
4. Monitor output in real-time

### Workflow Schedule

| Workflow | Schedule | Purpose |
|----------|----------|---------|
| **daily-social.yml** | Daily at 2 PM UTC (9 AM EST) | Posts curated content from bank |
| **dev-update-social.yml** | Weekdays at 8 PM UTC (3 PM EST) | Posts dev updates from commits |
| **weekend-philosophy.yml** | Sat/Sun at 4 PM UTC (11 AM EST) | Posts design philosophy threads |

### Disable Automatic Posting

Comment out the `schedule:` section in workflow files:

```yaml
on:
  # schedule:
  #   - cron: '0 14 * * *'
  workflow_dispatch: # Manual trigger still works
```

---

## 📊 Content Management

### Content Bank Structure

Located at: `public/social/content-bank.json`

```json
{
  "content": [
    {
      "id": 1,
      "type": "devUpdate",
      "hook": "Most games add accessibility as an option. We made it a feature.",
      "body": "Content body here...",
      "platforms": ["bluesky", "mastodon", "discord"],
      "priority": "high",
      "used": false
    }
  ]
}
```

### Content Types

- **devUpdate** (40%): Development progress, feature completions
- **devEducation** (30%): Unity tutorials, technical insights
- **designPhilosophy** (20%): Ethical gaming, accessibility
- **behindScenes** (10%): Architecture decisions, tool choices
- **loreSnippet**: Worldbuilding teasers (for weekends)

### Adding New Content

Edit `public/social/content-bank.json` manually or regenerate:

```bash
npm run content:build-bank
```

### Content Rotation

- Content is marked as `"used": true` after posting
- When all content is used, pool resets automatically
- Hooks rotate every 30 days to avoid repetition

---

## 🔍 Monitoring

### Posting History

Located at: `public/social/posting-history.json`

Logs every post with:
- Content ID and type
- Hook used
- Platforms posted to
- Success/failure status
- Timestamps

### Analytics (Future)

Analytics tracking is planned for:
- Engagement rates per platform
- Hook performance optimization
- Content type effectiveness
- Follower growth tracking

Stay tuned for `npm run social:analytics` and `npm run social:dashboard` commands!

---

## 🆘 Getting Help

### Check Logs

GitHub Actions logs show detailed output:
1. Go to **Actions** tab
2. Click failed workflow run
3. Expand job steps to see errors

### Common Issues

| Error | Solution |
|-------|----------|
| 429 Rate Limit | Increase `postDelay` in content-scheduler.js |
| Content exhaustion | Add more items to content-bank.json |
| Merge conflicts | Workflows commit history; pull before manual edits |

### Discord Webhook Testing

Test webhook directly with curl:

```bash
curl -H "Content-Type: application/json" \
     -d '{"content": "Test message from Ascendant Continuum! 🚀"}' \
     YOUR_DISCORD_WEBHOOK_URL
```

If this works, your webhook is configured correctly.

---

## 📝 Notes

- **DRY_RUN mode**: Test changes safely without posting
- **Manual posting**: Facebook, X/Twitter, Reddit still manual for now
- **Reddit strategy**: 90/10 value rule (9 educational posts per 1 game mention)
- **Posting frequency**: 1x daily automated, additional manual as needed
- **Content refresh**: Review and add new items to content-bank.json monthly

---

**Ready to go live? Set `DRY_RUN=false` in GitHub secrets and enable workflows!** 🎉
