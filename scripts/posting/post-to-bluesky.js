/**
 * Bluesky (AT Protocol) Posting Module
 * 
 * Posts content to Bluesky using the ATP API.
 * Supports text posts, threading, and basic media uploads.
 */

import { BskyAgent } from '@atproto/api';
import dotenv from 'dotenv';
import fs from 'fs';
import path from 'path';

dotenv.config();

// ═══════════════════════════════════════════════════════════════
// CONFIGURATION
// ═══════════════════════════════════════════════════════════════

const config = {
    handle: process.env.BLUESKY_IDENTIFIER || '',
    appPassword: process.env.BLUESKY_PASSWORD || '',
    service: 'https://bsky.social',
    maxLength: 300, // Bluesky character limit
    retryAttempts: 3,
    retryDelay: 2000, // ms
    dryRun: process.env.DRY_RUN === 'true',
};

// Validate credentials (skip in dry run mode)
if (!config.dryRun && (!config.handle || !config.appPassword)) {
    console.error('❌ Bluesky credentials not found in environment variables');
    console.error('Required: BLUESKY_IDENTIFIER, BLUESKY_PASSWORD');
    process.exit(1);
}

// ═══════════════════════════════════════════════════════════════
// BLUESKY CLIENT
// ═══════════════════════════════════════════════════════════════

let agent = null;

/**
 * Initialize and authenticate Bluesky agent
 */
async function initializeAgent() {
    if (agent) return agent;

    try {
        agent = new BskyAgent({ service: config.service });

        await agent.login({
            identifier: config.handle,
            password: config.appPassword,
        });

        console.log(`✓ Authenticated to Bluesky as @${config.handle}`);
        return agent;

    } catch (error) {
        console.error('❌ Bluesky authentication failed:', error.message);
        throw error;
    }
}

// ═══════════════════════════════════════════════════════════════
// POSTING FUNCTIONS
// ═══════════════════════════════════════════════════════════════

/**
 * Post text content to Bluesky
 * @param {string} text - Content to post
 * @param {object} options - Optional parameters
 * @returns {object} Post result
 */
export async function postToBluesky(text, options = {}) {
    const {
        media = null,
        replyTo = null,
        retry = true
    } = options;

    // Dry run mode
    if (config.dryRun) {
        console.log('[DRY RUN] Would post to Bluesky:');
        console.log('─'.repeat(50));
        console.log(text.substring(0, 200) + (text.length > 200 ? '...' : ''));
        console.log('─'.repeat(50));
        return {
            success: true,
            platform: 'bluesky',
            uri: 'at://did:plc:dryrun/app.bsky.feed.post/123',
            cid: 'dryrun-cid',
            url: `https://bsky.app/profile/${config.handle || 'dryrun'}/post/dryrun123`,
            timestamp: new Date().toISOString(),
        };
    }

    try {
        await initializeAgent();

        // Truncate if exceeds limit
        const truncatedText = text.length > config.maxLength
            ? text.substring(0, config.maxLength - 3) + '...'
            : text;

        const postData = {
            text: truncatedText,
            createdAt: new Date().toISOString(),
        };

        // Add media if provided
        if (media) {
            const uploadedMedia = await uploadMedia(media);
            if (uploadedMedia) {
                postData.embed = {
                    $type: 'app.bsky.embed.images',
                    images: [uploadedMedia],
                };
            }
        }

        // Add reply if provided
        if (replyTo) {
            postData.reply = replyTo;
        }

        const response = await agent.post(postData);

        console.log('✓ Posted to Bluesky successfully');
        console.log(`  URI: ${response.uri}`);
        console.log(`  CID: ${response.cid}`);

        return {
            success: true,
            platform: 'bluesky',
            uri: response.uri,
            cid: response.cid,
            url: `https://bsky.app/profile/${config.handle}/post/${response.uri.split('/').pop()}`,
            timestamp: new Date().toISOString(),
        };

    } catch (error) {
        console.error('❌ Bluesky posting failed:', error.message);

        if (retry && config.retryAttempts > 0) {
            console.log(`  Retrying in ${config.retryDelay}ms...`);
            await new Promise(resolve => setTimeout(resolve, config.retryDelay));
            return postToBluesky(text, { ...options, retry: false });
        }

        return {
            success: false,
            platform: 'bluesky',
            error: error.message,
            timestamp: new Date().toISOString(),
        };
    }
}

/**
 * Post a thread to Bluesky (multiple connected posts)
 * @param {string[]} posts - Array of post texts
 * @returns {object[]} Array of post results
 */
export async function postThreadToBluesky(posts) {
    const results = [];
    let previousPost = null;

    for (let i = 0; i < posts.length; i++) {
        const text = `${posts[i]}\n\n(${i + 1}/${posts.length})`;

        const replyTo = previousPost ? {
            root: results[0].uri ? { uri: results[0].uri, cid: results[0].cid } : null,
            parent: { uri: previousPost.uri, cid: previousPost.cid },
        } : null;

        const result = await postToBluesky(text, { replyTo });
        results.push(result);

        if (result.success) {
            previousPost = result;
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

/**
 * Upload media to Bluesky
 * @param {string} mediaPath - Path to image file
 * @returns {object} Uploaded media blob
 */
async function uploadMedia(mediaPath) {
    try {
        if (!fs.existsSync(mediaPath)) {
            console.error(`❌ Media file not found: ${mediaPath}`);
            return null;
        }

        const imageBuffer = fs.readFileSync(mediaPath);
        const response = await agent.uploadBlob(imageBuffer, {
            encoding: 'image/png', // Adjust based on file type
        });

        return {
            alt: '', // Add alt text support later
            image: response.data.blob,
        };

    } catch (error) {
        console.error('❌ Media upload failed:', error.message);
        return null;
    }
}

// ═══════════════════════════════════════════════════════════════
// UTILITY FUNCTIONS
// ═══════════════════════════════════════════════════════════════

/**
 * Format content for Bluesky (character limit, formatting)
 * @param {string} content - Raw content
 * @returns {string} Formatted content
 */
export function formatForBluesky(content) {
    // Bluesky supports plain text + links
    // Character limit: 300

    let formatted = content;

    // Ensure no em dashes (replace with regular dashes)
    formatted = formatted.replace(/—/g, '-');

    // Truncate if needed
    if (formatted.length > config.maxLength) {
        formatted = formatted.substring(0, config.maxLength - 3) + '...';
    }

    return formatted;
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

    if (currentPost) posts.push(currentPost);

    return posts;
}

// ═══════════════════════════════════════════════════════════════
// CLI TESTING (run directly with node)
// ═══════════════════════════════════════════════════════════════

if (import.meta.url === `file://${process.argv[1]}`) {
    // Test mode
    const testPost = async () => {
        console.log('🧪 Testing Bluesky posting module...\n');

        const testContent = "Test post from Ascendant Continuum social automation system.\n\nIf you're seeing this, authentication is working! 🎮✨";

        const result = await postToBluesky(testContent);

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
