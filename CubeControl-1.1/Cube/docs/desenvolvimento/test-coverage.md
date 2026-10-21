# Cobertura automatizada da versão atual

Esta matriz descreve suítes executáveis, não uma porcentagem de cobertura nem certificação física. O catálogo de execução está em `windows/Cube.Desktop.Tests/TestCatalog.cs`. O relatório de cada execução registra resultados reais; a presença de uma suíte nesta tabela não significa que ela passou.

## Executar e consultar

Na raiz do repositório:

```powershell
.\scripts\Test-CubeControl.ps1
.\scripts\Test-CubeControl.ps1 -Suite borderless,backing-session,catalog-contract
.\scripts\Build-CubeControl.ps1 -CompileOnly -OutputRoot C:\CubeControl-Validation\Release
.\scripts\Build-CubeControl.ps1 -OutputRoot C:\CubeControl-Validation\Release
.\scripts\Build-CubeControl.ps1 -Simulation -OutputRoot C:\CubeControl-Validation\Simulation
```

O executor compila os dois projetos de testes em Release, executa cada suíte local em processo separado e sequencialmente, e retorna código diferente de zero se houver falha ou timeout. `-SkipBuild` reutiliza os executáveis existentes: usar somente quando as fontes não mudaram. `-TimeoutSeconds` define o limite de cada processo (300 segundos por padrão).

Requisitos: Windows com sessão gráfica disponível, .NET 10 SDK, ferramentas C++ usadas pelo build nativo, PowerShell e Python 3. A suíte `factory-irs` também usa os arquivos locais originais `BFI.bin` e `BFIN.bin` em `%USERPROFILE%\Downloads\CubeSuite\bin`, conferidos por hash pelo catálogo; sua ausência causa falha explícita, sem download automático. Durante as suítes WPF, evitar interação com outras janelas para não disputar o foco.

`--list` no executável Desktop produz o catálogo JSON. `--suite <id>` escolhe uma suíte; os argumentos legados continuam aceitos. Argumentos desconhecidos ou múltiplos seletores são rejeitados. A suíte `documentation` aceita uma opção `--documentation-root=<pasta>` com ambos os seletores; a opção é rejeitada quando vazia, repetida ou associada a outra suíte. Os executores especializados `--qa` e `--usability` mantêm seus filtros de baterias e relatórios; `qa-desktop` e `qa-usability` executam todas as baterias locais correspondentes. O executor também inclui o protocolo básico, `qa-protocol` (incluindo prévia de chorus e recursos dos packs de IR), `doc-links` (Python 3) e `doc-catalog` (PowerShell).

Relatórios ficam em `C:\CubeControl-Validation\Tests\runs\<execução>`: `catalog.json`, `results.json`, stdout/stderr por suíte e diretórios de dados isolados. As baterias QA gravam também resultados por caso e JUnit. Capturas e demais artefatos locais ficam em `<suite>-data/reports`, sem sobrescrever resultados de outras execuções.

`CUBECONTROL_TEST_DATA` direciona preferências e dados do app para a pasta de cada suíte. TEMP/TMP também são isolados por processo. Sem essa variável, o aplicativo mantém os caminhos normais do perfil Windows. Não executar duas instâncias do script sobre a mesma raiz de artefatos.

## Funcionalidades e evidências

| Área | Cobertura local | Limites / verificação complementar |
| --- | --- | --- |
| Recursos e interface sem contornos | `borderless`: carregamento real de estilos, seleção, foco, desabilitação, temas, expansores, menus, tooltips e diálogos | Inspeção visual de composição, fontes e contraste renderizado; XML bem formado não comprova carregamento WPF |
| Navegação, entrada e acessibilidade | tema, localização, zoom, responsividade, controles, atalhos, QA de usabilidade | Narrator e interação física não são certificados por eventos sintéticos |
| Efeitos e presets | protocolo falso, falhas de comunicação, prévias, restauração, ciclo de edição e salvamento | USB, firmware real e persistência após desligar exigem pedal |
| Biblioteca e packs de IR | busca, catálogo, montagem, confirmação/cancelamento, transferência simulada e pré-audição com serviço falso | Escuta e gravação real no pedal ficam fora da execução local |
| Sessões e gravação | mixagem offline, offsets, solo/mute, migração/persistência, envelopes, exportação, cancelamento e falha preservando arquivo anterior | Captura WASAPI, latência e sincronização física exigem dispositivos |
| Metrônomo e prática | amostras de clique, tempo, configuração, importação e interface (`metronome-offline`, `backing-practice`) | Reprodução e loopback ficam nas suítes explicitamente excluídas |
| YouTube | serviços falsos, respostas atrasadas, cancelamento, download/prévia simulados | Nenhuma execução contra YouTube real no conjunto local |
| Documentação e pacote | catálogo, recursos, busca e renderização; validadores de links e empacotamento | Verificação editorial dos guias e abertura do pacote Simulation complementam os testes |

