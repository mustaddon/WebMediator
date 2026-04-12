namespace WebMediator.Client.Extensions;

internal static class JsonOptionsExt
{
    public static bool TryAdd(this IList<JsonConverter> converters, JsonConverter converter)
    {
        var type = converter.Type;

        if (type != null && !converters.Any(x => x.CanConvert(type)))
        {
            converters.Add(converter);
            return true;
        }

        return false;
    }
}
