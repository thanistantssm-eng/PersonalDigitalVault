USE PersonalDigitalVaultMssqlDb;
GO

SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
ORDER BY MigrationId;
GO

SELECT Id, FullName, Email, Role, IsActive, CreatedAtUtc
FROM Users
ORDER BY CreatedAtUtc DESC;
GO

SELECT COUNT(*) AS UserCount FROM Users;
SELECT COUNT(*) AS FolderCount FROM Folders;
SELECT COUNT(*) AS CategoryCount FROM Categories;
SELECT COUNT(*) AS DocumentCount FROM Documents;
SELECT COUNT(*) AS CredentialCount FROM CredentialRecords;
SELECT COUNT(*) AS UploadLogCount FROM UploadLogs;
GO
