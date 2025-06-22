using Y.Authentication.Application.Abstractions.Messaging;

namespace Y.Authentication.Application.Users.UseCases.CreateUser;
public sealed record CreateUserUseCase(
    string Email,
    string Password,
    string Name,
    DateOnly BirthDate) : IUseCase;
