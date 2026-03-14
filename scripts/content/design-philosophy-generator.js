/**
 * Design Philosophy Generator
 * 
 * Extracts ethical gaming principles and design philosophy from core docs.
 * Sources: SOUL.md, CONSTITUTION.md, ACCESSIBILITY_SPEC.md
 * Creates thought leadership content on player-first design.
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ═══════════════════════════════════════════════════════════════
// PHILOSOPHY THEMES & PRINCIPLES
// ═══════════════════════════════════════════════════════════════

const PHILOSOPHY_THEMES = {
    ethicalDesign: {
        title: 'Ethical Game Design',
        principles: [
            'Respect player time (1-5 minute sessions)',
            'No FOMO mechanics or dark patterns',
            'No pay-to-win, cosmetics only',
            'Digital Sunset feature (reminds players to rest)',
            'Progress persists, no artificial grind',
        ],
        impact: 'Revenue from joy, not addiction',
    },
    accessibilityInnovation: {
        title: 'Accessibility as Innovation',
        principles: [
            'Different colorblind modes reveal different content',
            'Accessibility choices = gameplay discovery paths',
            '8 vision variations, each unlocks unique secrets',
            'Not accommodation, but core feature everyone wants',
        ],
        impact: 'Proof that accessibility makes games better for everyone',
    },
    respectfulMonetization: {
        title: 'Respectful Monetization',
        principles: [
            'Free core experience, always',
            'Cosmetics-only purchases',
            'No ads, no tracking, no data sale',
            'No battle passes or seasonal FOMO',
            'Support the game, not to win, but to express yourself',
        ],
        impact: 'Sustainable without exploitation',
    },
    communityFirst: {
        title: 'Community-First Design',
        principles: [
            'Player rituals become permanent "fossils" for others',
            'NPCs remember what ALL players tell them',
            'Launch-day players get "Founder\'s Echo" recognition',
            'Global daily challenges connect players across time zones',
            'Every player contribution matters forever',
        ],
        impact: 'Living world where community creates lasting legacy',
    },
    mindfulPlay: {
        title: 'Mindful Play',
        principles: [
            'Short sessions designed for busy lives',
            'No pressure to play daily',
            'Universe evolves while you\'re away',
            'Encourages rest (Digital Sunset)',
            'Progress feels meaningful, not mandatory',
        ],
        impact: 'Gaming that fits life, not consumes it',
    },
};

// ═══════════════════════════════════════════════════════════════
// PHILOSOPHY EXTRACTION
// ═══════════════════════════════════════════════════════════════

/**
 * Read philosophy docs if available
 * @returns {Object} - Document contents
 */
function readPhilosophyDocs() {
    const docsPath = path.join(__dirname, '../..');
    const docs = {};

    try {
        const soulPath = path.join(docsPath, 'SOUL.md');
        if (fs.existsSync(soulPath)) {
            docs.soul = fs.readFileSync(soulPath, 'utf8');
        }
    } catch (error) {
        console.warn('⚠️ Could not read SOUL.md:', error.message);
    }

    try {
        const constitutionPath = path.join(docsPath, 'CONSTITUTION.md');
        if (fs.existsSync(constitutionPath)) {
            docs.constitution = fs.readFileSync(constitutionPath, 'utf8');
        }
    } catch (error) {
        console.warn('⚠️ Could not read CONSTITUTION.md:', error.message);
    }

    try {
        const accessibilityPath = path.join(docsPath, 'docs/design/ACCESSIBILITY_SPEC.md');
        if (fs.existsSync(accessibilityPath)) {
            docs.accessibility = fs.readFileSync(accessibilityPath, 'utf8');
        }
    } catch (error) {
        console.warn('⚠️ Could not read ACCESSIBILITY_SPEC.md:', error.message);
    }

    return docs;
}

/**
 * Generate philosophy post from theme
 * @param {string} themeName - Theme key from PHILOSOPHY_THEMES
 * @returns {Object} - Philosophy post object
 */