## Catálogo de suítes Desktop

| ID | Argumento legado | Requisito | Host WPF |
| --- | --- | --- | --- |
| `drag-drop` | `--drag-drop-only` | local | sim |
| `player-layout` | `--player-layout-only` | local | sim |
| `overdriven` | `--overdriven-only` | local | sim |
| `documentation` | `--documentation-only` | local | sim |
| `ir-audition` | `--ir-audition-only` | local | sim |
| `image-picker` | `--image-picker-only` | local | sim |
| `assets` | `--assets-only` | local | sim |
| `youtube-preview-live` | `--youtube-preview-live` | online | sim |
| `youtube` | `--youtube-only` | local | sim |
| `backing-profile` | `--backing-profile` | audio-device | sim |
| `backing-preview` | `--backing-preview` | audio-device | sim |
| `pack-workflow` | `--pack-workflow-only` | local | sim |
| `pack-review-ui` | `--pack-review-ui-only` | local | sim |
| `curated-packs` | `--curated-packs-only` | local | sim |
| `live-session` | `--live-session-only` | local | sim |
| `factory-irs` | `--factory-irs-only` | local | sim |
| `amp-capacitors` | `--amp-capacitors-only` | local | sim |
| `hardware-effect-cards` | `--hardware-effect-cards` | hardware | sim |
| `hardware-advanced-lifecycle` | `--hardware-advanced-lifecycle` | hardware | sim |
| `advanced-lifecycle` | `--advanced-lifecycle-only` | local | sim |
| `advanced-interaction` | `--advanced-interaction-only` | local | sim |
| `backing-responsive` | `--backing-responsive` | audio-device | sim |
| `backing-responsive-offline` | `--backing-responsive-offline` | local | sim |
| `metronome` | `--metronome-only` | audio-device | sim |
| `backing-practice` | `--backing-practice-only` | local | sim |
| `backing` | `--backing-only` | audio-device | sim |
| `effects-firmware-b` | `--effects-firmware-b-only` | local | sim |
| `delay` | `--delay-only` | local | sim |
| `reverb` | `--reverb-only` | local | sim |
| `chorus` | `--chorus-only` | local | sim |
| `phaser` | `--phaser-only` | local | sim |
| `carousel` | `--carousel-only` | local | sim |
| `artwork` | `--artwork-only` | local | sim |
| `buttons` | `--buttons-only` | local | sim |
| `effects` | `--effects-only` | local | sim |
| `chevrons` | `--chevrons-only` | local | sim |
| `scroll-profile` | `--scroll-profile` | local | sim |
| `snackbar` | `--snackbar-only` | local | sim |
| `auto-connect` | `--auto-connect-only` | local | sim |
| `save-presets` | `--save-presets-only` | local | sim |
| `motion` | `--motion-only` | local | sim |
| `ir-library-ui` | `--ir-library-ui` | local | sim |
| `ir-library` | `--ir-library-only` | local | sim |
| `ir-catalog` | `--ir-catalog-only` | local | sim |
| `progress` | `--progress-only` | local | sim |
| `options-menu` | `--options-menu-only` | local | sim |
| `ui-audit` | `--ui-audit` | local | sim |
| `microphone` | `--microphone-only` | local | sim |
| `knob-wheel` | `--knob-wheel-only` | local | sim |
| `density` | `--density-only` | local | sim |
| `zoom-compact` | `--zoom-compact-only` | local | sim |
| `zoom-dialogs` | `--zoom-dialogs-only` | local | sim |
| `zoom` | `--zoom-only` | local | sim |
| `responsive` | `--responsive-only` | local | sim |
| `volume` | `--volume-only` | local | sim |
| `wheel` | `--wheel-only` | local | sim |
| `chain` | `--chain-only` | local | sim |
| `presentation-render` | `--presentation-render` | local | sim |
| `theme` | `--theme-only` | local | sim |
| `localization` | `--localization-only` | local | sim |
| `midi-ports` | `--midi-ports-only` | local | não |
| `backing-audio-boundaries` | `--backing-audio-boundaries` | local | não |
| `multitrack-capture` | `--multitrack-capture` | local | não |
| `backing-daw` | `--backing-daw` | local | sim |
| `backing-session` | `--backing-session-only` | local | não |
| `backing-recording` | `--backing-recording-only` | local | não |
| `firmware-worker` | `--firmware-worker-only` | elevation | não |
| `editor` | `--editor-only` | local | sim |
| `default` | `--default` | local | sim |
| `direct-pack-editing` | `--direct-pack-editing` | local | sim |
| `footswitch` | `--footswitch` | local | sim |
| `pack-compact-ui` | `--pack-compact-ui` | local | sim |
| `backing-offline` | `--backing-offline` | local | sim |
| `metronome-offline` | `--metronome-offline` | local | sim |
| `borderless` | `--borderless` | local | sim |
| `qa-desktop` | `--qa-all` | local | não |
| `qa-usability` | `--usability-all` | local | não |
| `catalog-contract` | `--catalog-contract` | local | não |

