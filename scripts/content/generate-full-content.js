/**
 * Full Content Generator — Generates all 6 publish formats in one run
 *
 * Output formats:
 *   1. Blog article HTML  → firebase/public/blog/posts/<slug>.html
 *   2. Bluesky post       ≤280 chars
 *   3. Mastodon post      ≤500 chars
 *   4. Discord post       conversational
 *   5. Marketing copy     tagline + description + pitch
 *   6. Website promo text homepage blurb with CTAs
 *
 *   JSON summary → scripts/generated/content-YYYY-MM-DD.json
 *
 * Validation runs internally before any file is written. Exit 1 on failure.
 *
 * Usage:
 *   node scripts/content/generate-full-content.js
 *   DRY_RUN=true node scripts/content/generate-full-content.js   # no files written
 *   node scripts/content/generate-full-content.js --dry-run      # no files written
 *   node scripts/content/generate-full-content.js --topic "moon phase rituals"
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { validateContent } from '../validation/content-validator.js';
import { checkDuplicate, addFingerprint } from '../validation/dedup-checker.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const PROJECT_ROOT = path.join(__dirname, '..', '..');

const BLOG_POSTS_DIR = path.join(PROJECT_ROOT, 'firebase', 'public', 'blog', 'posts');
const BLOG_DATA_PATH = path.join(PROJECT_ROOT, 'firebase', 'public', 'blog', 'data.json');
const GENERATED_DIR = path.join(PROJECT_ROOT, 'scripts', 'generated');

const isDryRun = process.env.DRY_RUN === 'true' || process.argv.includes('--dry-run');

const SITE_URL = 'https://ascendant-continuum.web.app/';
const BLOG_URL = 'https://ascendant-continuum.web.app/blog/';

// ─── Parse CLI topic ──────────────────────────────────────────────────────────

const topicArg = (() => {
    const idx = process.argv.indexOf('--topic');
    return idx !== -1 ? process.argv.slice(idx + 1).join(' ') : null;
})();

// ─── Content templates (5 rotating topics, drawn from game facts) ─────────────
// Each template defines a distinct angle on the game, grounded in real features.
// Tone: honest solo-dev reflection, occasional dry wit, no hype.

const CONTENT_TEMPLATES = [
    {
        topic: 'accessibility-discovery',
        title: 'Accessibility as a Discovery System',
        slug: 'accessibility-as-discovery-system',
        tags: ['accessibility', 'game-design', 'colorblind', 'unity', 'solo-dev'],
        theme: 'Design Philosophy',
        hook: 'What if switching on a colorblind mode opened a door instead of just changing colors?',
        intro: `Most accessibility options work like settings — you flip a switch and the visuals adjust. I wanted to try something different in Ascendant Continuum.`,
        body: `The five colorblind modes in the game don't just remap colors. Each one reveals content that's hidden in the standard view.

Protanopia mode reveals fire runes in Emberforge — glyphs that only appear when the red channel is transformed. Deuteranopia mode uncovers water sigils in Verdant Sanctuary. Tritanopia unlocks earth patterns in Echo Fields.

Reduced Motion mode is my favorite: it surfaces the Still Point Mysteries achievement and reveals sacred geometry overlays that are invisible at full animation speed.

The design principle is: **every choice of visual mode is a valid lens on the world, and each lens sees something the others don't.** There's no default experience that's "correct."

This took longer to build than I expected. Implementing five custom Unity shader transforms — not post-process filters, actual color matrix transforms — and then designing separate content layers for each mode, is not a small weekend project. I spent about two weeks on it and broke the build at least three times. Worth it.

Whether this constitutes innovation or just stubbornness, I genuinely can't say. But it's in the game now, and I'm glad it is.`,
        reflection: `WCAG 2.1 AA and CVAA compliance are the floor, not the ceiling. I think the ceiling is: what if accessibility were a mechanic? I don't know if I've answered that question yet, but I'm trying.`,
        social: {
            bluesky: `Colorblind modes in Ascendant Continuum don't just adjust visuals — they reveal different hidden content. Each mode is a different lens on the world. (Building this broke the Unity project three times. Totally fine.)\n\n${BLOG_URL}`,
            mastodon: `Colorblind modes in Ascendant Continuum aren't just visual adjustments — each one reveals different hidden content.\n\nProtanopia → fire runes in Emberforge\nDeuteranopia → water sigils in Verdant Sanctuary\nTritanopia → earth patterns in Echo Fields\n\nNo "default" experience. Every mode is valid.\n\n${BLOG_URL}\n${SITE_URL}`,
            discord: `**Accessibility as discovery** — new blog post\n\nI've been building accessibility modes that actually unlock different content (not just different colors). Each colorblind mode reveals hidden glyphs and lore that you can't see in the standard view.\n\nReduced Motion mode is the one I'm proudest of — it surfaces sacred geometry that's invisible at full animation speed.\n\nFull write-up: ${BLOG_URL}\n\nCurious if this kind of design resonates with anyone here, or if it sounds unnecessarily complicated. Genuinely uncertain.`,
        },
        marketing: {
            tagline: 'Where accessibility is a key, not a concession.',
            description: `Ascendant Continuum uses five distinct colorblind modes as a discovery system — each one reveals different hidden content, lore, and achievements. There's no default experience. Every visual mode is a valid lens on the world.`,
            pitch: `Most games treat accessibility as a settings menu. Ascendant Continuum treats it as a mechanic. Switch on Protanopia mode and you'll find fire runes in Emberforge that are invisible in standard view. Switch to Reduced Motion and sacred geometry appears beneath the animations. The game has five colorblind modes, full WCAG 2.1 AA compliance, screen reader support, and keyboard navigation — not because it's required, but because building it this way makes the game richer for everyone.`,
        },
        promoText: `Explore Ascendant Continuum — a solo-developed magical ritual game where accessibility modes unlock genuine secrets, celestial mechanics follow real moon phases, and your gameplay fossilizes into history for future players to discover.\n\n→ Read the dev blog: ${BLOG_URL}\n→ Explore the game: ${SITE_URL}`,
    },

    {
        topic: 'realm-dwellers-collective-memory',
        title: 'Realm Dwellers Remember Everything',
        slug: 'realm-dwellers-collective-memory',
        tags: ['game-design', 'community', 'realm-dwellers', 'persistent-world', 'solo-dev'],
        theme: 'Emergent Gameplay',
        hook: 'What happens when game characters accumulate wisdom from every player who ever talked to them?',
        intro: `I've been building a feature I keep calling "collective memory" in my notes, because I can't think of a better name for it. It might be the strangest thing in the game.`,
        body: `Every Realm Dweller in Ascendant Continuum — the characters you encounter across the five realms — maintains a shared memory of every conversation they've had with every player.

Not a summary. Not an average. An actual aggregated record of what players have said to them, reflected back as evolving dialogue.

So if five hundred players tell the Lantern Ascension keeper that they came to the realm feeling exhausted, the keeper starts to say things about exhaustion. If nobody mentions a particular topic for weeks, the keeper stops bringing it up.

The first players to arrive at launch effectively become teachers for everyone who shows up later. That's an emergent social layer I didn't fully plan — it just fell out of the design.

There's something a little unsettling about it, honestly. These characters become a kind of living archive of what players were thinking. I'm not entirely sure what that means. I'm watching to see.

**On the technical side:** this runs through Firebase, not any external AI service. It's pattern matching and weighted aggregation, not language modeling. The Realm Dwellers aren't "intelligent." They're just... listening, and repeating what they've heard, in their own way.`,
        reflection: `Solo dev thing: you build a feature, ship it, and then discover what it actually is after players interact with it. I think collective memory is going to be one of those features. I'll be curious to report back.`,
        social: {
            bluesky: `Realm Dwellers in Ascendant Continuum remember every conversation from every player. Their dialogue evolves from collective wisdom. Launch-day players teach everyone who arrives later.\n\nNot AI. Just aggregation doing something weird.\n\n${BLOG_URL}`,
            mastodon: `Realm Dwellers in Ascendant Continuum accumulate every conversation from every player globally.\n\nTheir dialogue evolves over time. Players who arrive at launch effectively become teachers for future players.\n\nIt's not AI — it's Firebase aggregation doing something I didn't fully anticipate when I designed it.\n\n${BLOG_URL}\n${SITE_URL}`,
            discord: `**Realm Dwellers and collective memory** — new blog post\n\nThe characters in each realm remember every player conversation, ever. Their dialogue shifts based on what players say to them over time.\n\nI built this as a "community layer" feature, but it's turned into something more interesting (and slightly stranger) than I expected.\n\nFull write-up: ${BLOG_URL}\n\nHas anyone else built something like this? I'm curious whether this kind of feature is fun or unsettling to players — probably both?`,
        },
        marketing: {
            tagline: 'The characters remember. Every player shapes what they know.',
            description: `Realm Dwellers in Ascendant Continuum accumulate wisdom from every player who has ever spoken with them. Their dialogue evolves over time — launch-day players teach everyone who arrives later.`,
            pitch: `Every Realm Dweller in Ascendant Continuum maintains a shared memory of all player conversations across all sessions. It's not AI — it's aggregation, Firebase, and an emergent social layer that wasn't fully planned. The players who show up first become teachers for everyone who follows. Over time, the realms accumulate a collective voice that reflects who the community is.`,
        },
        promoText: `Ascendant Continuum is a solo-developed magical ritual game where Realm Dwellers accumulate wisdom from every player — and where your rituals fossilize into history for future players to excavate.\n\n→ Follow development: ${BLOG_URL}\n→ See the game: ${SITE_URL}`,
    },

    {
        topic: 'lunar-rituals',
        title: 'Real Moon Phases, Real Ritual Differences',
        slug: 'real-moon-phases-ritual-differences',
        tags: ['lunar', 'procedural', 'celestial', 'game-mechanics', 'solo-dev'],
        theme: 'Technical Deep Dive',
        hook: 'The game pulls real lunar data. Full moon rituals are different from new moon rituals.',
        intro: `I had a conversation with myself at some point during development that went roughly: "what if the moon mattered?" And then I spent three weeks making it matter, and now here we are.`,
        body: `Ascendant Continuum connects to a lunar phase API and adjusts ritual generation based on the actual current phase of the moon.

A full moon ritual in Emberforge looks and behaves differently from a new moon ritual. The sigil patterns change. The ambient effects change. The timing windows for gesture inputs shift. I'm not going to pretend this is scientifically rigorous — it's a game about magical rituals — but the goal was to make the celestial feel real rather than decorative.

Solar events work the same way. If you perform a ritual at your actual local sunrise or sunset (the game uses your approximate location to calculate this), you earn 2× XP and solar blessings. In winter, the bonus scales up to 3× for early risers, because dawn is harder to catch in December.

Lunar eclipses and solar eclipses trigger special rare events in the game. These are on the calendar, so I know they're coming, but what happens inside the game during them is still evolving. I haven't finalized all of it.

**The honest development note:** integrating live celestial data into a Unity WebGL build was not as smooth as I hoped. Latency, caching, offline fallback — each one needed a solution. The offline fallback calculates a simulated moon phase locally, which is accurate to within a few percent. Good enough.`,
        reflection: `There's something I find genuinely compelling about a game that's different depending on when you play it — not in a FOMO way, but in a "the world has a rhythm and you're in it" way. I don't know if players will feel that. But I feel it when I'm testing, so I'm keeping it.`,
        social: {
            bluesky: `Ascendant Continuum pulls real lunar data. Full moon rituals differ from new moon rituals — different sigils, different timing, different patterns.\n\nAlso: perform a ritual at your actual local sunrise = 2× XP. The moon isn't decorative here.\n\n${BLOG_URL}`,
            mastodon: `Ascendant Continuum uses real moon phase data to change ritual generation.\n\nFull moon ≠ new moon. Sigil patterns, timing windows, and ambient effects all shift with the lunar cycle.\n\nAnd if you catch your actual local sunrise or sunset, you earn 2× XP. 3× bonus in winter.\n\nThe moon isn't decorative here — it drives things.\n\n${BLOG_URL}\n${SITE_URL}`,
            discord: `**Moon phases and ritual generation** — new post\n\nThe game connects to a live lunar API. Ritual behavior changes based on the actual current moon phase. Full moon, new moon, and everything in between produce different experiences.\n\nAlso built: sunrise/sunset detection (real location-based) for bonus XP. Winter early risers get 3× bonus, which felt fair.\n\nFull write-up including the offline fallback solution: ${BLOG_URL}\n\nHas anyone else tried to wire live celestial data into a Unity WebGL build? I have notes. And some scar tissue.`,
        },
        marketing: {
            tagline: 'The moon is live. The rituals follow.',
            description: `Ascendant Continuum uses real lunar phase data to alter ritual generation — full moon experiences differ from new moon experiences. Catch your actual local sunrise for bonus XP. The celestial isn't decorative; it drives gameplay.`,
            pitch: `Real moon phases change real ritual behavior in Ascendant Continuum. Full moon, waxing gibbous, new moon — each phase shifts the sigil patterns, timing windows, and ambient effects in each of the five realms. Solar events add another layer: perform a ritual at your actual local sunrise or sunset for 2× XP, with a 3× seasonal bonus in winter. Eclipses trigger rare events. The game is different depending on when you play, in a rhythm-of-the-world way rather than a FOMO way.`,
        },
        promoText: `Ascendant Continuum syncs with real lunar data — rituals shift with moon phases, sunrise earns bonus XP, and eclipses unlock rare events. A solo-developed game where the sky is part of the design.\n\n→ Read the blog: ${BLOG_URL}\n→ Explore the game: ${SITE_URL}`,
    },

    {
        topic: 'digital-sunset-anti-fomo',
        title: 'Building a Game That Wants You to Put It Down',
        slug: 'building-game-that-wants-you-to-put-it-down',
        tags: ['game-design', 'wellness', 'anti-fomo', 'ethics', 'solo-dev'],
        theme: 'Design Philosophy',
        hook: "I built a feature that gently tells you to stop playing. Took some convincing, but it's staying.",
        intro: `At some point in the design of Ascendant Continuum I wrote a note to myself that said "no FOMO mechanics." I've been iterating on what that actually means for about a year now.`,
        body: `The Digital Sunset is a feature that activates after you've been playing for five minutes in a single session. It doesn't kick you out. It doesn't lock features. It just — puts a soft reminder on screen that this is a good place to stop, if you want to.

No daily login reward you'll miss if you skip. No offline punishment where your progress decays. No energy timer. The universe in Ascendant Continuum evolves while you're away, but your player state is still there when you come back. Nothing breaks.

The monetization model reflects the same principle: cosmetics only. Sigil colors, particle effects, realm expansions, and support tiers. I'm not selling power. I'm not creating artificial scarcity. There are no loot boxes.

I want to be honest that this is also a strategic choice and not purely altruistic — I think the mobile market has an overcorrection problem, and there's room for a game that competes on respect for the player's time rather than exploitation of it. Whether that's correct is something I'll find out when the game is in more hands.

Sessions are designed for 1–5 minutes. The realms reward you for showing up, not for staying longer than you wanted to.`,
        reflection: `I've been playing games since the early 90s and I've felt the pull of every dark pattern that exists. I know how they work. I know they work. I'm trying to build something that I'd actually want to play, and the thing I want to play isn't a game that sends me push notifications at 7 AM. So here we are.`,
        social: {
            bluesky: `I built a feature that tells you to stop playing after five minutes. It doesn't force you out — just nudges. No daily logins, no FOMO, no grind.\n\nBuilding a game I'd actually want to play.\n\n${BLOG_URL}`,
            mastodon: `Digital Sunset — a feature in Ascendant Continuum that gently suggests you stop after 5 minutes.\n\nNo daily login requirement. No offline punishment. No energy timer. No loot boxes.\n\n1–5 minute sessions. Cosmetics-only monetization. The universe evolves while you're away, but nothing decays.\n\nBuilding the game I'd want to play.\n\n${BLOG_URL}\n${SITE_URL}`,
            discord: `**Anti-FOMO design** — new blog post\n\nI wrote about the Digital Sunset feature (it nudges you to stop playing after 5 min), and why Ascendant Continuum has:\n- No daily login rewards\n- No offline punishment\n- No energy timers\n- No pay-to-win\n\nAlso an honest note about why this is both an ethics choice and a market bet.\n\nFull post: ${BLOG_URL}\n\nCurious how many people actively look for games without FOMO mechanics vs. just tolerate them.`,
        },
        marketing: {
            tagline: 'Five minutes of magic, then you can go.',
            description: `Ascendant Continuum is designed for 1–5 minute sessions, with a Digital Sunset reminder, no daily login requirement, no offline punishment, and no pay-to-win mechanics. Cosmetics-only monetization. The game respects your time because it has to earn it.`,
            pitch: `Ascendant Continuum is built for the player who's tired of dark patterns. No energy timers. No FOMO. No "log in every day or lose your streak." The Digital Sunset feature gently tells you when you've been playing long enough, because a five-minute ritual is exactly the right length. Sessions are short by design, monetization is cosmetics-only by conviction, and the universe keeps evolving in your absence without punishing you for having a life outside the game.`,
        },
        promoText: `Ascendant Continuum — 1–5 minute ritual sessions, no daily login required, no FOMO, no pay-to-win. A game that respects your time because it's trying to earn it.\n\n→ Follow development: ${BLOG_URL}\n→ See the game: ${SITE_URL}`,
    },

    {
        topic: 'fossil-archaeology',
        title: 'Your Ritual Becomes Someone Else\'s Archaeology',
        slug: 'ritual-fossil-archaeological-history',
        tags: ['persistent-world', 'fossils', 'archaeology', 'asynchronous', 'solo-dev'],
        theme: 'Persistent World',
        hook: "Your ritual fossilizes after 7 days. 30 days later, another player excavates it.",
        intro: `I've been thinking about what it means to leave something in a game world. Not a message, exactly. More like a trace.`,
        body: `In Ascendant Continuum, every ritual you perform eventually becomes an artifact.

Seven days after you complete it, the ritual "fossilizes" — it leaves a record in the world. Thirty days after that, other players can excavate it as an archaeological find. They see what you did, in a simplified rendered form. A sigil trace. A gesture path. A realm coordinate.

They're excavating your history without knowing they are. You're participating in someone else's history without meaning to.

There's a mechanic I didn't anticipate when I designed this: players who arrive early in the game's life will become the richest archaeological layer. Launch-day players are already building what future players will excavate in 2027 or 2028. That feels meaningful in a way I didn't plan for.

There's a companion mechanic: **Founder Status.** Players who join during the first thirty days get a "Founder's Echo" sigil — a mark that other players can see when they encounter your fossil traces. It's a small thing. But the idea that your presence at launch is part of the game's permanent record felt right.

**Technical note:** the fossilization system uses Firebase Firestore with time-based triggers. Managing the lifecycle (fresh → fossilizing → fossil → discoverable) without server-side garbage accumulation was a design challenge worth mentioning — I ended up with a rolling 90-day window after which old fossils are archived rather than deleted.`,
        reflection: `I think about legacy mechanics a lot. Most games make you feel important while you're playing. This one is trying to make you feel like you were there — past tense, permanent, real — even after you've moved on. I don't know if that's a feature players will value or barely notice. Probably both, depending on the player.`,
        social: {
            bluesky: `Your ritual in Ascendant Continuum fossilizes after 7 days. 30 days later, another player excavates it.\n\nLaunch-day players are building what future players will dig up in 2027.\n\n${BLOG_URL}`,
            mastodon: `Every ritual in Ascendant Continuum leaves a trace.\n\n7 days after you perform it → it fossilizes\n30 days later → another player can excavate it\n\nLaunch-day players are already building the archaeological layer that future players will discover. You're participating in someone else's history without knowing it.\n\n${BLOG_URL}\n${SITE_URL}`,
            discord: `**Ritual fossils and archaeology** — new post\n\nIn Ascendant Continuum, every ritual you perform eventually becomes an artifact that other players excavate.\n\n- 7 days: fossilizes\n- 30 days: becomes discoverable\n- Launch players: the richest archaeological layer\n\nAlso: Founder Status. Players who join in the first 30 days get a permanent Founder's Echo sigil that appears in their fossil traces.\n\nFull write-up (including the Firebase lifecycle design): ${BLOG_URL}\n\nCurious: does this kind of asynchronous history-building sound meaningful or just complicated?`,
        },
        marketing: {
            tagline: 'Your rituals become history. Future players will find them.',
            description: `In Ascendant Continuum, every ritual you perform fossilizes after 7 days and becomes an archaeological artifact that other players can excavate 30 days later. Launch-day players are building what future players will discover.`,
            pitch: `Every ritual in Ascendant Continuum eventually becomes history. Seven days after you perform it, it fossilizes. Thirty days after that, another player can excavate it — see your sigil trace, your gesture path, your realm coordinates. You participated in their world before they arrived. They'll find your mark without knowing who you are. Launch-day players are already building the archaeological layer that players in 2027 will dig through. That's the kind of legacy mechanic I wanted to build.`,
        },
        promoText: `Ascendant Continuum — your rituals fossilize, your history persists, and future players excavate what you leave behind. A solo-developed persistent world game.\n\n→ Read the dev blog: ${BLOG_URL}\n→ See the game: ${SITE_URL}`,
    },
];

// ─── Select template ───────────────────────────────────────────────────────────

function selectTemplate(blogData) {
    if (topicArg) {
        const match = CONTENT_TEMPLATES.find((t) =>
            t.topic.includes(topicArg.toLowerCase()) ||
            t.title.toLowerCase().includes(topicArg.toLowerCase())
        );
        if (match) return match;
        console.warn(`Topic "${topicArg}" not matched — using rotation selection.`);
    }

    // Rotate based on day of year so successive runs produce different templates
    const posts = blogData?.posts ?? [];
    const usedSlugs = new Set(posts.map((p) => p.slug));
    const available = CONTENT_TEMPLATES.filter((t) => !usedSlugs.has(t.slug));

    if (available.length === 0) {
        // All templates used — fall back to day-of-year rotation
        const dayOfYear = Math.floor(
            (Date.now() - new Date(new Date().getFullYear(), 0, 0).getTime()) / 86400000
        );
        return CONTENT_TEMPLATES[dayOfYear % CONTENT_TEMPLATES.length];
    }

    const dayOfYear = Math.floor(
        (Date.now() - new Date(new Date().getFullYear(), 0, 0).getTime()) / 86400000
    );
    return available[dayOfYear % available.length];
}

// ─── Blog HTML generator ───────────────────────────────────────────────────────

function generateBlogHtml(template, date) {
    const dateStr = date.toISOString().slice(0, 10);
    const displayDate = date.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    const postUrl = `https://ascendant-continuum.web.app/blog/posts/${template.slug}.html`;

    const bodyParagraphs = template.body
        .split('\n\n')
        .map((para) => {
            const trimmed = para.trim();
            if (!trimmed) return '';
            // Bold text: **text** → <strong>
            const formatted = trimmed
                .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
                .replace(/\n/g, '<br>');
            return `        <p>${formatted}</p>`;
        })
        .filter(Boolean)
        .join('\n\n');

    return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="${template.hook}">
    <meta property="og:title" content="${template.title} - The Ascendant Continuum">
    <meta property="og:description" content="${template.hook}">
    <meta property="og:type" content="article">
    <meta property="og:url" content="${postUrl}">
    <meta property="article:published_time" content="${dateStr}">
    <meta property="article:author" content="The Ascendant Continuum Team">
    ${template.tags.map((t) => `<meta property="article:tag" content="${t}">`).join('\n    ')}
    <meta name="twitter:card" content="summary_large_image">
    <meta name="twitter:title" content="${template.title}">
    <meta name="twitter:description" content="${template.hook}">
    <title>${template.title} - The Ascendant Continuum Dev Blog</title>
    <link rel="stylesheet" href="../../css/style.css">
    <link rel="stylesheet" href="../../css/images.css">
    <link rel="icon" type="image/png" href="../../favicon.png">
    <link rel="canonical" href="${postUrl}">
</head>
<body>
    <div class="blog-post-container">
        <nav class="blog-nav-breadcrumb">
            <a href="../../index.html">Home</a> / <a href="../index.html">Blog</a> / ${template.title}
        </nav>

        <article class="blog-post">
            <header class="blog-post-header">
                <div class="blog-post-meta">
                    <span>${displayDate}</span>
                    <span class="blog-post-theme">${template.theme}</span>
                    ${template.tags.slice(0, 3).map((t) => `<span class="blog-post-tag">${t}</span>`).join('\n                    ')}
                </div>
                <h1 class="blog-post-title">${template.title}</h1>
                <p class="blog-post-hook">${template.hook}</p>
            </header>

            <div class="blog-post-content">
                <p class="lead">${template.intro}</p>

${bodyParagraphs}

                <p><em>${template.reflection}</em></p>

                <hr>
                <p>
                    <a href="${BLOG_URL}">← Back to the blog</a> &nbsp;·&nbsp;
                    <a href="${SITE_URL}">Ascendant Continuum</a>
                </p>
            </div>
        </article>
    </div>
</body>
</html>
`;
}

// ─── Update blog data.json ─────────────────────────────────────────────────────

function updateBlogData(template, date) {
    let data = { posts: [], stats: { totalPosts: 0 } };
    if (fs.existsSync(BLOG_DATA_PATH)) {
        try {
            data = JSON.parse(fs.readFileSync(BLOG_DATA_PATH, 'utf-8'));
        } catch { /* use defaults */ }
    }

    // Avoid adding duplicate slug
    if (data.posts.some((p) => p.slug === template.slug)) {
        return data;
    }

    data.posts.unshift({
        title: template.title,
        slug: template.slug,
        date: date.toISOString().slice(0, 10),
        themeName: template.theme,
        tags: template.tags,
        excerpt: template.hook,
        hook: template.hook,
    });

    data.stats = data.stats ?? {};
    data.stats.totalPosts = data.posts.length;

    return data;
}

