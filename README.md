# cubeControl

Editor Windows para o **Cube Baby de guitarra**, com comunicação USB-MIDI. Permite editar presets A/B/C, trabalhar com gabinetes/IRs, criar variações de amps e reproduzir backing tracks pelo áudio do Windows.

## Comece aqui

- [Manual HTML offline](CubeControl-1.1/Cube/docs/manual/index.html): capítulos em português e inglês, busca integral, temas e impressão. No aplicativo: **Dispositivo e ajuda → Manual do cubeControl**.
- [Guia de uso](CubeControl-1.1/Cube/docs/usuario/pt-BR/primeiros-passos.md): conexão, edição, gravação e recuperação.
- [Documentação completa](CubeControl-1.1/Cube/docs/README.md): índice por assunto, incluindo pesquisa e referências.
- [Desenvolvimento e build](CubeControl-1.1/Cube/windows/README.md): requisitos, fluxo único, opções e destinos externos.
- [Arquitetura e dados locais](CubeControl-1.1/Cube/docs/desenvolvimento/arquitetura.md): projetos, fluxos e arquivos persistidos.
- [Revisão documental de 01/10/2026](CubeControl-1.1/Cube/docs/desenvolvimento/revisao-documental.md): verificações e divergências ainda presentes no código.

## Estado desta árvore

O código está em `CubeControl-1.1/Cube/windows/`. A versão do aplicativo é **1.1.0** (linha 1.1); o script único deriva os nomes dos pacotes dessa versão. Os inicializadores encontram esse script relativamente à própria pasta, e o build usa as fontes deste checkout. Consulte o guia de desenvolvimento para gerar o pacote completo.

A aplicação usa .NET 10, WPF e WPF UI 4.3.0. A edição de parâmetros é feita por USB-MIDI. Bluetooth aparece no fluxo de reprodução de áudio do Windows; não há controle Bluetooth de presets/efeitos implementado.

## Organização do repositório

| Local | Conteúdo |
| --- | --- |
| [CubeControl-1.1](CubeControl-1.1/README.md) | Aplicação Windows, testes, recursos e documentação do produto |
| [scripts/Build-CubeControl.ps1](scripts/Build-CubeControl.ps1) | Fluxo único de build e geração do pacote completo fora do projeto |
| [engenharia-reversa/](engenharia-reversa/README.md) | Índice do acervo de pesquisa, separado da aplicação |
| [engenharia-reversa/apk-cubesuite/](engenharia-reversa/apk-cubesuite/README.md) | Código e recursos extraídos do APK Android usado na investigação |
| [engenharia-reversa/cube-baby/](engenharia-reversa/cube-baby/README.md) | Relatórios de protocolo/amps e artefatos de recuperação e ensaios |

Na raiz ficam apenas este README, `.gitignore` e as pastas principais. Execute `./scripts/Build-CubeControl.ps1` para gerar o pacote completo em `C:\CubeControl-Build\publish\CubeControl-1.1.0`. Use `-Start` para abrir após o build, `-Simulation` para a edição simulada e `-OutputRoot` para escolher outra pasta externa. O build não cria arquivos nem pastas dentro do projeto.

O [README do jsoup](engenharia-reversa/apk-cubesuite/resources/META-INF/README.md) pertence a uma dependência do acervo Android e foi preservado como documento de terceiro.

## Limites de interpretação

Selecionar A/B/C no editor escolhe o preset **em edição**, sem comprovar qual footswitch está ativo. Prévia em RAM e gravação persistente são operações diferentes. Os ensaios documentados referem-se ao aparelho e às condições registrados; não certificam outros modelos ou firmwares. Consulte a [política de privacidade](CubeControl-1.1/Cube/docs/usuario/pt-BR/privacidade.md).
