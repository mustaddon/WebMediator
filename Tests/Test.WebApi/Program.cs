using MediatR;
using System.Reflection;
using Test.Requests;
using Test.WebApi.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddTransient<IRequestHandler<FileUpload<FileMetadata>, FileUploadResult<FileMetadata>>, FileUploadHandler<FileMetadata>>();

builder.Services.AddOpenApi("v1");
builder.Services.AddCors();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

var app = builder.Build();

app.MapMediator("mediatr", 
    cfg => cfg.RegisterNotAbstractTypesFromAssembly(typeof(Ping).Assembly),
    async x => await x.Services.GetRequiredService<IMediator>().Send(await x.ReadData(), x.CancellationToken));

app.MapMediator("sapi", async (x) =>
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