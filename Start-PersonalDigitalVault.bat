@echo off
setlocal
cd /d "%~dp0src\PersonalDigitalVault.Api"
echo ===============================================
echo   Personal Digital Vault - Full Stack Start
echo ===============================================
echo.
echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 goto :error

echo.
echo Starting frontend + backend at https://localhost:7187
echo Swagger: https://localhost:7187/swagger
echo Press Ctrl+C to stop the project.
echo.
dotnet run --launch-profile https
if errorlevel 1 goto :error
goto :end

:error
echo.
echo Project could not start. Check that .NET 8 SDK and SQL Server Express are installed and the SQLEXPRESS service is running.
pause
:end
endlocal
