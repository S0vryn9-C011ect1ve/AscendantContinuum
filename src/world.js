// ponytail: geolocation + digital sunset + touch-grass multiplier
// Graceful when GPS denied or unavailable — game works fully offline without it
window.ACWorld = (() => {
  let coords = null;         // {lat, lng}
  let sunsetAt = null;       // Date
  let closed = false;        // digital sunset engaged
  let listeners = [];

  function on(fn) { listeners.push(fn); }
  function emit(evt) { listeners.forEach(f => f(evt)); }

  function isParkish() {
    // ponytail: no reverse-geocoding offline — heuristic: user confirms outdoors via toggle
    return localStorage.getItem('ac_outdoors') === 'true';
  }
  function setOutdoors(v) {
    localStorage.setItem('ac_outdoors', v ? 'true' : 'false');
    emit({ type: 'outdoors', value: v });
  }
  function multiplier() { return isParkish() ? 2 : 1; }

  function locate() {
    if (!navigator.geolocation) return Promise.resolve(null);
    return new Promise(res => {
      navigator.geolocation.getCurrentPosition(
        p => { coords = { lat: p.coords.latitude, lng: p.coords.longitude }; emit({ type: 'locate', coords }); res(coords); },
        () => { emit({ type: 'locate-denied' }); res(null); },
        { timeout: 8000, maximumAge: 3600e3 }
      );
    });
  }

  function startSunsetWatch() {
    if (!coords || !window.ACAstro) return;
    const { sunset } = window.ACAstro.sunTimes(new Date(), coords.lat, coords.lng);
    sunsetAt = sunset;
    emit({ type: 'sunset', at: sunset });
    const tick = () => {
      if (sunsetAt && Date.now() >= sunsetAt.getTime() && !closed) {
        closed = true;
        emit({ type: 'digital-sunset' });
      }
    };
    setInterval(tick, 30e3); tick();
  }

  function dismissSunset() { closed = false; emit({ type: 'sunset-dismissed' }); }

  return { on, locate, startSunsetWatch, dismissSunset, isParkish, setOutdoors, multiplier,
    get coords() { return coords; }, get sunsetAt() { return sunsetAt; }, get closed() { return closed; } };
})();
