// Runs before paint, including on file:// and with storage disabled.
(() => {
  document.documentElement.classList.add('js');
  const supplied = new URLSearchParams(location.search).get('theme');
  let saved;
  try { saved = localStorage.getItem('cube-manual-theme'); } catch {}
  const mode = ['light', 'dark', 'system'].includes(supplied) ? supplied : saved;
  if (mode === 'light' || mode === 'dark') document.documentElement.dataset.theme = mode;
  window.manualTheme = mode || 'system';
})();
