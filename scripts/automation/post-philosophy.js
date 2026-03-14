/**
 * Philosophy Posting Script
 * Generates design philosophy post and posts to all platforms
 */

import { postToBluesky } from '../posting/post-to-bluesky.js';
import { postToMastodon } from '../posting/post-to-mastodon.js';
import { postAnnouncementToDiscord } from '../posting/post-to-discord.js';
import { getHook, customizeHook } from './viral-hooks-gaming.js';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const POSTING_HISTORY_PATH = path.join(__dirname, '../../public/social/posting-history.json');

// Philosophy themes
const philosophyThemes = [
    {
        title: 'Ethical Design',
        hook: 'Building a game that respects your time, not exploits it.',
        body: `Anti-FOMO design principles:

- Sessions end naturally, not artificially
- No daily login rewards
- No time-limited content
- Progress respects breaks

Games should enhance life, not consume it.

This is our design philosophy.`
    },
    {
        title: 'Accessibility Innovation',
        hook: 'What if accessibility unlocked secrets instead of just being an option?',
        body: `8 colorblind modes.
Each one reveals DIFFERENT hidden content.

Accessibility isn't accommodation.
It's a discovery system.

Different vision = different universe secrets.

Innovation through inclusion.`
    },
    {
        title: 'Community First',
        hook: 'Your actions become permanent universe lore.',
        body: `Every ritual you complete:
- Persists forever
- Teaches future players
- Becomes discoverable archaeology

Launch-day players create the universe's foundation.

Your gameplay is worldbuilding.

This is collective storytelling.`
    },
    {
        title: 'Respectful Monetization',
        hook: 'No loot boxes. No battle passes. No FOMO.',
        body: `Our monetization promise:

✅ Buy once, play forever
✅ All content available to all players
✅ No psychological manipulation
✅ No pay-to-win mechanics

If we make DLC, it's meaningful expansion.
Not extraction.

Players over profits.`
    },
    {
        title: 'Mindful Play',
        hook: 'Sessions that end when you want them to, not when we tell you.',
        body: `Digital Sunset mode:
- Your choice of session length
- Natural completion prompts
- Progress is saved, not punished

We designed the game to be put down.

Because healthy gaming is sustainable gaming.

Respect the player's time.`
    }
];

/**
 * Generate philosophy post
 */
function generatePhilosophyPost() {
    // Select a random theme
    const theme = philosophyThemes[Math.floor(Math.random() * philosophyThemes.length)];

    // Get a viral hook
    const hook = getHook('designPhilosophy');

    // Use the theme's hook if viral hook fails
    const finalHook = hook || theme.hook;

    const content = `${finalHook}\n\n${theme.body}\n\n#IndieGameDev #GameDev #EthicalGaming`;

    return {
        hook: finalHook,
        content,
        theme: theme.title
    };
}

/**
 * Main execution
 */
async function main() {
    console.log('🧠 Philosophy Poster Starting...\n');

    // Generate post
    const post = generatePhilosophyPost();

    console.log(`Theme: ${post.theme}`);
    console.log('Generated content:');
    console.log('─'.repeat(50));
    console.log(post.content);
    console.log('─'.repeat(50));
    console.log();

    // Post to all platforms
    const results = {
        timestamp: new Date().toISOString(),
        type: 'designPhilosophy',
        content: post.content,
        hook: post.hook,
        theme: post.theme,
        platforms: {}
    };

    try {
        // Bluesky
        console.log('📡 Posting to Bluesky...');
        const blueskyResult = await postToBluesky(post.content);
        results.platforms.bluesky = blueskyResult;
        console.log(`✅ Bluesky: ${blueskyResult.url || 'Posted'}\n`);
    } catch (error) {
        console.error(`❌ Bluesky failed: ${error.message}\n`);
        results.platforms.bluesky = { success: false, error: error.message };
    }

    try {
        // Mastodon
        console.log('📡 Posting to Mastodon...');
        const mastodonResult = await postToMastodon(post.content);
        results.platforms.mastodon = mastodonResult;
        console.log(`✅ Mastodon: ${mastodonResult.url || 'Posted'}\n`);
    } catch (error) {
        console.error(`❌ Mastodon failed: ${error.message}\n`);
        results.platforms.mastodon = { success: false, error: error.message };
    }

    try {
        // Discord
        console.log('📡 Posting to Discord...');
        const discordAnnouncement = {
            title: `🧠 ${post.theme}`,
            description: post.content,
            type: 'designPhilosophy',
            url: results.platforms.bluesky?.url || null
        };
        const discordResult = await postAnnouncementToDiscord(discordAnnouncement);
        results.platforms.discord = discordResult;
        console.log(`✅ Discord: Posted\n`);
    } catch (error) {
        console.error(`❌ Discord failed: ${error.message}\n`);
        results.platforms.discord = { success: false, error: error.message };
    }

    // Update posting history
    let history = { posts: [] };
    if (fs.existsSync(POSTING_HISTORY_PATH)) {
        history = JSON.parse(fs.readFileSync(POSTING_HISTORY_PATH, 'utf8'));
    }

    history.posts.push(results);
    fs.writeFileSync(POSTING_HISTORY_PATH, JSON.stringify(history, null, 2));
    console.log('✅ Posting history updated');

    console.log('\n🎉 Philosophy post published successfully!');
}

main().catch(error => {
    console.error('❌ Fatal error:', error);
    process.exit(1);
});
