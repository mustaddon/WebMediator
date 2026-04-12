using MediatR;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Test.Requests;

namespace Test.WebApi.Handlers
{
    public class AsyncStructsHandler : IRequestHandler<AsyncStructs, IAsyncEnumerable<int>>
    {
        public async Task<IAsyncEnumerable<int>> Handle(AsyncStructs request, CancellationToken cancellationToken)
        {
            return Create(request.Count > 0 ? request.Count.Value : int.MaxValue, cancellationToken);
        }

        private static async IAsyncEnumerable<int> Create(int count, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            int index = 0;

            while (!cancellationToken.IsCancellationRequested && index < count)
            {
                yield return index++;

                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    public class AsyncStructsHandler2 : IRequestHandler<AsyncStructs<SseItem<string>>, IAsyncEnumerable<SseItem<string>>>
    {
        public async Task<IAsyncEnumerable<SseItem<string>>> Handle(AsyncStructs<SseItem<string>> request, CancellationToken cancellationToken)
        {
            return Create(request.Count > 0 ? request.Count.Value : int.MaxValue, request.Type!, cancellationToken);
        }

        private static async IAsyncEnumerable<SseItem<string>> Create(int count, string type, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            int index = 0;

            while (!cancellationToken.IsCancellationRequested && index < count)
            {
                yield return new($"{index}: test", type) { EventId = index.ToString() };

                index++;

                await Task.Delay(1000, cancellationToken);
            }
        }

    }
}
