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
$buildArtifacts = $null
$youTubeTools = $null
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

# Keep pinned tool releases in the build itself; daily autobuild URLs expire.
function Prepare-YouTubeTools {
    $assets = @(
        @{ name = 'yt-dlp'; file = 'yt-dlp-2026.08.19.exe'; target = 'yt-dlp.exe'
           urls = @('https://github.com/yt-dlp/yt-dlp/releases/download/2026.08.19/yt-dlp.exe')
           sha256 = '66674953fe251b89f4d08c5f0e35e0728679bd67ab3d7d05c0562af101dd3e7a' },
        @{ name = 'deno'; file = 'deno-2.9.7.zip'; executables = @('deno.exe')
           urls = @('https://github.com/denoland/deno/releases/download/v2.9.7/deno-x86_64-pc-windows-msvc.zip')
           sha256 = 'a0c3101b4158d1dfb7d6a78a7bf0f3de80c96bb423c152beec8beb22786f2238' },
        @{ name = 'ffmpeg'; file = 'ffmpeg-8.1.2-essentials_build.zip'; executables = @('ffmpeg.exe', 'ffprobe.exe')
           urls = @('https://github.com/GyanD/codexffmpeg/releases/download/8.1.2/ffmpeg-8.1.2-essentials_build.zip',
                    'https://www.gyan.dev/ffmpeg/builds/packages/ffmpeg-8.1.2-essentials_build.zip')
           sha256 = 'db580001caa24ac104c8cb856cd113a87b0a443f7bdf47d8c12b1d740584a2ec' },
        @{ name = 'yt-dlp-license'; file = 'yt-dlp-LICENSE.txt'; target = 'yt-dlp-LICENSE.txt'
           urls = @('https://raw.githubusercontent.com/yt-dlp/yt-dlp/2026.08.19/LICENSE')
           sha256 = '7e12e5df4bae12cb21581ba157ced20e1986a0508dd10d0e8a4ab9a4cf94e85c' },
        @{ name = 'deno-license'; file = 'deno-LICENSE.txt'; target = 'deno-LICENSE.txt'
           urls = @('https://raw.githubusercontent.com/denoland/deno/v2.9.7/LICENSE.md')
           sha256 = 'f62497fffecc0852960c8d3e6934b9db86d16396e9b604072e923892cae3a588' }
    )
    # Hashtable ordering is not stable across PowerShell processes.
    $fingerprint = ($assets | ForEach-Object { $_.name; $_.file; $_.target; $_.executables; $_.urls; $_.sha256 }) -join "`n"
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $manifestHash = [BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($fingerprint))).Replace('-', '') }
    finally { $sha.Dispose() }
    $tools = Join-Path $OutputRoot 'youtube-tools'
    Assert-ChildPath $tools $OutputRoot
    $required = @('yt-dlp.exe', 'deno.exe', 'ffmpeg.exe', 'ffprobe.exe', 'yt-dlp-LICENSE.txt', 'deno-LICENSE.txt')
    # Reuse the installed package instead of retaining another copy of every tool.
    $candidates = @((Join-Path $destination 'Tools\YouTube'),
        (Join-Path $publishRoot "CubeControl-$version\Tools\YouTube"),
        (Join-Path $publishRoot "CubeControl-Simulation-$version\Tools\YouTube"), $tools) | Select-Object -Unique
    foreach ($candidate in $candidates) {
        Assert-ChildPath $candidate $OutputRoot
        $statePath = Join-Path $candidate 'versions.json'
        if (!(Test-Path -LiteralPath $statePath -PathType Leaf)) { continue }
        try {
            $state = Get-Content -LiteralPath $statePath -Raw -Encoding UTF8 | ConvertFrom-Json
            $valid = $state.manifestHash -eq $manifestHash -and @($state.files).Count -ge $required.Count
            foreach ($name in $required) { if ($name -notin $state.files.path) { $valid = $false } }
            if (@($state.files.path | Where-Object { $_ -like 'Licenses\ffmpeg\*' }).Count -eq 0) { $valid = $false }
            foreach ($file in $state.files) {
                $cached = Join-Path $candidate $file.path
                Assert-ChildPath $cached $candidate
                if (!(Test-Path -LiteralPath $cached -PathType Leaf) -or
                    (Get-FileHash -LiteralPath $cached -Algorithm SHA256).Hash -ne $file.sha256) { $valid = $false; break }
            }
            if (@(Get-ChildItem -LiteralPath $candidate -Recurse -File).Count -ne @($state.files).Count + 1) { $valid = $false }
            if ($valid) {
                $script:youTubeTools = $candidate
                Write-Host "YouTube tools reused from verified files: $candidate"
                return
            }
        } catch { Write-Host 'YouTube tool cache needs verification and preparation.' }
    }
    $temporary = Join-Path $OutputRoot ('.staging\youtube-tools-' + [Guid]::NewGuid().ToString('N'))
    $previous = Join-Path $OutputRoot ('.staging\youtube-previous-' + [Guid]::NewGuid().ToString('N'))
    Assert-ChildPath $temporary $OutputRoot
    Assert-ChildPath $previous $OutputRoot
    $prepared = Join-Path $temporary 'ready'
    [IO.Directory]::CreateDirectory($prepared) | Out-Null
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    Add-Type -AssemblyName System.Net.Http
    $client = New-Object Net.Http.HttpClient
    $client.DefaultRequestHeaders.UserAgent.ParseAdd('CubeControl-Build/1.1')
    try {
        foreach ($asset in $assets) {
            $download = Join-Path $temporary $asset.file
            # Reuse verified downloads left by the old preparation script during migration.
            $legacy = Join-Path $tools $asset.file
            if ((Test-Path -LiteralPath $legacy -PathType Leaf) -and
                (Get-FileHash -LiteralPath $legacy -Algorithm SHA256).Hash -eq $asset.sha256) {
                Copy-Item -LiteralPath $legacy -Destination $download
            } else {
                $failures = @()
                $downloaded = $false
                foreach ($url in $asset.urls) {
                    $response = $null; $stream = $null
                    $deadline = New-Object Threading.CancellationTokenSource
                    $deadline.CancelAfter(300000)
                    try {
                        Write-Host "Downloading YouTube tool: $($asset.file)"
                        $response = $client.GetAsync($url, [Net.Http.HttpCompletionOption]::ResponseHeadersRead, $deadline.Token).GetAwaiter().GetResult()
                        $null = $response.EnsureSuccessStatusCode()
                        $stream = [IO.File]::Create($download)
                        $inputStream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
                        try { $null = $inputStream.CopyToAsync($stream, 81920, $deadline.Token).GetAwaiter().GetResult() }
                        finally { $inputStream.Dispose() }
                        $stream.Dispose(); $stream = $null
                        if ((Get-FileHash -LiteralPath $download -Algorithm SHA256).Hash -ne $asset.sha256) { throw 'SHA256 mismatch' }
                        $downloaded = $true
                        break
                    } catch {
                        $failures += "$url : $($_.Exception.GetBaseException().Message)"
                        Write-Warning "Download failed for $($asset.name): $($_.Exception.GetBaseException().Message)"
                    } finally {
                        if ($stream) { $stream.Dispose() }
                        if ($response) { $response.Dispose() }
                        $deadline.Dispose()
                    }
                }
                if (!$downloaded) { throw "Could not prepare $($asset.name). $($failures -join ' | ')" }
            }
            if ($asset.file.EndsWith('.zip')) {
                # Extract only executables/notices, without a persistent expanded archive.
                $zip = [IO.Compression.ZipFile]::OpenRead($download)
                try {
                    foreach ($exe in $asset.executables) {
                        $entry = @($zip.Entries | Where-Object Name -eq $exe)
                        if ($entry.Count -ne 1) { throw "Executable missing or ambiguous in $($asset.file): $exe" }
                        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry[0], (Join-Path $prepared $exe))
                    }
                    $notices = @($zip.Entries | Where-Object { $_.Name -match '^(LICENSE|COPYING|README)' })
                    if ($asset.name -eq 'ffmpeg' -and @($notices | Where-Object Name -match '^(LICENSE|COPYING)').Count -eq 0) { throw 'FFmpeg license missing from archive.' }
                    foreach ($entry in $notices) {
                        $noticeRoot = Join-Path $prepared ('Licenses\' + $asset.name)
                        $notice = Join-Path $noticeRoot $entry.FullName.Replace('/', '\')
                        Assert-ChildPath $notice $noticeRoot
                        [IO.Directory]::CreateDirectory((Split-Path -Parent $notice)) | Out-Null
                        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $notice)
                    }
                } finally { $zip.Dispose() }
            } else { Copy-Item -LiteralPath $download -Destination (Join-Path $prepared $asset.target) }
            Remove-Item -LiteralPath $download -Force
        }
        $files = @(Get-ChildItem -LiteralPath $prepared -Recurse -File | ForEach-Object {
            [ordered]@{ path = $_.FullName.Substring($prepared.Length + 1); sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
        })
        # One file serves as packaged version information and verified cache state.
        [ordered]@{ manifestHash = $manifestHash; assets = $assets; files = $files } | ConvertTo-Json -Depth 7 |
            Set-Content -LiteralPath (Join-Path $prepared 'versions.json') -Encoding UTF8
        if (Test-Path -LiteralPath $tools) { Move-BuildDirectory $tools $previous }
        try { Move-BuildDirectory $prepared $tools }
        catch {
            if (Test-Path -LiteralPath $previous) { Move-BuildDirectory $previous $tools }
            throw
        }
        Remove-BuildDirectory $previous $OutputRoot
        $script:youTubeTools = $tools
        Write-Host "YouTube tools verified: $tools"
    } finally {
        $client.Dispose()
        Remove-BuildDirectory $temporary $OutputRoot
    }
}

