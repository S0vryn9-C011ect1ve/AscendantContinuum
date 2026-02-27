# HEARTBEAT — Ecosystem Operational Checks

## Vision

Instead of running separate cron jobs for calendar monitoring, Todoist checks, brand deal tracking, and fish tank monitoring, run all checks on a **single heartbeat poll** that batches related service calls.

---

## Architecture

### Heartbeat Cycle

```
Every 30 minutes:
  1. Poll calendar API → extract today's events, deadlines, urgent items
  2. Poll Todoist API → extract overdue tasks, flagged items
  3. Check brand deal tracker → new deal opportunities, expiring partnerships
  4. Check fish tank sensors → temperature, pH, feeding status
  5. Aggregate into a single status report
  6. Log to memory file (daily digest)
  7. Alert if any threshold hit (temp out of range, overdue critical task, etc.)
```

### Services Monitored

| Service | Check | Frequency | Threshold |
|---------|-------|-----------|-----------|
| **Calendar** | Today's events, upcoming deadlines | Real-time | Flagged events priority |
| **Todoist** | Overdue, high-priority, flagged | Real-time | Any overdue in critical projects |
| **Brand Deals** | New opportunities, expiration dates | Every 6 hours | Closing in <7 days alert |
| **Fish Tank** | Temp, pH, feeding log | Every 2 hours | Temp >78°F or <74°F alert |

---

## Implementation

### `heartbeat.js` API

```javascript
startHeartbeat({
  interval: 30 * 60 * 1000, // 30 minutes
  services: {
    calendar: { enabled: true, apiKey: process.env.GOOGLE_CALENDAR_API },
    todoist: { enabled: true, apiKey: process.env.TODOIST_API_KEY },
    brandDeals: { enabled: true, source: 'sheets' }, // Google Sheets or Airtable
    fishTank: { enabled: true, apiKey: process.env.IOT_SENSOR_KEY }
  },
  onHeartbeat: (status) => {
    // Called after every poll with aggregated status
  },
  onAlert: (alert) => {
    // Called if any threshold hit
  }
});
```

### Memory Output

Every heartbeat writes to `memory/YYYY-MM-DD.md`:

```markdown
## [2026-02-27] Heartbeat Status

### Calendar
- Focus block 2-4pm: blocked ✓
- 3mpwrApp board meeting: 5pm → Agenda ready

### Todoist
- Overdue: 0
- Due today: 3 (all scheduled)
- High flag: 2

### Brand Deals
- New: DocuSign partnership interest (follow up)
- Expiring: Accessibility audit sponsorship (4 days)

### Fish Tank
- Temp: 76°F ✓
- pH: 7.2 ✓
- Fed: 8am ✓ (next: 4pm)

### Alerts
- None
```

---

## Adding a New Heartbeat Check

1. **Define the service** in `heartbeat.js`:
   ```javascript
   services.myService = {
     enabled: true,
     apiKey: process.env.MY_SERVICE_KEY,
     threshold: { /* alert conditions */ }
   }
   ```

2. **Write the fetch function**:
   ```javascript
   async fetchMyService() {
     // return { status, data, alerts }
   }
   ```

3. **Add to aggregation loop** — heartbeat automatically calls it and includes result.

4. **Test with a dry-run**: `node heartbeat.js --dry-run`

---

## Active Integrations

- **Google Calendar**: OAuth2, requires user consent once
- **Todoist**: API key in `.secrets`
- **Brand Deals Tracker**: Airtable base or Google Sheets (read-only link)
- **Fish Tank Sensors**: IoT device API or MQTT broker

---

## Failure Handling

- If a service fails: log it, skip that check, continue others.
- If heartbeat itself crashes: orchestrator restarts it (pm2/systemd).
- Missing `.secrets` file → heartbeat warns and disables that service.

---

## Monitoring the Heartbeat

```bash
# View live heartbeat logs
tail -f logs/heartbeat.log

# Check last N heartbeats
cat logs/heartbeat-history.json | tail -20 | jq .

# Manual trigger
node heartbeat.js --now
```

---

## Next Steps

- [ ] Wire Google Calendar OAuth
- [ ] Set up Todoist API key in `.secrets`
- [ ] Create Airtable base for brand deals
- [ ] Test fish tank sensor connectivity
