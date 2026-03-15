import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { generateBlogPost } from '../content/blog-post-generator.js';
import { postToBluesky } from '../posting/post-to-bluesky.js';
import { postToMastodon } from '../posting/post-to-mastodon.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const BLOG_DIR = path.join(__dirname, '..', '..', 'firebase', 'public', 'blog');
const POSTS_DIR = path.join(BLOG_DIR, 'posts');
const DATA_FILE = path.join(BLOG_DIR, 'data.json');
const RSS_FILE = path.join(BLOG_DIR, 'rss.xml');

// Ensure directories exist
function ensureDirectories() {
    if (!fs.existsSync(BLOG_DIR)) {
        fs.mkdirSync(BLOG_DIR, { recursive: true });
    }
    if (!fs.existsSync(POSTS_DIR)) {
        fs.mkdirSync(POSTS_DIR, { recursive: true });
    }
}

// Load existing blog data
function loadBlogData() {
    if (!fs.existsSync(DATA_FILE)) {
        return { posts: [], stats: { totalPosts: 0, byTheme: {}, byTag: {} } };
    }
    try {
        const data = fs.readFileSync(DATA_FILE, 'utf-8');
        return JSON.parse(data);
    } catch (error) {
        console.error('Error loading blog data:', error.message);
        return { posts: [], stats: { totalPosts: 0, byTheme: {}, byTag: {} } };
    }
}

// Save blog data
function saveBlogData(data) {
    fs.writeFileSync(DATA_FILE, JSON.stringify(data, null, 2), 'utf-8');
}

// Generate RSS feed
function generateRSSFeed(posts) {
    const siteUrl = 'https://ascendantcontinuum.web.app';
    const feedUrl = `${siteUrl}/blog/rss.xml`;
    const now = new Date().toUTCString();

    // Sort posts by date descending, take most recent 20
    const recentPosts = posts
        .sort((a, b) => new Date(b.date) - new Date(a.date))
        .slice(0, 20);

    const rssItems = recentPosts.map(post => `
    <item>
      <title><![CDATA[${post.title}]]></title>
      <link>${siteUrl}/blog/posts/${post.slug}.html</link>
      <guid isPermaLink="true">${siteUrl}/blog/posts/${post.slug}.html</guid>
      <pubDate>${new Date(post.date).toUTCString()}</pubDate>
      <category><![CDATA[${post.themeName}]]></category>
      ${post.tags.map(tag => `<category><![CDATA[${tag}]]></category>`).join('\n      ')}
      <description><![CDATA[${post.excerpt}]]></description>
    </item>`).join('\n');

    const rssFeed = `<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
  <channel>
    <title>The Ascendant Continuum Dev Blog</title>
    <link>${siteUrl}/blog/</link>
    <description>Daily insights into building an accessibility-first magical adventure - Unity development, ethical game design, and community stories</description>
    <language>en-us</language>
    <lastBuildDate>${now}</lastBuildDate>
    <atom:link href="${feedUrl}" rel="self" type="application/rss+xml" />
    <image>
      <url>${siteUrl}/favicon.png</url>
      <title>The Ascendant Continuum</title>
      <link>${siteUrl}/blog/</link>
    </image>
${rssItems}
  </channel>
</rss>`;

    fs.writeFileSync(RSS_FILE, rssFeed, 'utf-8');
    console.log('✅ Generated RSS feed with', recentPosts.length, 'posts');
}

