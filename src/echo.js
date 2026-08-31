// ponytail: Echo Fields — trace constellations, accuracy tiers, orbs, mastery
window.ACEcho = (() => {
  const CONSTELLATIONS = [
    { name: 'Orion', stars: [
      { x:.30, y:.20, label:'Betelgeuse' }, { x:.42, y:.32, label:'Bellatrix' },
      { x:.50, y:.48, label:'Alnitak' }, { x:.56, y:.50, label:'Alnilam' },
      { x:.62, y:.52, label:'Mintaka' }, { x:.46, y:.72, label:'Saiph' }, { x:.66, y:.74, label:'Rigel' }] },
    { name: 'Cassiopeia', stars: [
      { x:.20, y:.40, label:'Caph' }, { x:.32, y:.30, label:'Schedar' },
      { x:.45, y:.38, label:'Gamma Cas' }, { x:.58, y:.28, label:'Ruchbah' }, { x:.70, y:.36, label:'Segin' }] },
    { name: 'Cygnus', stars: [
      { x:.50, y:.18, label:'Deneb' }, { x:.50, y:.38, label:'Sadr' },
      { x:.36, y:.44, label:'Delta Cyg' }, { x:.64, y:.44, label:'Gienah' }, { x:.50, y:.68, label:'Albireo' }] },
    { name: 'Lyra', stars: [
      { x:.30, y:.30, label:'Vega' }, { x:.44, y:.36, label:'Sheliak' },
      { x:.48, y:.48, label:'Sulafat' }, { x:.38, y:.52, label:'Delta Lyr' }, { x:.34, y:.40, label:'Zeta Lyr' }] },
  ];
  const KEY = 'ace_echo';
  const TIERS = [
    { name: 'Scout',      tolerance: 0.07, reward: 1 },
    { name: 'Navigator',  tolerance: 0.05, reward: 2 },
    { name: 'Astronomer', tolerance: 0.032, reward: 3 },
  ];
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }

  function dailyConstellation() {
    const d = new Date();
    const seed = d.getFullYear()*366 + d.getMonth()*31 + d.getDate();
    return { ...CONSTELLATIONS[seed % CONSTELLATIONS.length], seed };
  }

  function accuracy(pts, C) {
    if (!pts || pts.length < C.stars.length) return 1;
    let total = 0;
    for (let i = 0; i < C.stars.length; i++) {
      const dx = pts[i].x - C.stars[i].x, dy = pts[i].y - C.stars[i].y;
      total += Math.hypot(dx, dy);
    }
    return total / C.stars.length;
  }

  function tierFor(acc) {
    for (const t of TIERS) if (acc <= t.tolerance) return t;
    return null;
  }

  function completedToday() { return load()[new Date().toDateString()]; }
  function markCompleted(pts) {
    const C = dailyConstellation();
    const acc = accuracy(pts, C);
    const tier = tierFor(acc);
    const s = load();
    s[new Date().toDateString()] = { acc, tier: tier ? tier.name : 'Scout', reward: tier ? tier.reward : 1, t: Date.now() };
    save(s);
    return s[new Date().toDateString()];
  }
  function orbs() {
    const s = load();
    return Object.values(s).reduce((sum, e) => sum + (e.reward || 0), 0);
  }
  function mastery() {
    const s = load();
    const entries = Object.values(s);
    const tiers = {};
    entries.forEach(e => { tiers[e.tier] = (tiers[e.tier] || 0) + 1; });
    return { total: entries.length, tiers };
  }
  return { CONSTELLATIONS, TIERS, dailyConstellation, accuracy, tierFor, completedToday, markCompleted, orbs, mastery };
})();
