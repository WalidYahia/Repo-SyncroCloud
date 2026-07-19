# SyncroCloud — User-Management Flows (Dashboard → Mobile Port)

Exactly how the **web dashboard** implements user management, screen by screen, with the pages, user actions, and the **API calls in the order they fire**. Use this as the blueprint to reproduce the same behaviour in the mobile app.

Covers: **user create / update / delete**, **user↔device assignment**, and **user-device sensor permissions**.

- **Base URL:** `{host}/api` (dashboard reads it from `environment.apiUrl`).
- **Auth:** JWT Bearer on every call — `Authorization: Bearer {accessToken}`.
- Companion refs: capability/role rules in [Client-User-Management-Guide.md](./Client-User-Management-Guide.md); control policy in [User-Device-Control-Policy.md](./User-Device-Control-Policy.md); endpoint bodies in [Mobile-API-Users-Roles-Auth.md](./Mobile-API-Users-Roles-Auth.md).

> **Gating note (matches the dashboard):** the dashboard shows the Users area only when the caller holds the `CreateEditUser` privilege, and the device-access screen only with `AssignDeviceSensorToUser`. Privileges drive *visibility*; the **server still enforces role + tenant** and can return `403`. Mirror both in mobile.

---

## Screen map (dashboard routes → what mobile needs)

| Dashboard route | Purpose | Privilege to show it |
|---|---|---|
| `/users` | User list + create/edit/delete | `CreateEditUser` |
| `/users/:id/devices` | A user's assigned devices + sensor permissions | `AssignDeviceSensorToUser` |

Mobile needs the equivalents: a **Users list** screen, a **Create/Edit user** form, and a **User → Device access** screen with a **Sensor permissions** editor.

---

## 1. Users list screen (`/users`)

### On load — 3 calls (dashboard runs the first two in parallel, then the third)
```
GET /api/users                       → UserDto[]      (the table)
GET /api/users/roles                 → RoleDto[]      (role picker for create/edit)
GET /api/users/{currentUserId}/tenants → TenantDto[]  (the creator's own tenants, for the create form)
```

