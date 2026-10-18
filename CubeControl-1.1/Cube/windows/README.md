# cubeControl — desenvolvimento Windows

[Documentação](../docs/README.md) · [Guia de uso](../docs/usuario/pt-BR/primeiros-passos.md) · Revisão: 12/10/2026

O código está em `CubeControl-1.1/Cube/windows/`. Existe um único fluxo de geração do aplicativo: [scripts/Build-CubeControl.ps1](../../../scripts/Build-CubeControl.ps1).

## Requisitos e versão

- Windows e SDK .NET 10 disponível como `dotnet`. Não há SDK privado nem `global.json` nesta árvore.
- Acesso ao NuGet na primeira restauração. Dependência de interface: WPF UI 4.3.0; áudio: NAudio.Core e NAudio.Wasapi 3.1.0.
- Visual Studio Build Tools com Desktop development with C++, MSVC x64 e Windows SDK. O target `BuildBackingDsp` chama `Cube.Desktop/Native/Build.ps1`, que localiza a toolchain com `vswhere` e compila `CubeBackingDsp.dll`. O SDK .NET sozinho não atende a essa etapa.
- O projeto declara versão **1.1.0**; o script lê essa versão para nomear o pacote.
- O pacote é `win-x64`, inclui o runtime .NET e pode ser executado sem SDK. O alvo desktop é `net10.0-windows10.0.19041.0`; isso não representa uma matriz de versões do Windows testadas.

## Construir e abrir

A partir da **raiz do repositório**:

```powershell
.\scripts\Build-CubeControl.ps1
```

O comando restaura dependências, compila, prepara um pacote completo, verifica fontes/recursos e instala o resultado no destino externo. Não existe uma etapa separada de publicação.

| Opção | Efeito |
| --- | --- |
| Sem opções | Gera a edição normal, sem abrir o aplicativo |
| `-Start` | Executa o mesmo build e abre o pacote após sucesso |
| `-Simulation` | Gera a edição simulada, com configuração e transporte próprios |
| `-OutputRoot 'D:\CubeControl-Build'` | Escolhe outra pasta absoluta de saída, externa ao repositório |

As ferramentas do YouTube ficam em `C:\CubeControl-Build\youtube-tools` (ou em `youtube-tools` dentro do `OutputRoot` escolhido). Os próximos builds verificam e reutilizam os arquivos preparados, sem baixar ou descompactar novamente. A preparação é refeita quando o manifesto `scripts/youtube-tools.json` muda ou algum arquivo preparado está ausente ou alterado; os downloads existentes também são reutilizados quando seu SHA256 corresponde ao manifesto. Preserve essa pasta para manter o cache.

Exemplos do mesmo fluxo:

```powershell
.\scripts\Build-CubeControl.ps1 -Start
.\scripts\Build-CubeControl.ps1 -Simulation -Start
.\scripts\Build-CubeControl.ps1 -OutputRoot 'D:\CubeControl-Build'
```

Os scripts encontram o projeto pela própria localização, independentemente do diretório corrente do terminal. O destino não pode estar dentro do repositório, ser um ancestral dele ou atravessar links/junctions. Feche a edição instalada no destino antes de reconstruí-la.

Os inicializadores são atalhos para esse mesmo script:

| Inicializador | Chamada |
| --- | --- |
| `CubeControl-1.1/Cube/Iniciar-CubeControl.cmd` | `Build-CubeControl.ps1 -Start` |
| `CubeControl-1.1/Iniciar-Simulacao.cmd` | `Build-CubeControl.ps1 -Simulation -Start` |

Para abrir sem reconstruir, execute diretamente o `CubeControl.exe` do pacote já gerado. A edição normal tenta conectar automaticamente ao pedal. Para inspecionar a interface sem iniciar essa conexão:

```powershell
& 'C:\CubeControl-Build\publish\CubeControl-1.1.0\CubeControl.exe' --offline
```

`--offline` não cria um simulador nem impede conexão manual posterior. Evite abrir CubeSuite e cubeControl simultaneamente sobre a mesma porta MIDI.

## Saídas e verificações

Por padrão, todas as saídas de compilação ficam em `C:\CubeControl-Build`. `-OutputRoot` substitui essa raiz para intermediários e pacote final.

| Caminho relativo à raiz de saída | Finalidade |
| --- | --- |
| `bin/` e `obj/` | Resultados intermediários, usados pelo SDK |
| `.staging/` | Pacote temporário e cópia anterior durante a troca; limpos ao finalizar normalmente |
| `publish/CubeControl-1.1.0/` | Único pacote pronto da edição normal |
| `publish/CubeControl-Simulation-1.1.0/` | Pacote pronto da edição simulada, quando solicitada |

