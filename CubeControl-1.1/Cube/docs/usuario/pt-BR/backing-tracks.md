# Backing Tracks

## Reproduzir
Adicione arquivos MP3/WAV e escolha a saída de áudio do Windows. A lista referencia os arquivos originais; mantenha-os disponíveis. Reproduzir uma faixa não exige conexão USB com o pedal. Bluetooth usa a saída de áudio do Windows e não edita o pedal.

## Praticar um trecho
Use A/B para marcar um trecho e repetir. Ajuste velocidade e transposição conforme os controles disponíveis. BPM e tom detectados automaticamente são estimativas; você pode corrigi-los. O monitor da guitarra usa a entrada de áudio escolhida, não simula o DSP do pedal.

A identificação de BPM e tom é local e offline: analisa até cinco trechos de até 20 segundos, distribuídos pela faixa. Compara padrões rítmicos entre trechos e compensa pequenos desvios de afinação ao estimar a tonalidade maior ou menor. Silêncio, ritmo ambíguo, mudanças de andamento ou tonalidade podem resultar em **não identificado**. A faixa de detecção automática é de 40 a 240 BPM; a correção manual continua aceitando 20 a 300 BPM. Valores corrigidos manualmente são preservados ao reanalisar.

Faixas adicionadas, baixadas ou gravadas começam **sem afinação e sem preset associados**. Em **Corrigir dados da faixa**, deixe Afinação vazia para remover esse vínculo e escolha a opção sem associação para o preset. O tom estimado não identifica a afinação das cordas e não vincula um preset. Vínculos escolhidos manualmente são mantidos durante a sessão.

Os botões de voltar e avançar saltam cinco segundos. Eles aparecem somente com ícones; passe o mouse para consultar a ação.

Em **Tempo e tom**, role a roda do mouse sobre o percentual para alterar a velocidade de 1 em 1%, entre 50% e 150%. Sobre o valor em semitons, cada passo aumenta ou diminui o pitch em 1 semitom, entre −6 e +6. Rolar para cima aumenta; para baixo diminui. É necessário selecionar uma faixa. Clique nos valores para abrir as opções e restaurar velocidade ou tom. A roda sobre esses controles ajusta o valor sem rolar a página.

Fora de REC, ao escolher outra saída válida, a faixa continua a partir da posição atual, sem exigir Reproduzir novamente; pode haver uma breve interrupção durante a abertura do dispositivo. Uma faixa pausada permanece pausada. Trocar a entrada não pausa a faixa, e a monitoração é restabelecida se estava ligada. Entrada e saída ficam bloqueadas durante REC. Desconexão ou falha de dispositivo pode interromper o áudio e exige conferir a mensagem e a saída selecionada.

## Mixer

Abra o mixer pelo ícone de ajustes ao lado do metrônomo. Cada canal — **Faixa**, **Guitarra** e **Metrônomo** — tem um fader horizontal, um percentual editável e um botão de mute alinhado ao valor. O próprio botão mostra o estado silenciado; não há um segundo indicador separado. Os mutes são independentes e preservam os volumes escolhidos.

O mixer tem um único layout, com os comandos de monitoração e metrônomo sempre disponíveis. Atrás de cada fader, o medidor mostra o pico digital daquele canal após volume e mute, na escala de −60 a 0 dBFS, com retenção de pico por um segundo. Faixa e click são medidos separadamente; a guitarra só apresenta nível enquanto a monitoração estiver ativa. O aviso de saturação da entrada permanece independente do volume da guitarra. São picos de amostra, não medições de true peak ou VU analógico calibrado. **Resetar volumes** restaura faixa em 30%, guitarra em 0% e metrônomo em 100%, preservando os mutes. Um duplo clique no fader restaura apenas o volume daquele canal.

## Sessões e pistas

Cada música da playlist abre uma sessão com backing track e gravações sincronizadas. **Adicionar pista**, ou soltar arquivos sobre as pistas, importa áudio para essa sessão; adicionar arquivos à playlist cria músicas separadas. Selecionar uma pista muda o alvo de edição sem interromper as demais.

