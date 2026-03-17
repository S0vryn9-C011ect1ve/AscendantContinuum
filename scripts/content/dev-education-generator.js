/**
 * Dev Education Generator
 * 
 * Creates Unity tutorials and technical guides from codebase patterns.
 * Format: "Here's what I learned building X in Unity" with code snippets.
 * Designed for Reddit r/Unity3D value posts (90/10 rule).
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ═══════════════════════════════════════════════════════════════
// EDUCATIONAL TOPICS & TUTORIALS
// ═══════════════════════════════════════════════════════════════

const EDUCATION_TOPICS = {
    accessibilityShaders: {
        title: 'Making Unity UI Accessible: 5 Colorblind Modes Without Breaking Your Design',
        problem: 'Most games duplicate UI assets for accessibility. Wasteful and hard to maintain.',
        solution: 'Use Unity Shader Graph with color transformation matrices per vision type',
        keyPoints: [
            'Single UI asset, multiple shader variants',
            'ScriptableObject stores color matrices for each vision type',
            'Runtime shader property updates (no asset swapping)',
            'Works with all Unity UI components',
        ],
        codeSnippet: `// Simplified example - apply colorblind matrix to UI shader
public void ApplyColorblindMode(ColorblindType type) {
    Matrix4x4 colorMatrix = GetColorblindMatrix(type);
    Material uiMaterial = GetComponent<Image>().material;
    uiMaterial.SetMatrix("_ColorTransform", colorMatrix);
}`,
        benefit: 'Accessibility without duplicate assets. Lower memory, easier maintenance.',
        relatedGame: 'From Ascendant Continuum - each mode reveals different hidden content!',
    },
    proceduralRituals: {
        title: 'Procedural Generation in Unity: Creating 10,000+ Unique Interactions',
        problem: 'Handcrafting content doesn\'t scale. Players exhaust content quickly.',
        solution: 'Weighted random systems with rule-based constraints',
        keyPoints: [
            'ScriptableObject-based "Ritual Templates" with parameters',
            'Weighted random selection ensures variety distribution',
            'Constraint solver prevents impossible combinations',
            'Seeded generation for reproducibility (daily challenges)',
        ],
        codeSnippet: `// Procedural ritual generation
public Ritual GenerateRitual(int seed) {
    Random.InitState(seed);
    var template = PickWeightedRandom(ritualTemplates);
    var actions = GenerateActions(template.actionCount);
    var constraints = template.GetConstraints();
    return SolveConstraints(actions, constraints);
}`,
        benefit: 'One system = infinite content. Players always have something new to discover.',
        relatedGame: 'Powers the daily challenges in Ascendant Continuum',
    },
    particleObjectPooling: {
        title: 'Unity Particle Pooling: 60fps on Mobile WebGL',
        problem: 'Instantiating/destroying GameObjects causes garbage collection spikes (frame drops).',
        solution: 'Object pooling pattern with particle system reuse',
        keyPoints: [
            'Pre-instantiate particle systems at startup',
            'Reuse instead of destroy (SetActive false)',
            'Generic pool manager for all VFX types',
            'Warm pool based on expected usage patterns',
        ],
        codeSnippet: `// Object pool for particle systems
public class ParticlePool : MonoBehaviour {
    private Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
    
    public ParticleSystem Get() {
        if (pool.Count > 0) {
            var ps = pool.Dequeue();
            ps.gameObject.SetActive(true);
            ps.Play();
            return ps;
        }
        return Instantiate(prefab); // Fallback if pool empty
    }
    
    public void Return(ParticleSystem ps) {
        ps.Stop();
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
    }
}`,
        benefit: 'Eliminates frame drops. Essential for WebGL mobile performance.',
        relatedGame: 'How Ascendant Continuum runs smoothly with hundreds of simultaneous particles',
    },
    firebaseRealtimeSync: {
        title: 'Unity + Firebase: Building Collective NPC Memory',
        problem: 'Want NPCs to remember ALL players\' choices, not just individual saves.',
        solution: 'Firebase Realtime Database for global player interaction tracking',
        keyPoints: [
            'Cloud Functions aggregate player choices',
            'Unity client queries aggregate data (not raw player data)',
            'Privacy-preserving (no individual tracking)',
            'NPCs adapt dialogue based on community trends',
        ],
        codeSnippet: `// Query collective NPC knowledge from Firebase
async Task<string> GetNPCDialogue(string npcId) {
    var snapshot = await FirebaseDatabase.DefaultInstance
        .GetReference($"npc_memory/{npcId}/aggregated")
        .GetValueAsync();
    
    var communityChoices = snapshot.Value as Dictionary<string, object>;
    return AdaptDialogue(communityChoices); // Generate contextual response
}`,
        benefit: 'Living world where community genuinely shapes narrative over time.',
        relatedGame: 'Core system in Ascendant Continuum - NPCs evolve with player base',
    },
    addressablesWebGL: {
        title: 'Unity Addressables for WebGL: Fast Load Times on Any Device',
        problem: 'Large WebGL builds = long initial load, players bounce before game starts.',
        solution: 'Asset bundling with Unity Addressables for progressive loading',
        keyPoints: [
            'Core game loads first (< 5MB)',
            'Realms load on-demand as player enters',
            'Shared dependencies bundled separately',
            'Texture compression profiles per platform',
        ],
        codeSnippet: `// Load realm assets on-demand
async Task LoadRealm(string realmName) {
    var handle = Addressables.LoadAssetAsync<GameObject>($"Realms/{realmName}");
    await handle.Task;
    Instantiate(handle.Result); // Realm loaded only when needed
}`,
        benefit: 'Initial load in seconds, not minutes. Much better player retention.',
        relatedGame: 'Ascendant Continuum loads in < 10 seconds on mobile',
    },
    audioReactiveVisuals: {
        title: 'Unity Audio-Reactive Particle Systems (No Code, Shader Graph Only)',
        problem: 'Want particles to react to audio without writing complex audio analysis code.',
        solution: 'Audio spectrum data → Shader Graph → Particle Force Field',
        keyPoints: [
            'AudioSource.GetSpectrumData() for frequency analysis',
            'Custom Shader Graph node receives audio data',
            'Particle Force Field modulated by audio intensity',
            'Works with any audio source in scene',
        ],
        codeSnippet: `// Send audio data to shader
void Update() {
    AudioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
    float bassIntensity = spectrumData[0..10].Average();
    particleMaterial.SetFloat("_AudioIntensity", bassIntensity);
}`,
        benefit: 'Stunning audio-reactive visuals with minimal performance cost.',
        relatedGame: 'Ritual completion effects in Ascendant Continuum pulse with audio',
    },
    sessionTimerEthics: {
        title: 'Building Ethical Play Timers in Unity (No Punishment Design)',
        problem: 'Want to encourage healthy play without punishing players who stop.',
        solution: 'Gentle reminders + cosmetic rewards for taking breaks',
        keyPoints: [
            'Session timer triggers gentle notification (not blocking)',
            'Progress always saved, no penalties',
            'Cosmetic reward for playing < 15 min/day',
            'Server-side world evolution visible when returning',
        ],
        codeSnippet: `// Non-punitive play timer
void CheckSessionLength() {
    if (sessionTime > 15 * 60 && !reminderShown) {
        ShowGentleReminder("Consider taking a break! 💜");
        reminderShown = true;
        // NO penalties, NO progress loss
    }
}`,
        benefit: 'Respects player wellbeing while maintaining engagement. Ethical game design.',
        relatedGame: 'Digital Sunset feature in Ascendant Continuum',
    },
};

// ═══════════════════════════════════════════════════════════════
// EDUCATION GENERATION
// ═══════════════════════════════════════════════════════════════

/**
 * Generate educational post from topic
 * @param {string} topicKey - Topic key from EDUCATION_TOPICS
 * @returns {Object} - Education post object
 */
