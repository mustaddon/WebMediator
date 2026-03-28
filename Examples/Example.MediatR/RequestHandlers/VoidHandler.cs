using MediatR;
namespace Example.MediatR.Handlers;

public class VoidHandler : IRequestHandler<VoidRequest>
{
    public async Task Handle(VoidRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"VoidRequest: {request.Message}");
    }
}