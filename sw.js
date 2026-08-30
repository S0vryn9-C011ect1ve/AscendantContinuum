self.addEventListener('install', e => {
  e.waitUntil(caches.open('ac-v2').then(c => c.addAll(['/', '/index.html', '/manifest.json', '/src/astro.js', '/src/world.js', '/src/bridge.js', '/src/register-sw.js', '/src/sky.js'])));
  self.skipWaiting();
});
self.addEventListener('activate', e => {
  e.waitUntil(caches.keys().then(keys => Promise.all(keys.filter(k => k !== 'ac-v2').map(k => caches.delete(k)))));
  self.clients.claim();
});
self.addEventListener('fetch', e => {
  if (e.request.method !== 'GET') return;
  e.respondWith(caches.match(e.request).then(r => r || fetch(e.request).then(res => {
    if (res.status === 200) { const c = res.clone(); caches.open('ac-v1').then(cache => cache.put(e.request, c)); }
    return res;
  }).catch(() => caches.match('/'))));
});
