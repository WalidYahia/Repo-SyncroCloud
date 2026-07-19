# SyncroCloud — User ↔ Device Control Policy

How access and control of **devices** by **users** is governed: who is linked to a device, how per-sensor permissions work, who may assign them, how tenant scope bounds everything, and — importantly — **which parts are enforced by the server today versus only stored/gated in the client.**

Companion docs: [Client-User-Management-Guide.md](./Client-User-Management-Guide.md) (roles/privileges for user admin) and [Mobile-API-Users-Roles-Auth.md](./Mobile-API-Users-Roles-Auth.md) (auth/endpoint reference).

- **Base URL:** `{host}/api`
- **Auth:** JWT Bearer — `Authorization: Bearer {accessToken}`.
- Device IDs are plain strings (the hub serial/id); user/tenant IDs are GUIDs.

---

## 1. The model in one picture

```
Tenant ──< TenantUser >── User
  │                         │
  └──< Device               └──< DeviceUser >── Device
                                    │
                                    └─ SensorPermissions: [ { sensorId, access: Watch|Control } ]
```

- A **Device** belongs to exactly one **Tenant** (`Device.TenantId`).
- A **User** belongs to one or more **Tenants** (via `TenantUser`).
- A **User** is granted access to a **Device** through the **`DeviceUser`** bridge (many-to-many).
- Each `DeviceUser` link carries a **`SensorPermissions`** list — per-sensor access at either **`Watch`** (read-only) or **`Control`** (may actuate) level.

`DeviceUser` (composite PK `{DeviceId, UserId}`) is defined in
[SyncroInfraLayer/Entities/DeviceUser.cs](../SyncroInfraLayer/Entities/DeviceUser.cs); `SensorPermissions` is a JSONB column
(configured in [DeviceUserConfiguration.cs](../SyncroInfraLayer/Data/Configurations/DeviceUserConfiguration.cs), default `"[]"`, cascade-delete with both device and user).

---

## 2. The `DeviceUser` link — what it controls

The `DeviceUser` row is the unit of **device access**. Its presence answers "can this user see this device?"; its `SensorPermissions` list answers "which of the device's sensors, and at what level?".

```jsonc
// DeviceUserDto
{
  "deviceId": "HUB-000123",
  "userId": "guid",
  "linkedAt": "2026-07-15T09:00:00Z",
  "sensorPermissions": [
    { "sensorId": "installed-sensor-guid-1", "access": "Watch"   },  // 0 = read-only
    { "sensorId": "installed-sensor-guid-2", "access": "Control" }   // 1 = may actuate
  ]
}
```

`SensorAccessLevel` ([Enums.cs](../SyncroInfraLayer/Enums/Enums.cs)):

| Level | Value | Intended meaning |
|---|---|---|
| `Watch` | `0` | See the sensor and its readings; **no** actuation. |
| `Control` | `1` | Everything `Watch` allows **plus** turn on/off, inching, etc. |

> `SensorPermissions` is a **full-replace** list. `PUT …/permissions` overwrites the entire array — send the complete desired set every time, not a delta. A sensor absent from the list is treated as "no explicit grant."

---

## 3. How a user becomes linked to a device

### 3.1 Automatically, on device creation
When a device is created (`POST /api/devices`, `[Authorize]`), `DeviceService.CreateAsync(dto, createdByUserId)` auto-links, as `DeviceUser` rows:

1. **Every `TenantAdmin` of the device's tenant** (`UserService.GetTenantAdminIdsAsync(tenantId)` — the tenant's members intersected with `TenantAdmin` role membership), **plus**
2. **The creating user** (the JWT caller), deduplicated.

Each link starts with **empty** `SensorPermissions` (`"[]"`). So a fresh device is visible to the tenant's admins and its creator, but with no per-sensor grants until someone sets them.

### 3.2 Manually, by an admin
Via the device-user endpoints (see §5). `AssignUserAsync` is **idempotent** — re-assigning an already-linked user is a no-op that returns the existing link (permissions preserved).

---

## 4. Who may assign / manage device access

All device-user management endpoints are `[Authorize(Roles = SuperAdmin,TenantAdmin)]` **and** additionally scoped in-code by `CanManageDeviceAsync`:

- **`SuperAdmin`** — may manage any device's users (helper returns `true` unconditionally).
- **`TenantAdmin`** — may manage a device's users **only when the device's tenant is one the admin belongs to** (`GetTenantsAsync(caller).Any(t => t.Id == device.TenantId)`). Otherwise `403 Forbidden`.
- **`User`** — cannot reach these endpoints (`403` from the role gate).

See [DevicesController.cs](../SyncroCloudApi/Controllers/DevicesController.cs) `CanManageDeviceAsync` (lines 110–120).

---

## 5. Endpoints — device access & permissions

`✓` = allowed · `✗` = `403` · `TenantAdmin*` = only for devices in the caller's own tenant(s).

| Operation | Endpoint | SuperAdmin | TenantAdmin | User |
|---|---|:--:|:--:|:--:|
| List a device's users | `GET /api/devices/{deviceId}/users` | ✓ | ✓* | ✗ |
| Get one user↔device link (incl. permissions) | `GET /api/devices/{deviceId}/users/{userId}` | ✓ | ✓* | ✗ |
| Assign user to device | `POST /api/devices/{deviceId}/users/{userId}` | ✓ | ✓* | ✗ |
| Unassign user from device | `DELETE /api/devices/{deviceId}/users/{userId}` | ✓ | ✓* | ✗ |
| Replace a user's sensor permissions | `PUT /api/devices/{deviceId}/users/{userId}/permissions` | ✓ | ✓* | ✗ |

**Setting permissions** — body is the complete `SensorPermissionDto[]`:

