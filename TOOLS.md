# TOOLS — Local Setup & API Cheat Sheet

## Machine Setup

- **OS**: Windows 11 (dev machine)
- **Node.js**: v18.17.0 LTS
- **npm**: 9.6.7
- **Runtime engines**: Kimi K, DeepSeek v3.2, Qwen3.5-397B (optional for specialized tasks)

---

## SSH & Deployment

### Production Servers

| Service | Host | User | Port | Key |
|---------|------|------|------|-----|
| 3mpwrApp API | `api.3mpwr.app` | deploy | 22 | `.secrets/deploy_key` |
| Content Sync | `sync.3mpwr.app` | deploy | 22 | `.secrets/deploy_key` |
| Analytics | `analytics.3mpwr.app` | root | 22 | `.secrets/analytics_key` |

### Usage

```bash
# Deploy to production
ssh -i .secrets/deploy_key deploy@api.3mpwr.app "cd /app && git pull && npm run build"

# Check service status
ssh -i .secrets/deploy_key deploy@api.3mpwr.app "systemctl status 3mpwrapp"
```

---

## API Endpoints & Keys

All keys stored in `.secrets` (git-ignored). Update this list when stack changes.

### External APIs

| Service | Endpoint | Auth | Purpose | Key Location |
|---------|----------|------|---------|--------------|
| **GitHub Models** | `models.inference.ai.azure.com` | GITHUB_TOKEN | Content generation, curation | `.secrets/GITHUB_TOKEN` |
| **Todoist** | `api.todoist.com/rest/v2` | Personal Token | Task management heartbeat | `.secrets/TODOIST_API_KEY` |
| **Google Calendar** | `calendar.googleapis.com` | OAuth2 | Calendar integration | `.secrets/google_oauth.json` |
| **Airtable** | `api.airtable.com/v0` | Personal Token | Brand deals tracker | `.secrets/AIRTABLE_KEY` |
| **Firebase** | Firestore | SDK + service account | Community data, presence | `firebase/` folder |
| **Sentry** | `sentry.io` | DSN | Error monitoring | `.secrets/SENTRY_DSN` |

### Local APIs

| Service | Port | Purpose |
|---------|------|---------|
| Dev server | 3000 | 3mpwrApp local development |
| Storybook | 6006 | Component library |

---

## Environment Variables

### Required for agents to start

```bash
# .secrets (or .env.local)
GITHUB_TOKEN=ghp_...
TODOIST_API_KEY=...
GOOGLE_CALENDAR_API=...
SENTRY_DSN=https://...
FIREBASE_PROJECT_ID=...
```

### Optional (disable if not available)

```bash
AIRTABLE_KEY=... # Brand deals tracking
IOT_SENSOR_KEY=... # Fish tank sensors
```

---

## Installed CLI Tools

| Tool | Version | Purpose |
|------|---------|---------|
| `npm` | 9.6.7 | Package manager |
| `node` | 18.17.0 | Runtime |
| `git` | 2.40+ | Version control |
| `pm2` | 5.3+ | Process manager (agents) |
| `kubectl` | 1.27+ | K8s if running Docker (optional) |
| `ffmpeg` | 5.1+ | Video processing (optional) |

---

## Frequently Used Commands

```bash
# Run agents
npm run deploy:agents          # Start all agents
npm run heartbeat:start        # Heartbeat only
npm run heartbeat:logs         # View heartbeat activity

# Build & test
npm test                       # Run full test suite
npm run lint                   # ESLint + Prettier
npm run build                  # Production build

# Deployment
npm run deploy:prod            # Deploy to production
npm run sync:firebase          # Sync Firestore rules

# Debugging
npm run debug:agents           # Run with verbose logging
tail -f logs/agents/*.log      # Watch agent logs in real-time
```

---

## Secrets Rotation

When rotating keys:

1. Generate new key in service dashboard.
2. Update `.secrets` file locally.
3. Test locally with `npm run test:integration`.
4. Stage and commit environment secrets in CI/CD (GitHub Actions, etc.).
5. Redeploy production.
6. Delete old key in service.

Log rotation events to `memory/YYYY-MM-DD.md`.

---

## Common Issues

### "`GITHUB_TOKEN not found`"
- Ensure `.secrets` exists and is in `.gitignore`.
- Run `cp .secrets.example .secrets` and fill in values.

### "SSH connection refused"
- Check IP whitelist in firewall.
- Verify SSH key has correct permissions: `chmod 600 .secrets/deploy_key`.

### "Heartbeat not running"
- Check `pm2 list` for agent process status.
- Review logs: `tail -f logs/heartbeat.log`.
- Force restart: `pm2 restart all`.

---

## Stack Changes & Updates

When adding a new tool, service, or API:

1. Add entry to this file **immediately**.
2. Add to `.secrets.example` (without values).
3. Update CI/CD if needed.
4. Note the date: `[YYYY-MM-DD] Added X service`.

**Last Updated**: 2026-02-27
