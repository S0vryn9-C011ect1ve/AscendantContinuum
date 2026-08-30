// ponytail: Echo Fields — trace constellations by connecting stars in order
// Real constellation shapes, tap each star in sequence; complete = sigil + memory orb
window.ACEcho = (() => {
  // simplified real constellation shapes, normalized 0..1 space
  const CONSTELLATIONS = [
    { name: 'Orion', stars: [
      { x: .30, y: .20, label: 'Betelgeuse' }, { x: .42, y: .32, label: 'Bellatrix' },
      { x: .50, y: .48, label: 'Alnitak' }, { x: .56, y: .50, label: 'Alnilam' },
      { x: .62, y: .52, label: 'Mintaka' }, { x: .46, y: .72, label: 'Saiph' },
      { x: .66, y: .74, label: 'Rigel' }] },
    { name: 'Cassiopeia', stars: [
      { x: .20, y: .40, label: 'Caph' }, { x: .32, y: .30, label: 'Schedar' },
      { x: .45, y: .38, label: 'Gamma Cas' }, { x: .58, y: .28, label: 'Ruchbah' },
      { x: .70, y: .36, label: 'Segin' }] },
    { name: 'Cygnus', stars: [
      { x: .50, y: .18, label: 'Deneb' }, { x: .50, y: .38, label: 'Sadr' },
      { x: .36, y: .44, label: 'Delta Cyg' }, { x: .64, y: .44, label: 'Gienah' },
      { x: .50, y: .68, label: 'Albireo' }] },
    { name: 'Lyra', stars: [
      { x: .30, y: .30, label: 'Vega' }, { x: .44, y: .36, label: 'Sheliak' },
      { x: .48, y: .48, label: 'Sulafat' }, { x: .38, y: .52, label: 'Delta Lyr' },
      { x: .34, y: .40, label: 'Zeta Lyr' }] },
  ];
  const KEY = 'ace_echo';
  function load() { try { return JSON.parse(localStorage.getItem(KEY) || '{}'); } catch { return {}; } }
  function save(s) { localStorage.setItem(KEY, JSON.stringify(s)); }

  // daily pick — same constellation worldwide per date
  function dailyConstellation() {
    const d = new Date();
    const seed = d.getFullYear() * 366 + d.getMonth() * 31 + d.getDate();
    return CONSTELLATIONS[seed % CONSTELLATIONS.length];
  }

  function completedToday() { return load()[new Date().toDateString()]; }
  function markCompleted(accuracy) {
    const s = load();
    s[new Date().toDateString()] = { accuracy, t: Date.now() };
    save(s);
  }
  function totalCompleted() { return Object.keys(load()).length; }

  return { CONSTELLATIONS, dailyConstellation, completedToday, markCompleted, totalCompleted };
})();
