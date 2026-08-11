$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'src\PersonalDigitalVault.Api\PersonalDigitalVault.Api.csproj'

Write-Host "Dropping the LocalDB development database through EF Core..." -ForegroundColor Yellow
& dotnet ef database drop --force --project $project --startup-project $project
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Development database removed." -ForegroundColor Green
Write-Host "Run Apply-Migrations.bat or start the API to recreate it." -ForegroundColor Cyan
