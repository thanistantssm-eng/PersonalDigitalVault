# BRD Gap Fixes — 2026-08-09

This build closes the three BRD-alignment gaps identified during the project review.

## 1. Document View / Preview
- Added authenticated `GET /api/documents/{id}/preview`.
- The API checks document ownership, decrypts the stored bytes, verifies the SHA-256 integrity hash, and only then returns preview content.
- Inline preview supports PDF, TXT, JPG/JPEG, and PNG.
- DOC/DOCX records still have a secure View dialog with metadata and a Download-to-view option because browsers do not reliably render those formats natively.
- Frontend Documents table now includes a View/Preview action.

## 2. Proper Category Management
- Added `VaultCategory` model and `Categories` database table.
- Added owner-only category CRUD API: list, create, rename, and delete.
- Added a Categories page to the frontend navigation.
- Renaming a category updates linked credential records.
- Deleting a category does not delete credentials; it makes them uncategorised.
- Credential create/update can select an existing category or type a new category name, which is automatically registered.
- Existing demo databases are upgraded non-destructively at startup by creating the Categories table if required and importing category names already used by credentials.

## 3. Immediate Account Deactivation
- JWT signature and expiry are still validated normally.
- After token validation, the API now checks the user database to confirm that the token owner still exists and `IsActive == true`.
- If an administrator deactivates a user, that user's already-issued JWT is rejected on the next API request instead of remaining usable until its expiry time.

## Regression protection
The original encryption, SHA-256 hashing, ownership filters, folders, credential encryption/masking, admin privacy boundary, and download logic were kept intact.
