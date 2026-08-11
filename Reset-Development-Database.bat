@echo off
setlocal
cd /d "%~dp0"
echo.
echo This deletes the LocalDB development database: PersonalDigitalVaultMssqlDb
echo ALL data in that database will be deleted.
echo.
choice /C YN /M "Continue"
if errorlevel 2 exit /b 0

dotnet tool restore
if errorlevel 1 goto :error
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Reset-Development-Database.ps1"
if errorlevel 1 goto :error

echo.
pause
exit /b 0

:error
echo.
echo Reset failed. Make sure .NET 8 SDK is installed.
pause
exit /b 1
