/**
 * Dev Update Posting Script
 * Generates dev update from recent commits and posts to all platforms
 */

import { execSync } from 'child_process';
import { postToBluesky } from '../posting/post-to-bluesky.js';
import { postToMastodon } from '../posting/post-to-mastodon.js';
import { postAnnouncementToDiscord } from '../posting/post-to-discord.js';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const POSTING_HISTORY_PATH = path.join(__dirname, '../../public/social/posting-history.json');

/**
 * Get recent commits from the last 24 hours
 */
function getRecentCommits(hours = 24) {
    try {
        const since = new Date(Date.now() - hours * 60 * 60 * 1000).toISOString();
        const commits = execSync(
            `git log --since="${since}" --pretty=format:"%h|%s|%an" --no-merges`,
            { encoding: 'utf8' }
        );

        if (!commits.trim()) {
            return [];
        }

        return commits.trim().split('\n').map(line => {
            const [hash, message, author] = line.split('|');
            return { hash, message, author };
        }).filter(commit => {
            // Skip automated commits and CI commits
            if (commit.message.includes('[skip ci]')) return false;
            if (commit.message.includes('update social media posting history')) return false;
            if (commit.message.includes('Update dev posting history')) return false;
            if (commit.message.includes('Update philosophy posting history')) return false;
            if (commit.author === 'github-actions[bot]') return false;
            if (commit.author === 'Social Media Bot') return false;
            return true;
        });
    } catch (error) {
        console.error('Error fetching commits:', error.message);
        return [];
    }
}

/**
 * Categorize commit message
 */
function categorizeCommit(message) {
    const msg = message.toLowerCase();

    if (msg.startsWith('feat') || msg.includes('add') || msg.includes('implement')) return 'feature';
    if (msg.startsWith('fix') || msg.includes('bug') || msg.includes('issue')) return 'fix';
    if (msg.startsWith('perf') || msg.includes('optimize') || msg.includes('performance')) return 'performance';
    if (msg.startsWith('docs') || msg.includes('documentation')) return 'docs';
    if (msg.startsWith('style') || msg.includes('ui') || msg.includes('design')) return 'style';
    if (msg.startsWith('refactor')) return 'refactor';
    if (msg.startsWith('test')) return 'test';
    if (msg.startsWith('chore') || msg.includes('update') || msg.includes('bump')) return 'chore';

    return 'other';
}

/**
 * Generate dev update post from commits
 */
function generateDevUpdate(commits) {
    if (commits.length === 0) {
        return null;
    }

    // Filter out chores and focus on meaningful changes
    const meaningfulCommits = commits.filter(c => {
        const category = categorizeCommit(c.message);
        return category !== 'chore' && category !== 'docs';
    });

    if (meaningfulCommits.length === 0) {
        console.log('ℹ️  No meaningful changes to post about');
        return null;
    }

    // Group by category
    const grouped = meaningfulCommits.reduce((acc, commit) => {
        const category = categorizeCommit(commit.message);
        if (!acc[category]) acc[category] = [];
        acc[category].push(commit);
        return acc;
    }, {});

    // Create post content
    let content = '🔧 Dev Update\n\n';

    const categoryEmojis = {
        feature: '✨',
        fix: '🐛',
        performance: '⚡',
        style: '🎨',
        refactor: '♻️',
        test: '✅',
        other: '📝'
    };

    Object.entries(grouped).forEach(([category, commits]) => {
        const emoji = categoryEmojis[category] || '📝';
        content += `${emoji} ${category.charAt(0).toUpperCase() + category.slice(1)}:\n`;
        commits.forEach(commit => {
            const message = commit.message.replace(/^(feat|fix|perf|style|refactor|test|chore|docs)(\(.+?\))?:\s*/i, '');
            content += `• ${message}\n`;
        });
        content += '\n';
    });

    content += `#IndieGameDev #GameDev #Unity`;

    return content;
}

/**
 * Main execution
 */
async function main() {
    console.log('🚀 Dev Update Poster Starting...\n');

    // Get recent commits
    const commits = getRecentCommits(24);

    if (commits.length === 0) {
        console.log('ℹ️  No commits in the last 24 hours');
        return;
    }

    console.log(`📊 Found ${commits.length} commits\n`);

    // Generate post content
    const content = generateDevUpdate(commits);

    if (!content) {
        console.log('✅ No dev update needed');
        return;
    }

    console.log('Generated content:');
    console.log('─'.repeat(50));
    console.log(content);
    console.log('─'.repeat(50));
    console.log();

    // Post to all platforms
    const results = {
        timestamp: new Date().toISOString(),
        type: 'devUpdate',
        content: content,
        platforms: {}
    };

    try {
        // Bluesky
        console.log('📡 Posting to Bluesky...');
        const blueskyResult = await postToBluesky(content);
        results.platforms.bluesky = blueskyResult;
        console.log(`✅ Bluesky: ${blueskyResult.url || 'Posted'}\n`);
    } catch (error) {
        console.error(`❌ Bluesky failed: ${error.message}\n`);
        results.platforms.bluesky = { success: false, error: error.message };
    }

    try {
        // Mastodon
        console.log('📡 Posting to Mastodon...');
        const mastodonResult = await postToMastodon(content);
        results.platforms.mastodon = mastodonResult;
        console.log(`✅ Mastodon: ${mastodonResult.url || 'Posted'}\n`);
    } catch (error) {
        console.error(`❌ Mastodon failed: ${error.message}\n`);
        results.platforms.mastodon = { success: false, error: error.message };
    }

    try {
        // Discord
        console.log('📡 Posting to Discord...');
        const discordAnnouncement = {
            title: '🔧 Dev Update',
            description: content,
            type: 'devUpdate',
            url: results.platforms.bluesky?.url || null
        };
        const discordResult = await postAnnouncementToDiscord(discordAnnouncement);
        results.platforms.discord = discordResult;
        console.log(`✅ Discord: Posted\n`);
    } catch (error) {
        console.error(`❌ Discord failed: ${error.message}\n`);
        results.platforms.discord = { success: false, error: error.message };
    }

    // Update posting history
    let history = { posts: [] };
    if (fs.existsSync(POSTING_HISTORY_PATH)) {
        history = JSON.parse(fs.readFileSync(POSTING_HISTORY_PATH, 'utf8'));
    }

    history.posts.push(results);
    fs.writeFileSync(POSTING_HISTORY_PATH, JSON.stringify(history, null, 2));
    console.log('✅ Posting history updated');

    console.log('\n🎉 Dev update posted successfully!');
}

main().catch(error => {
    console.error('❌ Fatal error:', error);
    process.exit(1);
});
