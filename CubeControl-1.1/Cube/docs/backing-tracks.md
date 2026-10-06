# Backing Tracks

[Documentação](README.md) · Revisão: 05/10/2026

A área Backing Tracks reproduz arquivos locais pelo áudio do Windows. Pode usar uma saída Bluetooth do Cube Baby quando ela estiver conectada e disponível como dispositivo de reprodução no Windows. Isso não fornece controle Bluetooth dos efeitos.

## Reproduzir e praticar

1. Adicione ou arraste arquivos **MP3/WAV**, sem o limite de três músicas. Entradas duplicadas são ignoradas; remover uma faixa da lista não apaga o arquivo. Arquivos recusados são informados. A waveform é calculada sob demanda; BPM e tom entram em uma fila de análise local que prioriza a faixa selecionada.
2. Escolha explicitamente uma saída de áudio. O menu contextual do card Áudio oferece atualização dos dispositivos e Bluetooth, que abre as configurações do Windows; o aplicativo não emparelha o pedal por conta própria.
3. Selecione uma faixa e use reproduzir/pausar, parar, anterior/próxima ou saltos de cinco segundos. Selecionar não inicia áudio; duplo clique ou Enter na playlist reproduz a faixa.
4. Reordene a sequência com as ações disponíveis ou arrastando seus itens.
5. Defina A e B pelo botão direito na waveform, na posição clicada; pelo teclado, o menu usa a posição atual. O intervalo mínimo é 0,1 s. Ative **Loop A/B** para repetir; desligar o loop conserva os pontos e permite ouvir o restante da música. **Limpar A/B** remove a seleção. O menu da waveform também permite limpar um ponto individualmente.
6. O player **para ao fim da música**. No menu de botão direito do card de comandos, ative **Continuar sequência** para avançar automaticamente até a última faixa; erros interrompem a sequência.

Volume vai de 0 a 100 e controla apenas o acompanhamento. Mute mantém o volume escolhido. Velocidade: **50–150%**, com time-stretch Signalsmith para manter a afinação; **Tom** transpõe independentemente entre −6 e +6 semitons. Os resets restauram 100% e zero semitons separadamente. Clique no valor de velocidade para acessar 50/75/90/100%. Transformações extremas podem produzir artefatos: avalie o resultado no material utilizado.

O mini-player aparece fora da página Backing Tracks, compartilhando transporte e progresso. Clique no título para retornar. Stop volta ao início, ou a A quando o loop está ligado, sem restaurar velocidade/tom.

Trocar a faixa interrompe a reprodução anterior. Trocar ou desconectar a saída preserva a posição, mas exige pressionar Reproduzir novamente. Quando a saída preferida reaparece não há retomada automática nem fallback silencioso. Os seletores de entrada e saída ficam no card Áudio, no topo.

## Área de prática

A página usa um container central limitado a 1440 DIPs. A waveform fica acima de uma faixa compacta com transporte, velocidade, semitons e volume. Play/Pause tem 56 × 56 DIPs e os demais botões circulares têm 40 × 40. O display mantém décimos de segundo. O menu de botão direito do card de comandos inclui a consulta aos atalhos. Áudio e Playlist permanecem identificados por cabeçalhos com chevrons quando recolhidos. Os grupos do transporte ficam centralizados, inclusive nas linhas quebradas.

- **BPM original:** estimado localmente, com correção opcional de 20 a 300 pelo menu da track. O BPM atual acompanha a velocidade. “Não identificado” não usa 120 como resultado fictício; sem BPM, a grade de compassos fica oculta. A grade continua em **4/4**, a partir do início do arquivo, sem detectar anacruses ou mudanças de compasso.
- **Tom:** estimativa maior/menor com correção opcional. O tom atual acompanha a transposição, por exemplo Em → D#m a −1 semitom. Resultados incertos ficam como “Tom não identificado”.
- **Waveform:** altura de 160 px, com minimapa de 30 px unido à base, verde para o trecho passado e azul para o futuro, com cores adaptadas aos temas. Zoom de 1× a 16×; arraste o overview para escolher a região exibida sem alterar a reprodução. Os tempos e marcadores continuam referidos ao arquivo original.
- **Loop:** A/B é configurado somente pelo menu contextual. Os marcadores são indicadores, sem arraste; o botão circular no transporte liga/desliga o loop. A seleção é sombreada, com destaque maior quando o loop está ligado; “Ir para A” e “Limpar A/B” ficam no menu da waveform.
- **Seções:** use **Adicionar seção** no menu da waveform. O botão direito usa a posição clicada; o menu aberto por Shift+F10 ou pela tecla de contexto usa a posição atual. Um diálogo solicita o nome (Intro, Solo etc.). Os atalhos de seção ficam ordenados pelo tempo; cada seção tem um menu para navegar ou remover. Com loop ligado, a navegação fica restrita a A/B.
- **Playlist:** busca por título ou afinação, editável por faixa. Filtrar a faixa atual para fora da lista não interrompe sua reprodução. Reordenação continua disponível pelo menu, arraste e teclado.
- **Preset associado:** escolha Sem vínculo, A, B ou C. O cabeçalho lembra a associação ao selecionar/reproduzir a faixa. **A troca física deve ser feita no footswitch.** A seleção A/B/C do editor não equivale a ativar o preset físico, e o protocolo atual não fornece uma operação validada para automatizar essa troca.

