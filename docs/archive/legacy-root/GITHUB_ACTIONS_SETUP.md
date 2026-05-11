# Legacy Setup Note (Superseded)

This file previously contained legacy setup notes and is now superseded.

Use these canonical documents instead:

1. `docs/technical/GITHUB_SETUP.md` for repository and CI/CD setup
2. `docs/social/README.md` for social automation credentials and flow
3. `docs/security/SUPPLY_CHAIN_PROTECTION.md` for security controls

## Security Notice

- Never store real credentials, app passwords, or webhook URLs in tracked markdown files.
- Use GitHub Actions repository/environment secrets only.
- Rotate any credential that was previously exposed.

## Required Secret Names (Reference Only)

Populate values in GitHub Secrets, not in this file:

- `BLUESKY_HANDLE`
- `BLUESKY_APP_PASSWORD`
- `BLUESKY_IDENTIFIER`
- `BLUESKY_PASSWORD`
- `MASTODON_INSTANCE`
- `MASTODON_API_URL`
- `MASTODON_ACCESS_TOKEN`
- `DISCORD_WEBHOOK_URL`
- `FIREBASE_TOKEN`
- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`
