using SyncroApplicationLayer.Auth.Interfaces;

namespace SyncroCloudApi.Auth.Middleware;

public class ApiKeyMiddleware(RequestDelegate next)
{
    private const string Header = "X-Api-Key";
    private const string DeviceIdKey = "DeviceId";

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        if (!context.Request.Headers.TryGetValue(Header, out var key))
        {
            await next(context);
            return;
        }

        var deviceId = await apiKeyService.ValidateAsync(key.ToString());
        if (deviceId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired API key" });
            return;
        }

        context.Items[DeviceIdKey] = deviceId.Value;
        await next(context);
    }
}
