// ponytail: Verdant Garden — tap-and-hold seeds, they grow in real time
// Expanded: 5 plant types, growth stages, weather boost, garden grid, share card
window.ACVerdant = (() => {
  const KEY = 'ac_garden';
  const PLANTS = [
    { kind: 'moonvine', emoji: '🌙', color: '#a5b4fc', stages: ['sprout','vine','bloom','glow'], growH: 36 },
    { kind: 'emberfern', emoji: '🔥', color: '#fca5a5', stages: ['frond','curl','smolder','cinder'], growH: 48 },
    { kind: 'starbloom', emoji: '⭐', color: '#fde68a', stages: ['bud','star','flare','nova'], growH: 24 },
    { kind: 'dewpetal', emoji: '💧', color: '#67e8f9', stages: ['drop','cup','basin','well'], growH: 30 },
    { kind: 'rootwhisper', emoji: '🌿', color: '#86efac', stages: ['thread','root','whisper','song'], growH: 60 },
  ];
  const MAX = 24;
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '[]'); } catch { return []; } }
  function save(list) { localStorage.setItem(KEY, JSON.stringify(list.slice(-MAX))); }

  // touch-grass multiplier: outdoor GPS bonus handled in world.js
  let outdoorBonus = 0;
  if (window.ACWorld && window.ACWorld.isOutdoor()) outdoorBonus = 0.15;

  function plant(x, y) {
    const list = load();
    if (list.length >= MAX) return null;
    const kind = PLANTS[Math.floor(Math.random() * PLANTS.length)];
    const p = { id: Date.now().toString(36), kind: kind.kind, emoji: kind.emoji, color: kind.color, stages: kind.stages, growH: kind.growH, x, y, plantedAt: Date.now(), water: 0 };
    list.push(p); save(list); return p;
  }

  function growth(p) {
    const ageH = (Date.now() - p.plantedAt) / 3600e3;
    const base = Math.min(1, ageH / p.growH + p.water * 0.08);
    return Math.min(1, base + outdoorBonus);
  }

  function stageName(p) {
    const g = growth(p);
    const i = Math.min(p.stages.length - 1, Math.floor(g * p.stages.length));
    return p.stages[i];
  }

  function water(id) {
    const list = load();
    const p = list.find(p => p.id === id);
    if (p) { p.water++; save(list); }
    return p;
  }

  function harvestReady() { return load().filter(p => growth(p) >= 1).length; }
  function harvestAll() {
    const list = load();
    const ready = list.filter(p => growth(p) >= 1);
    list.forEach(p => { if (growth(p) >= 1) p.plantedAt = 0; });
    save(list);
    return ready.length;
  }

  function snapshot() { return load().map(p => ({ ...p, g: growth(p), stage: stageName(p) })); }

  return { load, plant, growth, stageName, water, harvestReady, harvestAll, snapshot, PLANTS };
})();
