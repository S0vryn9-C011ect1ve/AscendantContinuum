/**
 * Fix Content Priorities for Better Variety
 * 
 * Adjusts priorities in content-bank.json to prevent topic repetition.
 * Ensures accessibility/colorblind posts are spread out across priority levels.
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const CONTENT_BANK_PATH = path.join(__dirname, '../../public/social/content-bank.json');

// Keywords to identify accessibility/colorblind content
const ACCESSIBILITY_KEYWORDS = /colorblind|colour.?blind|color.?blind|vision type|protanopia|deuteranopia|tritanopia|accessibility mode|accessibility.*innovation|accessibility.*feature/i;

function main() {
    console.log('🔧 Fixing content priorities for better variety...\n');

    // Load content bank
    const contentBank = JSON.parse(fs.readFileSync(CONTENT_BANK_PATH, 'utf8'));

    // Find all accessibility posts
    const accessibilityPosts = contentBank.content.filter(item => {
        const text = `${item.hook} ${item.body}`;
        return ACCESSIBILITY_KEYWORDS.test(text);
    });

    console.log(`📊 Found ${accessibilityPosts.length} accessibility/colorblind posts`);

    // Current priority distribution
    const highPriorityCount = accessibilityPosts.filter(p => p.priority === 'high').length;
    const mediumPriorityCount = accessibilityPosts.filter(p => p.priority === 'medium').length;
    const lowPriorityCount = accessibilityPosts.filter(p => p.priority === 'low').length;

    console.log(`   High priority: ${highPriorityCount}`);
    console.log(`   Medium priority: ${mediumPriorityCount}`);
    console.log(`   Low priority: ${lowPriorityCount}\n`);

    // Strategy: Keep only 3-4 accessibility posts at high priority
    // Move the rest to medium or low

    // Sort by uniqueness/importance
    const keyPosts = [
        1,  // "Most games add accessibility as an option. We made it a feature." (flagship)
        10, // "What if accessibility unlocked secrets" (design philosophy)
        29, // "This accessibility feature became our best marketing angle"
    ];

    let changeCount = 0;

    accessibilityPosts.forEach(post => {
        const isKeyPost = keyPosts.includes(post.id);

        if (isKeyPost && post.priority !== 'high') {
            console.log(`✓ Upgrading #${post.id} to HIGH (key post)`);
            post.priority = 'high';
            changeCount++;
        } else if (!isKeyPost && post.priority === 'high') {
            // Downgrade to medium
            console.log(`✓ Downgrading #${post.id} to MEDIUM: "${post.hook.substring(0, 60)}..."`);
            post.priority = 'medium';
            changeCount++;
        }
    });

    // Also boost some non-accessibility posts to high priority for variety
    const diversePosts = contentBank.content.filter(item => {
        const text = `${item.hook} ${item.body}`;
        return !ACCESSIBILITY_KEYWORDS.test(text) &&
            (text.includes('procedural') ||
                text.includes('moon') ||
                text.includes('NPC') ||
                text.includes('collective memory') ||
                text.includes('realm') ||
                text.includes('Digital Sunset'));
    });

    // Ensure at least 8-10 diverse high-priority posts
    const currentHighPriority = contentBank.content.filter(item => item.priority === 'high');
    const diverseHighPriority = currentHighPriority.filter(item => {
        const text = `${item.hook} ${item.body}`;
        return !ACCESSIBILITY_KEYWORDS.test(text);
    });

    console.log(`\n📊 Current high-priority distribution:`);
    console.log(`   Accessibility: ${currentHighPriority.length - diverseHighPriority.length}`);
    console.log(`   Other topics: ${diverseHighPriority.length}`);

    // Boost some diverse posts if needed
    if (diverseHighPriority.length < 10) {
        const toBoost = diversePosts
            .filter(p => p.priority === 'medium')
            .slice(0, 10 - diverseHighPriority.length);

        console.log(`\n🚀 Boosting ${toBoost.length} diverse posts to high priority:`);
        toBoost.forEach(post => {
            console.log(`   ✓ #${post.id}: "${post.hook.substring(0, 60)}..."`);
            post.priority = 'high';
            changeCount++;
        });
    }

    // Save changes
    contentBank.meta.lastUpdated = new Date().toISOString();
    fs.writeFileSync(CONTENT_BANK_PATH, JSON.stringify(contentBank, null, 2));

    console.log(`\n✅ Complete! Made ${changeCount} priority adjustments`);
    console.log(`📝 Updated: ${CONTENT_BANK_PATH}`);

    // Final stats
    const finalAccessibilityHigh = contentBank.content.filter(item => {
        const text = `${item.hook} ${item.body}`;
        return ACCESSIBILITY_KEYWORDS.test(text) && item.priority === 'high';
    }).length;

    const finalTotalHigh = contentBank.content.filter(item => item.priority === 'high').length;

    console.log(`\n📊 Final result:`);
    console.log(`   Total high-priority posts: ${finalTotalHigh}`);
    console.log(`   Accessibility posts (high): ${finalAccessibilityHigh} (~${Math.round(finalAccessibilityHigh / finalTotalHigh * 100)}%)`);
    console.log(`   Other topics (high): ${finalTotalHigh - finalAccessibilityHigh} (~${Math.round((finalTotalHigh - finalAccessibilityHigh) / finalTotalHigh * 100)}%)\n`);
}

main();