## Exclusões e lacunas explícitas

A regressão de pitch usa uma senoide de 440 Hz a 48 kHz, 25 combinações de velocidade/transposição e uma exportação de sessão. Exige desvio inferior a 5 cents e duração correta. A exportação usa uma janela DSP de 240 ms; a reprodução conserva a configuração anterior de 120 ms. Esse ensaio não certifica qualidade perceptiva, transientes musicais nem latência em dispositivo.

- `hardware`: não abrir MIDI nem gravar no pedal durante a execução local. O cenário `hardware-advanced-recovery` foi removido: dependia de `LoadPendingChorus`, método inexistente, e de recuperação persistida que já não é usada. O seletor antigo é rejeitado antes de abrir dispositivos; `advanced-lifecycle` e `chorus` verificam a recuperação em memória e o tratamento dos arquivos legados com transporte simulado.
- `audio-device`: reprodução, loopback e perfis que usam saída real não entram no conjunto local; as variantes `backing-offline` e `metronome-offline` preservam as verificações independentes de dispositivo.
- `online`: a prévia real do YouTube exige execução separada.
- `elevation`: o worker de firmware usa UAC e precisa de execução explícita, embora seu cenário rejeite o arquivo antes de acessar dispositivos.
- Casos com parâmetros auxiliares são exercitados por suas suítes: BackingMusic/BackingUiReview por backing-practice; YouTubeDownload/YouTubePreviewData por youtube. Não duplicar esses testes como execuções vazias.
- Não há alegação de cobertura integral. Novas funcionalidades devem registrar cenário de sucesso, falha/cancelamento e persistência quando aplicável; testes devem observar comportamento, não somente procurar strings no XAML.

`backing-audio-boundaries` verifica ganho/silêncio, conservação de potência do panorama mono, compatibilidade do centro mono legado, limites do panorama, overflow/underrun e retomada do monitor em 44,1/48/96 kHz, escrita apenas no trecho solicitado do buffer e janela RMS com consumo independente do pico. Não usa dispositivos de áudio.

`catalog-contract` verifica IDs e aliases únicos, metadados reconhecidos pelo executor, argumentos inválidos/conflitantes e equivalência entre `--suite midi-ports` e seu alias legado, executando o cenário sintético em processos separados.

`multitrack-capture` verifica conversão de PCM assinado de 16/24/32 bits com amostras conhecidas, cabeçalho WAV finalizado, rejeição de pacotes após encerramento, encerramento repetido preservando falhas, rotas inválidas e monitoramento sem arquivo. Alterações de mix durante a captura não podem trocar canais nem criar outro destino; mute mantém o relógio de captura.
