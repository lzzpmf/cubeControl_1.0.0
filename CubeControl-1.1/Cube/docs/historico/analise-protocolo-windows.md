# CubeSuite: protocolo e migração para Windows

[Documentação](../README.md) · **Registro histórico** · Localização das fontes revisada em 01/10/2026

O aplicativo atual já é WPF/USB-MIDI; sua arquitetura está em [Arquitetura](../desenvolvimento/arquitetura.md). As propostas de WinUI/GATT e a sequência de próximas etapas abaixo pertencem à investigação inicial de 16/09/2026, não são instruções de migração ainda pendentes. O acervo desta árvore usa `sources/` e `resources/` dentro de `engenharia-reversa/apk-cubesuite/`, sem projeto Android compilável.

> Atualização de 17/09/2026: pedaleira WPF, envio contínuo, editor de IR e recuperação de fábrica foram implementados. Evidências adicionais e ensaios reversíveis estão em [IR e recursos de fábrica](../referencias/cubecontrol-ir-e-fabrica.md); uso atual em [windows/README.md](../../windows/README.md). As limitações históricas abaixo não descrevem a versão atual.

> Atualização após identificação do hardware: o alvo foi confirmado como **Cube Baby de guitarra via USB**. A implementação está em `windows/`; consultar `windows/README.md` para os testes físicos e limitações. O envelope abaixo é aproveitado no USB após conversão contínua para 7 bits e encapsulamento SysEx. O perfil USB específico foi complementado com referências abertas, pois a classe CubeBaby deste Android está vazia. A análise inicial abaixo descreve principalmente o código BLE disponível.

> Validação prática em **17/09/2026**: o usuário confirmou que todos os controles da interface Windows e o salvamento de presets funcionam no seu Cube Baby por USB. A implementação e seu executável estão disponíveis em `windows/`, com instruções em `windows/README.md`.

Registro histórico da análise estática inicial de 16/09/2026: naquele momento, nenhum comando havia sido enviado ao hardware e o modelo alvo ainda não havia sido informado. As seções abaixo preservam essa investigação inicial; os exemplos de outras famílias não são universais.

## Conclusão

É viável reconstruir o controle no Windows. A comunicação principal examinada usa BLE GATT proprietário, não uma porta MIDI convencional. Parte dos comandos encapsula MIDI; a configuração usa leitura/escrita de memória por endereço. Ter USB MIDI não prova que o dispositivo aceite esse protocolo de configuração por USB.

O acervo de referência é Android (`com.cubeSuite`), com sinais explícitos de descompilação JADX, dependências incorporadas, métodos sintéticos e trechos inválidos. O comentário Gradle `dependencies { // TODO: dependencies }` foi registrado na árvore histórica; não há esse projeto Gradle incluído neste checkout. O acervo serve como referência de protocolo, não como base diretamente compilável para Windows.

## Mapa do código

Todos os caminhos da tabela abaixo são relativos a `engenharia-reversa/apk-cubesuite/sources/com/cubeSuite/`, a partir da raiz deste repositório. `app/src/main/java/com/cubeSuite/` era o caminho da árvore histórica. Exemplo atual: [Communication.java](../../../../engenharia-reversa/apk-cubesuite/sources/com/cubeSuite/communication/Communication.java).

| Arquivo | Papel |
|---|---|
| `communication/Communication.java` | Cabeçalho, comandos, checksum, endereços e tamanhos |
| `communication/CommunicationUtil.java` | Envio BLE, blocos, notificações, espera e interpretação das respostas |
| `config/BleEnum.java` | UUIDs e MTU solicitado |
| `bluetooth/BleHandle.java` | Nomes reconhecidos e seleção da tela por modelo |
| `communication/LooperCommunication.java` | CCs dos loopers |
| `communication/MidikeyboardCommunication.java` | Escritas globais, presets, cores, SysEx e salvamento |
| `entity/midikeyboard/MidikeyboardEntry.java` | Layout binário dos teclados dessa família |
| `communication/FootCtrlCommunication.java` | Configuração dos pedais, com parte dependente de JNI |
| `communication/SmcCommunication.java` | Configuração SMC Mixer |
| `entity/fc2/MidiCodeStruct.java` | Campos enable, channel, type, data1 e data2 |

