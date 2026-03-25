#!/usr/bin/env node
/**
 * Additional Content Generator
 * Creates diverse social media content with authentic solo dev learning voice
 * Following: docs/CONTENT_VOICE_GUIDE.md
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// CONTENT ACCURACY: See docs/CONTENT_GUIDELINES.md for facts about developer, game, and voice
// Key: Solo dev (zero budget), artistic background, Copilot + free tools, no players yet

// New content following authentic solo dev voice
const NEW_CONTENT = [
    // Learning Unity - Honest Struggles
    {
        type: "devEducation",
        hook: "Spent 4 hours debugging this Unity error - turned out to be one line",
        body: "The error: 'NullReferenceException: Object reference not set'\n\nThe problem: Forgot to assign a public field in the Inspector\n\nLesson learned: Always check Inspector assignments before diving into code...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "devUpdate",
        hook: "Learning Unity's new Input System - way more powerful than I expected",
        body: "Old Input Manager: simple but limited\nNew Input System: complex but flexible\n\nJust got multi-touch gestures working with half the code. Worth the learning curve...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "behindScenes",
        hook: "Solo dev reality: what I accomplished this week",
        body: "Monday-Tuesday: Fixed 3 bugs from my list\nWednesday: Added gesture smoothing\nThursday-Friday: Learned shader basics\n\nNot glamorous, but steady progress. That's what matters...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "low"
    },
    {
        type: "devEducation",
        hook: "Here's how I'm using Addressables to keep build size under 100MB",
        body: "Started at 1.2GB - too big for mobile\n\nNow: 85MB core + on-demand realm loading\n\nKey: Split assets into logical groups, load only what's needed, cache locally...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "designPhilosophy",
        hook: "Building features that respect player attention",
        body: "Not building:\n- Infinite scroll feeds\n- Daily login pressure\n- Artificial wait timers\n\nBuilding instead:\n- 1-5 minute sessions\n- Progress saves instantly\n- Play when you want...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },

    // Budget & Resources
    {
        type: "behindScenes",
        hook: "What $2,400 in contractor work got me",
        body: "Hired artist for:\n- 5 realm concept art pieces\n- UI asset pack\n- Character designs\n\nResult: Professional look I couldn't achieve solo. Worth every dollar for a small budget...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "behindScenes",
        hook: "Free tools I use every day for solo dev",
        body: "VS Code (editing)\nGit + GitHub (version control)\nBlender (learning 3D, slowly)\nGIMP (quick image edits)\nObsidian (design docs)\n\nTotal cost: $0. Just time learning them...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "behindScenes",
        hook: "Learning when to DIY vs hire out",
        body: "DIYing: Programming, game design, system architecture\nHiring: Art, audio, music composition\n\nI could spend 6 months learning art... or 2 weeks earning money to hire an artist. Math is clear...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },

    // Technical Learning Moments
    {
        type: "devEducation",
        hook: "Object pooling in Unity - the performance win I should've done earlier",
        body: "Before: Instantiate/Destroy every frame = GC spikes, 40fps\nAfter: Pool of reusable objects = smooth 60fps\n\nPattern: Create pool at start, reuse objects, never destroy...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "devEducation",
        hook: "Learning async/await in Unity - cleaner code than coroutines",
        body: "Used to use coroutines for everything asynchronous\n\nThen discovered async/await with UniTask:\n- More readable\n- Better error handling\n- Easier to debug\n\nStill learning when to use which...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "devUpdate",
        hook: "Just implemented save/load with Firebase - cloud sync working",
        body: "Local saves are fragile (uninstall = data loss)\n\nAdded Firebase Realtime Database:\n- Cloud backup automatic\n- Cross-device sync\n- Offline support built-in\n\nStill testing edge cases...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "devEducation",
        hook: "ScriptableObjects finally clicked for me - here's the use case that helped",
        body: "Was using singleton managers for everything (messy)\n\nNow using ScriptableObjects for:\n- Game settings\n- Event channels\n- Shared data between scenes\n\nMuch cleaner, easier to test...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },

    // Game Design Process
    {
        type: "designPhilosophy",
        hook: "Designing for accessibility first, not as an afterthought",
        body: "Traditional: Build game, add accessibility later\nMy approach: Consider accessibility from day one\n\nResult: Features that help everyone, not just accommodate some players...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "devUpdate",
        hook: "Prototyped a feature, tested it, scrapped it. That's game dev",
        body: "Spent 3 days on puzzle mechanic\nPlaytested it (just me)\nRealized it wasn't fun\nDeleted it\n\nFeels bad, but that's iteration. Better to kill it now than ship unfun content...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "designPhilosophy",
        hook: "Planning features for eventual players (game still in dev)",
        body: "Building:\n- Time capsule messages between players\n- Asynchronous interactions\n- Legacy system for early adopters\n\nNo players yet, but designing like they're coming...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "devUpdate",
        hook: "Working on procedural generation - each test surprises me",
        body: "Building ritual generator that creates unique patterns\n\nEvery test run: different result\nSometimes beautiful\nSometimes broken\nAlways learning what constraints I need...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },

    // Mobile Development
    {
        type: "devEducation",
        hook: "Learning mobile performance the hard way",
        body: "Desktop/WebGL: 60fps no problem\nFirst Android build: 15fps, overheating phone\n\nFixed by:\n- Reducing draw calls\n- Lighter post-processing\n- Texture atlas batching\n\nNow: 45-60fps on mid-range phones...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "devUpdate",
        hook: "Touch controls are harder to design than I thought",
        body: "Keyboard/mouse: easy, precise\nTouch: imprecise, finger blocks view\n\nLearning:\n- Bigger touch targets\n- Visual feedback instantly\n- Gesture-based when possible\n\nStill iterating...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "devEducation",
        hook: "Testing on actual devices revealed problems the editor didn't show",
        body: "Unity Editor: Everything works perfectly\nActual phone: Font too small, UI clipped, colors washed out\n\nLesson: Test on real hardware early and often...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },

    // Workflow & Process
    {
        type: "behindScenes",
        hook: "My solo dev morning routine",
        body: "6am: Coffee + review yesterday's progress\n6:30-7am: Fix bugs from previous day\n7-9am: Deep work on new features\n9am: Break (crucial for sustainability)\n\nAfternoons: learning new skills, research...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "low"
    },
    {
        type: "behindScenes",
        hook: "Version control saved me today",
        body: "Broke the entire game trying a 'quick fix'\nGit reset --hard to yesterday\nLost 1 hour of work, but saved the project\n\nCommit early, commit often...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "low"
    },
    {
        type: "behindScenes",
        hook: "Building a game while learning Unity - progress update",
        body: "Month 1: Tutorials, basics, lots of confusion\nMonth 3: First playable prototype\nMonth 6: Core systems working\nNow: Polishing and adding content\n\nSlow but steady...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "devUpdate",
        hook: "Automated my build process - saves hours every week",
        body: "Before: Manual builds, upload, test = 30 mins each time\nNow: Git push → GitHub Actions builds everything\n\nSetup took a day, saving 2-3 hours/week...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },

    // Design Philosophy - Solo Dev Authentic
    {
        type: "designPhilosophy",
        hook: "Why I'm building a meditation game (not another shooter)",
        body: "Market says: Make battle royale/roguelike/survival game\nMy gut says: Build something calming\n\nFollowing gut. Market may not agree, but I'm learning tons...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "designPhilosophy",
        hook: "Solo dev advantage: can take creative risks big studios won't",
        body: "AAA studios: Safe bets, proven mechanics\nIndie solo: Weird experiments, niche ideas\n\nBuilding accessibility-as-gameplay. Risky? Yes. Interesting? Definitely...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "behindScenes",
        hook: "What keeps me motivated as a solo dev",
        body: "Not money (not profitable yet)\nNot glory (no one knows about it)\n\nIt's: Learning something new every day, seeing features come to life, building something that might help someone...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "low"
    },

    // Specific Technical Solutions
    {
        type: "devEducation",
        hook: "Here's how I'm handling colorblind accessibility",
        body: "Not just color filters - those are limited\n\nUsing:\n- Pattern overlays\n- Shape differentiation  \n- Symbol markers\n- Shader-based transformations\n\nTesting with Color Oracle simulator...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "devUpdate",
        hook: "Added screen reader support - way more complex than expected",
        body: "Thought it'd be: describe UI elements\nActually is: navigation order, context, interaction flow, dynamic content updates\n\nLearning: accessibility is deep and nuanced...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "devEducation",
        hook: "Learning shader programming through trial and error",
        body: "Week 1: Copied shaders, changed values randomly\nWeek 2: Started understanding variables\nWeek 4: Wrote first shader from scratch (simple, but mine)\n\nNow: Can modify shaders confidently...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "devUpdate",
        hook: "Integrated real lunar data into the game - API approach",
        body: "Using astronomy API to pull current moon phase\nRituals change based on actual lunar cycle\nFull moon = different experience than new moon\n\nTesting how players respond to real-world sync...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },

    // Learning Moments & Mistakes
    {
        type: "behindScenes",
        hook: "Mistakes I made early on (so you don't have to)",
        body: "1. No version control for first month (scary)\n2. Hardcoded everything (refactoring nightmare)\n3. No regular backups (lost work twice)\n4. Skipped mobile testing til late (painful)\n\nLearning...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "devEducation",
        hook: "Unity best practice I wish I'd known from day one",
        body: "Don't put logic in Update() unless it needs to run every frame\n\nBefore: 40 Update() methods = performance hit\nAfter: Events, coroutines, callbacks = clean & fast\n\nSeems obvious now...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "behindScenes",
        hook: "The bug that took 2 days to find (one character typo)",
        body: "Game crashed randomly on mobile\n2 days debugging\nFound it: 'transfrom' instead of 'transform'\n\nCompiler didn't catch it (it was in a string)\n\nLearning: typos are expensive...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "low"
    },

    // Future Planning (No players yet)
    {
        type: "devUpdate",
        hook: "Building community features before there's a community",
        body: "Game still in dev, zero players\n\nBut building:\n- Player message system\n- Achievement sharing\n- Async interactions\n\nPlanning ahead so features are ready when players arrive...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "designPhilosophy",
        hook: "Designing for the player experience I want to create",
        body: "Not designing for metrics or retention\nDesigning for: moments of discovery, calm focus, meaningful choices\n\nOnce there are players, their feedback will guide iteration...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "behindScenes",
        hook: "Solo dev challenge: getting feedback without players",
        body: "Can't A/B test without users\nCan't get feedback without players\n\nSo: Testing myself, watching gameplay videos, reading design post-mortems\n\nEventually need external feedback...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "low"
    },

    // Quick Tips
    {
        type: "devEducation",
        hook: "Quick Unity tip: Use [SerializeField] private instead of public",
        body: "Before: public variables everywhere (messy)\nAfter: [SerializeField] private (shows in Inspector, but encapsulated)\n\nCleaner code, same Inspector access...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    },
    {
        type: "devEducation",
        hook: "Pro tip: Unity Profiler is your best debugging friend",
        body: "Performance issue? Don't guess\n\nOpen Profiler:\n- See exact frame time breakdown\n- Find bottlenecks\n- Measure before/after optimizations\n\nGame changer for mobile performance...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "high"
    },
    {
        type: "devEducation",
        hook: "Learning C# while building a game - resources that helped",
        body: "Unity Learn tutorials (official, free)\nBrackeys YouTube (RIP, but videos still great)\nStack Overflow (obvious but essential)\nActual code practice (most important)\n\nCombination worked for me...",
        platforms: ["bluesky", "mastodon", "discord"],
        priority: "medium"
    }
];

/**
 * Add new content to content-bank.json
 */
