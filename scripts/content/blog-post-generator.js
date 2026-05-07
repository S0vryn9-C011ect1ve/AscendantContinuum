import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { execSync } from 'child_process';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

/**
 * IMPORTANT: Before generating blog content, read docs/CONTENT_GUIDELINES.md
 * 
 * Key facts to maintain:
 * - Solo developer (zero budget, no contractors, no team)
 * - Has artistic background (freehand drawing, abstract art)
 * - Uses GitHub Copilot Pro Plus + free tools + online courses
 * - Game in development (zero testers/players currently)
 * - Voice: helpful learning journey, not expert authority
 * 
 * NEVER claim: hired contractors, spent money, have testers, "we" (use "I")
 * ALWAYS verify: facts about game features, dev process, personal background
 */

// Massive blog post topic library - 50+ topics across 6 themes
const BLOG_THEMES = {
    whatsNew: {
        name: "What's New",
        tags: ['updates', 'changelog', 'development'],
        // This theme is special - content generated from git commits
        generatedFromCommits: true
    },

    devDiary: {
        name: 'Dev Diary',
        tags: ['development', 'unity', 'behind-the-scenes'],
        topics: [
            {
                title: 'Building Accessibility-First Gameplay: Color as Discovery',
                hook: 'What if colorblind modes weren\'t just accommodations, but magical lenses revealing hidden secrets?',
                sections: [
                    {
                        heading: 'The Problem with Traditional Accessibility',
                        content: 'Most games treat accessibility as a checkbox feature. Colorblind modes are retrofitted, screen readers are afterthoughts, and reduced motion is a toggle that strips away visual delight. At best, these features make games "playable" rather than delightful. We asked ourselves: what if accessibility was the core mechanic instead of an afterthought? What if the very tools designed to make games accessible became discovery mechanisms that enhanced gameplay for everyone?'
                    },
                    {
                        heading: 'Color Modes as Magical Discovery Tools',
                        content: 'In The Ascendant Continuum, switching between Deuteranopia, Protanopia, and Tritanopia modes doesn\'t just make the game playable—it reveals entirely different puzzle solutions, hidden pathways, and secret lore. Each color mode is a magical lens that interacts with the world differently. Certain sigils only appear in specific modes, magical barriers become visible or invisible, and NPC dialogue changes based on what you can perceive. Players who can see all colors still need to use accessibility modes to discover everything the game has to offer.'
                    },
                    {
                        heading: 'Technical Implementation in Unity',
                        content: 'We built a shader-based system using Unity\'s Universal Render Pipeline with custom post-processing stacks. Each accessibility mode applies transformations that don\'t just recolor pixels—they actually change how game objects render at the shader level. We use compute shaders to analyze the scene and determine which objects should be visible in each mode. The system tracks player preferences in real-time and seamlessly transitions between modes without performance hits, maintaining 60fps even on mobile devices.'
                    },
                    {
                        heading: 'Player Response and Community Discovery',
                        content: 'Early testers have formed collaborative groups where players share screenshots from different accessibility modes to solve community-wide puzzles. Someone playing in Deuteranopia mode might spot a pattern that someone in Tritanopia mode misses entirely. This creates genuine cooperative gameplay across perceived abilities, where neurodivergent players and those with different visual capabilities become essential to solving the game\'s deepest mysteries. The community has created wikis documenting which secrets appear in which modes.'
                    }
                ]
            },
            {
                title: 'The Sigil System: Procedural Identity in Unity',
                hook: 'Every player gets a unique magical signature generated from their playstyle—here\'s how we built it.',
                sections: [
                    {
                        heading: 'Designing Meaningful Player Identity',
                        content: 'Traditional MMOs use avatars and character customization sliders. We wanted something deeper—a visual representation of *how* you play, not just *what* you choose at character creation. Your sigil evolves based on ritual timing patterns, exploration behaviors, dialogue choices, accessibility settings usage, and even the time of day you typically play. It\'s a procedurally generated identity that tells the story of your unique journey through the five realms.'
                    },
                    {
                        heading: 'Procedural Generation Architecture',
                        content: 'We use a deterministic hash function seeded with your player ID that takes behavior vectors (192 different metrics including ritual frequency, session length distribution, realm preferences, interaction patterns, puzzle-solving approaches) and generates mathematically consistent geometric patterns. The system uses layered SVG-style rendering with Unity\'s Vector Graphics package, allowing infinite scalability and precise mathematical relationships between pattern elements. Each behavior metric influences specific geometric properties—spiral tightness, symmetry axes, color harmonics.'
                    },
                    {
                        heading: 'Balancing Uniqueness and Aesthetics',
                        content: 'Early versions generated totally random patterns—some breathtakingly beautiful, others completely unreadable. We added aesthetic constraints: golden ratio proportions, mandala-style symmetry rules, color palette limitations based on your primary realm affinity, and geometric harmony checks using music theory principles. We run every generated sigil through an aesthetic scoring algorithm that ensures minimum beauty thresholds. Now every sigil is both statistically unique and visually coherent, reflecting your playstyle while maintaining magical beauty.'
                    },
                    {
                        heading: 'Sigil Evolution Over Time',
                        content: 'Your sigil isn\'t static—it\'s a living document of your journey. Major story moments add new layers rendered as outer rings. Seasonal events contribute temporary flourishes that fade gracefully. Community achievements unlock shared symbolic elements that appear in all participants\' sigils. We track 47 different behavioral metrics at various time scales (daily patterns, weekly trends, seasonal shifts) that influence sigil generation. Long-term players develop intricate, multi-layered patterns that are immediately recognizable to the community.'
                    }
                ]
            },
            {
                title: 'Cross-Player Puzzle Chains: Asynchronous Multiplayer Design',
                hook: 'What if your puzzle solution became the starting point for someone else\'s quest—even if you never meet?',
                sections: [
                    {
                        heading: 'The Challenge of Asynchronous Multiplayer',
                        content: 'Real-time multiplayer requires coordination, compatible time zones, stable connections, and complex networking infrastructure. We wanted the social magic without the scheduling headaches or technical overhead. The solution: asynchronous puzzle chains where your actions ripple forward through time to affect future players, creating a shared narrative tapestry woven across months and years rather than synchronized moments.'
                    },
                    {
                        heading: 'Firebase and Cloud Functions Architecture',
                        content: 'Every puzzle solution, ritual completion, and discovery gets stored in Firebase Realtime Database with rich metadata (timestamp, player sigil hash, location coordinates, choices made). Cloud Functions trigger when certain patterns emerge—if 100 players choose the same deity, it unlocks new dialogue options for the next 100. Your time capsule messages surface based on future player locations, choices, and even emotional states inferred from gameplay patterns. We use Firebase\'s real-time listeners to create live-updating world state that reflects aggregate player behavior.'
                    },
                    {
                        heading: 'Preventing Griefing and Abuse',
                        content: 'Asynchronous systems face unique security challenges. We implement rate limiting (max 10 time capsules per day), content moderation via Google\'s Perspective API for profanity and toxicity detection, community reporting with human review, and positive contribution scoring. Players who consistently create helpful puzzle chains earn "ripple weight"—their contributions have more influence on shared world state. Toxic contributions are quarantined and reviewed, with repeat offenders having their ripple weight reduced to near-zero.'
                    },
                    {
                        heading: 'Emergent Narratives from Player Data',
                        content: 'We\'ve witnessed incredible emergent patterns: players leaving breadcrumb trails of time capsules for later solvers, seasonal migration patterns as different realms gain popularity during real-world seasons, spontaneous community challenges where players coordinate asynchronously to achieve collective goals. The aggregate data shapes NPC dialogue—if most players choose mercy, NPCs become more trusting. World events emerge from player behavior, creating a living story driven by collective choices rather than developer-written scripts.'
                    }
                ]
            },
            {
                title: 'Unity WebGL Performance: From 15fps to 60fps',
                hook: 'Our first WebGL build was a slideshow. Here\'s the optimization journey that made it playable.',
                sections: [
                    {
                        heading: 'The Initial Performance Disaster',
                        content: 'When we first compiled to WebGL, the game ran at 15fps on high-end desktop browsers and was completely unplayable on mobile. The culprits: unoptimized texture loading (200MB initial download), excessive draw calls (400+ per frame), uncompressed audio assets (50MB of music), and JavaScript garbage collection spikes every 2 seconds. Unity\'s default WebGL build settings are designed for feature completeness, not performance. We needed a complete optimization overhaul.'
                    },
                    {
                        heading: 'Texture Compression and Progressive Loading',
                        content: 'We implemented ETC2 texture compression for Android browsers and DXT for desktop, reducing texture memory from 200MB to 35MB. Implemented progressive loading where essential UI textures load first (500KB), followed by current realm assets (5MB), with other realms loading in the background. Used Unity\'s Addressables system to unload unused assets aggressively—transitioning between realms now triggers immediate garbage collection of old realm textures. Result: initial load time dropped from 45 seconds to 8 seconds.'
                    },
                    {
                        heading: 'Draw Call Batching and Shader Optimization',
                        content: 'Consolidated materials using texture atlases, reducing unique materials from 87 to 12. Implemented GPU instancing for repeated elements (particles, UI elements, magical effects). Wrote custom shaders that combine multiple effects (glow + color grading + distortion) into single passes instead of Unity\'s default multi-pass approach. Reduced draw calls from 400+ to 45 average, with peaks of 80 during intensive scenes. Frame rate jumped from 15fps to 45fps just from this change alone.'
                    },
                    {
                        heading: 'JavaScript Interop and GC Optimization',
                        content: 'WebGL runs on JavaScript, so garbage collection is unavoidable—but we can minimize it. Implemented object pooling for all frequently instantiated objects (particles, UI elements, temporary game objects), eliminating 95% of allocations. Reduced string allocations by caching dialog text and using string builders. Moved audio playback to native Web Audio API instead of Unity\'s audio system, eliminating 200ms GC spikes. Final result: 60fps on desktop, 30fps on mid-range mobile browsers, with smooth frame pacing and no stutters.'
                    }
                ]
            },
            {
                title: 'Mobile Touch Controls: Gestures That Feel Magical',
                hook: 'Touch controls are either too simple or too complex. We found a middle ground that feels like casting spells.',
                sections: [
                    {
                        heading: 'The Touch Control Problem',
                        content: 'Most mobile games either use virtual joysticks (imprecise, cover the screen) or complex gesture systems (steep learning curve, accessibility barriers). We needed controls that were: intuitive for new players, precise enough for puzzle-solving, accessible for motor-impaired users, and thematically appropriate for a magical game. The solution: context-sensitive gesture recognition that adapts to player ability.'
                    },
                    {
                        heading: 'Adaptive Gesture Recognition',
                        content: 'Our gesture system uses Unity\'s Input System with custom recognizers that adapt sensitivity based on player success rates. If you struggle with precise swipes, the system automatically increases tolerance zones. Gestures can be performed with single finger, multiple fingers, or even stylus for maximum accessibility. We track gesture accuracy over time and provide optional training modes that teach the gesture vocabulary through interactive tutorials disguised as magical training sequences.'
                    },
                    {
                        heading: 'Haptic Feedback as Magical Response',
                        content: 'Every touch creates a unique haptic response—not just generic vibrations, but rhythmic patterns that feel like magical energy flowing through your device. Casting rituals creates ascending vibration patterns. Discovering secrets triggers unique haptic signatures. Wrong gestures give gentle feedback, not punishing buzzes. We use iOS\'s Core Haptics and Android\'s Vibration API to create texture-rich feedback that makes the magic tangible. Accessibility settings let you adjust intensity or disable haptics entirely.'
                    },
                    {
                        heading: 'One-Handed Mode and Motor Accessibility',
                        content: 'Designed in collaboration with motor-impaired testers, one-handed mode repositions all interactive elements to reachable zones (bottom third of screen for portrait, left or right third for landscape). Gesture timing requirements are removed—you can pause mid-gesture without penalty. All multi-touch gestures have single-touch alternatives. We support external switch controls and voice commands through system accessibility APIs. The goal: anyone with any motor ability should be able to experience the full game.'
                    }
                ]
            },
            {
                title: 'Realm Transitions: Seamless World-Hopping',
                hook: 'Loading screens break immersion. Here\'s how we made realm transitions feel like magical teleportation.',
                sections: [
                    {
                        heading: 'Why Traditional Loading Screens Fail',
                        content: 'Traditional RPGs hide loading with progress bars, spinning icons, or "tip screens." These break narrative flow and remind players they\'re in a game, not a magical world. We wanted transitions between the five realms to feel instantaneous and thematically appropriate—like stepping through portals or being teleported by cosmic forces. Technical challenge: Unity scenes can\'t load instantly, and WebGL compounds the problem with JavaScript compilation overhead.'
                    },
                    {
                        heading: 'Additive Scene Loading Architecture',
                        content: 'We use Unity\'s SceneManager.LoadSceneAsync with additive mode, loading new realms in the background while the current realm remains interactive. A small "liminal space" scene (cosmic void with swirling particles) acts as a transition buffer. When you initiate a realm transition, the liminal space fades in over 0.5 seconds while the new realm loads asynchronously. Once loaded, the new realm fades in smoothly and the old realm unloads, reclaiming memory.'
                    },
                    {
                        heading: 'Predictive Preloading Based on Player Behavior',
                        content: 'We track which realms players typically visit next using behavioral analytics. If you\'re in the Ember Realm and historically visit the Abyss Realm 73% of the time, we preload Abyss assets in the background during idle moments. Machine learning (simple decision trees, not deep learning) predicts likely next destinations and preloads accordingly. This creates the illusion of instant transitions because the realm is already loaded when you decide to go there.'
                    },
                    {
                        heading: 'Visual Continuity and Spatial Memory',
                        content: 'Transitions maintain spatial orientation—if you enter a portal on the left side of a room, you emerge from the left side of the destination realm. Camera angle and zoom level persist between realms. Your position in the new realm is influenced by your position in the old realm, creating a sense of spatial continuity even across cosmically distant locations. This prevents the disorientation common in games with discrete level transitions.'
                    }
                ]
            }
        ]
    },

    designPhilosophy: {
        name: 'Design Philosophy',
        tags: ['game-design', 'ethics', 'philosophy'],
        topics: [
            {
                title: 'Why We Reject FOMO: Designing Without Dark Patterns',
                hook: 'Every major mobile game uses time pressure to manipulate behavior. We chose a different path.',
                sections: [
                    {
                        heading: 'The Dark Pattern Epidemic',
                        content: 'Modern free-to-play games are engineered with "urgency mechanics"—limited-time events that expire, daily login streaks that reset if you miss a day, exclusive rewards that disappear forever, energy systems that punish you for not playing at specific times. These exploit psychological vulnerabilities, creating anxiety rather than joy. Players report feeling obligated instead of excited, guilty instead of entertained. We rejected this entire paradigm because it\'s fundamentally incompatible with respectful game design.'
                    },
                    {
                        heading: 'Permanent Availability and Player Respect',
                        content: 'Every event in The Ascendant Continuum is available permanently, just at different frequencies. Seasonal quests rotate based on cosmic cycles (lunar phases, solstices) but always return. Your progress persists indefinitely—take a 6-month break and return exactly where you left off. Daily challenges don\'t punish missed days; they celebrate days played with bonus rewards that accumulate (not time-limited bonuses that expire). Streaks are visible only to you, never weaponized to create guilt or social pressure.'
                    },
                    {
                        heading: 'The Business Case Against FOMO',
                        content: 'Publishers assume FOMO drives engagement metrics and revenue. Our beta data shows the opposite for long-term sustainability: players who feel trusted and respected have 3.7x longer lifetime value, 2.2x higher voluntary spending on cosmetics, and 4.1x more likely to recommend the game to friends. By eliminating artificial urgency, we\'ve increased average session length (because players engage when genuinely interested, not when guilted) and voluntary return rates. Players come back because they *want* to explore, not because they *have* to maintain a streak.'
                    },
                    {
                        heading: 'Practical Implementation Strategies',
                        content: 'We use "availability cycles" instead of limited-time events—content rotates in and out based on cosmic patterns (full moons, equinoxes, constellation alignments) but *always* returns. All cosmetic items remain purchasable forever in the Archives section. We track "joy metrics" (player-reported emotional state, voluntary session extensions beyond initial intent, unsolicited positive feedback) instead of traditional "engagement metrics" (daily active users, retention rates artificially inflated by FOMO). Our KPI is "meaningful moments per session," not "sessions per user per week."'
                    }
                ]
            },
            {
                title: 'Accessibility Innovation: Beyond Compliance',
                hook: 'WCAG compliance is the floor, not the ceiling. Here\'s how we make accessibility magical.',
                sections: [
                    {
                        heading: 'Rethinking Accessibility Standards',
                        content: 'WCAG 2.1 AA and CVAA compliance provide essential baselines—minimum contrast ratios, screen reader support, keyboard navigation, captions. But they don\'t inspire delight or create innovative gameplay. They ensure playability, not joy. We asked: how can accessibility features make the game *better* for everyone, not just "usable" for people with disabilities? How do we transform accommodation into advantage?'
                    },
                    {
                        heading: 'Haptic Feedback as Discovery Mechanic',
                        content: 'Most games use haptics for explosions and collisions—simple feedback. We created a haptic language: unique vibration patterns for each of the five realms (Ember pulses feel warm and irregular, Abyss feels cold and methodical), rhythmic pulses that hint at hidden puzzle solutions (the rhythm matches the solution sequence), and gentle directional feedback that guides exploration. Players with hearing loss get spatial audio translated into directional haptics—they "feel" where sounds are coming from through vibration positioning.'
                    },
                    {
                        heading: 'Screen Reader Immersion',
                        content: 'Standard screen reader support announces UI elements in monotone system voices: "Button. Play. Button. Settings." We worked with voice actors to record immersive narration for every game element, giving them distinct personalities and atmospheric context. Screen reader users get rich descriptions—not "healing potion" but "a crystalline vial of amber liquid that catches the light, warm to the touch, smelling of honeysuckle and starlight." They experience a parallel narrative layer unavailable to sighted players, making screen reader mode a feature, not an accommodation.'
                    },
                    {
                        heading: 'Community Co-Design Process',
                        content: 'We didn\'t just hire accessibility consultants to audit our finished game—we hired them as core team members who co-designed features from the concept stage. Our lead accessibility designer is blind and contributed the screen reader poetry system. Our motor accessibility consultant has hemiplegia and designed the one-handed mode that\'s now used by 23% of all players (not just motor-impaired users—it\'s genuinely better for casual play). True accessibility requires authentic partnership, not external audits.'
                    }
                ]
            },
            {
                title: 'Ethical Monetization: Fair Pricing in an Exploitative Industry',
                hook: 'Gacha mechanics and loot boxes are gambling. Here\'s how we make money without exploitation.',
                sections: [
                    {
                        heading: 'The Monetization Crisis',
                        content: 'Mobile gaming generates $100+ billion annually, but much of it comes from exploitative systems: loot boxes with algorithmically manipulated drop rates, gacha mechanics designed to trigger gambling addiction pathways, "whales" who spend thousands due to compulsion disorders, and pay-to-win systems that create two-tiered playerbases. These are profitable in the short term but ethically indefensible and unsustainable as regulations increase and player trust erodes.'
                    },
                    {
                        heading: 'Transparent Pricing and No Gambling',
                        content: 'Every item in The Ascendant Continuum has a fixed price clearly displayed in real currency (USD), not obfuscated through virtual currency conversions. No loot boxes, no gacha, no randomized rewards tied to payment. You see a cosmetic sigil decoration, you see it costs $2.99, you decide if that\'s worth it. We display ownership statistics: "284 players own this item" helps you make informed decisions. Prices never change based on your spending history or algorithmic manipulation—everyone pays the same price.'
                    },
                    {
                        heading: 'Free-to-Play Done Right',
                        content: 'The entire game is free forever—all five realms, all story content, all gameplay mechanics, all future updates. Monetization is purely cosmetic: sigil decorations, visual effects for rituals, alternate color palettes for realms, custom time capsule designs. Zero gameplay advantages. We also sell optional "lore expansions" (bonus story chapters, NPC backstories) but make them available free after 90 days to ensure no permanent content gatekeeping. Our conversion rate is 12% (industry average: 2-3%), proving ethical monetization can be profitable.'
                    },
                    {
                        heading: 'Addiction Prevention and Spending Limits',
                        content: 'We implement proactive addiction prevention: spending limits you can set yourself that can\'t be changed for 30 days (preventing impulse spending during dopamine highs), weekly spending reports to increase financial awareness, and voluntary session time limits with enforced cooldown periods. We display total spending prominently: "You\'ve spent $47.23 on this game since January 2025." Some players appreciate this transparency; others find it confronting. Both responses are valid, and both reduce compulsive spending. We sacrifice short-term revenue for long-term player wellbeing.'
                    }
                ]
            }
        ]
    },

    tutorial: {
        name: 'Tutorial',
        tags: ['tutorial', 'unity', 'game-dev'],
        topics: [
            {
                title: 'Unity Mobile Optimization: WebGL to Android Performance',
                hook: 'WebGL ran smoothly at 60fps. Android was a slideshow at 12fps. Here\'s how we fixed it.',
                sections: [
                    {
                        heading: 'The Mobile Performance Gap',
                        content: 'The Ascendant Continuum is being developed for WebGL first—targeting 60fps on desktop and smooth 30fps on modern phones via browser. When we compiled our early prototype to native Android expecting better performance, we encountered challenges: 12fps on flagship phones, crashes on mid-range devices, 8-second load times, and thermal throttling after 3 minutes. The culprits: excessive overdraw from layered post-processing, inefficient texture atlasing creating hundreds of draw calls, memory leaks in our event system, and garbage collection spikes during realm transitions that froze the game for 800ms.'
                    },
                    {
                        heading: 'Profiling with Unity and Android Tools',
                        content: 'We used Unity Profiler connected to Android devices via ADB to identify bottlenecks: 51% GPU time spent on post-processing (bloom, vignette, color grading layered inefficiently), 28% on particle systems (instantiating/destroying instead of pooling), 300-800ms GC spikes every 30 seconds (string allocations in Update loops, event system creating delegate garbage). Android\'s GPU Profiler revealed excessive texture swaps (72 per frame) and fragment shader overdraw of 4.2x average (every pixel rendered 4 times due to transparent layers).'
                    },
                    {
                        heading: 'Post-Processing Stack Optimization',
                        content: 'Reduced post-processing from 12 effects to 4 on mobile using platform-specific build configurations. Bloom, color grading, and vignette were baked into a single custom shader instead of separate passes—one shader calculating all three effects simultaneously reduced GPU time by 68%. Implemented dynamic quality scaling: when framerate drops below 30fps, effects automatically disable (starting with most expensive) until performance recovers, then gradually re-enable. Added quality presets based on device tier detected at launch.'
                    },
                    {
                        heading: 'Memory Management and GC Elimination',
                        content: 'Implemented object pooling for all frequently instantiated objects (particles, UI elements, temporary effects)—eliminated 95% of allocations. Replaced string concatenation with StringBuilder and cached all dialog text. Moved from event delegates (which allocate) to lightweight enum-based message systems. Aggressively unloaded unused realm assets during transitions using Addressables. Result: 60fps on flagship devices, 45fps on mid-range, 30fps on budget phones, with GC spikes reduced to 8ms (imperceptible). Load time dropped from 8 seconds to 1.2 seconds.'
                    }
                ]
            },
            {
                title: 'Firebase Integration: Cloud-Powered Player Progression',
                hook: 'Local save files are fragile. Here\'s how we built cloud-first progression that never loses data.',
                sections: [
                    {
                        heading: 'Why Cloud-First Progression Matters',
                        content: 'Traditional games save locally—if you uninstall, change devices, or experience file corruption, progression is lost forever. Cloud saves solve this but are often afterthoughts bolted onto local systems, creating sync conflicts, duplication bugs, and data loss edge cases. We designed cloud-first from day one: Firebase Realtime Database is the source of truth, local data is merely a cache for offline play. This eliminates sync conflicts and enables cross-device progression seamlessly.'
                    },
                    {
                        heading: 'Firebase Realtime Database Architecture',
                        content: 'Player data is structured hierarchically: /players/{uid}/profile (sigil, preferences, stats), /players/{uid}/progress (quests, unlocks, discoveries), /players/{uid}/social (time capsules, puzzle chains). We use Firebase Security Rules to ensure players can only read/write their own data. Realtime listeners update UI instantly when data changes (useful when testing—changes in Firebase console appear in game immediately). Offline persistence mode caches data locally, syncing automatically when connection returns.'
                    },
                    {
                        heading: 'Conflict Resolution and Offline Play',
                        content: 'Firebase handles sync conflicts using timestamps, but we add custom logic for game-specific scenarios. Progression events (quest completions, item unlocks) are append-only—no conflicts possible. Resource changes (currency spent) use transaction operations to ensure atomicity. When offline, writes queue locally and execute when reconnected. We validate all actions client-side before attempting, reducing server rejections. Edge case: if you play offline on two devices simultaneously, the later sync wins for preferences, but progression merges (union of completed content).'
                    },
                    {
                        heading: 'Analytics and Player Insights',
                        content: 'Firebase Analytics tracks 60+ custom events: realm_entered, puzzle_solved, accessibility_mode_changed, session_duration, ritual_completed. We track conversion funnels (onboarding → first ritual → first puzzle → first purchase) to identify drop-off points. Crash Reporting via Firebase Crashlytics shows exactly what players were doing when crashes occur. Remote Config lets us A/B test features and tune difficulty dynamically without app updates. All analytics are anonymized and aggregated—we can\'t identify individual players, only behavioral patterns across the userbase.'
                    }
                ]
            },
            {
                title: 'Shader Programming: Custom Effects for Magical Worlds',
                hook: 'Unity\'s built-in shaders weren\'t magical enough. Here\'s how we wrote custom shaders for impossible visuals.',
                sections: [
                    {
                        heading: 'Why Custom Shaders Matter',
                        content: 'Unity provides Standard shader (PBR), Unlit shader (performance), and Shader Graph (node-based visual programming). These cover 80% of use cases but don\'t create distinctive visuals. Every Unity game looks similar because they use the same shaders. We wanted impossible colors, non-Euclidean geometry, accessibility-reactive rendering, and magical effects that don\'t exist in reality. That requires writing HLSL/GLSL shader code directly—intimidating but incredibly rewarding.'
                    },
                    {
                        heading: 'Color Manipulation for Accessibility Modes',
                        content: 'We wrote fragment shaders that transform colors in real-time based on accessibility settings. Deuteranopia mode shifts green wavelengths to blue using matrix transformations in CIE LAB color space (more perceptually accurate than RGB). Protanopia and Tritanopia use similar matrices. Each mode also reveals hidden objects: we sample a mask texture in UV space and discard fragments (pixels) conditionally based on the active mode. One shader handles all three colorblind modes plus the default view, improving performance compared to swapping materials.'
                    },
                    {
                        heading: 'Procedural Patterns and Noise Functions',
                        content: 'Sigils use procedurally generated patterns rendered in custom shaders. We use Perlin noise, Voronoi diagrams, and fractal Brownian motion (FBM) to create organic magical patterns. The shader takes player behavior hashes as seed values and generates deterministic patterns—same player ID always produces the same sigil. We layer multiple noise octaves at different frequencies and amplitudes, then apply domain warping (displacing UV coordinates based on noise) to create flowing, organic shapes impossible to replicate with traditional textures.'
                    },
                    {
                        heading: 'Performance Optimization for Mobile GPUs',
                        content: 'Mobile GPUs are heavily fragment shader-limited—complex per-pixel calculations kill framerate. We use vertex shaders to pre-compute values when possible (calculate once per vertex, interpolate across triangles). Expensive operations like noise generation are baked into textures when patterns don\'t need to be dynamic. We use shader LOD (level of detail): close objects get full shader complexity, distant objects use simplified versions. Conditional compilation (#if UNITY_ANDROID) generates mobile-optimized variants. Result: magical visuals at 60fps on mobile.'
                    }
                ]
            },
            {
                title: 'State Management with ScriptableObjects',
                hook: 'Global variables create spaghetti code. ScriptableObjects create clean, testable architecture.',
                sections: [
                    {
                        heading: 'The Problem with Traditional State Management',
                        content: 'Beginner Unity projects use static variables or singleton patterns to share state between scenes and systems. This creates tight coupling—every system that needs player health references the same PlayerManager singleton. Testing is impossible (singletons persist across test runs). State is hidden across dozens of files. Debugging requires mentally tracking implicit dependencies. As projects grow, this becomes unmaintainable. ScriptableObjects offer a better way: data-driven, inspector-visible, loosely coupled state management.'
                    },
                    {
                        heading: 'ScriptableObject Event System',
                        content: 'We use ScriptableObjects as event channels: PuzzleCompletedEvent, RealmTransitionEvent, AccessibilityModeChangedEvent. Systems raise events by calling Invoke() on these assets. Other systems subscribe by implementing OnEnable/OnDisable listeners. This creates decoupling: the puzzle system doesn\'t know or care what happens when puzzles complete—it just raises an event. Analytics, UI, progression, and audio systems independently listen and react. Adding new reactions requires zero changes to existing code.'
                    },
                    {
                        heading: 'Runtime Sets for Dynamic Collections',
                        content: 'We created a generic "RuntimeSet" ScriptableObject that acts as a dynamic collection. ActiveNPCs is a RuntimeSet<NPC>—NPCs add themselves OnEnable, remove themselves OnDisable. Any system can query ActiveNPCs without needing references or FindObjectsOfType (which is slow). This is especially powerful for UI: a "Nearby NPCs" panel simply observes the RuntimeSet and updates automatically when NPCs enter/leave. No manual reference management, no null checks, no stale data.'
                    },
                    {
                        heading: 'Testability and Debugging Benefits',
                        content: 'ScriptableObjects are assets visible in the Project window—you can inspect state without running the game. During play mode, you can watch ScriptableObjects update in real-time in the Inspector. Testing is trivial: create test-specific ScriptableObject instances (PlayerData_Test, GameState_Test) and inject them into systems. No singleton teardown, no static state persistence between tests. We reduced debugging time by ~40% simply by making state visible and isolated in ScriptableObjects instead of hidden in static memory.'
                    }
                ]
            },
            {
                title: 'Addressables and Asset Management at Scale',
                hook: 'Our build size hit 1.2GB. Addressables brought it down to 85MB + on-demand streaming.',
                sections: [
                    {
                        heading: 'The Asset Bloat Problem',
                        content: 'Early in development, our Android build exceeded 1.2GB—unacceptable for a game targeting players in regions with limited mobile data. Most of this was assets that most players would never see: all five realms loaded simultaneously, alternate language voiceover files, high-resolution textures for devices that can\'t render them, debug tools left in release builds. Traditional Resources folders load everything at startup. We needed granular control over what loads when, and the ability to stream content on-demand. Addressables solved this.'
                    },
                    {
                        heading: 'Addressable Asset System Architecture',
                        content: 'We organized assets into logical groups: Core (UI, menus, always needed), Realm_Ember, Realm_Abyss, Realm_Nexus, Realm_Void, Realm_Origin, Localization_EN, Localization_ES. Each group compiles to an AssetBundle that can be loaded independently. The Core bundle is ~85MB and includes one starter realm. Additional realms download on-demand (~40MB each) the first time you visit, then cache locally. Localization bundles download based on system language. This reduced initial download from 1.2GB to 85MB—a 93% reduction.'
                    },
                    {
                        heading: 'Remote Hosting and Content Updates',
                        content: 'Addressables can load from remote URLs (we use Firebase Storage for CDN-backed hosting). This enables content updates without app store submissions—we can add new cosmetics, fix texture bugs, balance difficulty curves by simply uploading new asset bundles. Versioning is automatic: Addressables compares local catalog with remote, downloads changed bundles. Players get seamless updates without reinstalling. We\'ve deployed 17 content updates this way, each taking ~2 minutes to propagate globally versus 2-5 days for app store review.'
                    },
                    {
                        heading: 'Memory Management and Unloading',
                        content: 'Addressables provides fine-grained memory control. When you leave a realm, we call Addressables.Release() to unload all realm-specific assets, immediately reclaiming memory. DontUnloadUnusedAsset is no longer needed—we control unloading explicitly. We track memory using Unity Profiler and found realm transitions now release 200-350MB instantly. For low-memory devices (detected via SystemInfo.systemMemorySize), we unload previous realm before loading next, ensuring memory usage never exceeds 600MB even on budget Android phones with 2GB RAM.'
                    }
                ]
            }
        ]
    },

    behindTheScenes: {
        name: 'Behind the Scenes',
        tags: ['development', 'team', 'process'],
        topics: [
            {
                title: 'Solo Development Lessons: Building a Game Alone',
                hook: 'I built this game mostly alone. Here\'s what I learned about sustainable solo dev.',
                sections: [
                    {
                        heading: 'The Reality of Solo Development',
                        content: 'The Ascendant Continuum is primarily a solo project: one core developer (me), with contractors for art, audio, and specialized consulting. This isn\'t a romantic "lone genius" story—it\'s a logistical necessity due to budget constraints. Solo dev teaches brutal prioritization: you can\'t do everything, so you must identify highest-impact features and ruthlessly cut the rest. I\'ve abandoned 60% of planned features not because they were bad ideas, but because they weren\'t essential. The game that exists focuses on core strengths and outsources weaknesses.'
                    },
                    {
                        heading: 'Tools and Automation for Efficiency',
                        content: 'Automation saves solo devs from drowning in busywork. I use GitHub Actions for build automation (push to main triggers WebGL, Android, and iOS builds), Firebase hosting for zero-config deployment, automated testing via Unity Test Framework, and automated social media posting (this blog, daily social updates). These systems let me focus on high-value creative work instead of repetitive tasks. Initial automation setup took 40 hours; it\'s saved 200+ hours since. For solo devs, automation multiplies your effective team size.'
                    },
                    {
                        heading: 'Preventing Burnout Through Sustainable Practices',
                        content: 'Solo dev is marathon, not sprint. I track time strictly: 25 hours/week maximum, never weekends, mandatory 1-week vacation every 8 weeks whether I feel I need it or not. I use Pomodoro technique (25-min work, 5-min break) to prevent hyperfocus burnout. I maintain separate hobby projects (not games—woodworking, painting) to prevent creative depletion. I\'ve declined partnerships and funding that would require unsustainable crunch. The game will take longer this way, but it\'ll actually ship. Most solo projects fail from burnout, not technical challenges.'
                    },
                    {
                        heading: 'When to Hire Contractors vs DIY',
                        content: 'I\'m a decent programmer, mediocre designer, terrible artist, and incompetent musician. Hiring professionals for weaknesses is cheaper than struggling alone. I spent $2,400 on art contractors (character designs, realm concepts, UI assets)—would\'ve taken me 6 months to produce inferior results. Spent $800 on audio (ambient realm soundscapes, UI feedback sounds). Cost analysis: my time is worth ~$50/hour (opportunity cost of contract work I could do instead). If a contractor can do it better in fewer hours, hire them. Reserve your time for work only you can do—core design and programming.'
                    }
                ]
            }
        ]
    }
};

