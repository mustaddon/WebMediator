using Mediator;
using System.Runtime.CompilerServices;
using Test.Requests;

namespace Test.WebApi.Handlers
{
    public class AsyncItemsHandler : MediatR.IRequestHandler<AsyncItems, IAsyncEnumerable<AsyncItem>>
    {
        public async Task<IAsyncEnumerable<AsyncItem>> Handle(AsyncItems request, CancellationToken cancellationToken)
        {
            return Create(request, cancellationToken);
        }

        private static async IAsyncEnumerable<AsyncItem> Create(AsyncItems request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var count = request.Count > 0 ? request.Count.Value : int.MaxValue;
            var delay = request.Delay ?? 1000;
            int index = 0;

            while (!cancellationToken.IsCancellationRequested && index < count)
            {
                yield return new()
                {
                    Index = index++,
                    Message = "data: \r\n\r\n 123 \n\n data: 555"
                };

                if (delay > 0)
                    await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