export function generatePhilosophyPost(themeName) {
    const theme = PHILOSOPHY_THEMES[themeName];

    if (!theme) {
        throw new Error(`Unknown philosophy theme: ${themeName}`);
    }

    return {
        type: 'designPhilosophy',
        theme: themeName,
        title: theme.title,
        principles: theme.principles,
        impact: theme.impact,
        timestamp: new Date().toISOString(),
    };
}

/**
 * Get random philosophy theme
 * @param {Array<string>} exclude - Theme names to exclude
 * @returns {string} - Theme name
 */
export function getRandomTheme(exclude = []) {
    const themes = Object.keys(PHILOSOPHY_THEMES).filter(t => !exclude.includes(t));
    return themes[Math.floor(Math.random() * themes.length)];
}

/**
 * Format philosophy post for social media
 * @param {Object} post - Philosophy post object
 * @param {string} platform - Target platform
 * @returns {Object} - Formatted content
 */
export function formatPhilosophyForPlatform(post, platform = 'bluesky') {
    const { title, principles, impact } = post;

    // Build content
    const sections = [];

    sections.push(`${title}\n`);

    // List principles
    principles.forEach(principle => {
        sections.push(`• ${principle}`);
    });

    sections.push('');
    sections.push(`Impact: ${impact}`);

    const content = sections.join('\n').trim();

    // Platform-specific adjustments
    if (platform === 'discord') {
        return {
            title: title,
            body: content,
            type: 'designPhilosophy',
            embed: {
                title: title,
                description: principles.join('\n'),
                footer: impact,
            },
        };
    }

    return {
        title: title,
        body: content,
        type: 'designPhilosophy',
    };
}

/**
 * Generate all available philosophy posts
 * @returns {Array<Object>} - Array of philosophy posts
 */
export function generateAllPhilosophyPosts() {
    return Object.keys(PHILOSOPHY_THEMES).map(themeName =>
        generatePhilosophyPost(themeName)
    );
}

// ═══════════════════════════════════════════════════════════════
// QUOTE EXTRACTION (from docs)
// ═══════════════════════════════════════════════════════════════

/**
 * Extract powerful quotes from philosophy docs
 * @returns {Array<string>} - Array of quotes
 */
export function extractPhilosophyQuotes() {
    const docs = readPhilosophyDocs();
    const quotes = [];

    // Extract blockquotes from markdown
    Object.values(docs).forEach(content => {
        const quoteRegex = /^>\s*(.+)$/gm;
        let match;
        while ((match = quoteRegex.exec(content)) !== null) {
            const quote = match[1].trim();
            if (quote.length > 20 && quote.length < 280) {
                quotes.push(quote);
            }
        }
    });

    return quotes;
}

// ═══════════════════════════════════════════════════════════════
// CLI INTERFACE (for testing)
// ═══════════════════════════════════════════════════════════════

if (import.meta.url === `file://${process.argv[1]}`) {
    console.log('🧠 Design Philosophy Generator\n');

    console.log('Available themes:');
    Object.keys(PHILOSOPHY_THEMES).forEach((key, index) => {
        console.log(`  ${index + 1}. ${PHILOSOPHY_THEMES[key].title}`);
    });
    console.log('');

    // Generate sample post
    const themeName = getRandomTheme();
    const post = generatePhilosophyPost(themeName);
    const formatted = formatPhilosophyForPlatform(post);

    console.log(`📝 Sample Post: "${post.title}"\n`);
    console.log(formatted.body);
    console.log('');

    // Extract quotes
    const quotes = extractPhilosophyQuotes();
    if (quotes.length > 0) {
        console.log(`💬 Found ${quotes.length} quotes in philosophy docs`);
        console.log('   Sample:', quotes[0]);
    }
}

export default {
    generatePhilosophyPost,
    getRandomTheme,
    formatPhilosophyForPlatform,
    generateAllPhilosophyPosts,
    extractPhilosophyQuotes,
    PHILOSOPHY_THEMES,
};
