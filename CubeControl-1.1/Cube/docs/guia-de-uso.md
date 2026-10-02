# Guia de uso do cubeControl

[Documentação](README.md) · Revisão: 01/10/2026

O cubeControl configura o Cube Baby **de guitarra** pelo USB-MIDI. Não há perfil implementado para Cube Baby Bass, AC, Chocolate ou Blackbox.

## Abrir e conectar

Use um executável já preparado ou os comandos do [guia Windows](../windows/README.md). O inicializador `CubeControl-1.1/Cube/Iniciar-CubeControl.cmd` chama o fluxo único `Build-CubeControl.ps1 -Start`, que gera e verifica o pacote 1.1.0 fora do projeto antes de abri-lo; esse fluxo exige o SDK .NET 10. Para abrir sem reconstruir, execute diretamente o `CubeControl.exe` do pacote.

1. Ligue o pedal e conecte um cabo USB de dados ao computador.
2. Feche outros editores que estejam usando a mesma porta MIDI.
3. Abra o cubeControl. A busca automática tenta localizar entrada/saída MIDI e identificar o aparelho; o botão de conexão permite tentar manualmente.
4. Aguarde a leitura de A/B/C e dos oito slots de IR. Ver os presets na tela não significa que o inventário terminou.

Detalhes da conexão ficam no menu de reticências e na área de ajuda. Desconectar manualmente pausa a reconexão automática; conectar novamente a retoma. O Bluetooth integrado do pedal não é usado para editar parâmetros; veja [Backing Tracks](backing-tracks.md) para reprodução de áudio.

## Preset editado, som ouvido e gravação

| Ação | Resultado |
| --- | --- |
| Selecionar A, B ou C no aplicativo | Escolhe o destino da edição; não aciona o footswitch |
| Ajustar um parâmetro disponível | Envia alterações live por uma fila serializada |
| Salvar/gravar A/B/C | Grava o banco completo dos **três presets**, com conferência por leitura |
| Exportar o banco | Salva um JSON da edição local no computador |
| Importar um banco | Após confirmação, aplica os campos conhecidos ao pedal; ainda exige salvar para persistir |
| Reler/reconectar | Obtém o estado do pedal; observe os avisos sobre substituição e backups da edição |
| Desfazer | Reverte ajustes do histórico de presets; não desfaz gravações de setores de IR/amp |

Para ouvir o preset que está editando, selecione o correspondente no pedal. As prévias experimentais de chorus e phaser conferem o contexto físico, mas não há acompanhamento geral dos footswitches fora delas. A leitura do banco pode refletir alterações live; leitura de retorno não equivale a comprovação de retenção após desligar.

## Pedaleira e gabinetes

A cadeia visual contém Amp, Chorus, Phaser, Delay, Reverb e Cabinet. Clicar em um card abre seu editor; o interruptor controla a função correspondente.

- Chorus e Phaser compartilham um parâmetro de modulação e não ficam ativos simultaneamente. Ajustes preparados de um efeito desligado podem permanecer locais até a ativação.
- O switch Tone do Amp não representa bypass geral do amplificador.
- Tempo de delay é um índice do pedal, sem conversão comprovada para milissegundos; não interprete todos os números como porcentagem, Hz ou dB.
- Imagens de amplificador/gabinete abrem seletores. Aplicar ou dar clique duplo confirma a escolha; cancelar preserva a seleção anterior.
- Nomes comerciais e fotos são referências de apresentação. O conteúdo de um slot pode ter sido substituído; veja o [catálogo](ampsEirs.md).

**Associar gabinete** aplica ao preset editado a associação configurada para o amp. **Associação automática** começa desligada e é persistida localmente; quando ligada, tenta aplicar a associação ao amp atual e nas escolhas seguintes. Sem associação válida, preserva o gabinete. A seção IR mantém seu estado ligado/desligado. Salvar A/B/C continua sendo necessário para persistir a seleção no pedal.

## Edição gráfica dos efeitos experimentais

