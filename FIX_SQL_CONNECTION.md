# SQL Server connection fix

This build uses **Microsoft SQL Server Express LocalDB** for local Visual Studio development.
It is still SQL Server/MSSQL; it does not use SQLite.

Default connection:

```text
Server=(localdb)\MSSQLLocalDB;Database=PersonalDigitalVaultMssqlDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## Run

1. Open Command Prompt and verify LocalDB:
   `sqllocaldb info`
2. If `MSSQLLocalDB` is listed, open the solution in Visual Studio and run it.
3. The application calls `Database.MigrateAsync()` in Development and creates/updates `PersonalDigitalVaultMssqlDb` automatically.
4. In Visual Studio: View -> SQL Server Object Explorer -> SQL Server -> `(localdb)\MSSQLLocalDB` -> Databases -> `PersonalDigitalVaultMssqlDb`.

## Manual migration

From the solution root:

```bat
dotnet tool restore
dotnet restore
dotnet ef database update --project src\PersonalDigitalVault.Api --startup-project src\PersonalDigitalVault.Api
```

## If you specifically want SQL Server Express instance

Replace the connection string server with `.\SQLEXPRESS`, but only if `SQL Server (SQLEXPRESS)` is installed and running on the machine.
