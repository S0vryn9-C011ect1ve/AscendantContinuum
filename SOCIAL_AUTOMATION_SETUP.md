# Social Media Automation Setup Guide

## Current Status: DRY RUN MODE ✅

All workflows are currently running in **DRY RUN** mode, which means:
- ✅ Workflows run successfully without failing
- ✅ Content is generated and logged
- ✅ No actual posts are made to social media
- ✅ You can see what WOULD be posted in GitHub Actions logs

This lets you test the automation without needing credentials configured yet.

---

## When You're Ready to Post for Real

### Step 1: Get Account Credentials

**Bluesky:**
1. Go to https://bsky.app/settings/app-passwords
2. Create a new app password named "Ascendant Continuum Automation"
3. Save the generated password (you'll need it in Step 2)

**Mastodon:**
1. Go to your Mastodon instance → Settings → Development → New Application
2. Name: "Ascendant Continuum Automation"
3. Scopes: `read`, `write`
4. Save and copy the Access Token

**Discord (optional):**
1. Go to your Discord server → Server Settings → Integrations → Webhooks
2. Create webhook for announcements channel
3. Copy the webhook URL

### Step 2: Configure GitHub Secrets

Go to: https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/settings/secrets/actions

Click **"New repository secret"** and add these:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `BLUESKY_IDENTIFIER` | Your Bluesky handle | `yourhandle.bsky.social` |
| `BLUESKY_PASSWORD` | App password from Step 1 | `xxxx-xxxx-xxxx-xxxx` |
| `MASTODON_INSTANCE` | Your Mastodon server URL | `https://mastodon.social` |
| `MASTODON_ACCESS_TOKEN` | Access token from Step 1 | `xxxxxxxxxxxxx` |
| `DISCORD_WEBHOOK_URL` | Webhook URL (optional) | `https://discord.com/api/webhooks/...` |

### Step 3: Enable Live Posting

Once secrets are configured, edit these workflow files and change `DRY_RUN` back to `false`:

1. `.github/workflows/daily-blog.yml` - Line 39: `DRY_RUN: false`
2. `.github/workflows/daily-social.yml` - Line 42: `DRY_RUN: 'false'`
3. `.github/workflows/dev-update-social.yml` - Line 42: `DRY_RUN: 'false'`
4. `.github/workflows/weekend-philosophy.yml` - Line 42: `DRY_RUN: 'false'`

Commit and push these changes.

### Step 4: Test Manually

Trigger a test run manually:
1. Go to https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/actions
2. Click any workflow (e.g., "Daily Social Media Post")
3. Click "Run workflow" → "Run workflow"
4. Check logs for success
5. Verify post appeared on your social accounts

---

## Scheduled Posting Times (After Enabling)

| Workflow | Schedule | What It Posts |
|----------|----------|---------------|
| **Daily Blog** | 10 AM UTC (5 AM EST) | Blog post + social announcement |
| **Daily Social** | 2 PM UTC (9 AM EST) | Content from 136-post bank |
| **Weekend Philosophy** | Sat/Sun 4 PM UTC | Nature/accessibility themes |
| **Dev Update** | On commits to main | Recent development changes |

---

## Troubleshooting

**Workflows still failing after adding secrets?**
- Check secret names match exactly (case-sensitive)
- Verify Bluesky app password has no spaces
- Test credentials locally: `npm run social:test-bluesky`

**Want to test before going live?**
- Keep DRY_RUN enabled
- Check workflow logs to see generated content
- No posts will be made until you change to `false`

**Need help?**
- Check workflow logs: https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/actions
- Test scripts locally: `DRY_RUN=true npm run social:post`
- All scripts handle missing credentials gracefully in DRY_RUN mode
