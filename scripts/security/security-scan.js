#!/usr/bin/env node
/**
 * Security Scanner for Ascendant Continuum
 * Monitors for compromised packages and vulnerabilities
 * 
 * Run: npm run security:full-scan
 */

import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// ═══════════════════════════════════════════════════════════════
// CONFIGURATION
// ═══════════════════════════════════════════════════════════════

const PROJECTS = [
    { name: 'Main Project', path: path.join(__dirname, '../..') },
    { name: 'Firebase Functions', path: path.join(__dirname, '../../firebase/functions') }
];

const CRITICAL_PACKAGES = ['axios', 'gaxios', 'node-forge', 'jsonwebtoken'];
const MIN_SAFE_VERSIONS = {
    'axios': '1.13.5',
    'gaxios': '6.7.1',
    'node-forge': '1.3.4',
    'jsonwebtoken': '9.0.0'
};

// ═══════════════════════════════════════════════════════════════
// UTILITIES
// ═══════════════════════════════════════════════════════════════

function execCommand(command, cwd) {
    try {
        return execSync(command, { cwd, encoding: 'utf8', stdio: 'pipe' });
    } catch (error) {
        return error.stdout || error.message;
    }
}

function compareVersions(v1, v2) {
    const parts1 = v1.split('.').map(Number);
    const parts2 = v2.split('.').map(Number);

    for (let i = 0; i < Math.max(parts1.length, parts2.length); i++) {
        const p1 = parts1[i] || 0;
        const p2 = parts2[i] || 0;
        if (p1 > p2) return 1;
        if (p1 < p2) return -1;
    }
    return 0;
}

// ═══════════════════════════════════════════════════════════════
// SECURITY CHECKS
// ═══════════════════════════════════════════════════════════════

function checkPackageVersions(projectPath, projectName) {
    console.log(`\n🔍 Checking ${projectName}...`);
    console.log('─'.repeat(60));

    const issues = [];

    for (const pkg of CRITICAL_PACKAGES) {
        try {
            const output = execCommand(`npm list ${pkg} --depth=0`, projectPath);

            // Extract version from npm list output
            const versionMatch = output.match(new RegExp(`${pkg}@([\\d.]+)`));
            if (versionMatch) {
                const installedVersion = versionMatch[1];
                const minVersion = MIN_SAFE_VERSIONS[pkg];

                if (minVersion) {
                    const comparison = compareVersions(installedVersion, minVersion);
                    if (comparison < 0) {
                        issues.push({
                            package: pkg,
                            installed: installedVersion,
                            minimum: minVersion,
                            severity: 'HIGH'
                        });
                        console.log(`⚠️  ${pkg}: ${installedVersion} (minimum: ${minVersion})`);
                    } else {
                        console.log(`✅ ${pkg}: ${installedVersion} (up to date)`);
                    }
                } else {
                    console.log(`ℹ️  ${pkg}: ${installedVersion}`);
                }
            } else {
                console.log(`ℹ️  ${pkg}: not installed`);
            }
        } catch (error) {
            console.log(`⚠️  ${pkg}: error checking version`);
        }
    }

    return issues;
}

function runNpmAudit(projectPath, projectName) {
    console.log(`\n🛡️  Running npm audit for ${projectName}...`);
    console.log('─'.repeat(60));

    const output = execCommand('npm audit --audit-level=moderate', projectPath);

    // Parse audit output for vulnerabilities
    const vulnMatch = output.match(/(\\d+) vulnerabilities/);
    if (vulnMatch) {
        const count = parseInt(vulnMatch[1]);
        if (count === 0) {
            console.log('✅ No vulnerabilities found');
            return { count: 0, details: [] };
        } else {
            console.log(`⚠️  Found ${count} vulnerabilities`);
            console.log(output);
            return { count, details: output };
        }
    }

    return { count: -1, details: 'Unable to parse audit results' };
}

