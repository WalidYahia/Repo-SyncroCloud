using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record SensorDto(Guid SensorId, string Name, UnitType UnitType, SensorType Type, ConnectionProtocol ConnectionProtocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta);

public record CreateSensorDto(string Name, UnitType UnitType, SensorType Type, ConnectionProtocol ConnectionProtocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta);

public record UpdateSensorDto(string Name, UnitType UnitType, SensorType Type, ConnectionProtocol ConnectionProtocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta);