Cada pista tem **Mute**, **Solo** e volume. Vários solos podem estar ativos; somente essas pistas serão ouvidas. Mute prevalece sobre solo. O canal Faixa do mixer controla o conjunto; monitoração e metrônomo continuam separados. Arraste músicas ou pistas para reordenar: a linha e o rótulo indicam onde serão inseridas, com rolagem nas bordas. Esc cancela. Os botões de mover para cima/baixo oferecem uma alternativa. A ordem não altera o início temporal.

A playlist salva sessões, ordem, mute/solo, volumes e parâmetros da tomada. O formato anterior é migrado para uma sessão por arquivo, com cópia `.v1.bak`; gravações antigas não são associadas automaticamente. Arquivos ausentes permanecem listados; use **Localizar áudio da pista**. Remover a pista não apaga o WAV.

## Gravar

Escolha entrada e saída e pressione **REC**. O transporte começa no cursor e uma nova pista aparece imediatamente com a waveform crescendo a partir das amostras capturadas. A captura grava apenas a entrada, no formato WASAPI do dispositivo; não inclui automaticamente backing track, metrônomo ou monitoração. Uma mistura fornecida pelo próprio dispositivo será capturada como recebida.

Durante REC, entrada, saída, seek, loop, velocidade, tom e troca de sessão ficam bloqueados. Mute, solo e reordenação continuam disponíveis. A prévia do YouTube fica bloqueada para não pausar o transporte. A tomada é contínua, sem gravação em voltas. É possível iniciar com velocidade/tom alterados; esses parâmetros são registrados para manter a reprodução posterior alinhada e afinada.

**Acompanhar gravação** mantém a ponta visível quando ampliada. Navegar no overview ou alterar zoom desativa o acompanhamento; marque novamente para retomar. **Compensação de latência (ms)** antecipa a tomada com valores positivos e a atrasa com negativos. Configure antes de gravar. O alinhamento usa timestamps de captura e o relógio do player, mas sua precisão física depende do dispositivo e deve ser conferida ouvindo uma tomada.

REC novamente finaliza e pausa o transporte; **Stop** finaliza e retorna ao início. A mesma pista passa de gravando para finalizando e salva. Sem música selecionada, cria-se uma sessão para a gravação. Capturas vazias não criam pistas válidas. Interrupções preservam o WAV recuperável indicado e identificam a pista como interrompida. Falha ao salvar a playlist é informada separadamente do salvamento do áudio.

Os arquivos recebem data/hora e identificador único em `Recordings` no diretório de dados. Fechar o app finaliza a captura. O envelope visual usa memória limitada e atualização nominal de 50 ms, sem carregar toda a gravação na RAM.

## Exportar

Escolha **Mixagem da sessão** ou **Pista selecionada**. Ambas geram WAV PCM de 16 bits, estéreo, a 48 kHz. A mixagem respeita alinhamento, mute/solo, volumes das pistas e volume/mute geral; exclui monitoração e metrônomo. A pista isolada ignora mute/solo e volume de reprodução.

Um intervalo A/B válido exporta uma passagem mesmo com loop desligado; sem intervalo válido, usa a duração disponível. Aplicar velocidade e tom usa os ajustes da sessão; desmarcada, a opção usa 100% e tom original, compensando os parâmetros registrados nas tomadas. A exportação preserva os arquivos de origem e pode ser cancelada.

## YouTube
A busca e o download precisam de internet e usam ferramentas incluídas no pacote. Escolha conteúdo próprio ou autorizado. Uma falha de rede não deve substituir um arquivo já existente. Importar não inicia reprodução automaticamente.

Playlist e YouTube ficam em cards separados abaixo do player, com buscas independentes. Em janela estreita, os cards ficam empilhados e abertos.

