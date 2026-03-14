# GitHub Actions Secrets Setup

## Add These Secrets to GitHub

Go to your repository on GitHub → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

Add each of these **exactly as shown** (case-sensitive):

### 1. BLUESKY_IDENTIFIER
```
ascendantcontinuum.bsky.social
```

### 2. BLUESKY_PASSWORD
```
7x3d-aigi-f4vm-gfio
```

### 3. MASTODON_INSTANCE
```
https://mastodon.social
```

### 4. MASTODON_ACCESS_TOKEN
```
oWFJ_soijkS6LH5Llj0AwKgkoGV5g2eUWbHvabYU4sc
```

### 5. DISCORD_WEBHOOK_URL
```
https://discord.com/api/webhooks/1482235598004813945/eYG7OUGifHzpgWgr1YJ6aDnom9oqqCER4PEDAjiM0n293_6eiHlfsJktF1MYDHQt8vVM
```

---

## Testing the Workflow

After adding all 5 secrets:

1. Go to **Actions** tab in your GitHub repository
2. Click on **"Daily Social Media Post"** workflow
3. Click **"Run workflow"** dropdown button
4. Select branch: **main**
5. Click **"Run workflow"** button (green)
6. Wait ~1-2 minutes
7. Check the workflow run for green checkmark ✅
8. Check your Bluesky and Mastodon accounts for the new post!

---

## Automated Schedule (Already Configured)

Once the manual test works, these will run automatically:

| Workflow | Schedule | Purpose |
|----------|----------|---------|
| Daily Social Media Post | 2 PM UTC (9 AM EST) | Daily posts from content bank |
| Dev Update Social | 8 PM UTC (3 PM EST) Mon-Fri | Posts after code commits |
| Weekend Philosophy | 4 PM UTC (11 AM EST) Sat-Sun | Weekend thought leadership |

---

## Troubleshooting

**If workflow fails:**
- Check secret names are EXACTLY as shown (case-sensitive)
- Verify no extra spaces in secret values
- Check workflow logs for specific error message

**If posts don't appear:**
- Verify secrets were added correctly
- Check that repository has Actions enabled
- Look at workflow run logs for errors
