# Archive Policy

This document defines how Wave 2 archive moves and Wave 3 delete decisions should be executed after Wave 1 consolidation is complete.

## Archive Principles

- Preserve historical filenames when moving legacy docs into archive locations.
- Keep date-bearing filenames intact for auditability.
- Preserve one canonical destination for active guidance; archive copies are history only.
- Add or retain tombstone notes where high-traffic legacy paths existed.

## Delete Principles

- Delete only after a canonical destination exists or the file is confirmed obsolete.
- Keep a delete manifest with reason codes: `duplicate`, `generated`, `superseded`, `non-project`, `stale-log`.
- Re-run validation before and after destructive cleanup.

## Pre-Archive Validation

- Structure validation passes.
- Canonical docs exist and are linked from the docs index.
- Migration tracker reflects file status accurately.

## Pre-Delete Validation

- Archive move completed where required.
- No unresolved references remain.
- Delete manifest reviewed.