## Monitor da guitarra

O card **Áudio**, no topo, contém somente os seletores de saída e entrada. Seu chevron recolhe o conteúdo, preservando o cabeçalho. Monitor, volume e medidor da guitarra ficam no card de comandos, separados do volume do acompanhamento. Atualize as entradas, escolha explicitamente a interface de captura do Windows e ative **Monitor da guitarra**. O monitor começa desligado e seu volume inicia em 0%; use fones e aumente o fader conforme necessário. Não há gravação em disco nem alteração do volume global do Windows.

A entrada é encaminhada à saída selecionada por WASAPI compartilhado, separadamente do player. Assim, pausar a track não desliga a guitarra. Trocar entrada ou saída desliga o monitor; ele não reinicia automaticamente. Erros de captura/reprodução também encerram o monitor. O fechamento libera ambos os dispositivos. A latência efetiva depende da interface, driver e saída; uma conexão Bluetooth pode introduzir atraso perceptível.

## Navegação e teclado

Clique na waveform para navegar; arrastar mostra uma prévia e confirma a posição ao soltar. Escape cancela a navegação em andamento. A roda avança/recua cinco segundos; Shift reduz para um segundo. Os faders sob o ponteiro aumentam/diminuem um ponto percentual por passo. Essas ações consomem o evento da roda, sem rolar a página. O minimapa conserva a navegação por arraste.

Com uma faixa selecionada, Espaço, L e as setas funcionam nas outras páginas do aplicativo também. Na página Backing Tracks, funcionam inclusive com foco nos botões, knobs ou playlist:

- **Espaço:** reproduzir/pausar.
- **L:** ligar/desligar loop, conservando os pontos.
- **← / →:** saltar cinco segundos; Shift aumenta o salto para dez.
- **↑ / ↓:** ajustar velocidade em passos de 5%.
- **Home:** início da faixa, ou A com loop ligado.
- **Na playlist:** Enter reproduz, Delete remove e Alt+↑/↓ reordena.

Campos de texto, comboboxes, menus e diálogos mantêm suas teclas para edição e navegação. Fora da página Backing Tracks, knobs e sliders de efeitos também preservam suas teclas. Ctrl+ +/−/0 continuam controlando o zoom da aplicação. O zoom da waveform é independente.

O menu da faixa oferece reprodução, edição dos dados manuais, reordenação, informações, mostrar no Explorer, localizar novamente e remoção. Abrir o menu de outra faixa não muda a seleção nem interrompe o áudio; as ações usam a faixa alvo. Editar, reordenar, remover ou localizar uma faixa diferente não troca a faixa em reprodução. Localizar outro arquivo limpa os dados de prática daquela entrada. Botão direito e Shift+F10/tecla de menu oferecem as mesmas ações contextuais. A única reticência fica na track atual; a playlist, as seções e a waveform não repetem esse botão.

## Exportar um trecho

Abra o menu de botão direito da waveform, ou seu equivalente pelo teclado, e escolha a exportação. O progresso e **Cancelar exportação** ficam visíveis fora da área rolável enquanto a operação estiver em andamento.

A exportação cria um **WAV PCM de 16 bits**, usando taxa e canais da fonte. Com A/B válido, exporta o trecho; sem seleção completa, exporta a faixa inteira. O modo padrão é **original**. Marque **Aplicar velocidade e tom** para exportar uma passagem processada; volume/mute de monitoração não entram no arquivo. O processamento inclui proteção de picos e compensação da latência DSP. Conversão para PCM16 usa dither.

