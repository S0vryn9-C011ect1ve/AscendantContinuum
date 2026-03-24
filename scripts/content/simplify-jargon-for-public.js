import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Read content bank
const contentPath = path.join(__dirname, '../../public/social/content-bank.json');
const contentBank = JSON.parse(fs.readFileSync(contentPath, 'utf8'));

let changesCount = 0;

// Helper function to update text
function simplifyJargon(text) {
    let updated = text;
    let changed = false;

    // NPC -> game character (with context)
    if (updated.includes('NPC') && !updated.includes('game character')) {
        updated = updated.replace(/NPCs/g, 'game characters (NPCs)');
        updated = updated.replace(/NPC/g, 'game character');
        changed = true;
    }

    // FPS (without explanation) -> frames per second
    if (updated.match(/\b\d+\s*FPS\b/) && !updated.includes('frames per second')) {
        updated = updated.replace(/(\d+)\s*FPS/g, '$1 FPS (frames per second)');
        changed = true;
    }

    // Unity (when too technical) -> add context
    if (updated.includes('Unity') && !updated.includes('game engine') && !updated.includes('Unity (')) {
        // Only add context if it's the first mention
        updated = updated.replace(/\b(Unity)\b/, 'Unity (game engine)');
        changed = true;
    }

    // ScriptableObjects -> data containers
    if (updated.includes('ScriptableObjects') && !updated.includes('data container')) {
        updated = updated.replace(/ScriptableObjects/g, 'data containers (ScriptableObjects)');
        changed = true;
    }

    // API -> add context
    if (updated.includes(' API') && !updated.includes('(API)') && !updated.includes('connection')) {
        updated = updated.replace(/\bAPI\b/g, 'API (data connection)');
        changed = true;
    }

    // Procedural -> algorithm-generated
    if (updated.includes('Procedural') && !updated.includes('generated')) {
        updated = updated.replace(/Procedural/g, 'Procedurally-generated (algorithm-created)');
        changed = true;
    }

    return { updated, changed };
}

// Process all posts
contentBank.content.forEach(post => {
    // Process hook
    const hookResult = simplifyJargon(post.hook);
    if (hookResult.changed) {
        post.hook = hookResult.updated;
        changesCount++;
    }

    // Process body
    const bodyResult = simplifyJargon(post.body);
    if (bodyResult.changed) {
        post.body = bodyResult.updated;
        changesCount++;
    }
});

// UPDATE POST #144 - User IS creating art/audio themselves
const post144 = contentBank.content.find(p => p.id === 144);
if (post144) {
    post144.hook = "Solo dev reality: creating everything myself";
    post144.body = "Programming, game design, art creation, audio production, system architecture, documentation - all DIY.\n\nUsing GitHub Copilot Pro for code help. Creative juices flowing for visuals and sound.\n\nLearning as I go...";
    changesCount++;
    console.log('\n✓ Updated post #144 to reflect DIY art/audio creation');
}

// Save
fs.writeFileSync(contentPath, JSON.stringify(contentBank, null, 2), 'utf8');

console.log('=== JARGON SIMPLIFICATION COMPLETE ===\n');
console.log(`✓ Total changes: ${changesCount}`);
console.log('✓ NPC → game character (NPC)');
console.log('✓ FPS → frames per second');
console.log('✓ Unity → Unity (game engine) on first mention');
console.log('✓ Technical terms given context');
console.log('✓ Post #144 updated: YES creating art/audio yourself\n');
