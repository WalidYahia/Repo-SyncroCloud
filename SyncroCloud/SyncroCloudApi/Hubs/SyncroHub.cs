using Microsoft.AspNetCore.SignalR;

namespace SyncroCloudApi.Hubs;

public class SyncroHub : Hub
{
    /// <summary>Client calls this to receive real-time events for a specific device.</summary>
    public Task JoinDevice(string deviceId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"device_{deviceId}");

    public Task LeaveDevice(string deviceId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"device_{deviceId}");
}