O botão de fones dentro de **Buscar faixas** acrescenta “backing track” ao termo pesquisado. Quando ativado, recebe fundo na cor de destaque e uma marca de confirmação; quando desativado, fica neutro e sem a marca. Ele modifica a busca, sem garantir que todos os resultados sejam acompanhamentos instrumentais.

Mantenha o mouse sobre um vídeo por 800 ms para pré-ouvir até 30 segundos. Se uma faixa estiver tocando, ela será pausada quando o áudio da prévia estiver pronto. Ao sair do card ou terminar a prévia, a faixa continua pausada na mesma posição; use Reproduzir para retomá-la. Se a preparação falhar, a faixa continua tocando. Pelo teclado, use Pré-ouvir e Escape para encerrar.

No primeiro download, escolha uma pasta. Ela será lembrada para os próximos downloads, inclusive após reiniciar o aplicativo. Use **Alterar pasta** no card YouTube para escolher outro destino. Nomes repetidos recebem um número, sem sobrescrever arquivos. O card mostra a etapa, uma barra de progresso indeterminada e a opção de cancelar durante o download, a conversão e a validação.

## Metrônomo

O metrônomo fica nos controles do player: o ícone de metrônomo liga/desliga e o botão com a mão recebe cliques repetidos para **Tap tempo**. Role sobre o número de BPM para aumentar ou diminuir de 1 em 1, entre 20 e 300, sem rolar a página. Clique no valor para editá-lo. A seta ao lado abre as opções de sincronização, compasso, acento, contagem e alinhamento. O volume e o mute próprios ficam no mixer.

Escolha Click, Bell, Ping, Woodblock ou Hi-hat no menu de sons, ou importe WAV mono/estéreo PCM ou float de até dois segundos. O app copia o arquivo para seus dados locais e lembra a seleção; arquivos indisponíveis retornam para Click.
Use **Ligar metrônomo** sem **Acompanhar faixa** para praticar mesmo com a playlist vazia. Ajuste BPM, Tap tempo, compasso, acento e volume próprios. O mute da faixa não silencia o metrônomo; use o mute do canal Metrônomo. Em 6/8, o BPM representa semínimas e cada clique representa uma colcheia.

Com **Acompanhar faixa**, o metrônomo usa o BPM detectado ou corrigido e acompanha a velocidade, pausa, seek e loop da faixa local. Use **Marcar primeiro tempo aqui** ou **Primeiro tempo (s)** para alinhar a grade. **Contagem (compassos)** aceita 0, 1 ou 2 e prepara o início; retomar pausa e repetir o loop não refaz a contagem. A prévia do YouTube não oferece sincronização; o modo independente continua disponível.

Preferências, BPM manual e alinhamento por arquivo são lembrados, mas o metrônomo sempre abre parado. Trocar de modo interrompe os cliques. Fora de REC, ao escolher outra saída válida, o app restabelece o metrônomo se estava ligado. Exportar WAV e gravar com REC não incluem automaticamente o metrônomo.

## Sessão e arquivos

`backing-playlist-v1.json` mantém o nome por compatibilidade de localização e passa a armazenar o formato de sessões versão 2: ordem, arquivos, pistas, mute/solo, volumes, início e parâmetros das tomadas, velocidade/tom da sessão e compensação de latência. Mudanças de organização e mix são persistidas e o fechamento salva novamente. Seleção, saída e estado de reprodução não são restaurados. O antigo `backing-tracks-v1.json` não é carregado nem atualizado. Preferências do metrônomo, BPM/alinhamento por arquivo e registros de downloads continuam separados. A playlist referencia os áudios e não é um backup deles. Consulte [Privacidade](privacidade.md).

Os sons do metrônomo, incluindo WAVs personalizados não silenciosos, têm pico normalizado a 0 dBFS antes do fader. O padrão e o reset do metrônomo são 100%; volumes já salvos são preservados. Reduzir o fader reduz o pico de saída. A normalização de pico não iguala o volume percebido dos timbres.
