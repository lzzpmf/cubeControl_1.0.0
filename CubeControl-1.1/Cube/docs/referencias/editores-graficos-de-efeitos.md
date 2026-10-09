# Editores gráficos dos efeitos

Revisão: 02/10/2026. Implementação WPF em `Controls/EffectGraphEditor.cs` e nas quatro Views `*Editor.xaml`. Os quatro efeitos estão funcionais conforme validação informada pelo usuário. Os desenhos são ilustrações editáveis, não medições ou simulações acústicas.

## Interação

Abra o criador no efeito correspondente. Os pequenos círculos pertencem às cristas, envelopes, bordas e superfícies desenhadas. Arrastar altera somente a propriedade anunciada. Selecionar revela nome, unidade, valor e explicação na faixa integrada acima do desenho, sem caixa flutuante ou tooltip. Clique no valor, pressione Enter ou dê duplo clique na alça para digitar. Enter confirma; Escape cancela. Sair da caixa sem confirmar descarta o texto. Entradas não finitas, fora da faixa ou que invertam os limites do Phaser são recusadas.

Tab/Shift+Tab percorrem os elementos, setas ajustam, Shift reduz o passo dos parâmetros contínuos. Os campos discretos são arredondados pelo modelo. Espaço, tecla de menu ou botão direito levam às opções na faixa integrada. Forma de onda oferece Original/Triangular; seções têm opções 1–3. O ciclo do Delay oferece congelar/retomar, recuperando a última velocidade congelada pela opção Congelar (100% se ainda não houver uma).

Escape durante o arraste, perda involuntária da captura, ocultação ou descarregamento restauram o valor inicial do gesto. A liberação normal, inclusive fora do desenho, conserva o último valor. Como a prévia existente é automática, cancelar pode enviar uma nova alteração restaurando o valor inicial; isso não desfaz o áudio já ouvido. Uma interrupção de transporte continua exigindo a recuperação existente, sem promessa de restauração no hardware.

Os alvos têm 36 DIPs e foco visível, sem reduzir texto/alvos junto com a largura do desenho. A superfície usa no mínimo 308 DIPs, acompanha a largura disponível até 900 DIPs e não contém rolagem própria. Na largura mínima, legendas longas são abreviadas visualmente; a explicação contextual conserva o conteúdo. A faixa contextual reserva espaço mesmo sem seleção; seus textos quebram em larguras menores. Hover realça o ponto, seleção preenche o ponto e foco de teclado acrescenta um contorno pontilhado. Os pontos são criados uma vez por tipo de efeito, e as opções inline são criadas quando muda o parâmetro selecionado; alterações redesenham primitivas vetoriais leves e reposicionam os mesmos alvos. Não há animações, timers de desenho ou dependências novas.

## Linguagem visual

O sinal direto usa traço neutro tracejado; o processado usa a cor do efeito. Chorus, Delay e Reverb reutilizam as cores dos respectivos estágios. O gráfico do Phaser usa laranja derivado da imagem existente, sem recolorir seus knobs ou cards. Em alto contraste, as cores do sistema substituem a paleta; labels, tracejado, espessura e foco mantêm a diferenciação.

Chorus compara original e cópia modulada e mantém o ciclo de modulação separado das ondas de áudio. Delay organiza direto e três ecos no tempo, com intervalo comum, queda por feedback e excursão horizontal da modulação. Phaser destaca a região espectral e o ciclo temporal. Reverb usa espaço simplificado e decaimento separado. Nenhum desenho representa fase atual, geometria acústica ou medição de áudio.

## Fonte de dados e fluxo

Cada alça mantém binding TwoWay com a propriedade do ViewModel do efeito. O desenho lê esses mesmos valores; a entrada numérica e a automação acessível passam pela mesma propriedade e seus limites. `GraphNode` oferece nome, ajuda, foco e RangeValue via UI Automation. Os recursos `CubeInset`, `CubeText`, `Muted`, `CubeFocus`, `CubeUiFont`, `CubePhaserGraph` e `CubeThemeRevision` seguem o tema existente. A inscrição de idioma é fraca e removida no descarregamento.

