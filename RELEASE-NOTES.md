# cubeControl 1.0.0

Versão de lançamento preparada a partir do commit `79761e3` (`cubeControl_1.0.0`).

- Configuração do Cube Baby por USB-MIDI, edição de presets, IRs e criadores de timbres já presentes na base.
- Backing Tracks com reprodução, prática e exportação de áudio.
- Manual offline em português e inglês incluído no pacote.
- Identificação do aplicativo e manifesto normalizada para 1.0.0; fontes organizadas em `CubeControl`, sem versão no caminho.
- Branches `MAIN`, `DEV` e `MASTER` sincronizadas no commit de lançamento, com histórico anterior preservado em tags de arquivo.

## Executar e compilar

Destino do pacote Windows x64: `C:\CubeControl-Build\publish\CubeControl-1.0.0`. Abra `CubeControl.exe`; use `--offline` para iniciar sem conexão automática. O manual está em `Assets/Documents/manual/index.html`.

Para compilar, instale SDK .NET 10, Visual Studio Build Tools com MSVC x64 e Windows SDK e execute `.\scripts\Build-CubeControl.ps1` na raiz. Consulte o [README](README.md) para os demais comandos.

As dependências, firmwares e formatos de dados mantêm suas versões próprias. As funções USB-MIDI e operações persistentes no pedal exigem validação com o dispositivo conectado. Recursos de recuperação que dependem de uma instalação externa do CubeSuite continuam sujeitos aos requisitos descritos no manual.
