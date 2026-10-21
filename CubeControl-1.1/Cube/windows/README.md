# cubeControl — compilação e execução

## Requisitos

Windows, SDK .NET 10, Visual Studio Build Tools com MSVC x64 e Windows SDK. O processamento de áudio nativo usa Signalsmith Stretch e é compilado junto ao aplicativo.

## Compilar e abrir

Na raiz do repositório:

```powershell
.\scripts\Build-CubeControl.ps1
.\scripts\Build-CubeControl.ps1 -Start
.\scripts\Build-CubeControl.ps1 -Simulation -Start
```

`-CompileOnly` compila sem gerar o pacote. `-OutputRoot 'D:\CubeControl-Build'` permite escolher um destino externo. O pacote normal fica em `C:\CubeControl-Build\publish\CubeControl-1.1.0`; a simulação usa `CubeControl-Simulation-1.1.0`.

O fluxo prepara runtime .NET, DSP nativo, recursos, documentação e ferramentas do YouTube. A primeira execução pode precisar de acesso ao NuGet e aos downloads dessas ferramentas. A publicação direta é bloqueada para preservar a integridade do pacote.

Para abrir um pacote existente sem conexão automática, execute `CubeControl.exe --offline`.

## Estrutura

### Aplicativo WinUI 3

`Cube.WinUI` contém as cinco áreas nativas. Execute `./scripts/Build-CubeWinUI.ps1 -Start` na raiz. Saída: `C:\CubeControl-WinUI-Build\publish\CubeControl.WinUI`. O script aceita `-OutputRoot` externo e verifica os recursos XAML publicados. `-SkipYouTubeTools` permite compilar para desenvolvimento sem preparar as ferramentas online; esse pacote não tem busca/prévia/download do YouTube disponíveis sem os executáveis correspondentes.

Base: C#/.NET 10, Windows mínimo 10 build 19041, x64, Windows App SDK 2.5.1 estável, unpackaged e self-contained. Versão verificada em 21/10/2026 nos [canais Microsoft](https://learn.microsoft.com/windows/apps/windows-app-sdk/release-channels). `Cube.Usb` e `Cube.Protocol` são referenciados. Os ViewModels e serviços existentes são compilados como fontes compartilhadas, com adaptações condicionais `WINUI` para dispatcher, imagens, coleções e diálogos. Nenhuma janela ou controle WPF é hospedado. Os seletores de arquivo/pasta usam os diálogos comuns do Windows por um adaptador Windows Forms com HWND proprietário; o framework correspondente acompanha a publicação.

| Área | Recursos conectados ao WinUI |
| --- | --- |
| Pedaleira | Banco A/B/C, parâmetros, importação/exportação, salvamento, equipamentos, imagens, associações amp/cabinet, desfazer/histórico e prévias avançadas de chorus/phaser/delay/reverb. |
| IRs | Biblioteca e busca, leitura/prévia/gravação/restauração, audição, packs de oito slots com edição, importação/exportação e revisão antes da aplicação. |
| Backing tracks | Sessões v5, pistas/takes, transporte, A–B, marcadores, BPM/tom/afinação, gravação/monitoramento, mixagem, metrônomo, waveform, seleção/edição de clipes e exportação WAV. Biblioteca local e comandos do YouTube reutilizam os serviços existentes. |
| Criador de amps | Modelos locais, importação/exportação, leitura do pedal, controles agrupados, valores exatos/tabelas, prévia e gravação confirmada. |
| Dispositivo e ajuda | Portas MIDI, conexão/desconexão, firmware, restaurações, diagnóstico, manuais e tema sistema/claro/escuro. |

Uma única instância dos modelos vive durante a janela: mudar de área preserva rascunhos, seleção e reprodução. O rodapé mantém o transporte disponível. O fechamento protege packs/banco alterados, espera a inicialização, bloqueia operações em andamento e encerra áudio/prévias. O worker elevado de firmware continua usando validação e confirmação existentes.

O diretório padrão continua `%LOCALAPPDATA%\CubeControl`, com os mesmos formatos e backups de migração. `--offline` desativa conexão automática; `--data-directory C:\pasta-de-teste` isola sessões/packs nos testes. A interface usa controles nativos WinUI com recursos visuais próprios para equipamentos musicais e respeita animações desativadas. Clipes têm seleção visual/lista e edição numérica, incluindo posição, duração, fades e take; a manipulação gestual do editor WPF não é reproduzida integralmente.

