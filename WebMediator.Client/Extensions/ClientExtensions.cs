using System.Net.ServerSentEvents;

namespace WebMediator.Client;

public static class WebMediatorClientExtensions
{
    public static string GetUrl<TRequest>(this IWebMediatorClient client, TRequest? request = default)
    {
        return client.GetUrl(typeof(TRequest), request);
    }



    public static Task Send(this IWebMediatorClient client, Type requestType, object? request = null, CancellationToken cancellationToken = default)
    {
        return client.Send(
            requestType: requestType,
            request: request,
            resultType: typeof(void),
            cancellationToken: cancellationToken);
    }

    public static Task Send(this IWebMediatorClient client, object request, CancellationToken cancellationToken = default)
    {
        return client.Send(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            resultType: typeof(void),
            cancellationToken: cancellationToken);
    }

    public static async Task<TResult?> Send<TResult>(this IWebMediatorClient client, object request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }

    public static async Task<TResult?> Send<TRequest, TResult>(this IWebMediatorClient client, TRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(
            requestType: typeof(TRequest),
            request: request,
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }



    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TRequest, TResult>(this IWebMediatorClient client, TRequest? request = default, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: typeof(TRequest),
            request: request,
            cancellationToken: cancellationToken);
    }
}