```http
PUT /api/devices/HUB-000123/users/{userId}/permissions
[
  { "sensorId": "installed-sensor-guid-1", "access": 0 },   // Watch
  { "sensorId": "installed-sensor-guid-2", "access": 1 }    // Control
]
```

---

## 6. What the link controls today — enforced vs. advisory

This is the part to read carefully. The data model expresses a rich per-sensor Watch/Control policy, but the **runtime enforcement is currently partial.**

### 6.1 Enforced: device **visibility** is scoped by the link
`GET /api/devices` returns **only the devices linked to the caller** — it calls `service.GetByUserAsync(CurrentUserId)` over the `DeviceUser` bridge ([DevicesController.cs](../SyncroCloudApi/Controllers/DevicesController.cs) line 14–17). So a user's device *list* is genuinely scoped by their `DeviceUser` rows.

### 6.2 **Not** enforced today: the Watch vs. Control level on actuation
The remote-control endpoints that actually actuate hardware —
[RemoteActionsController.cs](../SyncroCloudApi/Controllers/RemoteActionsController.cs) (`turn-on`, `turn-off`, `inching/enable`, `inching/disable`, scenarios) —
currently have **no `[Authorize]` attribute and perform no `SensorPermissions` check.** They publish the MQTT `RemoteAction` command to the hub and log the action; they do **not** verify that the caller holds `Control` (or even `Watch`) on the target sensor, nor that the caller is linked to the device at all.

**Consequence:** as implemented, `SensorAccessLevel.Watch` vs `Control` is **stored metadata**, not an enforced gate. A client that respects the policy will hide/disable actuation for `Watch`-only sensors, but the API will not reject a control call that bypasses that UI.

### 6.3 Several device endpoints are also unauthenticated
On [DevicesController.cs](../SyncroCloudApi/Controllers/DevicesController.cs), only `GET /` (list-mine) and `POST /` (create) plus the device-user block carry `[Authorize]`. `GET /tenant/{tenantId}`, `GET /user/{userId}`, `GET /{id}`, `PUT /{id}`, `PATCH /{id}/status`, and `DELETE /{id}` have **no** authorization attribute and are reachable anonymously.

> **Policy status:** The intended policy is "a user may watch the sensors granted at `Watch`+ and control those granted at `Control`, only on devices they are linked to." The **intended** rules are documented in §2–§5; the **enforcement gaps** in §6.2–§6.3 are known and should be closed server-side before the Watch/Control distinction can be relied on for security. Until then, treat client-side gating as UX only.

---

## 7. Recommended client behaviour (until §6 gaps are closed)

Drive the UI from the link + permissions, and don't rely on the API to reject a mis-scoped control call:

1. **Bootstrap the user's devices:** `GET /api/devices` → the devices they're linked to.
2. **Per device, resolve the caller's permissions:** an admin can read any link via `GET /api/devices/{deviceId}/users/{userId}`; for a self-service user, surface their granted sensors/levels from the same link data your backend exposes to them.
3. **Render per sensor:**
   - not in `sensorPermissions` → hide the sensor (or show as no-access).
   - `Watch` → show readings; **disable** all actuation controls (on/off, inching).
   - `Control` → show readings **and** enable actuation.
4. **Actuate** (only for `Control` sensors) via `POST /api/remote-actions/{hubId}/sensors/{installedSensorId}/turn-on` (or `turn-off`, `inching/enable`, `inching/disable`).
5. Handle `403`/`401` defensively — hide or disable, don't error-spam — since server-side scoping on these routes may tighten.

---

## 8. Lifecycle & edge cases

- **Cascade delete:** deleting a device or a user removes its `DeviceUser` rows automatically (FK cascade). No orphan links.
- **Idempotent assign:** re-`POST`ing an existing link returns it unchanged — safe to retry; it will **not** wipe existing `SensorPermissions`.
- **Permissions are replace-not-merge:** always `PUT` the full desired `SensorPermissionDto[]`. Omitting a sensor revokes its grant.
- **Empty grants by default:** auto-linked admins/creator start at `"[]"` — visible device, zero sensor grants until set.
- **Tenant move:** a device's tenant defines who can *manage* its users (§4). If a device changes tenant, previously-linked users remain linked, but management authority follows the **new** tenant's admins. (Re-review links after a tenant move.)
- **Multi-tenant admins:** a `TenantAdmin` in several tenants can manage device access for devices in any of those tenants.

---

## 9. Quick reference

| Need to… | Call |
|---|---|
| List devices the current user can access | `GET /api/devices` |
| List all users linked to a device | `GET /api/devices/{deviceId}/users` |
| Read a user's link + sensor permissions | `GET /api/devices/{deviceId}/users/{userId}` |
| Grant a user access to a device | `POST /api/devices/{deviceId}/users/{userId}` |
| Revoke a user's device access | `DELETE /api/devices/{deviceId}/users/{userId}` |
| Set which sensors (Watch/Control) a user gets | `PUT /api/devices/{deviceId}/users/{userId}/permissions` |
| Actuate a sensor (Control) | `POST /api/remote-actions/{hubId}/sensors/{installedSensorId}/turn-on` · `…/turn-off` · `…/inching/enable` · `…/inching/disable` |

---

## 10. Known gaps to close (server-side TODO)

1. Add `[Authorize]` + a `Control`-level `SensorPermissions` check on every `RemoteActionsController` actuation endpoint (§6.2).
2. Add authorization to the currently-anonymous device read/update/delete routes (§6.3).
3. Consider enforcing `Watch` for read endpoints that expose a sensor's readings to non-admin users.

Until these land, the Watch/Control policy is **advisory** (client-enforced), not server-enforced.
