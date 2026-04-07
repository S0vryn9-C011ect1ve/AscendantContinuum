/**
 * Weekly Digest Generator
 * 
 * Generates weekly summary posts that link to all daily updates:
 * - Aggregates past 7 days of daily updates
 * - Creates comprehensive weekly summary
 * - Includes links to all daily blog posts
 * - Highlights biggest features and improvements
 * 
 * Usage:
 *   node scripts/automation/generate-weekly.js              # Generate for last completed week
 *   node scripts/automation/generate-weekly.js --date=2026-04-07  # Generate for week containing this date
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const WHATS_NEW_DIR = path.join(__dirname, '..', '..', 'firebase', 'public', 'whats-new');
const DATA_FILE = path.join(WHATS_NEW_DIR, 'data.json');
const BLOG_POSTS_DIR = path.join(__dirname, '..', '..', 'firebase', 'public', 'blog', 'posts');

const args = process.argv.slice(2);
const dateArg = args.find(arg => arg.startsWith('--date='));
const dryRun = args.includes('--dry-run');

/**
 * Get Monday of the week containing the given date
 * @param {Date} date - Date object
 * @returns {Date} - Monday of that week
 */
function getWeekStart(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Monday
    return new Date(d.setDate(diff));
}

/**
 * Get Sunday of the week containing the given date
 * @param {Date} date - Date object
 * @returns {Date} - Sunday of that week
 */
function getWeekEnd(date) {
    const weekStart = getWeekStart(date);
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekStart.getDate() + 6);
    return weekEnd;
}

/**
 * Load daily updates from data.json
 * @returns {Object} - Data object with dailyUpdates array
 */
function loadData() {
    if (!fs.existsSync(DATA_FILE)) {
        console.error('❌ Error: data.json not found. Run generate-whatsnew.js first.');
        process.exit(1);
    }

    return JSON.parse(fs.readFileSync(DATA_FILE, 'utf-8'));
}

/**
 * Get daily updates for a specific week
 * @param {Date} weekStart - Monday of the week
 * @returns {Array} - Array of daily update objects
 */
function getDailyUpdatesForWeek(weekStart) {
    const data = loadData();
    const weekEnd = getWeekEnd(weekStart);

    const weekStartStr = weekStart.toISOString().split('T')[0];
    const weekEndStr = weekEnd.toISOString().split('T')[0];

    return data.dailyUpdates.filter(update => {
        return update.date >= weekStartStr && update.date <= weekEndStr;
    }).sort((a, b) => new Date(a.date) - new Date(b.date));
}

/**
 * Generate weekly digest from daily updates
 * @param {Array} dailyUpdates - Array of daily update objects
 * @param {Date} weekStart - Monday of the week
 * @returns {Object} - Weekly digest object
 */
function generateWeeklyDigest(dailyUpdates, weekStart) {
    const weekEnd = getWeekEnd(weekStart);
    const weekStartStr = weekStart.toISOString().split('T')[0];

    // Aggregate stats
    let totalCommits = 0;
    let totalFeatures = 0;
    let totalFixes = 0;
    let totalImprovements = 0;
    let totalDocs = 0;

    const allFeatures = [];
    const allFixes = [];
    const allImprovements = [];

    dailyUpdates.forEach(update => {
        totalCommits += update.totalCommits || 0;
        totalFeatures += update.categories.features || 0;
        totalFixes += update.categories.fixes || 0;
        totalImprovements += update.categories.improvements || 0;
        totalDocs += update.categories.docs || 0;

        if (update.highlights.features) {
            allFeatures.push(...update.highlights.features);
        }
        if (update.highlights.fixes) {
            allFixes.push(...update.highlights.fixes);
        }
        if (update.highlights.improvements) {
            allImprovements.push(...update.highlights.improvements);
        }
    });

    // Select highlights (top items from each category)
    const highlights = {
        features: allFeatures.slice(0, 8),
        fixes: allFixes.slice(0, 5),
        improvements: allImprovements.slice(0, 5)
    };

    return {
        weekStart: weekStartStr,
        weekEnd: weekEnd.toISOString().split('T')[0],
        daysWithUpdates: dailyUpdates.length,
        totalCommits,
        categories: {
            features: totalFeatures,
            fixes: totalFixes,
            improvements: totalImprovements,
            docs: totalDocs
        },
        highlights,
        dailyPosts: dailyUpdates.map(u => ({
            date: u.date,
            url: u.blogPostUrl,
            commits: u.totalCommits
        })),
        blogPostUrl: null // Will be set when blog post is created
    };
}

