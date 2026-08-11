@echo off
setlocal
cd /d "%~dp0"
echo ===============================================
echo   Personal Digital Vault - MSSQL LocalDB EF Migration
echo ===============================================
echo.
echo Restoring local EF Core tool...
dotnet tool restore
if errorlevel 1 goto :error

echo.
echo Restoring NuGet packages...
dotnet restore PersonalDigitalVault.sln
if errorlevel 1 goto :error

echo.
echo Applying EF Core migrations to (localdb)\MSSQLLocalDB...
dotnet ef database update --project src\PersonalDigitalVault.Api\PersonalDigitalVault.Api.csproj --startup-project src\PersonalDigitalVault.Api\PersonalDigitalVault.Api.csproj
if errorlevel 1 goto :error

echo.
echo Migration completed successfully.
echo Database: PersonalDigitalVaultMssqlDb
echo SQL Server: (localdb)\MSSQLLocalDB
goto :end

:error
echo.
echo Migration failed.
echo Check that .NET 8 SDK and SQL Server Express LocalDB are installed.
exit /b 1

:end
endlocal
