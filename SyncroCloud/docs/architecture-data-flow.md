# SyncroCloud — System Data Flow Architecture

## System Components

- **Client** — Mobile app / Angular dashboard
- **API** — SyncroCloud REST API
- **Database** — PostgreSQL (via EF Core)
- **MQTT Broker** — Message bus between cloud and devices
- **Device** — SmartGuardHub (controls units locally)
- **Unit/Sensor** — Physical hardware (e.g., SonOff switches)

---

## Core Principle

> **The device is the single source of truth for sensor state. The cloud mirrors that state.**

This one rule, applied consistently, eliminates nearly all synchronization problems.

---

## Scenario-by-Scenario Analysis

### 1. Remote Action — On/Off (State Change)

**What needs to happen:**
- Unit physically changes state
- Action is logged (who, what, when, result)
- Current sensor state is persisted in DB

**Best flow:**

```
Client
  → POST /api/remote-actions/{hubId}/sensors/{id}/turn-on
  → API publishes MQTT RemoteAction (blocks, waits for ack)
  → Hub executes HTTP command on unit
  → Hub reads back state from unit (confirmation)
  → Hub publishes new state to data topic  ──────────────────────┐
  → Hub sends ack to API (result + new state snapshot in payload) │
  → API on ack received:                                          │
      ├─ Write DeviceActionLog (user, action, result, timestamp)  │
      ├─ Update DeviceSensor.LastReading from ack payload         │
      └─ Return result to client                                  │
  → MQTT listener receives data topic message ◄──────────────────┘
      └─ Update DB (idempotent safety net — handles race condition)
```

**Key point:** The ack payload must include the unit's current state so the API can update
the DB in the same round-trip, without waiting for a separate data-topic message. The
data-topic publish is still sent but acts as a redundant safety net.

---

### 2. Remote Update — Display Name

**What needs to happen:**
- DB updated (`DisplayName` is a cloud concept, not hardware)
- Hub updated (it also stores sensor names locally for its own operation)

**Problem with routing through MQTT first:** If the hub is offline, the client-facing
operation fails even though it is just renaming a label in the cloud.

**Best flow — separate concerns:**

```
Client
  → PUT /api/device-sensors/{id}/name  (not under remote-actions)
  → API updates DeviceSensor.DisplayName in DB immediately
  → API returns 200 to client (DB update is the authoritative operation)
  → API fires MQTT notification to hub (best-effort, no await, no block)
  → Hub (if online):  receives notification, updates its local config
  → Hub (if offline): on next reconnect, re-syncs from cloud via DeviceSensorConfig topic
```

**Why:** Display name is metadata owned by the cloud. The hub is a consumer of it, not
the owner. The sync direction is **cloud → hub** for naming.

---

### 3. Remote Action — Enable/Disable Inching (Hardware Config Change)

**What needs to happen:**
- Physical unit config is changed (hardware-level, cannot be faked)
- DB updated to reflect the new config — **only if the hardware actually changed**

**Best flow:**

```
Client
  → POST /api/remote-actions/{hubId}/sensors/{id}/inching/enable
  → API publishes MQTT RemoteAction (blocks, waits for ack)
  → Hub sends HTTP to unit to set inching config
  → Hub reads back config from unit (verify it was applied)
  → Hub sends ack (success + applied config values in payload)
  → API on SUCCESSFUL ack:
      ├─ Update DeviceSensor.IsInInchingMode + InchingModeWidthInMs in DB
      ├─ Write DeviceActionLog
      └─ Return success to client
  → API on FAILED ack:
      └─ Return error to client  ← DB is NOT touched (hardware unchanged)
```

**Key point:** DB update is conditional on hub ack success. The DB must reflect actual
hardware state, not intended state. If the hub says it failed, the DB stays as-is.

---

### 4. Local Actions — Client Communicates Directly with Device (Same LAN)

**Problem:** Cloud has no visibility into this path at all.

**Core contract:** After every local change, the hub **must** publish the updated state or
config to MQTT. This is an obligation, not an option. The cloud relies entirely on this.

There are two MQTT channels used for local → cloud sync:

| Channel | Topic | Used for |
|---|---|---|
| State channel | `Syncro/{deviceId}/sensors/{sensorId}/data` | On/Off state changes |
| Config channel | `Syncro/{deviceId}/DeviceSensorConfig` | Sensor install, name, inching config |

---

#### 4a. Local — Add Sensor

**What needs to happen:**
- Sensor is registered on hub and associated with a physical unit
- Cloud learns about the new sensor and creates a DB record

