using System.Collections.Generic;
using System.Net.ServerSentEvents;

namespace Example;

public class ExampleAsyncEvents : MediatR.IRequest<IAsyncEnumerable<SseItem<string>>>
{
    public string Type { get; set; }
}

public class ExampleAsyncEventsStream : MediatR.IStreamRequest<ExampleEvent>
{
    public string Type { get; set; }
}

public class ExampleEvent
{
    public string Type { get; set; }
    public string Text { get; set; }
}
