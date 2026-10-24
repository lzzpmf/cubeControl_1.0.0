# Backing Tracks — Cube Studio

O Cube Studio ajuda a tocar e estudar com um acompanhamento. A área de estudo tem duas faixas fixas: **Acompanhamento** e **Minha guitarra**, cada uma com waveform, volume e silenciar. A busca permanece à direita.

## Escolher uma backing

Use **Importar áudio**, Ctrl+I ou arraste um arquivo de áudio para a timeline. Na busca, digite pelo menos três caracteres ou cole um link. Escolha busca de acompanhamentos ou busca livre. **Baixar** apenas salva o MP3 na pasta escolhida; **Usar backing** reutiliza um download válido ou baixa e carrega o áudio.

Uma backing nova entra inteira desde o início, substitui a anterior, limpa A/B e restaura velocidade de 100% e transposição zero. Havendo tentativas ainda não salvas em áudio, o Studio pede confirmação antes de descartá-las.

Os resultados mostram título, duração, waveform real e estado do download. Permanecer sobre um resultado prepara silenciosamente até 30 segundos de áudio. Antes da análise, aparece um estado neutro. **Ouvir** inicia a prévia por ação explícita e pausa os sons do estudo. Autoria e origem ficam nos detalhes. Áudios baixados podem mostrar a waveform completa.

Busca e download continuam disponíveis durante reprodução e captura. Durante gravação, prévias e substituição da backing ficam bloqueadas. Preparações obsoletas são canceladas. Sair do Studio encerra a prévia, mas mantém o estudo tocando.

Escolha a pasta no primeiro download e use **Alterar pasta** depois. O painel mostra progresso e cancelamento; downloads repetidos preservam arquivos existentes. Saída e volume da prévia ficam nas opções da busca e usam uma saída do Windows, inclusive quando o estudo usa ASIO.

## Tocar e repetir trechos

Clique na régua para posicionar o cursor. Espaço alterna reprodução e pausa. **Parar** encerra reprodução, captura e metrônomo e volta ao início. Voltar e avançar movem cinco segundos.

**Transposição** vai de −12 a +12 semitons, com −/+, e retorno a zero. **Velocidade** usa somente porcentagem, de 25% a 200%, com slider, valor editável e retorno a 100%. Os controles afetam backing e tentativa selecionada, conservando o contexto de cada captura.

Marque o início com **A** e o fim com **B**, ou use Shift+arraste na régua. Arraste os marcadores para ajustar o trecho. **Repetir A/B** ou L liga e desliga a repetição. Shift+roda amplia a timeline; a roda navega quando ampliada. Ctrl+0 com foco na timeline enquadra o estudo; fora dela, restaura o zoom do Cube.

## Metrônomo independente

Ligar/desligar, BPM, **TAP** e volume ficam visíveis. O padrão é desligado, 100 BPM e 4/4; compasso e timbre ficam nas opções. TAP define andamento e fase do clique.

A velocidade da backing nunca muda o BPM do metrônomo; BPM e TAP nunca mudam a velocidade da backing. O clique funciona sem arquivo, com a backing pausada e depois do fim do áudio. Play, pause, seek e loop não reiniciam sua fase. **Parar** desliga tudo. O clique não se alinha automaticamente às batidas de um arquivo.

## Configurar e gravar a guitarra

Abra as opções de áudio no rodapé. Escolha WASAPI para dispositivos do Windows ou ASIO para o driver da interface. No WASAPI, selecione entrada e saída separadamente; no ASIO, escolha o dispositivo duplex. **Painel ASIO** abre as opções do fabricante. Selecione o canal da guitarra, a taxa e o buffer disponíveis e clique em **Aplicar**.

O retorno por software começa desligado a cada execução. Ative-o para ouvir a entrada pelo aplicativo. Se já ouve pelo retorno direto da interface ou pedaleira, mantenha-o desligado para evitar escuta duplicada. Configurações de dispositivo ficam bloqueadas durante captura.

Posicione o cursor e use **Gravar** ou R. Não é necessário armar faixa. Uma tentativa anterior não toca durante uma nova captura. Termine pelo botão de gravação, pausa ou Parar. Sem loop, a timeline cresce; cada passagem do loop gera uma tentativa independente.

Escolha uma tentativa para vê-la e ouvi-la. Cada uma preserva posição, velocidade e transposição da captura, mesmo quando gravada em outro trecho. Arraste as extremidades da waveform da guitarra para aparar ou use o menu de contexto no cursor. Excluir remove da lista; Ctrl+Z desfaz aparar/excluir e Ctrl+Y refaz. O arquivo original não é modificado.

Reprodução, captura e metrônomo continuam ao navegar no Cube. O miniplayer mostra o estado e oferece **Parar** e **Abrir Studio**.

## Salvar áudio e encerrar

**Salvar áudio** guarda somente a tentativa selecionada, já aparada e alinhada. Escolha **Guitarra com backing** ou **Somente guitarra**. O padrão é WAV estéreo PCM 24-bit; MP3 320 kbps é a alternativa. O metrônomo fica excluído. Há progresso/cancelamento, e o arquivo só é publicado após concluir.

Trocar de backing, limpar o estudo ou fechar pede descarte quando há tentativas ainda não exportadas. Aparar uma tentativa exportada exige salvar essa nova versão para evitar o aviso. Mudar tom, velocidade ou A/B não gera pedido de salvamento.

O Studio inicia vazio e não abre, salva nem restaura projetos. Arquivos antigos `.cubestudio.json`, `.ministudio.json`, `.cubeproject` e suas mídias permanecem intocados. Downloads e exportações ficam nas pastas escolhidas. Temporários usam `PracticeRecordings` no diretório de dados do aplicativo, em uma pasta própria por execução, e são limpos após descarte confirmado.

## Resolver problemas

Confira saída, volumes e silenciar nas duas faixas. Para gravar, confira entrada e canal nas opções de áudio. ASIO exige o driver instalado; WASAPI usa os dispositivos disponíveis no Windows.

Falhas de dispositivo ou escrita interrompem a operação e preservam áudio recuperável. Nas opções de áudio, use **Recuperar gravações** e escolha um registro disponível. A recuperação cria uma tentativa a partir de uma cópia do áudio, sem abrir projeto. Confira e use Salvar áudio para guardá-la. Fontes de recuperação permanecem preservadas.

Busca, prévia e download precisam de internet. Confira a mensagem do resultado e a pasta escolhida em caso de falha. Ferramentas ausentes exigem reinstalar o pacote completo. Cancelamento mantém arquivos já concluídos.
