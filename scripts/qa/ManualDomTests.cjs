// In-memory DOM unit tests; does not open a browser, navigate or load resources.
// npm install --prefix <temporary-folder> jsdom
// node scripts/qa/ManualDomTests.cjs <temporary-folder>/node_modules/jsdom
const {JSDOM}=require(process.argv[2]||'jsdom');
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const root=path.resolve(__dirname,'../../CubeControl-1.1/Cube/docs/manual');
let count=0;
const check=(condition,label)=>{assert(condition,label);count++;};
for(const lang of ['pt-BR','en-US']) {
 const dom=new JSDOM(fs.readFileSync(path.join(root,lang,'presets.html'),'utf8'),{url:`file:///C:/Manual%20test/${lang}/presets.html?theme=dark`,runScripts:'outside-only'});
 const w=dom.window,d=w.document;
 // file origins intentionally have no available localStorage in jsdom.
 for(const script of ['assets/theme.js',`${lang}/search-index.js`,'assets/manual.js']) w.eval(fs.readFileSync(path.join(root,script),'utf8'));
 check(d.documentElement.dataset.theme==='dark','App theme takes precedence without storage');
 const select=d.getElementById('theme');select.value='light';select.dispatchEvent(new w.Event('change'));
 check(d.documentElement.dataset.theme==='light','Theme changes with storage unavailable');
 check(new URL(d.querySelector('.language').href).searchParams.get('theme')==='light','Locale switch preserves theme');
 check(d.querySelector('.language').href.includes('/presets.html'),'Locale switch preserves chapter');
 const input=d.getElementById('search');
 const search=value=>{input.value=value;input.dispatchEvent(new w.Event('input'));return [...d.querySelectorAll('#result-list a')];};
 check(search('WASAPI').some(a=>a.href.includes('backing-tracks.html')),'Search reaches full article text');
 if(lang==='pt-BR') {
   const plain=search('conexao').map(a=>a.href);const accented=search('CONEXÃO').map(a=>a.href);
   check(plain.length>0 && JSON.stringify(plain)===JSON.stringify(accented),'Search ignores accents and case');
 }
 check(search('WASAPI nonexistentterm').length===0,'All query terms must match');
 check(!d.getElementById('search-status').textContent.match(/^\d/),'Empty result is explained');
 check(search('<img src=x onerror=alert(1)>').length===0&&!d.querySelector('#search-results img'),'Search query remains inert text');
 search('WASAPI');d.getElementById('clear-search').click();
 check(d.getElementById('search-results').hidden&&input.value==='','Clear resets results');
 search('RAM');d.dispatchEvent(new w.KeyboardEvent('keydown',{key:'Escape'}));
 check(input.value===''&&d.activeElement===input,'Escape clears search and restores focus');
 const menu=d.getElementById('menu');menu.click();
 check(menu.getAttribute('aria-expanded')==='true'&&d.getElementById('chapters').classList.contains('open'),'Mobile menu expands');
 d.dispatchEvent(new w.KeyboardEvent('keydown',{key:'Escape'}));
 check(menu.getAttribute('aria-expanded')==='false'&&d.activeElement===menu,'Escape closes menu and restores focus');
 select.value='system';select.dispatchEvent(new w.Event('change'));
 check(!d.documentElement.hasAttribute('data-theme'),'System theme removes explicit override');
 dom.window.close();
}
console.log(`PASS: ${count} manual DOM checks, both languages, storage disabled, no resource loading.`);
