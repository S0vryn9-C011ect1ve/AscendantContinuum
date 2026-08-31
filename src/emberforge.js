// ponytail: Emberforge — daily ritual pattern shapes (spiral/cross/bloom)
window.ACEmber = (() => {
  const KEY = 'ac_ember';
  const PATTERNS = ['spiral','cross','bloom'];
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }
  function today() { return new Date().toDateString(); }
  function dailyPattern() {
    const d = new Date();
    const seed = d.getFullYear() * 366 + d.getMonth() * 31 + d.getDate();
    const kind = PATTERNS[seed % 3];
    const pts = [];
    const cx = 0.5, cy = 0.45;
    if (kind === 'spiral') {
      for (let i = 0; i < 5; i++) {
        const a = (i / 5) * Math.PI * 2;
        const r = 0.12 + i * 0.04;
        pts.push({ x: cx + Math.cos(a) * r, y: cy + Math.sin(a) * r, lit: false });
      }
    } else if (kind === 'cross') {
      pts.push({ x: cx, y: cy - 0.18, lit: false }, { x: cx, y: cy + 0.18, lit: false },
               { x: cx - 0.18, y: cy, lit: false }, { x: cx + 0.18, y: cy, lit: false },
               { x: cx, y: cy, lit: false });
    } else {
      pts.push({ x: cx, y: cy - 0.15, lit: false }, { x: cx - 0.15, y: cy, lit: false },
               { x: cx + 0.15, y: cy, lit: false }, { x: cx - 0.10, y: cy + 0.13, lit: false },
               { x: cx + 0.10, y: cy + 0.13, lit: false });
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
      if (Math.hypot(p.x - x, p.y - y) < 0.06) { p.lit = true; save(s); return true; }
    }
    return false;
  }
  function allLit() { return getToday().points.every(p => p.lit); }
  function sparks() { return Object.keys(load()).length * 10 + (allLit() ? 5 : 0); }
  return { dailyPattern, tap, allLit, sparks, getToday };
})();