Há perfis separados para SMK25, SMK25Mini, SMK25II, SMC Mixer, SMC Pad, FootCtrl, FC2, loopers e outros. Não misturar seus mapas de endereços.

## Transporte BLE

| Uso | UUID |
|---|---|
| Serviço | `0000ae40-0000-1000-8000-00805f9b34fb` |
| Escrita | `0000ae41-0000-1000-8000-00805f9b34fb` |
| Notificações | `0000ae42-0000-1000-8000-00805f9b34fb` |

O app solicita MTU 203 em `activitys/ControlAndPdfActivity.java`; `MainActivity.java` configura divisão BLE de 200 bytes. Separadamente, `CommunicationUtil` divide escritas em blocos de **173 bytes de dados**, adiciona 15 bytes de protocolo e espera resposta antes de continuar. Leituras são divididas em até **1000 bytes**. O tempo de espera implementado usa 3000 iterações com sleep de 1 ms, aproximadamente 3 segundos, não um prazo preciso.

No Windows, descobrir as características e suas propriedades, assinar notificações antes de enviar comandos, respeitar o tamanho efetivo de transporte e reconstruir respostas fragmentadas. Sucesso de escrita GATT e confirmação do protocolo são eventos distintos.

## Formato binário extraído

```text
00 59 CMD LEN24_LE PAYLOAD CHECKSUM
```

`LEN24_LE` é o tamanho do payload, excluindo cabeçalho e checksum. Inteiros multibyte são little-endian. O checksum é `(~sum(payload)) & 0xFF`; não inclui os seis bytes iniciais. Payload vazio produz `FF`.

| CMD | Significado no código | Payload |
|---|---|---|
| `11` | Consultar nome/versão | vazio |
| `12` | Short message / other info (nomes duplicados) | consulta genérica vazia; sem semântica validada |
| `13` | Versão de firmware | consulta genérica vazia |
| `15` | Encapsular MIDI | bytes MIDI |
| `17` | Tipo do dispositivo | consulta genérica vazia |
| `1B` | Endereço de dados de som | consulta genérica vazia |
| `20` | Valor codec | consulta genérica vazia |
| `21` | Apagar bloco | região + endereço U32 |
| `22` | Escrever | região + endereço U32 + tamanho U24 + dados |
| `23` | Ler | região + endereço U32 + tamanho U24 |

Valores na tabela em hexadecimal. As consultas além de `11` foram identificadas por constantes; isso não demonstra suporte em todo firmware.

Regiões declaradas: 0 NOR, 1 NAND, 2 SD, 3 EFF, 4 DEV, 5 USR, 6 RSV1, 7 RSV2. Os helpers `*ToRAM` usam região 6, embora a constante a chame RSV1. A interpretação depende do modelo.

Exemplos derivados dos construtores, ainda sem validação física:

```text
Consultar nome/versão:
00 59 11 00 00 00 FF

Ler 13 bytes da região 4, endereço 0 (leitura básica FootCtrl):
00 59 23 08 00 00 04 00 00 00 00 0D 00 00 EE

Encapsular CC7=100 no canal MIDI 1 (volume dos loopers examinados):
00 59 15 03 00 00 B0 07 64 E4
```

Esses envelopes completos vão à característica BLE proprietária. Não enviá-los como se fossem uma mensagem MIDI ou SysEx de uma porta MIDI.

## MIDI já localizado

`LooperCommunication.java` usa `B0 CC valor`, encapsulado no comando `15`:

| Função do método | Pro / Auto | Mini |
|---|---|---|
| Volume | CC7 | CC7 |
| Gate | CC31 | CC31 |
| On | CC20 | não definido nessa classe |
| Next | CC21 | não definido nessa classe |
| Stop | CC22 | CC21 |
| Sync | CC23 | CC22 |

Os nomes refletem os métodos. Valores de acionamento e limites de UI precisam ser conferidos nas telas do modelo escolhido; não presumir 127 para todo botão.

Nos teclados, `sendSysEx` grava comprimento + conteúdo SysEx na configuração USR. Isso configura a mensagem do controlador e não significa executar imediatamente o SysEx no equipamento conectado.

## Configurações recuperáveis

