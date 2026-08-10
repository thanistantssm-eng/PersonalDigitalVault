# Backend Viva Guide — Tanglish

## 1. Project backend enna pannudhu?

“Personal Digital Vault backend users-a authenticate pannudhu, avanga own folders, documents, credentials mattum manage panna allow pannudhu. Metadata SQL Server-la irukkum. Sensitive files and values AES encryption use panni protect pannappadum. SHA-256 hash file integrity verify pannum.”

## 2. JWT enna?

JWT login success aana backend issue pannura signed token. Token-la user ID, email, role, expiry claims irukkum. Ovvoru protected API request-la Bearer token anuppanum.

## 3. Authentication vs authorization

- Authentication: “Nee yaar?” — login and JWT validation.
- Authorization: “Unakku indha data access irukka?” — role and ownership check.

## 4. User vera user document ID guess pannina?

Every document query-la `x.Id == id && x.UserId == loggedInUserId` check irukku. ID correct-a guess pannalum owner match illaina record return aagathu.

## 5. Hashing vs encryption

- Password hashing one-way; original password retrieve panna mudiyadhu.
- AES encryption reversible; correct key use panni file/credential decrypt pannalaam.
- SHA-256 file-a hide pannathu illa; changed/corrupted-a irukka verify pannum.

## 6. Upload flow

Validate file → read bytes → SHA-256 hash → AES encrypt → protected folder save → metadata database save.

## 7. Download flow

JWT check → ownership check → encrypted bytes read → AES decrypt → SHA-256 compare → original file return.

## 8. Why files outside wwwroot?

`wwwroot` public static files-ku. Private file anga irundha direct URL access risk irukku. ProtectedStorage API authorization pass aana mattum read pannappadum.

## 9. Admin user files access panna mudiyuma?

Mudiyadhu. Admin users and settings manage pannalaam, dashboard counts paakalaam. Private content endpoints logged-in owner ID filter use pannudhu.

## 10. EF Core enna?

C# entities and LINQ use panni SQL Server operations panna ORM. DbContext database session and tables-a represent pannum.

## 11. Main tables

Users, Folders, Documents, CredentialRecords, UploadLogs, ApplicationSettings.

## 12. Why DTO?

Database entity direct-a expose panna security and coupling problem varum. DTO request/response-la required fields mattum control pannum. PasswordHash and encrypted storage path API response-la varadhu.

## 13. Error handling epdi?

Global exception middleware consistent JSON error response return pannum. Validation errors ASP.NET `[ApiController]` automatic 400 response-a handle pannum.

## 14. SHA-256 mismatch aana?

Backend download stop panni conflict/error response return pannum. Changed or corrupted file user-ku original file-nu return panna koodadhu.

## 15. One-minute viva answer

“Personal Digital Vault is an ASP.NET Core Web API connected to SQL Server through Entity Framework Core. Users register and log in, and the API issues JWT tokens. Every folder, document and credential record is linked to a UserId, so ownership authorization prevents cross-user access. Passwords are stored as one-way hashes. Uploaded file bytes and sensitive credential values are encrypted using AES-256-GCM. Before encryption, the system generates a SHA-256 hash, and during download it recalculates the hash to verify integrity. Files are stored outside the public web directory. Administrators can manage accounts and view dashboard counts, but they do not receive an endpoint to decrypt users’ private content.”
