/**
 * Content Bank Auditor — Scans public/social/content-bank.json for quality issues
 *
 * Flags entries with:
 *   - Hype / inauthentic tone
 *   - Placeholder text  (e.g. "[link to gist]")
 *   - "NPC" — replace with "Realm Dwellers"
 *   - Generic content with no game-specific details
 *   - Missing canonical links
 *   - Character limit violations per platform
 *
 * Usage:
 *   node scripts/audit/audit-content-bank.js              # full audit, outputs report
 *   node scripts/audit/audit-content-bank.js --json       # output as JSON (for rewrite-content-bank.js)
 *
 * Produces: scripts/audit/audit-report-YYYY-MM-DD.json (when --json flag used)
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const PROJECT_ROOT = path.join(__dirname, '..', '..');

const CONTENT_BANK_PATH = path.join(PROJECT_ROOT, 'public', 'social', 'content-bank.json');
const REPORT_DIR = path.join(__dirname);

// ─── Platform character limits ────────────────────────────────────────────────

const PLATFORM_LIMITS = {
    bluesky: 280,
    mastodon: 500,
};

// ─── Hype / inauthentic language patterns ────────────────────────────────────

const HYPE_PATTERNS = [
    { pattern: /literally doesn't exist elsewhere/i, label: 'exaggerated uniqueness claim' },
    { pattern: /steal this/i, label: 'clickbait hook ("Steal this")' },
    { pattern: /game mechanic that literally/i, label: 'hyperbole' },
    { pattern: /REVOLUTIONARY|NEVER DONE BEFORE|GAME CHANGING|MIND-?BLOWING/i, label: 'all-caps hype language' },
    { pattern: /you won't believe/i, label: 'clickbait phrasing' },
    { pattern: /nobody is talking about/i, label: 'false exclusivity' },
    { pattern: /this breaks everything/i, label: 'hyperbole' },
    { pattern: /viral/i, label: '"viral" — avoid' },
];

// ─── Placeholder / broken content ────────────────────────────────────────────

const PLACEHOLDER_PATTERNS = [
    { pattern: /\[link to gist\]/i, label: 'broken placeholder: [link to gist]' },
    { pattern: /\[TBD\]/i, label: 'placeholder: [TBD]' },
    { pattern: /\[coming soon\]/i, label: 'placeholder: [coming soon]' },
    { pattern: /\[URL\]/i, label: 'placeholder: [URL]' },
    { pattern: /\[insert/i, label: 'placeholder: [insert ...]' },
    { pattern: /TODO:/i, label: 'unfinished TODO marker' },
];

// ─── Deprecated terminology ───────────────────────────────────────────────────

const TERMINOLOGY_PATTERNS = [
    { pattern: /\bNPC\b/gi, label: 'outdated term "NPC" — use "Realm Dwellers"' },
    { pattern: /\bgame characters?\b/gi, label: 'vague "game characters" — use "Realm Dwellers"' },
];

// ─── Game-specific anchor terms (at least one should appear) ─────────────────

const GAME_SPECIFIC_TERMS = [
    'Emberforge', 'Verdant', 'Echo Fields', 'Dawn Citadel', 'Lantern',
    'ritual', 'sigil', 'deity', 'Realm Dweller', 'constellation',
    'moon phase', 'lunar', 'colorblind', 'Digital Sunset', 'fossil',
    'ascendant', 'continuum', 'ascendant-continuum.web.app',
    'accessibility', 'cosmetic', 'FOMO', 'solo dev',
];

// ─── Canonical links ──────────────────────────────────────────────────────────

const CANONICAL_LINKS = [
    'https://ascendant-continuum.web.app/',
    'https://ascendant-continuum.web.app/blog/',
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

function countChars(text) {
    try {
        return [...new Intl.Segmenter().segment(text)].length;
    } catch {
        return [...text].length;
    }
}

function combinedText(entry) {
    return [entry.hook ?? '', entry.body ?? ''].filter(Boolean).join('\n');
}

// ─── Audit a single entry ─────────────────────────────────────────────────────

function auditEntry(entry) {
    const issues = [];
    const text = combinedText(entry);

    // 1. Hype language
    for (const { pattern, label } of HYPE_PATTERNS) {
        if (pattern.test(text)) {
            issues.push({ type: 'hype_tone', detail: label });
        }
    }

    // 2. Placeholders
    for (const { pattern, label } of PLACEHOLDER_PATTERNS) {
        if (pattern.test(text)) {
            issues.push({ type: 'placeholder', detail: label });
        }
    }

    // 3. Deprecated terminology
    for (const { pattern, label } of TERMINOLOGY_PATTERNS) {
        if (pattern.test(text)) {
            issues.push({ type: 'terminology', detail: label });
        }
    }

    // 4. Generic content (no game-specific terms)
    const hasGameTerm = GAME_SPECIFIC_TERMS.some((term) =>
        text.toLowerCase().includes(term.toLowerCase())
    );
    if (!hasGameTerm) {
        issues.push({
            type: 'generic_content',
            detail: 'No game-specific terms found — could be about any game',
        });
    }

    // 5. Missing canonical link
    const hasLink = CANONICAL_LINKS.some((link) => text.includes(link));
    if (!hasLink) {
        issues.push({
            type: 'missing_link',
            detail: 'No canonical site/blog link included',
        });
    }

    // 6. Character limit violations per platform
    if (Array.isArray(entry.platforms)) {
        for (const platform of entry.platforms) {
            const limit = PLATFORM_LIMITS[platform];
            if (!limit) continue;
            const len = countChars(text);
            if (len > limit) {
                issues.push({
                    type: 'char_limit',
                    detail: `${platform}: ${len}/${limit} chars — exceeds limit`,
                });
            }
        }
    }

    return issues;
}

// ─── Main audit ───────────────────────────────────────────────────────────────

function runAudit(outputJson = false) {
    if (!fs.existsSync(CONTENT_BANK_PATH)) {
        console.error(`content-bank.json not found at: ${CONTENT_BANK_PATH}`);
        process.exit(1);
    }

    let bank;
    try {
        bank = JSON.parse(fs.readFileSync(CONTENT_BANK_PATH, 'utf-8'));
    } catch (err) {
        console.error(`Failed to parse content-bank.json: ${err.message}`);
        process.exit(1);
    }

    const items = bank.content ?? [];
    const allIssues = [];
    const rewriteQueue = [];

    const typeCounts = {};

    for (const entry of items) {
        const issues = auditEntry(entry);
        if (issues.length > 0) {
            allIssues.push({ id: entry.id, type: entry.type, issues });
            rewriteQueue.push(entry.id);
            for (const issue of issues) {
                typeCounts[issue.type] = (typeCounts[issue.type] ?? 0) + 1;
            }
        }
    }

    const report = {
        generatedAt: new Date().toISOString(),
        totalItems: items.length,
        totalFlagged: allIssues.length,
        summary: typeCounts,
        rewriteQueue,
        issues: allIssues,
    };

    if (outputJson) {
        const reportPath = path.join(
            REPORT_DIR,
            `audit-report-${new Date().toISOString().slice(0, 10)}.json`
        );
        fs.mkdirSync(REPORT_DIR, { recursive: true });
        fs.writeFileSync(reportPath, JSON.stringify(report, null, 2), 'utf-8');
        console.log(`Audit report saved: ${reportPath}`);
        return report;
    }

    // Human-readable output
    console.log('\n📋 Content Bank Audit Report');
    console.log('═'.repeat(60));
    console.log(`  Total entries:  ${items.length}`);
    console.log(`  Flagged:        ${allIssues.length}`);
    console.log(`  Clean:          ${items.length - allIssues.length}`);
    console.log('\n  Issue breakdown:');
    for (const [type, count] of Object.entries(typeCounts)) {
        console.log(`    ${type.padEnd(20)} ${count}`);
    }

    console.log('\n─'.repeat(60));
    console.log('  Flagged entries:\n');

    for (const entry of allIssues) {
        console.log(`  ID ${String(entry.id).padStart(3)} [${entry.type}]`);
        for (const issue of entry.issues) {
            const icon = issue.type === 'hype_tone' || issue.type === 'placeholder' ? '❌' : '⚠️ ';
            console.log(`    ${icon} ${issue.type}: ${issue.detail}`);
        }
        console.log('');
    }

    if (allIssues.length > 0) {
        console.log(`Run 'npm run audit:rewrite' to preview fixes without writing.`);
        console.log(`Run 'npm run audit:rewrite-live' to apply fixes to content-bank.json.\n`);
    } else {
        console.log('✅ Content bank is clean — no issues found.\n');
    }

    return report;
}

// ─── CLI ──────────────────────────────────────────────────────────────────────

const args = process.argv.slice(2);
const outputJson = args.includes('--json');
runAudit(outputJson);
