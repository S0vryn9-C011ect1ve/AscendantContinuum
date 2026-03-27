#!/usr/bin/env node
/**
 * Content Voice Updater
 * Updates social media posts to use authentic solo dev learning voice
 * Based on: docs/CONTENT_VOICE_GUIDE.md
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Voice transformation rules
const VOICE_TRANSFORMATIONS = [
    {
        pattern: /^Most games (.*)\. We (made it|do|made|built) (.*)$/,
        replacement: (match, p1, p2, p3) => `Learning how ${p1} - found you can ${p3}`,
        description: 'Most games X. We Y → Learning X - found Y'
    },
    {
        pattern: /^Steal this Unity (system|pattern|approach|trick)/i,
        replacement: 'This Unity pattern worked well for',
        description: 'Steal this → This pattern worked'
    },
    {
        pattern: /^Most Unity devs get (.*) wrong\. Here's the right way$/,
        replacement: (match, p1) => `Learning ${p1} - here's what helped`,
        description: 'Devs get X wrong → Learning X'
    },
    {
        pattern: /^Every game does \{X\}\. We're doing \{Y\} instead$/,
        replacement: 'Learning game design patterns - trying this approach',
        description: 'Every game X. We Y → Learning patterns'
    },
    {
        pattern: /^(.+) nobody (teaches|explains|mentions)/i,
        replacement: (match, p1) => `${p1} - here's what worked`,
        description: 'Nobody teaches → Here\'s what worked'
    },
    {
        pattern: /^Why we (chose|built|created|made)/i,
        replacement: (match, p1) => `Why I ${p1}`,
        description: 'Why we → Why I'
    },
    {
        pattern: /^(.*)\. We (.*)/,
        replacement: (match, p1, p2) => `${p1} - I ${p2}`,
        description: 'We → I (general)'
    },
    {
        pattern: /^Procedural generation sounds scary\. It's not\. Here's why$/,
        replacement: 'Learning procedural generation - simpler than it sounds',
        description: 'Sounds scary → Learning X'
    }
];

// Body content transformations
const BODY_TRANSFORMATIONS = [
    {
        pattern: /\bWe (built|created|made|implemented)\b/g,
        replacement: 'I $1',
        description: 'We → I in body'
    },
    {
        pattern: /\bOur (game|system|approach)\b/g,
        replacement: 'The $1',
        description: 'Our → The in body'
    },
    {
        pattern: /\bUs:\s*/g,
        replacement: 'My approach: ',
        description: 'Us: → My approach:'
    },
    {
        pattern: /\bPlayers? (will|can|discover)/g,
        replacement: 'Planning: players $1',
        description: 'Players X → Planning: players X'
    }
];

/**
 * Transform a hook to match new voice guidelines
 */
function transformHook(hook) {
    let transformed = hook;
    let matchedRule = null;

    for (const rule of VOICE_TRANSFORMATIONS) {
        const match = hook.match(rule.pattern);
        if (match) {
            if (typeof rule.replacement === 'function') {
                transformed = hook.replace(rule.pattern, rule.replacement);
            } else {
                transformed = rule.replacement;
            }
            matchedRule = rule.description;
            break;
        }
    }

    return { transformed, changed: transformed !== hook, rule: matchedRule };
}

/**
 * Transform body content to match new voice guidelines
 */
function transformBody(body) {
    let transformed = body;
    const rulesApplied = [];

    for (const rule of BODY_TRANSFORMATIONS) {
        const before = transformed;
        transformed = transformed.replace(rule.pattern, rule.replacement);
        if (transformed !== before) {
            rulesApplied.push(rule.description);
        }
    }

    return { transformed, changed: transformed !== body, rules: rulesApplied };
}

/**
 * Update all content in content-bank.json
 */
function updateContentVoice() {
    const contentBankPath = path.join(__dirname, '../../public/social/content-bank.json');
    const contentBank = JSON.parse(fs.readFileSync(contentBankPath, 'utf-8'));

    console.log('\n═══════════════════════════════════════════════════════════\n');
    console.log('🎯 CONTENT VOICE UPDATER\n');
    console.log('═══════════════════════════════════════════════════════════\n');

    const stats = {
        totalPosts: contentBank.content.length,
        hooksChanged: 0,
        bodiesChanged: 0,
        noChanges: 0
    };

    const changes = [];

    const updated = contentBank.content.map(post => {
        const hookResult = transformHook(post.hook);
        const bodyResult = transformBody(post.body);

        if (hookResult.changed || bodyResult.changed) {
            changes.push({
                id: post.id,
                type: post.type,
                oldHook: post.hook,
                newHook: hookResult.transformed,
                hookChanged: hookResult.changed,
                bodyChanged: bodyResult.changed,
                rule: hookResult.rule
            });

            if (hookResult.changed) stats.hooksChanged++;
            if (bodyResult.changed) stats.bodiesChanged++;

            return {
                ...post,
                hook: hookResult.transformed,
                body: bodyResult.transformed
            };
        } else {
            stats.noChanges++;
            return post;
        }
    });

    // Show preview of changes
    console.log('📝 VOICE CHANGES PREVIEW\n');
    console.log('═══════════════════════════════════════════════════════════\n');

    changes.slice(0, 10).forEach((change, index) => {
        console.log(`\n[${index + 1}] Post #${change.id} (${change.type})`);
        if (change.hookChanged) {
            console.log(`\nOLD: "${change.oldHook}"`);
            console.log(`NEW: "${change.newHook}"`);
            console.log(`Rule: ${change.rule}`);
        }
        if (change.bodyChanged) {
            console.log(`Body: Updated with voice transformations`);
        }
    });

    if (changes.length > 10) {
        console.log(`\n... and ${changes.length - 10} more changes\n`);
    }

    console.log('\n═══════════════════════════════════════════════════════════\n');
    console.log(`✓ Total posts processed: ${stats.totalPosts}`);
    console.log(`✓ Hooks updated: ${stats.hooksChanged} posts`);
    console.log(`✓ Bodies updated: ${stats.bodiesChanged} posts`);
    console.log(`✓ No changes needed: ${stats.noChanges} posts`);
    console.log('\n═══════════════════════════════════════════════════════════\n');

    // Save updated content
    const updatedContentBank = {
        ...contentBank,
        meta: {
            ...contentBank.meta,
            lastUpdated: new Date().toISOString().split('T')[0]
        },
        content: updated
    };

    fs.writeFileSync(contentBankPath, JSON.stringify(updatedContentBank, null, 2));

    return { stats, changes };
}

// Run voice update
try {
    const result = updateContentVoice();
    console.log('✅ Content voice update complete!\n');
    console.log(`Updated ${result.stats.hooksChanged + result.stats.bodiesChanged} posts to match new voice guidelines.\n`);
} catch (error) {
    console.error('❌ Error updating content voice:', error);
    process.exit(1);
}
