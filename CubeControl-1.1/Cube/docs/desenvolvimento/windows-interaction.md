# Interação, zoom e aparência do Windows

[Documentação](../README.md) · [Design system](../DESIGN_SYSTEM.md) · Revisão: 10/10/2026

- Zoom de 50% a 200%, em passos de 10%, nas reticências. Menos, percentual, mais e restaurar. Atalhos: Ctrl +, Ctrl -, Ctrl 0. Preferência local em `%LOCALAPPDATA%/CubeControl/zoom-v1.json`; 100% preserva a densidade anterior de 90%. Não modifica as configurações de DPI do Windows. O manifesto declara PerMonitorV2.
- Rolagem da roda nas páginas e diálogos interpolada por 180 ms, com alvo acumulado para entrada rápida/fracionada. `CompositionTarget.Rendering` agenda os quadros, sem timer de 16 ms ou loop permanente. A inscrição termina ao concluir, ocultar ou descarregar a superfície; navegação pelo teclado/toque e reposicionamento externo interrompem o movimento. Limites permitem continuar a rolagem no contêiner externo.
- Knobs, seleção de equipamento e cadeia preservam suas interações da roda. Zoom, layout e navegação não enviam comandos MIDI.
- Transições de opacidade e deslocamento usam 167 ms, com `DesiredFrameRate=240`. Animações desativadas no Windows, alto contraste ou renderização sem aceleração desativam os movimentos decorativos. A duração da rolagem depende do tempo transcorrido, não da contagem de quadros.
- Mica acompanha `EnableTransparency` e notificações do Windows mesmo com tema Claro/Escuro manual. Transparência desligada, sistema incompatível ou alto contraste usam fundo sólido. A janela mantém o chrome existente para evitar o erro de contexto Freezable ao trocar WindowChrome no WPF UI 4.3.
- A/B/C e ações se reorganizam em larguras pequenas. Diálogos com pouco espaço reduzem margens e quebram texto dos botões, preservando o conteúdo rolável e os comandos nativos.

## Verificação

### Player e mixer de Backing Tracks

O player organiza tempo/tom, transporte e mixer/metrônomo em grupos adaptativos. Os saltos de cinco segundos usam somente ícones, com tooltip e nome acessível; as legendas “−5 s” e “+5 s” foram removidas. O metrônomo usa `CubeMetronomeIcon`, vetor compartilhado no botão de ativação e no canal do mixer, com cor herdada do controle.

| Controle sob o mouse | Passo da roda | Limites |
| --- | --- | --- |
| Velocidade | 1 ponto percentual | 50–150% |
| Pitch | 1 semitom | −6…+6 |
| BPM | 1 BPM | 20–300 |

Rolar para cima aumenta e para baixo diminui. Os handlers de `MainWindow.BackingMenus.cs` acumulam deltas fracionados até 120 unidades por passo, com acumuladores independentes e limpeza ao sair do controle. `EditorPageWheel` preserva esses alvos, evitando rolagem da página. Velocidade e pitch exigem faixa selecionada e reutilizam os setters existentes. BPM respeita o modo independente ou **Acompanhar faixa**. Clicar continua abrindo as opções de tempo/tom ou a edição de BPM.

O mixer tem largura máxima de 620 DIPs, faders horizontais e um único layout. Cada canal tem um mute junto ao percentual, com tooltip e nome acessível de silenciar/restaurar som. Os três mutes são independentes. Os comandos de monitor/metrônomo ficam sempre disponíveis. Os medidores atrás dos faders mostram picos de amostra separados após volume e mute, de −60 a 0 dBFS; a saturação da entrada é detectada antes do volume. A atualização visual ocorre aproximadamente a cada 33 ms, com queda de 24 dB/s e retenção de pico e saturação por um segundo. Reset restaura 30/0/30% e preserva mute; duplo clique restaura o fader individual.

O toggle de fones no campo de busca vincula `AppendBackingTrack` e acrescenta “backing track” à consulta. Ativado, usa `CubeAccent`, texto sobre destaque e marca de confirmação; desativado, aparência neutra sem a marca. `AutomationProperties.ItemStatus` informa Ligado/Desligado com localização. A indicação combina forma e cor.

`--player-layout-only` verifica passos, sentidos, deltas fracionados e limites da roda, além de contenção do transporte e mixer em 640/1600 DIPs, zoom 50/100/200%, pt-BR/en-US e temas claro/escuro. Gera capturas em `C:/CubeControl-Build/reports/player-layout`, incluindo os dois modos do mixer e estados do botão de busca. A suíte passou durante a implementação e capturas foram inspecionadas; isso não substitui validação auditiva, Narrator ou mouse físico.

### YouTube em Backing Tracks

Os dez resultados aparecem em mosaico adaptativo, com miniaturas 16:9. Os botões de baixar MP3 e pré-ouvir ficam sobre a imagem e aparecem com o mouse ou foco de teclado; em toque, tocar no card revela as ações.

