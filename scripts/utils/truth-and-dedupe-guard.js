const PROHIBITED_PATTERNS = [
    { label: 'team author metadata', regex: /The Ascendant Continuum Team/i },
    {
        label: 'paid external team claim',
        regex: /\b(we|our)\b[^.\n]{0,100}\b(hired|hire|contractor|contractors|consultant|consultants|paid team|team members)\b/i,
    },
    {
        label: 'player behavior tracking claim',
        regex: /\b(tracks how you play|tracking your behavior|behavioral analytics|tracks your behavior)\b/i,
    },
    {
        label: 'placeholder phrasing',
        regex: /\b(coming soon|placeholder|this html file serves as a placeholder|\btbd\b|\btodo\b)\b/i,
    },
];

export function getProhibitedPatterns() {
    return PROHIBITED_PATTERNS;
}

export function findProhibitedMatches(text) {
    if (!text || typeof text !== 'string') return [];

    return PROHIBITED_PATTERNS
        .filter(({ regex }) => regex.test(text))
        .map(({ label }) => label);
}

export function assertNoProhibitedContent(text, context = 'content') {
    const matches = findProhibitedMatches(text);
    if (matches.length > 0) {
        throw new Error(`Blocked ${context}: prohibited patterns found -> ${matches.join(', ')}`);
    }
}

function normalizeText(value) {
    return (value || '')
        .toLowerCase()
        .replace(/https?:\/\/\S+/g, '')
        .replace(/\s+/g, ' ')
        .trim();
}

export function socialFingerprint(entry) {
    return `${normalizeText(entry.hook)}||${normalizeText(entry.body)}`;
}
