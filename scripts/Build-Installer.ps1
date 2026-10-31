[CmdletBinding()]
param(
    [string]$OutputRoot = 'C:\CubeControl-Build',
    [string]$CompilerPath,
    [switch]$SkipPublish
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
if ($OutputRoot.Equals($repositoryRoot, [StringComparison]::OrdinalIgnoreCase) -or
    $OutputRoot.StartsWith($repositoryRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Escolha um OutputRoot fora do repositorio.'
}

if (-not $CompilerPath) {
    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) { $CompilerPath = $command.Source }
    else {
        $candidates = @(
            (Join-Path $OutputRoot 'installer-tools\InnoSetup\ISCC.exe'),
            "${env:ProgramFiles}\Inno Setup 7\ISCC.exe",
            "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe",
            "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
            "$env:LOCALAPPDATA\Programs\Inno Setup 7\ISCC.exe",
            "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
        )
        $CompilerPath = $candidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
    }
}
if (-not $CompilerPath -or -not (Test-Path -LiteralPath $CompilerPath -PathType Leaf)) {
    throw 'Instale Inno Setup 6.3 ou superior (https://jrsoftware.org/isdl.php), ou informe -CompilerPath com o caminho de ISCC.exe.'
}
$CompilerPath = (Resolve-Path -LiteralPath $CompilerPath).Path

$projectPath = Join-Path $repositoryRoot 'CubeControl\Cube\windows\Cube.Desktop\Cube.Desktop.csproj'
[xml]$project = Get-Content -LiteralPath $projectPath -Raw
$version = $project.SelectSingleNode('/Project/PropertyGroup/Version').InnerText
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Versao do projeto invalida.' }

if (-not $SkipPublish) {
    # Separate process: the unified build terminates with exit.
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Build-CubeControl.ps1') -OutputRoot $OutputRoot
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao publicar o aplicativo; instalador nao gerado.' }
}
$packageRoot = Join-Path $OutputRoot "publish\CubeControl-$version"
foreach ($required in @('CubeControl.exe', 'CubeControl.dll', 'coreclr.dll', 'CubeStudioDsp.dll',
    'CubeControl.runtimeconfig.json', 'THIRD-PARTY-NOTICES.txt', 'Assets\Documents\manual\index.html',
    'Tools\YouTube\yt-dlp.exe', 'Tools\YouTube\deno.exe', 'Tools\YouTube\ffmpeg.exe', 'Tools\YouTube\ffprobe.exe')) {
    if (-not (Test-Path -LiteralPath (Join-Path $packageRoot $required) -PathType Leaf)) {
        throw "Pacote incompleto: $required. Execute novamente sem -SkipPublish."
    }
}
if ([Reflection.AssemblyName]::GetAssemblyName((Join-Path $packageRoot 'CubeControl.dll')).Version -ne [version]"$version.0") {
    throw 'A versao do pacote difere da versao do projeto.'
}
& (Join-Path $PSScriptRoot 'Validate-Documentation.ps1') -PackageRoot $packageRoot

$installerOutput = Join-Path $OutputRoot 'installer'
New-Item -ItemType Directory -Path $installerOutput -Force | Out-Null
& $CompilerPath "/DPackageRoot=$packageRoot" "/DInstallerOutput=$installerOutput" "/DAppVersion=$version" (Join-Path $PSScriptRoot 'installer\CubeControl.iss')
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilacao do instalador.' }
$installer = Join-Path $installerOutput "CubeControl-$version-Setup-x64.exe"
if (-not (Test-Path -LiteralPath $installer -PathType Leaf)) { throw 'Instalador nao encontrado.' }
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash
Set-Content -LiteralPath "$installer.sha256" -Value "$hash  $([IO.Path]::GetFileName($installer))" -Encoding ASCII
Write-Host "Instalador gerado: $installer"
Write-Host 'Distribua somente o Setup-x64.exe. O arquivo .sha256 permite conferir a integridade.'
