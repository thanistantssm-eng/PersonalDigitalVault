# Personal Digital Vault Demo — Authentication + MSSQL Database

This build uses SQL Server Express / MSSQL with EF Core 8 migrations.

## Database

```text
SQL Server instance: (localdb)\MSSQLLocalDB
Database: PersonalDigitalVaultMssqlDb
```

Connection string:

```text
Server=(localdb)\MSSQLLocalDB;Database=PersonalDigitalVaultMssqlDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## Demo login

- Email: `admin@pdv.local`
- Password: `ChangeMe!123`

The administrator is seeded after the migration is applied in Development.

## Authentication included

- User registration: `POST /api/auth/register`
- User login: `POST /api/auth/login`
- JWT bearer token generation
- Password hashing using ASP.NET Core `PasswordHasher<VaultUser>`
- Protected API endpoints using `[Authorize]`
- Frontend token storage and automatic `Authorization: Bearer <token>` header

## Apply database migration

Recommended first run:

```text
Apply-Migrations.bat
```

Then start the project with Visual Studio or:

```text
Start-PersonalDigitalVault.bat
```

Development also has `Database:AutoMigrateInDevelopment = true`, so pending migrations are automatically applied when the API starts.

## Main tables

- Users
- Folders
- Categories
- Documents
- CredentialRecords
- UploadLogs
- ApplicationSettings
- __EFMigrationsHistory

## View the database

In SQL Server Management Studio connect to:

```text
(localdb)\MSSQLLocalDB
```

Then expand **Databases -> PersonalDigitalVaultMssqlDb -> Tables**.

For complete migration commands, read `MSSQL_MIGRATION_GUIDE.md`.
