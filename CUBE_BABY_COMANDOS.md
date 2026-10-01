# Cube Baby — comandos enviados e recebidos

Pesquisa realizada em 26/09/2026. Modelo: CUVAVE/M-VAVE **Cube Baby de guitarra**. Não extrapolar para Cube Baby Bass, AC, CubeTurner, Chocolate ou Blackbox.

Este documento reúne todos os opcodes encontrados nas fontes e capturas consultadas, os parâmetros acessíveis por memória e suas respostas. **Não é uma especificação oficial exaustiva do firmware**: comandos não observados, campos desconhecidos e diferenças de implementação estão identificados. A pesquisa inicial foi est?tica; depois foram realizadas as consultas f?sicas descritas na se??o 10, sem comandos de escrita ou apagamento.

## 1. Fontes e grau de evidência

- **Capturado**: mensagem encontrada nos arquivos públicos de tráfego CubeSuite ↔ Cube Baby; comprova o formato naquela sessão, não em todos os firmwares.
- **Implementado/documentado**: consta em código específico da Cube Baby, mas nem toda afirmação possui captura pública correspondente.
- **Genérico/não confirmado**: encontrado no Android compartilhado com outros produtos, ou interpretação ainda sem comprovação suficiente.

Fontes locais:

- [Communication.java](sources/com/cubeSuite/communication/Communication.java): construção de pacotes, consultas, seletores de memória e checksum.
- [CommunicationUtil.java](sources/com/cubeSuite/communication/CommunicationUtil.java): envio BLE e interpretação das respostas.
- [CubeBaby.java](sources/com/cubeSuite/entity/Instrument/CubeBaby.java): classe vazia; não fornece mapa específico do pedal.
- [BleEnum.java](sources/com/cubeSuite/config/BleEnum.java): UUIDs do transporte BLE compartilhado.
- [LooperCommunication.java](sources/com/cubeSuite/communication/LooperCommunication.java): evidencia que os CC encapsulados do looper pertencem a outro produto.

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

A lista `BLE_DEVICE_CONFIGS` de [BleHandle.java](sources/com/cubeSuite/bluetooth/BleHandle.java) não inclui Cube Baby. Os UUIDs `AE40`/`AE41`/`AE42` do Android continuam sendo evidência de suporte BLE genérico do CubeSuite, não de suporte deste pedal.

**Conclusão:** o Windows reconhece o Cube Baby pelo Bluetooth clássico, mas não foi encontrado um canal Bluetooth para controlar seus parâmetros. Os serviços observados são de áudio e controle de reprodução. O controle pelo Bluetooth integrado permanece sem suporte comprovado; implementar apenas um transporte Bluetooth no CubeControl não resolve essa lacuna. A comunicação USB-MIDI continua sendo o caminho validado. Não foram enviados comandos de alteração de presets ou firmware durante esta verificação.

## 3. Catálogo dos comandos observados

Host = computador/editor; pedal = Cube Baby.

| Opcode | Direção observada | Payload | Função e evidência |
|---|---|---|---|
| `0x00` | Pedal → host | Um byte de status | ACK de operações de escrita, apagamento e controle IR. Nas capturas, status `00`; nas consultas f?sicas adicionais, status `01`. Ver ressalva abaixo. |
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

A leitura do banco A+B+C usa `M=05`, `A=00000000`, `N=48`. A edição live usa `M=05`, endereços a partir de `80000000`. Portanto, edição live e gravação persistente são operações distintas, apesar de compartilharem opcode e seletor.

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

Escrever um parâmetro live: `0x22`, `M=05`, endereço calculado, `N=1`, valor de um byte. Ler o banco persistente não necessariamente reflete uma alteração live ainda não salva.

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
| `0x13` | `CMD_Q_FIRMWARE_VER` | Consulta genérica de firmware declarada no Android; teste f?sico retornou somente ACK/status `01`, sem vers?o (se??o 10). |
| `0x15` | `CMD_MIDI_WRITE` | Encapsula bytes MIDI; usado pelo looper no Android. Não comprova CC/PC na Cube Baby. |
| `0x17` | `CMD_Q_DEV_TYPE` | Consulta genérica de tipo de dispositivo; suporte Cube Baby não confirmado. |
| `0x1B` | `CMD_Q_SOUND_DATA_ADDR` | Consulta genérica de endereço de dados de áudio; suporte Cube Baby não confirmado. |
| `0x20` | `CMD_Q_CODEC_VAL` | Consulta genérica de valor do codec; significado e suporte Cube Baby não confirmados. |

Para as consultas genéricas, `makeQueryPacket` constrói payload vazio; a mera existência desse construtor ou dessas constantes não demonstra que o firmware responda. O opcode `0x12`, embora também declarado genericamente, foi separado porque aparece de fato nas capturas da Cube Baby.

## 9. O que o pedal pode enviar e o que continua desconhecido

