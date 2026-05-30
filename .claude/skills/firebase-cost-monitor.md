# Firebase Cost Monitoring Skill

**Type:** Tool-Powered  
**Purpose:** Prevent unexpected Firebase bills through proactive monitoring

---

## Cost Targets

| Service | Daily Limit | Monthly Limit | Cost if Exceeded |
|---------|-------------|---------------|------------------|
| Firestore Reads | 50K | 1.5M | $0.06 per 100K |
| Firestore Writes | 20K | 600K | $0.18 per 100K |
| Storage Downloads | 1GB | 30GB | $0.12 per GB |
| Storage Uploads | 100MB | 3GB | $0.026 per GB |
| Hosting Bandwidth | 300MB | 10GB | $0.15 per GB |

**Target monthly cost:** < $5 with rate limits  
**Alert threshold:** $2 (early warning)

---

## Rate Limits Implemented

### Firestore (firestore.rules)

```javascript
function rateLimit(seconds) {
  return resource == null || request.time > resource.data.lastWrite + duration.value(seconds, 's');
}

// User updates: Max 6 per minute
allow write: if isOwner(userId) && rateLimit(10);

// Challenge submissions: Max 2 per minute
allow create: if isOwner(request.resource.data.userId) && rateLimit(30);

// Time capsules: Max 1 per minute
allow create: if isOwner(request.resource.data.creatorId) && rateLimit(60);
```

### Storage (storage.rules)

```javascript
// Replay videos: 10MB max
allow write: if request.resource.size < 10 * 1024 * 1024;

// Sigil images: 2MB max
allow write: if request.resource.size < 2 * 1024 * 1024;
```

---

## Monitoring Commands

### Check Current Usage

```bash
# Via Firebase CLI
firebase projects:list
firebase firestore:usage --project ascendant-continuum

# Via Console
https://console.firebase.google.com/project/ascendant-continuum/usage
```

### Estimate Monthly Cost

```javascript
// Based on current rate limits
const estimatedMonthlyCost = {
  firestore: {
    reads: (1.5e6 / 100000) * 0.06,   // $0.90
    writes: (600000 / 100000) * 0.18,  // $1.08
    storage: 1 * 0.026                 // $0.03
  },
  storage: {
    downloads: (30 / 10) * 0.12,       // $0.36
    uploads: (3 / 10) * 0.026          // $0.01
  },
  hosting: {
    bandwidth: (10 / 10) * 0.15        // $0.15
  }
};

// Total: ~$2.53/month (under $5 target)
```

---

## Cost Spike Scenarios

### Scenario 1: Bot Attack (Spam Writes)
**Without limits:** 10K writes/min × 60 min = 600K writes/hour = $10.80/hour  
**With limits:** 2 writes/min × 60 min = 120 writes/hour = $0.02/hour  
**Savings:** 99.8% cost reduction

### Scenario 2: Viral Growth (Legitimate Traffic)
**Without limits:** 1000 users × 100 reads/session = 100K reads = $0.06  
**With limits:** Throttled at user level, spreads over time  
**Result:** Graceful degradation vs. surprise bill

### Scenario 3: Large File Uploads
**Without limits:** User uploads 100MB replay = $0.26 × 1000 users = $260  
**With limits:** 10MB max = $0.026 × 1000 users = $26  
**Savings:** 90% cost reduction

---

## Automated Monitoring

Add to `Maintenance.ps1`:

```powershell
function Check-FirebaseCosts {
    Write-Host "Checking Firebase usage..." -ForegroundColor Cyan
    
    # Fetch usage from Firebase REST API
    $projectId = "ascendant-continuum"
    $apiKey = $env:FIREBASE_API_KEY
    
    # Check if approaching limits
    # Alert if > 80% of monthly quota used
    
    # Log to monitoring file
    $date = Get-Date -Format "yyyy-MM-dd"
    Add-Content -Path "firebase-usage-$date.log" -Value $usageData
}
```

---

## Alert Triggers

Set up alerts in Firebase Console:

1. **Budget Alert:** Email when cost > $2/month
2. **Quota Alert:** Email when reads > 1M/month
3. **Anomaly Alert:** Email if traffic spikes 10×

Configure at: `https://console.firebase.google.com/project/ascendant-continuum/settings/budget`

---

## Optimization Strategies

### Reduce Reads
- Cache frequently accessed data in client memory
- Use local storage for static content (constellations, achievements)
- Batch reads instead of individual queries
- Implement offline-first with sync

### Reduce Writes
- Debounce progress updates (save every 60 sec, not every action)
- Batch writes (accumulate, then write once)
- Use increment operations instead of read-modify-write

### Reduce Storage
- Compress replay videos before upload (target: < 2MB)
- Convert sigil images to WebP format
- Delete old replays after 30 days

---

## Emergency Cost Reduction

If costs exceed $5/month:

1. **Immediate:** Disable user-generated content uploads
2. **Week 1:** Implement aggressive caching
3. **Week 2:** Move static assets to CDN (cheaper than Firebase Storage)
4. **Week 3:** Consider PostgreSQL for high-write data

---

## Weekly Checklist

- [ ] Check Firebase Console usage dashboard
- [ ] Verify costs < $2 for the week
- [ ] Review Firestore query counts (any anomalies?)
- [ ] Check Storage usage (growing unexpectedly?)
- [ ] Audit new code for inefficient queries
- [ ] Update rate limits if needed

---

## When to Upgrade Firebase Plan

**Stay on Spark (Free) until:**
- Costs consistently hit $5/month
- 1000+ DAU (daily active users)
- Need phone auth or multi-region

**Then upgrade to Blaze (Pay-as-you-go):**
- Better performance
- No hard limits
- Auto-scaling
- But REQUIRES strict cost monitoring
