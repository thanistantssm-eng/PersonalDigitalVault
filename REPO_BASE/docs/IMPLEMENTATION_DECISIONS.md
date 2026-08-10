# Implementation Decisions

BRD exact-a specify pannadha technical details inga transparent-ah document pannappattirukku.

1. **Framework version:** .NET 8 selected as the implementation target.
2. **AES mode:** AES-256-GCM selected. GCM encryption-oda authentication tag-um provide pannum.
3. **File limit:** Configurable 10 MB default. BRD exact size limit sollala.
4. **File types:** PDF, DOC, DOCX, TXT, JPG, JPEG, PNG default. Configuration-la change pannalaam.
5. **Folder depth:** Flat document folders. Nested folder requirement BRD-la illa.
6. **Category management:** Credential categories have separate owner-only create/rename/delete management. Credential create/update-la new category name type pannina automatically category create aagum.
7. **Folder deletion:** Folder delete pannumbodhu documents delete aagathu; `FolderId` null aagi unfiled state-ku pogum.
8. **Category deletion:** Category delete pannumbodhu credential record delete aagathu; `Category` null aagi uncategorised state-ku pogum.
9. **Document preview:** PDF, TXT, JPG/JPEG, PNG browser-la secure inline preview. DOC/DOCX-ku metadata view + secure download because native browser rendering reliable illa.
10. **Logout:** JWT stateless system; client token remove pannum. Server-side token blacklist BRD-la require pannala.
11. **Immediate deactivation:** Token signature/expiry valid-a irundhaalum each protected request-la user `IsActive` database-la re-check pannappadum. Admin deactivate pannina old JWT next request-la reject aagum.
12. **Credential search:** Title and category search pannum. Encrypted username/secret direct SQL search panna mudiyadhu.
13. **Admin privacy:** Admin-ku private document download or credential reveal bypass endpoint create pannala.
14. **Development database:** MSSQL / SQL Server Express schema is managed by EF Core migrations. Development startup uses `Database.MigrateAsync()` when `AutoMigrateInDevelopment` is enabled; production should apply migrations during deployment.
