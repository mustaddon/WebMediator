using Microsoft.AspNetCore.Http.HttpResults;
using TypeSerialization.Json;

namespace WebMediator;

public class WebMediatorEndpoint
{
    public WebMediatorEndpoint(WebMediatorDelegate handler, WebMediatorConfig config)
    {
        _handler = handler;
        _typeDeserializer = config.TypeDeserializer ?? TypeDeserializer.Default;
        _jsonOptions = config.JsonSerialization;
        _fillEmptyRequests = config.CreatingInstancesOnEmptyRequests;

        if (!_jsonOptions.Converters.Any(x => typeof(JsonTypeConverter).IsAssignableFrom(x.GetType())))
            _jsonOptions.Converters.Add(new JsonTypeConverter(_typeDeserializer));
    }

    readonly bool _fillEmptyRequests;
    readonly WebMediatorDelegate _handler;
    readonly TypeDeserializer _typeDeserializer;
    readonly JsonSerializerOptions _jsonOptions;

    public Task<IResult> Handler(HttpContext ctx, string type, string? data)
    {
        var dataType = _typeDeserializer.Deserialize(type)!;

        ctx.Response.Headers.AddNoCache();
        ctx.Response.Headers.AddExposedHeaders();

        return GetResult(ctx, _handler(new(
            httpContext: ctx,
            dataType: dataType,
            dataGetter: (t) => DataGetter(ctx, t, data))));
    }

    async Task<object?> DataGetter(HttpContext ctx, Type type, string? json)
    {
        var streamProp = type.GetStreamProperty();

        if (!string.IsNullOrEmpty(json))
        {
            var obj = JsonSerializer.Deserialize(json, type, _jsonOptions);
            streamProp?.SetValue(obj, await DeserializeBody(ctx, typeof(Stream)));
            return obj;
        }

        if (streamProp != null)
        {
            var obj = type.CreateInstance();
            streamProp.SetValue(obj, await DeserializeBody(ctx, typeof(Stream)));
            return obj;
        }

        return await DeserializeBody(ctx, type);
    }

    async Task<object?> DeserializeBody(HttpContext ctx, Type type)
    {
        if (type == typeof(Stream))
            return new HttpStream(ctx.Request);

        try
        {
            return await JsonSerializer.DeserializeAsync(ctx.Request.Body, type, _jsonOptions, ctx.RequestAborted);
        }
        catch (JsonException ex)
        {
            if (ex.LineNumber == 0 && ex.BytePositionInLine == 0)
                return _fillEmptyRequests ? type.CreateInstance() : null;

            throw;
        }
    }

    async Task<IResult> GetResult(HttpContext ctx, Task<object?> valueTask)
    {
        var value = await valueTask;

        if (value == null)
            return Results.NoContent();

        if (value is IResult result)
        {
            ctx.Response.Headers.AddDataType(
                result.TryGetJsonType(out var jsonType) 
                    ? jsonType 
                    : typeof(Stream));

            return result;
        }

        if (value is Stream stream)
        {
            ctx.Response.Headers.AddDataType(typeof(Stream));
            return Results.Stream(stream);
        }

        var resultType = value.GetType();

        ctx.Response.Headers.AddDataType(resultType);

        var streamProp = resultType.GetStreamProperty();

        if (streamProp != null && streamProp.GetValue(value) is Stream streamPropVal)
        {
            streamProp.SetValue(value, null);
            ctx.Response.Headers.AddData(value, _jsonOptions);
            ctx.Response.Headers.AddDataStreamProperty(streamProp.Name);
            streamProp.SetValue(value, streamPropVal);
            return Results.Stream(streamPropVal);
        }

        return Results.Json(value, _jsonOptions);
    }

}
