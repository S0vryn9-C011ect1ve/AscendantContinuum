/**
 * What's New Daily Update Generator
 * 
 * Generates daily changelog updates from git commits:
 * - Analyzes commits from the past 24 hours (or custom range)
 * - Categorizes changes (features, fixes, improvements, docs)
 * - Creates daily blog post with detailed writeup
 * - Updates whats-new/data.json with summary
 * - Can generate historical data from all commits
 * 
 * Usage:
 *   node scripts/automation/generate-whatsnew.js                    # Daily update (last 24h)
 *   node scripts/automation/generate-whatsnew.js --days=7           # Last 7 days
 *   node scripts/automation/generate-whatsnew.js --historical       # All history (grouped by day)
 *   node scripts/automation/generate-whatsnew.js --date=2026-03-15  # Specific date
 */

import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const WHATS_NEW_DIR = path.join(__dirname, '..', '..', 'firebase', 'public', 'whats-new');
const DATA_FILE = path.join(WHATS_NEW_DIR, 'data.json');
const BLOG_POSTS_DIR = path.join(__dirname, '..', '..', 'firebase', 'public', 'blog', 'posts');

// Parse command line arguments
const args = process.argv.slice(2);
const isHistorical = args.includes('--historical');
const daysArg = args.find(arg => arg.startsWith('--days='));
const dateArg = args.find(arg => arg.startsWith('--date='));
const dryRun = args.includes('--dry-run');
const isForce = args.includes('--force'); // override: generate even if no meaningful changes

// ── File-path based impact classification ────────────────────────────────────
// If a commit ONLY touches these paths it is internal and should not appear publicly
const INTERNAL_ONLY_PATHS = [
    /^\.github\//,
    /^scripts\/automation\//,
    /^scripts\/posting\//,
    /^scripts\/analytics\//,
    /^scripts\/validation\//,
    /^scripts\/audit\//,
    /^scripts\/content\//,
    /^scripts\/security\//,
    /^package(-lock)?\.json$/,
    /^node_modules\//,
    /^\.gitignore$/,
    /^\.(eslint|prettier|babel|jest).*/,
];

// Commit message patterns that are ALWAYS internal — skip regardless of files
const SKIP_MSG_PATTERNS = [
    /\[skip ci\]/i,
    /github-actions\[bot\]/i,
    /^(chore|ci|build|test|style|wip)[\ :]/i,
    /npm audit/i,
    /update.*depend/i,
    /bump.*version/i,
    /posting history/i,
    /daily what'?s new/i,
    /^📊/,
];

// Technical terms → plain English replacements for public-facing text
const TECH_TO_PLAIN = [
    [/firebase analytics/gi, 'website tracking'],
    [/crashlytics/gi, 'crash reporting'],
    [/firebase(?!\s+hosting)/gi, 'cloud services'],
    [/websocket/gi, 'live connection'],
    [/webgl/gi, 'browser version'],
    [/\bURP\b/g, 'graphics system'],
    [/render pipeline/gi, 'graphics engine'],
    [/\bshader(s)?\b/gi, 'visual effect$1'],
    [/\bprefab(s)?\b/gi, 'game object$1'],
    [/scriptable object(s)?/gi, 'game setting$1'],
    [/\bcoroutine(s)?\b/gi, 'background process$1'],
    [/null reference/gi, 'technical error'],
    [/\bdependency\b/gi, 'software component'],
    [/\bnpm\b/g, 'software'],
    [/\bgit\b/gi, 'code history'],
    [/\bworkflow(s)?\b/gi, 'automation$1'],
    [/\byaml\b/gi, 'configuration'],
    [/\bjson\b/gi, 'data'],
    [/\bhtml\b/gi, 'page'],
    [/\bcss\b/gi, 'styling'],
    [/\bapi\b/gi, 'service'],
    [/\bsdk\b/gi, 'toolkit'],
    [/\bUX\b/g, 'experience'],
    [/\bUI\b/g, 'interface'],
    [/measurementId/gi, 'tracking ID'],
    [/analytics toggle/gi, 'privacy setting'],
    [/dedup(-?checker)?/gi, 'duplicate check'],
    [/content.?bank/gi, 'content library'],
    [/\bbackfill\b/gi, 'historical data'],
];

/**
 * Replace technical terms with plain English equivalents
 */
function toPlainEnglish(text) {
    let result = text;
    for (const [pattern, replacement] of TECH_TO_PLAIN) {
        result = result.replace(pattern, replacement);
    }
    return result;
}

