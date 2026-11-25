@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo Iniciando SoundGraph Pro em http://localhost:5180 ...
dotnet run
pause
