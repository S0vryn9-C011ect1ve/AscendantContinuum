import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const contentPath = path.join(__dirname, '../../public/social/content-bank.json');
const content = JSON.parse(fs.readFileSync(contentPath, 'utf8'));

console.log('=== POST #144 (DIY Art/Audio Reality) ===\n');
const post144 = content.content.find(p => p.id === 144);
console.log('Hook:', post144.hook);
console.log('Body:', post144.body);

console.log('\n=== SAMPLE JARGON SIMPLIFICATIONS ===\n');

const post2 = content.content.find(p => p.id === 2);
console.log('[Post #2]', post2.hook);
console.log('Excerpt:', post2.body.substring(0, 120) + '...\n');

const post181 = content.content.find(p => p.id === 181);
if (post181) {
    console.log('[Post #181]', post181.hook);
    console.log('Excerpt:', post181.body.substring(0, 120) + '...\n');
}

const post13 = content.content.find(p => p.id === 13);
console.log('[Post #13]', post13.hook);
console.log('Excerpt:', post13.body.substring(0, 120) + '...\n');

console.log('=== WHAT CHANGED ===\n');
console.log('✓ NPC → game character (NPC)');
console.log('✓ Unity → Unity (game engine) on first mention');
console.log('✓ FPS → FPS (frames per second)');
console.log('✓ ScriptableObjects → data containers (ScriptableObjects)');
console.log('✓ API → API (data connection)');
console.log('✓ Procedural → Procedurally-generated (algorithm-created)');
console.log('✓ Post #144 now reflects: CREATING art/audio yourself!\n');

console.log('Total posts:', content.meta.totalItems);
console.log('All content now accessible to general public! 🎉');
