# Arquitetura e dados locais

[Documentação](../README.md) · Revisão: 12/10/2026

O aplicativo Windows é C#/.NET 10 com WPF. O acervo descompilado Android em `engenharia-reversa/apk-cubesuite/` reúne `sources/` e `resources/` e fornece evidências de protocolo; não é dependência de execução nem solução de build do aplicativo.

## Projetos

Os caminhos abaixo são relativos a [windows/](../../windows/README.md).

| Projeto | Responsabilidade |
| --- | --- |
| Cube.Protocol | Envelope, parser, empacotamento SysEx, perfil Cube Baby, sessão, fila, formatos de IR/amp e backups |
| Cube.Usb | Implementação de `IMidiConnection` com `Windows.Devices.Midi` e descoberta de portas |
| Cube.Desktop | Janela WPF, ViewModels, controles, aparência, idiomas, IRs, criador de amps e áudio de backing tracks |
| Cube.Tests | Executável de testes de protocolo e `FakeConnection` |
| Cube.Desktop.Tests | Executável de testes WPF, cenários simulados e capturas |
| Cube.Diagnostics | Operações de diagnóstico/pesquisa que acessam o pedal |

O desktop referencia Cube.Usb, que referencia Cube.Protocol. O build de simulação inclui `Cube.Tests/FakeConnection.cs` por condição no projeto desktop. O áudio de backing tracks utiliza NAudio Core/Wasapi 3.1.0 e saída WASAPI, fora do protocolo de controle do pedal. O processamento de velocidade e pitch usa `CubeBackingDsp.dll`, compilada em C++ com Signalsmith Stretch. O player tem fila própria de comandos, loops, metrônomo, gravação e exportação. Busca, download e prévia do YouTube usam ferramentas preparadas pelo build: yt-dlp, FFmpeg/ffprobe e Deno.

## Caminho de uma edição

```mermaid
flowchart LR
    UI["WPF / ViewModels"] --> Q["LiveEditQueue"]
    Q --> S["CubeSession"]
    S --> C["CubePacket / UsbSysEx"]
    C --> U["WindowsMidiConnection"]
    U <--> P["Cube Baby USB-MIDI"]
    U --> R["Assembler / parser / resposta"]
    R --> S
    S --> UI
```

`LiveEditQueue` reúne mudanças de parâmetros e mantém o preset de destino de cada comando. `CubeSession` serializa operações com semáforo e mantém uma requisição pendente por vez. O timeout padrão de troca é 1,5 segundo. O parser verifica envelope/checksum e as leituras esperadas conferem região, endereço e tamanho.

Formato: `00 59 CMD LEN24_LE PAYLOAD CHECKSUM`, com checksum `(~sum(payload)) & 0xFF`, seguido de empacotamento contínuo em sete bits entre `F0/F7`. A referência completa está no [catálogo de comandos](../../../../engenharia-reversa/cube-baby/CUBE_BABY_COMANDOS.md).

## Estado e persistência

O editor mantém banco de referência (`baseline`), última confirmação (`confirmed`) e edição (`edited`). A/B/C têm 16 bytes cada, total de 48; treze campos são conhecidos em cada preset. Bytes reservados são preservados.

| Domínio | Prévia/edição | Persistência |
| --- | --- | --- |
| Presets | Região 5, `0x80000000 + preset × 16 + offset` | Escrita dos 48 bytes em região 5, endereço 0 |
| IR | Flag em região 4, `0x764`; 2052 bytes a partir de `0x768` | Oito setores de 4096 bytes, região 0, bases `0x69000–0x70000` |
| Amp | Corpo de 1568 bytes em região 4, `0x1C–0x63B` | Nove setores de 4096 bytes, região 0, bases `0x60000–0x68000` |
| Preferências e backing tracks | Estado local da interface/player | Arquivos no perfil Windows, sem gravação desses dados no pedal |

As bases da tabela são inícios de setores, não limites finais de conteúdo. A leitura do banco pode refletir edições live; somente ler de volta não prova persistência após desligamento. Uma confirmação de gravação exige a comparação implementada pelo fluxo, além do ACK.

A conexão automática identifica o aparelho, lê o banco e inventaria IRs. Existe código experimental de footswitch, mas `StartFootswitchMonitoring` não é chamado no fluxo atual. Ver [investigação](../historico/physical-switch-investigation.md).

## Criadores de efeitos e atualização de firmware

Chorus, Phaser, Delay e Reverb têm prévias em RAM com verificação de assinaturas de código/tabelas do firmware e do contexto físico. O perfil adicional `cubebaby_B_20220706.up` possui endereços e hashes próprios em [CubeSession.EffectsFirmware](../../windows/Cube.Protocol/CubeSession.EffectsFirmware.cs). Compatibilidade recusada pelo criador não implica, por si só, bloqueio da edição normal de presets. Os criadores consultam o estado físico ao preparar a prévia; a seleção A/B/C da interface não comprova o footswitch ativo.