// Generate HTML template for blog post
function generatePostHTML(post) {
    const postUrl = `https://ascendantcontinuum.web.app/blog/posts/${post.slug}.html`;
    const shareTextEncoded = encodeURIComponent(`${post.title}\n\n${post.hook}\n\nRead more:`);

    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="${post.excerpt.replace(/"/g, '&quot;')}">
    <meta property="og:title" content="${post.title} - The Ascendant Continuum">
    <meta property="og:description" content="${post.excerpt.replace(/"/g, '&quot;')}">
    <meta property="og:type" content="article">
    <meta property="og:url" content="${postUrl}">
    <meta property="article:published_time" content="${post.date}">
    <meta property="article:author" content="The Ascendant Continuum Team">
    ${post.tags.map(tag => `<meta property="article:tag" content="${tag}">`).join('\n    ')}
    <meta name="twitter:card" content="summary_large_image">
    <meta name="twitter:title" content="${post.title}">
    <meta name="twitter:description" content="${post.excerpt.replace(/"/g, '&quot;')}">
    <title>${post.title} - The Ascendant Continuum Dev Blog</title>
    <link rel="stylesheet" href="../../css/style.css">
    <link rel="stylesheet" href="../../css/images.css">
    <link rel="icon" type="image/png" href="../../favicon.png">
    <link rel="canonical" href="${postUrl}">
    <style>
        .blog-post-container {
            max-width: 800px;
            margin: 0 auto;
            padding: 5rem var(--spacing-md) var(--spacing-xl);
        }
        .blog-nav-breadcrumb {
            padding-bottom: var(--spacing-md);
            font-size: 0.9rem;
            color: var(--color-secondary);
        }
        .blog-nav-breadcrumb a {
            color: var(--color-secondary);
            text-decoration: none;
        }
        .blog-nav-breadcrumb a:hover {
            text-decoration: underline;
        }
        .blog-post-header {
            margin-bottom: var(--spacing-lg);
            padding-bottom: var(--spacing-md);
            border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        }
        .blog-post-meta {
            display: flex;
            gap: var(--spacing-sm);
            font-size: 0.9rem;
            color: var(--color-secondary);
            margin-bottom: var(--spacing-md);
            flex-wrap: wrap;
        }
        .blog-post-tag {
            background: rgba(78, 205, 196, 0.2);
            padding: 0.25rem 0.75rem;
            border-radius: 20px;
            font-size: 0.8rem;
        }
        .blog-post-title {
            font-size: 2.5rem;
            line-height: 1.2;
            margin-bottom: var(--spacing-sm);
            background: var(--gradient-hero);
            -webkit-background-clip: text;
            background-clip: text;
            -webkit-text-fill-color: transparent;
        }
        .blog-post-hook {
            font-size: 1.25rem;
            font-style: italic;
            color: var(--color-secondary);
            margin-bottom: var(--spacing-lg);
            padding: var(--spacing-md);
            background: rgba(78, 205, 196, 0.1);
            border-left: 4px solid var(--color-secondary);
            border-radius: 4px;
            line-height: 1.6;
        }
        .blog-post-content {
            font-size: 1.1rem;
            line-height: 1.8;
            color: var(--color-light);
        }
        .blog-post-content h2 {
            font-size: 1.75rem;
            margin-top: var(--spacing-lg);
            margin-bottom: var(--spacing-md);
            color: var(--color-primary);
        }
        .blog-post-content p {
            margin-bottom: var(--spacing-md);
            white-space: pre-wrap;
        }
        .blog-post-footer {
            margin-top: var(--spacing-xl);
            padding-top: var(--spacing-md);
            border-top: 1px solid rgba(255, 255, 255, 0.1);
            text-align: center;
        }
        .blog-back-link {
            display: inline-block;
            color: var(--color-primary);
            text-decoration: none;
            font-weight: 600;
            transition: transform 0.2s ease;
            margin-bottom: var(--spacing-md);
        }
        .blog-back-link:hover {
            transform: translateX(-4px);
        }
        .blog-share-buttons {
            display: flex;
            gap: var(--spacing-sm);
            justify-content: center;
            margin-top: var(--spacing-md);
            flex-wrap: wrap;
        }
        .blog-share-btn {
            padding: 0.5rem 1rem;
            background: var(--gradient-card);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 8px;
            color: var(--color-light);
            text-decoration: none;
            font-size: 0.9rem;
            transition: transform 0.2s ease;
            display: inline-block;
        }
        .blog-share-btn:hover {
            transform: translateY(-2px);
        }
        @media (max-width: 768px) {
            .blog-post-title {
                font-size: 2rem;
            }
            .blog-post-content {
                font-size: 1rem;
            }
        }
    </style>
</head>
<body>
    <nav class="top-nav" aria-label="Primary">
        <div class="container top-nav-inner">
            <a href="/" class="top-nav-brand">Ascendant Continuum</a>
            <div class="top-nav-links">
                <a href="/#latest" class="top-nav-link">Updates</a>
                <a href="/blog/" class="top-nav-link">Blog</a>
                <a href="/#faq" class="top-nav-link">FAQ</a>
                <a href="/#signup" class="top-nav-link">Waitlist</a>
                <a href="/play/" class="top-nav-link top-nav-link-play">Play</a>
            </div>
        </div>
    </nav>

    <article class="blog-post-container">
        <div class="blog-nav-breadcrumb">
            <a href="/">Home</a> / <a href="/blog/">Blog</a> / ${post.title}
        </div>

        <header class="blog-post-header">
            <div class="blog-post-meta">
                <time datetime="${post.date}">${new Date(post.date).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}</time>
                ${post.tags.map(tag => `<span class="blog-post-tag">${tag}</span>`).join('')}
            </div>
            <h1 class="blog-post-title">${post.title}</h1>
        </header>

        <div class="blog-post-content">
            ${post.content}
        </div>

        <footer class="blog-post-footer">
            <a href="/blog/" class="blog-back-link">← Back to all posts</a>
            
            <div class="blog-share-buttons">
                <a href="https://bsky.app/intent/compose?text=${shareTextEncoded}%20${encodeURIComponent(postUrl)}" 
                   target="_blank" 
                   rel="noopener noreferrer" 
                   class="blog-share-btn">📱 Share on Bluesky</a>
                <a href="https://mastodon.social/share?text=${shareTextEncoded}%20${encodeURIComponent(postUrl)}" 
                   target="_blank" 
                   rel="noopener noreferrer" 
                   class="blog-share-btn">🐘 Share on Mastodon</a>
            </div>
        </footer>
    </article>
