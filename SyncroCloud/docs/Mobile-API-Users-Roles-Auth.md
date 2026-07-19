# SyncroCloud API — Users, Roles & Auth Reference (Mobile)

Reference for the mobile app developer covering **Authentication**, **User Profile**, **Users**, and **Roles/Privileges**.

- **Base URL:** `{host}/api` (e.g. `https://api.syncrocloud.example/api`)
- **Format:** JSON request/response bodies. Property names are `camelCase` on the wire.
- **Auth scheme:** JWT Bearer. Send `Authorization: Bearer {accessToken}` on every protected call.
- **IDs:** all resource ids are GUIDs (strings), except **device ids which are strings** (not relevant to this doc).

---

## 1. Concepts

### Roles (built-in)
| Role | Meaning |
|------|---------|
| `SuperAdmin` | Full system access, all tenants. |
| `TenantAdmin` | Manages users/devices **within their own tenant(s)** only. |
| `User` | Standard end user. |

A user can belong to **multiple tenants**. A `SuperAdmin` implicitly has access to all tenants.



### Identity rules
- **Phone number is mandatory and unique** — it is the primary login identifier.
- **Email is optional** (may be `null`). Login also accepts email if present.
- Login accepts **either phone or email** in a single `emailOrPhone` field.

---

## 2. Authentication — `/api/auth`

No auth required except `revoke`.

### `POST /api/auth/register` — Self-registration
Public. Always creates a plain `User` (role cannot be chosen here).

**Request**
```json
{
  "phoneNumber": "+201234567890",
  "password": "P@ssw0rd!",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "tenantId": "3f1c...-guid",
  "email": "ahmed@example.com"
}
```
`email` is optional (omit or `null`).

**Responses**
- `200 OK` → `{ "message": "User registered successfully" }`
- `400 Bad Request` → `{ "errors": ["<reason>", ...] }`

### `POST /api/auth/login`
**Request**
```json
{ "emailOrPhone": "+201234567890", "password": "P@ssw0rd!" }
```
`emailOrPhone` accepts the phone number **or** the email.

**Responses**
- `200 OK` → `TokenResponse` (below)
- `401 Unauthorized` → `{ "error": "Invalid credentials" }`

**TokenResponse**
```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "b64-random-string",
  "expiresAt": "2026-07-15T12:34:56Z"
}
```
`expiresAt` is the **access token** expiry (UTC). Store both tokens securely (e.g. Keychain / Keystore).

### `POST /api/auth/refresh`
Exchange a valid refresh token for a new token pair.

**Request** `{ "refreshToken": "<token>" }`
**Responses**
- `200 OK` → `TokenResponse`
- `401 Unauthorized` → `{ "error": "Invalid or expired refresh token" }`

### `POST /api/auth/revoke` 🔒
Requires `Authorization` header. Invalidates a refresh token (use on logout).

**Request** `{ "refreshToken": "<token>" }`
**Responses**
- `204 No Content` (revoked)
- `400 Bad Request` → `{ "error": "Token not found or already revoked" }`

> **Recommended token flow:** on `401` from any protected endpoint, call `/refresh` once; if that also fails, force re-login. On logout, call `/revoke` then discard tokens.

---

## 3. Current user profile — `/api/profile` 🔒

### `GET /api/profile`
Returns the authenticated user's identity, roles and privileges. Call this right after login to drive role/privilege-based UI.

**Response `200 OK` (`UserProfileDto`)**
```json
{
  "userId": "3f1c...-guid",
  "phoneNumber": "+201234567890",
  "email": "ahmed@example.com",
  "roles": ["TenantAdmin"],
  "privileges": ["CreateEditUser", "ManageSensorToDevice"]
}
```
- `email` may be `null`.
- `401 Unauthorized` if the token is missing/invalid.

---

## 4. Users — `/api/users` 🔒

All endpoints require authentication. Access is further restricted per-endpoint as noted. `TenantAdmin`s are scoped to **their own tenants**; `SuperAdmin` is unrestricted. A user may always read/update **their own** record.

### `UserDto` (response shape)
```json
{
  "id": "3f1c...-guid",
  "phoneNumber": "+201234567890",
  "email": "ahmed@example.com",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "createdAt": "2026-07-15T10:00:00Z",
  "isActive": true,
  "roles": ["User"]
}
```

### Endpoints

| Method & Path | Who can call | Purpose |
|---|---|---|
| `GET /api/users` | `SuperAdmin` | List all users. |
| `GET /api/users/roles` | `SuperAdmin`, `TenantAdmin` | List assignable roles → `RoleDto[]` `{ id, name }`. |
| `GET /api/users/tenant/{tenantId}` | `SuperAdmin`, `TenantAdmin`* | Users belonging to a tenant → `UserDto[]`. |
| `GET /api/users/{id}` | Self, `SuperAdmin`, `TenantAdmin`* | Single user → `UserDto`. |
| `GET /api/users/{id}/tenants` | Self, `SuperAdmin`, `TenantAdmin`* | Tenants the user belongs to → `TenantDto[]`. |
| `POST /api/users` | `SuperAdmin`, `TenantAdmin`* | Create a user. |
| `PUT /api/users/{id}` | Self, `SuperAdmin`, `TenantAdmin`* | Update name/email/active flag. |
| `PATCH /api/users/{id}/role` | `SuperAdmin`, `TenantAdmin`* | Change a user's role. |
| `POST /api/users/{id}/tenants/{tenantId}` | `SuperAdmin`, `TenantAdmin`* | Add user to a tenant. |
| `DELETE /api/users/{id}/tenants/{tenantId}` | `SuperAdmin`, `TenantAdmin`* | Remove user from a tenant. |
| `DELETE /api/users/{id}` | `SuperAdmin` | Delete a user. |

