# Aplicação Windows do cubeControl

Esta pasta contém a aplicação WPF para o Cube Baby de guitarra via USB-MIDI.

- [Guia de uso](Cube/docs/usuario/pt-BR/primeiros-passos.md).
- [Índice completo da documentação](Cube/docs/README.md).
- [Compilação, testes e publicação](Cube/windows/README.md).
- [Arquitetura e armazenamento](Cube/docs/desenvolvimento/arquitetura.md).
- [Visão do repositório e acervo de pesquisa](../README.md).

O código e os recursos ficam em `Cube/windows/`; a documentação do produto fica em `Cube/docs/`. Os inicializadores `Cube/Iniciar-CubeControl.cmd` e `Iniciar-Simulacao.cmd` chamam o mesmo script `scripts/Build-CubeControl.ps1` com `-Start`, usando também `-Simulation` no segundo. O script gera e verifica o pacote completo antes de abrir a edição correspondente.

O projeto declara **1.1.0**, versão da linha **1.1**. A pasta de fontes e os nomes de publicação estão alinhados; o script usa a versão declarada no projeto para nomear os pacotes.

O desenvolvimento requer Windows, SDK .NET 10 disponível como `dotnet` e Visual Studio Build Tools com MSVC x64 e Windows SDK para o DSP nativo de áudio; a primeira restauração pode acessar o NuGet. Saídas e intermediários ficam em `C:\CubeControl-Build`, ou na pasta externa escolhida com `-OutputRoot`; o build não grava no projeto. O acervo Android em `../engenharia-reversa/apk-cubesuite/` serve à pesquisa e não participa da compilação Windows.
