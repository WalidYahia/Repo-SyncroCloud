using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface ILocationSyncService
{
    Task<SyncResultDto> SyncAsync();
}
