namespace SyncroApplicationLayer.DTOs;

// ── Incoming fulfillment request ──────────────────────────
public record GoogleHomeRequest(string RequestId, List<GoogleHomeInput> Inputs);

public record GoogleHomeInput(string Intent, GoogleHomePayload? Payload);

public record GoogleHomePayload(
    List<GoogleHomeCommand>?   Commands,
    List<GoogleHomeDeviceRef>? Devices);

public record GoogleHomeCommand(
    List<GoogleHomeDeviceRef>  Devices,
    List<GoogleHomeExecution>  Execution);

public record GoogleHomeDeviceRef(string Id);

// Params values deserialize as JsonElement when using System.Text.Json
public record GoogleHomeExecution(string Command, Dictionary<string, object>? Params);
