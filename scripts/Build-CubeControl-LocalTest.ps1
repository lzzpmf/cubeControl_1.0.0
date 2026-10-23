<#
.SYNOPSIS
Publica uma copia de teste sem baixar ou incluir ferramentas do YouTube.
.EXAMPLE
.\scripts\Build-CubeControl-LocalTest.ps1 -Start
.NOTES
Nao altera o aplicativo nem o build oficial. Requer SDK .NET 10 e ferramentas
C++ usadas pelo projeto. O restore do NuGet pode precisar de rede.
YouTube nao pode ser validado nesta distribuicao sem suas ferramentas externas.
#>
[CmdletBinding()]
param(
    [switch]$Simulation,
    [switch]$Start,
    [string]$OutputRoot = (Join-Path $env:LOCALAPPDATA 'CubeControl-LocalTest')
)

# Isola as variaveis mesmo quando o terminal executa o arquivo com dot-sourcing.
& {
$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$project = Join-Path $repositoryRoot 'CubeControl-1.1\Cube\windows\Cube.Desktop\Cube.Desktop.csproj'
$mutex = $null
$locked = $false
$previousPath = $env:PATH

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
        throw 'OutputRoot deve ficar fora do repositorio e nao pode ser ancestral dele.'
    }
    $ancestor = $OutputRoot
    while ($ancestor) {
        if ((Test-Path -LiteralPath $ancestor) -and
            ((Get-Item -LiteralPath $ancestor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "O destino nao pode atravessar um link ou junction: $ancestor"
        }
        $ancestor = Split-Path -Parent $ancestor
    }

    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    $dotnet = if ($dotnetCommand) { $dotnetCommand.Source } else { Join-Path $env:ProgramFiles 'dotnet\dotnet.exe' }
    if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) { throw 'SDK .NET 10 nao encontrado.' }
    $env:PATH = (Split-Path -Parent $dotnet) + ';' + $env:PATH
    $mutex = [Threading.Mutex]::new($false, 'Local\CubeControlBuild')
    $locked = $mutex.WaitOne(0)
    if (-not $locked) { throw 'Outro build do CubeControl esta em andamento.' }

    # Um diretorio novo por execucao impede incluir ferramentas de caches antigos
    # e preserva tanto os pacotes oficiais quanto testes anteriores.
    $runRoot = Join-Path $OutputRoot ((Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N'))
    $artifacts = Join-Path $runRoot 'artifacts'
    $destination = Join-Path $runRoot 'app'
    [IO.Directory]::CreateDirectory($runRoot) | Out-Null
    $configuration = if ($Simulation) { 'Simulation' } else { 'Release' }
    $publishArgs = @('publish', $project, '-c', $configuration, '-r', 'win-x64',
        '--self-contained', 'true', '--artifacts-path', $artifacts,
        '-p:CubeUnifiedPublish=true', '-o', $destination)
    if ($Simulation) { $publishArgs += '-p:CubeSimulation=true' }
    Write-Host 'Publicando teste local sem FFmpeg, yt-dlp ou Deno...'
    & $dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { throw "Falha na publicacao. Saida de diagnostico preservada em: $runRoot" }

    foreach ($required in 'CubeControl.exe', 'CubeControl.dll', 'coreclr.dll', 'CubeBackingDsp.dll') {
        if (-not (Test-Path -LiteralPath (Join-Path $destination $required) -PathType Leaf)) {
            throw "Arquivo obrigatorio ausente: $required"
        }
    }
    if (Test-Path -LiteralPath (Join-Path $destination 'Tools\YouTube')) {
        throw 'A publicacao de teste incluiu ferramentas do YouTube inesperadamente.'
    }
    @'
Distribuicao de teste local, sem ferramentas do YouTube.
O codigo do aplicativo e o script oficial de build nao foram alterados.
Downloads/conversoes do YouTube nao fazem parte deste teste.
O restore de dependencias .NET e a compilacao nativa continuam necessarios.
Esta copia usa as preferencias e os dados normais do aplicativo ao ser aberta.
'@ | Set-Content -LiteralPath (Join-Path $destination 'TESTE-LOCAL.txt') -Encoding UTF8
    Write-Host "Aplicativo pronto: $(Join-Path $destination 'CubeControl.exe')"
    if ($Start) { Start-Process -FilePath (Join-Path $destination 'CubeControl.exe') -WorkingDirectory $destination }
} finally {
    if ($locked) { $mutex.ReleaseMutex() }
    if ($mutex) { $mutex.Dispose() }
    $env:PATH = $previousPath
}
}
