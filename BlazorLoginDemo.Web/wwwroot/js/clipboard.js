// ES module. No deprecated APIs.
export async function copyText(text) {
  const s = (text ?? "").toString();
  if (!window.isSecureContext || !navigator.clipboard?.writeText) return false;
  try { await navigator.clipboard.writeText(s); return true; } catch { return false; }
}

// optional, for sanity check during import
export function ping() { return "clipboard.js loaded"; }
