using MediatR;

namespace Example;

public class VoidRequest : IRequest
{
    public string Message { get; set; }
}
