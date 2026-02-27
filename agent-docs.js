#!/usr/bin/env node

/**
 * Agent Docs Loader
 * 
 * Loads SOUL, USER, HEARTBEAT, TOOLS, LEARNINGS, MEMORY docs
 * Provides programmatic access for agents to reference personality, 
 * operational procedures, and memory management policies
 */

const fs = require('fs');
const path = require('path');

class AgentDocsLoader {
    constructor(baseDir = process.cwd()) {
        this.baseDir = baseDir;
        this.docs = {};
        this.loadStatus = {};
    }

    /**
     * Load a single markdown doc
     */
    loadDoc(filename) {
        try {
            const filepath = path.join(this.baseDir, filename);
            const content = fs.readFileSync(filepath, 'utf8');
            this.docs[filename.replace('.md', '')] = content;
            this.loadStatus[filename] = 'loaded';
            return content;
        } catch (error) {
            this.loadStatus[filename] = `error: ${error.message}`;
            console.warn(`⚠️  Could not load ${filename}: ${error.message}`);
            return null;
        }
    }

    /**
     * Load all core docs
     */
    loadAll() {
        const docs = ['SOUL.md', 'USER.md', 'HEARTBEAT.md', 'TOOLS.md', 'LEARNINGS.md', 'MEMORY.md'];

        docs.forEach(doc => this.loadDoc(doc));

        return this.docs;
    }

    /**
     * Get a specific doc
     */
    get(docName) {
        return this.docs[docName] || null;
    }

    /**
     * Get personality (SOUL)
     */
    getSoul() {
        return this.get('SOUL');
    }

    /**
     * Get founder context (USER)
     */
    getUser() {
        return this.get('USER');
    }

    /**
     * Get operational polling design (HEARTBEAT)
     */
    getHeartbeat() {
        return this.get('HEARTBEAT');
    }

    /**
     * Get tools cheat sheet (TOOLS)
     */
    getTools() {
        return this.get('TOOLS');
    }

    /**
     * Get learning rules (LEARNINGS)
     */
    getLearnings() {
        return this.get('LEARNINGS');
    }

    /**
     * Get memory policy (MEMORY)
     */
    getMemory() {
        return this.get('MEMORY');
    }

    /**
     * Print startup banner showing what was loaded
     */
    printStartupBanner() {
        const loaded = Object.values(this.loadStatus).filter(s => s === 'loaded').length;
        const failed = Object.entries(this.loadStatus).filter(([_, s]) => s !== 'loaded');

        console.log(`
╔════════════════════════════════════════════════════════════════════════╗
║                   📖 AGENT DOCS LOADED                                ║
╠════════════════════════════════════════════════════════════════════════╣
║                                                                        ║
║  ✓ SOUL.md          → Personality & decision framework                ║
║  ✓ USER.md          → Founder profile & context                       ║
║  ✓ HEARTBEAT.md     → Operational polling (calendar, Todoist, etc)    ║
║  ✓ TOOLS.md         → API keys, SSH, deployment targets               ║
║  ✓ LEARNINGS.md     → Mistake → Fix → Rule workflow                   ║
║  ✓ MEMORY.md        → Long-term knowledge base policy                 ║
║                                                                        ║
╠════════════════════════════════════════════════════════════════════════╣
║  Status: ${loaded}/6 docs loaded                                          ║
${failed.length > 0 ? `║  ⚠️  Warnings: ${failed.map(([name, status]) => `${name} ${status}`).join(', ')}${' '.repeat(60 - (12 + failed.map(([name, status]) => `${name} ${status}`).join(', ').length))}║` : '║  All systems ready ✓                                              ║'}
║                                                                        ║
╚════════════════════════════════════════════════════════════════════════╝
    `);
    }

    /**
     * Get status summary (for logging)
     */
    getStatus() {
        return {
            loadedCount: Object.values(this.loadStatus).filter(s => s === 'loaded').length,
            status: this.loadStatus
        };
    }
}

module.exports = { AgentDocsLoader };
