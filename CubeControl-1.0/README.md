# CubeControl 1.0

Aplicativo Windows para o Cube Baby de guitarra via USB-MIDI.

- Aplicativo: `Cube/Iniciar-CubeControl.cmd`.
- Simulador: `Iniciar-Simulacao.cmd`.
- Desenvolvimento, testes e publicação: [guia Windows](Cube/windows/README.md).
- Código e recursos: `Cube/windows/`.
- Protocolo, comportamento e referências técnicas: `Cube/docs/`.

A versão 1.0.0 é a base de continuidade do aplicativo. O histórico anterior permanece no Git do projeto original em `C:/Cube/Cube-Control`. O projeto Android de referência e os pacotes de desenvolvimento antigos não fazem parte da aplicação Windows.
Esta pasta é a cópia de trabalho para continuidade da versão 1.0.0. A publicação exige que o código e os recursos de produção estejam sincronizados com `C:\CubeControl-1.0`; divergências bloqueiam o processo e precisam ser corrigidas, pois o script não mescla o código-fonte automaticamente. Código, testes e recursos foram preservados; caches, versões antigas, projeto Android, modelos NAM e scripts de migração já executada não foram copiados. Os documentos técnicos podem conter referências históricas ao acervo original.

O desenvolvimento usa o SDK .NET 10 instalado no Windows, pelo comando `dotnet`. A primeira compilação pode precisar de acesso ao NuGet para restaurar pacotes. Compilações e intermediários ficam em `C:\CubeControl-Build`. Feche as instâncias do CubeControl antes de usar os inicializadores: eles executam `..\Publicar-CubeControl.ps1` para publicar, sincronizar os destinos oficial e build e verificar os hashes dos arquivos. Só depois de concluir sem erros abrem a versão correspondente em `C:\CubeControl-Build\publish`. Os comandos e destinos estão detalhados no [guia Windows](Cube/windows/README.md).
