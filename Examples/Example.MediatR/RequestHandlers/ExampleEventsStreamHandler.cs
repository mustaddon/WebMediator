using MediatR;
using System.Runtime.CompilerServices;

namespace Example.MediatR.Handlers;


public class ExampleEventsStreamHandler : IStreamRequestHandler<ExampleAsyncEventsStream, ExampleEvent>
{
    public async IAsyncEnumerable<ExampleEvent> Handle(ExampleAsyncEventsStream request, [EnumeratorCancellation] CancellationToken cancellationToken)
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