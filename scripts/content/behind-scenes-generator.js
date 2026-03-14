/**
 * Behind-the-Scenes Generator
 * 
 * Creates "how we built X" content from technical documentation.
 * Sources: ARCHITECTURE.md, LEARNINGS.md, TOOLS.md
 * Format: Dev diary posts showcasing technical decisions and insights.
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ═══════════════════════════════════════════════════════════════
// TECHNICAL TOPICS & INSIGHTS
// ═══════════════════════════════════════════════════════════════

const TECHNICAL_TOPICS = {
    proceduralGeneration: {
        title: 'Procedural Generation System',
        context: 'Why we chose procedural over handcrafted content',
        insights: [
            'Infinite discovery - every ritual is unique',
            'Development efficiency - one system generates 10,000+ variations',
            'Personal experience - no two players see same sequence',
            'Real celestial events trigger procedural modifications',
        ],
        implementation: 'Algorithm pulls from 5 magical realms × 12 ritual types × lunar cycle data',
        benefit: 'Computational creativity meets infinite replayability',
    },
    accessibilityArchitecture: {
        title: 'Accessibility Mode Architecture',
        context: 'How we built 8 colorblind modes that unlock different content',
        insights: [
            'Custom shader variants for each vision type',
            'No duplicate UI assets - all generated from base colors',
            'Content visibility controlled by shader parameters',
            'Each mode has unique secret layer in scene',
        ],
        implementation: 'Unity Shader Graph + ScriptableObject configuration system',
        benefit: 'Accessibility that\'s gameplay, not just accommodation',
    },
    npcMemorySystem: {
        title: 'NPC Collective Intelligence',
        context: 'Building NPCs that remember what ALL players tell them',
        insights: [
            'Firebase Realtime Database stores global player interactions',
            'Dialogue system queries collective knowledge',
            'NPCs evolve responses based on community trends',
            'Privacy-preserving aggregation (no individual tracking)',
        ],
        implementation: 'Cloud Functions aggregate player choices → NPC dialogue adapts',
        benefit: 'Community genuinely shapes the narrative over time',
    },
    ritualPersistence: {
        title: 'Ritual Fossil System',
        context: 'How player rituals become permanent archaeological record',
        insights: [
            'Every completed ritual stored as "fossil" in Firestore',
            'Future players can discover past players\' rituals',
            'Spatial data stores where rituals occurred',
            'Launch-day fossils marked as "Founder\'s Echo"',
        ],
        implementation: 'Firestore geoqueries + time-sorted indexes for discovery',
        benefit: 'Every player contribution matters forever',
    },
    digitalSunset: {
        title: 'Digital Sunset Feature',
        context: 'Building ethical play limits without punishment',
        insights: [
            'Gentle reminder after 15 minutes of play',
            'No penalties for stopping - progress always saved',
            'Cosmetic reward for taking breaks (not playing)',
            'Universe evolution visible when you return',
        ],
        implementation: 'Session timer + push notifications + server-side world state updates',
        benefit: 'Respects player wellbeing while maintaining engagement',
    },
    webglOptimization: {
        title: 'WebGL Performance Optimization',
        context: 'Getting Unity game to run smoothly in browser',
        insights: [
            'Asset bundling strategy for fast initial load',
            'Texture compression for mobile devices',
            'Object pooling for particle effects',
            'LOD system for distant realm objects',
        ],
        implementation: 'Unity Addressables + WebGL build pipeline customization',
        benefit: 'Playable on any device without app install',
    },
    firebaseArchitecture: {
        title: 'Firebase Backend Architecture',
        context: 'Serverless backend without vendor lock-in',
        insights: [
            'Cloud Functions for game logic',
            'Firestore for player data + ritual fossils',
            'Firebase Auth with anonymous play support',
            'Hosting for WebGL build',
        ],
        implementation: 'Infrastructure as code (Firebase config files in repo)',
        benefit: 'Scales automatically, costs only what we use',
    },
};

// ═══════════════════════════════════════════════════════════════
// BEHIND-SCENES GENERATION
// ═══════════════════════════════════════════════════════════════

/**
 * Generate behind-the-scenes post from topic
 * @param {string} topicName - Topic key from TECHNICAL_TOPICS
 * @returns {Object} - Behind-scenes post object
 */
export function generateBehindScenesPost(topicName) {
    const topic = TECHNICAL_TOPICS[topicName];

    if (!topic) {
        throw new Error(`Unknown technical topic: ${topicName}`);
    }

    return {
        type: 'behindScenes',
        topic: topicName,
        title: topic.title,
        context: topic.context,
        insights: topic.insights,
        implementation: topic.implementation,
        benefit: topic.benefit,
        timestamp: new Date().toISOString(),
    };
}

