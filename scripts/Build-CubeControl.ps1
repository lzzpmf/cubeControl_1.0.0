param(
    [switch]$Simulation,
    [switch]$CompileOnly,
    [switch]$Start,
    [string]$OutputRoot = 'C:\CubeControl-Build'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$workspace = Join-Path $repositoryRoot 'CubeControl-1.1\Cube\windows'
$mutex = $null
$locked = $false
$staging = $null
$backup = $null
$installed = $false
$exitCode = 1

function Assert-ChildPath([string]$Path, [string]$Parent) {
    $resolved = [IO.Path]::GetFullPath($Path)
    $prefix = [IO.Path]::GetFullPath($Parent).TrimEnd('\', '/') + '\'
    if (-not $resolved.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Caminho fora do diretorio de saida: $Path"
    }
    if ((Test-Path -LiteralPath $resolved) -and
        ((Get-Item -LiteralPath $resolved -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "O destino nao pode ser um link ou junction: $Path"
    }
}

function Remove-BuildDirectory([string]$Path, [string]$Parent) {
    Assert-ChildPath $Path $Parent
    if (Test-Path -LiteralPath $Path) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Move-BuildDirectory([string]$Source, [string]$Destination) {
    Assert-ChildPath $Source $OutputRoot
    Assert-ChildPath $Destination $OutputRoot
    for ($attempt = 0; $attempt -lt 10; $attempt++) {
        try {
            [IO.Directory]::Move($Source, $Destination)
            return
        } catch [IO.IOException] {
            if ($attempt -eq 9) { throw }
        } catch [UnauthorizedAccessException] {
            if ($attempt -eq 9) { throw }
        }
        Start-Sleep -Milliseconds 500
    }
}

function Get-SourceHashes {
    $hashes = @{}
    $files = @(Get-ChildItem -LiteralPath $workspace -File |
        Where-Object { $_.Extension -in '.props', '.targets' -or $_.Name -in 'README.md', 'THIRD-PARTY-NOTICES.txt' })
    foreach ($projectName in 'Cube.Desktop', 'Cube.Protocol', 'Cube.Usb') {
        $files += Get-ChildItem -LiteralPath (Join-Path $workspace $projectName) -File -Recurse |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    }
    if ($Simulation) { $files += Get-Item -LiteralPath (Join-Path $workspace 'Cube.Tests\FakeConnection.cs') }
    foreach ($file in $files) {
        $hashes[$file.FullName.Substring($workspace.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    $guidesRoot = Join-Path (Split-Path -Parent $workspace) 'docs\usuario'
    foreach ($file in Get-ChildItem -LiteralPath $guidesRoot -File -Recurse) {
        $hashes['docs\usuario\' + $file.FullName.Substring($guidesRoot.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    $hashes['Validate-Documentation.ps1'] = (Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Validate-Documentation.ps1') -Algorithm SHA256).Hash
    foreach ($helper in 'Prepare-YouTubeTools.ps1', 'youtube-tools.json') { $hashes[$helper] = (Get-FileHash -LiteralPath (Join-Path $PSScriptRoot $helper) -Algorithm SHA256).Hash }
    $hashes['build-script'] = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash
    return $hashes
}

function Assert-SameHashes($Expected, $Actual) {
    $different = @($Expected.Keys | Where-Object { $Expected[$_] -ne $Actual[$_] })
    $different += @($Actual.Keys | Where-Object { -not $Expected.ContainsKey($_) })
    if ($different.Count) {
        throw ('As fontes mudaram durante o build. Execute novamente: ' + ($different -join ', '))
    }
}

try {
    if ([string]::IsNullOrWhiteSpace($OutputRoot) -or $OutputRoot -notmatch '^(?:[A-Za-z]:[\\/]|\\\\[^\\]+\\[^\\]+)') {
        throw 'OutputRoot deve ser um caminho absoluto fora do repositorio.'
    }
    $OutputRoot = [IO.Path]::GetFullPath($OutputRoot).TrimEnd('\', '/')
    $repoPrefix = $repositoryRoot.TrimEnd('\', '/') + '\'
    $outputPrefix = $OutputRoot + '\'
    if ($OutputRoot -eq $repositoryRoot -or
        $outputPrefix.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase) -or
        $repoPrefix.StartsWith($outputPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'OutputRoot deve ficar fora do repositorio e nao pode ser um diretorio ancestral dele.'
    }
    # Reject junctions in the output path, including existing ancestors.
    $ancestor = $OutputRoot
    while ($ancestor) {
        if ((Test-Path -LiteralPath $ancestor) -and
            ((Get-Item -LiteralPath $ancestor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "OutputRoot nao pode atravessar um link ou junction: $ancestor"
        }
        $ancestor = Split-Path -Parent $ancestor
    }
    foreach ($directory in 'bin', 'obj', 'publish', '.staging') {
        Assert-ChildPath (Join-Path $OutputRoot $directory) $OutputRoot
    }
    $projectPath = Join-Path $workspace 'Cube.Desktop\Cube.Desktop.csproj'
    [xml]$project = Get-Content -LiteralPath $projectPath
    $version = $project.SelectSingleNode('/Project/PropertyGroup/Version').InnerText
    if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Versao do projeto invalida.' }
    $configuration = if ($Simulation) { 'Simulation' } else { 'Release' }
    $name = if ($Simulation) { "CubeControl-Simulation-$version" } else { "CubeControl-$version" }
    $publishRoot = Join-Path $OutputRoot 'publish'
    $destination = Join-Path $publishRoot $name
    $executable = Join-Path $destination 'CubeControl.exe'
    Assert-ChildPath $destination $publishRoot
    $mutex = [Threading.Mutex]::new($false, 'Local\CubeControlBuild')
    $locked = $mutex.WaitOne(0)
    if (-not $locked) { throw 'Outro build do CubeControl esta em andamento.' }
    if ($CompileOnly) {
        if ($Start) { throw 'CompileOnly nao pode ser combinado com Start.' }
        # Compile managed/WPF/native code without publishing or running verification suites.
        $compileRoot = Join-Path $OutputRoot ('compile-' + [Guid]::NewGuid().ToString('N'))
        Assert-ChildPath $compileRoot $OutputRoot
        $compileArgs = @('build', $projectPath, '-c', $configuration, '-r', 'win-x64',
            '--artifacts-path', $compileRoot)
        if ($Simulation) { $compileArgs += '-p:CubeSimulation=true' }
        & dotnet @compileArgs
        if ($LASTEXITCODE -ne 0) { throw "Falha na compilacao: $compileRoot" }
        Write-Host "Compilacao concluida, sem testes ou publicacao: $compileRoot"
        $exitCode = 0
        exit $exitCode
    }
    if (Get-Process -Name CubeControl -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $executable }) {
        throw 'Feche esta edicao do CubeControl antes de reconstruir seu pacote.'
    }
    $token = [Guid]::NewGuid().ToString('N')
    $stagingRoot = Join-Path $OutputRoot '.staging'
    $staging = Join-Path $stagingRoot "$name-$token"
    $backup = Join-Path $stagingRoot "$name-previous-$token"
    Assert-ChildPath $staging $stagingRoot
    Assert-ChildPath $backup $stagingRoot
    [IO.Directory]::CreateDirectory($staging) | Out-Null
    # Wildcard WPF resources silently omit a missing file. Require REC for the
    # official dark artwork and every finish before starting tools or compilation.
    $controlsRoot = Join-Path $workspace 'Cube.Desktop\Assets\Images\Controls'
    $recordArtwork = @(Join-Path $controlsRoot 'BackingTrack\record.png')
    $recordArtwork += @(Get-ChildItem -LiteralPath (Join-Path $controlsRoot 'Finishes') -Directory |
        ForEach-Object { Join-Path $_.FullName 'record.png' })
    foreach ($recordFile in $recordArtwork) {
        if (-not (Test-Path -LiteralPath $recordFile -PathType Leaf)) {
            throw "Arte REC obrigatoria ausente: $recordFile"
        }
    }
    & (Join-Path $PSScriptRoot 'Prepare-YouTubeTools.ps1') -OutputRoot $OutputRoot
    $sources = Get-SourceHashes
    $publishArgs = @('publish', $projectPath, '-c', $configuration, '-r', 'win-x64',
        '--self-contained', 'true', '--artifacts-path', $OutputRoot,
        '-p:CubeUnifiedPublish=true', '-o', $staging)
    if ($Simulation) { $publishArgs += '-p:CubeSimulation=true' }
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilacao. O pacote anterior foi preservado.' }
    Assert-SameHashes $sources (Get-SourceHashes)
    foreach ($required in 'CubeControl.exe', 'CubeControl.dll', 'CubeControl.deps.json', 'CubeControl.runtimeconfig.json', 'coreclr.dll', 'CubeBackingDsp.dll', 'NAudio.Core.dll', 'NAudio.Wasapi.dll', 'Tools\YouTube\yt-dlp.exe', 'Tools\YouTube\ffmpeg.exe', 'Tools\YouTube\ffprobe.exe', 'Tools\YouTube\deno.exe', 'Tools\YouTube\versions.json') {
        if (-not (Test-Path -LiteralPath (Join-Path $staging $required) -PathType Leaf)) {
            throw "Arquivo obrigatorio ausente do pacote: $required"
        }
    }
    $assembly = [Reflection.AssemblyName]::GetAssemblyName((Join-Path $staging 'CubeControl.dll'))
    if ($assembly.Version -ne [version]"$version.0") { throw 'Versao da DLL diferente da versao do projeto.' }
    $desktop = Join-Path $workspace 'Cube.Desktop'
    $resources = @{}
    foreach ($folder in 'Assets\Images\Equipment', 'Assets\Images\Controls\Knobs', 'Assets\Images\Controls\Faders', 'Assets\ImpulseResponses', 'Localization') {
        foreach ($file in Get-ChildItem -LiteralPath (Join-Path $desktop $folder) -File -Recurse) {
            if (($folder.StartsWith('Assets\Images\') -and $file.Extension -in '.png', '.jpg', '.jpeg' -and $file.Name -notmatch '_oficial_') -or
                ($folder -eq 'Assets\ImpulseResponses' -and ($file.Extension -eq '.wav' -or $file.Name -in 'Overdriven.catalog.v1.json', 'readme.txt')) -or
                ($folder -eq 'Localization' -and ($file.Extension -eq '.json' -or $file.Name -eq 'README.md'))) {
                $resources[$file.FullName.Substring($desktop.Length + 1)] = $file.FullName
            }
        }
    }
    $catalogSource = Join-Path $desktop 'Assets\Archive\Images'
    foreach ($category in 'Cabeçotes', 'Caixas', 'Pedais', 'Guitarras', 'Dispositivos', 'Equipment', 'Racks') {
        foreach ($file in Get-ChildItem -LiteralPath (Join-Path $catalogSource $category) -File -Recurse) {
            if ($file.Extension -in '.png', '.jpg', '.jpeg' -and $file.BaseName -notmatch '^(Botão|Botao|Fader|footswitch|knob|switch|interruptor)') {
                $relative = 'Assets\Images\Catalog\' + $file.FullName.Substring($catalogSource.Length + 1)
                $resources[$relative] = $file.FullName
            }
        }
    }
    $resources['Assets\IrPacks.v1.json'] = Join-Path $desktop 'Assets\IrPacks.v1.json'
    & (Join-Path $PSScriptRoot 'Validate-Documentation.ps1') -PackageRoot $staging
    $resources['LEIA-ME.md'] = Join-Path $workspace 'README.md'
    $resources['THIRD-PARTY-NOTICES.txt'] = Join-Path $workspace 'THIRD-PARTY-NOTICES.txt'
    foreach ($relative in $resources.Keys) {
        if ((Get-FileHash -LiteralPath $resources[$relative] -Algorithm SHA256).Hash -ne
            (Get-FileHash -LiteralPath (Join-Path $staging $relative) -Algorithm SHA256).Hash) {
            throw "Recurso diferente da fonte: $relative"
        }
    }
    if (Test-Path -LiteralPath (Join-Path $staging 'Assets\Archive')) {
        throw 'O acervo de alternativas nao deve acompanhar o pacote.'
    }
    $package = @{}
    foreach ($file in Get-ChildItem -LiteralPath $staging -File -Recurse) {
        $package[$file.FullName.Substring($staging.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    $info = [ordered]@{ version = $version; configuration = $configuration; runtime = 'win-x64'
        builtAtUtc = [DateTime]::UtcNow.ToString('o'); sources = $sources; files = $package }
    $info | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $staging 'build-info.json') -Encoding UTF8
    [IO.Directory]::CreateDirectory($publishRoot) | Out-Null
    # Keep the previous package until the validated replacement is in place.
    if (Test-Path -LiteralPath $destination) { Move-BuildDirectory $destination $backup }
    try {
        Move-BuildDirectory $staging $destination
        $installed = $true
    } catch {
        if (Test-Path -LiteralPath $backup) { Move-BuildDirectory $backup $destination }
        throw
    }
    Write-Host "Build concluido e verificado: $destination"
    Write-Host "Executavel: $executable"
    if ($Start) { Start-Process -FilePath $executable -WorkingDirectory $destination }
    $exitCode = 0
} catch {
    Write-Error $_ -ErrorAction Continue
} finally {
    foreach ($temporary in @($staging) + $(if ($installed) { @($backup) } else { @() })) {
        if ($temporary) {
            try { Remove-BuildDirectory $temporary (Join-Path $OutputRoot '.staging') }
            catch { Write-Warning "Nao foi possivel limpar o temporario: $temporary. $($_.Exception.Message)" }
        }
    }
    if ($locked) { $mutex.ReleaseMutex() }
    if ($mutex) { $mutex.Dispose() }
}
exit $exitCode
