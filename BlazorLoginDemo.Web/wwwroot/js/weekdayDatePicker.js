window.weekdayPicker = {
  init: function (inputId, opts) {
    const el = document.getElementById(inputId);
    if (!el) return;

    const msgEl = opts.messageId ? document.getElementById(opts.messageId) : null;

    const fmt = (d) => {
      const y = d.getFullYear();
      const m = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      return `${y}-${m}-${day}`;
    };

    const parse = (v) => {
      // Force local midnight to avoid timezone shift when constructing Date
      // 'YYYY-MM-DDT00:00:00' is safe for HTML date values.
      return new Date(v + 'T00:00:00');
    };

    const isDisabledDay = (d) => {
      const dow = d.getDay(); // 0=Sun..6=Sat
      if (opts.disableSunday && dow === 0) return true;
      if (opts.disableSaturday && dow === 6) return true;
      return false;
    };

    const minDate = opts.minDate ? parse(opts.minDate) : null;

    const clampNextValid = (d) => {
      // ensure >= min
      if (minDate && d < minDate) d = new Date(minDate.getTime());

      // step forward until weekday ok
      while (isDisabledDay(d)) {
        d.setDate(d.getDate() + 1);
      }
      return d;
    };

    const showMsg = (text) => {
      if (!msgEl) return;
      msgEl.textContent = text || '';
      if (text) {
        msgEl.classList.add('text-warning');
      } else {
        msgEl.classList.remove('text-warning');
      }
    };

    const handle = () => {
      if (!el.value) { showMsg(''); return; }
      const picked = parse(el.value);
      const adjusted = clampNextValid(new Date(picked.getTime()));
      if (fmt(picked) !== fmt(adjusted)) {
        el.value = fmt(adjusted);
        // Trigger Blazor two-way binding refresh
        el.dispatchEvent(new Event('input', { bubbles: true }));
        showMsg('Weekend or too-early date adjusted to next available day.');
      } else {
        showMsg('');
      }
    };

    // Re-validate whenever user picks
    el.addEventListener('change', handle);
    el.addEventListener('input', handle);

    // Run once on init (in case server pre-filled an invalid date)
    handle();
  }
};
