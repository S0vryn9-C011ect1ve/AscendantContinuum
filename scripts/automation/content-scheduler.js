/**
 * Content Scheduler — Main Social Media Orchestrator
 * 
 * Coordinates automated content posting across Bluesky, Mastodon, and Discord.
 * Implements smart rotation, hook selection, and posting history tracking.
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import {
    postToBluesky,
    postThreadToBluesky,
    formatForBluesky,
    needsThreading as blueskyNeedsThreading,
    splitIntoThread as blueskySplitThread
} from '../posting/post-to-bluesky.js';
import {
    postToMastodon,
    postThreadToMastodon,
    formatForMastodon,
    needsThreading as mastodonNeedsThreading,
    splitIntoThread as mastodonSplitThread,
    getContentWarning
} from '../posting/post-to-mastodon.js';
import {
    postAnnouncementToDiscord,
    formatForDiscord,
    needsSplitting,
    postMessagesToDiscord,
    splitIntoMessages
} from '../posting/post-to-discord.js';
import { getHook, customizeHook } from './viral-hooks-gaming.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ═══════════════════════════════════════════════════════════════
// FILE PATHS
// ═══════════════════════════════════════════════════════════════

const CONTENT_BANK_PATH = path.join(__dirname, '../../public/social/content-bank.json');
const POSTING_HISTORY_PATH = path.join(__dirname, '../../public/social/posting-history.json');

// ═══════════════════════════════════════════════════════════════
// CONFIGURATION
// ═══════════════════════════════════════════════════════════════

const config = {
    platforms: ['bluesky', 'mastodon', 'discord'], // Platforms to post to
    contentDistribution: {
        devUpdate: 0.40,
        devEducation: 0.30,
        designPhilosophy: 0.20,
        behindScenes: 0.10,
    },
    hookRotationDays: 30, // Don't reuse hooks within 30 days
    postDelay: 2000, // ms between platform posts (rate limiting)
    dryRun: process.env.DRY_RUN === 'true', // Test mode (no actual posting)
};

// ═══════════════════════════════════════════════════════════════
// MAIN ORCHESTRATION FUNCTION
// ═══════════════════════════════════════════════════════════════

/**
 * Main scheduler: pick content, post to platforms, log results
 */
export async function runScheduler() {
    console.log('🚀 Social Media Content Scheduler Starting...\n');

    try {
        // 1. Load content bank and posting history
        const contentBank = loadContentBank();
        const postingHistory = loadPostingHistory();

        // 2. Select content to post
        const selectedContent = selectContent(contentBank, postingHistory);

        if (!selectedContent) {
            console.log('⚠️  No available content to post (all used or filtered)');
            return;
        }

        console.log(`📝 Selected content: #${selectedContent.id} (${selectedContent.type})`);
        console.log(`   Hook: "${selectedContent.hook}"`);
        console.log(`   Platforms: ${selectedContent.platforms.join(', ')}\n`);

        // 3. Post to each platform
        const results = [];

        for (const platform of selectedContent.platforms) {
            if (config.platforms.includes(platform)) {
                console.log(`📡 Posting to ${platform.toUpperCase()}...`);

                const result = await postToPlatform(platform, selectedContent);
                results.push(result);

                // Rate limiting between platforms
                if (platform !== selectedContent.platforms[selectedContent.platforms.length - 1]) {
                    await sleep(config.postDelay);
                }
            }
        }

        // 4. Update content bank (mark as used)
        updateContentBank(contentBank, selectedContent.id);

        // 5. Log posting history
        logPostingHistory(postingHistory, selectedContent, results);

        // 6. Print summary
        printSummary(selectedContent, results);

        return { selectedContent, results };

    } catch (error) {
        console.error('❌ Scheduler failed:', error.message);
        console.error(error.stack);
        process.exit(1);
    }
}

// ═══════════════════════════════════════════════════════════════
// CONTENT SELECTION LOGIC
// ═══════════════════════════════════════════════════════════════

/**
 * Select content from bank based on rotation rules
 */
function selectContent(contentBank, postingHistory) {
    const today = new Date().toISOString().split('T')[0];
    const dayOfWeek = new Date().getDay(); // 0 = Sunday, 6 = Saturday

    // Filter: get unused content
    const unusedContent = contentBank.content.filter(item => !item.used);

    if (unusedContent.length === 0) {
        // All content used, reset the pool
        console.log('♻️  All content used, resetting pool...');
        contentBank.content.forEach(item => item.used = false);
        return selectContent(contentBank, postingHistory); // Recurse with reset pool
    }

    // Apply time-based content type preferences
    let preferredTypes = [];

    if (dayOfWeek >= 1 && dayOfWeek <= 5) {
        // Weekdays: dev updates
        preferredTypes = ['devUpdate', 'devEducation'];
    } else {
        // Weekends: philosophy + lore
        preferredTypes = ['designPhilosophy', 'loreSnippet'];
    }

    // Try to get preferred type first
    let candidates = unusedContent.filter(item => preferredTypes.includes(item.type));

    // If no preferred type available, use any
    if (candidates.length === 0) {
        candidates = unusedContent;
    }

    // Sort by priority (high > medium > low)
    const priorityOrder = { high: 3, medium: 2, low: 1 };
    candidates.sort((a, b) => {
        return (priorityOrder[b.priority] || 0) - (priorityOrder[a.priority] || 0);
    });

    // Avoid posting same type as last post (diversity)
    const lastPost = postingHistory.posts[postingHistory.posts.length - 1];
    if (lastPost && candidates.length > 1) {
        const differentType = candidates.find(c => c.type !== lastPost.contentType);
        if (differentType) {
            return differentType;
        }
    }

    // Return top candidate
    return candidates[0];
}

