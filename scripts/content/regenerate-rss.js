/**
 * Regenerate RSS Feed with Correct URLs
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const DATA_FILE = path.join(__dirname, '../../firebase/public/blog/data.json');
const RSS_FILE = path.join(__dirname, '../../firebase/public/blog/rss.xml');

function generateRSSFeed() {
    const siteUrl = 'https://ascendant-continuum.web.app';
    const feedUrl = `${siteUrl}/blog/rss.xml`;
    const now = new Date().toUTCString();

    // Load blog data
    const blogData = JSON.parse(fs.readFileSync(DATA_FILE, 'utf-8'));
    const posts = blogData.posts;

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
    console.log('✅ Regenerated RSS feed with', recentPosts.length, 'posts');
    console.log(`📍 Site URL updated to: ${siteUrl}`);
}

generateRSSFeed();
