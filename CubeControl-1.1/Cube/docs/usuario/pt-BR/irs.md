# IRs, gabinetes e associações

## Escolher um gabinete
O pedal tem oito slots de IR. O conteúdo pode ter sido substituído: não confunda o número do slot com a identidade do gabinete. A biblioteca incluída pode ser usada offline.

## Importar e experimentar
Escolha um WAV de resposta de impulso pela interface e observe a compatibilidade informada. Um WAV de IR não é um arquivo de amp nem uma música para Backing Tracks. A prévia de IR altera a RAM do pedal, sem substituir nenhum dos oito slots gravados. Escolher um WAV externo ou selecionar um IR na biblioteca individual inicia a prévia automaticamente quando o pedal está conectado e a sessão não está bloqueada; sem conexão, apenas prepara o arquivo local. O estado anterior fica em memória nesta execução para restauração; use Encerrar prévia antes de fechar.

Importar para um slot e gravar são operações no aparelho. Exporte ou preserve o backup do conteúdo anterior antes de substituí-lo. Consulte os avisos e o slot indicado na confirmação.

## Pré-escuta com guitarra gravada
Selecione um dos cinco trechos de guitarra e escolha a saída USB de áudio do pedal. É necessário ter conexão MIDI operacional e prévia de IR confirmada em RAM. O computador reproduz o WAV cru; o processamento é feito pelo pedal, com seus efeitos atuais. A lista de saídas é a do Windows: escolher outra saída não aplica o IR por software. O volume começa em 30% e a repetição vem ligada.

Trocar o IR pausa e silencia a pré-escuta durante a transferência; a reprodução só retoma na mesma posição após confirmação. Pausar conserva a posição; parar volta ao início. Trocar de trecho durante a reprodução reinicia com o novo trecho. Sair da seção IRs, trocar de saída, desconectar ou encerrar a prévia interrompe a reprodução. Backing Tracks, prévias do YouTube e pré-escuta de IRs não tocam simultaneamente.

## Montar e aplicar packs
Em **Packs de IRs**, escolha qualquer pack e edite diretamente. A primeira alteração em um predefinido cria uma montagem independente e mostra o aviso amarelo **Criando pack**; o original permanece intacto. Para começar vazio, remova os IRs dos oito slots. Arraste um arquivo da biblioteca para o cartão de destino. Arrastar entre cartões troca os IRs e suas distâncias. A lixeira aparece ao passar o mouse ou focar o cartão; também é possível remover com Delete. O menu de contexto oferece alternativas pelo teclado. Slots vazios preservam o conteúdo atual do pedal. Montar e buscar não iniciam prévia nem gravam no aparelho.

Use **Revisar e aplicar** para escolher quais IRs enviar e ajustar a distância de cada um, de 0 a 99. Esses ajustes valem somente para aquela aplicação: não modificam o pack salvo. O app compara os destinos com o pedal e evita gravar conteúdo idêntico. A confirmação final indica os slots que serão substituídos. Cancelar preserva sua montagem.

As ações **Revisar e aplicar** e **Restaurar IRs originais** ficam acima da grade. **Salvar pack** solicita nome e descrição em uma janela e guarda a montagem para reutilização; não é obrigatório salvar antes de aplicar. Salvar uma montagem baseada em predefinido cria um pack pessoal; salvar alterações em um pessoal atualiza esse pack. Descartar restaura o pack de origem. Ao trocar um pack com alterações, escolha **Salvar**, **Descartar** ou **Cancelar**.

**Exportar** cria um `.cubeirpack.zip` com os WAVs, que pode ser importado em outro computador. **Importar** abre uma montagem para revisão, sem salvá-la nem aplicá-la automaticamente; os WAVs importados ficam nos dados do usuário e aparecem também na biblioteca individual. Arquivos antigos `.cubeirpack.json` continuam aceitos, mas dependem dos WAVs referenciados. Referências ausentes são indicadas nos cartões e não podem ser selecionadas para aplicação.

Os packs binários BFI/BFIN ficam na ação separada **Restaurar IRs originais**, quando os recursos originais estão disponíveis. Consulte [Backups e recuperação](recuperacao.md).

### Limites e aplicação parcial
WAVs individuais e os WAVs de um pack portátil têm limite de 32 MiB por arquivo. O pack valida os hashes e o formato de todos os WAVs antes de copiá-los para a biblioteca importada; arquivos alterados ou inválidos são recusados. Importar um ZIP já pode copiar WAVs para os dados do usuário, mesmo antes de salvar a montagem.

Aplicar um pack grava um slot por vez. Se houver falha, a mensagem lista os slots concluídos; a operação não desfaz automaticamente os slots já gravados. Confira o resultado antes de repetir. As cópias automáticas anteriores permanecem somente na memória da sessão; exporte backups que precisar conservar.

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
não altera o controle **Distance** de 0–100 do pedal. Sufixos como LC45, EQ, P0,
CUT1 e UA610 identificam variantes; não implicam processamento aplicado pelo
app. SP1-PREVIEW indica arquivos de preview, cada um identificado pelo próprio
microfone e nome original.

Os arquivos são mono a 44,1 kHz: 82 IRs GOV são PCM 16 bits/50 ms (o readme
informa float 32 bits); os demais são PCM 24 bits/100 ms. O app converte para
48 kHz e mantém 512 amostras, cerca de 10,7 ms, sem normalizar o ganho. Não
aplica automaticamente cortes, EQ, ganho ou correção de fase. As recomendações
de filtros e ajustes do fornecedor são orientações de escuta.
