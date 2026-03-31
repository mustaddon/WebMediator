using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebMediator;

namespace Microsoft.AspNetCore.Builder;

public static class WebMediatorEndpointBuilder
{
    public static IEndpointConventionBuilder MapMediator(this IEndpointRouteBuilder builder, Action<WebMediatorConfig> configurator, WebMediatorDelegate handler)
    {
        return MapMediator(builder, "mediator", configurator, handler);
    }

    public static IEndpointConventionBuilder MapMediator(this IEndpointRouteBuilder builder, string route, Action<WebMediatorConfig> configurator, WebMediatorDelegate handler)
    {
        var config = new WebMediatorConfig(); configurator(config);
        return MapMediator(builder, route, config, handler);
    }

    public static IEndpointConventionBuilder MapMediator(this IEndpointRouteBuilder builder, string route, WebMediatorDelegate handler)
    {
        return MapMediator(builder, route,
            cfg => cfg.RegisterNotAbstractTypesFromAssembly(Assembly.GetCallingAssembly()),
            handler);
    }

    public static IEndpointConventionBuilder MapMediator(this IEndpointRouteBuilder builder, string route, WebMediatorConfig config, WebMediatorDelegate handler)
    {
        config ??= new();

        if (config.ReturnOnUnregisteredDataType == null
            && builder is WebApplication webApplication
            && webApplication.Environment.IsProduction())
            config.ReturnOnUnregisteredDataType = static ctx => Results.NotFound();

        var endpoint = new WebMediatorEndpoint(handler, config);
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