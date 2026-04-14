using MediatR;
using System.Runtime.CompilerServices;

namespace Example.MediatR.Handlers;


public class AsyncItemsStreamHandler : IStreamRequestHandler<AsyncItemsStream, AsyncItem>
{
    public async IAsyncEnumerable<AsyncItem> Handle(AsyncItemsStream request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < request.Count; i++)
        {
            yield return new()
            {
                Index = i,
                Text = $"example async item #{i}"
            };
            await Task.Delay(request.Delay ?? 1000, cancellationToken);
        }
    }
}