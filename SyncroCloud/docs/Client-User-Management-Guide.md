# SyncroCloud — Client User-Management Guide

How a client app (mobile or web) manages **users** once a session is established, broken down by **role** and **privilege**.

This is a task/capability guide. For exact request/response bodies of every endpoint, see the companion reference: [Mobile-API-Users-Roles-Auth.md](./Mobile-API-Users-Roles-Auth.md).

- **Base URL:** `{host}/api`
- **Auth:** JWT Bearer — `Authorization: Bearer {accessToken}` on every call below.
- All IDs are GUID strings (device IDs are plain strings, not relevant here).

---

## 1. The two-layer permission model (read this first)

SyncroCloud gates user management on **two independent layers**. A client must respect both:

| Layer | What it is | Where it's enforced | What the client does with it |
|---|---|---|---|
| **Privileges** | Fine-grained codes (e.g. `CreateEditUser`) attached to a user's role. | **Client UI only** — the API's user endpoints do **not** check privileges. | Show / hide buttons and screens (UX gating). |
| **Roles + tenant scope** | Built-in role (`SuperAdmin` / `TenantAdmin` / `User`) plus which tenant(s) the user belongs to. | **Server** — every `/api/users` and `/api/devices/{id}/users` endpoint. | Cannot be bypassed; expect `403` if violated. |

> **Golden rule:** use **privileges** to decide what to *show*, but never assume a shown action will succeed — the **server enforces by role and tenant**. Always handle `403 Forbidden` gracefully (hide/disable rather than error-spam).

Concretely:
- The `CreateEditUser` privilege makes the client *display* the "New User" / "Edit" controls.
- The server still requires the caller to be `SuperAdmin`, or a `TenantAdmin` acting **within their own tenant** and **not granting `SuperAdmin`** — otherwise `403`.

---

## 2. Bootstrap: what to load right after login

```
GET /api/profile        → UserProfileDto
```

```jsonc
{
  "userId": "guid",
  "phoneNumber": "+2012...",
  "email": "a@b.com | null",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "roles": ["TenantAdmin"],          // built-in role(s)
  "privileges": ["CreateEditUser", "AssignDeviceSensorToUser", ...]
}
```

Cache `roles` and `privileges` for the session and drive the whole UI from them. Re-fetch after any action that could change the caller's own role.

---

## 3. Roles and their default privileges

Three built-in roles are seeded. **Built-in roles cannot be deleted.**

| Role | Default seeded privileges | Scope |
|---|---|---|
| `SuperAdmin` | **All 8 privileges** | Entire system, all tenants. |
| `TenantAdmin` | **All 8 privileges** | **Only the tenant(s) they belong to.** |
| `User` | **None** | Themselves only (self-service). |

Custom roles can be created (`POST /api/roles`) with any subset of privileges. `SuperAdmin` and `TenantAdmin` are seeded with the full set, but a role's privilege set can be edited afterward (`PUT /api/roles/{id}/privileges`), so **always read privileges from `/api/profile`, never hard-code them per role name.**

---

## 4. Privileges relevant to user management

Of the 8 system privileges, these drive **user-management** UI:

| Privilege code | Display name | Gates (in the client) |
|---|---|---|
| `CreateEditUser` | Can Create User/Edit | "New User" button, Edit user, Delete user, Change role. |
| `AssignDeviceSensorToUser` | Can Assign device/sensor to user | "Manage Device Access" (assign/unassign devices, set sensor permissions). |
| `ManageRoles` | Can Manage Roles | Roles screen (create role, edit privileges). |
| `ManageTenants` | Can Manage Tenants | Tenants screen (needed to pick tenants when creating users). |

The remaining privileges (`DefineSensor`, `CreateDevice`, `ManageSensorToDevice`, `ManageScenarioToDevice`) concern device/sensor management, not user management.

---

## 5. Capability matrix (server-enforced) — user operations

`✓` = allowed · `✗` = `403 Forbidden` · `Self` = allowed only when the target user **is the caller**.
`TenantAdmin*` = allowed **only within their own tenant(s)** and **never granting/assigning `SuperAdmin`**.

