import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const PROJECT_ROOT = path.join(__dirname, '..', '..');

const POSTS_DIR = path.join(PROJECT_ROOT, 'firebase', 'public', 'blog', 'posts');
const DATA_PATH = path.join(PROJECT_ROOT, 'firebase', 'public', 'blog', 'data.json');

const CANONICAL_REGEX = /<link\s+rel="canonical"\s+href="https:\/\/ascendant-continuum\.web\.app\/blog\/posts\/.+?\.html"/i;
const PLACEHOLDER_REGEX = /\[link to gist\]|\[TBD\]|\[coming soon\]|\[URL\]|TODO:/i;

function run() {
    const files = fs.readdirSync(POSTS_DIR).filter((f) => f.endsWith('.html'));

    const stats = {
        totalFiles: files.length,
        missingCanonical: 0,
        missingSiteLink: 0,
        missingBlogLink: 0,
        containsNPC: 0,
        containsGameCharacters: 0,
        containsPlaceholder: 0,
        replacementChar: 0,
    };

    const issues = [];

    for (const file of files) {
        const text = fs.readFileSync(path.join(POSTS_DIR, file), 'utf8');
        const fileIssues = [];

        if (!CANONICAL_REGEX.test(text)) {
            stats.missingCanonical++;
            fileIssues.push('missing_canonical');
        }

        if (!text.includes('https://ascendant-continuum.web.app/')) {
            stats.missingSiteLink++;
            fileIssues.push('missing_site_link');
        }

        if (!text.includes('https://ascendant-continuum.web.app/blog/')) {
            stats.missingBlogLink++;
            fileIssues.push('missing_blog_link');
        }

        if (/\bNPCs?\b/i.test(text)) {
            stats.containsNPC++;
            fileIssues.push('contains_NPC');
        }

        if (/\bgame characters?\b/i.test(text)) {
            stats.containsGameCharacters++;
            fileIssues.push('contains_game_characters');
        }

        if (PLACEHOLDER_REGEX.test(text)) {
            stats.containsPlaceholder++;
            fileIssues.push('contains_placeholder');
        }

        if (text.includes('�')) {
            stats.replacementChar++;
            fileIssues.push('replacement_char');
        }

        if (fileIssues.length > 0) {
            issues.push({ file, issues: fileIssues });
        }
    }

    const data = JSON.parse(fs.readFileSync(DATA_PATH, 'utf8'));
    const indexed = new Set((data.posts || []).map((p) => `${p.slug}.html`));
    const onDisk = new Set(files);

    const missingOnDisk = [...indexed].filter((f) => !onDisk.has(f));
    const extraOnDisk = [...onDisk].filter((f) => !indexed.has(f));

    const report = {
        stats,
        dataPosts: (data.posts || []).length,
        missingOnDiskCount: missingOnDisk.length,
        extraOnDiskCount: extraOnDisk.length,
        issueCount: issues.length,
        sampleIssues: issues.slice(0, 20),
    };

    console.log('BLOG_CONSISTENCY_REPORT');
    console.log(JSON.stringify(report, null, 2));

    const hasProblems =
        stats.missingCanonical > 0 ||
        stats.missingSiteLink > 0 ||
        stats.missingBlogLink > 0 ||
        stats.containsNPC > 0 ||
        stats.containsGameCharacters > 0 ||
        stats.containsPlaceholder > 0 ||
        stats.replacementChar > 0 ||
        missingOnDisk.length > 0 ||
        extraOnDisk.length > 0;

    if (hasProblems) {
        process.exit(1);
    }
}

run();
