# Backing Tracks

## Reproduzir
Adicione arquivos MP3/WAV e escolha a saída de áudio do Windows. A lista referencia os arquivos originais; mantenha-os disponíveis. Reproduzir uma faixa não exige conexão USB com o pedal. Bluetooth usa a saída de áudio do Windows e não edita o pedal.

## Praticar um trecho
Use A/B para marcar um trecho e repetir. Ajuste velocidade e transposição conforme os controles disponíveis. BPM e tom detectados automaticamente são estimativas; você pode corrigi-los. O monitor da guitarra usa a entrada de áudio escolhida, não simula o DSP do pedal.

A identificação de BPM e tom é local e offline: analisa até cinco trechos de até 20 segundos, distribuídos pela faixa. Compara padrões rítmicos entre trechos e compensa pequenos desvios de afinação ao estimar a tonalidade maior ou menor. Silêncio, ritmo ambíguo, mudanças de andamento ou tonalidade podem resultar em **não identificado**. A faixa de detecção automática é de 40 a 240 BPM; a correção manual continua aceitando 20 a 300 BPM. Valores corrigidos manualmente são preservados ao reanalisar.

Faixas adicionadas, baixadas ou gravadas começam **sem afinação e sem preset associados**. Em **Corrigir dados da faixa**, deixe Afinação vazia para remover esse vínculo e escolha a opção sem associação para o preset. O tom estimado não identifica a afinação das cordas e não vincula um preset. Vínculos escolhidos manualmente são mantidos durante a sessão.

Os botões de voltar e avançar saltam cinco segundos. Eles aparecem somente com ícones; passe o mouse para consultar a ação.

Em **Tempo e tom**, role a roda do mouse sobre o percentual para alterar a velocidade de 1 em 1%, entre 50% e 150%. Sobre o valor em semitons, cada passo aumenta ou diminui o pitch em 1 semitom, entre −6 e +6. Rolar para cima aumenta; para baixo diminui. É necessário selecionar uma faixa. Clique nos valores para abrir as opções e restaurar velocidade ou tom. A roda sobre esses controles ajusta o valor sem rolar a página.

Ao escolher outra saída válida, a faixa continua a partir da posição atual, sem exigir Reproduzir novamente; pode haver uma breve interrupção durante a abertura do dispositivo. Uma faixa pausada permanece pausada. Trocar a entrada não pausa a faixa, e a monitoração é restabelecida se estava ligada. A entrada fica bloqueada durante REC. Desconexão ou falha de dispositivo pode interromper o áudio e exige conferir a mensagem e a saída selecionada.

## Mixer

Abra o mixer pelo ícone de ajustes ao lado do metrônomo. Cada canal — **Faixa**, **Guitarra** e **Metrônomo** — tem um fader horizontal, um percentual editável e um botão de mute alinhado ao valor. O próprio botão mostra o estado silenciado; não há um segundo indicador separado. Os mutes são independentes e preservam os volumes escolhidos.

O mixer tem um único layout, com os comandos de monitoração e metrônomo sempre disponíveis. Atrás de cada fader, o medidor mostra o pico digital daquele canal após volume e mute, na escala de −60 a 0 dBFS, com retenção de pico por um segundo. Faixa e click são medidos separadamente; a guitarra só apresenta nível enquanto a monitoração estiver ativa. O aviso de saturação da entrada permanece independente do volume da guitarra. São picos de amostra, não medições de true peak ou VU analógico calibrado. **Resetar volumes** restaura faixa em 30%, guitarra em 0% e metrônomo em 30%, preservando os mutes. Um duplo clique no fader restaura apenas o volume daquele canal.

## Gravar
Escolha **Entrada da guitarra** acima do player e pressione **REC**, após Loop. A gravação começa imediatamente, sem diálogo, e captura somente essa entrada. Não inclui automaticamente backing track, metrônomo ou sons da saída. O formato WAV segue o formato de captura WASAPI do dispositivo.

REC não inicia a faixa nem altera monitor, mute ou volumes. Durante a captura, o botão fica ativo e o player mostra **Gravando áudio** e o tempo decorrido. A entrada fica bloqueada até concluir; você pode trocar a saída de reprodução. A captura direta não depende do volume ou mute do monitor. Se o dispositivo fornecer uma mistura na própria entrada, essa mistura fará parte da captura.

Pressione **REC** novamente para encerrar somente a gravação; **Parar** encerra a reprodução e a gravação, inclusive sem faixa selecionada. Após finalizar o cabeçalho WAV, o app adiciona uma nova faixa ao fim da playlist. A seleção e a reprodução atuais são preservadas; se a playlist estava vazia, a gravação é selecionada, sem tocar automaticamente.

Os arquivos recebem data/hora e identificador único em `Recordings`, no diretório de dados da execução; a simulação usa seu próprio diretório. Use **Mostrar arquivo** no menu da faixa para localizar o WAV. Fechar o app finaliza a captura e registra a faixa antes de salvar a playlist. Arquivos sem amostras não geram faixas. Em falhas, preserve o WAV recuperável indicado; ele não é apresentado como gravação concluída. Se somente a inclusão ou persistência da playlist falhar, a mensagem informa que o WAV foi salvo.

## Exportar
A exportação gera WAV PCM de 16 bits. Com A e B válidos, exporta uma passagem do trecho, mesmo se a repetição estiver desligada; sem um trecho válido, exporta a faixa inteira. O intervalo mínimo entre A e B é 0,1 segundo.

No diálogo, **Aplicar velocidade e tom (sem volume/mute)** começa desmarcado. Marque para incluir os ajustes de velocidade e transposição; deixe desmarcado para manter a velocidade e o tom originais. Volume e mute do player não entram na exportação, que também não inclui guitarra, metrônomo ou sons de outros aplicativos. REC captura somente a entrada selecionada.

O aplicativo impede usar como destino um áudio presente na playlist. Outro arquivo existente pode ser substituído após a confirmação no diálogo de salvar; escolha um nome novo para preservá-lo. A substituição só ocorre ao concluir a exportação. Cancelar mantém o destino anterior. Remover uma faixa da lista não apaga o áudio.

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

Preferências, BPM manual e alinhamento por arquivo são lembrados, mas o metrônomo sempre abre parado. Trocar de modo interrompe os cliques. Ao escolher outra saída válida, o app restabelece o metrônomo se estava ligado. Exportar WAV e gravar com REC não incluem automaticamente o metrônomo.

## Sessão e arquivos
A ordem e os caminhos da playlist são salvos em `backing-playlist-v1.json` após gravações concluídas e ao fechar o app. Arquivos ausentes continuam na lista para usar **Localizar novamente**. Seleção, saída e ajustes de reprodução permanecem apenas na sessão; o antigo `backing-tracks-v1.json` não é carregado nem atualizado. As preferências do metrônomo, seu BPM manual e alinhamento por arquivo, além da pasta e dos registros de downloads do YouTube, são guardados separadamente. Os áudios baixados permanecem na pasta escolhida; a lista não é um backup dos arquivos. Consulte [Privacidade](privacidade.md) para armazenamento e serviços externos.