Exportar não interrompe o playback. A operação pode ser cancelada, preserva o arquivo de destino anterior em caso de falha e impede sobrescrever os áudios presentes na playlist. O arquivo final é gravado somente após concluir o processamento.

A waveform é calculada em blocos, usando o maior pico entre canais, sem somar sinais estéreo. O cache fica em memória, limitado a 32 MiB; não guarda áudios completos. Falha na waveform não impede tentar reproduzir. Formatos/decodificadores efetivamente disponíveis dependem do Windows.

## Estado da sessão

A playlist e os ajustes de prática ficam em memória nesta versão: A/B, velocidade, tom, BPM, afinação, seções e associação de preset são mantidos enquanto a faixa estiver na sessão. Reabrir o aplicativo inicia uma playlist vazia, sem autoplay ou monitor. O antigo `backing-tracks-v1.json` não é carregado nem atualizado.

Mudança de tamanho/data do áudio invalida waveform, A/B e seções temporais. Arquivos ausentes permanecem na lista durante a sessão. O cache de waveform mantém o limite de 32 MiB.

## Motor e compilação

Playback usa NAudio.Core/Wasapi 3.1.0 e WASAPI compartilhado. Tempo/tom usam a DLL `CubeBackingDsp.dll`, compilada com MSVC x64 e Windows SDK pelo build do projeto. A publicação continua pelo script `scripts/Build-CubeControl.ps1`, incluindo simulação. Fontes Signalsmith e licenças MIT ficam fixadas em `Cube.Desktop/Native/vendor`; não há download durante o build nativo.

Loop é delimitado por frames no motor, com rampas de até 2 ms nas bordas; não depende do timer visual. O caminho processado tem proteção de pico vinculada entre canais, não normalização de loudness. Diagnósticos de erro ficam em `backing-audio.log`, limitado por tamanho, separado da comunicação USB.

A aprovação auditiva de velocidade/pitch, latência Bluetooth e ensaio contínuo de uma hora depende de validação em uma máquina com o motor nativo compilado e saídas reais. Compilar somente o código gerenciado não comprova esses critérios.

O player é independente da sessão USB-MIDI; não transmite presets nem grava áudio no pedal. Dados de simulação ficam na pasta do editor simulado. Consulte [privacidade](POLITICA-DE-PRIVACIDADE.md).

Fonte: [BackingTracksViewModel.cs](../windows/Cube.Desktop/ViewModels/BackingTracksViewModel.cs), [forma de onda](../windows/Cube.Desktop/Controls/BackingWaveform.cs) e [composição da janela](../windows/Cube.Desktop/MainWindow.xaml).

## Validação histórica da reestruturação

`dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -- --backing-practice-only` valida metadados musicais, limites de zoom, seleção A/B, seções, busca sem interromper a faixa, atalhos, monitor inicialmente desligado e layout em 640/1280/1800 px e escalas de 150%/200%. Capturas ficam em `C:/CubeControl-Build/backing-validation/reports/practice-*.png`.

A suíte `--backing-only` passa pelos testes de transporte WASAPI e renderização, mas apresenta uma falha de precisão de pitch a 50%/−6 semitons. A mesma falha foi reproduzida no commit anterior `5f6800a`, com build nativo novo, sem as alterações de interface. Essa limitação permanece aberta. O monitor ainda requer validação auditiva com entrada de guitarra e saída reais.

## Correção de responsividade — 04/10/2026

A refatoração expôs um custo elevado de `RenderTargetBitmap.Render` no thread da interface: a criação síncrona de duas imagens da waveform chegava a vários segundos em redimensionamentos. O cache também era descartado sempre que a página saía da árvore visual. A correção gera os pixels diretamente, reutiliza buffers, preserva o cache ao trocar abas e mantém os desenhos de grade/compassos/seções em cache. Mudanças de posição ou foco não recriam as imagens. O layout dos racks só é reconfigurado ao cruzar um breakpoint.

Comparação local com o mesmo executável de testes em Debug, faixa sintética de cinco minutos e janela inicial de 1500 × 950. Tempos totais incluem 20 ms de intervalo por ação; não são uma garantia para qualquer máquina:

