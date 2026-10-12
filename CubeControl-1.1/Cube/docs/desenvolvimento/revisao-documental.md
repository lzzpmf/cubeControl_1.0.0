# Revisão e manutenção da documentação

[Documentação](../README.md) · Data: 01/10/2026

## Atualização verificada — 12/10/2026

Esta seção atualiza o estado do produto; as seções seguintes preservam os resultados e as pendências registrados em 01/10/2026, que não devem ser interpretados integralmente como o estado atual.

- `AmpImage.Validate` aceita ganho e compensação até 100. Os nove amps originais passaram nas verificações de carregamento sem alteração dos dados; a incompatibilidade com o antigo teto 60 foi resolvida.
- O fechamento normal chama o encerramento dos quatro efeitos, `RestoreAmpAsync` e `EndPreviewAsync`. A ausência de tentativa de restaurar amp ao fechar foi resolvida; falhas ainda podem levar à confirmação de fechamento sem recuperação concluída.
- Os snapshots automáticos de banco, IR, amp e dos quatro efeitos ficam em memória no editor atual. Não há retomada automática por `pending-*-preview.json` após reiniciar. Exportações explícitas e arquivos de diagnósticos são fluxos distintos.
- O desktop inclui áudio NAudio/WASAPI e DSP nativo C++, exigindo MSVC x64/Windows SDK além do SDK .NET. A arquitetura também contempla criadores com assinaturas específicas de firmware e atualização por auxiliar elevado.

Verificações executadas nesta análise, com SDK .NET 10.0.401:

| Verificação | Resultado observado |
| --- | --- |
| `dotnet build CubeControl-1.1/Cube/windows/Cube.Desktop/Cube.Desktop.csproj -c Release --artifacts-path C:/CubeControl-Build/study-20261012` | Sucesso, zero avisos e erros, incluindo DSP nativo |
| Mesmo build Release no destino padrão, antes da tentativa isolada | Falha CS2001 por 15 arquivos WPF gerados ausentes; causa dos intermediários não determinada |
| `Cube.Tests`, sem filtro | Interrompido em `one transaction and minimum 100 ms interval`, `ExtendedTests.cs:58`; a fila não impõe esse intervalo |
| `Cube.Tests --effects-firmware-b-only` | 52 verificações passaram |
| `Cube.Tests --ir-packs-only` | 46 verificações passaram |
| `Cube.Tests --reverb-only` | Interrompido em `external bank edit blocks recovery`, `ReverbTests.cs:79` |

Os testes foram executados na raiz por `dotnet run --project CubeControl-1.1/Cube/windows/Cube.Tests -c Release`; os modos específicos acrescentaram `--no-build -- <modo>`. As falhas continuam visíveis: ainda é necessário alinhar o contrato de temporização e investigar a divergência de recuperação de reverb. O sucesso da compilação isolada não demonstra que o problema dos intermediários padrão foi corrigido.

Não houve acesso ao pedal, testes visuais WPF, escuta, atualização de firmware ou geração/instalação do pacote completo. Os testes simulados não comprovam persistência após desligar nem compatibilidade física com outros firmwares.

## Complemento da revisão — 12/10/2026

Uma segunda conferência estática identificou e corrigiu lacunas adicionais nos guias atuais:

- `BackingTracksViewModel` mantém playlist e reprodução na sessão, sem ler/gravar `backing-tracks-v1.json`. Metrônomo e downloads do YouTube têm arquivos próprios de preferências.
- `GearImagePreferences` salva `gear-images-v1.json` e migra apenas imagens do arquivo visual antigo. Nomes de packs usam `ir-pack-names-v1.json`; o editor não lê `IrNames/*.txt`.
- `PrepareAdvancedEffectAsync` exige modo PRESET, seleciona o preset físico na interface e pode escrever parâmetros live para preparar uma nova prévia. Os guias agora distinguem essa preparação de uma leitura sem escrita e não exigem desligar modulação para delay/reverb.
- A descrição de serviços externos inclui a prévia de áudio do YouTube acionada por hover.

Esta conferência não executou os diagnósticos nem acessou o pedal. Não representa nova execução dos testes de protocolo registrados acima.

