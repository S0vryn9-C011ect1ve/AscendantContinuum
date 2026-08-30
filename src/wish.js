// ponytail: local-first Wish Wall — lanterns stored in localStorage, sync layer pluggable later
window.ACWish = (() => {
  const KEY = 'ac_lanterns';
  const WISHES = [
    'May your path be lit.', 'A quiet wish for healing.', 'For those still searching.',
    'May the stars keep you.', 'Rest is also progress.', 'You are not alone out here.',
    'For the ones who persist.', 'A light for heavy days.', 'May your roots hold.',
    'Somewhere, someone wished this for you.',
  ];
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '[]'); } catch { return []; } }
  function save(list) { localStorage.setItem(KEY, JSON.stringify(list.slice(-60))); }
  function release(text) {
    const list = load();
    const l = { id: Date.now().toString(36), text: text || WISHES[Math.floor(Math.random() * WISHES.length)],
      x: 0.1 + Math.random() * 0.8, drift: (Math.random() - 0.5) * 0.0002, t: Date.now() };
    list.push(l); save(list); return l;
  }
  return { load, release, WISHES };
})();