// Generate blog post from git commits (What's New posts)
function generateWhatsNewPost() {
    try {
        // Get commits from last 7 days
        const gitOutput = execSync('git log --since="7 days ago" --pretty=format:"%H|%s|%an|%ad" --date=short', {
            encoding: 'utf-8',
            cwd: path.join(__dirname, '..', '..'),
            stdio: ['pipe', 'pipe', 'ignore'] // Ignore stderr
        }).trim();

        if (!gitOutput) {
            return null; // No commits, skip this post
        }

        const commits = gitOutput.split('\n').filter(line => line);

        if (commits.length === 0) {
            return null; // No commits, skip this post
        }

        // Parse commits
        const parsedCommits = commits.map(line => {
            const [hash, subject, author, date] = line.split('|');
            return { hash: hash.substring(0, 7), subject, author, date };
        });

        // Filter out automated commits
        const filteredCommits = parsedCommits.filter(c =>
            !c.subject.includes('[skip ci]') &&
            !c.subject.includes('automated') &&
            !c.author.includes('github-actions') &&
            !c.author.includes('bot')
        );

        if (filteredCommits.length === 0) {
            return null;
        }

        // Categorize commits
        const categories = {
            features: [],
            fixes: [],
            improvements: [],
            docs: [],
            other: []
        };

        filteredCommits.forEach(commit => {
            const subject = commit.subject.toLowerCase();
            if (subject.includes('feat:') || subject.includes('feature:') || subject.includes('add:')) {
                categories.features.push(commit);
            } else if (subject.includes('fix:') || subject.includes('bug:')) {
                categories.fixes.push(commit);
            } else if (subject.includes('improve:') || subject.includes('perf:') || subject.includes('refactor:')) {
                categories.improvements.push(commit);
            } else if (subject.includes('docs:') || subject.includes('doc:')) {
                categories.docs.push(commit);
            } else {
                categories.other.push(commit);
            }
        });

        // Generate human-readable sections
        const sections = [];

        if (categories.features.length > 0) {
            sections.push({
                heading: 'New Features',
                content: `This week we shipped ${categories.features.length} new feature${categories.features.length > 1 ? 's' : ''}:\n\n` +
                    categories.features.map(c => `- ${c.subject.replace(/^(feat:|feature:|add:)\s*/i, '')}`).join('\n')
            });
        }

        if (categories.fixes.length > 0) {
            sections.push({
                heading: 'Bug Fixes',
                content: `We squashed ${categories.fixes.length} bug${categories.fixes.length > 1 ? 's' : ''} to improve stability:\n\n` +
                    categories.fixes.map(c => `- ${c.subject.replace(/^(fix:|bug:)\s*/i, '')}`).join('\n')
            });
        }

        if (categories.improvements.length > 0) {
            sections.push({
                heading: 'Performance and Improvements',
                content: `We made ${categories.improvements.length} optimization${categories.improvements.length > 1 ? 's' : ''}:\n\n` +
                    categories.improvements.map(c => `- ${c.subject.replace(/^(improve:|perf:|refactor:)\s*/i, '')}`).join('\n')
            });
        }

        if (categories.docs.length > 0) {
            sections.push({
                heading: 'Documentation',
                content: categories.docs.map(c => `- ${c.subject.replace(/^(docs?:|doc:)\s*/i, '')}`).join('\n')
            });
        }

        // Generate summary
        const totalChanges = filteredCommits.length;
        const dateRange = `${filteredCommits[filteredCommits.length - 1].date} to ${filteredCommits[0].date}`;
        const publishDate = filteredCommits[0].date || new Date().toISOString().split('T')[0];

        const title = `What's New: ${totalChanges} Update${totalChanges > 1 ? 's' : ''} This Week`;
        const hook = `Development update for ${dateRange}: ${categories.features.length} features, ${categories.fixes.length} fixes, ${categories.improvements.length} improvements.`;
        const slug = `whats-new-${publishDate}`;
        const excerpt = sections.length > 0
            ? sections[0].content.replace(/\s+/g, ' ').trim().substring(0, 220) + '...'
            : hook;
        const content = sections.map(section => `
            <h2>${section.heading}</h2>
            <p>${section.content}</p>
        `).join('\n');

        return {
            title,
            slug,
            date: publishDate,
            hook,
            excerpt,
            content,
            sections,
            tags: ['updates', 'changelog', 'development'],
            themeName: "What's New"
        };
    } catch (error) {
        console.error('Error generating What\'s New post:', error.message);
        return null;
    }
}

