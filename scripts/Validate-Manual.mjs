import fs from 'node:fs';
import path from 'node:path';
import vm from 'node:vm';
import assert from 'node:assert/strict';
import { fileURLToPath } from 'node:url';
const repo=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const root=path.join(repo,'CubeControl-1.1/Cube/docs/manual');
const walk=p=>fs.readdirSync(p,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(p,e.name)):[path.join(p,e.name)]);
const files=walk(root), htmls=files.filter(p=>p.endsWith('.html'));
const decode=s=>s.replace(/&amp;/g,'&').replace(/&quot;/g,'"').replace(/&#39;/g,"'").replace(/&lt;/g,'<').replace(/&gt;/g,'>');
let links=0;
function validateLink(file,href) {
  if (/^(?:https?:|mailto:)/.test(href)) return;
  assert(!/^[a-z]+:/i.test(href),`Unsupported scheme: ${href}`);
  const [url,anchor]=href.split('#'), pathname=url.split('?')[0];
  const target=pathname?path.resolve(path.dirname(file),decodeURIComponent(pathname)):file;
  assert(target.startsWith(root+path.sep),`Link escapes manual: ${file} -> ${href}`);
  assert(fs.existsSync(target),`Missing target: ${file} -> ${href}`);
  if(anchor && target.endsWith('.html')) {
    const source=fs.readFileSync(target,'utf8');
    assert(source.includes(`id="${decodeURIComponent(anchor)}"`),`Missing anchor: ${file} -> ${href}`);
  }
  links++;
}
for(const file of htmls) {
 const source=fs.readFileSync(file,'utf8');
 assert.equal((source.match(/<h1[ >]/g)||[]).length,1,`One h1: ${file}`);
 const ids=[...source.matchAll(/\bid="([^"]+)"/g)].map(m=>m[1]);
 assert.equal(new Set(ids).size,ids.length,`Unique IDs: ${file}`);
 for(const m of source.matchAll(/\b(?:href|src)="([^"]+)"/g)) validateLink(file,decode(m[1]));
 assert(!/<(?:script|link|img)[^>]+(?:src|href)="https?:/i.test(source),'No network resources');
}
for(const lang of ['pt-BR','en-US']) {
 const ctx={window:{}};vm.runInNewContext(fs.readFileSync(path.join(root,lang,'search-index.js'),'utf8'),ctx);
 const entries=ctx.window.manualSearch.entries;
 assert(entries.length>60,`Full-text sections indexed for ${lang}`);
 for(const entry of entries) validateLink(path.join(root,lang,'index.html'),entry.url);
 assert(entries.some(e=>e.text.includes('WASAPI')),'Advanced recording is searchable');
}
for(const file of files.filter(p=>p.endsWith('.js'))) new vm.Script(fs.readFileSync(file,'utf8'),{filename:file});
for(const m of fs.readFileSync(path.join(root,'assets/manual.css'),'utf8').matchAll(/url\(['"]?([^)'" ]+)['"]?\)/g)) validateLink(path.join(root,'assets/manual.css'),m[1]);
assert.equal(fs.readFileSync(path.join(root,'assets/InterVariable.woff2')).subarray(0,4).toString(),'wOF2','Real local WOFF2 font');
for(const f of files.filter(p=>p.endsWith('.pdf'))) assert.equal(fs.readFileSync(f).subarray(0,5).toString(),'%PDF-');
assert.equal(htmls.length,33,'Bilingual chapters, home pages and language entry');
console.log(`PASS: ${htmls.length} pages, ${links} local links/anchors, both search indexes, JavaScript syntax, font and PDF resources.`);
