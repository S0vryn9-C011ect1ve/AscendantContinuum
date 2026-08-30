// ponytail: minimal sky background shim for prototype
(() => {
  const canvas = document.getElementById('sky');
  if (!canvas) return;
  const ctx = canvas.getContext('2d');
  let W, H, dpr = Math.max(1, window.devicePixelRatio || 1);
  const stars = Array.from({ length: 260 }, () => ({
    x: Math.random(), y: Math.random(), r: Math.random() * 1.2 + 0.3, a: Math.random(), speed: Math.random() * 0.01 + 0.002, phase: Math.random() * 6.28
  }));
  function resize() {
    const r = canvas.parentElement.getBoundingClientRect();
    W = r.width; H = r.height;
    canvas.width = W * dpr; canvas.height = H * dpr;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  }
  window.addEventListener('resize', resize);
  resize();
  function frame() {
    const t = Date.now() * 0.001;
    ctx.clearRect(0, 0, W, H);
    const bg = ctx.createLinearGradient(0, 0, 0, H);
    bg.addColorStop(0, '#0b0d14'); bg.addColorStop(0.55, '#0f172a'); bg.addColorStop(1, '#0b0d14');
    ctx.fillStyle = bg; ctx.fillRect(0, 0, W, H);
    for (const s of stars) {
      const x = s.x * W, y = s.y * H;
      const flicker = 0.55 + 0.45 * Math.sin(t * s.speed * 10 + s.phase);
      ctx.globalAlpha = s.a * flicker;
      ctx.fillStyle = '#fff';
      ctx.beginPath(); ctx.arc(x, y, s.r, 0, Math.PI * 2); ctx.fill();
    }
    ctx.globalAlpha = 1;
    requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);
})();