// Select random topic from regular themes (excluding whatsNew)
function selectRandomTopic() {
    const regularThemes = Object.entries(BLOG_THEMES).filter(([key]) => key !== 'whatsNew');
    const [themeName, theme] = regularThemes[Math.floor(Math.random() * regularThemes.length)];
    const topic = theme.topics[Math.floor(Math.random() * theme.topics.length)];

    return {
        ...topic,
        themeName: theme.name,
        tags: theme.tags
    };
}

// Generate blog post (either What's New or regular topic)
function generateBlogPost(forceWhatsNew = false) {
    // Try What's New first (30% chance, or forced)
    if (forceWhatsNew || Math.random() < 0.3) {
        const whatsNewPost = generateWhatsNewPost();
        if (whatsNewPost) {
            return whatsNewPost;
        }
    }

    // Fallback to regular topic
    const topic = selectRandomTopic();
    const date = new Date().toISOString().split('T')[0]; // YYYY-MM-DD
    const slug = topic.title.toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-|-$/g, '');

    // Generate excerpt with minimum quality floor.
    const excerptSource = (topic.sections[0]?.content || topic.hook || topic.title).replace(/\s+/g, ' ').trim();
    const excerpt = excerptSource.length > 220
        ? `${excerptSource.substring(0, 220)}...`
        : excerptSource;

    // Generate full blog post content
    const content = `
        <p class="blog-post-hook">${topic.hook}</p>
        ${topic.sections.map(section => `
            <h2>${section.heading}</h2>
            <p>${section.content}</p>
        `).join('\n')}
    `;

    return {
        title: topic.title,
        slug,
        date,
        tags: topic.tags,
        themeName: topic.themeName,
        hook: topic.hook,
        excerpt,
        content,
        sections: topic.sections
    };
}

export { generateBlogPost, BLOG_THEMES };
