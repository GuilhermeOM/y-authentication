using Y.Authentication.Application.Abstractions.Messaging;

namespace Y.Authentication.Application.Users.UseCases.Example;
public sealed record ExampleUseCase(string ExampleRequest) : IUseCase<ExampleUseCaseResponse>;

public sealed record ExampleUseCaseResponse(string ExampleResponse);
