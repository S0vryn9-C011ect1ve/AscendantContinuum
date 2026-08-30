// ponytail: local-first time capsules — plant now, open later, tagged with sky state
window.ACCapsule = (() => {
  const KEY = 'ac_capsules';
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '[]'); } catch { return []; } }
  function save(list) { localStorage.setItem(KEY, JSON.stringify(list.slice(-40))); }
  function plant(text, days, sky) {
    const list = load();
    const c = { id: Date.now().toString(36), text, openAt: Date.now() + days * 864e5,
      plantedAt: Date.now(), sky: sky || 'unknown', x: 0.1 + Math.random() * 0.8, y: 0.6 + Math.random() * 0.3, opened: false };
    list.push(c); save(list); return c;
  }
  function due() { const now = Date.now(); return load().filter(c => !c.opened && c.openAt <= now); }
  function open(id) {
    const list = load();
    const c = list.find(c => c.id === id);
    if (c) { c.opened = true; save(list); }
    return c;
  }
  return { load, plant, due, open };
})();
