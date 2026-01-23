namespace Y.Authentication.Domain.ValueObjects;
public sealed record AuthToken(string TokenType, string Token, DateTime ExpiresAt);