Não há cópia para `builds/` dentro do projeto, pasta permanente `-dev-build` ou distribuição para pastas datadas. A simulação é outra edição do aplicativo gerada pelo mesmo script, não outra forma de build.

O script usa mutex, compara os hashes das fontes antes/depois da compilação, confere versão da DLL, arquivos essenciais, imagens, WAVs, traduções, manual e documentos copiados. O pacote inclui `build-info.json` com versão, configuração, data UTC e hashes SHA-256 das fontes e arquivos; o manifesto não inclui seu próprio hash.

O pacote anterior é preservado durante a compilação e verificação. Somente depois é substituído por movimentação de pastas no mesmo volume; falha na instalação tenta restaurá-lo. Uma interrupção abrupta do processo pode deixar temporários em `.staging/`. Não há sincronização de outra árvore de fontes nem escrita no pedal durante o build.

[Directory.Build.props](Directory.Build.props) define a saída externa padrão. O script passa a raiz selecionada ao SDK. [Directory.Build.targets](Directory.Build.targets) bloqueia publicação direta sem a propriedade interna fornecida pelo script.

## Simulação

Use `-Simulation` no mesmo script. Ele inclui `FakeConnection.cs` e define `CUBE_SIMULATION` por meio de `CubeSimulation=true`. Os limites e os dados locais estão no [guia de simulação](../docs/historico/simulacao-temporaria.md).

## Testes existentes

Os comandos abaixo executam suítes de teste; não geram pacote de distribuição. Os testes são **programas executáveis**, acionados por `dotnet run`; não são suítes descobertas por `dotnet test`.

O modo desktop `--firmware-worker-only` testa a comunicação com o auxiliar elevado e pode solicitar autorização do Windows. Usa uma extensão inválida, garantindo rejeição antes de enumerar ou acessar o pedal; não grava firmware.

```powershell
dotnet run --project .\CubeControl-1.1\Cube\windows\Cube.Tests -c Release
dotnet run --project .\CubeControl-1.1\Cube\windows\Cube.Desktop.Tests -c Release -- --responsive-only
```

`Cube.Tests` exercita codecs, perfil, sessão, fila e operações com conexão falsa. `Cube.Desktop.Tests` usa WPF; a maioria dos modos usa transporte simulado, mas os modos `--hardware-*` abaixo acessam o pedal real. Alguns modos abrem janelas e produzem capturas locais. Escolha um modo por execução: o despacho usa precedência fixa, não combina suítes quando recebe vários filtros.

Filtros de `Cube.Tests`, passados após `--`: `--communication-only`, `--ir-packs-only`, `--effects-firmware-b-only`, `--delay-only`, `--reverb-only`, `--chorus-preview-only`, `--chorus-only` e `--phaser-only`. Por exemplo:

```powershell
dotnet run --project .\CubeControl-1.1\Cube\windows\Cube.Tests -c Release -- --delay-only
```

