using System.Collections.Generic;
using System.Net.ServerSentEvents;

namespace Test.Requests
{
    public class AsyncEventsRequest : MediatR.IRequest<IAsyncEnumerable<AsyncEvent>>
    {
        public string? Type { get; set; }
        public int? ErrorIndex { get; set; }
    }

    public class AsyncEventsRequestSse : MediatR.IRequest<IAsyncEnumerable<SseItem<AsyncEvent>>>
    {
        public string? Type { get; set; }
        public int? ErrorIndex { get; set; }
    }

    public class AsyncEvents : MediatR.IStreamRequest<AsyncEvent>
    {
        public string? Type { get; set; }
        public int? ErrorIndex { get; set; }
    }

    public class AsyncEventsSse : MediatR.IStreamRequest<SseItem<AsyncEvent>>
    {
        public string? Type { get; set; }
        public int? ErrorIndex { get; set; }
    }

    public class AsyncEvent
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
    }

}
