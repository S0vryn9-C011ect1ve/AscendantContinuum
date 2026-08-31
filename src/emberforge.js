// ponytail: Emberforge — daily ritual pattern shapes + moon bonus + spark economy
window.ACEmber = (() => {
  const KEY = 'ac_ember';
  const PATTERNS = ['spiral','cross','bloom'];
  const MOON_BONUS = { '🌕':3, '🌖':2, '🌗':1, '🌘':1, '🌑':0, '🌒':1, '🌓':2, '🌔':2 };
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }
  function today() { return new Date().toDateString(); }
  function moon() {
    const n = new Date();
    const s = n.getFullYear()*366 + n.getMonth()*31 + n.getDate();
    const phases = ['🌑','🌒','🌓','🌔','🌕','🌖','🌗','🌘'];
    return phases[s % 8];
  }
  function dailyPattern() {
    const d = new Date();
    const seed = d.getFullYear() * 366 + d.getMonth() * 31 + d.getDate();
    const kind = PATTERNS[seed % 3];
    const cx = 0.5, cy = 0.45;
    const pts = [];
    if (kind === 'spiral') {
      for (let i = 0; i < 6; i++) {
        const a = (i/6) * Math.PI * 2;
        const r = 0.12 + i*0.035;
        pts.push({ x: cx + Math.cos(a)*r, y: cy + Math.sin(a)*r, lit: false });
      }
    } else if (kind === 'cross') {
      pts.push({ x: cx, y: cy - 0.18, lit: false }, { x: cx, y: cy + 0.18, lit: false },
               { x: cx - 0.18, y: cy, lit: false }, { x: cx + 0.18, y: cy, lit: false },
               { x: cx - 0.10, y: cy - 0.10, lit: false }, { x: cx + 0.10, y: cy + 0.10, lit: false });
    } else {
      pts.push({ x: cx, y: cy - 0.16, lit: false }, { x: cx - 0.14, y: cy + 0.02, lit: false },
               { x: cx + 0.14, y: cy + 0.02, lit: false }, { x: cx - 0.08, y: cy + 0.15, lit: false },
               { x: cx + 0.08, y: cy + 0.15, lit: false }, { x: cx, y: cy - 0.06, lit: false });
    }
    return { kind, points: pts };
  }
  function getToday() {
    const s = load();
    if (!s[today()]) s[today()] = dailyPattern();
    return s[today()];
  }
  function tap(x, y) {
    const s = load();
    const d = getToday();
    for (const p of d.points) {
      if (p.lit) continue;
      if (Math.hypot(p.x - x, p.y - y) < 0.07) { p.lit = true; save(s); return true; }
    }
    return false;
  }
  function allLit() { return getToday().points.every(p => p.lit); }
  function sparks() {
    const m = moon();
    const bonus = MOON_BONUS[m] || 0;
    return Object.keys(load()).length * 10 + (allLit() ? 5 + bonus : 0);
  }
  return { dailyPattern, moon, tap, allLit, sparks, getToday };
})();