/**
 * Generate blog post HTML for weekly digest
 * @param {Object} digest - Weekly digest object
 * @returns {string} - HTML content
 */
function generateWeeklyBlogPostHTML(digest) {
    const weekStartObj = new Date(digest.weekStart);
    const weekEndObj = new Date(digest.weekEnd);

    const dateFormat = { month: 'long', day: 'numeric', year: 'numeric' };
    const formattedStart = weekStartObj.toLocaleDateString('en-US', dateFormat);
    const formattedEnd = weekEndObj.toLocaleDateString('en-US', dateFormat);

    const slug = `weekly-digest-${digest.weekStart}`;

    const summary = `Weekly development digest: ${digest.totalCommits} commits across ${digest.daysWithUpdates} days with ${digest.categories.features} features, ${digest.categories.fixes} fixes, and ${digest.categories.improvements} improvements.`;

    const featuresSection = digest.highlights.features.length > 0 ? `
            <h2>✨ Features This Week</h2>
            <p>We shipped ${digest.categories.features} new feature${digest.categories.features !== 1 ? 's' : ''} this week. Highlights:</p>
            <ul>
                ${digest.highlights.features.map(f => `<li>${f}</li>`).join('\n                ')}
            </ul>
    ` : '';

    const fixesSection = digest.highlights.fixes.length > 0 ? `
            <h2>🐛 Bug Fixes</h2>
            <p>Squashed ${digest.categories.fixes} bug${digest.categories.fixes !== 1 ? 's' : ''} to improve stability. Key fixes:</p>
            <ul>
                ${digest.highlights.fixes.map(f => `<li>${f}</li>`).join('\n                ')}
            </ul>
    ` : '';

    const improvementsSection = digest.highlights.improvements.length > 0 ? `
            <h2>⚡ Performance & Improvements</h2>
            <p>Made ${digest.categories.improvements} optimization${digest.categories.improvements !== 1 ? 's' : ''} and improvements:</p>
            <ul>
                ${digest.highlights.improvements.map(f => `<li>${f}</li>`).join('\n                ')}
            </ul>
    ` : '';

    const dailyPostsSection = digest.dailyPosts.length > 0 ? `
            <h2>📅 Daily Updates</h2>
            <p>Catch up on each day's progress:</p>
            <ul>
                ${digest.dailyPosts.map(post => {
        const postDate = new Date(post.date);
        const dayName = postDate.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });
        return `<li><strong>${dayName}</strong> - ${post.commits} commit${post.commits !== 1 ? 's' : ''} ${post.url ? `(<a href="${post.url}">Read update</a>)` : ''}}</li>`;
    }).join('\n                ')}
            </ul>
    ` : '';

    return `<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Weekly Digest: ${formattedStart} - ${formattedEnd} | The Ascendant Continuum</title>
    <meta name="description" content="${summary}">
    <meta property="og:title" content="Weekly Digest: ${formattedStart} - ${formattedEnd}">
    <meta property="og:description" content="${summary}">
    <meta property="og:type" content="article">
    <link rel="stylesheet" href="../styles.css">
</head>

<body>
    <article class="blog-post">
        <header class="post-header">
            <div class="post-category">Weekly Digest</div>
            <h1>Development Week: ${formattedStart} - ${formattedEnd}</h1>
            <div class="post-meta">
                <time datetime="${digest.weekStart}">Week of ${formattedStart}</time>
                <span class="post-stats">${digest.totalCommits} commits across ${digest.daysWithUpdates} days</span>
            </div>
        </header>

        <div class="post-content">
            <p class="lead">${summary}</p>

            <div class="stats-summary">
                <h2>📊 Week at a Glance</h2>
                <ul>
                    <li><strong>Total commits:</strong> ${digest.totalCommits}</li>
                    <li><strong>Days with updates:</strong> ${digest.daysWithUpdates} of 7</li>
                    <li><strong>New features:</strong> ${digest.categories.features}</li>
                    <li><strong>Bug fixes:</strong> ${digest.categories.fixes}</li>
                    <li><strong>Improvements:</strong> ${digest.categories.improvements}</li>
                    ${digest.categories.docs > 0 ? `<li><strong>Documentation updates:</strong> ${digest.categories.docs}</li>` : ''}
                </ul>
            </div>

${featuresSection}
${fixesSection}
${improvementsSection}
${dailyPostsSection}

            <div class="post-footer">
                <p><a href="/whats-new/">← Back to What's New</a></p>
                <p><a href="/blog/">View All Blog Posts →</a></p>
            </div>
        </div>
    </article>
</body>

</html>`;
}