| Operation | Endpoint | SuperAdmin | TenantAdmin | User |
|---|---|:--:|:--:|:--:|
| List **all** users | `GET /api/users` | ✓ | ✗ | ✗ |
| List users of a tenant | `GET /api/users/tenant/{tenantId}` | ✓ | ✓* | ✗ |
| Get one user | `GET /api/users/{id}` | ✓ | ✓* | Self |
| Get a user's tenants | `GET /api/users/{id}/tenants` | ✓ | ✓* | Self |
| List assignable roles | `GET /api/users/roles` | ✓ | ✓ | ✗ |
| Create user | `POST /api/users` | ✓ | ✓* | ✗ |
| Edit user (name/email/active) | `PUT /api/users/{id}` | ✓ | ✓* | Self |
| Change a user's role | `PATCH /api/users/{id}/role` | ✓ | ✓* | ✗ |
| Add user to tenant | `POST /api/users/{id}/tenants/{tenantId}` | ✓ | ✓* | ✗ |
| Remove user from tenant | `DELETE /api/users/{id}/tenants/{tenantId}` | ✓ | ✓* | ✗ |
| Delete user | `DELETE /api/users/{id}` | ✓ | ✗ | ✗ |

**Device access management** (part of managing a user), all `TenantAdmin*` scoped by the **device's** tenant:

| Operation | Endpoint | SuperAdmin | TenantAdmin | User |
|---|---|:--:|:--:|:--:|
| List a device's users | `GET /api/devices/{deviceId}/users` | ✓ | ✓* | ✗ |
| Get a user↔device link | `GET /api/devices/{deviceId}/users/{userId}` | ✓ | ✓* | ✗ |
| Assign device to user | `POST /api/devices/{deviceId}/users/{userId}` | ✓ | ✓* | ✗ |
| Unassign device | `DELETE /api/devices/{deviceId}/users/{userId}` | ✓ | ✓* | ✗ |
| Set per-sensor permissions | `PUT /api/devices/{deviceId}/users/{userId}/permissions` | ✓ | ✓* | ✗ |

> To see a user's assigned devices from the "user" side, use `GET /api/devices/user/{userId}`.

### How "tenant scope" is decided (for `TenantAdmin`)
- **Manage a tenant** (create user in it, add/remove membership, list its users): the tenant must be one the `TenantAdmin` belongs to.
- **Manage a user**: allowed if the target user **shares at least one tenant** with the caller (or is the caller).
- **Manage a device's users**: allowed if the **device's tenant** is one the caller belongs to.

---

## 6. Client-side gating recipe

Drive visibility from privileges; drive correctness from handling `403`.

```ts
// pseudo-code, after loading profile
const can = (code) => profile.privileges.includes(code);
const isSuperAdmin  = profile.roles.includes('SuperAdmin');

showNewUserButton      = can('CreateEditUser');
showEditUserButton     = can('CreateEditUser');
showDeleteUserButton   = can('CreateEditUser') && isSuperAdmin; // server: delete is SuperAdmin-only
showChangeRoleControl  = can('CreateEditUser');
showDeviceAccessButton = can('AssignDeviceSensorToUser');
showRolesScreen        = can('ManageRoles');

// When populating the role dropdown for a non-SuperAdmin creator,
// omit the SuperAdmin role — the server rejects it anyway.
assignableRoles = rolesFromApi.filter(r => isSuperAdmin || r.name !== 'SuperAdmin');
```

On any `403` from a user endpoint: treat as "not permitted in this context" — hide/disable the control and, if it came from an outdated privilege cache, re-fetch `/api/profile`.

---

## 7. Per-role playbooks

### 7.1 `SuperAdmin`
Full, cross-tenant user administration.
1. **List** everyone: `GET /api/users`.
2. **Create** a user (any role including `SuperAdmin`, any tenant(s)): `GET /api/users/roles` → `POST /api/users` with `roleId` + `tenantIds`.
3. **Edit / change role / tenant membership / device access:** any endpoint in §5.
4. **Delete** a user: `DELETE /api/users/{id}` (only `SuperAdmin` can).
5. **Roles:** full CRUD via `/api/roles`, including deleting non-built-in roles.

