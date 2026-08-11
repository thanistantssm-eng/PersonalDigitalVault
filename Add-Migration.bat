@echo off
setlocal
cd /d "%~dp0"
if "%~1"=="" (
  echo Usage: Add-Migration.bat MigrationName
  echo Example: Add-Migration.bat AddDocumentTags
  exit /b 1
)

dotnet tool restore
if errorlevel 1 exit /b 1

dotnet ef migrations add %~1 --project src\PersonalDigitalVault.Api\PersonalDigitalVault.Api.csproj --startup-project src\PersonalDigitalVault.Api\PersonalDigitalVault.Api.csproj --output-dir Migrations
endlocal
