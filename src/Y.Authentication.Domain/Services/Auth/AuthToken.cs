namespace Y.Authentication.Domain.Services.Auth;
public sealed record AuthToken(string TokenType, string Token, DateTime ExpiresAt);
