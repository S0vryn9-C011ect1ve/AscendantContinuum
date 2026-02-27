# LEARNINGS — Mistake → Fix → Rule → Remember

## Philosophy

Every mistake is a data point. Capture it. Fix it. Learn from it. Teach the system to avoid it.

---

## Process

### When you make a mistake:

1. **Fix it immediately** — don't wait, don't report, just fix.
2. **Write the lesson** — what went wrong? Why?
3. **Write the rule** — what prevents this next time?
4. **Add to this file** — under the appropriate category.
5. **Review at session start** — read LEARNINGS.md before ANY task.

### Format

```markdown
## [Category] Mistake Name

**Date**: YYYY-MM-DD  
**Severity**: Critical / High / Medium / Low

**What Happened**:
Clear description of the mistake. What did I do? What was the impact?

**Root Cause**:
Why did this happen? What assumption was wrong?

**Fix**:
What did I do to fix it? How long did it take?

**Rule to Prevent It**:
Specific, actionable rule. "Always X before Y." Not vague.

**Added to Checklist**: Yes / No
```

---

## Categories

### Architecture & Design

### Security & Secrets

### Testing & Quality

### Deployment & Operations

### Communication & Coordination

---

## Session Start Checklist

Before starting work:

- [ ] Read LEARNINGS.md (scan titles, read recent entries)
- [ ] Check if any rule applies to today's task
- [ ] Ask: "Have I made this mistake before?"

---

## Example Entries (Template)

```markdown
## [Security] Committed API key to repo

**Date**: 2026-02-20  
**Severity**: Critical

**What Happened**:
Accidentally committed `.secrets` file with GitHub token to main branch. Token exposed in public repo history.

**Root Cause**:
Assumed `.gitignore` was working. Didn't verify before first commit. Trusted default config.

**Fix**:
1. Rotated token immediately (5 min).
2. Forced history rewrite to remove key (10 min).
3. Educated self on git-secrets tool and pre-commit hooks (15 min).

**Rule to Prevent It**:
Before any commit to any repo:
1. Run `git diff --cached` and scan for `ghp_`, `sk_`, `private_key`.
2. If found, `git reset` immediately.
3. Use `pre-commit` hook to auto-block secrets (install once per machine).

**Added to Checklist**: Yes — now in "Pre-Commit Verification"

---

## [Testing] Forgot to test edge case

**Date**: 2026-02-21  
**Severity**: Medium

**What Happened**:
Shipped agent code that crashed when API returned empty array. Didn't consider "no results" scenario.

**Root Cause**:
Tested happy path only. Assumed API always returns data. No defensive coding.

**Fix**:
1. Added null/empty checks to function (2 min).
2. Wrote test for empty result (5 min).
3. Added to test template so future tests include edge cases (10 min).

**Rule to Prevent It**:
For any function that processes external data:
1. Write test for: null, empty array, malformed data, timeout.
2. Add comments: "// Handles: X, Y, Z"
3. Code review asks: "What if this data isn't there?"

**Added to Checklist**: Yes — now in "Testing Requirements"
```

---

## Active Rules (Master List)

### Pre-Commit Verification
- [ ] Scan `git diff` for secrets before commit.
- [ ] Run `npm test` locally.
- [ ] Check for console.log or debug code.

### Testing Requirements
- [ ] Happy path + edge cases (null, empty, error).
- [ ] Any external API call needs error handler.
- [ ] New functions → new tests.

### Security Rules
- [ ] Never log sensitive data (tokens, emails, IDs).
- [ ] `.secrets` always in `.gitignore`.
- [ ] Rotate tokens quarterly.

### Documentation Requirements
- [ ] If code needs explanation → add comment or break into smaller function.
- [ ] Any new API endpoint → document with curl example.

### Deployment Rules
- [ ] Test in staging before production.
- [ ] Have rollback plan before deploying breaking changes.
- [ ] Notify users of downtime >5 minutes.

---

## Session Log Template

At end of each session, append to `memory/YYYY-MM-DD.md`:

```markdown
### Learnings Today

**Mistakes**:
- None / [List with links to LEARNINGS.md]

**New Rules Added**:
- [Rule name]

**Rules That Saved Us**:
- [Rule name] — prevented [issue]
```

---

**Last Review**: 2026-02-27  
**Total Lessons**: 0  
**Rules in Active Use**: 7
