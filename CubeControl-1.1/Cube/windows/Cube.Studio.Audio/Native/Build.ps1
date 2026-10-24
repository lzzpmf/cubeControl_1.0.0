param([Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) { throw 'Studio DSP requires Visual Studio Build Tools with Desktop development with C++ (MSVC x64 and Windows SDK).' }
$installation = & $vswhere -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $installation) {
    $installation = & $vswhere -latest -prerelease -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
}
if (-not $installation) { throw 'MSVC x64 was not found. Install the C++ build tools before building Studio DSP.' }
$msbuild = Join-Path $installation 'MSBuild\Current\Bin\MSBuild.exe'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
& $msbuild (Join-Path $PSScriptRoot 'CubeStudioDsp.vcxproj') /nologo /v:minimal /p:Configuration=Release /p:Platform=x64 "/p:StudioNativeOutput=$([IO.Path]::GetFullPath($OutputDirectory))\"
if ($LASTEXITCODE -ne 0) { throw 'Studio DSP native build failed.' }
