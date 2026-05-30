# /analyze-players Command

Analyze player feedback and metrics to prioritize features.

## Usage

```
/analyze-players [source]
```

## Parameters

- `source` (optional): `firebase` | `survey` | `reddit` | `all` (default: `all`)

## What It Does

1. **Fetch Data:** Pull player metrics from Firebase Analytics (if implemented)
2. **Parse Feedback:** Extract themes from survey responses, Reddit comments
3. **Prioritize:** Rank issues by frequency × severity
4. **Recommend:** Generate action plan based on data

## Output Format

```markdown
## Player Feedback Analysis

### Top Issues (by frequency)
1. **Onboarding Confusion** (18/25 players) - CRITICAL
2. **First Realm Too Short** (12/25 players) - HIGH
3. **IAP Not Working** (8/25 players) - HIGH

### Feature Requests (by demand)
1. **Dark Mode** (14/25 players)
2. **Gamepad Support** (9/25 players)

### Recommended Actions
1. Rewrite onboarding tutorial (Week 1)
2. Add 2 puzzles to Realm 1 (Week 2)
3. Implement Unity IAP (Week 3)
```

## Data Sources

- Firebase Firestore: `/users`, `/challengeSubmissions`
- Waitlist: `/waitlist` with feedback field
- Reddit: Comments on launch posts
- Email: Survey responses

## Metrics to Track

- Day 1/3/7 retention rates
- Session length distribution
- Realm completion rates
- Drop-off points (which scene?)
- IAP conversion rate
