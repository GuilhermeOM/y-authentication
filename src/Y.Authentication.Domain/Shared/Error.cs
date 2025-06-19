using System.Net;

namespace Y.Authentication.Domain.Shared;
public sealed record Error
{
    public string Code { get; }
    public string Description { get; }

    public HttpStatusCode? StatusCode { get; }

    public static Error None => new(string.Empty, string.Empty);

    public Error(string code, string description = "")
    {
        Code = code;
        Description = description;
    }

    public Error(HttpStatusCode code, string description = "") : this(code.ToString(), description)
    {
        StatusCode = code;
    }
}
