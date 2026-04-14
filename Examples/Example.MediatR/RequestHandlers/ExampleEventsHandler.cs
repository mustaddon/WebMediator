using MediatR;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;

namespace Example.MediatR.Handlers;

public class ExampleEventsHandler : IRequestHandler<ExampleAsyncEvents, IAsyncEnumerable<SseItem<string>>>
{
    public async Task<IAsyncEnumerable<SseItem<string>>> Handle(ExampleAsyncEvents request, CancellationToken cancellationToken)
    {
        return ExampleGenerator(request, cancellationToken);
    }

    async IAsyncEnumerable<SseItem<string>> ExampleGenerator(ExampleAsyncEvents request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int index = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return new($"#{index} example event", request.Type)
            {
                EventId = index.ToString()
            };
            await Task.Delay(request.Delay ?? 1000, cancellationToken);
            index++;
        }
    }
}
