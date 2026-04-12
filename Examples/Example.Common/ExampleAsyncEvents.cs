using System.Collections.Generic;

namespace Example;

public class ExampleAsyncEvents : MediatR.IRequest<IAsyncEnumerable<ExampleEvent>>
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
