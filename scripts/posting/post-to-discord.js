/**
 * Discord Webhook Posting Module
 * 
 * Posts announcements/updates to Discord channel via webhook.
 * Discord webhooks are simple - no OAuth, just POST to webhook URL.
 */

import fetch from 'node-fetch';
import dotenv from 'dotenv';
import { fitForPlatform, normalizeForPublishing } from '../utils/publish-text-utils.js';

dotenv.config();

const config = {
    webhookUrl: process.env.DISCORD_WEBHOOK_URL || '',
    maxLength: 2000, // Discord message limit
    dryRun: process.env.DRY_RUN === 'true',
};

// ═══════════════════════════════════════════════════════════════
// DISCORD API CLIENT
// ═══════════════════════════════════════════════════════════════

/**
 * Post message to Discord channel via webhook
 * @param {string} content - Message text (up to 2000 chars)
 * @param {Object} options - Additional options
 * @param {string} options.username - Override webhook username
 * @param {string} options.avatarUrl - Override webhook avatar
 * @param {Array} options.embeds - Embed objects (rich content)
 * @returns {Promise<Object>} - Result with success status
 */
export async function postToDiscord(content, options = {}) {
    const webhookUrl = config.webhookUrl;

    // Dry run mode
    if (config.dryRun) {
        console.log('[DRY RUN] Would post to Discord:');
        console.log('─'.repeat(50));
        console.log(content.substring(0, 200) + (content.length > 200 ? '...' : ''));
        if (options.embeds) {
            console.log('With embeds:', options.embeds.length, 'embed(s)');
        }
        console.log('─'.repeat(50));
        return {
            success: true,
            platform: 'discord',
            messageId: 'dry-run-' + Date.now(),
        };
    }

    if (!webhookUrl) {
        throw new Error('DISCORD_WEBHOOK_URL environment variable not set');
    }

    content = fitForPlatform(content, 'discord');

    // Validate content length
    if (content.length > 2000) {
        console.warn('⚠️ Discord content truncated to 2000 characters');
        content = content.substring(0, 1997) + '...';
    }

    // Build Discord message payload
    const payload = {
        content: content,
        username: options.username || 'Ascendant Continuum Updates',
        avatar_url: options.avatarUrl || undefined,
    };

    // Add embeds if provided (rich content)
    if (options.embeds && options.embeds.length > 0) {
        payload.embeds = options.embeds;
    }

    try {
        const response = await fetch(webhookUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(payload),
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Discord API error: ${response.status} ${errorText}`);
        }

        // Webhooks return 204 No Content on success
        console.log('✅ Posted to Discord successfully');
        return {
            success: true,
            platform: 'discord',
            timestamp: new Date().toISOString(),
        };
    } catch (error) {
        console.error('❌ Discord posting failed:', error.message);
        return {
            success: false,
            platform: 'discord',
            error: error.message,
            timestamp: new Date().toISOString(),
        };
    }
}

/**
 * Post announcement with rich embed formatting
 * @param {Object} announcement - Announcement data
 * @param {string} announcement.title - Announcement title
 * @param {string} announcement.description - Main content
 * @param {string} announcement.type - Content type (devUpdate, lore, etc.)
 * @param {string} announcement.url - Optional link
 * @returns {Promise<Object>} - Result with success status
 */
export async function postAnnouncementToDiscord(announcement) {
    const { title, description, type, url } = announcement;

    // Color coding by content type
    const colors = {
        devUpdate: 0x5865f2, // Blurple (Discord brand color)
        devEducation: 0x57f287, // Green
        designPhilosophy: 0xfee75c, // Yellow
        behindScenes: 0xeb459e, // Pink
        lore: 0x9b59b6, // Purple
    };

    const embed = {
        title: title,
        description: description,
        color: colors[type] || colors.devUpdate,
        timestamp: new Date().toISOString(),
        footer: {
            text: 'Ascendant Continuum',
        },
    };

    if (url) {
        embed.url = url;
    }

    return await postToDiscord('', {
        embeds: [embed],
    });
}

/**
 * Format content for Discord (convert markdown if needed)
 * @param {string} content - Raw content text
 * @returns {string} - Discord-formatted content
 */
export function formatForDiscord(content) {
    return normalizeForPublishing(content);
}

/**
 * Check if content needs splitting (2000 char limit)
 * @param {string} content - Content to check
 * @returns {boolean} - True if content exceeds limit
 */
export function needsSplitting(content) {
    return content.length > 2000;
}

/**
 * Split long content into multiple messages
 * @param {string} content - Long content
 * @returns {Array<string>} - Array of message chunks
 */
export function splitIntoMessages(content) {
    const maxLength = 1900; // Leave buffer for safety
    const messages = [];
    let currentMessage = '';

    const lines = content.split('\n');

    for (const line of lines) {
        if ((currentMessage + line + '\n').length > maxLength) {
            if (currentMessage) {
                messages.push(currentMessage.trim());
                currentMessage = '';
            }

            // If single line is too long, split it
            if (line.length > maxLength) {
                const words = line.split(' ');
                for (const word of words) {
                    if ((currentMessage + word + ' ').length > maxLength) {
                        messages.push(currentMessage.trim());
                        currentMessage = word + ' ';
                    } else {
                        currentMessage += word + ' ';
                    }
                }
            } else {
                currentMessage = line + '\n';
            }
        } else {
            currentMessage += line + '\n';
        }
    }

    if (currentMessage) {
        messages.push(currentMessage.trim());
    }

    return messages;
}

/**
 * Post multiple messages in sequence (for long content)
 * @param {Array<string>} messages - Array of message chunks
 * @param {Object} options - Posting options
 * @returns {Promise<Object>} - Result with success status
 */
export async function postMessagesToDiscord(messages, options = {}) {
    const results = [];

    for (let i = 0; i < messages.length; i++) {
        const message = messages[i];
        const isFirst = i === 0;
        const isLast = i === messages.length - 1;

        // Add continuation indicators
        let content = message;
        if (!isFirst) {
            content = `*(continued)*\n\n${content}`;
        }
        if (!isLast) {
            content = `${content}\n\n*(continued below...)*`;
        }

        const result = await postToDiscord(content, options);
        results.push(result);

        // Wait between messages to avoid rate limits
        if (!isLast) {
            await new Promise(resolve => setTimeout(resolve, 1000));
        }
    }

    return {
        success: results.every(r => r.success),
        platform: 'discord',
        messageCount: messages.length,
        results: results,
        timestamp: new Date().toISOString(),
    };
}

// ═══════════════════════════════════════════════════════════════
// EXPORTS
// ═══════════════════════════════════════════════════════════════

export default {
    postToDiscord,
    postAnnouncementToDiscord,
    formatForDiscord,
    needsSplitting,
    splitIntoMessages,
    postMessagesToDiscord,
};
