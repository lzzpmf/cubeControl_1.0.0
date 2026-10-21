param(
    [switch]$Offline,
    [string]$OutputRoot = 'C:\CubeControl-WinUI-Build'
)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$output = [IO.Path]::GetFullPath($OutputRoot)
if ($output.Equals($repository, [StringComparison]::OrdinalIgnoreCase) -or $output.StartsWith($repository + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Use uma saida fora do repositorio.' }
$edition = if ($Offline) { 'offline' } else { 'full' }
$run = Join-Path $output ('distribution\' + [Guid]::NewGuid().ToString('N'))
$stage = Join-Path $run 'CubeControl'
New-Item -ItemType Directory -Path $stage -Force | Out-Null
& (Join-Path $PSScriptRoot 'Build-CubeWinUI.ps1') -OutputRoot $output -PublishDirectory $stage -SkipYouTubeTools:$Offline
if (!$Offline) {
    foreach ($tool in @('yt-dlp.exe', 'deno.exe', 'ffmpeg.exe', 'ffprobe.exe')) {
        if (!(Test-Path -LiteralPath (Join-Path $stage "Tools\YouTube\$tool"))) { throw "Ferramenta obrigatoria ausente: $tool" }
    }
} elseif (Test-Path -LiteralPath (Join-Path $stage 'Tools\YouTube')) {
    # Remove only the fresh staging copy, never the shared build cache.
    $tools = [IO.Path]::GetFullPath((Join-Path $stage 'Tools\YouTube'))
    if (!$tools.StartsWith($stage + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Destino invalido.' }
    Remove-Item -LiteralPath $tools -Recurse -Force
}
foreach ($file in @('coreclr.dll', 'hostfxr.dll', 'Microsoft.UI.Xaml.dll', 'THIRD-PARTY-NOTICES.txt')) {
    if (!(Test-Path -LiteralPath (Join-Path $stage $file))) { throw "Dependencia ausente: $file" }
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Verify-CubeWinUI.ps1') -Destination $stage
@"
CubeControl WinUI 1.1.0 - Windows x64 - $edition

Extraia TODA a pasta para um diretorio gravavel e execute CubeControl.WinUI.exe.
Nao execute diretamente dentro do ZIP. .NET e Windows App SDK acompanham o app.
Minimo declarado pelo projeto: Windows build 19041, x64. Validado localmente no Windows 11 22631.
Pacote nao assinado. Nao ha instalador, atualizador automatico nem registro no sistema.
Para verificar os arquivos: powershell -NoProfile -File .\Verify-CubeWinUI.ps1
O SHA256 externo do ZIP detecta alteracoes; nao substitui assinatura de um editor confiavel.

Atualizacao: feche o app, extraia a nova versao em OUTRA pasta e execute-a.
Dados permanecem em %LOCALAPPDATA%\CubeControl. Faca backup dessa pasta antes de atualizar.
Rollback: use o pacote anterior com o backup compativel; nao sobrescreva dados novos.
Remocao: feche o app e remova somente a pasta extraida. Os dados do usuario permanecem.
Para testes isolados: CubeControl.WinUI.exe --offline --data-directory C:\pasta-de-teste

Edicao offline: sem ferramentas para baixar/prever audio do YouTube.
Edicao full: inclui ferramentas do YouTube; requer rede e disponibilidade do provedor.
Consulte THIRD-PARTY-NOTICES.txt e Assets\Documents para avisos e manuais.
Validacao em maquina limpa e assinatura de distribuicao ainda pendentes.
"@ | Set-Content -LiteralPath (Join-Path $stage 'LEIA-ME.txt') -Encoding UTF8
$files = @(Get-ChildItem -LiteralPath $stage -Recurse -File | Sort-Object FullName | ForEach-Object {
    [ordered]@{ Path = $_.FullName.Substring($stage.Length + 1); Sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
})
[ordered]@{ Version = '1.1.0'; Architecture = 'x64'; Edition = $edition; Files = $files } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $stage 'distribution.json') -Encoding UTF8
& (Join-Path $stage 'Verify-CubeWinUI.ps1')
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = Join-Path $run "CubeControl-WinUI-1.1.0-win-x64-$edition.zip"
[IO.Compression.ZipFile]::CreateFromDirectory($stage, $zip, [IO.Compression.CompressionLevel]::Optimal, $true)
((Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash + '  ' + [IO.Path]::GetFileName($zip)) | Set-Content -LiteralPath ($zip + '.sha256') -Encoding ASCII
$test = Join-Path $run 'extracted'
[IO.Compression.ZipFile]::ExtractToDirectory($zip, $test)
$extracted = Join-Path $test 'CubeControl'
& (Join-Path $extracted 'Verify-CubeWinUI.ps1')
& (Join-Path $PSScriptRoot 'Test-CubeWinUI.ps1') -Executable (Join-Path $extracted 'CubeControl.WinUI.exe')
& (Join-Path $PSScriptRoot 'Test-CubeWinUI.Identity.ps1') -Executable (Join-Path $extracted 'CubeControl.WinUI.exe') -EvidenceDirectory (Join-Path $run 'identity-evidence')
Write-Host "Pacote validado: $zip"
