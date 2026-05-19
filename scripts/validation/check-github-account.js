import fs from 'fs';
import path from 'path';
import { execSync } from 'child_process';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const projectRoot = path.resolve(__dirname, '..', '..');
const policyPath = path.join(projectRoot, '.github-account-policy.json');

function run(command) {
    try {
        return execSync(command, {
            cwd: projectRoot,
            stdio: ['ignore', 'pipe', 'pipe'],
            encoding: 'utf8',
        });
    } catch (error) {
        const stdout = error?.stdout ? String(error.stdout) : '';
        const stderr = error?.stderr ? String(error.stderr) : '';
        return `${stdout}\n${stderr}`.trim();
    }
}

function getRemoteOwner() {
    const output = run('git remote get-url --push origin').trim();
    if (!output) return null;

    let match = output.match(/github\.com[:/]([^/]+)\/([^/]+?)(?:\.git)?$/i);
    if (!match) {
        match = output.match(/^([^/]+)\/([^/]+?)(?:\.git)?$/i);
    }

    return match ? match[1] : null;
}

function getActiveGhAccount() {
    const status = run('gh auth status --hostname github.com');
    if (!status) return null;

    const lines = status.split(/\r?\n/);
    let lastAccount = null;

    for (const line of lines) {
        const accountMatch = line.match(/account\s+([^\s]+)\s*\(/i);
        if (accountMatch) {
            lastAccount = accountMatch[1];
            continue;
        }

        if (/Active account:\s*true/i.test(line)) {
            return lastAccount;
        }
    }

    return null;
}

function loadPolicy() {
    if (!fs.existsSync(policyPath)) {
        return {
            enabled: true,
            defaultAllowedAccounts: [],
            ownerRules: {},
        };
    }

    try {
        return JSON.parse(fs.readFileSync(policyPath, 'utf8'));
    } catch (error) {
        console.error('❌ GitHub account check: invalid JSON in .github-account-policy.json');
        console.error(error.message);
        process.exit(1);
    }
}

function main() {
    if (process.env.GH_ACCOUNT_CHECK_BYPASS === '1') {
        console.log('⚠️ GitHub account pre-push check bypassed via GH_ACCOUNT_CHECK_BYPASS=1');
        return;
    }

    const policy = loadPolicy();
    if (policy.enabled === false) return;

    const remoteOwner = getRemoteOwner();
    if (!remoteOwner) {
        console.log('⚠️ GitHub account pre-push check skipped (could not determine origin owner)');
        return;
    }

    const defaultAllowed = Array.isArray(policy.defaultAllowedAccounts)
        ? policy.defaultAllowedAccounts
        : [];
    const ownerRules = policy.ownerRules && typeof policy.ownerRules === 'object'
        ? policy.ownerRules
        : {};

    const allowedAccounts = Array.isArray(ownerRules[remoteOwner])
        ? ownerRules[remoteOwner]
        : defaultAllowed;

    if (allowedAccounts.length === 0) {
        console.log(`ℹ️ GitHub account pre-push check: no allowlist configured for owner '${remoteOwner}', skipping.`);
        return;
    }

    const activeAccount = getActiveGhAccount();
    if (!activeAccount) {
        console.error('❌ Could not determine active GitHub CLI account. Push blocked for safety.');
        console.error('   Run: gh auth status');
        console.error('   Then switch: gh auth switch --user <account>');
        console.error('   Or bypass once: GH_ACCOUNT_CHECK_BYPASS=1 git push');
        process.exit(1);
    }

    if (!allowedAccounts.includes(activeAccount)) {
        console.error('❌ GitHub account safety check failed. Push blocked.');
        console.error(`   Repo owner: ${remoteOwner}`);
        console.error(`   Active GH account: ${activeAccount}`);
        console.error(`   Allowed account(s): ${allowedAccounts.join(', ')}`);
        console.error('   Switch account: gh auth switch --user <allowed-account>');
        console.error('   Sync git auth: gh auth setup-git');
        console.error('   Temporary bypass: GH_ACCOUNT_CHECK_BYPASS=1 git push');
        process.exit(1);
    }

    console.log(`✅ GitHub account check passed (${activeAccount} -> ${remoteOwner})`);
}

main();