/**
 * Returns the list of files changed in a commit
 */
function getCommitFiles(hash) {
    try {
        return execSync(
            `git show --name-only --format="" ${hash}`,
            { encoding: 'utf-8' }
        ).trim().split('\n').filter(Boolean);
    } catch (_) {
        return [];
    }
}

/**
 * Returns true if at least one changed file is user-facing
 * (i.e. not ALL files are in internal-only paths)
 */
function hasUserFacingFiles(files) {
    if (files.length === 0) return false;
    return !files.every(f => INTERNAL_ONLY_PATHS.some(p => p.test(f)));
}

/**
 * Returns true if this commit should appear in the public What's New feed
 */
function isUserImpacting(commit) {
    // Always skip automated/bot commits
    if (commit.author === 'github-actions[bot]') return false;
    if (SKIP_MSG_PATTERNS.some(p => p.test(commit.message))) return false;

    // Check what files were actually changed
    const files = getCommitFiles(commit.hash);
    return hasUserFacingFiles(files);
}

/**
 * Build a 2–4 sentence plain-English summary from categorised commit descriptions
 */
function generatePublicSummary(features, fixes, improvements) {
    const sentences = [];

    if (features.length > 0) {
        const desc = features.slice(0, 2).map(toPlainEnglish).join(' and ');
        sentences.push('We added ' + desc);
    }
    if (fixes.length > 0) {
        const desc = fixes.slice(0, 2).map(toPlainEnglish).join(' and ');
        sentences.push('We fixed ' + desc);
    }
    if (improvements.length > 0) {
        const desc = improvements.slice(0, 2).map(toPlainEnglish).join(' and ');
        sentences.push('We improved ' + desc);
    }

    if (sentences.length === 0) return '';

    return sentences
        .map(s => {
            s = s.trim();
            if (!s.match(/[.!?]$/)) s += '.';
            return s.charAt(0).toUpperCase() + s.slice(1);
        })
        .join(' ');
}

/**
 * Get commits for a specific date range
 * @param {string} since - ISO date string (YYYY-MM-DD)
 * @param {string} until - ISO date string (YYYY-MM-DD)
 * @returns {Array} - Array of commit objects
 */
function getCommitsByDateRange(since, until) {
    try {
        const sinceDate = new Date(since);
        sinceDate.setHours(0, 0, 0, 0);
        const untilDate = new Date(until);
        untilDate.setHours(23, 59, 59, 999);

        const cmd = `git log --since="${sinceDate.toISOString()}" --until="${untilDate.toISOString()}" --pretty=format:"%H|||%an|||%ae|||%ad|||%s|||%b" --date=iso`;
        const output = execSync(cmd, { encoding: 'utf-8' }).trim();

        if (!output) return [];

        return output.split('\n\n').map(entry => {
            const parts = entry.split('|||');
            if (parts.length < 6) return null;
            const [hash, author, email, date, subject, body] = parts;
            return {
                hash: hash.trim(),
                author: author.trim(),
                email: email.trim(),
                date: date.trim(),
                subject: subject.trim(),
                body: body ? body.trim() : '',
                message: subject.trim()
            };
        }).filter(Boolean);
    } catch (error) {
        console.error('Error getting commits:', error.message);
        return [];
    }
}

/**
 * Get commits from the last N days
 * @param {number} days - Number of days to look back
 * @returns {Array} - Array of commit objects
 */
function getRecentCommits(days = 1) {
    const now = new Date();
    const until = now.toISOString().split('T')[0];
    const since = new Date(now);
    since.setDate(since.getDate() - days);
    const sinceDate = since.toISOString().split('T')[0];

    return getCommitsByDateRange(sinceDate, until);
}

/**
 * Get all commits in history, grouped by day
 * @returns {Object} - Object with dates as keys, commit arrays as values
 */
function getAllCommitsByDay() {
    try {
        const cmd = `git log --pretty=format:"%H|||%an|||%ae|||%ad|||%s|||%b" --date=short`;
        const output = execSync(cmd, { encoding: 'utf-8' }).trim();

        if (!output) return {};

        const commitsByDay = {};

        output.split('\n\n').forEach(entry => {
            const parts = entry.split('|||');
            if (parts.length < 6) return;
            const [hash, author, email, date, subject, body] = parts;
            const dateKey = date.trim();

            if (!commitsByDay[dateKey]) {
                commitsByDay[dateKey] = [];
            }

            commitsByDay[dateKey].push({
                hash: hash.trim(),
                author: author.trim(),
                email: email.trim(),
                date: dateKey,
                subject: subject.trim(),
                body: body ? body.trim() : '',
                message: subject.trim()
            });
        });

        return commitsByDay;
    } catch (error) {
        console.error('Error getting all commits:', error.message);
        return {};
    }
}