Manter o mouse por 800 ms inicia até 30 segundos de pré-escuta, na saída selecionada e respeitando volume/mute do acompanhamento. Uma faixa local em reprodução é pausada quando o áudio da prévia está pronto, preservando a posição; encerrar a prévia não retoma a faixa. Falhas durante a preparação mantêm a faixa tocando. Iniciar a faixa local encerra a prévia primeiro. A prévia não aplica tom, velocidade ou loop e não adiciona arquivos à playlist. Sair do card, pressionar Escape, mudar a busca/página/saída, ocultar os resultados, desativar a janela ou iniciar um download cancela a prévia. Pelo teclado, use o botão Pré-ouvir; apenas receber foco não toca áudio.

Playlist e YouTube usam cards separados abaixo do player, com filtros independentes. A partir de 860 DIPs disponíveis, a playlist ocupa 300 DIPs à esquerda, com intervalo de 16 DIPs e listas limitadas a 480 DIPs de altura. Em larguras menores, ficam empilhados e abertos, com rolagem da página. Downloads usam `InfiniteProgressBar` dentro do card e lembram a pasta em `youtube-downloads-v1.json` no diretório de dados do aplicativo.

Falhas e carregamento aparecem no card; uma nova tentativa exige sair e voltar ou acionar Pré-ouvir. A preparação tem limite de 20 segundos. A implementação usa yt-dlp, FFmpeg e NAudio já presentes no aplicativo, com buffer limitado e uma prévia por vez. `--youtube-only` verifica o ciclo simulado e o layout; `--youtube-preview-live` exercita rede e WASAPI reais com volume zero.

### Demais verificações

Testes com MIDI simulado: `--zoom-only` (50/100/150/200%, pt-BR/en-US, 640–1920 DIPs), `--zoom-dialogs-only`, `--options-menu-only`, `--motion-only`, `--theme-only`, `--wheel-only`, `--localization-only`. O teste dos diálogos exige viewport útil e ações dentro do host, além de capturas para revisão visual.

`--scroll-profile` registra intervalos entre callbacks WPF no editor conectado ao simulador em `C:\CubeControl-Build\reports\interaction\scroll-profile.json`, conforme [ScrollProfileTests.cs](../../windows/Cube.Desktop.Tests/ScrollProfileTests.cs). O antigo caminho `windows/artifacts/interaction` pertencia à árvore histórica. O relatório é gerado, não incluído no checkout. Uma execução histórica a 1280×720, render tier 2, registrou 289 intervalos: mediana de 4 ms e percentil 95 de 12 ms. Esses valores são callbacks de renderização; não são FPS efetivamente apresentados pelo DWM/monitor. Não certificam 240 FPS constantes, ausência universal de engasgos, nem troca física entre monitores com DPIs diferentes.

Os modos são argumentos de `Cube.Desktop.Tests`, executado conforme o [guia Windows](../../windows/README.md). Não foram repetidos nesta revisão documental.

## Referências

