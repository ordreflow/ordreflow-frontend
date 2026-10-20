namespace OrdreFlow.Frontend.Models;

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt);
