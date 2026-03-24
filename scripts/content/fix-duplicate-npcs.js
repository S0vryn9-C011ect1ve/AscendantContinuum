import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const contentPath = path.join(__dirname, '../../public/social/content-bank.json');
const contentBank = JSON.parse(fs.readFileSync(contentPath, 'utf8'));

let fixed = 0;

contentBank.content.forEach(post => {
    const originalHook = post.hook;
    const originalBody = post.body;

    // Fix duplicate replacements
    post.hook = post.hook.replace(/game characters \(game characters\)/g, 'game characters (NPCs)');
    post.hook = post.hook.replace(/game character \(game character\)/g, 'game character (NPC)');

    post.body = post.body.replace(/game characters \(game characters\)/g, 'game characters (NPCs)');
    post.body = post.body.replace(/game character \(game character\)/g, 'game character (NPC)');

    if (originalHook !== post.hook || originalBody !== post.body) {
        fixed++;
    }
});

fs.writeFileSync(contentPath, JSON.stringify(contentBank, null, 2), 'utf8');

console.log(`✓ Fixed ${fixed} duplicate replacements`);
