using System.Runtime.CompilerServices;
using Test.Requests;

namespace Test.WebApi.Handlers
{
    public class AsyncStreamHandler : MediatR.IStreamRequestHandler<AsyncStream, AsyncStreamItem>
    {

        public async IAsyncEnumerable<AsyncStreamItem> Handle(AsyncStream request, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            int index = 0;

            while (!cancellationToken.IsCancellationRequested && index < request.Count)
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
