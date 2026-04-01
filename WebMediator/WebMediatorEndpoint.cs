using TypeSerialization.Json;

namespace WebMediator;

public class WebMediatorEndpoint
{
    public WebMediatorEndpoint(WebMediatorDelegate handler, WebMediatorConfig config)
    {
        _handler = handler;
        _config = config;

        _typeDeserializer = config.TypeDeserializer ?? TypeDeserializer.Default;

        if (!config.JsonSerialization.Converters.Any(x => typeof(JsonTypeConverter).IsAssignableFrom(x.GetType())))
            config.JsonSerialization.Converters.Add(new JsonTypeConverter(_typeDeserializer));
    }

    readonly WebMediatorConfig _config;
    readonly WebMediatorDelegate _handler;
    readonly TypeDeserializer _typeDeserializer;

    public Task<IResult> Handler(HttpContext ctx, string type, string? data)
    {
        Type dataType;

        try
        {
            dataType = _typeDeserializer.Deserialize(type)!;
        }
        catch
        {
            if (_config.NotFoundTypeResult != null)
                return _config.NotFoundTypeResult(ctx);

            throw;
        }

        return GetResult(ctx, new WebMediatorRequest(
            httpContext: ctx,
            dataType: dataType,
            dataGetter: (t) => DataGetter(ctx, t, data)));
    }

    async Task<object?> DataGetter(HttpContext ctx, Type type, string? json)
    {
        var streamProp = type.GetStreamProperty();

        if (!string.IsNullOrEmpty(json))
        {
            var obj = JsonSerializer.Deserialize(json, type, _config.JsonSerialization);
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
            return await JsonSerializer.DeserializeAsync(ctx.Request.Body, type, _config.JsonSerialization, ctx.RequestAborted);
        }
        catch (JsonException ex)
        {
            if (ex.LineNumber == 0 && ex.BytePositionInLine == 0)
                return _config.CreatingInstancesOnEmptyRequests ? type.CreateInstance() : null;

            throw;
        }
    }

    async Task<IResult> GetResult(HttpContext ctx, WebMediatorRequest request)
    {
        ctx.Response.Headers.AddNoCache();

        var value = await _handler(request);

        ctx.Response.Headers.AddExposedHeaders();

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
            ctx.Response.Headers.AddData(value, _config.JsonSerialization);
            ctx.Response.Headers.AddDataStreamProperty(streamProp.Name);
            streamProp.SetValue(value, streamPropVal);
            return Results.Stream(streamPropVal);
        }

        return Results.Json(value, _config.JsonSerialization);
    }

}
