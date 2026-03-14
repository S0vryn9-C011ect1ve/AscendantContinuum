/**
 * Viral Hooks Library for Ascendant Continuum Game - Pre-Launch Phase
 * 
 * Scroll-stopping hooks optimized for gaming/dev audiences.
 * Organized by content type with engagement-tested patterns.
 * Rotates hooks every 30 days to avoid repetition.
 */

export const viralHooks = {
    // ═══════════════════════════════════════════════════════════════
    // DEV UPDATES (40% of content) - Progress posts, feature launches
    // ═══════════════════════════════════════════════════════════════
    devUpdates: [
        "Most games add accessibility as an option. We made it a feature.",
        "Just shipped a game mechanic that literally doesn't exist yet.",
        "Here's what 6 months of indie dev taught me about {topic}.",
        "Today I built something impossible.",
        "This feature broke my brain (in the best way).",
        "Spent all week on this. Worth it.",
        "Most indie devs skip this step. That's why they fail.",
        "Building in public: here's what's working (and what's not).",
        "I thought this would take 2 hours. It took 2 weeks. Here's why.",
        "Every game does {X}. We're doing {Y} instead.",
        "This one line of code changed everything.",
        "Prototyped → tested → scrapped → rebuilt. Here's the result.",
        "The hardest part of gamedev isn't coding. It's {topic}.",
        "Shipped v2.0 today. Here's everything that changed.",
        "What happens when you let players {action}? Chaos. Beautiful chaos.",
    ],

    // ═══════════════════════════════════════════════════════════════
    // DEV EDUCATION (30% of content) - Tutorials, how-tos, Unity tips
    // ═══════════════════════════════════════════════════════════════
    devEducation: [
        "Steal this Unity system for your game.",
        "Here's the exact system I use for {feature}.",
        "Most Unity devs get {topic} wrong. Here's the right way.",
        "If you only learn one thing about {topic}, learn this.",
        "I reverse-engineered {game mechanic}. Here's how it works.",
        "The Unity trick no one talks about:",
        "3 years of Unity mistakes condensed into 3 minutes.",
        "This accessibility implementation took 2 days. Worth every minute.",
        "How to build {feature} in Unity without {common problem}.",
        "Stop using {bad practice}. Do this instead.",
        "Unity optimization 101: {specific tip}",
        "The shader trick that saved me 40 hours:",
        "Procedural generation sounds scary. It's not. Here's why.",
        "How I made Unity UI work for 8 colorblind modes (without duplication).",
        "The performance boost you're missing: {optimization tip}",
        "Your players will notice this. (Most devs ignore it.)",
        "I tested 5 approaches. This one won.",
        "WebGL optimization nobody teaches: {specific technique}",
    ],

    // ═══════════════════════════════════════════════════════════════
    // DESIGN PHILOSOPHY (20% of content) - Ethical gaming, thought leadership
    // ═══════════════════════════════════════════════════════════════
    designPhilosophy: [
        "Building a game that respects your time, not exploits it.",
        "What if accessibility unlocked secrets instead of just being an option?",
        "Ethical game design is possible. Here's the proof.",
        "Most games are designed to be addictive. We're doing the opposite.",
        "The player fantasy nobody's building for:",
        "What happens when you remove FOMO from a game?",
        "Games don't have to manipulate players to succeed.",
        "Anti-grind philosophy: why we cap sessions at 5 minutes.",
        "Revenue from joy, not addiction.",
        "The game industry's biggest lie: {controversial take}",
        "What if every player mattered? Like, actually mattered?",
        "We're not building a game. We're building a universe.",
        "Accessibility as innovation, not obligation.",
        "The real cost of dark patterns (and why we refuse to use them).",
        "Player-first design sounds obvious. Almost nobody does it.",
        "What I learned ditching traditional progression systems:",
        "Games can be beautiful without being predatory.",
    ],

    // ═══════════════════════════════════════════════════════════════
    // BEHIND-THE-SCENES (10% of content) - Process, tools, architecture
    // ═══════════════════════════════════════════════════════════════
    behindScenes: [
        "Friday dev diary: here's what broke this week.",
        "Why we chose {tool/tech} over {alternative}.",
        "The architecture decision that saved us months:",
        "Here's my entire indie dev stack (and why).",
        "This is how procedural generation actually works:",
        "The git commit that changed the project's direction:",
        "What 1,000 commits taught me about {topic}.",
        "Our dev process in 60 seconds:",
        "The automation that runs this whole project:",
        "Real talk: here's what almost killed the project.",
        "Tools I can't live without (and the ones I regret using).",
        "Why we built our own {system} instead of using {off-the-shelf}.",
        "The debugging session from hell (and what I learned).",
        "Computational creativity meets game design:",
        "Efficiency hack: how we ship faster with less code.",
    ],

    // ═══════════════════════════════════════════════════════════════
    // LORE SNIPPETS (Pre-launch only: worldbuilding teasers, no spoilers)
    // ═══════════════════════════════════════════════════════════════
    loreSnippets: [
        "The {realm} realm: where {theme} becomes reality.",
        "Every spark you release becomes a constellation for future seekers.",
        "In the Emberforge, creation itself is the ritual.",
        "What if your choices left permanent marks on the universe?",
        "Five realms. Infinite possibilities. One universe.",
        "Magic isn't given. It's discovered.",
        "The universe remembers. Every ritual. Every choice. Forever.",
        "Where science fiction meets mythological wonder:",
        "Each realm teaches something different. What will you learn?",
        "Not a game world. A living universe.",
    ],
};

