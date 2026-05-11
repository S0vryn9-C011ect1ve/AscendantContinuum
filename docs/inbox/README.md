# Docs Inbox

This folder is the automatic landing zone for newly created root-level markdown files that do not already belong to an approved root path.

## How It Works

- `npm run organize:watch` watches the repository root.
- Any new root `.md` file that is not in the approved root list is moved here automatically.
- Use this folder as a triage queue before placing the file into its canonical docs domain.

## Canonical Targets

- `docs/operations/`
- `docs/technical/`
- `docs/social/`
- `docs/security/`
- `docs/standards/`