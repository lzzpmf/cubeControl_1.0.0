# cubeControl 1.0.0

Aplicativo Windows para configurar o Cube Baby por USB-MIDI, criar timbres e praticar com Backing Tracks.

## Requisitos e compilação

Windows x64, SDK .NET 10, Visual Studio Build Tools com MSVC x64 e Windows SDK. A primeira compilação requer acesso ao NuGet e aos downloads das ferramentas de áudio.

Na raiz do repositório:

```powershell
.\scripts\Build-CubeControl.ps1 -CompileOnly
.\scripts\Build-CubeControl.ps1
.\scripts\Build-CubeControl.ps1 -Start
```

O pacote completo, com runtime .NET, DSP nativo, ferramentas de áudio, licenças e manual offline, fica em `C:\CubeControl-Build\publish\CubeControl-1.0.0`. Execute `CubeControl.exe --offline` para abrir sem conexão automática com o pedal. O usuário do pacote não precisa instalar o SDK.

## Estrutura e documentação

- `CubeControl/Cube/windows`: projetos do aplicativo e [instruções detalhadas de compilação](CubeControl/Cube/windows/README.md).
- `CubeControl/Cube/docs/usuario`: guias em português e inglês.
- `CubeControl/Cube/docs/manual`: manual HTML offline, gerado com `node scripts/Build-Manual.mjs`.
- `scripts/Validate-Documentation.ps1`: validação dos guias, catálogo e manual; aceita `-PackageRoot` para validar o pacote.
- [Notas da versão 1.0.0](RELEASE-NOTES.md).

## Branches e versão

`MAIN` é a branch padrão e representa a versão de lançamento; `DEV` recebe o desenvolvimento e `MASTER` é a referência de compatibilidade. Na normalização da versão 1.0.0, as três apontam para o mesmo commit, identificado pela tag `v1.0.0`.

O lançamento deriva de `79761e3` (`cubeControl_1.0.0`). Os estados anteriores estão preservados nas tags `archive/dev-before-1.0.0`, `archive/master-before-1.0.0` e `archive/Win-Ui-before-1.0.0`.

As pastas do código não incluem a versão. O nome do repositório no GitHub permanece `cubeControl_v1.1`.