- `currentUserId` = the logged-in user's id (from the JWT / auth state).
- **Mobile adjustment for `TenantAdmin`:** `GET /api/users` is **`SuperAdmin`-only** on the server. A `TenantAdmin` must instead list per tenant: for each of their tenants call `GET /api/users/tenant/{tenantId}` and merge. (The dashboard is primarily used by SuperAdmin, so it calls `GET /api/users` directly — don't copy that blindly.)

### `UserDto` (table row)
```jsonc
{
  "id": "guid",
  "phoneNumber": "+2010...",
  "email": "a@b.com | null",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "createdAt": "2026-07-15T09:00:00Z",
  "isActive": true,
  "roles": ["TenantAdmin"]
}
```
Columns shown: name (`firstName lastName`), phone, email, roles, status (active/inactive), createdAt, actions (edit / delete / manage device access).

### `RoleDto` = `{ "id": "guid", "name": "TenantAdmin" }`

---

## 2. Create user

**Action:** tap **New User** → form dialog → **Save**.

### Form fields & validation (exactly as dashboard)
| Field | Required | Rule |
|---|---|---|
| `phoneNumber` | ✅ | non-empty (this is the login identifier) |
| `password` | ✅ | min length **8** |
| `firstName` | ✅ | non-empty |
| `lastName` | ✅ | non-empty |
| `email` | ❌ | optional; empty → send `null` |
| `roleId` | ✅ | pick from `GET /api/users/roles` |
| `tenantIds` | conditional | see tenant logic below |

### Tenant-selection logic (important — replicate this)
The dashboard decides `tenantIds` from the chosen role and the creator's tenants:
- **Role is `SuperAdmin`** → `tenantIds = []` (cross-tenant; no tenant membership needed).
- **Creator has exactly one tenant** → auto-use all of the creator's tenants (no picker shown).
- **Creator has multiple tenants** → show a multi-select; **require at least one** selected.

### Call
```
POST /api/users
```
```jsonc
// CreateUserDto
{
  "phoneNumber": "+2010...",
  "password": "min-8-chars",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "roleId": "role-guid",
  "tenantIds": ["tenant-guid", ...],   // [] when SuperAdmin
  "email": "a@b.com"                    // or null
}
```
- **Success** → `201` `UserDto`; close form, refresh list.
- **Error** → surface `err.error.message ?? err.error.detail`. Common: `400` phone already registered; `403` role/tenant scope violation (e.g. a `TenantAdmin` trying to create a `SuperAdmin` — the server rejects it).

---

## 3. Edit user

**Action:** row → **Edit** → form dialog → **Save**.

### Form fields (prefilled from the row)
| Field | Notes |
|---|---|
| `email` | editable; empty → `null` |
| `firstName` | required |
| `lastName` | required |
| `isActive` | toggle |
| `roleId` | prefilled from `user.roles[0]` matched against `RoleDto[]` |

> **Phone number is not editable here** — the dashboard omits it deliberately (changing the login identifier is out of scope). Don't add it to the edit form.

### Calls — up to **two**, sequential
The dashboard splits profile changes from role changes:
```
1) PUT /api/users/{id}
   { "email": "...|null", "firstName": "...", "lastName": "...", "isActive": true }   // UpdateUserDto

2) (only if the role was changed)
   PATCH /api/users/{id}/role
   { "roleId": "new-role-guid" }                                                       // UpdateUserRoleDto
```
Logic: always `PUT` the profile; **then**, only if `roleId` differs from the original, `PATCH` the role. Refresh the list after the last call. (A `TenantAdmin` cannot set `SuperAdmin` → expect `403` on the `PATCH`.)

---

## 4. Delete user

**Action:** row → **Delete** → confirm → call.
```
DELETE /api/users/{id}   → 204
```
- Dashboard shows a native confirm first, then refreshes the list.
- **`SuperAdmin`-only** on the server — hide/disable this action for non-SuperAdmin, or expect `403`.

---

## 5. User → Device access screen (`/users/:id/devices`)

Here `:id` is the **target user's** id.

### On load — the dashboard does this (forkJoin, then a follow-up)
```
GET /api/users/{userId}              → UserDto      (header)
GET /api/devices/user/{userId}       → DeviceDto[]  (devices already assigned to the user)
GET /api/users/{userId}/tenants      → TenantDto[]

// then, for each of the user's tenants (parallel), to compute what CAN still be assigned:
GET /api/devices/tenant/{tenantId}   → DeviceDto[]
```
- **Assigned devices** = `GET /api/devices/user/{userId}` (this is the table).
- **Available to assign** = union of the user's tenants' devices **minus** already-assigned (dedup by `deviceId`).

### 5.1 Assign a device
**Action:** **Assign Device** → pick from the *available* list → confirm.
```
POST /api/devices/{deviceId}/users/{userId}   → DeviceUserDto
```
- Body is empty (`{}`). Idempotent: re-assigning returns the existing link unchanged.
- **New behaviour to know:** on a fresh link the server now **seeds every installed sensor at `Watch`** automatically, so the returned `DeviceUserDto.sensorPermissions` comes back pre-populated. Mobile doesn't need to set permissions to make the device usable at Watch level.
- Refresh the assigned list after.

### 5.2 Unassign a device
```
DELETE /api/devices/{deviceId}/users/{userId}   → 204
```
Dashboard confirms first, then refreshes.

### Authorization for this screen
All `…/devices/{deviceId}/users…` writes are **`SuperAdmin` / `TenantAdmin`** only, and a `TenantAdmin` is further limited to devices in **their own tenant** (else `403`). The single-link `GET …/users/{userId}` additionally allows a **user to read their own** link (self-service).

---

## 6. Sensor permissions (per assigned device)

**Action:** on an assigned device row → **Manage Sensor Access** → dialog → **Save Permissions**.

### On open — 2 calls (parallel)
```
GET /api/devicesensors/device/{deviceId}     → DeviceSensorDto[]   (all installed sensors)
GET /api/devices/{deviceId}/users/{userId}   → DeviceUserDto       (current permissions)
```

### Dialog behaviour (current dashboard)
- Lists **every installed sensor** of the device.
- Each sensor has an access level: **`Watch`** or **`Control`** (no "none" / exclusion — every listed sensor is granted).
- Default for a sensor with no existing permission = **`Watch`**; existing permissions load at their saved level.
- `DeviceSensorDto.id` is the identifier used as `sensorId` in the permission payload.

### Save — full replace
```
PUT /api/devices/{deviceId}/users/{userId}/permissions
```
```jsonc
// SensorPermissionDto[] — send the COMPLETE desired set (this replaces the whole list)
[
  { "sensorId": "installed-sensor-id-1", "access": "Watch" },
  { "sensorId": "installed-sensor-id-2", "access": "Control" }
]
```
- `access` values: `"Watch"` or `"Control"` (backend enum `Watch=0`, `Control=1` — the API accepts the string names as the dashboard sends them).
- It's a **replace, not a merge** — omitting a sensor drops its grant. Always send the full array.
- Returns the updated `DeviceUserDto`.

### `DeviceUserDto`
```jsonc
{
  "deviceId": "HUB-000123",
  "userId": "guid",
  "linkedAt": "2026-07-15T09:00:00Z",
  "sensorPermissions": [ { "sensorId": "...", "access": "Watch" } ]
}
```

> **Enforcement caveat (from [User-Device-Control-Policy.md](./User-Device-Control-Policy.md)):** `Watch` vs `Control` is currently **stored but not enforced** on the actuation endpoints — the remote-control routes don't yet check it. The mobile app should still honour it in the UI (disable actuation for `Watch`), but be aware the server does not yet reject a bypassing control call.

---

## 7. End-to-end sequence (happy path)

```
Users list
  GET /api/users               (or GET /api/users/tenant/{tenantId} for TenantAdmin)
  GET /api/users/roles
  GET /api/users/{me}/tenants

Create
  POST /api/users

Edit
  PUT   /api/users/{id}
  PATCH /api/users/{id}/role           (only if role changed)

Delete
  DELETE /api/users/{id}               (SuperAdmin only)

Device access (per user)
  GET  /api/users/{id}
  GET  /api/devices/user/{id}
  GET  /api/users/{id}/tenants
  GET  /api/devices/tenant/{tenantId}  (per tenant → available list)
  POST   /api/devices/{deviceId}/users/{id}     (assign → auto Watch on all sensors)
  DELETE /api/devices/{deviceId}/users/{id}     (unassign)

Sensor permissions (per device)
  GET /api/devicesensors/device/{deviceId}
  GET /api/devices/{deviceId}/users/{id}
  PUT /api/devices/{deviceId}/users/{id}/permissions   (full replace)
```

---

## 8. Reference — request DTOs (from the dashboard models)

```ts
// CreateUserDto
{ phoneNumber; password; firstName; lastName; roleId; tenantIds: string[]; email?: string | null }

// UpdateUserDto
{ email: string | null; firstName; lastName; isActive: boolean }

// UpdateUserRoleDto
{ roleId: string }

// SensorPermissionDto
{ sensorId: string; access: 'Watch' | 'Control' }
```

---

## 9. Gotchas to carry into mobile

1. **`GET /api/users` is SuperAdmin-only.** For `TenantAdmin`, list per tenant (`/api/users/tenant/{id}`) instead — copying the dashboard's `getAll()` verbatim will `403` for tenant admins.
2. **Edit = up to two calls.** Profile (`PUT`) and role (`PATCH`) are separate; only `PATCH` when the role actually changed.
3. **Phone isn't editable** on update; it's required and unique on create.
4. **Tenant logic on create** depends on role (SuperAdmin → none) and how many tenants the creator has (1 → implicit, many → required multi-select).
5. **Assign auto-grants Watch** on all sensors now — no extra permissions call needed for basic access.
6. **Permissions PUT is a full replace** — always send the complete list.
7. **Delete is SuperAdmin-only**; hide it otherwise.
8. **Handle `403` gracefully** everywhere — privileges gate the UI, but role+tenant is the real (server) boundary.
