using System.Globalization;
using Y.Authentication.Application.Abstractions.Messaging;

namespace Y.Authentication.Application.Users.UseCases.CreateUser;
public sealed class CreateUserUseCase : IUseCase
{
    private string? _name = null;

    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? Name
    {
        get => _name;
        set => _name = value is null ? null : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Trim());
    }
    public DateOnly BirthDate { get; set; }
}

