@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\scripts\Build-CubeControl.ps1" -Start
if errorlevel 1 (
    echo Nao foi possivel construir ou iniciar o CubeControl.
    pause
    exit /b 1
)
