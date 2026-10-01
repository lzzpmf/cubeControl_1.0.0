@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\lzzpm\Downloads\cubeSuiteReverse\Publicar-CubeControl.ps1" -Simulation
if errorlevel 1 (
    echo Nao foi possivel sincronizar o CubeControl.
    pause
    exit /b 1
)
start "" "C:\CubeControl-Build\publish\CubeControl-Simulation-1.0.0\CubeControl.exe"
