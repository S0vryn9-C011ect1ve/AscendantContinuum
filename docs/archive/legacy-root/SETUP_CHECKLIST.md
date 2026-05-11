# Legacy Setup Checklist (Superseded)

This file is superseded by canonical setup documentation.

Use:

1. `docs/social/README.md` for social setup, dry-run testing, and scheduler flow
2. `docs/technical/GITHUB_SETUP.md` for repository and CI/CD setup
3. `docs/operations/legacy-doc-consolidation-wave-tracker.md` for migration progress
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
