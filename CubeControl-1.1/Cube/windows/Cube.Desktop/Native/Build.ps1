param([Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) { throw 'Backing DSP requires Visual Studio Build Tools with Desktop development with C++ (MSVC x64 and Windows SDK).' }
$installation = & $vswhere -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $installation) {
    $installation = & $vswhere -latest -prerelease -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
}
if (-not $installation) { throw 'MSVC x64 was not found. Install the C++ build tools before building Backing DSP.' }
$msbuild = Join-Path $installation 'MSBuild\Current\Bin\MSBuild.exe'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
& $msbuild (Join-Path $PSScriptRoot 'CubeBackingDsp.vcxproj') /nologo /v:minimal /p:Configuration=Release /p:Platform=x64 "/p:BackingNativeOutput=$([IO.Path]::GetFullPath($OutputDirectory))\"
if ($LASTEXITCODE -ne 0) { throw 'Backing DSP native build failed.' }
