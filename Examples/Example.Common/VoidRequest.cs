using MediatR;
using System;

namespace Example;

public class TypeRequest : IRequest<Type>
{
    public Type Type { get; set; }
}
