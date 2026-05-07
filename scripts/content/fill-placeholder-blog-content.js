import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const postsDir = path.join(__dirname, '..', '..', 'firebase', 'public', 'blog', 'posts');

const contentBySlug = {
    'cosmic-identity-system-alignments': [
        ['How Identity Works Without a Character Creator', [
            'Instead of asking you to pick an identity from a menu, this system lets identity emerge from your ritual habits over time.',
            'If you play at dawn, your alignment trends differently than someone who plays in late-night sessions. If you favor restoration rituals, your profile diverges from someone who prefers challenge loops.'
        ]],
        ['The Twelve Alignment Model', [
            'Each alignment represents a cluster of consistent behavior, not a permanent label. The goal is reflection, not restriction.',
            'You can drift across alignments as your routine changes. That means identity is treated as living context, not a locked class.'
        ]],
        ['Design Constraints', [
            'The biggest design risk is overfitting identity to short-term behavior. To avoid that, alignment updates are smoothed over longer windows.',
            'Another risk is making one alignment feel superior. Every alignment is being designed to unlock different strengths rather than a strict power hierarchy.'
        ]],
        ['Why This Matters', [
            'This approach supports self-directed play. You are not punished for changing your style, and you are not trapped by an early decision.',
            'The long-term objective is to make the identity system feel like a mirror of your journey, not a template you were forced to choose on day one.'
        ]]
    ],

    'community-mysteries-months-to-solve': [
        ['Designing Mysteries for Long-Term Collaboration', [
            'Most puzzle arcs are solved quickly because all clues are delivered in one burst. This design intentionally spreads clues across time, contexts, and ritual states.',
            'The result is slower, community-driven discovery where different people connect different fragments at different moments.'
        ]],
        ['Structure of a Multi-Month Mystery', [
            'Each mystery is broken into layers: entry clue, pattern clue, contradiction clue, and synthesis clue. No single clue is enough alone.',
            'Clues are written so they can be independently useful while still pointing toward a shared resolution.'
        ]],
        ['Moderation and Fairness', [
            'Long-form mysteries can become gatekept if early solvers monopolize information. To reduce that, clue channels are designed to resurface older leads on a cadence.',
            'The objective is cooperative momentum instead of a race that excludes late arrivals.'
        ]],
        ['Expected Outcome', [
            'A successful mystery should create a sense of collective authorship: one person spots the anomaly, another tests the sequence, another finds the interpretation.',
            'That collaborative chain is the feature. The answer matters, but the social process matters more.'
        ]]
    ],

    'dawn-citadel-light-refraction-physics': [
        ['Why Use Real Optics in Puzzle Design', [
            'Light puzzles often feel arbitrary when beam behavior changes scene by scene. Using consistent refraction rules creates trust and learnability.',
            'When players understand that angle and medium matter, puzzle solving becomes reasoning instead of trial-and-error.'
        ]],
        ['Core Puzzle Grammar', [
            'The Dawn Citadel set uses a small grammar: source, medium, prism, and target. Complexity comes from composition, not hidden rules.',
            'Early puzzles teach one variable at a time. Later puzzles combine multiple media and moving targets to test planning.'
        ]],
        ['Readability and Accessibility', [
            'Physics-driven puzzles can become visually noisy. To keep them readable, beam paths use high-contrast boundaries and clear depth separation.',
            'The interaction layer is designed to show expected beam preview before commitment so players can reason without repeated penalty.'
        ]],
        ['Production Notes', [
            'The practical challenge is balancing simulation credibility with frame budget. Some effects are approximated where full fidelity does not improve decision quality.',
            'The design target is consistent behavior first, visual flourish second.'
        ]]
    ],

    'eternal-archive-30-day-unlock': [
        ['Purpose of a Delayed Archive', [
            'The Eternal Archive unlock is intentionally delayed so memory and reflection become part of progression.',
            'Immediate access often turns archives into checklists. A delayed unlock turns it into a milestone that marks time invested with intention.'
        ]],
        ['What the Archive Contains', [
            'When unlocked, the archive organizes key moments: ritual milestones, narrative turns, and personal notes tied to those moments.',
            'Entries are arranged to help you review growth, not just completion.'
        ]],
        ['Design Tradeoffs', [
            'A delay can frustrate completion-focused players, so the system still surfaces lightweight progress context before full unlock.',
            'The full archive remains the reward for consistency, while early cues prevent a feeling of opacity.'
        ]],
        ['Long-Term Goal', [
            'The archive is meant to be a personal timeline you return to, not a one-time reward screen.',
            'By tying unlock to elapsed journey time, the archive feels earned and emotionally anchored.'
        ]]
    ],

    'lantern-ascension-fluid-dynamics-gameplay': [
        ['Why Lantern Motion Uses Fluid Principles', [
            'Lantern movement is central to the emotional tone of this system. Basic linear interpolation felt artificial and predictable.',
            'Fluid-influenced movement gives motion a natural rhythm that supports a calmer ritual loop.'
        ]],
        ['Gameplay Impact', [
            'Lantern position now responds to flow zones and turbulence pockets, creating route planning that feels organic instead of rigid.',
            'Players are encouraged to read currents and timing rather than brute-force direct paths.'
        ]],
        ['Performance Considerations', [
            'Full simulation is expensive, so gameplay uses a hybrid approach: authored flow fields plus lightweight runtime modulation.',
            'This keeps motion expressive while protecting framerate on constrained devices.'
        ]],
        ['Tuning Strategy', [
            'The tuning pass focuses on consistency and legibility. If motion is physically plausible but unreadable, it hurts gameplay.',
            'The target is believable behavior that still communicates intention clearly during fast decisions.'
        ]]
    ],

    'living-lore-universe-rewrites-history': [
        ['Living Lore as a World-State System', [
            'Living Lore treats narrative as a system that can evolve over time instead of a fixed sequence of authored beats.',
            'When conditions change, historical context can shift, and future encounters reflect that shift.'
        ]],
        ['How Rewrites Stay Coherent', [
            'World updates are constrained by lore-safe rules so changes feel like alternate interpretation, not contradiction.',
            'Narrative anchors are preserved while secondary details adapt based on progression context.'
        ]],
        ['Player Experience Goal', [
            'The design target is to make return visits meaningful. A familiar place should feel recognizably itself, but not frozen.',
            'This creates the sense that the world is keeping time with you rather than waiting in stasis.'
        ]],
        ['Implementation Direction', [
            'Current work is focused on authoring tools: branching history tags, validation checks, and safe rollback paths for bad states.',
            'That tooling discipline is what makes dynamic lore maintainable over the long run.'
        ]]
    ],

    'meteor-showers-real-astronomy-game-events': [
        ['Tying Events to Real Celestial Timing', [
            'Meteor event windows are aligned to real astronomy schedules to make timing feel grounded and meaningful.',
            'This creates anticipation loops that are discoverable outside the game itself and reinforces a real-world cadence.'
        ]],
        ['Event Design Pattern', [
            'Each event has three phases: approach, peak, and fade. Rewards and atmosphere shift by phase so participation feels dynamic.',
            'The structure supports both short drop-in sessions and deeper sessions during peak windows.'
        ]],
        ['Reliability and Edge Cases', [
            'Time-zone handling and daylight transitions are core reliability risks. Event checks use normalized UTC gates with local presentation layers.',
            'This prevents event drift while still presenting timings in the player local context.'
        ]],
        ['Why It Improves the World', [
            'Real-timed events reduce the feeling of arbitrary content drops. The sky cadence becomes part of world logic.',
            'That consistency helps events feel discovered rather than announced.'
        ]]
    ],

    'real-stargazing-anti-screen-time-mechanic': [
        ['Designing a Mechanic That Rewards Looking Away', [
            'This feature aims to reduce compulsive session patterns by creating moments where stepping away from continuous screen focus is beneficial.',
            'The core idea is not punishment. It is a healthier play rhythm supported by intentional mechanics.'
        ]],
        ['Interaction Flow', [
            'Stargazing windows are surfaced as optional prompts, not forced interruptions. You choose whether to engage in that moment.',
            'If you opt in, the activity is short, focused, and designed to return you to the game loop without friction.'
        ]],
        ['Accessibility Considerations', [
            'Outdoor and visibility assumptions can exclude people, so every stargazing mechanic has an equivalent accessible path.',
            'Equivalent paths preserve progression value so no one is penalized for environment or ability constraints.'
        ]],
        ['Expected Benefit', [
            'The broader goal is to make session boundaries feel natural. Players should finish sessions feeling complete, not drained.',
            'A healthier cadence supports long-term trust and aligns with the project anti-pressure design principles.'
        ]]
    ],

    'time-capsules-messages-to-future-self': [
        ['Why Time Capsules Matter in Play', [
            'Time capsules turn short sessions into long-arc meaning by letting you write to a future version of yourself.',
            'That delayed reflection creates emotional continuity across days and weeks.'
        ]],
        ['Mechanic Structure', [
            'You set a message, choose a release window, and seal it. The release moment is intentionally framed as a ritual checkpoint.',
            'Capsules can be private reflection or a narrative artifact tied to your current progression state.'
        ]],
        ['Content and Tone Guidelines', [
            'Because messages may be revisited during difficult days, guidance language emphasizes encouragement and concrete reminders over vague motivation.',
            'The design focus is usefulness and care rather than novelty.'
        ]],
        ['Long-Term Role', [
            'Over time, capsules build a personal archive of intent and follow-through. That archive supports self-awareness and momentum.',
            'This is one of the clearest examples of the game treating time as a meaningful design material.'
        ]]
    ],

    'weather-integration-real-rain-delays-rituals': [
        ['Why Weather Is Part of Ritual Timing', [
            'Weather integration makes ritual timing feel less abstract by grounding parts of progression in real environmental context.',
            'Rain delays and condition modifiers create natural pacing variation without artificial cooldown pressure.'
        ]],
        ['System Behavior', [
            'The weather layer interprets conditions into gameplay tags such as reduced visibility, delayed ignition, or resonance bonus windows.',
            'These tags modify ritual setup and execution requirements rather than hard-blocking participation.'
        ]],
        ['Fallback and Reliability', [
            'Network gaps and API uncertainty are handled with safe fallback states so rituals remain playable under degraded conditions.',
            'Fallback behavior is explicit in UI so players understand what rule set is active.'
        ]],
        ['Design Outcome', [
            'Weather variance introduces meaningful day-to-day texture while preserving fairness and readability.',
            'The goal is a world that feels responsive to context, not random or punitive.'
        ]]
    ]
};

function buildHtml(sections) {
    const lines = ['            <div class="blog-post-content">'];

    for (const [heading, paragraphs] of sections) {
        lines.push(`                <h2>${heading}</h2>`);
        for (const paragraph of paragraphs) {
            lines.push(`                <p>${paragraph}</p>`);
        }
        lines.push('');
    }

    lines.push('                <p>More updates: <a href="/blog/">/blog/</a> | Daily progress: <a href="/whats-new/">/whats-new/</a></p>');
    lines.push('            </div>');
    lines.push('        </article>');

    return lines.join('\n');
}

let updated = 0;

for (const [slug, sections] of Object.entries(contentBySlug)) {
    const filePath = path.join(postsDir, `${slug}.html`);
    if (!fs.existsSync(filePath)) {
        console.warn(`Missing file for slug: ${slug}`);
        continue;
    }

    const raw = fs.readFileSync(filePath, 'utf8');
    const replacement = buildHtml(sections);
    const next = raw.replace(/\s*<div class="blog-post-content">[\s\S]*?<\/article>/, `\n${replacement}`);

    if (next !== raw) {
        fs.writeFileSync(filePath, next, 'utf8');
        updated += 1;
    }
}

console.log(`Updated ${updated} blog post files.`);
