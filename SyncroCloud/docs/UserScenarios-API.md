# User Scenarios API

Base path: `api/DeviceScenarios`

All scenarios for a device live together as one record server-side, but you always
read/write a **single scenario** at a time — the backend merges your change into
the device's scenario list automatically.

## Endpoints

### Get all scenarios for a device

```
GET /api/DeviceScenarios/device/{deviceId}
```

**200 OK** → `UserScenario[]`

### Get one scenario

```
GET /api/DeviceScenarios/device/{deviceId}/{scenarioId}
```

**200 OK** → `UserScenario`
**404 Not Found** → `{ "message": "Scenario with id '...' not found." }`

### Create a scenario

```
POST /api/DeviceScenarios/device/{deviceId}
Content-Type: application/json
```

Body: `UserScenario` — set `id` to `""` (server generates a GUID and returns it).

**201 Created** → `UserScenario` (with the assigned `id`)

### Update a scenario

```
PUT /api/DeviceScenarios/device/{deviceId}/{scenarioId}
Content-Type: application/json
```

Body: `UserScenario` — `id` in the body is ignored; the `{scenarioId}` in the URL wins.

**200 OK** → `UserScenario`
**404 Not Found** → scenario doesn't exist on this device

### Delete a scenario

```
DELETE /api/DeviceScenarios/device/{deviceId}/{scenarioId}
```

**204 No Content**
**404 Not Found** → scenario doesn't exist on this device

## Sample payload

```json
{
  "id": "e1273052-07d5-4d74-8236-398a2a576574",
  "name": "Morning",
  "isEnabled": true,
  "targetSensorId": "10016ca843_0",
  "action": "On",
  "logicOfConditions": "And",
  "conditions": [
    {
      "condition": "OnTime",
      "durationInSeconds": 0,
      "time": "11:08:00",
      "sensorsDependency": null
    },
    {
      "condition": "OnOtherSensorValue",
      "durationInSeconds": 0,
      "time": null,
      "sensorsDependency": [
        {
          "sensorId": "10016ca843_1",
          "sensorType": 1,
          "value": "1",
          "operator": "Equals"
        }
      ]
    }
  ]
}
```