Exemplo da família `MidikeyboardEntry`: global de 19 bytes; oito presets de 474 bytes, total 3792 bytes. O global usa região 4 e os presets região 5 nos helpers da família. Global: preset no offset 0, curvas nos offsets 1/2, parâmetros de arpejador entre 8 e 14, tempo U16 no offset 16 e inputMode no 18. O offset 15 não recebe campo nesse parser.

Cada preset começa em `índice * 474`: canal em +0, oitava I8 em +1, transposição I8 em +2, mínimo/máximo em +3/+4. Os demais parâmetros são mapeados no mesmo arquivo. Não aplicar esse tamanho a Mini ou II.

`MidikeyboardCommunication.saveConfig` e `SmcCommunication.saveConfig` enviam uma escrita de dados vazios na região 5, endereço 0. Esse comando tem significado de persistência nessas famílias; não descartá-lo por tamanho zero nem generalizá-lo a todas as outras.

FootCtrl básico: modo no endereço 0, canal no 1, tipo de comando em `2 + 2*i`, toggle em `3 + 2*i` (cinco entradas), TRS MIDI no 12. Configurações avançadas usam offsets e serialização JNI.

## Limitações e cuidados de reconstrução

- `communication/JNIUtils.java` depende de `libGetOffset.so` para offsets/serialização FootCtrl e CubeTurner. O registro original mencionava binários Android ARM/x86 em `app/src/main/lib/`; esse diretório e essas bibliotecas não estão incluídos nesta árvore. Não foi localizado fonte C/C++ no acervo inspecionado. Bibliotecas Android não são DLLs Windows; perfis que dependam delas exigem investigação própria.
- A descompilação de `getReadResData` contém loops suspeitos; `eraseFlash` cria uma thread sem iniciá-la no texto recuperado. Não copiar esses trechos literalmente.
- Há leituras do buffer sem verificar se todos os bytes chegaram, manipulação de bytes com sinal e checksum com promoção de tipo suspeita. Reimplementar parser incremental com bytes sem sinal, limites e checksum de oito bits.
- O lock não serializa necessariamente envio e resposta como uma única transação. Usar uma fila única com uma solicitação pendente por vez.
- O parser de escrita espera cabeçalho `00 59`, comando de resposta `00` e status zero. O formato completo do ACK precisa de captura; o parser original não valida todos os seus campos.
- Não foi localizado uso de `android.media.midi`, `UsbManager` ou UUIDs padrão BLE-MIDI no pacote da aplicação pesquisado. Isso sustenta o caminho BLE proprietário encontrado, mas não demonstra ausência de outros recursos no firmware.

## Arquitetura proposta na investigação inicial

Separar interface, perfil do modelo, codec de protocolo, fila de transações e transporte. Para um aplicativo Windows nativo, C# com WinUI é uma opção coerente: interface independente do protocolo e adaptador GATT usando APIs do Windows. Um adaptador MIDI separado pode servir às mensagens musicais quando o dispositivo expuser essa porta.

O Windows documenta descoberta GATT, escrita e notificações em [Bluetooth GATT Client](https://learn.microsoft.com/en-us/windows/apps/develop/devices-sensors/gatt-client), e portas MIDI em [MIDI para Windows apps](https://learn.microsoft.com/en-us/windows/apps/develop/media-authoring-processing/midi). A existência dessas APIs não confirma compatibilidade do dispositivo específico.

Fluxo proposto: descobrir → conectar → assinar notificações → identificar modelo/versão → ler estado → habilitar controles do perfil → enviar mudanças em fila → confirmar resposta → atualizar estado → salvar quando solicitado. Sliders devem agrupar mudanças pendentes; leituras e salvamentos não devem disputar respostas.

## Próximas etapas propostas em 16/09/2026

1. Identificar modelo, firmware e conexão desejada.
2. Extrair somente o perfil correspondente: parâmetros, regiões, offsets, tipos, limites e comandos de persistência.
3. Criar codec independente com vetores de bytes e parser testado para fragmentação, checksum incorreto, timeout e respostas concatenadas.
4. Validar descoberta/identificação/leitura no dispositivo e comparar o estado com o app original.
5. Validar uma alteração reversível com leitura de retorno; depois expandir funções e implementar a interface moderna.

Este documento preserva a análise e o mapa inicial. O aplicativo Windows para Cube Baby USB foi implementado posteriormente e validado pelo usuário, conforme as atualizações no início deste documento. O catálogo completo de todos os modelos permanece fora do escopo.