### 7.2 `TenantAdmin`
Same capabilities as `SuperAdmin` **but bounded to their own tenant(s)** and **cannot**:
- List *all* users (`GET /api/users` → `403`) — use `GET /api/users/tenant/{tenantId}` per tenant instead.
- Grant or assign the `SuperAdmin` role (`POST /api/users` / `PATCH /api/users/{id}/role` with a `SuperAdmin` role id → `403`).
- Delete users (`DELETE /api/users/{id}` → `403`).
- Delete roles (`DELETE /api/roles/{id}` → `403`).

Typical flow: load tenants (`GET /api/users/{selfId}/tenants`) → for each, `GET /api/users/tenant/{tenantId}` → manage those users; when creating, restrict the tenant picker to the caller's own tenants and the role picker to non-`SuperAdmin` roles.

### 7.3 `User` (standard, no privileges)
Self-service only. A plain user has **no** user-management privileges, so the client should show **none** of the admin user screens. What still works for them:
- `GET /api/profile` — their identity/roles/privileges.
- `GET /api/users/{selfId}` and `GET /api/users/{selfId}/tenants` — read their own record.
- `PUT /api/users/{selfId}` — update **their own** first/last name, email (phone number is **not** editable here).
- `GET /api/devices/user/{selfId}` — the devices assigned to them.

They **cannot** change their own role, list other users, or manage other users/tenants (all `403`).

---

## 8. Common flows (endpoint · who · notes)

**Create a user**
1. `GET /api/users/roles` → pick a `roleId` (non-`SuperAdmin` unless caller is `SuperAdmin`).
2. Pick `tenantIds` (must be within caller's scope unless `SuperAdmin`).
3. `POST /api/users` → `201` `UserDto`, or `400` (e.g. phone already registered), or `403` (scope/role violation).

**Edit a user** — `PUT /api/users/{id}` with `{ email, firstName, lastName, isActive }`. Phone cannot be changed here.

**Change a user's role** — `PATCH /api/users/{id}/role` with `{ roleId }`. `TenantAdmin` cannot set `SuperAdmin`.

**Tenant membership** — `POST` / `DELETE /api/users/{id}/tenants/{tenantId}` → `204`.

**Manage device access for a user**
1. `GET /api/devices/user/{userId}` → devices already assigned.
2. `POST /api/devices/{deviceId}/users/{userId}` to assign; `DELETE` to unassign.
3. `PUT /api/devices/{deviceId}/users/{userId}/permissions` with `SensorPermissionDto[]` to scope which sensors the user sees.

**Delete a user** — `DELETE /api/users/{id}` (`SuperAdmin` only).

---

## 9. Gotchas

- **Privilege ≠ permission.** A user could hold `CreateEditUser` (e.g. via a custom role) yet still be blocked by role/tenant rules on the server. Show on privilege, but always handle `403`.
- **Delete is `SuperAdmin`-only** even though `CreateEditUser` shows the button — hide delete for non-`SuperAdmin`, or expect `403`.
- **No `SuperAdmin` escalation.** `TenantAdmin`s can never create or promote to `SuperAdmin`; filter it out of role pickers.
- **Multi-tenant users.** A user can belong to several tenants; a `TenantAdmin` can manage them as long as **one** shared tenant exists.
- **Read privileges live.** Role→privilege mappings are editable, so always source privileges from `/api/profile`, never from the role name.
- **Self-management is always allowed** for read/update of one's own record (but not role changes).

---

## 10. Quick reference

| Need to… | Call |
|---|---|
| Know what the current user can do | `GET /api/profile` |
| List roles to assign | `GET /api/users/roles` |
| List users (all / by tenant) | `GET /api/users` · `GET /api/users/tenant/{tenantId}` |
| Create / edit / delete user | `POST` · `PUT /api/users/{id}` · `DELETE /api/users/{id}` |
| Change role | `PATCH /api/users/{id}/role` |
| Tenant membership | `POST`/`DELETE /api/users/{id}/tenants/{tenantId}` |
| Device access for a user | `GET /api/devices/user/{userId}` · `POST`/`DELETE /api/devices/{deviceId}/users/{userId}` |
| Manage roles/privileges | `/api/roles` (see reference doc §5) |