O controle não conhece protocolo, endereço DSP, arquivo de preset nem sessão USB. Os eventos `Edited` existentes continuam alimentando `Send*Edits`; o transporte mantém sua serialização, coalescência de pendências, conferência e backups. Não foi criada uma escrita paralela nem uma entrada de desfazer por movimento. O histórico global de presets não incluía os parâmetros internos e continua separado; Escape cancela o gesto atual, não constitui um novo histórico global.

Durante a resolução inicial do DataContext, os bindings usam o mínimo do parâmetro como valor provisório. A leitura usada pelo desenho também limita valores à faixa finita antes de calcular coordenadas, sem escrever essa proteção no ViewModel ou no pedal. Isso evita `log(0)` nas bordas do Phaser: o log do Windows registrou esse caso como posição `Left = −∞` e encerramento durante a criação da janela.

| Ação | Significado preservado |
| --- | --- |
| Ler efeito ativo | Confere identidade, firmware e contexto físico, captura referência e inicia edição em RAM com recuperação |
| Editar com prévia ativa | Solicita aplicação automática na RAM pela infraestrutura existente |
| Editar sem prévia | Altera o modelo local; não grava o pedal |
| Restaurar efeito | Restaura e confere o snapshot anterior à prévia; não é reset para defaults de fábrica |
| Importar | Carrega arquivo local validado e seus parâmetros; o carregamento não dispara `Edited`; use Aplicar na prévia ativa |
| Aplicar ajustes | Envia os parâmetros locais para a prévia ativa; não salva A/B/C nem grava flash |
| Exportar | Salva arquivo local específico do efeito, identidade e mapeamento; não persiste no pedal |

As mensagens distinguem edição local alterada, pendência de aplicação, RAM conferida, importação aguardando aplicação e interrupção/erro. A faixa de estado informa desconexão, efeito desligado no preset do aplicativo, indisponibilidade e prévia em RAM não salva no pedal. A ativação real ainda é conferida pela leitura; a UI não liga/desliga efeitos para contornar requisitos. Erros detalhados permanecem na apresentação global existente e na mensagem do efeito quando fornecida pelo fluxo.

Chorus e Phaser compartilham o bloco de modulação: ative o algoritmo escolhido no mesmo preset físico A/B/C em modo PRESET antes de ler. Delay exige seção delay/modulação ativa e Chorus/Phaser desligados. Reverb exige reverb e seção reverb/cabinet ativos e modulação desligada. Prévia bloqueia operações concorrentes conforme os predicados existentes. Firmware desconhecido, contexto incompatível e backups inválidos continuam sendo restrições reais. Recuperações pendentes não são reaplicadas silenciosamente.

## Contrato dos parâmetros

Os valores originais abaixo vêm dos registros `*Parameters`, ViewModels e pesquisas vinculadas, e não de imagens. “Inicial local” é somente o valor do ViewModel antes da leitura. O valor lido do preset físico prevalece. Todos os destinos passam por `Effect.Property → Effect.Parameters → Apply*Preview` quando há prévia; sem prévia, permanecem locais até aplicação. Os offsets são os campos já existentes do snapshot DSP, não novos comandos.

### Delay

| Propriedade | Significado / unidade | Limites na UI | Original / inicial local | Elemento e gesto | Conversão / destino |
| --- | --- | --- | --- | --- | --- |
| `TimeSamples` | Tempo solicitado, amostras internas | 2400–16350, inteiro | `2400 + 450 × TIME`; inicial 9600 | Primeira repetição, horizontal; demais reposicionadas com intervalo comum | inteiro → `0x696` |
| `Feedback` | Ganho de retorno, % | 0–99,21875 | controle normal /128; inicial 25% | Contorno inferior do segundo eco; expandir aumenta realimentação sem mudar intervalo | /100 → `0x698` |
| `Depth` | Profundidade da modulação temporal, % do original | 0–100 | 100 = 0,1 | Crista/limite da excursão; expandir verticalmente | /1000 → `0x69C` |
| `Rate` | Velocidade relativa, % do original | 0–100 | 100 = 0,01 | Extremidade do ciclo; alongar reduz; opção inline congela/retoma | /10000 → `0x6A0` |
| `Mix` | Mistura interna, escala percentual do campo | 0–92,1875 | controle normal /128; inicial 25 | Envelope da combinação; expandir aumenta o parâmetro único | /100 → `0x6DC` |

