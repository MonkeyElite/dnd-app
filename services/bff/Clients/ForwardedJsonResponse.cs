namespace DndApp.Bff.Clients;

public sealed record ForwardedJsonResponse(int StatusCode, string ContentType, string Body);
