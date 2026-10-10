# cubeControl — desenvolvimento Windows

[Documentação](../docs/README.md) · [Guia de uso](../docs/usuario/pt-BR/primeiros-passos.md) · Revisão: 01/10/2026

O código está em `CubeControl-1.1/Cube/windows/`. Existe um único fluxo de geração do aplicativo: [scripts/Build-CubeControl.ps1](../../../scripts/Build-CubeControl.ps1).

## Requisitos e versão

- Windows e SDK .NET 10 disponível como `dotnet`. Não há SDK privado nem `global.json` nesta árvore.
- Acesso ao NuGet na primeira restauração. Dependência de interface: WPF UI 4.3.0.
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

`Cube.Tests` exercita codecs, perfil, sessão, fila e operações com conexão falsa. `Cube.Desktop.Tests` usa WPF e transporte simulado; alguns modos abrem janelas e produzem capturas locais. Escolha um modo por execução:

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

## Recursos e manutenção

O modo `--youtube-preview-live` exige as ferramentas preparadas por `scripts/Prepare-YouTubeTools.ps1 -OutputRoot C:\CubeControl-Build` antes da compilação e uma saída de áudio disponível. Ele verifica início e cancelamento do stream real; a escuta audível continua sendo uma verificação manual.

[Arquitetura](../docs/desenvolvimento/arquitetura.md) descreve os seis projetos. O pacote inclui 276 WAVs de biblioteca, manual PDF, idiomas, imagens e `FactoryAmps.bin` embutido. Não inclui modelos NAM nem exige checkout externo do WPF UI.

Este arquivo é copiado como `LEIA-ME.md` para o pacote; seus links de desenvolvimento pressupõem a árvore do repositório. Os avisos ficam em [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt). Dados e backups do usuário ficam fora da pasta de build.

Os antigos scripts de publicação separada foram removidos. As cópias anteriores em `builds/` e `*-dev-build` foram arquivadas uma única vez em `C:\CubeControl-Build\legacy\`; essa pasta não participa do novo fluxo.

### Personalização de imagens

`Personalizar imagem` abre o acervo de equipamentos na categoria atual. O rodapé mantém `Escolher do computador`, `Aplicar imagem` e `Cancelar` disponíveis durante a rolagem. A seleção é provisória até aplicar ou dar duplo clique; Escape e fechar cancelam. PNG/JPEG de até 20 MB são copiados para o armazenamento local apenas após confirmar. As imagens não alteram o som nem enviam comandos ao pedal.

O catálogo usa uma grade de três ou mais colunas em largura normal e se adapta a uma ou duas em janela compacta ou zoom elevado. O filtro picklist inclui Guitarras e Racks; setas navegam entre cartões. As linhas são virtualizadas e as miniaturas carregadas sob demanda. Além de `Assets/Images/Equipment`, o build distribui as alternativas de Cabeçotes, Caixas, Pedais, Guitarras, Dispositivos, Equipment e Racks do Archive em `Assets/Images/Catalog`, preservando as subpastas e excluindo controles. Os arquivos originais permanecem no Archive, que não acompanha o pacote. A publicação confere os hashes de todas as imagens distribuídas.

Verificação direcionada: `dotnet run --project .\CubeControl-1.1\Cube\windows\Cube.Desktop.Tests -c Release -- --image-picker-only`. As capturas ficam em `C:/CubeControl-Build/reports/gear-image-picker`.

## Central de documentação

Guias pt-BR/en-US têm fontes únicas em `../docs/usuario`. O projeto publica o catálogo e esses arquivos em `Assets/Documents/guias`, junto aos PDFs preservados em `Assets/Documents/manuais`. O build executa `scripts/Validate-Documentation.ps1 -PackageRoot` antes de aceitar o pacote.

Na raiz, use `python scripts/qa/validate_document_links.py` para links do acervo e `dotnet run --project CubeControl-1.1/Cube/windows/Cube.Desktop.Tests -c Release -- --documentation-only` para catálogo, leitura, busca, erros, idiomas e renderizações. As capturas ficam em `C:/CubeControl-Build/documentation-qa`. A central abre em janela não modal a partir de Dispositivo e ajuda e não depende do pedal.
