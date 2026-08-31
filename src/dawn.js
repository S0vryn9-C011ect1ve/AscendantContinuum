// ponytail: Dawn Citadel — prism beam reflection + difficulty tiers + memory orbs
window.ACDawn = (() => {
  const KEY = 'acd_dawn';
  const TIERS = [
    { name: 'Soft',      maxMiss: 6,  reward: 1 },
    { name: 'Standard',  maxMiss: 3,  reward: 2 },
    { name: 'Precise',   maxMiss: 1,  reward: 3 },
  ];
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }

  function dailyLayout() {
    const d = new Date();
    let h = d.getFullYear() * 366 + d.getMonth() * 31 + d.getDate();
    const r = () => { h = (h * 1664525 + 1013904223) >>> 0; return (h % 1000) / 1000; };
    return {
      source: { x: 0.12, y: 0.25 + r()*0.3 },
      prism: { x: 0.45 + r()*0.1, y: 0.45 + r()*0.15 },
      target: { x: 0.82 + r()*0.05, y: 0.3 + r()*0.4 },
    };
  }

  function beamGeometry(L, prismAngleDeg) {
    const a = prismAngleDeg * Math.PI / 180;
    const nx = Math.cos(a + Math.PI/2), ny = Math.sin(a + Math.PI/2);
    let dx = L.prism.x - L.source.x, dy = L.prism.y - L.source.y;
    const len = Math.hypot(dx, dy); dx /= len; dy /= len;
    const dot = dx*nx + dy*ny;
    const rx = dx - 2*dot*nx, ry = dy - 2*dot*ny;
    const tx = L.target.x - L.prism.x, ty = L.target.y - L.prism.y;
    const cross = Math.abs(tx*ry - ty*rx);
    const along = tx*rx + ty*ry;
    return { hit: along > 0 && cross < 0.025, along, cross, rx, ry };
  }

  function solveAngle(L) {
    for (let deg = 0; deg < 360; deg += 1) if (beamGeometry(L, deg).hit) return deg;
    return null;
  }

  function completedToday() { return load()[new Date().toDateString()]; }
  function markCompleted(angle, tierName, reward) {
    const s = load();
    s[new Date().toDateString()] = { angle, tier: tierName, reward, t: Date.now() };
    save(s);
  }
  function orbs() {
    const s = load();
    return Object.values(s).reduce((sum, e) => sum + (e.reward || 0), 0);
  }
  return { dailyLayout, beamGeometry, solveAngle, completedToday, markCompleted, orbs, TIERS };
})();
