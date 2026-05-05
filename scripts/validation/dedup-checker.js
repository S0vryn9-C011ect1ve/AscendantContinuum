/**
 * Deduplication Checker — Prevents repeat and near-repeat content from publishing
 *
 * Five detection layers:
 *   1. Exact hash match (SHA-256) — instant block
 *   2. Fuzzy similarity via Dice coefficient bigrams — block ≥90%, flag 75–89%
 *   3. Link-frequency check — flag if same URL used >3× in 30 days with <75% text variation
 *   4. Hook similarity — separate check on short hook text (60% threshold)
 *   5. Structural pattern — detects identical bullet/list structures on same topic
 *
 * Fingerprint store: public/social/content-fingerprints.json
 *
 * Usage (CLI):
 *   node scripts/validation/dedup-checker.js --audit           # scan full content bank
 *   node scripts/validation/dedup-checker.js --backfill        # seed fingerprints from history
 *   node scripts/validation/dedup-checker.js --check <text>    # check a single piece of text
 *
 * Usage (module):
 *   import { checkDuplicate, addFingerprint, backfillFingerprints } from './dedup-checker.js';
 *   const result = await checkDuplicate({ hook: '...', body: '...' });
 */

import fs from 'fs';
import path from 'path';
import crypto from 'crypto';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const PROJECT_ROOT = path.join(__dirname, '..', '..');

const FINGERPRINTS_PATH = path.join(PROJECT_ROOT, 'public', 'social', 'content-fingerprints.json');
const HISTORY_PATH = path.join(PROJECT_ROOT, 'public', 'social', 'posting-history.json');
const CONTENT_BANK_PATH = path.join(PROJECT_ROOT, 'public', 'social', 'content-bank.json');

// Thresholds
const BLOCK_THRESHOLD = 0.90;   // ≥90% similar → BLOCK
const FLAG_THRESHOLD = 0.75;    // 75–89% → FLAG
const HOOK_FLAG_THRESHOLD = 0.60; // hooks are short; flag at 60%
const LINK_REUSE_DAYS = 30;
const LINK_REUSE_MAX = 3;
const FINGERPRINT_WINDOW_DAYS = 90;

// ─── Text normalization ────────────────────────────────────────────────────────

function normalizeForHash(text) {
    return text
        .toLowerCase()
        .replace(/https?:\/\/\S+/g, ' URL ')  // replace URLs so link changes don't affect similarity
        .replace(/[^\w\s]/g, ' ')              // strip punctuation
        .replace(/\s+/g, ' ')
        .trim();
}

// ─── SHA-256 hash ──────────────────────────────────────────────────────────────

function hashText(text) {
    return crypto.createHash('sha256').update(normalizeForHash(text)).digest('hex');
}

// ─── Dice coefficient (bigram similarity) ─────────────────────────────────────

function getBigrams(text) {
    const bigrams = new Set();
    const t = normalizeForHash(text);
    for (let i = 0; i < t.length - 1; i++) {
        bigrams.add(t.slice(i, i + 2));
    }
    return bigrams;
}

function diceCoefficient(a, b) {
    if (!a || !b) return 0;
    if (a === b) return 1;
    const bigramsA = getBigrams(a);
    const bigramsB = getBigrams(b);
    if (bigramsA.size === 0 || bigramsB.size === 0) return 0;
    let intersection = 0;
    for (const bg of bigramsA) {
        if (bigramsB.has(bg)) intersection++;
    }
    return (2 * intersection) / (bigramsA.size + bigramsB.size);
}

// ─── Structural pattern detection ─────────────────────────────────────────────

function extractStructure(text) {
    // Represents the text as a sequence of line-types: bullet, numbered, blank, text
    return text
        .split('\n')
        .map((line) => {
            const t = line.trim();
            if (!t) return 'blank';
            if (/^[-•*]\s/.test(t)) return 'bullet';
            if (/^\d+\.\s/.test(t)) return 'numbered';
            return 'text';
        })
        .join(',');
}

