// ponytail: Verdant Garden — tap-and-hold seeds, they grow in real time
window.ACVerdant = (() => {
  const KEY = 'ac_garden';
  const PLANTS = ['moonvine', 'emberfern', 'starbloom', 'dewpetal', 'rootwhisper'];
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '[]'); } catch { return []; } }
  function save(list) { localStorage.setItem(KEY, JSON.stringify(list.slice(-24))); }
  function plant(x, y) {
    const list = load();
    const p = { id: Date.now().toString(36), kind: PLANTS[Math.floor(Math.random() * PLANTS.length)],
      x, y, plantedAt: Date.now(), water: 0 };
    list.push(p); save(list); return p;
  }
  // growth: 0→1 over 48h, watering accelerates
  function growth(p) {
    const ageH = (Date.now() - p.plantedAt) / 3600e3;
    return Math.min(1, ageH / 48 + p.water * 0.08);
  }
  function water(id) {
    const list = load();
    const p = list.find(p => p.id === id);
    if (p) { p.water++; save(list); }
    return p;
  }
  return { load, plant, growth, water };
})();
