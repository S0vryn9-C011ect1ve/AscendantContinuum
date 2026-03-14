/**
 * Lore Snippet Generator
 * 
 * Generates non-spoiler worldbuilding teasers from realm documentation.
 * Sources: realm docs (Emberforge, Verdant Sanctuary, Echo Fields, Dawn Citadel, Lantern Ascension)
 * Creates atmospheric lore posts that intrigue without revealing gameplay.
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ═══════════════════════════════════════════════════════════════
// REALM LORE DATA
// ═══════════════════════════════════════════════════════════════

const REALMS = {
    emberforge: {
        name: 'Emberforge',
        essence: 'Creation & Energy',
        description: 'A cosmic forge where creation itself is the ritual',
        atmosphere: 'Flames dance, embers glow, sparks become constellations',
        activities: ['Tap flames', 'Hold embers', 'Trace sigils of fire'],
        mystery: 'Each spark you release becomes a constellation for future seekers',
        theme: 'Your creations persist in the universe. Forever.',
        emoji: '🔥',
    },
    verdantSanctuary: {
        name: 'Verdant Sanctuary',
        essence: 'Growth & Reflection',
        description: 'A living garden where growth unfolds with gentle patience',
        atmosphere: 'Soft greens, flowing water, leaves that whisper secrets',
        activities: ['Tend cosmic flora', 'Watch roots spread through stardust', 'Listen to ancient songs'],
        mystery: 'Plants here remember every visitor, growing differently for each',
        theme: 'Slow growth is still growth. Every step matters.',
        emoji: '🌿',
    },
    echoFields: {
        name: 'Echo Fields',
        essence: 'Memory & Imagination',
        description: 'A realm where past and possibility intertwine',
        atmosphere: 'Shimmering echoes, translucent memories, infinite reflections',
        activities: ['Trace forgotten paths', 'Discover ritual fossils', 'Leave your mark in time'],
        mystery: 'Every ritual here echoes forward. Future seekers will find your traces.',
        theme: 'You are not alone. Others have walked here before you.',
        emoji: '✨',
    },
    dawnCitadel: {
        name: 'Dawn Citadel',
        essence: 'Wonder & Knowledge',
        description: 'A luminous sanctuary where seekers discover mysteries',
        atmosphere: 'Golden light, towering spires, infinite libraries of stars',
        activities: ['Unlock ancient wisdom', 'Solve cosmic riddles', 'Chart new constellations'],
        mystery: 'Some knowledge waits for specific visitors. Will it recognize you?',
        theme: 'Curiosity is the compass. Wonder is the way.',
        emoji: '🌅',
    },
    lanternAscension: {
        name: 'Lantern Ascension',
        essence: 'Liminal Peace',
        description: 'A threshold realm between waking and dreaming',
        atmosphere: 'Soft lantern glow, floating platforms, weightless serenity',
        activities: ['Float through twilight', 'Release glowing wishes', 'Rest in gentle darkness'],
        mystery: 'This realm appears only when you need it most',
        theme: 'Sometimes the bravest thing is to rest.',
        emoji: '🏮',
    },
};

// ═══════════════════════════════════════════════════════════════
// LORE SNIPPET GENERATION
// ═══════════════════════════════════════════════════════════════

/**
 * Generate lore snippet for a specific realm
 * @param {string} realmKey - Realm key from REALMS
 * @returns {Object} - Lore snippet object
 */
export function generateLoreSnippet(realmKey) {
    const realm = REALMS[realmKey];

    if (!realm) {
        throw new Error(`Unknown realm: ${realmKey}`);
    }

    return {
        type: 'lore',
        realm: realmKey,
        realmName: realm.name,
        essence: realm.essence,
        description: realm.description,
        atmosphere: realm.atmosphere,
        activities: realm.activities,
        mystery: realm.mystery,
        theme: realm.theme,
        emoji: realm.emoji,
        timestamp: new Date().toISOString(),
    };
}

/**
 * Get random realm
 * @param {Array<string>} exclude - Realm keys to exclude
 * @returns {string} - Realm key
 */
export function getRandomRealm(exclude = []) {
    const realms = Object.keys(REALMS).filter(r => !exclude.includes(r));
    return realms[Math.floor(Math.random() * realms.length)];
}

/**
 * Format lore snippet for social media
 * @param {Object} snippet - Lore snippet object
 * @param {string} platform - Target platform
 * @returns {Object} - Formatted content
 */
