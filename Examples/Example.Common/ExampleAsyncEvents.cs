using System.Collections.Generic;
using System.Net.ServerSentEvents;

namespace Example;

public class ExampleAsyncEvents : MediatR.IRequest<IAsyncEnumerable<SseItem<string>>>
{
    public string Type { get; set; }
}

