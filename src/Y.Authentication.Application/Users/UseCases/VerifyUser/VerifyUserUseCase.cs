using Y.Authentication.Application.Abstractions.Messaging;

namespace Y.Authentication.Application.Users.UseCases.VerifyUser;
public sealed record VerifyUserUseCase(string VerificationToken) : IUseCase;