export function formatLoreForPlatform(snippet, platform = 'bluesky') {
    const { realmName, description, atmosphere, activities, mystery, theme, emoji } = snippet;

    // Build atmospheric content
    const sections = [];

    sections.push(`${emoji} The ${realmName}:\n`);
    sections.push(description + '.\n');
    sections.push(atmosphere + '.\n');

    // Activities (short list format)
    activities.forEach(activity => {
        sections.push(activity + '.');
    });

    sections.push('');
    sections.push(mystery + '.');
    sections.push('');
    sections.push(`_"${theme}"_`);

    const content = sections.join('\n').trim();

    // Platform-specific adjustments
    if (platform === 'mastodon') {
        // Mastodon formatting (use CW for spoiler-free intrigue)
        return {
            title: `${emoji} Realm Spotlight: ${realmName}`,
            body: content,
            type: 'lore',
            contentWarning: 'Worldbuilding (no spoilers)',
        };
    }

    if (platform === 'discord') {
        return {
            title: `${emoji} Realm Spotlight: ${realmName}`,
            body: content,
            type: 'lore',
            embed: {
                title: `${emoji} ${realmName}`,
                description: description,
                fields: [
                    { name: 'Essence', value: snippet.essence },
                    { name: 'Mystery', value: mystery },
                ],
                color: getRealmColor(snippet.realm),
            },
        };
    }

    return {
        title: `${emoji} Realm Spotlight: ${realmName}`,
        body: content,
        type: 'lore',
    };
}

/**
 * Get Discord embed color for realm
 * @param {string} realmKey - Realm key
 * @returns {number} - Discord color (hex as decimal)
 */
function getRealmColor(realmKey) {
    const colors = {
        emberforge: 0xff6b35, // Orange-red
        verdantSanctuary: 0x4ecdc4, // Teal-green
        echoFields: 0x95b8d1, // Soft blue
        dawnCitadel: 0xffd93d, // Golden yellow
        lanternAscension: 0xb8b8ff, // Lavender
    };
    return colors[realmKey] || 0x5865f2;
}

/**
 * Generate all realm lore snippets
 * @returns {Array<Object>} - Array of lore snippets
 */
export function generateAllLoreSnippets() {
    return Object.keys(REALMS).map(realmKey => generateLoreSnippet(realmKey));
}

// ═══════════════════════════════════════════════════════════════
// REALM DOCUMENTATION EXTRACTION (if files exist)
// ═══════════════════════════════════════════════════════════════

/**
 * Read realm documentation files if available
 * @returns {Object} - Realm doc contents
 */
function readRealmDocs() {
    const realmDocsPath = path.join(__dirname, '../../docs/design/realms');
    const docs = {};

    if (!fs.existsSync(realmDocsPath)) {
        return docs;
    }

    try {
        const files = fs.readdirSync(realmDocsPath);
        files.forEach(file => {
            if (file.endsWith('.md')) {
                const realmKey = path.basename(file, '.md').replace(/_/g, '');
                const content = fs.readFileSync(path.join(realmDocsPath, file), 'utf8');
                docs[realmKey] = content;
            }
        });
    } catch (error) {
        console.warn('⚠️ Could not read realm docs:', error.message);
    }

    return docs;
}

/**
 * Extract atmospheric descriptions from realm docs
 * @returns {Array<string>} - Array of atmospheric phrases
 */
export function extractAtmosphericPhrases() {
    const docs = readRealmDocs();
    const phrases = [];

    // Look for descriptive paragraphs (typically 20-150 chars)
    Object.values(docs).forEach(content => {
        const lines = content.split('\n');
        lines.forEach(line => {
            const stripped = line.replace(/^[#*-\s]+/, '').trim();
            if (stripped.length > 20 && stripped.length < 150 && /[a-z]/.test(stripped)) {
                phrases.push(stripped);
            }
        });
    });

    return phrases;
}

// ═══════════════════════════════════════════════════════════════
// CLI INTERFACE (for testing)
// ═══════════════════════════════════════════════════════════════

if (import.meta.url === `file://${process.argv[1]}`) {
    console.log('✨ Lore Snippet Generator\n');

    console.log('Available realms:');
    Object.keys(REALMS).forEach((key, index) => {
        console.log(`  ${index + 1}. ${REALMS[key].emoji} ${REALMS[key].name} (${REALMS[key].essence})`);
    });
    console.log('');

    // Generate sample snippet
    const realmKey = getRandomRealm();
    const snippet = generateLoreSnippet(realmKey);
    const formatted = formatLoreForPlatform(snippet);

    console.log(`📝 Sample Snippet:\n`);
    console.log(formatted.body);
    console.log('');

    // Check for realm docs
    const docs = readRealmDocs();
    console.log(`📚 Found ${Object.keys(docs).length} realm documentation files`);
}

export default {
    generateLoreSnippet,
    getRandomRealm,
    formatLoreForPlatform,
    generateAllLoreSnippets,
    extractAtmosphericPhrases,
    REALMS,
};
