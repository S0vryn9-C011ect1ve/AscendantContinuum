// ponytail: Lantern Ascension — drift upward, avoid obstacles, leave wishes
window.ACLantern = (() => {
  const KEY = 'ac_lantern';
  const MAX = 12;
  const WISHES = [
    'For the tired and the trying.','A light for heavy days.','May the quiet hold you.',
    'For the ones who persist.','You are not alone out here.','Rest is also progress.',
    'For those still searching.','A light for someone grieving.','May your roots hold.',
    'For someone starting over.','A light for the caregiver.','For the ones who connect others.',
  ];
  const OBSTACLES = [ // normalized x ranges + y band
    { x: 0.2, y: 0.7, w: 0.08 }, { x: 0.6, y: 0.5, w: 0.10 }, { x: 0.35, y: 0.35, w: 0.07 }
  ];
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }

  function randomWish() { return WISHES[Math.floor(Math.random()*WISHES.length)]; }

  function release(text) {
    const list = load();
    if ((list.lanterns || []).length >= MAX) return null;
    const l = {
      id: Date.now().toString(36),
      text: text || randomWish(),
      x: 0.45 + Math.random()*0.1,
      y: 0.88,
      vx: (Math.random()-0.5)*0.018,
      vy: -0.011 - Math.random()*0.007,
      wind: Math.random()*0.012,
      litAt: Date.now(),
      altitude: 0,
    };
    list.lanterns = list.lanterns || [];
    list.lanterns.push(l);
    save(list);
    return l;
  }

  function collide(l) {
    for (const o of OBSTACLES) {
      if (Math.abs(l.x - o.x) < o.w && l.y < o.y && l.y > o.y - 0.12) return true;
    }
    return false;
  }

  function tick() {
    const list = load();
    if (!list.lanterns) return [];
    const now = Date.now();
    list.lanterns = list.lanterns.filter(l => {
      const age = (now - l.litAt)/1000;
      if (age > 90) return false;
      l.x += l.vx + Math.sin(now/3000 + l.altitude)*l.wind;
      l.y += l.vy;
      l.altitude = Math.max(0, Math.floor((0.88 - l.y)*1000));
      if (l.x < 0 || l.x > 1 || l.y < -0.1) return false;
      if (collide(l)) return false;
      return true;
    });
    save(list);
    return list.lanterns;
  }

  function recordAltitude() {
    const list = load();
    const best = (list.lanterns || []).reduce((m,l)=>Math.max(m,l.altitude), 0);
    if (best > (list.recordAltitude || 0)) { list.recordAltitude = best; save(list); }
    return best;
  }

  function wishes() { return [...(load().wishes || [])].reverse().slice(0, 20); }
  function leaveWish(text) {
    const s = load(); s.wishes = s.wishes || []; s.wishes.push({ text, t: Date.now() }); save(s);
  }

  return { release, tick, collide, randomWish, recordAltitude, wishes, leaveWish };
})();
