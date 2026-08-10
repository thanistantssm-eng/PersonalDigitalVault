# API Endpoints

## Authentication
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- Every authenticated request re-checks that the JWT owner account still exists and is active. A deactivated account's existing token is rejected immediately.

## Profile
- `GET /api/profile`
- `PUT /api/profile`

## Folders
- `GET /api/folders`
- `GET /api/folders/{id}`
- `POST /api/folders`
- `PUT /api/folders/{id}`
- `DELETE /api/folders/{id}`

## Categories
- `GET /api/categories`
- `GET /api/categories/{id}`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`

Category rename updates linked credential records. Category delete keeps the credentials and clears only their category.

## Documents
- `GET /api/documents?search=&folderId=`
- `GET /api/documents/{id}`
- `POST /api/documents`
- `GET /api/documents/{id}/preview` — owner-only inline preview for PDF, TXT, JPG/JPEG, and PNG; integrity is verified before content is returned
- `GET /api/documents/{id}/download`
- `GET /api/documents/{id}/integrity`
- `PUT /api/documents/{id}/rename`
- `DELETE /api/documents/{id}`

## Secure credentials
- `GET /api/credentials?search=` — masked
- `GET /api/credentials/{id}` — masked
- `GET /api/credentials/{id}/reveal` — decrypted owner-only response
- `POST /api/credentials`
- `PUT /api/credentials/{id}`
- `DELETE /api/credentials/{id}`

When a credential is saved with a new category name, that category is created automatically for the current user.

## Administrator
- `GET /api/admin/dashboard`
- `GET /api/admin/users`
- `PATCH /api/admin/users/{id}/status`
- `GET /api/admin/settings`
- `PUT /api/admin/settings/{key}`
