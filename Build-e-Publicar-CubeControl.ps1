param([switch]$Simulation)

& "$PSScriptRoot\Publicar-CubeControl.ps1" -Simulation:$Simulation -Etapa Ambos
exit $LASTEXITCODE
