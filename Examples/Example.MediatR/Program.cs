using Example;
using Example.MediatR.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PingHandler).Assembly))
    .AddOpenApi("v1")
    .AddCors();

var app = builder.Build();

app.MapMediator("mediator",
    // register possible request types
    cfg => cfg.RegisterTypesAssignableTo<MediatR.IBaseRequest>(typeof(Ping).Assembly),
    // handler with a call to MediatR
    async ctx => await ctx.Services.GetRequiredService<MediatR.IMediator>()
        .Send(await ctx.ReadData(), ctx.CancellationToken));
//async ctx => Results.Json(null));

app.MapOpenApi();
app.UseCors(x => x.SetIsOriginAllowed(_ => true).AllowCredentials().AllowAnyHeader());

app.Run();
