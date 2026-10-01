# Cube Baby: IR, distância e recursos de fábrica

Implementação e ensaios de 17/09/2026, específicos ao Cube Baby de guitarra identificado como `SINCO-CubeBaby`. O manual fornecido está em `windows/artifacts/Cube-Baby-M-Vave-Guitar-Pedal-User-Manual.md` e orienta nomes, modos e apresentação; os endereços não foram deduzidos do manual.

## Evidência do CubeSuite original

Origem examinada: `C:\Users\lzzpm\Downloads\CubeSuite\CubeSuite.exe` e `bin/BFI.bin`. Análise estática do PE32 x86 e de suas chamadas Qt, comparada ao protocolo USB de referência. Não foi feita captura dinâmica do tráfego do CubeSuite; a confirmação adicional foi leitura/escrita reversível no próprio pedal com a implementação C#.

| Recurso | SHA-256 |
|---|---|
| CubeSuite.exe | `0E0183FD6DC1B347534C9CE1E3677ECDA2A6AE77FE3DA8D6314D43C7C2CFD2E6` |
| bin/BFI.bin, 69.632 bytes | `2023EE73672E43A0381A37AE5ECB7B1E298836E84B052DF609D4EF9509EE37E5` |

Os recursos não são redistribuídos. `FactoryResources.Load` exige esses checksums e extrai os dados da instalação original. Outra versão permanece bloqueada até análise própria. Não há acesso à atualização de firmware.

### Distância

`CCubeBabyIREditDlg::OnDialChanged`, VA `0x4A3770`, lê `QDial::value`, converte para float e divide pelo float 100 em VA `0x584580`. Os quatro bytes são escritos na região 4, endereço `0x768`. Logo, 0–100% corresponde a `percent / 100f`, não a um ganho presumido. A informação de silêncio em 100% vem do manual; não foi medida acusticamente.

### Presets de fábrica

`OnResetSound`, VA `0x4A3E10`, usa 48 bytes em VA `0x584B94`, RVA `0x184B94`, com ImageBase `0x400000`:

```text
070709053441141E0704010101000000
040205052C46171E0705010101000000
01070705346F140C0502010101000000
```

O original utiliza a região 0, endereço `0x30000`, com escrita e comparação por leitura (rotina VA `0x4A5930`). O CubeControl extrai os valores originais, aplica os 13 campos conhecidos de cada preset e usa a operação de salvar banco da região 5/endereço zero, já validada pelo usuário. Preserva os bytes reservados do aparelho. Essa sequência equivalente foi ensaiada fisicamente com leitura dos 48 bytes e recuperação do banco anterior; não foi inventado um comando de reset.

### IRs de fábrica

`OnFactoryIR`, VA `0x4A38A0`, referencia `bin/BFI.bin` em VA `0x584B88`. O intervalo `0x9000..0x10FFF` fornece oito setores de 4096 bytes, na ordem dos slots. Cada setor original é preservado integralmente, incluindo cabeçalho e preenchimento.

## Operações USB implementadas

Envelope e conversão SysEx permanecem os mesmos da edição de presets.

| Operação | Região/endereço | Detalhes |
|---|---|---|
| Ler/gravar IR em flash | região 0, `0x69000 + slotZeroBased * 0x1000` | Slots 1–8: `0x69000` até `0x70000` |
| Apagar setor | CMD `21`, região 0 + endereço U32 LE | Limitado aos oito endereços acima |
| Ler | CMD `23` | Blocos de até 113 bytes |
| Escrever | CMD `22` | Blocos de 128 bytes |
| Flag de prévia | região 4, `0x764`, 1 byte | Snapshot inicial observado: 3; firmware informa também bit ready `0x02` |
| Payload RAM | região 4, `0x768`, 2052 bytes | Float distância + 512 amostras float32 LE |
| Registrar atualização | CMD `24` | Região 4, endereço U32 + valor U32; somente `0x764/1` e `0x768/2052` |

Prévia: ler flag/payload anteriores → persistir snapshot → flag zero/registrar → escrever payload → registrar 2052 bytes → aplicar flag de prévia → ler e comparar. Encerrar prévia restaura flag e payload originais e confere. Um snapshot pendente sobrevive ao reinício do aplicativo em `%LOCALAPPDATA%/CubeControl/pending-ir-preview.json`; reconectar e encerrar a prévia conclui a recuperação. A identidade disponível é comparada, mas não se afirma que ela seja um número de série único.

IR próprio em flash: cabeçalho U32 zero + payload de 2052 bytes + zeros até 4096. WAV PCM 16/24/32 ou float32, inclusive extensible; mono ou estéreo (média). Reamostragem linear para 48 kHz, truncamento/preenchimento para 512 amostras, sem normalização. A amplitude visual da forma de onda não altera os dados.

Antes de apagar: ler todos os 4096 bytes, comparar com a captura esperada, criar `.bin` exato e manifesto `cubecontrol-ir-v1`, reler/checksum do arquivo e obter confirmação na interface. Falha de backup impede erase. Depois: gravar 32 blocos e comparar os 4096 bytes lidos. ACK isolado nunca significa sucesso. Após iniciar erase, não há cancelamento voluntário entre blocos; uma desconexão real interrompe e exige recuperação do backup.

## Ensaios e alcance da validação

1. `hardware-ir-validation`: prévia RAM, gravação de WAV no slot 8, comparação completa, restauração do slot e da RAM; banco A/B/C inalterado.
2. `hardware-factory-validation`: backup dos oito setores e banco; aplicação de presets e oito IRs originais; leitura de confirmação de cada um; restauração exata dos oito setores, banco e RAM. Resultado em `factory-validation.json`.
3. `hardware-live-queue`: 40 alterações rápidas com destinos A/C, quatro transmissões, flush antes de salvar, confirmação e restauração do banco inicial daquele ensaio.

Cada ensaio captura o estado vigente ao começar; capturas de horários diferentes podem conter edições posteriores do usuário. Não se usa um dump histórico fixo para sobrescrever o aparelho.

Esses ensaios validam bytes e comportamento de comunicação neste aparelho. Não equivalem a comparação auditiva, teste em outros firmwares nem comprovação de retenção após desligar a energia. O usuário já havia validado controles e salvamento da versão anterior na prática.
