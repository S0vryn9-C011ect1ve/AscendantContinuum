# Legacy Doc Consolidation Wave Tracker

This tracker governs strict execution order:

1. Wave 1: must-merge docs
2. Wave 2: archive-only docs
3. Wave 3: delete-after-verification

No later wave starts until the previous wave is complete.

## Wave 1 - Must Merge (Complete)

| Source | Destination | Status | Notes |
|---|---|---|---|
| `START_HERE.md` | `README.md` + `docs/technical/GITHUB_SETUP.md` | done | superseded pointer created |
| `START_HERE_TODAY.md` | `docs/BUILD_READINESS_STATUS.md` | done | superseded pointer created |
| `MASTER_INDEX.md` | `README.md` | done | superseded pointer created |
| `GETTING_STARTED.md` | `docs/technical/GITHUB_SETUP.md` | done | superseded pointer created |
| `GITHUB_ACTIONS_SETUP.md` | `docs/technical/GITHUB_SETUP.md` + `docs/social/README.md` | done | secrets removed; pointer kept |
| `SOCIAL_AUTOMATION_SETUP.md` | `docs/social/README.md` + `docs/technical/GITHUB_SETUP.md` | done | superseded pointer created |
| `SETUP_CHECKLIST.md` | `docs/social/README.md` + `docs/technical/GITHUB_SETUP.md` | done | superseded pointer created |
| `AUTOMATION_DEPLOYMENT_GUIDE.md` | `docs/technical/GITHUB_SETUP.md` + `docs/social/README.md` | done | superseded pointer created |
| `BLOG_POST_CHECKLIST.md` | `blog/BLOG_CREATION_GUIDE.md` | done | superseded pointer created |
| `FOUNDATION_COMPLETE.md` | `README.md` + `docs/operations/project-overview.md` | done | superseded pointer created |
| `PROJECT_SUMMARY.md` | `README.md` + `docs/operations/project-overview.md` | done | superseded pointer created |
| `CONSTITUTION.md` | `docs/legal/governance/README.md` + `CONTRIBUTING.md` | done | superseded pointer created |
| `EMBERFORGE_EXACT_WIRING.md` | `docs/operations/implementation-guides.md` | done | superseded pointer created |
| `EMBERFORGE_SUPER_SIMPLE.md` | `docs/operations/implementation-guides.md` | done | superseded pointer created |
| `EMBERFORGE_POLISH_SIMPLE.md` | `docs/operations/implementation-guides.md` + `docs/POLISH_ART_AUDIO_ROADMAP.md` | done | superseded pointer created |
| `VERDANT_SUPER_SIMPLE.md` | `docs/operations/implementation-guides.md` | done | superseded pointer created |
| `UNITY_SUPER_SIMPLE.md` | `docs/operations/implementation-guides.md` | done | superseded pointer created |
| `CONTENT_AUDIT_REPORT.md` | `docs/social/CONTENT_GOVERNANCE.md` | done | superseded pointer created |
| `CONTENT_DUPLICATION_AUDIT.md` | `docs/social/CONTENT_GOVERNANCE.md` | done | superseded pointer created |
| `CONTENT_OPTIMIZATION_REPORT.md` | `docs/social/CONTENT_GOVERNANCE.md` | done | superseded pointer created |
| `CONTENT_FIXES_COMPLETE.md` | `docs/social/CONTENT_GOVERNANCE.md` | done | superseded pointer created |
| `CONTENT_ACCURACY_AUDIT_2026-03-18.md` | `docs/social/CONTENT_GOVERNANCE.md` | done | superseded pointer created |
| `FIREBASE_SETUP_GUIDE.md` | `docs/technical/GITHUB_SETUP.md` | done | superseded pointer created |
| `FIREBASE_FIX_SIMPLE.md` | `docs/technical/GITHUB_SETUP.md` | done | superseded pointer created |
| `TESTING_GUIDE.md` | `docs/technical/testing-and-verification.md` | done | superseded pointer created |
| `SCENE_ASSEMBLY_GUIDE.md` | `docs/operations/unity-scene-prefab-workflows.md` | done | superseded pointer created |
| `PREFAB_CREATION_GUIDE.md` | `docs/operations/unity-scene-prefab-workflows.md` | done | superseded pointer created |
| `OPTIMIZATION_GUIDE.md` | `docs/technical/OPTIMIZATION_SECURITY.md` | done | superseded pointer created |
| `STATUS.md` | `docs/BUILD_READINESS_STATUS.md` | done | superseded pointer created |
| `CODE_STATUS.md` | `docs/BUILD_READINESS_STATUS.md` | done | superseded pointer created |
| `DEVELOPMENT_STATUS.md` | `docs/BUILD_READINESS_STATUS.md` | done | superseded pointer created |
| `SECURITY.md` | `docs/security/SUPPLY_CHAIN_PROTECTION.md` | done | root policy linked to canonical security docs |
| `AGENT_INSTRUCTIONS.md` | `docs/operations/agent-operations-context.md` | done | superseded pointer created |
| `AGENT_OPERATIONS.md` | `docs/operations/agent-operations-context.md` | done | superseded pointer created |
| `AGENT_REPORT_TODAY.md` | `docs/operations/agent-operations-context.md` | done | superseded pointer created |
| `AGENT-ORCHESTRATOR.md` | `docs/operations/agent-operations-context.md` | done | superseded pointer created |
| `LEARNINGS.md` | `docs/operations/agent-operations-context.md` | done | superseded pointer created |
| `COMPLETION_SUMMARY.md` | `docs/operations/project-overview.md` + `docs/operations/launch-resources.md` | done | superseded pointer created |
| `6_WEEK_LAUNCH_ROADMAP.md` | `docs/operations/launch-resources.md` | done | superseded pointer created |
| `ZERO_BUDGET_AUDIO_GUIDE.md` | `docs/operations/launch-resources.md` | done | superseded pointer created |
| `ZERO_BUDGET_MARKETING_GUIDE.md` | `docs/operations/launch-resources.md` | done | superseded pointer created |
| `TUTORIAL_IMPLEMENTATION_GUIDE.md` | `docs/operations/launch-resources.md` + `docs/operations/implementation-guides.md` | done | superseded pointer created |
| `TODAY_WORK_SESSION.md` | `docs/operations/launch-resources.md` | done | superseded pointer created |
| `TODAYS_UNITY_WORK.md` | `docs/operations/launch-resources.md` | done | superseded pointer created |
| `SOCIAL_POSTING_FIXED.md` | `docs/operations/website-automation-history.md` | done | superseded pointer created |
| `WHATS_NEW_SETUP_COMPLETE.md` | `docs/operations/website-automation-history.md` | done | superseded pointer created |
| `VERIFICATION_REPORT_2026-05-01.md` | `docs/operations/website-automation-history.md` + `docs/technical/testing-and-verification.md` | done | superseded pointer created |
| `SECURITY_AUDIT_APRIL_6_2026.md` | `docs/security/security-operations-history.md` | done | superseded pointer created |
| `SECURITY_HARDENING_COMPLETE.md` | `docs/security/security-operations-history.md` | done | superseded pointer created |
| `SECURITY_WEEKLY_SCAN_COMPLETE.md` | `docs/security/security-operations-history.md` | done | superseded pointer created |
| `SOCKET_DEV_VERIFICATION.md` | `docs/security/security-operations-history.md` | done | superseded pointer created |
| `MAXIMUM_PARANOIA_MODE_COMPLETE.md` | `docs/security/security-operations-history.md` | done | superseded pointer created |
| `HEARTBEAT.md` | `docs/operations/legacy-cross-project-context.md` | done | superseded pointer created |
| `MEMORY.md` | `docs/operations/legacy-cross-project-context.md` | done | superseded pointer created |
| `SOUL.md` | `docs/operations/legacy-cross-project-context.md` | done | superseded pointer created |
| `USER.md` | `docs/operations/legacy-cross-project-context.md` | done | superseded pointer created |
| `TOOLS.md` | `docs/operations/legacy-cross-project-context.md` | done | superseded pointer created |

## Wave 2 - Archive Only (Complete)

Entry criteria:

- All Wave 1 rows complete.
- Link and structure checks pass.
- Canonical docs updated.
- Archive moves follow `docs/operations/archive-policy.md`.

Validation status:

- `npm run validate:structure`: passed
- `npm run validate:links`: passed

Wave 2 execution status:

- 56 legacy root docs moved to `docs/archive/legacy-root/` with filename preservation.
- Root tombstones recreated for high-traffic legacy entry points.
- Archive index created at `docs/archive/README.md`.
- `npm run validate:dedup`: passed.
- Content validator executed with required payload format and real social copy sample: passed.

## Wave 3 - Delete After Verification (Ready After Manifest Review)

Entry criteria:

- Wave 2 complete.
- Delete manifest approved.
- CI and link checks clean.
