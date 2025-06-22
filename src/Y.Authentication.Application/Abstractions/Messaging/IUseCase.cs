namespace Y.Authentication.Application.Abstractions.Messaging;
public interface IUseCase;

public interface IUseCase<TResponse> where TResponse : class;