// ─── Fingerprint store I/O ────────────────────────────────────────────────────

function loadFingerprints() {
    if (!fs.existsSync(FINGERPRINTS_PATH)) {
        return { fingerprints: [], lastUpdated: null };
    }
    try {
        return JSON.parse(fs.readFileSync(FINGERPRINTS_PATH, 'utf-8'));
    } catch {
        return { fingerprints: [], lastUpdated: null };
    }
}

function saveFingerprints(store) {
    store.lastUpdated = new Date().toISOString();
    fs.mkdirSync(path.dirname(FINGERPRINTS_PATH), { recursive: true });
    fs.writeFileSync(FINGERPRINTS_PATH, JSON.stringify(store, null, 2), 'utf-8');
}

// ─── Recent fingerprints only ─────────────────────────────────────────────────

function recentFingerprints(fingerprints) {
    const cutoff = Date.now() - FINGERPRINT_WINDOW_DAYS * 24 * 60 * 60 * 1000;
    return fingerprints.filter((fp) => fp.postedAt && new Date(fp.postedAt).getTime() > cutoff);
}

// ─── Core check ───────────────────────────────────────────────────────────────

/**
 * Check whether a piece of content is a duplicate or near-duplicate.
 *
 * @param {{ hook?: string, body: string, platform?: string }} content
 * @returns {{ allowed: boolean, action: 'block'|'flag'|'allow', reasons: string[], similarTo: string[] }}
 */
