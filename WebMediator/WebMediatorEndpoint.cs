using Microsoft.Extensions.Caching.Memory;
using TypeSerialization.Json;

namespace WebMediator;

public class WebMediatorEndpoint
{
    public WebMediatorEndpoint(WebMediatorDelegate handler, WebMediatorConfig config)
    {
        _handler = handler;
        _config = config;
        _cache = config.MemoryCache ?? StaticCache.Instance;
        _typeDeserializer = config.TypeDeserializer ?? TypeDeserializer.Default;

        _jsonOptions = new(config.JsonSerialization)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        _jsonOptions.Converters.TryAdd(new JsonTypeConverter(_typeDeserializer));

        _jsonOptionsIndented = new(_jsonOptions)
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    readonly WebMediatorConfig _config;
    readonly WebMediatorDelegate _handler;
    readonly TypeDeserializer _typeDeserializer;
    readonly JsonSerializerOptions _jsonOptions;
    readonly JsonSerializerOptions _jsonOptionsIndented;
    readonly IMemoryCache _cache;

    public Task<IResult> Handler(HttpContext ctx, string type, string? data)
    {
        Type dataType;

        try
        {
            dataType = _typeDeserializer.Deserialize(type)!;
        }
        catch
        {
            if (_config.TypeNotFoundResult != null)
                return _config.TypeNotFoundResult(ctx);

            throw;
        }

        return GetResult(ctx, new WebMediatorRequest(
            httpContext: ctx,
            dataType: dataType,
            dataGetter: (t) => DataGetter(ctx, t, data)));
    }

    async Task<object?> DataGetter(HttpContext ctx, Type type, string? json)
    {
        var streamProp = _cache.GetStreamProperty(type);

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
                result.TryGetJsonType(out var jsonType) ? jsonType
                : result.TryGetSseType(out var sseType) ? sseType.ToAsyncEnumerableType()
                : typeof(Stream));

            return result;
        }

        if (value is Stream stream)
        {
            ctx.Response.Headers.AddDataType(typeof(Stream));
            return Results.Stream(stream);
        }

        if (_cache.TryGetAsyncEnumerableItemType(value.GetType(), out var itemType, out var sseItemType))
        {
            ctx.Response.Headers.AddDataType((sseItemType ?? itemType).ToAsyncEnumerableType());
            return Results.ServerSentEvents(value
                .ToAsyncEnumerable(itemType, ctx.RequestAborted)
                .ToSse(_jsonOptionsIndented, itemType, sseItemType != null, ctx.RequestAborted));
        }

        var resultType = value.GetType();

        ctx.Response.Headers.AddDataType(resultType);

        var streamProp = _cache.GetStreamProperty(resultType);

        if (streamProp != null && streamProp.GetValue(value) is Stream streamPropVal)
        {
            streamProp.SetValue(value, null);
            ctx.Response.Headers.AddData(value, _jsonOptionsIndented);
            ctx.Response.Headers.AddDataStreamProperty(streamProp.Name, _jsonOptionsIndented);
            streamProp.SetValue(value, streamPropVal);
            return Results.Stream(streamPropVal);
        }

        return Results.Json(value, _jsonOptions);
    }

}
