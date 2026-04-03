# Blog Post Creation Checklist

Use this checklist every time you create a new blog post.

## Before Creating Blog Post

- [ ] Content is written and ready (800-2000 words)
- [ ] You have chosen a clear URL slug (lowercase, hyphens, descriptive)
- [ ] You have selected 3-5 relevant tags
- [ ] You have a compelling one-liner hook

## Creating the HTML File

- [ ] Copy `blog/BLOG_POST_TEMPLATE.html` to `firebase/public/blog/posts/[slug].html`
- [ ] Replace `[Blog Post Title]` with actual title (3 places)
- [ ] Replace `[Brief description]` with SEO-friendly description
- [ ] Replace `[YYYY-MM-DD]` with publish date
- [ ] Replace `[Month Day, Year]` with formatted date
- [ ] Add 3-5 tags
- [ ] Add compelling hook
- [ ] **CONVERT ALL CONTENT FROM MARKDOWN TO HTML**
- [ ] Add actual paragraphs, headings, code blocks, lists
- [ ] NO placeholder text saying "see markdown file"
- [ ] Add related posts section at bottom

## Update Metadata

- [ ] Add entry to TOP of `firebase/public/blog/data.json`
- [ ] Include: title, slug, date, themeName, tags, excerpt, hook
- [ ] Verify JSON is valid (no trailing commas, proper quotes)

## Social Media Announcement

- [ ] Add blog announcement to `firebase/public/social-media-content.json`
- [ ] Use template: `blog-[slug]` as ID
- [ ] Set category: `"blog-announcement"`
- [ ] Include all 3 platforms: `["bluesky", "mastodon", "discord"]`
- [ ] Use direct link: `https://ascendant-continuum.web.app/blog/posts/[slug].html`
- [ ] Update metadata.totalPosts count
- [ ] Update metadata.categories.blog-announcement count

## Testing

- [ ] Open HTML file locally in browser
- [ ] Verify all content displays (no "undefined" or missing sections)
- [ ] Check CSS styling loads correctly
- [ ] Test all internal links work
- [ ] Verify breadcrumb navigation works
- [ ] Test on mobile viewport

## Deploy

- [ ] `git add firebase/public/blog/`
- [ ] `git commit -m "Add blog post: [Title]"`
- [ ] `git push origin main`
- [ ] `cd firebase`
- [ ] `firebase deploy --only hosting`
- [ ] Visit live site to verify: `https://ascendant-continuum.web.app/blog/posts/[slug].html`

## After Deploy

- [ ] Verify blog post appears on main blog index
- [ ] Test social media links work
- [ ] Check that automated posting queue has the announcement
- [ ] Confirm post will go out at next scheduled time (10 AM UTC for blog, 2 PM UTC for social)

---

## ⚠️ CRITICAL REMINDERS

1. **NEVER create placeholder HTML files** - every blog post MUST have full content
2. **Convert markdown to proper HTML** - don't link to .md files
3. **Test before deploying** - catch issues locally first
4. **Update both data.json AND social-media-content.json** - double announcement system

---

## Need Help?

See `blog/BLOG_CREATION_GUIDE.md` for detailed instructions and examples.
