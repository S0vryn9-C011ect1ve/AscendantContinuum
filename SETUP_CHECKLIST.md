# Setup Checklist - Social Media Automation

## ✅ Completed Steps

- [x] All code files created (23 files)
- [x] npm dependencies installed
- [x] Security vulnerabilities reduced (12 → 2, 83% improvement)
- [x] GitHub Actions secrets added by you
- [x] .env file created with template

---

## 🔄 Next Steps (DO THESE IN ORDER)

### Step 1: Fill in .env Credentials ⚠️ REQUIRED

Open the `.env` file in the root directory (created for you) and replace the placeholder values:

#### 1a. Bluesky Credentials

```bash
BLUESKY_IDENTIFIER=your-handle.bsky.social
BLUESKY_PASSWORD=your-app-password-here
```

**How to get:**
1. Go to https://bsky.app and log in
2. Go to Settings → App Passwords
3. Click "Add App Password"
4. Name it "Ascendant Continuum Automation"
5. Copy the generated password (you can't see it again!)
6. Your identifier is your handle (e.g., `ascendant.bsky.social`)

#### 1b. Mastodon Credentials

```bash
MASTODON_INSTANCE=https://your-instance.social
MASTODON_ACCESS_TOKEN=your-access-token-here
```

**How to get:**
1. Go to your Mastodon instance (e.g., mastodon.social, mastodon.gamedev.place)
2. Log in and go to Settings → Development
3. Click "New Application"
4. Name: "Ascendant Continuum Automation"
5. Scopes: Check `write:statuses` (at minimum)
6. Submit
7. Click on your new application
8. Copy the "Your access token" value
9. Your instance URL is the base URL (e.g., `https://mastodon.gamedev.place`)

**Recommended instance for gamedevs:** https://mastodon.gamedev.place

#### 1c. Discord Webhook

```bash
DISCORD_WEBHOOK_URL=https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN
```

**How to get:**
1. Open your Discord server
2. Go to the channel you want posts in (e.g., #announcements)
3. Click channel settings (gear icon)
4. Go to Integrations → Webhooks
5. Click "New Webhook"
6. Name it "Ascendant Continuum Bot"
7. Copy the webhook URL
8. Click "Save Changes"

#### 1d. Dry Run Mode (Keep TRUE for now)

```bash
DRY_RUN=true
```

**Don't change this yet!** We'll test with dry run mode first.

---

### Step 2: Test Individual Platform Modules (Safe Testing)

Run these commands **one at a time** to test each platform in dry-run mode (won't actually post):

```bash
# Test Bluesky module
npm run social:test-bluesky

# Test Mastodon module
npm run social:test-mastodon

# Test Discord module
npm run social:test-discord
```

**Expected output for each:**
```
[DRY RUN] Would post to [platform]:
──────────────────────────────────────────────────
[Post preview here]
──────────────────────────────────────────────────
```

**If you see errors:**
- Check your .env file for typos
- Verify credentials are correct
- Check that instance URLs include `https://`
- Bluesky identifier should be your handle, not email

---

### Step 3: Test Content Generators (No Posting)

These generate content but don't post it:

```bash
# Test dev update generator (reads git commits)
npm run social:dev-update

# Test philosophy content
npm run content:philosophy

# Test behind-the-scenes content
npm run content:behind-scenes

# Test lore snippets
npm run content:lore

# Test education content
npm run content:education
```

**Expected output:**
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🎮 DEV UPDATE - [Date]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Generated content preview]
```

---

### Step 4: Test Full Scheduler (Dry Run)

This tests the complete posting workflow without actually posting:

```bash
npm run social:test-all
```

**Expected output:**
```
🎮 Starting Ascendant Continuum Social Media Poster...
[DRY RUN MODE: No actual posts will be made]

📋 Content Bank Status:
  Total items: 100
  Available items: 100

🎯 Selected content:
  Type: dev-update
  Hook: [viral hook]
  Body: [content preview]

[DRY RUN] Would post to bluesky:
[...]
[DRY RUN] Would post to mastodon:
[...]
[DRY RUN] Would post to discord:
[...]

✅ All platforms completed (DRY RUN)
```

---

### Step 5: Make Your First REAL Post 🚀

**Only do this after Steps 1-4 work!**

#### 5a. Change DRY_RUN to false

Edit `.env`:
```bash
DRY_RUN=false  # <-- Change this
```

#### 5b. Post manually

```bash
npm run social:post
```

**This will:**
1. Select content from the content bank
2. Post to Bluesky, Mastodon, and Discord
3. Log results to `public/social/posting-history.json`
4. Mark content as "used" in `public/social/content-bank.json`

#### 5c. Verify posts

Check each platform:
- Bluesky: https://bsky.app/profile/[your-handle]
- Mastodon: [your-instance]/web/@[your-username]
- Discord: Check your announcements channel

---

### Step 6: Test GitHub Actions Workflow

**Prerequisites:** Steps 1-5 completed successfully

#### 6a. Verify GitHub Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions

**Required secrets** (you said you added these):
- `BLUESKY_IDENTIFIER`
- `BLUESKY_PASSWORD`
- `MASTODON_INSTANCE`
- `MASTODON_ACCESS_TOKEN`
- `DISCORD_WEBHOOK_URL`

#### 6b. Manual workflow test

1. Go to Actions tab on GitHub
2. Select "Daily Social Media Post" workflow
3. Click "Run workflow" dropdown
4. Select branch: `main`
5. Click "Run workflow" button
6. Wait ~1 minute
7. Click on the workflow run to see logs

**Expected result:**
- ✅ Green checkmark
- Logs show successful posts
- New commit: "Update posting history [timestamp]"
- Posts visible on all platforms

**If it fails:**
- Check workflow logs for error messages
- Verify secrets are named exactly as shown above (case-sensitive!)
- Check that secrets don't have extra spaces or quotes

---

### Step 7: Enable Automated Scheduling ⏰

**Once manual workflow test works:**

The workflows are already configured to run automatically:
- **Daily Social Media Post**: Every day at 2 PM UTC (9 AM EST)
- **Dev Update Social**: Weekdays at 8 PM UTC (3 PM EST) after code commits
- **Weekend Philosophy**: Saturdays & Sundays at 4 PM UTC (11 AM EST)

**No code changes needed** - they're already active!

**To pause automation:**
1. Go to .github/workflows/daily-social.yml
2. Comment out the `cron:` line by adding `#` at the start
3. Commit the change

**To resume:**
- Remove the `#` from the cron line

---

## 📊 Monitoring & Maintenance

### Check Posting History

```bash
cat public/social/posting-history.json | tail -20
```

Shows last 20 posts made.

### Check Content Bank Status

```bash
npm run content:build-bank
```

Rebuilds content bank and shows distribution.

### Manually Add Content

Edit `public/social/content-bank.json` and add items following this schema:

```json
{
  "id": 101,
  "type": "dev-update",
  "hook": "Your attention-grabbing hook",
  "body": "Main content here.\n\nCan have multiple paragraphs.",
  "platforms": ["bluesky", "mastodon", "discord"],
  "priority": 1,
  "used": false,
  "lastUsed": null
}
```

### View Scheduled Workflows

```bash
cat .github/workflows/daily-social.yml | grep 'cron:'
cat .github/workflows/dev-update-social.yml | grep 'cron:'
cat .github/workflows/weekend-philosophy.yml | grep 'cron:'
```

---

## ❌ Troubleshooting

### Error: "Missing credentials"

- Check .env file exists in project root
- Verify all 5 variables are set (not placeholders)
- Check for typos in variable names

### Error: "Authentication failed" (Bluesky)

- Use your handle (e.g., `yourname.bsky.social`), not email
- Use app password, not account password
- App password is 19 characters with dashes (e.g., `xxxx-xxxx-xxxx-xxxx`)
- Create new app password if unsure

### Error: "401 Unauthorized" (Mastodon)

- Check access token is correct (copy-paste from Mastodon)
- Verify token has `write:statuses` scope
- Check instance URL has `https://` prefix
- Try creating new application in Mastodon

### Error: "404 Not Found" (Discord)

- Check webhook URL is complete (should be ~120 characters)
- Verify webhook wasn't deleted in Discord
- Check channel still exists

### Workflow fails on GitHub

- Check secrets are added (Settings → Secrets)
- Verify secret names match exactly (case-sensitive)
- Check repository has Actions enabled
- View workflow logs for specific error

### Posts not showing on platform

- Check DRY_RUN=false in .env (and in GitHub secrets)
- Verify account isn't rate-limited
- Check platform status pages for outages
- Try posting manually first

---

## 🎯 Current Status Summary

| Item | Status | Action Needed |
|------|--------|---------------|
| Code files | ✅ Complete | None |
| npm packages | ✅ Installed | None |
| Security | ✅ 83% improved | Optional: Accept remaining 2 vulnerabilities (see [SECURITY.md](SECURITY.md)) |
| .env file | ⚠️ Template only | **Fill in your credentials** |
| GitHub secrets | ✅ Added by you | Verify names match exactly |
| Local testing | ⏳ Not started | **Run Steps 2-5 above** |
| GitHub Actions | ⏳ Not tested | **Run Step 6 above** |
| Automation active | ✅ Ready | Will run on schedule after Step 6 |

---

## 📚 Additional Resources

- [Testing Guide](docs/social/TESTING_GUIDE.md) - Detailed testing procedures
- [Security Note](docs/social/SECURITY.md) - Understanding npm vulnerabilities
- [Implementation Complete](docs/social/IMPLEMENTATION_COMPLETE.md) - Full feature list
- [README](docs/social/README.md) - Architecture overview

---

## ✅ Quick Start (TL;DR)

If you just want to get started ASAP:

```bash
# 1. Edit .env with your real credentials
code .env

# 2. Test without posting
npm run social:test-all

# 3. Make first real post
# (change DRY_RUN=false in .env first)
npm run social:post

# 4. Test GitHub Actions
# Go to Actions tab → Run "Daily Social Media Post" workflow

# Done! Automation is live.
```

---

**Need help?** Check [docs/social/TESTING_GUIDE.md](docs/social/TESTING_GUIDE.md) for detailed troubleshooting or refer to the error messages above.
