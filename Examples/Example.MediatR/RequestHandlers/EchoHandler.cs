using MediatR;
namespace Example.MediatR.Handlers;

public class EchoHandler : IRequestHandler<Echo, object>
{
    public Task<object> Handle(Echo request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Message);
    }
}