/**
 * Get random technical topic
 * @param {Array<string>} exclude - Topic names to exclude
 * @returns {string} - Topic name
 */
export function getRandomTopic(exclude = []) {
    const topics = Object.keys(TECHNICAL_TOPICS).filter(t => !exclude.includes(t));
    return topics[Math.floor(Math.random() * topics.length)];
}

/**
 * Format behind-scenes post for social media
 * @param {Object} post - Behind-scenes post object
 * @param {string} platform - Target platform
 * @returns {Object} - Formatted content
 */
export function formatBehindScenesForPlatform(post, platform = 'bluesky') {
    const { title, context, insights, implementation, benefit } = post;

    // Build content - "Friday Dev Diary" style
    const sections = [];

    sections.push(`🛠️ Friday Dev Diary\n`);
    sections.push(`${title}\n`);
    sections.push(context + '\n');

    // Key insights
    insights.forEach((insight, index) => {
        sections.push(`${index + 1}. ${insight}`);
    });

    sections.push('');
    sections.push(`💡 Implementation: ${implementation}`);
    sections.push('');
    sections.push(`✨ Why it matters: ${benefit}`);

    const content = sections.join('\n').trim();

    // Platform-specific adjustments
    if (platform === 'discord') {
        return {
            title: `🛠️ Dev Diary: ${title}`,
            body: content,
            type: 'behindScenes',
            embed: {
                title: title,
                description: context,
                fields: insights.map((insight, i) => ({
                    name: `Insight ${i + 1}`,
                    value: insight,
                })),
            },
        };
    }

    return {
        title: `Friday Dev Diary: ${title}`,
        body: content,
        type: 'behindScenes',
    };
}

/**
 * Generate all available behind-scenes posts
 * @returns {Array<Object>} - Array of posts
 */
export function generateAllBehindScenesPosts() {
    return Object.keys(TECHNICAL_TOPICS).map(topicName =>
        generateBehindScenesPost(topicName)
    );
}

// ═══════════════════════════════════════════════════════════════
// DOCUMENTATION EXTRACTION
// ═══════════════════════════════════════════════════════════════

/**
 * Read technical docs if available
 * @returns {Object} - Document contents
 */
function readTechnicalDocs() {
    const docsPath = path.join(__dirname, '../..');
    const docs = {};

    const docPaths = [
        { key: 'architecture', path: 'docs/technical/ARCHITECTURE.md' },
        { key: 'learnings', path: 'LEARNINGS.md' },
        { key: 'tools', path: 'TOOLS.md' },
    ];

    docPaths.forEach(({ key, path: docPath }) => {
        try {
            const fullPath = path.join(docsPath, docPath);
            if (fs.existsSync(fullPath)) {
                docs[key] = fs.readFileSync(fullPath, 'utf8');
            }
        } catch (error) {
            console.warn(`⚠️ Could not read ${docPath}:`, error.message);
        }
    });

    return docs;
}

/**
 * Extract technical insights from documentation
 * @returns {Array<string>} - Array of insights
 */
export function extractTechnicalInsights() {
    const docs = readTechnicalDocs();
    const insights = [];

    // Look for bullet points in docs (common pattern for insights)
    Object.values(docs).forEach(content => {
        const bulletRegex = /^[\s-]*[-*+]\s+(.+)$/gm;
        let match;
        while ((match = bulletRegex.exec(content)) !== null) {
            const insight = match[1].trim();
            if (insight.length > 30 && insight.length < 200) {
                insights.push(insight);
            }
        }
    });

    return insights;
}

// ═══════════════════════════════════════════════════════════════
// CLI INTERFACE (for testing)
// ═══════════════════════════════════════════════════════════════

if (import.meta.url === `file://${process.argv[1]}`) {
    console.log('🛠️ Behind-the-Scenes Generator\n');

    console.log('Available topics:');
    Object.keys(TECHNICAL_TOPICS).forEach((key, index) => {
        console.log(`  ${index + 1}. ${TECHNICAL_TOPICS[key].title}`);
    });
    console.log('');

    // Generate sample post
    const topicName = getRandomTopic();
    const post = generateBehindScenesPost(topicName);
    const formatted = formatBehindScenesForPlatform(post);

    console.log(`📝 Sample Post: "${post.title}"\n`);
    console.log(formatted.body);
    console.log('');

    // Extract insights
    const insights = extractTechnicalInsights();
    if (insights.length > 0) {
        console.log(`💡 Found ${insights.length} insights in technical docs`);
    }
}

export default {
    generateBehindScenesPost,
    getRandomTopic,
    formatBehindScenesForPlatform,
    generateAllBehindScenesPosts,
    extractTechnicalInsights,
    TECHNICAL_TOPICS,
};
