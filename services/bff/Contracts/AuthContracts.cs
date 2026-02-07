namespace DndApp.Bff.Contracts;

public sealed record LoginRequest(string Username, string Password);

public sealed record RegisterWithInviteRequest(string InviteCode, string Username, string DisplayName, string Password);