/**
 * Categorize commit by message
 * @param {string} message - Commit message
 * @returns {string} - Category: 'features', 'fixes', 'improvements', 'docs', 'other'
 */
function categorizeCommit(message) {
    const lower = message.toLowerCase();

    // Features
    if (lower.match(/^(feat|feature|add|added|new|implement|create)[\s:]/)) {
        return 'features';
    }

    // Fixes
    if (lower.match(/^(fix|fixed|bug|bugfix|resolve|resolved|patch)[\s:]/)) {
        return 'fixes';
    }

    // Improvements
    if (lower.match(/^(improve|improved|enhancement|enhance|update|updated|refactor|optimize|optimized|perf|performance)[\s:]/)) {
        return 'improvements';
    }

    // Documentation
    if (lower.match(/^(docs?|documentation|readme|comment)[\s:]/)) {
        return 'docs';
    }

    // Automation/CI
    if (lower.match(/^(ci|workflow|automat|deploy|build)[\s:]/)) {
        return 'automation';
    }

    return 'other';
}

/**
 * Clean commit message for display - extracts meaningful details from subject and body
 * @param {Object} commit - Commit object with subject and body
 * @returns {string} - Cleaned, detailed message
 */
function cleanMessage(commit) {
    let message = commit.subject || commit.message;
    const body = commit.body || '';

    // Remove conventional commit prefix
    message = message.replace(/^(feat|feature|fix|bug|docs?|chore|refactor|perf|test|ci|build|improve|add|update|create|implement)[\s:]+/i, '');

    // If there's a body, extract ALL meaningful content
    if (body) {
        const lines = body.split('\n').map(l => l.trim()).filter(l => l);
        const bullets = lines.filter(l => l.match(/^[-*]\s+/));

        if (bullets.length > 0) {
            // Get ALL bullet points for detailed descriptions
            const details = bullets.map(b => b.replace(/^[-*]\s+/, '').trim());
            message = message + ': ' + details.join('; ');
        } else {
            // Extract first paragraph from body (non-empty lines joined)
            const paragraph = lines.filter(l => !l.match(/^(feat|fix|add|update|improve|create)/i)).slice(0, 4).join(' ');
            if (paragraph.length > 15) {
                message = message + ' - ' + paragraph;
            }
        }
    }

    // Capitalize first letter
    message = message.charAt(0).toUpperCase() + message.slice(1);

    // Ensure it ends with period if it doesn't have punctuation
    if (!message.match(/[.!?]$/)) {
        message += '.';
    }

    // Increased character limit for richer details
    if (message.length > 400) {
        message = message.substring(0, 397) + '...';
    }

    return message;
}

/**
 * Generate daily update object from commits
 * @param {Array} commits - Array of commit objects
 * @param {string} date - ISO date string (YYYY-MM-DD)
 * @returns {Object} - Daily update object
 */
function generateDailyUpdate(commits, date) {
    // Separate user-impacting commits from internal ones
    const meaningfulCommits = isForce ? commits : commits.filter(isUserImpacting);

    const categories = {
        features: [],
        fixes: [],
        improvements: [],
        docs: [],
        automation: [],
        other: []
    };

    const categoryCounts = {
        features: 0,
        fixes: 0,
        improvements: 0,
        docs: 0
    };

    // Categorise ONLY meaningful commits
    meaningfulCommits.forEach(commit => {
        const category = categorizeCommit(commit.message);
        const cleaned = cleanMessage(commit);
        categories[category].push(cleaned);
        if (categoryCounts.hasOwnProperty(category)) categoryCounts[category]++;
    });

    // Generate highlights (top 3 items per category)
    const highlights = {
        features: categories.features.slice(0, 3),
        fixes: categories.fixes.slice(0, 3),
        improvements: categories.improvements.slice(0, 3)
    };

    const meaningful = meaningfulCommits.length > 0;

    // Plain-English summary for the public What's New page
    const summary = meaningful
        ? generatePublicSummary(highlights.features, highlights.fixes, highlights.improvements)
        : '';

    return {
        date,
        totalCommits: commits.length,
        meaningfulCommits: meaningfulCommits.length,
        meaningful,
        categories: categoryCounts,
        highlights,
        summary,
        blogPostUrl: null // Will be set when blog post is created
    };
}

