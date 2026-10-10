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

O mixer tem largura máxima de 620 DIPs e faders horizontais. Cada canal tem um único mute junto ao percentual, disponível nos modos Compacto e Expandido, com tooltip e nome acessível de silenciar/restaurar som. Os três mutes são independentes. Expandido acrescenta comandos de monitor/metrônomo e medidores: entrada antes do volume; saída com faixa e click sincronizado. Reset restaura 30/0/30% e preserva mute; duplo clique restaura o fader individual.

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

Na seção IRs, selecione um dos cinco trechos de guitarra e a saída USB de áudio do pedal. A reprodução requer conexão MIDI operacional e uma prévia de IR confirmada em RAM. O WAV cru é processado pelo pedal, mantendo os efeitos atuais. Volume inicial: 30%; repetição ativada.

Trocar o IR pausa e silencia o áudio durante a transferência, retomando na mesma posição somente após confirmação. Trocar o trecho reinicia sua reprodução; pausar preserva a posição e parar volta ao início. Sair de IRs, trocar a saída, desconectar, encerrar a prévia ou fechar o app interrompe a pré-escuta. Backing Tracks, prévias do YouTube e pré-escuta de IRs não tocam simultaneamente. Nenhum preset ou slot é salvo automaticamente.