\* `TenantAdmin` only for tenants/users within their own tenant(s), and **cannot** grant or assign the `SuperAdmin` role. Violations return `403 Forbidden`.

### `POST /api/users` — Create user
**Request (`CreateUserDto`)**
```json
{
  "phoneNumber": "+201112223334",
  "password": "P@ssw0rd!",
  "firstName": "Sara",
  "lastName": "Mostafa",
  "tenantIds": ["3f1c...-guid", "9a2d...-guid"],
  "roleId": "7bd0...-guid",
  "email": "sara@example.com"
}
```
- `email` optional. `tenantIds` may hold one or more tenant ids. Get valid `roleId`s from `GET /api/users/roles`.

**Responses**
- `201 Created` → `UserDto` (with `Location` header to `GET /api/users/{id}`)
- `400 Bad Request` → `{ "message": "<reason, e.g. phone already registered>" }`
- `403 Forbidden` — TenantAdmin attempting a `SuperAdmin` grant or a tenant outside their scope.

### `PUT /api/users/{id}` — Update user
**Request (`UpdateUserDto`)** — note: **phone number cannot be changed here.**
```json
{ "email": "new@example.com", "firstName": "Sara", "lastName": "Mostafa", "isActive": true }
```
**Responses** `200 OK` → `UserDto` · `403 Forbidden` · `404 Not Found`.

### `PATCH /api/users/{id}/role` — Change role
**Request (`UpdateUserRoleDto`)** `{ "roleId": "7bd0...-guid" }`
**Responses** `200 OK` → `UserDto` · `403 Forbidden` · `404 Not Found`.

### Add / Remove tenant membership
- `POST /api/users/{id}/tenants/{tenantId}` → `204 No Content` (or `404`).
- `DELETE /api/users/{id}/tenants/{tenantId}` → `204 No Content` (or `404`).

**`TenantDto`**
```json
{ "id": "3f1c...-guid", "name": "Cairo HQ", "createdAt": "2026-01-01T00:00:00Z", "isActive": true }
```

---

## 5. Roles & Privileges — `/api/roles` 🔒

Requires `SuperAdmin` or `TenantAdmin` (delete is `SuperAdmin` only). Built-in roles (`SuperAdmin`, `TenantAdmin`, `User`) cannot be deleted.

| Method & Path | Purpose |
|---|---|
| `GET /api/roles/privileges` | All privileges in the system → `PrivilegeDto[]`. |
| `GET /api/roles` | All roles with their privileges → `RoleDetailDto[]`. |
| `GET /api/roles/{id}` | Single role → `RoleDetailDto`. |
| `POST /api/roles` | Create a role with privileges. |
| `PUT /api/roles/{id}/privileges` | Replace a role's privilege set. |
| `DELETE /api/roles/{id}` | Delete a non-built-in role (`SuperAdmin` only). |

**`PrivilegeDto`** `{ "id": "guid", "code": "CreateEditUser", "name": "Can Create User/Edit" }`

**`RoleDetailDto`**
```json
{
  "id": "7bd0...-guid",
  "name": "Technician",
  "privileges": [
    { "id": "guid", "code": "ManageSensorToDevice", "name": "Can Manage Sensors to device" }
  ]
}
```

### `POST /api/roles` — Create role
**Request (`CreateRoleDto`)**
```json
{ "name": "Technician", "privilegeIds": ["guid-1", "guid-2"] }
```
**Responses** `201 Created` → `RoleDetailDto` · `400 Bad Request` → `{ "message": "Role 'Technician' already exists." }`

### `PUT /api/roles/{id}/privileges` — Replace privileges
**Request (`UpdateRolePrivilegesDto`)** `{ "privilegeIds": ["guid-1", "guid-3"] }`
This **replaces** the entire privilege set (not additive).
**Responses** `200 OK` · `404 Not Found`.

---

## 6. Errors — common shapes

| Status | Body | When |
|---|---|---|
| `400 Bad Request` | `{ "message": "..." }` or `{ "errors": [...] }` | Validation / business rule failed. |
| `401 Unauthorized` | `{ "error": "..." }` | Missing/invalid/expired access token, bad credentials. |
| `403 Forbidden` | *(empty)* | Authenticated but not allowed (wrong role / outside tenant scope). |
| `404 Not Found` | `{ "message": "<Resource> with id '<id>' not found." }` | Resource missing. |
| `409 Conflict` | `{ "message": "..." }` | Unique-key conflict. |

---

## 7. Quick mobile flow

1. **Login** → `POST /api/auth/login`, store `accessToken` + `refreshToken`.
2. **Bootstrap UI** → `GET /api/profile`; use `roles`/`privileges` to gate screens/actions.
3. **Attach** `Authorization: Bearer {accessToken}` to every protected request.
4. **On 401** → try `POST /api/auth/refresh` once; on failure, re-login.
5. **Logout** → `POST /api/auth/revoke` with the refresh token, then clear stored tokens.
