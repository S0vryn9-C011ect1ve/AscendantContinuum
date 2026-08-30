// ponytail: Dawn Citadel — rotate prism to guide a light beam to the target
// One mirror, tap to rotate 15°; align beam to the citadel's heart-crystal
window.ACDawn = (() => {
  const KEY = 'acd_dawn';
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }

  // daily layout — source, prism center, target; seeded per date
  function dailyLayout() {
    const d = new Date();
    let h = d.getFullYear() * 366 + d.getMonth() * 31 + d.getDate();
    const r = () => { h = (h * 1664525 + 1013904223) >>> 0; return (h % 1000) / 1000; };
    return {
      source: { x: 0.12, y: 0.25 + r() * 0.3 },
      prism: { x: 0.45 + r() * 0.1, y: 0.45 + r() * 0.15 },
      target: { x: 0.82 + r() * 0.05, y: 0.3 + r() * 0.4 },
      requiredAngle: 0, // solved at runtime by geometry
    };
  }

  // beam: source → prism → reflect at prismAngle → does it hit target?
  function beamGeometry(L, prismAngleDeg) {
    const a = prismAngleDeg * Math.PI / 180;
    // mirror face direction is angle a; normal is a + 90°
    const nx = Math.cos(a + Math.PI / 2), ny = Math.sin(a + Math.PI / 2);
    // incoming direction
    let dx = L.prism.x - L.source.x, dy = L.prism.y - L.source.y;
    const len = Math.hypot(dx, dy); dx /= len; dy /= len;
    // reflection: r = d - 2(d·n)n
    const dot = dx * nx + dy * ny;
    const rx = dx - 2 * dot * nx, ry = dy - 2 * dot * ny;
    // distance from target to the reflected line through prism
    const tx = L.target.x - L.prism.x, ty = L.target.y - L.prism.y;
    const cross = Math.abs(tx * ry - ty * rx);
    const along = tx * rx + ty * ry; // must be positive (beam goes toward target)
    return { hit: along > 0 && cross < 0.025, along, cross, rx, ry };
  }

  // required angle that solves today's layout
  function solveAngle(L) {
    for (let deg = 0; deg < 360; deg += 1) {
      if (beamGeometry(L, deg).hit) return deg;
    }
    return null;
  }

  function completedToday() { return load()[new Date().toDateString()]; }
  function markCompleted(angle) {
    const s = load();
    s[new Date().toDateString()] = { angle, t: Date.now() };
    save(s);
  }

  return { dailyLayout, beamGeometry, solveAngle, completedToday, markCompleted };
})();