function addNewContent() {
    const contentBankPath = path.join(__dirname, '../../public/social/content-bank.json');
    const contentBank = JSON.parse(fs.readFileSync(contentBankPath, 'utf-8'));

    console.log('\n═══════════════════════════════════════════════════════════\n');
    console.log('📝 ADDITIONAL CONTENT GENERATOR\n');
    console.log('═══════════════════════════════════════════════════════════\n');

    const startingId = Math.max(...contentBank.content.map(p => p.id)) + 1;

    const newPosts = NEW_CONTENT.map((post, index) => ({
        id: startingId + index,
        type: post.type,
        hook: post.hook,
        body: post.body,
        platforms: post.platforms,
        media: null,
        priority: post.priority,
        used: false
    }));

    // Update content distribution
    const typeCount = {};
    [...contentBank.content, ...newPosts].forEach(post => {
        typeCount[post.type] = (typeCount[post.type] || 0) + 1;
    });

    const updatedContentBank = {
        meta: {
            ...contentBank.meta,
            totalItems: contentBank.content.length + newPosts.length,
            lastUpdated: new Date().toISOString().split('T')[0],
            contentDistribution: {
                devUpdates: typeCount.devUpdate || 0,
                devEducation: typeCount.devEducation || 0,
                designPhilosophy: typeCount.designPhilosophy || 0,
                behindScenes: typeCount.behindScenes || 0,
                loreSnippet: typeCount.loreSnippet || 0
            }
        },
        content: [...contentBank.content, ...newPosts]
    };

    // Show summary
    console.log(`✓ Added ${newPosts.length} new posts\n`);
    console.log('Content Distribution:\n');
    console.log(`  Dev Updates: ${typeCount.devUpdate || 0}`);
    console.log(`  Dev Education: ${typeCount.devEducation || 0}`);
    console.log(`  Design Philosophy: ${typeCount.designPhilosophy || 0}`);
    console.log(`  Behind Scenes: ${typeCount.behindScenes || 0}`);
    console.log(`  Lore Snippets: ${typeCount.loreSnippet || 0}`);
    console.log(`\n  Total: ${updatedContentBank.meta.totalItems} posts\n`);

    console.log('Sample New Posts:\n');
    newPosts.slice(0, 5).forEach(post => {
        console.log(`[${post.id}] ${post.hook}`);
    });

    console.log(`\n... and ${newPosts.length - 5} more\n`);
    console.log('═══════════════════════════════════════════════════════════\n');

    // Save
    fs.writeFileSync(contentBankPath, JSON.stringify(updatedContentBank, null, 2));

    return { oldCount: contentBank.content.length, newCount: newPosts.length, total: updatedContentBank.meta.totalItems };
}

// Run generator
try {
    const result = addNewContent();
    console.log(`✅ Content generation complete!\n`);
    console.log(`Added ${result.newCount} new posts (${result.oldCount} → ${result.total} total)\n`);
} catch (error) {
    console.error('❌ Error generating content:', error);
    process.exit(1);
}
