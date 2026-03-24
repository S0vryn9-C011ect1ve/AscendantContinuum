import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Read content bank
const contentPath = path.join(__dirname, '../../public/social/content-bank.json');
const contentBank = JSON.parse(fs.readFileSync(contentPath, 'utf8'));

// 1. Remove post #142 (false $2,400 contractor claim)
const removeId = 142;
contentBank.content = contentBank.content.filter(post => post.id !== removeId);

// 2. Update post #144 to reflect reality (no hiring, DIY everything)
const post144 = contentBank.content.find(p => p.id === 144);
if (post144) {
    post144.hook = "Solo dev reality: doing everything myself";
    post144.body = "DIYing: Programming, game design, art, audio sourcing, system architecture, documentation\n\nNo budget for contractors. Using GitHub Copilot Pro to help with code. Free tools for everything else.\n\nLearning as I go...";
}

// 3. Create NEW lore-based posts from actual game features
const NEW_LORE_POSTS = [
    {
        type: "loreSnippet",
        hook: "The Five Realms exist beyond normal space",
        body: "Emberforge (fire/creation), Verdant Sanctuary (nature/growth), Echo Fields (memory/stars), Dawn Citadel (light/knowledge), Lantern Ascension (liminal reflection space)\n\nEach has unique rules, mysteries, and ways of teaching seekers...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "The Ascendant Flame watches from Emberforge",
        body: "One of six deities guiding different playstyles. The Flame celebrates growth, learning, playful experimentation.\n\nPhilosophy: 'Every spark of curiosity is a universe waiting to ignite.'\n\n1% chance to witness deity appearance...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "loreSnippet",
        hook: "Sigils are more than collectibles - they're your magical signature",
        body: "Double Flame (Emberforge), Blooming Loop (Verdant), Spiral Rune (Echo Fields), Radiant Star (Dawn Citadel), Lantern Orb (Ascension)\n\nCombine them to create rituals. First discoverer names the combination globally...",
        platforms: ["bluesky", "mastoan", "discord"],
        priority: "medium"
    },
    {
        type: "designPhilosophy",
        hook: "Real moon phases affect the game world",
        body: "New Moon: Hidden paths revealed\nFull Moon: Maximum power, all realms glow\nLunar Eclipse: Special realm opens for 3 hours\n\nIntegrated real astronomical data via API. The cosmos is playable...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "Echo Fields remembers every player who ever visited",
        body: "Past rituals become 'memory fossils' after 30 days. You can discover echoes from launch day, see patterns from players who came before.\n\nArchaeological gameplay. The game forgets nothing...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "NPCs have collective memory across ALL players",
        body: "Spark (Emberforge flame sprite) remembers what thousands of players tell him. Dialogue evolves based on community input.\n\nIf 1000 players mention 'creativity,' new rituals unlock.\n\nNever done before...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "devUpdate",
        hook: "Colorblind modes reveal different hidden content",
        body: "Protanopia: Crimson Veil runes in flames\nDeuteranopia: Emerald Mysteries in gardens\nTritanopia: Azure Pathways in stars\nAchromatopsia: Sacred geometry puzzles\n\nAccessibility as core gameplay innovation...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "Herald of Joyful Curiosity loves riddles",
        body: "One of the six Pantheon deities. Androgynous starlight figure with mischievous smile.\n\nWhispers: 'Have you tapped that shadow three times?'\n\nReveals hidden puzzle twists for explorers. Serendipity moments...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "loreSnippet",
        hook: "Verdant Sanctuary responds to how you treat it",
        body: "Nurture plants patiently: They bloom with rare secrets\nRush through: Standard rewards\nReturn frequently: Garden remembers you, changes appearance\n\nThe realm has memory. Gentleness is rewarded...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "designPhilosophy",
        hook: "1-5 minute sessions by design",
        body: "Complete ritual in 3 minutes\nLeaving? Beautiful 'Goodbye Ritual' plays\nDigital Sunset warns when you've played too long\n\nRespecting player time isn't optional - it's the foundation. Anti-addiction by design...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "Lantern Ascension is the space between journeys",
        body: "Liminal void where floating lanterns carry wishes. Release yours upward, join thousands of others ascending.\n\nSee ghostly traces of other players (anonymous, async). Pure meditation space...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "loreSnippet",
        hook: "Daily Constellation Challenge connects everyone",
        body: "Every player worldwide gets THE SAME procedural ritual each day. Share spoiler-free emoji grids (Wordle-style).\n\nGlobal community touchpoint. Creates daily ritual habit. Different success metrics (not just speed)...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "Dawn Citadel celebrates every discovery with pure joy",
        body: "NPCs genuinely cheer for you. Geometric light puzzles, prism refraction challenges, collaborative mysteries.\n\nThe Scribe of Knowledge: 'Your pattern reminds me of one from 1,000 years ago... beautiful.'\n\nRadiant accomplishment...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "loreSnippet",
        hook: "Screen Reader mode reveals audio-only secrets",
        body: "NPCs whisper different dialogue when VoiceOver/TalkBack active. Hidden sound-based rituals. Exclusive 'Storyteller's Sigil.'\n\nAccessibility unlocks content others can't access...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "Personal Sigils are procedurally generated from your journey",
        body: "Created after Day 4 based on:\n- Favorite realm (color palette)\n- Playstyle archetype (pattern)\n- Pantheon alignment (glow)\n- Play frequency (animation)\n\nYour unique magical signature. No two identical...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "loreSnippet",
        hook: "Ritual naming system: First discoverer names it forever",
        body: "Combine Double Flame + Glowing Thread = ?\n\nFirst player to discover gets to name the ritual. Name appears globally for all future players.\n\nHall of Fame for ritual creators. Legacy...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "Archivist of Bright Memories keeps every moment safe",
        body: "Pantheon deity: Silver-haired robed figure surrounded by floating books of light.\n\n'Every moment you've lived here is a treasure. I keep them safe, so you may revisit joy whenever you need.'\n\nReflection mode shows your entire journey timeline...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "loreSnippet",
        hook: "Meteor showers trigger global events",
        body: "Perseids (Aug 12): Falling sparks rain from sky\nReal-time: Active when actual meteor shower peaks\nCollect shooting star fragments (extra rare)\n\nAstronomy integrated into gameplay. Real celestial events matter...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "designPhilosophy",
        hook: "No fail states - only experimentation and discovery",
        body: "Wrong ritual combination? Playful animation: 'Hmm, these energies don't quite align... try another!'\n\nNo punishments. No timers (optional). No FOMO mechanics.\n\nPositive empowerment philosophy...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "loreSnippet",
        hook: "Infinite Loop Nexus evolves with you",
        body: "Central hub that reflects your journey:\n- Background shifts based on favorite realm\n- Changes with time of day (syncs real world)\n- Rare events transform it entirely\n\nYour space. Your story visualized...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    }
];

// Add to content bank with new IDs
const maxId = Math.max(...contentBank.content.map(p => p.id));
const newPosts = NEW_LORE_POSTS.map((post, index) => ({
    id: maxId + 1 + index,
    ...post,
    media: null,
    used: false
}));

contentBank.content.push(...newPosts);

// Update metadata
const typeDistribution = {};
contentBank.content.forEach(post => {
    typeDistribution[post.type] = (typeDistribution[post.type] || 0) + 1;
});

contentBank.meta = {
    totalItems: contentBank.content.length,
    contentDistribution: typeDistribution,
    lastUpdated: new Date().toISOString()
};

// Save
fs.writeFileSync(contentPath, JSON.stringify(contentBank, null, 2), 'utf8');

console.log('\n✓ Removed post #142 (false contractor claim)');
console.log('✓ Updated post #144 (budget reality)');
console.log(`✓ Added ${NEW_LORE_POSTS.length} new lore posts`);
console.log(`\nNew total: ${contentBank.content.length} posts`);
console.log('\nContent Distribution:');
Object.entries(typeDistribution).forEach(([type, count]) => {
    console.log(`  ${type}: ${count}`);
});

console.log('\n=== SAMPLE NEW LORE POSTS ===\n');
newPosts.slice(0, 5).forEach(post => {
    console.log(`[${post.id}] ${post.type}: ${post.hook}`);
});
console.log(`... and ${newPosts.length - 5} more lore posts`);
