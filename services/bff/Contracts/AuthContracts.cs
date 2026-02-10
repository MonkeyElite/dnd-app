namespace DndApp.Bff.Contracts;

public sealed record LoginRequest(string Username, string Password);

public sealed record RegisterWithInviteRequest(string InviteCode, string Username, string DisplayName, string Password);

public sealed record AuthUserDto(Guid UserId, string Username, string DisplayName, bool IsPlatformAdmin);

public sealed record AuthResponseDto(string AccessToken, string? RefreshToken, AuthUserDto User);
