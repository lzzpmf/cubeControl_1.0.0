# cubeControl

Aplicativo Windows para o Cube Baby de guitarra, com edição de presets via USB-MIDI, biblioteca de IRs, criadores de amps e efeitos e backing tracks com reprodução, gravação e edição de pistas.

O código de produção está em `CubeControl-1.1/Cube/windows/`. A versão é 1.1.0, em .NET 10 e WPF.

- [Compilação e execução](CubeControl-1.1/Cube/windows/README.md)
- [Documentação do aplicativo](CubeControl-1.1/Cube/docs/README.md)
- [Manual offline](CubeControl-1.1/Cube/docs/manual/index.html)

## Compilar

Execute `./scripts/Build-CubeControl.ps1`. Use `-Start` para abrir após a compilação, `-Simulation` para a edição simulada e `-OutputRoot` para escolher outra pasta externa. O destino padrão é `C:\CubeControl-Build`.

Os scripts restantes são dependências da compilação, preparação das ferramentas de áudio/YouTube e geração e validação do manual. Testes, diagnósticos, pesquisa e artefatos de desenvolvimento não fazem parte desta árvore.