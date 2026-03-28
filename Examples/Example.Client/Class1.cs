using System;
using System.Collections.Generic;
using System.Text;

namespace Example;

internal class MediatorReq1 : Mediator.IRequest
{
}


internal class MediatorReq2 : Mediator.IRequest<int>
{
}