## Conferência de IRs e recursos de fábrica — 12/10/2026

A leitura de `LoadWavAndPreviewAsync`, `SaveMatches`, `FactoryResources.Load` e `IrPackCatalog.LoadAvailable` confirmou outras correções nos guias pt-BR/en-US: prévia de IR em RAM sem substituição de slot, início automático ao selecionar um WAV com sessão conectada e não bloqueada, associações amp–gabinete apenas na sessão e requisitos externos distintos para presets de fábrica e packs BFI/BFIN. A contagem direta em `Assets/ImpulseResponses` encontrou 958 WAVs, dos quais 682 em Overdriven.fr; as páginas atuais foram atualizadas e as contagens históricas do plano de QA foram identificadas como tais. Nenhuma gravação ou consulta ao pedal foi executada nesta conferência.

## Reanálise dos comandos de teste e validação — 12/10/2026

A conferência dos despachos em `Cube.Tests/Program.cs`, `Cube.Desktop.Tests/Program.cs` e da implementação em `AdvancedHardwareTests.cs` encontrou lacunas adicionais:

- O guia Windows agora explicita os filtros do protocolo e que a execução sem filtro não agrega todas as suítes dedicadas. Vários filtros na mesma chamada não combinam os testes; prevalece a ordem do despacho.
- A descrição genérica de transporte simulado foi corrigida: três modos desktop `--hardware-*` abrem o pedal real. O guia registra alterações, confirmação automática no modelo, evidências e tentativa de restauração.
- O ramo `--hardware-advanced-recovery` ainda espera `pending-chorus-preview.json`, `LoadPendingChorus` e arquivamento legado. A busca no código desktop não encontrou esse método nem o fluxo legado. Essa incompatibilidade continua sendo uma pendência de implementação; documentá-la não corrige o teste nem fornece recuperação persistente ao editor.
- O guia de validação agora fornece comandos reproduzíveis e distingue a raiz do pacote usada por `-PackageRoot` da pasta `Assets/Documents` usada por `--documentation-root`. Os resultados de 08/10 foram identificados como históricos.

Verificações executadas nesta reanálise: `python scripts/qa/validate_document_links.py` e `scripts/Validate-Documentation.ps1`, ambos aprovados (18 entradas e 34 arquivos no catálogo). Não foram repetidos testes de protocolo/WPF, build, inspeção visual ou ensaios no pedal. As alterações desta etapa são somente documentais e preservam as edições anteriores do checkout.

## Escopo e resultado

Foram analisados os **21 arquivos Markdown originais** do repositório, incluindo relatórios da raiz, documentação da aplicação, READMEs de recursos e o README de terceiro em `resources/META-INF`. O comportamento descrito foi confrontado com projetos C#/XAML, scripts PowerShell/CMD, catálogos e artefatos binários disponíveis.

A organização passou a ter um [README de entrada](../../../../README.md), [índice central](../README.md), guias de uso/recursos, referência de arquitetura, padrões de interface e histórico separado. Os relatórios técnicos ganharam navegação; na reorganização posterior descrita abaixo, foram transferidos para `engenharia-reversa/cube-baby/`. O README do jsoup foi preservado como documento de terceiro.

A etapa inicial desta revisão alterou somente documentação, sem modificar aplicação, testes, scripts, binários ou configuração do pedal e sem executar publicação. Posteriormente, a pedido do mantenedor, a padronização em 1.1 alterou a pasta de fontes, os metadados de versão e os scripts, conforme o registro abaixo.

## Cobertura dos documentos originais

