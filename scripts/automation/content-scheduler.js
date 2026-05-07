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
    postToDiscord,
    formatForDiscord,
    needsSplitting,
    postMessagesToDiscord,
    splitIntoMessages
} from '../posting/post-to-discord.js';
import { getHook, customizeHook } from './viral-hooks-gaming.js';
import {
    ensureUrlAtEnd,
    extractLastUrl,
    normalizeForPublishing,
    truncatePreservingLastUrl
} from '../utils/publish-text-utils.js';

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
    contentCooldownDays: 30, // Don't repost same content within 30 days
    topicCooldownDays: 7, // Spread similar topics apart by 7 days minimum
    postDelay: 2000, // ms between platform posts (rate limiting)
    dryRun: process.env.DRY_RUN === 'true', // Test mode (no actual posting)
};

// Topic keyword groups for diversity tracking
const TOPIC_KEYWORDS = {
    'colorblind-accessibility': ['colorblind', 'colour.?blind', 'color.?blind', 'vision type', 'protanopia', 'deuteranopia', 'tritanopia', 'accessibility mode'],
    'npc-memory': ['npc', 'game character', 'collective memory', 'collective intelligence'],
    'moon-phases': ['moon', 'lunar', 'celestial'],
    'digital-sunset': ['digital sunset', 'rest', 'wellness'],
    'realms-lore': ['emberforge', 'verdant', 'echo fields', 'dawn', 'lantern', 'realm'],
    'procedural-generation': ['procedural', 'algorithm-created', 'procedurally'],
    'monetization': ['monetization', 'pay-to-win', 'dlc', 'cosmetics', 'revenue'],
    'unity-tech': ['unity profiler', 'webgl', 'draw calls', 'fps', 'shader'],
    'anti-fomo': ['fomo', 'daily login', 'grind', 'addiction'],
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

        if (!config.dryRun) {
            // 4. Update content bank (mark as used)
            updateContentBank(contentBank, selectedContent.id);

            // 5. Log posting history
            logPostingHistory(postingHistory, selectedContent, results);
        } else {
            console.log('DRY RUN: Skipping content bank/history writes');
        }

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
 * Detect topic from content text
 */
function detectTopics(content) {
    const text = `${content.hook} ${content.body}`.toLowerCase();
    const topics = [];

    for (const [topic, keywords] of Object.entries(TOPIC_KEYWORDS)) {
        for (const keyword of keywords) {
            const regex = new RegExp(keyword, 'i');
            if (regex.test(text)) {
                topics.push(topic);
                break; // Only add topic once
            }
        }
    }

    return topics;
}

/**
 * Get recent topics from posting history (last N days)
 */
function getRecentTopics(postingHistory, days = 7) {
    const cutoffDate = new Date();
    cutoffDate.setDate(cutoffDate.getDate() - days);

    const recentTopics = new Set();

    postingHistory.posts
        .filter(post => new Date(post.timestamp) > cutoffDate)
        .forEach(post => {
            if (post.topics) {
                post.topics.forEach(topic => recentTopics.add(topic));
            }
        });

    return recentTopics;
}

/**
 * Calculate days since last use
 */
function daysSinceLastUse(lastUsedDate) {
    if (!lastUsedDate) return Infinity;
    const lastUsed = new Date(lastUsedDate);
    const now = new Date();
    const diffMs = now - lastUsed;
    return Math.floor(diffMs / (1000 * 60 * 60 * 24));
}

/**
 * Get content IDs that have been posted recently (within cooldown period)
 */
function getRecentlyPostedIds(postingHistory, days) {
    const cutoffDate = new Date();
    cutoffDate.setDate(cutoffDate.getDate() - days);

    const recentIds = new Set();

    postingHistory.posts
        .filter(post => new Date(post.timestamp) > cutoffDate)
        .forEach(post => {
            recentIds.add(post.contentId);
        });

    return recentIds;
}

/**
 * Select content from bank based on smart rotation rules
 */
function selectContent(contentBank, postingHistory) {
    const today = new Date().toISOString().split('T')[0];
    const dayOfWeek = new Date().getDay(); // 0 = Sunday, 6 = Saturday

    // Get recently used topics to avoid repetition
    const recentTopics = getRecentTopics(postingHistory, config.topicCooldownDays);

    // Get content IDs posted recently (source of truth: posting history, not content-bank)
    const recentlyPostedIds = getRecentlyPostedIds(postingHistory, config.contentCooldownDays);

    console.log(`🚫 Recently posted (last ${config.contentCooldownDays} days): ${recentlyPostedIds.size} posts excluded`);

    // Filter: content that hasn't been posted in the last 30 days
    const availableContent = contentBank.content.filter(item => {
        return !recentlyPostedIds.has(item.id);
    });

    if (availableContent.length === 0) {
        console.log('⚠️  All content posted within last 30 days!');
        console.log('♻️  Resetting cooldown - will select least recently posted content');

        // Find the oldest post from history
        const postDates = new Map();
        postingHistory.posts.forEach(post => {
            const existingDate = postDates.get(post.contentId);
            const postDate = new Date(post.timestamp);
            if (!existingDate || postDate > existingDate) {
                postDates.set(post.contentId, postDate);
            }
        });

        // Get all content sorted by how long ago it was posted
        const allContentWithDates = contentBank.content.map(item => {
            const lastPosted = postDates.get(item.id);
            const daysSince = lastPosted
                ? Math.floor((new Date() - lastPosted) / (1000 * 60 * 60 * 24))
                : Infinity;
            return { item, daysSince, lastPosted };
        });

        // Sort by oldest first
        allContentWithDates.sort((a, b) => b.daysSince - a.daysSince);

        console.log(`   Selecting from posts not used in ${allContentWithDates[0].daysSince}+ days`);

        // Use the 10 oldest posts as available pool
        return selectContent(
            { ...contentBank, content: allContentWithDates.slice(0, 10).map(x => x.item) },
            postingHistory
        );
    }

    // Apply time-based content type preferences
    let preferredTypes = [];

    if (dayOfWeek >= 1 && dayOfWeek <= 5) {
        // Weekdays: dev updates & education
        preferredTypes = ['devUpdate', 'devEducation'];
    } else {
        // Weekends: philosophy + lore
        preferredTypes = ['designPhilosophy', 'loreSnippet'];
    }

    // Filter by preferred type first
    let candidates = availableContent.filter(item => preferredTypes.includes(item.type));

    // If no preferred type available, use any available content
    if (candidates.length === 0) {
        candidates = availableContent; // Use all available content
    }

    // Filter out content with recently used topics (avoid topic repetition)
    const topicDiverseCandidates = candidates.filter(item => {
        const topics = detectTopics(item);
        return !topics.some(topic => recentTopics.has(topic));
    });

    // If topic filtering removed everything, use original candidates
    if (topicDiverseCandidates.length > 0) {
        candidates = topicDiverseCandidates;
        console.log(`✓ Topic diversity filter: ${topicDiverseCandidates.length} candidates (avoiding: ${Array.from(recentTopics).join(', ')})`);
    }

    // Avoid posting same type as last post (type diversity)
    const lastPost = postingHistory.posts[postingHistory.posts.length - 1];
    if (lastPost && candidates.length > 1) {
        const differentType = candidates.find(c => c.type !== lastPost.contentType);
        if (differentType) {
            const sameTypeCount = candidates.filter(c => c.type === lastPost.contentType).length;
            console.log(`✓ Type diversity: avoiding ${lastPost.contentType} (last post)`);
        }
    }

    // Score each candidate based on:
    // 1. Priority (high = 100, medium = 50, low = 25)
    // 2. Days since last post (from posting history - more points for older posts)
    // 3. Type match bonus (preferred types get +30)
    const priorityScore = { high: 100, medium: 50, low: 25 };

    // Build map of content ID -> last posted date
    const lastPostedMap = new Map();
    postingHistory.posts.forEach(post => {
        const existing = lastPostedMap.get(post.contentId);
        const postDate = new Date(post.timestamp);
        if (!existing || postDate > existing) {
            lastPostedMap.set(post.contentId, postDate);
        }
    });

    const scoredCandidates = candidates.map(item => {
        let score = 0;

        // Priority score
        score += priorityScore[item.priority] || 0;

        // Recency score (more points for posts not posted recently)
        const lastPosted = lastPostedMap.get(item.id);
        if (!lastPosted) {
            score += 200; // Never posted = highest recency bonus
        } else {
            const daysSince = Math.floor((new Date() - lastPosted) / (1000 * 60 * 60 * 24));
            score += Math.min(daysSince * 5, 150); // Cap at 150 points
        }

        // Type match bonus
        if (preferredTypes.includes(item.type)) {
            score += 30;
        }

        // Avoid same type as last post
        if (lastPost && item.type === lastPost.contentType && candidates.length > 1) {
            score -= 40;
        }

        return { item, score, lastPosted };
    });

    // Sort by score (highest first)
    scoredCandidates.sort((a, b) => b.score - a.score);

    // Debug log top 3 candidates
    console.log('\n📊 Top candidates:');
    scoredCandidates.slice(0, 3).forEach((candidate, i) => {
        const daysSince = candidate.lastPosted
            ? Math.floor((new Date() - candidate.lastPosted) / (1000 * 60 * 60 * 24))
            : Infinity;
        const daysSinceStr = daysSince === Infinity ? 'never' : `${daysSince}d ago`;
        console.log(`   ${i + 1}. [Score: ${candidate.score}] #${candidate.item.id} - ${candidate.item.hook.substring(0, 50)}... (${candidate.item.type}, priority: ${candidate.item.priority}, last: ${daysSinceStr})`);
    });
    console.log('');

    // Return top candidate
    return scoredCandidates[0]?.item || null;
}

/**
 * Post content to specific platform
 */
async function postToPlatform(platform, content) {
    const fallbackUrl = content.url || extractLastUrl(content.body) || 'https://ascendant-continuum.web.app/blog/';
    const fullText = ensureUrlAtEnd(`${content.hook}\n\n${content.body}`, fallbackUrl);

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
            const normalized = normalizeForPublishing(fullText);

            if (blueskyNeedsThreading(normalized)) {
                const thread = blueskySplitThread(normalized).map(part => formatForBluesky(part));
                const results = await postThreadToBluesky(thread);
                return results[0]; // Return first post result
            } else {
                const singlePost = formatForBluesky(truncatePreservingLastUrl(normalized, 300));
                return await postToBluesky(singlePost, { media: content.media });
            }
        }

        if (platform === 'mastodon') {
            const normalized = normalizeForPublishing(fullText);
            const contentWarning = getContentWarning(content.type);

            if (mastodonNeedsThreading(normalized)) {
                const thread = mastodonSplitThread(normalized).map(part => formatForMastodon(part));
                const results = await postThreadToMastodon(thread, { contentWarning });
                return results[0]; // Return first post result
            } else {
                const singlePost = formatForMastodon(truncatePreservingLastUrl(normalized, 500));
                return await postToMastodon(singlePost, {
                    media: content.media,
                    contentWarning,
                });
            }
        }

        if (platform === 'discord') {
            const formatted = formatForDiscord(fullText);

            if (needsSplitting(formatted)) {
                const messages = splitIntoMessages(formatted);
                return await postMessagesToDiscord(messages);
            } else {
                return await postToDiscord(formatted);
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
    // Detect topics for this post
    const topics = detectTopics(content);

    const post = {
        id: history.posts.length + 1,
        contentId: content.id,
        contentType: content.type,
        hook: content.hook,
        topics: topics, // Track topics for diversity
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