export function generateEducationPost(topicKey) {
    const topic = EDUCATION_TOPICS[topicKey];

    if (!topic) {
        throw new Error(`Unknown education topic: ${topicKey}`);
    }

    return {
        type: 'devEducation',
        topic: topicKey,
        title: topic.title,
        problem: topic.problem,
        solution: topic.solution,
        keyPoints: topic.keyPoints,
        codeSnippet: topic.codeSnippet,
        benefit: topic.benefit,
        relatedGame: topic.relatedGame,
        timestamp: new Date().toISOString(),
    };
}

/**
 * Get random education topic
 * @param {Array<string>} exclude - Topic keys to exclude
 * @returns {string} - Topic key
 */
export function getRandomTopic(exclude = []) {
    const topics = Object.keys(EDUCATION_TOPICS).filter(t => !exclude.includes(t));
    return topics[Math.floor(Math.random() * topics.length)];
}

/**
 * Format education post for Reddit (90% value, 10% game mention)
 * @param {Object} post - Education post object
 * @returns {Object} - Formatted Reddit post
 */
export function formatEducationForReddit(post) {
    const { title, problem, solution, keyPoints, codeSnippet, benefit, relatedGame } = post;

    // Build Reddit-style post
    const sections = [];

    sections.push(`# ${title}\n`);
    sections.push(`**The Problem:**`);
    sections.push(problem + '\n');

    sections.push(`**The Solution:**`);
    sections.push(solution + '\n');

    sections.push(`**Key Implementation Points:**\n`);
    keyPoints.forEach((point, index) => {
        sections.push(`${index + 1}. ${point}`);
    });

    sections.push('\n**Code Example:**\n');
    sections.push('```csharp');
    sections.push(codeSnippet);
    sections.push('```\n');

    sections.push(`**Why This Works:**`);
    sections.push(benefit + '\n');

    sections.push('---\n');
    sections.push(`*${relatedGame}*`); // Subtle game mention (10%)

    const content = sections.join('\n').trim();

    return {
        title: title,
        body: content,
        type: 'devEducation',
        platform: 'reddit',
        subreddit: 'Unity3D', // Primary target
    };
}

