import fs from 'fs';
import path from 'path';
import { allowedRootDirectories, allowedRootFiles, allowedRootMarkdown } from './root-structure-rules.js';

const root = process.cwd();

const errors = [];
const warnings = [];

const rootEntries = fs.readdirSync(root, { withFileTypes: true });
for (const entry of rootEntries) {
    if (entry.isDirectory()) {
        if (!allowedRootDirectories.has(entry.name)) {
            warnings.push(`Unexpected root directory: ${entry.name}`);
        }
        continue;
    }

    if (entry.name.endsWith('.md')) {
        if (!allowedRootMarkdown.has(entry.name)) {
            errors.push(`Unauthorized root markdown file: ${entry.name}`);
        }
        continue;
    }

    if (
        !allowedRootFiles.has(entry.name) &&
        !entry.name.endsWith('.png') &&
        !entry.name.endsWith('.pdf') &&
        !entry.name.endsWith('.csproj')
    ) {
        warnings.push(`Unexpected root file: ${entry.name}`);
    }
}

const publicPath = path.join(root, 'public');
if (fs.existsSync(publicPath)) {
    const publicEntries = fs.readdirSync(publicPath, { withFileTypes: true });
    for (const entry of publicEntries) {
        if (entry.name !== 'social') {
            errors.push(`Deprecated root public path in use: public/${entry.name}`);
        }
    }
}

const requiredDocs = [
    'docs/standards/repository-structure.md',
    'docs/standards/naming-conventions.md',
    'docs/standards/asset-management.md',
    'docs/operations/legacy-doc-consolidation-wave-tracker.md',
    'docs/technical/GITHUB_SETUP.md'
];

for (const relativePath of requiredDocs) {
    const fullPath = path.join(root, relativePath);
    if (!fs.existsSync(fullPath)) {
        errors.push(`Missing required canonical doc: ${relativePath}`);
    }
}

if (warnings.length) {
    console.log('Structure warnings:');
    for (const warning of warnings) {
        console.log(`- ${warning}`);
    }
}

if (errors.length) {
    console.error('Structure validation failed:');
    for (const error of errors) {
        console.error(`- ${error}`);
    }
    process.exit(1);
}

console.log('Repository structure validation passed.');
