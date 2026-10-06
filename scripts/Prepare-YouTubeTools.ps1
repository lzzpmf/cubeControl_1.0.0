param([Parameter(Mandatory=$true)][string]$OutputRoot)
$ErrorActionPreference = 'Stop'
$manifestPath = Join-Path $PSScriptRoot 'youtube-tools.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$root = [IO.Path]::GetFullPath((Join-Path $OutputRoot 'youtube-tools'))
$repository = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot)).TrimEnd('\') + '\'
if ($root.StartsWith($repository, [StringComparison]::OrdinalIgnoreCase)) { throw 'Tool cache must be outside the repository.' }
[IO.Directory]::CreateDirectory($root) | Out-Null
$tools = Join-Path $root 'Tools\YouTube'
[IO.Directory]::CreateDirectory($tools) | Out-Null
foreach ($asset in $manifest.assets) {
    $archive = Join-Path $root $asset.file
    if (!(Test-Path -LiteralPath $archive) -or (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $asset.sha256) {
        Invoke-WebRequest -Uri $asset.url -OutFile $archive -UseBasicParsing
    }
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $asset.sha256) { throw "SHA256 mismatch: $($asset.file)" }
    if ($asset.file.EndsWith('.zip')) {
        $expanded = Join-Path $root ($asset.file + '.expanded')
        Expand-Archive -LiteralPath $archive -DestinationPath $expanded -Force
        foreach ($exe in $asset.executables) {
            $source = @(Get-ChildItem -LiteralPath $expanded -Filter $exe -Recurse -File)
            if ($source.Count -ne 1) { throw "Executable not found: $exe" }
            Copy-Item -LiteralPath $source[0].FullName -Destination (Join-Path $tools $exe) -Force
        }
        # Preserve the upstream license texts and build documentation.
        $notices = Join-Path $tools ('Licenses\' + $asset.name)
        [IO.Directory]::CreateDirectory($notices) | Out-Null
        Get-ChildItem -LiteralPath $expanded -Recurse -File | Where-Object { $_.Name -match '^(LICENSE|COPYING|README)' } | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $notices $_.Name) -Force }
    } else { Copy-Item -LiteralPath $archive -Destination (Join-Path $tools $asset.target) -Force }
}
Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $tools 'versions.json') -Force
Write-Host "YouTube tools verified: $tools"
