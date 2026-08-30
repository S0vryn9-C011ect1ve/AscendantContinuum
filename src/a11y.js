// ponytail: accessibility engine — vision modes, haptics, audio cues, per-mode secrets
window.ACA11y = (() => {
  const MODES = ['normal', 'deuteranopia', 'protanopia', 'tritanopia', 'monochrome', 'simplified'];
  // palette: accent per mode — chosen so mode-critical elements stay distinguishable
  const PALETTES = {
    normal:        { accent: '#f59e0b', hidden: null },
    deuteranopia:  { accent: '#00b7eb', hidden: '#7c3aed' }, // red-green safe, violet secrets
    protanopia:    { accent: '#0ea5e9', hidden: '#facc15' }, // yellow secrets pop
    tritanopia:    { accent: '#fb7185', hidden: '#34d399' }, // blue-yellow safe
    monochrome:    { accent: '#e5e7eb', hidden: '#ffffff' }, // brightness-only secrets
    simplified:    { accent: '#f59e0b', hidden: null },
  };
  const MODE_LABELS = {
    normal: 'Normal vision',
    deuteranopia: 'Deuteranopia (red-green)',
    protanopia: 'Protanopia (red-green)',
    tritanopia: 'Tritanopia (blue-yellow)',
    monochrome: 'Monochrome',
    simplified: 'Simplified',
  };
  const SECRETS = {
    deuteranopia: 'Violet Sigil revealed — you see what others cannot.',
    protanopia: 'Golden Sigil revealed — hidden to normal sight.',
    tritanopia: 'Emerald Sigil revealed — a quiet gift of your vision.',
    monochrome: 'White Sigil revealed — pure light finds you.',
    simplified: 'Calm Sigil revealed — the realm slows for you.',
  };

  let mode = localStorage.getItem('ac_mode') || 'normal';
  let audio = null;

  function palette() { return PALETTES[mode] || PALETTES.normal; }
  function secretFor() { return SECRETS[mode] || null; }

  function haptic(pattern) {
    if (navigator.vibrate) navigator.vibrate(pattern);
  }
  function tapBuzz() { haptic(12); }
  function completeBuzz() { haptic([40, 60, 40, 60, 120]); }

  function tone(freq = 440, ms = 120, type = 'sine') {
    try {
      audio = audio || new (window.AudioContext || window.webkitAudioContext)();
      if (audio.state === 'suspended') audio.resume();
      const o = audio.createOscillator(), g = audio.createGain();
      o.type = type; o.frequency.value = freq;
      g.gain.setValueAtTime(0.08, audio.currentTime);
      g.gain.exponentialRampToValueAtTime(0.0001, audio.currentTime + ms / 1000);
      o.connect(g); g.connect(audio.destination);
      o.start(); o.stop(audio.currentTime + ms / 1000);
    } catch { /* no audio — fine */ }
  }
  function describe(text) {
    // ponytail: aria-live is the screen-reader channel; tone is the non-visual cue
    const live = document.getElementById('a11yLive');
    if (live) live.textContent = text;
    tone(520, 90);
  }

  return { MODES, MODE_LABELS, palette, secretFor, tapBuzz, completeBuzz, tone, describe,
    get mode() { return mode; }, set mode(m) { mode = m; localStorage.setItem('ac_mode', m); } };
})();
