using MediatR;
using System.Collections.Generic;

namespace Test.Requests
{
    public class AsyncStructs : IRequest<IAsyncEnumerable<int>>
    {
        public string? Type { get; set; }
        public int? Count { get; set; }
    }

    public class AsyncStructs<T> : IRequest<IAsyncEnumerable<T>>
    {
        public string? Type { get; set; }
        public int? Count { get; set; }
    }

}
