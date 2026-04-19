using Microsoft.Extensions.DependencyInjection;
using SyncroApplicationLayer.Interfaces;
using SyncroApplicationLayer.Services;

namespace SyncroApplicationLayer.Extensions;

public static class MqttServiceExtensions
{
    public static IServiceCollection AddMqttService(this IServiceCollection services)
    {
        services.AddSingleton<MqttService>();
        services.AddSingleton<IMqttService>(sp => sp.GetRequiredService<MqttService>());
        services.AddHostedService(sp => sp.GetRequiredService<MqttService>());
        return services;
    }
}
