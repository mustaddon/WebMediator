using MediatR;
using System.Net.ServerSentEvents;
using System.Reflection;
using Test.Requests;
using Test.WebApi.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddTransient<IRequestHandler<FileUpload<FileMetadata>, FileUploadResult<FileMetadata>>, FileUploadHandler<FileMetadata>>();

builder.Services.AddMemoryCache();
builder.Services.AddOpenApi("v1");
builder.Services.AddCors();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    //options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
    //options.SerializerOptions.Converters.Add(new JsonTypeConverter(TypeDeserializer.Default));
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

var app = builder.Build();

foreach (var route in new[] { "mediatr", "mediator", })
    app.MapMediator(route,
        cfg => cfg
            .RegisterNotAbstractTypesFromAssembly(typeof(Ping).Assembly)
            .RegisterTypes([typeof(SseItem<>)]),
        async x =>
        {
            var data = await x.ReadData();
            var mediator = x.Services.GetRequiredService<IMediator>();
            return typeof(IBaseRequest).IsAssignableFrom(x.DataType)
                ? await mediator.Send(data, x.CancellationToken)
                : mediator.CreateStream(data, x.CancellationToken);
        });

app.MapMediator("sapi",
    cfg => cfg.RegisterNotAbstractTypesFromAssembly(Assembly.GetExecutingAssembly()),
    async (x) =>
    {
        var data = await x.ReadData();
        if (data is Stream stream)
        {
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms, x.CancellationToken);
            ms.Position = 0;
            return ms;
        }

        return data;
    });

app.MapGet("/", () => Results.Stream(File.OpenRead(@".\files\test.html"), "text/html; charset=utf-8"));
app.MapOpenApi();
app.UseCors(x => x.SetIsOriginAllowed(_ => true).AllowCredentials().AllowAnyHeader());

app.Run();