using System.Reflection;

namespace Example;

public class AsyncItemsStream : MediatR.IStreamRequest<AsyncItem>
{
    public int Count { get; set; }
}

public class AsyncItem
{
    public int Index { get; set; }
    public string Text { get; set; }
}