**Best flow:**

```
Client (on local network)
  → POST /hub-api/sensors/install  (Hub's own local API)
  → Hub validates and registers sensor locally
  → Hub communicates with unit (reads info, confirms connectivity)
  → Hub publishes full sensor list to MQTT DeviceSensorConfig topic
  → MQTT listener on Cloud:
      ├─ Calls SyncFromDeviceAsync (upsert — creates new DeviceSensor in DB)
      ├─ Write DeviceActionLog (source = "Local", action = "InstallSensor")
      └─ Push SignalR notification to connected clients
  → Hub returns success to client (local response, does not wait for cloud)
```

**Key point:** The hub generates the sensor ID using the same `ComputeId` formula as the
cloud (`{deviceId}_{sensorType}_{unitId}_{switch}_{addr}_{port}`). Both sides produce the
same deterministic ID — no coordination needed for ID assignment.

**Risk:** If the hub publishes to MQTT but the broker is unreachable, the cloud never
learns about the new sensor. The `IsPendingSync` flag (see Gap 4) handles this — on
reconnect the hub republishes its full config.

---

#### 4b. Local — Update Display Name

**What needs to happen:**
- Hub updates its local label for the sensor
- Cloud DB is updated to reflect the new name
- Other connected clients (dashboard, other mobile sessions) see the change

**Best flow:**

```
Client (on local network)
  → PUT /hub-api/sensors/{id}/name  (Hub's own local API)
  → Hub updates its local sensor config (display name)
  → Hub publishes full sensor list to MQTT DeviceSensorConfig topic
  → MQTT listener on Cloud:
      ├─ SyncFromDeviceAsync updates DeviceSensor.DisplayName in DB
      └─ Push SignalR notification to connected clients (name changed)
  → Hub returns success to client (local response)
```

**Ownership conflict:** Display name has two possible owners — cloud (remote update,
scenario 2) and hub (local update, this scenario). The rule is:

> **Last write wins, and the hub's publish is always treated as the authoritative state
> at the time it was made.**

When a remote rename (scenario 2) and a local rename race, the MQTT listener's
`SyncFromDeviceAsync` will overwrite the cloud's version if the hub publishes after.
This is acceptable: both writes carry valid intent, and eventual consistency is the goal.

---

#### 4c. Local — Enable/Disable Inching (Hardware Config Change)

**What needs to happen:**
- Inching config is applied to the physical unit
- Cloud DB is updated — **only if the hardware actually changed**

**Best flow:**

```
Client (on local network)
  → POST /hub-api/sensors/{id}/inching/enable  (Hub's own local API)
  → Hub sends HTTP to unit to set inching config
  → Hub reads back config from unit (verify it was applied)
  → On unit confirmation:
      ├─ Hub updates its local sensor record (IsInInchingMode, InchingTimeInMs)
      └─ Hub publishes full sensor list to MQTT DeviceSensorConfig topic
  → MQTT listener on Cloud:
      ├─ SyncFromDeviceAsync updates DeviceSensor.IsInInchingMode + InchingModeWidthInMs
      ├─ Write DeviceActionLog (source = "Local", action = "EnableInching")
      └─ Push SignalR notification to connected clients
  → On unit failure:
      └─ Hub returns error to client, does NOT publish to MQTT (DB stays as-is)
  → Hub returns result to client (local response)
```

**Key point:** Same rule as remote inching (scenario 3) — the DB update is conditional
on the unit confirming the config was applied. The hub enforces this locally before
publishing to MQTT.

---

#### 4d. Local — On/Off Action (State Change)

```
Client (on local network)
  → POST /hub-api/sensors/{id}/turn-on  (Hub's own local API)
  → Hub executes HTTP command on unit
  → Hub reads back state from unit
  → Hub publishes new state to MQTT data topic
  → MQTT listener on Cloud:
      ├─ Update DeviceSensor.LastReading in DB
      ├─ Write DeviceActionLog (source = "Local")
      └─ Push SignalR notification to connected clients
  → Hub returns result to client (local response)
```

---

### 5. Device-Initiated Action — Autonomous Change

Examples: a scenario fires, a physical button is pressed on the SonOff, a scheduled
task runs on the hub.

**Best flow:**

```
Physical event triggers on unit / hub scenario fires
  → Hub detects state change (event callback or periodic poll)
  → Hub reads current state from unit
  → Hub publishes to MQTT data topic (with event source metadata)
  → MQTT listener on Cloud:
      ├─ Update DeviceSensor.LastReading in DB
      ├─ Write DeviceActionLog (source = "Device", no user)
      └─ Push SignalR notification to all connected clients
```

