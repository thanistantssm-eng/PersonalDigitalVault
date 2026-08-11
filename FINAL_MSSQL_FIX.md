# Final MSSQL / EF Core Migrations Fix

## Why the previous error happened

The older project created `PersonalDigitalVaultDb` with `EnsureCreatedAsync()`. That database already contains tables such as `ApplicationSettings`, but it was not created/owned by EF Core Migrations. Running the initial migration against that same database can therefore try to create an existing table again.

## What this final build changes

- Uses SQL Server LocalDB (MSSQL provider).
- Uses a fresh migration-owned database: `PersonalDigitalVaultMssqlDb`.
- Keeps the old `PersonalDigitalVaultDb` untouched.
- Uses `Database.MigrateAsync()` only; there is no `EnsureCreatedAsync()` path.
- Applies `20260810154000_InitialMssqlSchema` automatically in Development.
- Creates `__EFMigrationsHistory` normally through EF Core.

## Run

1. Stop the old Visual Studio run (`Shift+F5`).
2. Open `PersonalDigitalVault.sln` from this folder.
3. Build -> Clean Solution.
4. Build -> Rebuild Solution.
5. Press `F5`.

The application should create/update:

`(localdb)\\MSSQLLocalDB -> Databases -> PersonalDigitalVaultMssqlDb`

## Verify

In Visual Studio: View -> SQL Server Object Explorer -> SQL Server -> `(localdb)\\MSSQLLocalDB` -> Databases -> `PersonalDigitalVaultMssqlDb`.

Expected tables include:

- ApplicationSettings
- Categories
- CredentialRecords
- Documents
- Folders
- UploadLogs
- Users
- __EFMigrationsHistory

## Command-line alternative

From the solution folder:

```powershell
dotnet tool restore
dotnet restore
dotnet ef database update --project src/PersonalDigitalVault.Api --startup-project src/PersonalDigitalVault.Api
```

## Old data

The old `PersonalDigitalVaultDb` is not deleted. If its existing demo data must be copied into the new migration-owned database, do that as a separate data migration after the new schema is running successfully.