Os snapshots dos quatro efeitos são copiados e validados em memória pelo editor. O fechamento normal tenta encerrar essas prévias, restaurar o amp e restaurar o IR; se uma etapa falhar, apresenta a opção de fechar mesmo assim. Encerramento abrupto não executa esse fluxo nem conserva os snapshots para a próxima execução.

A atualização de firmware usa [FirmwareUpdater](../../windows/Cube.Desktop/FirmwareUpdater.cs), separadamente da sessão USB-MIDI. Um processo auxiliar elevado comunica progresso por named pipe. O fluxo valida extensão, tamanho, cabeçalho e CRC do arquivo `.up`, confere seu SHA-256 após a confirmação, valida o auxiliar de reinicialização e a identificação da flash e verifica o CRC dos blocos gravados. Essas verificações não constituem homologação de qualquer arquivo ou modelo de pedal.

## Arquivos do usuário

Diretório padrão: `%LOCALAPPDATA%\CubeControl`. Com `CubeSimulation=true`, os dados do editor usam `%LOCALAPPDATA%\CubeControl-Simulation`.

| Caminho relativo | Conteúdo / comportamento |
| --- | --- |
| `visual-preferences-v1.json` | Associações de gabinetes, nomes por conteúdo, imagens e preferência de associação automática |
| `IrNames/` | Arquivos `.txt` por hash do IR com o nome local do WAV; mantidos também para compatibilidade com metadados anteriores |
| `Images/` | Cópias de imagens personalizadas |
| `backing-tracks-v1.json` | Lista de caminhos de áudio, ordem, seleção, saída, volume e mute |
| `appearance-v1.json` | Tema, sempre na pasta CubeControl |
| `language-v1.json` | Idioma, sempre na pasta CubeControl |
| `zoom-v1.json` | Zoom, sempre na pasta CubeControl |

Os fluxos automáticos de `BackupBank`, `BackupIr` e preservação de amp mantêm cópias em memória da sessão. As prévias de IR e dos criadores também não geram nem carregam arquivos `pending-*-preview.json` no editor atual. Reconectar na mesma execução pode permitir restaurar uma prévia, conforme identidade e contexto; reiniciar o aplicativo perde esses snapshots. Exportar banco, backup de setor IR, amp ou ajustes de efeito é uma operação explícita, com arquivo no destino escolhido. Arquivos de recuperação de versões anteriores e evidências produzidas por diagnósticos não equivalem a recuperação automática do editor.

Tema, idioma e zoom são compartilhados entre aplicativo real e simulado. Exportações ficam no destino escolhido. Logs da sessão são mantidos na interface; diagnósticos podem gravar arquivos nos caminhos solicitados. Não há telemetria implementada no código de produção inspecionado.

## Formatos e recursos

| Recurso | Origem / formato |
| --- | --- |
| Banco de presets | JSON `cubecontrol-bank-v1`: dispositivo, data e `bankHex` de 48 bytes |
| Backup de setor IR | Manifesto `cubecontrol-ir-v1` e `.bin` de 4096 bytes, com integridade verificada |
| WAV preparado | Mono, 48 kHz, 512 amostras float32, sem normalização de amplitude |
| Setor de amp | 4096 bytes; corpo editável separado do cabeçalho e preenchimento |
| Biblioteca de IRs | 276 WAVs em `Cube.Desktop/Assets/ImpulseResponses`, copiados à saída |
| Amps originais | `Assets/FactoryAmps.bin`, 36864 bytes, recurso embutido `Cube.FactoryAmps`; usado para reconhecer o nome pelo conteúdo |
| Presets e IRs de fábrica | Extraídos da instalação original do CubeSuite por `FactoryResources.Load`, com hashes específicos |
| Manual | PDF em `Cube.Desktop/Assets/Documents`, copiado à saída |
| Idiomas e fotos | JSON em `Localization` e PNGs em `Assets/Images/Equipment` |

`FactoryAmps.bin` não substitui `BFI.bin` no fluxo de recuperação de presets/IRs. O CubeSuite.exe e o BFI.bin exigidos pelo carregador não são redistribuídos nesta árvore. O conteúdo e os hashes são documentados em [IR e fábrica](../referencias/cubecontrol-ir-e-fabrica.md).

## Fontes de implementação

[Perfil de presets](../../windows/Cube.Protocol/CubeBabyProfile.cs), [sessão](../../windows/Cube.Protocol/CubeSession.cs), [fila](../../windows/Cube.Protocol/LiveEditQueue.cs), [transporte Windows](../../windows/Cube.Usb/WindowsMidiConnection.cs), [estado do editor](../../windows/Cube.Desktop/ViewModels/EditorViewModel.cs) e [projeto desktop](../../windows/Cube.Desktop/Cube.Desktop.csproj).

Os resultados históricos, as correções já incorporadas e as falhas de testes reproduzidas em 12/10/2026, além do registro da padronização de versão/publicação em 1.1, estão reunidos na [revisão documental](revisao-documental.md).
