# Personal Digital Vault — BRD-Aligned Final Demo

This package contains the connected ASP.NET Core 8 backend, HTML/CSS/JavaScript frontend, JWT authentication, SQL Server database, AES-encrypted private data, and SHA-256 file integrity checking.

The 2026-08-09 BRD gap-fix build also includes:
- secure document View/Preview for PDF, TXT, JPG/JPEG, and PNG;
- full owner-only credential Category CRUD;
- immediate rejection of an existing JWT after an administrator deactivates that account.

The database schema is now managed by **EF Core migrations** against MSSQL/SQL Server Express. Read `MSSQL_MIGRATION_GUIDE.md` for first-time setup and future schema changes.

## Start

Read `MSSQL_MIGRATION_GUIDE.md` and `RUN_PROJECT_STEP_BY_STEP.md`, or:

1. Make sure SQL Server Express (`(localdb)\MSSQLLocalDB`) is running.
2. Run `Apply-Migrations.bat` once.
3. Run `Trust-HTTPS-Certificate.bat` once.
4. Open `PersonalDigitalVault.sln`.
5. Select `PersonalDigitalVault.Api` and the `https` profile.
6. Press F5.

Connected application:

```text
https://localhost:7187/index.html
```

Swagger:

```text
https://localhost:7187/swagger
```

Development administrator:

```text
admin@pdv.local
ChangeMe!123
```

The frontend is served from `src/PersonalDigitalVault.Api/wwwroot` and automatically calls the API at the same origin under `/api`.

For the exact changes added to close the BRD gaps, read `BRD_GAP_FIXES.md`.
