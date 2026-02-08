namespace DndApp.Identity.Contracts;

public sealed record LoginRequest(string Username, string Password);

public sealed record RegisterWithInviteRequest(string InviteCode, string Username, string DisplayName, string Password);

public sealed record AuthUserResponse(Guid UserId, string Username, string DisplayName, bool IsPlatformAdmin);

public sealed record AuthResponse(string AccessToken, string? RefreshToken, AuthUserResponse User);

public sealed record ErrorResponse(string Message);