/**
 * Post content to specific platform
 */
async function postToPlatform(platform, content) {
    const fullText = `${content.hook}\n\n${content.body}`;

    if (config.dryRun) {
        console.log(`  [DRY RUN] Would post to ${platform}:`);
        console.log(`  "${fullText.substring(0, 100)}..."`);
        return {
            success: true,
            platform,
            dryRun: true,
            timestamp: new Date().toISOString(),
        };
    }

    try {
        if (platform === 'bluesky') {
            const formatted = formatForBluesky(fullText);

            if (blueskyNeedsThreading(formatted)) {
                const thread = blueskySplitThread(formatted);
                const results = await postThreadToBluesky(thread);
                return results[0]; // Return first post result
            } else {
                return await postToBluesky(formatted, { media: content.media });
            }
        }

        if (platform === 'mastodon') {
            const formatted = formatForMastodon(fullText);
            const contentWarning = getContentWarning(formatted, content.type);

            if (mastodonNeedsThreading(formatted)) {
                const thread = mastodonSplitThread(formatted);
                const results = await postThreadToMastodon(thread, { contentWarning });
                return results[0]; // Return first post result
            } else {
                return await postToMastodon(formatted, {
                    media: content.media,
                    contentWarning,
                });
            }
        }

        if (platform === 'discord') {
            const formatted = formatForDiscord(fullText);

            // Discord announcements use embed format for better presentation
            const announcement = {
                title: content.hook,
                description: content.body,
                type: content.type,
                url: content.url || null,
            };

            if (needsSplitting(formatted)) {
                const messages = splitIntoMessages(formatted);
                return await postMessagesToDiscord(messages);
            } else {
                return await postAnnouncementToDiscord(announcement);
            }
        }

    } catch (error) {
        console.error(`  ❌ Failed to post to ${platform}:`, error.message);
        return {
            success: false,
            platform,
            error: error.message,
            timestamp: new Date().toISOString(),
        };
    }
}

// ═══════════════════════════════════════════════════════════════
// FILE OPERATIONS
// ═══════════════════════════════════════════════════════════════

/**
 * Load content bank JSON
 */
function loadContentBank() {
    if (!fs.existsSync(CONTENT_BANK_PATH)) {
        console.error('❌ Content bank not found:', CONTENT_BANK_PATH);
        process.exit(1);
    }

    return JSON.parse(fs.readFileSync(CONTENT_BANK_PATH, 'utf8'));
}

/**
 * Load posting history JSON
 */
function loadPostingHistory() {
    if (!fs.existsSync(POSTING_HISTORY_PATH)) {
        // Create initial history file
        const initialHistory = {
            posts: [],
            stats: {
                totalPosts: 0,
                byPlatform: {},
                byType: {},
            },
        };
        fs.writeFileSync(POSTING_HISTORY_PATH, JSON.stringify(initialHistory, null, 2));
        return initialHistory;
    }

    return JSON.parse(fs.readFileSync(POSTING_HISTORY_PATH, 'utf8'));
}

/**
 * Update content bank (mark content as used)
 */
function updateContentBank(contentBank, contentId) {
    const content = contentBank.content.find(c => c.id === contentId);
    if (content) {
        content.used = true;
        content.lastUsed = new Date().toISOString();
    }

    fs.writeFileSync(CONTENT_BANK_PATH, JSON.stringify(contentBank, null, 2));
    console.log('✓ Content bank updated');
}

/**
 * Log posting results to history
 */
function logPostingHistory(history, content, results) {
    const post = {
        id: history.posts.length + 1,
        contentId: content.id,
        contentType: content.type,
        hook: content.hook,
        timestamp: new Date().toISOString(),
        platforms: results.map(r => r.platform),
        results: results.map(r => ({
            platform: r.platform,
            success: r.success,
            url: r.url,
            error: r.error,
        })),
    };

    history.posts.push(post);

    // Update stats
    history.stats.totalPosts++;
    results.forEach(r => {
        history.stats.byPlatform[r.platform] = (history.stats.byPlatform[r.platform] || 0) + 1;
    });
    history.stats.byType[content.type] = (history.stats.byType[content.type] || 0) + 1;

    fs.writeFileSync(POSTING_HISTORY_PATH, JSON.stringify(history, null, 2));
    console.log('✓ Posting history updated');
}

// ═══════════════════════════════════════════════════════════════
// UTILITIES
// ═══════════════════════════════════════════════════════════════

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function printSummary(content, results) {
    console.log('\n' + '═'.repeat(60));
    console.log('📊 POSTING SUMMARY');
    console.log('═'.repeat(60));
    console.log(`Content ID: #${content.id}`);
    console.log(`Type: ${content.type}`);
    console.log(`Hook: "${content.hook}"`);
    console.log(`\nResults:`);

    results.forEach(r => {
        const status = r.success ? '✅' : '❌';
        console.log(`  ${status} ${r.platform.toUpperCase()}: ${r.success ? r.url : r.error}`);
    });

    console.log('═'.repeat(60) + '\n');
}

// ═══════════════════════════════════════════════════════════════
// CLI EXECUTION
// ═══════════════════════════════════════════════════════════════

// Run scheduler when executed directly (not imported)
runScheduler()
    .then(() => {
        console.log('✅ Scheduler completed successfully');
        process.exit(0);
    })
    .catch((error) => {
        console.error('❌ Scheduler failed:', error);
        process.exit(1);
    });
