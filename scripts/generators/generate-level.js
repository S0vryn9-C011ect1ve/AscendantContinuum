import fs from 'fs';
import path from 'path';

const [, , slugArg] = process.argv;
if (!slugArg) {
    console.error('Usage: npm run generate:level -- <level-slug>');
    process.exit(1);
}

const slug = slugArg.toLowerCase().replace(/[^a-z0-9-]/g, '-');
const targetDir = path.join(process.cwd(), 'docs', 'operations', 'levels');
const targetFile = path.join(targetDir, `${slug}.md`);

fs.mkdirSync(targetDir, { recursive: true });
if (fs.existsSync(targetFile)) {
    console.error(`Refusing to overwrite existing file: ${targetFile}`);
    process.exit(1);
}

const content = `# ${slug}\n\n## Purpose\n\n## Scene Mapping\n\n## Gameplay Beats\n\n## Assets\n\n## Validation Checklist\n\n- [ ] Scene path assigned\n- [ ] Prefabs mapped\n- [ ] Audio mapped\n- [ ] UI/HUD requirements captured\n`;

fs.writeFileSync(targetFile, content, 'utf8');
console.log(`Created ${path.relative(process.cwd(), targetFile)}`);