| Sequência | Antes | Depois |
| --- | ---: | ---: |
| 20 trocas de seção | 35,73 s | 0,54 s |
| 24 redimensionamentos | 80,36 s | 1,11 s |
| 8 alternâncias maximizar/restaurar | 72,67 s | 0,36 s |

O tempo acumulado de construção das imagens nos 24 redimensionamentos caiu de 69,86 s para 0,25 s. Em 60 mudanças de posição, a alocação gerenciada caiu de aproximadamente 20 MiB para 2,4 MiB. Não houve reconstrução de imagens durante cliques, trocas de seção com tamanho estável ou reprodução; parado, o perfil não registrou novos renders/layouts.

`dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -- --backing-responsive` mede cliques, busca, navegação, redimensionamento, maximização, minimização, pan e reprodução WASAPI silenciada. Verifica que interações estáveis não recriem o cache e que sua construção não volte a bloquear a interface por centenas de milissegundos. O teste aguarda o fechamento real da janela para liberar o áudio antes de remover a faixa temporária. A suíte `--backing-practice-only` também passou após a correção, com inspeção das capturas de waveform/loop/overview.

## Histórico: primeiro player compacto e menus contextuais — 05/10/2026

O cabeçalho mostra o título da faixa e uma única linha de metadados (BPM atual, tom, afinação e preset). **Editar dados da faixa**, no menu do título ou do item da playlist, abre um diálogo para os campos manuais. O BPM inicial não é uma detecção e o tom desconhecido continua indicado por “—”. Editar esses dados não restaura zoom, posição, loop ou ajustes de velocidade/tom.

A waveform de 160 DIPs, o minimapa de 30 DIPs, o playhead, as cores de progresso e o cache otimizado foram preservados. Abaixo ficam zoom, A/B, loop com estado textual e atalhos de seções. A/B continuam disponíveis pelo teclado e pelo gesto Shift + arraste. Limpar pontos, ir para A, adicionar seção e exportar ficam no menu contextual, acessível também pelo botão **⋯**.

Os antigos cards Prática, Mixer e Reprodução/loop e o expansor misto Seções e exportação foram substituídos pela faixa compacta e pelo painel **Áudio**. Há apenas um botão **Adicionar faixas**, no cabeçalho. O botão Áudio informa **Monitor ligado** enquanto a guitarra estiver sendo monitorada, mesmo com o painel recolhido.

O transporte usa os PNGs originais de `Assets/imagens/backingTrackBotoes`, preservando relevo e transparência. O enquadramento elimina a diferença de margens entre os assets em memória; a decodificação é compartilhada e a renderização usa interpolação de alta qualidade. Play e pausa alternam conforme o estado real; os saltos exibem −5 s/+5 s. Os botões de faixa anterior/próxima usam símbolos distintos. Assets sem uma ação correspondente não criam funcionalidades. Alto contraste usa símbolos e o fader alternativo com cores do sistema; foco e estado desabilitado são explicitados.

A velocidade mantém o knob compacto e o menu de percentuais/restauração; a transposição tem os botões musicais, valor em semitons e restauração no menu. O volume usa o fader horizontal oficial e mute. Os medidores continuam sendo indicadores de pico pós-volume, não VU calibrado, com atualização durante reprodução/monitoramento.

A playlist ocupa 280 DIPs úteis, mais 16 de separação, quando aberta lateralmente. Abaixo de 1200 DIPs de janela ou 1120 DIPs disponíveis na página, começa recolhida; a escolha explícita do usuário prevalece. Em janela estreita, a lista aberta fica abaixo do player. O transporte usa uma área fixa quando há pelo menos 900 DIPs de largura na página e 440 DIPs no viewport disponível. Quando o conteúdo cabe, acompanha sua altura sem reservar um grande vazio; quando excede o viewport, permanece acessível enquanto o conteúdo rola. Abaixo desses limites entra no fluxo após a waveform. Redimensionar somente a altura também atualiza essa decisão. A página tem um único responsável pela rolagem, evitando que o transporte fique fora da janela.

### Revisão visual e de usabilidade — 05/10/2026

Esta etapa posterior foi compilada e testada com autorização, sem gerar pacote nem atualizar a publicação. Os resultados históricos acima continuam separados desta revisão.

