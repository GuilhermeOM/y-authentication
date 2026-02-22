namespace Y.Authentication.Domain.ValueObjects;

public record PasswordHash(byte[] Salt, byte[] Hash);
