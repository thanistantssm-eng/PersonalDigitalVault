# SQL Server cascade-path error fix

## Error

`Introducing FOREIGN KEY constraint 'FK_Documents_Users_UserId' on table 'Documents' may cause cycles or multiple cascade paths.`

## Cause

The model previously configured both of these database actions:

1. Deleting a user cascades directly to that user's documents.
2. Deleting a user cascades to folders, while deleting a folder sets its documents' `FolderId` to null.

SQL Server treats both `CASCADE` and `SET NULL` as cascading referential actions. That created two paths from `Users` to `Documents`.

## Code correction

In `src/PersonalDigitalVault.Api/Data/VaultDbContext.cs`, the folder-to-document foreign key now uses:

```csharp
.OnDelete(DeleteBehavior.NoAction);
```

`FoldersController.Delete` already changes affected documents to `FolderId = null` before deleting the folder, so folder deletion still works safely.

## One-time database reset

Because schema creation previously failed, remove the partially created development database once:

1. Close the running API and Visual Studio debugging session.
2. Double-click `Reset-Development-Database.bat`.
3. Press `Y` to confirm.
4. Open `PersonalDigitalVault.sln`.
5. Run the HTTPS profile again.

`Database.MigrateAsync()` / `Apply-Migrations.bat` will create the database with the corrected foreign keys.
