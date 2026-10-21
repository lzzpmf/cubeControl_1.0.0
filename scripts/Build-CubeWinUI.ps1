param(
    [switch]$Start,
    [switch]$SkipYouTubeTools,
    [string]$OutputRoot = 'C:\CubeControl-WinUI-Build',
    [string]$PublishDirectory
)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$output = [IO.Path]::GetFullPath($OutputRoot)
if ($output.Equals($repository, [StringComparison]::OrdinalIgnoreCase) -or
    $output.StartsWith($repository + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Escolha uma pasta de saida fora do repositorio.'
}
. (Join-Path $PSScriptRoot 'YouTubeTools.ps1')
if (!$SkipYouTubeTools) { Prepare-YouTubeTools }
$project = Join-Path $repository 'CubeControl-1.1\Cube\windows\Cube.WinUI\Cube.WinUI.csproj'
$publish = if ($PublishDirectory) { [IO.Path]::GetFullPath($PublishDirectory) } else { Join-Path $output 'publish\CubeControl.WinUI' }
& dotnet publish $project -c Release -p:Platform=x64 "-p:ArtifactsPath=$output" -o $publish
if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar o host WinUI: $LASTEXITCODE" }
$exe = Join-Path $publish 'CubeControl.WinUI.exe'
if (!(Test-Path -LiteralPath $exe)) { throw 'Executavel WinUI nao encontrado.' }
foreach ($resource in @('CubeControl.WinUI.pri', 'App.xbf', 'MainWindow.xbf', 'WorkspacePage.xbf', 'Appearance/Studio.xbf', 'Assets/Images/Controls/Knobs/knob_oficial_01.png', 'Assets/Images/Controls/Finishes/official-light/fader-vertical.png', 'CubeBackingDsp.dll', 'Assets/IrPacks.v1.json', 'Localization/en-US.json')) {
    if (!(Test-Path -LiteralPath (Join-Path $publish $resource))) { throw "Recurso WinUI ausente: $resource" }
}
Write-Host "Host WinUI: $exe"
if ($Start) { Start-Process -FilePath $exe -WorkingDirectory $publish }
