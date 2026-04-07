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
    console.log('📊 Posting weekly digest to social media...\n');

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
        highlights.push(`✨ ${weeklyDigest.highlights.features[0]}`);
    }
    if (weeklyDigest.highlights.fixes.length > 0) {
        highlights.push(`🐛 ${weeklyDigest.highlights.fixes[0]}`);
    }
    if (weeklyDigest.highlights.improvements.length > 0) {
        highlights.push(`⚡ ${weeklyDigest.highlights.improvements[0]}`);
    }

    const highlightText = highlights.slice(0, 3).join('\n');

    const postText = `📊 Weekly Development Digest

${formattedStart} - ${formattedEnd}

This week:
📦 ${weeklyDigest.totalCommits} commits across ${weeklyDigest.daysWithUpdates} days
✨ ${weeklyDigest.categories.features} features
🐛 ${weeklyDigest.categories.fixes} fixes
⚡ ${weeklyDigest.categories.improvements} improvements

Highlights:
${highlightText}

Read the full digest:
https://ascendant-continuum.web.app${weeklyDigest.blogPostUrl}

#GameDev #IndieGame #WeeklyUpdate #AscendantContinuum`;

    // Discord version (with embed)
    const discordEmbed = {
        embeds: [{
            title: `📊 Weekly Digest: ${formattedStart} - ${formattedEnd}`,
            description: `${weeklyDigest.totalCommits} commits across ${weeklyDigest.daysWithUpdates} days`,
            color: 0x8B5CF6, // Purple
            fields: [
                {
                    name: '✨ Features',
                    value: `${weeklyDigest.categories.features}`,
                    inline: true
                },
                {
                    name: '🐛 Fixes',
                    value: `${weeklyDigest.categories.fixes}`,
                    inline: true
                },
                {
                    name: '⚡ Improvements',
                    value: `${weeklyDigest.categories.improvements}`,
                    inline: true
                },
                {
                    name: '🌟 Top Highlights',
                    value: highlights.slice(0, 3).join('\n') || 'Various improvements',
                    inline: false
                }
            ],
            url: `https://ascendant-continuum.web.app${weeklyDigest.blogPostUrl}`,
            footer: {
                text: 'The Ascendant Continuum'
            },
            timestamp: new Date().toISOString()
        }]
    };

    console.log('📝 Post text:\n');
    console.log(postText);
    console.log('\n' + '═'.repeat(70) + '\n');

    if (DRY_RUN) {
        console.log('🔍 DRY RUN: Skipping actual posting');
        return;
    }

    // Post to platforms
    const results = {
        bluesky: null,
        mastodon: null,
        discord: null
    };

    try {
        console.log('📤 Posting to Bluesky...');
        results.bluesky = await postToBluesky(postText);
        console.log('✅ Bluesky posted successfully');
    } catch (error) {
        console.error('❌ Bluesky error:', error.message);
    }

    try {
        console.log('📤 Posting to Mastodon...');
        results.mastodon = await postToMastodon(postText);
        console.log('✅ Mastodon posted successfully');
    } catch (error) {
        console.error('❌ Mastodon error:', error.message);
    }

    try {
        console.log('📤 Posting to Discord...');
        results.discord = await postToDiscord(discordEmbed);
        console.log('✅ Discord posted successfully');
    } catch (error) {
        console.error('❌ Discord error:', error.message);
    }

    console.log('\n✅ Social media posting complete\n');
}

main().catch(error => {
    console.error('❌ Fatal error:', error);
    process.exit(1);
});
