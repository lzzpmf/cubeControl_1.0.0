# M-VAVE Cube Baby: Manual do usuário

Edição revisada e ampliada para usuários · Português (Brasil) · 8 de outubro de 2026

[English (US)](../en-US/Cube-Baby-M-Vave-Guitar-Pedal-User-Manual.md) · [PDF original](../../oficial/en-US/Cube-Baby-M-Vave-Guitar-Pedal-User-Manual-Oficial.pdf)

Este guia trata da **Cube Baby de guitarra** e do cubeControl para Windows. Reúne o manual do fabricante incluído no projeto, orientações práticas e o comportamento do aplicativo implementado nesta árvore. É uma revisão independente, não um novo manual oficial do fabricante. Cube Baby Bass, AC, Chocolate e Blackbox estão fora deste guia.

## Sumário

- [Primeiros passos](#primeiros-passos)
- [Alimentação e conexões](#alimentação-e-conexões)
- [Controles e efeitos](#controles-e-efeitos)
- [Modos, footswitches e salvamento](#modos-footswitches-e-salvamento)
- [Afinador e Bluetooth](#afinador-e-bluetooth)
- [Construindo timbres úteis](#construindo-timbres-úteis)
- [Usando o cubeControl](#usando-o-cubecontrol)
- [IRs, prévias e backups](#irs-prévias-e-backups)
- [Criadores avançados](#criadores-avançados)
- [Tabelas de referência](#tabelas-de-referência)
- [Solução de problemas](#solução-de-problemas)
- [Especificações e cuidados](#especificações-e-cuidados)
- [Leitura complementar e verificação](#leitura-complementar-e-verificação)

## Primeiros passos

1. Abaixe o volume do amplificador/caixa que receberá o sinal. Comece com volume baixo também nos fones.
2. Conecte a guitarra em **IN** com um cabo de instrumento mono. Conecte **OUT** ao amplificador/interface ou use a saída dedicada de fones.
3. Ligue o pedal. **PRESET** é o modo padrão de execução descrito no manual original.
4. Pressione A, B ou C para escolher um timbre salvo. Aumente o volume de escuta aos poucos enquanto toca.
5. Para editar no cubeControl, conecte um cabo USB **de dados** e selecione o mesmo A/B/C fisicamente no pedal e no aplicativo.
6. Exporte o banco antes de alterações importantes. Salve os ajustes comuns dos presets no pedal quando quiser mantê-los após desligar.

Um preset recupera configurações; não reorganiza a ordem de processamento. Chorus e Phaser compartilham um bloco de modulação e não funcionam juntos.

## Alimentação e conexões

### Bateria e carregamento

O manual original informa bateria interna de **780 mAh**, autonomia de até **6 horas** e cerca de **2 horas** para carga completa com adaptador de **5 V / 2 A**. São valores de referência, não resultados garantidos para uma bateria envelhecida.

Use uma fonte USB adequada de 5 V ou um power bank. USB também conecta ao computador. Um cabo que carrega corretamente pode não ter condutores de dados. O pedal possui indicador de carga; o manual incluído não fornece uma tabela completa de cores e estados.

### Conexões do painel traseiro

| Porta | Finalidade |
| --- | --- |
| IN | Entrada da guitarra: P10 (6,35 mm), mono TS. |
| Chave de alimentação | Liga/desliga o pedal. |
| USB | Carregamento, conexão ao computador, edição de presets e transferência de IRs. |
| Saída de gravação para celular | Gravação com celular e cabo compatíveis. |
| Saída de fones | Conexão para headphones. |
| OUT | Saída de áudio: P10 (6,35 mm), mono TS. |

### Escolha onde ouvir

| Configuração | Ponto de partida |
| --- | --- |
| Fones | Use a saída de fones. Comece com IR ligado e volume baixo. |
| Interface, mesa ou caixa de resposta ampla | Use OUT em entrada compatível. Comece com IR ligado; ajuste o ganho receptor para evitar saturação. |
| Entrada frontal do amp de guitarra | Use OUT, canal limpo e pouco ganho no amp. Compare IR OFF com IR ligado; amp e falante também modificam o som. |
| Retorno de efeitos do amp | Se o amp oferecer essa conexão, comece com volume receptor baixo e compare IR OFF. Siga as instruções de conexão do amplificador. |
| Gravação no celular | Use a saída de gravação e cabo/adaptador compatível; teste a entrada do aplicativo de gravação. |
| Gravação no computador | O manual original descreve funcionamento como placa de som USB. Confira se o pedal aparece como dispositivo de áudio no sistema e no programa de gravação. Edição USB-MIDI, sozinha, não confirma disponibilidade de áudio USB. |

As portas de gravação e de fones têm finalidades diferentes. O adaptador USB-C/Lightning do celular precisa aceitar a entrada de áudio necessária; um adaptador apenas para fones pode não funcionar. O manual não especifica todas as pinagens, drivers ou compatibilidades com celulares. Não presuma que qualquer cabo ou adaptador funcionará.

Um **IR** (resposta ao impulso) modifica o som como uma resposta capturada de gabinete/microfone. Não é um modelo de amp. Enviar som com IR para um falante de guitarra acrescenta outra coloração de caixa; escolha ouvindo a sua configuração real.

## Controles e efeitos

A ordem de processamento documentada é:

**Afinador → Pré-amplificador → Chorus ou Phaser → Delay → Reverb → IR CAB**

Você escolhe efeitos e ajustes, não uma cadeia com ordem livre.

| Controle | O que faz |
| --- | --- |
| TYPE | Seleciona um dos nove modelos de pré-amplificador. Compare com volume de escuta semelhante. |
| GAIN | Altera a saturação do pré-amplificador. Mais ganho não significa apenas mais volume de saída. |
| TONE | Ajusta o brilho. Reduza se o timbre ficar áspero. |
| MOD | Metade esquerda: Chorus; centro: modulação desligada; metade direita: Phaser. |
| TIME | Define o intervalo entre repetições do delay, não a quantidade de repetições. |
| FB | Define a realimentação do delay: valores maiores prolongam as repetições. |
| MIX | Ajusta a contribuição do delay em relação ao sinal direto. Totalmente à esquerda remove o delay. |
| REVERB | Ajusta a quantidade de reverberação. Totalmente à esquerda remove o reverb. O controle comum do preset combina nível reverberado e realimentação. |
| IR CAB | Seleciona OFF ou um dos oito IRs armazenados: nove posições ao todo. |
| VOLUME | Controla o nível. Confira após mudar modo, amp, ganho ou gabinete. |

**TIME e FB:** TIME afasta ou aproxima os ecos; FB faz com que continuem por mais tempo. Aumente MIX apenas o suficiente para ouvir os ecos sem perder a definição das notas. O TIME comum do aplicativo usa a escala própria do pedal; não é uma leitura documentada em milissegundos ou BPM.

**MOD:** Chorus acrescenta movimento e espessura; Phaser cria uma varredura móvel no espectro. A escala original do software é **0–6 Chorus, 7–8 OFF, 9–15 Phaser**. São valores de seleção, não porcentagens.

**REVERB:** Um pouco acrescenta espaço. Uma ambiência forte pode esconder frases rápidas, principalmente com delay também ativo.

## Modos, footswitches e salvamento

### Consulta rápida

“Toque” significa pressionar brevemente os dois switches juntos e soltá-los. “Segure” significa manter pressionado por aproximadamente 2 segundos.

| Gesto / contexto | Ação documentada |
| --- | --- |
| A, B ou C em PRESET | Seleciona o preset salvo correspondente. |
| B+C, toque | Alternância LIVE/PRESET. O texto descreve explicitamente a entrada em LIVE; o painel identifica o par como LIVE/PRESET. |
| A+B, toque | Entra em EDIT, segundo o manual incluído. |
| B+C, segurar por 2 segundos | Entra no afinador. |
| A+B, segurar por 2 segundos | Ativa o Bluetooth. |
| A, B ou C, segurar em LIVE | Salva os ajustes atuais nesse destino. O exemplo especifica A por 2 segundos. |

Um toque e uma pressão longa no mesmo par têm funções diferentes. O manual incluído não explica completamente como sair do afinador, desativar Bluetooth, sair de EDIT ou qual gesto exato ativa bypass. Esses detalhes não são apresentados aqui como instruções verificadas.

### PRESET

Use A/B/C para recuperar os três timbres armazenados. O manual identifica **todos os indicadores de preset apagados** como bypass nesse modo. Não aplique essa interpretação aos indicadores de LIVE ou do afinador.

Os valores salvos podem diferir das posições físicas dos knobs. Ouça ao mover um knob, em vez de presumir que sua posição já corresponde ao preset recuperado.

### LIVE

LIVE serve para construir um timbre diretamente nos knobs. Os grupos de footswitches do painel são:

| Footswitch | Grupo do painel |
| --- | --- |
| A | IR CAB / REVERB |
| B | DELAY / MOD |
| C | TONE / AMP |

São controles de grupos, não seis interruptores independentes de efeitos. Em particular, a seção Tone do cubeControl não equivale a bypass geral do amp.

O manual afirma que o volume final é ajustado para **80% em LIVE**, mas seu exemplo também orienta ajustar VOLUME. Essa interação não é totalmente explicada. Comece com volume baixo e confira a escuta; 80% não é um nível de saída medido e garantido.

### Monte e salve um timbre no pedal

1. Entre em LIVE com um toque breve em B+C.
2. Coloque MOD no centro e MIX e REVERB totalmente à esquerda para começar com som mais simples. Escolha IR OFF ou um gabinete adequado à saída.
3. Ative os grupos necessários do painel, escolha TYPE e ajuste GAIN/TONE enquanto toca. Comece com VOLUME baixo.
4. Acrescente modulação, delay e reverb aos poucos, comparando com volume semelhante.
5. Segure o destino A, B ou C para salvar, conforme a descrição de LIVE no manual. O exemplo usa A por 2 segundos. Isso substitui os ajustes do preset de destino.
6. Volte a PRESET usando a combinação B+C LIVE/PRESET indicada no painel e recupere o destino para conferir.

### EDIT

O manual descreve EDIT como um modo que mantém valores anteriores dos parâmetros, com indicadores dos knobs mostrando o estado dos efeitos. Orienta voltar a PRESET para ajustar volume, mas não fornece uma sequência completa de saída/salvamento em EDIT. Use LIVE acima para um fluxo físico documentado de salvamento ou a gravação explícita de banco do cubeControl para editar no computador.

## Afinador e Bluetooth

### Afinador

Segure B+C por 2 segundos. Toque uma corda solta por vez e espere a nota estabilizar. O painel mostra a afinação padrão, da mais grave à mais aguda:

| Corda | Nota | Indicador do knob identificado |
| --- | --- | --- |
| 6 | E (Mi) | IR CAB |
| 5 | A (Lá) | REVERB |
| 4 | D (Ré) | MIX |
| 3 | G (Sol) | FB |
| 2 | B (Si) | TIME |
| 1 | E (Mi) | MOD |

| Indicador | Significado |
| --- | --- |
| >, acima de TONE | Nota abaixo da afinação: aumente a altura. |
| ■, acima de GAIN | Nota afinada. |
| <, acima de TYPE | Nota acima da afinação: diminua a altura. |

Nesse modo, os LEDs indicam afinação, não valores dos efeitos. O manual não estabelece suporte a afinações alternativas, silenciamento de saída ou um gesto de saída do afinador.

### Acompanhamento por Bluetooth

1. Segure A+B por 2 segundos para ativar Bluetooth.
2. Abra as configurações Bluetooth do celular e conecte-se ao pedal quando disponível.
3. Comece a música com volume baixo e equilibre a reprodução com a guitarra.

A **luz BT azul piscando** indica ativo sem conexão; **azul contínuo** indica conectado, segundo o manual. Bluetooth reproduz áudio do celular pelo pedal. O cubeControl edita por USB-MIDI. Reprodução Bluetooth não é uma forma documentada de gravar guitarra sem fio.

## Construindo timbres úteis

São pontos de partida para escuta, não receitas calibradas de knobs nem novos presets de fábrica.

| Objetivo | Ponto de partida | Ajuste fino |
| --- | --- | --- |
| Clean | TYPE 1 ou 2, GAIN moderado, MOD desligado, pouco REVERB, IR adequado. | Acrescente um pouco de Chorus se desejar; mantenha MIX baixo para preservar o ataque dos acordes. |
| Crunch | Experimente um modelo de overdrive, aumente GAIN aos poucos, modere delay/reverb. | Explore volume da guitarra e força da palhetada; reduza TONE se ficar áspero. |
| Solo | Escolha um modelo com saturação, acrescente delay moderado e um pouco de reverb. | Aumente FB com cuidado; compare com o preset de base antes de escolher o aumento de volume do solo. |

Ajuste nesta ordem: **configuração de escuta → TYPE → GAIN → TONE → IR → modulação → delay → reverb → volume final**. Mude uma coisa por vez. Ganho excessivo reduz a separação das notas; FB/reverb excessivos escondem o fraseado. Trocar o IR pode alterar timbre e volume percebido.

Compare A/B/C na mesma saída e no volume de escuta pretendido. Exporte um banco de que goste antes de continuar experimentando.

## Usando o cubeControl

### Conecte e selecione o preset ouvido

1. Ligue a Cube Baby de guitarra e conecte um cabo USB de dados ao Windows.
2. Feche outros editores que estejam usando suas portas MIDI.
3. Abra o cubeControl e espere identificação, leitura de A/B/C e inventário dos oito slots de IR. Use o botão de conexão para tentar novamente se necessário.
4. Selecione A/B/C fisicamente no pedal e depois escolha a mesma letra no aplicativo.

**Selecionar A/B/C no aplicativo escolhe o destino da edição. Não aciona o footswitch nem garante qual preset você ouve.** O fluxo atual de conexão não inicia monitoramento geral dos footswitches. Siga as orientações adicionais de contexto físico dos criadores avançados.

### Edite, salve, importe e exporte

| Ação | Resultado |
| --- | --- |
| Alterar um controle comum disponível do preset | Envia alterações live pela fila de comunicação; salvar é separado. |
| Salvar / Gravar A/B/C | Salva o banco completo dos três presets e confere por leitura de retorno. Revise A, B e C antes. |
| Exportar banco | Grava o banco local em JSON no computador; não salva no pedal. |
| Importar banco | Após confirmação, aplica os campos conhecidos ao vivo. Salve A/B/C depois para persistir. |
| Reler / reconectar | Obtém o estado atual do pedal; confira avisos antes de substituir trabalho local. |
| Desfazer | Reverte histórico comum de presets, não gravações de slots de IR/amp. |

Uma leitura de retorno confirma dados transferidos, não um novo teste após desligar. Para conferir retenção, conclua operações, restaure prévias temporárias, salve e recupere os presets após uma reinicialização normal.

**Guarde um backup em arquivo:** exporte o banco preferido antes de importar ou substituir. Nesta árvore, cópias automáticas de recuperação de banco e IR ficam na memória da sessão do aplicativo. Não substituem arquivos exportados nem garantem recuperação após fechar ou ocorrer uma falha.

### Outros recursos úteis

A associação de gabinete pode selecionar um gabinete vinculado a um amp; a associação automática começa desligada. Salve A/B/C para persistir uma seleção de gabinete alterada no preset.

Backing Tracks reproduz pela saída de áudio escolhida no Windows, independentemente da edição USB. Escolha a saída real de escuta e equilibre seu volume. A associação de preset de uma faixa é um lembrete; troque fisicamente no footswitch.

Tema, idioma e zoom ficam no menu de opções. O zoom permite 50–200% e Ctrl +, Ctrl -, Ctrl 0. Preferências visuais não alteram o som.

## IRs, prévias e backups

### Entenda os destinos

| Destino | Significado |
| --- | --- |
| Arquivo local | WAV, banco exportado ou arquivo de criador no computador. Guardá-lo, sozinho, não altera o pedal. |
| Prévia em RAM | Som temporário no pedal conectado. Restaure/encerre para voltar à referência capturada. |
| Slot armazenado / banco de presets | Gravação persistente explícita. Salvar A/B/C e gravar slots de IR/amp são operações separadas. |

### Experimente e grave um IR

1. Antes de substituir um IR, leia seu slot e exporte o **JSON de backup de IR** oferecido. A exportação de banco de presets não contém os oito IRs.
2. Escolha um IR da biblioteca ou WAV externo. **A seleção inicia a prévia automaticamente quando o pedal conectado está disponível.**
3. Ouça com volume baixo. Distância do microfone usa 0–100%, sem equivalência documentada em centímetros. O manual original diz que 100% produz silêncio; evite esse extremo ao investigar ausência de som. Este guia não fez medição acústica desse comportamento.
4. Escolha o destino **slot 1–8**, confira seu conteúdo atual e confirme a gravação. O setor anterior fica na memória da sessão; os dados gravados são verificados.
5. Selecione o gabinete armazenado no preset e salve A/B/C se essa seleção mudou. Se necessário, selecione outro gabinete fisicamente e volte para recarregar o slot gravado.
6. Ao experimentar sem gravar, use **Encerrar prévia / End preview** e espere a confirmação de restauração.

Suporte a WAV externo: até **32 MiB**, PCM de 16/24/32 bits ou float32, mono/estéreo. O preparo gera mono em **48 kHz / 512 amostras**, reamostrando e truncando/preenchendo quando necessário, sem normalização. Ampliar visualmente a forma de onda não aumenta o volume do IR.

Packs de IRs podem substituir slots selecionados. Examine as mudanças propostas e exporte backups antes. Arquivos pessoais `.cubeirpack.json` guardam referências/hashes da biblioteca, não WAVs incorporados; resolva referências ausentes antes de aplicar.

### Recupere e restaure

Não desconecte nem feche durante a gravação de um slot. Se a transferência falhar, leia o slot afetado e restaure com backup exportado. Esta árvore mantém snapshots da prévia de IR e recuperação de slots na memória da sessão, sem recuperação automática a partir de um processo anterior do aplicativo. Encerre prévias e restaure com o aplicativo ainda aberto sempre que possível.

Restauração de fábrica e packs identificados são recursos diferentes; a disponibilidade depende de recursos locais reconhecidos. Reconhecer BFI/BFIN não estabelece todos os modelos comerciais de gabinetes nem prova que um pack seja o conteúdo de fábrica da sua unidade. Use reconhecimento por conteúdo, não apenas posição do slot.

## Criadores avançados

Os criadores de Chorus, Phaser, Delay e Reverb oferecem ajustes internos além dos knobs físicos. Seus gráficos explicam controles; não são respostas de áudio medidas nem simulações acústicas.

Comece com presets físico/do aplicativo correspondentes em **PRESET** e use **Ler efeito ativo**. Siga as mensagens: o aplicativo atual prepara/confere o contexto e pode ativar o efeito ou desligar modulação conflitante. Confira os ajustes comuns do preset depois.

| Criador | Contexto audível necessário / controles úteis |
| --- | --- |
| Chorus | Chorus no bloco compartilhado de modulação; profundidade, velocidade, mistura, realimentação, forma de onda e dois ganhos. |
| Phaser | Phaser no lugar de Chorus; velocidade, limites de varredura, nível de saída e seções. |
| Delay | Seção delay/modulação ativa, Chorus/Phaser desligados; tempo, feedback/mistura finos, profundidade/velocidade de modulação temporal. |
| Reverb | Reverb e seção reverb/cabinet ativos, modulação desligada; níveis direto/reverberado, seleção de decaimento, retorno, amortecimento. |

Selecione uma alça do gráfico para ver nome, valor, unidade e explicação. Arraste para ajustar; Tab/setas editam pelo teclado, Enter abre entrada numérica, Escape cancela o gesto atual. Sessões RAM ativas podem aplicar mudanças automaticamente. **Aplicar ajustes não salva na flash.**

Exporte `.chorus.json`, `.phaser.json`, `.delay.json` ou `.reverb.json` para guardar ajustes internos localmente. Importar prepara valores locais; Aplicar envia à sessão RAM ativa. Esses criadores não persistem controles internos na flash, e salvar A/B/C não acrescenta essa capacidade. Controles comuns, recargas de seções ou mudanças físicas podem reinicializar/interromper ajustes internos personalizados.

Use **Restaurar** antes da edição comum de presets ou de outra prévia. Restaurar volta à referência capturada, não aos padrões de fábrica. Snapshots de recuperação atualmente ficam na memória do processo. Exporte arquivos dos criadores para guardar seu trabalho; esses arquivos não são snapshots completos de recuperação.

O **criador de amps** pode experimentar em RAM e gravar explicitamente um slot de modelo de amp. A gravação é separada de A/B/C e dos slots de IR. Restaure sua prévia explicitamente antes de fechar; não dependa do fechamento da janela para restaurar. Leia o guia específico antes de substituir um modelo.

## Tabelas de referência

Preservam nomes/categorias do PDF incluído, não um inventário verificado do seu pedal hoje. Modelos/IRs podem ter sido substituídos. Nomes comerciais e imagens do aplicativo são referências, não certificações acústicas.

### Nove pré-amplificadores

| Posição TYPE | Nome no manual | Categoria do manual |
| --- | --- | --- |
| 1 | Power-Zone Clean | Clean |
| 2 | US Gold 100 Clean | Clean |
| 3 | Two Stone Coral OD | Overdrive |
| 4 | Doctor3 B | Overdrive |
| 5 | Cali JP A | Overdrive |
| 6 | Day Tripper OD | Distortion |
| 7 | Shittcow Dist | Distortion |
| 8 | Wo Stone Coral OD | Distortion |
| 9 | Mr Smith Dist | Distortion |

Day Tripper OD permanece como Distortion aqui, conforme o PDF; um catálogo visual do aplicativo pode classificá-lo de outra forma.

### Seletor de gabinete e slots do software

O PDF numera posições do seletor de 1 a 9; o software oferece slots de armazenamento de 1 a 8 mais OFF. **OFF não é um nono slot de armazenamento.**

| Posição do seletor no PDF | Slot de IR no software | Referência do manual | Formato |
| --- | --- | --- | --- |
| 1 | OFF | Sem IR | — |
| 2 | 1 | Line 6 Vetta | 2×12 |
| 3 | 2 | Marshall 1960AV | 4×12 |
| 4 | 3 | Marshall 1960A T75 LA610 | 4×12 |
| 5 | 4 | VHT Deliverance | 2×12 |
| 6 | 5 | Soldano | 2×12 |
| 7 | 6 | Peavey 5150 + Mesa Oversized | 4×12 |
| 8 | 7 | JSX KT77 + Mesa Stiletto AKG C1000 | 4×12 |
| 9 | 8 | Diezel V30 SM57 (Soldano HR PowerAmp) | 4×12 |

### Presets descritos no PDF

| Preset | Ajustes descritos | Categoria |
| --- | --- | --- |
| A | Wo Stone Coral OD + Delay + Reverb + VHT Deliverance 2×12 | Distortion |
| B | Cali JP A + Delay + Reverb + Soldano 2×12 V30 | Overdrive |
| C | US Gold 100 Clean + Chorus + Delay + Reverb + Marshall 1960A T75 LA610 | Clean |

V30 no preset B reproduz o texto do PDF; não identifica por si só o falante de um IR. Não sobrescreva seus ajustes apenas para coincidir com esses exemplos históricos.

## Solução de problemas

| Sintoma | Verificação |
| --- | --- |
| Sem som de guitarra | Alimentação/bateria, volume da guitarra, IN/OUT, entrada receptora, VOLUME, modo ativo. Encerre prévias; evite distância de IR em 100%. |
| Som áspero/fino | Experimente IR ligado em fones/saída de resposta ampla; compare IR OFF no amp de guitarra. Reduza TONE/GAIN; confira saturação. |
| Notas emboladas | Reduza GAIN, MIX/FB do delay, REVERB; compare som seco/processado. |
| Aplicativo muda, mas o preset ouvido não | Combine A/B/C físico com a seleção do aplicativo. |
| USB carrega, mas a edição não conecta | Cabo de dados, outra porta USB, alimentação, outros editores MIDI. |
| Luz Bluetooth piscando | Celular não conectado; confira pareamento/volume. Bluetooth não é o transporte de edição. |
| Alterações somem ao reiniciar | Diferencie exportação, edição live, prévia RAM e salvamento. Ajustes internos dos criadores de efeitos são temporários. |
| Edição bloqueada | Conclua operações e restaure/encerre prévias ativas. Siga mensagens de contexto/firmware em vez de operações concorrentes. |
| Banco mudou no pedal | Exporte trabalho local se necessário, releia, revise antes de salvar. |
| Nome de IR diferente da tabela | Conteúdo pode ser personalizado/de outro pack; identifique por conteúdo, não posição. |
| Gravação no computador/celular falha | Disponibilidade de entrada de áudio, permissões, cabo/adaptador, entrada selecionada no programa. |
| Gravação de slot falhou | Preserve backups exportados, releia, restaure/confira conteúdo pretendido. Gravação parcial não é conclusão. |

## Especificações e cuidados

Os valores abaixo vêm do manual incluído, não de novas medições.

| Item | Especificação |
| --- | --- |
| Entrada / saída | P10 (6,35 mm), mono TS |
| Especificação de áudio | 48 kHz / 24 bits conforme o manual; não garante todos os formatos de gravação USB |
| Bateria | 780 mAh |
| Referência de autonomia / carga | Até 6 horas / cerca de 2 horas com 5 V / 2 A |
| Dimensões | 161 × 57 × 17,5 mm (comprimento × largura × altura) |
| Peso | 260 g |
| Acessórios listados | Cabos de carga e gravação; confira sua embalagem |

Mantenha seco; evite temperaturas extremas e impactos; limpe com pano seco. Não desmonte. Guarde em local fresco e seco. Descarte o pedal e sua bateria interna em serviço adequado de coleta/reciclagem de eletrônicos, seguindo as orientações locais.

## Leitura complementar e verificação

- [PDF original](../../oficial/en-US/Cube-Baby-M-Vave-Guitar-Pedal-User-Manual-Oficial.pdf): identificações físicas, atalhos, tabelas, especificações.
- [Guia de uso do cubeControl](../../../../../../../docs/historico/guia-de-uso.md): conexão e fluxos.
- [Editores gráficos dos efeitos](../../../../../../../docs/referencias/editores-graficos-de-efeitos.md): controles/significados dos parâmetros.
- [Criador de amps](../../../../../../../docs/referencias/criador-de-amps.md) e [Backing Tracks](../../../../../../../docs/historico/backing-tracks.md): fluxos específicos.
- [Catálogo de amps e gabinetes](../../../../../../../docs/referencias/ampsEirs.md): aliases, identidade de packs, limites.

O comportamento do aplicativo foi conferido no código atual de presets/IRs e criadores em 8 de outubro de 2026. Alguns guias vinculados descrevem comportamentos anteriores, especialmente recuperação em disco e ativação nos criadores; nessas diferenças, prevalece o comportamento atual descrito aqui. Esta revisão não fez novo teste físico, de áudio ou de desligamento/religamento. Gestos físicos e compatibilidades não especificados estão identificados, sem completar as lacunas por suposição.