A execução sem filtro não inclui todos esses ramos: executa a validação de amps, phaser, verificações gerais, `ExtendedTests` e `MusicalTests`. Delay, reverb, chorus e firmware B têm entradas dedicadas. Uma exceção encerra a execução e impede as verificações seguintes. Os resultados e falhas anteriormente observados estão na [revisão documental](../docs/desenvolvimento/revisao-documental.md#atualização-verificada--12102026); a existência de um filtro não significa que ele passou nesta revisão.

O modo `--effects-firmware-b-only` está disponível nos dois projetos e testa os quatro criadores com o perfil de `cubebaby_B_20220706.up`. A [análise do firmware B](../../../engenharia-reversa/cube-baby/CUBE_BABY_PHASER.md#perfil-adicional-firmware-b-de-06072022) preserva a correspondência entre o arquivo, as leituras USB e os endereços usados pelo protocolo.

| Área | Argumentos disponíveis |
| --- | --- |
| Layout e zoom | `--responsive-only`, `--zoom-only`, `--zoom-compact-only`, `--zoom-dialogs-only`, `--density-only`, `--volume-only`, `--player-layout-only` |
| Idioma e aparência | `--localization-only`, `--theme-only`, `--options-menu-only`, `--ui-audit` |
| Interação | `--motion-only`, `--wheel-only`, `--knob-wheel-only`, `--microphone-only`, `--chain-only` |
| Equipamentos e feedback | `--carousel-only`, `--artwork-only`, `--buttons-only`, `--effects-only`, `--chevrons-only`, `--snackbar-only`, `--progress-only` |
| Sessão e biblioteca | `--auto-connect-only`, `--save-presets-only`, `--ir-library-only` (alias `--ir-catalog-only`), `--ir-library-ui` |
| YouTube | `--youtube-only` (busca/download simulados, ciclo de prévia e mosaico), `--youtube-preview-live` (rede, FFmpeg e saída WASAPI reais, com volume zero) |
| Outros | `--editor-only`, `--presentation-render`, `--scroll-profile` |

O despacho dos modos está em [Cube.Desktop.Tests/Program.cs](Cube.Desktop.Tests/Program.cs). Testes simulados não certificam áudio, USB físico ou persistência após desligar. `Cube.Diagnostics` acessa hardware e não integra a verificação offline da documentação.

### Exceções com pedal real nos testes desktop

`--hardware-advanced-lifecycle` e `--hardware-effect-cards` executam [AdvancedHardwareTests](Cube.Desktop.Tests/AdvancedHardwareTests.cs): abrem as portas de nome exato `USB-Midi`, leem o estado e alteram controles/prévias dos quatro efeitos. O segundo também alterna os cartões simples/avançados. Exigem CubeControl e CubeSuite fechados e aceitam automaticamente as perguntas do modelo de interface. Tentam fechar as prévias e restaurar o banco live em `finally`; falhas de comunicação podem impedir essa conclusão. As evidências ficam em `C:/CubeControl-Build/advanced-lifecycle/hardware-edit-<data UTC>`, incluindo `initial.json`, `transport.log` e, somente ao concluir a restauração verificada do banco, `restored.txt`.

`--hardware-advanced-recovery` também abre o pedal real, mas seu ramo está desatualizado: espera `%LOCALAPPDATA%/CubeControl/pending-chorus-preview.json` e procura por reflexão o método `LoadPendingChorus`, ausente no editor atual. Essa preparação ocorre antes do bloco `try/finally` de restauração. Não use esse modo como procedimento de recuperação; ele precisa ser corrigido no código. Sua presença no despacho não comprova suporte a recuperar prévias em disco. Consulte o [guia de recuperação atual](../docs/usuario/pt-BR/recuperacao.md).

## Diagnósticos e pesquisa com hardware

Referência conferida no código em 12/10/2026. `Cube.Diagnostics` é um executável de pesquisa, separado dos testes offline e do editor. Os argumentos são despachados em [Program.cs](Cube.Diagnostics/Program.cs). Sem argumentos, enumera e imprime as portas MIDI e encerra; não apresenta um catálogo de ajuda. Os comandos abaixo não foram executados nesta revisão.

Na raiz do repositório, o formato é `dotnet run --project CubeControl-1.1/Cube/windows/Cube.Diagnostics -c Release -- COMANDO ARGUMENTOS`. `PASTA-NOVA` representa um diretório ainda inexistente; escolha uma pasta externa ao checkout para as evidências. Os modos que recebem esse argumento recusam uma pasta já existente. Os caminhos de evidência são independentes da raiz de saída do build.

### Observação e consultas sem escrita de parâmetros

| Comando e argumentos | Comportamento e evidências |
| --- | --- |
| `probe-cube` | Identifica o pedal, lê e valida o banco; imprime identidade e banco em JSON |
| `observe-physical-inputs PASTA-NOVA SEGUNDOS` | Identifica uma vez e depois apenas recebe mensagens por 1–600 segundos; grava `transport.log` e `summary.json` |
| `observe-physical-state PASTA-NOVA SEGUNDOS` | Verifica firmware e consulta banco, DSP e estado físico por 1–600 segundos, com espera de um segundo entre capturas; grava `initial.json`, `sample-*.json`, log e resumo |
| `effects-firmware-b PASTA-NOVA` | Captura regiões de evidência, verifica os quatro efeitos, alias do banco e contexto; grava BINs, `dsp.bin`, `transport.log` e `verified.json` após sucesso |
| `compare-factory-irs PASTA-NOVA` | Lê os oito setores, exporta backups e compara com a referência do CubeSuite; grava `comparison.json` |
| `phaser-snapshot PASTA-NOVA` | Três capturas de DSP/banco, log e `report.json` com diferenças e hashes |
| `phaser-map PASTA-NOVA` | Mapeamento em `mapping.json`, evidência `code-before-dsp.bin` e log |
| `chorus-map PASTA-NOVA`, `reverb-map PASTA-NOVA`, `delay-map PASTA-NOVA` | Mapeamento em `mapping.json`, BINs das regiões consultadas e log |

Observação passiva e consulta periódica não são equivalentes: `observe-physical-inputs` não envia consultas durante a janela de observação, mas envia identificação antes dela. Seu contador representa mensagens brutas recebidas, não acionamentos físicos. Silêncio nessa janela não prova que um controle nunca transmite. `observe-physical-state` envia consultas e depende das assinaturas de firmware verificadas pelo leitor de pesquisa; seu período real inclui o tempo das leituras.

Os modos com `PASTA-NOVA` acima exigem CubeControl e CubeSuite fechados. A seleção de portas varia: observação física e `probe-cube` usam `CubeMidiPorts.Select`; os módulos de efeitos aceitam `USB-Midi` e o sufixo numérico entre colchetes, exigindo um par único; `compare-factory-irs` procura o nome exato `USB-Midi`. Este último também depende dos recursos reconhecidos em `Downloads/CubeSuite/CubeSuite.exe` no perfil do usuário.

### Ensaios com alteração do aparelho

| Comandos | Argumentos após o comando | Alcance |
| --- | --- | --- |
| `phaser-sweep`, `phaser-controls` | `PASTA-NOVA SLOT-FISICO-0-2` | Ensaio com escrita e restauração; o slot é zero-based e deve corresponder ao físico |
| `chorus-sweep`, `chorus-controls`, `chorus-transitions` | `PASTA-NOVA` | Alterações live/RAM, capturas e intenções em disco |
| `reverb-sweep`, `reverb-controls`, `reverb-transitions` | `PASTA-NOVA` | Alterações live/RAM, capturas e intenções em disco |
| `delay-sweep`, `delay-controls`, `delay-transitions`, `delay-modulation`, `delay-runtime` | `PASTA-NOVA` | Ensaios de delay com alterações live/RAM e registros; não tratar `runtime` como observação passiva |

Esses modos tentam restaurar o estado ao final, mas perda de conexão ou divergência de contexto pode impedir a confirmação. Os arquivos produzidos pelos diagnósticos são evidências de pesquisa em disco; não são snapshots que o editor carregará automaticamente ao reiniciar. Delay usa `CreateNew`, `WriteThrough`, flush em disco e releitura nos registros JSON dos ensaios; chorus/reverb e os callbacks do phaser usam escrita e releitura. Não atribua a todos os modos a mesma garantia de durabilidade.

Há outros ensaios em `Program.cs`: `verify-live`, `verify-live-queue` e `verify-musical` enviam alterações ao pedal; `verify-ir` também substitui o setor IR 8, e `verify-factory` grava presets e setores IR. Não são testes offline. `verify-ir` depende ainda do caminho relativo `.tools/cubecontrol/test/fixtures/ir/033-tonehub-impulse.wav`. Os modos antigos podem usar destinos relativos em `windows/artifacts` e não seguem todos a regra de pasta nova; confira o ramo específico antes de executá-los. Uma mensagem de sucesso de leitura ou ACK não substitui a confirmação final de restauração.

Fontes: [observação física](Cube.Diagnostics/PhysicalInputsResearch.cs), [phaser](Cube.Diagnostics/PhaserResearch.cs), [chorus](Cube.Diagnostics/ChorusResearch.cs), [reverb](Cube.Diagnostics/ReverbResearch.cs) e [delay](Cube.Diagnostics/DelayResearch.cs). Consulte também os [relatórios de engenharia reversa](../../../engenharia-reversa/cube-baby/README.md), que preservam os contextos dos ensaios anteriores.

## Recursos e manutenção

O modo `--youtube-preview-live` exige as ferramentas preparadas por `scripts/Prepare-YouTubeTools.ps1 -OutputRoot C:\CubeControl-Build` antes da compilação e uma saída de áudio disponível. Ele verifica início e cancelamento do stream real; a escuta audível continua sendo uma verificação manual.

[Arquitetura](../docs/desenvolvimento/arquitetura.md) descreve os seis projetos. O acervo de biblioteca contém 958 WAVs em 12/10/2026 (276 anteriores e 682 de Overdriven.fr), copiados pelo projeto ao pacote, além de manual PDF, idiomas, imagens e `FactoryAmps.bin` embutido. Não inclui modelos NAM nem exige checkout externo do WPF UI.

Este arquivo é copiado como `LEIA-ME.md` para o pacote; seus links de desenvolvimento pressupõem a árvore do repositório. Os avisos ficam em [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt). Dados e backups do usuário ficam fora da pasta de build.

Os antigos scripts de publicação separada foram removidos. As cópias anteriores em `builds/` e `*-dev-build` foram arquivadas uma única vez em `C:\CubeControl-Build\legacy\`; essa pasta não participa do novo fluxo.

### Personalização de imagens

`Personalizar imagem` abre o acervo de equipamentos na categoria atual. O rodapé mantém `Escolher do computador`, `Aplicar imagem` e `Cancelar` disponíveis durante a rolagem. A seleção é provisória até aplicar ou dar duplo clique; Escape e fechar cancelam. PNG/JPEG de até 20 MB são copiados para o armazenamento local apenas após confirmar. As imagens não alteram o som nem enviam comandos ao pedal.

O catálogo usa uma grade de três ou mais colunas em largura normal e se adapta a uma ou duas em janela compacta ou zoom elevado. O filtro picklist inclui Guitarras e Racks; setas navegam entre cartões. As linhas são virtualizadas e as miniaturas carregadas sob demanda. Além de `Assets/Images/Equipment`, o build distribui as alternativas de Cabeçotes, Caixas, Pedais, Guitarras, Dispositivos, Equipment e Racks do Archive em `Assets/Images/Catalog`, preservando as subpastas e excluindo controles. Os arquivos originais permanecem no Archive, que não acompanha o pacote. A publicação confere os hashes de todas as imagens distribuídas.

Verificação direcionada: `dotnet run --project .\CubeControl-1.1\Cube\windows\Cube.Desktop.Tests -c Release -- --image-picker-only`. As capturas ficam em `C:/CubeControl-Build/reports/gear-image-picker`.

## Central de documentação

Guias pt-BR/en-US têm fontes únicas em `../docs/usuario`. O projeto publica o catálogo e esses arquivos em `Assets/Documents/guias`, junto aos PDFs preservados em `Assets/Documents/manuais`. O build executa `scripts/Validate-Documentation.ps1 -PackageRoot` antes de aceitar o pacote.

Na raiz, use `python scripts/qa/validate_document_links.py` para links do acervo e `dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -c Release -- --documentation-only` para catálogo, leitura, busca, erros, idiomas e renderizações. As capturas ficam em `C:/CubeControl-Build/documentation-qa`. A central abre em janela não modal a partir de Dispositivo e ajuda e não depende do pedal.

### Recursos e cobertura das alterações recentes

As artes REC de BackingTrack e de cada acabamento são recursos WPF embutidos pelo projeto, não arquivos externos a copiar manualmente. `Build-CubeControl.ps1` exige sua presença antes de compilar. Os novos controles e serviços C# entram pelo SDK; o catálogo de 34 packs, traduções e guias mantêm o fluxo de publicação existente.

Após autorização para executar, os modos existentes cobrem as regressões: `--assets-only` confere PNGs embutidos e REC por acabamento; `--curated-packs-only` confere os 34 packs e grupos; `--player-layout-only` confere o mixer único e o botão externo à busca; `--documentation-only` confere a barra compartilhada; `--backing-practice-only` inclui a análise musical, vínculos vazios e a resposta temporal dos medidores. Não é necessário um novo comando de execução.

Essas alterações de cobertura foram revisadas estaticamente, sem execução nesta revisão. Validação de precisão em músicas reais, WASAPI, troca física de dispositivos, temas/DPI e gravação no pedal permanece separada.

### Sessões com pistas e gravação incremental

O modo `--backing-session-only` prepara cobertura para mistura sincronizada, offsets, mute/solo, velocidade/tom relativos, exportação, envelope limitado, migração da playlist e cálculo de inserção com filtros. `--backing-recording-only` mantém a cobertura do escritor WAV; gravação sincronizada exige entrada e saída. Os cenários de packs agora verificam feedback em adorner independente da borda.

Esses testes foram preparados, mas não executados nesta alteração, assim como nenhum build. Quando houver autorização, compilar pelo fluxo documentado e executar os modos pertinentes. A DLL nativa precisa ser reconstruída: os limites internos relativos passaram a 1/3–3× e ±12 semitons, enquanto os controles da sessão permanecem em 50–150% e ±6 semitons. Captura longa, latência, desconexão, renderização, alto contraste e interação por teclado exigem verificação posterior.