| Documento / grupo | Revisão aplicada |
| --- | --- |
| `CubeControl-1.1/README.md` e `Cube/windows/README.md` | Caminhos reais, versão declarada, requisitos, build/testes e etapas de publicação |
| `CUBE_BABY_COMANDOS.md` | Navegação, caracteres corrompidos, consultas físicas e leitura do banco versus persistência |
| `CUBE_BABY_AMPS.md` | Navegação, cronologia, último estado registrado RV06 e limites históricos |
| `CUBE_BABY_AMPS_FABRICA.md` | Índice clicável, verificação integral da extração e incompatibilidade com o limite de ganho do validador |
| `CUBE_BABY_AMPS_CONTROLES_E_ESCUTA.md` | Teto individual de controles, Shift, apresentações de EQ e distinção entre propriedades internas e interface visível |
| `DESIGN_SYSTEM.md` | Regras atuais consolidadas; texto anterior preservado em `historico-interface.md` |
| `wpf-ui-integracoes.md` | Componentes implementados separados de propostas futuras |
| `windows-interaction.md` | Zoom e interação conferidos; caminho atual do relatório de rolagem |
| `snackbars.md` | Durações/eventos confrontados com o serviço e navegação adicionada |
| `simulacao-temporaria.md` | Execução sem publicação, propriedade obrigatória, dados compartilhados e limitações |
| `POLITICA-DE-PRIVACIDADE.md` | Responsável/contato fornecidos pelo mantenedor, amps, áudios, armazenamento e exclusão |
| `ampsEirs.md` | Índices e referências visuais separados; retirada de porcentagens de certeza sem método |
| `cabinet-characteristics.md` | Características vinculadas ao conteúdo reconhecido e divergência do 1960AV registrada |
| `effect-descriptions.md` | Origem histórica e vínculo com os textos efetivos da interface |
| `cubecontrol-ir-e-fabrica.md` | Manual existente, origem dos recursos, confirmação/backup e ensaios históricos identificados |
| `physical-switch-investigation.md` | Experimento retirado, ausência de monitoramento ativo e artefatos não incluídos |
| `analise-protocolo-windows.md` | Acervo Android atual em sources/resources; proposta WinUI/BLE identificada como histórica |
| `Localization/README.md` | Submenu real, compartilhamento com simulação e comandos com diretório explícito |
| `Assets/Images/Equipment/README.md` | Phaser e recursos adicionais; uso efetivo separado de imagens apenas disponíveis |
| `resources/META-INF/README.md` | Identificado como README do jsoup; mantido integralmente |

## Divergências ainda presentes no produto

Registro histórico de 01/10/2026: as pendências abaixo foram mantidas fora daquela revisão. Consulte a atualização de 12/10/2026 acima para os itens já resolvidos ou cujo comportamento mudou.

| Tema | Evidência | Consequência / trabalho pendente |
| --- | --- | --- |
| Ganho de amps originais | [AmpImage.Validate](../../windows/Cube.Protocol/AmpImage.cs) versus [extração](../../../../engenharia-reversa/cube-baby/CUBE_BABY_AMPS_FABRICA.md) | Limite 60 recusa TYPEs 6/7/8 cujos dados chegam a 100 |
| Categoria Day Tripper | [EquipmentCatalog](../../windows/Cube.Protocol/EquipmentCatalog.cs) versus [CubeBabyProfile](../../windows/Cube.Protocol/CubeBabyProfile.cs) | Apresentação Overdrive versus categoria Distortion; não alterar índices para resolver texto |
| Descrição Marshall 1960AV | `EquipmentCatalog` versus `GearItem.Category` | Descrição longa ainda diz Vintage 30; características e referência oficial usam G12-Vintage |
| Confiança do Wo Stone | `EquipmentCatalog` mantém “~70%” | Percentual sem método de validação; documentação atual usa associação não confirmada |
| Prévia de amp ao fechar | [MainWindow.xaml.cs](../../windows/Cube.Desktop/MainWindow.xaml.cs) chama `EndPreviewAsync` de IR; [desconexão manual](../../windows/Cube.Desktop/ViewModels/EditorViewModel.cs) também chama `RestoreAmpCore` | Fechamento não restaura amp da mesma forma; orientar restauração explícita |
| Recuperação de amp após reiniciar | [EditorViewModel.Amps](../../windows/Cube.Desktop/ViewModels/EditorViewModel.Amps.cs) | Backups em disco não equivalem a retomada automática da identidade/estado de prévia |
| Teste de temporização | [ExtendedTests.cs](../../windows/Cube.Tests/ExtendedTests.cs) exige 100 ms; [LiveEditQueue](../../windows/Cube.Protocol/LiveEditQueue.cs) não impõe esse intervalo | Critério do teste e comportamento precisam de decisão e alinhamento |
| Descrição da curva | [AmpEditor.xaml](../../windows/Cube.Desktop/Views/AmpEditor.xaml) descreve progressão de sinais suaves a fortes | A consulta por amplitude é hipótese apoiada pela escuta; indexação exata do DSP não foi demonstrada |
| Footswitch | Método experimental existe, mas não é iniciado | Seleção A/B/C na interface não confirma posição física |
| Recursos de fábrica | Carregamento fixo em Downloads/CubeSuite no perfil do usuário | Sem seletor de origem atual; versões de arquivos não reconhecidas deixam a recuperação indisponível |