/**
 * Save weekly digest to data.json
 * @param {Object} digest - Weekly digest object
 */
function saveWeeklyDigest(digest) {
    const data = loadData();

    if (!data.weeklyDigests) {
        data.weeklyDigests = [];
    }

    // Remove existing digest for this week if it exists
    data.weeklyDigests = data.weeklyDigests.filter(d => d.weekStart !== digest.weekStart);

    // Add new digest
    data.weeklyDigests.push(digest);

    // Sort by week start descending
    data.weeklyDigests.sort((a, b) => new Date(b.weekStart) - new Date(a.weekStart));

    // Update metadata
    data.lastUpdated = new Date().toISOString();
    data.meta.totalWeeklyDigests = data.weeklyDigests.length;

    if (!dryRun) {
        fs.writeFileSync(DATA_FILE, JSON.stringify(data, null, 2), 'utf-8');
        console.log(`✅ Saved digest to ${DATA_FILE}`);
    } else {
        console.log('🔍 DRY RUN: Would save to', DATA_FILE);
    }
}

/**
 * Main execution
 */
async function main() {
    console.log('📊 Weekly Digest Generator\n');
    console.log('═'.repeat(70));

    // Determine target week
    let targetDate;
    if (dateArg) {
        targetDate = new Date(dateArg.split('=')[1]);
    } else {
        // Last completed week (previous Monday to Sunday)
        const today = new Date();
        const lastMonday = getWeekStart(today);
        lastMonday.setDate(lastMonday.getDate() - 7); // Go back one week
        targetDate = lastMonday;
    }

    const weekStart = getWeekStart(targetDate);
    const weekEnd = getWeekEnd(targetDate);

    console.log(`\n📅 Generating weekly digest for:`);
    console.log(`   Week: ${weekStart.toISOString().split('T')[0]} to ${weekEnd.toISOString().split('T')[0]}\n`);

    // Get daily updates for this week
    const dailyUpdates = getDailyUpdatesForWeek(weekStart);

    if (dailyUpdates.length === 0) {
        console.log('ℹ️ No daily updates found for this week. Run generate-whatsnew.js first.');
        return;
    }

    console.log(`Found ${dailyUpdates.length} daily updates for this week\n`);

    // Generate weekly digest
    const digest = generateWeeklyDigest(dailyUpdates, weekStart);

    // Generate blog post
    const blogHTML = generateWeeklyBlogPostHTML(digest);
    const blogSlug = `weekly-digest-${digest.weekStart}`;
    const blogPath = path.join(BLOG_POSTS_DIR, `${blogSlug}.html`);

    if (!dryRun) {
        fs.writeFileSync(blogPath, blogHTML, 'utf-8');
        digest.blogPostUrl = `/blog/posts/${blogSlug}.html`;
        console.log(`✅ Created blog post: ${blogSlug}.html`);
    }

    // Save to data.json
    saveWeeklyDigest(digest);

    // Display summary
    console.log('\n📊 Weekly Digest Summary:');
    console.log(`   Week: ${digest.weekStart} to ${digest.weekEnd}`);
    console.log(`   Days with updates: ${digest.daysWithUpdates}`);
    console.log(`   Total commits: ${digest.totalCommits}`);
    console.log(`   Features: ${digest.categories.features}`);
    console.log(`   Fixes: ${digest.categories.fixes}`);
    console.log(`   Improvements: ${digest.categories.improvements}`);
    console.log(`   Blog post: ${digest.blogPostUrl || 'N/A'}`);

    console.log('\n✅ Weekly digest generated successfully\n');
}

// Run if called directly
if (import.meta.url === `file://${process.argv[1]}`) {
    main().catch(error => {
        console.error('❌ Error:', error);
        process.exit(1);
    });
}

export { generateWeeklyDigest, getWeekStart, getWeekEnd };
