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
    $statePath = Join-Path $tools 'versions.json'
    $required = @('yt-dlp.exe', 'deno.exe', 'ffmpeg.exe', 'ffprobe.exe', 'yt-dlp-LICENSE.txt', 'deno-LICENSE.txt')
    if (Test-Path -LiteralPath $statePath -PathType Leaf) {
        try {
            $state = Get-Content -LiteralPath $statePath -Raw -Encoding UTF8 | ConvertFrom-Json
            $valid = $state.manifestHash -eq $manifestHash -and @($state.files).Count -ge $required.Count
            foreach ($name in $required) { if ($name -notin $state.files.path) { $valid = $false } }
            if (@($state.files.path | Where-Object { $_ -like 'Licenses\ffmpeg\*' }).Count -eq 0) { $valid = $false }
            foreach ($file in $state.files) {
                $cached = Join-Path $tools $file.path
                Assert-ChildPath $cached $tools
                if (!(Test-Path -LiteralPath $cached -PathType Leaf) -or
                    (Get-FileHash -LiteralPath $cached -Algorithm SHA256).Hash -ne $file.sha256) { $valid = $false; break }
            }
            if (@(Get-ChildItem -LiteralPath $tools -Recurse -File).Count -ne @($state.files).Count + 1) { $valid = $false }
            if ($valid) { Write-Host "YouTube tools reused from verified cache: $tools"; return }
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
        Write-Host "YouTube tools verified: $tools"
    } finally {
        $client.Dispose()
        Remove-BuildDirectory $temporary $OutputRoot
    }
}