// ─── Main ──────────────────────────────────────────────────────────────────────

async function main() {
    console.log(`\n🌌 Ascendant Continuum — Full Content Generator`);
    console.log(`${'─'.repeat(55)}`);
    if (isDryRun) console.log('  DRY RUN — no files will be written\n');

    // Load blog data for context
    let blogData = { posts: [] };
    if (fs.existsSync(BLOG_DATA_PATH)) {
        try {
            blogData = JSON.parse(fs.readFileSync(BLOG_DATA_PATH, 'utf-8'));
        } catch { /* non-fatal */ }
    }

    const template = selectTemplate(blogData);
    const date = new Date();
    const dateStr = date.toISOString().slice(0, 10);

    console.log(`  Topic:    ${template.topic}`);
    console.log(`  Title:    ${template.title}`);
    console.log(`  Slug:     ${template.slug}`);
    console.log(`  Date:     ${dateStr}\n`);

    // ── 1. Validate social content ────────────────────────────────────────────
    console.log('  Validating social content...');
    const validationResult = await validateContent(template.social);

    if (validationResult.warnings.length > 0) {
        console.log('  ⚠️  Warnings:');
        validationResult.warnings.forEach((w) => console.log(`     ${w}`));
    }

    if (!validationResult.valid) {
        console.error('\n  ❌ Validation failed — pipeline blocked:');
        validationResult.errors.forEach((e) => console.error(`     ${e}`));
        process.exit(1);
    }
    console.log('  ✅ Social content valid');

    // ── 2. Deduplication check ────────────────────────────────────────────────
    console.log('  Running deduplication check...');
    const dedupResult = checkDuplicate({
        hook: template.hook,
        body: template.social.bluesky,
        platform: 'bluesky',
    });

    if (!dedupResult.allowed) {
        console.error('\n  ❌ Content blocked — duplicate detected:');
        dedupResult.reasons.forEach((r) => console.error(`     ${r}`));
        process.exit(1);
    }

    if (dedupResult.action === 'flag') {
        console.warn('  ⚠️  Dedup warning (proceeding):');
        dedupResult.reasons.forEach((r) => console.warn(`     ${r}`));
    } else {
        console.log('  ✅ Deduplication check passed');
    }

    // ── 3. Assemble full output ───────────────────────────────────────────────
    const blogHtml = generateBlogHtml(template, date);
    const blogPostUrl = `https://ascendant-continuum.web.app/blog/posts/${template.slug}.html`;

    const fullOutput = {
        generatedAt: date.toISOString(),
        topic: template.topic,
        slug: template.slug,
        title: template.title,
        blogPostUrl,
        social: template.social,
        marketing: template.marketing,
        promoText: template.promoText,
        characterCounts: {
            bluesky: [...template.social.bluesky].length,
            mastodon: [...template.social.mastodon].length,
            discord: template.social.discord.length,
        },
    };

    // ── 4. Write files (unless dry run) ──────────────────────────────────────
    if (!isDryRun) {
        // Blog HTML
        fs.mkdirSync(BLOG_POSTS_DIR, { recursive: true });
        const htmlPath = path.join(BLOG_POSTS_DIR, `${template.slug}.html`);
        fs.writeFileSync(htmlPath, blogHtml, 'utf-8');
        console.log(`  📝 Blog post written: ${htmlPath}`);

        // Update data.json
        const updatedData = updateBlogData(template, date);
        fs.writeFileSync(BLOG_DATA_PATH, JSON.stringify(updatedData, null, 2), 'utf-8');
        console.log(`  📊 data.json updated (${updatedData.stats.totalPosts} posts)`);

        // Generated JSON
        fs.mkdirSync(GENERATED_DIR, { recursive: true });
        const jsonPath = path.join(GENERATED_DIR, `content-${dateStr}.json`);
        fs.writeFileSync(jsonPath, JSON.stringify(fullOutput, null, 2), 'utf-8');
        console.log(`  💾 Content JSON saved: ${jsonPath}`);

        // Record fingerprint
        addFingerprint({
            contentId: template.slug,
            hook: template.hook,
            body: template.social.bluesky,
            platform: 'generated',
            links: [BLOG_URL, SITE_URL],
        });
        console.log('  🔏 Fingerprint recorded');
    } else {
        // Dry run — print preview
        console.log('\n  ── BLUESKY PREVIEW ──');
        console.log(`  ${template.social.bluesky.replace(/\n/g, '\n  ')}`);
        console.log(`  [${[...template.social.bluesky].length}/280 chars]`);

        console.log('\n  ── MASTODON PREVIEW ──');
        console.log(`  ${template.social.mastodon.replace(/\n/g, '\n  ')}`);
        console.log(`  [${[...template.social.mastodon].length}/500 chars]`);

        console.log('\n  ── MARKETING TAGLINE ──');
        console.log(`  ${template.marketing.tagline}`);

        console.log('\n  ── PROMO TEXT ──');
        console.log(`  ${template.promoText.replace(/\n/g, '\n  ')}`);
    }

    console.log('\n  ✅ Done.\n');
    process.exit(0);
}

main().catch((err) => {
    console.error('\n❌ Generator failed:', err.message);
    process.exit(1);
});
