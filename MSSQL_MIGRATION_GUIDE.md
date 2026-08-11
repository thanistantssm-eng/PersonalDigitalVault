# Personal Digital Vault — MSSQL + EF Core Migration Guide

This build uses **Microsoft SQL Server / SQL Server Express** with **Entity Framework Core 8 migrations**.

## Default development connection

```text
Server=(localdb)\MSSQLLocalDB
Database=PersonalDigitalVaultMssqlDb
Authentication=Windows / Trusted Connection
```

Connection string file:

```text
src/PersonalDigitalVault.Api/appsettings.Development.json
```

Default value:

```text
Server=(localdb)\MSSQLLocalDB;Database=PersonalDigitalVaultMssqlDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

If your SQL Server instance has another name, change only the `Server=` value. Examples:

```text
Server=localhost
Server=(localdb)\MSSQLLocalDB
Server=DESKTOP-NAME\SQLEXPRESS
```

## First-time migration

### Easy method

Run:

```text
Apply-Migrations.bat
```

It performs:

```bash
dotnet tool restore
dotnet restore PersonalDigitalVault.sln
dotnet ef database update --project src/PersonalDigitalVault.Api/PersonalDigitalVault.Api.csproj --startup-project src/PersonalDigitalVault.Api/PersonalDigitalVault.Api.csproj
```

EF creates the `PersonalDigitalVaultMssqlDb` database, all tables, indexes, foreign keys, and the `__EFMigrationsHistory` table.

## Visual Studio Package Manager Console

From Visual Studio:

```powershell
Update-Database -Project PersonalDigitalVault.Api -StartupProject PersonalDigitalVault.Api
```

## Future model changes

After changing a model or `VaultDbContext`, create a migration:

```text
Add-Migration.bat AddYourChangeName
```

or:

```bash
dotnet ef migrations add AddYourChangeName --project src/PersonalDigitalVault.Api/PersonalDigitalVault.Api.csproj --startup-project src/PersonalDigitalVault.Api/PersonalDigitalVault.Api.csproj --output-dir Migrations
```

Then apply it:

```bash
dotnet ef database update --project src/PersonalDigitalVault.Api/PersonalDigitalVault.Api.csproj --startup-project src/PersonalDigitalVault.Api/PersonalDigitalVault.Api.csproj
```

## Existing old demo database

Older ZIP versions used `EnsureCreated()` and the database name `PersonalDigitalVaultDemoDb`. That schema is not migration-tracked.

For a clean development conversion:

1. Keep a backup if you need old data.
2. Use the new database name `PersonalDigitalVaultMssqlDb`.
3. Run `Apply-Migrations.bat`.

This avoids conflicts between an old `EnsureCreated` schema and EF migration history.

## Verify in SSMS

Connect to:

```text
(localdb)\MSSQLLocalDB
```

Then expand:

```text
Databases
  -> PersonalDigitalVaultMssqlDb
     -> Tables
```

Expected application tables:

- Users
- Folders
- Categories
- Documents
- CredentialRecords
- UploadLogs
- ApplicationSettings
- __EFMigrationsHistory

You can also run:

```text
Database/InspectDemoDatabase.sql
```

## Runtime behavior

In Development, `Database:AutoMigrateInDevelopment` is enabled. Therefore the application also calls `Database.MigrateAsync()` at startup and applies pending migrations automatically.

For production, apply migrations as a deployment step instead of depending on automatic startup migration.

## Legacy EnsureCreated database compatibility

> Final build note: this package intentionally uses a fresh migration-owned database named `PersonalDigitalVaultMssqlDb`; the older database is left untouched.
