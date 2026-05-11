import fs from 'fs';
import path from 'path';

const projectRoot = process.cwd();
const assetsRoot = path.join(projectRoot, 'Assets', '_Project');
const outputDir = path.join(projectRoot, 'docs', 'operations');
const outputPath = path.join(outputDir, 'asset-manifest.json');

function walk(dir, files = []) {
    if (!fs.existsSync(dir)) {
        return files;
    }
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            walk(fullPath, files);
        } else if (!entry.name.endsWith('.meta')) {
            files.push(path.relative(projectRoot, fullPath).replace(/\\/g, '/'));
        }
    }
    return files;
}

const manifest = {
    generatedAt: new Date().toISOString(),
    art: walk(path.join(assetsRoot, 'Art')),
    audio: walk(path.join(assetsRoot, 'Audio')),
    prefabs: walk(path.join(assetsRoot, 'Prefabs')),
    scenes: walk(path.join(assetsRoot, 'Scenes')),
    shaders: walk(path.join(assetsRoot, 'Shaders'))
};

fs.mkdirSync(outputDir, { recursive: true });
fs.writeFileSync(outputPath, `${JSON.stringify(manifest, null, 2)}\n`, 'utf8');
console.log(`Wrote ${path.relative(projectRoot, outputPath)}`);
