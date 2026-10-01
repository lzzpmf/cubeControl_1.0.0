param(
    [switch]$Simulation,
    [ValidateSet('Build', 'Publicar', 'Ambos')]
    [string]$Etapa = 'Publicar'
)

$ErrorActionPreference = 'Stop'
$canonical = 'C:\CubeControl-1.0\Cube\windows'
$workspace = Join-Path $PSScriptRoot 'CubeControl-1.0\Cube\windows'
$configuration = if ($Simulation) { 'Simulation' } else { 'Release' }
$name = if ($Simulation) { 'CubeControl-Simulation-1.0.0' } else { 'CubeControl-1.0.0' }
$official = Join-Path 'C:\CubeControl-Build\publish' $name
$build = Join-Path 'C:\CubeControl-Build\publish' "$name-dev-build"
$buildComplete = Join-Path $build '.build-complete'
$mutex = [Threading.Mutex]::new($false, 'Local\CubeControlPublish')
$locked = $false

function Get-SourceHashes([string]$Root) {
    $hashes = @{}
    $files = @(Get-ChildItem -LiteralPath $Root -File | Where-Object Extension -in '.props', '.targets')
    foreach ($project in 'Cube.Desktop', 'Cube.Protocol', 'Cube.Usb') {
        $files += Get-ChildItem -LiteralPath (Join-Path $Root $project) -File -Recurse |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' -and $_.Extension -ne '.md' }
    }
    if ($Simulation) { $files += Get-Item -LiteralPath (Join-Path $Root 'Cube.Tests\FakeConnection.cs') }
    foreach ($file in $files) {
        $hashes[$file.FullName.Substring($Root.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    return $hashes
}

function Assert-SameHashes($Expected, $Actual, [string]$Message) {
    $different = @($Expected.Keys | Where-Object { $Expected[$_] -ne $Actual[$_] })
    $different += @($Actual.Keys | Where-Object { -not $Expected.ContainsKey($_) })
    if ($different.Count) { throw ($Message + "`n" + ($different -join "`n")) }
}

try {
    $locked = $mutex.WaitOne(0)
    if (-not $locked) { throw 'Outra publicacao do CubeControl esta em andamento.' }
    if ($Etapa -ne 'Build' -and (Get-Process -Name CubeControl -ErrorAction SilentlyContinue)) {
        throw 'Feche o CubeControl antes de publicar para atualizar todos os destinos.'
    }
    if ($Etapa -eq 'Ambos') {
        $sources = Get-SourceHashes $workspace
        $previous = Get-SourceHashes $canonical
        foreach ($relative in $sources.Keys) {
            if ($sources[$relative] -ne $previous[$relative]) {
                $destination = Join-Path $canonical $relative
                [System.IO.Directory]::CreateDirectory((Split-Path $destination)) | Out-Null
                Copy-Item -LiteralPath (Join-Path $workspace $relative) -Destination $destination -Force
            }
        }
        foreach ($relative in $previous.Keys) {
            if (-not $sources.ContainsKey($relative)) {
                Remove-Item -LiteralPath (Join-Path $canonical $relative) -Force
            }
        }
        Assert-SameHashes $sources (Get-SourceHashes $canonical) 'Falha ao sincronizar o codigo com o workspace.'
        Write-Host 'Codigo sincronizado: workspace -> C:\CubeControl-1.0\Cube\windows'
    }
    if ($Etapa -ne 'Publicar') {
        if (Test-Path -LiteralPath $buildComplete) { Remove-Item -LiteralPath $buildComplete -Force }
        $sources = Get-SourceHashes $workspace
        $publishArgs = @('publish', (Join-Path $workspace 'Cube.Desktop\Cube.Desktop.csproj'), '-c', $configuration,
            '-r', 'win-x64', '--self-contained', 'true', '-p:CubeUnifiedPublish=true', '-o', $build)
        if ($Simulation) { $publishArgs += '-p:CubeSimulation=true' }
        & dotnet @publishArgs
        if ($LASTEXITCODE -ne 0) { throw 'Falha no build do CubeControl; execute o build novamente antes de publicar.' }
        Assert-SameHashes $sources (Get-SourceHashes $workspace) 'O workspace mudou durante o build. Execute novamente.'
        foreach ($asset in Get-ChildItem -LiteralPath (Join-Path $workspace 'Cube.Desktop\Assets\Equipment') -Filter '*.png' -File) {
            $output = Join-Path $build ('Assets\Equipment\' + $asset.Name)
            if ((Get-FileHash -LiteralPath $asset.FullName).Hash -ne (Get-FileHash -LiteralPath $output).Hash) {
                throw "Imagem compilada diferente da fonte: $($asset.Name)"
            }
        }
        Set-Content -LiteralPath $buildComplete -Value (Get-FileHash -LiteralPath (Join-Path $build 'CubeControl.dll') -Algorithm SHA256).Hash
        Write-Host "Build concluido: $build"
        if ($Etapa -eq 'Build') { exit 0 }
    }
    if (-not (Test-Path -LiteralPath $buildComplete) -or -not (Test-Path -LiteralPath (Join-Path $build 'CubeControl.exe'))) {
        throw 'Build concluido nao encontrado. Execute Build-CubeControl.ps1 primeiro.'
    }
    if ((Get-Content -LiteralPath $buildComplete) -ne (Get-FileHash -LiteralPath (Join-Path $build 'CubeControl.dll') -Algorithm SHA256).Hash) {
        throw 'O build foi alterado. Execute Build-CubeControl.ps1 novamente.'
    }
    [System.IO.Directory]::CreateDirectory((Join-Path $PSScriptRoot 'builds')) | Out-Null
    $targets = if ($Simulation) {
        @(Join-Path $PSScriptRoot "builds\$name")
    } else {
        @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'builds') -Directory |
            Where-Object { $_.Name -match '^CubeControl-(\d{8}-\d{6}|1\.0\.0)$' } |
            Select-Object -ExpandProperty FullName)
    }
    if (-not $targets.Count) { $targets = @(Join-Path $PSScriptRoot "builds\$name") }
    $published = @{}
    foreach ($file in Get-ChildItem -LiteralPath $build -File -Recurse) {
        $published[$file.FullName.Substring($build.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    foreach ($target in (@($official) + $targets)) {
        $resolvedTarget = [System.IO.Path]::GetFullPath($target)
        $allowedRoot = if ($target -eq $official) { 'C:\CubeControl-Build\publish\' } else { [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'builds')) + '\' }
        if (-not $resolvedTarget.StartsWith($allowedRoot, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Destino fora do diretorio de publicacao: $target"
        }
        & robocopy $build $resolvedTarget /MIR /IS /IT /R:1 /W:1 /NFL /NDL /NJH /NJS /NP
        if ($LASTEXITCODE -ge 8) { throw "Falha ao sincronizar $target" }
        $actual = @{}
        foreach ($file in Get-ChildItem -LiteralPath $target -File -Recurse) {
            $actual[$file.FullName.Substring($target.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        }
        Assert-SameHashes $published $actual "Publicacao divergente em $target"
        Write-Host "Verificado: $target ($($actual.Count) arquivos identicos)"
    }
    Write-Host "Oficial: $official"
    Write-Host "CubeControl.dll SHA256: $($published['CubeControl.dll'])"
    exit 0
} catch {
    Write-Error $_ -ErrorAction Continue
    exit 1
} finally {
    if ($locked) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
