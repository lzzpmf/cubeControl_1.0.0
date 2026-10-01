# CubeControl 1.0

Editor WPF para Cube Baby de guitarra via USB-MIDI. Requer Windows; as publicações sincronizadas incluem o runtime .NET 10. Para desenvolver ou usar os inicializadores que compilam e publicam, use o SDK .NET 10 instalado no Windows, disponível pelo comando `dotnet`. Não é necessário um SDK dentro do projeto.

## Executar

Abra `Cube/Iniciar-CubeControl.cmd` para o pedal real ou `Iniciar-Simulacao.cmd` para o simulador, a partir da raiz de uma das cópias de `CubeControl-1.0`. Feche as instâncias do CubeControl antes de executar. Os inicializadores sempre chamam `Publicar-CubeControl.ps1` e só abrem o executável oficial após a publicação sincronizada terminar sem erros.

## Desenvolver e validar

Execute a partir de `Cube/`:

```powershell
dotnet build windows/Cube.Desktop/Cube.Desktop.csproj -c Release
dotnet run --project windows/Cube.Tests -c Release
dotnet run --project windows/Cube.Desktop.Tests -c Release -- --responsive-only
```

Os testes de interface também oferecem `--localization-only`, `--ir-library-only` e `--save-presets-only`. São testes simulados; não substituem validação de áudio e comunicação com o pedal físico.

## Compilar e publicar sem descasar versões

Execute os blocos abaixo no PowerShell. Os caminhos são absolutos; não dependem da pasta atual do terminal.

A publicação usa o projeto em `C:\CubeControl-1.0\Cube\windows`. A cópia em `C:\Users\lzzpm\Downloads\cubeSuiteReverse\CubeControl-1.0\Cube\windows` também contém o criador de amps, Backing Tracks e as imagens atualizadas. Mantenha as alterações de código e recursos correspondentes nas duas árvores: o script compara os arquivos de produção e bloqueia a publicação se houver divergências. Ele não mescla nem sincroniza o código-fonte automaticamente; os arquivos divergentes são listados para correção antes de tentar novamente.

### Apenas compilar, sem publicar

```powershell
dotnet build "C:\CubeControl-1.0\Cube\windows\Cube.Desktop\Cube.Desktop.csproj" -c Release
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar o CubeControl.' }
```

Compilar gera os artefatos em `C:\CubeControl-Build`, conforme `Directory.Build.props`. Isso não atualiza os executáveis publicados nem sincroniza os destinos. Para disponibilizar as alterações no aplicativo oficial e na build, execute a publicação abaixo.

### Publicar e sincronizar o aplicativo para o pedal real

Feche todas as instâncias do CubeControl, inclusive a simulação. Não altere os arquivos-fonte durante a publicação.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\lzzpm\Downloads\cubeSuiteReverse\Publicar-CubeControl.ps1"
if ($LASTEXITCODE -ne 0) { throw 'A publicação sincronizada não foi concluída. Corrija o erro e execute novamente.' }
```

O script compila uma vez em Release para Windows x64, com o runtime incluído, e distribui os mesmos arquivos:

- Oficial: `C:\CubeControl-Build\publish\CubeControl-1.0.0`.
- Build: atualiza as pastas existentes `cubeSuiteReverse\builds\CubeControl-<data-hora>` e `CubeControl-1.0.0`. Se nenhuma delas existir, cria `builds\CubeControl-1.0.0`.

As pastas com data são atualizadas no lugar; não são arquivos históricos imutáveis. O script não cria uma nova pasta datada a cada execução.

As imagens de equipamentos são sempre copiadas pelo projeto e comparadas com as fontes. Depois da distribuição, o script compara os nomes e hashes SHA-256 de todos os arquivos publicados em cada destino. Considere a publicação concluída somente quando terminar sem erros, mostrar `Verificado` para cada destino e informar o hash de `CubeControl.dll`. Uma falha após o início da publicação pode deixar destinos parcialmente atualizados; corrija o erro e execute novamente antes de usar os aplicativos.

Para abrir o aplicativo oficial já publicado, sem recompilar:

```powershell
& "C:\CubeControl-Build\publish\CubeControl-1.0.0\CubeControl.exe"
```

### Publicar e sincronizar o simulador (opcional)

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\lzzpm\Downloads\cubeSuiteReverse\Publicar-CubeControl.ps1" -Simulation
if ($LASTEXITCODE -ne 0) { throw 'A publicação sincronizada do simulador não foi concluída.' }
```

A simulação é publicada separadamente em `C:\CubeControl-Build\publish\CubeControl-Simulation-1.0.0` e sincronizada com `cubeSuiteReverse\builds\CubeControl-Simulation-1.0.0`. Esse comando não atualiza a publicação para o pedal real. Leitura, prévia e gravação de amps no dispositivo ficam desabilitadas no simulador.

### Regras de publicação

Use sempre `Publicar-CubeControl.ps1` ou os inicializadores. Publicações diretas de `Cube.Desktop` com `dotnet publish` são bloqueadas por `Directory.Build.targets`; não contorne o bloqueio com `CubeUnifiedPublish`, pois essa propriedade é usada internamente pelo script. Não publique separadamente nem copie somente o EXE: mantenha a pasta completa, incluindo bibliotecas e recursos.

A versão interna continua definida em `Cube.Desktop/Cube.Desktop.csproj`; a igualdade das publicações é verificada pelos hashes, não pelo nome da pasta ou pela data. O script de publicação não inicia o aplicativo nem grava dados no pedal. Somente os inicializadores abrem o aplicativo após a sincronização.

## Estrutura

- `Cube.Desktop`: interface, traduções, manual e biblioteca de IRs.
- `Cube.Protocol` e `Cube.Usb`: protocolo e transporte USB-MIDI.
- `Cube.Tests` e `Cube.Desktop.Tests`: regressões e conexão simulada.
- `Cube.Diagnostics`: ferramentas de diagnóstico do pedal.
- `../docs`: comportamento, referências e registros técnicos do protocolo.
- `THIRD-PARTY-NOTICES.txt`: avisos de terceiros.

A biblioteca incluída funciona offline. WAVs externos continuam disponíveis; prévia em RAM e gravação persistente são operações distintas. Esta base inclui os 276 WAVs utilizados pelo aplicativo. Modelos NAM, arquivos de origem e registros de migração permanecem no projeto original. Backups e preferências do usuário ficam fora da pasta de publicação.

O WPF UI é restaurado pelo NuGet (`WPF-UI` 4.3.0). Não há dependência do checkout externo da biblioteca.
### Associação automática de gabinetes

No painel do amplificador, **Associar gabinete** aplica ao preset atual o gabinete definido em **Associações**. O interruptor **Associação automática** começa desligado e sua escolha é salva nas preferências locais. Quando ligado, aplica a associação ao amplificador atual se a edição estiver disponível e a cada amplificador escolhido no aplicativo. Sem associação válida, o gabinete atual é preservado. Desligar mantém o gabinete selecionado e permite continuar usando o bot?o manual. A seção IR mantém seu estado ligado/desligado; a gravação permanente continua sendo feita por **Salvar presets**.