/**
 * Format education post for social media (shorter, no code)
 * @param {Object} post - Education post object
 * @param {string} platform - Target platform
 * @returns {Object} - Formatted content
 */
export function formatEducationForPlatform(post, platform = 'bluesky') {
    const { title, problem, solution, keyPoints, benefit } = post;

    // Shorter format for Twitter/Bluesky/Mastodon
    const sections = [];

    sections.push(`💡 Unity Dev Tip:\n`);
    sections.push(`${title}\n`);
    sections.push(`Problem: ${problem}\n`);
    sections.push(`Solution: ${solution}\n`);

    sections.push('Key points:');
    keyPoints.slice(0, 3).forEach((point, index) => {
        sections.push(`${index + 1}. ${point}`);
    });

    sections.push('');
    sections.push(`Why it matters: ${benefit}`);

    const content = sections.join('\n').trim();

    return {
        title: `Unity Dev Tip: ${title}`,
        body: content,
        type: 'devEducation',
    };
}

/**
 * Generate all education posts
 * @returns {Array<Object>} - Array of posts
 */
export function generateAllEducationPosts() {
    return Object.keys(EDUCATION_TOPICS).map(topicKey =>
        generateEducationPost(topicKey)
    );
}

// ═══════════════════════════════════════════════════════════════
// CLI INTERFACE (for testing)
// ═══════════════════════════════════════════════════════════════

if (import.meta.url === `file://${process.argv[1]}`) {
    console.log('💡 Dev Education Generator\n');

    console.log('Available topics:');
    Object.keys(EDUCATION_TOPICS).forEach((key, index) => {
        console.log(`  ${index + 1}. ${EDUCATION_TOPICS[key].title}`);
    });
    console.log('');

    // Generate sample post
    const topicKey = getRandomTopic();
    const post = generateEducationPost(topicKey);

    console.log(`📝 Sample Reddit Post:\n`);
    const redditFormatted = formatEducationForReddit(post);
    console.log(redditFormatted.body);
    console.log('');

    console.log('═'.repeat(50));
    console.log('\n📱 Sample Social Post:\n');
    const socialFormatted = formatEducationForPlatform(post);
    console.log(socialFormatted.body);
}

export default {
    generateEducationPost,
    getRandomTopic,
    formatEducationForReddit,
    formatEducationForPlatform,
    generateAllEducationPosts,
    EDUCATION_TOPICS,
};
