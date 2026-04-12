using MediatR;
using System.Collections.Generic;

namespace Test.Requests
{
    public class AsyncStream : MediatR.IStreamRequest<AsyncStreamItem>
    {
        public string? Message { get; set; }

        public int? Count { get; set; }
    }

    public class AsyncStreamItem
    {
        public int Index { get; set; }
        public string? Message { get; set; }
    }

}
