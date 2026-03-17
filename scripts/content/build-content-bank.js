#!/usr/bin/env node
/**
 * Content Bank Builder - Generates remaining 80 items for content-bank.json
 * Run with: node scripts/content/build-content-bank.js
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Additional 80 items to add (21-100)
const additionalContent = [
    // Dev Updates (21-40) - 20 more items
    {
        id: 21,
        type: "devUpdate",
        hook: "Shipped v2.0 today. Here's everything that changed.",
        body: "v2.0 Release Notes:\n\n✓ 8 colorblind modes (each reveals unique content)\n✓ Real moon phase integration\n✓ NPC collective memory system\n✓ Digital Sunset wellness feature\n✓ 5 magical realms (Emberforge, Verdant, Echo, Dawn, Lantern)\n\nRelease Candidate 1 is live. WebGL playable now: ascendant-continuum.firebaseapp.com/play",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 22,
        type: "devUpdate",
        hook: "I thought this would take 2 hours. It took 2 weeks. Here's why.",
        body: "Implementing screen reader support for Unity WebGL.\n\nExpected: add ARIA labels, done.\nReality: WebGL canvas accessibility is... complex.\n\nSolved:\n1. Custom DOM overlay for screen readers\n2. Focus management system\n3. Audio description pipeline\n4. Testing with NVDA, JAWS, VoiceOver\n\n2 weeks well spent. Fully accessible.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 23,
        type: "devUpdate",
        hook: "What happens when you let players create permanent universe marks?",
        body: "Player rituals become archaeological discoveries.\n\nEarly adopters = universe founders.\nTheir sigils = permanent lore.\nFuture players discover their creations.\n\nLegacy system without leaderboards.\nContribution without competition.\n\nChaos? No. Beautiful permanence.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 24,
        type: "devUpdate",
        hook: "Today I built something impossible.",
        body: "Reduced motion mode that reveals sacred geometry.\n\nFull motion: swirling particles, dynamic camera.\nReduced motion: subtle transformations, underlying patterns visible.\n\nAccessibility mode unlocks \"Still Point Mysteries\" achievement.\n\nImpossible became necessary. Then became innovative.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 25,
        type: "devUpdate",
        hook: "The hardest part of gamedev isn't coding. It's restraint.",
        body: "Every feature request sounds good.\n\n\"Add multiplayer!\"\n\"Make it open world!\"\n\"Add crafting system!\"\n\nSaying no is hard. But focus wins.\n\n1-5 minute sessions.\n5 realms.\nInfinite discovery.\n\nThat's it. That's the game.\n\nRestraint = clarity.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 26,
        type: "devUpdate",
        hook: "Here's what 6 months of indie dev taught me about sustainability.",
        body: "Lessons from 6 months solo:\n\n1. Ship fast, iterate faster\n2. Automate everything (GitHub Actions = lifesaver)\n3. Document decisions (future you will thank you)\n4. Build for maintainability, not just features\n5. Rest is productive\n\nSustainable indie dev = long-term success.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 27,
        type: "devUpdate",
        hook: "Most indie devs skip this step. That's why they fail.",
        body: "Accessibility testing.\n\nNot \"we'll add it later.\"\nNot \"it's too expensive.\"\nNot \"only a small percentage needs it.\"\n\nDay 1. Every feature. Every mode tested.\n\n20% of players have some accessibility need.\nThat's not niche. That's mainstream.\n\nSkip it, lose players. Simple.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 28,
        type: "devUpdate",
        hook: "Prototyped → tested → scrapped → rebuilt. Iteration in action.",
        body: "Daily Constellation Challenge v1: everyone gets same puzzle.\nProblem: felt like homework.\n\nv2: procedural puzzle with shared seed.\nProblem: no sense of community.\n\nv3: same puzzle + shareable emoji grid (Wordle-style).\nSolution: community + no spoilers + social proof.\n\nIteration compounds.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 29,
        type: "devUpdate",
        hook: "This accessibility feature became our best marketing angle.",
        body: "Colorblind modes that reveal different secrets.\n\nStarted as: compliance requirement.\nBecame: flagship innovation.\nResult: press coverage, community excitement, player demand.\n\nAccessibility = competitive advantage.\n\nDoing the right thing can also be the smart thing.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 30,
        type: "devUpdate",
        hook: "Computational creativity meets game design.",
        body: "Procedural ritual system architecture:\n\n1. Realm theme (fire, water, earth, air, void)\n2. Ritual type (tap, hold, trace, swipe)\n3. Moon phase modifier (new, full, waxing, waning)\n4. Constraint validator (no impossible combos)\n5. Visual generator (particle effects, colors)\n\n5 × 12 × 8 × ∞ = truly infinite.\n\nComputers create. Humans discover.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 31,
        type: "devUpdate",
        hook: "Building a living universe, not just a game.",
        body: "Traditional game: static world, scripted events.\n\nAscendant Continuum:\n- Procedural rituals (never repeat)\n- NPC collective memory (evolves with community)\n- Player fossils (permanent legacy)\n- Real celestial events (moon phases matter)\n\nUniverse that remembers, adapts, grows.\n\nLiving, not scripted.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 32,
        type: "devUpdate",
        hook: "Zero monetization dark patterns. By design.",
        body: "Monetization strategy:\n\n✓ Cosmetic upgrades (sigil colors, effects)\n✓ Realm expansions (new content)\n✓ Support tiers (patron benefits)\n\n✗ No energy timers\n✗ No loot boxes\n✗ No pay-to-win\n✗ No artificial scarcity\n✗ No FOMO\n\nEthical monetization = sustainable business.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 33,
        type: "devUpdate",
        hook: "WebGL launch is live. Here's what surprised me.",
        body: "Launched WebGL version yesterday.\n\nExpected: desktop players only.\nReality: 40% mobile browsers, 35% desktop, 25% tablets.\n\nLesson: WebGL = truly cross-platform.\nNo app store approval.\nNo downloads.\nInstant play.\n\nThe web still wins for accessibility.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 34,
        type: "devUpdate",
        hook: "5 realms, 5 different emotional experiences.",
        body: "Realm design philosophy:\n\nEmberforge = Creation energy (tap flames)\nVerdant Sanctuary = Growth reflection (tend gardens)\nEcho Fields = Memory imagination (trace constellations)\nDawn Citadel = Wonder knowledge (solve light puzzles)\nLantern Ascension = Liminal peace (float upward)\n\nEach realm teaches something different.\nEach ritual heals something different.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 35,
        type: "devUpdate",
        hook: "Digital Sunset: the anti-addiction feature.",
        body: "After 5 minutes:\n\n\"You've been exploring for 5 minutes. The universe will be here when you return.\"\n\n[Gentle reminder + optional exit]\n\nNo penalties. No pressure. Just care.\n\nGames can nurture instead of exploit.\n\nDigital wellness, built in.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 36,
        type: "devUpdate",
        hook: "From prototype to Release Candidate in 6 months.",
        body: "Timeline:\n\nMonth 1-2: Core systems (rituals, realms)\nMonth 3-4: Accessibility features (8 modes)\nMonth 5: Procedural generation + lunar integration\nMonth 6: Polish, testing, WebGL optimization\n\nNow: Release Candidate 1, playable live.\n\nNext: App Store/Google Play submission.\n\nSpeed matters. Perfection doesn't.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 37,
        type: "devUpdate",
        hook: "NPCs that learn from thousands of players at once.",
        body: "Collective intelligence system:\n\n1. Player talks to NPC\n2. Conversation stored (anonymized)\n3. NPC aggregates patterns from all players\n4. Dialogue evolves based on collective wisdom\n5. Launch-day players teach future players\n\nNot AI. Community-driven intelligence.\n\nEvery player contributes to NPC growth.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 38,
        type: "devUpdate",
        hook: "Real moon phases affect in-game rituals.",
        body: "Lunar integration:\n\n- Full moon = enhanced sigil visibility\n- New moon = hidden patterns emerge\n- Eclipses = rare ritual variations\n- Blue moon = ultra-rare events\n\nReal celestial data drives procedural generation.\n\nYour game experience syncs with actual cosmos.\n\nScience + magic = perfect blend.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 39,
        type: "devUpdate",
        hook: "Founder's Echo sigils: permanence as reward.",
        body: "Launch-day players get special \"Founder's Echo\" sigils.\n\nNot for money.\nNot for skill.\nFor being there first.\n\nYour early rituals = permanent universe lore.\nFuture players discover YOUR creations.\n\nLegacy system without competition.\n\nParticipation matters more than performance.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 40,
        type: "devUpdate",
        hook: "Built a game that doesn't want you to play it constantly.",
        body: "Anti-grind design:\n\n- Sessions: 1-5 minutes max\n- No daily login bonuses\n- No streaks to maintain\n- Digital Sunset reminder\n- Universe evolves offline\n\nPlay when you want.\nLeave when you need.\nProgress persists.\n\nRespect > retention.\n\nEthical game design.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },

    // Dev Education (41-70) - 30 items
    {
        id: 41,
        type: "devEducation",
        hook: "Unity optimization 101: memory management for mobile.",
        body: "Mobile Unity performance:\n\n1. Object pooling (avoid instantiate/destroy)\n2. Texture compression (ASTC for Android/iOS)\n3. Audio compression (Vorbis, not PCM)\n4. Sprite atlasing (reduce draw calls)\n5. Profile ALWAYS (Unity Profiler)\n\nBefore: 400MB RAM, crashes on budget phones.\nAfter: 180MB RAM, runs on 2016 devices.\n\nOptimization = accessibility.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 42,
        type: "devEducation",
        hook: "If you only learn one thing about WebGL, learn this.",
        body: "Unity WebGL killer: garbage collection pauses.\n\nProblem: GC can freeze game for 100ms+.\n\nSolution:\n```csharp\n// Force incremental GC\nGarbageCollector.GCMode = GarbageCollector.Mode.Disabled;\nGarbageCollector.CollectIncremental(1000); // 1ms budget\n```\n\nSmooth 60 FPS without stutters.\n\nWebGL = different rules.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 43,
        type: "devEducation",
        hook: "How I made Unity UI work for screen readers.",
        body: "Unity WebGL + screen reader support:\n\n1. Create DOM overlay above canvas\n2. Map Unity UI to semantic HTML\n3. Handle focus with ARIA live regions\n4. Test with NVDA, JAWS, VoiceOver\n\n```html\n<div role=\"button\" aria-label=\"Start Ritual\"\n     tabindex=\"0\">...canvas overlay...</div>\n```\n\nAccessibility requires DOM, not just canvas.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 44,
        type: "devEducation",
        hook: "Stop using PlayerPrefs for everything. Do this instead.",
        body: "PlayerPrefs is slow + limited.\n\nBetter: JSON serialization to persistent path.\n\n```csharp\nvar data = JsonUtility.ToJson(gameState);\nFile.WriteAllText(\n  Application.persistentDataPath + \"/save.json\", \n  data\n);\n```\n\nFaster, more flexible, version-controllable.\n\nPlayerPrefs = quick prototypes only.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 45,
        type: "devEducation",
        hook: "Procedural generation 101: constraints are everything.",
        body: "Procedural generation without constraints = chaos.\n\nBad: Random.Range(0, 100) everywhere.\n\nGood:\n1. Define valid combinations\n2. Validate before generation\n3. Fallback to known-good configs\n4. Test edge cases exhaustively\n\nConstraints paradox: limits create infinite possibilities.\n\nStructure enables creativity.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 46,
        type: "devEducation",
        hook: "The Unity trick no one talks about: ScriptableObjects for game design.",
        body: "Stop hardcoding game data in MonoBehaviours.\n\nScriptableObjects = designer-friendly data:\n\n```csharp\n[CreateAssetMenu]\npublic class RealmData : ScriptableObject {\n  public string realmName;\n  public Color themeColor;\n  public RitualType[] rituals;\n}\n```\n\nDesigners edit in Inspector.\nProgrammers never touched.\n\nSeparation of concerns.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 47,
        type: "devEducation",
        hook: "How to build async/await properly in Unity.",
        body: "Coroutines are old school. Use async/await:\n\n```csharp\nasync Task LoadRitualAsync() {\n  var data = await FetchFromFirebase();\n  await Task.Delay(100); // instead of yield\n  ApplyRitualData(data);\n}\n```\n\nCleaner code, better error handling, modern C#.\n\nCoroutines still have uses, but async wins for network/file ops.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 48,
        type: "devEducation",
        hook: "Unity shader basics: make your game beautiful without tanking FPS.",
        body: "Mobile shader optimization:\n\n1. Use unlit shaders (skip lighting calculations)\n2. Vertex shaders > fragment shaders (fewer operations)\n3. Avoid discard/clip (kills early-z)\n4. Test on actual devices (not editor)\n\nBeautiful doesn't mean expensive.\n\nSimple shaders = 60 FPS magic.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 49,
        type: "devEducation",
        hook: "I tested 5 approaches to touch input. This one won.",
        body: "Unity touch input comparison:\n\n1. Input.touches ✗ (no gesture recognition)\n2. UI EventSystem ✗ (UI only)\n3. Input System package ✓ (best, but complex)\n4. Touch Script asset ✗ (overkill)\n5. Custom gesture recognizer ✓ (perfect control)\n\nWinner: New Input System for flexibility.\n\nModern, maintained, supports all platforms.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 50,
        type: "devEducation",
        hook: "Here's the exact system I use for game state management.",
        body: "Game state pattern in Unity:\n\n```csharp\npublic interface IGameState {\n  void Enter();\n  void Update();\n  void Exit();\n}\n\npublic class StateMachine {\n  private IGameState current;\n  public void ChangeState(IGameState next) {\n    current?.Exit();\n    current = next;\n    current.Enter();\n  }\n}\n```\n\nMenu, Gameplay, Ritual, Results states.\n\nClean. Debuggable. Scalable.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 51,
        type: "devEducation",
        hook: "Firebase + Unity: the integration nobody explains properly.",
        body: "Unity Firebase setup (actual steps):\n\n1. Download google-services.json (Android) / GoogleService-Info.plist (iOS)\n2. Place in Assets/\n3. Import Firebase SDK (Unity Package Manager)\n4. Initialize in first scene:\n```csharp\nFirebaseApp.CheckAndFixDependenciesAsync();\n```\n\nCommon gotcha: version mismatches.\nSolution: Use Firebase BoM (Bill of Materials).\n\nWorks perfectly once configured right.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 52,
        type: "devEducation",
        hook: "Your Unity build is too big. Here's how to fix it.",
        body: "Unity build size optimization:\n\n1. Code stripping: High (IL2CPP)\n2. Texture compression: ASTC 6x6\n3. Audio compression: Vorbis quality 70\n4. Mesh compression: Med/High\n5. Strip unused shaders\n\nBefore: 120MB APK\nAfter: 38MB APK\n\n68% reduction. Same game.\n\nSize = accessibility for low-storage devices.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 53,
        type: "devEducation",
        hook: "How I reverse-engineered smooth particle effects for mobile.",
        body: "Mobile particle optimization:\n\nBad: 500 particles, alpha blending, constant emission.\nResult: 15 FPS.\n\nGood:\n- 50-100 particles max\n- Additive blending (faster)\n- Burst emission, not continuous\n- Texture atlasing\n- GPU instancing\n\nResult: 60 FPS, looks better.\n\nFewer particles, smarter effects.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 54,
        type: "devEducation",
        hook: "3 years of Unity mistakes condensed: audio edition.",
        body: "Unity audio mistakes I made:\n\n1. Using PCM audio (200MB of audio files)\n   Fix: Vorbis compression (18MB)\n\n2. Loading all audio at start (memory spike)\n   Fix: Streaming + async loading\n\n3. No audio pooling (GC stutter)\n   Fix: AudioSource pool\n\nAudio optimization = forgotten optimization.\n\nDon't forget audio.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 55,
        type: "devEducation",
        hook: "Unity UI performance: what the docs don't tell you.",
        body: "UGUI performance secrets:\n\n1. Raycast targets: disable on non-interactive elements\n2. Canvas batching: one canvas per update frequency\n3. Text mesh pro > legacy Text (always)\n4. Disable \"Pixel Perfect\" (CPU killer)\n\nOne setting change = 2x FPS improvement.\n\nUI is often the bottleneck.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 56,
        type: "devEducation",
        hook: "How to actually test your Unity game on low-end devices.",
        body: "Device testing strategy:\n\n1. Get actual budget phones (2016-2018 models)\n2. Test on throttled devices (battery saver mode)\n3. Profile with Unity Profiler remotely\n4. Test on slow networks (3G)\n5. Check with 1GB RAM devices\n\nEmulator testing = lies.\nReal device testing = truth.\n\nBuy a $50 used phone. Test constantly.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 57,
        type: "devEducation",
        hook: "The performance boost you're missing: object pooling.",
        body: "Unity instantiate/destroy = GC hell.\n\nObject pooling pattern:\n\n```csharp\nQueue<GameObject> pool = new();\n\npublic GameObject Get() {\n  if (pool.Count > 0) return pool.Dequeue();\n  return Instantiate(prefab);\n}\n\npublic void Return(GameObject obj) {\n  obj.SetActive(false);\n  pool.Enqueue(obj);\n}\n```\n\nUse for: particles, projectiles, UI elements.\n\nSmooth FPS, zero stutter.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 58,
        type: "devEducation",
        hook: "Scene loading without freezing the game.",
        body: "Unity async scene loading:\n\n```csharp\nasync Task LoadSceneAsync(string name) {\n  var op = SceneManager.LoadSceneAsync(name);\n  op.allowSceneActivation = false;\n  \n  while (op.progress < 0.9f) {\n    // Update progress bar\n    await Task.Yield();\n  }\n  \n  op.allowSceneActivation = true;\n}\n```\n\nSmooth transitions, no freezing.\n\nAsynchronous = professional feel.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 59,
        type: "devEducation",
        hook: "Unity Input System vs old Input Manager: definitive comparison.",
        body: "Old Input Manager:\n✗ Hardcoded key bindings\n✗ No rebinding at runtime\n✗ Polling every frame\n\nNew Input System:\n✓ Event-driven (more efficient)\n✓ Runtime rebinding\n✓ Controller, touch, keyboard unified\n✓ Action-based (not key-based)\n\nMigration pain: worth it.\n\nNew Input System = future-proof.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 60,
        type: "devEducation",
        hook: "Debugging Unity mobile builds: tools that actually work.",
        body: "Mobile debugging toolkit:\n\n1. adb logcat (Android) / Xcode Console (iOS)\n2. Unity Remote (test without builds)\n3. On-device debug build (enable logging)\n4. Firebase Crashlytics (crash reports)\n5. Internal testing tracks (Google Play / TestFlight)\n\nDesktop debugging ≠ mobile reality.\n\nReal devices reveal real bugs.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 61,
        type: "devEducation",
        hook: "How I implement save systems that never corrupt.",
        body: "Robust save system:\n\n1. Write to temp file first\n2. Validate JSON integrity\n3. Atomic rename (temp → real)\n4. Keep backup of previous save\n5. Version your save format\n\n```csharp\nFile.WriteAllText(tempPath, json);\nif (ValidateSave(tempPath)) {\n  File.Replace(tempPath, savePath, backupPath);\n}\n```\n\nNever lose player progress.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 62,
        type: "devEducation",
        hook: "Unity coroutines: what they're good for (and what they're not).",
        body: "Coroutines are good for:\n✓ Timed sequences (wait, animate, wait)\n✓ Frame-spread operations\n✓ Simple state machines\n\nCoroutines are BAD for:\n✗ Network calls (use async/await)\n✗ File operations (use async/await)\n✗ Complex error handling\n\nRight tool, right job.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 63,
        type: "devEducation",
        hook: "TextMeshPro vs Unity Text: it's not even close.",
        body: "Legacy Unity Text:\n✗ Blurry on high DPI\n✗ No rich text effects\n✗ Poor performance\n\nTextMeshPro:\n✓ Crisp on any resolution\n✓ Rich text, gradients, outlines\n✓ Better performance (SDF rendering)\n✓ Free, built-in\n\nThere is no reason to use legacy Text.\n\nTMP always.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 64,
        type: "devEducation",
        hook: "Implementing color accessibility: beyond simple filters.",
        body: "Colorblind accessibility done right:\n\n1. Don't just apply filters\n2. Redesign color schemes per mode\n3. Add pattern/shape differentiation\n4. Test with Color Oracle / Sim Daltonism\n5. Get feedback from colorblind testers\n\nFilterbad = still hard to use.\nRedesign = actually usable.\n\nAccessibility = intentional design.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 65,
        type: "devEducation",
        hook: "Unity build automation with GitHub Actions: complete guide.",
        body: "CI/CD for Unity:\n\n1. Use game-ci/unity-builder action\n2. Store license in GitHub Secrets\n3. Build for multiple platforms in parallel\n4. Upload artifacts to releases\n5. Auto-deploy to Firebase/itch.io\n\nEvery push = tested build.\n\nAutomation saves weeks of manual work.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 66,
        type: "devEducation",
        hook: "Mobile battery optimization: the settings you're forgetting.",
        body: "Unity mobile battery optimization:\n\n1. Target framerate: 30 FPS (not 60)\n   `Application.targetFrameRate = 30;`\n2. Screen.sleepTimeout = auto\n3. Disable unnecessary sensors\n4. Bake lighting (no realtime)\n5. Reduce particle updates\n\n30 FPS game = 2x battery life.\n\nPlayer retention > visual flex.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 67,
        type: "devEducation",
        hook: "How to make Unity play nice with version control.",
        body: "Unity + Git setup:\n\n1. Force text serialization:\n   Edit → Project Settings → Asset Serialization: Force Text\n2. Use .gitignore for Unity\n3. Smart Merge Tool for scenes\n4. LFS for large assets\n\n```\n*.meta\nLibrary/\nTemp/\nBuilds/\n```\n\nProper VCS setup = no more merge conflicts.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 68,
        type: "devEducation",
        hook: "The Unity profiler features you didn't know existed.",
        body: "Advanced Unity Profiler tips:\n\n1. Deep Profiling (enable for specific frames)\n2. Memory Profiler package (find leaks)\n3. Frame Debugger (see draw calls)\n4. Physics Debugger (visualize collisions)\n5. Custom markers: `Profiler.BeginSample()`\n\nProfiler is not just CPU graph.\n\nMaster it = master performance.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 69,
        type: "devEducation",
        hook: "Unity asset store mistakes that cost me thousands.",
        body: "Asset Store lessons:\n\n1. Don't buy solutions, buy components\n2. Check last update date (abandoned assets = tech debt)\n3. Read 1-star reviews first\n4. Test in clean project before importing\n5. Most \"complete game\" assets = unusable\n\nBest purchase: UI/audio/VFX packs.\nWorst purchase: \"Complete RPG System.\"\n\nAssets accelerate, not replace, development.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 70,
        type: "devEducation",
        hook: "How I debug \"it works in editor but not in build\" issues.",
        body: "Build-specific bugs debugging:\n\n1. Enable Development Build + Script Debugging\n2. Check Managed Stripping Level (try Low)\n3. Look for missing resources (case sensitivity)\n4. Test in-editor with batch mode\n5. Check platform-specific APIs\n\nEditor ≠ build environment.\n\nAlways test actual builds early.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },

    // Design Philosophy (71-90) - 20 items
    {
        id: 71,
        type: "designPhilosophy",
        hook: "Games don't have to manipulate players to succeed.",
        body: "Anti-manipulation design:\n\n✗ No dark patterns\n✗ No infinite progression treadmills\n✗ No social pressure mechanics\n✗ No artificial urgency\n\nEthical game design is not charity.\nIt's sustainable business.\n\nRespect players = long-term loyalty.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 72,
        type: "designPhilosophy",
        hook: "What happens when you remove FOMO from a game?",
        body: "Removed FOMO mechanics:\n\n✗ No time-limited events\n✗ No daily login rewards\n✗ No limited cosmetics\n✗ No \"you missed out\" messaging\n\nResult: Players come back BECAUSE they want to, not because they feel obligated.\n\nVoluntary engagement > forced retention.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 73,
        type: "designPhilosophy",
        hook: "Player-first design sounds obvious. Almost nobody does it.",
        body: "Player-first vs business-first:\n\nBusiness-first: maximize DAU, session length, ARPU.\nPlayer-first: respect time, honor agency, build trust.\n\nBoth can be profitable.\nOnly one is sustainable.\n\nShort-term metrics lie.\nLong-term trust wins.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 74,
        type: "designPhilosophy",
        hook: "The game industry's biggest lie: engagement = success.",
        body: "Engagement metrics are broken.\n\n\"Average session: 45 minutes!\"\nTranslation: Players feel trapped.\n\n\"90% day-7 return!\"\nTranslation: FOMO mechanics working.\n\nBetter metric: player wellbeing.\nDo they feel GOOD after playing?\n\nQuality > quantity.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 75,
        type: "designPhilosophy",
        hook: "Anti-grind philosophy: why we cap sessions at 5 minutes.",
        body: "Most games: keep players playing forever.\n\nUs: encourage players to leave.\n\n1-5 minute sessions.\nDigital Sunset reminder.\nNo infinite progression.\n\nWhy? Burnout kills communities.\nRespect preserves them.\n\nSustainable play = sustainable game.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 76,
        type: "designPhilosophy",
        hook: "Accessibility as innovation, not obligation.",
        body: "Traditional: Accessibility = compliance.\n\nUs: Accessibility = competitive advantage.\n\nColorblind modes reveal different secrets.\nReduced motion unlocks sacred geometry.\nRhythm-free mode = pattern puzzles.\n\nAccessibility players get EXCLUSIVE content.\n\nInnovation disguised as inclusion.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 77,
        type: "designPhilosophy",
        hook: "The real cost of dark patterns (and why we refuse to use them).",
        body: "Dark patterns cost:\n\n- Player trust (immediate)\n- Community health (medium-term)\n- Brand reputation (long-term)\n- Employee morale (ongoing)\n\nGain: short-term revenue bump.\n\nNot worth it.\n\nEthics aside, it's bad business.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 78,
        type: "designPhilosophy",
        hook: "Games can be beautiful without being predatory.",
        body: "False dichotomy: \"Ethical games can't be profitable.\"\n\nCounterexamples:\n- Stardew Valley (solo dev, millions sold)\n- Hades (no microtransactions, huge success)\n- Celeste (accessibility-first, critical acclaim)\n\nEthical design = competitive differentiation.\n\nDoing right = doing well.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 79,
        type: "designPhilosophy",
        hook: "What I learned ditching traditional progression systems.",
        body: "Removed:\n✗ XP bars\n✗ Level numbers\n✗ Power curves\n✗ Grinding requirements\n\nReplaced with:\n✓ Discovery (find secrets)\n✓ Expression (create sigils)\n✓ Legacy (permanent marks)\n✓ Community (collective growth)\n\nProgression without treadmill.\nMeaning without metrics.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 80,
        type: "designPhilosophy",
        hook: "The player fantasy nobody's building for: mindful gaming.",
        body: "Underserved player fantasy:\n\n\"I want a game that respects my time.\nThat helps me relax, not stress.\nThat I can play for 5 minutes and feel fulfilled.\nThat doesn't make me feel guilty for leaving.\"\n\nMindful gaming = blue ocean market.\n\nBuild what others ignore.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 81,
        type: "designPhilosophy",
        hook: "We're not building a game. We're building a universe.",
        body: "Game thinking: finite content, consumable experience.\n\nUniverse thinking:\n- Procedural infinity\n- Player legacy systems\n- Evolving NPC intelligence\n- Real-world celestial integration\n\nGames end.\nUniverses persist.\n\nThink bigger.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 82,
        type: "designPhilosophy",
        hook: "Most games are designed to be addictive. We're doing the opposite.",
        body: "Addiction mechanics we refuse:\n\n✗ Variable reward schedules\n✗ Loss aversion (energy timers)\n✗ Social obligation (guild pressure)\n✗ Escalating commitment (sunk cost)\n\nInstead: transparency, agency, boundaries.\n\nAddiction = short-term gain, long-term loss.\n\nHealth > habit formation.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 83,
        type: "designPhilosophy",
        hook: "Contribution without competition: the legacy system.",
        body: "Traditional: leaderboards, rankings, winners/losers.\n\nUs: every player leaves permanent marks.\n\nNo rankings.\nNo comparison.\nJust contribution.\n\nYour rituals = universe archaeology.\nYour conversations = NPC wisdom.\nYour presence = permanent.\n\nCollaboration > competition.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 84,
        type: "designPhilosophy",
        hook: "Why ethical game design is our competitive advantage.",
        body: "Competitors: race to the bottom (dark patterns).\n\nUs: differentiation through ethics.\n\nMarket positioning:\n- \"The game that respects you\"\n- \"Play mindfully\"\n- \"Accessibility = discovery\"\n\nPress coverage, community loyalty, brand strength.\n\nEthics = marketing gold.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 85,
        type: "designPhilosophy",
        hook: "The false choice: accessibility vs gameplay.",
        body: "Traditional: \"Accessible mode is easier/worse.\"\n\nUs: \"Accessible modes reveal different content.\"\n\nAccessibility ≠ compromise.\nAccessibility = alternate discovery path.\n\nProtanopia players see different secrets.\nNot easier. Different.\n\nDestroy the false choice.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "high",
        used: false
    },
    {
        id: 86,
        type: "designPhilosophy",
        hook: "Community-driven universe design philosophy.",
        body: "Traditional: developers create, players consume.\n\nUs: players co-create universe.\n\n- NPC dialogue evolves from player input\n- Rituals become permanent fossils\n- Founder players shape future lore\n\nPlayers = co-authors.\n\nUniverse grows WITH community, not FOR them.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 87,
        type: "designPhilosophy",
        hook: "Why we chose wonder over challenge.",
        body: "Most games: overcome difficulty.\n\nUs: experience wonder.\n\nNo fail states.\nNo game overs.\nNo punishment.\n\nJust discovery, beauty, connection.\n\nChallenge has its place.\nBut wonder is underserved.\n\nBuild for awe.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 88,
        type: "designPhilosophy",
        hook: "The economics of ethical monetization.",
        body: "Ethical monetization metrics:\n\n- Conversion rate: lower (no pressure)\n- LTV: higher (trust = retention)\n- Churn: lower (respect = loyalty)\n- CAC: lower (word-of-mouth)\n\nNet result: more profitable long-term.\n\nEthics = sustainable unit economics.\n\nDo well by doing good.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 89,
        type: "designPhilosophy",
        hook: "Design for the margins, succeed in the mainstream.",
        body: "Design for accessibility:\n\n- Colorblind players\n- Motor impairment\n- Screen reader users\n- Neurodivergent players\n\nResult: EVERYONE benefits.\n\nBetter UX, clearer design, more usable.\n\nInclusive design = better design.\n\nMargin → mainstream.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 90,
        type: "designPhilosophy",
        hook: "Slow games in a fast world: counter-programming strategy.",
        body: "Gaming trend: faster, louder, more.\n\nUs: slower, quieter, less.\n\n1-5 minute sessions.\nMeditative gameplay.\nAnti-stress design.\n\nCounter-programming = untapped market.\n\nWhen everyone zigs, zag.\n\nSlow is strategic.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },

    // Behind-the-Scenes (91-100) - 10 items
    {
        id: 91,
        type: "behindScenes",
        hook: "The architecture decision that saved us months.",
        body: "Early decision: build for WebGL first, mobile later.\n\nReasoning:\n- Instant distribution (no app store approval)\n- Faster iteration (no build times)\n- Cross-platform by default\n- Easier testing\n\nResult: Release Candidate 1 in 6 months.\n\nPlatform choice = velocity multiplier.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 92,
        type: "behindScenes",
        hook: "Here's my entire indie dev stack (and why).",
        body: "Indie dev toolkit:\n\n- Engine: Unity (WebGL support)\n- Backend: Firebase (free tier scales)\n- Version control: Git + GitHub\n- CI/CD: GitHub Actions\n- Design: Figma (collaboration)\n- Audio: Audacity + free assets\n- Analytics: Firebase + custom logging\n\nTotal monthly cost: $0 (free tiers).\n\nBootstrapped indie stack.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 93,
        type: "behindScenes",
        hook: "What 1,000 commits taught me about solo dev velocity.",
        body: "1,000 commit lessons:\n\n1. Small commits > big commits (rollback easier)\n2. Ship broken to main = force completion\n3. Automate everything (tests, builds, deploy)\n4. Document decisions in commit messages\n5. Git history = project memory\n\nVelocity = decision speed + automation.\n\nMomentum compounds.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 94,
        type: "behindScenes",
        hook: "The automation that runs this whole project.",
        body: "GitHub Actions workflows:\n\n- Auto-build on every push\n- Run tests before merge\n- Deploy to Firebase automatically\n- Generate release notes from commits\n- Update documentation\n\nZero manual deployment.\n\nPush to main = live in 10 minutes.\n\nAutomation = indie dev superpower.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 95,
        type: "behindScenes",
        hook: "Real talk: here's what almost killed the project.",
        body: "Crisis point: month 4.\n\nProblem: feature creep spiral.\nAdded: multiplayer, crafting, currency systems.\nResult: 0% progress on core gameplay.\n\nFix: ruthless scope cut.\nRemoved: 60% of planned features.\nFocused: 5 realms, procedural rituals.\n\nResult: shipped in 2 months.\n\nScope discipline saved the project.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "medium",
        used: false
    },
    {
        id: 96,
        type: "behindScenes",
        hook: "Why we built our own analytics instead of using off-the-shelf.",
        body: "Custom analytics reasoning:\n\n- Privacy-first (no third-party tracking)\n- Exactly what we need (no bloat)\n- Own our data (no vendor lock-in)\n- Learning opportunity\n\nCost: 2 days development.\nBenefit: lifetime control.\n\nBuild vs buy: strategic decision.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 97,
        type: "behindScenes",
        hook: "The debugging session from hell (and what I learned).",
        body: "Bug: WebGL build crashed on iOS Safari.\nEditor: worked perfectly.\nAndroid: worked perfectly.\niOS Safari: instant crash.\n\n6 hours of debugging later:\nCause: iOS Safari doesn't support WebGL 2.0 fully.\nFix: Fallback to WebGL 1.0 with feature detection.\n\nLesson: test EVERY target platform actually.\n\nAssumptions kill projects.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 98,
        type: "behindScenes",
        hook: "Tools I can't live without (and the ones I regret using).",
        body: "Can't live without:\n✓ Unity Profiler\n✓ Git\n✓ VS Code + extensions\n✓ Color Oracle (colorblind simulator)\n✓ Firebase\n\nRegret using:\n✗ Overly complex asset (Complete Game Kit™)\n✗ Too many analytics tools\n✗ Premature optimization tools\n\nSimple stack = sustainable.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 99,
        type: "behindScenes",
        hook: "Efficiency hack: how we ship faster with less code.",
        body: "Code efficiency principles:\n\n1. ScriptableObjects for data (no hardcoding)\n2. Procedural generation (write once, infinite content)\n3. Unity's new Input System (handles all devices)\n4. Firebase for backend (no server management)\n5. Reusable components (DRY principle)\n\nWrite 10 lines, get 10,000 variations.\n\nLeverage > brute force.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    },
    {
        id: 100,
        type: "behindScenes",
        hook: "Our dev process in 60 seconds.",
        body: "Daily dev loop:\n\n1. Morning: review analytics, check issues\n2. Dev: 2-hour focused coding block\n3. Test: on actual devices\n4. Commit: push to GitHub\n5. Deploy: automatic via GitHub Actions\n6. Document: update project docs\n7. Rest: Digital Sunset applies to devs too\n\nConsistent > heroic.\n\nSustainable development wins.",
        platforms: ["bluesky", "mastodon"],
        media: null,
        priority: "low",
        used: false
    }
];

// Read existing content-bank.json
const contentBankPath = path.join(__dirname, '../../public/social/content-bank.json');
const existingData = JSON.parse(fs.readFileSync(contentBankPath, 'utf8'));

// Merge additional content with existing
existingData.content = [...existingData.content, ...additionalContent];
existingData.meta.totalItems = existingData.content.length;
existingData.meta.lastUpdated = new Date().toISOString().split('T')[0];

// Write updated content bank
fs.writeFileSync(contentBankPath, JSON.stringify(existingData, null, 2));

console.log(`✅ Content bank updated: ${existingData.content.length} total items`);
console.log(`Distribution:`);
console.log(`  - Dev Updates: ${existingData.content.filter(c => c.type === 'devUpdate').length}`);
console.log(`  - Dev Education: ${existingData.content.filter(c => c.type === 'devEducation').length}`);
console.log(`  - Design Philosophy: ${existingData.content.filter(c => c.type === 'designPhilosophy').length}`);
console.log(`  - Behind Scenes: ${existingData.content.filter(c => c.type === 'behindScenes').length}`);
console.log(`  - Lore Snippets: ${existingData.content.filter(c => c.type === 'loreSnippet').length}`);