**The hub's polling contract:** If the unit does not support push notifications (SonOff
does not push unsolicited events), the hub must poll. The `syncPeriodicity` field already
exists for this purpose. This is the mechanism that keeps the cloud in sync with
autonomous device changes.

---

## Cross-Cutting Gaps to Address

### Gap 1 — No Action Logging

There is no `DeviceActionLog` table. Every action (remote, local, device-initiated)
should create a log record with:

| Field | Description |
|---|---|
| `Id` | Primary key |
| `DeviceId` | Which device |
| `InstalledSensorId` | Which sensor |
| `Action` | e.g., TurnOn, EnableInching |
| `Source` | Remote / Local / Device |
| `TriggeredByUserId` | Null for device-initiated |
| `Result` | Ok / Error / Timeout |
| `StateBefore` | Snapshot of state before action |
| `StateAfter` | Snapshot of state after action |
| `Timestamp` | UTC |

---

### Gap 2 — Ack Payload Must Carry New State

Currently `RemoteActionAckDto.DevicePayload` is `JsonElement?` (opaque). For the DB
update to happen in the same round-trip, the hub must include the unit's current state
in the ack payload in a structured way. Otherwise the cloud has to wait for the
subsequent data-topic message — creating a window where the DB is stale after the API
has already returned 200 to the client.

---

### Gap 3 — No Real-Time Push to Clients

The MQTT listener updates the DB, but clients (dashboard/mobile) have no way to learn
about it without polling. **SignalR** should be added:

```
MQTT listener receives state update
  → Updates DB
  → Publishes event to SignalR hub
  → SignalR hub pushes to all connected clients subscribed to that device
```

This handles scenarios 4 and 5 transparently — clients see device and local changes
in real time without polling.

---

### Gap 4 — No Offline Command Queue

When the hub is offline, remote action calls fail (MQTT not connected, or hub not
subscribed). For actions in scenarios 1 and 3, the client gets an error. For scenario 2
(display name), the DB update succeeds but the hub never gets notified.

**Solution — pending sync flag:**
- Add `IsPendingSync` flag to `DeviceSensor`
- Set it when hub is offline and a config change could not be delivered
- On hub reconnect, hub sends its `DeviceSensorConfig` to cloud; cloud compares and
  pushes any pending changes back to the hub via `CloudSensorConfig` topic

---

### Gap 5 — Split Ownership of DisplayName

`UpdateUnitNameAsync` currently routes through `IMqttService` (hub first). It should
route through the data service (DB first), with a best-effort MQTT notification to the
hub. The ownership direction is wrong.

---

## Decision Summary

| Scenario | Trigger | DB Updated By | When | Hub Updated By |
|---|---|---|---|---|
| Remote — On/Off | Client → API | API (from ack payload) | After ack received | MQTT RemoteAction |
| Remote — Display Name | Client → API | API (direct DB write) | Immediately | MQTT notification (best-effort) |
| Remote — Inching Config | Client → API | API (from ack payload) | After **successful** ack only | MQTT RemoteAction |
| Local — Add Sensor | Client → Hub | MQTT listener (SyncFromDevice) | On DeviceSensorConfig message | Itself |
| Local — Display Name | Client → Hub | MQTT listener (SyncFromDevice) | On DeviceSensorConfig message | Itself |
| Local — Inching Config | Client → Hub | MQTT listener (SyncFromDevice) | On DeviceSensorConfig message, **only after unit confirms** | Itself |
| Local — On/Off | Client → Hub | MQTT listener | On data-topic message | Itself |
| Device-initiated | Physical event | MQTT listener | On data-topic message | Itself |

---

## Invariant

> **The DB is never updated with a state that the hub has not confirmed.**

| Path | Confirmation mechanism |
|---|---|
| Remote action (API → Hub) | Hub sends explicit MQTT ack with result |
| Remote config (API → Hub) | Hub sends explicit MQTT ack with applied config |
| Local action (Client → Hub) | Hub publishes to data topic only after unit confirms |
| Local config (Client → Hub) | Hub publishes to DeviceSensorConfig only after unit confirms |
| Device-initiated | Hub publishes to data topic after reading back unit state |

In every path the hub — not the client, not the API — decides what state is true and
publishes it. The cloud records what the hub reports, never what the client intended.