/**
 * Generate blog post HTML from daily update
 * @param {Object} update - Daily update object
 * @param {Array} allCommits - Full commit list for detailed content
 * @returns {string} - HTML content
 */
function generateBlogPostHTML(update, allCommits) {
    const dateObj = new Date(update.date);
    const formattedDate = dateObj.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    const slug = `daily-update-${update.date}`;

    const featuresSection = update.highlights.features.length > 0 ? `
            <h2>What's New</h2>
            <ul>
                ${update.highlights.features.map(f => `<li>${toPlainEnglish(f)}</li>`).join('\n                ')}
            </ul>
    ` : '';

    const fixesSection = update.highlights.fixes.length > 0 ? `
            <h2>Fixes</h2>
            <ul>
                ${update.highlights.fixes.map(f => `<li>${toPlainEnglish(f)}</li>`).join('\n                ')}
            </ul>
    ` : '';

    const improvementsSection = update.highlights.improvements.length > 0 ? `
            <h2>Improvements</h2>
            <ul>
                ${update.highlights.improvements.map(f => `<li>${toPlainEnglish(f)}</li>`).join('\n                ')}
            </ul>
    ` : '';

    const summary = update.summary ||
        `Development update for ${formattedDate}. Work continues on the game.`;

    return `<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>What's New: ${formattedDate} | The Ascendant Continuum</title>
    <meta name="description" content="${summary}">
    <meta property="og:title" content="Daily Update - ${formattedDate}">
    <meta property="og:description" content="${summary}">
    <meta property="og:type" content="article">
    <link rel="stylesheet" href="../styles.css">
</head>

<body>
    <article class="blog-post">
        <header class="post-header">
            <div class="post-category">What's New</div>
            <h1>Daily Update: ${formattedDate}</h1>
            <div class="post-meta">
                <time datetime="${update.date}">${formattedDate}</time>
                <span class="post-stats">${update.totalCommits} commits</span>
            </div>
        </header>

        <div class="post-content">
            <p class="lead">${summary}</p>

${featuresSection}
${fixesSection}
${improvementsSection}

            <h2>📊 Development Stats</h2>
            <ul>
                <li><strong>Total commits:</strong> ${update.totalCommits}</li>
                <li><strong>Features:</strong> ${update.categories.features}</li>
                <li><strong>Bug fixes:</strong> ${update.categories.fixes}</li>
                <li><strong>Improvements:</strong> ${update.categories.improvements}</li>
                ${update.categories.docs > 0 ? `<li><strong>Documentation:</strong> ${update.categories.docs}</li>` : ''}
            </ul>

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
 * Save daily update to data.json
 * @param {Object} update - Daily update object
 */
function saveDailyUpdate(update) {
    let data = { dailyUpdates: [], weeklyDigests: [], meta: {} };

    if (fs.existsSync(DATA_FILE)) {
        data = JSON.parse(fs.readFileSync(DATA_FILE, 'utf-8'));
    }

    // Remove existing entry for this date if it exists
    data.dailyUpdates = data.dailyUpdates.filter(u => u.date !== update.date);

    // Add new update
    data.dailyUpdates.push(update);

    // Sort by date descending
    data.dailyUpdates.sort((a, b) => new Date(b.date) - new Date(a.date));

    // Update metadata
    data.lastUpdated = new Date().toISOString();
    data.meta = {
        totalDailyUpdates: data.dailyUpdates.length,
        totalWeeklyDigests: (data.weeklyDigests || []).length,
        oldestUpdateDate: data.dailyUpdates[data.dailyUpdates.length - 1]?.date || null,
        newestUpdateDate: data.dailyUpdates[0]?.date || null
    };

    if (!dryRun) {
        fs.writeFileSync(DATA_FILE, JSON.stringify(data, null, 2), 'utf-8');
        console.log(`✅ Saved update to ${DATA_FILE}`);
    } else {
        console.log('🔍 DRY RUN: Would save to', DATA_FILE);
    }
}

/**
 * Main execution
 */
async function main() {
    console.log('📅 What\'s New Daily Update Generator\n');
    console.log('═'.repeat(70));

    // Ensure directories exist
    if (!fs.existsSync(WHATS_NEW_DIR)) {
        fs.mkdirSync(WHATS_NEW_DIR, { recursive: true });
    }
    if (!fs.existsSync(BLOG_POSTS_DIR)) {
        fs.mkdirSync(BLOG_POSTS_DIR, { recursive: true });
    }

    if (isHistorical) {
        console.log('\n📚 HISTORICAL MODE: Processing all commits...\n');
        const commitsByDay = getAllCommitsByDay();
        const dates = Object.keys(commitsByDay).sort();

        console.log(`Found commits across ${dates.length} days (${dates[dates.length - 1]} to ${dates[0]})\n`);

        let processedCount = 0;
        for (const date of dates) {
            const commits = commitsByDay[date];

            if (commits.length === 0) continue;

            console.log(`\n📅 Processing ${date} (${commits.length} commits)...`);

            const update = generateDailyUpdate(commits, date);

            // Generate blog post
            const blogHTML = generateBlogPostHTML(update, commits);
            const blogSlug = `daily-update-${date}`;
            const blogPath = path.join(BLOG_POSTS_DIR, `${blogSlug}.html`);

            if (!dryRun) {
                fs.writeFileSync(blogPath, blogHTML, 'utf-8');
                update.blogPostUrl = `/blog/posts/${blogSlug}.html`;
                console.log(`  ✅ Created blog post: ${blogSlug}.html`);
            }

            // Save to data.json
            saveDailyUpdate(update);

            processedCount++;
        }

        console.log('\n═'.repeat(70));
        console.log(`\n✅ Historical generation complete: ${processedCount} days processed\n`);

    } else if (dateArg) {
        // Specific date
        const targetDate = dateArg.split('=')[1];
        console.log(`\n📅 Generating update for specific date: ${targetDate}\n`);

        const commits = getCommitsByDateRange(targetDate, targetDate);

        if (commits.length === 0) {
            console.log(`ℹ️ No commits found for ${targetDate}`);
            return;
        }

        console.log(`Found ${commits.length} commits for ${targetDate}\n`);

        const update = generateDailyUpdate(commits, targetDate);

        // Generate blog post
        const blogHTML = generateBlogPostHTML(update, commits);
        const blogSlug = `daily-update-${targetDate}`;
        const blogPath = path.join(BLOG_POSTS_DIR, `${blogSlug}.html`);

        if (!dryRun) {
            fs.writeFileSync(blogPath, blogHTML, 'utf-8');
            update.blogPostUrl = `/blog/posts/${blogSlug}.html`;
            console.log(`✅ Created blog post: ${blogSlug}.html`);
        }

        saveDailyUpdate(update);

        console.log('\n✅ Update generated successfully\n');

    } else {
        // Default: last 24 hours or specified days
        const days = daysArg ? parseInt(daysArg.split('=')[1]) : 1;
        const today = new Date().toISOString().split('T')[0];

        console.log(`\n📅 Generating update for last ${days} day(s) (date: ${today})\n`);

        const commits = getRecentCommits(days);

        if (commits.length === 0) {
            console.log(`ℹ️ No commits found in the last ${days} day(s)`);
            return;
        }

        console.log(`Found ${commits.length} commits\n`);

        const update = generateDailyUpdate(commits, today);

        if (!update.meaningful && !isForce) {
            console.log('ℹ️ No user-impacting changes detected. Skipping What\'s New entry.');
            console.log('   (Use --force to generate anyway)');
            return;
        }

        if (!update.meaningful && isForce) {
            console.log('⚠️  No user-impacting changes, but --force is set. Generating anyway.');
        }

        // Generate blog post
        const blogHTML = generateBlogPostHTML(update, commits);
        const blogSlug = `daily-update-${today}`;
        const blogPath = path.join(BLOG_POSTS_DIR, `${blogSlug}.html`);

        if (!dryRun) {
            fs.writeFileSync(blogPath, blogHTML, 'utf-8');
            update.blogPostUrl = `/blog/posts/${blogSlug}.html`;
            console.log(`✅ Created blog post: ${blogSlug}.html`);
        }

        saveDailyUpdate(update);

        // Display summary
        console.log('\n📊 Update Summary:');
        console.log(`   Date: ${update.date}`);
        console.log(`   Total commits: ${update.totalCommits}`);
        console.log(`   Features: ${update.categories.features}`);
        console.log(`   Fixes: ${update.categories.fixes}`);
        console.log(`   Improvements: ${update.categories.improvements}`);
        console.log(`   Blog post: ${update.blogPostUrl || 'N/A'}`);

        console.log('\n✅ Daily update generated successfully\n');
    }
}

// Run if called directly
if (import.meta.url.endsWith(path.basename(process.argv[1]))) {
    main().catch(error => {
        console.error('❌ Error:', error);
        process.exit(1);
    });
}

export { generateDailyUpdate, getCommitsByDateRange, getAllCommitsByDay };
