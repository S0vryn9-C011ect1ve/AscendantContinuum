/**
 * Delete incorrect blog post from social media only (no repost)
 */

import { BskyAgent } from '@atproto/api';
import dotenv from 'dotenv';

dotenv.config();

// Post IDs to delete (budget breakdown post with WRONG $10K claim)
const BLUESKY_POST_URI = 'at://did:plc:lb4jy3h5kqhtgemunsuheqzd/app.bsky.feed.post/3mh7mm5ae6s2k';
const MASTODON_POST_ID = '116241464775116110';

async function deleteInaccuratePosts() {
    console.log('🗑️  Deleting inaccurate blog post from social media...');
    console.log('    Reason: Game built with $0 budget, not $10K\n');

    // Delete from Bluesky
    try {
        const agent = new BskyAgent({ service: 'https://bsky.social' });
        await agent.login({
            identifier: process.env.BLUESKY_IDENTIFIER,
            password: process.env.BLUESKY_PASSWORD
        });

        await agent.deletePost(BLUESKY_POST_URI);
        console.log('✅ Deleted from Bluesky');
    } catch (error) {
        console.log('⚠️  Bluesky deletion:', error.message);
    }

    // Delete from Mastodon
    try {
        const mastodonInstance = process.env.MASTODON_INSTANCE || 'https://mastodon.social';
        const response = await fetch(`${mastodonInstance}/api/v1/statuses/${MASTODON_POST_ID}`, {
            method: 'DELETE',
            headers: {
                Authorization: `Bearer ${process.env.MASTODON_ACCESS_TOKEN}`
            }
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }

        console.log('✅ Deleted from Mastodon');
    } catch (error) {
        console.log('⚠️  Mastodon deletion:', error.message);
    }

    console.log('\n✅ Done! Inaccurate posts removed.');
    console.log('   Blog post file deleted from website.');
    console.log('   Only accurate "From Healthcare to Game Dev" post remains.');
}

deleteInaccuratePosts().catch(console.error);
