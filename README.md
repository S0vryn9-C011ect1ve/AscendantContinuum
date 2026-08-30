# Ascendant Continuum — Emberforge Ritual Prototype

## Run
```bash
npx serve . -p 3000 -s
# then open http://localhost:3000
```

## Build paths
- Web: static files + PWA manifest + service worker
- Unity WebGL: export `index.html` + `assets/` into `TemplateData/` and WebGL build
- Android TWA: wrap with a minimal WebView pointing to `/index.html`; bridge surface in `src/bridge.js`

## Offline
Service worker precaches app shell. Ritual progress stored in localStorage.
