from pathlib import Path
import json
base=Path('CubeControl-1.1/Cube')
translations={
'Aumentar tom':'Increase pitch','Diminuir tom':'Decrease pitch',
'Arraste o corpo para mover e as bordas para aparar. Alt ignora o snap.':'Drag the body to move and edges to trim. Alt bypasses snapping.',
'Escolha uma entrada e saída disponíveis para monitorar.':'Choose an available input and output to monitor.',
'Dispositivo de áudio':'Audio device','Cortar':'Cut','Importar':'Import','Exportar':'Export','Opções':'Options','Compassos':'Bars',
'Biblioteca de áudio':'Audio library','Arquivos locais':'Local files','Buscar arquivos':'Search files','Adicionar pasta':'Add folder',
'Todos':'All','Loops':'Loops','One Shots':'One Shots','Vocais':'Vocals','Sessões':'Sessions','Sessões e entrada':'Sessions and input',
'Parar pré-escuta':'Stop preview','Arraste áudio aqui':'Drop audio here','Monitorar entrada':'Monitor input',
'Não foi possível carregar a biblioteca local.':'Could not load the local library.',
'Não foi possível salvar a biblioteca.':'Could not save the library.',
'Não foi possível pré-ouvir o arquivo.':'Could not preview the file.'}
for language in ['pt-BR','en-US']:
 p=base/'windows/Cube.Desktop/Localization'/f'{language}.json'
 data=json.loads(p.read_text(encoding='utf-8-sig'))
 # Normalize existing stray translations so the catalog loader sees them.
 for key in list(data):
  if key not in ['Code','Name','Version','Strings']:data['Strings'].setdefault(key,data.pop(key))
 for key,value in translations.items():data['Strings'][key]=key if language=='pt-BR' else value
 p.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