## Verificações executadas nesta revisão

- Inventário dos 21 Markdown originais e leitura das fontes de implementação pertinentes.
- Reconstrução dos **nove setores de 4096 bytes** usando todas as 3537 linhas hexadecimais da extração, incluindo cabeçalhos/campos mistos e preenchimento. Resultado: igualdade integral com o backup.
- Reconversão dos **3348 valores float32** aos bytes originais, sem divergência.
- Hash do backup de 65536 bytes confirmado: `506577f416011e5119b9064a30d2a2899f5fee73a917f232c0dec47565384657`.
- `FactoryAmps.bin` conferido byte a byte contra os primeiros 36864 bytes do backup.
- `CUBE_BABY_ROCKERVERB_V6.bin` possui 4096 bytes; hash do corpo de 1568 bytes confirmado: `17830faeff6b3f4e4b2f819bc255faa424a75b4bce56ffe6a647a29c5e23c3ef`.
- Compilação Release de `Cube.Desktop` com SDK 10.0.303: **êxito, zero avisos e zero erros**.
- Execução de `Cube.Tests`: **41 verificações passaram antes da falha** em `one transaction and minimum 100 ms interval`, linha 58 de `ExtendedTests.cs`. O restante da suíte não foi executado após essa exceção. Não se declara aprovação integral.
- Os 272 links e imagens locais dos 29 Markdown resultantes foram conferidos, incluindo âncoras: nenhum destino ausente. Todos os documentos são alcançáveis a partir do README da raiz.

Não foram executados testes WPF visuais, nova captura de áudio, comunicação com o pedal, atualização de firmware ou publicação. As descrições dos ensaios de setembro permanecem evidências históricas.

