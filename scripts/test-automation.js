/**
 * Test Automation — Quick diagnostic for blog and social media automation
 * 
 * Tests both systems locally without posting to external services.
 * Run with: DRY_RUN=true node scripts/test-automation.js
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const projectRoot = path.join(__dirname, '..');

console.log('\n🔍 AUTOMATION DIAGNOSTICS\n');
console.log('═'.repeat(60));
console.log(`\n📂 Project root: ${projectRoot}\n`);

// ═══════════════════════════════════════════════════════════════
// 1. CHECK FILE STRUCTURE
// ═══════════════════════════════════════════════════════════════

console.log('\n📁 Checking file structure...\n');

const filesToCheck = [
    'scripts/automation/post-blog.js',
    'scripts/automation/content-scheduler.js',
    'scripts/content/blog-post-generator.js',
    'scripts/posting/post-to-bluesky.js',
    'scripts/posting/post-to-mastodon.js',
    'scripts/posting/post-to-discord.js',
    'public/social/content-bank.json',
    'public/social/posting-history.json',
    'firebase/public/blog/index.html',
    'firebase/public/blog/data.json',
    '.github/workflows/daily-blog.yml',
    '.github/workflows/daily-social.yml',
];

let allFilesExist = true;
for (const file of filesToCheck) {
    const fullPath = path.join(projectRoot, file);
    const exists = fs.existsSync(fullPath);
    console.log(`${exists ? '✅' : '❌'} ${file}`);
    if (!exists) allFilesExist = false;
}

// ═══════════════════════════════════════════════════════════════
// 2. CHECK CONTENT BANK
// ═══════════════════════════════════════════════════════════════

console.log('\n\n📊 Checking content bank...\n');

try {
    const contentBankPath = path.join(projectRoot, 'public/social/content-bank.json');
    const contentBank = JSON.parse(fs.readFileSync(contentBankPath, 'utf-8'));

    console.log(`✅ Total items: ${contentBank.meta.totalItems}`);
    console.log(`✅ Phase: ${contentBank.meta.phase}`);
    console.log(`✅ Content array length: ${contentBank.content.length}`);

    if (contentBank.content.length !== contentBank.meta.totalItems) {
        console.log(`⚠️  WARNING: Mismatch between meta.totalItems (${contentBank.meta.totalItems}) and actual content length (${contentBank.content.length})`);
    }

    // Check for unused items
    const unusedItems = contentBank.content.filter(item => !item.used);
    console.log(`✅ Unused items available: ${unusedItems.length}`);

    // Distribution
    const distribution = {};
    contentBank.content.forEach(item => {
        distribution[item.type] = (distribution[item.type] || 0) + 1;
    });
    console.log('\n📈 Content distribution:');
    Object.entries(distribution).forEach(([type, count]) => {
        console.log(`   ${type}: ${count}`);
    });
} catch (error) {
    console.log(`❌ Error reading content bank: ${error.message}`);
    allFilesExist = false;
}

// ═══════════════════════════════════════════════════════════════
// 3. CHECK BLOG DATA
// ═══════════════════════════════════════════════════════════════

console.log('\n\n📝 Checking blog data...\n');

try {
    const blogDataPath = path.join(__dirname, '..', '..', 'firebase/public/blog/data.json');
    const blogData = JSON.parse(fs.readFileSync(blogDataPath, 'utf-8'));

    console.log(`✅ Total blog posts: ${blogData.stats.totalPosts}`);
    console.log(`✅ Posts array length: ${blogData.posts.length}`);

    if (blogData.posts.length > 0) {
        const latestPost = blogData.posts[blogData.posts.length - 1];
        console.log(`✅ Latest post: "${latestPost.title}"`);
        console.log(`   Date: ${latestPost.date}`);
        console.log(`   Theme: ${latestPost.themeName}`);
    }

    // Check if blog index exists
    const blogIndexPath = path.join(projectRoot, 'firebase/public/blog/index.html');
    if (fs.existsSync(blogIndexPath)) {
        console.log('✅ Blog index page exists');
    } else {
        console.log('❌ Blog index page missing');
    }

    // Check if RSS exists
    const rssPath = path.join(projectRoot, 'firebase/public/blog/rss.xml');
    if (fs.existsSync(rssPath)) {
        console.log('✅ RSS feed exists');
    } else {
        console.log('⚠️  RSS feed not found (will be generated on first blog post)');
    }
} catch (error) {
    console.log(`❌ Error reading blog data: ${error.message}`);
}

// ═══════════════════════════════════════════════════════════════
//  4. CHECK ENVIRONMENT VARIABLES
// ═══════════════════════════════════════════════════════════════

console.log('\n\n🔐 Checking environment variables...\n');

const requiredEnvVars = [
    'BLUESKY_IDENTIFIER',
    'BLUESKY_PASSWORD',
    'MASTODON_ACCESS_TOKEN',
    'MASTODON_API_URL',
    'DISCORD_WEBHOOK_URL',
];

import dotenv from 'dotenv';
dotenv.config();

let allEnvVarsSet = true;
for (const envVar of requiredEnvVars) {
    const isSet = !!process.env[envVar];
    console.log(`${isSet ? '✅' : '❌'} ${envVar}`);
    if (!isSet) allEnvVarsSet = false;
}

// ═══════════════════════════════════════════════════════════════
// 5. SUMMARY
// ═══════════════════════════════════════════════════════════════

console.log('\n\n' + '═'.repeat(60));
console.log('📋 SUMMARY');
console.log('═'.repeat(60) + '\n');

if (allFilesExist) {
    console.log('✅ All required files exist');
} else {
    console.log('❌ Some required files are missing');
}

if (allEnvVarsSet) {
    console.log('✅ All environment variables are set');
} else {
    console.log('⚠️  Some environment variables are missing (expected for local testing)');
}

console.log('\n💡 NEXT STEPS:\n');

console.log('1. Test blog generation (dry-run):');
console.log('   DRY_RUN=true node scripts/automation/post-blog.js\n');

console.log('2. Test social media posting (dry-run):');
console.log('   DRY_RUN=true node scripts/automation/content-scheduler.js\n');

console.log('3. Deploy blog to Firebase hosting:');
console.log('   cd firebase && firebase deploy --only hosting\n');

console.log('4. Manually trigger GitHub Actions workflows:');
console.log('   - Go to https://github.com/YOUR_REPO/actions');
console.log('   - Select workflow (daily-blog or daily-social)');
console.log('   - Click "Run workflow"\n');

console.log('5. Check workflow runs for errors:');
console.log('   - https://github.com/YOUR_REPO/actions\n');

console.log('═'.repeat(60) + '\n');
