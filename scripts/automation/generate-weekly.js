/**
 * Weekly Digest Generator
 *
 * Generates weekly summary posts that link to all daily updates:
 * - Aggregates past 7 days of daily updates
 * - Creates comprehensive weekly summary (only if meaningful activity exists)
 * - Includes links to all daily blog posts
 * - Highlights biggest features and improvements
 *
 * Usage:
 *   node scripts/automation/generate-weekly.js              # Generate for last completed week
 *   node scripts/automation/generate-weekly.js --date=2026-04-07  # Generate for week containing date
 *   node scripts/automation/generate-weekly.js --dry-run    # Preview without writing files
 *   node scripts/automation/generate-weekly.js --force      # Generate even if no meaningful activity
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
const isForce = args.includes('--force'); // override: generate even if no meaningful activity

/**
 * Get Monday of the week containing the given date
 */
function getWeekStart(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Monday
    return new Date(d.setDate(diff));
}

/**
 * Get Sunday of the week containing the given date
 */
function getWeekEnd(date) {
    const weekStart = getWeekStart(date);
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekStart.getDate() + 6);
    return weekEnd;
}

/**
 * Load data.json
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
 */
function getDailyUpdatesForWeek(weekStart) {
    const data = loadData();
    const weekEnd = getWeekEnd(weekStart);

    const weekStartStr = weekStart.toISOString().split('T')[0];
    const weekEndStr = weekEnd.toISOString().split('T')[0];

    return data.dailyUpdates
        .filter(update => update.date >= weekStartStr && update.date <= weekEndStr)
        .sort((a, b) => new Date(a.date) - new Date(b.date));
}

/**
 * Aggregate daily updates into a weekly digest object
 */
function generateWeeklyDigest(dailyUpdates, weekStart) {
    const weekEnd = getWeekEnd(weekStart);
    const weekStartStr = weekStart.toISOString().split('T')[0];

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

        if (update.highlights.features) allFeatures.push(...update.highlights.features);
        if (update.highlights.fixes) allFixes.push(...update.highlights.fixes);
        if (update.highlights.improvements) allImprovements.push(...update.highlights.improvements);
    });

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
        categories: { features: totalFeatures, fixes: totalFixes, improvements: totalImprovements, docs: totalDocs },
        highlights,
        dailyPosts: dailyUpdates.map(u => ({ date: u.date, url: u.blogPostUrl, commits: u.totalCommits })),
        blogPostUrl: null
    };
}

/**
 * Generate blog post HTML for weekly digest
 */
function generateWeeklyBlogPostHTML(digest) {
    const weekStartObj = new Date(digest.weekStart);
    const weekEndObj = new Date(digest.weekEnd);

    const longFmt = { month: 'long', day: 'numeric', year: 'numeric' };
    const shortFmt = { month: 'short', day: 'numeric' };

    const formattedStart = weekStartObj.toLocaleDateString('en-US', longFmt);
    const formattedEnd = weekEndObj.toLocaleDateString('en-US', longFmt);
    const shortStart = weekStartObj.toLocaleDateString('en-US', shortFmt);
    const shortEnd = weekEndObj.toLocaleDateString('en-US', shortFmt);

    const slug = `weekly-digest-${digest.weekStart}`;
    const titleStr = `Weekly Dev Update - ${shortStart} to ${shortEnd}`;

    // Plain-English summary sentence
    const parts = [];
    if (digest.categories.features > 0) parts.push(`${digest.categories.features} new feature${digest.categories.features !== 1 ? 's' : ''}`);
    if (digest.categories.fixes > 0) parts.push(`${digest.categories.fixes} fix${digest.categories.fixes !== 1 ? 'es' : ''}`);
    if (digest.categories.improvements > 0) parts.push(`${digest.categories.improvements} improvement${digest.categories.improvements !== 1 ? 's' : ''}`);
    const summary = parts.length > 0
        ? `This week we shipped ${parts.join(', ')}.`
        : 'Development work continued this week.';

    // Section HTML helpers
    function listSection(heading, items) {
        if (items.length === 0) return '';
        const lis = items.map(f => `            <li>${f}</li>`).join('\n');
        return `\n        <h2>${heading}</h2>\n        <ul>\n${lis}\n        </ul>\n`;
    }

    const featuresSection = listSection("What's New", digest.highlights.features);
    const fixesSection = listSection('Bug Fixes', digest.highlights.fixes);
    const improvementsSection = listSection('Improvements', digest.highlights.improvements);

    const linkedPosts = digest.dailyPosts.filter(p => p.url);
    const dailyPostsSection = linkedPosts.length > 0
        ? '\n        <h2>Day-by-Day</h2>\n        <ul>\n' +
        linkedPosts.map(post => {
            const d = new Date(post.date);
            const label = d.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });
            return `            <li><a href="${post.url}">${label}</a></li>`;
        }).join('\n') +
        '\n        </ul>\n'
        : '';

    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>${titleStr} | The Ascendant Continuum</title>
    <meta name="description" content="${summary}">
    <meta property="og:title" content="${titleStr}">
    <meta property="og:description" content="${summary}">
    <meta property="og:type" content="article">
    <link rel="stylesheet" href="../styles.css">
</head>
<body>
    <article class="blog-post">
        <header class="post-header">
            <div class="post-category">Weekly Update</div>
            <h1>${titleStr}</h1>
            <div class="post-meta">
                <time datetime="${digest.weekStart}">${formattedStart} - ${formattedEnd}</time>
            </div>
        </header>
        <div class="post-content">
            <p class="lead">${summary}</p>
