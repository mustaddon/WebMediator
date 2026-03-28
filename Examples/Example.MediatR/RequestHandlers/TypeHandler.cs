using MediatR;
namespace Example.MediatR.Handlers;

public class TypeHandler : IRequestHandler<TypeRequest, Type>
{
    public async Task<Type> Handle(TypeRequest request, CancellationToken cancellationToken)
    {
        return request.Type!;
    }
}