- [Microsoft: renderização por quadro no WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-render-on-a-per-frame-interval-using-compositiontarget)
- [Microsoft: DesiredFrameRate é uma meta, não uma garantia](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.animation.timeline.desiredframerate?view=windowsdesktop-10.0)
- [Microsoft: Mica e fallback de transparência](https://learn.microsoft.com/en-us/windows/apps/design/style/mica)

## Pré-escuta de IRs com guitarra gravada

Na seção IRs, selecione um dos cinco trechos de guitarra e a saída USB de áudio do pedal. A reprodução requer conexão MIDI operacional, mas não exige IR carregado: sem prévia de IR, o pedal processa o WAV com o preset atual e os ajustes aplicados à cadeia, inclusive prévias de amps. Uma prévia de IR ativa e não confirmada continua bloqueando o início. O WAV cru é processado pelo pedal, mantendo os efeitos atuais. Volume inicial: 30%; repetição ativada.

Trocar o IR pausa e silencia o áudio durante a transferência, retomando na mesma posição somente após confirmação. Trocar o trecho reinicia sua reprodução; pausar preserva a posição e parar volta ao início. Navegar para a Pedaleira ou o Criador de amps mantém a pré-escuta e exibe controles compactos de pausa/parada. Restaurar o IR pausa a reprodução e retoma após confirmação; trocar a saída, desconectar ou fechar o app interrompe o áudio. Backing Tracks, prévias do YouTube e pré-escuta de IRs não tocam simultaneamente. Nenhum preset ou slot é salvo automaticamente.

## Navegação, documentação e biblioteca compartilhada

A roda sobre a barra principal troca de aba a cada 120 unidades, sem circular nos extremos; fora dela, a rolagem atual permanece. Alt isolado é consumido na janela principal, sem remover foco; Tab/Shift+Tab e combinações do sistema permanecem disponíveis. Tooltips duram cinco segundos desde a exibição, após o atraso inicial de 1,2 segundo.

A documentação usa FluentWindow e TitleBar compartilhados; a barra fica fora do conteúdo escalado. Buscas de IRs, playlist, YouTube e documentação usam os estilos compartilhados de cápsula. Em Buscar faixas, o botão que acrescenta backing track fica fora do campo, à direita, com espaçamento de 8 DIPs. A lista local e os resultados/downloads usam linhas compactas; apenas os resultados exibem a waveform lateral quando há espaço e dados.

Gerenciar IRs compartilha uma IrLibraryViewModel entre análise/prévia individual e packs. Entrar em Packs invalida carregamentos individuais pendentes; não encerra uma prévia já ativa. Os 34 packs preservam seus grupos do catálogo; editar predefinidos cria cópia. As ações de aplicar, restaurar, salvar e descartar ficam após os slots.

No player, a troca para uma saída válida preserva a posição e a intenção de reproduzir ou permanecer pausado. Entrada não interrompe a faixa; monitoração ativa é restabelecida. REC captura somente a entrada, cria uma pista sincronizada e bloqueia entrada, saída, seek, loop, velocidade, tom e troca de sessão durante captura/finalização. Abertura de dispositivos pode causar uma breve lacuna; falhas/desconexões continuam usando o tratamento de erro.

A análise musical offline tem identidade de cache versionada, até cinco trechos de 20 segundos, reamostragem antialiasing, fluxo espectral e consenso para BPM; cromas de 36 posições por oitava e correção de desvio de afinação para tom. Confianças são heurísticas, não probabilidades calibradas. Faixas novas não recebem afinação nem preset automaticamente; correções manuais prevalecem.

## Sessões de áudio e feedback de drop

`BackingSessionAudio` mistura pistas em float estéreo a 48 kHz, com relógio comum, conversão de sample rate e ajuste de velocidade/tom relativo à tomada. Reprodução e exportação compartilham esse processamento. Mute precede solo; vários solos são permitidos. Decoders continuam avançando pistas silenciadas para preservar sincronização.

`BackingRecording` acumula picos incrementais e registra o QPC do primeiro pacote WASAPI. A UI consulta o envelope a cada 50 ms; a duração deriva das amostras escritas. O envelope compacta picos ao atingir 32.768 bins. A compensação de latência é aplicada ao início da pista sem modificar o WAV. Precisão física, carga contínua e estabilidade de dispositivo dependem de validação posterior em hardware.

`DragFeedback` desenha feedback em adorner independente da seleção. Playlist e pistas compartilham cálculo de inserção na coleção original, inclusive com filtro. O editor de packs informa adição, substituição ou troca antes do drop e não grava automaticamente no pedal. Cancelamento e saída do destino limpam a camada. Botões e menus mantêm alternativas ao gesto.

Cobertura preparada em `--backing-session-only`: áudio sintético, envelope, persistência e cálculo de inserção. Os modos existentes de gravação e packs também precisam ser executados após autorização. Nenhum teste ou build foi executado nesta implementação. Conferir posteriormente temas, alto contraste, pt-BR/en-US, zoom, janela compacta, teclado, captura longa e desconexão.

### Comunicação otimizada de prévias em RAM (validação pendente)

Chorus, Delay e Reverb validam o contexto antes do lote, enviam somente campos diferentes em ordem com ACK e verificam o resultado ao final. Não releem banco, seleção física e bloco DSP entre campos. Phaser e amp já seguem a estratégia de diferenças; o IR mantém transferência e conferência completas. Mudanças físicas durante o lote podem ser detectadas somente ao final; não há promessa de impedir interferência intermediária.

A retirada de prévias bloqueia novas edições durante a restauração. A monitoração não inicia consultas durante operações ocupadas ou retirada; uma consulta já iniciada termina antes da operação. As filas existentes consolidam valores pendentes. Progresso de IR/amp é publicado no máximo a cada 50 ms, preservando mudanças de etapa e conclusão. Packs são resolvidos uma vez por aplicação normal, mantendo nova resolução quando vierem do catálogo.

O log `PERF` registra duração, leituras, comandos de escrita e bytes SysEx TX/RX por operação instrumentada, incluindo operações interrompidas (o registro não significa sucesso). Identificação não conta como escrita; registros/apagamentos contam. Tamanhos de blocos, timeouts e serialização permanecem iguais. Flash mantém comparação prévia, backup e readback. Nenhum cache é compartilhado entre sessões.

Cobertura preparada em `CommunicationPerformanceTests`, `ChorusPreviewTests`, `DelayTests` e `ReverbTests`: limites de leituras, restauração, alterações externas e falhas. As suítes existentes de fila, amp, Phaser, IR e packs cobrem coalescência, ACK, timeout e recuperação. Não executadas nesta alteração. Após autorização, executar as suítes e medir no pedal início/retirada de cada prévia, leitura de slots, packs e presets; comparar contagem e duração, confirmar falhas e recuperação. Sem medições físicas, não afirmar ganho de tempo.
