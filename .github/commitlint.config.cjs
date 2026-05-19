module.exports = {
    // Self-contained ruleset so CI does not depend on extra npm packages.
    parserPreset: {
        parserOpts: {
            headerPattern: /^([a-z]+)(?:\(([\w\-]+)\))?:\s(.+)$/,
            headerCorrespondence: ['type', 'scope', 'subject']
        }
    },
    rules: {
        'type-enum': [
            2,
            'always',
            [
                'build',
                'chore',
                'ci',
                'docs',
                'feat',
                'fix',
                'perf',
                'refactor',
                'revert',
                'security',
                'style',
                'test'
            ]
        ],
        'type-case': [2, 'always', 'lower-case'],
        'type-empty': [2, 'never'],
        'subject-empty': [2, 'never'],
        'subject-case': [0],
        'header-max-length': [2, 'always', 100]
    }
};
