namespace WebMediator.Client;

public interface IWebMediatorClient
{
    Task<object?> Send(object? request, Type requestType, Type resultType, CancellationToken cancellationToken = default);
}

