# Cube Baby — comandos enviados e recebidos

[Documentação](../../CubeControl-1.1/Cube/docs/README.md) · [Arquitetura atual](../../CubeControl-1.1/Cube/docs/arquitetura.md)

Pesquisa iniciada em 26/09/2026, com verificação Bluetooth em 30/09/2026 e revisão documental em 01/10/2026. Modelo: CUVAVE/M-VAVE **Cube Baby de guitarra**. Não extrapolar para Cube Baby Bass, AC, CubeTurner, Chocolate ou Blackbox.

Este documento reúne todos os opcodes encontrados nas fontes e capturas consultadas, os parâmetros acessíveis por memória e suas respostas. **Não é uma especificação oficial exaustiva do firmware**: comandos não observados, campos desconhecidos e diferenças de implementação estão identificados. A pesquisa inicial foi estática; depois foram realizadas as consultas físicas descritas na seção 10, sem comandos de escrita ou apagamento.

## Navegação

- [1. Fontes e grau de evidência](#1-fontes-e-grau-de-evidência)
- [2. Transporte e formato](#2-transporte-e-formato)
- [3. Catálogo dos comandos observados](#3-catálogo-dos-comandos-observados)
- [4. Memórias e endereços](#4-memórias-e-endereços)
- [5. Todos os parâmetros de preset mapeados](#5-todos-os-parâmetros-de-preset-mapeados)
- [6. Operações completas](#6-operações-completas)
- [7. Exemplos de bytes](#7-exemplos-de-bytes)
- [8. Comandos adicionais encontrados, sem suporte comprovado na Cube Baby](#8-comandos-adicionais-encontrados-sem-suporte-comprovado-na-cube-baby)
- [9. O que o pedal pode enviar e o que continua desconhecido](#9-o-que-o-pedal-pode-enviar-e-o-que-continua-desconhecido)
- [10. Teste físico no pedal conectado em 26/09/2026](#10-teste-físico-no-pedal-conectado-em-26092026)
- [11. Captura de atualização de firmware em 03/10/2026](#11-captura-de-atualização-de-firmware-em-03102026)
- [12. Catálogo dos firmwares incluídos no app](#12-catálogo-dos-firmwares-incluídos-no-app)
- [13. Investigação de entrada em modo flash via USB](#13-investigação-de-entrada-em-modo-flash-via-usb)

## 1. Fontes e grau de evidência

- **Capturado**: mensagem encontrada nos arquivos públicos de tráfego CubeSuite ↔ Cube Baby; comprova o formato naquela sessão, não em todos os firmwares.
- **Implementado/documentado**: consta em código específico da Cube Baby, mas nem toda afirmação possui captura pública correspondente.
- **Genérico/não confirmado**: encontrado no Android compartilhado com outros produtos, ou interpretação ainda sem comprovação suficiente.

Fontes locais:

- [Communication.java](../apk-cubesuite/sources/com/cubeSuite/communication/Communication.java): construção de pacotes, consultas, seletores de memória e checksum.
- [CommunicationUtil.java](../apk-cubesuite/sources/com/cubeSuite/communication/CommunicationUtil.java): envio BLE e interpretação das respostas.
- [CubeBaby.java](../apk-cubesuite/sources/com/cubeSuite/entity/Instrument/CubeBaby.java): classe vazia; não fornece mapa específico do pedal.
- [BleEnum.java](../apk-cubesuite/sources/com/cubeSuite/config/BleEnum.java): UUIDs do transporte BLE compartilhado.
- [LooperCommunication.java](../apk-cubesuite/sources/com/cubeSuite/communication/LooperCommunication.java): evidencia que os CC encapsulados do looper pertencem a outro produto.

Fontes externas, consultadas diretamente:

1. [Fabricante — downloads](https://www.cuvave.com/download): informa o suporte da versão PC do CubeSuite à Cube Baby por USB; não fornece, nessa página, uma tabela de opcodes.
2. [Pesquisa original de Pedro Ferreira](https://garden.turbofish.cc/notes/Reverse-engineering-the-Cuvave-Cube-Baby): engenharia reversa do SysEx USB.
3. [cuvave-midi](https://github.com/pferreir/cuvave-midi/tree/1ec042829751a9f32e3692612ef7b734531e2e0c), revisão `1ec042829751a9f32e3692612ef7b734531e2e0c`: [parser](https://github.com/pferreir/cuvave-midi/blob/1ec042829751a9f32e3692612ef7b734531e2e0c/src/parser.rs) e [mapa de memória](https://github.com/pferreir/cuvave-midi/blob/1ec042829751a9f32e3692612ef7b734531e2e0c/README.md).
4. CubeControl/ToneHub, revisão `0a06a5bfedd5d39259c080922e9face1376db0d9`:
   - [Construtores de comandos](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/requests.ts).
   - [Decodificador](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/decoder.ts).
   - [Empacotamento de sete bits](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/seven-bit.ts).
   - [Parâmetros live](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/live.ts) e [banco de presets](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/preset.ts).
   - [Formato de IR](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-protocol/src/ir.ts) e [sequências da API](https://github.com/MrGariZack/cubecontrol/blob/0a06a5bfedd5d39259c080922e9face1376db0d9/packages/cube-baby-api/src/client.ts).
   - [Capturas públicas](https://github.com/MrGariZack/cubecontrol/tree/0a06a5bfedd5d39259c080922e9face1376db0d9/test/fixtures/captures): 18 arquivos, de `003-cubesuite-startup` a `021-ir-upload`, com lacunas de numeração.

Nesta análise foram decodificados **584 envelopes** desses 18 arquivos. Todos apresentaram comprimento e checksum consistentes. Isso valida a interpretação dos arquivos, não o funcionamento em hardware nesta sessão. As capturas contêm os opcodes `00`, `11`, `12`, `21`, `22`, `23` e `24`.

## 2. Transporte e formato

### USB-MIDI da Cube Baby

O tráfego usa MIDI System Exclusive:

```text
F0 PACK7(envelope) F7
```

`PACK7` distribui os bits do envelope em grupos consecutivos de sete bits, começando pelos bits menos significativos. Cada byte interno do SysEx fica entre `00` e `7F`. Não se trata simplesmente de remover o bit mais alto de cada byte.

A implementação CubeControl completa a saída com septetos zero até `ceil(8 × tamanho_do_envelope / 7)`. Seu código registra que omitir esse preenchimento pode fazer o pedal ignorar algumas escritas. A implementação Rust antiga adiciona um septeto final mesmo em certos casos já alinhados; as capturas devem prevalecer ao reproduzir uma mensagem.

Envelope **antes** de PACK7:

| Offset | Tamanho | Campo |
|---|---:|---|
| 0 | 2 | Cabeçalho `00 59` |
| 2 | 1 | Opcode |
| 3 | 3 | Tamanho do payload, unsigned little-endian de 24 bits |
| 6 | N | Payload |
| 6 + N | 1 | Checksum |

`checksum = (~sum(payload)) & 0xFF`. Não inclui cabeçalho, opcode, tamanho ou o próprio checksum. Payload vazio resulta em `FF`. O envelope possui `7 + N` bytes.

Nesta documentação, `M` é o seletor de memória de um byte; `A32` é um endereço LE32; `N24` é um tamanho LE24; `V32` é um valor LE32; `D` são os bytes de dados. Hexadecimais usam `0x` ou blocos de bytes; faixas de valores e tamanhos são decimais.

### BLE encontrado no Android

O código compartilhado transmite o envelope `00 59 ...` pelo serviço `0000ae40-0000-1000-8000-00805f9b34fb`, escrita `0000ae41-0000-1000-8000-00805f9b34fb` e notificações `0000ae42-0000-1000-8000-00805f9b34fb`.

Esses UUIDs **não comprovam controle BLE da Cube Baby**. O suporte Bluetooth de áudio não comprova um serviço BLE de edição. No USB-MIDI, usar o encapsulamento SysEx acima.

### Verificação Bluetooth no Windows — 30/09/2026

O usuário confirmou o pedal ligado e com Bluetooth em modo de emparelhamento. O adaptador Intel do computador estava presente, mas o rádio Bluetooth estava desligado; foi ativado para a investigação e permaneceu ligado ao final.

A consulta nativa de Bluetooth clássico retornou `CubeBaby`, já emparelhado com o Windows e sem conexão ativa naquele momento. `BluetoothEnumerateInstalledServices` retornou os três serviços abaixo. Essa API enumera os serviços conhecidos pelo Windows; o resultado não deve ser interpretado como prova de ausência de qualquer serviço oculto no firmware.

| UUID do serviço | Perfil | Finalidade |
|---|---|---|
| `0000110b-0000-1000-8000-00805f9b34fb` | Audio Sink (A2DP) | Recepção de áudio Bluetooth |
| `0000110c-0000-1000-8000-00805f9b34fb` | A/V Remote Control Target (AVRCP) | Controle de reprodução de áudio/vídeo |
| `0000110e-0000-1000-8000-00805f9b34fb` | A/V Remote Control (AVRCP) | Controle de reprodução de áudio/vídeo |

Nomes dos perfis conferidos na [tabela de identificadores da Bluetooth SIG](https://www.bluetooth.com/wp-content/uploads/Files/Specification/HTML/Assigned_Numbers/out/en/index-en.html). Esses perfis não demonstram suporte à edição de efeitos, presets ou IRs.

Uma busca BLE ativa de 15 segundos, usando `BluetoothLEAdvertisementWatcher`, recebeu anúncios de outro aparelho próximo, mas nenhum identificado como Cube Baby. Portanto, não foi possível consultar serviços GATT do pedal nessa sessão. A ausência de anúncios nessa janela não exclui outros modos de operação ou serviços proprietários não descobertos.

A lista `BLE_DEVICE_CONFIGS` de [BleHandle.java](../apk-cubesuite/sources/com/cubeSuite/bluetooth/BleHandle.java) não inclui Cube Baby. Os UUIDs `AE40`/`AE41`/`AE42` do Android continuam sendo evidência de suporte BLE genérico do CubeSuite, não de suporte deste pedal.

**Conclusão:** o Windows reconhece o Cube Baby pelo Bluetooth clássico, mas não foi encontrado um canal Bluetooth para controlar seus parâmetros. Os serviços observados são de áudio e controle de reprodução. O controle pelo Bluetooth integrado permanece sem suporte comprovado; implementar apenas um transporte Bluetooth no CubeControl não resolve essa lacuna. A comunicação USB-MIDI continua sendo o caminho validado. Não foram enviados comandos de alteração de presets ou firmware durante esta verificação.

## 3. Catálogo dos comandos observados

Host = computador/editor; pedal = Cube Baby.

| Opcode | Direção observada | Payload | Função e evidência |
|---|---|---|---|
| `0x00` | Pedal → host | Um byte de status | ACK de operações de escrita, apagamento e controle IR. Nas capturas, status `00`; nas consultas físicas adicionais, status `01`. Ver ressalva abaixo. |
| `0x11` | Host → pedal | Vazio | Solicitar identificação/nome e informações de versão. Capturado na inicialização. |
| `0x11` | Pedal → host | Texto + metadados binários | Responder à identificação; não assumir que todo o payload seja uma string. |
| `0x12` | Host → pedal | Vazio | Consultar informações adicionais/curtas. Capturado na inicialização; significado exato não resolvido. |
| `0x12` | Pedal → host | Seis bytes na captura | Resposta observada `01 00 00 00 00 00`; campos sem interpretação comprovada. |
| `0x21` | Host → pedal | `M A32` | Apagar bloco/setor de memória. Capturado no upload do IR em flash. |
| `0x22` | Host → pedal | `M A32 N24 D` | Escrever memória: parâmetros live, banco persistente ou dados de IR, conforme seletor/endereço. |
| `0x23` | Host → pedal | `M A32 N24` | Ler memória. |
| `0x23` | Pedal → host | `M A32 N24 D` | Entregar os bytes solicitados, repetindo seletor, endereço e tamanho. |
| `0x24` | Host → pedal | `M A32 V32` | Operação auxiliar de registro/atualização do DSP no fluxo de IR. O papel interno exato é parcialmente desconhecido. |

Fontes: capturas `003`, `004–017`, `020`, `021`; construtores e decodificadores citados na seção 1.

### Identificação: `0x11`

Exemplo capturado, já decodificado:

```text
Pedido:   00 59 11 00 00 00 FF
Resposta: 00 59 11 1B 00 00
          53 49 4E 43 4F 2D 43 75 62 65 42 61 62 79 20 20 20 20 20
          01 13 14 15 16 17 18 19
          1E
```

O texto é `SINCO-CubeBaby` seguido de espaços; o payload completo tem 27 bytes. Os oito bytes finais são metadados sem significado individual confirmado. Os parsers divergem sobre o limite do nome, portanto não fixar cegamente um campo de 16 bytes ou interpretar esses metadados como uma versão sem evidência.

### Informações adicionais: `0x12`

```text
Pedido:   00 59 12 00 00 00 FF
Resposta: 00 59 12 06 00 00 01 00 00 00 00 00 FE
```

`Communication.java` chama esse opcode tanto `CMD_Q_SHORTE_MSG` quanto `CMD_Q_OTHER_INFO`. Sua presença na Cube Baby está comprovada pelas capturas, embora `decoder.ts` o devolva como desconhecido. Não há base para chamar seus bytes de preset ativo, bateria ou modo.

### Resposta ACK: `0x00`

```text
00 59 00 01 00 00 00 FF
```

O Android considera status `00` sucesso. Os parsers Rust e TypeScript convertem `status > 0` em um booleano positivo, porém as próprias capturas de escritas usam `00`. Assim, **`accepted=false` dessas bibliotecas não prova que a escrita falhou**. Preservar o byte bruto e, quando aplicável, conferir o resultado por leitura e comparação. Não foi encontrada uma tabela confiável de outros códigos de erro.

O ACK não transporta endereço nem identificador da requisição. Correlacionar operações em sequência; apenas receber um ACK não comprova persistência após desligamento.

### Leitura e escrita: `0x23` / `0x22`

Em leitura, o payload do pedido tem 8 bytes. Na resposta tem `8 + N` bytes. Na escrita também tem `8 + N` bytes. O primeiro tamanho do envelope conta todo o payload; `N24` conta somente os dados.

A leitura do banco A+B+C usa `M=05`, `A=00000000`, `N=48`. A edição live usa `M=05`, endereços a partir de `80000000`. Portanto, edição live e gravação persistente são operações distintas, apesar de compartilharem opcode e seletor. A leitura em endereço zero pode refletir as alterações live: não é uma leitura independente da flash. Essa distinção foi confirmada posteriormente na seção 12 da [investigação dos amps](CUBE_BABY_AMPS.md).

### Apagamento: `0x21`

O payload tem 5 bytes e **não contém tamanho**. Na captura `021`, o host envia `M=00`, `A=00070000` antes de gravar IR. A implementação de referência trata o setor como 4096 bytes. Não inferir que esse tamanho valha para toda memória ou todo firmware. Apagar esse setor elimina o conteúdo anterior do gabinete correspondente.

### Controle auxiliar de IR: `0x24`

Combinações efetivamente capturadas:

| M | Endereço | V32 | Interpretação limitada pela evidência |
|---|---|---|---|
| `04` | `00000764` | `00000001` | Atualização associada à flag do IR, enviada depois de escrever a flag tanto em 0 quanto em 1. |
| `04` | `00000768` | `00000804` (2052) | Atualização associada ao bloco de IR de 2052 bytes. |

O último campo é um inteiro de quatro bytes nesse formato. Não confundi-lo com `N24` de `0x22`, nem concluir que seja sempre um valor de registro: na segunda operação coincide com o tamanho do bloco. O parser Rust originalmente chama essas operações `Mystery1` e `Mystery2`.

## 4. Memórias e endereços

| M | Endereço/faixa | Uso e certeza |
|---|---|---|
| `00` | `00002100`, 20 bytes | Bloco lido na inicialização; estrutura não decodificada nesta pesquisa. |
| `00` | `00003000`, 1 byte | Lido na inicialização; o README Rust o identifica como USB loopback. A captura retorna `01`, mas não comprova escrita ou semântica completa. |
| `05` | `00000000`, 48 bytes | Banco persistente A+B+C; leitura e gravação capturadas. |
| `05` | `80000000–8000000C` | Treze campos live do slot A; escritas capturadas. |
| `05` | `80000010–8000001C` | Campos live do slot B; mapeados pela implementação e relatos de experimentos posteriores. |
| `05` | `80000020–8000002C` | Campos live do slot C; mapeados pela implementação e relatos de experimentos posteriores. |
| `04` | `00000764`, 1 byte | Flag de uso/atualização de IR; escritas `00` e `01` capturadas. |
| `04` | `00000768`, 4 bytes | Float32 LE chamado `distance` no bloco live. |
| `04` | `0000076C–00000F6B` | 2048 bytes de amostras IR: 512 float32 LE. |
| `00` | `00069000 + slot × 1000` | Setores de IR persistentes; slot 0–7, stride hexadecimal `1000` = 4096 bytes. Mapa documentado na implementação. |
| `00` | `00070000` | Slot 7, gabinete 8; destino de upload capturado. |

Os experimentos 037/038 citados por `live.ts` não estão entre as 18 capturas públicas analisadas. A existência das três regiões B/C no código foi conferida, mas não foi reproduzida em um pedal.

Seletores genéricos declarados no Android: `00 NOR_EXT`, `01 NAND_EXT`, `02 SD_EXT`, `03 EFF_EXT`, `04 DEV_EXT`, `05 USR_EXT`, `06 RSV1_EXT`, `07 RSV2_EXT`. Isso não comprova que todos estejam implementados na Cube Baby. Em particular, o auxiliar Android chamado `makeWritePacketToRAM` usa `06`, enquanto as operações específicas acima usam `04` e `05`.

## 5. Todos os parâmetros de preset mapeados

Cada slot possui 16 bytes. A=0, B=1, C=2:

```text
Endereço live = 0x80000000 + slot × 16 + offset
Posição no banco persistente = slot × 16 + offset
```

Escrever um parâmetro live: `0x22`, `M=05`, endereço calculado, `N=1`, valor de um byte. A leitura do banco em endereço zero refletiu alterações live nos ensaios locais posteriores; ler de volta não comprova gravação persistente.

| Offset | Campo | O que faz | Faixa adotada pela referência |
|---|---|---|---|
| `00` | `type` | Seleciona o modelo de preamp/drive | 0–8 |
| `01` | `gain` | Ganho do preamp/drive | 0–7 |
| `02` | `tone` | Tonalidade do preamp | 0–15 |
| `03` | `reverb` | Quantidade de reverb | 0–15 |
| `04` | `feedback` | Realimentação/repetições do delay | 0–127 |
| `05` | `volume` | Volume do preset; campo separado do IR | 0–127 |
| `06` | `time` | Tempo do delay, em índice bruto | 0–31 |
| `07` | `mix` | Mistura do delay | 0–118 |
| `08` | `modulation` | Ajuste de chorus/phaser | 0–15 |
| `09` | `cabinet` | Desliga ou seleciona gabinete IR | 0–8 |
| `0A` | `irSection` | Habilitação da seção IR | 0/1 |
| `0B` | `delaySection` | Habilitação da seção delay | 0/1 |
| `0C` | `toneSection` | Habilitação da seção tone/preamp | 0/1 |
| `0D–0F` | Sem nome | Bytes finais, observados como zero; preservar | Não definida |

As faixas vêm de `live.ts`; não foram todas percorridas nas capturas disponíveis. Por exemplo, a captura de GAIN contém 3–7, TONE 2–13 e MIX 7–14. Portanto, os limites da biblioteca não são uma medição independente desta pesquisa. Não converter automaticamente TIME para milissegundos, VOLUME para dB ou MIX para percentual.

Segundo a implementação, MOD 0–6 corresponde a chorus, 7–8 a desligado e 9–15 a phaser. **MOD=0 não significa desligado.** Para as flags de seção, a interpretação implementada é 0=desligada e 1=ligada.

`cabinet=0` desliga a seleção IR; `1–8` seleciona os slots ROM `0–7`. A referência registra que 0 não recarrega a RAM do IR. As posições físicas 1–9 do knob e os valores digitais 0–8 usam bases diferentes.

**Selecionar o endereço de edição A/B/C não é acionar o footswitch correspondente.** As capturas `018-preset-switch-a-b-c` e `019-footswitch-a-b-c` contêm consultas de inicialização, sem comando de ativação de preset e sem notificação específica comprovada de footswitch.

## 6. Operações completas

### Inicialização observada

Na captura `003`, o host executa, nesta ordem:

1. `0x11`, identificação; recebe `0x11`.
2. `0x23`, memória `00`, endereço `00002100`, 20 bytes; recebe os dados.
3. `0x12`, informações adicionais; recebe seis bytes.
4. `0x23`, memória `05`, endereço zero, 48 bytes; recebe o banco A+B+C.
5. `0x23`, memória `00`, endereço `00003000`, um byte; recebe `01`.

É uma sequência capturada do CubeSuite, não uma prova de que todas as consultas sejam obrigatórias em qualquer editor.

### Salvar presets

Na captura `020`, há uma escrita live de VOLUME A para 100, seguida de uma escrita dos **48 bytes completos** na memória `05`, endereço zero, e uma leitura de conferência. A API da referência acrescenta uma leitura inicial para preservar os demais campos.

Não foi observado opcode exclusivo “Save”. Backup é leitura do banco; restauração é escrita do banco. Preservar os outros presets e os bytes sem nome ao modificar uma parte do banco.

### Carregar IR somente na RAM

Sequência capturada em `021`:

1. `0x22`: memória `04`, `00000764`, um byte `00`.
2. `0x24`: memória `04`, `00000764`, inteiro `1`.
3. `0x22`: bloco de 2052 bytes a partir de `00000768`, dividido em 16 partes de 128 bytes e uma parte final de 4 bytes.
4. `0x24`: memória `04`, `00000768`, inteiro `2052` (`00000804`).
5. `0x22`: memória `04`, `00000764`, um byte `01`.
6. `0x24`: memória `04`, `00000764`, inteiro `1`.

O bloco começa com um float32 LE chamado distância/MIC DIST, seguido de 512 amostras float32 LE, mono a 48 kHz. Isso equivale a aproximadamente 10,67 ms de amostras. Os nomes “distance” no live e “volume” no cabeçalho ROM divergem no código de referência; a unidade física e a interpretação exata desse float não estão resolvidas aqui.

### Gravar IR persistentemente

O fluxo de upload capturado continua com `0x21` em memória `00`, endereço `00070000`, seguido de escritas `0x22` e leituras `0x23` para conferência.

- Imagem enviada pelo CubeSuite: 2056 bytes = cabeçalho de 8 bytes + 512 amostras de 4 bytes.
- Cabeçalho da implementação: quatro bytes de presença/tipo (`01 00 00 00` em imagens de fábrica, `00 00 00 00` em upload) + um float32 LE.
- Escrita em partes de até 128 bytes; leitura de conferência da referência em partes de até 113 bytes. Esses valores são escolhas observadas/implementadas, não máximos universais do protocolo.
- Setor físico modelado: 4096 bytes. A referência atual completa a imagem de 2056 bytes com zeros; isso difere de deixar a cauda apagada em `FF` após reproduzir somente o upload do CubeSuite.
- A capacidade do setor não comprova que o DSP processe 1022 amostras: o bloco live analisado contém 512.
- Outros slots usam `00069000 + slot × 0x1000`; substituir slots 0–6 sobrescreve gabinetes de fábrica. A captura analisada comprova diretamente o upload no slot 7.

## 7. Exemplos de bytes

Exceto onde explicitamente chamado **SysEx**, cada linha é um envelope decodificado e precisa passar por PACK7 antes do envio USB. Os exemplos de escrita/apagamento descrevem tráfego; não foram executados nesta pesquisa.

```text
# Identificar — envelope
00 59 11 00 00 00 FF
# Identificar — SysEx capturado completo
F0 00 32 45 00 00 00 40 7F F7

# Consultar informações adicionais — envelope
00 59 12 00 00 00 FF

# Ler banco A+B+C (48 bytes)
00 59 23 08 00 00 05 00 00 00 00 30 00 00 CA

# VOLUME A = 100, somente live
00 59 22 09 00 00 05 05 00 00 80 01 00 00 64 10

# Selecionar cabinet digital 1 no slot A — envelope
00 59 22 09 00 00 05 09 00 00 80 01 00 00 01 6F
# Mesma escrita — SysEx completo da referência Rust
F0 00 32 09 49 00 00 40 02 09 00 00 00 18 00 00 00 01 5E 01 F7

# ACK observado
00 59 00 01 00 00 00 FF

# Controle auxiliar do bloco IR de 2052 bytes
00 59 24 09 00 00 04 68 07 00 00 04 08 00 00 80

# Apagar setor do gabinete 8 — operação destrutiva
00 59 21 05 00 00 00 00 00 07 00 F8
```

## 8. Comandos adicionais encontrados, sem suporte comprovado na Cube Baby

| Opcode | Origem/nome | O que a fonte permite afirmar |
|---|---|---|
| `0x00`, payload vazio | `Init` no parser Rust | O parser reconhece a forma, mas sua direção, necessidade e efeito no pedal não foram comprovados pelas capturas consultadas. Não confundir com ACK de um byte. |
| `0x13` | `CMD_Q_FIRMWARE_VER` | Consulta genérica de firmware declarada no Android; teste físico retornou somente ACK/status `01`, sem versão (seção 10). |
| `0x15` | `CMD_MIDI_WRITE` | Encapsula bytes MIDI; usado pelo looper no Android. Não comprova CC/PC na Cube Baby. |
| `0x17` | `CMD_Q_DEV_TYPE` | Consulta genérica; teste físico retornou ACK/status `01`, sem tipo (seção 10). |
| `0x1B` | `CMD_Q_SOUND_DATA_ADDR` | Consulta genérica; teste físico retornou ACK/status `01`, sem endereço (seção 10). |
| `0x20` | `CMD_Q_CODEC_VAL` | Consulta genérica; teste físico retornou ACK/status `01`, sem valor de codec (seção 10). |

Para as consultas genéricas, `makeQueryPacket` constrói payload vazio; a mera existência desse construtor ou dessas constantes não demonstra que o firmware responda. O opcode `0x12`, embora também declarado genericamente, foi separado porque aparece de fato nas capturas da Cube Baby.

## 9. O que o pedal pode enviar e o que continua desconhecido

Mensagens comprovadas pedal → host: identificação `0x11`, informações adicionais `0x12`, conteúdos de memória `0x23` e ACK `0x00`. Não foi encontrada, no conjunto analisado, evidência de transmissão espontânea de notas MIDI, CC, Program Change, posição de knobs, preset ativo, estado do afinador ou eventos A/B/C.

Também não foram confirmados, nesse transporte USB-MIDI, comandos remotos de ativação de A/B/C, bypass global, afinador, tap tempo, reset de fábrica, reinicialização ou atualização de firmware. A ausência nessas fontes **não prova impossibilidade**; significa que não há bytes confiáveis para documentar tais funções como suportadas. A atualização pelo bootloader USB Mass Storage foi capturada posteriormente e está descrita na seção 11.

O protocolo permite endereçamento genérico, mas isso não autoriza tratar todos os endereços como conhecidos. Permanecem abertos os campos de `0x12`, a estrutura completa de `00002100`, a semântica de todos os status ACK, os metadados de identificação e a extensão real do mapa de memória. Este catálogo reúne análise estática, capturas e as consultas físicas da seção 10. As limitações de cada etapa são próprias dela; comandos de escrita descritos pelas capturas não foram executados naquela rodada de consultas.


## 10. Teste físico no pedal conectado em 26/09/2026

### Procedimento

Comunicação direta pelo WinMM do Windows, sem instalar bibliotecas e sem criar scripts persistentes. Entrada e saída selecionadas pelo nome exato `USB-Midi`, ambas únicas. Dispositivo USB enumerado com `VID_301A&PID_5555`, interface `MI_03`. A identificação retornou `SINCO-CubeBaby`, com o mesmo payload de 27 bytes apresentado na seção 3. Não foi possível extrair uma versão de firmware confiável desses metadados.

Foi enviado um pedido de cada consulta, com janela de recebimento de 1,5 segundo. Antes dos testes e depois de cada consulta, `0x11` confirmou que o pedal continuava respondendo com identificação válida. Todas as respostas abaixo passaram pela verificação de comprimento e checksum. Os bytes desta tabela são envelopes decodificados, antes/depois do empacotamento SysEx.

| Consulta | Pedido | Resposta | Conclusão física |
|---|---|---|---|
| `0x12` | `00 59 12 00 00 00 FF` | `00 59 12 06 00 00 01 00 00 00 00 00 FE` | Consulta respondida; reproduz a captura pública. Os seis campos continuam sem significado confirmado. |
| `0x13` | `00 59 13 00 00 00 FF` | `00 59 00 01 00 00 01 FE` | Retorna status `01`, sem resposta `0x13` nem versão. Função de consulta de firmware não confirmada. |
| `0x17` | `00 59 17 00 00 00 FF` | `00 59 00 01 00 00 01 FE` | Retorna status `01`, sem resposta `0x17` nem tipo de dispositivo. |
| `0x1B` | `00 59 1B 00 00 00 FF` | `00 59 00 01 00 00 01 FE` | Retorna status `01`, sem resposta `0x1B` nem endereço de áudio. |
| `0x20` | `00 59 20 00 00 00 FF` | `00 59 00 01 00 00 01 FE` | Retorna status `01`, sem resposta `0x20` nem valor de codec. |

Para as quatro últimas consultas, o SysEx recebido foi `F0 00 32 01 08 00 00 40 00 7E 01 F7`. Não houve timeout: houve uma resposta explícita de status. A interpretação provável é recusa/comando não suportado nesse formato. Não é possível distinguir, apenas por esse byte, comando inexistente, payload inadequado ou outra condição; portanto não declarar suporte funcional nem rejeição universal em todo firmware.

### Integridade dos presets

O banco de 48 bytes (`0x23`, memória `05`, endereço zero) foi lido antes e depois e comparado byte a byte: **idêntico**. Conteúdo em ambos os momentos, separado por slot:

```text
A: 02 04 07 00 0E 7F 12 0A 08 03 01 01 01 00 00 00
B: 04 02 05 05 2C 46 17 1E 07 05 01 01 01 00 00 00
C: 01 07 07 05 34 6F 14 0C 05 02 01 01 01 00 00 00
```

Essa comparação cobre os 48 bytes lidos do banco, não uma auditoria de toda a memória ou uma medição de áudio. As portas foram fechadas normalmente ao término.

### Limites do teste

Não foram transmitidos `0x00` vazio (Init de efeito desconhecido), `0x15` (MIDI encapsulado sem mapa Cube Baby), comandos MIDI PC/CC adivinhados, escritas `0x22`, controles DSP `0x24`, apagamentos `0x21`, reset ou atualização de firmware. Também não foi realizada varredura de opcodes/endereço nem uma sequência coordenada de acionamento físico dos footswitches.

Assim, foram testadas todas as consultas adicionais declaradas no Android e identificadas neste documento como candidatas. Os demais comandos não confirmados continuam pendentes; o teste não os transforma em comandos suportados.

## 11. Captura de atualização de firmware em 03/10/2026

Atualização executada pelo usuário no SmartLink Flash Loader V1.0.9, com reinicialização automática habilitada. O usuário informou que entrou no bootloader ligando o pedal conectado ao PC com o terceiro footswitch pressionado. Após a gravação, o atualizador indicou verificação e conclusão, e o usuário confirmou o retorno ao modo preset. Nenhum comando de gravação foi enviado pelo CubeControl ou pela análise.

### Evidência preservada

- Captura: [flash-update-20261003-130744.pcap](artefatos/flash-update-20261003-130744.pcap), 575.537 bytes, 1.106 registros completos, sem registro final truncado.
- SHA-256 da captura: `61e47f6aa59bdf871abd37bacdeb402a27f621feb4fbc6be53828e9232a4dffa`.
- Firmware efetivamente transmitido: `cubebaby_B_20220706.up`, 504.600 bytes, SHA-256 `9184852bdebfa1d939ad21a250a579104ec1e3de17f072edbd8311015daad216`. Comparação integral do payload capturado com o arquivo local, não apenas do nome.
- Executável observado: SHA-256 `2ab8a2ad82e0dbc2e88c6bcef56176988676842b9a132230cc56833823ee00bf`.
- Bootloader: `USB\VID_301A&PID_2800\20121120222017`, disco `SMTLINK DEVICE`. Endereço USB 22 no barramento capturado; esse endereço é temporário e não deve ser fixado no app.
- Após reiniciar: `VID_301A&PID_5555`, interfaces normais de áudio/MIDI. A captura estava filtrada no endereço do bootloader; a nova enumeração foi confirmada separadamente pelo Windows.

### Transporte e envelope

Os comandos observados usam USB Mass Storage Bulk-Only, com OUT `0x01` e IN `0x81`, e não SysEx. O CBW de 31 bytes começa por `55 53 42 43` (`USBC`), seguido de tag de 4 bytes, tamanho da transferência em UInt32 little-endian, flags (`00` saída ou `80` entrada), LUN `00`, comprimento do CDB `10` e CDB de 16 bytes. As tags variam e são repetidas pelo CSW; não são constantes do protocolo.

O CSW de 13 bytes começa por `55 53 42 53` (`USBS`), seguido da tag, resíduo UInt32 little-endian e status. Foram observadas 29 transações proprietárias: 28 terminaram com tag correspondente, resíduo zero e status zero. A última, associada ao reset, não retornou CSW e foi seguida por erros USB e remoção. Isso descreve esta captura, não autoriza tratar qualquer desconexão como sucesso.

### Sequência proprietária observada

Nos comandos abaixo, `N` e `A` são UInt32 little-endian. Para os comandos de bloco, o CDB tem `C0 opcode N[4] A[4] 00 00 00 00 00 00`.

| Ordem/operação | CDB / parâmetros | Dados e resposta observados |
|---|---|---|
| Preparação | `C0 81 09 00 00 00 00 00 00 00 00 00 00 00 00 00` | Sem dados; CSW sucesso. Papel interno não determinado. |
| Consulta exibida como FLASH ID | `C0 00 04 00 00 00 00 00 00 00 00 00 00 00 00 00` | IN de 4 bytes: `85 12 85 12`; corresponde a `12851285` no log. |
| Preparação/apagamento provável de bloco | Opcode `05`, `N=0`, `A=endereço` | Sem dados; CSW sucesso. Granularidade real do apagamento não comprovada. |
| Escrita de bloco | Opcode `02`, `N=tamanho`, `A=endereço` | OUT de exatamente N bytes; CSW sucesso. |
| Verificação de bloco | Opcode `82`, `N=tamanho`, `A=endereço` | IN de **2 bytes**, embora N no CDB seja o tamanho do bloco; CRC little-endian e CSW sucesso. |
| Preparação final | Mesmo `C0 81 09 ...` inicial | Sem dados; CSW sucesso. |
| Carga auxiliar de reset | Opcode `09`, `N=0x3000`, `A=0x00820000` | OUT de 12.288 bytes; CSW sucesso. Destino RAM é interpretação, não prova da topologia da memória. |
| Execução/reset provável | `C0 0A 00 00 00 00 01 00 82 00 00 00 00 00 00 00` | Sem dados; remoção do dispositivo sem CSW. Campo em bytes 6–9: `0x00820001`. |

Para cada endereço, o atualizador executou `05 → 02 → 82` antes do próximo bloco. A ordem foi `0x10000`, `0x20000`, `0x30000`, `0x40000`, `0x50000`, `0x60000`, `0x70000` e **`0x00000` por último**. Não houve leitura integral de retorno: a verificação capturada é por CRC.

### Arquivo `.up` e CRC comprovados nesta amostra

O arquivo B tem cabeçalho de 256 bytes e payload de 504.344 bytes (`0x7B218`). A concatenação dos blocos capturados, ordenados pelo endereço, é exatamente `arquivo[256:]`, sem transformação. SHA-256 do payload: `16870a3981858bd3f74fe326363a4961c3cd77944d3a5554b3988c7c396576db`.

No cabeçalho, bytes 0–5 são `CONFIG`; offset `0x10` contém o tamanho do payload em UInt32 little-endian; offset `0x14` contém `73 C8 00 00`; offsets `0xFE–0xFF` contêm `55 AA`. O CRC do payload inteiro é `0xC873`, igual ao valor armazenado em `0x14`. Demais campos não foram interpretados nesta análise; não se deve aceitar qualquer `.up` apenas pela extensão.

O CRC reproduzido nos oito blocos usa polinômio `0x1021`, inicialização `0xFFFF`, sem reflexão e XOR final zero (CRC-16/CCITT-FALSE; reproduzível por `binascii.crc_hqx(dados, 0xFFFF)`). A resposta do pedal transporta esse valor em little-endian.

| Endereço | Bytes | CRC confirmado |
|---|---:|---|
| `0x10000` | 65.536 | `0x176B` |
| `0x20000` | 65.536 | `0xB1E1` |
| `0x30000` | 65.536 | `0x53FF` |
| `0x40000` | 65.536 | `0xA8D3` |
| `0x50000` | 65.536 | `0x1D0F` |
| `0x60000` | 65.536 | `0x164A` |
| `0x70000` | 45.592 | `0xAE54` |
| `0x00000` | 65.536 | `0x4BF8` |

### Reset e limites para implementação

A carga auxiliar tem SHA-256 `fc283220367a9aec33064864b9997a2c449d2e986f5401409c4db357b8381606`. Seus primeiros 12.032 bytes coincidem integralmente com `flash_loader_files/reset.bin` a partir do offset 256. Os 256 bytes finais enviados não pertencem a esse arquivo: começam por `73 F5 DC 83 3F B1 12 00 D0 F4 BA 08 40 84 D1 04`, seguidos de 240 zeros. A origem e a necessidade desses bytes permanecem desconhecidas; não os transformar em constantes nem presumir que sejam preenchimento válido sem investigar o carregador.

A captura começou com o bootloader já conectado: não cobre sua enumeração inicial nem eventuais consultas anteriores ao início da captura. Há comandos SCSI de manutenção do Windows além dos 29 comandos proprietários. A análise estática abaixo esclarece o acesso Windows, a preparação e a origem dos bytes extras do reset. Permanecem sem validação física o reset com buffer corrigido, o tratamento de falhas, a compatibilidade das variantes A/C e a preservação de presets/IRs após atualização. A evidência não equivale a validação de um atualizador próprio no hardware.

### Complemento: análise estática do carregador em 03/10/2026

Nova captura de conexão: [flash-initial-20261003-132459.pcap](artefatos/flash-initial-20261003-132459.pcap), 57.959 bytes, 1.096 registros completos, SHA-256 `59f382e72c25b4038cdecea8d00ce8205eebd2876fdc81688c2f70a1b980e3c7`. O pedal enumerou no endereço USB 24 como `301A:2800`. Foram observados apenas comandos SCSI padrão (`00`, `03`, `12`, `1A`, `23`, `25`), sem comandos proprietários `C0`, escrita ou apagamento. Assim, essa reconexão com o carregador já aberto não confirma a execução da preparação identificada estaticamente. As 89 respostas de sense observadas foram `70 00 02 00 00 00 00 0C 00 00 00 00 3A 00 00 00 00 00`; não confundir falhas das consultas de mídia com falha de atualização, que não foi iniciada.

Reabertura do carregador com o pedal já em flash: [flash-loader-open-20261003-132658.pcap](artefatos/flash-loader-open-20261003-132658.pcap), 57.204 bytes, 1.068 registros completos, SHA-256 `1ef35f4423cd62268761dd09fe18ccbed1dd4a7c48bd9d38cabb3424e1d7a136`. O processo do carregador mudou para PID 4216. Novamente nenhum CDB proprietário foi transmitido: apenas 102 `TEST UNIT READY`, 102 `REQUEST SENSE` e sete `MODE SENSE(6)`. Portanto, abrir/reabrir o programa não demonstrou uma preparação proprietária obrigatória anterior ao início da gravação. A sequência `C0 81 09 ...` continua comprovada fisicamente no início da atualização anterior e identificada estaticamente; não afirmar que ela ocorre automaticamente ao abrir o aplicativo.

Executável com o mesmo SHA-256 acima, inspecionado por `dumpbin /disasm` do Visual Studio, sem executá-lo sobre o pedal. Endereços abaixo são VAs dessa compilação x86, base `0x00400000`.

**Acesso Windows confirmado.** A rotina `0x00415050` abre um caminho de volume `\\.\X:` com `CreateFileW`, acesso `GENERIC_READ | GENERIC_WRITE`, compartilhamento leitura/escrita e `OPEN_EXISTING` (`0x00415107–0x00415117`). Usa `IOCTL_STORAGE_QUERY_PROPERTY=0x002D1400` (`0x00415162`) para consultar o descritor e exige barramento USB (`BusType=7`). O carregador enumera letras de unidades; a string de formato está em `0x004A55A0` e as identificações `SMTLINK`/`DEVICE` em `0x004A5540`/`0x004A5550`.

As rotinas `0x004153D0` (entrada) e `0x004154A0` (saída) chamam `DeviceIoControl` com **`IOCTL_SCSI_PASS_THROUGH_DIRECT=0x0004D014`** (`0x00415461` e `0x00415531`). O pacote local x86 tem 80 bytes, `Length=44`, `CdbLength=16`, `SenseInfoLength=26`, `SenseInfoOffset=48`, `TimeOutValue=10` segundos e ponteiro direto para os dados. O campo `DataIn` difere entre leitura e escrita; o CDB observado na captura é copiado sem envelope USB montado pelo aplicativo. Windows/USBSTOR gera CBW/CSW.

O wrapper original verifica apenas o retorno booleano de `DeviceIoControl`. Uma implementação própria deve verificar também `ScsiStatus`, sense e quantidade transferida, além do CRC. Não copiar o layout x86 para um processo x64: tamanho/alinhamento de ponteiros muda. O buffer de dados deve respeitar `AlignmentMask` do adaptador, conforme a [documentação Microsoft](https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/ntddscsi/ni-ntddscsi-ioctl_scsi_pass_through_direct). A identificação do destino deve ser vinculada ao PnP `301A:2800`, sem fixar letra, número de disco ou endereço USB. Abertura/gravação por implementação própria e requisitos de elevação ainda precisam de validação local. USBPcap serve à pesquisa e não é dependência do transporte do app.

**Preparação identificada.** A rotina de conexão `0x0040BDDF–0x0040BE4A` abre/valida o dispositivo e envia `C0 81` com cinco bytes de configuração. `0x004149FF–0x00414A69` carrega esses valores do INI. O byte 2 do CDB é `clkDiv`, com fallback `09` quando zero; os bytes 3–6 vêm das quatro configurações seguintes do INI. Com a configuração observada, o resultado é `C0 81 09 00 00 00 00 ...`. A operação é repetida no início da gravação (`0x0040A33C–0x0040A37B`) e antes do reset (`0x0040A935–0x0040A95F`). Isso identifica a origem dos parâmetros, mas não o efeito elétrico interno de cada opção. A enumeração inicial precisa de captura própria para confirmar todo o fluxo desde a conexão, sem inferir ausência de outras operações pelo trecho analisado.

**Bytes extras do reset explicados: leitura além do buffer.** Em `0x0040A8EA`, o carregador obtém o tamanho integral de `reset.bin` (12.288) e o guarda em `[esp+0x10]`. Em `0x0040A8F3`, calcula tamanho menos `0x100`; aloca 12.032 bytes (`0x0040A8FA`), posiciona o arquivo em `0x100` (`0x0040A914`) e lê 12.032 bytes (`0x0040A924`). Entretanto, em `0x0040A964–0x0040A971`, passa o tamanho **integral**, 12.288, à rotina de envio `0x00402630`, que o utiliza tanto no CDB quanto em `DataTransferLength`. Portanto, envia 256 bytes além do buffer alocado. Isso explica os bytes estranhos da captura como memória adjacente do processo, não como conteúdo válido de `reset.bin`.

O código chama o comando `09` para carregar o auxiliar em `0x00820000` e, se a transferência retornar sucesso, `0A` com `0x00820001` (`0x00402690`). A implementação própria deve usar um buffer de tamanho consistente e nunca reproduzir os bytes vazados. O envio apenas dos 12.032 bytes reais é a correção derivada do código, validada no ensaio isolado abaixo.

### Validação física do reset corrigido em 03/10/2026

Teste solicitado explicitamente pelo usuário, com o pedal em flash. Nenhum comando de apagamento ou gravação do firmware foi enviado. Um processo Python x64 elevado usou `CreateFileW` e `IOCTL_SCSI_PASS_THROUGH_DIRECT` em `\\.\PHYSICALDRIVE3`, após confirmar o vínculo PnP com `301A:2800`, descritor USB e identificação `SMTLINK`/`DEVICE`. O adaptador retornou `AlignmentMask=0`; estrutura SPTD x64 de 56 bytes e buffer alocado por `VirtualAlloc`. O arquivo `reset.bin` foi conferido pelo SHA-256 antes do envio.

Sequência efetivamente capturada:

1. `C0 81 09 00 00 00 00 00 00 00 00 00 00 00 00 00`: preparação aceita, CSW sucesso.
2. `C0 09 00 2F 00 00 00 00 82 00 00 00 00 00 00 00`: **12.032 bytes**, exatamente `reset.bin[256:]`, SHA-256 `f1be00b63550945c81a9624bdda3209d65c7bd09959aea3924d3870c45eda0e2`. Tanto a API Windows quanto o CBW e o payload USB confirmaram esse tamanho; CSW sucesso, resíduo zero.
3. `C0 0A 00 00 00 00 01 00 82 00 00 00 00 00 00 00`: comando final sem dados. A chamada Windows retornou erro 121, sem CSW; em seguida o dispositivo flash desapareceu e o Windows enumerou `301A:5555`, com interfaces `MI_00` e `MI_03` em status OK.

**Resultado:** reset corrigido validado nesta unidade, pelo retorno ao modo USB normal, sem os 256 bytes fora do buffer. O erro 121 isolado não deve ser classificado como sucesso: a confirmação é a remoção do bootloader seguida da enumeração do mesmo pedal em modo normal. Não houve ensaio auditivo, comparação de presets/IRs nem nova atualização completa pelo transporte próprio.

Evidências: [registro das chamadas](artefatos/reset-corrected-20261003.json) e [captura USB](artefatos/reset-corrected-20261003.pcap), 70.572 bytes, 1.096 registros completos, SHA-256 `19a0d9092517bd3c3eb72cf24f8d5eab7d7176b28caf577f8ba64e5806614a9e`. A captura incluiu a nova enumeração USB; tráfego posterior à remoção não deve ser associado à resposta do comando `0A` apenas pela proximidade temporal.

## 12. Catálogo dos firmwares incluídos no app

Reavaliação em **03/10/2026**, diretamente sobre [Assets/Firmwares/CubeBaby](../../CubeControl-1.1/Cube/windows/Cube.Desktop/Assets/Firmwares/CubeBaby). Esta seção descreve os arquivos atuais dessa pasta; os nomes históricos das capturas anteriores não devem ser usados como identificadores de conteúdo.

### Datas e integridade

As datas abaixo foram extraídas **dos nomes**, não de metadados de compilação ou de notas oficiais de lançamento. As datas de modificação locais são de 03/10/2026 e não comprovam a idade do firmware. Todos os arquivos contêm a string ASCII `VER-20191115`; por ser comum a todas as imagens e não ter origem funcional identificada, ela não comprova a versão/data do firmware completo.

| Arquivo atual | Data indicada no nome | Tamanho | CRC-16 do payload | Observação comprovada |
|---|---|---:|---|---|
| `cubebaby_20210830.up` | 30/08/2021 | 504.600 bytes | `6557` | Conteúdo distinto dos demais. |
| `cubebaby_20220111.up` | 11/01/2022 | 504.600 bytes | `C873` | Idêntico ao arquivo denominado B na atualização física registrada na seção 11. |
| `cubebaby_A_20220706.up` | 06/07/2022 | 504.600 bytes | `97D1` | Conteúdo distinto dos demais. |
| `cubebaby_B_20220706.up` | 06/07/2022 | 504.600 bytes | `ACDE` | Conteúdo distinto dos demais; diferente do B usado na captura histórica. |

Todos os quatro passaram novamente pela conferência do cabeçalho `CONFIG`, tamanho declarado, marcador final `55 AA` do cabeçalho e CRC-16/CCITT-FALSE do payload. Cada arquivo contém cabeçalho de 256 bytes e payload de 504.344 bytes. São **quatro arquivos e quatro imagens únicas, sem duplicatas**. O usuário removeu `cubebaby_20220525old.up`, anteriormente confirmado como idêntico ao B atual. O arquivo C/PDF encontrado na análise anterior também não está nesta pasta. Integridade estrutural não equivale a compatibilidade de hardware.

### Identificação inequívoca por SHA-256

| Arquivo | SHA-256 do arquivo completo |
|---|---|
| `cubebaby_20210830.up` | `1b93f8550923a197db0538f1a7ca086c0bd2f6c7f1c2212512a4d26f92544fe8` |
| `cubebaby_20220111.up` | `9184852bdebfa1d939ad21a250a579104ec1e3de17f072edbd8311015daad216` |
| `cubebaby_A_20220706.up` | `b90e9eaf10c8036e4847e22449e122718761fa0bce63f06804da2ab9feaeaa0a` |
| `cubebaby_B_20220706.up` | `d99a9bc4ba0636118801b1e3a8d11adadf03831d316c7477af8de015ba5bd743` |

A correspondência entre `20220111` e o B histórico foi confirmada por comparação integral com `engenharia-reversa/cube-baby/cubebaby_B_20220706.up`, além do hash. Não se alteraram nomes, seleção padrão do app ou arquivos binários nesta reavaliação.

### Diferenças binárias

Comparação byte a byte em posições iguais do **payload**, excluindo os 256 bytes do cabeçalho. Os números não representam quantidade de funções corrigidas ou novas: deslocamentos de código e dados também produzem diferenças.

| Par de conteúdos | Posições diferentes no payload |
|---|---:|
| `20210830` × `20220111` | 31.238 |
| `20210830` × B atual | 61.475 |
| `20210830` × A atual | 77.205 |
| `20220111` × B atual | 56.465 |
| `20220111` × A atual | 75.495 |
| B atual × A atual | 61.494 |

Todas as diferenças observadas entre imagens distintas ficam entre offsets de payload `0x00007C` e `0x04F14B`. Os três blocos finais, a partir de `0x050000` até o fim do payload, são idênticos em todos os quatro arquivos. Isso não prova preservação de presets/IRs numa atualização: a operação inclui apagamento/gravação e esses endereços não foram classificados funcionalmente nesta comparação.

Todos contêm identificações `CubeBaby` e `SINCO-CubeBaby`, além de referências comuns ao atualizador. A inspeção não encontrou uma data interna distinta que confirme as datas dos nomes. **Não há evidência suficiente nesta comparação para atribuir às versões mudanças de timbre, efeitos, correções, melhorias ou variantes físicas específicas.** Essas diferenças funcionais exigem análise de código/dados ou ensaios controlados; nomes A/B e cronologia aparente, isoladamente, não as demonstram. Nenhum firmware foi enviado ao pedal durante esta reavaliação.

### Investigação de cronologia e origem dos pacotes — 03/10/2026

Foram inspecionados os ZIPs locais em `C:\Users\lzzpm\Downloads`, incluindo arquivos ZIP internos, sem extrair ou modificar os firmwares. Os hashes dos membros foram comparados com os arquivos atuais do app. Datas de entradas ZIP são metadados editáveis, sem fuso horário registrado; corroboram o histórico do pacote, mas não certificam data de compilação. A procedência desses ZIPs não foi autenticada junto ao fabricante nesta investigação.

| Arquivo atual do app | Correspondência integral nos pacotes locais | Evidência temporal |
|---|---|---|
| `cubebaby_20210830.up` | Firmware homônimo dentro de `2. CubeSuite V2.3.3 and Firmware 8.30.21.zip` | Entrada ZIP: 30/08/2021 18:40:34. Nome do pacote coerente com agosto de 2021. |
| `cubebaby_20220111.up` | Firmware homônimo dentro de `3. CubeSuite and Firmware 01.11.2022.zip`; também `cubebaby_B_20220706.up` no pacote de julho | Entrada do pacote de janeiro: 07/01/2022 16:59:52. A imagem foi reutilizada no pacote de julho, sem alteração de bytes. |
| `cubebaby_B_20220706.up` | `cubebaby_20220525old.up` dentro de `7. Firmware 05.25.2022.zip`; também `cubebaby_A_20220706.up` no pacote de julho | Entrada no pacote de maio: 01/07/2022 14:35:44; diverge da data do nome. Não atribuir compilação em maio ou julho apenas com isso. |
| `cubebaby_A_20220706.up` | `cubebaby_C_20220706.up` dentro de `10. Firmwares 7.6.22 (A, B and C).zip` | Não encontrada nos pacotes anteriores examinados. Todas as entradas desse ZIP têm 01/01/1980 00:00:00, data padrão sem utilidade cronológica. |

O PDF `C:\Users\lzzpm\Downloads\10. Firmwares 7.6.22 (A, B and C)\CUBE_BABY.pdf`, incluído no pacote de três variantes, informa que a descontinuação de um chip levou a diferentes hardwares Cube Baby, exigindo firmwares correspondentes. Isso fornece evidência documental de **variantes de compatibilidade**, não de uma sequência A → B → C de melhorias. O documento não identifica chips, números de série ou uma regra que associe cada letra a uma placa específica. Seu texto recomenda experimentar variantes após uma atualização equivocada; essa orientação do documento não foi executada nem adotada como método de identificação nesta análise.

A busca interna por datas ASCII compactas/separadas, datas de compilação no formato textual e strings de versão/build encontrou somente `VER-20191115`, no mesmo offset de payload (`0x754` para o início de `VER`) em todas as imagens. Não foi localizado identificador distinto de compilação nessas strings. Cabeçalhos iguais salvo CRC e diferenças de código não fornecem, sozinhos, uma ordem cronológica confiável.

**Conclusão:** o pacote de julho de 2022 é o mais recente entre os pacotes locais examinados, segundo sua identificação. Ele reúne variantes, incluindo duas imagens já presentes em pacotes anteriores. O **A atual do app (C no ZIP original)** é o único conteúdo dos três que não foi encontrado nos pacotes anteriores examinados; é candidato a ter sido introduzido nessa distribuição, mas isso **não comprova que foi compilado por último nem que seja adequado à unidade do usuário**. Não há base para rotular um arquivo como “firmware mais recente” de forma absoluta. O conteúdo fisicamente validado na captura desta sessão continua sendo `9184852b...`, atualmente denominado `cubebaby_20220111.up`, independentemente das letras usadas em cada pasta.

A consulta web localizou o [portal de downloads M-VAVE](https://www.cuvave.com/download), mas a leitura direta dessa página e de `/fcp` falhou por timeout; não se obteve pacote remoto ou changelog autenticado para ampliar a cronologia. Nenhum resultado de fórum foi usado como prova de versão. Nesta investigação, somente esta documentação foi atualizada; nomes dos arquivos, seleção padrão e código do aplicativo foram preservados.

## 13. Investigação de entrada em modo flash via USB

Investigação estática em 03/10/2026, sem enviar comandos ao pedal. **Não foi identificado um procedimento confirmado para passar de `301A:5555` (normal) para `301A:2800` (flash) por USB.** Isso não demonstra impossibilidade absoluta; significa que não há sequência validada para substituir o terceiro footswitch durante a inicialização.

### Evidência nos quatro firmwares

Desmontagem Thumb com Capstone do analisador que verifica o envelope `00 59`, comprimento de payload e checksum. Offsets abaixo são relativos ao payload do `.up` (offset no arquivo = valor + `0x100`), não endereços para escrita no pedal.

| Arquivo atual | Início do analisador | Comandos explicitamente despachados |
|---|---|---|
| `cubebaby_20210830.up` | `0x4D4CC` | `11`, `12`, `21`, `22`, `23` |
| `cubebaby_20220111.up` | `0x4D4CC` | `11`, `12`, `21`, `22`, `23` |
| `cubebaby_A_20220706.up` | `0x49F00` | `11`, `12`, `21`, `22`, `23` |
| `cubebaby_B_20220706.up` | `0x49CB8` | `11`, `12`, `21`, `22`, `23` |

No `20220111`, após o caso `23`, o caminho padrão em `0x4D6CA` prepara uma resposta de oito bytes, em vez de chamar um reset. Os casos são identificação (`11`/`12`) e operações de memória (`21`/`22`/`23`). O ramo de escrita `22`, seletor `04`, soma uma base ao endereço e chama cópia de memória; isso é uma primitiva genérica, **não um comando de bootloader**. Endereços arbitrários poderiam atingir estado, ponteiros ou periféricos, mas nenhum endereço/valor foi estabelecido como transição correta para flash.

A análise cobre esse handler localizado, não todo o caminho USB nem rotinas em ROM interna do chip, que não estão necessariamente contidas no `.up`. Por isso, não extrapolar sua lista de casos para afirmar que todo comando fora dela é impossível; por exemplo, há evidência histórica de `24` em outro fluxo de IR. Não foram testados opcodes desconhecidos, sobrescritas de ponteiros, flags de boot ou registradores de reset.

Uma rotina com padrão de reset Cortex-M foi localizada no payload `0x44F0` do `20220111`: barreira, escrita no bloco SCB com literal `0x05FA0004` e laço de espera. Sua presença não prova entrada em flash: não foi estabelecida uma chamada alcançável por USB nem a condição que selecionaria o bootloader após reiniciar. As strings `sl_update_root.bin`/`sl_update.upd2` também existem, sem um fluxo USB normal → flash demonstrado.

### Descritores USB e ferramentas

A captura preservada `reset-corrected-20261003.pcap` contém as configurações dos dois modos. No modo normal, as interfaces declaradas são classe `01`, subclasses `01` (controle de áudio), `02` (streaming de áudio) e `03` (MIDI). Não há interface DFU declarada nessa configuração. Em flash, há classe `08`, subclasse `06`, protocolo `50` (Mass Storage/SCSI/Bulk-Only). Portanto, não foi encontrado um caminho padrão DFU de runtime nessa unidade; isso não exclui pedidos vendor-specific não mapeados.

O `flash_loader` analisado abre o dispositivo de armazenamento já enumerado. Seus comandos `C0 09`/`C0 0A` e o `reset.bin` validado dependem desse modo e fazem o caminho **flash → normal**. Eles não são uma sequência utilizável diretamente pela interface USB-MIDI normal. Reconectar a interface USB no Windows tampouco foi comprovado como equivalente à sequência física de entrada no bootloader.

As fontes específicas consultadas — [pesquisa de Pedro Ferreira](https://garden.turbofish.cc/notes/Reverse-engineering-the-Cuvave-Cube-Baby), [parser cuvave-midi](https://github.com/pferreir/cuvave-midi) e `Communication.java` local — não forneceram uma sequência normal → flash. Comandos OTA encontrados em pesquisas de outros produtos M-VAVE não foram tratados como compatíveis com a Cube Baby.

**Conclusão para o aplicativo:** manter a instrução física atualmente validada. Um modo automático ainda exigiria mapear a seleção de boot e um caminho alcançável pelo USB normal, possivelmente envolvendo ROM interna, e depois validar a transição isoladamente no hardware. Não foi alterado código do app nem a função de atualização nesta investigação.
