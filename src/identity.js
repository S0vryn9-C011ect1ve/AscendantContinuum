// ponytail: deterministic cosmic identity from play history — no server needed
window.ACIdentity = (() => {
  const ADJ = ['Ember','Tide','Star','Moss','Dawn','Lantern','Veil','Storm','Root','Wisp','Lunar','Solar','Aurora','Fern','Quartz','Sage'];
  const NOUN = ['Wanderer','Keeper','Seeker','Weaver','Tracer','Listener','Binder','Watcher','Sower','Carver','Dreamer','Chronicler'];
  const REALM = ['of Emberforge','of the Still Veil','of Echo Fields','of Dawn Citadel','of Lantern Sky','of Verdant Garden','of the First Light','of the Quiet Deep'];

  function hash(str) {
    let h = 2166136261;
    for (let i = 0; i < str.length; i++) { h ^= str.charCodeAt(i); h = Math.imul(h, 16777619); }
    return h >>> 0;
  }

  function identity(progress) {
    const seed = JSON.stringify(progress) || '{}';
    const h = hash(seed);
    const name = `${ADJ[h % ADJ.length]}-${ADJ[(h >>> 8) % ADJ.length].toLowerCase()} ${NOUN[(h >>> 4) % NOUN.length]} ${REALM[(h >>> 12) % REALM.length]}`;
    const hue = h % 360;
    const points = 3 + (h >>> 6) % 4;
    return { name, hue, points };
  }

  // draws sigil onto a 2d ctx at cx,cy radius r
  function drawSigil(ctx, cx, cy, r, ident) {
    const { hue, points } = ident;
    ctx.save();
    ctx.strokeStyle = `hsl(${hue},70%,60%)`;
    ctx.fillStyle = `hsl(${hue},70%,60%)`;
    ctx.lineWidth = Math.max(1, r * 0.06);
    ctx.beginPath();
    for (let i = 0; i <= points; i++) {
      const a = (i / points) * Math.PI * 2 - Math.PI / 2;
      const x = cx + Math.cos(a) * r, y = cy + Math.sin(a) * r;
      i ? ctx.lineTo(x, y) : ctx.moveTo(x, y);
    }
    ctx.closePath(); ctx.stroke();
    ctx.beginPath(); ctx.arc(cx, cy, r * 0.18, 0, Math.PI * 2); ctx.fill();
    ctx.restore();
  }

  return { identity, drawSigil };
})();