As repetições e seu envelope expressam qualitativamente a proporção sucessiva de feedback; não modelam filtros, suavização de TIME ou amplitude medida. A linha do tempo é esquemática e comprime a faixa para manter alvos legíveis, sem unidades de milissegundos. A profundidade altera a excursão do atraso, sem alterar altura dos ecos. O avanço de fase é `Rate/10000 / D`, com `D` sendo o tempo interno suavizado: a legenda usa o tempo solicitado como referência estática, não como leitura do estado dinâmico. Alterar TIME atualiza essa referência sem alterar Rate. Zero é mostrado como fase congelada, sem período infinito ou alegação de conhecer a fase atual do pedal.

Para `m = Mix/100`: abaixo de 0,5, direto = 1 e eco = 2m; a partir de 0,5, direto = 2−2m e eco = 1. O direto e os ecos da linha temporal, com as barras de mistura, mostram essa função; 50 mantém ambos integrais. Não há ganhos independentes, 100% wet, BPM, ping-pong ou multitap.

### Chorus

| Propriedade | Significado / unidade | Limites na UI | Original | Elemento e gesto | Conversão / destino |
| --- | --- | --- | --- | --- | --- |
| `Depth` | Excursão temporal, % do original | 0–100 | 100 = 0,4 | Crista da trajetória; expandir verticalmente | ×0,4/100 → `0x644` |
| `Speed` | Velocidade, Hz estimados | 0,384–3,024 | default 1,488; depende de MOD | Extremidade do ciclo; alongar reduz velocidade | /48000 → `0x648` |
| `Mix` | Contribuição processada, % do original | 0–100 | 100 = 0,85 | Envelope na contribuição de saída; destaca o ramo | ×0,85/100 → `0x688` |
| `Feedback` | Ganho de realimentação, % | 0–10 | 0 | Amplitude no contorno de retorno externo ao fluxo principal, sem mudar o caminho temporal | /100 → `0x64C` |
| `WetGain` | Ganho do ramo atrasado, % do original | 50–100 | 100 = 1 | Envelope do atraso, antes da combinação | /100 → primeiro `b0`, `0x650` |
| `CombinedGain` | Ganho do ramo combinado, % do original | 50–100 | 100 = 1 | Envelope após combinação; independente dos outros ganhos | /100 → segundo `b0`, `0x66C` |
| `Waveform` | Seletor discreto, sem unidade | 0 ou 1 | 0 = Original | Opções Original/Triangular na faixa contextual | byte → `0x640` |

Topologia consultada: `processado = ganhoCombinado × (entrada + ganhoAtrasado × atrasado)/2`; entrada do buffer recebe `entrada + feedback × processado`; saída = `(1−m) × entrada + m × processado`, com `m = 0,85 × Mix/100`. A contribuição direta desenhada acompanha essa função real. 100% preserva a mistura original, não corresponde a wet puro. O original de ganhos unitários produz `0,575 × entrada + 0,425 × atrasado`. O seletor Original chama ROM; sua curva é uma ilustração da inferência estrutural, não uma reconstrução ou medição da ROM. Duas leituras coincidentes não foram expostas como vozes independentes.

### Phaser

| Propriedade | Significado / unidade | Limites na UI | Original | Elemento e gesto | Conversão / destino |
| --- | --- | --- | --- | --- | --- |
| `Speed` | Velocidade, Hz estimados | 0,48–6,24 | default 0,48; depende de MOD | Extremidade de ciclo temporal separado; alongar reduz | /48000 → `0x644` |
| `Lower` | Extremo inferior, Hz estimados | 300–Upper | 300 | Borda esquerda logarítmica; arraste horizontal | float → `0x64C` |
| `Upper` | Extremo superior, Hz estimados | Lower–3000 | 3000 | Borda direita logarítmica; arraste horizontal | float → `0x650` |
| `Level` | Ganho de saída, % do original | 0–100 | 100 = 0,64 | Envelope de saída; expandir aumenta, sem editar vales | ×0,64/100 → `0x654` |
| `Sections` | Quantidade discreta de seções | 1–3 | 3 | Opções 1–3 na faixa contextual / setas | quantidade−1 → `0x641` |

