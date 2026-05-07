import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const projectRoot = path.join(__dirname, '..', '..');
const postsDir = path.join(projectRoot, 'firebase', 'public', 'blog', 'posts');
const socialBank = path.join(projectRoot, 'public', 'social', 'content-bank.json');

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
    }
}

function validateSocialBank() {
    const json = JSON.parse(fs.readFileSync(socialBank, 'utf8'));
    for (const entry of json.content || []) {
        const text = `${entry.hook || ''} ${entry.body || ''}`;
        const urls = text.match(/https?:\/\/[^\s)]+/gi) || [];

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
validateSocialBank();

if (hasError) {
    process.exit(1);
}

console.log('Auto publish guard passed.');
