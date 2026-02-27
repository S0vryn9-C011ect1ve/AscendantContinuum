# Ecosystem-Wide Dead Man's Switch Protocol

**Version:** 1.0  
**Created:** 2026-02-26  
**Owner:** Founder / Cross-Platform Governance Board  
**Applies to:** S0vryn9 C011ect1ve (parent), 3mpwrApp, Ascendant Continuum, Mutual Aid Foundation  
**Status:** ACTIVE (Phase 1-2); Full governance board activation occurs at Phase 3

---

## 🎯 Purpose

This document defines the unified process for ensuring S0vryn9 C011ect1ve ecosystem continuity in the event of founder incapacity, death, or voluntary transition. It is the single source of truth for all three platforms and the foundation.

**Core Principle:** The ecosystem survives the founder. Community governance, user councils, and transparent decision-making replace sole-founder dependency.

---

## 📋 Governance Structure (Current & Future)

### Phase 1-2 (Current: Bootstrap — No Elections)

| Role | Responsibility | Now |
|------|-----------------|-----|
| **Founder** | S0vryn9 finances, backend infrastructure, strategy | ✅ Active (you) |
| **Backend Trustee (Primary)** | Named by founder; infrastructure safeguard | ⏳ TBD (founder-appointed) |
| **User Councils** | Elected general admins per platform | ⏳ Planned at launch |
| **Cross-Platform Governance Board** | TBD; forms at Phase 3 when foundation exists | ⏳ Phase 3+ |

### Phase 3+ (When Mutual Aid Foundation + Community Elections Exist)

