// Run with Node.js: node scripts/Build-Manual.mjs. No downloaded dependencies.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createHash } from 'node:crypto';
const repo = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const root = path.join(repo, 'CubeControl-1.1/Cube/docs/manual');
const guides = path.join(repo, 'CubeControl-1.1/Cube/docs/usuario');
const catalog = JSON.parse(fs.readFileSync(path.join(guides, 'catalogo.v1.json'), 'utf8'));
const version = fs.readFileSync(path.join(repo, 'CubeControl-1.1/Cube/windows/Cube.Desktop/Cube.Desktop.csproj'), 'utf8').match(/<Version>(.*?)<\/Version>/)[1];
const esc = s => String(s).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const slug = s => s.toLowerCase().replace(/[^\p{L}\p{N}_\-\s]/gu, '').replace(/ /g, '-');
const inline = s => s.split(/(\[[^\]]+\]\([^)]+\)|\*\*[^*]+\*\*|`[^`]+`|\*[^*]+\*)/g).map(t => {
  if (t.startsWith('[')) { const m = t.match(/^\[([^\]]+)\]\(([^)]+)\)$/); if(m) return `<a href="${esc(m[2].replace(/\.md(?=#|$)/, '.html'))}">${esc(m[1])}</a>`; }
  if(t.startsWith('**')) return `<strong>${esc(t.slice(2,-2))}</strong>`;
  if(t.startsWith('`')) return `<code>${esc(t.slice(1,-1))}</code>`;
  if(t.startsWith('*')) return `<em>${esc(t.slice(1,-1))}</em>`;
  return esc(t);
}).join('');
function markdown(text) {
  const lines = text.replace(/\r/g,'').split('\n'), out = [], headings = [], seen = new Map();
  for(let i=0;i<lines.length;i++) {
    const line=lines[i]; if(!line.trim() || /^# /.test(line)) continue;
    const h=line.match(/^(#{2,6}) (.+)$/);
    if(h) { const base=slug(h[2]), n=seen.get(base)||0, id=base+(n?`-${n}`:''); seen.set(base,n+1); headings.push({title:h[2],id,level:h[1].length}); out.push(`<h${h[1].length} id="${esc(id)}">${inline(h[2])}</h${h[1].length}>`); continue; }
    if(line.startsWith('```')) { const code=[]; while(++i<lines.length&&!lines[i].startsWith('```')) code.push(lines[i]); out.push(`<pre><code>${esc(code.join('\n'))}</code></pre>`); continue; }
    if(line.startsWith('|') && /^[| :\-]+$/.test(lines[i+1]||'')) {
      const cells=s=>s.trim().replace(/^\||\|$/g,'').split('|').map(s=>s.trim()); const header=cells(line); i++;
      let rows=''; while((lines[i+1]||'').startsWith('|')) rows+=`<tr>${cells(lines[++i]).map(c=>`<td>${inline(c)}</td>`).join('')}</tr>`;
      out.push(`<div class="table-scroll" tabindex="0"><table><thead><tr>${header.map(c=>`<th scope="col">${inline(c)}</th>`).join('')}</tr></thead><tbody>${rows}</tbody></table></div>`); continue;
    }
    if(/^(?:[-*]|\d+\.) /.test(line)) {
      const ordered=/^\d/.test(line), tag=ordered?'ol':'ul', re=ordered?/^\d+\. /:/^[-*] /; const items=[];
      do { items.push(`<li>${inline(lines[i].replace(re,''))}</li>`); if(!re.test(lines[i+1]||'')) break; i++; } while(i<lines.length);
      out.push(`<${tag}>${items.join('')}</${tag}>`); continue;
    }
    const p=[line]; while(lines[i+1]?.trim()&&!/^(?:#|\||```|[-*] |\d+\. )/.test(lines[i+1])) p.push(lines[++i]);
    out.push(`<p>${inline(p.join(' '))}</p>`);
  }
  return {html:out.join('\n'),headings};
}
const order=['primeiros-passos','pedaleira','presets','irs','criador-amps','criadores-efeitos','criador-chorus','criador-phaser','criador-delay','criador-reverb','backing-tracks','simulacao','recuperacao','problemas','privacidade'];
const words={
 'pt-BR':{manual:'Manual do usuário',home:'Visão geral',skip:'Ir para o conteúdo',menu:'Capítulos',search:'Buscar no manual',hint:'Busque por uma ação, um controle ou uma dúvida.',toc:'Nesta página',previous:'Anterior',next:'Próximo',print:'Imprimir página',theme:'Tema',system:'Sistema',light:'Claro',dark:'Escuro',title:'Entenda o controle.\nEncontre seu som.',intro:'Do primeiro cabo USB ao ajuste fino de um efeito. Um guia para editar, ouvir e guardar seus timbres no cubeControl.',start:'Conectar e começar',read:'Entender os presets',chapters:'Explore o manual',offline:'Disponível offline',model:'Cube Baby · guitarra',figure:'Representação ilustrativa do pedal. As posições e os detalhes visuais não substituem os controles do seu aparelho.',route:'Três destinos para um ajuste',routeIntro:'Ouvir uma mudança, guardar um arquivo e gravar no pedal são ações diferentes.',computer:'Computador',file:'Arquivo local',preview:'Prévia em RAM',pedal:'Memória do pedal',export:'Exportar',listen:'Ouvir',save:'Gravar',routeNote:'Diagrama conceitual. Cada recurso oferece ações próprias; os criadores de chorus, phaser, delay e reverb não gravam seus ajustes na memória permanente.',before:'Antes de começar',result:'Ao concluir',steps:'Roteiro rápido',reference:'Consulta detalhada',glossary:'Termos que aparecem no caminho',glossaryIntro:'Uma referência curta para acompanhar as instruções.',pdf:'Manuais do pedal em PDF',pdfIntro:'Documentos de referência do equipamento. Os procedimentos atuais do cubeControl estão nos capítulos acima.',nojs:'O conteúdo e os links funcionam sem JavaScript. Ative-o para usar busca e escolher o tema.',groups:['Começar','Construir o timbre','Criadores','Praticar','Consultar'],footer:'Manual local · Sem conexão com o pedal pelo navegador',review:'Conteúdo da versão',select:'Idioma',all:'Todos os capítulos',empty:'Nenhum resultado. Tente outro termo.',results:'resultados',clear:'Limpar busca'},
 'en-US':{manual:'User manual',home:'Overview',skip:'Skip to content',menu:'Chapters',search:'Search the manual',hint:'Search for an action, a control or a question.',toc:'On this page',previous:'Previous',next:'Next',print:'Print page',theme:'Theme',system:'System',light:'Light',dark:'Dark',title:'Understand the controls.\nFind your sound.',intro:'From the first USB cable to fine adjustments inside an effect. A guide to editing, hearing and saving your tones in cubeControl.',start:'Connect and get started',read:'Understand presets',chapters:'Explore the manual',offline:'Available offline',model:'Cube Baby · guitar',figure:'Illustrative pedal artwork. Positions and visual details do not replace the controls on your device.',route:'Three destinations for an edit',routeIntro:'Hearing a change, saving a file and writing to the pedal are different actions.',computer:'Computer',file:'Local file',preview:'RAM preview',pedal:'Pedal memory',export:'Export',listen:'Listen',save:'Write',routeNote:'Conceptual diagram. Available actions depend on the feature; chorus, phaser, delay and reverb creators do not write their settings to permanent memory.',before:'Before you start',result:'When finished',steps:'Quick workflow',reference:'Detailed reference',glossary:'Terms along the way',glossaryIntro:'A short reference to help you follow the instructions.',pdf:'Pedal manuals in PDF',pdfIntro:'Reference documents for the hardware. Current cubeControl procedures are covered in the chapters above.',nojs:'Content and links work without JavaScript. Enable it to search and choose a theme.',groups:['Start','Build your tone','Creators','Practice','Reference'],footer:'Local manual · The browser does not control the pedal',review:'Documentation for version',select:'Language',all:'All chapters',empty:'No results. Try another term.',results:'results',clear:'Clear search'}
};
// Editorial additions complement the shared guides, rather than duplicating them.
const workflows={
 'primeiros-passos':[
  ['Pacote completo extraído e cabo USB de dados.','Conecte o Cube Baby e aguarde a leitura do banco.','Escolha o mesmo A/B/C no aplicativo e no footswitch.','Ajuste um controle da Pedaleira e ouça o resultado.','Você conhece o caminho entre conexão, edição e escuta. Salvar o banco é uma etapa separada.'],
  ['Extract the complete package and have a USB data cable ready.','Connect the Cube Baby and wait for the bank to load.','Choose the same A/B/C in the app and on the footswitch.','Adjust a pedalboard control and listen.','You understand connection, editing and listening. Writing the bank is a separate step.']],
 'presets':[
  ['Pedal conectado e banco carregado. Exporte uma cópia antes de alterações importantes.','Selecione o destino A/B/C e o mesmo preset físico.','Faça os ajustes e confira o som.','Use Salvar A/B/C quando quiser gravar o banco completo.','A confirmação informa a gravação e leitura do banco; não comprova retenção após desligar.'],
  ['Connect the pedal and load its bank. Export a copy before important changes.','Select the target A/B/C and the matching physical preset.','Adjust the controls and check the sound.','Use Save A/B/C when you want to write the complete bank.','The confirmation reports writing and reading the bank; it does not prove retention after power-off.']],
 'irs':[
  ['WAV de resposta de impulso compatível e backup do slot que pretende substituir.','Abra Gerenciar IRs e selecione um arquivo na aba individual.','Com o pedal conectado, acompanhe a prévia e ouça o resultado.','Encerre a prévia para restaurar ou use a ação de gravação, conferindo o destino.','Prévia e substituição de slot têm resultados diferentes; confira a mensagem antes de continuar.'],
  ['Have a compatible impulse-response WAV and a backup of the slot you intend to replace.','Open Manage IRs and select a file in the individual tab.','With the pedal connected, check the preview state and listen.','End the preview to restore, or use the write action and check its destination.','Previewing and replacing a slot have different outcomes; check the message before continuing.']],
 'backing-tracks':[
  ['Arquivo MP3/WAV e saída de áudio do Windows. Gravar também exige uma entrada de áudio.','Adicione uma faixa e selecione a saída.','Reproduza e use A/B para praticar um trecho.','Para gravar, configure a entrada, arme a pista com R e use REC.','Você pode ouvir a faixa e guardar tomadas. A playlist referencia os áudios; preserve os arquivos.'],
  ['Have an MP3/WAV and a Windows audio output. Recording also requires an audio input.','Add a track and choose the output.','Play it and use A/B to practise a section.','To record, configure the input, arm a track with R and use REC.','You can listen and keep takes. The playlist references audio files; keep those files available.']],
 'criador-amps':[
  ['Amp base ou BIN válido de 4096 bytes. Preserve o conteúdo anterior antes de gravar.','Escolha a origem e observe se a prévia está ativa.','Ajuste gradualmente e compare com a referência.','Exporte o BIN para guardar no computador; use Salvar no pedal apenas para substituir o slot confirmado.','O BIN conserva sua edição local. Após gravar, troque de amp no pedal e volte para recarregar o slot.'],
  ['Have a base amp or a valid 4096-byte BIN. Preserve the old content before writing.','Choose the source and check whether preview is active.','Adjust gradually and compare with the reference.','Export BIN to keep a local copy; use Save to pedal only to replace the confirmed slot.','The BIN preserves your local edit. After writing, switch amps on the pedal and back to reload the slot.']]
};
const glossary={
 'pt-BR':[['Preset','Conjunto de ajustes de um timbre. A/B/C identifica três destinos de edição e gravação.'],['IR — resposta de impulso','Arquivo que descreve uma resposta usada na seção de gabinete. Um WAV de IR não é uma música.'],['RAM','Memória usada durante o funcionamento. Uma prévia em RAM não substitui automaticamente um slot gravado.'],['Flash','Memória persistente do aparelho. Gravar um slot substitui seu conteúdo.'],['USB-MIDI','Canal de mensagens entre editor e pedal. A reprodução de áudio usa dispositivos de áudio do Windows.'],['dBFS','Escala digital de nível de áudio. 0 dBFS é o limite de referência; ganho em dB é outro controle.']],
 'en-US':[['Preset','A set of tone settings. A/B/C identifies three editing and saving destinations.'],['IR — impulse response','A file describing a response used by the cabinet section. An IR WAV is not a song.'],['RAM','Memory used while the device operates. A RAM preview does not automatically replace a saved slot.'],['Flash','Persistent device memory. Writing a slot replaces its content.'],['USB-MIDI','The message channel between editor and pedal. Audio playback uses Windows audio devices.'],['dBFS','A digital audio level scale. 0 dBFS is the reference ceiling; gain in dB is a different control.']]
};
const write=(name,content)=>{const dest=path.join(root,name);fs.mkdirSync(path.dirname(dest),{recursive:true});fs.writeFileSync(dest,content);};
function diagram(w) {
 return `<figure class="flow"><svg viewBox="0 0 700 230" role="img" aria-labelledby="flow-title flow-desc"><title id="flow-title">${w.route}</title><desc id="flow-desc">${w.routeIntro} ${w.routeNote}</desc><g fill="none" stroke="currentColor" stroke-width="1.5"><path d="M190 115 H295 V45 H410 M295 115 H410 M295 115 V185 H410"/><path d="m402 40 8 5-8 5m0 60 8 5-8 5m0 60 8 5-8 5"/><rect x="15" y="85" width="175" height="60" rx="4"/><rect x="410" y="15" width="275" height="60" rx="4"/><rect x="410" y="85" width="275" height="60" rx="4"/><rect x="410" y="155" width="275" height="60" rx="4"/></g><g fill="currentColor" font-size="18" text-anchor="middle"><text x="103" y="122">${w.computer}</text><text x="548" y="52">${w.file}</text><text x="548" y="122">${w.preview}</text><text x="548" y="192">${w.pedal}</text><g font-size="13"><text x="352" y="33">${w.export}</text><text x="352" y="103">${w.listen}</text><text x="352" y="173">${w.save}</text></g></g></svg><figcaption>${w.routeNote}</figcaption></figure>`;
}
for(const [lang,w] of Object.entries(words)) {
 const en=lang==='en-US', other=en?'pt-BR':'en-US', docs=order.map(id=>{const entry=catalog.documents.find(e=>e.id===id);return {...entry,...entry.versions.find(v=>v.language===lang)};});
 const groups=[[docs[0]],[...docs.slice(1,4)],[...docs.slice(4,10)],[docs[10]],[...docs.slice(11)]];
 const searchIndex=[];
 function shell(id,title,summary,body,headings=[]) {
  const current=docs.findIndex(d=>d.id===id);
  const nav=groups.map((items,i)=>`<div class="nav-group"><p>${w.groups[i]}</p>${items.map(d=>`<a href="${d.id}.html"${id===d.id?' aria-current="page"':''}>${esc(d.title)}</a>`).join('')}</div>`).join('');
  return `<!doctype html>\n<html lang="${lang}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><meta name="color-scheme" content="light dark"><meta name="description" content="${esc(summary)}"><title>${esc(title)} · cubeControl</title><link rel="stylesheet" href="../assets/manual.css"><script src="../assets/theme.js"></script><script defer src="search-index.js"></script><script defer src="../assets/manual.js"></script></head>
<body><a class="skip" href="#main">${w.skip}</a><header class="topbar"><a class="wordmark" href="index.html">cubeControl <span>${w.manual}</span></a><div class="toolbar"><a class="language" href="../${other}/${id}.html" lang="${other}" aria-label="${w.select}: ${en?'Português':'English'}">${en?'PT':'EN'}</a><label class="theme-control">${w.theme}<select id="theme"><option value="system">${w.system}</option><option value="light">${w.light}</option><option value="dark">${w.dark}</option></select></label><button id="menu" class="menu-button" aria-expanded="false" aria-controls="chapters">${w.menu}</button></div></header>
<div class="layout"><nav id="chapters" class="sidebar" aria-label="${w.menu}"><div class="nav-heading">cubeControl ${version}</div><a href="index.html"${id==='index'?' aria-current="page"':''}>${w.home}</a>${nav}<div class="offline">${w.offline}<br><span>${w.model}</span></div></nav><main id="main" tabindex="-1">
<div class="search-block"><label for="search">${w.search}</label><div class="search-input"><input id="search" type="search" placeholder="${w.hint}" autocomplete="off" aria-controls="search-results"><button id="clear-search" hidden>${w.clear}</button></div><div id="search-results" hidden><p id="search-status" role="status" aria-live="polite"></p><ul id="result-list"></ul></div></div>
<noscript><p class="notice">${w.nojs}</p></noscript>${id!=='index'?`<div class="eyebrow">${String(current+1).padStart(2,'0')} / ${String(docs.length).padStart(2,'0')} · ${w.manual}</div><h1>${esc(title)}</h1><p class="lead">${esc(summary)}</p><div class="article-tools"><button class="print">${w.print}</button><span>${w.review} ${version}</span></div>`:''}
${headings.length?`<details class="page-index" open><summary>${w.toc}</summary><ul>${headings.filter(h=>h.level===2).map(h=>`<li><a href="#${esc(h.id)}">${esc(h.title)}</a></li>`).join('')}</ul></details>`:''}<article>${body}</article>
${current>=0?`<nav class="pagination" aria-label="${w.menu}"><a href="${current?docs[current-1].id:'index'}.html"><small>← ${w.previous}</small>${esc(current?docs[current-1].title:w.home)}</a>${current<docs.length-1?`<a href="${docs[current+1].id}.html"><small>${w.next} →</small>${esc(docs[current+1].title)}</a>`:''}</nav>`:''}<footer>${w.footer}<br>${w.review} ${version} · <a href="privacidade.html">${esc(docs.at(-1).title)}</a></footer></main></div></body></html>`;
 }
 for(const d of docs) {
  const raw=fs.readFileSync(path.join(guides,lang,d.id+'.md'),'utf8'); const parsed=markdown(raw);
  const flow=workflows[d.id]?.[en?1:0]; let opening='';
  if(flow) opening=`<section class="workflow" aria-labelledby="quick-workflow"><h2 id="quick-workflow">${w.steps}</h2><p><strong>${w.before}.</strong> ${esc(flow[0])}</p><ol>${flow.slice(1,4).map(s=>`<li>${esc(s)}</li>`).join('')}</ol><p class="outcome"><strong>${w.result}.</strong> ${esc(flow[4])}</p></section>`;
  const headings=flow?[{title:w.steps,id:'quick-workflow',level:2},...parsed.headings]:parsed.headings;
  write(`${lang}/${d.id}.html`,shell(d.id,d.title,d.summary,opening+parsed.html,headings));
  if(flow) searchIndex.push({title:d.title,section:w.steps,url:`${d.id}.html#quick-workflow`,text:flow.join(' '),keywords:d.keywords.join(' ')});
  const sections=raw.split(/(?=^## )/m); for(const [i,s] of sections.entries()) {const heading=s.match(/^## (.+)/m)?.[1]; searchIndex.push({title:d.title,section:heading||d.title,url:`${d.id}.html${heading?'#'+slug(heading):''}`,text:s.replace(/[#*`|]/g,'').replace(/\[([^\]]+)\]\([^)]+\)/g,'$1').trim(),keywords:d.keywords.join(' ')});}
 }
 const pdfs=catalog.documents.filter(d=>d.category==='manuais').flatMap(d=>d.versions);
 const home=`<section class="hero"><div class="eyebrow">${w.manual} / ${version}</div><h1>${w.title.split('\n').join('<br>')}</h1><p class="lead">${w.intro}</p><div class="hero-actions"><a class="primary" href="primeiros-passos.html">${w.start} <span aria-hidden="true">↗</span></a><a href="presets.html">${w.read} →</a></div><figure class="equipment"><img src="../assets/cube-baby.png" width="1456" height="1072" alt="${en?'Illustrative three-footswitch pedal artwork':'Arte ilustrativa de pedal com três footswitches'}"><figcaption>${w.figure}</figcaption></figure></section>
<section><h2 id="chapters-list">${w.chapters}</h2><div class="chapter-list">${docs.map((d,i)=>`<a href="${d.id}.html"><span class="chapter-number">${String(i+1).padStart(2,'0')}</span><span><strong>${esc(d.title)}</strong><span>${esc(d.summary)}</span></span><span aria-hidden="true">↗</span></a>`).join('')}</div></section>
<section><h2 id="edit-paths">${w.route}</h2><p>${w.routeIntro}</p>${diagram(w)}</section>
<section><h2 id="glossary">${w.glossary}</h2><p>${w.glossaryIntro}</p><dl class="glossary">${glossary[lang].map(([a,b])=>`<div><dt>${a}</dt><dd>${b}</dd></div>`).join('')}</dl></section>
<section><h2 id="pedal-pdfs">${w.pdf}</h2><p>${w.pdfIntro}</p><ul class="pdf-list">${pdfs.map(p=>`<li><a href="../references/${p.path}">${esc(p.title)}</a> <small>${p.language} · PDF</small></li>`).join('')}</ul></section>`;
 write(`${lang}/index.html`,shell('index',w.manual,w.intro,home));
 searchIndex.push({title:w.glossary,section:w.glossary,url:'index.html#glossary',text:glossary[lang].flat().join(' '),keywords:''});
 searchIndex.push({title:w.route,section:w.route,url:'index.html#edit-paths',text:w.routeIntro+' '+w.routeNote,keywords:''});
 write(`${lang}/search-index.js`,'window.manualSearch = '+JSON.stringify({entries:searchIndex,empty:w.empty,results:w.results})+';\n');
}
// Keep direct-open distribution self-contained, including the hardware references.
for(const d of catalog.documents.filter(d=>d.category==='manuais')) for(const v of d.versions) {
 const dest=path.join(root,'references',v.path);fs.mkdirSync(path.dirname(dest),{recursive:true});fs.copyFileSync(path.join(repo,'CubeControl-1.1/Cube/windows/Cube.Desktop/Assets/Documents',v.path),dest);
}
fs.mkdirSync(path.join(root,'assets'),{recursive:true});
fs.copyFileSync(path.join(repo,'CubeControl-1.1/Cube/windows/Cube.Desktop/Assets/Images/Equipment/Devices/SINCO-CubeBaby.png'),path.join(root,'assets/cube-baby.png'));
write('index.html','<!doctype html><html lang="pt-BR"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>cubeControl · Manual</title><link rel="stylesheet" href="assets/manual.css"><script src="assets/theme.js"></script></head><body><main class="language-page"><p class="eyebrow">cubeControl · 1.1.0</p><h1>Manual do usuário<br><span lang="en">User manual</span></h1><p>Escolha o idioma / Choose your language</p><div class="hero-actions"><a class="primary" href="pt-BR/index.html" lang="pt-BR">Português</a><a href="en-US/index.html" lang="en-US">English →</a></div></main></body></html>');
const inputs=['scripts/Build-Manual.mjs','CubeControl-1.1/Cube/docs/usuario/catalogo.v1.json',...order.flatMap(id=>['pt-BR','en-US'].map(l=>`CubeControl-1.1/Cube/docs/usuario/${l}/${id}.md`)),
 'CubeControl-1.1/Cube/windows/Cube.Desktop/Assets/Images/Equipment/Devices/SINCO-CubeBaby.png',
 ...catalog.documents.filter(d=>d.category==='manuais').flatMap(d=>d.versions.map(v=>'CubeControl-1.1/Cube/windows/Cube.Desktop/Assets/Documents/'+v.path))];
write('sources.json',JSON.stringify({version,inputs:Object.fromEntries(inputs.map(p=>[p,createHash('sha256').update(fs.readFileSync(path.join(repo,p))).digest('hex')]))},null,2)+'\n');
console.log(`Generated ${order.length*2+3} pages for cubeControl ${version}.`);
