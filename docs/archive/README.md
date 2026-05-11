# Docs Archive Index

This folder stores historical documentation moved out of the repository root during Wave 2 archival.

## Archive Policy

- Archive moves follow `docs/operations/archive-policy.md`.
- Historical filenames are preserved for auditability.
- Canonical guidance stays under active docs; archive files are history-only.

## Archived Sets

- `docs/archive/legacy-root/`: 56 legacy root markdown files moved in Wave 2.

## Validation Gate Used

- `npm run validate:structure`
- `npm run validate:links`
- `npm run validate:dedup`
- `node scripts/validation/content-validator.js <payload.json>`
