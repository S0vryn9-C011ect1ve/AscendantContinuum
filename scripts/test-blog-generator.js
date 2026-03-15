import { generateBlogPost } from '../content/blog-post-generator.js';

console.log('Testing blog post generator...\n');

try {
    const post = generateBlogPost();
    console.log('✅ Post generated successfully!');
    console.log(`Title: ${post.title}`);
    console.log(`Theme: ${post.themeName}`);
    console.log(`Tags: ${post.tags.join(', ')}`);
    console.log(`Slug: ${post.slug}`);
    console.log(`Excerpt length: ${post.excerpt.length} chars`);
    console.log(`Content length: ${post.content.length} chars`);
    console.log(`\nFirst section:`);
    console.log(post.sections[0].heading);
} catch (error) {
    console.error('❌ Error:', error.message);
    console.error(error.stack);
}
