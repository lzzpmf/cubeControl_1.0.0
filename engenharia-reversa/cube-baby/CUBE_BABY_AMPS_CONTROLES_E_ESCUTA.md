# Cube Baby — controles de amp: documentação técnica e de escuta

[Documentação](../../CubeControl-1.1/Cube/docs/README.md) · [Uso do criador](../../CubeControl-1.1/Cube/docs/criador-de-amps.md)

Relatos consolidados em 30/09/2026; implementação reconferida em 01/10/2026. As observações musicais foram preservadas. As seções de código abaixo descrevem esta árvore, incluindo os limites individuais e as apresentações de EQ já implementadas. Isso não estabelece novos limites de operação comprovadamente estáveis.

## Navegação

- [1. Contexto e força das evidências](#1-contexto-e-força-das-evidências)
- [2. Visão musical e interpretação por controle](#2-visão-musical-e-interpretação-por-controle)
- [3. Mapeamento para programação](#3-mapeamento-para-programação)
- [4. Como números podem mudar o som: interpretações delimitadas](#4-como-números-podem-mudar-o-som-interpretações-delimitadas)
- [5. Curva: evidência histórica separada](#5-curva-evidência-histórica-separada)
- [6. Evolução da interface e pendências](#6-evolução-da-interface-e-pendências)
- [7. Fontes locais e conferência](#7-fontes-locais-e-conferência)

## 1. Contexto e força das evidências

| Item | Registro |
|---|---|
| Amp informado | Two Stone / Two Rock Coral, terceira posição, **TYPE 2**; nome no perfil: Two Stone Coral OD |
| Método informado | Guitarra tocada durante os ajustes; os demais faders fora do teste retornavam a **100%** |
| Limites relatados | “Não pode passar de 100” significa **fica mudo ou trava**, conforme esclarecimento do usuário |
| Conteúdo binário do amp durante o ensaio | Não capturado nesta sessão; a identificação do modelo não comprova que o setor estivesse idêntico ao de fábrica |
| Guitarra, captador, entrada e cadeia de escuta | Não registrados para este ensaio; não transportar automaticamente condições das sessões anteriores |
| Ganho, tone, volume, cabinet, efeitos e força de palhetada | Valores exatos não registrados |
| Versão executada e firmware | Não registrados; o código consultado é uma referência atual, não prova da versão ensaiada |
| Faixas percorridas e primeiro valor de falha | Não registrados numericamente por controle |
| Data/hora de execução do ensaio | Não registrada; a data acima é da consolidação |

O retorno dos outros faders a 100% favorece a comparação isolada. Os relatos sobre grupos em conjunto são registrados como tais; não há uma sequência completa das combinações testadas. Não houve captura de áudio, medição espectral, log USB ou nova operação no pedal durante a elaboração deste documento.

### Classificação usada

- **O — Observação musical:** relato direto do usuário, válido nas condições ensaiadas.
- **C — Implementação confirmada:** operação identificada no código ou mapeamento sustentado pela documentação de leituras anteriores.
- **H — Hipótese técnica:** explicação compatível com a observação, ainda sem demonstração específica no firmware.
- **P — Pendente:** informação ou experimento necessário para distinguir explicações.

“Ficou mudo ou travou” é o sintoma relatado. **Não comprova estouro de memória, desconexão USB, travamento do processador ou instabilidade de filtro.** A ausência de som e a perda de comunicação precisam ser verificadas separadamente.

## 2. Visão musical e interpretação por controle

Os nomes musicais abaixo são descrições provisórias do efeito ouvido, não nomes oficiais de funções do DSP. “Funciona tudo” significa apenas funcionamento na faixa efetivamente percorrida, cujos extremos não foram registrados. Não equivale a estabilidade universal.

### Controles gerais

| Controle | O — escuta | C — operação | H / P — interpretação e pendência |
|---|---|---|---|
| Ganho | “Funciona como ganho mesmo” | Multiplica os oito valores da tabela interna de ganho | H: altera a excitação de uma etapa não linear. P: determinar a relação exata com o ganho de preset, sem presumir que sejam o mesmo comando |
| Compensação | “Funciona muito bem como compensação em volume sem distorcer” | Multiplica outra tabela de oito valores | H: atua predominantemente no nível nas condições testadas. P: posição na cadeia e interação com ganho/volume; não prova ausência de distorção para qualquer ajuste |
| EQ A e EQ B | “Funcionam bem”; interface pouco intuitiva | Cada EQ recalcula cinco coeficientes de um filtro final | P: avaliar apresentação clássica e moderna para os mesmos parâmetros; funcionamento percebido não mede a resposta total do amp |

### Coeficientes A

Os índices abaixo se referem à tabela de 50 valores, com indexação iniciada em zero. Os endereços completos estão na seção 3.

| Controle / índices | O — descrição do guitarrista | Limite observado | H — leitura técnica possível | P — o que falta separar |
|---|---|---|---|---|
| A01 / 0–5 | “Ganho” | Não informado | Alteração de nível interno ou da energia que chega a uma etapa não linear | Saturação real versus aumento de volume; relação com o multiplicador de ganho |
| A02 / 6–11 | “Corpo” | Acima de 100%: silêncio/travamento relatado | Mudança de balanço espectral ou ressonância que aumenta a sensação de corpo | Frequência, direção e primeiro valor de falha |
| A03 / 12–17 | “Médio Graves funciona tudo” | Sem falha relatada na faixa testada | Mudança de conteúdo relativo em médio-graves | Faixa exata ensaiada e resposta ao aumentar/diminuir |
| A04 / 18–24 | “Médios” | Acima de 100%: silêncio/travamento relatado | Alteração de resposta ou ressonância na região percebida como médios | Separar ganho, largura e frequência; confirmar limiar |
| A05 / 25–30 | “Punch — funciona tudo” | Sem falha relatada na faixa testada | Balanço espectral e/ou resposta ao ataque podem explicar impacto maior | Não presumir compressor ou controle de transientes; comparar ataque e nível sustentado |
| A06 / 31–36 | Integra o conjunto A06–08 | Não informado individualmente | Ver interpretação conjunta abaixo | Contribuição isolada e dependência dos outros grupos |
| A07 / 37–42 | Integra o conjunto A06–08 | Não informado individualmente | Ver interpretação conjunta abaixo | Contribuição isolada e dependência dos outros grupos |
| A08 / 43–49 | Integra o conjunto A06–08 | Não informado individualmente | Ver interpretação conjunta abaixo | Contribuição isolada e dependência dos outros grupos |

**O — A06–08 juntos:** “funcionam mais em conjunto como um sutil corte de ardido (agudos)”. **H:** mudança conjunta do balanço de altas frequências, de ressonâncias ou do conteúdo harmônico. **P:** não foi registrada a direção dos três faders nem uma correspondência entre suas posições; não há base para definir ainda um único macrocontrole ou chamar o conjunto de filtro passa-baixas.

### Coeficientes B

| Controle / índices | O — descrição do guitarrista | Limite observado | H — leitura técnica possível | P — o que falta separar |
|---|---|---|---|---|
| B01 / 0–5 | “Diminuir faz parecer um SM; aumentar faz parecer um ribbon” | Acima de 100%: silêncio/travamento relatado | Mudança de presença/brilho ou balanço espectral compatível com a analogia | “SM” não identifica um modelo específico; não implica simulação física de microfone. Falta registrar em qual faixa aumentar produziu a impressão antes da falha |
| B02 / 6–11 | Médios quase sozinhos, “médio de balde”; reduzir pouco causa muito efeito | Acima de 100%: falha; redução de cerca de 1% já faz diferença relatada | Ênfase ressonante ou atenuação relativa das outras regiões pode gerar caráter nasal | Frequência, largura, nível e menor passo musicalmente útil |
| B03 / 12–17 | Mais agressivo que B02; médio-agudos quase sozinhos | Acima de 100%: falha; redução de cerca de 1% já faz diferença relatada | Alteração espectral mais concentrada ou ressonante | Não converter diretamente em frequência/Q sem identificar a estrutura |
| B04 / 18–24 | “Corpo de médio graves lama” | Não informado | Acúmulo relativo de energia em região percebida como médio-graves | Direção, intensidade e diferença em relação a B05/B06 |
| B05 / 25–30 | “Corpo de médio graves lama” | Não informado | Mesma descrição musical não implica mesma função matemática | Dependência e contribuição isolada |
| B06 / 31–36 | “Corpo de médio graves lama” | Não informado | Possível contribuição ao mesmo balanço espectral percebido | Dependência e contribuição isolada |
| B07 / 37–42 | “Médios um pouco mais definidos” | Não informado | Redistribuição de energia ou redução de mascaramento pode mudar a clareza | Direção e distinção de B08 |
| B08 / 43–49 | “Médios um pouco mais definidos” | Não informado | Mesmas possibilidades, sem identificação de banda exata | Direção e distinção de B07 |

**O — B02/B03:** lembram o efeito de wah. Isso descreve o caráter ouvido; **não confirma a existência de um algoritmo de wah ou de varredura de frequência**. A proposta de restringir mais esses controles permanece pendente de definição de faixa e resolução.

## 3. Mapeamento para programação

### 3.1 Convenções e blocos

Todos os intervalos abaixo incluem o primeiro e o último byte. Os offsets são relativos ao início de um setor de amp de 4096 bytes. Os valores das tabelas são **float32 IEEE 754 little-endian**, quatro bytes por elemento; percentuais são controles da interface, não valores armazenados diretamente nessas tabelas.

- Flash: seletor `00`, base do amp `0x60000 + TYPE × 0x1000`. Para TYPE 2: `0x62000`.
- RAM ativa: seletor `04`. O corpo do setor em `0x00C–0x62B` corresponde à RAM `0x01C–0x63B` (1568 bytes). Nesse corpo, `endereço_RAM = offset_setor + 0x10`.
- O endereço RAM representa o **amp ativo**, não um endereço diferente por slot. Escolher um slot na interface não comprova a seleção física desse amp.
- Para o elemento `i`: `offset = base_tabela + 4 × i`. Para flash absoluta do TYPE 2, somar `0x62000`.

| Bloco | Elementos | Offset no setor | RAM ativa, seletor 04 | Operação do criador |
|---|---:|---|---|---|
| Ganho interno | 8 | `0x00C–0x02B` | `0x01C–0x03B` | Multiplica todos por um fator |
| Compensação | 8 | `0x02C–0x04B` | `0x03C–0x05B` | Multiplica todos por outro fator |
| Coeficientes A | 50 | `0x050–0x117` | `0x060–0x127` | Oito grupos proporcionais |
| Curva | 256 | `0x11C–0x51B` | `0x12C–0x52B` | Dezesseis grupos proporcionais, com limitação a 0–1 |
| Coeficientes B | 50 | `0x51C–0x5E3` | `0x52C–0x5F3` | Oito grupos proporcionais |
| EQ A final | 5 | `0x5E8–0x5FB` | `0x5F8–0x60B` | Recalcula `[b0,b1,b2,a1,a2]` |
| EQ B final | 5 | `0x604–0x617` | `0x614–0x627` | Recalcula `[b0,b1,b2,a1,a2]` |

Os campos entre esses blocos não são implicitamente parte dos controles. Por exemplo, o escalar em `0x118` e os metadados junto aos EQs são preservados por essas edições.

### 3.2 Ganho de preset versus ganho interno

**C:** `CubeBabyProfile.Parameters` define o ganho comum como um **byte de 0 a 7**, no offset 1 do preset. Para a edição live via protocolo, o endereço é `05:(0x80000000 + preset × 16 + 1)`, com presets A/B/C numerados 0/1/2.

**C:** o ganho do criador altera oito floats em `04:0x01C–0x03B`. Ele não envia o mesmo byte do ganho comum. A compensação também é distinta do volume geral do preset, cujo offset é 5 e a faixa é 0–127.

**H:** a coincidência entre oito posições de ganho e oito entradas da tabela é compatível com seleção de entradas ou processamento relacionado a elas. **P:** não comprova indexação direta, interpolação nem equivalência auditiva entre mudar uma posição e multiplicar a tabela. A posição do knob físico não foi mapeada integralmente neste ensaio.

Exemplo da operação do criador: uma entrada `0,5` com multiplicador `2×` produz `1,0`; aplicar novamente não produz `2,0`. O cálculo continua usando a base editada e o mesmo multiplicador. A coluna de resultado das tabelas de ganho/compensação inclui esse fator.

### 3.3 Oito grupos de cada conjunto de coeficientes

Regra confirmada: `início = floor(g × 50 / 8)` e `fim_exclusivo = floor((g + 1) × 50 / 8)`, para `g = 0..7`.

| Grupo | Indices | Quantidade | A: setor | A: RAM 04 | B: setor | B: RAM 04 |
|---|---|---:|---|---|---|---|
| 01 | 0-5 | 6 | `0x050-0x067` | `0x060-0x077` | `0x51C-0x533` | `0x52C-0x543` |
| 02 | 6-11 | 6 | `0x068-0x07F` | `0x078-0x08F` | `0x534-0x54B` | `0x544-0x55B` |
| 03 | 12-17 | 6 | `0x080-0x097` | `0x090-0x0A7` | `0x54C-0x563` | `0x55C-0x573` |
| 04 | 18-24 | 7 | `0x098-0x0B3` | `0x0A8-0x0C3` | `0x564-0x57F` | `0x574-0x58F` |
| 05 | 25-30 | 6 | `0x0B4-0x0CB` | `0x0C4-0x0DB` | `0x580-0x597` | `0x590-0x5A7` |
| 06 | 31-36 | 6 | `0x0CC-0x0E3` | `0x0DC-0x0F3` | `0x598-0x5AF` | `0x5A8-0x5BF` |
| 07 | 37-42 | 6 | `0x0E4-0x0FB` | `0x0F4-0x10B` | `0x5B0-0x5C7` | `0x5C0-0x5D7` |
| 08 | 43-49 | 7 | `0x0FC-0x117` | `0x10C-0x127` | `0x5C8-0x5E3` | `0x5D8-0x5F3` |

Os conjuntos A/B de 50 elementos **não são os EQs A/B finais**. Cada EQ final ocupa somente cinco floats em outro endereço.

### 3.4 Dezesseis grupos da curva

Regra: grupo visível `j = 1..16` controla os índices `16 × (j−1)` até `16 × j−1`.

| Grupo | Indices | Offset no setor | RAM 04 |
|---|---|---|---|
| 01 | 0-15 | `0x11C-0x15B` | `0x12C-0x16B` |
| 02 | 16-31 | `0x15C-0x19B` | `0x16C-0x1AB` |
| 03 | 32-47 | `0x19C-0x1DB` | `0x1AC-0x1EB` |
| 04 | 48-63 | `0x1DC-0x21B` | `0x1EC-0x22B` |
| 05 | 64-79 | `0x21C-0x25B` | `0x22C-0x26B` |
| 06 | 80-95 | `0x25C-0x29B` | `0x26C-0x2AB` |
| 07 | 96-111 | `0x29C-0x2DB` | `0x2AC-0x2EB` |
| 08 | 112-127 | `0x2DC-0x31B` | `0x2EC-0x32B` |
| 09 | 128-143 | `0x31C-0x35B` | `0x32C-0x36B` |
| 10 | 144-159 | `0x35C-0x39B` | `0x36C-0x3AB` |
| 11 | 160-175 | `0x39C-0x3DB` | `0x3AC-0x3EB` |
| 12 | 176-191 | `0x3DC-0x41B` | `0x3EC-0x42B` |
| 13 | 192-207 | `0x41C-0x45B` | `0x42C-0x46B` |
| 14 | 208-223 | `0x45C-0x49B` | `0x46C-0x4AB` |
| 15 | 224-239 | `0x49C-0x4DB` | `0x4AC-0x4EB` |
| 16 | 240-255 | `0x4DC-0x51B` | `0x4EC-0x52B` |

Essa divisão é por índices. Não autoriza interpretar cada fader como uma faixa de Hz.

### 3.5 Percentuais, referência e edição manual

**C — coeficientes A/B:** A02, A04 e B01–B03 usam **90%–100%**; os outros grupos usam **90%–110%**. Para cada entrada do trecho:

```text
limite_superior = 1,0 nos grupos A02/A04/B01/B02/B03; 1,1 nos demais
fator = clamp(percentual / 100, 0,9, limite_superior)
novo_valor = float32(referência_do_elemento × fator)
```

No centro, 100% mantém a referência. 99% representa redução proporcional de 1%, não subtração de 1 do coeficiente nem corte de 1 dB. Para coeficientes negativos, aumentar o percentual aumenta sua magnitude negativa. Zero permanece zero.

**C — curva:** percentual de 0%–200%, com `novo_valor = clamp(float32(referência × percentual / 100), 0, 1)`. Dobrar um ponto já próximo de 1 pode apenas levá-lo ao limite. Não há suavização automática entre grupos; ajustes distintos podem introduzir descontinuidades.

A referência é capturada ao carregar o amp. Arrastar repetidamente não multiplica o último resultado: recalcula a partir dessa referência. Voltar a 100% recupera os valores de referência, salvo mudança de referência por edição manual ou novo carregamento.

No ViewModel, editar um valor válido da tabela de curva ou coeficientes recaptura **todo o trecho correspondente** como referência e retorna seu fader a 100%. Os limites percentuais são relativos à referência vigente, não limites absolutos do arquivo de fábrica. Esse caminho de alteração direta dos valores não é restringido pela faixa percentual, mas as tabelas brutas não estão expostas no XAML atual.

O modelo contém `Reference`, `Value` e `Effective`. Nos grupos de curva/coeficientes, o fader altera `Value` e `Effective`; nas tabelas de ganho/compensação, o multiplicador mantém `Value` e modifica `Effective`. A referência do grupo é atualizada quando seus valores são editados diretamente. A interface atual exibe controles e valores formatados, sem painel de tabelas “Base/A enviar” ou edição textual desses elementos; a descrição anterior dessas colunas correspondia a outra apresentação.

O modelo normaliza a posição em 0–1000. Arraste/teclado usam resolução de 1 nessa escala, ou 0,1 com Shift no criador. Isso corresponde a 0,02/0,002 ponto percentual na faixa 90–110%, ou 0,01/0,001 na faixa 90–100%. A roda tem multiplicadores próprios de sensibilidade. B02/B03 exibem três casas decimais; isso não comprova precisão de resposta musical nem estabilidade. `AmpControl.TryEdit` existe no modelo, mas não está ligado a um campo de texto no XAML atual.

### 3.6 EQs finais e validações atuais

**C:** o criador deriva frequência, nível e Q dos coeficientes carregados e, quando o usuário movimenta um controle de EQ, recalcula os cinco floats desse filtro. Frequência e Q usam escala logarítmica de interface; nível usa escala linear. Limites: 20–20000 Hz, −12 a +12 dB e Q de 0,1 a 10.

O cálculo implementa filtros de pico/vale normalizados para 48000 Hz. A identificação matemática desse formato e dessa taxa está documentada na análise anterior; não é uma nova medição da taxa interna do pedal. Os coeficientes seguem a interpretação:

```text
H(z) = (b0 + b1·z⁻¹ + b2·z⁻²) / (1 + a1·z⁻¹ + a2·z⁻²)
```

Metadados vizinhos de frequência/ganho/Q não são sincronizados por `SetEq`; são preservados. Isso precisa permanecer visível na interpretação técnica de arquivos gerados, sem afirmar que todos os campos descritivos do setor correspondem ao EQ editado.

`AmpImage.Validate` verifica tamanho/cabeçalho, valores finitos dos blocos conhecidos, limites de ganho/compensação/curva e estabilidade dos **dois EQs finais**. Não verifica uma estrutura de filtros ou estabilidade específica dos conjuntos de **50 coeficientes A/B**. Um conjunto finito aceito pelo arquivo pode ainda produzir silêncio ou outro comportamento inadequado no processamento.

A validação limita ganho interno a 60; os TYPEs 6/7/8 originais têm entradas até 100 e são recusados ao carregar/importar no criador. Trata-se de uma divergência do aplicativo, não de corrupção dos dados de fábrica. As apresentações **Clássico** e **Moderno** dos EQs já existem em `AmpEditor.xaml`; o gráfico representa os coeficientes, sem medir a resposta acústica total.

## 4. Como números podem mudar o som: interpretações delimitadas

### Coeficientes não equivalem a controles de volume

**C:** os grupos são uma divisão da interface, com comprimentos 6/6/6/7/6/6/6/7. Não há no agrupamento código que identifique estágios reais do firmware. Mesmo que uma tabela contenha filtros, esses limites podem atravessar estruturas matemáticas.

**H — mecanismo possível, não formato confirmado para A/B:** em um filtro recursivo, mudar termos de realimentação pode alterar polos, ressonância e estabilidade; em outras estruturas, pode alterar ganho, resposta espectral ou condicionamento de uma etapa não linear. Multiplicar parte desses termos não tem necessariamente o efeito de multiplicar apenas a saída. Pequenas alterações podem ter efeito grande perto de uma região sensível.

Isso oferece explicações possíveis para a sensibilidade de B02/B03 e para falhas acima de 100%, mas não determina a causa. Não é válido inferir “dez biquads” somente porque há 50 floats, nem calcular uma frequência por fader sem identificar a organização real.

### Tradução musical sem falsa precisão

- **Corpo / médio-graves:** sensação de preenchimento pode vir de reforço relativo, redução de outras regiões ou interação com distorção. Não fixa uma frequência.
- **Punch:** pode refletir ataque, nível ou balanço espectral; não prova processamento de dinâmica dedicado.
- **Ardido:** pode envolver agudos, ressonâncias ou harmônicos da saturação. Reduzi-lo não prova a existência de um corte simples de agudos.
- **Balde / wah:** são compatíveis com ênfase espectral concentrada ou caráter nasal; não identificam o algoritmo.
- **Lama:** pode corresponder a excesso de corpo e mascaramento percebido. Não prova perda de resolução temporal ou memória insuficiente.
- **SM / ribbon:** analogias de timbre, não identificação de microfone, IR ou bloco de simulação acústica.

Comparações futuras precisarão considerar volume: um ajuste mais alto pode parecer mais cheio ou agressivo mesmo quando o objetivo é comparar apenas timbre. O ganho percebido de A01 não demonstra equivalência com o controle de ganho comum ou com a tabela de oito valores.

### Falha de som e comunicação

**O:** A02, A04, B01, B02 e B03 foram associados a falha acima de 100%. **C:** o código atual limita esses cinco grupos a 100%. Essa restrição atende ao teto relatado, mas não demonstra estabilidade em toda a faixa inferior nem em outros modelos.

**P:** faltam o primeiro percentual de falha, o tempo até o silêncio, a resposta USB durante o evento e a forma de recuperação. Também não foi confirmada a versão de transporte usada no ensaio, inclusive se já estava sem pausas artificiais. Portanto, não atribuir a falha exclusivamente aos coeficientes nem exclusivamente à temporização. ACK e leitura de bytes corretos não comprovam processamento de áudio correto.

## 5. Curva: evidência histórica separada

Os relatos atuais detalham os controles gerais e os coeficientes A/B; não fornecem uma caracterização dos 16 faders da curva no TYPE 2.

Em **outra sessão**, registrada em [CUBE_BABY_AMPS.md](CUBE_BABY_AMPS.md), a curva foi testada em oito trechos de 32 pontos, zerando ou dobrando um trecho por vez em relação à referência S00. Houve relatos de chiado inicialmente mais agudo, depois com caráter mais médio, e nos trechos finais mais discreto ou dependente de palhetada forte/cordas graves. Alterações anteriores para curva linear e outras formas também produziram mudanças de crunch, corpo e saturação percebidos.

**H:** o conjunto é compatível com uma tabela de transferência de amplitude participando da não linearidade. A dependência da palhetada dá suporte a essa interpretação, mas não revela a indexação exata, tratamento do sinal negativo, interpolação ou ordem do DSP. Os efeitos de descontinuidades entre trechos também podem participar.

Esses testes não tornam os índices bandas de EQ e não descrevem automaticamente o comportamento dos atuais 16 grupos. Tampouco devem ser atribuídos ao ensaio presente do Two Stone Coral.

## 6. Evolução da interface e pendências

Esta seção distingue o que já aparece no código das investigações ainda abertas. Nenhuma mudança no aplicativo foi feita durante esta revisão documental.

| Tema | Evidência que motiva | Decisão ou investigação pendente |
|---|---|---|
| Ganho comum versus interno | Byte de preset e tabela são operações distintas | Nomear claramente ambos; investigar dependência antes de sugerir equivalência |
| Limites por controle | A02/A04/B01/B02/B03 falham acima de 100% no relato | Teto individual de 100% implementado; faixa inferior continua sem validação universal |
| B02/B03 | Aproximadamente 1% já produz muito efeito | Faixa 90–100%, três casas decimais e Shift implementados; avaliar facilidade de ajuste e estabilidade |
| A06–08 | Efeito conjunto de sutil corte de ardido | Avaliar macrocontrole após confirmar direções e proporções; preservar acesso individual no debug |
| B04–06 | Mesma descrição de corpo/lama | Avaliar combinação somente após diferenciar contribuições e interações |
| B07–08 | Médios mais definidos | Avaliar controle conjunto, sem assumir que sejam redundantes |
| Linguagem musical | Usuário descreve corpo, punch, definição e wah | Usar nomes provisórios acompanhados de grupo/índices no debug e contexto do TYPE 2 |
| EQ clássico/moderno | EQs funcionam, apresentação pode melhorar | Ambas as apresentações já implementadas; validar interação e legibilidade sem confundir EQ com coeficientes A/B |
| Referência e debug | Ganho/compensação e grupos mantêm bases de formas diferentes | Propriedades técnicas existem no ViewModel; eventual painel de dados precisa expor seu significado sem alegar que já está visível |
| Generalização | Um modelo identificado, sem ensaio equivalente dos demais | Repetir comparações controladas em outros modelos antes de tratar os rótulos como universais |
| Diagnóstico da falha | Silêncio/travamento sem captura de comunicação | Distinguir áudio ausente, resposta USB, conteúdo lido e recuperação necessária |

Para uma futura rodada de confirmação, registrar: amp/arquivo e hash, versão do app, cadeia de áudio, controles fixos, grupo e direção, percentual anterior/novo, retorno a 100%, resultado musical, presença de som e estado de comunicação. Esses registros são pendências; nenhum teste adicional foi executado aqui.

## 7. Fontes locais e conferência

- Relatos e esclarecimentos do usuário registrados na consolidação de 30/09/2026: TYPE 2, retorno dos outros faders a 100% e significado de “não pode passar de 100”.
- [Histórico técnico e de escuta](CUBE_BABY_AMPS.md), incluindo ensaios S01–S16 e interpretação dos EQs finais.
- [Tabelas de fábrica dos nove amps](CUBE_BABY_AMPS_FABRICA.md), referência de estrutura e conteúdo, não prova do estado durante este ensaio.
- Código atual: [AmpViewModel.cs](../../CubeControl-1.1/Cube/windows/Cube.Desktop/ViewModels/AmpViewModel.cs) (`Load`, `Build`, `GroupControls`, `EqControls`); [AmpImage.cs](../../CubeControl-1.1/Cube/windows/Cube.Protocol/AmpImage.cs); [CubeBabyProfile.cs](../../CubeControl-1.1/Cube/windows/Cube.Protocol/CubeBabyProfile.cs); [CubeSession.Amps.cs](../../CubeControl-1.1/Cube/windows/Cube.Protocol/CubeSession.Amps.cs); [AmpEditor.xaml](../../CubeControl-1.1/Cube/windows/Cube.Desktop/Views/AmpEditor.xaml). A referência anterior a `C:\CubeControl-1.1\Cube\windows` não corresponde ao diretório de fontes deste checkout.

As divisões de índices e os intervalos de bytes foram conferidos pelas fórmulas de quatro bytes por float. Os oito grupos cobrem exatamente 50 elementos por conjunto; os dezesseis grupos cobrem exatamente os 256 pontos da curva, sem lacunas nem sobreposição. As hipóteses sobre organização interna e causa das falhas permanecem explicitamente abertas.

A consolidação original produziu somente este documento, sem modificação do aplicativo ou comando ao pedal. A revisão de 01/10/2026 faz parte da [revisão documental do projeto](../../CubeControl-1.1/Cube/docs/revisao-documental.md), com verificações offline próprias; não repete os ensaios de escuta.
