/**
 * Test Content Selection Logic
 * Simulates what the scheduler would pick for the next 10 posts
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const CONTENT_BANK_PATH = path.join(__dirname, '../../public/social/content-bank.json');
const POSTING_HISTORY_PATH = path.join(__dirname, '../../public/social/posting-history.json');

// Topic keyword groups for diversity tracking
const TOPIC_KEYWORDS = {
    'colorblind-accessibility': ['colorblind', 'colour.?blind', 'color.?blind', 'vision type', 'protanopia', 'deuteranopia', 'tritanopia', 'accessibility mode'],
    'npc-memory': ['npc', 'game character', 'collective memory', 'collective intelligence'],
    'moon-phases': ['moon', 'lunar', 'celestial'],
    'digital-sunset': ['digital sunset', 'rest', 'wellness'],
    'realms-lore': ['emberforge', 'verdant', 'echo fields', 'dawn', 'lantern', 'realm'],
    'procedural-generation': ['procedural', 'algorithm-created', 'procedurally'],
    'monetization': ['monetization', 'pay-to-win', 'dlc', 'cosmetics', 'revenue'],
    'unity-tech': ['unity profiler', 'webgl', 'draw calls', 'fps', 'shader'],
    'anti-fomo': ['fomo', 'daily login', 'grind', 'addiction'],
};

const config = {
    contentCooldownDays: 30,
    topicCooldownDays: 7,
};

function detectTopics(content) {
    const text = `${content.hook} ${content.body}`.toLowerCase();
    const topics = [];

    for (const [topic, keywords] of Object.entries(TOPIC_KEYWORDS)) {
        for (const keyword of keywords) {
            const regex = new RegExp(keyword, 'i');
            if (regex.test(text)) {
                topics.push(topic);
                break;
            }
        }
    }

    return topics;
}

function getRecentTopics(postingHistory, days = 7) {
    const cutoffDate = new Date();
    cutoffDate.setDate(cutoffDate.getDate() - days);

    const recentTopics = new Set();

    postingHistory.posts
        .filter(post => new Date(post.timestamp) > cutoffDate)
        .forEach(post => {
            if (post.topics) {
                post.topics.forEach(topic => recentTopics.add(topic));
            }
        });

    return recentTopics;
}

function main() {
    console.log('🧪 Testing Content Selection Logic\n');
    console.log('═'.repeat(70));

    // Load data
    const contentBank = JSON.parse(fs.readFileSync(CONTENT_BANK_PATH, 'utf8'));
    const postingHistory = JSON.parse(fs.readFileSync(POSTING_HISTORY_PATH, 'utf8'));

    // Show current state
    const usedCount = contentBank.content.filter(c => c.used).length;
    const unusedCount = contentBank.content.filter(c => !c.used).length;

    console.log(`\n📊 Content Bank Status:`);
    console.log(`   Total posts: ${contentBank.content.length}`);
    console.log(`   Used: ${usedCount}`);
    console.log(`   Available: ${unusedCount}\n`);

    // Recent topics
    const recentTopics = getRecentTopics(postingHistory, 7);
    console.log(`🚫 Topics posted in last 7 days: ${Array.from(recentTopics).join(', ') || 'none'}\n`);

    // Show last 5 posts
    console.log(`📜 Last 5 posts:`);
    postingHistory.posts.slice(-5).forEach(post => {
        console.log(`   ${post.timestamp.split('T')[0]} - #${post.contentId} (${post.contentType}): "${post.hook.substring(0, 50)}..."`);
    });

    console.log('\n' + '═'.repeat(70));
    console.log('\n🎯 Next 10 Posts (Simulation):\n');

    // Simulate next 10 posts
    for (let i = 1; i <= 10; i++) {
        // Find available content (not used)
        const available = contentBank.content.filter(c => !c.used);

        if (available.length === 0) {
            console.log(`\n   ${i}. ♻️  All content used, would reset pool\n`);
            contentBank.content.forEach(c => c.used = false);
            continue;
        }

        // Filter by topics
        const recentTopicsNow = getRecentTopics(postingHistory, 7);
        const topicDiverse = available.filter(item => {
            const topics = detectTopics(item);
            return !topics.some(topic => recentTopicsNow.has(topic));
        });

        const candidates = topicDiverse.length > 0 ? topicDiverse : available;

        // Score candidates
        const priorityScore = { high: 100, medium: 50, low: 25 };
        const scored = candidates.map(item => {
            let score = priorityScore[item.priority] || 0;
            const topics = detectTopics(item);
            return { item, score, topics };
        });

        scored.sort((a, b) => b.score - a.score);

        const selected = scored[0].item;
        const topics = scored[0].topics;

        // Mark as used
        selected.used = true;
        selected.lastUsed = new Date().toISOString();

        // Add to history
        postingHistory.posts.push({
            contentId: selected.id,
            contentType: selected.type,
            hook: selected.hook,
            topics: topics,
            timestamp: new Date().toISOString(),
        });

        console.log(`   ${i}. #${selected.id} [${selected.priority}] (${selected.type})`);
        console.log(`      "${selected.hook}"`);
        if (topics.length > 0) {
            console.log(`      Topics: ${topics.join(', ')}`);
        }
        console.log('');
    }

    console.log('═'.repeat(70));
    console.log('\n✅ Test complete! The scheduler will now provide much better variety.\n');
}

main();