Os centros dos botões de transporte foram alinhados; as legendas de cinco segundos têm uma linha própria. Em largura reduzida, saltos, play e parar permanecem juntos, com anterior/próxima e opções na linha seguinte. Velocidade, semitons e volume compartilham alturas de rótulo e controle. O percentual de velocidade e os indicadores de menu agora aparecem corretamente.

A playlist cresce conforme os itens, até seu limite de altura, sem reservar um painel vazio para uma única faixa. Foram removidas margens duplicadas, mensagens vazias e metadados sem faixa. Abrir Áudio traz o painel para a área visível. Em janelas baixas com zoom elevado, a navegação da página usa altura compacta para preservar espaço de trabalho.

O diálogo de metadados exibe o nome longo no conteúdo, com reticências e tooltip, e mantém um título curto. O ajuste compartilhado de diálogos remove a coluna de uma ação ausente: Salvar e Cancelar deixam de ficar separados por um espaço vazio ou quebrados a 200%. A suíte de diálogos também foi executada para cobrir essa alteração compartilhada.

#### Evidências desta execução

Artefatos em `C:/CubeControl-Build/backing-ui-review`, incluindo logs e capturas. Compilação Debug isolada por `-p:ArtifactsPath=C:/CubeControl-Build/backing-ui-review/artifacts`.

| Verificação | Resultado |
| --- | --- |
| `--backing-practice-only` | Passou: verificações funcionais preservadas e revisão adicional de layout/interação |
| `--backing-responsive` | Passou: menus de outra faixa durante reprodução silenciada preservam seleção e playback; cache não é reconstruído nesses menus, na reprodução ou nas interações estáveis |
| Tema | 66 verificações passaram |
| Localização | 13 verificações passaram |
| `--zoom-dialogs-only` | Passou em janelas compactas e amplas, a 100% e 200%, sem edições MIDI |

A matriz gera 27 capturas de janela: 640×720, 1280×720 e 1800×1000, a 100%, 150% e 200%, em escuro/português, claro/inglês e alto contraste/inglês. Também gera detalhes dos controles e estados adicionais de playlist/Áudio, nome longo, várias seções, menus, diálogos, exportação e lista vazia. Capturas representativas foram inspecionadas visualmente; limites automáticos dos controles complementam essa inspeção, sem equivaler a uma aprovação estética de cada combinação.

As regressões adicionais cobrem menus por teclado e eventos de mouse sem mudar a faixa, edição/reordenação/remoção de outra faixa, salvamento de metadados, criação de seção na posição solicitada, loop incompleto/completo, transposição, mute, teclas do fader e mudança de transporte ao redimensionar somente a altura. O teste de exportação verifica a apresentação e o cancelamento com uma operação em andamento simulada e executa separadamente a gravação real de um WAV original; não comprova cancelamento de um processamento longo pelo diálogo nativo de arquivos.

O alto contraste foi exercitado pelo tema da aplicação, sem alterar a configuração do Windows. As escalas são aplicadas à árvore WPF; não substituem validação de troca de monitor com DPI diferente. Permanecem fora desta revisão a aprovação auditiva, o monitor físico da guitarra, Narrator e o problema histórico de precisão de pitch citado acima. Waveform, minimapa, progresso e cache foram preservados nesta etapa.

## Cards com chevrons e análise musical — revisão posterior de 05/10/2026

Esta etapa substitui a organização histórica descrita acima. Áudio e Playlist usam chevrons, as reticências redundantes foram removidas e o monitor foi incorporado ao card de comandos. O transporte considera a altura real de seus grupos para decidir entre área fixa e fluxo rolável. Abrir Áudio traz os seletores para a área visível. Os novos PNGs transparentes ficam em `Assets/imagens/backingTrackBotoes/oficial`, preservando os originais; foram gerados tomando o knob oficial como referência de material. Waveform, minimapa, progresso e cache de pixels foram preservados.

### Análise automática: implementação e limites

O serviço gerenciado usa o decoder existente e analisa até três trechos distribuídos pelo arquivo, somando no máximo 90 segundos. O BPM usa envelope de ataques e autocorrelação; o tom usa FFT, perfil cromático e correlação com perfis maiores/menores. A leitura escolhe o canal de maior energia por bloco para evitar cancelamento de fase estéreo. Não há envio de áudio, modelo remoto, dependência nativa nova ou gravação de metadados no arquivo.

