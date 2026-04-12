using Microsoft.AspNetCore.Http.HttpResults;

namespace WebMediator.Extensions;

internal static class HttpExtensions
{
    public static bool TryGetJsonType(this IResult result, out Type jsonType)
    {
        var type = result.GetType();
        if (type.IsGenericType 
            && !type.IsGenericTypeDefinition 
            && type.GetGenericTypeDefinition() == typeof(JsonHttpResult<>))
        {
            jsonType = type.GetGenericArguments()[0];
            return true;
        }

        jsonType = default!;
        return false;
    }

    public static bool TryGetSseType(this IResult result, out Type sseType)
    {
        var type = result.GetType();
        if (type.IsGenericType
            && !type.IsGenericTypeDefinition
            && type.GetGenericTypeDefinition() == typeof(ServerSentEventsResult<>))
        {
            sseType = type.GetGenericArguments()[0];
            return true;
        }

        sseType = default!;
        return false;
    }

    

    public static IHeaderDictionary AddDataType(this IHeaderDictionary headers, Type type)
    {
        headers[Headers.DATA_TYPE] = type.Serialize();
        return headers;
    }

    public static IHeaderDictionary AddDataStreamProperty(this IHeaderDictionary headers, string propName)
    {
        headers[Headers.DATA_STREAM_PROPERTY] = propName;
        return headers;
    }

    public static IHeaderDictionary AddData(this IHeaderDictionary headers, object? value, JsonSerializerOptions options)
    {
        headers[Headers.DATA] = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(value, options));
        return headers;
    }

    public static IHeaderDictionary AddExposedHeaders(this IHeaderDictionary headers)
    {
        headers["access-control-expose-headers"] = Headers.CorsExpose;
        return headers;
    }

    public static IHeaderDictionary AddNoCache(this IHeaderDictionary headers)
    {
        foreach (var kvp in NoCacheHeaders)
            headers[kvp.Key] = kvp.Value;

        return headers;
    }

    static readonly Dictionary<string, string> NoCacheHeaders = new()
    {
        { "Cache-Control", "no-cache, no-store, must-revalidate" },
        { "Pragma", "no-cache" },
        { "Expires", "0" },
    };
}