Mensagens comprovadas pedal → host: identificação `0x11`, informações adicionais `0x12`, conteúdos de memória `0x23` e ACK `0x00`. Não foi encontrada, no conjunto analisado, evidência de transmissão espontânea de notas MIDI, CC, Program Change, posição de knobs, preset ativo, estado do afinador ou eventos A/B/C.

Também não foram confirmados comandos remotos de ativação de A/B/C, bypass global, afinador, tap tempo, reset de fábrica, reinicialização ou atualização de firmware. A ausência nessas fontes **não prova impossibilidade**; significa que não há bytes confiáveis para documentar tais funções como suportadas.

O protocolo permite endereçamento genérico, mas isso não autoriza tratar todos os endereços como conhecidos. Permanecem abertos os campos de `0x12`, a estrutura completa de `00002100`, a semântica de todos os status ACK, os metadados de identificação e a extensão real do mapa de memória. Este catálogo cobre o conjunto encontrado nas fontes citadas, com validação estática e de capturas, sem validação física nesta sessão.


## 10. Teste físico no pedal conectado em 26/09/2026

### Procedimento

Comunicação direta pelo WinMM do Windows, sem instalar bibliotecas e sem criar scripts persistentes. Entrada e sa?da selecionadas pelo nome exato `USB-Midi`, ambas únicas. Dispositivo USB enumerado com `VID_301A&PID_5555`, interface `MI_03`. A identifica??o retornou `SINCO-CubeBaby`, com o mesmo payload de 27 bytes apresentado na se??o 3. N?o foi poss?vel extrair uma vers?o de firmware confi?vel desses metadados.

Foi enviado um pedido de cada consulta, com janela de recebimento de 1,5 segundo. Antes dos testes e depois de cada consulta, `0x11` confirmou que o pedal continuava respondendo com identificação válida. Todas as respostas abaixo passaram pela verifica??o de comprimento e checksum. Os bytes desta tabela s?o envelopes decodificados, antes/depois do empacotamento SysEx.

| Consulta | Pedido | Resposta | Conclus?o f?sica |
|---|---|---|---|
| `0x12` | `00 59 12 00 00 00 FF` | `00 59 12 06 00 00 01 00 00 00 00 00 FE` | Consulta respondida; reproduz a captura p?blica. Os seis campos continuam sem significado confirmado. |
| `0x13` | `00 59 13 00 00 00 FF` | `00 59 00 01 00 00 01 FE` | Retorna status `01`, sem resposta `0x13` nem vers?o. Fun??o de consulta de firmware n?o confirmada. |
| `0x17` | `00 59 17 00 00 00 FF` | `00 59 00 01 00 00 01 FE` | Retorna status `01`, sem resposta `0x17` nem tipo de dispositivo. |
| `0x1B` | `00 59 1B 00 00 00 FF` | `00 59 00 01 00 00 01 FE` | Retorna status `01`, sem resposta `0x1B` nem endere?o de ?udio. |
| `0x20` | `00 59 20 00 00 00 FF` | `00 59 00 01 00 00 01 FE` | Retorna status `01`, sem resposta `0x20` nem valor de codec. |

Para as quatro últimas consultas, o SysEx recebido foi `F0 00 32 01 08 00 00 40 00 7E 01 F7`. Não houve timeout: houve uma resposta explícita de status. A interpretação provável ? recusa/comando n?o suportado nesse formato. Não ? poss?vel distinguir, apenas por esse byte, comando inexistente, payload inadequado ou outra condi??o; portanto n?o declarar suporte funcional nem rejeição universal em todo firmware.

### Integridade dos presets

O banco de 48 bytes (`0x23`, mem?ria `05`, endere?o zero) foi lido antes e depois e comparado byte a byte: **idêntico**. Conte?do em ambos os momentos, separado por slot:

```text
A: 02 04 07 00 0E 7F 12 0A 08 03 01 01 01 00 00 00
B: 04 02 05 05 2C 46 17 1E 07 05 01 01 01 00 00 00
C: 01 07 07 05 34 6F 14 0C 05 02 01 01 01 00 00 00
```

Essa compara??o cobre o banco persistente, n?o uma auditoria de toda a mem?ria ou uma medi??o de ?udio. As portas foram fechadas normalmente ao t?rmino.

### Limites do teste

N?o foram transmitidos `0x00` vazio (Init de efeito desconhecido), `0x15` (MIDI encapsulado sem mapa Cube Baby), comandos MIDI PC/CC adivinhados, escritas `0x22`, controles DSP `0x24`, apagamentos `0x21`, reset ou atualiza??o de firmware. Tamb?m n?o foi realizada varredura de opcodes/endere?o nem uma sequ?ncia coordenada de acionamento f?sico dos footswitches.

Assim, foram testadas todas as consultas adicionais declaradas no Android e identificadas neste documento como candidatas. Os demais comandos n?o confirmados continuam pendentes; o teste n?o os transforma em comandos suportados.
