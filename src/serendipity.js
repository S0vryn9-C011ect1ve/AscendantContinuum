// ponytail: serendipity events — aurora sweep, deity silhouette, blessing beam
window.ACSerendipity = (() => {
  let active = null, start = 0;
  function trigger(type) { active = type; start = Date.now(); return type; }
  function roll() {
    const r = Math.random();
    if (r < 0.02) return trigger('deity');
    if (r < 0.06) return trigger('aurora');
    if (r < 0.15) return trigger('blessing');
    return null;
  }
  // render overlays; call each frame AFTER world render, BEFORE ctx.restore()
  function render(ctx, W, H, t, cam) {
    if (!active) return;
    const age = (Date.now() - start) / 1000;
    if (age > 8) { active = null; return; }
    const fade = age < 1 ? age : age > 7 ? (8 - age) : 1;
    if (active === 'aurora') {
      // wave bands sweeping across the sky
      for (let i = 0; i < 4; i++) {
        const yOff = -H * 0.35 + i * H * 0.12 + Math.sin(t * 0.8 + i) * 12;
        const g = ctx.createLinearGradient(-W/2, yOff - 30, -W/2, yOff + 30);
        const hue = 140 + i * 30;
        g.addColorStop(0, `hsla(${hue},80%,60%,0)`);
        g.addColorStop(0.5, `hsla(${hue},80%,60%,${0.10 * fade})`);
        g.addColorStop(1, `hsla(${hue},80%,60%,0)`);
        ctx.fillStyle = g;
        ctx.fillRect(-W/2, yOff - 30, W, 60);
      }
    } else if (active === 'deity') {
      // large translucent radiant figure center-sky
      const cx = 0, cy = -H * 0.15, R = 90 + Math.sin(t * 2) * 8;
      const g = ctx.createRadialGradient(cx, cy, 0, cx, cy, R);
      g.addColorStop(0, `hsla(48,90%,75%,${0.35 * fade})`);
      g.addColorStop(0.6, `hsla(48,90%,65%,${0.12 * fade})`);
      g.addColorStop(1, 'hsla(48,90%,60%,0)');
      ctx.fillStyle = g;
      ctx.beginPath(); ctx.arc(cx, cy, R, 0, Math.PI * 2); ctx.fill();
      // rays
      ctx.strokeStyle = `hsla(48,90%,75%,${0.25 * fade})`;
      ctx.lineWidth = 1.5;
      for (let i = 0; i < 12; i++) {
        const a = (i / 12) * Math.PI * 2 + t * 0.1;
        ctx.beginPath();
        ctx.moveTo(cx + Math.cos(a) * R * 0.5, cy + Math.sin(a) * R * 0.5);
        ctx.lineTo(cx + Math.cos(a) * R * 1.2, cy + Math.sin(a) * R * 1.2);
        ctx.stroke();
      }
    } else if (active === 'blessing') {
      // vertical light column at a random fixed spot
      const bx = W * 0.15;
      const g = ctx.createLinearGradient(bx, -H/2, bx, H/2);
      g.addColorStop(0, `hsla(48,95%,80%,${0.30 * fade})`);
      g.addColorStop(1, 'hsla(48,95%,80%,0)');
      ctx.fillStyle = g;
      ctx.fillRect(bx - 12, -H/2, 24, H);
      // sparkles
      for (let i = 0; i < 16; i++) {
        const sy = -H/2 + ((t * 120 + i * 90) % H);
        const sx = bx + Math.sin(i * 4.7) * 20 + Math.sin(t * 3 + i) * 6;
        ctx.globalAlpha = (0.6 + 0.4 * Math.sin(t * 5 + i)) * fade;
        ctx.fillStyle = '#fef3c7';
        ctx.beginPath(); ctx.arc(sx, sy, 1.6, 0, Math.PI * 2); ctx.fill();
      }
      ctx.globalAlpha = 1;
    }
  }
  return { roll, render, trigger, get active() { return active; } };
})();