| Role | Responsibility | Then |
|------|-----------------|------|
| **Founder** | S0vryn9 finances, reports quarterly to Governance Board | ✅ Continues |
| **Backend Trustee (Primary)** | Appointed by founder; maintains infrastructure | ✅ Active |
| **Backend Trustee (Voted #2, #3)** | Elected by Governance Board; co-oversight of ecosystem | ✅ Elected (Phase 3) |
| **User Councils** (3) | Elected community admins, real-time platform feedback | ✅ Elected |
| **Cross-Platform Governance Board** (9) | 3 delegates from each User Council; veto + emergency authority | ✅ Active |

### Emergency Activation (When Dead Man's Switch Triggers)

**The 3-Trustee Oversight Model takes effect:**

| Role | Responsibility |
|------|-----|
| **Backend Trustee (Primary)** | Founder-appointed; leads technical response, infrastructure continuity |
| **Trustee #2 (Voted)** | Governance Board-elected; ensures board direction is followed |
| **Trustee #3 (Voted)** | Governance Board-elected; independent verification + ethical oversight |

**Collectively, the 3 trustees:**
- Oversee entire ecosystem stabilization (Phases 1-3)
- Direct Emergency Council/Governance Board election process
- Approve major decisions requiring board consensus (3/3 must agree)
- Manage financial transitions + platform transitions
- Report daily to community on activation status
- Expand governance board and user councils to full operational capacity

---

## 🚨 Dead Man's Switch Triggers

The protocol activates when ANY of these conditions occur:

### 1. **Inactivity Trigger (Automatic)**
- **Condition**: Founder has not logged into S0vryn9 admin dashboard for **90 consecutive days**
- **Detection**: Automated system check runs weekly; alerts sent at Day 60, 75, 85
- **Email**: Founder receives warning emails → "Haven't logged in; everything ok?" 
- **Activation**: Day 90 trigger sent to Backend Trustee + legal representative
- **Day 95**: If still no founder response, protocol auto-activates

### 2. **Emergency Trigger (Cross-Platform Board Vote)**
- **Condition**: Cross-Platform Governance Board votes (requires 6/9 majority) that founder is incapacitated
- **Evidence**: Medical certification, family notification, legal documentation
- **Waiting Period**: 30-day public notice period before activation
- **Why 30 days?**: Allows founder to challenge if this was filed in error
- **Activation**: Day 31 if no founder override

### 3. **Legal/Death Trigger (Verified Document)**
- **Condition**: Verified death certificate or legal incapacity declaration submitted by executor/POA
- **Verification**: Backend Trustee + legal representative confirm document authenticity
- **Activation**: Immediate (no waiting period)

### 4. **Voluntary Transition Trigger (Founder Initiated)**
- **Condition**: Founder submits written declaration to transition to governance board
- **Form**: Signed letter + public announcement
- **Transition Period**: 60 days (allows orderly handoff)
- **Activation**: Day 1 = begin election process; Day 60 = full transition complete

---

## 📅 Phase-by-Phase Activation Protocol

---

### **PHASE 1: EMERGENCY STABILIZATION (Days 1-7)**

**Timeline**: When trigger activates (auto Day 95 inactivity, or Day 31 emergency vote, or immediate for death)

**Objective**: Stop the bleeding. Secure all systems. Prevent chaos.

#### **Day 1 Action: Immediate Lockdown**

**Who acts?**
- **Backend Trustee (Primary)** (technical lead; founder-appointed)
- **Backend Trustees #2 & #3** (board-voted; if already elected in Phase 3, they activate now)
- Legal representative (holder of founder's succession documents)
- **3-Trustee Oversight Council** (if Phase 3+: all three trustees meet immediately with board chair)

**What happens?**

1. **Infrastructure Freeze (3-Trustee Coordination)**
   - **Primary Backend Trustee** logs into all systems (S0vryn9, 3mpwrApp, Ascendant backends)
   - **Trustees #2 & #3** authenticate and verify all system states (independent dual-check)
   - Sets all platforms to **read-only mode** (users can read + play, but no new data writes to critical systems)
   - All three trustees sign off on freeze action (audit trail: who authorized, when, why)
   - Exception: Health/safety emergencies can still be processed (e.g., crisis helpline messages in 3mpwrApp)
   - All scheduled deployments → cancelled
   - API rate limits → tightened to prevent exploitation during uncertainty

2. **Access Audit (3-Trustee Verification)**
   - **Primary Trustee** reviews all active admin sessions
   - **Trustee #2** independently audits access logs for anomalies
   - **Trustee #3** verifies no unauthorized modifications occurred during inactivity
   - Revokes any unauthorized or inactive credentials (requires 2/3 trustee approval)
   - Logs all access attempts for next 7 days (audit trail for legal + board review)
   - Disables webhook endpoints (prevents external systems from triggering writes)
   - Daily sync-up call: All three trustees confirm status

3. **Communication Cascade**
   - **In-app banner** (all three platforms): "The S0vryn9 ecosystem is undergoing leadership transition. Services are in read-only mode while we ensure continuity. Updates every 24 hours."
   - **Email to all registered users**: Founder status, what's paused, what's working, support contact info
   - **Social media**: Single unified post across X, Bluesky, Mastodon, Facebook
   - **Discord/community channels**: Pinned message with FAQ + support thread

4. **Legal Sequence**
   - Legal representative verifies trigger legitimacy (death cert, medical docs, emergency vote tally)
   - Records verification in timestamped log
   - Notifies **three trusted parties** (if pre-designated by founder):
     - Emergency contact #1
     - Emergency contact #2
     - Organization representative (e.g., disability advocacy partner)
   - These three become the **Continuity Advisory Group** for this phase

#### **Days 2-3: Stakeholder Notification (3-Trustee Briefings)**

1. **Founder's Family/Emergency Contacts**
   - Legal representative confirms status with founder's emergency contact
   - **Primary Trustee** briefs family on infrastructure status + governance transition
   - Shares succession plan overview
   - **Trustees #2 & #3** available for technical questions
   - Explains next steps in plain language (3-trustee team will oversee ecosystem)

2. **Platform Stakeholders**
   - 3mpwrApp: Notify disability advocacy partners, legal representatives of users
   - Ascendant: Notify game community leaders, partners, any external publishers
   - Mutual Aid Foundation (if exists): Notify beneficiary community, board chair

3. **Financial**
   - Bank statements frozen at trigger date
   - Accountant notified (prepare financial transparency for next phase)
   - Payment processors notified (pause any scheduled payouts, await board approval)

#### **Days 4-7: System Verification & Documentation (3-Trustee Audit)**

1. **Backup Verification (2/3 Trustee Sign-Off)**
   - **Primary Trustee** verifies all critical backups are intact and restorable
   - **Trustee #2** independently tests restore process (user data, transaction history, community logs)
   - **Trustee #3** verifies encryption keys are secure and documented
   - Requires 3/3 sign-off before proceeding to Phase 2
   - Confirms encrypted founder credentials are stored securely (in password manager or vault accessed by all 3 trustees)

2. **Documentation Gathering**
   - Collect all system documentation (API keys, deployment procedures, database schemas)
   - Organize in **Continuity Handoff Folder** (encrypted, accessible to Trustee + Board when formed)
   - Include: README, quick-start guides, emergency contact list, incident response procedures

3. **Legal Filings**
   - If death: File preliminary succession notice with relevant authorities
   - If incapacity: Update corporate records with temporary administrator
   - Archive all documentation in legal file (date-stamped)

#### **End of Phase 1 Deliverable:**
- ✅ All systems secured in read-only mode
- ✅ All stakeholders notified
- ✅ No platform access compromised
- ✅ Legal verification complete
- ✅ Continuity documentation organized

---

### **PHASE 2: COMMUNITY GOVERNANCE BOARD ELECTION (Days 8-45)**

**Timeline**: Runs in parallel with Phase 1 stabilization. Ends between Day 30-45 depending on platform maturity.

**Objective**: Community elects leaders who will guide each platform forward.

#### **Precondition: Do User Councils Exist Yet?**

**If Phase 1-2 (Now):** No elections have happened. Platforms have only general admins, not elected councils.
- **Action**: Skip to "Emergency Council Formation" below

**If Phase 3+ (Future):** User Councils already exist and elected.
- **Action**: Go to "Formal Board Election" process

---

#### **Path A: Emergency Council Formation (Phase 1-2 Scenario, Under 3-Trustee Guidance)**

**Used if this trigger happens before community elections are live.**

**Days 8-10: Identify Community Leaders (3-Trustee Facilitation)**

1. **Longest-Serving Leadership** (Data-driven; curated by 3-trustee team)
   - **Primary Trustee** pulls data: 5 most active general admins / moderators per platform (by tenure + actions)
   - **Trustee #2** cross-checks for conflicts of interest
   - **Trustee #3** verifies accessibility advocacy history
   - 3mpwrApp: Identify 5 most active general admins / moderators
   - Ascendant: Identify 5 most active community leaders / moderators
   - Mutual Aid Foundation (if exists): Identify 5 most active advocates / board members

2. **Eligibility Criteria** (Must meet ALL)
   - Active on platform for **minimum 6 months**
   - Disability lived experience OR documented ally work (2+ years activism minimum)
   - No conflict of interest with founder (not family, not paid contractor currently)
   - Willing to serve 3-year term (commitment letter required)

3. **Nomination Call**
   - Public post: "Help us elect platform stewards"
   - Open nominations for 3 days (Days 8-10)
   - Accept self-nominations only
   - Require: Statement of experience + conflict of interest disclosure

**Days 11-14: Vetting**

1. **Public Forum**
   - Each nominee posts short bio (500 words)
   - Community can ask questions in dedicated thread
   - Nominees answer publicly

2. **Accessibility Audit**
   - Review nominee's accessibility advocacy history
   - Check: Have they supported disabled users? Accessibility improvements? Community building with PWD?
   - Red flag: Anyone with history of gatekeeping or exclusion

3. **Conflict Check**
   - Legal representative reviews for any disqualifying conflicts
   - Transparency: Results posted publicly with rationale

**Days 15-20: Voting**

1. **Voting Mechanism**
   - **Who votes?** All platform users with 30+ days active
   - **Vote weight:**
     - PWD users = **2 votes** each
     - Ally users = **1 vote** each
   - **Threshold**: Top 5 nominees with highest vote counts become Emergency Council
   - Tiebreaker: PWD users vote to break ties (disability-led principle preserved)

2. **Transparency**
   - Vote counts posted publicly (anonymized)
   - All ballots stored for audit trail
   - Results announced Day 21

**Days 21-25: Emergency Council Formation**

1. **Orientation**
   - Meet individually with Backend Trustee + legal representative
   - Full system walkthrough (what data exists, how platforms work, what's frozen)
   - Sign Emergency Council Charter (legal doc, confidentiality + responsibilities)

2. **First Council Session**
   - Meet together (cross-platform reps)
   - Review frozen systems together
   - Understand Phase 3: What governance model will replace this?

#### **Path B: Formal Board Election (Phase 3+ Scenario)**

**If this trigger happens AFTER User Councils already elected:**

**Days 8-14: Formal Nomination**

1. **Existing User Council Delegates**
   - Each platform's User Council (5 members) nominates 3 delegates to Cross-Platform Board
   - Nominees must be User Council members OR users with 1+ year platform tenure

2. **Public Vetting** (same as Path A Days 11-14)

**Days 15-20: Formal Voting** (same as Path A)

**Days 21-25: Cross-Platform Board Formation**
- 9-person board (3 from each platform) meets for first time
- Signs board charter + confidentiality agreements
- Reviews frozen systems together

---

#### **End of Phase 2 Deliverable:**
- ✅ Emergency Council OR Cross-Platform Board elected (9 members total)
- ✅ All members signed legal agreements
- ✅ First meeting held; systems briefing complete
- ✅ Public communication explaining new leadership

---

### **PHASE 3: TECHNICAL & FINANCIAL TRANSITION (Days 31-60)**

**Timeline**: Board now has infrastructure access. Founder's solo control → shared governance.

**Objective**: Move from read-only to operational governance. Each platform's council takes daily control.

#### **Days 31-35: Access Handoff**

1. **Firebase/Backend Access Transfer**
   - Backend Trustee creates new **governance-level admin accounts** (one per board member)
   - Accounts have scoped permissions (only their platform's data, no cross-reads)
   - Founder's original admin account → archived (read-only, audit-only)
   - All credentials stored in encrypted vault shared by:
     - Backend Trustee (technical)
     - Board chair (governance)
     - Legal representative (oversight)

2. **Secrets & Keys Rotation**
   - Rotate all API keys (any external integrations)
   - Regenerate database connection secrets
   - Update SSH keys for server access
   - No old credentials remain active

3. **Testing Phase**
   - Each board member tests their new access in **staging environment**
   - Confirm: Can read their platform's data, can write to appropriate collections, cannot access other platforms' sensitive data
   - Dry-run: Practice making one small test change, then rollback

#### **Days 36-40: Financial Transition (3-Trustee Oversight)**

1. **S0vryn9 Account Transfer** (if founder is incapacitated/deceased)
   - **Scenario A** (Founder living but transitioning): Founder retains account, shares read-only reporting with Board + 3 trustees
   - **Scenario B** (Founder deceased/incapacitated): **All 3 trustees** + Board chair gain co-signer access (requires 2/3 trustee + board chair for any withdrawal)
   - Bank account → add Board chair + Primary Trustee as co-signers (signature required for any withdrawal; 2 of 3 must approve)
   - All financial records transferred to transparent ledger (reviewed by Board quarterly, audited by trustees)
   - **Trustee #2** maintains independent financial records (verification copy)
   - **Trustee #3** conducts monthly reconciliation (trustees vs. board ledger)

2. **Monthly/Quarterly Reporting Setup**
   - Finance template created
   - Due: Every quarter (March 31, June 30, Sept 30, Dec 31)
   - Includes: Revenue by platform, expenses, allocations to foundation, any staff compensation
   - Public: Anonymized version posted to community (no PII)

3. **Payment Processor Updates**
   - Stripe / payment gateway: Add Board chair as account contact
   - Requires 2 people to approve any payout (if platform has > $X revenue)
   - Notification: Board chair gets email for every transaction over threshold

#### **Days 41-45: Policy Adoption (3-Trustee + Board Coordination)**

1. **Multi-Signature Governance Rules** (Per 3mpwrApp model + 3-Trustee Enforcement)
   - Major decisions require **3/5 board approval** minimum + **2/3 trustee sign-off** during Phase 3
   - This prevents any single trustee from overruling board decisions
   - Primary Trustee enforces technical multi-sig; Trustees #2 & #3 verify
   - What triggers this? (examples)
     - Any change to platform fees or monetization
     - Removal of features or user data
     - Partnerships or integrations
     - Allocation changes to foundation
   - Voting conducted via secure form (timestamped, logged)

2. **Firestore Rules Update** (Technical enforcement by 3 trustees)
   - Deploy new Firestore security rules: `require 3/5 board approval + 2/3 trustee approval for god-mode writes`
   - **All 3 trustees** must authenticate to deploy (prevents single-trustee unilateral action)
   - Audit log created per transaction (who, when, what, why, which trustees approved)
   - Board must document reasoning for each major action
   - Trustees maintain separate audit log (verify board log integrity)

3. **Decision Log Template**
   - Board commits to logging all major decisions in shared document
   - Public archive: Every quarter, non-sensitive decisions published
   - Transparency: Community can see why governance board made X choice

#### **Days 46-50: Platforms Resume Operations (3-Trustee Go-Live Approval)**

1. **Read-Only Lifted** (Requires 3/3 Trustee Sign-Off)
   - **All 3 trustees** test production systems in staging environment first
   - **Trustee #2 & #3** independently verify no security issues introduced
   - Users can now create new posts, upload evidence, make purchases
   - Real-time moderation resumes (User Council handles flagged content; trustees monitor)
   - Features resume (games update in Ascendant, new tools in 3mpwrApp)
   - Trustees on-call for first 72 hours of live operations (emergency escalation protocol)

2. **Support Restart**
   - User support teams (each platform) resume handling tickets
   - Crisis support prioritized (3mpwrApp accessibility issues, Ascendant bugs, etc.)

3. **Community Update**
   - Blog post: "Transition complete. Here's your new governance structure"
   - FAQ addressing concerns: "Will my data be safe? Who has access? How do I contact leadership?"

#### **Days 51-60: Full Normalization**

1. **New Admin Onboarding**
   - User Council members (general admins) onboarded to report to new board
   - Chain of command clarified: User Council → Board → (if Phase 3+) Founder for quarterly reporting

2. **Scheduled Maintenance Window**
   - Deploy any queued bug fixes and feature updates
   - Confirm all systems stable under new governance

3. **External Communication**
   - Email to all stakeholders: Partners, investors, advocacy organizations
   - Message: "Transition successful. Governance is now community-led."

#### **End of Phase 3 Deliverable:**
- ✅ All admin access transferred to board (founder archived or co-manages)
- ✅ Financial accounts co-signed or transferred
- ✅ Multi-signature rules deployed and tested
- ✅ Platforms fully operational under new governance
- ✅ Community informed + supportive

---

### **PHASE 4: ONGOING GOVERNANCE (Day 61+)**

**Timeline**: Permanent new normal.

**Objective**: Sustain community governance indefinitely.

#### **Quarterly Cycles (3-Trustee Continued Oversight)**

**Every 3 months (March, June, Sept, Dec):**

1. **Joint Trustees + Board Meeting** (In-person or video)
   - **All 3 trustees** attend + present infrastructure/financial audit
   - Review quarterly financial report (S0vryn9; compared against trustees' independent copy)
   - Emergency escalations from User Councils
   - Strategic decisions needed (partnerships, major features, etc.)
   - Vote on any major decisions (3/5 board approval + 2/3 trustee verification)
   - Trustees publish independent audit summary (did board follow rules?)
   - Publish decision log (anonymized; includes trustee attestation)

2. **Community Townhall** (Open to all users)
   - Board presents quarterly decisions
   - Q&A: Users can ask anything
   - Feedback collection for next quarter

3. **Financial Transparency Report**
   - Public + anonymized version (no founder PII if applicable)
   - Breakdown: Revenue, expenses, foundation allocations
   - Comparison to last quarter + annual plan

#### **Annual Cycles**

**Every 12 months (February 26, per founding date):**

1. **Leadership Re-election** (Optional, user-voted)
   - If board is performing well: Members can choose to continue
   - If member steps down: Replacement election held (30-day process)
   - Vote: Same weighting as original (PWD 2x, Allies 1x)

2. **Strategic Review**
   - Where are 3mpwrApp, Ascendant, foundation at?
   - Adjust allocation percentages if revenue changed
   - Set goals for next 12 months

3. **Audit**
   - External or board-conducted financial audit
   - Security audit: Any breaches or close calls?
   - Community sentiment survey

#### **In Case of Emergency** (Any time)

**Board Emergency Session** (24-hour call)
- Security breach detected
- Financial irregularity found
- Critical user complaint / privacy violation
- Backend Trustee calls emergency board session
- Decision made by 2/3 majority (faster than 3/5 normal threshold)

---

## 🛡️ Continuity Safeguards

### **Backend Trustee Role (Critical)**

**Who:** Named by founder later (TBD)

**Access:**
- ✅ Full Firestore admin access (read-only copy of secrets)
- ✅ GitHub admin (to deploy security patches)
- ✅ Database backups (restore in emergency)
- ✅ SSH access to servers

**Cannot:**
- ❌ Unilaterally access user data (must log request + reasoning)
- ❌ Delete data without board approval
- ❌ Add themselves as platform admin (board must approve)

**In Succession Event:**
- Works immediately with Cross-Platform Board
- Enables board members' access to systems
- Maintains infrastructure stability
- Reports all actions to board weekly

**Backup Plan:** If Backend Trustee becomes unavailable:
- Secondary trustee (to be named later, or elected from board technical member)
- 2-of-3 required to activate (trustee #1, trustee #2, board chair)

---

### **Documentation & Storage**

**Founder Succession Instructions:**
- Stored in: Password manager (1Password, Bitwarden) **+ lawyer's office (physical copy)** + encrypted GitHub private repo (PGP key held by 3 trusted parties)
- Contents: List of trustees, board criteria, financial thresholds, emergency contacts

**What gets stored?**
- Founder's personal notes on platform philosophy / decision-making
- List of key relationships (partners, advocacy orgs, experts)
- Financial targets + runway assumptions
- Any personal wishes for platform direction post-transition

**Who can access?**
- Founder (obviously)
- Legal representative
- Backend Trustee
- Board chair (once board exists)

---

## 📊 Success Criteria

By end of Day 60, the transition is successful if:

- ✅ All systems are operational (users can use platforms)
- ✅ New governance board is elected and functioning
- ✅ Financial accountability established (board sees all spending)
- ✅ Community trusts the new leadership (sentiment survey >70% positive)
- ✅ No security breaches or data loss during transition
- ✅ Founder's work is documented for future reference

---

## 🚦 Escalation Matrix

| Trigger | Response | Urgency |
|---------|----------|---------|
| Founder missing 3+ months | Activate Phase 1 (lockdown) | CRITICAL |
| Board member compromised | Emergency board vote to remove | HIGH |
| Data breach discovered | Freeze affected data + notify stakeholders | CRITICAL |
| Financial irregularity | Audit + board investigation | HIGH |
| User files complaint about decision | Board reviews + transparency report | MEDIUM |
| Server goes down | Backend Trustee + board tech member fix immediately | CRITICAL |

---

## ✅ Version History

| Version | Date | Author | Change |
|---------|------|--------|--------|
| 1.0 | 2026-02-26 | Founder | Initial comprehensive protocol — all 4 phases detailed with timelines and responsibilities |
