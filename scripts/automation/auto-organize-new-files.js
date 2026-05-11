import fs from 'fs';
import path from 'path';
import { allowedRootMarkdown } from '../validation/root-structure-rules.js';

const root = process.cwd();
const inboxDir = path.join(root, 'docs', 'inbox');

function ensureInbox() {
    fs.mkdirSync(inboxDir, { recursive: true });
}

function moveToInbox(filePath) {
    ensureInbox();

    const fileName = path.basename(filePath);
    const destination = path.join(inboxDir, fileName);

    if (path.resolve(filePath) === path.resolve(destination)) {
        return null;
    }

    if (!fs.existsSync(filePath) || !fs.statSync(filePath).isFile()) {
        return null;
    }

    const finalDestination = fs.existsSync(destination)
        ? path.join(inboxDir, `${path.parse(fileName).name}-${Date.now()}${path.parse(fileName).ext}`)
        : destination;

    fs.renameSync(filePath, finalDestination);
    return finalDestination;
}

function organizeExistingRootMarkdown() {
    const entries = fs.readdirSync(root, { withFileTypes: true });
    for (const entry of entries) {
        if (!entry.isFile()) {
            continue;
        }

        if (!entry.name.endsWith('.md')) {
            continue;
        }

        if (allowedRootMarkdown.has(entry.name)) {
            continue;
        }

        const source = path.join(root, entry.name);
        const destination = moveToInbox(source);
        if (destination) {
            console.log(`Moved ${entry.name} -> ${path.relative(root, destination)}`);
        }
    }
}

function watchRoot() {
    console.log('Watching repository root for new misplaced markdown files...');

    fs.watch(root, { persistent: true }, (eventType, fileName) => {
        if (!fileName || typeof fileName !== 'string') {
            return;
        }

        if (!fileName.endsWith('.md')) {
            return;
        }

        if (allowedRootMarkdown.has(fileName)) {
            return;
        }

        const source = path.join(root, fileName);
        setTimeout(() => {
            try {
                const destination = moveToInbox(source);
                if (destination) {
                    console.log(`Auto-organized ${fileName} -> ${path.relative(root, destination)}`);
                }
            } catch (error) {
                console.error(`Failed to organize ${fileName}:`, error.message);
            }
        }, 150);
    });

    process.on('SIGINT', () => {
        console.log('\nStopping organizer watcher.');
        process.exit(0);
    });
}

ensureInbox();

if (process.argv.includes('--scan-once')) {
    organizeExistingRootMarkdown();
    process.exit(0);
}

organizeExistingRootMarkdown();
watchRoot();