// ponytail: self-contained astronomy math — no deps, no network
// Sunrise/sunset (NOAA algorithm), moon position approximation, event calendar
window.ACAstro = (() => {
  const RAD = Math.PI / 180;

  function dayOfYear(d) {
    return Math.floor((d - new Date(d.getFullYear(), 0, 0)) / 864e5);
  }

  // NOAA solar calc — good to ~1 minute
  function sunTimes(date, lat, lng) {
    const N = dayOfYear(date);
    const zenith = 90.833;
    function calc(isSunrise) {
      const lngHour = lng / 15;
      const t = N + ((isSunrise ? 6 : 18) - lngHour) / 24;
      const M = 0.9856 * t - 3.289;
      let L = M + 1.916 * Math.sin(M * RAD) + 0.020 * Math.sin(2 * M * RAD) + 282.634;
      L = ((L % 360) + 360) % 360;
      let RA = Math.atan(0.91764 * Math.tan(L * RAD)) / RAD;
      RA = ((RA % 360) + 360) % 360;
      RA += Math.floor(L / 90) * 90 - Math.floor(RA / 90) * 90;
      RA /= 15;
      const sinDec = 0.39782 * Math.sin(L * RAD);
      const cosDec = Math.cos(Math.asin(sinDec));
      const cosH = (Math.cos(zenith * RAD) - sinDec * Math.sin(lat * RAD)) / (cosDec * Math.cos(lat * RAD));
      if (cosH > 1 || cosH < -1) return null; // polar day/night
      let H = isSunrise ? 360 - Math.acos(cosH) / RAD : Math.acos(cosH) / RAD;
      H /= 15;
      const T = H + RA - 0.06571 * t - 6.622;
      let UT = T - lngHour;
      let dayOffset = 0;
      while (UT < 0) { UT += 24; dayOffset--; }
      while (UT >= 24) { UT -= 24; dayOffset++; }
      const out = new Date(date);
      out.setUTCHours(0, 0, 0, 0);
      out.setUTCDate(out.getUTCDate() + dayOffset);
      out.setUTCHours(Math.floor(UT), Math.round((UT % 1) * 60), 0, 0);
      return out;
    }
    const result = { sunrise: calc(true), sunset: calc(false) };
    // NOAA edge case: for western longitudes the sunset can land a day early — bump it
    if (result.sunrise && result.sunset && result.sunset < result.sunrise) {
      result.sunset = new Date(result.sunset.getTime() + 864e5);
    }
    return result;
  }

  // Static event calendar — major recurring showers + notable events
  // ponytail: hardcoded list beats an API; add year entries as needed
  const EVENTS = [
    { name: 'Quadrantids Meteor Shower', month: 1, day: 3, window: 2, emoji: '☄️', realm: 'Echo Fields' },
    { name: 'Lyrids Meteor Shower', month: 4, day: 22, window: 2, emoji: '☄️', realm: 'Echo Fields' },
    { name: 'Eta Aquariids', month: 5, day: 5, window: 2, emoji: '☄️', realm: 'Echo Fields' },
    { name: 'Summer Solstice', month: 6, day: 21, window: 1, emoji: '☀️', realm: 'Verdant Garden' },
    { name: 'Perseids Meteor Shower', month: 8, day: 12, window: 3, emoji: '☄️', realm: 'Echo Fields' },
    { name: 'Autumn Equinox', month: 9, day: 22, window: 1, emoji: '🍂', realm: 'Verdant Garden' },
    { name: 'Orionids Meteor Shower', month: 10, day: 21, window: 2, emoji: '☄️', realm: 'Echo Fields' },
    { name: 'Leonids Meteor Shower', month: 11, day: 17, window: 2, emoji: '☄️', realm: 'Echo Fields' },
    { name: 'Geminids Meteor Shower', month: 12, day: 14, window: 2, emoji: '☄️', realm: 'Echo Fields' },
    { name: 'Winter Solstice', month: 12, day: 21, window: 1, emoji: '❄️', realm: 'Dawn Citadel' },
  ];

  function activeEvents(now) {
    const m = now.getMonth() + 1, d = now.getDate();
    return EVENTS.filter(e => Math.abs((e.month * 31 + e.day) - (m * 31 + d)) <= e.window);
  }

  function nextEvent(now) {
    const md = now.getMonth() * 31 + now.getDate();
    const sorted = [...EVENTS].sort((a, b) => (a.month * 31 + a.day) - (b.month * 31 + b.day));
    for (const e of sorted) if (e.month * 31 + e.day > md) {
      const days = (e.month * 31 + e.day) - md;
      return { ...e, daysUntil: days };
    }
    const first = sorted[0];
    return { ...first, daysUntil: (12 * 31) - md + first.month * 31 + first.day };
  }

  return { sunTimes, activeEvents, nextEvent, dayOfYear };
})();
