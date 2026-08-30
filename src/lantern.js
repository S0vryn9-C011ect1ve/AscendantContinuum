// ponytail: Lantern Ascension — release lanterns that drift upward in shared sky
// Local-first alt/echo of ACWish; focused rendered for the Lantern realm
window.ACLantern = (() => {
  const LINES = [
    'For the tired and the trying.', 'A light for heavy days.', 'May the quiet hold you.',
    'For the ones who persist.', 'You are not alone out here.', 'Rest is also progress.',
    'For those still searching.', 'A light for someone grieving.', 'May your roots hold.',
    'For someone starting over.', 'A light for the caregiver.', 'For the ones who connect others.',
  ];
  function random() { return LINES[Math.floor(Math.random() * LINES.length)]; }
  // release uses ACWish store — same lanterns appear in both realms
  function release(text) { return window.ACWish.release(text || random()); }
  return { release, LINES };
})();
