namespace Y.Authentication.Domain.Options;
public sealed class AuthOptions
{
    public string Secret { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