function Get-SourceHashes {
    $hashes = @{}
    $files = @(Get-ChildItem -LiteralPath $workspace -File |
        Where-Object { $_.Extension -in '.props', '.targets' -or $_.Name -in 'README.md', 'THIRD-PARTY-NOTICES.txt' })
    foreach ($projectName in 'Cube.Desktop', 'Cube.Protocol', 'Cube.Usb', 'Cube.Studio.Core', 'Cube.Studio.Audio') {
        $files += Get-ChildItem -LiteralPath (Join-Path $workspace $projectName) -File -Recurse |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    }
    foreach ($file in $files) {
        $hashes[$file.FullName.Substring($workspace.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    $guidesRoot = Join-Path (Split-Path -Parent $workspace) 'docs\usuario'
    foreach ($file in Get-ChildItem -LiteralPath $guidesRoot -File -Recurse) {
        $hashes['docs\usuario\' + $file.FullName.Substring($guidesRoot.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    $manualRoot = Join-Path (Split-Path -Parent $workspace) 'docs\manual'
    foreach ($file in Get-ChildItem -LiteralPath $manualRoot -File -Recurse) {
        $hashes['docs\manual\' + $file.FullName.Substring($manualRoot.Length + 1)] = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
    $hashes['Build-Manual.mjs'] = (Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Build-Manual.mjs') -Algorithm SHA256).Hash
    $hashes['Validate-Documentation.ps1'] = (Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Validate-Documentation.ps1') -Algorithm SHA256).Hash
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
        # Compile managed/WPF code without publishing or running verification suites.
        $compileRoot = $OutputRoot
        $compileTools = Join-Path $destination 'Tools\YouTube'
        if (!(Test-Path -LiteralPath $compileTools -PathType Container)) { $compileTools = Join-Path $OutputRoot 'youtube-tools' }
        $compileArgs = @('build', $projectPath, '-c', $configuration, '-r', 'win-x64',
            '--artifacts-path', $compileRoot, "-p:CubeYouTubeToolsPath=$compileTools",
            '-p:DebugType=None', '-p:DebugSymbols=false')
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
    $buildArtifacts = Join-Path $stagingRoot "$name-work-$token"
    Assert-ChildPath $staging $stagingRoot
    Assert-ChildPath $backup $stagingRoot
    Assert-ChildPath $buildArtifacts $stagingRoot
    [IO.Directory]::CreateDirectory($staging) | Out-Null
    Prepare-YouTubeTools
    $sources = Get-SourceHashes
    $publishArgs = @('publish', $projectPath, '-c', $configuration, '-r', 'win-x64',
        '--self-contained', 'true', '--artifacts-path', $buildArtifacts,
        "-p:CubeYouTubeToolsPath=$youTubeTools",
        '-p:DebugType=None', '-p:DebugSymbols=false',
        '-p:CubeUnifiedPublish=true', '-o', $staging)
    if ($Simulation) { $publishArgs += '-p:CubeSimulation=true' }
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilacao. O pacote anterior foi preservado.' }
    Assert-SameHashes $sources (Get-SourceHashes)
    foreach ($required in 'CubeControl.exe', 'CubeControl.dll', 'CubeControl.deps.json', 'CubeControl.runtimeconfig.json', 'coreclr.dll', 'NAudio.Core.dll', 'NAudio.Wasapi.dll', 'NAudio.Asio.dll', 'Cube.Studio.Core.dll', 'Cube.Studio.Audio.dll', 'CubeStudioDsp.dll', 'Licenses\Signalsmith-Stretch.txt', 'Licenses\Signalsmith-Linear.txt', 'Tools\YouTube\yt-dlp.exe', 'Tools\YouTube\ffmpeg.exe', 'Tools\YouTube\ffprobe.exe', 'Tools\YouTube\deno.exe', 'Tools\YouTube\versions.json') {
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
                ($folder -eq 'Localization' -and $file.Extension -eq '.json')) {
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
    foreach ($temporary in @($staging, $buildArtifacts) + $(if ($installed) { @($backup) } else { @() })) {
        if ($temporary) {
            try { Remove-BuildDirectory $temporary (Join-Path $OutputRoot '.staging') }
            catch { Write-Warning "Nao foi possivel limpar o temporario: $temporary. $($_.Exception.Message)" }
        }
    }
    if ($installed) {
        try { Remove-BuildDirectory (Join-Path $OutputRoot 'youtube-tools') $OutputRoot }
        catch { Write-Warning "Nao foi possivel limpar o cache de ferramentas ja publicadas: $($_.Exception.Message)" }
    }
    if ($locked) {
        try {
            $stagingRoot = Join-Path $OutputRoot '.staging'
            Assert-ChildPath $stagingRoot $OutputRoot
            if ((Test-Path -LiteralPath $stagingRoot -PathType Container) -and
                @(Get-ChildItem -LiteralPath $stagingRoot -Force).Count -eq 0) {
                [IO.Directory]::Delete($stagingRoot, $false)
            }
        } catch { Write-Warning "Nao foi possivel remover a pasta temporaria vazia: $($_.Exception.Message)" }
    }
    if ($locked) { $mutex.ReleaseMutex() }
    if ($mutex) { $mutex.Dispose() }
}
exit $exitCode
