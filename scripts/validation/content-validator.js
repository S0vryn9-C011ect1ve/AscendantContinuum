/**
 * Content Validator — Pre-publish gate for character limits, encoding, and formatting
 *
 * Validates social post content against platform rules before anything
 * is committed or posted. Also calls the link-checker internally.
 *
 * Usage (CLI):
 *   node scripts/validation/content-validator.js scripts/generated/content-2026-05-04.json
 *
 * Usage (module):
 *   import { validateContent } from './content-validator.js';
 *   const result = await validateContent({ bluesky: '...', mastodon: '...', discord: '...' });
 */

import fs from 'fs';
import { checkLinks, extractUrls } from './link-checker.js';

// ─── Platform limits ──────────────────────────────────────────────────────────

const LIMITS = {
    bluesky: 280,
    mastodon: 500,
};

// ─── Smart-quote / problem character maps ─────────────────────────────────────

const NORMALIZATION_MAP = [
    [/[\u2018\u2019]/g, "'"],   // curly single quotes → straight
    [/[\u201C\u201D]/g, '"'],   // curly double quotes → straight
    [/\u2013/g, '-'],            // en-dash → hyphen
    [/\u2014/g, '--'],           // em-dash → double hyphen
    [/\u2026/g, '...'],          // ellipsis character → three dots
    [/\uFEFF/g, ''],             // BOM
    [/[\u0000-\u0008\u000B\u000C\u000E-\u001F]/g, ''], // control chars (keep \t \n \r)
];

// Placeholder patterns that mean content was never finished
const PLACEHOLDER_PATTERNS = [
    /\[link (?:to|here|goes here)\]/i,
    /\[TBD\]/i,
    /\[coming soon\]/i,
    /\[URL\]/i,
    /\[insert/i,
    /TODO:/i,
];

/**
 * Normalize a string: remove BOM, normalize quotes, strip control chars.
 * @param {string} text
 * @returns {string}
 */
export function normalizeText(text) {
    let out = text;
    for (const [pattern, replacement] of NORMALIZATION_MAP) {
        out = out.replace(pattern, replacement);
    }
    return out;
}

/**
 * Count characters as Bluesky/Mastodon do — grapheme clusters, not code units.
 * Falls back to simple string length if Intl.Segmenter is unavailable.
 * @param {string} text
 * @returns {number}
 */
function countChars(text) {
    if (typeof Intl?.Segmenter === 'function') {
        const seg = new Intl.Segmenter();
        return [...seg.segment(text)].length;
    }
    return [...text].length; // Handles emoji/surrogates better than .length
}

/**
 * Validate content for a single platform.
 * @param {string} platform
 * @param {string} text
 * @returns {{ errors: string[], warnings: string[] }}
 */
function validatePlatform(platform, text) {
    const errors = [];
    const warnings = [];

    if (!text || typeof text !== 'string') {
        errors.push(`[${platform}] Content is missing or not a string`);
        return { errors, warnings };
    }

    const normalized = normalizeText(text);
    const charCount = countChars(normalized);
    const limit = LIMITS[platform];

    // Character limit check
    if (limit && charCount > limit) {
        errors.push(
            `[${platform}] Exceeds character limit: ${charCount}/${limit} chars`
        );
    } else if (limit && charCount > limit * 0.95) {
        warnings.push(
            `[${platform}] Near character limit: ${charCount}/${limit} chars`
        );
    }

    // Placeholder text check
    for (const pattern of PLACEHOLDER_PATTERNS) {
        if (pattern.test(text)) {
            errors.push(`[${platform}] Contains placeholder text: matches ${pattern}`);
        }
    }

    // Orphan http fragment (malformed URL)
    if (/(?<!\S)https?:(?!\/)/.test(text)) {
        warnings.push(`[${platform}] Possibly malformed URL (http: or https: not followed by //)`);
    }

    return { errors, warnings };
}

/**
 * Validate a content object across all platforms, including link checking.
 *
 * @param {{ bluesky?: string, mastodon?: string, discord?: string }} content
 * @param {{ skipLinkCheck?: boolean }} [options]
 * @returns {Promise<{ valid: boolean, errors: string[], warnings: string[] }>}
 */
export async function validateContent(content, options = {}) {
    const errors = [];
    const warnings = [];

    // Per-platform text validation
    for (const platform of ['bluesky', 'mastodon', 'discord']) {
        if (content[platform] == null) {
            warnings.push(`[${platform}] No content provided — skipping platform`);
            continue;
        }
        const result = validatePlatform(platform, content[platform]);
        errors.push(...result.errors);
        warnings.push(...result.warnings);
    }

    // Link validation across all platforms
    if (!options.skipLinkCheck) {
        const allText = Object.values(content).filter(Boolean).join('\n');
        const urls = extractUrls(allText);

        if (urls.length === 0) {
            warnings.push('[links] No URLs found in content — consider adding the blog link');
        } else {
            const linkResults = await checkLinks(urls);
            for (const result of linkResults) {
                if (!result.ok) {
                    errors.push(`[links] Broken URL: ${result.url} — ${result.error}`);
                }
            }
        }
    }

    return {
        valid: errors.length === 0,
        errors,
        warnings,
    };
}

// ─── CLI entry point ───────────────────────────────────────────────────────────

if (process.argv[1] && process.argv[1].endsWith('content-validator.js')) {
    const filePath = process.argv[2];

    if (!filePath) {
        console.error('Usage: node scripts/validation/content-validator.js <content-json-file>');
        console.error('  The JSON file must contain { bluesky, mastodon, discord } keys.');
        process.exit(1);
    }

    if (!fs.existsSync(filePath)) {
        console.error(`File not found: ${filePath}`);
        process.exit(1);
    }

    let content;
    try {
        content = JSON.parse(fs.readFileSync(filePath, 'utf-8'));
    } catch (err) {
        console.error(`Failed to parse JSON: ${err.message}`);
        process.exit(1);
    }

    // Support both flat { bluesky, mastodon, discord } and nested { social: { bluesky, ... } }
    const socialContent = content.social ?? content;

    console.log('\n🔍 Content Validator\n' + '─'.repeat(50));

    const result = await validateContent(socialContent);

    if (result.warnings.length > 0) {
        console.log('\n⚠️  Warnings:');
        result.warnings.forEach((w) => console.log(`   ${w}`));
    }

    if (result.errors.length > 0) {
        console.log('\n❌ Errors (pipeline blocked):');
        result.errors.forEach((e) => console.error(`   ${e}`));
        console.log('');
        process.exit(1);
    }

    console.log('\n✅ All checks passed — content is valid.\n');
    process.exit(0);
}