A fila executa uma análise por vez e prioriza a seleção. Remover/substituir uma faixa cancela seu trabalho; resultados tardios são conferidos contra a faixa alvo e a identidade do arquivo. O cache musical é separado do cache da waveform, fica em memória (até 256 resultados) e usa caminho/tamanho/data. Alterar esses atributos invalida o resultado. Trocar abas ou abrir menus não inicia nova análise.

Resultados automáticos são identificados por ≈ nos indicadores; correções manuais não têm esse prefixo. A estimativa de andamento procura periodicidades de aproximadamente 40–240 BPM; a correção manual admite 20–300. O resultado pode ser desconhecido. Andamentos variáveis, síncopes, pulsações de meio/dobro do tempo, modos, modulações e material pouco tonal podem produzir estimativas inadequadas. A correção manual permanece no menu **Corrigir dados da faixa**, junto de **Analisar novamente**. Valores corrigidos não são sobrescritos por análise em andamento nem por reanálise explícita. Afinação e preset continuam manuais. Não há certificação de precisão em músicas comerciais nesta revisão.

### Evidências desta revisão

Os logs e capturas desta etapa ficam em `C:/CubeControl-Build/backing-ui-review/compact-analysis`, separados da revisão anterior. A matriz cobre os três tamanhos, três escalas e três combinações de tema/idioma já descritos. Inclui verificações de chevrons, grupos de transporte, acesso aos seletores, roda nos dois faders e na waveform, limites de loop, teclado, menus de outra faixa e exportação.

Os testes de análise usam pulsos de 60/90/120/180 BPM, progressões sintéticas em dó maior/lá menor, silêncio, ruído, cancelamento, invalidação do cache por alteração do arquivo e correção manual durante análise. A aprovação desses casos não mede precisão em músicas reais. Os testes de reprodução são silenciados; monitor físico e avaliação auditiva continuam pendentes. A exportação em andamento é uma apresentação simulada com cancelamento, acompanhada de uma gravação real de WAV original separada.

Resultado final desta etapa: compilação e `--backing-practice-only`, `--backing-responsive`, `--theme-only` (66 verificações), `--localization-only` (13 verificações) e `--zoom-dialogs-only` passaram. Foram geradas 27 capturas da matriz e estados adicionais; houve inspeção visual de exemplos amplos, compactos, claro/escuro e alto contraste. Os rótulos das seções na waveform agora usam reticências para não se sobrepor. O teste de responsividade registrou zero reconstruções das imagens da waveform durante menus de outra faixa, reprodução e navegação durante reprodução. Tempos variam conforme a máquina e carga; não constituem garantia de latência máxima.

Capturas representativas: `practice-Dark-pt-BR-1800-1.png`, `BackingTransportRack-Dark-640-2.png`, `practice-Light-en-US-1280-1.png` e `BackingTransportRack-HighContrast-1800-2.png`. Logs finais: `practice.log`, `backing-responsive.log`, `theme-only.log`, `localization-only.log` e `zoom-dialogs-only.log`. Nenhum pacote foi gerado e nenhuma publicação foi atualizada.

## Busca e download do YouTube

A busca da playlist também consulta o YouTube após 600 ms sem digitação, a partir de três caracteres. Os resultados locais aparecem imediatamente; até dez resultados online aparecem em uma seção separada. Limpar o campo cancela a pesquisa online.

**Baixar MP3** abre Salvar como. O aplicativo obtém a melhor fonte de áudio disponível e converte para MP3 a 320 kbps; a conversão não aumenta a qualidade original. Um download por vez pode ser cancelado. O arquivo só substitui o destino confirmado após validação por ffprobe, e é adicionado à playlist sem iniciar reprodução. Vídeos privados, restritos e transmissões ao vivo não fazem parte deste fluxo.

O pacote inclui yt-dlp, Deno, FFmpeg e ffprobe com versões e SHA256 em `scripts/youtube-tools.json`. O script oficial de build prepara essas ferramentas; o aplicativo não as baixa nem atualiza em execução. Para desenvolvimento, execute `scripts/Prepare-YouTubeTools.ps1 -OutputRoot <raiz externa>` e compile usando a mesma raiz em `ArtifactsPath`.

Validação automatizada: `dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -c Release -- --youtube-only`. Cobre debounce, respostas antigas, falha de rede, cancelamento, preservação do destino, importação sem autoplay e renderizações em três temas e duas larguras. Testes de download com conteúdo real exigem um vídeo próprio ou autorizado.
