import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const contentPath = path.join(__dirname, '../../public/social/content-bank.json');

// Read as text
let rawText = fs.readFileSync(contentPath, 'utf8');

// PowerShell ConvertTo-Json adds extra spaces after colons
// Replace ":  " with ": " to fix
rawText = rawText.replace(/:\s\s+/g, ': ');

// Try to parse
try {
    const contentBank = JSON.parse(rawText);

    // Fix any duplicate NPC references
    let fixed = 0;
    contentBank.content.forEach(post => {
        const originalHook = post.hook;
        const originalBody = post.body;

        // Fix duplicates
        post.hook = post.hook.replace(/game characters \(game characters\)/g, 'game characters (NPCs)');
        post.hook = post.hook.replace(/game character \(game character\)/g, 'game character (NPC)');

        post.body = post.body.replace(/game characters \(game characters\)/g, 'game characters (NPCs)');
        post.body = post.body.replace(/game character \(game character\)/g, 'game character (NPC)');

        if (originalHook !== post.hook || originalBody !== post.body) {
            fixed++;
        }
    });

    // Save with proper formatting
    fs.writeFileSync(contentPath, JSON.stringify(contentBank, null, 2), 'utf8');

    console.log('✓ Fixed JSON formatting (PowerShell → standard)');
    console.log(`✓ Fixed ${fixed} duplicate NPC references`);
    console.log('✓ File saved with proper 2-space indentation');

} catch (err) {
    console.error('Error parsing JSON:', err.message);
    console.log('\nTrying alternative fix...');

    // If still failing, manually fix the most obvious issues
    rawText = rawText.replace(/:\s+{/g, ': {');
    rawText = rawText.replace(/:\s+\[/g, ': [');
    rawText = rawText.replace(/:\s+"([^"]+)"/g, ': "$1"');
    rawText = rawText.replace(/:\s+(\d+)/g, ': $1');
    rawText = rawText.replace(/:\s+(true|false|null)/g, ': $1');

    fs.writeFileSync(contentPath, rawText, 'utf8');
    console.log('✓ Applied manual spacing fixes');
}
