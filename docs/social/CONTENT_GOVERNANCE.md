# Content Governance

This is the canonical home for social and blog content quality rules during repository consolidation.

## Truthfulness Rules

- Match claims to the current codebase and canonical docs.
- Do not present aspirational or planned features as shipped.
- Use present-progressive framing for in-development work when public release is not complete.
- Treat automation-generated content as untrusted until reviewed.

## Known Accuracy Lessons

### Accessibility claims
- Use the real implemented count for colorblind modes.
- When referencing feature behavior, prefer code-backed wording over marketing shorthand.

### Development-stage claims
- Avoid "launched," "shipped," "live," or release-candidate wording unless that state is verified in canonical status docs.
- Public web/build availability must match `docs/BUILD_READINESS_STATUS.md`.

### Community-feature claims
- Do not write as if a live player community already exists unless the repo state confirms it.
- Planned future-facing community systems must be framed as roadmap or design intent, not current reality.

## Duplication and Variety Rules

- Avoid clustering too many posts around the same feature or philosophy in the same publishing window.
- Prefer widening coverage across underrepresented systems before generating more variants of the same topic.
- Maintain a balanced mix across accessibility, realm mechanics, technical execution, mindful design, and community systems.

## Optimization Rules

- Keep platform length limits in view before content is approved.
- Prefer ASCII-safe output for automated content unless a platform-specific reason requires otherwise.
- Optimize hooks without overstating progress or inventing social proof.

## Review Checklist

1. Verify factual claims against code or canonical docs.
2. Verify development-stage wording against `docs/BUILD_READINESS_STATUS.md`.
3. Check for repeated topic clustering in the content bank.
4. Confirm platform length constraints and encoding safety.
5. Confirm any automation changes update the governing generator or source bank, not only generated output.

## Canonical References

- `docs/social/README.md`
- `docs/social/TESTING_GUIDE.md`
- `docs/BUILD_READINESS_STATUS.md`
- `docs/technical/GITHUB_SETUP.md`