Os criadores de Chorus, Phaser, Delay e Reverb usam alças nas curvas, envelopes e superfícies. Selecione um elemento para ver nome e valor; arraste seu contorno ou pressione Enter para digitar. Tab percorre os elementos, setas ajustam e Escape cancela o arraste atual. O menu contextual oferece formas de onda, seções e congelamento da modulação do Delay. A prévia continua automática na RAM quando ativa; **Aplicar ajustes** não salva no pedal. Veja o [contrato completo dos parâmetros e gestos](editores-graficos-de-efeitos.md).

## Criador de chorus experimental

No card **Chorus**, abra **Criador de chorus · experimental**. Ative o chorus no preset físico, em modo PRESET, selecione o mesmo A/B/C no aplicativo e use **Ler chorus ativo**. O editor oferece profundidade, velocidade, mistura, realimentação, forma de onda e dois ganhos. A validação sonora permanece pendente; a mistura em 100% mantém a configuração original e não significa somente efeito.

Os ajustes são enviados à RAM e conferidos por leitura. Use **Restaurar chorus** para voltar à referência. Enquanto a prévia estiver ativa, a edição comum dos presets e as prévias de phaser, amp e IR ficam bloqueadas. Desconectar, reler ou fechar normalmente também tenta restaurar. Trocas físicas e recargas externas interrompem a prévia.

Arquivos `.chorus.json` ficam no computador: importar prepara a edição; **Aplicar ajustes** envia à prévia ativa; exportar salva os sete parâmetros. Esses arquivos não são gravados na flash pelo editor. Se uma prévia for interrompida, o aplicativo preserva o backup e pede restauração ao reconectar, sem reaplicar automaticamente. Veja [limites, evidências e recuperação](../../../engenharia-reversa/cube-baby/CUBE_BABY_CHORUS.md).

## Criador de reverb experimental

No card **Reverb**, abra **Criador de reverb · experimental**. Ative reverb e seção reverb/cabinet, desligue a modulação e selecione o mesmo preset físico A/B/C em modo PRESET. Use **Ler reverb ativo** para iniciar a prévia.

O editor separa sinal direto, sinal reverberado, realimentação, retorno das linhas e amortecimento. O controle original **Quantidade** combina realimentação e ganho reverberado. As unidades do criador não representam tempo de cauda ou frequência medidos; a validação sonora ainda está pendente.

Use **Restaurar reverb** antes de voltar à edição comum ou a outras prévias. Arquivos `.reverb.json` ficam no computador: importar prepara os ajustes e **Aplicar ajustes** envia à prévia ativa. Desligar/religar o efeito perde a personalização. Desconexão, releitura e fechamento normais tentam restaurar; interrupções preservam backup para recuperação explícita, sem reaplicar automaticamente. Não há gravação desses controles na flash. Veja [evidências, limites e recuperação](../../../engenharia-reversa/cube-baby/CUBE_BABY_REVERB.md).

## IRs: biblioteca, prévia e gravação

A biblioteca incluída contém 276 WAVs e funciona offline. Também é possível selecionar um WAV externo de até 32 MiB. O preparo aceita PCM 16/24/32 bits e float32, mono ou estéreo; converte para mono/48 kHz e usa 512 amostras, truncando ou preenchendo quando necessário.

1. Escolha um arquivo ou item da biblioteca. **Com o pedal conectado e disponível, essa seleção inicia a prévia automaticamente.**
2. Ajuste a distância do microfone; concluir o gesto aplica a prévia. A escala é 0–100%, sem correspondência documentada em centímetros.
3. Para conservar o timbre, escolha o slot de destino 1–8 e use a ação de gravação. A prévia sozinha não substitui um slot.
4. Confira o destino na confirmação. O fluxo preserva o setor anterior em backup antes de apagar, grava e compara o setor completo.
5. Ao terminar de experimentar, use **Encerrar prévia** para restaurar a RAM anterior. Depois de gravar, pode ser necessário selecionar outro gabinete no pedal e voltar para recarregar o slot.

O desenho da onda pode ampliar a visualização sem normalizar o áudio. Os [detalhes de formato e recuperação](cubecontrol-ir-e-fabrica.md) distinguem o bloco RAM do setor flash.

