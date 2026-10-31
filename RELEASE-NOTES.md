# cubeControl 1.0.0

Notas atualizadas para o estado atual do projeto, mantendo a versão **1.0.0**.

## Aparência e personalização

- Sete temas adicionais: Grafite, Azul Meia-Noite, Floresta, Vinho, Âmbar Vintage, Areia e Gelo, além de Sistema, Claro e Escuro.
- Remoção das bordas decorativas nos sete temas novos. Cards, campos e demais superfícies se distinguem pelo contraste dos fundos, preservando os indicadores de foco.
- Temas Claro original do Windows, Escuro e Sistema preservados.
- Catálogo ampliado para 26 acabamentos independentes do tema: metálicos, retrô, times, Skeleton, Cartoon, Anime, Boho, Papercut, receivers, vitrolas e mini systems.
- Preferências de tema e acabamento persistidas entre sessões; alto contraste do Windows tem prioridade sobre as paletas personalizadas.

## Recursos da versão

- Configuração do Cube Baby por USB-MIDI, edição de presets, IRs e criadores de timbres já presentes na base.
- Backing Tracks com reprodução, prática e exportação de áudio.
- Manual offline em português e inglês incluído no pacote.
- Identificação do aplicativo e manifesto normalizada para 1.0.0; fontes organizadas em `CubeControl`, sem versão no caminho.

## Organização do projeto

- Materiais auxiliares e artefatos de verificação separados dos arquivos usados para executar e compilar o aplicativo.
- Código, recursos visuais, catálogos, firmwares, manuais e licenças necessários ao pacote preservados.
- README atualizado para a estrutura atual, sem referências às capturas de tela removidas da árvore ativa.

## Executar e compilar

Destino do pacote Windows x64: `C:\CubeControl-Build\publish\CubeControl-1.0.0`. Abra `CubeControl.exe`; use `--offline` para iniciar sem conexão automática. O manual está em `Assets/Documents/manual/index.html`.

Para compilar, instale SDK .NET 10, Visual Studio Build Tools com MSVC x64 e Windows SDK e execute `.\scripts\Build-CubeControl.ps1` na raiz. Consulte o [README](README.md) para os demais comandos.

As dependências, firmwares e formatos de dados mantêm suas versões próprias. As funções USB-MIDI e operações persistentes no pedal exigem validação com o dispositivo conectado. Recursos de recuperação que dependem de uma instalação externa do CubeSuite continuam sujeitos aos requisitos descritos no manual.