${featuresSection}${fixesSection}${improvementsSection}${dailyPostsSection}
            <div class="post-footer">
                <p><a href="/whats-new/">Back to What's New</a></p>
                <p><a href="/blog/">View All Blog Posts</a></p>
            </div>
        </div>
    </article>
</body>
</html>`;
}

/**
 * Save weekly digest to data.json
 */
function saveWeeklyDigest(digest) {
    const data = loadData();

    if (!data.weeklyDigests) {
        data.weeklyDigests = [];
    }

    // Remove existing digest for this week if it exists
    data.weeklyDigests = data.weeklyDigests.filter(d => d.weekStart !== digest.weekStart);
    data.weeklyDigests.push(digest);
    data.weeklyDigests.sort((a, b) => new Date(b.weekStart) - new Date(a.weekStart));

    data.lastUpdated = new Date().toISOString();
    data.meta.totalWeeklyDigests = data.weeklyDigests.length;

    if (!dryRun) {
        fs.writeFileSync(DATA_FILE, JSON.stringify(data, null, 2), 'utf-8');
        console.log(`\u2705 Saved digest to ${DATA_FILE}`);
    } else {
        console.log('\uD83D\uDD0D DRY RUN: Would save to', DATA_FILE);
    }
}

/**
 * Main execution
 */
async function main() {
    console.log('\uD83D\uDCCA Weekly Digest Generator\n');
    console.log('\u2550'.repeat(70));

    // Determine target week
    let targetDate;
    if (dateArg) {
        targetDate = new Date(dateArg.split('=')[1]);
    } else {
        // Last completed week (previous Monday to Sunday)
        const today = new Date();
        const lastMonday = getWeekStart(today);
        lastMonday.setDate(lastMonday.getDate() - 7);
        targetDate = lastMonday;
    }

    const weekStart = getWeekStart(targetDate);
    const weekEnd = getWeekEnd(targetDate);

    console.log('\n\uD83D\uDCC5 Generating weekly digest for:');
    console.log(`   Week: ${weekStart.toISOString().split('T')[0]} to ${weekEnd.toISOString().split('T')[0]}\n`);

    // Get daily updates for this week
    const dailyUpdates = getDailyUpdatesForWeek(weekStart);

    if (dailyUpdates.length === 0) {
        console.log('\u2139\uFE0F No daily updates found for this week. Run generate-whatsnew.js first.');
        return;
    }

    console.log(`Found ${dailyUpdates.length} daily updates for this week\n`);

    // ── Meaningful-activity gate ──────────────────────────────────────────────
    // A weekly blog post is only worthwhile when real, user-facing work happened.
    // Each daily entry written by generate-whatsnew.js carries a `meaningful`
    // flag.  Fall back to checking category totals for legacy entries.
    const meaningfulDays = dailyUpdates.filter(u => {
        if (typeof u.meaningful === 'boolean') return u.meaningful;
        const cats = u.categories || {};
        return (cats.features || 0) + (cats.fixes || 0) + (cats.improvements || 0) > 0;
    });

    if (meaningfulDays.length === 0 && !isForce) {
        console.log('\u2139\uFE0F  No meaningful user-facing activity this week.');
        console.log('   Skipping weekly blog post. (Use --force to override)');
        return;
    }

    if (meaningfulDays.length === 0 && isForce) {
        console.log('\u26A0\uFE0F  No meaningful activity, but --force is set. Generating anyway.');
    } else {
        console.log(`\u2705 ${meaningfulDays.length} day(s) with meaningful changes - generating weekly post\n`);
    }
    // ─────────────────────────────────────────────────────────────────────────

    // Generate weekly digest
    const digest = generateWeeklyDigest(dailyUpdates, weekStart);

    // Generate blog post
    const blogHTML = generateWeeklyBlogPostHTML(digest);
    const blogSlug = `weekly-digest-${digest.weekStart}`;
    const blogPath = path.join(BLOG_POSTS_DIR, `${blogSlug}.html`);

    if (!dryRun) {
        fs.writeFileSync(blogPath, blogHTML, 'utf-8');
        digest.blogPostUrl = `/blog/posts/${blogSlug}.html`;
        console.log(`\u2705 Created blog post: ${blogSlug}.html`);
    }

    // Save to data.json
    saveWeeklyDigest(digest);

    // Display summary
    console.log('\n\uD83D\uDCCA Weekly Digest Summary:');
    console.log(`   Week: ${digest.weekStart} to ${digest.weekEnd}`);
    console.log(`   Days with updates: ${digest.daysWithUpdates}`);
    console.log(`   Total commits: ${digest.totalCommits}`);
    console.log(`   Features: ${digest.categories.features}`);
    console.log(`   Fixes: ${digest.categories.fixes}`);
    console.log(`   Improvements: ${digest.categories.improvements}`);
    console.log(`   Blog post: ${digest.blogPostUrl || 'N/A'}`);

    console.log('\n\u2705 Weekly digest generated successfully\n');
}

// Run if called directly
if (import.meta.url.endsWith(path.basename(process.argv[1]))) {
    main().catch(error => {
        console.error('\u274C Error:', error);
        process.exit(1);
    });
}

export { generateWeeklyDigest, getWeekStart, getWeekEnd };