function checkForCompromisedPackages(projectPath, projectName) {
    console.log(`\n🔐 Checking for compromised packages in ${projectName}...`);
    console.log('─'.repeat(60));

    try {
        const packageLockPath = path.join(projectPath, 'package-lock.json');
        if (!fs.existsSync(packageLockPath)) {
            console.log('⚠️  package-lock.json not found');
            return [];
        }

        const packageLock = JSON.parse(fs.readFileSync(packageLockPath, 'utf8'));
        const compromisedList = [];

        // Check for suspicious patterns
        const checkPackage = (name, data) => {
            // Check for unexpected scripts in dependencies
            if (data.hasInstallScript) {
                console.log(`⚠️  ${name} has install script (review recommended)`);
            }

            // Check for integrity mismatches (would indicate tampering)
            if (data.resolved && !data.integrity) {
                console.log(`⚠️  ${name} missing integrity hash`);
                compromisedList.push({ name, issue: 'Missing integrity' });
            }
        };

        // Check all packages in lock file
        if (packageLock.packages) {
            for (const [pkgPath, data] of Object.entries(packageLock.packages)) {
                if (pkgPath && pkgPath !== '') {
                    const pkgName = pkgPath.replace('node_modules/', '');
                    checkPackage(pkgName, data);
                }
            }
        }

        if (compromisedList.length === 0) {
            console.log('✅ No compromised packages detected');
        }

        return compromisedList;
    } catch (error) {
        console.log(`⚠️  Error checking for compromised packages: ${error.message}`);
        return [];
    }
}

// ═══════════════════════════════════════════════════════════════
// REPORT GENERATION
// ═══════════════════════════════════════════════════════════════

function generateSecurityReport(results) {
    console.log('\n' + '═'.repeat(60));
    console.log('🔒 SECURITY SCAN SUMMARY');
    console.log('═'.repeat(60));
    console.log(`Scan Date: ${new Date().toISOString()}`);

    let hasIssues = false;

    // Version issues
    const allVersionIssues = results.flatMap(r => r.versionIssues);
    if (allVersionIssues.length > 0) {
        console.log('\n⚠️  VERSION ISSUES:');
        allVersionIssues.forEach(issue => {
            console.log(`   • ${issue.package}: ${issue.installed} → ${issue.minimum} (${issue.severity})`);
        });
        hasIssues = true;
    }

    // Audit issues
    const totalVulns = results.reduce((sum, r) => sum + r.auditResult.count, 0);
    if (totalVulns > 0) {
        console.log(`\n⚠️  VULNERABILITIES: ${totalVulns} found`);
        hasIssues = true;
    } else {
        console.log('\n✅ VULNERABILITIES: None found');
    }

    // Compromised packages
    const allCompromised = results.flatMap(r => r.compromised);
    if (allCompromised.length > 0) {
        console.log('\n🚨 COMPROMISED PACKAGES:');
        allCompromised.forEach(pkg => {
            console.log(`   • ${pkg.name}: ${pkg.issue}`);
        });
        hasIssues = true;
    }

    // Overall status
    console.log('\n' + '─'.repeat(60));
    if (!hasIssues) {
        console.log('✅ OVERALL STATUS: SECURE');
        console.log('   All checks passed. No critical issues detected.');
    } else {
        console.log('⚠️  OVERALL STATUS: NEEDS ATTENTION');
        console.log('   Review issues above and take appropriate action.');
    }
    console.log('═'.repeat(60) + '\n');

    // Save report
    const reportPath = path.join(__dirname, '../../logs/security-scan-report.md');
    const reportDir = path.dirname(reportPath);

    if (!fs.existsSync(reportDir)) {
        fs.mkdirSync(reportDir, { recursive: true });
    }

    const report = `# Security Scan Report\n\nGenerated: ${new Date().toISOString()}\n\n## Summary\n\n- Total Projects: ${results.length}\n- Version Issues: ${allVersionIssues.length}\n- Vulnerabilities: ${totalVulns}\n- Compromised Packages: ${allCompromised.length}\n\n## Status: ${hasIssues ? '⚠️ NEEDS ATTENTION' : '✅ SECURE'}\n`;

    fs.writeFileSync(reportPath, report);
    console.log(`📄 Report saved to: ${reportPath}`);

    return !hasIssues;
}

// ═══════════════════════════════════════════════════════════════
// MAIN
// ═══════════════════════════════════════════════════════════════

async function main() {
    console.log('🔒 Ascendant Continuum Security Scanner');
    console.log('═'.repeat(60));
    console.log(`Checking ${PROJECTS.length} projects...\n`);

    const results = [];

    for (const project of PROJECTS) {
        const versionIssues = checkPackageVersions(project.path, project.name);
        const auditResult = runNpmAudit(project.path, project.name);
        const compromised = checkForCompromisedPackages(project.path, project.name);

        results.push({
            project: project.name,
            versionIssues,
            auditResult,
            compromised
        });
    }

    const isSecure = generateSecurityReport(results);

    process.exit(isSecure ? 0 : 1);
}

// Run if called directly
if (import.meta.url === `file://${process.argv[1]}`) {
    main().catch(error => {
        console.error('❌ Security scan failed:', error);
        process.exit(1);
    });
}

export default { main };
