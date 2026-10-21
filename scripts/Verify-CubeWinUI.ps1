param([string]$Directory = $PSScriptRoot)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($Directory).TrimEnd('\')
$manifest = Get-Content -LiteralPath (Join-Path $root 'distribution.json') -Raw | ConvertFrom-Json
if ($manifest.Version -ne '1.1.0' -or $manifest.Architecture -ne 'x64' -or !$manifest.Files.Count) { throw 'Manifesto de distribuicao invalido.' }
foreach ($file in $manifest.Files) {
    $path = [IO.Path]::GetFullPath((Join-Path $root $file.Path))
    if (!$path.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Caminho invalido no manifesto.' }
    if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Arquivo ausente: $($file.Path)" }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.Sha256) { throw "Integridade invalida: $($file.Path)" }
}
Write-Host "PASS: integridade de $($manifest.Files.Count) arquivos ($($manifest.Edition))."
