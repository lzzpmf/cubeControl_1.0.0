# IRs, gabinetes e associações

## Escolher um gabinete
O pedal tem oito slots de IR. O conteúdo pode ter sido substituído: não confunda o número do slot com a identidade do gabinete. A biblioteca incluída pode ser usada offline.

## Importar e experimentar
Escolha um WAV de resposta de impulso pela interface e observe a compatibilidade informada. Um WAV de IR não é um arquivo de amp nem uma música para Backing Tracks. A prévia de IR usa temporariamente o slot do pedal e preserva uma referência para recuperação; siga as ações de encerrar/restaurar antes de outras gravações.

Importar para um slot e gravar são operações no aparelho. Exporte ou preserve o backup do conteúdo anterior antes de substituí-lo. Consulte os avisos e o slot indicado na confirmação.

## Montar e aplicar packs
Em **Packs de IRs**, escolha qualquer pack e edite diretamente. A primeira alteração em um predefinido cria uma montagem independente e mostra o aviso amarelo **Criando pack**; o original permanece intacto. Para começar vazio, remova os IRs dos oito slots. Arraste um arquivo da biblioteca para o cartão de destino. Arrastar entre cartões troca os IRs e suas distâncias. A lixeira aparece ao passar o mouse ou focar o cartão; também é possível remover com Delete. O menu de contexto oferece alternativas pelo teclado. Slots vazios preservam o conteúdo atual do pedal. Montar e buscar não iniciam prévia nem gravam no aparelho.

Use **Revisar e aplicar** para escolher quais IRs enviar e ajustar a distância de cada um, de 0 a 99. Esses ajustes valem somente para aquela aplicação: não modificam o pack salvo. O app compara os destinos com o pedal e evita gravar conteúdo idêntico. A confirmação final indica os slots que serão substituídos. Cancelar preserva sua montagem.

As ações **Revisar e aplicar** e **Restaurar IRs originais** ficam acima da grade. **Salvar pack** solicita nome e descrição em uma janela e guarda a montagem para reutilização; não é obrigatório salvar antes de aplicar. Salvar uma montagem baseada em predefinido cria um pack pessoal; salvar alterações em um pessoal atualiza esse pack. Descartar restaura o pack de origem. Ao trocar um pack com alterações, escolha **Salvar**, **Descartar** ou **Cancelar**.

**Exportar** cria um `.cubeirpack.zip` com os WAVs, que pode ser importado em outro computador. **Importar** abre uma montagem para revisão, sem salvá-la nem aplicá-la automaticamente; os WAVs importados ficam nos dados do usuário e aparecem também na biblioteca individual. Arquivos antigos `.cubeirpack.json` continuam aceitos, mas dependem dos WAVs referenciados. Referências ausentes são indicadas nos cartões e não podem ser selecionadas para aplicação.

Os packs binários BFI/BFIN ficam na ação separada **Restaurar IRs originais**, quando os recursos originais estão disponíveis. Consulte [Backups e recuperação](recuperacao.md).

## Associar ao amp
Associar gabinete aplica ao preset editado a associação configurada para o amp. Associação automática começa desligada e fica guardada localmente. Quando ligada, tenta aplicar a associação ao amp atual e às próximas escolhas. Sem associação válida, o gabinete é preservado.

## Identidade e imagens
Nomes e fotos de fabricantes ajudam na escolha, mas não comprovam que um IR reproduza exatamente aquele equipamento. Personalizações visuais não alteram o áudio.

Veja [Backups e recuperação](recuperacao.md).

## Coleções Overdriven.fr

A biblioteca Overdriven.fr reúne 13 coleções e 682 IRs. Use os filtros de coleção,
caixa, falante, microfone, posição e distância de captura junto da busca. Os
filtros se combinam; **Limpar filtros** restaura a biblioteca inteira. **Não
informado** seleciona arquivos sem metadados. Filtrar não envia áudio ao pedal.

A árvore segue coleção → microfone → IR. Abra **Informações de captura** para
consultar o formato original, origem, posição, variantes e nome do arquivo.
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
