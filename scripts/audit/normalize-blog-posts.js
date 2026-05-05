import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const PROJECT_ROOT = path.join(__dirname, '..', '..');

const POSTS_DIR = path.join(PROJECT_ROOT, 'firebase', 'public', 'blog', 'posts');
const DATA_PATH = path.join(PROJECT_ROOT, 'firebase', 'public', 'blog', 'data.json');

const SITE_URL = 'https://ascendant-continuum.web.app/';
const BLOG_URL = 'https://ascendant-continuum.web.app/blog/';

function stripTags(html) {
    return html
        .replace(/<script[\s\S]*?<\/script>/gi, '')
        .replace(/<style[\s\S]*?<\/style>/gi, '')
        .replace(/<[^>]+>/g, ' ')
        .replace(/\s+/g, ' ')
        .trim();
}

function decodeHtmlEntities(text) {
    return text
        .replace(/&amp;/g, '&')
        .replace(/&lt;/g, '<')
        .replace(/&gt;/g, '>')
        .replace(/&quot;/g, '"')
        .replace(/&#39;/g, "'");
}

function extractFirstMatch(text, regex) {
    const match = text.match(regex);
    return match ? match[1].trim() : '';
}

function normalizeTerminology(html) {
    let out = html;
    out = out.replace(/\bgame characters?\s*\(\s*NPCs?\s*\)/gi, 'Realm Dwellers');
    out = out.replace(/\bNPCs\b/g, 'Realm Dwellers');
    out = out.replace(/\bNPC\b/g, 'Realm Dweller');
    out = out.replace(/\bgame characters?\b/gi, 'Realm Dwellers');
    out = out.replace(/\bRealm Dwellers\s*\(\s*Realm Dwellers\s*\)/gi, 'Realm Dwellers');
    return out;
}

function ensureCanonicalTag(html, slugFile) {
    const canonicalHref = `https://ascendant-continuum.web.app/blog/posts/${slugFile}`;
    const canonicalTag = `    <link rel="canonical" href="${canonicalHref}">`;

    if (/<link\s+rel=["']canonical["']/i.test(html)) {
        return html.replace(
            /<link\s+rel=["']canonical["'][^>]*>/i,
            canonicalTag
        );
    }

    return html.replace(/<\/head>/i, `${canonicalTag}\n</head>`);
}

function ensureCanonicalBodyLinks(html) {
    const hasSite = html.includes(SITE_URL);
    const hasBlog = html.includes(BLOG_URL);
    if (hasSite && hasBlog) return html;

    const canonicalBlock = [
        '<div class="canonical-links">',
        `    <p><a href="${BLOG_URL}">Ascendant Continuum Blog</a> · <a href="${SITE_URL}">Ascendant Continuum</a></p>`,
        '</div>',
    ].join('\n');

    if (/class=["']canonical-links["']/i.test(html)) {
        return html;
    }

    if (/<\/article>/i.test(html)) {
        return html.replace(/<\/article>/i, `${canonicalBlock}\n</article>`);
    }

    if (/<\/main>/i.test(html)) {
        return html.replace(/<\/main>/i, `${canonicalBlock}\n</main>`);
    }

    return html.replace(/<\/body>/i, `${canonicalBlock}\n</body>`);
}

function inferDate(slugFile, html) {
    const fromTime = extractFirstMatch(html, /<time[^>]*datetime=["']([^"']+)["']/i);
    if (/^\d{4}-\d{2}-\d{2}/.test(fromTime)) {
        return fromTime.slice(0, 10);
    }

    const slug = slugFile.replace('.html', '');
    const dateFromSlug = slug.match(/(\d{4}-\d{2}-\d{2})/);
    if (dateFromSlug) return dateFromSlug[1];

    return new Date().toISOString().slice(0, 10);
}

function inferTitle(html, slugFile) {
    let title = extractFirstMatch(html, /<h1[^>]*>([\s\S]*?)<\/h1>/i);
    if (!title) title = extractFirstMatch(html, /<title>([\s\S]*?)<\/title>/i);
    if (!title) title = slugFile.replace('.html', '').replace(/-/g, ' ');

    title = decodeHtmlEntities(stripTags(title));
    title = title.replace(/\s*[-|]\s*The Ascendant Continuum\s*$/i, '').trim();
    return title;
}

function inferExcerpt(html) {
    let excerpt = extractFirstMatch(html, /<meta\s+name=["']description["']\s+content=["']([\s\S]*?)["'][^>]*>/i);
    if (!excerpt) excerpt = extractFirstMatch(html, /<p[^>]*class=["']lead["'][^>]*>([\s\S]*?)<\/p>/i);
    if (!excerpt) excerpt = extractFirstMatch(html, /<p[^>]*>([\s\S]*?)<\/p>/i);
    excerpt = decodeHtmlEntities(stripTags(excerpt));
    return excerpt.slice(0, 280);
}

function inferTheme(html, slugFile) {
    let theme = extractFirstMatch(html, /<span[^>]*class=["']blog-post-theme["'][^>]*>([\s\S]*?)<\/span>/i);
    if (!theme) theme = extractFirstMatch(html, /<div[^>]*class=["']post-category["'][^>]*>([\s\S]*?)<\/div>/i);
    if (!theme && /daily-update/i.test(slugFile)) theme = 'Daily Update';
    return decodeHtmlEntities(stripTags(theme || 'Development'));
}

function inferTags(html) {
    const tags = [];
    const regexMeta = /<meta\s+property=["']article:tag["']\s+content=["']([^"']+)["']/gi;
    let match;
    while ((match = regexMeta.exec(html)) !== null) {
        tags.push(match[1].trim());
    }

    if (tags.length > 0) return [...new Set(tags)].slice(0, 8);

    const regexSpan = /<span[^>]*class=["']tag["'][^>]*>([\s\S]*?)<\/span>/gi;
    while ((match = regexSpan.exec(html)) !== null) {
        tags.push(decodeHtmlEntities(stripTags(match[1])));
    }

    return [...new Set(tags.filter(Boolean))].slice(0, 8);
}

function buildPostRecord(slugFile, html, existing) {
    const slug = slugFile.replace('.html', '');
    const date = inferDate(slugFile, html);
    const title = inferTitle(html, slugFile);
    const excerpt = inferExcerpt(html);
    const hook = excerpt.length > 160 ? `${excerpt.slice(0, 157)}...` : excerpt;

    return {
        title,
        slug,
        date,
        themeName: inferTheme(html, slugFile),
        tags: inferTags(html),
        excerpt,
        hook,
        ...(existing?.featuredImage ? { featuredImage: existing.featuredImage } : {}),
    };
}

function run() {
    const files = fs.readdirSync(POSTS_DIR).filter((f) => f.endsWith('.html'));

    let fixedCanonical = 0;
    let fixedLinks = 0;
    let fixedTerminology = 0;

    for (const file of files) {
        const fullPath = path.join(POSTS_DIR, file);
        const original = fs.readFileSync(fullPath, 'utf8');

        let updated = original;

        const termNormalized = normalizeTerminology(updated);
        if (termNormalized !== updated) {
            fixedTerminology++;
            updated = termNormalized;
        }

        const withCanonical = ensureCanonicalTag(updated, file);
        if (withCanonical !== updated) {
            fixedCanonical++;
            updated = withCanonical;
        }

        const withLinks = ensureCanonicalBodyLinks(updated);
        if (withLinks !== updated) {
            fixedLinks++;
            updated = withLinks;
        }

        if (updated !== original) {
            fs.writeFileSync(fullPath, updated, 'utf8');
        }
    }

    const data = JSON.parse(fs.readFileSync(DATA_PATH, 'utf8'));
    const existingPosts = Array.isArray(data.posts) ? data.posts : [];
    const existingBySlug = new Map(existingPosts.map((p) => [p.slug, p]));

    const reconciled = [];
    let addedToIndex = 0;

    for (const file of files) {
        const slug = file.replace('.html', '');
        const fullPath = path.join(POSTS_DIR, file);
        const html = fs.readFileSync(fullPath, 'utf8');
        const existing = existingBySlug.get(slug);
        const record = buildPostRecord(file, html, existing);
        reconciled.push(existing ? { ...existing, ...record } : record);
        if (!existing) addedToIndex++;
    }

    reconciled.sort((a, b) => {
        const ad = (a.date || '').localeCompare(b.date || '');
        if (ad !== 0) return -ad;
        return (a.title || '').localeCompare(b.title || '');
    });

    data.posts = reconciled;
    data.stats = data.stats || {};
    data.stats.totalPosts = reconciled.length;
    data.lastUpdated = new Date().toISOString();

    fs.writeFileSync(DATA_PATH, JSON.stringify(data, null, 2), 'utf8');

    console.log('BLOG_NORMALIZATION_COMPLETE');
    console.log(JSON.stringify({
        totalHtmlFiles: files.length,
        fixedTerminology,
        fixedCanonical,
        fixedLinks,
        addedToIndex,
        finalIndexedPosts: reconciled.length,
    }, null, 2));
}

run();