Execute `./scripts/Test-CubeWinUI.ps1`: verifica cinco rotas, MIDI sem conectar, cria uma pista, altera nome/afinação por UI Automation, confere o JSON v5, carrega/edita um amp local, preserva um rascunho de pack entre áreas e testa cancelar/confirmar o fechamento. Os testes `tests/Cube.Persistence.Tests` e `tests/Cube.Export.Tests` validam a lógica compartilhada, incluindo dados antigos, takes/clipes, falhas e cancelamento. A revisão visual inicial foi feita na janela publicada.

Acessibilidade no host nativo: F6 alterna entre navegação, conteúdo e transporte; Shift+F6 percorre o sentido inverso. Diálogos preservam o foco de origem quando o controle continua disponível, respeitam o escopo modal e oferecem rolagem para formulários extensos. Listas recebem nomes acessíveis, os comandos dos packs identificam o slot, campos recebem contexto e os módulos possuem títulos semânticos. Estados de gravação, salvamento, erros e status emitem eventos de região dinâmica, sem anunciar o relógio a cada atualização. Os botões quebram texto em grades que adaptam o número de colunas à largura disponível. Os clipes usam o estilo nativo de botão, preservam o foco ao redesenhar e identificam pista silenciada sem depender de opacidade. A edição por lista e valores numéricos permanece disponível.

