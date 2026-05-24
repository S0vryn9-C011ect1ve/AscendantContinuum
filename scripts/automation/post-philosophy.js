/**
 * Philosophy Posting Script
 * Generates design philosophy post and posts to all platforms
 */

import { postToBluesky } from '../posting/post-to-bluesky.js';
import { postToMastodon } from '../posting/post-to-mastodon.js';
import { postAnnouncementToDiscord } from '../posting/post-to-discord.js';
import { getHook, customizeHook } from './viral-hooks-gaming.js';
import {
    ensureUrlAtEnd,
    normalizeDevelopmentClaims,
    normalizeForPublishing,
} from '../utils/publish-text-utils.js';
import {
    assertNoProhibitedContent,
    socialFingerprint,
} from '../utils/truth-and-dedupe-guard.js';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const POSTING_HISTORY_PATH = path.join(__dirname, '../../public/social/posting-history.json');
const MAIN_SITE_URL = 'https://ascendant-continuum.web.app/';

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
- Teaches future explorers
- Becomes discoverable archaeology

Early development rituals shape the universe's foundation.

Your gameplay is worldbuilding.

This is collective storytelling.`
    },
    {
        title: 'Respectful Monetization',
        hook: 'No loot boxes. No battle passes. No FOMO.',
        body: `Our monetization promise:

✅ Buy once, play forever
    ✅ All content available without paywalls
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

    const baseContent = normalizeDevelopmentClaims(`${finalHook}\n\n${theme.body}\n\n#IndieGameDev #GameDev #EthicalGaming`);
    const content = ensureUrlAtEnd(baseContent, MAIN_SITE_URL);

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

    const normalizedContent = normalizeForPublishing(post.content);
    assertNoProhibitedContent(normalizedContent, 'philosophy social post');

    // Post to all platforms
    const results = {
        timestamp: new Date().toISOString(),
        type: 'designPhilosophy',
        content: normalizedContent,
        hook: post.hook,
        theme: post.theme,
        platforms: {}
    };

    let history = { posts: [] };
    if (fs.existsSync(POSTING_HISTORY_PATH)) {
        history = JSON.parse(fs.readFileSync(POSTING_HISTORY_PATH, 'utf8'));
    }

    const fingerprint = socialFingerprint({ hook: post.hook, body: normalizedContent });
    const alreadyPosted = history.posts.some(entry => {
        const entryFingerprint = entry.fingerprint || socialFingerprint({
            hook: entry.hook || '',
            body: entry.content || entry.body || ''
        });
        return entryFingerprint === fingerprint;
    });

    if (alreadyPosted) {
        console.log('ℹ️ Skipping duplicate philosophy post (fingerprint match).');
        return;
    }

    results.fingerprint = fingerprint;

    try {
        // Bluesky
        console.log('📡 Posting to Bluesky...');
        const blueskyResult = await postToBluesky(normalizedContent);
        results.platforms.bluesky = blueskyResult;
        console.log(`✅ Bluesky: ${blueskyResult.url || 'Posted'}\n`);
    } catch (error) {
        console.error(`❌ Bluesky failed: ${error.message}\n`);
        results.platforms.bluesky = { success: false, error: error.message };
    }

    try {
        // Mastodon
        console.log('📡 Posting to Mastodon...');
        const mastodonResult = await postToMastodon(normalizedContent);
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
            description: normalizedContent,
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

    if (DRY_RUN) {
        console.log('DRY RUN: skipping posting history update');
    } else {
        history.posts.push(results);
        fs.writeFileSync(POSTING_HISTORY_PATH, JSON.stringify(history, null, 2));
        console.log('✅ Posting history updated');
    }

    console.log('\n🎉 Philosophy post published successfully!');
}

main().catch(error => {
    console.error('❌ Fatal error:', error);
    process.exit(1);
});