p=base/'docs/DESIGN_SYSTEM.md';s=p.read_text(encoding='utf-8-sig');a=s.index('## Player, mixer e busca de backing tracks');b=s.index('### Feedback de arrastar e soltar',a)
s=s[:a]+'''## Player, mixer e busca de backing tracks

A seção segue a composição Mini DAW aprovada: barra contínua superior de 68 DIPs; arranjo e mixer em proporção 59/41; biblioteca lateral de 310 DIPs, sobreposta abaixo de 1200 DIPs de largura disponível. A navegação principal permanece. Não usar cartões, badges ou expansores para fragmentar o arranjo. Janelas menores usam rolagem horizontal na barra e nos canais; o mixer permanece visível, sem redução de controles por escala.

A barra apresenta dispositivo WASAPI, REC, Play/Pause, Stop, Loop, metrônomo, BPM, velocidade, tom, Cortar, Importar e Exportar. Reutilizar superfícies, tipografia, ícones e controles musicais compartilhados. Os cabeçalhos têm 335 DIPs; as pistas, 96 DIPs. Régua em compassos, clipes, loop e cursor compartilham o eixo temporal.

Cabeçalhos e mixer apresentam S/M/R/I, entrada, ganho e pan sincronizados. I monitora a entrada independentemente de R, sem iniciar transporte ou gravação. O estado efetivo é desligado em falha de dispositivo e ao restaurar sessões. Bloqueios de captura e roteamento continuam válidos. Não criar canais auxiliares de monitoração, Bus ou metrônomo. Master fica fixo à direita do mixer, sem monitoração; os canais reais rolam horizontalmente e usam ganho em dB e medidores compartilhados `BackingGainControl` e `BackingFaderMeter`.

A biblioteca contém Arquivos locais e YouTube. Arquivos locais têm busca, categorias manuais Todos/Loops/One Shots/Vocais, prévia e inserção na pista/cursor; não mostrar conteúdo fictício. A prévia pausa a sessão quando pronta e não a retoma automaticamente. Pastas e categorias são persistidas separadamente dos arquivos de áudio.

Novas sessões têm uma pista vazia; a última sessão disponível é restaurada sem tocar, armar ou monitorar. Sessões legadas mantêm seus dados e processamento, incluindo tomadas e fades, embora ferramentas avançadas não façam parte da interface. Cortar divide no cursor, e operações essenciais oferecem desfazer/refazer. A gravação continua preservando áudio anterior e arquivos recuperáveis. A atualização visual nominal de 50 ms não comprova latência física. Persistência de sessões permanece na versão 4.

''' + s[b:];p.write_text(s,encoding='utf-8')
pt='''# Backing Tracks

## Sessão e controles

A seção abre a última sessão recuperável; sem sessão anterior, cria uma pista vazia. Use o menu de reticências para criar pistas, selecionar sessões, escolher a entrada de áudio, abrir a biblioteca e desfazer/refazer. Cabeçalhos permitem editar nomes e acessar ações pelo menu de contexto. Os arquivos originais continuam necessários; a sessão não é um backup dos áudios.

A barra superior reúne saída WASAPI, Rec, Play/Pause, Stop, Loop, metrônomo, BPM, velocidade, tom, Cortar, Importar e Exportar. BPM define a grade de compassos; não estica o áudio. Velocidade altera a reprodução. Loop é ajustado na régua. Cortar divide no cursor os clipes selecionados ou o clipe da pista selecionada. Importar cria pistas com arquivos MP3/WAV. Exportar oferece sessão/pista e sessão inteira/intervalo A/B.

## Pistas e mixer

S ativa solo; M silencia; R arma a gravação; I monitora a entrada ao vivo. Os botões, entrada, ganho e pan da pista e do mixer compartilham o mesmo estado. I funciona com R desligado e não inicia gravação ou reprodução. Escolha o dispositivo de entrada no menu e seus canais na pista. Falhas de captura desligam a monitoração e informam o problema. Entrada, armação e monitoração ficam bloqueadas durante REC/finalização; ganho, pan, mute e solo continuam ajustáveis.

REC grava as pistas armadas pelo mesmo relógio. Stop finaliza e retorna ao início. Áudio anterior e tomadas recuperáveis são preservados. Os arquivos gravados recebem o sinal antes de ganho e pan; metrônomo e acompanhamento não entram automaticamente na gravação. A precisão física depende do dispositivo.

Arraste clipes para mover, bordas para aparar e arquivos para inserir na pista e posição desejadas. Delete exclui, Ctrl+C/Ctrl+V copia/cola, Ctrl+Z/Ctrl+Y desfaz/refaz. As edições não apagam os arquivos originais. Sessões antigas continuam legíveis, incluindo tomadas e fades já existentes.

O mixer inferior tem apenas canais reais e Master fixo à direita. Ganho é mostrado em dB e níveis em dBFS. Janelas menores mantêm controles acessíveis por rolagem. A monitoração começa desligada ao reabrir.

## Biblioteca

Em Arquivos locais, Adicionar pasta indexa MP3/WAV. A busca combina com Todos, Loops, One Shots e Vocais. Atribua categorias pelo menu de contexto do arquivo; nomes não definem categorias automaticamente. O botão de prévia reproduz o arquivo; clicar novamente ou usar Parar pré-escuta encerra. A sessão é pausada quando a prévia está pronta e permanece pausada. A seta insere no cursor da pista selecionada; arrastar escolhe pista e tempo.

A aba YouTube mantém busca, pré-escuta e download. Use conteúdo próprio ou autorizado. Internet é necessária apenas para esses recursos. Pastas e categorias locais são lembradas; os áudios não são copiados para a biblioteca. Arquivos indisponíveis não podem ser inseridos ou pré-ouvidos.
'''
en='''# Backing Tracks

## Session and transport

The section restores the last recoverable session, or creates one empty track. The options menu provides track creation, session selection, audio input, the library and undo/redo. Edit track names and use header context menus for track actions. Sessions reference original files and are not audio backups.

The toolbar contains WASAPI output, Rec, Play/Pause, Stop, Loop, metronome, BPM, speed, pitch, Cut, Import and Export. BPM sets the bar grid without stretching audio. Speed changes playback. Edit the loop on the ruler. Cut splits selected clips at the cursor, or the clip under the cursor on the selected track. Import creates tracks from MP3/WAV files. Export offers session/track and entire session/A/B range.

## Tracks and mixer

S enables solo, M mutes, R arms recording, and I monitors live input. Track headers and mixer channels share buttons, input routing, gain and pan. I works independently of R and does not start playback or recording. Choose the input device in the options menu and its channels on each track. Capture failures disable monitoring and display an error. Input, arming and monitoring are locked during recording/finalization; gain, pan, mute and solo remain adjustable.

Rec captures armed tracks using one clock. Stop finalizes and returns to the start. Previous audio and recoverable takes are preserved. Capture records before gain and pan; backing audio and metronome are not automatically recorded. Physical timing depends on the device.

Drag clips to move, edges to trim and files to insert at a track/time. Delete removes clips, Ctrl+C/Ctrl+V copies/pastes, and Ctrl+Z/Ctrl+Y undoes/redoes. Edits do not delete original audio. Older sessions retain their takes and existing fades.

The lower mixer shows actual channels and a fixed Master at the right. Gain is in dB and levels in dBFS. Smaller windows provide scrolling. Monitoring is off when reopening a session.

## Library

Under Local files, Add folder indexes MP3/WAV files. Search combines with All, Loops, One Shots and Vocals. Assign categories in the file context menu; names do not determine categories. Preview pauses the session only when ready and leaves it paused afterward. Click again or choose Stop preview to stop. The insertion button adds audio at the selected track/cursor; dragging chooses the destination and time.

The YouTube tab retains search, preview and download. Use your own or authorized content. Only these features need internet. Local folders and categories are remembered without copying audio into the library. Unavailable files cannot be inserted or previewed.
'''
for language,text in [('pt-BR',pt),('en-US',en)]:
 (base/'docs/usuario'/language/'backing-tracks.md').write_text(text,encoding='utf-8')
