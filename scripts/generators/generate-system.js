import fs from 'fs';
import path from 'path';

const [, , nameArg, domainArg = 'Gameplay'] = process.argv;
if (!nameArg) {
    console.error('Usage: npm run generate:system -- <SystemName> [DomainFolder]');
    process.exit(1);
}

const className = nameArg.replace(/[^a-zA-Z0-9]/g, '');
const domain = domainArg.replace(/[^a-zA-Z0-9]/g, '');
const targetDir = path.join(process.cwd(), 'Assets', '_Project', 'Scripts', domain);
const targetFile = path.join(targetDir, `${className}.cs`);

fs.mkdirSync(targetDir, { recursive: true });
if (fs.existsSync(targetFile)) {
    console.error(`Refusing to overwrite existing file: ${targetFile}`);
    process.exit(1);
}

const content = `using UnityEngine;\n\nnamespace AscendantContinuum.${domain}\n{\n    public class ${className} : MonoBehaviour\n    {\n        private void Awake()\n        {\n        }\n    }\n}\n`;

fs.writeFileSync(targetFile, content, 'utf8');
console.log(`Created ${path.relative(process.cwd(), targetFile)}`);
