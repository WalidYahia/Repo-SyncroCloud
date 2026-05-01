using System.Text.Json;
using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record RemoteActionEnvelope(string RequestId, JsonCommandType JsonCommandType, JsonElement? CommandPayload);

public record RemoteActionAckDto(string RequestId, RemoteActionState State, JsonElement? DevicePayload);

// ── Mobile API request bodies ─────────────────────────────
public record EnableInchingRequest(string UnitId, int InchingTimeInMs);

public record DisableInchingRequest(string UnitId);

public record UpdateUnitNameRequest(string Name);

// ── Command payloads ──────────────────────────────────────
public record TurnUnitPayload(string InstalledSensorId);

public record EnableInchingPayload(string InstalledSensorId, string UnitId, int InchingTimeInMs);

public record DisableInchingPayload(string InstalledSensorId, string UnitId);

public record UpdateUnitNamePayload(string InstalledSensorId, string Name);

public record SaveScenarioPayload(MqttUserScenarioDto UserScenario);

public record DeleteScenarioPayload(DeleteScenarioIdDto UserScenario);

public record DeleteScenarioIdDto(string Id);

// ── Scenario structure ────────────────────────────────────
public record MqttUserScenarioDto(
    string Id,
    string Name,
    bool IsEnabled,
    string TargetSensorId,
    SwitchOutletStatus Action,
    ScenarioLogic LogicOfConditions,
    List<MqttScenarioConditionDto> Conditions);

public record MqttScenarioConditionDto(
    ScenarioCondition Condition,
    int DurationInSeconds,
    string? Time,
    List<MqttSensorDependencyDto>? SensorsDependency);

public record MqttSensorDependencyDto(
    string SensorId,
    int SensorType,
    string Value,
    ScenarioOperator Operator);
