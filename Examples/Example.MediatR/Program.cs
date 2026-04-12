using Example;
using Example.MediatR.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PingHandler).Assembly))
    .AddCors();

var app = builder.Build();

app.MapMediator("mediator",
    // register possible request types
    cfg => cfg
        .RegisterTypesAssignableTo<MediatR.IBaseRequest>(typeof(Ping).Assembly)
        .RegisterTypes([typeof(ExampleAsyncEventsStream)]),

    // handler with a call to MediatR
    async ctx =>
    {
        var request = await ctx.ReadData();
        var mediatorSvc = ctx.Services.GetRequiredService<MediatR.IMediator>();

        return typeof(MediatR.IBaseRequest).IsAssignableFrom(ctx.DataType)
            ? await mediatorSvc.Send(request, ctx.CancellationToken)
            : mediatorSvc.CreateStream(request, ctx.CancellationToken);
    });


app.UseCors(x => x.AllowAnyOrigin());
app.Run();
