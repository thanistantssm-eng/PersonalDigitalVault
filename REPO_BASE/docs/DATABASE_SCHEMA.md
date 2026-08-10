# Database Schema

## Users
- `Id` — primary key
- `FullName`
- `Email` — unique
- `PasswordHash`
- `PhoneNumber`
- `Role`
- `IsActive`
- timestamps

## Folders
- `Id` — primary key
- `Name`
- `UserId` — owner foreign key
- timestamps
- Unique per owner: `(UserId, Name)`

## Categories
- `Id` — primary key
- `Name`
- `UserId` — owner foreign key
- timestamps
- Unique per owner: `(UserId, Name)`
- Used to manage credential categories independently from credential records

The table is created and versioned by EF Core migrations.

## Documents
- `Id` — primary key
- `OriginalFileName`
- `StoredFileName`
- `RelativePath`
- `ContentType`
- `FileSizeBytes`
- `IntegrityHashSha256`
- `UserId` — owner foreign key
- `FolderId` — optional folder foreign key
- timestamps

## CredentialRecords
- `Id` — primary key
- `Title`
- `Category` — managed category name; credential remains valid if category is deleted
- `UsernameCipherText`
- `SecretCipherText`
- `WebsiteCipherText`
- `NotesCipherText`
- `UserId` — owner foreign key
- timestamps

## UploadLogs
Successful upload history. Admin dashboard `TotalUploads` count-ku use pannappadum.

## ApplicationSettings
Simple key/value settings managed by administrator.

## Relationships

```text
User 1 ─── * Folder
User 1 ─── * Category
User 1 ─── * Document
User 1 ─── * CredentialRecord
Folder 1 ─── * Document (optional relationship)
```