O teste UI Automation também cobre nomes das listas nas cinco áreas, F6/Shift+F6 e retorno do foco após cancelar um diálogo. Estes testes não certificam a experiência de fala do Narrator, contraste em todos os temas ou ampliação de texto/DPI. Orientação consultada: [navegação de foco e acessibilidade](https://learn.microsoft.com/windows/apps/develop/input/focus-navigation) e [interações por teclado](https://learn.microsoft.com/windows/apps/design/input/keyboard-interactions).

Desempenho do host (21/10/2026): atualizações consecutivas da linha do tempo são agrupadas no dispatcher; a geometria da waveform é reutilizada enquanto dados, viewport e largura permanecem iguais. O cursor continua independente. As grades de ações só refazem suas linhas ao cruzar o limite entre uma e duas colunas. A atualização de coleções filtradas usa um conjunto para testar remoções e evita buscas para itens que já estão na posição correta, preservando seleção e notificações incrementais. A verificação do modo diagnóstico é feita uma vez por processo.

`./scripts/Measure-CubeWinUI.ps1` executa uma medição reproduzível das cinco rotas, memória privada e CPU acumulada, em diretório temporário isolado, offline e sem diagnóstico. É uma sondagem UI Automation com polling de 100 ms; mede a disponibilidade do título da página, não tempo de frame nem conclusão da renderização. Na máquina de revisão (Windows 11 Enterprise 22631, i5-1345U), Release x64, dados vazios, uma passagem antes/depois registrou respectivamente: Pedaleira 148/154 ms; IRs 262/253 ms; Backing tracks 443/486 ms; amps 156/154 ms; dispositivo 143/161 ms; memória privada 203,7/204,5 MiB. CPU acumulada 891/391 ms. Uma amostra não permite atribuir melhorias ou regressões a essas diferenças. Não há alegação de redução comprovada da latência de navegação. O teste funcional completo continua sendo `Test-CubeWinUI.ps1`.

Ainda é necessário perfilar sessões grandes, gravação/reprodução prolongadas, latência de áudio e frames com WPR/WPA para estabelecer limites de capacidade. Fontes consultadas: [layout WinUI](https://learn.microsoft.com/windows/apps/develop/performance/optimize-xaml-layout) e [listas e virtualização](https://learn.microsoft.com/windows/apps/develop/performance/optimize-gridview-and-listview). Esta etapa remove trabalho redundante identificado no host; não certifica desempenho em qualquer carga ou hardware.

Validação ainda necessária: pedal físico (incluindo prévias e firmware), captura de áudio real, Narrator, alto contraste, DPI entre monitores e máquina limpa. O provisionamento dos binários do YouTube foi bloqueado por HTTP 403 no ambiente de revisão; não foi validado ao vivo. O catálogo e a seleção de idioma são compartilhados, com novos rótulos em inglês. Os acabamentos gráficos personalizados do WPF foram substituídos pelos controles Fluent; o aplicativo WPF permanece disponível durante a validação de hardware.

Fontes de implementação consultadas em 21/10/2026: [DispatcherQueue](https://learn.microsoft.com/windows/apps/develop/dispatcherqueue), [ContentDialog e XamlRoot](https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.contentdialog?view=windows-app-sdk-2.0) e [data binding WinUI](https://learn.microsoft.com/windows/apps/develop/data-binding/data-binding-in-depth).

- `Cube.Desktop`: interface WPF, áudio, recursos e transporte de simulação.
- `Cube.Usb`: comunicação MIDI pelas APIs do Windows.
- `Cube.Protocol`: protocolo, fila de edição, formatos e operações do pedal.
- `../docs/usuario`: guias publicados em português e inglês.
- `../docs/manual`: manual HTML offline.

O transporte simulado é compilado somente com `CubeSimulation=true`. Não envia comandos ao pedal real.

## Documentação e ferramentas de build

Atualize os guias nos dois idiomas e execute `node scripts/Build-Manual.mjs` na raiz após mudanças no conteúdo. `scripts/Validate-Documentation.ps1` verifica o catálogo e o manual e é chamado pelo build.

O próprio `Build-CubeControl.ps1` prepara as quatro ferramentas necessárias à busca, prévia e download de áudio: yt-dlp, Deno, FFmpeg e ffprobe. As versões, URLs e hashes SHA-256 ficam em `scripts/YouTubeTools.ps1`, compartilhado pelos builds WPF e WinUI. FFmpeg usa o pacote de versão 8.1.2 Essentials, com espelhos GitHub e gyan.dev, sem depender de compilações diárias com retenção curta.

As ferramentas prontas ficam em `youtube-tools` na raiz externa de saída. O build confere seus hashes e reutiliza o cache sem rede quando está íntegro. ZIPs e extrações são temporários e removidos ao terminar. Um único `versions.json` identifica as versões e valida o cache; executáveis e licenças acompanham o aplicativo em `Tools/YouTube`. Não é necessário instalar essas ferramentas no Windows. `-CompileOnly` reutiliza as mesmas pastas de intermediários, sem criar uma pasta adicional a cada execução.

Dados do usuário ficam em `%LOCALAPPDATA%\CubeControl`; a simulação usa `CubeControl-Simulation`, com algumas preferências compartilhadas. Presets em edição, prévias em RAM e gravação persistente são operações distintas.

Consulte [os guias](../docs/README.md) e [os avisos de terceiros](THIRD-PARTY-NOTICES.txt).


## Distribuição WinUI 3

Execute `powershell -NoProfile -File scripts/Package-CubeWinUI.ps1 -Offline` na raiz para produzir a edição offline. Sem `-Offline`, o comando provisiona e exige yt-dlp, Deno, FFmpeg e FFprobe. A indisponibilidade dessas ferramentas interrompe a edição completa; não é convertida silenciosamente em offline. A edição offline exclui ferramentas eventualmente presentes no cache.

A saída fica em `C:\CubeControl-WinUI-Build\distribution\<identificador>`, com ZIP x64 autocontido, SHA-256 externo, manifesto dos arquivos e instruções de instalação, atualização, backup e remoção. Cada execução publica em uma pasta nova para evitar arquivos obsoletos. O script confere recursos, runtime .NET, Microsoft.UI.Xaml e DSP, verifica os hashes, extrai o ZIP e executa `Test-CubeWinUI.ps1` sobre o executável extraído. Uma falha interrompe a rotina antes da mensagem de pacote validado.

Para usar, extraia toda a pasta e execute `CubeControl.WinUI.exe`. Não é necessário instalar o SDK de desenvolvimento. Os dados permanecem em `%LOCALAPPDATA%\CubeControl`; para atualizar, feche o app e extraia a versão nova em outra pasta, após fazer backup dos dados. A remoção da pasta do aplicativo preserva os dados. O manifesto pode ser conferido com `powershell -NoProfile -File Verify-CubeWinUI.ps1` dentro da pasta extraída.

O pacote é **não assinado**, sem instalador ou atualização automática. SHA-256 detecta alteração, não comprova identidade do editor. A validação local não equivale a teste em máquina limpa, e o mínimo declarado no projeto (build 19041 x64) não é uma certificação de todos os sistemas suportados. Assinatura com certificado de publicação e validação em máquina limpa permanecem pendentes. Nenhum pacote foi publicado externamente. Modelo adotado: [distribuição autocontida por ZIP/Xcopy da Microsoft](https://learn.microsoft.com/windows/apps/package-and-deploy/), consultado em 21/10/2026.


### Identidade visual do CubeControl em WinUI 3

A apresentação recupera os acabamentos do WPF mantendo o host WinUI nativo. A navegação superior compacta, o preset ativo, a conexão e o transporte permanecem disponíveis. Em janelas estreitas, conexão e transporte distribuem seus controles em duas linhas; as áreas excedentes ficam no menu de navegação.

A pedaleira apresenta a cadeia com seletores inspirados em footswitches e o painel do módulo selecionado. `Views/SignalChainView`, `Views/LibraryBrowser` e `Views/AmplifierRackView` organizam os componentes específicos. A biblioteca de IRs apresenta seleção e detalhes lado a lado quando há espaço. O editor de amps reúne carregamento/arquivos e painéis de parâmetros. Backing tracks destaca timeline, pistas e mixagem com faders; configuração e ajuda usam os mesmos recursos de superfícies e botões.

`Controls/InstrumentControl` implementa knob/fader com RangeValue e ScrollItem para UI Automation, arraste, deltas fracionários da roda, teclado, ajuste fino com Shift e restauração por duplo clique/Backspace. A entrada numérica permanece disponível. Atualizações do modelo apenas redesenham o controle; o evento de edição existe somente para entrada do usuário. Gestos usam o agrupamento existente dos modelos. As referências dos parâmetros do banco sobrevivem à navegação e são atualizadas ao estabelecer um banco limpo; amps usam sua restauração própria. Mixagem restaura os valores neutros.

O catálogo de acabamentos é compilado a partir da fonte do WPF. Knobs, faders, footswitches e teclas de transporte usam os assets existentes, normalizados uma vez por imagem e mantidos em cache. A opção Oficial acompanha o tema; preferências de `appearance-v1.json`, incluindo campos desconhecidos, são preservadas ao alterar tema/acabamento. A área de aparência oferece uma prévia interativa. Alto contraste usa desenho/cores do sistema. Animações de pressão de 120 ms e de abertura/seleção de 160 ms respeitam a preferência de animações do Windows; o valor do parâmetro é atualizado imediatamente.

A implementação acrescenta contratos de apresentação e recursos XAML. Protocolo, USB, formatos de persistência, plataforma e dependências permanecem os existentes. Referência da plataforma para a personalização: [templates de controles WinUI](https://learn.microsoft.com/en-us/windows/apps/develop/platform/xaml/xaml-control-templates), consultada em 21/10/2026. Projeto: .NET 10, Windows mínimo 19041, x64, Windows App SDK 2.5.1 declarado no projeto.

Verificação a partir da raiz do repositório:

```powershell
./scripts/Build-CubeWinUI.ps1 -SkipYouTubeTools
./scripts/Test-CubeWinUI.ps1
./scripts/Test-CubeWinUI.Identity.ps1
./scripts/Measure-CubeWinUI.ps1
./scripts/Package-CubeWinUI.ps1 -Offline
```

`Test-CubeWinUI.Identity.ps1` usa dados isolados e verifica RangeValue, limites, edição exata, teclado, roda de alta resolução, arraste, duplo clique, restauração do amp, recursos dos onze acabamentos, tema claro, persistência/reabertura e controles persistentes em larguras de 760/480 pixels. Produz screenshots e inventários JSON dos controles habilitados/desabilitados. Aceita `-Executable` e `-EvidenceDirectory`. A validação de pacote executa as duas suítes sobre a cópia extraída, com evidências fora do ZIP.

As medições descritas anteriormente são do layout anterior e não constituem comparação de desempenho desta identidade. Reprodução/gravação prolongada durante gestos, troca de preset e desconexão com hardware, Narrator, contraste e DPI de 100/150/200% exigem validação manual própria. Um build ou uma execução UI Automation não certifica esses cenários. As mesmas ações devem ser conferidas com animações desativadas e texto ampliado.


Validação local da identidade em 21/10/2026: Release x64, Windows 11 Enterprise 22631, i5-1345U, DPI 120 (125%), dados de teste isolados e modo offline. Build, integridade de 1.860 arquivos, regressão funcional e suíte de identidade passaram na cópia extraída do ZIP. A suíte de identidade incluiu ajuste fino por Shift e reabertura das preferências. Screenshots e inventários por área ficam na pasta `identity-evidence` da execução de distribuição.

Uma passagem de `Measure-CubeWinUI.ps1`, no mesmo computador e com dados vazios, registrou:

| Cenário | Host anterior | Identidade atual |
| --- | ---: | ---: |
| Pedaleira | 401 ms | 227 ms |
| IRs | 520 ms | 815 ms |
| Backing tracks | 861 ms | 957 ms |
| Criador de amps | 226 ms | 295 ms |
| Dispositivo e ajuda | 176 ms | 245 ms |
| Memória privada ao final | 204,5 MiB | 204,9 MiB |
| CPU acumulada | 3.188 ms | 2.156 ms |

O host atual foi medido primeiro. São amostras únicas com polling de 100 ms, sem controle de cache/carga do sistema, e medem disponibilidade do título por UI Automation. Não medem latência de áudio, frames ou fluidez de arraste. O aumento observado em IRs acompanha a apresentação de uma lista visível no lugar da seleção recolhida; as diferenças não isolam causas nem estabelecem ganho/regressão comprovados. As pendências manuais descritas acima permanecem.
