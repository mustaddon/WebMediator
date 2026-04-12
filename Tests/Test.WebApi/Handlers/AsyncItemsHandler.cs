using MediatR;
using System.Runtime.CompilerServices;
using Test.Requests;

namespace Test.WebApi.Handlers
{
    public class AsyncItemsHandler : IRequestHandler<AsyncItems, IAsyncEnumerable<AsyncItem>>
    {
        public async Task<IAsyncEnumerable<AsyncItem>> Handle(AsyncItems request, CancellationToken cancellationToken)
        {
            return Create(request.Count > 0 ? request.Count.Value : int.MaxValue, cancellationToken);
        }

        private static async IAsyncEnumerable<AsyncItem> Create(int count, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            int index = 0;

            while (!cancellationToken.IsCancellationRequested && index < count)
            {
                yield return new()
                {
                    Index = index++,
                    Message = "data: \r\n\r\n 123 \n\n data: 555"
                };

                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
