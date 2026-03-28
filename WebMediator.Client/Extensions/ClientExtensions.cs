namespace WebMediator.Client;

public static class WebMediatorClientExtensions
{
    public static Task Send(this IWebMediatorClient client, object? request, Type requestType, CancellationToken cancellationToken = default)
    {
        return client.Send(request,
            requestType: requestType,
            resultType: typeof(void),
            cancellationToken: cancellationToken);
    }

    public static Task Send(this IWebMediatorClient client, object request, CancellationToken cancellationToken = default)
    {
        return client.Send(request ?? throw new ArgumentNullException(nameof(request)),
            requestType: request.GetType(),
            resultType: typeof(void),
            cancellationToken: cancellationToken);
    }

    public static async Task<TResult?> Send<TResult>(this IWebMediatorClient client, object request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(request ?? throw new ArgumentNullException(nameof(request)),
            requestType: request.GetType(),
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }

    public static async Task<TResult?> Send<TRequest, TResult>(this IWebMediatorClient client, TRequest? request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(request,
            requestType: typeof(TRequest),
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }
}

