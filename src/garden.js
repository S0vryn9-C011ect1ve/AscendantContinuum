// ponytail: Verdant Garden — plant, grow, harvest; outdoor GPS bonus
window.ACVerdant = (() => {
  const KEY = 'ac_garden';
  const PLANTS = [
    { id:'sage',       name:'Sage',            icon:'🌿', stages:['seed','sprout','leaf','bloom','harvest'], days:4 },
    { id:'mint',       name:'Mint',            icon:'🍃', stages:['seed','sprout','leaf','bloom','harvest'], days:3 },
    { id:'lavender',   name:'Lavender',        icon:'💜', stages:['seed','sprout','bud','bloom','harvest'], days:5 },
    { id:'thyme',      name:'Thyme',           icon:'🌱', stages:['seed','sprout','leaf','bloom','harvest'], days:3 },
    { id:'elder',      name:'Elderberry',      icon:'🫐', stages:['seed','sprout','branch','fruit','harvest'], days:6 },
  ];
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }
  function today() { return new Date().toDateString(); }

  function outdoorBonus() {
    if (!navigator.geolocation) return 0;
    return new Promise(resolve => {
      navigator.geolocation.getCurrentPosition(
        () => resolve(0.15),
        () => resolve(0),
        { timeout: 4000, maximumAge: 60000 }
      );
    });
  }

  function plant(typeId) {
    const type = PLANTS.find(p => p.id === typeId);
    if (!type) return null;
    const s = load();
    const plot = (s.plots || []);
    const empty = plot.findIndex(p => !p.typeId);
    if (empty === -1) return null;
    plot[empty] = { typeId, planted: Date.now(), stage: 0, watered: false };
    s.plots = plot;
    save(s);
    return { plotIndex: empty, type };
  }

  function water(index) {
    const s = load();
    const p = (s.plots || [])[index];
    if (!p) return false;
    p.watered = true;
    save(s);
    return true;
  }

  function tick() {
    const s = load();
    const now = Date.now();
    (s.plots || []).forEach(p => {
      const type = PLANTS.find(t => t.id === p.typeId);
      if (!type || p.stage >= type.stages.length - 1) return;
      const dayMs = 86400000;
      const grow = (p.watered ? 0.6 : 0.35);
      const age = (now - p.planted) / dayMs;
      p.stage = Math.min(type.stages.length - 1, Math.floor(age / (type.days / (type.stages.length - 1)) * grow) + (p.watered ? 1 : 0));
    });
    save(s);
    return s.plots;
  }

  function harvest(index) {
    const s = load();
    const p = (s.plots || [])[index];
    if (!p) return null;
    const type = PLANTS.find(t => t.id === p.typeId);
    const ready = type && p.stage >= type.stages.length - 1;
    if (!ready) return null;
    const harvested = { ...p };
    s.plots[index] = null;
    save(s);
    return harvested;
  }

  function stats() {
    const s = load();
    const plots = (s.plots || []);
    const grown = plots.filter(p => p && p.stage >= (PLANTS.find(t=>t.id===p.typeId)?.stages.length||5) - 1).length;
    return { plots: plots.length, grown };
  }

  return { PLANTS, load, save, plant, water, tick, harvest, stats, outdoorBonus };
})();
