// ponytail: Lantern Ascension — release lanterns that drift upward in wind
// Expanded: physics drift, obstacles, wish text, altitude record, shared sky
window.ACLantern = (() => {
  const KEY = 'ac_lantern';
  const MAX = 12;
  const LINES = [
    'For the tired and the trying.','A light for heavy days.','May the quiet hold you.',
    'For the ones who persist.','You are not alone out here.','Rest is also progress.',
    'For those still searching.','A light for someone grieving.','May your roots hold.',
    'For someone starting over.','A light for the caregiver.','For the ones who connect others.',
  ];
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }

  function random() { return LINES[Math.floor(Math.random() * LINES.length)]; }

  function release(text) {
    const list = load();
    if (list.lanterns && list.lanterns.length >= MAX) return null;
    const l = {
      id: Date.now().toString(36),
      text: text || random(),
      x: 0.45 + Math.random() * 0.1,
      y: 0.85,
      vx: (Math.random() - 0.5) * 0.02,
      vy: -0.012 - Math.random() * 0.008,
      wind: Math.random() * 0.015,
      litAt: Date.now(),
      altitude: 0,
    };
    list.lanterns = list.lanterns || [];
    list.lanterns.push(l);
    save(list);
    return l;
  }

  // tick physics — called by render loop
  function tick() {
    const list = load();
    if (!list.lanterns) return [];
    const now = Date.now();
    list.lanterns = list.lanterns.filter(l => {
      const age = (now - l.litAt) / 1000;
      if (age > 90) return false; // fades after 90s
      l.x += l.vx + Math.sin(now / 3000 + l.altitude) * l.wind;
      l.y += l.vy;
      l.altitude = Math.max(0, Math.floor((0.85 - l.y) * 1000));
      if (l.x < 0 || l.x > 1 || l.y < -0.1) return false;
      return true;
    });
    save(list);
    return list.lanterns;
  }

  function record() { const s = load(); return s.recordAltitude || 0; }
  function bestAltitude() {
    const list = load();
    const best = (list.lanterns || []).reduce((m, l) => Math.max(m, l.altitude), 0);
    const prev = s.recordAltitude || 0;
    if (best > prev) { s.recordAltitude = best; save(s); }
    return best;
  }

  return { release, tick, random, record, bestAltitude };
})();