// ═══════════════════════════════════════════════════════════════
// POWER WORDS & MODIFIERS
// ═══════════════════════════════════════════════════════════════
// Use these to customize hooks dynamically

export const powerWords = {
    urgency: ["now", "today", "just", "this week"],
    curiosity: ["secret", "hidden", "nobody talks about", "the truth"],
    validation: ["here's why", "here's how", "the exact", "the real"],
    empowerment: ["steal this", "you can", "simple", "easy"],
    emotion: ["beautiful", "chaos", "worth it", "changed everything"],
    numbers: ["3 years", "2 days", "5 approaches", "one thing"],
    authority: ["tested", "proven", "exact system", "the right way"],
};

// ═══════════════════════════════════════════════════════════════
// PLATFORM-SPECIFIC ADAPTATIONS
// ═══════════════════════════════════════════════════════════════

export const platformAdaptations = {
    bluesky: {
        // Bluesky audience loves: technical depth, indie dev stories, community-first
        maxLength: 300,
        style: "conversational, technical, community-focused",
        emojiUsage: "minimal (only when it adds meaning)",
        threadingPreference: "yes (for tutorials and deep-dives)",
    },

    mastodon: {
        // Mastodon audience loves: ethical tech, open source, anti-corporate
        maxLength: 500,
        style: "thoughtful, philosophical, anti-commercial",
        emojiUsage: "rare (prefer descriptive language)",
        threadingPreference: "yes (for complex ideas)",
        contentWarnings: "use for mental health topics, game spoilers",
    },
};

// ═══════════════════════════════════════════════════════════════
// HOOK SELECTION LOGIC
// ═══════════════════════════════════════════════════════════════

/**
 * Get a random hook for a specific content type
 * @param {string} contentType - devUpdates, devEducation, designPhilosophy, behindScenes, loreSnippets
 * @param {Array} usedHooks - Array of recently used hooks to avoid repetition
 * @returns {string} Selected hook
 */
export function getHook(contentType, usedHooks = []) {
    const availableHooks = viralHooks[contentType] || [];

    // Filter out recently used hooks (30-day rotation)
    const unusedHooks = availableHooks.filter(hook => !usedHooks.includes(hook));

    // If all hooks have been used, reset the pool
    const hookPool = unusedHooks.length > 0 ? unusedHooks : availableHooks;

    // Return random hook from pool
    return hookPool[Math.floor(Math.random() * hookPool.length)];
}

/**
 * Customize a hook with specific topic/action
 * @param {string} hook - Base hook with {topic} or {action} placeholders
 * @param {object} replacements - Object with replacement values
 * @returns {string} Customized hook
 */
export function customizeHook(hook, replacements = {}) {
    let customized = hook;

    Object.keys(replacements).forEach(key => {
        const placeholder = `{${key}}`;
        customized = customized.replace(placeholder, replacements[key]);
    });

    return customized;
}

/**
 * Track hook usage for rotation
 * @param {string} hook - The hook that was used
 * @param {string} historyFile - Path to posting-history.json
 */
export function trackHookUsage(hook, historyFile) {
    // This will be implemented in content-scheduler.js
    // Stores hook + timestamp in posting-history.json
    // Filters out hooks older than 30 days for reuse
}

// ═══════════════════════════════════════════════════════════════
// EXAMPLE USAGE
// ═══════════════════════════════════════════════════════════════

// Get a dev education hook
// const hook = getHook('devEducation', ['Steal this Unity system for your game.']);
// console.log(hook); // Returns a different hook from the pool

// Customize a hook
// const customized = customizeHook(
//   "Here's what 6 months of indie dev taught me about {topic}.",
//   { topic: "accessibility design" }
// );
// console.log(customized); // "Here's what 6 months of indie dev taught me about accessibility design."
