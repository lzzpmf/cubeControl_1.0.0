@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\lzzpm\Downloads\cubeSuiteReverse\Build-e-Publicar-CubeControl.ps1"
if errorlevel 1 (
    echo Nao foi possivel sincronizar o CubeControl.
    pause
    exit /b 1
)
start "" "C:\CubeControl-Build\publish\CubeControl-1.0.0\CubeControl.exe"
