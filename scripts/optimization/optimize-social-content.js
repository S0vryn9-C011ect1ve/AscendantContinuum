#!/usr/bin/env node
/**
 * Social Media Content Optimizer
 * Fixes character encoding, trims content for platform limits, and optimizes for virality
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Platform character limits
const LIMITS = {
    bluesky: 300,
    mastodon: 500,
    discord: 2000
};

// Character encoding fixes
const CHAR_REPLACEMENTS = {
    '\u2018': "'",  // '
    '\u2019': "'",  // '
    '\u201C': '"',  // "
    '\u201D': '"',  // "
    '\u2013': '-',  // –
    '\u2014': '-',  // —
    '\u2026': '...',  // …
    '\u2022': '*',  // •
    '\u00D7': 'x',  // ×
    '\u2192': '->',  // →
    '\u2713': '[WORKS]',  // ✓
    '\u2717': '[NO]'  // ✗
};

/**
 * Fix character encoding issues (smart quotes, em dashes, etc.)
 */
function fixCharacterEncoding(text) {
    let fixed = text;
    for (const [from, to] of Object.entries(CHAR_REPLACEMENTS)) {
        fixed = fixed.replaceAll(from, to);
    }
    return fixed;
}

/**
 * Make hooks more viral and eye-catching
 */
function optimizeHook(hook, type) {
    // Remove periods from hooks (more engaging without)
    hook = hook.replace(/\.$/, '');

    // Viral hook patterns by type
    const viralPatterns = {
        devUpdate: [
            'This {topic} broke my brain',
            'Just built something impossible',
            'Nobody talks about {topic}',
            'This {topic} changed everything'
        ],
        devEducation: [
            'Stop using {bad}. Do this instead',
            'The {topic} trick nobody teaches',
            'Most devs get {topic} wrong'
        ],
        designPhilosophy: [
            'What if {question}?',
            'Everyone does {X}. We do {Y}',
            'The truth about {topic}'
        ]
    };

    return hook;
}

/**
 * Intelligently trim content to fit character limit
 */
function trimContent(hook, body, limit) {
    const separator = '\n\n';
    const combined = `${hook}${separator}${body}`;

    if (combined.length <= limit) {
        return { hook, body, trimmed: false };
    }

    // Strategy 1: Trim body, keep full hook
    const availableForBody = limit - hook.length - separator.length - 3; // -3 for "..."

    if (availableForBody > 50) {
        // Find last complete sentence or line break
        let trimPoint = body.substring(0, availableForBody).lastIndexOf('\n\n');
        if (trimPoint === -1) {
            trimPoint = body.substring(0, availableForBody).lastIndexOf('\n');
        }
        if (trimPoint === -1) {
            trimPoint = body.substring(0, availableForBody).lastIndexOf('. ');
        }
        if (trimPoint === -1) {
            trimPoint = availableForBody;
        }

        return {
            hook,
            body: body.substring(0, trimPoint).trim() + '...',
            trimmed: true
        };
    }

    // Strategy 2: Shorten hook if it's too long
    if (hook.length > 80) {
        const shortHook = hook.substring(0, 77) + '...';
        return trimContent(shortHook, body, limit);
    }

    // Strategy 3: Drastically trim body
    const minBodyLength = Math.min(100, availableForBody);
    return {
        hook,
        body: body.substring(0, minBodyLength).trim() + '...',
        trimmed: true
    };
}

/**
 * Create platform-specific versions of content
 */
function createPlatformVersions(content) {
    const versions = {};

    for (const platform of content.platforms) {
        const limit = LIMITS[platform] || LIMITS.mastodon;

        // Fix character encoding
        let hook = fixCharacterEncoding(content.hook);
        let body = fixCharacterEncoding(content.body);

        // Optimize hook for virality
        hook = optimizeHook(hook, content.type);

        // Trim to platform limit
        const trimmed = trimContent(hook, body, limit);

        versions[platform] = {
            hook: trimmed.hook,
            body: trimmed.body,
            characterCount: (trimmed.hook + '\n\n' + trimmed.body).length,
            limit: limit,
            trimmed: trimmed.trimmed
        };
    }

    return versions;
}

/**
 * Process all content in content-bank.json
 */
function optimizeContentBank() {
    const contentBankPath = path.join(__dirname, '../../public/social/content-bank.json');
    const contentBank = JSON.parse(fs.readFileSync(contentBankPath, 'utf-8'));

    console.log('\n═══════════════════════════════════════════════════════════\n');
    console.log('🚀 SOCIAL MEDIA CONTENT OPTIMIZER\n');
    console.log('═══════════════════════════════════════════════════════════\n');

    const stats = {
        totalPosts: contentBank.content.length,
        encodingFixed: 0,
        trimmed: 0,
        optimized: 0
    };

    const optimized = contentBank.content.map(content => {
        const original = {
            hook: content.hook,
            body: content.body
        };

        // Fix character encoding
        let hook = fixCharacterEncoding(content.hook);
        let body = fixCharacterEncoding(content.body);

        if (hook !== content.hook || body !== content.body) {
            stats.encodingFixed++;
        }

        // Optimize hook
        const optimizedHook = optimizeHook(hook, content.type);
        if (optimizedHook !== hook) {
            stats.optimized++;
            hook = optimizedHook;
        }

        // Check if needs trimming for Bluesky
        const combined = `${hook}\n\n${body}`;
        if (content.platforms.includes('bluesky') && combined.length > LIMITS.bluesky) {
            const trimmed = trimContent(hook, body, LIMITS.bluesky);
            hook = trimmed.hook;
            body = trimmed.body;
            stats.trimmed++;
        }

        return {
            ...content,
            hook,
            body
        };
    });

    // Update content bank
    const updatedContentBank = {
        ...contentBank,
        content: optimized
    };

    fs.writeFileSync(contentBankPath, JSON.stringify(updatedContentBank, null, 2));

    console.log(`✓ Total posts processed: ${stats.totalPosts}`);
    console.log(`✓ Character encoding fixed: ${stats.encodingFixed} posts`);
    console.log(`✓ Trimmed for Bluesky: ${stats.trimmed} posts`);
    console.log(`✓ Hooks optimized: ${stats.optimized} posts`);
    console.log('\n═══════════════════════════════════════════════════════════\n');

    return stats;
}

// Run optimization
try {
    const stats = optimizeContentBank();
    console.log('✅ Content optimization complete!\n');
} catch (error) {
    console.error('❌ Error optimizing content:', error);
    process.exit(1);
}