Uma prévia de IR interrompida mantém `pending-ir-preview.json` na pasta de dados. Reconecte o aparelho correspondente e encerre a prévia antes de remover esse arquivo. Uma desconexão durante gravação flash exige conferir o resultado e usar o backup quando necessário.

## Criador de delay experimental

Abra **Delay → Criador de delay · experimental**. Ative a seção delay/modulação, desligue chorus/phaser, selecione no aplicativo o mesmo A/B/C físico e use **Ler delay ativo**. O criador oferece profundidade e velocidade de modulação temporal, tempo em amostras internas e resolução fina de feedback/mistura.

Profundidade e velocidade são percentuais do original. Profundidade zero elimina a oscilação temporal; velocidade zero congela sua fase. A velocidade efetiva depende do tempo. Mistura interna 50 mantém direto e eco integrais; não é uma soma complementar de porcentagens. A conversão do tempo para milissegundos continua sem medição por áudio.

A prévia altera somente RAM e bloqueia edições concorrentes de presets, amps, IRs e outros criadores. **Restaurar delay**, releitura, desconexão e fechamento normal tentam restaurar e conferir a referência. Em falha, preserve `pending-delay-preview.json` e os backups de `DelayRecovery/`; após recuperar, releia ou reconecte antes de iniciar outra prévia.

Arquivos `.delay.json` guardam a edição no computador. Importar não envia ao pedal; **Aplicar ajustes** envia à prévia ativa. Não há salvamento em flash nem prévia física na simulação. A avaliação sonora permanece pendente. Veja o [relatório e as evidências](../../../engenharia-reversa/cube-baby/CUBE_BABY_DELAY.md).

## Criador de amps e Backing Tracks

O [Criador de amps](criador-de-amps.md) edita um setor de modelo de amp, separado dos presets A/B/C e dos IRs. Carregar/importar pela interface pode iniciar uma prévia em RAM. Há limitações conhecidas no carregamento de três modelos originais.

[Backing Tracks](backing-tracks.md) reproduz até três arquivos MP3/WAV pelo áudio do Windows, com seleção de saída, trechos A/B e exportação WAV. Sua reprodução independe da conexão USB de edição.

## Aparência e arquivos locais

Tema, idioma e zoom ficam no menu de reticências. Zoom: 50–200%, em passos de 10%, com Ctrl +, Ctrl − e Ctrl 0. Essas preferências não alteram o áudio do pedal.

Backups, imagens, associações e preferências ficam em `%LOCALAPPDATA%\CubeControl`. A simulação usa outra pasta para os dados do editor, mas compartilha tema, idioma e zoom. Consulte [armazenamento e formatos](arquitetura.md) e [privacidade](POLITICA-DE-PRIVACIDADE.md).

## Problemas comuns

| Sintoma | Verificação |
| --- | --- |
| Porta não encontrada | Alimentação, cabo de dados, enumeração de entrada/saída e outro editor ocupando a porta |
| Interface muda, mas o preset ouvido não | Selecione A/B/C fisicamente; a interface indica o preset em edição |
| Banco mudou no aparelho | Preserve/exporte a edição e releia antes de salvar; o aplicativo detectou divergência |
| Edição bloqueada durante prévia | Encerre/restaure a prévia correspondente antes de voltar à edição de presets |
| IR aparece sem nome/foto de fábrica | Identificação depende do conteúdo reconhecido, não apenas do número do slot |
| Recurso de fábrica indisponível | É necessária a versão original do CubeSuite reconhecida pelos hashes do carregador |
| Amp original recusado por ganho fora da faixa | Limitação atual do validador nos TYPEs 6, 7 e 8; veja [Criador de amps](criador-de-amps.md) |
| Inicializador não encontra o script/executável | Preserve a estrutura do checkout, confirme o SDK .NET 10 e verifique o erro de build no [guia Windows](../windows/README.md) |

A ajuda física do pedal está no [manual PDF incluído](../windows/Cube.Desktop/Assets/Cube-Baby-M-Vave-Guitar-Pedal-User-Manual.pdf). O estado atual do dispositivo não pode ser deduzido de capturas históricas.
