using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Test.Requests;

namespace Test.WebApi.Handlers;

public class AsyncEventsHandler : MediatR.IStreamRequestHandler<AsyncEvents, AsyncEvent>
{
    public async IAsyncEnumerable<AsyncEvent> Handle(AsyncEvents request, [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        int index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (request.ErrorIndex == index)
                throw new Exception("test");

            yield return new()
            {
                Type = request.Type,
                Message = $"тест: {index++}"
            };

            await Task.Delay(1000, cancellationToken);
        }
    }
}

public class AsyncEventsSseHandler : MediatR.IStreamRequestHandler<AsyncEventsSse, SseItem<AsyncEvent>>
{
    public async IAsyncEnumerable<SseItem<AsyncEvent>> Handle(AsyncEventsSse request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (request.ErrorIndex == index)
                throw new Exception("test");

            yield return new(new()
            {
                Type = request.Type,
                Message = $"test: {index++}"
            }, request.Type);

            await Task.Delay(1000, cancellationToken);
        }
    }
}

public class AsyncEventsRequestHandler : MediatR.IRequestHandler<AsyncEventsRequest, IAsyncEnumerable<AsyncEvent>>
{
    public async Task<IAsyncEnumerable<AsyncEvent>> Handle(AsyncEventsRequest request, CancellationToken cancellationToken)
    {
        return Generator(request, cancellationToken);
    }

    async IAsyncEnumerable<AsyncEvent> Generator(AsyncEventsRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (request.ErrorIndex == index)
                throw new Exception("test");

            yield return new()
            {
                Type = request.Type,
                Message = $"test: {index++}"
            };

            await Task.Delay(1000, cancellationToken);
        }
    }
}

public class AsyncEventsRequestSseHandler : MediatR.IRequestHandler<AsyncEventsRequestSse, IAsyncEnumerable<SseItem<AsyncEvent>>>
{
    public async Task<IAsyncEnumerable<SseItem<AsyncEvent>>> Handle(AsyncEventsRequestSse request, CancellationToken cancellationToken)
    {
        return Generator(request, cancellationToken);
    }

    async IAsyncEnumerable<SseItem<AsyncEvent>> Generator(AsyncEventsRequestSse request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (request.ErrorIndex == index)
                throw new Exception("test");

            yield return new(new(){
                Type = request.Type,
                Message = $"test: {index++}"
            }, request.Type);

            await Task.Delay(1000, cancellationToken);
        }
    }
}