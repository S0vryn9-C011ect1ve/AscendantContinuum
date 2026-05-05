/**
 * Link Checker — HTTP validation for all URLs before publishing
 *
 * Validates that every link in content returns HTTP 200 before
 * anything gets posted or committed. No external dependencies —
 * uses Node.js built-in https/http modules only.
 *
 * Usage (CLI):
 *   node scripts/validation/link-checker.js https://example.com https://other.com
 *   node scripts/validation/link-checker.js --fail-fast https://example.com
 *
 * Usage (module):
 *   import { checkLinks, checkLink } from './link-checker.js';
 *   const results = await checkLinks(['https://ascendant-continuum.web.app/blog/']);
 */

import https from 'https';
import http from 'http';

const MAX_REDIRECTS = 5;
const TIMEOUT_MS = 10_000;

/**
 * Check a single URL. Follows redirects up to MAX_REDIRECTS.
 * Tries HEAD first, falls back to GET if HEAD returns 405 or similar.
 * @param {string} url
 * @param {number} [redirectCount=0]
 * @param {'HEAD'|'GET'} [method='HEAD']
 * @returns {Promise<{ url: string, status: number|null, ok: boolean, error: string|null }>}
 */
export async function checkLink(url, redirectCount = 0, method = 'HEAD') {
    if (redirectCount > MAX_REDIRECTS) {
        return { url, status: null, ok: false, error: `Too many redirects (>${MAX_REDIRECTS})` };
    }

    let parsed;
    try {
        parsed = new URL(url);
    } catch {
        return { url, status: null, ok: false, error: 'Invalid URL format' };
    }

    const lib = parsed.protocol === 'https:' ? https : http;

    return new Promise((resolve) => {
        const options = {
            hostname: parsed.hostname,
            port: parsed.port || (parsed.protocol === 'https:' ? 443 : 80),
            path: parsed.pathname + parsed.search,
            method,
            headers: {
                'User-Agent': 'AscendantContinuum-LinkChecker/1.0',
                Accept: 'text/html,application/xhtml+xml,*/*',
            },
            timeout: TIMEOUT_MS,
        };

        const req = lib.request(options, (res) => {
            const { statusCode, headers } = res;

            // Drain the body so the socket can be reused
            res.resume();

            // Follow 301/302/307/308 redirects
            if ([301, 302, 307, 308].includes(statusCode) && headers.location) {
                const next = new URL(headers.location, url).href;
                resolve(checkLink(next, redirectCount + 1, method));
                return;
            }

            // HEAD returning 405 Method Not Allowed — retry with GET
            if (method === 'HEAD' && statusCode === 405) {
                resolve(checkLink(url, redirectCount, 'GET'));
                return;
            }

            const ok = statusCode >= 200 && statusCode < 300;
            resolve({
                url,
                status: statusCode,
                ok,
                error: ok ? null : `HTTP ${statusCode}`,
            });
        });

        req.on('timeout', () => {
            req.destroy();
            resolve({ url, status: null, ok: false, error: `Timeout after ${TIMEOUT_MS}ms` });
        });

        req.on('error', (err) => {
            resolve({ url, status: null, ok: false, error: err.message });
        });

        req.end();
    });
}

/**
 * Check multiple URLs concurrently.
 * @param {string[]} urls
 * @returns {Promise<Array<{ url: string, status: number|null, ok: boolean, error: string|null }>>}
 */
export async function checkLinks(urls) {
    if (!Array.isArray(urls) || urls.length === 0) {
        return [];
    }
    return Promise.all(urls.map((u) => checkLink(u)));
}

/**
 * Extract all absolute URLs from a string.
 * @param {string} text
 * @returns {string[]}
 */
export function extractUrls(text) {
    const pattern = /https?:\/\/[^\s"'<>)\]]+/g;
    const found = text.match(pattern) ?? [];
    // Deduplicate
    return [...new Set(found)];
}

// ─── CLI entry point ───────────────────────────────────────────────────────────

if (process.argv[1] && process.argv[1].endsWith('link-checker.js')) {
    const args = process.argv.slice(2);
    const failFast = args.includes('--fail-fast');
    const urls = args.filter((a) => !a.startsWith('--'));

    if (urls.length === 0) {
        console.error('Usage: node scripts/validation/link-checker.js [--fail-fast] <url1> [url2 ...]');
        process.exit(1);
    }

    console.log(`\n🔗 Checking ${urls.length} URL(s)...\n`);

    const results = await checkLinks(urls);
    let hasFailure = false;

    for (const result of results) {
        if (result.ok) {
            console.log(`  ✅ ${result.status} — ${result.url}`);
        } else {
            console.error(`  ❌ ${result.error} — ${result.url}`);
            hasFailure = true;
            if (failFast) {
                console.error('\nFailing fast — pipeline blocked.');
                process.exit(1);
            }
        }
    }

    if (hasFailure) {
        console.error(`\n❌ ${results.filter((r) => !r.ok).length} link(s) failed. Pipeline blocked.\n`);
        process.exit(1);
    }

    console.log(`\n✅ All ${results.length} link(s) valid.\n`);
    process.exit(0);
}
