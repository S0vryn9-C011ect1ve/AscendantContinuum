console.log('Test script starting...');
console.log('import.meta.url:', import.meta.url);
console.log('process.argv[1]:', process.argv[1]);
console.log('file:// + argv[1]:', `file://${process.argv[1]}`);
console.log('Match?', import.meta.url === `file://${process.argv[1]}`);

import { runScheduler } from './scripts/automation/content-scheduler.js';

console.log('About to run scheduler...');
runScheduler()
    .then(() => {
        console.log('✅ Scheduler completed');
        process.exit(0);
    })
    .catch((error) => {
        console.error('❌ Scheduler failed:', error);
        process.exit(1);
    });
