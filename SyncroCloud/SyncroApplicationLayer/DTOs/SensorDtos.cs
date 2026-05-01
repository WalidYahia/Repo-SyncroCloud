using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record SensorDto(
    Guid SensorId, string Name,
    SensorType Type, ConnectionProtocol ConnectionProtocol,
    string BaseUrl, string PortNo,
    string DataPath, string InfoPath, string InchingPath,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta);

public record CreateSensorDto(
    string Name,
    SensorType Type, ConnectionProtocol ConnectionProtocol,
    string BaseUrl, string PortNo,
    string DataPath, string InfoPath, string InchingPath,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta);

public record UpdateSensorDto(
    string Name,
    SensorType Type, ConnectionProtocol ConnectionProtocol,
    string BaseUrl, string PortNo,
    string DataPath, string InfoPath, string InchingPath,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta);
