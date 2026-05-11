import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import {
    findProhibitedMatches,
    socialFingerprint,
} from '../utils/truth-and-dedupe-guard.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const projectRoot = path.join(__dirname, '..', '..');
const postsDir = path.join(projectRoot, 'firebase', 'public', 'blog', 'posts');
const socialBank = path.join(projectRoot, 'public', 'social', 'content-bank.json');
const blogDataPath = path.join(projectRoot, 'firebase', 'public', 'blog', 'data.json');

let hasError = false;

function fail(message) {
    hasError = true;
    console.error(`ERROR: ${message}`);
}

function getFiles(dir, extension) {
    return fs.readdirSync(dir)
        .filter(name => name.endsWith(extension))
        .map(name => path.join(dir, name));
}

function validateBlogPosts() {
    const files = getFiles(postsDir, '.html');

    for (const file of files) {
        const content = fs.readFileSync(file, 'utf8');
        const rel = path.relative(projectRoot, file).replace(/\\/g, '/');

        if (content.includes('This HTML file serves as a placeholder')) {
            fail(`Placeholder content found in ${rel}`);
        }

        if (/href="\.\.\.\/\.\.\.\/blog\/.*\.md"/i.test(content)) {
            fail(`Dead markdown development link found in ${rel}`);
        }

        if (/\)\}<\/li>/.test(content)) {
            fail(`Malformed HTML token found in ${rel}`);
        }

        const blocked = findProhibitedMatches(content);
        if (blocked.length > 0) {
            fail(`Prohibited content found in ${rel}: ${blocked.join(', ')}`);
        }
    }
}

function validateBlogIndex() {
    const data = JSON.parse(fs.readFileSync(blogDataPath, 'utf8'));
    const slugSeen = new Set();
    const titleSeen = new Set();

    for (const post of data.posts || []) {
        if (slugSeen.has(post.slug)) {
            fail(`Duplicate blog slug in data.json: ${post.slug}`);
        }
        slugSeen.add(post.slug);

        if (titleSeen.has(post.title)) {
            fail(`Duplicate blog title in data.json: ${post.title}`);
        }
        titleSeen.add(post.title);

        const htmlPath = path.join(postsDir, `${post.slug}.html`);
        if (!fs.existsSync(htmlPath)) {
            fail(`Blog index references missing HTML: ${post.slug}.html`);
        }

        const blocked = findProhibitedMatches(`${post.title}\n${post.excerpt || ''}`);
        if (blocked.length > 0) {
            fail(`Prohibited content found in data.json entry ${post.slug}: ${blocked.join(', ')}`);
        }
    }
}

function validateSocialBank() {
    const json = JSON.parse(fs.readFileSync(socialBank, 'utf8'));
    const fingerprintSeen = new Set();

    for (const entry of json.content || []) {
        const text = `${entry.hook || ''} ${entry.body || ''}`;
        const urls = text.match(/https?:\/\/[^\s)]+/gi) || [];

        const blocked = findProhibitedMatches(text);
        if (blocked.length > 0) {
            fail(`Prohibited content found in content bank entry ${entry.id}: ${blocked.join(', ')}`);
        }

        const fingerprint = socialFingerprint(entry);
        if (fingerprintSeen.has(fingerprint)) {
            fail(`Duplicate social content detected for entry ${entry.id}`);
        }
        fingerprintSeen.add(fingerprint);

        if (urls.length === 0) {
            fail(`Content bank entry ${entry.id} has no URL`);
        }

        for (const url of urls) {
            if (!url.startsWith('https://ascendant-continuum.web.app')) {
                fail(`Content bank entry ${entry.id} has unexpected URL: ${url}`);
            }
        }
    }
}

validateBlogPosts();
validateBlogIndex();
validateSocialBank();

if (hasError) {
    process.exit(1);
}

console.log('Auto publish guard passed.');
