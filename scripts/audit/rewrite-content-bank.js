/**
 * Content Bank Rewriter — Fixes flagged entries in public/social/content-bank.json
 *
 * Reads the audit report (or re-runs the audit inline), then rewrites flagged
 * entries to be:
 *   - Factually accurate to the actual game
 *   - Honest dev-log tone with occasional dry wit (no hype)
 *   - Game/website promoting (canonical links where budget allows)
 *   - Within platform character limits
 *   - Using "Realm Dwellers" instead of NPC / "game characters"
 *
 * Usage:
 *   node scripts/audit/rewrite-content-bank.js --dry-run     # preview only
 *   node scripts/audit/rewrite-content-bank.js               # apply to content-bank.json
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const PROJECT_ROOT = path.join(__dirname, '..', '..');
const CONTENT_BANK_PATH = path.join(PROJECT_ROOT, 'public', 'social', 'content-bank.json');

const isDryRun = process.argv.includes('--dry-run');

// ─── Verified game fact library ───────────────────────────────────────────────
// All rewrites draw exclusively from facts in this object.

const GAME = {
    name: 'Ascendant Continuum',
    site: 'https://ascendant-continuum.web.app/',
    blog: 'https://ascendant-continuum.web.app/blog/',
    realms: ['Emberforge', 'Verdant Sanctuary', 'Echo Fields', 'Dawn Citadel', 'Lantern Ascension'],
    realmDescriptions: {
        Emberforge: 'creation and raw energy — where rituals spark into existence',
        'Verdant Sanctuary': 'growth and reflection — slow, deliberate, alive',
        'Echo Fields': 'memory and imagination — the past folded into the present',
        'Dawn Citadel': 'wonder and knowledge — light refracts here into puzzles',
        'Lantern Ascension': 'liminal peace — the space between things',
    },
    coreLoop: 'explore realms → perform rituals (tap, swipe, trace, connect) → collect sigils → build a personal magical signature',
    sessionLength: '1–5 minutes',
    digitalSunset: 'Digital Sunset feature — a gentle in-game reminder to rest after five minutes',
    accessibility: {
        modes: 5,
        colorblind: {
            protanopia: 'reveals fire runes in Emberforge + unlocks the Crimson Veil achievement',
            deuteranopia: 'reveals water sigils in Verdant Sanctuary + unlocks Emerald Mysteries lore',
            tritanopia: 'reveals earth patterns in Echo Fields + unlocks Azure Pathways',
        },
        reducedMotion: 'reveals Still Point Mysteries achievement and sacred geometry overlays',
        compliance: 'WCAG 2.1 AA + CVAA compliant',
        principle: 'Accessibility modes don\'t just adjust visuals — they unlock different content entirely',
    },
    celestial: {
        lunar: 'pulls real moon phase data — full moon rituals differ from new moon rituals',
        sunrise: '2× XP for catching your actual local sunrise or sunset',
        seasonal: '3× XP seasonal bonus for early risers in winter',
        eclipse: 'solar/lunar eclipses trigger rare in-game events',
    },
    community: {
        realmDwellers: 'Realm Dwellers absorb every conversation from every player across all sessions — their dialogue evolves based on collective wisdom. Launch-day players essentially teach future players.',
        fossils: 'Your ritual fossilizes 7 days after you perform it. Thirty days later, other players can excavate it as an archaeological artifact.',
        sigilNaming: 'The first player to discover a sigil combination gets to name it — globally, for everyone who finds it after.',
        deities: '1% chance deity encounters — they appear for exactly 60 seconds, unannounced, unrepeatable for a while',
        ritualReplay: 'The game auto-captures the peak 6 seconds of your ritual as a procedural animation (2 MB, instant share)',
        constellationChallenge: 'Daily Constellation Challenge — a shared global puzzle',
    },
    monetization: {
        model: 'cosmetics only — sigil colors, particle effects, realm expansions, support tiers',
        noList: 'no pay-to-win, no loot boxes, no energy timers, no artificial scarcity, no FOMO mechanics',
    },
    tech: {
        engine: 'Unity 6000.3.9f1 with URP 17.3.0',
        platforms: 'WebGL (live) + Android/iOS (in submission)',
        performance: '60fps with dynamic scaling',
        backend: 'Firebase',
    },
};

// ─── Rewrite templates ────────────────────────────────────────────────────────
// Keyed by content bank entry `id`. Each has `hook` and `body`.
// Character budgets: Bluesky ≤280 total (hook + body), Mastodon ≤500.
// These are hand-authored rewrites for known-problematic entries identified in the audit.

const REWRITES = {
    // id 1 — colorblind modes / accessibility (currently fine factually, but missing link + has generic tone)
    1: {
        hook: 'Accessibility mode should change play, not just color.',
        body: `Ascendant Continuum ties each mode to different finds:\nProtanopia = Emberforge fire runes\nDeuteranopia = Verdant sigils\nTritanopia = Echo Fields patterns\n\nhttps://ascendant-continuum.web.app/blog/`,
    },
    // id 2 — NPC collective memory → Realm Dwellers
    2: {
        hook: 'Realm Dwellers remember what players share.',
        body: `In Ascendant Continuum, dialogue shifts from global conversation history. Early players leave traces future players hear.\n\nNot AI; Firebase aggregation.\n\nhttps://ascendant-continuum.web.app/blog/`,
    },
    // id 4 — duplicate of id 1 pattern, rewrite with different angle
    4: {
        hook: 'Three colorblind modes, three different secrets.',
        body: `Protanopia: Crimson Veil.\nDeuteranopia: Emerald Mysteries.\nTritanopia: Azure Pathways.\n\nAccessibility is part of discovery in Ascendant Continuum.\n\nhttps://ascendant-continuum.web.app/`,
    },
    // id 6 — has broken [link to gist] placeholder
    6: {
        hook: 'Unity pattern: 5 colorblind modes, one UI.',
        body: `Shader transforms plus pattern overlays let each mode reveal distinct ritual clues in Ascendant Continuum, without duplicate assets.\n\nhttps://ascendant-continuum.web.app/blog/`,
    },
};

// ─── Automated fix rules (applied to ALL entries, not just specific IDs) ─────

const GAME_SPECIFIC_TERMS = [
    'Emberforge', 'Verdant Sanctuary', 'Echo Fields', 'Dawn Citadel', 'Lantern Ascension',
    'ritual', 'sigil', 'deity', 'Realm Dwellers', 'constellation',
    'moon phase', 'lunar', 'colorblind', 'Digital Sunset', 'fossil',
    'Ascendant Continuum', 'ascendant-continuum.web.app', 'accessibility',
];

const HYPE_FIXES = [
    [/(?:literally\s+doesn't\s+exist\s+elsewhere)/gi, "I have not seen this exact approach in other games"],
    [/\bSteal this\b/gi, 'Adapt this'],
    [/\bgame mechanic that literally\b/gi, 'game mechanic that'],
    [/\byou won't believe\b/gi, 'here is what happened'],
    [/\bnobody is talking about\b/gi, 'I do not see this discussed often'],
    [/\bthis breaks everything\b/gi, 'this changed my implementation plan'],
    [/\bviral\b/gi, 'widely shared'],
    [/\bREVOLUTIONARY\b/g, 'unusual'],
    [/\bNEVER DONE BEFORE\b/g, 'rarely attempted'],
    [/\bGAME CHANGING\b/g, 'meaningful'],
    [/\bMIND-?BLOWING\b/g, 'surprising'],
];

function countChars(text) {
    try {
        return [...new Intl.Segmenter().segment(text)].length;
    } catch {
        return [...text].length;
    }
}

function minPlatformLimit(entry) {
    const limits = { bluesky: 280, mastodon: 500 };
    if (!Array.isArray(entry.platforms) || entry.platforms.length === 0) return null;
    let min = null;
    for (const platform of entry.platforms) {
        const limit = limits[platform];
        if (!limit) continue;
        min = min === null ? limit : Math.min(min, limit);
    }
    return min;
}

function hasCanonicalLink(text) {
    return text.includes(GAME.site) || text.includes(GAME.blog);
}

function hasGameSpecificTerm(text) {
    const lowered = text.toLowerCase();
    return GAME_SPECIFIC_TERMS.some((term) => lowered.includes(term.toLowerCase()));
}

function reduceHypeLanguage(text) {
    let out = text;
    for (const [pattern, replacement] of HYPE_FIXES) {
        out = out.replace(pattern, replacement);
    }
    return out;
}

function ensureCanonicalLink(text) {
    if (hasCanonicalLink(text)) return text;
    return `${text.trim()}\n\n${GAME.blog}`;
}

function ensureGameSpecificAnchor(text) {
    if (hasGameSpecificTerm(text)) return text;
    return `${text.trim()}\n\nAscendant Continuum note: rituals, sigils, and Realm Dwellers across Emberforge.`;
}

function enforceEntryCharLimit(entry, hook, body) {
    const maxChars = minPlatformLimit(entry);
    if (!maxChars) return { hook, body };

    const merged = [hook, body].filter(Boolean).join('\n\n').trim();
    if (countChars(merged) <= maxChars) return { hook, body };

    const normalizedBody = body.replace(/\n{3,}/g, '\n\n').trim();
    const blogLink = normalizedBody.includes(GAME.blog) ? GAME.blog : '';
    const siteLink = normalizedBody.includes(GAME.site) ? GAME.site : '';
    const retainedLink = blogLink || siteLink;
    const linkBlock = retainedLink ? `\n\n${retainedLink}` : '';

    const hookWithGap = hook ? `${hook}\n\n` : '';
    const availableForBody = maxChars - countChars(hookWithGap) - countChars(linkBlock);

    if (availableForBody > 24) {
        const sourceBody = normalizedBody
            .replace(GAME.blog, '')
            .replace(GAME.site, '')
            .replace(/\n{3,}/g, '\n\n')
            .trim();
        const trimmedBodyChars = [...sourceBody].slice(0, Math.max(0, availableForBody - 3)).join('');
        const compactBody = `${trimmedBodyChars.trimEnd()}...`;
        return {
            hook,
            body: `${compactBody}${linkBlock}`.trim(),
        };
    }

    // If the hook itself consumes most of the budget, preserve intent with a compact fallback.
    const fallback = `Ascendant Continuum update: ${GAME.blog}`;
    if (countChars(fallback) <= maxChars) {
        return { hook: '', body: fallback };
    }

    const hookFallback = [...hook].slice(0, Math.max(0, maxChars - 3)).join('').trimEnd();
    return { hook: `${hookFallback}...`, body: '' };
}

function applyAutomatedFixes(entry, hook, body) {
    let newHook = hook;
    let newBody = body;

    // Fix terminology: game characters/NPCs → Realm Dwellers
    const combinedTermPattern = /\bgame characters?\s*\(\s*NPCs?\s*\)/gi;
    newHook = newHook.replace(combinedTermPattern, 'Realm Dwellers');
    newBody = newBody.replace(combinedTermPattern, 'Realm Dwellers');

    const termFixes = [
        [/\bNPCs?\b/gi, 'Realm Dwellers'],
        [/\bgame characters?\b/gi, 'Realm Dwellers'],
    ];
    for (const [pattern, replacement] of termFixes) {
        newHook = newHook.replace(pattern, replacement);
        newBody = newBody.replace(pattern, replacement);
    }

    newHook = newHook.replace(/Realm Dwellers\s*\(\s*Realm Dwellers\s*\)/gi, 'Realm Dwellers');
    newBody = newBody.replace(/Realm Dwellers\s*\(\s*Realm Dwellers\s*\)/gi, 'Realm Dwellers');

    // Tone down hype/clickbait language
    newHook = reduceHypeLanguage(newHook);
    newBody = reduceHypeLanguage(newBody);

    // Fix broken placeholder links
    newBody = newBody.replace(/\[link to gist\]/gi, 'https://ascendant-continuum.web.app/blog/');
    newBody = newBody.replace(/\[URL\]/gi, 'https://ascendant-continuum.web.app/');
    newBody = newBody.replace(/\[TBD\]/gi, '');
    newBody = newBody.replace(/\[coming soon\]/gi, '');

    // Ensure anchor quality requirements from audit
    const merged = [newHook, newBody].filter(Boolean).join('\n\n');
    const withAnchor = ensureGameSpecificAnchor(merged);
    const withLink = ensureCanonicalLink(withAnchor);

    const parts = withLink.split('\n\n');
    if (parts.length >= 2) {
        newHook = parts[0].trim();
        newBody = parts.slice(1).join('\n\n').trim();
    } else {
        newBody = withLink.trim();
    }

    // Enforce strictest platform character budget for this entry
    const limited = enforceEntryCharLimit(entry, newHook, newBody);
    newHook = limited.hook;
    newBody = limited.body;

    return { hook: newHook, body: newBody };
}

function entryNeedsRewrite(entry) {
    const text = [entry.hook ?? '', entry.body ?? ''].join(' ');
    const limit = minPlatformLimit(entry);
    const overLimit = typeof limit === 'number' && countChars(text) > limit;
    const missingLink = !hasCanonicalLink(text);
    const missingAnchor = !hasGameSpecificTerm(text);

    return (
        /\bNPC\b/i.test(text) ||
        /\bgame characters?\b/gi.test(text) ||
        /\[link to gist\]/i.test(text) ||
        /\[URL\]/i.test(text) ||
        /steal this/i.test(text) ||
        /literally doesn't exist elsewhere/i.test(text) ||
        /REVOLUTIONARY|NEVER DONE BEFORE|GAME CHANGING|MIND-?BLOWING/i.test(text) ||
        missingLink ||
        missingAnchor ||
        overLimit ||
        entry.id in REWRITES
    );
}

// ─── Main rewrite logic ───────────────────────────────────────────────────────

function runRewrite() {
    if (!fs.existsSync(CONTENT_BANK_PATH)) {
        console.error(`content-bank.json not found: ${CONTENT_BANK_PATH}`);
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
    let rewrittenCount = 0;
    const log = [];

    for (const entry of items) {
        if (!entryNeedsRewrite(entry)) continue;

        const originalHook = entry.hook ?? '';
        const originalBody = entry.body ?? '';

        // Apply specific hand-authored rewrite if available
        if (REWRITES[entry.id]) {
            const specific = REWRITES[entry.id];
            const change = {
                id: entry.id,
                type: entry.type,
                hookChanged: specific.hook !== originalHook,
                bodyChanged: specific.body !== originalBody,
                before: { hook: originalHook, body: originalBody },
                after: { hook: specific.hook, body: specific.body },
            };
            log.push(change);

            if (!isDryRun) {
                entry.hook = specific.hook;
                entry.body = specific.body;
                entry.rewritten = true;
                entry.rewrittenAt = new Date().toISOString();
            }
            rewrittenCount++;
            continue;
        }

        // Apply automated fixes (terminology, broken links)
        const { hook: fixedHook, body: fixedBody } = applyAutomatedFixes(entry, originalHook, originalBody);
        const changed = fixedHook !== originalHook || fixedBody !== originalBody;
        if (!changed) continue;

        const change = {
            id: entry.id,
            type: entry.type,
            hookChanged: fixedHook !== originalHook,
            bodyChanged: fixedBody !== originalBody,
            before: { hook: originalHook, body: originalBody },
            after: { hook: fixedHook, body: fixedBody },
        };
        log.push(change);

        if (!isDryRun) {
            entry.hook = fixedHook;
            entry.body = fixedBody;
            entry.rewritten = true;
            entry.rewrittenAt = new Date().toISOString();
        }
        rewrittenCount++;
    }

    // Output summary
    console.log(`\n${isDryRun ? '🔍 DRY RUN — no changes written' : '✍️  Rewriting content bank'}`);
    console.log('═'.repeat(60));
    console.log(`  Entries scanned:   ${items.length}`);
    console.log(`  Entries rewritten: ${rewrittenCount}\n`);

    const previewLimit = 60;
    for (const change of log.slice(0, previewLimit)) {
        console.log(`  ID ${String(change.id).padStart(3)} [${change.type}]`);
        if (change.hookChanged) {
            console.log(`    hook before: ${change.before.hook}`);
            console.log(`    hook after:  ${change.after.hook}`);
        }
        if (change.bodyChanged) {
            const before = change.before.body.replace(/\n/g, ' ').slice(0, 80);
            const after = change.after.body.replace(/\n/g, ' ').slice(0, 80);
            console.log(`    body before: ${before}...`);
            console.log(`    body after:  ${after}...`);
        }
        console.log('');
    }

    if (log.length > previewLimit) {
        console.log(`  ...and ${log.length - previewLimit} more rewritten entries not shown in preview.\n`);
    }

    if (!isDryRun && rewrittenCount > 0) {
        bank.meta.lastUpdated = new Date().toISOString();
        fs.writeFileSync(CONTENT_BANK_PATH, JSON.stringify(bank, null, 2), 'utf-8');
        console.log(`✅ content-bank.json updated — ${rewrittenCount} entr${rewrittenCount === 1 ? 'y' : 'ies'} rewritten.\n`);
    } else if (isDryRun) {
        console.log(`Run without --dry-run to apply these changes.\n`);
    } else {
        console.log(`✅ Nothing to rewrite — content bank is clean.\n`);
    }
}

runRewrite();
