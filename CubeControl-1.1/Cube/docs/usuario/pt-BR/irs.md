# IRs, gabinetes e associações

## Escolher um gabinete
O pedal tem oito slots de IR. O conteúdo pode ter sido substituído: não confunda o número do slot com a identidade do gabinete. A biblioteca incluída pode ser usada offline.

## Biblioteca compartilhada
**Gerenciar IRs** oferece as abas **Análise e prévia individual** e **Packs de IRs (8 slots)**, com a mesma biblioteca, busca e seleção. A aba individual abre inicialmente; a escolha fica preservada durante a sessão. Em Packs, selecionar um arquivo apenas o disponibiliza para montar os slots. Trocar de aba não aplica IRs nem descarta a montagem; uma prévia já ativa permanece no pedal. Volte à aba individual para encerrá-la.

## Importar e experimentar
Escolha um WAV de resposta de impulso pela interface e observe a compatibilidade informada. Um WAV de IR não é um arquivo de amp nem uma música para Backing Tracks. A prévia de IR altera a RAM do pedal, sem substituir nenhum dos oito slots gravados. Escolher um WAV externo ou selecionar um IR na biblioteca individual inicia a prévia automaticamente quando o pedal está conectado e a sessão não está bloqueada; sem conexão, apenas prepara o arquivo local. O estado anterior fica em memória nesta execução para restauração; use Encerrar prévia antes de fechar.

Importar para um slot e gravar são operações no aparelho. Exporte ou preserve o backup do conteúdo anterior antes de substituí-lo. Consulte os avisos e o slot indicado na confirmação.

## Pré-escuta com guitarra gravada
Selecione um dos cinco trechos de guitarra e escolha a saída USB de áudio do pedal. É necessário ter conexão MIDI operacional; não é necessário carregar um IR. Sem prévia de IR, você ouve o preset atual. A pré-escuta acompanha as alterações aplicadas à cadeia, inclusive as prévias do Criador de amps; edições que ficaram apenas no computador ainda não alteram o som. Uma prévia de IR interrompida precisa ser confirmada ou restaurada antes de iniciar. O computador reproduz o WAV cru; o processamento é feito pelo pedal, com seus efeitos atuais. A lista de saídas é a do Windows: escolher outra saída não aplica o IR por software. O volume começa em 30% e a repetição vem ligada.

Trocar o IR pausa e silencia a pré-escuta durante a transferência; a reprodução só retoma na mesma posição após confirmação. Pausar conserva a posição; parar volta ao início. Trocar de trecho durante a reprodução reinicia com o novo trecho. A reprodução continua ao navegar para a Pedaleira ou o Criador de amps, com controles compactos para pausar e parar. Encerrar a prévia de IR pausa durante a restauração e retoma o preset restaurado após confirmação. Trocar de saída, desconectar ou fechar o app interrompe a reprodução. Backing Tracks, prévias do YouTube e pré-escuta de IRs não tocam simultaneamente.

## Montar e aplicar packs
O catálogo inclui **34 packs predefinidos**, agrupados por marca, estilo e guitarra, com oito slots e distância inicial de 80%. As descrições são sugestões de uso, não simulações de guitarra ou homologação por audição. Packs pessoais aparecem em **Meus packs**.

Em **Packs de IRs**, escolha qualquer pack e edite diretamente. A primeira alteração em um predefinido cria uma montagem independente e mostra o aviso amarelo **Criando pack**; o original permanece intacto. Para começar vazio, remova os IRs dos oito slots. Arraste um arquivo da biblioteca para o cartão de destino. Arrastar entre cartões troca os IRs e suas distâncias. A lixeira aparece ao passar o mouse ou focar o cartão; também é possível remover com Delete. O menu de contexto oferece alternativas pelo teclado. Slots vazios preservam o conteúdo atual do pedal. Montar e buscar não iniciam prévia nem gravam no aparelho.

Ajuste o **Volume** diretamente no knob de cada card, de 0 a 99% (distância no protocolo). O valor aparece no tooltip e acompanha a montagem ao salvar o pack. Clique em **Aplicar** para enviar os slots preenchidos: aparece apenas a confirmação da ação, com **OK** ou **Sair**. O app compara os destinos, preserva um backup e evita gravar conteúdo idêntico. Slots vazios permanecem intactos; corrija ou remova referências ausentes antes de aplicar.

As ações **Aplicar** e **Restaurar IRs originais** ficam abaixo da grade. **Salvar pack** solicita nome e descrição em uma janela e guarda a montagem para reutilização; não é obrigatório salvar antes de aplicar. Salvar uma montagem baseada em predefinido cria um pack pessoal; salvar alterações em um pessoal atualiza esse pack. Descartar restaura o pack de origem. Ao trocar um pack com alterações, escolha **Salvar**, **Descartar** ou **Cancelar**.

**Exportar** cria um `.cubeirpack.zip` com os WAVs, que pode ser importado em outro computador. **Importar** abre uma montagem para revisão, sem salvá-la nem aplicá-la automaticamente; os WAVs importados ficam nos dados do usuário e aparecem também na biblioteca individual. Arquivos antigos `.cubeirpack.json` continuam aceitos, mas dependem dos WAVs referenciados. Referências ausentes são indicadas nos cartões e não podem ser selecionadas para aplicação.

Os packs binários BFI/BFIN ficam na ação separada **Restaurar IRs originais**, quando os recursos originais estão disponíveis. Consulte [Backups e recuperação](recuperacao.md).