export function checkDuplicate(content) {
    const { hook = '', body, platform = 'all' } = content;
    const fullText = [hook, body].filter(Boolean).join('\n');

    const store = loadFingerprints();
    const recent = recentFingerprints(store.fingerprints);

    const reasons = [];
    const similarTo = [];
    let worstAction = 'allow';

    const fullHash = hashText(fullText);

    for (const fp of recent) {
        // 1. Exact hash
        if (fp.hash === fullHash) {
            reasons.push(`Exact duplicate of fingerprint ${fp.contentId ?? fp.id} (posted ${fp.postedAt?.slice(0, 10)})`);
            similarTo.push(fp.contentId ?? fp.id);
            worstAction = 'block';
            continue;
        }

        // 2. Fuzzy body similarity
        const bodySimilarity = diceCoefficient(body, fp.body ?? '');
        if (bodySimilarity >= BLOCK_THRESHOLD) {
            reasons.push(
                `Body is ${Math.round(bodySimilarity * 100)}% similar to fingerprint ${fp.contentId ?? fp.id} — BLOCK`
            );
            similarTo.push(fp.contentId ?? fp.id);
            worstAction = 'block';
        } else if (bodySimilarity >= FLAG_THRESHOLD) {
            reasons.push(
                `Body is ${Math.round(bodySimilarity * 100)}% similar to fingerprint ${fp.contentId ?? fp.id} — FLAG`
            );
            similarTo.push(fp.contentId ?? fp.id);
            if (worstAction === 'allow') worstAction = 'flag';
        }

        // 4. Hook similarity (separate, lower threshold)
        if (hook && fp.hook) {
            const hookSimilarity = diceCoefficient(hook, fp.hook);
            if (hookSimilarity >= HOOK_FLAG_THRESHOLD && bodySimilarity < FLAG_THRESHOLD) {
                reasons.push(
                    `Hook is ${Math.round(hookSimilarity * 100)}% similar to fingerprint ${fp.contentId ?? fp.id} hook`
                );
                if (worstAction === 'allow') worstAction = 'flag';
            }
        }

        // 5. Structural pattern
        const structA = extractStructure(body);
        const structB = extractStructure(fp.body ?? '');
        if (structA === structB && structA.length > 10 && bodySimilarity >= 0.50) {
            reasons.push(
                `Identical structure pattern as fingerprint ${fp.contentId ?? fp.id} — consider varying format`
            );
            if (worstAction === 'allow') worstAction = 'flag';
        }
    }

    // 3. Link frequency check
    const urlPattern = /https?:\/\/[^\s"'<>)\]]+/g;
    const links = fullText.match(urlPattern) ?? [];
    for (const link of links) {
        const recentWithSameLink = recent.filter((fp) => {
            if (!fp.links?.includes(link)) return false;
            const daysSince = (Date.now() - new Date(fp.postedAt).getTime()) / (1000 * 60 * 60 * 24);
            return daysSince <= LINK_REUSE_DAYS;
        });
        if (recentWithSameLink.length >= LINK_REUSE_MAX) {
            const textVariation = recentWithSameLink.every(
                (fp) => diceCoefficient(body, fp.body ?? '') >= FLAG_THRESHOLD
            );
            if (textVariation) {
                reasons.push(
                    `Link ${link} used ${recentWithSameLink.length}× in ${LINK_REUSE_DAYS} days with similar text — possible link spam`
                );
                if (worstAction === 'allow') worstAction = 'flag';
            }
        }
    }

    return {
        allowed: worstAction !== 'block',
        action: worstAction,
        reasons,
        similarTo,
    };
}

/**
 * Add a fingerprint to the store after successful post.
 * @param {{ contentId?: string|number, hook?: string, body: string, platform: string, links?: string[] }} entry
 */
export function addFingerprint(entry) {
    const store = loadFingerprints();
    const fullText = [entry.hook ?? '', entry.body].filter(Boolean).join('\n');

    store.fingerprints.push({
        contentId: entry.contentId ?? null,
        hash: hashText(fullText),
        hook: entry.hook ?? null,
        body: entry.body,
        platform: entry.platform,
        links: entry.links ?? [],
        postedAt: new Date().toISOString(),
    });

    saveFingerprints(store);
}

// ─── Backfill from posting history ────────────────────────────────────────────

/**
 * Seed the fingerprint store from existing posting history + content bank.
 * Safe to run multiple times — skips entries that already have a matching hash.
 */
export function backfillFingerprints() {
    if (!fs.existsSync(HISTORY_PATH)) {
        console.warn('No posting-history.json found — nothing to backfill.');
        return;
    }

    const store = loadFingerprints();
    const existingHashes = new Set(store.fingerprints.map((fp) => fp.hash));

    let history;
    try {
        history = JSON.parse(fs.readFileSync(HISTORY_PATH, 'utf-8'));
    } catch {
        console.error('Could not parse posting-history.json');
        return;
    }

    // Load content bank to get hook/body for each contentId
    let bankMap = {};
    if (fs.existsSync(CONTENT_BANK_PATH)) {
        try {
            const bank = JSON.parse(fs.readFileSync(CONTENT_BANK_PATH, 'utf-8'));
            for (const item of bank.content ?? []) {
                bankMap[item.id] = item;
            }
        } catch { /* non-fatal */ }
    }

    let added = 0;
    for (const post of history.posts ?? []) {
        const bankEntry = bankMap[post.contentId];
        const hook = bankEntry?.hook ?? post.hook ?? '';
        const body = bankEntry?.body ?? post.body ?? '';
        if (!body) continue;

        const fullText = [hook, body].filter(Boolean).join('\n');
        const hash = hashText(fullText);

        if (existingHashes.has(hash)) continue;

        store.fingerprints.push({
            contentId: post.contentId ?? null,
            hash,
            hook: hook || null,
            body,
            platform: post.platforms?.join(',') ?? 'unknown',
            links: [],
            postedAt: post.timestamp ?? new Date().toISOString(),
        });
        existingHashes.add(hash);
        added++;
    }

    saveFingerprints(store);
    console.log(`Backfill complete — added ${added} fingerprint(s). Total: ${store.fingerprints.length}`);
}

// ─── Audit mode: scan content bank for internal duplicates ────────────────────

function auditContentBank() {
    if (!fs.existsSync(CONTENT_BANK_PATH)) {
        console.error('content-bank.json not found');
        process.exit(1);
    }

    const bank = JSON.parse(fs.readFileSync(CONTENT_BANK_PATH, 'utf-8'));
    const items = bank.content ?? [];
    const issues = [];

    console.log(`\n🔍 Dedup Audit — scanning ${items.length} content bank entries\n`);
    console.log('─'.repeat(60));

    for (let i = 0; i < items.length; i++) {
        for (let j = i + 1; j < items.length; j++) {
            const a = items[i];
            const b = items[j];

            const bodySim = diceCoefficient(a.body, b.body);
            const hookSim = diceCoefficient(a.hook, b.hook);

            if (bodySim >= BLOCK_THRESHOLD) {
                issues.push({
                    severity: 'BLOCK',
                    ids: [a.id, b.id],
                    bodySimilarity: Math.round(bodySim * 100),
                    hookSimilarity: Math.round(hookSim * 100),
                    reason: 'Near-identical body text',
                });
            } else if (bodySim >= FLAG_THRESHOLD) {
                issues.push({
                    severity: 'FLAG',
                    ids: [a.id, b.id],
                    bodySimilarity: Math.round(bodySim * 100),
                    hookSimilarity: Math.round(hookSim * 100),
                    reason: 'High body similarity',
                });
            } else if (hookSim >= HOOK_FLAG_THRESHOLD) {
                issues.push({
                    severity: 'FLAG',
                    ids: [a.id, b.id],
                    bodySimilarity: Math.round(bodySim * 100),
                    hookSimilarity: Math.round(hookSim * 100),
                    reason: 'Similar hooks',
                });
            }
        }
    }

    if (issues.length === 0) {
        console.log('✅ No duplicate or near-duplicate issues found.\n');
        return;
    }

    const blocks = issues.filter((i) => i.severity === 'BLOCK');
    const flags = issues.filter((i) => i.severity === 'FLAG');

    console.log(`❌ BLOCK (≥${BLOCK_THRESHOLD * 100}% similarity): ${blocks.length} pair(s)`);
    console.log(`⚠️  FLAG  (≥${FLAG_THRESHOLD * 100}% similarity): ${flags.length} pair(s)\n`);

    for (const issue of issues) {
        const icon = issue.severity === 'BLOCK' ? '❌' : '⚠️ ';
        console.log(
            `${icon} IDs [${issue.ids.join(', ')}] — body: ${issue.bodySimilarity}%, hook: ${issue.hookSimilarity}% — ${issue.reason}`
        );
    }

    console.log(`\nTotal issues: ${issues.length}`);
    console.log('Run `npm run audit:rewrite` to review proposed fixes.\n');

    // Exit with non-zero so CI can catch it if needed
    if (blocks.length > 0) process.exit(1);
}

// ─── CLI entry point ───────────────────────────────────────────────────────────

if (process.argv[1] && process.argv[1].endsWith('dedup-checker.js')) {
    const args = process.argv.slice(2);

    if (args.includes('--audit')) {
        auditContentBank();
    } else if (args.includes('--backfill')) {
        backfillFingerprints();
    } else if (args.includes('--check')) {
        const text = args.slice(args.indexOf('--check') + 1).join(' ');
        if (!text) {
            console.error('Usage: node dedup-checker.js --check <text>');
            process.exit(1);
        }
        const result = checkDuplicate({ body: text });
        console.log(JSON.stringify(result, null, 2));
        if (!result.allowed) process.exit(1);
    } else {
        console.log('Usage:');
        console.log('  node scripts/validation/dedup-checker.js --audit       Scan content bank');
        console.log('  node scripts/validation/dedup-checker.js --backfill    Seed fingerprints from history');
        console.log('  node scripts/validation/dedup-checker.js --check <text>  Check a string');
        process.exit(0);
    }
}
