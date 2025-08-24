// Minimal JS helper for local time updates.
// Works in Blazor Server or WASM.

let _intervalId = null;

export function getTimeZone() {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || "Local time";
  } catch {
    return "Local time";
  }
}

export function startLocalTicker(dotnetRef) {
  stopLocalTicker(); // just in case

  _intervalId = setInterval(() => {
    const now = new Date();

    // Format HH:mm:ss (24-hour, zero-padded) in browser-local time
    const hh = String(now.getHours()).padStart(2, "0");
    const mm = String(now.getMinutes()).padStart(2, "0");
    const ss = String(now.getSeconds()).padStart(2, "0");
    const local = `${hh}:${mm}:${ss}`;

    // Push to .NET
    dotnetRef.invokeMethodAsync("UpdateLocalFromJs", local);
  }, 1000);
}

export function stopLocalTicker() {
  if (_intervalId !== null) {
    clearInterval(_intervalId);
    _intervalId = null;
  }
}
