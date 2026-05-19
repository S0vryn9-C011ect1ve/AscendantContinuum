/**
 * Mastodon Posting Module
 * 
 * Posts content to Mastodon using the Megalodon library (modern Mastodon client).
 * Supports text posts, threading, and content warnings.
 */

import generator from 'megalodon';
import dotenv from 'dotenv';
import { fitForPlatform, normalizeForPublishing } from '../utils/publish-text-utils.js';

dotenv.config();

// ═══════════════════════════════════════════════════════════════
// CONFIGURATION
// ═══════════════════════════════════════════════════════════════

const config = {
    instance: process.env.MASTODON_INSTANCE || 'https://mastodon.social',
    accessToken: process.env.MASTODON_ACCESS_TOKEN || '',
    maxLength: 500, // Mastodon character limit (default)
    dryRun: process.env.DRY_RUN === 'true',
};

// Validate credentials (skip in dry run mode)
if (!config.dryRun && !config.accessToken) {
    console.error('❌ Mastodon credentials not found in environment variables');
    console.error('Required: MASTODON_ACCESS_TOKEN, MASTODON_INSTANCE');
    process.exit(1);
}

// ═══════════════════════════════════════════════════════════════
// POSTING FUNCTIONS
// ═══════════════════════════════════════════════════════════════

/**
 * Post text content to Mastodon
 * @param {string} text - Content to post
 * @param {object} options - Optional parameters
 * @returns {object} Post result
 */
export async function postToMastodon(text, options = {}) {
    const {
        contentWarning = null,
        visibility = 'public', // public, unlisted, private, direct
        inReplyTo = null,
    } = options;

    // Dry run mode
    if (config.dryRun) {
        console.log('[DRY RUN] Would post to Mastodon:');
        console.log('─'.repeat(50));
        if (contentWarning) console.log(`  CW: ${contentWarning}`);
        console.log(text.substring(0, 200) + (text.length > 200 ? '...' : ''));
        console.log('─'.repeat(50));
        return {
            success: true,
            platform: 'mastodon',
            id: 'dry-run-' + Date.now(),
            url: 'https://example.social/@dryrun/123',
            timestamp: new Date().toISOString(),
        };
    }

    try {
        const truncatedText = fitForPlatform(text, 'mastodon');

        // Create Mastodon client
        const client = generator('mastodon', config.instance, config.accessToken);

        // Build post options
        const postOptions = {
            visibility: visibility,
        };

        if (contentWarning) {
            postOptions.spoiler_text = contentWarning;
        }

        if (inReplyTo) {
            postOptions.in_reply_to_id = inReplyTo;
        }

        // Post to Mastodon
        const response = await client.postStatus(truncatedText, postOptions);

        console.log('✓ Posted to Mastodon successfully');
        console.log(`  ID: ${response.data.id}`);
        console.log(`  URL: ${response.data.url}`);

        return {
            success: true,
            platform: 'mastodon',
            id: response.data.id,
            url: response.data.url,
            timestamp: response.data.created_at,
        };

    } catch (error) {
        console.error('❌ Mastodon posting failed:', error.message);
        return {
            success: false,
            platform: 'mastodon',
            error: error.message,
            timestamp: new Date().toISOString(),
        };
    }
}

/**
 * Post a thread to Mastodon (multiple connected posts)
 * @param {string[]} posts - Array of post texts
 * @param {object} options - Optional parameters
 * @returns {object[]} Array of post results
 */
export async function postThreadToMastodon(posts, options = {}) {
    const results = [];
    let previousPostId = null;

    for (let i = 0; i < posts.length; i++) {
        const text = `${posts[i]}\n\n(${i + 1}/${posts.length})`;

        const result = await postToMastodon(text, {
            ...options,
            inReplyTo: previousPostId,
        });

        results.push(result);

        if (result.success) {
            previousPostId = result.id;
            // Rate limiting: wait between posts
            if (i < posts.length - 1) {
                await new Promise(resolve => setTimeout(resolve, 1000));
            }
        } else {
            console.error(`❌ Thread broke at post ${i + 1}`);
            break;
        }
    }

    return results;
}

// ═══════════════════════════════════════════════════════════════
// UTILITY FUNCTIONS
// ═══════════════════════════════════════════════════════════════

/**
 * Format content for Mastodon (character limit, formatting)
 * @param {string} content - Raw content
 * @returns {string} Formatted content
 */
export function formatForMastodon(content) {
    return normalizeForPublishing(content);
}

/**
 * Check if content needs to be a thread
 * @param {string} content - Content to check
 * @returns {boolean} True if threading needed
 */
export function needsThreading(content) {
    return content.length > config.maxLength;
}

/**
 * Split content into thread posts
 * @param {string} content - Long content
 * @returns {string[]} Array of post texts
 */
export function splitIntoThread(content) {
    const posts = [];
    const paragraphs = content.split('\n\n');

    let currentPost = '';

    for (const paragraph of paragraphs) {
        // Reserve space for thread counter "(1/X)"
        const availableSpace = config.maxLength - 20;

        if ((currentPost + '\n\n' + paragraph).length < availableSpace) {
            currentPost += (currentPost ? '\n\n' : '') + paragraph;
        } else {
            if (currentPost) posts.push(currentPost);
            currentPost = paragraph;
        }
    }

    // Add last post
    if (currentPost) posts.push(currentPost);

    return posts;
}

/**
 * Get content warning for post type
 * @param {string} type - Content type
 * @returns {string|null} Content warning text
 */
export function getContentWarning(type) {
    const warnings = {
        'lore': 'Ascendant Continuum Lore',
        'spoiler': 'Game Spoiler',
        'technical': 'Technical Discussion',
    };
    return warnings[type] || null;
}

// Export for testing or manual use
export const test = async () => {
    console.log('🧪 Testing Mastodon posting module...');
    const result = await postToMastodon('This is a test post from Ascendant Continuum!');
    console.log('Test result:', result);
    return result;
};

// ═══════════════════════════════════════════════════════════════
// CLI TESTING (run directly with node)
// ═══════════════════════════════════════════════════════════════

if (import.meta.url === `file://${process.argv[1]}`) {
    // Test mode
    const testPost = async () => {
        console.log('🧪 Testing Mastodon posting module...\n');

        const testContent = "Test post from Ascendant Continuum social automation system.\n\nIf you are seeing this, authentication is working.\n\n#IndieGameDev #GameDev";

        const result = await postToMastodon(testContent);

        if (result.success) {
            console.log('\n✅ Test successful!');
            console.log(`View post: ${result.url}`);
        } else {
            console.log('\n❌ Test failed');
            console.log(`Error: ${result.error}`);
        }
    };

    testPost().catch(console.error);
}
