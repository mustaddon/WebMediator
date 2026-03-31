using MediatR;

namespace Example;

public class Echo : IRequest<object>
{
    public object Message { get; set; }
}
