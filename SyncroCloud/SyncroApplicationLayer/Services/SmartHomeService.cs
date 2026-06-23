using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroApplicationLayer.Services;

public class SmartHomeService(
    IUserService userService,
    IDeviceService deviceService,
    IDeviceSensorService sensorService,
    IMqttService mqtt) : ISmartHomeService
{
    public async Task<IReadOnlyList<DeviceSensorDto>> DiscoverAsync(Guid userId)
    {
        var tenants = await userService.GetTenantsAsync(userId);
        var result  = new List<DeviceSensorDto>();

        foreach (var tenant in tenants)
        {
            var devices = await deviceService.GetByTenantAsync(tenant.Id);
            foreach (var device in devices)
            {
                var sensors = await sensorService.GetByDeviceAsync(device.DeviceId);
                result.AddRange(sensors.Where(s => s.IsActive));
            }
        }

        return result;
    }

    public Task<DeviceSensorDto?> GetSensorAsync(string installedSensorId) =>
        sensorService.GetByIdAsync(installedSensorId);

    public async Task<RemoteActionAckDto> TurnOnAsync(string installedSensorId, CancellationToken ct)
    {
        var sensor = await sensorService.GetByIdAsync(installedSensorId)
            ?? throw new KeyNotFoundException($"Sensor '{installedSensorId}' not found");
        return await mqtt.TurnOnUnitAsync(sensor.DeviceId, installedSensorId, ct);
    }

    public async Task<RemoteActionAckDto> TurnOffAsync(string installedSensorId, CancellationToken ct)
    {
        var sensor = await sensorService.GetByIdAsync(installedSensorId)
            ?? throw new KeyNotFoundException($"Sensor '{installedSensorId}' not found");
        return await mqtt.TurnOffUnitAsync(sensor.DeviceId, installedSensorId, ct);
    }
}
