# Personal Digital Vault — Run Step by Step (MSSQL Migration Build)

## Connected architecture

```text
Frontend: https://localhost:7187/index.html
API:      https://localhost:7187/api
Swagger:  https://localhost:7187/swagger
Database: SQL Server Express / MSSQL
```

The HTML/CSS/JavaScript frontend is inside `src/PersonalDigitalVault.Api/wwwroot`, so frontend and backend run from the same ASP.NET Core application.

## Required software

1. Visual Studio 2022 with **ASP.NET and web development** workload
2. .NET 8 SDK (or a newer SDK that can build `net8.0`)
3. Microsoft SQL Server Express / SQL Server
4. SQL Server Management Studio (recommended for viewing the database)

## Database configuration

Development connection string is in:

```text
src/PersonalDigitalVault.Api/appsettings.Development.json
```

Default:

```text
Server=(localdb)\MSSQLLocalDB
Database=PersonalDigitalVaultMssqlDb
Trusted_Connection=True
TrustServerCertificate=True
```

If your instance is different, edit the `Server=` value before running migrations.

## First-time setup

1. Extract the ZIP.
2. Make sure the SQL Server Express service is running.
3. Double-click `Apply-Migrations.bat`.
4. Wait until **Migration completed successfully** appears.
5. Double-click `Trust-HTTPS-Certificate.bat` once.
6. Open `PersonalDigitalVault.sln` in Visual Studio.
7. Wait for NuGet restore.
8. Set `PersonalDigitalVault.Api` as Startup Project.
9. Select the **https** profile.
10. Press **F5**.
11. Open `https://localhost:7187/index.html`.

## What Apply-Migrations.bat does

```bash
dotnet tool restore
dotnet restore PersonalDigitalVault.sln
dotnet ef database update --project src/PersonalDigitalVault.Api/PersonalDigitalVault.Api.csproj --startup-project src/PersonalDigitalVault.Api/PersonalDigitalVault.Api.csproj
```

It creates/updates:

```text
PersonalDigitalVaultMssqlDb
```

and tracks schema versions in:

```text
__EFMigrationsHistory
```

## Development auto migration

`appsettings.Development.json` contains:

```json
"Database": {
  "AutoMigrateInDevelopment": true
}
```

So the API also calls `Database.MigrateAsync()` during Development startup and applies pending migrations.

## View database in SSMS

1. Open SQL Server Management Studio.
2. Server name: `(localdb)\MSSQLLocalDB`
3. Authentication: **Windows Authentication**.
4. Expand **Databases**.
5. Expand `PersonalDigitalVaultMssqlDb`.
6. Expand **Tables**.

Expected tables:

```text
Users
Folders
Categories
Documents
CredentialRecords
UploadLogs
ApplicationSettings
__EFMigrationsHistory
```

## Test account

```text
Email: admin@pdv.local
Password: ChangeMe!123
```

This administrator is seeded after the database is migrated in Development.

## Complete testing order

1. Login with the development admin or register a normal user.
2. Create a folder.
3. Create credential categories.
4. Add/edit/reveal a secure credential.
5. Upload a PDF/image/document.
6. Check Documents list and preview supported files.
7. Verify file integrity.
8. Download the file.
9. Update the user profile.
10. Test administrator dashboard and user activation/deactivation.

## Swagger testing

Open:

```text
https://localhost:7187/swagger
```

For protected endpoints:

1. Run `POST /api/auth/login`.
2. Copy the returned token.
3. Click **Authorize**.
4. Paste the token only.

## Future database changes

Create a new migration:

```text
Add-Migration.bat AddYourChangeName
```

Apply it:

```text
Apply-Migrations.bat
```

See `MSSQL_MIGRATION_GUIDE.md` for the full commands.

## Common errors

### Cannot connect to `(localdb)\MSSQLLocalDB`

Open **Services** in Windows and make sure **SQL Server (SQLEXPRESS)** is running. If your instance has another name, change `ConnectionStrings:DefaultConnection` in `appsettings.Development.json`.

### Login failed for user / Windows authentication issue

Use Windows Authentication with a local SQL Server instance, or replace the connection string with the SQL login credentials configured on your SQL Server.

### Existing old `PersonalDigitalVaultDemoDb`

Older builds used `EnsureCreated()` and LocalDB. The new migration build uses `PersonalDigitalVaultMssqlDb`; this intentionally avoids conflicting migration history. Keep or back up the old DB if you need its data.

### Browser certificate warning

Run `Trust-HTTPS-Certificate.bat` and restart the browser/project.

### Port 7187 already in use

Stop the conflicting process or change the HTTPS port in `Properties/launchSettings.json`.

### NuGet / EF tool restore error

Check internet access, then run from solution root:

```bash
dotnet tool restore
dotnet restore PersonalDigitalVault.sln
```

## Important files

```text
src/PersonalDigitalVault.Api/Data/VaultDbContext.cs
src/PersonalDigitalVault.Api/Data/VaultDbContextFactory.cs
src/PersonalDigitalVault.Api/Data/DbInitializer.cs
src/PersonalDigitalVault.Api/Migrations/
src/PersonalDigitalVault.Api/appsettings.Development.json
.config/dotnet-tools.json
Apply-Migrations.bat
Add-Migration.bat
MSSQL_MIGRATION_GUIDE.md
```
