using Mediator;
using System.Net.ServerSentEvents;

namespace WebMediator.Client;

public static class WebMediatorClientExtensionsMediator
{
    public static string GetUrl(this IWebMediatorClient client, IMessage request)
    {
        return client.GetUrl(request?.GetType() ?? throw new ArgumentNullException(nameof(request)), request);
    }

    public static string GetUrl(this IWebMediatorClient client, IStreamMessage request)
    {
        return client.GetUrl(request?.GetType() ?? throw new ArgumentNullException(nameof(request)), request);
    }

    public static Task Send(this IWebMediatorClient client, IRequest request, CancellationToken cancellationToken = default)
    {
        return client.Send(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            resultType: typeof(void),
            cancellationToken: cancellationToken);
    }

    public static async Task<TResult?> Send<TResult>(this IWebMediatorClient client, IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }

    public static Task Send(this IWebMediatorClient client, ICommand request, CancellationToken cancellationToken = default)
    {
        return client.Send(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            resultType: typeof(void),
            cancellationToken: cancellationToken);
    }

    public static async Task<TResult?> Send<TResult>(this IWebMediatorClient client, ICommand<TResult> request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }

    public static async Task<TResult?> Send<TResult>(this IWebMediatorClient client, IQuery<TResult> request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }

    public static async Task<IAsyncEnumerable<TResult>?> Send<TResult>(this IWebMediatorClient client, IStreamRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            resultType: typeof(IAsyncEnumerable<TResult>),
            cancellationToken: cancellationToken);

        return (IAsyncEnumerable<TResult>?)result;
    }


    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(this IWebMediatorClient client, IStreamRequest<SseItem<TResult>> request, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            cancellationToken: cancellationToken);
    }

    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(this IWebMediatorClient client, IStreamRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            cancellationToken: cancellationToken);
    }


    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(this IWebMediatorClient client, IRequest<IAsyncEnumerable<SseItem<TResult>>> request, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            cancellationToken: cancellationToken);
    }

    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(this IWebMediatorClient client, IRequest<IAsyncEnumerable<TResult>> request, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            cancellationToken: cancellationToken);
    }


    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(this IWebMediatorClient client, IQuery<IAsyncEnumerable<SseItem<TResult>>> request, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            cancellationToken: cancellationToken);
    }

    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(this IWebMediatorClient client, IQuery<IAsyncEnumerable<TResult>> request, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            cancellationToken: cancellationToken);
    }


    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(this IWebMediatorClient client, ICommand<IAsyncEnumerable<SseItem<TResult>>> request, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            cancellationToken: cancellationToken);
    }

    public static IAsyncEnumerable<SseItem<TResult>> EventStream<TResult>(this IWebMediatorClient client, ICommand<IAsyncEnumerable<TResult>> request, CancellationToken cancellationToken = default)
    {
        return client.EventStream<TResult>(
            requestType: request?.GetType() ?? throw new ArgumentNullException(nameof(request)),
            request: request,
            cancellationToken: cancellationToken);
    }
}
