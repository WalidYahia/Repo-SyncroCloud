namespace SyncroApplicationLayer.DTOs;

// ── Incoming directive (ASP.NET model binding handles camelCase → PascalCase) ──
public record AlexaRequest(AlexaDirective Directive);

public record AlexaDirective(
    AlexaDirectiveHeader  Header,
    AlexaEndpointRef?     Endpoint,
    AlexaRequestPayload?  Payload);

public record AlexaDirectiveHeader(
    string  Namespace,
    string  Name,
    string  MessageId,
    string? CorrelationToken,
    string  PayloadVersion);

public record AlexaEndpointRef(string EndpointId, AlexaScope Scope);

public record AlexaScope(string Type, string Token);

public record AlexaRequestPayload(AlexaScope? Scope);
