/**
 * Post Weekly Digest to Social Media
 * 
 * Announces the weekly digest summary to all platforms
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

function getWeekStart(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Monday
    return new Date(d.setDate(diff));
}

async function main() {
    console.log('Posting weekly digest to social media...\n');

    // Load data
    if (!fs.existsSync(DATA_FILE)) {
        console.error('❌ Error: data.json not found');
        process.exit(1);
    }

    const data = JSON.parse(fs.readFileSync(DATA_FILE, 'utf-8'));

    // Get last week's digest (previous Monday)
    const today = new Date();
    const lastMonday = getWeekStart(today);
    lastMonday.setDate(lastMonday.getDate() - 7);
    const weekKey = lastMonday.toISOString().split('T')[0];

    const weeklyDigest = data.weeklyDigests?.find(d => d.weekStart === weekKey);

    if (!weeklyDigest) {
        console.log(`ℹ️ No weekly digest found for week starting ${weekKey}`);
        return;
    }

    // Format message
    const weekStartObj = new Date(weeklyDigest.weekStart);
    const weekEndObj = new Date(weeklyDigest.weekEnd);

    const dateFormat = { month: 'short', day: 'numeric' };
    const formattedStart = weekStartObj.toLocaleDateString('en-US', dateFormat);
    const formattedEnd = weekEndObj.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });

    // Create highlights list
    const highlights = [];
    if (weeklyDigest.highlights.features.length > 0) {
        highlights.push(`Feature: ${weeklyDigest.highlights.features[0]}`);
    }
    if (weeklyDigest.highlights.fixes.length > 0) {
        highlights.push(`Fix: ${weeklyDigest.highlights.fixes[0]}`);
    }
    if (weeklyDigest.highlights.improvements.length > 0) {
        highlights.push(`Improvement: ${weeklyDigest.highlights.improvements[0]}`);
    }

    const highlightText = highlights.length > 0
        ? highlights.slice(0, 3).map(h => `- ${h}`).join('\n')
        : '- Various improvements';
    const postUrl = `https://ascendant-continuum.web.app${weeklyDigest.blogPostUrl}`;

    const postText = ensureUrlAtEnd(normalizeDevelopmentClaims(`Weekly Development Digest

${formattedStart} - ${formattedEnd}

This week:
- ${weeklyDigest.totalCommits} commits across ${weeklyDigest.daysWithUpdates} days
- ${weeklyDigest.categories.features} features
- ${weeklyDigest.categories.fixes} fixes
- ${weeklyDigest.categories.improvements} improvements

Highlights:
${highlightText}

Read the full digest:
${postUrl}

#GameDev #IndieGame #WeeklyUpdate #AscendantContinuum`), postUrl);
    assertNoProhibitedContent(postText, 'weekly digest social post');

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