A conferência externa foi pontual: [Marshall 1960AV](https://www.marshall.com/us/en/product/1960av-4x12-angled-cabinet), documentação oficial do [SnackbarService](https://wpfui.lepo.co/api/Wpf.Ui.SnackbarService.html) e texto consolidado da [LGPD](https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709compilado.htm). Não houve revalidação integral dos downloads, links e equipamentos de todas as fontes históricas.

## Padronização em 1.1 — 01/10/2026

Registro histórico da padronização: a divergência de versão/publicação foi resolvida após a revisão inicial, alinhando os scripts e o [projeto](../../windows/Cube.Desktop/Cube.Desktop.csproj). O fluxo de publicação descrito nesta etapa foi posteriormente substituído pelo [build único](../../../../scripts/Build-CubeControl.ps1), registrado abaixo:

- A pasta de fontes passou a ser `CubeControl-1.1`; links e comandos foram atualizados.
- O projeto declara `1.1.0`, com identidade de assembly/manifesto `1.1.0.0`.
- O script central lê a versão do projeto para nomear os pacotes e verifica a versão da DLL antes de aceitar o build ou distribuí-lo.
- Build e publicação usam este checkout. A dependência de uma segunda árvore canônica em `C:\` foi removida.
- Os inicializadores usam caminhos relativos e executam build/publicação antes de abrir a edição normal ou simulada.
- O filtro de distribuição reconhece o nome da versão atual e pastas datadas; não seleciona pacotes de outras versões.

Os comandos e destinos atuais estão no [guia Windows](../../windows/README.md). As outras pendências de implementação da tabela acima continuam abertas.

Validação da padronização:

- `Build-CubeControl.ps1` e `Build-CubeControl.ps1 -Simulation` concluídos com sucesso, incluindo geração dos pacotes `1.1.0-dev-build` e verificação dos recursos.
- Versões de assembly e arquivo das duas DLLs confirmadas em `1.1.0.0`; marcadores `.build-complete` conferidos por SHA-256.
- A guarda de versão aceitou a DLL atual e recusou uma DLL anterior `1.0.0.0`. O filtro selecionou a pasta 1.1.0 e uma pasta datada, excluindo outras versões e a edição simulada do conjunto normal.
- Sintaxe dos três scripts PowerShell e resolução dos caminhos dos dois inicializadores conferidas.
- Os 598 arquivos rastreados fora das alterações previstas preservaram seus hashes Git após a renomeação, incluindo os binários de referência e recursos.
- Os 29 Markdown e seus 272 links/imagens locais foram reconferidos após a mudança de pasta.

Foram preparados builds para validação, sem executar a etapa de distribuição para destinos oficiais ou abrir o aplicativo/pedal. A suíte de protocolo não foi repetida nessa padronização; a falha de temporização registrada na revisão inicial permanece pendente.

## Organização da raiz — 01/10/2026

Registro histórico da reorganização; o número de scripts e os destinos de build desta etapa foram posteriormente simplificados pelo fluxo único descrito a seguir.

A raiz passou a conter somente `README.md`, `.gitignore` e as pastas principais. O acervo de pesquisa ganhou um [índice próprio](../../../../engenharia-reversa/README.md):

- `engenharia-reversa/apk-cubesuite/`: as pastas `sources/` e `resources/` do APK descompilado, com conteúdo preservado.
- `engenharia-reversa/cube-baby/`: os quatro relatórios antes mantidos na raiz; seu subdiretório `artefatos/` reúne os dois binários e o JSON dos ensaios, sem alterações de conteúdo.
- `scripts/`: os três scripts PowerShell. O script central encontra a raiz pelo diretório pai; `builds/` continua sendo uma saída da raiz e passou a ser ignorada pelo Git.
- `CubeControl-1.1/`: aplicação, testes e documentação do produto mantidos na localização já padronizada.

Links, comandos, inicializadores e mensagem do bloqueio de publicação foram ajustados aos novos caminhos. Os números de arquivos e links das verificações anteriores descrevem aquelas etapas, antes dos três novos índices de engenharia reversa.

Validação da organização: 5446 arquivos do acervo movido (fontes/recursos Android e os três artefatos de ensaio) conservaram seus SHA-256; os 32 Markdown e seus 298 links/imagens locais foram conferidos, incluindo âncoras. Todos os documentos são alcançáveis pelo README principal. A sintaxe dos scripts e os caminhos dos inicializadores passaram na conferência. Os builds normal e simulado foram executados com sucesso a partir de `C:\`, usando os scripts em sua nova pasta, sem distribuição final nem acesso ao pedal.

## Build único externo — 01/10/2026

O único ponto de entrada para gerar o pacote do aplicativo é `scripts/Build-CubeControl.ps1`. Ele combina restauração, compilação, pacote completo, verificações e instalação no destino externo. Os scripts antigos de publicação separada foram removidos.

- `-Simulation` seleciona a edição simulada; `-Start` abre o pacote depois do sucesso.
- `-OutputRoot` permite escolher outra pasta absoluta externa. Por padrão, intermediários e pacote ficam em `C:\CubeControl-Build`.
- Há um pacote final por edição em `publish/`. A preparação e a cópia anterior usam `.staging/` temporariamente; não há distribuição para `builds/` ou pastas datadas.
- Fontes, versão da DLL, arquivos essenciais e recursos copiados são conferidos. `build-info.json` registra hashes, versão, configuração e data UTC.
- Os dois inicializadores são atalhos do mesmo script, sem caminhos de executável duplicados.
- As cópias legadas do repositório e as pastas `*-dev-build` foram movidas uma única vez para `C:\CubeControl-Build\legacy\`, com hashes conferidos. Elas não participam do novo fluxo.

O [guia Windows](../../windows/README.md) contém os comandos vigentes. Os comandos de teste continuam sendo procedimentos de verificação, sem produzir outro pacote de distribuição.

Validação do fluxo único:

- Build real normal e simulado concluídos no destino padrão. Também foi executado um build simulado com `-OutputRoot` apontando para um diretório externo com espaços, incluindo intermediários nessa raiz alternativa.
- Comparação antes/depois de um build: os 6082 arquivos do repositório mantiveram tamanho e data de modificação, sem arquivos novos/removidos e sem recriação de `builds/`.
- Destino dentro do projeto recusado antes de criar a pasta.
- Quatro cenários isolados com compilador substituído por uma função de teste: falha de compilação, alteração de fonte, falha na instalação e sucesso. As falhas preservaram/restauraram o pacote anterior; o sucesso removeu arquivos obsoletos do pacote. Os temporários foram limpos.
- Os 32 Markdown e seus 300 links/imagens locais foram reconferidos, incluindo âncoras; todos continuam alcançáveis pelo README principal.

O aplicativo não foi aberto e não houve acesso ao pedal nessas verificações. A falha conhecida da suíte de protocolo não foi alterada por esta mudança de build.

## Conferência transversal do comportamento documentado — 12/10/2026

Esta rodada confrontou as instruções atuais com os caminhos de execução e os eventos da interface. As correções foram aplicadas também aos guias em inglês e às palavras-chave do catálogo de ajuda.

| Área | Evidência de código | Correção documental |
|---|---|---|
| Criadores de efeitos | `EditorViewModel.EffectCard.cs`, preparação dos quatro efeitos e `EffectGraphEditor.cs` | Abrir opções avançadas tenta iniciar prévia; retornar à edição simples não restaura; preparação pode enviar parâmetros; confirmação numérica ao perder foco; painel responsivo. |
| Biblioteca de IRs | `IrLibraryViewModel.cs` e catálogo de metadados | Busca textual, árvore por marca/configuração/modelo, tratamento de arquivos sem metadados e remoção das instruções de filtros antigos. |
| Pré-escuta de IRs | `IrAuditionViewModel` e serviço de reprodução | Cinco gravações, saída Windows, processamento no pedal, condições para tocar, pausa na transferência e exclusão entre reproduções. |
| Packs portáteis | `IrPackStore.Portable` e `ApplyPreparedIrPackAsync` | Limite por WAV, validação antes das cópias, arquivos importados persistentes e gravação parcial sem rollback automático. |
| Firmware | `EditorViewModel.Firmware.cs` e atualizador | Fluxo separado do carregador de IRs, desconexão MIDI, validação do arquivo, elevação e limites de recuperação. |
| Ajuda e recursos | `DocumentCatalog.cs`, projeto Desktop e catálogos de imagens | Busca em metadados, fallback de idioma, fontes publicadas, imagens/IDs atuais e distinção dos manuais preservados. |

Validação documental desta rodada: 536 links locais sem destinos ou âncoras inválidos; catálogo com 18 entradas e 34 arquivos publicados; `git diff --check` sem erros de espaços. A revisão é estática: confirma regras implementadas, não medições acústicas, comportamento do hardware nem resultados visuais em execução. Relatos históricos, manuais preservados e hipóteses de engenharia reversa mantêm sua origem; não foram convertidos em conclusões novas. Nenhum código funcional foi alterado e nenhum comando foi enviado ao pedal nesta rodada. Resultados antigos de build/testes continuam sendo registros das respectivas execuções.

## Como manter a documentação

1. Atualize o guia do recurso junto com mudanças observáveis no código, incluindo prévia automática, gravação e recuperação.
2. Declare a pasta de execução de cada comando. Prefira caminhos relativos ao repositório; mantenha caminhos absolutos apenas quando forem comportamento real ou evidência histórica.
3. Use a versão do projeto como versão declarada do aplicativo; diferencie nomes de pastas, pacotes e versões antigas.
4. Preserve bytes, hashes e relatos originais. Acrescente novas interpretações datadas sem reescrever um resultado de ensaio como se fosse medição nova.
5. Separe fatos de código, comparação binária, escuta, hipótese e pendência. Uma faixa aceita pelo arquivo não comprova estabilidade do áudio.
6. Ligue novos documentos ao índice; confira destinos e âncoras após renomear ou mover arquivos.
7. Registre testes executados, falhas e alcance real. Não copie uma aprovação antiga como resultado da revisão atual.

O design system atual orienta implementação; o histórico explica decisões anteriores. A documentação de terceiros mantém sua autoria e finalidade próprias.
