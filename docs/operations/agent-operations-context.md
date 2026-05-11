# Agent Operations Context

This document is the canonical destination for historical agent workflow and orchestration notes during repository consolidation.

## Purpose

The repository previously kept several root-level agent documents that mixed project execution, agent behavior, orchestration, and session reporting. Those documents are now treated as historical operational context, not active repository policy.

## Canonical Rules

- Repository policy is governed by the current repo standards, contributor guidance, and migration tracker.
- Root-level legacy agent documents should not be used as the source of truth for new repository changes.
- New agent/process guidance belongs under `docs/operations/` or repository governance files, not at the root.

## Historical Themes Consolidated Here

### Agent execution and orchestration
- Orchestrator-first workflow and subagent coordination patterns.
- Preference for planning, prioritization, and execution tracking.
- Historical model/runtime notes captured for context only.

### Agent session operations
- Daily execution cadence, task selection discipline, and progress logging.
- Preference for linking canonical docs instead of duplicating status blocks.
- Historical TDD-oriented execution guidance for feature slices.

### Agent reports and lessons
- Verification snapshots, readiness assessments, and one-off agent reports are historical records.
- Lessons learned, mistake-prevention patterns, and execution rules should be migrated into contributor/governance guidance when still relevant.

## Active Repository Destinations

Use these canonical docs instead of the legacy root agent files:

- `README.md`
- `CONTRIBUTING.md`
- `docs/_index.md`
- `docs/operations/project-overview.md`
- `docs/operations/implementation-guides.md`
- `docs/operations/legacy-doc-consolidation-wave-tracker.md`
- `docs/technical/testing-and-verification.md`

## Historical Source Set

This document replaces the need to consult these root-level files as active guidance:

- `AGENT_INSTRUCTIONS.md`
- `AGENT_OPERATIONS.md`
- `AGENT_REPORT_TODAY.md`
- `AGENT-ORCHESTRATOR.md`
- `LEARNINGS.md`

## Scope Rule

These materials are historical context only. They do not override current repository governance or current runtime/system instructions.
