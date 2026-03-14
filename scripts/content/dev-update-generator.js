/**
 * Dev Update Generator
 * 
 * Parses git commits from past 24 hours and generates user-friendly dev updates.
 * Categorizes changes as features, fixes, systems, polish, docs.
 * Adapted from empowrapp-site's weekly-update-generator.js
 */

import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ═══════════════════════════════════════════════════════════════
// GIT COMMIT PARSING
// ═══════════════════════════════════════════════════════════════

/**
 * Get git commits from past N hours
 * @param {number} hours - Hours to look back (default: 24)
 * @returns {Array<Object>} - Array of commit objects
 */
function getRecentCommits(hours = 24) {
    try {
        // Get commits from past N hours
        const since = `${hours} hours ago`;
        const command = `git log --since="${since}" --pretty=format:"%H|%an|%ae|%ad|%s" --date=iso`;

        const output = execSync(command, {
            encoding: 'utf8',
            cwd: path.join(__dirname, '../..'),
        });

        if (!output.trim()) {
            return [];
        }

        return output.trim().split('\n').map(line => {
            const [hash, author, email, date, message] = line.split('|');
            return { hash, author, email, date, message };
        });
    } catch (error) {
        console.error('❌ Error fetching git commits:', error.message);
        return [];
    }
}

/**
 * Categorize commit by type
 * @param {string} message - Commit message
 * @returns {string} - Category: feature, fix, system, polish, docs, other
 */
function categorizeCommit(message) {
    const msg = message.toLowerCase();

    // Features (new capabilities)
    if (msg.match(/^(feat|feature|add|implement|create|new)/)) {
        return 'feature';
    }

    // Bug fixes
    if (msg.match(/^(fix|bug|hotfix|patch|resolve)/)) {
        return 'fix';
    }

    // Systems/architecture
    if (msg.match(/^(system|refactor|optimize|perf|architecture)/)) {
        return 'system';
    }

    // Polish/improvements
    if (msg.match(/^(polish|improve|enhance|tweak|update)/)) {
        return 'polish';
    }

    // Documentation
    if (msg.match(/^(docs|doc|documentation|readme)/)) {
        return 'docs';
    }

    // Chores/misc
    if (msg.match(/^(chore|build|ci|test|deps)/)) {
        return 'other';
    }

    return 'other';
}

/**
 * Generate human-friendly summary of commit
 * @param {Object} commit - Commit object
 * @returns {string} - User-friendly description
 */
function generateSummary(commit) {
    let summary = commit.message;

    // Remove conventional commit prefixes
    summary = summary.replace(/^(feat|fix|docs|style|refactor|perf|test|chore|build|ci)(\(.+?\))?:\s*/i, '');

    // Capitalize first letter
    summary = summary.charAt(0).toUpperCase() + summary.slice(1);

    // Remove trailing periods
    summary = summary.replace(/\.$/, '');

    return summary;
}

// ═══════════════════════════════════════════════════════════════
// DEV UPDATE GENERATION
// ═══════════════════════════════════════════════════════════════

/**
 * Generate dev update from recent commits
 * @param {Object} options - Generation options
 * @param {number} options.hours - Hours to look back (default: 24)
 * @param {boolean} options.includeMinor - Include docs/chores (default: false)
 * @returns {Object|null} - Dev update object or null if no meaningful changes
 */
export function generateDevUpdate(options = {}) {
    const { hours = 24, includeMinor = false } = options;

    const commits = getRecentCommits(hours);

    if (commits.length === 0) {
        console.log('ℹ️ No commits found in the past', hours, 'hours');
        return null;
    }

    // Categorize commits
    const categorized = {
        feature: [],
        fix: [],
        system: [],
        polish: [],
        docs: [],
        other: [],
    };

    commits.forEach(commit => {
        const category = categorizeCommit(commit.message);
        categorized[category].push({
            ...commit,
            summary: generateSummary(commit),
        });
    });

    // Filter out non-meaningful commits unless includeMinor
    const meaningfulCategories = includeMinor
        ? ['feature', 'fix', 'system', 'polish', 'docs']
        : ['feature', 'fix', 'system', 'polish'];

    const hasMeaningfulChanges = meaningfulCategories.some(
        cat => categorized[cat].length > 0
    );

    if (!hasMeaningfulChanges) {
        console.log('ℹ️ No meaningful changes (only chores/misc)');
        return null;
    }

    // Generate update object
    return {
        type: 'devUpdate',
        timestamp: new Date().toISOString(),
        period: `past ${hours} hours`,
        commitCount: commits.length,
        categories: categorized,
        summary: generateUpdateSummary(categorized),
    };
}

