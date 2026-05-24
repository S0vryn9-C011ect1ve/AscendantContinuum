/**
 * Post What's New Daily Update to Social Media
 * 
 * Announces the daily What's New update to all platforms
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { postToBluesky } from '../posting/post-to-bluesky.js';
import { postToMastodon } from '../posting/post-to-mastodon.js';
import { postToDiscord } from '../posting/post-to-discord.js';
import {
    ensureUrlAtEnd,
    normalizeDevelopmentClaims,
    normalizeForPublishing,
} from '../utils/publish-text-utils.js';
import { assertNoProhibitedContent } from '../utils/truth-and-dedupe-guard.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const DATA_FILE = path.join(__dirname, '..', '..', 'firebase', 'public', 'whats-new', 'data.json');

const DRY_RUN = process.env.DRY_RUN === 'true';

async function main() {
    console.log('Posting daily What\'s New update to social media...\n');

    // Load data
    if (!fs.existsSync(DATA_FILE)) {
        console.error('❌ Error: data.json not found');
        process.exit(1);
    }

    const data = JSON.parse(fs.readFileSync(DATA_FILE, 'utf-8'));

    // Get today's update
    const today = new Date().toISOString().split('T')[0];
    const todayUpdate = data.dailyUpdates.find(u => u.date === today);

    if (!todayUpdate) {
        console.log('ℹ️ No update found for today');
        return;
    }

    // Format message
    const dateObj = new Date(todayUpdate.date);
    const formattedDate = dateObj.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });

    // Create post text
    const highlights = [];
    if (todayUpdate.categories.features > 0) {
        highlights.push(`${todayUpdate.categories.features} new feature${todayUpdate.categories.features !== 1 ? 's' : ''}`);
    }
    if (todayUpdate.categories.fixes > 0) {
        highlights.push(`${todayUpdate.categories.fixes} bug fix${todayUpdate.categories.fixes !== 1 ? 'es' : ''}`);
    }
    if (todayUpdate.categories.improvements > 0) {
        highlights.push(`${todayUpdate.categories.improvements} improvement${todayUpdate.categories.improvements !== 1 ? 's' : ''}`);
    }

    const highlightText = highlights.length > 0 ? highlights.map(h => `- ${h}`).join('\n') : '- Internal maintenance updates';
    const postUrl = `https://ascendant-continuum.web.app${todayUpdate.blogPostUrl}`;

    const postText = ensureUrlAtEnd(normalizeDevelopmentClaims(`What's New - ${formattedDate}

${todayUpdate.totalCommits} commits today:

${highlightText}

Read the full update:
${postUrl}

#GameDev #IndieGame #Unity #AscendantContinuum`), postUrl);
    assertNoProhibitedContent(postText, "what's new social post");

    console.log('Post text:\n');
    console.log(postText);
    console.log('\n' + '═'.repeat(70) + '\n');

    if (DRY_RUN) {
        console.log('DRY RUN: Skipping actual posting');
        return;
    }

    // Post to platforms
    const results = {
        bluesky: null,
        mastodon: null,
        discord: null
    };

    try {
        console.log('Posting to Bluesky...');
        results.bluesky = await postToBluesky(normalizeForPublishing(postText));
        console.log('Bluesky posted successfully');
    } catch (error) {
        console.error('❌ Bluesky error:', error.message);
    }

    try {
        console.log('Posting to Mastodon...');
        results.mastodon = await postToMastodon(normalizeForPublishing(postText));
        console.log('Mastodon posted successfully');
    } catch (error) {
        console.error('❌ Mastodon error:', error.message);
    }

    try {
        console.log('Posting to Discord...');
        results.discord = await postToDiscord(normalizeForPublishing(postText));
        console.log('Discord posted successfully');
    } catch (error) {
        console.error('❌ Discord error:', error.message);
    }

    console.log('\nSocial media posting complete\n');
}

main().catch(error => {
    console.error('❌ Fatal error:', error);
    process.exit(1);
});
