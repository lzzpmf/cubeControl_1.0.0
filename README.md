# cubeControl

**Seu Cube Baby no computador: edite timbres, organize IRs e toque com acompanhamento.**

Aplicativo para Windows que reúne edição do pedal por USB-MIDI, criação de amps, biblioteca de respostas de impulso e um ambiente de prática com gravação de guitarra. Interface em português e inglês, com temas, acabamentos e zoom ajustáveis.

**Versão 1.0.0 · Windows x64 · C# / WPF / .NET 10 · DSP em C++**

[Primeiros passos](#primeiros-passos) · [Recursos](#conheça-os-recursos) · [Compilação](#compilar-a-partir-do-código) · [Documentação](#documentação) · [Contribuir](#contribuir)

## O que você pode fazer

| Área | Principais funções |
| --- | --- |
| **Pedaleira** | Editar presets A/B/C, ajustar efeitos e importar ou exportar bancos. |
| **IRs e gabinetes** | Explorar a biblioteca, analisar respostas, ouvir prévias e montar packs de oito slots. |
| **Backing Tracks** | Importar ou buscar acompanhamentos, alterar tom e velocidade, repetir trechos e gravar guitarra. |
| **Criador de amps** | Ajustar EQ e caráter de um modelo base, experimentar em RAM e salvar no pedal. |
| **Dispositivo e ajuda** | Consultar a conexão USB, abrir manuais e acessar recuperação e diagnóstico. |

## Conheça os recursos

### Aparência e personalização

- **Dez opções de tema:** Sistema, Claro, Escuro, Grafite, Azul Meia-Noite, Floresta, Vinho, Âmbar Vintage, Areia e Gelo.
- Os **sete temas adicionais não usam bordas decorativas** nos cards e demais superfícies; a separação visual vem do contraste entre fundos. Claro original do Windows, Escuro e Sistema mantêm seus estilos.
- **26 acabamentos de controles**, escolhidos independentemente do tema, incluindo estilos metálicos, retrô, times, Skeleton, Cartoon, Anime, Boho e Papercut, além de receivers, vitrolas e mini systems.
- Tema e acabamento são salvos entre sessões. O acabamento Oficial acompanha o tema, e o alto contraste do Windows tem prioridade sobre as paletas personalizadas.
- Zoom da interface de **50% a 200%**.

Essas melhorias fazem parte da **versão 1.0.0**. Veja as [notas da versão](RELEASE-NOTES.md).

### Pedaleira: construa seu timbre

Escolha um preset e ajuste o som em uma interface com knobs, chaves e referências visuais de amplificadores e gabinetes.

- Edição de **A, B e C**, volume geral e indicação de alterações pendentes.
- Controles de **Amp, Chorus, Phaser, Delay, Reverb e Gabinete**.
- Catálogo visual de amps e associação entre amplificador e gabinete.
- Importação e exportação do banco, releitura do aparelho e histórico de ajustes.
- Criadores de chorus, phaser, delay e reverb com parâmetros avançados e prévia em RAM nos firmwares reconhecidos.

Selecionar A/B/C no aplicativo define o destino da edição. Para ouvir o preset correspondente, selecione o mesmo footswitch no pedal. Os criadores avançados de efeitos trabalham em RAM; seus ajustes não são gravados na flash.

[Guia da Pedaleira](CubeControl/Cube/docs/usuario/pt-BR/pedaleira.md) · [Presets e arquivos](CubeControl/Cube/docs/usuario/pt-BR/presets.md) · [Criadores de efeitos](CubeControl/Cube/docs/usuario/pt-BR/criadores-efeitos.md)

### IRs: encontre o gabinete para seu som

Busque por marca, gabinete ou captação, carregue arquivos WAV e compare opções antes de substituir um slot do pedal.

- Gráficos do **impulso no tempo** e da **resposta em frequência**.
- Informações de captura, formato original e preparação para o aparelho.
- Prévia no pedal em RAM, separada da gravação no slot.
- Controle de distância, leitura e exportação de backup e restauração de IR.
- Pré-escuta de trechos de guitarra pelo pedal conectado.

#### Packs de IRs

Organize conjuntos de até **oito IRs** por estilo ou preferência. Arraste arquivos da biblioteca para os slots e troque as posições dos cartões para reorganizar o pack.

- Packs predefinidos e montagens pessoais.
- Ajustes individuais por slot e revisão antes da aplicação.
- Importação e exportação de packs portáteis `.cubeirpack.zip`, incluindo os WAVs.
- Slots vazios preservam o conteúdo correspondente do pedal ao aplicar o pack.

[Guia de IRs e gabinetes](CubeControl/Cube/docs/usuario/pt-BR/irs.md)

### Backing Tracks: pratique, grave e escute sua evolução

Um espaço de estudo com duas faixas: **Acompanhamento** e **Minha guitarra**. Importe um áudio local ou use a busca integrada de acompanhamentos no YouTube.

- Reprodução com waveforms, posicionamento do cursor e **loop A/B**.
- Transposição de **−12 a +12 semitons** e velocidade de **25% a 200%** para a backing.
- Metrônomo independente com BPM, TAP, compasso, timbre e volume.
- Volume, pan, mute e solo por faixa.
- Captura de guitarra com **ASIO ou WASAPI**, seleção de canais e opções de buffer.
- Tentativas independentes e edição das extremidades da gravação.
- Exportação de guitarra com backing ou somente guitarra em **WAV 24 bits** ou **MP3 320 kbps**.
- Miniplayer para acompanhar a prática ao navegar por outras áreas do app.

Tom e velocidade afetam a backing; a guitarra mantém o áudio original. O metrônomo não entra na exportação. O Studio funciona por sessão: exporte as tentativas que deseja guardar, pois não há salvamento ou restauração de projetos.

[Guia de Backing Tracks](CubeControl/Cube/docs/usuario/pt-BR/backing-tracks.md)

### Criador de amps: personalize a resposta do amplificador

Escolha um modelo base ou importe um BIN compatível e explore os ajustes com prévia no pedal conectado.

- Controles de ganho e compensação.
- **Equalização** com curvas dos filtros e resposta combinada.
- Ajustes de **corpo e textura**, incluindo médios, punch e suavidade.
- Ajustes de **caráter e definição**, ressonâncias e corpo dos graves.
- Perfis de **resposta e capacitores**, com intensidade ajustável.
- Importação e exportação do modelo, prévia em RAM e gravação no slot escolhido.

A curva de EQ representa a resposta calculada dos filtros, não uma medição do amp. Os perfis são propostas para escuta. O criador utiliza o formato de amp aceito pelo pedal; não importa modelos NAM.

[Guia do Criador de amps](CubeControl/Cube/docs/usuario/pt-BR/criador-amps.md)

### Dispositivo e ajuda: conexão, manuais e diagnóstico

Consulte a comunicação e encontre orientações sem sair do aplicativo.

- Detalhes da conexão USB e atalhos dos footswitches físicos.
- Manual HTML offline com busca, capítulos, impressão e idiomas português/inglês.
- Guias do aplicativo e manuais do pedal.
- Histórico de operações da sessão e registro de comunicação USB.
- Recuperação de presets e IRs originais, quando os recursos necessários estão disponíveis.
- Fluxo de reinstalação ou atualização de firmware compatível.

[Backups e recuperação](CubeControl/Cube/docs/usuario/pt-BR/recuperacao.md) · [Problemas comuns](CubeControl/Cube/docs/usuario/pt-BR/problemas.md)

## Primeiros passos

### Usar um pacote compilado

Quando distribuído como instalador, execute **`CubeControl-1.0.0-Setup-x64.exe`**. Ele inclui o runtime .NET e os recursos do aplicativo, instala para todos os usuários em `C:\Program Files\cubeControl` por padrão e solicita permissão de administrador, cria um atalho no menu Iniciar e oferece um atalho opcional na área de trabalho. A remoção é feita pelos aplicativos instalados do Windows. Não é necessário receber a pasta de build nem instalar o SDK .NET.

O instalador atende Windows x64, a partir do Windows 10 build 19041. Preferências, gravações e backups salvos fora da pasta de instalação são preservados na desinstalação. Drivers ASIO específicos da interface de áudio devem ser instalados separadamente quando utilizados.

Para a distribuição em pasta:

1. Extraia o **pacote completo** para uma pasta do computador.
2. Mantenha os recursos e subpastas junto de `CubeControl.exe`.
3. Conecte o Cube Baby de guitarra com um **cabo USB de dados**.
4. Feche outros editores que estejam usando a porta MIDI do pedal.
5. Abra `CubeControl.exe` e aguarde a identificação e leitura do aparelho.
6. Selecione o mesmo preset A/B/C no aplicativo e no pedal e comece a ajustar.

O pacote publicado inclui o runtime .NET; **não é necessário instalar o SDK para usar o aplicativo**. Para iniciar sem conexão automática:

```powershell
.\CubeControl.exe --offline
```

O pacote gerado pelo build fica em `C:\CubeControl-Build\publish\CubeControl-1.0.0`. O ZIP do código-fonte do GitHub não substitui esse pacote executável.

### Compatibilidade

| Item | Suporte |
| --- | --- |
| Sistema | Windows x64; alvo do projeto Windows 10 build 19041 ou posterior. |
| Pedal | Cube Baby de guitarra, por USB-MIDI. |
| Outros modelos | Não há perfil implementado para Cube Baby Bass, AC, Chocolate ou Blackbox. |
| Áudio de prática | Dispositivos WASAPI do Windows e interfaces com driver ASIO. |
| Idiomas | Português brasileiro e inglês. |
| Interface | Temas, acabamentos e zoom de 50% a 200%. |
| Internet | Necessária para busca, prévias e downloads online; biblioteca local e manual funcionam offline. |

Bluetooth não é utilizado para editar o pedal. Para gravar guitarra, configure a entrada de áudio e os canais da sua interface nas opções de Backing Tracks.

### Edição, prévia e gravação

| Ação | O que acontece |
| --- | --- |
| Editar um preset | Ajusta parâmetros disponíveis ao vivo; salvar o banco torna a edição persistente. |
| Prévia de IR ou amp | Permite experimentar na RAM do pedal, sem gravar automaticamente o slot. |
| Salvar presets | Grava o banco A/B/C. |
| Gravar IR ou salvar amp no pedal | Substitui o conteúdo do destino indicado. |
| Exportar | Guarda um arquivo no computador para reutilização ou backup. |

Exporte os backups que quiser conservar. As cópias automáticas em memória duram apenas a sessão. A restauração de fábrica depende de recursos externos do CubeSuite descritos no [guia de recuperação](CubeControl/Cube/docs/usuario/pt-BR/recuperacao.md); eles não acompanham o pacote do aplicativo.

## Documentação

| Conteúdo | Acesso |
| --- | --- |
| Primeira utilização | [Primeiros passos](CubeControl/Cube/docs/usuario/pt-BR/primeiros-passos.md) |
| Construção do timbre | [Pedaleira](CubeControl/Cube/docs/usuario/pt-BR/pedaleira.md) · [Presets](CubeControl/Cube/docs/usuario/pt-BR/presets.md) · [Criadores de efeitos](CubeControl/Cube/docs/usuario/pt-BR/criadores-efeitos.md) |
| Biblioteca e criação | [IRs](CubeControl/Cube/docs/usuario/pt-BR/irs.md) · [Criador de amps](CubeControl/Cube/docs/usuario/pt-BR/criador-amps.md) |
| Estudo e gravação | [Backing Tracks](CubeControl/Cube/docs/usuario/pt-BR/backing-tracks.md) |
| Suporte | [Recuperação](CubeControl/Cube/docs/usuario/pt-BR/recuperacao.md) · [Problemas comuns](CubeControl/Cube/docs/usuario/pt-BR/problemas.md) |
| English | [Getting started](CubeControl/Cube/docs/usuario/en-US/primeiros-passos.md) |
| Versão | [Notas da versão 1.0.0](RELEASE-NOTES.md) |

No pacote, abra `Assets/Documents/manual/index.html` no navegador para consultar o manual offline, ou use **Dispositivo e ajuda → Manual do cubeControl**. No GitHub, os guias Markdown acima permitem leitura direta.

## Compilar a partir do código

### Requisitos de desenvolvimento

- Windows x64.
- **SDK .NET 10**.
- **Visual Studio Build Tools**, com MSVC x64 e Windows SDK.
- **Node.js**, para regenerar o manual após alterações nos guias.
- Acesso ao NuGet e aos downloads das ferramentas na primeira preparação.

Execute na raiz do repositório:

```powershell
# Compilar em Release sem gerar o pacote
.\scripts\Build-CubeControl.ps1 -CompileOnly

# Gerar e verificar o pacote completo
.\scripts\Build-CubeControl.ps1

# Gerar o pacote e abrir o aplicativo
.\scripts\Build-CubeControl.ps1 -Start

# Escolher outro diretório externo de saída
.\scripts\Build-CubeControl.ps1 -OutputRoot 'D:\CubeControl-Build'
```

Use o script para publicar: ele prepara o runtime, compila o DSP nativo, inclui recursos, ferramentas e licenças e confere a integridade do pacote. A publicação direta do projeto é bloqueada para manter esse fluxo.

Para atualizar e validar a documentação:

```powershell
node scripts/Build-Manual.mjs
.\scripts\Validate-Documentation.ps1
.\scripts\Validate-Documentation.ps1 -PackageRoot 'C:\CubeControl-Build\publish\CubeControl-1.0.0'
```

Consulte as [instruções detalhadas de compilação](CubeControl/Cube/windows/README.md) para os diretórios de artefatos e a preparação das ferramentas.

### Organização do código

### Gerar o instalador EXE

Instale o [Inno Setup](https://jrsoftware.org/isdl.php) 6.3 ou superior no computador de desenvolvimento. Depois execute:

```powershell
# Publica o pacote completo e gera o instalador
.\scripts\Build-Installer.ps1

# Reutiliza um pacote completo já publicado e atualizado
.\scripts\Build-Installer.ps1 -SkipPublish

# Informa o compilador caso não seja localizado automaticamente
.\scripts\Build-Installer.ps1 -CompilerPath 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
```

Saída: `C:\CubeControl-Build\installer\CubeControl-1.0.0-Setup-x64.exe`, acompanhada de um arquivo SHA-256. Compartilhe apenas o `.exe` para instalar em outro PC. A versão é lida do projeto; o fluxo atual mantém **1.0.0**. O script não assina digitalmente o instalador.

### Organização do código

```text
CubeControl/Cube/
├── windows/
│   ├── Cube.Desktop/       # Interface WPF, recursos e integração
│   ├── Cube.Protocol/      # Protocolo, formatos e operações do pedal
│   ├── Cube.Usb/           # Comunicação MIDI no Windows
│   ├── Cube.Studio.Core/   # Estado e operações da prática
│   └── Cube.Studio.Audio/  # Captura, reprodução, exportação e DSP nativo
└── docs/
    ├── usuario/            # Guias em português e inglês
    └── manual/             # Manual HTML offline gerado
scripts/                    # Build, geração e validação documental
```

A aplicação usa **C# e WPF sobre .NET 10**, com **NAudio** para integração de áudio e **Signalsmith Stretch/Linear em C++** no DSP nativo. O build inclui yt-dlp, Deno, FFmpeg e ffprobe para os recursos online e de áudio.

## Privacidade e arquivos locais

O aplicativo não exige conta para editar o pedal. Preferências e dados locais ficam, em geral, em `%LOCALAPPDATA%\CubeControl`; backups e exportações são gravados nos destinos escolhidos pelo usuário. A busca e os downloads online se comunicam com serviços externos.

Consulte a [política de privacidade](CubeControl/Cube/docs/usuario/pt-BR/privacidade.md) para detalhes sobre armazenamento, temporários e conexões de rede.

## Contribuir

Para relatar um problema, inclua a versão do app, a versão do Windows, o modelo do pedal e os passos para reproduzir. Para falhas de áudio, informe o driver ASIO/WASAPI, a interface e a configuração utilizada. Prints e registros de comunicação ajudam no diagnóstico; remova informações pessoais antes de compartilhá-los.

Para mudanças no código, descreva o comportamento esperado e os testes realizados. Atualize os guias nos dois idiomas quando houver mudanças de uso e regenere o manual.

### Branches e lançamento

- **MAIN:** branch padrão e referência de lançamento.
- **DEV:** desenvolvimento.
- **MASTER:** referência de compatibilidade.
- **v1.0.0:** tag do lançamento 1.0.0, preparado a partir de `79761e3`.

Na normalização de 1.0.0, as três branches foram alinhadas ao mesmo commit. Os estados anteriores foram preservados nas tags `archive/dev-before-1.0.0`, `archive/master-before-1.0.0` e `archive/Win-Ui-before-1.0.0`.

## Créditos e componentes de terceiros

Desenvolvido por **Luiz Paulo Martins Ferreira**.

Consulte [THIRD-PARTY-NOTICES.txt](CubeControl/Cube/windows/THIRD-PARTY-NOTICES.txt) e as licenças incluídas no pacote para os componentes e recursos de terceiros. Marcas, nomes e imagens de equipamentos identificam suas referências e pertencem aos respectivos titulares.