/**
 * Generate human-friendly summary text
 * @param {Object} categorized - Categorized commits
 * @returns {string} - Summary text
 */
function generateUpdateSummary(categorized) {
    const parts = [];

    if (categorized.feature.length > 0) {
        const count = categorized.feature.length;
        const plural = count > 1 ? 'features' : 'feature';
        parts.push(`Shipped ${count} new ${plural}`);
    }

    if (categorized.fix.length > 0) {
        const count = categorized.fix.length;
        const plural = count > 1 ? 'fixes' : 'fix';
        parts.push(`${count} bug ${plural}`);
    }

    if (categorized.system.length > 0) {
        parts.push('system improvements');
    }

    if (categorized.polish.length > 0) {
        parts.push('polish updates');
    }

    if (parts.length === 0) {
        return 'Minor updates';
    }

    if (parts.length === 1) {
        return parts[0];
    }

    if (parts.length === 2) {
        return `${parts[0]} + ${parts[1]}`;
    }

    return parts.slice(0, -1).join(', ') + ', and ' + parts[parts.length - 1];
}

/**
 * Format dev update for social media posting
 * @param {Object} update - Dev update object from generateDevUpdate()
 * @param {string} platform - Target platform (bluesky, mastodon, discord)
 * @returns {Object} - Formatted content with title and body
 */
export function formatDevUpdateForPlatform(update, platform = 'bluesky') {
    if (!update) return null;

    const { categories, summary } = update;

    // Build content sections
    const sections = [];

    // Header
    sections.push(`🚀 Dev Update\n`);

    // Features
    if (categories.feature.length > 0) {
        sections.push('✨ New Features:');
        categories.feature.forEach(commit => {
            sections.push(`• ${commit.summary}`);
        });
        sections.push('');
    }

    // Fixes
    if (categories.fix.length > 0) {
        sections.push('🔧 Bug Fixes:');
        categories.fix.forEach(commit => {
            sections.push(`• ${commit.summary}`);
        });
        sections.push('');
    }

    // Systems
    if (categories.system.length > 0) {
        sections.push('⚙️ System Improvements:');
        categories.system.forEach(commit => {
            sections.push(`• ${commit.summary}`);
        });
        sections.push('');
    }

    // Polish
    if (categories.polish.length > 0) {
        sections.push('✨ Polish:');
        categories.polish.forEach(commit => {
            sections.push(`• ${commit.summary}`);
        });
        sections.push('');
    }

    const content = sections.join('\n').trim();

    return {
        title: `Dev Update: ${summary}`,
        body: content,
        type: 'devUpdate',
    };
}

// ═══════════════════════════════════════════════════════════════
// CLI INTERFACE (for testing)
// ═══════════════════════════════════════════════════════════════

if (import.meta.url === `file://${process.argv[1]}`) {
    console.log('🔍 Generating dev update from recent commits...\n');

    const update = generateDevUpdate({ hours: 24, includeMinor: false });

    if (update) {
        console.log('📊 Commit Analysis:');
        console.log(`   Total commits: ${update.commitCount}`);
        console.log(`   Features: ${update.categories.feature.length}`);
        console.log(`   Fixes: ${update.categories.fix.length}`);
        console.log(`   Systems: ${update.categories.system.length}`);
        console.log(`   Polish: ${update.categories.polish.length}\n`);

        console.log('📝 Generated Summary:');
        console.log(`   "${update.summary}"\n`);

        const formatted = formatDevUpdateForPlatform(update, 'bluesky');
        if (formatted) {
            console.log('✅ Formatted for Social Media:\n');
            console.log(formatted.body);
        }
    } else {
        console.log('ℹ️ No meaningful changes to report');
    }
}

export default {
    generateDevUpdate,
    formatDevUpdateForPlatform,
    getRecentCommits,
};
