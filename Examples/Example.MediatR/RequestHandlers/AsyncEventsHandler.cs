using MediatR;
using System.Runtime.CompilerServices;

namespace Example.MediatR.Handlers;

public class AsyncEventsHandler : IRequestHandler<ExampleAsyncEvents, IAsyncEnumerable<ExampleEvent>>
{
    public async Task<IAsyncEnumerable<ExampleEvent>> Handle(ExampleAsyncEvents request, CancellationToken cancellationToken)
    {
        return ExampleGenerator(request, cancellationToken);
    }

    async IAsyncEnumerable<ExampleEvent> ExampleGenerator(ExampleAsyncEvents request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            yield return new()
            {
                Type = request.Type,
                Text = $"#{index++} example event"
            };

            await Task.Delay(1000, cancellationToken);
        }
    }
}
