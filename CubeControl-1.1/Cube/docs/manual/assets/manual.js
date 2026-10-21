(() => {
  const $ = id => document.getElementById(id);
  const normalize = text => text.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase();
  const theme = $('theme');
  theme.value = window.manualTheme;
  function propagateTheme() {
    for (const a of document.querySelectorAll('a[href]')) {
      const url = new URL(a.getAttribute('href'), location.href);
      if (url.pathname.endsWith('.html') && !a.getAttribute('href').startsWith('#')) {
        url.searchParams.set('theme', theme.value);
        a.href = url.href;
      }
    }
  }
  theme.addEventListener('change', () => {
    if (theme.value === 'system') delete document.documentElement.dataset.theme;
    else document.documentElement.dataset.theme = theme.value;
    try { localStorage.setItem('cube-manual-theme', theme.value); } catch {}
    // Replace only this entry so back/forward keep their native behaviour.
    const url = new URL(location.href); url.searchParams.set('theme', theme.value);
    try { history.replaceState(null, '', url); } catch {}
    propagateTheme();
  });
  propagateTheme();
  const menu = $('menu'), sidebar = $('chapters');
  menu.addEventListener('click', () => {
    const open = menu.getAttribute('aria-expanded') !== 'true';
    menu.setAttribute('aria-expanded', String(open)); sidebar.classList.toggle('open', open);
  });
  document.addEventListener('keydown', e => {
    if(e.key === 'Escape') {
      if (sidebar.classList.contains('open')) { sidebar.classList.remove('open'); menu.setAttribute('aria-expanded','false'); menu.focus(); }
      else if ($('search').value) { $('search').value=''; search(); $('search').focus(); }
    }
  });
  document.querySelector('.print')?.addEventListener('click', () => window.print());
  const index = window.manualSearch;
  if (!index) return;
  const entries = index.entries.map(e=>({...e, normalized:normalize(e.title+' '+e.section+' '+e.text+' '+e.keywords), body:normalize(e.text)}));
  function search() {
    const query=$('search').value.trim(), terms=normalize(query).split(/\s+/).filter(Boolean), box=$('search-results'), list=$('result-list');
    list.replaceChildren(); box.hidden=!terms.length; $('clear-search').hidden=!terms.length;
    if(!terms.length) return;
    const matches=entries.filter(e=>terms.every(t=>e.normalized.includes(t))).sort((a,b)=>{
      const score=e=>terms.reduce((n,t)=>n+(normalize(e.title).includes(t)?3:0)+(normalize(e.section).includes(t)?2:0),0);
      return score(b)-score(a);
    });
    $('search-status').textContent=matches.length?`${matches.length} ${index.results}`:index.empty;
    for(const e of matches) {
      const li=document.createElement('li'), a=document.createElement('a'), title=document.createElement('strong'), snippet=document.createElement('span');
      a.href=e.url; title.textContent=e.title+(e.section!==e.title?' · '+e.section:'');
      const pos=Math.max(0,...terms.map(t=>e.body.indexOf(t)).filter(p=>p>=0)); const start=Math.max(0,pos-65);
      snippet.textContent=(start?'…':'')+e.text.slice(start,start+230)+(start+230<e.text.length?'…':'');
      a.append(title,snippet);li.append(a);list.append(li);
    }
    propagateTheme();
  }
  $('search').addEventListener('input',search);
  $('clear-search').addEventListener('click',()=>{$('search').value='';search();$('search').focus();});
  // Browsers may restore the search input when navigating back from a result.
  window.addEventListener('pageshow',search);
})();
