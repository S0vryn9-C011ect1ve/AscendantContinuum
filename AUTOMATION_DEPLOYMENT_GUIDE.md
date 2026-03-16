# Automation Deployment & Testing Guide

## Current Issues Found

### 1. Blog Not Visible on Website
**Problem**: Blog files exist locally but haven't been deployed to Firebase hosting yet.

**Solution**:
```powershell
cd firebase
firebase deploy --only hosting
```

**Alternative** - Commit and push to trigger GitHub Actions deployment:
```powershell
git add firebase/public/blog/
git commit -m "chore: deploy blog files to hosting"
git push origin main
```
This will trigger the `build-deploy.yml` workflow which includes Firebase hosting deployment.

---

### 2. Social Media Automation Not PostingI
**Problem**: Workflows haven't run yet - they're scheduled for specific times:
- `daily-blog.yml`: 10 AM UTC daily  
- `daily-social.yml`: 2 PM UTC daily
- `dev-update-social.yml`: On push to main (weekdays)
- `weekend-philosophy.yml`: Saturdays 2 PM UTC

**Solutions**:

#### Option A: Manual Trigger (Immediate)
1. Go to GitHub Actions: `https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/actions`
2. Select workflow (`Daily Blog Post` or `Daily Social Media Post`)
3. Click "Run workflow" button
4. Select branch `main`
5. Click green "Run workflow" button

#### Option B: Wait for Scheduled Time
- Blog posts will auto-generate tomorrow at 10 AM UTC (2 AM PST / 5 AM EST)
- Social posts will auto-publish tomorrow at 2 PM UTC (6 AM PST / 9 AM EST)

#### Option C: Test Locally First
```powershell
# Test blog generation (dry-run mode)
cd "d:\1-Ascendant Continuum Game"
$env:DRY_RUN="true"
node scripts/automation/post-blog.js

# Test social media posting (dry-run mode)
$env:DRY_RUN="true"
node scripts/automation/content-scheduler.js

# Test with real posting (CAUTION: will post to platforms)
$env:DRY_RUN="false"
node scripts/automation/content-scheduler.js
```

---

### 3. Environment Variable Mismatch
**Problem**: Workflows use `MASTODON_API_URL` but posting script uses `MASTODON_INSTANCE`.

**Fixed**: Added `MASTODON_API_URL` to `.env` file.

**GitHub Secrets** - Verify these are set at:
`https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/settings/secrets/actions`

Required secrets:
- `BLUESKY_IDENTIFIER` ✅
- `BLUESKY_PASSWORD` ✅
- `MASTODON_ACCESS_TOKEN` ✅
- `MASTODON_API_URL` ⚠️ (verify set)
- `MASTODON_INSTANCE` ⚠️ (verify set)
- `DISCORD_WEBHOOK_URL` ✅
- `FIREBASE_TOKEN` (for hosting deployment)

---

## Quick Fix Steps (In Order)

### Step 1: Deploy Blog to Firebase 🚀
```powershell
cd "d:\1-Ascendant Continuum Game\firebase"
firebase deploy --only hosting
```
**Expected**: Blog will be visible at `https://ascendantcontinuum.web.app/blog/`

---

### Step 2: Test Social Media Automation 🧪
```powershell
cd "d:\1-Ascendant Continuum Game"
$env:DRY_RUN="true"
node scripts/automation/content-scheduler.js
```
**Expected**: Console output showing what *would* be posted (without actually posting)

---

### Step 3: Manually Trigger First Blog Post 📝
Go to GitHub Actions and manually run `Daily Blog Post` workflow:
1. Visit: `https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/actions/workflows/daily-blog.yml`
2. Click "Run workflow" 
3. Wait ~2 minutes for completion
4. Check blog at `https://ascendantcontinuum.web.app/blog/`

---

### Step 4: Manually Trigger First Social Post 📱
Go to GitHub Actions and manually run `Daily Social Media Post` workflow:
1. Visit: `https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/actions/workflows/daily-social.yml`
2. Click "Run workflow"
3. Wait ~2 minutes for completion 
4. Check Bluesky, Mastodon, Discord for new post

---

## Verification Checklist

After running the above steps, verify:

- [ ] Blog visible at `https://ascendantcontinuum.web.app/blog/`
- [ ] Blog index shows at least 1 post
- [ ] RSS feed accessible at `https://ascendantcontinuum.web.app/blog/rss.xml`
- [ ] Social media post appears on Bluesky
- [ ] Social media post appears on Mastodon
- [ ] Social media post appears in Discord
- [ ] GitHub Actions workflows show green checkmarks
- [ ] No errors in workflow logs

---

## Monitoring & Troubleshooting

### Check Workflow Runs
Visit: `https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/actions`

Look for:
- ✅ Green checkmarks = success
- ❌ Red X's = failure (click to see logs)
- 🟡 Yellow circle = running

### Check Local Logs
```powershell
# View posting history
cat "d:\1-Ascendant Continuum Game\public\social\posting-history.json" | ConvertFrom-Json | Format-List

# View blog data  
cat "d:\1-Ascendant Continuum Game\firebase\public\blog\data.json" | ConvertFrom-Json | Format-List

# View content bank status
cd "d:\1-Ascendant Continuum Game"
node scripts/test-automation.js
```

### Common Issues

**Issue**: Blog workflow runs but no blog post appears
- Check workflow logs for errors
- Verify Git commits successful (look for "blog: automated daily post" commits)
- Check if firebase deployment ran after commit

**Issue**: Social media workflow runs but nothing posts
- Check if `DRY_RUN` is set to `false` in workflow
- Verify secrets are set correctly in GitHub
- Check platform API status (Bluesky, Mastodon, Discord)

**Issue**: "No unused items" error in social media workflow
- Content bank has 133 unused items - shouldn't happen
- If it does, run: `node scripts/reset-content-bank.js` (to be created if needed)

---

## Automated Schedule (Once Working)

| Time (UTC) | Workflow | What It Does |
|-----------|----------|--------------|
| 10:00 AM | `daily-blog.yml` | Generate & publish blog post, auto-share to social media |
| 2:00 PM | `daily-social.yml` | Post from content bank to Bluesky, Mastodon, Discord |
| 2:00 PM Sat | `weekend-philosophy.yml` | Post design philosophy content |
| On commit (weekdays) | `dev-update-social.yml` | Share development updates |

All times are UTC. Convert to your timezone:
- **PST**: Subtract 8 hours (Blog: 2 AM, Social: 6 AM)
- **EST**: Subtract 5 hours (Blog: 5 AM, Social: 9 AM)

---

## Next Steps

1. **Deploy blog** using Step 1 above ⬆️
2. **Manually trigger** both workflows using Steps 3-4 ⬆️
3. **Verify** all systems working using checklist ⬆️
4. **Monitor** first auto-run tomorrow morning
5. **Iterate** based on any errors in logs

Need help? Check workflow logs at: `https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/actions`