As bordas têm alvos em alturas distintas para permitir limites iguais sem sobreposição. Não há deslocamento composto da região. Os vales acompanham a região delimitada e são ilustrativos, sem associação numérica entre vales e seções ou alegação de resposta calculada. Setas vetoriais indicam ida e volta, sem animação. Não foram adicionados feedback ou mistura; Level não é dry/wet.

### Reverb

| Propriedade | Significado / unidade | Limites na UI | Original / inicial local | Elemento e gesto | Conversão / destino |
| --- | --- | --- | --- | --- | --- |
| `Dry` | Ganho direto, % | 0–100 | 100 = 1 | Envelope sobre caminho direto; expandir aumenta somente Dry | /100 → `0x708` |
| `Wet` | Ganho reverberado, % | 0–90 | depende de Quantidade; inicial 55 | Envelope das reflexões na saída; independente de Dry | /100 → `0x70C` |
| `Decay` | Seleção de realimentação, escala interna | 1–15, inteiro | depende de Quantidade; inicial 8 | Envelope de persistência; elevar contorno aumenta seleção | vetor da tabela → `0x6EA`, 12 bytes |
| `ReturnLevel` | Retorno conjunto das linhas, % do original | 0–100 | 100 = byte 80 | Envelope do retorno comum das linhas | arredondar ×0,8 → `0x6E1` |
| `Damping` | Amortecimento, escala interna | 0–10, inteiro | 10 | Borda do campo reverberado; aprofundar aumenta o peso do estado anterior | `(40+2i)×valor/10`, divisão inteira, i=0…5 → `0x6E3–0x6E8` |

Dry e Wet permanecem independentes. O retorno original de cada linha tem multiplicador 80/128; zerá-lo não elimina todo o ramo processado, pois há excitação adicional antes da difusão/filtros. Os quinze conjuntos de realimentação aumentam os coeficientes; a persistência desenhada é qualitativa, sem RT60, segundos ou equivalência acústica.

O helper de amortecimento pondera estado anterior por `a/512` e amostra atual por `(512−a)/512`, com saturação e chamadas repetidas já documentadas na pesquisa. 10 significa coeficientes originais, não absorção máxima. Zero zera esse peso particular, sem prometer ausência de toda perda espectral no algoritmo. A sala em vista plana mostra fonte, caminho direto, poucas reflexões conceituais, campo reverberado e saída. Não há paredes, fonte ou ouvinte arrastáveis; não representa metros, materiais, difusão ou geometria acústica editável.

## Controles principais e limitações

Master e controles normais dos pedais não foram redesenhados. MOD pode recarregar configurações de Chorus/Phaser; TIME, feedback e MIX normais recarregam seus campos de Delay; Quantidade do Reverb combina realimentação e ganho reverberado. Reativar seções pode reinicializar ajustes internos. Os bloqueios e monitores de prévia existentes continuam responsáveis pela consistência.

Não há persistência nova em flash, novo algoritmo DSP, controles físicos inventados ou validação sonora adicional. Os esquemas não calculam uma resposta de áudio. A evolução visual preserva todos os contratos de parâmetros e os fluxos existentes de leitura, aplicação e restauração.

Fontes: [Chorus](../../../../engenharia-reversa/cube-baby/CUBE_BABY_CHORUS.md), [Phaser](../../../../engenharia-reversa/cube-baby/CUBE_BABY_PHASER.md), [Delay](../../../../engenharia-reversa/cube-baby/CUBE_BABY_DELAY.md), [Reverb](../../../../engenharia-reversa/cube-baby/CUBE_BABY_REVERB.md); `ViewModels/*ViewModel.cs`, `EditorViewModel.{Chorus,Phaser,Delay,Reverb}.cs` e `Cube.Protocol/*Parameters.cs`.
