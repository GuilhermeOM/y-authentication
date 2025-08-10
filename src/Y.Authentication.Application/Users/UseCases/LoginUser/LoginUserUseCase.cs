using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Services.Auth;

namespace Y.Authentication.Application.Users.UseCases.LoginUser;
public sealed record LoginUserUseCase(string Email, string Password) : IUseCase<AuthToken>;