O pack BFIN reconhecido pelo conteúdo usa as seguintes referências, com nomes, imagens e descrições correspondentes. A opção **Desligado** é separada dos oito IRs; na tabela de origem ela ocupa a primeira linha, antes do Line 6 Vetta.

| Slot de IR | Referência BFIN | Tipo de caixa |
|---|---|---|
| 1 | Line 6 Vetta | 2×12 |
| 2 | Marshall 1960AV | 4×12 |
| 3 | Marshall 1960A T75 LA610 | 4×12 |
| 4 | VHT Deliverance | 2×12 |
| 5 | Soldano | 2×12 |
| 6 | Peavey 5150 + Mesa Oversized | 4×12 |
| 7 | JSX KT77 + Mesa Stiletto AKG C1000 | 4×12 |
| 8 | Diezel V30 SM57 (Soldano HR PowerAmp) | 4×12 |

As referências acompanham o conteúdo reconhecido mesmo quando ele é movido para outro slot. Nomes e imagens personalizados continuam disponíveis.

### Limites e aplicação parcial
WAVs individuais e os WAVs de um pack portátil têm limite de 32 MiB por arquivo. O pack valida os hashes e o formato de todos os WAVs antes de copiá-los para a biblioteca importada; arquivos alterados ou inválidos são recusados. Importar um ZIP já pode copiar WAVs para os dados do usuário, mesmo antes de salvar a montagem.

Aplicar um pack grava um slot por vez. Se houver falha, a mensagem lista os slots concluídos; a operação não desfaz automaticamente os slots já gravados. Confira o resultado antes de repetir. As cópias automáticas anteriores permanecem somente na memória da sessão; exporte backups que precisar conservar.

## Sortear um IR no gabinete
Na cadeia de efeitos, em **Gabinete**, **Sortear e aplicar** escolhe um arquivo de toda a biblioteca, incluindo importados e ignorando filtros, e um volume inteiro entre 66% e 85% (parâmetro chamado distância no protocolo). O destino é o gabinete do preset atual, não o slot escolhido separadamente na página IRs. A operação grava diretamente, sem confirmação adicional, e aplica em RAM para ouvir imediatamente; não salva o preset nem altera o estado ligado/desligado do IR. Repetições são possíveis.

É necessário estar conectado, com banco carregado, slot válido e biblioteca disponível. O arquivo é validado antes da gravação. Se a gravação concluir, mas a aplicação imediata falhar, a mensagem informa essa diferença: o slot já foi substituído. As cópias anteriores ficam apenas na memória da sessão.

## Associar ao amp
Associar gabinete aplica ao preset editado a associação configurada para o amp. As associações e a opção de associação automática ficam apenas na sessão atual; ao reabrir, a opção começa desligada e as associações precisam ser configuradas novamente. Salvar associações não envia comandos ao pedal. Quando ligada, tenta aplicar a associação ao amp atual e às próximas escolhas. Sem associação válida, o gabinete é preservado.

## Identidade e imagens
Nomes e fotos de fabricantes ajudam na escolha, mas não comprovam que um IR reproduza exatamente aquele equipamento. Personalizações visuais não alteram o áudio.

Veja [Backups e recuperação](recuperacao.md).

## Coleções Overdriven.fr

A biblioteca Overdriven.fr reúne 13 coleções e 682 IRs. A busca combina todos os termos digitados, sem distinguir maiúsculas de minúsculas, no caminho, nome e metadados do arquivo. Apague o texto para voltar à biblioteca inteira. Não há seletores separados por coleção, microfone ou distância. Buscar não envia áudio ao pedal; selecionar um IR na biblioteca individual pode iniciar a prévia.

Os arquivos com metadados seguem a árvore marca → configuração de falantes → modelo e falante → captação. Overdriven.fr identifica a origem, não uma marca de gabinete. Arquivos sem metadados seguem suas pastas; os importados ficam em **IRs importados**. Ao expandir uma marca, as demais são recolhidas. Consulte os detalhes da seleção para formato original, origem, identificação, evidências, posição, variantes e nome do arquivo.
Modelos de microfone apresentados como “inspirado em” vêm da documentação do
fornecedor. Códigos sem equivalência documentada são preservados.

Na referência visual do fornecedor, P00 é o centro da calota, P05 fica sobre a
calota, P09 próximo da borda da calota, P10 na borda, P20 no cone e P30 próximo da
borda externa. Outros códigos, como P12 e P25, não têm localização exata
comprovada nessa imagem. A foto original não é incluída no app.

A distância no nome é aproximada e em **milímetros**. Ela descreve a captura e
não altera o controle **Distance** de 0–99 do aplicativo. Sufixos como LC45, EQ, P0,
CUT1 e UA610 identificam variantes; não implicam processamento aplicado pelo
app. SP1-PREVIEW indica arquivos de preview, cada um identificado pelo próprio
microfone e nome original.

Os arquivos são mono a 44,1 kHz: 82 IRs GOV são PCM 16 bits/50 ms (o readme
informa float 32 bits); os demais são PCM 24 bits/100 ms. O app converte para
48 kHz e mantém 512 amostras, cerca de 10,7 ms, sem normalizar o ganho. Não
aplica automaticamente cortes, EQ, ganho ou correção de fase. As recomendações
de filtros e ajustes do fornecedor são orientações de escuta.
