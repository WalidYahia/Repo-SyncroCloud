using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record SensorDto(Guid SensorId, string Name, UnitType UnitType, SensorType Type, ConnectionProtocol ConnectionProtocol);

public record CreateSensorDto(string Name, UnitType UnitType, SensorType Type, ConnectionProtocol ConnectionProtocol);

public record UpdateSensorDto(string Name, UnitType UnitType, SensorType Type, ConnectionProtocol ConnectionProtocol);
