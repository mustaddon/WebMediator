using Mediator;

namespace WebMediator.Client;

public static class WebMediatorClientExtensionsMediator
{
    public static Task Send(this IWebMediatorClient client, IRequest request, CancellationToken cancellationToken = default)
    {
        return client.Send(request ?? throw new ArgumentNullException(nameof(request)),
            requestType: request.GetType(),
            resultType: typeof(void),
            cancellationToken: cancellationToken);
    }

    public static async Task<TResult?> Send<TResult>(this IWebMediatorClient client, IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(request ?? throw new ArgumentNullException(nameof(request)),
            requestType: request.GetType(),
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }

    public static Task Send(this IWebMediatorClient client, ICommand request, CancellationToken cancellationToken = default)
    {
        return client.Send(request ?? throw new ArgumentNullException(nameof(request)),
            requestType: request.GetType(),
            resultType: typeof(void),
            cancellationToken: cancellationToken);
    }

    public static async Task<TResult?> Send<TResult>(this IWebMediatorClient client, ICommand<TResult> request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(request ?? throw new ArgumentNullException(nameof(request)),
            requestType: request.GetType(),
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }

    public static async Task<TResult?> Send<TResult>(this IWebMediatorClient client, IQuery<TResult> request, CancellationToken cancellationToken = default)
    {
        var result = await client.Send(request ?? throw new ArgumentNullException(nameof(request)),
            requestType: request.GetType(),
            resultType: typeof(TResult),
            cancellationToken: cancellationToken);

        return (TResult?)result;
    }
}
