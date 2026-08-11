# Frontend–Backend Connection Check Report

## Connection design

- Frontend copied into `src/PersonalDigitalVault.Api/wwwroot`.
- ASP.NET Core serves the frontend with `UseDefaultFiles()` and `UseStaticFiles()`.
- Frontend uses the current browser origin and appends `/api`.
- The HTTPS profile serves UI and API from `https://localhost:7187`.
- Local development CORS also accepts localhost origins if the frontend is tested separately.

## API route matching

| Frontend function | Backend endpoint |
|---|---|
| Register | `POST /api/auth/register` |
| Login | `POST /api/auth/login` |
| Logout | `POST /api/auth/logout` |
| Folder list/create | `GET/POST /api/folders` |
| Folder rename/delete | `PUT/DELETE /api/folders/{id}` |
| Document list/upload | `GET/POST /api/documents` |
| Document download | `GET /api/documents/{id}/download` |
| Integrity verification | `GET /api/documents/{id}/integrity` |
| Document rename | `PUT /api/documents/{id}/rename` |
| Document delete | `DELETE /api/documents/{id}` |
| Credential list/create | `GET/POST /api/credentials` |
| Credential reveal | `GET /api/credentials/{id}/reveal` |
| Credential edit/delete | `PUT/DELETE /api/credentials/{id}` |
| Profile view/update | `GET/PUT /api/profile` |
| Admin dashboard | `GET /api/admin/dashboard` |
| Admin users | `GET /api/admin/users` |
| User status | `PATCH /api/admin/users/{id}/status` |
| Admin settings | `GET /api/admin/settings` |
| Save setting | `PUT /api/admin/settings/{key}` |

## Development configuration

- Database: SQL Server Express / MSSQL, `PersonalDigitalVaultMssqlDb`, managed by EF Core migrations.
- Database tables: automatically created in Development.
- JWT authentication: configured.
- AES-256 key: configured in `appsettings.Development.json`.
- Development administrator seed: enabled.
- Maximum upload: 10 MB.

## Validation performed

- JavaScript syntax checked with Node.js.
- JSON configuration files parsed successfully.
- Frontend CSS/JavaScript/image references checked.
- Frontend endpoint paths cross-checked against controller routes.
- AES development key verified as a 32-byte Base64 key.

A full ASP.NET runtime build was not executed in the packaging environment because the .NET CLI was unavailable there. Run Visual Studio Build or `dotnet build` on the target Windows computer after NuGet restore.