</body>
</html>`;
}

// Post blog link to social media
async function postToSocialMedia(post) {
    const postUrl = `https://ascendantcontinuum.web.app/blog/posts/${post.slug}.html`;
    const socialText = `📝 New blog post: ${post.title}\n\n${post.hook}\n\nRead more: ${postUrl}\n\n#GameDev #Unity #AccessibilityFirst #IndieGame`;

    try {
        console.log('\n📢 Posting blog link to social media...');

        // Post to Bluesky
        try {
            await postToBluesky(socialText);
            console.log('✅ Posted to Bluesky');
        } catch (error) {
            console.error('❌ Bluesky posting failed:', error.message);
        }

        // Small delay between posts
        await new Promise(resolve => setTimeout(resolve, 2000));

        // Post to Mastodon
        try {
            await postToMastodon(socialText);
            console.log('✅ Posted to Mastodon');
        } catch (error) {
            console.error('❌ Mastodon posting failed:', error.message);
        }

        console.log('✅ Social media posting complete\n');
    } catch (error) {
        console.error('❌ Error posting to social media:', error.message);
    }
}

// Create and publish a new blog post
async function publishBlogPost() {
    console.log('🎨 Generating new blog post...');

    ensureDirectories();

    // Generate post
    const post = generateBlogPost();
    console.log(`📝 Generated: "${post.title}"`);
    console.log(`🎭 Theme: ${post.themeName}`);
    console.log(`🏷️  Tags: ${post.tags.join(', ')}`);
    console.log(`📅 Date: ${post.date}`);

    // Load existing data
    const blogData = loadBlogData();

    // Check if post with same slug already exists today
    const existingPost = blogData.posts.find(p => p.slug === post.slug && p.date === post.date);
    if (existingPost) {
        console.log('⚠️  Post with same title already published today. Generating alternative...');
        // Try generating a different post
        const alternativePost = generateBlogPost();
        if (blogData.posts.find(p => p.slug === alternativePost.slug && p.date === alternativePost.date)) {
            console.log('⚠️  Alternative also exists. Skipping today\'s post.');
            return false;
        }
        Object.assign(post, alternativePost);
    }

    // Create HTML file
    const postHTML = generatePostHTML(post);
    const postFilePath = path.join(POSTS_DIR, `${post.slug}.html`);
    fs.writeFileSync(postFilePath, postHTML, 'utf-8');
    console.log(`✅ Created HTML file: ${post.slug}.html`);

    // Add to blog data (only metadata, not full content)
    blogData.posts.push({
        title: post.title,
        slug: post.slug,
        date: post.date,
        tags: post.tags,
        themeName: post.themeName,
        excerpt: post.excerpt
    });

    // Update stats
    blogData.stats.totalPosts = blogData.posts.length;
    blogData.stats.byTheme = blogData.stats.byTheme || {};
    blogData.stats.byTheme[post.themeName] = (blogData.stats.byTheme[post.themeName] || 0) + 1;
    blogData.stats.byTag = blogData.stats.byTag || {};
    post.tags.forEach(tag => {
        blogData.stats.byTag[tag] = (blogData.stats.byTag[tag] || 0) + 1;
    });

    saveBlogData(blogData);
    console.log('✅ Updated blog data.json');

    // Generate RSS feed
    generateRSSFeed(blogData.posts);

    // Post to social media
    await postToSocialMedia(post);

    console.log(`\n🎉 Blog post published successfully!`);
    console.log(`📍 View at: /blog/posts/${post.slug}.html`);
    console.log(`📊 Stats: ${blogData.stats.totalPosts} total posts`);

    return true;
}

// Main execution
async function main() {
    try {
        const success = await publishBlogPost();
        if (success) {
            process.exit(0);
        } else {
            console.log('ℹ️  No post published today (duplicate or no commits)');
            process.exit(0); // Exit successfully even if skipped
        }
    } catch (error) {
        console.error('❌ Error publishing blog post:', error);
        console.error(error.stack);
        process.exit(1);
    }
}

main();
