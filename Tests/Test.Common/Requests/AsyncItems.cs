using MediatR;
using System.Collections.Generic;

namespace Test.Requests
{
    public class AsyncItems : IRequest<IAsyncEnumerable<AsyncItem>>
    {
        public string? Message { get; set; }

        public int? Count { get; set; }
    }

    public class AsyncItem
    {
        public int Index { get; set; }
        public string? Message { get; set; }
    }

}
