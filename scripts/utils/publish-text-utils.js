const URL_REGEX = /https?:\/\/[^\s)]+/gi;

export const PLATFORM_LIMITS = Object.freeze({
    bluesky: 300,
    mastodon: 500,
    discord: 2000,
});

const REPLACEMENTS = [
    [/\u2018|\u2019/g, "'"],
    [/\u201C|\u201D/g, '"'],
    [/\u2013|\u2014/g, '-'],
    [/\u2026/g, '...'],
    [/\u2190|\u2192/g, '->'],
    [/\u2260/g, '!='],
    [/\u00B1/g, '+/-']
];

export function normalizeForPublishing(text) {
    if (!text) return '';

    let normalized = String(text);

    for (const [pattern, replacement] of REPLACEMENTS) {
        normalized = normalized.replace(pattern, replacement);
    }

    // Strip non-ASCII characters to keep post text platform-safe.
    normalized = normalized
        .normalize('NFKD')
        .replace(/[^\x00-\x7F]/g, '')
        .replace(/\r\n/g, '\n')
        .replace(/\n{3,}/g, '\n\n')
        .trim();

    return normalized;
}

export function extractUrls(text) {
    if (!text) return [];
    const matches = String(text).match(URL_REGEX);
    return matches ? matches : [];
}

export function extractLastUrl(text) {
    const urls = extractUrls(text);
    return urls.length > 0 ? urls[urls.length - 1] : null;
}

export function ensureUrlAtEnd(text, fallbackUrl = null) {
    const clean = normalizeForPublishing(text);
    const url = extractLastUrl(clean) || fallbackUrl;

    if (!url) return clean;

    const withoutUrls = clean.replace(URL_REGEX, '').replace(/\s+$/g, '');
    return `${withoutUrls}\n\n${url}`;
}

export function truncatePreservingLastUrl(text, maxLength) {
    const clean = normalizeForPublishing(text);
    if (clean.length <= maxLength) return clean;

    const lastUrl = extractLastUrl(clean);
    if (!lastUrl) {
        return `${clean.substring(0, maxLength - 3).trimEnd()}...`;
    }

    if (lastUrl.length + 6 >= maxLength) {
        return `${clean.substring(0, maxLength - 3).trimEnd()}...`;
    }

    const body = clean.replace(URL_REGEX, '').trim();
    const allowedBodyLength = maxLength - lastUrl.length - 4;
    const trimmedBody = body.length > allowedBodyLength
        ? `${body.substring(0, allowedBodyLength - 3).trimEnd()}...`
        : body;

    return `${trimmedBody}\n\n${lastUrl}`;
}

export function fitForPlatform(text, platform, fallbackUrl = null) {
    const limit = PLATFORM_LIMITS[platform];
    if (!limit) {
        return normalizeForPublishing(text);
    }

    const withUrl = fallbackUrl
        ? ensureUrlAtEnd(text, fallbackUrl)
        : normalizeForPublishing(text);

    return truncatePreservingLastUrl(withUrl, limit);
}
