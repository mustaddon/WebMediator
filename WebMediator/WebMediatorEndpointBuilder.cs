using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WebMediator;

namespace Microsoft.AspNetCore.Builder;

public static class WebMediatorEndpointBuilder
{
    internal static WebMediatorConfig CreateConfig(IEndpointRouteBuilder builder)
    {
        var config = new WebMediatorConfig()
        {
            MemoryCache = builder.ServiceProvider.GetService<IMemoryCache>(),
        };

        var jsonOptions = builder.ServiceProvider.GetService<IOptions<JsonOptions>>()?.Value.SerializerOptions;
        if (jsonOptions != null)
            config.JsonSerialization = jsonOptions;

        if (config.TypeNotFoundResult == null
            && builder is WebApplication webApplication
            && webApplication.Environment.IsProduction())
            config.TypeNotFoundResult = static ctx => Task.FromResult(Results.NotFound());

        return config;
    }

    public static IEndpointConventionBuilder MapMediator(this IEndpointRouteBuilder builder, string route, Action<WebMediatorConfig> configurator, WebMediatorDelegate handler)
    {
        var config = CreateConfig(builder);
        configurator(config);
        return MapMediator(builder, route, config, handler);
    }

    internal static IEndpointConventionBuilder MapMediator(this IEndpointRouteBuilder builder, string route, WebMediatorConfig config, WebMediatorDelegate handler)
    {
        var endpoint = new WebMediatorEndpoint(handler, config ?? CreateConfig(builder));
        var group = builder.MapGroup(route);
        group.MapGet(PATTERN, endpoint.Handler);
        group.MapPost(PATTERN, endpoint.Handler);

        //if (builder.ServiceProvider.GetService<ICorsPolicyProvider>() != null)
        //{
        //    group.RequireCors(x => x.SetIsOriginAllowed(static _ => true)
        //        .AllowAnyMethod()
        //        .AllowAnyHeader()
        //        .AllowCredentials());
        //}

        return group;
    }

    const string PATTERN = "/{type:required}";
}