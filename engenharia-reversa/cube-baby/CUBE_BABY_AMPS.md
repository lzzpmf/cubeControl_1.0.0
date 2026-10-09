# Cube Baby: investigação dos amps e preamps

[Documentação](../../CubeControl-1.1/Cube/docs/README.md) · [Criador atual](../../CubeControl-1.1/Cube/docs/referencias/criador-de-amps.md)

Pesquisa iniciada em 26/09/2026; diário com etapas posteriores, revisado documentalmente em 01/10/2026. Escopo: Cube Baby de guitarra, protocolo USB, pacote oficial do CubeSuite e leituras do pedal conectado. Este documento complementa [CUBE_BABY_COMANDOS.md](CUBE_BABY_COMANDOS.md).

## Navegação

- [1. Resultado principal](#1-resultado-principal)
- [2. Quais são os nove modelos](#2-quais-são-os-nove-modelos)
- [3. Controles, modos e cadeia](#3-controles-modos-e-cadeia)
- [4. Pacote oficial: arquivos relevantes](#4-pacote-oficial-arquivos-relevantes)
- [5. Leituras físicas realizadas](#5-leituras-físicas-realizadas)
- [6. Estrutura interna observada de cada bloco](#6-estrutura-interna-observada-de-cada-bloco)
- [7. O que se pode concluir sobre a arquitetura](#7-o-que-se-pode-concluir-sobre-a-arquitetura)
- [8. Situação inicial da importação de amps](#8-situação-inicial-da-importação-de-amps)
- [9. O que os testes anteriores esclarecem sobre amps](#9-o-que-os-testes-anteriores-esclarecem-sobre-amps)
- [10. Limites registrados na etapa inicial](#10-limites-registrados-na-etapa-inicial)
- [11. Evidência reproduzível](#11-evidência-reproduzível)
- [12. Sessão de escuta live — 26/09/2026](#12-sessão-de-escuta-live--26092026)
- [13. Endereço gravável da cópia de trabalho do amp — investigação local](#13-endereço-gravável-da-cópia-de-trabalho-do-amp--investigação-local)
- [14. T21 — dados do TYPE 5 na estrutura live do primeiro modelo](#14-t21--dados-do-type-5-na-estrutura-live-do-primeiro-modelo)
- [15. P01 — gravação autorizada do primeiro amp na flash](#15-p01--gravação-autorizada-do-primeiro-amp-na-flash)
- [16. P02 — persistência e funcionamento após desligar/ligar](#16-p02--persistência-e-funcionamento-após-desligarligar)
- [17. G01/G02 — redução da tabela candidata a ganho em LIVE](#17-g01g02--redução-da-tabela-candidata-a-ganho-em-live)
- [18. C01/C02 — tabela candidata a compensação](#18-c01c02--tabela-candidata-a-compensação)
- [19. F01 — primeiro conjunto de coeficientes](#19-f01--primeiro-conjunto-de-coeficientes)
- [20. W01 — curva de 256 pontos linear](#20-w01--curva-de-256-pontos-linear)
- [21. S00/S01 — oito partes de 32 pontos, contrastes de ±100%](#21-s00s01--oito-partes-de-32-pontos-contrastes-de-100)
- [22. R01–R03 — restauração do amp 0 original](#22-r01r03--restauração-do-amp-0-original)
- [23. Comparação dos nove relatos com os números originais](#23-comparação-dos-nove-relatos-com-os-números-originais)
- [24. Rockerverb clean: prototipo RV01 em RAM](#24-rockerverb-clean-prototipo-rv01-em-ram)

## 1. Resultado principal

**Último estado registrado neste diário:** a flash do primeiro amp foi restaurada ao clean original na seção 22. Depois disso, os protótipos da seção 24 voltaram a modificar somente a RAM; RV06 foi aprovado na escuta e ficou aplicado ao final daquele ensaio. Não houve nova leitura do pedal na revisão documental de 01/10/2026, portanto este registro não informa seu estado atual.

As expressões “estado atual”, “pendente” e “não realizado” dentro das etapas abaixo referem-se ao momento de cada ensaio. Resultados posteriores podem resolver pendências anteriores. Para funcionalidades e limitações do aplicativo atual, consulte o guia do criador.

**Foi comprovada a substituição persistente do primeiro amp:** o setor `00:0x60000`, com 4096 bytes, recebeu os dados do TYPE 5; após desligar e ligar, a leitura confirmou integralmente os bytes gravados e o usuário confirmou o som distorcido, percebido como idêntico ao amp de origem. Procedimento, falha inicial recuperada e confirmação pós-reinicialização estão nas seções 15 e 16.

Na investigação inicial foram encontrados nove blocos de 4096 bytes em `00:0x60000–0x68FFF`. Antes da modificação deliberada, seus 36.864 bytes eram idênticos aos primeiros nove blocos de `BFI0615.bin` e à região correspondente de `cubebaby_A_20220706.up`. Esse retrato identifica os dados originais: o primeiro slot foi alterado em P01/P02 e restaurado na seção 22.

Existem dados estruturados de amp acessíveis e graváveis por USB: a edição na estrutura de trabalho alterou o som em live, e a substituição do primeiro setor flash persistiu e foi utilizada após reinicialização. Isso não comprova compatibilidade com formatos externos, como NAM, nem valida a escrita em qualquer endereço ou modelo de pedal. As seções iniciais preservam o histórico da investigação; suas pendências de escrita do primeiro amp foram resolvidas pelos testes posteriores.

### Graus de certeza

- **Medido:** bytes recebidos do pedal, checksum, endereços, tamanhos, hashes e comparações de arquivos.
- **Documentado:** funções e nomes publicados pelo fabricante ou implementados nas referências de protocolo.
- **Inferido:** associação semântica de campos internos, arquitetura DSP e correspondência entre um setor e um nome de amp.
- **Desconhecido:** comportamento que exigiria rastrear o firmware, alterar dados ou medir áudio.

## 2. Quais são os nove modelos

O manual oficial distingue dois sons limpos, três de overdrive e quatro de distorção. A tabela abaixo abrevia os nomes, removendo os prefixos e sufixos de categoria. A ordem é a do manual; a associação com `type=0–8` é feita pelo índice, não por identificação textual retornada pelo pedal. [S1][S3]

| Posição TYPE | Valor USB | Nome abreviado do manual | Categoria | Setor associado pela ordem |
|---:|---:|---|---|---|
| 1 | 0 | Power-Zone | Limpo | `0x00060000` |
| 2 | 1 | US Gold 100 | Limpo | `0x00061000` |
| 3 | 2 | Two Stone Coral | Overdrive | `0x00062000` |
| 4 | 3 | Doctor3 B | Overdrive | `0x00063000` |
| 5 | 4 | Cali JP A | Overdrive | `0x00064000` |
| 6 | 5 | Day Tripper | Distorção | `0x00065000` |
| 7 | 6 | Shittcow | Distorção | `0x00066000` |
| 8 | 7 | Wo Stone Coral | Distorção | `0x00067000` |
| 9 | 8 | Mr Smith | Distorção | `0x00068000` |

O nome do sexto modelo contém um sufixo de overdrive no original, embora a coluna de categoria o classifique como distorção. O oitavo aparece como “Wo”, diferente de “Two” no terceiro. Essas inconsistências pertencem à fonte; não foram corrigidas por adivinhação. Não foi localizada uma correspondência oficial inequívoca desses nomes com modelos comerciais de Marshall, Fender, Mesa etc. [S1]

Não confundir essa lista com a lista de gabinetes: nomes de fabricantes encontrados em IRs não identificam automaticamente o preamp correspondente.

## 3. Controles, modos e cadeia

O manual coloca o preamp antes de modulação, delay, reverb e gabinete. TYPE escolhe o modelo, GAIN regula ganho e TONE regula brilho. Em LIVE/EDIT, o footswitch C controla a seção TONE/AMP; em PRESET, A/B/C selecionam presets. O editor pode alterar um preset diferente do que está audível no pedal. [S1]

O protocolo conhecido contém estes campos de um byte:

| Campo | Offset no slot de 16 bytes | Endereço live A | Endereço live B | Endereço live C | Faixa da referência |
|---|---|---|---|---|---|
| `type` | `0x00` | `0x80000000` | `0x80000010` | `0x80000020` | 0–8 |
| `gain` | `0x01` | `0x80000001` | `0x80000011` | `0x80000021` | 0–7 |
| `tone` | `0x02` | `0x80000002` | `0x80000012` | `0x80000022` | 0–15 |
| `volume` | `0x05` | `0x80000005` | `0x80000015` | `0x80000025` | 0–127 |
| `toneSection` | `0x0C` | `0x8000000C` | `0x8000001C` | `0x8000002C` | 0/1 |

Seletor de memória: `05`. Escrita live: opcode `22`, endereço do campo, tamanho 1 e valor. Persistência: banco completo de 48 bytes em memória `05`, endereço zero. Esses endereços de parâmetros **não são os endereços dos dados dos modelos** em memória `00`. [S3][S4]

As faixas não representam dB, Hz ou percentuais. Não foi demonstrada a curva de TONE, a posição exata de VOLUME no DSP, nem se todos os modelos usam GAIN da mesma forma. `type=0` seleciona o primeiro modelo; não significa bypass. `toneSection` é a flag de habilitação conhecida. Escolher o endereço do slot B/C para editar não é uma ordem comprovada de acionamento do footswitch.

### Evidência nas capturas públicas

As capturas foram decodificadas novamente, com estes resultados: [S4]

| Captura | Escritas host → pedal | Endereço | Valores presentes | Respostas |
|---|---:|---|---|---|
| `014-parameter-type` | 19 | `0x80000000` | Todos de 0 a 8 | 19 ACKs |
| `004-parameter-gain` | 9 | `0x80000001` | 3–7 | 9 ACKs |
| `006-parameter-tone` | 14 | `0x80000002` | Subconjunto entre 2 e 13 | 14 ACKs |
| `017-parameter-tone-section` | 8 | `0x8000000C` | Alternância 0/1 | 8 ACKs |

Ao mudar TYPE nessas capturas, o host envia somente um byte de seleção. Não transmite um modelo de amp inteiro a cada mudança. Isso é compatível com o carregamento interno de um dos modelos já armazenados.

## 4. Pacote oficial: arquivos relevantes

O ZIP foi obtido pelo link de Windows publicado no centro de downloads do fabricante. Foi inspecionado em memória, sem instalar ou executar o programa e sem manter cópias dos binários no projeto. [S2]

| Arquivo dentro do ZIP | Tamanho | Resultado da inspeção |
|---|---:|---|
| `CubeSuite/CubeSuite.exe` | 3.945.984 bytes | Editor Windows com suporte a vários dispositivos; classes e strings específicas de Cube Baby, CubeSugar e DigAmp. |
| `CubeSuite/bin/BFI.bin` | 69.632 bytes | 17 setores de 4096 bytes. Os nove primeiros diferem do pedal; os oito seguintes possuem estrutura de gabinete. |
| `CubeSuite/bin/BFI0615.bin` | 69.632 bytes | Os primeiros nove setores são idênticos aos nove setores lidos do pedal. |
| `CubeSuite/bin/BFIN.bin` | 32.768 bytes | Oito setores com estrutura compatível com IR; não é evidência de nove amps adicionais. |
| `CubeSuite/bin/up/cubebaby_A_20220706.up` | 504.600 bytes | Atualização contendo exatamente os mesmos nove setores encontrados no pedal. Não foi aplicada. |

O nome do arquivo `.up` contém uma data, mas **isso não identifica por si só o firmware instalado**. O pacote também contém arquivos para outros aparelhos. A presença de um `.bin` no ZIP não o torna compatível com a Cube Baby.

### Correspondência entre três fontes independentes de bytes

```text
Pedal, memória 00:       [0x60000 .. 0x68FFF]   36.864 bytes
BFI0615.bin:            [0x00000 .. 0x08FFF]   36.864 bytes
cubebaby_A_20220706.up: [0x60100 .. 0x690FF]   36.864 bytes

Comparação byte a byte: iguais
SHA-256 do conjunto:
e90872f17992c2f01d307ee159bb1e95ac59581f797bcf1254d0eb866239145e
```

O deslocamento `+0x100` entre endereço do pedal e offset no `.up` foi confirmado **nessa região**. Não foi presumido como regra de todas as seções do arquivo.

### A referência a importação de amps no executável

As strings de importação de amp aparecem agrupadas com `DigAmp`, `m_importAmpData`, `m_openAmpFile`, o filtro `*.ampNGData` e a configuração `/Config/OpenDigAmpFileDir`. A RTTI também identifica uma classe `DigAmp` separada de `CCubeBabyIREditDlg`.

Já os metadados da classe de IR da Cube Baby contêm operações como `OnOpenIRFile`, `OnSave`, `OnFactoryIR` e `OnResetSound`.

**Conclusão limitada:** o executável inclui uma interface de importação de amps no contexto DigAmp; não foi demonstrado um caminho de execução dessa interface para a Cube Baby. Uma busca textual por “Import Amp Data” no executável compartilhado não confirma suporte neste pedal. Não foi reconstruído o grafo completo de chamadas do programa. [S2]

## 5. Leituras físicas realizadas

Dispositivo: `USB-Midi`, `VID_301A&PID_5555`, identificado como `SINCO-CubeBaby`. O transporte foi o WinMM, com SysEx de sete bits conforme o protocolo conhecido. Não foram instaladas dependências.

Foram realizados:

1. Identificação e leitura dos 48 bytes do banco de presets.
2. Leitura do descritor de 20 bytes em memória `00`, endereço `0x2100`.
3. Leitura inicial dos 64 bytes de cada um dos nove setores candidatos e do primeiro setor de gabinete conhecido.
4. Leitura integral dos nove setores candidatos, em partes de até 113 bytes: 37 requisições por setor, 333 no total, cobrindo 36.864 bytes.
5. Conferência de seletor, endereço, tamanho e checksum de cada resposta.
6. Comparação integral com os arquivos oficiais, nova leitura dos presets e identificação final.

Todos os setores completos corresponderam a `BFI0615.bin`; nenhum correspondeu ao respectivo setor de `BFI.bin`. Os presets permaneceram idênticos antes e depois. Nenhuma escrita, mudança live de amp, apagamento ou atualização foi enviada. As portas foram fechadas ao fim.

### Descritor lido em `0x2100`

Payload de dados, sem envelope:

```text
01 00 00 00 A2 4E 00 00 00 00 03 00 00 00 06 00 00 90 06 00
```

Interpretado como cinco inteiros LE32: `1`, `0x4EA2`, `0x30000`, `0x60000`, `0x69000`. Os dois últimos coincidem com a base dos nove setores investigados e a base conhecida dos gabinetes. A função dos demais campos não foi determinada. A coincidência reforça o mapa, mas não define sozinha a estrutura completa desse descritor.

### Exemplo de leitura de um setor candidato

```text
# Envelope decodificado: ler 64 bytes em memória 00, endereço 00060000
00 59 23 08 00 00 00 00 00 06 00 40 00 00 B9
```

Esse envelope requer empacotamento de sete bits e delimitadores `F0/F7` para USB. Foi usado apenas o opcode de leitura `0x23` nos acessos à região dos modelos.

## 6. Estrutura interna observada de cada bloco

Os offsets abaixo são relativos ao começo de um setor de 4096 bytes. A análise refere-se aos dados efetivamente lidos, que correspondem a `BFI0615.bin`; não generalizar para `BFI.bin` ou outros firmwares.

| Offset relativo | Tamanho | Observação medida | Interpretação |
|---|---:|---|---|
| `0x000–0x00B` | 12 bytes | Cabeçalho `01 07 00 00 00 00 00 00 00 00 00 00` nos nove blocos | Significado dos bytes não comprovado; `07` não deve ser chamado automaticamente de contagem de filtros. |
| `0x00C–0x02B` | 32 bytes | Oito float32 LE positivos, geralmente crescentes | Candidatos a uma tabela de ganho para os oito valores de GAIN; correlação estrutural, sem rastreamento do DSP. |
| `0x02C–0x04B` | 32 bytes | Oito float32 LE constantes ou decrescentes | Candidatos a compensação de nível associada ao ganho; não confundir com o parâmetro VOLUME do preset. |
| `0x04C–0x04F` | 4 bytes | Zero nos blocos inspecionados | Reservado/flag/valor ainda desconhecido. |
| `0x050–0x117` | 200 bytes | 50 float32, que podem ser agrupados em dez conjuntos de cinco | Estrutura compatível com coeficientes de filtros biquad; ordem, sinal e execução não confirmados. |
| `0x118–0x11B` | 4 bytes | Valor variável entre modelos | Não interpretado. |
| `0x11C–0x51B` | 1024 bytes | 256 float32; valores entre 0 e 1 nos nove blocos | Forte candidato a tabela de transferência/saturação, não a um IR de gabinete. A utilização pelo código ainda é hipótese. |
| `0x51C–0x5E3` | 200 bytes | Outros 50 float32, agrupáveis em dez conjuntos de cinco | Possível segundo conjunto de filtros. Não confirma ordem antes/depois da saturação. |
| `0x5E4–0x62B` | 72 bytes | Campos numéricos e bytes de interpretação mista | Não tratar toda a região como floats só porque parte dela produz valores plausíveis. |
| `0x62C–0xFFF` | 2516 bytes | Todos zero nos nove setores | Preenchimento observado; não comprova espaço livre utilizável pelo DSP. |

Em todos os setores, o último byte não zero foi o offset decimal 1579 (`0x62B`). O prefixo de 1580 bytes contém os dados não nulos, mas **não foi estabelecido que o firmware leia exatamente 1580 bytes**.

### Tabela numérica em `0x00C`: candidata a ganho

Valores arredondados para facilitar leitura. As colunas são índices da tabela; a associação ao knob GAIN é inferida.

| TYPE USB | Índice 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 |
|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| 0 | 0,007 | 0,008 | 0,009 | 0,020 | 0,030 | 0,040 | 0,050 | 0,060 |
| 1 | 0,007 | 0,008 | 0,009 | 0,020 | 0,030 | 0,040 | 0,050 | 0,060 |
| 2 | 0,5 | 1 | 2 | 3 | 5 | 7 | 10 | 15 |
| 3 | 12 | 12 | 16 | 20 | 26 | 30 | 38 | 45 |
| 4 | 0,5 | 1 | 2 | 3 | 5 | 7 | 10 | 15 |
| 5 | 0,5 | 1 | 5 | 10 | 20 | 35 | 50 | 60 |
| 6 | 30 | 40 | 50 | 60 | 70 | 80 | 90 | 100 |
| 7 | 2 | 5 | 10 | 20 | 40 | 60 | 80 | 100 |
| 8 | 30 | 40 | 50 | 60 | 70 | 80 | 90 | 100 |

Não são valores medidos de ganho em dB. Não comparar a sonoridade dos modelos usando apenas esses números: filtros, normalizações e outros campos também diferem.

### Tabela em `0x02C`: candidata a compensação

| TYPE USB | Oito valores |
|---:|---|
| 0 | `2` em todas as posições |
| 1 | `1,5` em todas as posições |
| 2 | `0,14` em todas as posições |
| 3 | `0,34; 0,30; 0,28; 0,25; 0,23; 0,22; 0,22; 0,22` |
| 4 | `0,20; 0,18; 0,15; 0,145; 0,14; 0,13; 0,13; 0,13` |
| 5 | `0,12` em todas as posições |
| 6 | `0,13` em todas as posições |
| 7 | `0,15` em todas as posições |
| 8 | `0,16` em todas as posições |

### Curvas e semelhanças entre modelos

A região `0x11C–0x51B` começa em zero e aproxima-se de 1. Nem todas as curvas são estritamente crescentes: TYPE 6/8 têm 95 passos decrescentes, e TYPE 0/5 têm 18. Portanto, não simplificar automaticamente a curva para uma função `tanh` ou presumir uma tabela monotônica.

Comparações byte a byte dos blocos completos:

| Par de TYPEs | Curva de 256 floats | Diferenças no setor de 4096 bytes | Implicação limitada |
|---|---|---:|---|
| 0 e 5 | Idêntica | 398 bytes | Curva compartilhada, com diferenças importantes em outras regiões. |
| 2 e 7 | Idêntica | 63 bytes | A faixa `0x04C–0x603` também é idêntica; diferem tabelas iniciais e parte final. |
| 6 e 8 | Idêntica | 45 bytes | A faixa `0x04C–0x603` também é idêntica; diferem a segunda tabela inicial e parte final. |

Isso é evidência de reutilização de dados entre modelos. Não prova que sejam o mesmo som, nem que baste mudar ganho para reproduzir um pelo outro.

## 7. O que se pode concluir sobre a arquitetura

**Medido:** existem cabeçalhos, duas tabelas de oito floats, conjuntos compatíveis com coeficientes e uma curva de 256 pontos por modelo; o mesmo conteúdo está no pedal, em um arquivo de dados e no pacote de firmware.

**Inferência técnica:** um mecanismo comum pode combinar ganho, filtros e uma tabela de saturação, usando conjuntos diferentes de dados para os nove modelos. Essa estrutura é compatível com modelagem por filtros e waveshaping. Não foi identificado o algoritmo em execução, sua ordem de estágios, o tratamento da metade negativa da curva, interpolação, oversampling, estados internos ou resposta dinâmica.

**Não demonstrado:** uso de redes neurais, NAM, captura de amplificador, simulação de circuito ou uma implementação específica de biquads. Também não foi identificado com confiança o DSP/SoC e sua capacidade a partir do material acessível. Os documentos FCC de fotos internas foram localizados, mas o acesso retornou bloqueio HTTP 403; nenhuma identificação de chip foi deduzida dessas fotos não inspecionadas.

## 8. Situação inicial da importação de amps

Esta seção registra o conhecimento antes dos ensaios de escrita. As seções 14–16 comprovam substituição RAM/flash nesse aparelho; a aplicação atual implementa importação de setores `.bin`, prévia e gravação, com as limitações documentadas no [guia do criador](../../CubeControl-1.1/Cube/docs/referencias/criador-de-amps.md). Formatos externos como NAM continuam sem suporte.

O gabinete possui um formato e uma sequência conhecidos de transferência de amostras IR. O modelo de amp usa um bloco diferente, com parâmetros e curvas cuja semântica ainda está parcialmente desconhecida. Um WAV de gabinete não é intercambiável com esse bloco. [S3][S5]

Na etapa inicial, a descoberta permitia distinguir quatro situações:

| Operação | Estado do conhecimento |
|---|---|
| Escolher um dos nove amps | Comando de seleção conhecido e capturado. |
| Ajustar GAIN/TONE e salvar o preset | Campos e persistência conhecidos. |
| Ler os dados internos associados aos nove amps | Realizado no pedal nesta investigação. |
| Gravar um amp modificado ou externo e fazê-lo funcionar | Não realizado e não confirmado. |

Já naquela etapa não era correto afirmar que novos amps exigiriam necessariamente substituir todo o firmware. **Modificar apenas tabelas de um modelo é uma possibilidade técnica mais concreta após esta descoberta**, mas depende de permissões de escrita, formato válido e mecanismo de recarga ainda não confirmados.

Também não é correto concluir que a existência de `0x22` e `0x21` torna todos os setores graváveis. Esses comandos foram observados em outras regiões; sua aceitação e os efeitos na região dos amps não foram testados. A memória acessível por leitura pode ter restrições distintas de escrita.

O fato de o arquivo `.up` transportar os blocos demonstra que são dados distribuídos na atualização. Não prova que o atualizador aceite modelos arbitrários, nem que o editor ofereça importação individual.

## 9. O que os testes anteriores esclarecem sobre amps

As consultas `0x13`, `0x17`, `0x1B` e `0x20`, testadas anteriormente, responderam somente status `01`. Elas não retornaram versão, tipo ou endereços adicionais úteis para interpretar amps. A função desses comandos continua não confirmada neste firmware. O status não demonstra bloqueio à importação de modelos. [S6]

O banco de presets lido nesta investigação contém:

| Preset | TYPE bruto | GAIN bruto | TONE bruto | VOLUME bruto | Seção AMP |
|---|---:|---:|---:|---:|---:|
| A | 2 | 4 | 7 | 127 | 1 |
| B | 4 | 2 | 5 | 70 | 1 |
| C | 1 | 7 | 7 | 111 | 1 |

Essa é uma fotografia dos presets armazenados em 26/09/2026, não dos padrões de fábrica nem uma identificação do preset atualmente audível. Não foi feita medição de áudio, e nenhuma alteração live foi enviada durante a investigação dos amps.

## 10. Limites registrados na etapa inicial

As pendências de escrita, persistência e recuperação do primeiro setor foram resolvidas nas seções 14–16 e 22. A tabela desta etapa não deve ser usada para declarar esses recursos inexistentes na aplicação atual.

- A leitura integral e a igualdade dos bytes estão confirmadas; a associação semântica de cada campo ainda exige evidência adicional.
- Os nomes vêm do manual, não de strings dentro dos setores. A ligação de cada nome ao setor segue a ordem dos nove índices e não foi validada por comparação acústica.
- Não foram identificados limites seguros de valores internos, checksums locais de modelo, assinatura necessária ou protocolo de recarga de amp.
- Não foi testada a escrita em `0x60000–0x68FFF`, nem a persistência e recuperação de um modelo modificado.
- Os nomes de arquivo e a igualdade dos dados não bastam para identificar a revisão completa do firmware instalado.
- Não foram medidos ganho real, equalização, ruído, latência, aliasing, compressão ou fidelidade a amplificadores comerciais.

## 11. Evidência reproduzível

### Hashes das fontes oficiais inspecionadas

| Artefato | SHA-256 |
|---|---|
| ZIP CubeSuite, 14.785.970 bytes | `d9a1add312ae3fa82f93fefd32a723e541536dd7ed9b2434db364fe8c5a17423` |
| `CubeSuite.exe` | `0e0183fd6dc1b347534c9ce1e3677ecda2a6ae77fe3da8d6314d43c7c2cfd2e6` |
| `BFI.bin` | `2023ee73672e43a0381a37ae5ecb7b1e298836e84b052df609d4ef9509ee37e5` |
| `BFI0615.bin` | `4f78d5d8d967276a6f069b52887df87f9e8ce56a53dc89b368a2e335aeb53f97` |
| `cubebaby_A_20220706.up` | `d99a9bc4ba0636118801b1e3a8d11adadf03831d316c7477af8de015ba5bd743` |

O endereço de download pode receber versões novas. Os hashes identificam os bytes efetivamente analisados, não uma promessa sobre downloads futuros.

### Hashes dos setores completos recebidos do pedal

| Endereço em memória `00` | SHA-256 dos 4096 bytes |
|---|---|
| `0x60000` | `2cef59f28050aa30e5e7b475b5d8e3ac0e0139fbf5fb9f96c363a7c0cc7d308b` |
| `0x61000` | `ca3c62961ba95cbcd2d8313925aeff2579dc429e07a7a4a46d626a9fe6961da7` |
| `0x62000` | `546417480944ea75cea9d861acb02358c6f459d22e82144632b7f496b62f1507` |
| `0x63000` | `45105d090aaf92ee60d710ec24d0680109af603c46498a840045bf60512e04a2` |
| `0x64000` | `19d2037ff4d44a96543b4d98570c38a03510f8715e533ebe05b2bc805b014dd9` |
| `0x65000` | `c6b6c68433f84e9dca369ca3e2ee77fdaf8491645df05a90ff41f03033ed138e` |
| `0x66000` | `8e5ebd94b7aa945bd84ccca315664ad199a09fc3c3308685e2fc229605819f15` |
| `0x67000` | `edc243b00b9a900dc687eb1b006420cde296ff896027101a59b24f3a7a9692f3` |
| `0x68000` | `60aa9d8cfa1a09094c9f6aa9c068801fc07b65a9b13aab2b41a6267eab2004e4` |

### Fontes

- **[S1]** [Manual oficial Cube Baby](https://manualf.oss-cn-hongkong.aliyuncs.com/manual/Combined-Effect-Pedals/CUBE-BABY.pdf), 28 páginas: páginas do PDF 5, 8–12 para modelos, modos e interface; link localizado em [products-data.js do fabricante](https://www.cuvave.com/products-data.js). A tabela foi conferida no texto das seções inglesa e chinesa; a tentativa de renderização web não forneceu inspeção visual utilizável.
- **[S2]** [Centro de downloads](https://www.cuvave.com/download) e [ZIP oficial do CubeSuite para Windows](https://yms-file-store.oss-cn-hongkong.aliyuncs.com/software/pc/CubeSuite.zip?x-oss-traffic-limit=5192000). Arquivos e offsets citados foram inspecionados diretamente no ZIP.
- **[S3]** [Mapa de parâmetros CubeControl](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/live.ts), [presets](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/preset.ts) e [construtores de mensagens](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/requests.ts).
- **[S4]** [Capturas públicas CubeControl](https://github.com/MrGariZack/cubecontrol/tree/0a06a5bfedd5d39259c080922e9face1376db0d9/test/fixtures/captures), especialmente `004`, `006`, `014` e `017`.
- **[S5]** [Formato de IR implementado](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/ir.ts).
- **[S6]** [Relatório local de comandos e consultas físicas](CUBE_BABY_COMANDOS.md), seção 10. As novas leituras da região dos amps estão documentadas neste arquivo, não naquele relatório anterior.

O Android deste projeto não fornece o processamento dos amps: [CubeBaby.java](../apk-cubesuite/sources/com/cubeSuite/entity/Instrument/CubeBaby.java) está vazio. [Communication.java](../apk-cubesuite/sources/com/cubeSuite/communication/Communication.java) fornece apenas operações genéricas de comunicação. O pacote Windows e a comparação física foram necessários para avançar além desse limite.

## 12. Sessão de escuta live — 26/09/2026

O usuário informou estar conectado e pronto, sem ajustes a preservar. A saída de escuta ainda não foi informada. A identificação USB confirmou `SINCO-CubeBaby`. Antes das alterações, os nove setores foram relidos e mantiveram os hashes da seção 11. O banco inicial manteve os valores da seção 9.

| Teste | Alteração enviada | Retorno do pedal | Relato de escuta | Estado após o teste |
|---|---|---|---|---|
| T01 | Volume A: 127 → 115, seletor `05`, endereço live `0x80000005`, opcode `22`, um byte | ACK `00`; leitura mostrou 115 | Usuário: “parece que ouvi sim”, solicitando redução maior para confirmar | Restaurado a 127 por escrita live; os 48 bytes lidos voltaram ao estado inicial |
| T02 | Volume A: 127 → 64, mesmo comando/endereço, a pedido do usuário | ACK `00`; leitura mostrou 64, demais 47 bytes inalterados | Usuário confirmou: “ficou evidente” | Mantido em 64 para os testes de timbre; valor original para restauração: 127 |
| T03 | GAIN A: 4 → 0, seletor `05`, endereço live `0x80000001`, opcode `22`, um byte | ACK `00`; leitura mostrou 0, demais 47 bytes inalterados | Usuário confirmou: “sim mudou”; não especificou quais características mudaram | GAIN ficou em 0 até T04; TYPE 2, TONE 7 e volume 64; GAIN original para restauração: 4 |
| T04 | GAIN A: 0 → 7, mesmo comando/endereço | ACK `00`; leitura mostrou 7, demais 47 bytes inalterados | Usuário: “sim confirmado”; confirmação geral da comparação, sem quantificar volume nem separar distorção e sustain | GAIN mantido em 7; TYPE 2, TONE 7 e volume 64; GAIN original para restauração: 4 |
| T05 | TONE A: 7 → 0, seletor `05`, endereço live `0x80000002`, opcode `22`, um byte | ACK `00`; leitura mostrou 0, demais 47 bytes inalterados | Usuário: “sim continue”, em resposta à comparação de abafamento/agudos; sem descrição adicional | TONE ficou em 0 até T06; TYPE 2, GAIN 7 e volume 64; TONE original para restauração: 7 |
| T06 | TONE A: 0 → 15, mesmo comando/endereço | ACK `00`; leitura mostrou 15, demais 47 bytes inalterados | Usuário: “sim continue”, em resposta à comparação de brilho/estridência; sem distinguir essas características | TONE mantido em 15; TYPE 2 até T07, GAIN 7 e volume 64; TONE original para restauração: 7 |
| T07 | TYPE A: 2 → 0, seletor `05`, endereço live `0x80000000`, opcode `22`, um byte | ACK `00`; leitura mostrou 0, demais 47 bytes inalterados | Usuário descreveu o som como “limpo” | TYPE ficou em 0 até T08; GAIN 7, TONE 15 e volume 64; TYPE original para restauração: 2 |
| T08 | TYPE A: 0 → 5, mesmo comando/endereço | ACK `00`; leitura mostrou 5, demais 47 bytes inalterados | Usuário: “distorçao e volume mais alto”, em comparação ao TYPE 0 | TYPE mantido em 5; GAIN 7 até T09, TONE 15 e volume 64; TYPE original para restauração: 2 |
| T09 | GAIN A: 7 → 0 no TYPE 5, seletor `05`, endereço live `0x80000001`, opcode `22`, um byte | ACK `00`; leitura mostrou 0, demais 47 bytes inalterados | Usuário: “mutou” (som inaudível); não foi medida amplitude | GAIN ficou em 0 até T10; TYPE 5, TONE 15 e volume 64; GAIN original para restauração: 4 |
| T10 | GAIN A: 0 → 7 no TYPE 5, mesmo comando/endereço, para verificar retorno do som | ACK `00`; leitura mostrou 7, demais 47 bytes inalterados; banco completo igual ao final de T08 | Usuário confirmou o retorno: “sim voultou” | GAIN ficou em 7 até T11; TYPE 5, TONE 15 e volume 64; GAIN original para restauração: 4 |
| T11 | GAIN A: 7 → 1 no TYPE 5, seletor `05`, endereço live `0x80000001`, opcode `22`, um byte | ACK `00`; leitura mostrou 1, demais 47 bytes inalterados | Usuário: “quase inaudivel”; não caracterizou a distorção nesse nível | GAIN ficou em 1 até T12; TYPE 5, TONE 15 e volume 64; GAIN original para restauração: 4 |
| T12 | GAIN A: 1 → 4 no TYPE 5, mesmo comando/endereço | ACK `00`; leitura mostrou 4, demais 47 bytes inalterados | Usuário: “claramente audivel”; não informou se limpo ou distorcido | GAIN mantido em 4; TYPE 5 até T13, TONE 15 e volume 64 |
| T13 | TYPE A: 5 → 0 com GAIN 4, seletor `05`, endereço live `0x80000000`, opcode `22`, um byte | ACK `00`; leitura mostrou 0, demais 47 bytes inalterados | Usuário: “limpo”; comparação de volume não informada | TYPE ficou em 0 até T14; GAIN 4, TONE 15 e volume 64; TYPE original para restauração: 2 |
| T14 | TYPE A: 0 → 2 com GAIN 4, mesmo comando/endereço | ACK `00`; leitura mostrou 2, demais 47 bytes inalterados | Usuário: “saturado” | TYPE ficou em 2 até T15; GAIN 4, TONE 15 e volume 64; referência para comparação com TYPE 7 |
| T15 | TYPE A: 2 → 7 com GAIN 4, mesmo comando/endereço | ACK `00`; leitura mostrou 7, demais 47 bytes inalterados | Usuário: “mais saturacao e volume”, em relação ao TYPE 2 | TYPE ficou em 7 até T16; GAIN 4, TONE 15 e volume 64; TYPE original para restauração: 2 |
| T16 | TYPE A: 7 → 6 com GAIN 4, mesmo comando/endereço | ACK `00`; leitura mostrou 6, demais 47 bytes inalterados | Usuário: “mesma saturacao porem mais agudo mais abelhudo”, em relação ao TYPE 7; volume não informado | TYPE ficou em 6 até T17; GAIN 4, TONE 15 e volume 64; referência para comparação com TYPE 8 |
| T17 | TYPE A: 6 → 8 com GAIN 4, mesmo comando/endereço | ACK `00`; leitura mostrou 8, demais 47 bytes inalterados | Usuário: “parecido com o anterior porem com mais medios”; volume não informado | TYPE mantido em 8; GAIN 4, TONE 15 até T18 e volume 64; TYPE original para restauração: 2 |
| T18 | TONE A: 15 → 7 no TYPE 8, seletor `05`, endereço live `0x80000002`, opcode `22`, um byte | ACK `00`; leitura mostrou 7, demais 47 bytes inalterados | Usuário: “diminuiu ficou mais fechado e escuro”, sobre o caráter abelhudo | TYPE 8 até T19, GAIN 4, TONE 7 e volume 64 |
| T19 | TYPE A: 8 → 2, seletor `05`, endereço live `0x80000000`, opcode `22`, um byte | ACK `00`; leitura mostrou 2; banco igual ao inicial exceto volume A 64 em vez de 127 | Preparação para verificação após desligar/ligar, ainda pendente | TYPE 2, GAIN 4, TONE 7 e volume 64; nenhum comando de salvar enviado |
| T20 | Após usuário informar “religuei”: somente identificação `11` e leitura `23`, seletor `05`, endereço zero, 48 bytes | Volume A voltou a 127; todos os 48 bytes iguais ao banco inicial, sem diferenças | Reinicialização física informada pelo usuário; nenhuma nova avaliação acústica solicitada | TYPE 2, GAIN 4, TONE 7 e volume 127; banco inicial restaurado |

T08 compara dois modelos cujas curvas de 256 floats coincidem (seção 7), mas cujos setores têm outros bytes diferentes. Uma diferença audível não identifica isoladamente a função desses bytes nem comprova que a curva seja usada da mesma maneira pelo DSP. Nenhuma tabela foi editada.

Resultado T14/T15: com os demais ajustes constantes, TYPE 2 foi descrito como saturado e TYPE 7 com mais saturação e volume. Esse segundo par também compartilha a curva identificada, mas não todo o bloco de dados. O relato reforça que a curva isolada não determina o som completo; não identifica quais diferenças internas causam o resultado.

Resultado T16/T17: TYPE 8 foi descrito como parecido com TYPE 6, porém com mais médios, mantendo GAIN 4, TONE 15 e volume 64. Os modelos compartilham a curva identificada, mas a diferença percebida não permite atribuir frequências, filtros ou coeficientes específicos. T18 reduz somente TONE no TYPE 8 para comparar sua influência no caráter agudo relatado.

Em T18, a redução de TONE tornou o som mais fechado e escuro e diminuiu o caráter abelhudo, segundo o usuário. Em T19, modelo, ganho e tone voltaram aos valores iniciais. Somente volume A permanece diferente (64 contra 127), para verificar se retorna ao valor inicial após um desligamento real. A reinicialização física e a leitura posterior ainda estão pendentes; não há conclusão de persistência neste ponto.

**Resultado posterior de T20:** após o desligamento/religamento informado pelo usuário, a leitura retornou volume A 127, descartando o ajuste live 64. O banco completo de 48 bytes correspondeu exatamente ao início da sessão. Nessa etapa foram enviados somente identificação e leitura. Isso confirma que a alteração live de volume testada não sobreviveu à reinicialização; não comprova isoladamente a volatilidade de TYPE, GAIN e TONE, que já haviam sido restaurados antes de desligar, nem autoriza extrapolar para escritas em tabelas internas. A pendência de reinicialização mencionada em T19 está resolvida.

Resultado de escuta T07/T08: com os mesmos valores de GAIN, TONE, volume e gabinete, o usuário descreveu TYPE 0 como limpo e TYPE 5 com distorção e maior volume. Portanto, a igualdade da curva identificada não basta para prever igualdade de som nesses ajustes. O relato é subjetivo, sem medição de nível e sem identificação isolada do estágio responsável. T09 verifica a resposta do TYPE 5 à redução de GAIN.

Em T09, GAIN 0 no TYPE 5 foi percebido como silêncio. Isso não demonstra um comando de mute, coeficiente interno zero ou causa no DSP; pode representar nível abaixo do audível nessa cadeia. T10 retorna ao ajuste de T08 para verificar a reversibilidade acústica antes de avançar.

O retorno do som em T10 foi confirmado pelo usuário. A sequência GAIN 7 → 0 → 7 produziu som → silêncio percebido → som, mantendo os demais parâmetros. T11 testa o valor adjacente 1 para distinguir esse comportamento de uma redução de ganho ainda audível.

Em T11, GAIN 1 foi descrito como quase inaudível. Isso confirma um sinal percebido muito baixo nesse ajuste, sem estabelecer o nível em dB ou provar a existência de mute em GAIN 0. T12 testa GAIN 4 como ponto intermediário, mantendo o mesmo modelo e demais controles.

64 corresponde aproximadamente à metade da escala 0–127; não é uma medição de redução de 50% da intensidade ou do volume percebido. O ACK e a leitura confirmam a aceitação do parâmetro, não o efeito acústico nem qual preset está audível.

**Correção da interpretação de persistência:** ler o banco de 48 bytes no seletor `05`, endereço zero, também reflete a alteração live. Portanto, a expressão “presets armazenados” da seção 9 não deve ser interpretada como uma leitura independente da flash. Comparar esse banco antes e depois não comprova se uma alteração sobrevive ao desligamento; essa verificação ainda não havia sido realizada naquele ponto. T20 confirmou posteriormente o descarte da alteração live de volume após desligar.

Nesta sessão não foram enviados comandos de salvar, apagar ou escrever os setores dos amps. Foram enviadas somente consultas e escritas dos parâmetros live de volume, GAIN, TONE e TYPE. As tabelas internas dos modelos não foram alteradas.

## 13. Endereço gravável da cópia de trabalho do amp — investigação local

Etapa posterior à sessão de escuta, executada a pedido do usuário, sem acesso à internet. Fontes: pedal conectado, transporte já utilizado e `Communication.java` local. A afirmação de ausência de alterações em tabelas da seção 12 refere-se àquela etapa: nesta seção foi alterado e restaurado um float da cópia de trabalho.

**Descoberta:** seletor `04`, região a partir de `0x10`, contém dados do modelo selecionado. Foi confirmada uma escrita reversível de quatro bytes em **`04:0x2C`**, por opcode `22`. Isso não é escrita no setor flash `00:0x62000`.

### Correspondência entre flash e cópia de trabalho

Foram lidos `00:0x62000`, 1584 bytes, e `04:0`, 1892 bytes. Com TYPE 2, os 1582 bytes de flash a partir do offset `0x02` coincidiram com os bytes do seletor `04` a partir de `0x12`. O deslocamento entre os campos correspondentes é `+0x10`. Os dois primeiros bytes do cabeçalho diferem: flash `01 07`, cópia de trabalho `03 04`; seu significado não foi determinado.

| Campo associado por igualdade de bytes | Offset no setor flash do amp | Endereço no seletor `04` |
|---|---|---|
| Oito floats candidatos a ganho | `0x00C–0x02B` | `0x01C–0x03B` |
| Oito floats candidatos a compensação | `0x02C–0x04B` | `0x03C–0x05B` |
| Primeiro conjunto de 50 floats | `0x050–0x117` | `0x060–0x127` |
| Escalar ainda sem função confirmada | `0x118–0x11B` | `0x128–0x12B` |
| Curva de 256 floats | `0x11C–0x51B` | `0x12C–0x52B` |
| Segundo conjunto de 50 floats | `0x51C–0x5E3` | `0x52C–0x5F3` |

Esses endereços identificam conteúdo lido; apenas `0x2C` recebeu o teste de escrita abaixo. As funções de ganho, compensação e filtros continuam hipóteses estruturais, não uma identificação do código DSP.

### A região acompanha TYPE

1. Foi lida a região `04:0x10`, tamanho `0x61C`, com TYPE 2.
2. Uma escrita live conhecida selecionou TYPE 0. A mesma região mudou em 1164 bytes e, excluindo os dois bytes iniciais, coincidiu integralmente com o trecho correspondente de `00:0x60000`.
3. TYPE 2 foi restaurado. A região voltou exatamente ao conteúdo anterior e o banco de presets também permaneceu igual ao inicial.

Portanto, não é somente uma cópia estática de um arquivo: a região acompanha a seleção do modelo. Isso sustenta sua interpretação como estrutura de trabalho, sem ainda provar quando o DSP consome cada campo.

### Escrita mínima confirmada

Condição: TYPE 2, GAIN 4. Em `04:0x2C` havia float32 little-endian `5.0` (`00 00 A0 40`), quinto elemento da tabela candidata a ganho.

- Enviado opcode `22`, seletor `04`, endereço `0x2C`, tamanho 4, dados `00 00 20 40` (`2.5`). ACK `00`.
- Uma leitura intercalada da flash evitou confiar apenas no conteúdo da última mensagem. A leitura posterior de `04:0x2C` retornou exatamente `00 00 20 40`.
- A comparação de toda a região de `0x61C` bytes mostrou mudança somente no byte `04:0x2E`, como esperado para esses dois floats.
- Restaurados `00 00 A0 40` pelo mesmo comando, com ACK `00`. A região comparada voltou integralmente ao estado inicial.
- Os 48 bytes dos presets permaneceram iguais. O setor completo de 4096 bytes em `00:0x62000` permaneceu idêntico, SHA-256 `546417480944ea75cea9d861acb02358c6f459d22e82144632b7f496b62f1507`.

Pacote SysEx efetivamente enviado para `5.0 → 2.5`:

```text
F0 00 32 09 61 00 00 00 02 2C 00 00 00 40 00 00 00 00 00 00 01 34 0D F7
```

Não foram enviados apagamento, salvamento, escrita na flash ou comando `24` de aplicação/recarga. Após o teste, o usuário relatou: “eu senti uma mudanca de ganho enquanto fazia o teste.” É uma evidência auditiva de efeito durante a investigação, compatível com aplicação live da edição em `04:0x2C`. O relato não especifica o instante nem a direção da mudança; como também houve uma comparação anterior TYPE 2 → 0 → 2, ainda não isola a edição do float como causa. A repetição somente do campo, com comparação de escuta, permanece necessária para confirmar essa atribuição.

O teste comprova escrita e restauração nessa estrutura, mas não comprova importação de um amp inteiro nem descarte dessa escrita específica após desligar. O valor foi restaurado antes do fim. Nenhum novo comando foi enviado ao registrar esse relato.

### Seletores que produziram respostas enganosas

O auxiliar Android `makeReadPacketToRAM` usa seletor `06`, mas isso não demonstrou acesso RAM válido neste pedal. Depois de ler 32 bytes conhecidos da flash, consultas de 32 bytes ao endereço zero dos seletores `03`, `06` e `07` devolveram os mesmos 31 bytes finais da leitura anterior, trocando o primeiro por `DC`, `D9` e `D8`, respectivamente. Esses primeiros bytes coincidem com o checksum do payload de cada requisição. O envelope e seu checksum foram válidos, mas o conteúdo indica reaproveitamento de buffer, não uma leitura confiável desses seletores. Não foram enviados comandos de escrita a eles.

**Estado final:** TYPE 2, GAIN 4, TONE 7, volume 127; float restaurado a 5.0; banco original e setor flash do TYPE 2 conferidos. O candidato concreto para experimentos de tabela é o seletor `04`, com escrita comprovada em `0x2C`, e não o seletor genérico `06` do Android.

## 14. T21 — dados do TYPE 5 na estrutura live do primeiro modelo

Pedido: carregar um amp no slot 1 e deixar aplicado para escuta com guitarra. Foi mantida a restrição anterior de teste temporário: “slot 1” foi interpretado como primeiro modelo (TYPE 0), no preset A. Não houve substituição permanente na flash nem importação de formato externo.

Procedimento executado:

1. Identificação do pedal e conferência dos 48 bytes iniciais dos presets.
2. Leitura dos setores completos TYPE 0 (`00:0x60000`) e TYPE 5 (`00:0x65000`), com hashes iguais aos já documentados.
3. Volume A reduzido a 64 e seleção live de TYPE 0. A região de trabalho `04:0x10`, tamanho `0x62C`, correspondeu ao setor TYPE 0, exceto os dois primeiros bytes do cabeçalho.
4. Volume A temporariamente zerado durante a transferência para evitar escuta de um conjunto parcialmente carregado.
5. Transferidos **1568 bytes** do TYPE 5, offsets `0x00C–0x62B`, para **`04:0x01C–0x63B`**, opcode `22`, blocos de até 96 bytes. Todos receberam ACK `00`. Os primeiros 12 bytes da estrutura em `04:0x10` foram preservados.
6. Leitura completa confirmou o corpo igual ao TYPE 5 e cabeçalho preservado; 398 bytes diferem da estrutura anterior do TYPE 0. SHA-256 do corpo transferido: `49c1f7d6b07894bd6deae045a4200088f2bdb57531f184c67f65b53bd1961c0c`.
7. Volume restabelecido a 64. Banco conferido: apenas TYPE A (2 → 0) e volume A (127 → 64) diferem do início. GAIN 4, TONE 7, gabinete e demais campos preservados.
8. Releitura dos 4096 bytes de `00:0x60000` confirmou que a flash do primeiro modelo permaneceu intacta.

**Estado deixado para escuta:** TYPE 0 selecionado no preset A, corpo de dados do TYPE 5 na estrutura de trabalho, GAIN 4, TONE 7, volume 64. Sem comando `24`, apagamento ou salvamento. Leitura confirma os dados carregados; a aplicação acústica ainda aguarda o relato do usuário.

Essa é uma estrutura de trabalho compartilhada que acompanha a seleção TYPE, não um banco de nove slots RAM independentes. Trocar de modelo pode recarregar os dados originais e perder o teste. Como o cabeçalho e o seletor TYPE 0 foram preservados, ainda não se deve afirmar que o som será idêntico ao TYPE 5 selecionado normalmente.

### T22/T23 — retorno ao conteúdo original e repetição

O usuário relatou que o primeiro amp continuava limpo. T22 fez somente leituras: banco com TYPE 0, GAIN 4, TONE 7 e volume 64, mas o corpo `04:0x1C–0x63B` correspondia integralmente ao TYPE 0 original, com 398 bytes diferentes do TYPE 5. Portanto, o estado de memória validado durante T21 não permaneceu até T22; a escuta limpa não demonstra que o DSP ignorou uma cópia TYPE 5 ainda presente. Não se determinou quando ou por que o conteúdo original retornou.

T23 repetiu a transferência dos 1568 bytes do TYPE 5 e confirmou a igualdade imediatamente e em nova leitura após espera de um segundo. Em seguida enviou o comando live de volume com o mesmo valor 64: a região continuou igual ao TYPE 5. Isso não confirmou a hipótese de recarga por qualquer escrita de volume; também não reproduziu a transição 0 → 64 usada em T21.

Por fim, o corpo foi transferido novamente e lido integralmente, com igualdade ao TYPE 5. Nenhum controle foi escrito depois dessa última transferência. Os 48 bytes dos presets permaneceram iguais. **Estado deixado em T23:** TYPE 0, GAIN 4, TONE 7, volume 64, corpo TYPE 5 confirmado na estrutura de trabalho; aguardando nova escuta. Somente escritas RAM e o comando live de volume já conhecido; nenhuma escrita flash ou comando de recarga `24`.

### T24 — efeito acústico confirmado pelo usuário

Após T23, o usuário relatou: “distorceu”. Uma conferência exclusivamente por leitura confirmou TYPE 0, GAIN 4, TONE 7 e volume 64, com os 1568 bytes de `04:0x1C–0x63B` ainda exatamente iguais ao corpo do TYPE 5 (SHA-256 `49c1f7d6b07894bd6deae045a4200088f2bdb57531f184c67f65b53bd1961c0c`), e 398 bytes diferentes do TYPE 0 original.

Assim, com o primeiro modelo ainda selecionado e os mesmos controles de T22, a substituição do corpo de dados na estrutura de trabalho produziu distorção percebida, enquanto o conteúdo original havia soado limpo. Há confirmação funcional por escuta de alteração live do amp via seletor `04`, sem necessidade de enviar `24` nesta sequência. Não houve medição acústica, comparação cega ou prova de equivalência exata ao TYPE 5 selecionado normalmente. Isso não demonstra gravação permanente, suporte a um formato externo nem uma função isolada para cada tabela.

**Estado atual após T24:** configuração experimental mantida para escuta, sem novas escritas nesta etapa. A causa da recarga observada em T22 continua desconhecida.

## 15. P01 — gravação autorizada do primeiro amp na flash

O usuário autorizou explicitamente substituir permanentemente o primeiro amp por um distorcido e verificar após desligar/ligar. Foi usado o setor completo do TYPE 5 do próprio pedal, sem IR e sem formato externo. Destino: seletor `00`, `0x60000–0x60FFF` (TYPE 0, primeira posição).

### Recuperação e falha inicial

Antes de apagar, foram lidos os 65536 bytes de `0x60000–0x6FFFF` para preservar também a região vizinha, pois o alcance do apagamento nessa área ainda não havia sido confirmado. Arquivo de recuperação: [CUBE_BABY_FLASH_60000_6FFFF_ORIGINAL.bin](artefatos/CUBE_BABY_FLASH_60000_6FFFF_ORIGINAL.bin), SHA-256 `506577f416011e5119b9064a30d2a2899f5fee73a917f232c0dec47565384657`. Os primeiros 4096 bytes são o amp original do primeiro slot. O arquivo foi relido e conferido antes de qualquer apagamento.

O opcode `21`, seletor `00`, endereço `0x60000`, recebeu ACK `00`, e a leitura do setor retornou 4096 bytes `FF`. Os cabeçalhos dos setores vizinhos permaneceram iguais.

A primeira programação usou blocos de 96 bytes e falhou na comparação completa, apesar dos ACKs de sucesso. A tentativa inicial de restauração com o mesmo tamanho também falhou. As diferenças e os trechos `FF` em posições periódicas são compatíveis com programação que dá a volta dentro de páginas de 256 bytes quando uma mensagem atravessa seu limite; isso é uma inferência a partir dos bytes, não identificação do chip.

O setor foi então apagado e **o amp original foi restaurado com blocos alinhados de 64 bytes**. Os 4096 bytes voltaram integralmente ao original, SHA-256 `2cef59f28050aa30e5e7b475b5d8e3ac0e0139fbf5fb9f96c363a7c0cc7d308b`. Só após essa confirmação foi repetida a gravação desejada. Não reutilizar para flash a divisão arbitrária de 96 bytes usada com sucesso na RAM.

### Gravação concluída e conferida antes de reiniciar

- Volume live zerado durante a operação, sem salvar presets.
- Apagamento: opcode `21`, seletor `00`, endereço `0x60000`; leitura confirmou o setor apagado.
- Programação: opcode `22`, mesmo seletor, 64 blocos de 64 bytes nos endereços `0x60000 + i × 64`, cobrindo exatamente 4096 bytes; ACK `00` em todas as operações.
- Releitura integral: conteúdo igual ao setor original TYPE 5, SHA-256 **`c6b6c68433f84e9dca369ca3e2ee77fdaf8491645df05a90ff41f03033ed138e`**.
- Os outros 61440 bytes preservados (`0x61000–0x6FFFF`) foram relidos integralmente e permaneceram idênticos. Não foram enviados comandos de escrita fora do setor de destino, exceto parâmetros live do preset A.
- Ao final foram enviados TYPE A 0 e volume A 64. GAIN A 4 e TONE A 7; demais campos do banco preservados. Nenhum salvamento do banco de presets.

**Limite da confirmação atual:** a escrita no endereço flash e sua leitura estão confirmadas. A região de trabalho do amp NÃO correspondeu ao corpo recém-gravado após os comandos live finais. Não se determinou se isso decorre de cache, modo LIVE, seleção física ou outra condição de recarga. Portanto, não afirmar que o amp novo já está ativo no áudio. O desligamento/religamento físico e a escuta na primeira posição ainda estão pendentes, assim como a releitura pós-reinicialização para confirmar persistência.

O primeiro slot permanece com os dados TYPE 5 gravados; o original está preservado no arquivo de recuperação. Os hashes antigos da seção 11 descrevem o estado anterior a esta modificação deliberada.

## 16. P02 — persistência e funcionamento após desligar/ligar

O usuário informou ter desligado e ligado o pedal e relatou que o primeiro amp foi reescrito e está com som idêntico ao amp usado como origem. A origem técnica do procedimento foi TYPE USB 5, setor `00:0x65000`; o destino foi TYPE USB 0, primeira posição, setor `00:0x60000`. Esses índices evitam ambiguidade entre numeração física e USB.

Após esse relato, foram enviados somente identificação e leituras dos dois setores completos, sem qualquer escrita ou reaplicação em RAM:

| Verificação pós-reinicialização | Resultado |
|---|---|
| Primeiro setor igual aos 4096 bytes gravados em P01 | Sim |
| Primeiro setor igual ao setor de origem TYPE 5 | Sim, byte a byte |
| Primeiro setor diferente do amp clean original preservado | Sim |
| SHA-256 do primeiro setor e do setor de origem | `c6b6c68433f84e9dca369ca3e2ee77fdaf8491645df05a90ff41f03033ed138e` |
| Relato de escuta após desligar/ligar | Som percebido como idêntico ao amp de origem |

**Conclusão confirmada neste dispositivo:** é possível substituir permanentemente os dados do primeiro amp por dados de outro amp interno, via comandos USB; o conteúdo sobrevive ao desligamento e é utilizado pelo pedal. A identidade dos bytes é objetiva; a equivalência sonora é um relato de escuta, sem medição acústica. Não se trata de upload de IR, mera seleção de TYPE 5 nem reaplicação temporária pelo computador após reiniciar.

O pedal permanece com o primeiro amp modificado. O original continua disponível no arquivo de recuperação da seção 15. A pendência de persistência e carregamento após reiniciar registrada em P01 está resolvida.

## 17. G01/G02 — redução da tabela candidata a ganho em LIVE

O usuário informou modo LIVE, primeiro amp selecionado, knobs TONE e GAIN em aproximadamente 50%. G01 leu a estrutura de trabalho e encontrou a tabela do TYPE 5 agora instalado no primeiro slot: `[0.5, 1, 5, 10, 20, 35, 50, 60]`. Os oito floats da tabela candidata a compensação eram aproximadamente `0.12`. O banco de presets indicava A TYPE 0, GAIN 4, TONE 7 e volume 127; esses campos não foram tratados como medição da posição física atual dos knobs em LIVE. O cabeçalho de trabalho continha `03 03` em `04:0x10`, cuja interpretação não foi assumida.

Para testar a tabela sem pressupor o índice ativo, G02 escreveu somente seus 32 bytes, opcode `22`, seletor `04`, endereço `0x1C`, reduzindo todos os oito valores pela metade:

| Índice | Original | G02 |
|---|---:|---:|
| 0 | 0.5 | 0.25 |
| 1 | 1 | 0.5 |
| 2 | 5 | 2.5 |
| 3 | 10 | 5 |
| 4 | 20 | 10 |
| 5 | 35 | 17.5 |
| 6 | 50 | 25 |
| 7 | 60 | 30 |

ACK `00`; releitura de todo o corpo de 1568 bytes confirmou a nova tabela e todos os demais bytes inalterados, incluindo a candidata a compensação, filtros e curva. Nenhum parâmetro live do banco foi escrito, nenhum comando de recarga enviado e nenhuma escrita flash realizada. A redução dos números em 50% não implica redução de 50% da distorção ou do volume percebido.

**Estado deixado em G02:** tabela reduzida aplicada na estrutura de trabalho para escuta; aguardando relato. A flash mantém o TYPE 5 instalado no primeiro slot em P01/P02. Os valores originais acima permitem restaurar a tabela sem trocar os controles.

### G03 — redução de 90% em relação ao original

O usuário pediu contrastes maiores: redução de 90% e aumento ao máximo. Sem relato acústico específico sobre G02, foi aplicada primeiro a redução: `[0.05, 0.1, 0.5, 1, 2, 3.5, 5, 6]`, em float32, mantendo 10% de cada valor original (não 10% da tabela já reduzida em G02).

G03 escreveu apenas os 32 bytes de `04:0x1C`, recebeu ACK `00` e confirmou pela releitura dos 1568 bytes que todo o restante do corpo permaneceu igual. Não houve escrita flash, ajuste dos knobs via protocolo ou comando de recarga. **Estado atual:** tabela G03 mantida para escuta; resposta do usuário pendente.

Para o contraste alto, foi proposto usar 60, maior valor original desta tabela, em todas as posições para não depender do índice físico ativo. Esse contraste ainda não foi enviado. Não se conhece o limite numérico seguro acima dos valores originais; 60 não é uma afirmação do máximo suportado pelo DSP.

### G04 — contraste alto com todos os valores em 60

Resultado de escuta G03: o usuário informou “ficou mais baixo e menos saturado”. Isso sustenta a participação da tabela em ganho/saturação nessa configuração, sem estabelecer a fórmula do DSP ou a unidade dos coeficientes.

Após o relato, G04 substituiu os oito floats de `04:0x1C–0x3B` por `[60, 60, 60, 60, 60, 60, 60, 60]`. Foi usado o maior valor da tabela original, não um máximo arbitrário do formato float. O usuário foi avisado sobre o possível aumento de volume.

ACK `00`; releitura integral dos 1568 bytes confirmou os valores enviados e todo o restante do corpo inalterado. Sem escrita flash, comando de recarga ou alteração de outros parâmetros. **Estado atual:** G04 aplicado em RAM para escuta; resultado acústico pendente. A tabela original para restauração permanece `[0.5, 1, 5, 10, 20, 35, 50, 60]`.

### G05 — confirmação acústica do contraste alto

O usuário relatou: “saturacao sustain e volume subiram”. Uma consulta somente de leitura confirmou que os oito floats continuavam em 60. Nenhuma nova escrita foi enviada nesta etapa.

Comparação G03/G04: reduzindo os oito coeficientes a 10% dos originais, o usuário percebeu menor volume e saturação; colocando todos em 60, percebeu aumento de saturação, sustain e volume. Isso confirma funcionalmente a influência desse bloco no ganho/saturação nessa configuração do pedal. Não determina a curva de interpolação, o índice ativo em LIVE, a unidade dos valores nem a posição exata do estágio no DSP. Sustain é percepção de escuta, não medição temporal.

**Estado mantido após G05:** tabela de oito valores em 60 apenas na estrutura de trabalho. A flash conserva a tabela original do TYPE 5 instalado no primeiro slot, `[0.5, 1, 5, 10, 20, 35, 50, 60]`. Relatos registrados; não houve nova gravação permanente.

## 18. C01/C02 — tabela candidata a compensação

O usuário solicitou continuar pela compensação, depois coeficientes e curva de 256 pontos. As etapas seguintes permanecem testes em RAM, com resultado acústico de cada etapa registrado antes da próxima.

C01 restaurou os oito valores de ganho originais `[0.5, 1, 5, 10, 20, 35, 50, 60]`, recebeu ACK `00` e confirmou por leitura que os demais bytes do corpo permaneceram iguais. A compensação continha oito floats aproximadamente iguais a 0.12. Foi anunciada e realizada uma pausa de dez segundos para escuta da referência antes da alteração seguinte; não houve confirmação separada de que o usuário tocou durante essa pausa.

C02 escreveu oito floats 0.012 no lugar de 0.12 (redução de 90%) em **`04:0x3C–0x5B`**, opcode `22`, 32 bytes. ACK `00`; releitura do corpo de 1568 bytes confirmou que apenas essa tabela mudou. Ganho restaurado, conjuntos de coeficientes, curva e demais dados preservados. Nenhuma escrita flash ou comando de recarga enviado.

**Estado atual:** compensação candidata em 0.012 nas oito posições, mantida para escuta. Resultado pendente: comparar volume e caráter da distorção com a referência de ganho original. Ainda não foi determinado se esse bloco atua antes ou depois da saturação; o nome “compensação” continua uma hipótese de função. Coeficientes e curva ainda não foram editados nesta sequência.

### C03 — resultado e restauração

O usuário informou: “somente o volume caiu muito e a distorcao ficou”. O resultado é compatível com compensação de nível, mas não determina a posição exata do bloco no DSP nem comprova invariância espectral objetiva. C03 restaurou os oito floats a 0.12; ACK `00` e releitura do corpo completo confirmaram a restauração e todos os demais bytes inalterados.

## 19. F01 — primeiro conjunto de coeficientes

Depois de restaurar a compensação, foi anunciada e realizada uma pausa de dez segundos para escuta da referência. Não houve confirmação separada de escuta durante a pausa.

Como a função e a ordem individual dos coeficientes ainda não foram identificadas, F01 não inventou valores nem escalou possíveis coeficientes de realimentação. Substituiu o conjunto completo de 50 floats (200 bytes) em **`04:0x60–0x127`** pelo conjunto de fábrica do amp clean TYPE 0, offsets `0x50–0x117` da imagem original preservada. A integridade dessa imagem foi verificada por SHA-256. O conjunto anterior foi conferido contra o TYPE 5 preservado na mesma imagem.

Transferência por opcode `22`, seletor `04`, em blocos de 96, 96 e 8 bytes, todos com ACK `00`. Releitura do corpo completo de 1568 bytes confirmou somente a substituição prevista: 136 bytes diferentes dentro do conjunto de 200 bytes, nenhum byte alterado fora dele.

- SHA-256 anterior (TYPE 5): `ef90e62497dc4e0dfb211134111a23769066135ddb014c35e3bb98f7a930f3e8`.
- SHA-256 aplicado (TYPE 0 clean original): `6fed47da2d02f1360f08a509ad24d5841981c939978a3aed10e011fa4467519e`.

Ganho e compensação estão nas tabelas originais do TYPE 5. A curva de 256 pontos, o segundo conjunto de coeficientes e os demais dados permaneceram iguais. Não houve escrita flash nem comando de recarga. Usar um conjunto existente evita inventar coeficientes, mas não comprova a função de cada estágio nem equivalência com a cadeia completa do amp clean.

**Estado atual:** primeiro conjunto do clean aplicado na estrutura de trabalho do amp distorcido; aguardando relato sobre brilho, graves/médios, saturação e volume. Os 200 bytes originais para restauração estão nos offsets `0x5050–0x5117` do arquivo de recuperação. A curva ainda não foi modificada.

### F02/F03 — relato do primeiro conjunto e teste do segundo

Sobre F01, o usuário relatou “graves aumentara maisardido e mais volume e saturacao”. Registro: mais graves, volume e saturação; expressão “mais ardido” preservada, sem atribuir frequência específica ou posição do filtro no DSP.

F02 restaurou os 200 bytes do primeiro conjunto ao TYPE 5 original. ACKs `00` e releitura integral confirmaram a restauração, sem alteração fora do conjunto. Foi anunciada e realizada uma pausa de dez segundos para escuta da referência, sem confirmação separada de escuta durante a pausa.

F03 substituiu somente o **segundo conjunto de 50 floats**, `04:0x52C–0x5F3`, pelo conjunto do TYPE 0 clean original, offsets `0x51C–0x5E3` da imagem de recuperação. O conjunto anterior foi conferido contra os offsets `0x551C–0x55E3` (TYPE 5) dessa imagem. Transferência em blocos de 96, 96 e 8 bytes, ACKs `00`; leitura integral dos 1568 bytes confirmou 142 bytes diferentes dentro do conjunto e nenhum fora dele.

- SHA-256 anterior (TYPE 5): `d81b840c07192b3e425859cde87f60ac62c8929a75b10fe532f5aedb03875285`.
- SHA-256 aplicado (clean original): `fa2fa4e3fc4eb3c82386e8900eca8e30e37b611f658c2f54b17b4ec7414ff11e`.

**Estado atual após F03:** somente o segundo conjunto está substituído pelo conjunto clean; primeiro conjunto, ganho, compensação e curva estão originais do TYPE 5. Sem escrita flash ou comando de recarga. Aguardando relato para depois restaurar esse conjunto e iniciar o teste da curva de 256 pontos.

### F04 — resultado do segundo conjunto e restauração

O usuário relatou sobre F03: “diminuiu drasticamente o volume e ficou parecido crunch”. Isso registra uma mudança de nível e de caráter percebido, sem medir espectro ou determinar se o estágio atua antes/depois da saturação. F04 restaurou o segundo conjunto do TYPE 5 original; ACKs `00` e leitura integral confirmaram a restauração e os demais bytes inalterados.

## 20. W01 — curva de 256 pontos linear

Após restaurar os dois conjuntos de coeficientes, foi anunciada e realizada uma pausa de dez segundos para escuta da referência, sem confirmação separada de escuta durante a pausa. Ganho e compensação também estavam restaurados.

W01 substituiu somente a curva em **`04:0x12C–0x52B`**, 1024 bytes, por 256 floats little-endian calculados como **`y[i] = i / 255`, para i de 0 a 255**. Primeiro ponto 0, último 1; pontos intermediários crescentes. A curva anterior foi conferida contra os offsets `0x511C–0x551B` (TYPE 5) da imagem de recuperação validada por hash.

A rampa preserva a faixa 0–1 e os extremos da tabela original, mas isso não significa bypass de todo o amp nem garante áudio totalmente limpo. A forma de indexação, o tratamento de sinal negativo, a interpolação e os demais estágios do DSP continuam desconhecidos. O objetivo é comparar o efeito acústico de mudar apenas a forma dessa tabela.

Transferência por opcode `22`, seletor `04`, blocos de até 96 bytes, todos com ACK `00`. Releitura integral dos 1568 bytes do corpo confirmou somente a troca prevista: 890 bytes diferentes dentro da curva, nenhum fora dela.

- SHA-256 da curva original: `9e43dfd28ed8031c2ce1376dc51579b82e5fc13a1063e0e3a5fe3accf36387ad`.
- SHA-256 da curva linear aplicada: `010413efe9fc4438fee48de66c4d09f377b28af6a9fe2522201e8c1dbb831fc8`.

**Estado atual:** curva linear mantida em RAM para escuta; ganho, compensação, dois conjuntos de coeficientes e demais campos preservados. Sem comando de recarga ou escrita flash. Resultado acústico pendente; a flash mantém o amp TYPE 5 instalado anteriormente no primeiro slot.

### W02 — resultado acústico da curva linear

O usuário relatou: “ficou mais limpo, mais magro com mais estalado, um crunch leve”. Uma verificação somente por leitura confirmou os 1024 bytes ainda iguais à rampa linear, SHA-256 `010413efe9fc4438fee48de66c4d09f377b28af6a9fe2522201e8c1dbb831fc8`. Nenhuma nova escrita foi enviada.

O teste confirma que mudar a forma dessa tabela altera o caráter da distorção e do timbre percebidos. A limpeza parcial é compatível com a hipótese de uma curva de transferência não linear, mas não identifica sua implementação. O crunch residual não permite localizar sozinho outro estágio de saturação: também podem participar limites de indexação/clipping dessa própria etapa ou a cadeia externa, cuja saída de escuta não foi informada.

Resumo dos relatos com alteração isolada de cada bloco:

| Alteração | Resultado relatado |
|---|---|
| Ganho a 10% dos valores originais | Menor volume e saturação |
| Oito valores de ganho em 60 | Mais saturação, sustain e volume |
| Compensação 0.12 → 0.012 | Volume muito menor, distorção preservada |
| Primeiro conjunto de coeficientes do clean | Mais graves, caráter “mais ardido”, volume e saturação |
| Segundo conjunto de coeficientes do clean | Volume drasticamente menor, caráter crunch |
| Curva linear de 256 pontos | Mais limpo, magro e estalado, com crunch leve |

**Estado mantido após W02:** somente a curva linear experimental permanece em RAM. As tabelas de ganho e compensação e os dois conjuntos de coeficientes estão restaurados aos dados do TYPE 5. Não houve gravação permanente das variações testadas nas seções 17–20.

### W03 — curva suave em busca de mais corpo

O usuário pediu modificar a curva linear adicionando médios e graves. Foi esclarecido que a tabela de transferência não oferece reforço independente por frequência como um equalizador: alterar sua forma pode mudar harmônicos e corpo percebido, mas não garante ganho seletivo em graves/médios. Os conjuntos de coeficientes foram preservados para isolar a curva.

W03 substituiu a rampa por **`y = 1.5*x - 0.5*x^3`, com `x = i/255`, i de 0 a 255**. A curva é crescente em 0–1, mantém os extremos 0 e 1, tem inclinação inicial 1.5 e inclinação zero no extremo superior. A intenção é experimentar uma transição suave para saturação e maior corpo percebido; o efeito acústico não foi pressuposto. O tratamento do sinal negativo e da indexação pelo DSP continua desconhecido.

Escrita dos 1024 bytes em `04:0x12C–0x52B`, opcode `22`, ACKs `00`. A curva linear anterior foi conferida antes de escrever. A releitura de todo o corpo confirmou a nova curva e nenhum byte alterado fora dela; 811 bytes diferem da rampa anterior. SHA-256 aplicado: `f03615b6baf3fe86410977988d78a8633d7a91f140552ed0fd1f394b803a14cc`.

**Estado atual:** curva suave W03 aplicada somente em RAM para escuta. Ganho, compensação, coeficientes e flash preservados. Ainda não há confirmação de aumento percebido de corpo, graves ou médios.

### W04/W04b — intensificação da curva suave

Relato sobre W03: “aumentou um pouco o corpo e ate um pouco de volume e saturacao”. O usuário pediu continuar aumentando.

W04 aplicou inicialmente `2.5*x - 2.5*x^3 + x^5`. Após a escrita, a revisão da fórmula identificou que ela ultrapassa 1 (aproximadamente 1.061 no pico) e deixa de ser crescente no trecho final. Isso contrariava a faixa anunciada de 0–1. A fórmula foi substituída imediatamente em W04b; não houve relato de escuta separado para W04 nem escrita flash. Hash da curva transitória: `4d400e045125606440967b9d2a8e2d99d0b533e1b99863b1e1478c5120af85c5`.

**W04b aplicado:** composição `f(f(x))`, onde `f(x)=1.5*x-0.5*x^3` e `x=i/255`. A inclinação inicial aumenta de 1.5 em W03 para 2.25, mantendo extremos 0 e 1. Os 256 valores float32 foram verificados quanto à faixa 0–1 e monotonicidade antes da escrita. Isso intensifica a curvatura, sem representar reforço seletivo por frequência.

Escrita somente de `04:0x12C–0x52B`, ACKs `00`; releitura integral do corpo confirmou a curva enviada e os demais bytes inalterados. SHA-256 final: `bd8d48aaf2ac48bd75b6c90db6bd7d5585ad4150e0d641c5d9616faa9ab2a4f5`.

**Estado atual:** W04b mantido em RAM para escuta, demais tabelas originais do TYPE 5 e flash preservadas. Resultado acústico pendente; comparar o ajuste final estabilizado com W03, não com a curva transitória W04.

### W05 — alteração somente no trecho central

Relato sobre W04b: “ficou mais crunch ainda e ganhou mais medios e corpo”. O usuário pediu explorar outros pontos da curva para entender seu funcionamento.

W05 usou W04b como referência e modificou somente o intervalo central de índices 64–191. Para `x=i/255`, foi calculado `w=0.20*sin(pi*(i-64)/127)^2` nesse intervalo e `y_novo=(1-w)*y_W04b+w*x`. Fora dele os bytes foram preservados. A janela suaviza a transição nas bordas: os pontos 64 e 191 permaneceram iguais, e os pontos efetivamente alterados foram 65–190.

Os 256 floats finais foram verificados antes da escrita: finitos, crescentes e entre 0 e 1. Foram enviados apenas os 512 bytes de `04:0x22C–0x42B`, opcode `22`, ACKs `00`; releitura do corpo completo confirmou todos os bytes previstos e nenhum fora da curva alterado. SHA-256 final: `eb43c500c15e078b4f99e22ec56181278bcf719e5a6b3c1a72619a4eac3bb721`.

| Ponto | W04b | W05 |
|---|---:|---:|
| 64 | 0.527816 | 0.527816 |
| 96 | 0.729169 | 0.693463 |
| 128 | 0.870512 | 0.796813 |
| 160 | 0.953162 | 0.921799 |
| 191 | 0.989080 | 0.989080 |

Este é um teste da região central da tabela, não de uma faixa de frequências: índice central não significa médios de equalização. Sua associação exata à amplitude do sinal depende da indexação ainda desconhecida do DSP. A escuta com palhetadas fracas e fortes pode ajudar a caracterizar o efeito, sem presumir o resultado.

**Estado atual:** W05 aplicado somente em RAM, aguardando relato. Demais blocos e flash preservados; referência para comparação: W04b.

## 21. S00/S01 — oito partes de 32 pontos, contrastes de ±100%

O usuário pediu dividir a curva em oito partes iguais e testar cada parte com +100% e −100%, documentando os efeitos. Não houve relato acústico específico de W05 antes desse novo protocolo.

**Referência S00:** W05 com cada um dos 256 floats reduzido pela metade, faixa 0–0.5. Essa normalização permite dobrar qualquer parte sem ultrapassar 1. SHA-256 da referência: `3dcc22b2fdfc82c83ffcd9c3c64bedd522429e25a6b8128e9a5f5b8ff01861ce`. A curva é reproduzível pelas fórmulas W04b/W05, arredondando a float32 ao final de cada etapa, e dividindo os valores W05 por dois. A escrita e a releitura integral confirmaram somente a mudança da curva.

Cada contraste será calculado em relação a S00: −100% zera os 32 valores; +100% dobra os 32 valores originais daquela parte. Os outros 224 pontos devem permanecer iguais à referência. Não acumular modificações entre partes. A referência deve ser reaplicada e ouvida antes de cada contraste.

| Parte | Índices (base zero) | Endereços no seletor `04` | −100% | +100% |
|---|---|---|---|---|
| 1 | 0–31 | `0x12C–0x1AB` | S01: usuário associou ao agudo extremo/chiado; direção da mudança não especificada | S02: mesmo chiado, um pouco menos “quebrado” |
| 2 | 32–63 | `0x1AC–0x22B` | S03: ainda na região de chiados agudos; sensação de descida de frequências | S04: chiado percebido mais para os médios |
| 3 | 64–95 | `0x22C–0x2AB` | S05: chiado ainda percebido como agudo | S06: chiado mais natural, caráter de médios |
| 4 | 96–127 | `0x2AC–0x32B` | S07: ainda chiado, caráter de médios | S08: outro chiado, mais natural, ainda médio |
| 5 | 128–159 | `0x32C–0x3AB` | S09: crunch “quebrado”, caráter agudo/médio | S10: chiado com caráter de médios |
| 6 | 160–191 | `0x3AC–0x42B` | S11: chiado aparentemente mais grave, “menos amostras de chiado” | S12: chiado também menos presente |
| 7 | 192–223 | `0x42C–0x4AB` | S13: chiado mais aparente nas cordas médias e graves | S14: mais suave, somente ao tocar forte |
| 8 | 224–255 | `0x4AC–0x52B` | S15: chiado quase inexistente, discreto apenas nas cordas graves | S16: quase sem chiado |

São intervalos de índices de uma tabela de transferência, não bandas de frequência identificadas. Os cortes locais pedidos podem criar degraus ou trechos não monotônicos e gerar distorção; a faixa numérica limitada não elimina esse efeito. Não interpretar um relato de mais graves/médios como prova de que aquela parte controla uma banda de EQ.

Após aplicar S00, foi anunciada e realizada pausa de dez segundos para ouvir a referência. S01 então escreveu somente 128 bytes zero em `04:0x12C–0x1AB`. ACKs `00`; releitura integral do corpo confirmou apenas a alteração prevista, com as outras sete partes iguais a S00 e nenhum outro bloco alterado. SHA-256 da curva S01: `f6aa2f9c154a2c4762aa83d039d15f1e96e084577c0cfed3f42df7df3ba313db`.

**Estado atual:** parte 1 zerada, demais partes na referência S00, somente em RAM. Aguardando descrição de volume, ataque, sustain, distorção/ruído e resposta às palhetadas fracas e fortes. Nenhuma escrita flash.

### S02 — parte 1 com +100%

Relato sobre S01: “essa parte é o agudo extremo aquele bem chiado mesmo”. Foi registrado como associação perceptiva ao agudo extremo/chiado, sem presumir se zerar os pontos aumentou ou reduziu esse caráter. Isso não identifica os índices 0–31 como uma banda de frequência: uma alteração de curva pode gerar harmônicos, e os degraus introduzidos pelo experimento também podem participar do chiado.

S00 foi restaurada e conferida por releitura integral, seguida de pausa anunciada de dez segundos. S02 dobrou apenas os 32 floats dos índices 0–31, em relação a S00. As outras sete partes ficaram iguais à referência. Foram escritos somente 128 bytes em `04:0x12C–0x1AB`; ACKs `00`; releitura do corpo completo confirmou apenas a alteração prevista. Valores dentro de 0–1, sem impor monotonicidade às fronteiras entre partes neste protocolo.

SHA-256 da curva S02: `0c1d5ed8068b20773fa08dffea0d87b5c6058c60ebb5ea7cd08cce977d8d4203`. **Estado atual:** parte 1 dobrada em RAM, demais partes na referência; nenhuma escrita flash. Aguardando comparação específica do chiado com S00 e esclarecimento da direção percebida em S01.

### S03 — parte 2 com −100%

Relato sobre S02: “o mesmo chiado porem um pouco menos 'quebrado'”. A direção do chiado em S01 em relação à referência continua não especificada; não foi inferida uma função de EQ a partir desses relatos.

A parte 1 foi restaurada a S00 e a releitura integral confirmou a referência completa. Após pausa anunciada de dez segundos, S03 zerou somente os pontos 32–63, 128 bytes em `04:0x1AC–0x22B`. ACKs `00`; conferência do corpo completo confirmou a alteração isolada, sem mudanças nas outras sete partes ou outros blocos. SHA-256: `bde1e14dc3b3a39ce1adb0abe977d79e1e2462f9c1cbf9fb63107592daacfe03`.

**Estado atual:** somente parte 2 zerada, demais partes em S00, em RAM; escuta pendente. Nenhuma escrita flash. Esse recorte introduz degraus nas fronteiras, que devem ser considerados ao interpretar mudanças de ruído e distorção.

### S04 — parte 2 com +100%

Relato sobre S03: “ta na regiao dos chiados agudos ainda parece que estamos descendo as frewuqencias”. Registrada a percepção de chiado agudo e aparente descida de frequências, sem converter essa impressão em mapeamento comprovado entre índices e bandas de EQ. A direção de aumento/redução do chiado em relação a S00 não foi especificada.

S00 foi restaurada e conferida integralmente. Após pausa anunciada de dez segundos, S04 dobrou somente os pontos 32–63 em relação a S00: escrita dos 128 bytes de `04:0x1AC–0x22B`, ACKs `00`. A releitura do corpo confirmou apenas a alteração prevista; as outras sete partes e os demais blocos permaneceram iguais. SHA-256 da curva: `b4d7bca5db42ea8ea1d4ff0966f53c105352ef479cd0fbfade7c358f1ea76f6f`.

**Estado atual:** parte 2 dobrada em RAM; demais partes em S00; escuta pendente. Nenhuma escrita flash.

### S05 — parte 3 com −100%

Relato sobre S04: “sim o chiado esta ficando mais pro medio”. Registrada a percepção de deslocamento do caráter do chiado em direção aos médios, sem determinar frequência, intensidade ou função de EQ dos índices.

A parte 2 foi restaurada a S00; leitura do corpo confirmou a referência completa e os demais dados inalterados. Após pausa anunciada de dez segundos, S05 zerou somente os pontos 64–95: 128 bytes em `04:0x22C–0x2AB`, ACKs `00`. A leitura integral confirmou apenas a alteração prevista. SHA-256 da curva: `3526a07d1189e44aedabebf8c9356675aad336a3b13a9cb19bc340cd52e3dc62`.

**Estado atual:** parte 3 zerada em RAM; demais sete partes na referência S00; escuta pendente. Nenhuma escrita flash. Mantém-se a limitação de que degraus na tabela podem gerar harmônicos e não equivalem a uma banda de frequência isolada.

### S06 — parte 3 com +100%

Relato sobre S05: “chiado ainda esse me pareceu ainda agudo”. Registrado chiado percebido como agudo; não foram informadas mudanças de volume ou corpo. O relato não sustenta uma progressão obrigatória de agudos para graves conforme o índice aumenta.

A parte 3 foi restaurada a S00, com leitura integral confirmando a referência. Após pausa anunciada de dez segundos, S06 dobrou apenas os pontos 64–95 em relação a S00. Escrita de 128 bytes em `04:0x22C–0x2AB`, ACKs `00`; releitura do corpo confirmou apenas a alteração prevista, mantendo as demais sete partes e todos os outros blocos iguais. SHA-256 da curva: `71183885f320817bc37db9c0717a25dcb19a82a46b873bb974cb11bf0076814b`.

**Estado atual:** parte 3 dobrada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash.

### S07 — parte 4 com −100%

Relato sobre S06: “chiado mais natural medio”. Registrada a percepção de chiado mais natural e caráter de médios, sem atribuir uma faixa de frequência medida aos índices 64–95.

A parte 3 foi restaurada a S00; leitura integral confirmou a referência completa e os demais bytes preservados. Após pausa anunciada de dez segundos, S07 zerou somente os pontos 96–127, 128 bytes em `04:0x2AC–0x32B`, com ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, mantendo as outras sete partes e demais blocos iguais. SHA-256 da curva: `9f3995a262f4d4049e9ad3f7079ffb7ae57eddf41272a74bdf985be6a47d3dfc`.

**Estado atual:** parte 4 zerada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash.

### S08 — parte 4 com +100%

Relato sobre S07: “ainda chiado ok medio”. Registrado chiado persistente com caráter de médios; volume, corpo e ataque não foram especificados.

A parte 4 foi restaurada a S00; leitura integral confirmou a referência completa e os demais bytes preservados. Após pausa anunciada de dez segundos, S08 dobrou somente os pontos 96–127 em relação a S00: 128 bytes em `04:0x2AC–0x32B`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, mantendo as outras sete partes e demais blocos iguais. SHA-256 da curva: `d58b1f0b5e4e1605ab8b84c9e68a7761121d48226f204365f6f891e09f57fa79`.

**Estado atual:** parte 4 dobrada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash.

### S09 — parte 5 com −100%

Relato sobre S08: “outro chiado mais natual ainda medio”. Registrado chiado diferente, mais natural e ainda com caráter de médios; sem quantificação de volume ou atribuição de frequência aos índices.

A parte 4 foi restaurada a S00 e a leitura integral confirmou a referência completa. Após pausa anunciada de dez segundos, S09 zerou somente os pontos 128–159: 128 bytes em `04:0x32C–0x3AB`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista; as outras sete partes e demais blocos permaneceram iguais. SHA-256 da curva: `dd71b1072b26f91a8b0bb5b9970aed4b6679e313a42f9bfd066ee77d4568c33c`.

**Estado atual:** parte 5 zerada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash.

### S10 — parte 5 com +100%

Relato sobre S09: “crunch quebrado agudo medio”. Registrado crunch com textura “quebrada” e caráter entre agudos e médios, sem medida de frequência ou relato específico de volume/sustain.

A parte 5 foi restaurada a S00; leitura integral confirmou a referência completa. Após pausa anunciada de dez segundos, S10 dobrou somente os pontos 128–159 em relação a S00: 128 bytes em `04:0x32C–0x3AB`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, mantendo as outras sete partes e demais blocos iguais. SHA-256 da curva: `dbaf90669788268271d30ab1a29ccfcb75e6e399c75dd05006a68e8d73747b98`.

**Estado atual:** parte 5 dobrada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash.

### S11 — parte 6 com −100%; pausa de referência reduzida

Relato sobre S10: “medio chiado”. Registrado chiado com caráter de médios. O usuário pediu escuta de três segundos daqui em diante; a pausa anunciada de referência foi reduzida de dez para três segundos. As leituras e transferências acrescentam tempo de execução além dessa pausa; não se trata de uma comutação precisamente a cada três segundos.

A parte 5 foi restaurada a S00 e a leitura integral confirmou a referência completa. Após pausa anunciada de três segundos, S11 zerou somente os pontos 160–191: 128 bytes em `04:0x3AC–0x42B`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, com as outras sete partes e demais blocos iguais. SHA-256 da curva: `0c014cc91aba636b6272fa8ca1b2d78f8cf4458ccf6a5ed77d8434a9b23d5681`.

**Estado atual:** parte 6 zerada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash. Próximas pausas de referência: três segundos.

### S12 — parte 6 com +100%

Relato sobre S11: “parece estar ficando mais grave o chiado menos amostras de chiado”. Registrada a impressão de chiado mais grave e menos presente; “amostras” é expressão do usuário, não medição de amostragem digital, quantidade de harmônicos ou eventos de ruído.

A parte 6 foi restaurada a S00; leitura integral confirmou a referência completa. Após pausa anunciada de três segundos, S12 dobrou somente os pontos 160–191 em relação a S00: 128 bytes em `04:0x3AC–0x42B`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, mantendo as outras sete partes e demais blocos iguais. SHA-256 da curva: `7f1666fc03e159fe12a6ee2970bda2e8e2922552b7553d7189a5a144022be01d`.

**Estado atual:** parte 6 dobrada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash.

### S13 — parte 7 com −100%

Relato sobre S12: “chiado menos presente tambem”. Registrada redução percebida da presença do chiado também no contraste positivo da parte 6, sem relato novo sobre sua tonalidade ou volume geral. O fato de os dois contrastes produzirem essa impressão não estabelece uma relação linear de ganho por frequência.

A parte 6 foi restaurada a S00; leitura integral confirmou a referência completa. Após pausa anunciada de três segundos, S13 zerou somente os pontos 192–223: 128 bytes em `04:0x42C–0x4AB`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, mantendo as outras sete partes e demais blocos iguais. SHA-256 da curva: `04ba9122baa37635ab4a70366d4c8716f1986b62dbb5990cb841d1a6068feacd`.

**Estado atual:** parte 7 zerada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash.

### S14 — parte 7 com +100%

Relato sobre S13: “chiado somente mais aparente nas cordas medias e graves”. Registrada maior percepção do chiado ao tocar essas cordas; isso não identifica diretamente uma banda de EQ, pois cordas diferentes também apresentam amplitudes, envelopes e conteúdo harmônico distintos.

A parte 7 foi restaurada a S00; leitura integral confirmou a referência completa. Após pausa anunciada de três segundos, S14 dobrou somente os pontos 192–223 em relação a S00: 128 bytes em `04:0x42C–0x4AB`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, mantendo as outras sete partes e demais blocos iguais. SHA-256 da curva: `6835bc18cab29aeba414f583845006071e2321a67dfe88c5be83d6665ccd8589`.

**Estado atual:** parte 7 dobrada em RAM, demais partes em S00; aguardando escuta. Nenhuma escrita flash.

### S15 — parte 8 com −100%

Relato sobre S14: “mais suave somente quando toca forte”. Registrado chiado mais suave, percebido somente ao tocar forte. A dependência da força da palhetada é compatível com participação da amplitude na consulta da curva, mas não identifica a indexação exata do DSP.

A parte 7 foi restaurada a S00; leitura integral confirmou a referência completa. Após pausa anunciada de três segundos, S15 zerou somente os pontos 224–255: 128 bytes em `04:0x4AC–0x52B`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, mantendo as outras sete partes e demais blocos iguais. SHA-256 da curva: `ecfd0f958079041a729d48b98cade2fb31297f3b927720b7913ceb01cb1a906f`.

**Estado atual:** parte 8 zerada em RAM, incluindo o último ponto da curva; demais partes em S00; aguardando escuta. Nenhuma escrita flash. Este contraste deliberadamente altera o extremo superior da tabela, conforme o protocolo de zerar todos os valores da parte.

### S16 — parte 8 com +100%

Relato sobre S15: “chiado quase inexistente somente nas graves e pouco”. Registrado chiado muito discreto, percebido apenas nas cordas graves; não foi informado corte de som ou mudança específica de volume.

A parte 8 foi restaurada a S00; leitura integral confirmou a referência completa. Após pausa anunciada de três segundos, S16 dobrou somente os pontos 224–255 em relação a S00: 128 bytes em `04:0x4AC–0x52B`, ACKs `00`. A releitura do corpo completo confirmou apenas a alteração prevista, mantendo as outras sete partes e demais blocos iguais. SHA-256 da curva: `82061dcbd978af5acb54034d0f6e51176811e325e2c73403125fb152beb830a3`.

**Estado atual:** parte 8 dobrada em RAM, demais partes em S00. Os 16 contrastes foram enviados; falta o relato de escuta de S16 para concluir o registro. Nenhuma escrita flash nesta série.

### Conclusão da série S01–S16

Relato sobre S16: “quse sem chiado”. Os 16 contrastes agora têm relatos registrados na tabela acima. Após esse relato, a parte 8 foi restaurada a S00; ACKs `00` e releitura de todo o corpo confirmaram a referência completa, sem outras alterações.

Observações sustentadas pelos relatos:

- As primeiras partes produziram percepções de chiado agudo/médio e mudanças de textura, incluindo caráter “quebrado” ou mais natural.
- Nas partes finais, o chiado foi menos presente e, em alguns relatos, percebido nas cordas médias/graves ou apenas ao tocar forte.
- Na parte 6, ambos os contrastes foram descritos como menos chiados; na parte 8, ambos foram descritos como quase sem chiado. Portanto, os resultados não demonstram um controle linear de ganho por banda de frequência.
- A dependência da intensidade da palhetada é compatível com uma tabela consultada por amplitude do sinal. Não houve rastreamento do DSP ou medição suficiente para confirmar o mapeamento exato dos índices.

**Não foi demonstrado um equalizador de oito bandas.** Os contrastes criaram descontinuidades e possíveis trechos não monotônicos, capazes de alterar o conteúdo harmônico. Cordas graves também diferem em amplitude e envelope. Os relatos não isolam esses fatores e não permitem atribuir uma frequência em Hz a cada parte. Não houve captura de áudio, sinal de teste calibrado, comparação cega ou nivelamento de volume entre condições.

**Estado final da série:** curva de referência S00 (W05 com valores pela metade, máximo 0.5) na RAM, não a curva original do TYPE 5. Nenhuma parte permanece isoladamente zerada ou dobrada. Ganho, compensação e conjuntos de coeficientes continuam restaurados aos valores do TYPE 5. A flash permanece com o amp distorcido gravado no primeiro slot em P01/P02; nenhuma variação desta série foi salva permanentemente.

## 22. R01–R03 — restauração do amp 0 original

Pedido explícito: “restaure o amp 0 para o original”. Foi validado o SHA-256 da imagem de recuperação e utilizado seu primeiro setor, correspondente ao clean original, em `00:0x60000–0x60FFF`.

R01 zerou temporariamente o volume live, apagou somente o setor do primeiro amp e o programou em blocos alinhados de 64 bytes. A leitura dos 4096 bytes confirmou igualdade integral com o original, SHA-256 `2cef59f28050aa30e5e7b475b5d8e3ac0e0139fbf5fb9f96c363a7c0cc7d308b`.

A conferência adicional do endereço anterior `00:0x5F000` produziu divergência e interrompeu R01 depois da restauração correta da flash. R02 concluiu a restauração live: TYPE A 0, volume 127, banco com GAIN A 4 e TONE A 7; demais campos do banco iguais aos valores conhecidos. O corpo RAM de 1568 bytes em `04:0x1C–0x63B` foi conferido integralmente contra os dados do amp clean original, removendo as curvas experimentais. O setor seguinte completo, `00:0x61000`, permaneceu igual à imagem original.

R03 esclareceu a leitura de `00:0x5F000`: após duas leituras de referência distintas (`00:0x6000C` e `00:0x6500C`), a consulta de 32 bytes em `0x5F000` reproduziu os 31 bytes finais da resposta anterior e colocou `EA`, checksum da requisição, no primeiro byte. Portanto, esse endereço não forneceu uma leitura confiável de memória; a divergência não constitui evidência de alteração de um setor vizinho. Não houve escrita em `0x5F000` nem tentativa de “corrigir” seu conteúdo.

**Estado final confirmado por leitura:** amp 0 clean original restaurado na flash e na estrutura de trabalho; nenhuma curva experimental mantida. Presets não foram salvos. Não foi realizado novo desligamento/religamento nesta restauração; a verificação atual é a comparação integral dos bytes gravados e dos dados em RAM.

## 23. Comparação dos nove relatos com os números originais

Análise somente dos arquivos locais, sem internet e sem comandos ao pedal. Dados: imagem original preservada e [extração completa](CUBE_BABY_AMPS_FABRICA.md). Relatos qualitativos fornecidos pelo usuário para os nove modelos. Não foi confirmado que essa comparação usou controles, gabinete, guitarra e nível de escuta idênticos; os relatos não são medições niveladas de resposta em frequência.

### Relações já sustentadas pelos testes

Alterar a tabela de ganho modificou saturação, sustain e volume percebidos. Reduzir a compensação diminuiu principalmente o nível, mantendo o caráter distorcido. Alterar a curva modificou textura/crunch e corpo. Trocar conjuntos de coeficientes alterou timbre e nível, podendo também mudar a saturação percebida. Essas observações são causais nos testes realizados, mas não revelam toda a ordem de processamento nem garantem a mesma intensidade de efeito em todos os amps.

### Ganho, compensação e escuta por modelo

Os índices 3 e 4 abaixo são dois elementos centrais das tabelas; não são uma identificação do índice usado por um knob físico exatamente em 50%. Valores não estão em dB. Não multiplicar ganho e compensação para prever volume de saída de uma cadeia não linear.

| TYPE / modelo | Relato do usuário | Ganho índices 3 / 4 | Compensação índices 3 / 4 | Leitura possível dos dados |
|---|---|---:|---:|---|
| 0 Power-Zone | Clean cristalino, volume muito baixo | 0.02 / 0.03 | 2 / 2 | Excitação numérica muito menor que nos drives, compatível com limpeza e nível baixo; compensação alta não basta para prever o resultado. |
| 1 US Gold 100 | Graves muito acentuados; um pouco mais alto que Power-Zone, ainda baixo | 0.02 / 0.03 | 1.5 / 1.5 | Mesmo ganho do Power-Zone e compensação menor, porém percebido mais alto: filtros, curva e outros fatores importam. O bloco final contém forte reforço calculado de graves. |
| 2 Two Stone Coral | Bom volume, médios pronunciados, graves sem exagero; equilibrado | 3 / 5 | 0.14 / 0.14 | Base compartilhada com TYPE 7, mas ganho bem menor. Não foi isolado um coeficiente que sozinho explique os médios relatados. |
| 3 Doctor3 B | Mais aberto/presente e ardido que TYPE 2; volume adequado e equilibrado | 20 / 26 | 0.25 / 0.23 | Ganho maior, mas curva, coeficientes e escala interna diferentes; não implica proporcionalidade de distorção. Há um corte calculado em 350 Hz no bloco final. |
| 4 Cali JP A | Moderno/rock, algum excesso de graves | 3 / 5 | 0.145 / 0.14 | Mesmo ganho do TYPE 2, porém curva mais elevada no mesmo índice e filtros diferentes. O excesso de graves não é explicado isoladamente pelos dois filtros finais identificados. |
| 5 Day Tripper | Mais saturado; médio-agudos pronunciados, volume alto | 10 / 20 | 0.12 / 0.12 | Curva idêntica à do clean Power-Zone, mas ganho centenas de vezes maior nesses índices; a curva isolada não define clean versus drive. |
| 6 Shittcow | Mais distorcido que Day Tripper; médio-agudos fortes, volume alto | 60 / 70 | 0.13 / 0.13 | Ganho 6× / 3.5× o de TYPE 5 nesses índices; consistente com maior excitação, sem converter essa razão diretamente em distorção ou volume. |
| 7 Wo Stone Coral | Médios/graves equilibrados; bom volume, distorção com corpo | 20 / 40 | 0.15 / 0.15 | Mesma curva e dois conjuntos principais do TYPE 2, ganho aproximadamente 6.67× / 8× maior, compensação ligeiramente maior e bloco final diferente. |
| 8 Mr Smith | Distorção forte, médios e presença evidentes | 60 / 70 | 0.16 / 0.16 | Mesmo ganho, curva e dois conjuntos principais do TYPE 6; compensação 23.08% maior e diferenças no bloco final. |

Contraprova importante: o US Gold foi percebido mais alto mesmo com compensação 1.5 contra 2 do Power-Zone. Portanto, não há uma regra universal “maior valor nessa tabela = amp mais alto” quando todos os outros blocos também mudam. A influência isolada da compensação foi demonstrada mantendo os demais blocos constantes.

### Descoberta: dois filtros paramétricos numericamente identificáveis no bloco final

Os campos anteriormente tratados apenas como configuração mista contêm dois grupos de cinco floats em **offsets flash relativos `0x5E8–0x5FB` e `0x604–0x617`**. Interpretando-os como `[b0,b1,b2,a1,a2]`, a função é:

```text
H(z) = (b0 + b1*z^-1 + b2*z^-2) / (1 + a1*z^-1 + a2*z^-2)
cos(w0) = -a1 / (1 + a2)
frequência = Fs * w0 / (2*pi)
ganho no centro = 20*log10((b0-b2)/(1-a2))
```

Nos 18 grupos (dois por amp), `b1 == a1`, `b0+b2` coincide com `1+a2` dentro do arredondamento float32, e os polos calculados estão dentro do círculo unitário. Esse padrão é compatível com filtros paramétricos de pico/vale, com ganho unitário nos extremos DC/Nyquist sob essa convenção.

Há ainda metadados que conferem com o cálculo independente dos coeficientes:

| Grupo | Cinco coeficientes | Frequência candidata (uint16 LE) | Ganho codificado (int8) | Outros bytes |
|---|---|---|---|---|
| EQ-A final | `0x5E8–0x5FB` | `0x5FE–0x5FF` | `0x600` | `0x5FC–0x5FD` = 02 00; `0x601–0x603` sem decodificação conclusiva |
| EQ-B final | `0x604–0x617` | `0x61A–0x61B` | `0x61C` | `0x618–0x619` = 02 00; `0x61D–0x61F` sem decodificação conclusiva |

A escala que coincide com o ganho calculado é **`ganho_dB = int8 * 12 / 127`**. Usando **Fs = 48000 Hz**, as frequências calculadas coincidem com os inteiros armazenados. Erro máximo nos 18 filtros: **0.012132 Hz** na frequência e **0.000039 dB** no ganho. Uma reconstrução como biquad de pico/vale, usando frequência e ganho dos metadados e largura inferida dos coeficientes, reproduziu os coeficientes com erro absoluto máximo **1.884e-7**.

Isso é evidência matemática forte de formato e parametrização, além de mera semelhança de números. Não é ainda um teste isolado de ativação desses filtros no DSP, nem uma confirmação da ordem em que são executados. Os bytes remanescentes e os valores de `0x620–0x62B` continuam sem função comprovada. A faixa RAM correspondente aos cinco coeficientes é `04:0x5F8–0x60B` e `04:0x614–0x627`.

| Modelo | EQ-A final: centro | Ganho no centro | EQ-B final: centro | Ganho no centro |
|---|---:|---:|---:|---:|
| Power-Zone | 2233 Hz | +6.33 dB | 7467 Hz | −2.17 dB |
| US Gold 100 | 78 Hz | +6.33 dB | 960 Hz | +7.94 dB |
| Two Stone Coral | 10047 Hz | −1.13 dB | 11733 Hz | +5.57 dB |
| Doctor3 B | 350 Hz | −6.14 dB | 5867 Hz | −7.28 dB |
| Cali JP A | 10047 Hz | −1.13 dB | 15000 Hz | −8.03 dB |
| Day Tripper | 82 Hz | −5.10 dB | 12000 Hz | −7.75 dB |
| Shittcow | 260 Hz | +4.72 dB | 12000 Hz | −6.71 dB |
| Wo Stone Coral | 10047 Hz | −1.13 dB | 10667 Hz | −3.50 dB |
| Mr Smith | 260 Hz | +4.72 dB | 12000 Hz | −7.75 dB |

Esses são valores de estágios individuais calculados, não a resposta total do amp nem um gráfico medido da saída. Um corte em 12 kHz não contradiz presença em 2–5 kHz; frequência central não é toda a largura do filtro. A taxa 48 kHz é sustentada pela correspondência conjunta coeficientes/metadados, não medida por captura de áudio nesta análise.

### Como os filtros finais ajudam a interpretar os relatos

- **US Gold 100:** o reforço calculado em 78 Hz oferece uma explicação concreta e testável para graves acentuados; o reforço em 960 Hz também pode contribuir para nível e corpo. A causalidade acústica isolada ainda não foi testada.
- **Power-Zone:** reforço em 2233 Hz é compatível com clareza/presença percebida, enquanto a tabela de ganho muito baixa ajuda a explicar a limpeza. Não prova sozinho a causa do baixo volume.
- **Doctor3 B:** corte em 350 Hz pode reduzir sensação de abafamento/corpo baixo e favorecer a percepção de abertura em relação a outro modelo. Seu corte em 5867 Hz mostra por que “mais aberto” não deve ser traduzido automaticamente por “mais ganho em todos os agudos”.
- **Day Tripper:** corte em 82 Hz é compatível com menor ênfase de subgraves, deixando médio-agudos relativamente destacados. Ganho e curva explicam parte da saturação, mas não fornecem sozinhos a resposta espectral.
- **Cali JP A:** a comparação com TYPE 2 controla a tabela de ganho, que é igual. Filtros principais, curva, escalar `0x118` e campos finais diferem; ainda não é possível atribuir seu excesso de graves a um único campo.
- **Shittcow/Mr Smith:** os dois filtros EQ-A são iguais. O EQ-B tem o mesmo centro de 12 kHz, mas ganho e largura diferentes; compensação também muda. Logo, diferenças de médios/presença não podem ser atribuídas a uma curva ou tabela de ganho diferente, pois são iguais nesse par. Outros campos finais e condições da escuta ainda podem participar.
- **Two Stone/Wo Stone:** compartilham curva, coeficientes A/B principais e EQ-A final. Ganho, compensação, EQ-B e o campo `0x628` diferem. Isso restringe os candidatos que explicam maior corpo e saturação do Wo Stone sem exigir hipóteses sobre curvas completamente diferentes.

### O que a curva diz — e o que não diz

No ponto 64/255, a curva do Cali JP A vale aproximadamente 0.927, contra 0.706 no Two Stone, apesar da mesma tabela de ganho. Isso mostra uma diferença importante na transferência numérica, mas não revela por si só equalização, compressão dinâmica ou o ponto realmente consultado durante a execução. Power-Zone e Day Tripper têm curva exatamente igual; Two Stone e Wo Stone também; Shittcow e Mr Smith também. Os resultados dos oito trechos não permitem transformá-los em oito bandas de EQ.

**Conclusão:** já é possível relacionar blocos a comportamento e formular explicações testáveis por modelo. A descoberta dos dois filtros paramétricos finais fornece agora frequências e ganhos matematicamente consistentes, que devem ser distinguidos de resposta acústica total ou causalidade já confirmada. Nenhuma configuração do pedal foi alterada nesta comparação; o primeiro amp permanece restaurado ao original.


## 24. Rockerverb clean: prototipo RV01 em RAM

Referencia: `untitled.mp3`, somente guitarra clean segundo o usuario; escuta em monitores PreSonus. Decodificacao local com Media Foundation: 41.0525 s, 44100 Hz, stereo; pico -1.32 dBFS, RMS -17.46 dBFS. A energia do trecho concentra-se em 80-640 Hz. Essas medidas incluem notas, guitarra, gabinete e captacao; nao equivalem a resposta do amplificador. Nao houve audicao direta do arquivo pelo assistente nem captura comparativa com a mesma DI.

Primeira aproximacao preparada em [CUBE_BABY_ROCKERVERB_CLEAN_V1.json](artefatos/CUBE_BABY_ROCKERVERB_CLEAN_V1.json) e aplicada com sucesso em RAM. Base: TYPE 0 Power-Zone de fabrica, conferido integralmente antes da escrita. Parametros escolhidos para escuta, nao extraidos do Rockerverb real:

| RAM selector 04 | Alteracao |
|---|---|
| 0x3C, 32 bytes | Oito valores de compensacao: 2.0 para 3.0 |
| 0x5F8, 20 bytes | Substituicao do EQ-A final: 200 Hz, +3 dB, Q 0.7 |
| 0x614, 20 bytes | Substituicao do EQ-B final: 2800 Hz, +1.5 dB, Q 0.8 |

Os ganhos dos filtros sao relativos a unidade, nao acrescimos sobre os filtros originais. Coeficientes calculados para 48 kHz; raios maximos dos polos 0.98439 e 0.81187. Metadados de frequencia/ganho/Q e flags permaneceram originais: este artefato destina-se somente ao ensaio dos coeficientes em RAM. A atuacao acustica dos filtros ainda depende da escuta do usuario.

Ganho, curva de 256 pontos e dois conjuntos principais de coeficientes preservados. Leitura integral dos 1568 bytes confirmou o corpo esperado apos as tres escritas; SHA256 `c098faa56015fd495472b478fb8402fd6afbc822dd6a0bbb5b22ed55556d1b57`. Banco de 48 bytes permaneceu identico. Nenhum comando de escrita ou apagamento de flash foi enviado. Prototipo deixado ativo para escuta; nao ha confirmacao de equivalencia sonora ao amp real.


### RV02 - mais nivel, medios e medio-agudos

Usuario relatou volume baixo na RV01 e pediu mais compressao, medios e medio-agudos. Aplicada em RAM apos confirmar SHA256 integral da RV01: ganho original multiplicado por 1.5 (oito floats em 0x1C), compensacao de 3 para 4.5 (0x3C), EQ-A substituido por 700 Hz/+3 dB/Q 0.7 (0x5F8), EQ-B por 2800 Hz/+3 dB/Q 0.8 (0x614). Curva original preservada. O ganho maior busca maior compressao pela nao linearidade existente, mas pode gerar crunch; nao foi implementado nem identificado compressor com ataque/release.

Leitura integral confirmou os bytes: SHA256 `617dae5706f27f4f06c5f6c57b11399e0a7fc7744f3fdc8ef497526f63610134`. Banco de presets inalterado; nenhuma escrita em flash. RV02 deixada para escuta, resultado sonoro pendente. O JSON RV01 continua representando a primeira versao, nao o estado atual RV02.


### RV03 - reforco de medios em 900-1200 Hz

A pedido do usuario, EQ-A final substituido por filtro de pico em 1050 Hz, +5 dB, Q 2 (RAM 04:0x5F8, 20 bytes). O reforco concentra-se na regiao solicitada, sem cortes abruptos nos limites 900 e 1200 Hz. Substitui 700 Hz/+3 dB/Q 0.7 da RV02. Demais bytes do corpo e banco de presets preservados; nenhuma escrita em flash. Pre-condicao RV02 conferida e leitura integral posterior validada. SHA256 do corpo: `1981b1ed94b5e49a85dd7327fc5ad6256459755c9da37130298472c800047771`. Bytes enviados: `a97e823f373cf7bf40306e3f373cf7bf932d733f`. Deixado ativo para escuta; resultado sonoro pendente.


### RV04 - mais ganho para resposta a palhetada forte

Usuario relatou RV03 ainda limpa demais mesmo tocando forte. Dobrados somente os oito valores da tabela de ganho atual (RAM 04:0x1C, 32 bytes), agora aproximadamente [0.021, 0.024, 0.027, 0.060, 0.090, 0.120, 0.150, 0.180], equivalentes a 3 vezes a tabela de fabrica. Compensacao, curva e filtros preservados; o volume percebido pode mudar como consequencia do ganho. Objetivo: avaliar surgimento de crunch na palhetada forte, ainda sem confirmacao sonora.

RV03 conferida antes da escrita. Leitura integral confirmou RV04; banco de presets inalterado e nenhuma escrita em flash. SHA256 do corpo: `cfb41283e779c9fe331d039fb21389fe1991347ee3fab7391d2e63f40df34e20`. Bytes enviados: `3208ac3ca69bc43c1a2fdd3c8fc2753deb51b83d8fc2f53d9a99193eeb51383e`. Deixada ativa em RAM para escuta.


### RV05 - busca de crunch e corpo em 200 Hz

Usuario: RV04 nao produziu crunch nem tocando forte; pediu pequeno reforco entre 150 e 250 Hz. Tabelas originais consultadas: TYPE 2 [0.5,1,2,3,5,7,10,15], TYPE 5 [0.5,1,5,10,20,35,50,60]. A RV04 permanecia muito abaixo desses valores. Hipotese de trabalho: excitacao insuficiente da curva nao linear; valores da tabela nao permitem deduzir sozinhos limiar de crunch.

Aplicados em RAM: ganho RV04 multiplicado por 5, aproximadamente [0.105,0.120,0.135,0.300,0.450,0.600,0.750,0.900] em 04:0x1C; EQ-B substituido de 2800 Hz/+3 dB/Q 0.8 para 200 Hz/+2 dB/Q 1.2 em 04:0x614. Preservados EQ-A 1050 Hz/+5 dB/Q 2, curva e compensacao. O reforco nao se limita abruptamente a 150-250 Hz. Alteracoes simultaneas atendem ao pedido, mas nao constituem isolamento acustico de cada variavel.

RV04 integralmente conferida antes da escrita. Leitura posterior confirmou RV05; SHA256 do corpo `4b217305d13803b9ef07d08ad9bfde96c5acee78ac9cf8e03faca3b88ebe4b1b`. Demais bytes e banco preservados, nenhuma escrita em flash. Crunch ainda depende da confirmacao do usuario. Deixada ativa para escuta.


### RV06 - metade do ganho da RV05

Usuario confirmou que o crunch da RV05 veio muito bem e pediu reduzir o ganho em 2 vezes. Aplicada somente divisao por 2 nos oito floats de RAM 04:0x1C: aproximadamente [0.0525,0.0600,0.0675,0.1500,0.2250,0.3000,0.3750,0.4500]. Equalizacao, curva e compensacao preservadas. RV05 conferida antes da escrita; leitura integral confirmou RV06. Banco inalterado, nenhuma escrita em flash. SHA256 do corpo: `17830faeff6b3f4e4b2f819bc255faa424a75b4bce56ffe6a647a29c5e23c3ef`. Deixada ativa em RAM para escuta; avaliacao sonora da RV06 pendente.


**Retorno da escuta RV06:** usuario confirmou "SIM MUITO BOM" ao avaliar se o crunch ficou no ponto. RV06 aprovada na escuta; isso nao confirma equivalencia ao Rockerverb real. Nenhuma nova alteracao enviada ao pedal; configuracao permanece somente em RAM.
