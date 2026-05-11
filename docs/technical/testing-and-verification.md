# Testing and Verification

This is the canonical testing and verification guide for repository consolidation.

## Testing Layers

1. Feature testing while a system is being implemented.
2. Integration testing across progression, save/load, UI, audio, and accessibility.
3. Device and platform testing before release.
4. Automation validation for website and social workflows.

## Minimum Verification Standard

- Verify the touched gameplay or automation behavior directly.
- Check console, build, or workflow output for errors.
- Confirm accessibility-sensitive changes still behave correctly.
- Record unresolved gaps in canonical docs instead of creating new root notes.

## Gameplay Verification Focus

When working on realms or UI, verify:

- scene loads without blocking errors
- controller and prefab references are assigned
- core interaction loop works in Play Mode
- progress or UI feedback updates correctly
- no obvious regression in save/load, audio, or accessibility behavior

## Platform and Release Checks

- WebGL and Firebase-hosted website checks should align with `docs/BUILD_READINESS_STATUS.md`.
- Device coverage should include at least one representative mobile profile plus WebGL browser coverage when relevant.
- Use local smoke tests before broader release claims.

## Automation and Content Checks

- Use `docs/social/TESTING_GUIDE.md` for social posting validation.
- Use `docs/social/CONTENT_GOVERNANCE.md` for factual-claim review rules.
- Run repo and content validators before merge when scripts or automation are touched.

## Historical Verification Records

- Point-in-time verification snapshots belong in canonical history docs, not new root markdown files.
- Website and automation milestone verification is consolidated in `docs/operations/website-automation-history.md`.
- Security verification history is consolidated in `docs/security/security-operations-history.md`.

## Related Canonical Docs

- `docs/BUILD_READINESS_STATUS.md`
- `docs/technical/GITHUB_SETUP.md`
- `docs/technical/OPTIMIZATION_SECURITY.md`
- `docs/social/TESTING_GUIDE.md`
