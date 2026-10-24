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

- `Cube.Desktop`: interface WPF, Studio para tocar e estudar em Backing Tracks, recursos e transporte de simulação do pedal.
- `Cube.Studio.Core`: prática não persistente, acompanhamento, tentativas independentes e histórico de trim/exclusão. O editor legado continua isolado para compatibilidade dos testes, sem acesso no fluxo novo.
- `Cube.Studio.Audio`: captura/reprodução ASIO e WASAPI, análise, mix e exportação.
- `Cube.Studio.Audio/Native`: wrapper C++ e fontes vendorizadas do Signalsmith Stretch/Linear; `Build.ps1` gera `CubeStudioDsp.dll` em `studio-native` na raiz externa de artefatos, em `%LOCALAPPDATA%\CubeControl-BuildCache\studio-native` nos builds diretos; a publicação usa intermediários temporários.
- `Cube.Usb`: comunicação MIDI pelas APIs do Windows.
- `Cube.Protocol`: protocolo, fila de edição, formatos e operações do pedal.
- `../docs/usuario`: guias publicados em português e inglês.
- `../docs/manual`: manual HTML offline.

O transporte simulado é compilado somente com `CubeSimulation=true`. Não envia comandos ao pedal real.

## Documentação e ferramentas de build

Atualize os guias nos dois idiomas e execute `node scripts/Build-Manual.mjs` na raiz após mudanças no conteúdo. `scripts/Validate-Documentation.ps1` verifica o catálogo e o manual e é chamado pelo build.

O próprio `Build-CubeControl.ps1` prepara as quatro ferramentas necessárias à busca, prévia e download de áudio: yt-dlp, Deno, FFmpeg e ffprobe. As versões, URLs e hashes SHA-256 ficam no script, sem manifesto ou script auxiliar. FFmpeg usa o pacote de versão 8.1.2 Essentials, com espelhos GitHub e gyan.dev, sem depender de compilações diárias com retenção curta.

As ferramentas são reutilizadas diretamente de `Tools/YouTube` de um pacote publicado, após conferir os hashes. Na primeira publicação, `youtube-tools` na raiz externa de saída funciona apenas como cache de preparação; ele é removido depois que o pacote é instalado com sucesso. O build confere seus hashes e reutiliza o cache sem rede quando está íntegro. ZIPs e extrações são temporários e removidos ao terminar. Um único `versions.json` identifica as versões e valida o cache; executáveis e licenças acompanham o aplicativo em `Tools/YouTube`. Não é necessário instalar essas ferramentas no Windows. `-CompileOnly` reutiliza as mesmas pastas de intermediários, sem criar uma pasta adicional a cada execução.

Dados do usuário ficam em `%LOCALAPPDATA%\CubeControl`; a simulação usa `CubeControl-Simulation`, com algumas preferências compartilhadas. Presets em edição, prévias em RAM e gravação persistente são operações distintas.

O Studio inicia vazio, com duas funções fixas e sem persistir projetos. Fontes importadas permanecem nos locais originais. Tentativas temporárias ficam em `PracticeRecordings/<execução>`; descarte confirmado limpa apenas arquivos pertencentes à execução, e falhas preservam áudio recuperável. Salvar áudio exporta a tentativa aparada, com ou sem backing, para WAV24 ou MP3. Velocidade explícita e metrônomo usam contratos independentes. O DSP nativo é copiado para a saída/publicação como `CubeStudioDsp.dll`, com as licenças em `Licenses`.

O manual offline acompanha o aplicativo em Assets/Documents/manual/index.html. Consulte tambem os avisos de terceiros em THIRD-PARTY-NOTICES.txt.

A publicação usa intermediários isolados em .staging e os remove ao terminar, inclusive em caso de falha. Não gera build-info.json nem símbolos PDB. O modo -CompileOnly mantém bin, obj e studio-native como saída de compilação. Builds diretos e a análise do editor usam por padrão %LOCALAPPDATA%\CubeControl-BuildCache; o script define explicitamente sua própria saída externa.
