param([switch]$Simulation)

& "$PSScriptRoot\Publicar-CubeControl.ps1" -Simulation:$Simulation -Etapa Build
exit $LASTEXITCODE
