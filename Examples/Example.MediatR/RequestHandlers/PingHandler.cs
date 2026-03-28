using MediatR;
namespace Example.MediatR.Handlers;

public class PingHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Pong { Message = request.Message + " PONG" });
    }
}