# Blog Post Creation Guide

## ⚠️ CRITICAL RULE: Always Create FULL HTML Content

**NEVER create placeholder blog posts that link to markdown files.**

Every blog post HTML file MUST contain the full article content in proper HTML format.

---

## Step-by-Step Process

### 1. Write Content First
- Write your blog post content (can start in markdown for drafting)
- Aim for 800-2000 words for substantial posts
- Include code examples, lists, headers, and images where relevant

### 2. Create HTML File with Full Content
- Use `BLOG_POST_TEMPLATE.html` as starting point
- Copy template to `firebase/public/blog/posts/your-post-slug.html`
- Replace ALL placeholders with actual content

### 3. Convert Content to Proper HTML
- `# Heading` → `<h2>Heading</h2>`
- `## Subheading` → `<h3>Subheading</h3>`
- Regular paragraphs → `<p>Content here</p>`
- Code blocks → `<pre><code>your code</code></pre>`
- Lists → `<ul><li>Item</li></ul>` or `<ol><li>Item</li></ol>`
- Links → `<a href="url">text</a>`
- Bold → `<strong>text</strong>`
- Italic → `<em>text</em>`

### 4. Update blog/data.json
Add entry to the TOP of the posts array:

```json
{
  "title": "Your Blog Post Title",
  "slug": "your-post-slug",
  "date": "2026-04-02",
  "themeName": "Category Name",
  "tags": ["tag1", "tag2", "tag3"],
  "excerpt": "Brief 1-2 sentence description for the blog index page",
  "hook": "Compelling one-liner that appears at the top of the post"
}
```

### 5. Create Markdown Copy (Optional)
- If you want a markdown version in the `blog/` directory, that's fine
- But it's NOT required and should NOT be the primary source
- The HTML file is always the source of truth

### 6. Test Locally
- Open `firebase/public/blog/posts/your-post-slug.html` in a browser
- Verify all content displays correctly
- Check that CSS styling works
- Test all links

### 7. Add to Automated Posting Queue
Add a blog announcement post to `firebase/public/social-media-content.json`:

```json
{
    "id": "blog-your-slug",
    "category": "blog-announcement",
    "platforms": ["bluesky", "mastodon", "discord"],
    "content": "📝 New Blog Post: [Title]\n\n[Brief description]\n\nRead: https://ascendant-continuum.web.app/blog/posts/your-post-slug.html\n\n#[RelevantHashtag]"
}
```

### 8. Deploy
```powershell
git add firebase/public/blog/
git commit -m "Add new blog post: [Title]"
git push origin main
cd firebase
firebase deploy --only hosting
```

---

## Common Mistakes to AVOID

❌ **DON'T:**
- Create placeholder HTML files that say "See markdown file"
- Link to markdown files from HTML
- Use relative paths like `../../../blog/file.md`
- Forget to add actual content

✅ **DO:**
- Put full article content in HTML file
- Use proper HTML semantic tags
- Include proper metadata (title, description, og tags)
- Test that content displays correctly before deploying

---

## Content Structure Best Practices

### Opening
- Start with a hook paragraph (why should they care?)
- State the problem or opportunity
- Preview what they'll learn

### Body
- Break into clear sections with H2/H3 headings
- Use short paragraphs (3-5 sentences max)
- Include code examples with syntax highlighting
- Add lists for scannable content
- Use blockquotes for important callouts

### Conclusion
- Summarize key takeaways
- Provide next steps or calls to action
- Link to related posts
- Invite feedback/discussion

### Visual Elements
- Use `<pre><code>` for code blocks
- Use `<blockquote>` for highlighting key points
- Use lists (`<ul>`, `<ol>`) for easy scanning
- Consider adding ASCII art or diagrams if appropriate

---

## SEO Checklist

- [ ] Meta description (150-160 characters)
- [ ] OpenGraph title and description
- [ ] Article published date
- [ ] Proper H1 (only one per page)
- [ ] H2/H3 structure for subsections
- [ ] Alt text for any images
- [ ] Internal links to related posts
- [ ] Clean, readable URL slug

---

## Automated Posting Integration

After creating a blog post:

1. **Add to content bank** (optional for rotation):
   - File: `public/social/content-bank.json`
   - Add variations about the blog post

2. **Add blog announcement** (required for immediate posting):
   - File: `firebase/public/social-media-content.json`
   - Will be posted by `run-daily-blog.ps1` automation

3. **Schedule** (if using Windows Task Scheduler):
   - Blog posts: 10 AM UTC daily
   - Social announcements: 2 PM UTC daily

---

## Example: Good vs Bad

### ❌ BAD (Placeholder)
```html
<body>
    <article>
        <h1>My Blog Post</h1>
        <p><em>For the full article, see <a href="../../../blog/my-post.md">my-post.md</a></em></p>
    </article>
</body>
```

### ✅ GOOD (Full Content)
```html
<body>
    <div class="blog-post-container">
        <article class="blog-post">
            <header class="blog-post-header">
                <h1 class="blog-post-title">My Blog Post</h1>
            </header>
            
            <div class="blog-post-content">
                <h2>Introduction</h2>
                <p>This is the actual article content with real value...</p>
                
                <h2>Main Section</h2>
                <p>More detailed explanation with examples...</p>
                
                <pre><code>// Actual code example
function example() {
    return "Real content";
}
</code></pre>
                
                <h2>Conclusion</h2>
                <p>Key takeaways and next steps...</p>
            </div>
        </article>
    </div>
</body>
```

---

## Quick Reference

**Template Location:** `blog/BLOG_POST_TEMPLATE.html`  
**Output Directory:** `firebase/public/blog/posts/`  
**Metadata File:** `firebase/public/blog/data.json`  
**Social Announcements:** `firebase/public/social-media-content.json`  

**Remember:** The HTML file IS the blog post. No placeholders. No markdown links. Full content only.
