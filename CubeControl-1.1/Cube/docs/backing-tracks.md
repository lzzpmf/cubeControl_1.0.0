# Backing Tracks

[Documentação](README.md) · Revisão: 02/10/2026

A área Backing Tracks reproduz arquivos locais pelo áudio do Windows. Pode usar uma saída Bluetooth do Cube Baby quando ela estiver conectada e disponível como dispositivo de reprodução no Windows. Isso não fornece controle Bluetooth dos efeitos.

## Reproduzir e praticar

1. Adicione ou arraste arquivos **MP3/WAV**, sem o limite de três músicas. Entradas duplicadas são ignoradas; remover uma faixa da lista não apaga o arquivo. Arquivos recusados são informados. A análise é feita sob demanda.
2. Escolha explicitamente uma saída de áudio. O botão de Bluetooth abre as configurações do Windows; o aplicativo não emparelha o pedal por conta própria.
3. Selecione uma faixa e use reproduzir/pausar, parar, anterior/próxima ou saltos de cinco segundos. Selecionar não inicia áudio; duplo clique ou Enter na playlist reproduz a faixa.
4. Reordene a sequência com as ações disponíveis ou arrastando seus itens.
5. Marque A e B na posição atual e refine os pontos arrastando seus marcadores. O intervalo mínimo é 0,1 s. Ative **Loop A/B** para repetir; desligar o loop conserva os pontos e permite ouvir o restante da música. **Limpar A/B** remove a seleção. O menu da waveform também permite limpar um ponto individualmente.
6. O player **para ao fim da música**. Ative **Continuar sequência** para avançar automaticamente até a última faixa; erros interrompem a sequência.

Volume vai de 0 a 100 e controla apenas o acompanhamento. Mute mantém o volume escolhido. Velocidade: **50–150%**, com time-stretch Signalsmith para manter a afinação; **Tom** transpõe independentemente entre −6 e +6 semitons. Os resets restauram 100% e zero semitons separadamente. Clique no valor de velocidade para acessar 50/75/90/100%. Transformações extremas podem produzir artefatos: avalie o resultado no material utilizado.

O mini-player aparece fora da página Backing Tracks, compartilhando transporte e progresso. Clique no título para retornar. Stop volta ao início, ou a A quando o loop está ligado, sem restaurar velocidade/tom.

Trocar a faixa interrompe a reprodução anterior. Trocar ou desconectar a saída preserva a posição, mas exige pressionar Reproduzir novamente. Quando a saída preferida reaparece não há retomada automática nem fallback silencioso. O seletor distingue saída selecionada e saída em uso.

## Navegação e teclado

Clique na waveform para navegar; arrastar mostra uma prévia e confirma a posição ao soltar. Escape cancela o gesto. Os pontos A/B também são confirmados ao soltar.

Na waveform/área do player: Space reproduz/pausa; ←/→ saltam cinco segundos; Shift+←/→ saltam dez; A/B marcam pontos; L alterna loop; Home vai ao início ou a A com loop ligado. Na playlist: Enter reproduz, Delete remove e Alt+↑/↓ reordena. Controles de texto, botões, sliders e menus mantêm suas teclas próprias. Ctrl+ +/−/0 continuam controlando o zoom da aplicação.

O menu da faixa oferece remoção, reordenação, informações, mostrar no Explorer e localizar novamente um arquivo movido. Localizar outro arquivo limpa os dados de prática daquela entrada.

## Exportar um trecho

A exportação cria um **WAV PCM de 16 bits**, usando taxa e canais da fonte. Com A/B válido, exporta o trecho; sem seleção completa, exporta a faixa inteira. O modo padrão é **original**. Marque **Aplicar velocidade e tom** para exportar uma passagem processada; volume/mute de monitoração não entram no arquivo. O processamento inclui proteção de picos e compensação da latência DSP. Conversão para PCM16 usa dither.

Exportar não interrompe o playback. A operação pode ser cancelada, preserva o arquivo de destino anterior em caso de falha e impede sobrescrever os áudios presentes na playlist. O arquivo final é gravado somente após concluir o processamento.

A waveform é calculada em blocos, usando o maior pico entre canais, sem somar sinais estéreo. O cache fica em memória, limitado a 32 MiB; não guarda áudios completos. Falha na waveform não impede tentar reproduzir. Formatos/decodificadores efetivamente disponíveis dependem do Windows.

## O que é salvo

`backing-tracks-v1.json` na pasta de dados do editor guarda:

- Caminhos dos arquivos e sua ordem, sem copiar os áudios.
- Faixa selecionada e identificador da saída preferida.
- Volume e mute.
- Modo de sequência e ajustes A/B, velocidade e tom por faixa.

O arquivo mantém seu nome, com schema v2 e leitura da versão anterior. Posição, loop ligado e estado tocando não são persistidos: a reabertura começa em zero, sem loop e sem autoplay. Ajustes restaurados continuam visíveis. Mudança de tamanho/data do áudio invalida waveform e pontos temporais. Arquivos ausentes permanecem na lista.

## Motor e compilação

Playback usa NAudio.Core/Wasapi 3.1.0 e WASAPI compartilhado. Tempo/tom usam a DLL `CubeBackingDsp.dll`, compilada com MSVC x64 e Windows SDK pelo build do projeto. A publicação continua pelo script `scripts/Build-CubeControl.ps1`, incluindo simulação. Fontes Signalsmith e licenças MIT ficam fixadas em `Cube.Desktop/Native/vendor`; não há download durante o build nativo.

Loop é delimitado por frames no motor, com rampas de até 2 ms nas bordas; não depende do timer visual. O caminho processado tem proteção de pico vinculada entre canais, não normalização de loudness. Diagnósticos de erro ficam em `backing-audio.log`, limitado por tamanho, separado da comunicação USB.

A aprovação auditiva de velocidade/pitch, latência Bluetooth e ensaio contínuo de uma hora depende de validação em uma máquina com o motor nativo compilado e saídas reais. Compilar somente o código gerenciado não comprova esses critérios.

O player é independente da sessão USB-MIDI; não transmite presets nem grava áudio no pedal. Dados de simulação ficam na pasta do editor simulado. Consulte [privacidade](POLITICA-DE-PRIVACIDADE.md).

Fonte: [BackingTracksViewModel.cs](../windows/Cube.Desktop/ViewModels/BackingTracksViewModel.cs), [forma de onda](../windows/Cube.Desktop/Controls/BackingWaveform.cs) e [composição da janela](../windows/Cube.Desktop/MainWindow.xaml).
