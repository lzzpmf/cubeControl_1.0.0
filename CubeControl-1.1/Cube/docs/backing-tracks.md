# Backing Tracks

[Documentação](README.md) · Revisão: 01/10/2026

A área Backing Tracks reproduz arquivos locais pelo áudio do Windows. Pode usar uma saída Bluetooth do Cube Baby quando ela estiver conectada e disponível como dispositivo de reprodução no Windows. Isso não fornece controle Bluetooth dos efeitos.

## Reproduzir e praticar

1. Adicione ou arraste até **três arquivos MP3/WAV**. Entradas duplicadas são ignoradas; remover uma faixa da lista não apaga o arquivo.
2. Escolha explicitamente uma saída de áudio. O botão de Bluetooth abre as configurações do Windows; o aplicativo não emparelha o pedal por conta própria.
3. Selecione uma faixa e use reproduzir/pausar, parar, anterior/próxima ou saltos de cinco segundos.
4. Reordene a sequência com as ações disponíveis ou arrastando seus itens.
5. Delimite um trecho A/B pelos marcadores da onda ou pelos comandos de marcar início/fim. Repetir volta ao início do trecho; sem repetição, o fim avança para a próxima faixa e encerra após a última.

Volume vai de 0 a 100 e há mute próprio do player. Velocidade: **50–150%**, aplicada pela API de mídia do Windows; não há algoritmo próprio documentado de preservação de afinação. A janela mantém um miniplayer quando há reprodução carregada.

Trocar a faixa ou a saída interrompe a reprodução anterior. Desconectar a saída também a interrompe. Quando a saída preferida reaparece, é preciso pressionar reproduzir; não há retomada automática do som.

## Exportar um trecho

A exportação cria um **WAV PCM de 16 bits**, usando taxa de amostragem e canais do arquivo original quando disponíveis. O recorte usa os tempos A/B; não incorpora velocidade, volume ou mute do player. O aplicativo impede escolher como destino um dos arquivos presentes na sequência.

A análise visual da onda decodifica temporariamente em mono/8 kHz/16 bits para calcular picos; essa representação não altera o arquivo original nem define a qualidade da reprodução. Formatos/decodificadores efetivamente disponíveis dependem do Windows.

## O que é salvo

`backing-tracks-v1.json` na pasta de dados do editor guarda:

- Caminhos dos arquivos e sua ordem, sem copiar os áudios.
- Faixa selecionada e identificador da saída preferida.
- Volume e mute.

Recortes A/B, posição, velocidade e modo de repetição **não são persistidos** por esse arquivo na implementação atual. Mover ou excluir um áudio pode deixar uma entrada sem arquivo na próxima execução. A restauração da lista não inicia reprodução.

O player é independente da sessão USB-MIDI; não transmite presets nem grava áudio no pedal. Dados de simulação ficam na pasta do editor simulado. Consulte [privacidade](POLITICA-DE-PRIVACIDADE.md).

Fonte: [BackingTracksViewModel.cs](../windows/Cube.Desktop/ViewModels/BackingTracksViewModel.cs), [forma de onda](../windows/Cube.Desktop/Controls/BackingWaveform.cs) e [composição da janela](../windows/Cube.Desktop/MainWindow.xaml).
